using UnityEngine;
using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{
    public class BoxedLinkAtom : InlineAtom
    {
        public static BoxedLinkAtom Get()
        {
            var atom = ObjPool<BoxedLinkAtom>.Get();
            return atom;
        }

        public float margin;
        public float lineThickness;
        public bool underline;
        public string key;
        public Color32 color;

        public override Box CreateBox(TexBoxingState state)
        {
            if (atom == null)
                return StrutBox.Empty;
            else
            {
                if (underline)
                {
                    var baseBox = atom.CreateBox(state);
                    var box = HorizontalBox.Get(baseBox);

                    box.Add(StrutBox.Get(-box.width, 0, 0));
                    box.Add(StrikeBox.Get(
                        color, baseBox.height, baseBox.width, baseBox.depth,
                            0, lineThickness, StrikeBox.StrikeMode.underline,
                            0, 0, 0, 0, 0));

                    return LinkBox.Get(box, key, margin);
                }
                else
                    return LinkBox.Get(atom.CreateBox(state), key, margin);
            }
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            var r = state.Ratio;
            margin = state.Math.lineThickness * r;
            lineThickness = state.Math.lineThickness * r;
            underline = command == "href";
            key = string.Empty;
            color = state.Color.current;
            SkipWhiteSpace(value, ref position);
            if (position < value.Length && value[position] == '[')
            {
                key = ReadGroup(value, ref position, '[', ']');
            }
            var thegroup = ReadGroup(value, ref position);
            if (key.Length == 0)
                key = thegroup;
            atom = state.parser.Parse(thegroup, state, true);
        }

        public override void Flush()
        {
            ObjPool<BoxedLinkAtom>.Release(this);
            base.Flush();
        }
    }
}
