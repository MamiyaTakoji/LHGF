using System;
using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{
    public sealed class DocumentAtom : LayoutAtom
    {

        public bool mergeable = false;

        public static DocumentAtom Get()
        {
            return ObjPool<DocumentAtom>.Get();
        }

        public static DocumentAtom GetAsBlock(Atom atom, TexParserState state)
        {
            var doc = Get();
            var row = RowAtom.Get();
            state.Environment.Push(TexEnvironment.Inline, false);
            row.ProcessParameters(null, state);
            state.Environment.Pop();
            row.Add(FlexibleAtom.Get(new FlexibleMetric(0, state.Paragraph.alignment, state.Paragraph.alignment, 1, 1)));
            row.Add(atom);
            row.Add(FlexibleAtom.Get(new FlexibleMetric(0, 1-state.Paragraph.alignment, 1 - state.Paragraph.alignment, 1, 1)));
            doc.Add(row);
            return doc;
        }

        public override void Flush()
        {
            base.Flush();
            mergeable = false;
            ObjPool<DocumentAtom>.Release(this);
        }

        public override Box CreateBox(TexBoxingState state)
        {
            var box = VerticalBox.Get();
            for (int i = 0; i < children.Count; i++)
            {
                box.Add(children[i].CreateBox(state));
            }
            FlexibleAtom.HandleFlexiblesVertical(this, box, state.height);
            return box;
        }


        // Not multi-thread-safe
        static readonly string[] temp = new string[10];

        public override CharType LeftType { get; }
        public override CharType RightType { get; }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            if (state.parser.macroCommands.TryGetValue(command, out InlineMacroAtom.ParametrizedMacro macro))
            {
                var count = Math.Min(temp.Length, macro.bracetedParameters + macro.bracketedParameters);
                for (int i = 1; i <= count; i++)
                {
                    SkipWhiteSpace(value, ref position);

                    if (i <= macro.bracketedParameters)
                    {
                        if (position < value.Length && value[position] == '[')
                            temp[i] = ReadGroup(value, ref position, '[', ']');
                        else
                            continue;
                    }
                    else
                    {
                        temp[i] = LookForALetter(value, ref position);
                    }
                }
                var a = state.parser.Parse(string.Format(macro.formatableValue, temp), state);
                children.AddRange(a.children);
                a.children.Clear();
                a.Flush();
                SkipWhiteSpace(value, ref position);
            }
        }
    }
}
