using UnityEngine;
using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{

    public class InputAtom : BlockAtom
    {
        public TextAsset file;

        public override Atom Unpack() => atom;

        public static InputAtom Get()
        {
            return ObjPool<InputAtom>.Get();
        }

        public override void Flush()
        {
            ObjPool<InputAtom>.Release(this);
            base.Flush();
            file = null;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            var path = LookForAToken(value, ref position);
            file = Resources.Load<TextAsset>(path);
            if (file)
            {
                atom = state.parser.Parse(file.text, state);
                ((DocumentAtom)atom).mergeable = true;
            }
        }
    }
}
