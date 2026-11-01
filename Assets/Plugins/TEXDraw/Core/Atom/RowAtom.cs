namespace TexDrawLib
{
    public class RowAtom : LayoutAtom
    {
        public float lineSpace;

        public static RowAtom Get()
        {
            return ObjPool<RowAtom>.Get();
        }

        public override void Flush()
        {
            ObjPool<RowAtom>.Release(this);
            base.Flush();
        }

        public override Box CreateBox(TexBoxingState state)
        {
            var box = HorizontalBox.Get();
            for (int i = 0; i < children.Count; i++)
            {
                box.Add(children[i].CreateBox(state));
            }
            FlexibleAtom.HandleFlexiblesHorizontal(box, state.width);
            box.height += lineSpace / 2;
            box.depth += lineSpace / 2;
            return box;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            var inlined = state.Environment.current.IsInline();
            lineSpace = ((!inlined ? state.Paragraph.paragraphSpacing : 0) + state.Paragraph.lineSpacing) * state.Ratio;
            // DocumentAtom has no parameters
        }
    }
}
