using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{
    public class VerbAtom : BlockAtom
    {
        public override Atom Unpack() => atom;

        public static VerbAtom Get()
        {
            return ObjPool<VerbAtom>.Get();
        }

        public static VerbAtom Get(string command, TexParserState state)
        {
            return ObjPool<VerbAtom>.Get();
        }

        public override void Flush()
        {
            ObjPool<VerbAtom>.Release(this);
            base.Flush();
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            if (value != null && position < value.Length)
            {
                var delimiter = value[position];
                var verb = ReadGroup(value, ref position, delimiter, delimiter);
                state.PushStates();
                state.parser.environmentGroups["verbatim"].beginState(state);
                atom = state.parser.environmentGroups["verbatim"].interpreter(state, verb, 0);
                ((DocumentAtom)atom).mergeable = true;
                state.PopStates();
            }
        }
    }
}
