using System;
using System.Text.RegularExpressions;
using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{
    public abstract class AbstractAtom : Atom
    {
        public override CharType Type => CharTypeInternal.Invalid;
        public override CharType LeftType => Type;
        public override CharType RightType => Type;
    }
    public abstract class DisplayAtom : Atom
    {
        public override CharType LeftType => Type;
        public override CharType RightType => Type;
    }

    public abstract class InlineAtom : Atom
    {
        public Atom atom;

        public override CharType Type => atom.Type;
        public override CharType LeftType => atom?.LeftType ?? CharTypeInternal.Invalid;
        public override CharType RightType => atom?.RightType ?? CharTypeInternal.Invalid;

        public override void Flush()
        {
            atom?.Flush();
            atom = null;
        }
    }

    public class ScriptedAtom : InlineMacroAtom
    {
        public static ScriptedAtom Get(ParametrizedMacro token)
        {
            var atom = ObjPool<ScriptedAtom>.Get();
            atom.macro = token;
            return atom;
        }
        public ParametrizedMacro macro;

        public override Box CreateBox(TexBoxingState state)
        {
           return atom?.CreateBox(state);
        }
        public override void Flush()
        {
            ObjPool<ScriptedAtom>.Release(this);
            macro = default;
            base.Flush();
        }
        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            Process(macro, state, value, ref position);
        }
    }
    
    public class InlineMacroAtom : InlineAtom
    {
        public static Atom Get()
        {
            return ObjPool<InlineMacroAtom>.Get();
        }

        public override Box CreateBox(TexBoxingState state)
        {
            return atom?.CreateBox(state) ?? StrutBox.Empty;
        }

        public override void Flush()
        {
            ObjPool<InlineMacroAtom>.Release(this);
            base.Flush();
        }

        // Not multi-thread-safe
        static readonly string[] temp = new string[10];

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            if (state.parser.macroCommands.TryGetValue(command, out ParametrizedMacro macro))
            {
                Process(macro, state, value, ref position);
            }
            else if (state.parser.genericCommands.TryGetValue(command, out InlineState context))
            {
                Process(context, state, value, ref position);
            }
        }

        public void Process(ParametrizedMacro macro, TexParserState state, string value, ref int position)
        {
            var count = Math.Min(temp.Length, macro.bracetedParameters + macro.bracketedParameters);
            for (int i = 1; i <= count; i++)
            {
                SkipWhiteSpace(value, ref position);
                if (i <= macro.bracketedParameters)
                    if (position < value.Length && value[position] == '[')
                        temp[i] = ReadGroup(value, ref position, '[', ']');
                    else
                        continue;
                else
                    temp[i] = LookForALetter(value, ref position);
            }
            SkipWhiteSpace(value, ref position);
            atom = state.parser.Parse(string.Format(macro.formatableValue, temp), state, true);
        }

        public void Process(InlineState context, TexParserState state, string value, ref int position)
        {
            SkipWhiteSpace(value, ref position);
            if (context.requiresGroupContext)
                state.PushStates();
            context.stateProcessor?.Invoke(state);

            if (context.requiresGroupContext)
            {
                SkipWhiteSpace(value, ref position);
                if (!(position < value.Length && value[position] == beginGroupChar))
                    atom = SpaceAtom.Empty;
                else
                {
                    var str = ReadGroup(value, ref position);
                    atom = context.interpreter != null ? context.interpreter(state, str, 0).atom : state.parser.Parse(str, state, true);
                }
                state.PopStates();
            } else if (context.interpreter != null)
            {
                (position, atom) = context.interpreter(state, value, position); 
            }
            SkipWhiteSpace(value, ref position);
        }

        public readonly struct InlineState
        {
            public readonly string name;
            public readonly Action<TexParserState> stateProcessor;
            public readonly Func<TexParserState, string, int, (int pos, Atom atom)> interpreter;
            public readonly bool requiresGroupContext;

            public InlineState(string name,
                               Action<TexParserState> stateProcessor = null,
                               Func<TexParserState, string, int, (int, Atom)> interpreter = null,
                               bool requiresGroupContext = false)
            {
                this.name = name;
                this.stateProcessor = stateProcessor;
                this.interpreter = interpreter;
                this.requiresGroupContext = requiresGroupContext;
            }
        }

        public readonly struct ParametrizedMacro
        {
            public readonly string macroKey;
            public readonly string formatableValue;
            public readonly int bracketedParameters;
            public readonly int bracetedParameters;

            public readonly string rawKey, rawValue;
            static readonly Regex replacer = new Regex(@"#(\d)");

            public ParametrizedMacro(StringPair pair)
            {
                int brack = 0, brace = 0, pos = 0, stop = 0;
                string key = pair.key;

                while (pos < key.Length)
                {
                    var offset = key.IndexOf('#', pos);
                    if (offset < 0) { if (stop == 0) stop = key.Length - 1; break; }
                    if (key[offset] == '#')
                    {
                        if (stop == 0)
                            stop = offset - 1;
                        if (key[offset - 1] == '[')
                            brack++;
                        else
                            brace++;
                        pos = offset + 1;
                    }
                }
                if (key[stop] == '[')
                    stop--;
                if (key[0] == '\\')
                    macroKey = key.Substring(1, stop);
                else
                    macroKey = key.Substring(0, stop + 1);
                bracketedParameters = brack;
                bracetedParameters = brace;
                formatableValue = replacer.Replace(pair.value.Replace("{", "{{").Replace("}", "}}"), "{$1}");
                rawKey = pair.key;
                rawValue = pair.value;
            }
        }

    }
}
