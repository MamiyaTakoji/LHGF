using UnityEngine;

namespace TexDrawLib
{
    public class HorizontalRuleAtom : AbstractAtom
    {
        public float top;
        public float bottom;
        public float thickness;
        public Color32 color;

        public static HorizontalRuleAtom Get()
        {
            return ObjPool<HorizontalRuleAtom>.Get();
        }

        public override Box CreateBox(TexBoxingState state)
        {
            var vbox = VerticalBox.Get();
            if (top > 0)
                vbox.Add(StrutBox.Get(0, top, 0));
            if (thickness > 0)
                vbox.Add(RuleBox.Get(color, state.width, thickness, 0));
            if (bottom > 0)
                vbox.Add(StrutBox.Get(0, top, 0));
            return vbox;
        }

        public override void Flush()
        {
            top = bottom = thickness = 0;
            ObjPool<HorizontalRuleAtom>.Release(this);
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            color = state.Color.current;
            switch (command)
            {
                case "toprule":
                    thickness = state.Math.lineThickness * state.Ratio * 2;
                    bottom = state.Paragraph.lineSpacing;
                    break;
                case "bottomrule":
                    thickness = state.Math.lineThickness * state.Ratio * 2;
                    top = state.Paragraph.lineSpacing;
                    break;
                case "midrule":
                    thickness = state.Math.lineThickness * state.Ratio;
                    bottom = top = state.Paragraph.lineSpacing;
                    break;
                case "hline":
                default:
                    thickness = state.Math.lineThickness * state.Ratio;
                    break;
            }
        }
    }
}
