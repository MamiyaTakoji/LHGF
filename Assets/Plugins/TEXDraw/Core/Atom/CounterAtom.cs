using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{
    public class CounterAtom : AbstractAtom
    {

        public static CounterAtom Get()
        {
            return ObjPool<CounterAtom>.Get();
        }

        public int number = 1;

        public string code;

        public override CharType Type => CharTypeInternal.Invalid;
        public override CharType LeftType => Type;
        public override CharType RightType => Type;

        public override Box CreateBox(TexBoxingState state)
        {
            return StrutBox.Get(0,0,0);
        }

        public override void Flush()
        {
            ObjPool<CounterAtom>.Release(this);
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            if (command == "count")
            {
                code = "count" + LookForAWordOrDigit(value, ref position);

                if (state.Metadata.TryGetValue(code, out Atom atom) && atom is CounterAtom catom)
                {
                    number = catom.number;
                }
                else
                {
                    number = 0;
                    state.Metadata[code] = this;
                }
                if (position < value.Length && value[position] == '=')
                {
                    position++;
                    if (TexUtility.TryParse(LookForAWordOrDigit(value, ref position), out number))
                        (state.Metadata[code] as CounterAtom).number = number;
                }
            } else
            {
                var atom = state.parser.ParseToken(value, state, ref position);
                var code = (atom as CounterAtom)?.code ?? "";
                SkipWhiteSpace(value, ref position);
                var token = LookForAWordOrDigit(value, ref position);
                SkipWhiteSpace(value, ref position);
                var param1 = token == "by" ? LookForAWordOrDigit(value, ref position) : "0";
                TexUtility.TryParse(param1, out int parsedParam1);
                if (state.Metadata.TryGetValue(code, out Atom atom2) && atom2 is CounterAtom)
                {
                    var catom = atom2 as CounterAtom;
                    number = catom.number = processByCommand(command, catom.number, parsedParam1);
                }
                else
                {
                    number = processByCommand(command, 0, parsedParam1);
                    state.Metadata[code] = this;
                }
            }
        }

        static int processByCommand(string command, int n, int p)
        {
            switch (command)
            {
                case "advance":
                    return n + p;
                case "multiply":
                    return n * p;
                case "divide":
                    return n / p;
                default:
                    return n;
            }
        }
    }
}
