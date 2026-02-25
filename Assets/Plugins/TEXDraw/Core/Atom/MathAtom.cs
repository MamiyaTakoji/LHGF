using System;

namespace TexDrawLib
{
    public class MathAtom : RowAtom
    {
        float alignment;
        float thin, medium, thick;
        public bool scripted = false, inlined = false;

        public static new MathAtom Get()
        {
            return ObjPool<MathAtom>.Get();
        }

        public float translateGlueCode(int code)
        {
            switch (code)
            {
                case 0:
                    return 0;
                case 1:
                    return thin;
                case 2:
                    return scripted ? thin : medium;
                case 3:
                default:
                    return scripted ? thin : thick;
            }
        }

        public override void Add(Atom atom)
        {
            if (atom is SymbolAtom sy && sy.metadata.symbol == "minus")
            {
                // minus special case
                if (children.Count == 0 || (children[children.Count - 1] is SymbolAtom psy && psy.Type != CharType.Ordinary))
                    atom = OverridedTypeAtom.Get(sy, CharType.Ordinary);
            }
            base.Add(atom);
        }

        public override Box CreateBox(TexBoxingState state)
        {
            var box = HorizontalBox.Get();
            if (children.Count == 1 && children[0] is SymbolAtom satom && (satom.metadata.larger.Has || satom.metadata.extension.enabled))
            {
                if (!state.restricted)
                {
                    box.Add(satom.CreateBoxMinWidth(state.width));
                    return box;
                } 
                else if (!state.interned)
                {
                    box.Add(satom.CreateBoxMinHeight(state.height));
                    return box;
                }
            }
            state.Push();
            state.restricted = true;
            var lastAtomType = CharTypeInternal.Inner;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is SpaceAtom sa && sa.width == 0) 
                    continue; 
                if (i > 0)
                {
                    var curAtomType = children[i].LeftType;
                    var glue = translateGlueCode(TEXPreference.main.GetGlue(lastAtomType, curAtomType));
                    if (glue > 0)
                    {
                        var glueBox = StrutBox.Get(glue, 0, 0);
                        box.Add(glueBox);
                    }
                }
                lastAtomType = children[i].RightType;
                box.Add(children[i].CreateBox(state));
            }
            state.Pop();
            if (state.restricted)
                return CheckBox(box);
            else if (!inlined && !FlexibleAtom.HandleFlexiblesHorizontal(box, state.width))
                return HorizontalBox.Get(CheckBox(box), state.width, alignment);
            else 
                return CheckBox(box);
        }

        float minHeight, minDepth;

        Box CheckBox(HorizontalBox b)
        {
            if (!inlined)
            {
                b.height = Math.Max(b.height, minHeight) + lineSpace / 2;
                b.depth = Math.Max(b.depth, minDepth) + lineSpace / 2;
            }
            return b;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            var r = state.Ratio;
            alignment = state.Paragraph.alignment;
            thin = state.Math.thinSpace * r;
            medium = state.Math.mediumSpace * r;
            thick = state.Math.thickSpace * r;
            var style = state.Environment.current.GetMathStyle();
            minHeight = style >= TexMathStyle.Script ? 0 : state.Typeface.lineAscent * r;
            minDepth = style >= TexMathStyle.Text ? 0 : state.Typeface.lineDescent * r;
            scripted = style >= TexMathStyle.Script;
            inlined = state.Environment.current.IsInline();
            lineSpace = ((!inlined ? state.Paragraph.paragraphSpacing : 0) + state.Paragraph.lineSpacing) * r;

            base.ProcessParameters(command, state, value, ref position);
        }

        public override void Flush()
        {
            ObjPool<MathAtom>.Release(this);
            base.Flush();
        }
    }
}
