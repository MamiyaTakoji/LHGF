using System;
using System.Linq;

namespace TexDrawLib
{
    public class GroupAtom : BlockAtom
    {
        public static GroupAtom Get()
        {
            return ObjPool<GroupAtom>.Get();
        }

        public override void Flush()
        {
            ObjPool<GroupAtom>.Release(this);
            base.Flush();
        }

        static readonly char[] trimmedChar = new char[] { '\n', '\r' };

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            if (command == "$$")
            {
                ProcessGroup(state, value, ref position, command, command, state.parser.environmentGroups["math"]);
            }
            else if (command == "[")
            {
                ProcessGroup(state, value, ref position, "\\[", "\\]", state.parser.environmentGroups["math"]);
            }
            else if (command == "begin")
            {
                var name = TexParserUtility.LookForAToken(value, ref position);
                if (string.IsNullOrEmpty(name))
                    return;
                if (state.parser.environmentGroups.TryGetValue(name, out GroupState kind))
                {
                    var beginPair = "\\begin{" + name + "}";
                    var endPair = "\\end{" + name + "}";
                    ProcessGroup(state, value, ref position, beginPair, endPair, kind);
                } else if (!name.Contains(' '))
                {
                    var beginPair = "\\begin{" + name + "}";
                    var endPair = "\\end{" + name + "}";
                    ProcessGroup(state, value, ref position, beginPair, endPair, new GroupState());
                }
            }
            else if (command == "begingroup")
            {
                var beginPair = "\\begingroup";
                var endPair = "\\endgroup";
                ProcessGroup(state, value, ref position, beginPair, endPair, new GroupState());
            }

        }

        private void ProcessGroup(TexParserState state, string value, ref int position, string beginPair, string endPair, in GroupState kind)
        {
            state.PushStates();
            kind.beginState?.Invoke(state);

            var pos = position - beginPair.Length;
            if (pos >= 0)
            {
                string document = TexParserUtility.ReadStringGroup(value, ref pos, beginPair, endPair);
                var processedDocument = document.Trim(trimmedChar);

                pos = kind.parameterPrepocessor?.Invoke(state, value, 0) ?? 0;
                if (kind.interpreter != null)
                    atom = kind.interpreter(state, processedDocument, pos);
                else
                    atom = state.parser.Parse(processedDocument, state, false, ref pos);
                position += document.Length + endPair.Length;
            } 

            kind.endState?.Invoke(state);
            state.PopStates();
            TexParserUtility.SkipWhiteSpace(value, ref position);
        }

        public override Atom Unpack() => atom;

        public readonly struct GroupState
        {
            public readonly string name;
            public readonly Action<TexParserState> beginState;
            public readonly Action<TexParserState> endState;
            public readonly Func<TexParserState, string, int, int> parameterPrepocessor;
            public readonly Func<TexParserState, string, int, Atom> interpreter;

            public GroupState(string name, Action<TexParserState> beginState = null, Action<TexParserState> endState = null, Func<TexParserState, string, int, int> parameterPrepocessor = null, Func<TexParserState, string, int, Atom> interpreter = null)
            {
                this.name = name;
                this.beginState = beginState;
                this.endState = endState;
                this.parameterPrepocessor = parameterPrepocessor;
                this.interpreter = interpreter;
            }
        }


    }
}
