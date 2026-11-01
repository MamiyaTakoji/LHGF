namespace TexDrawLib
{
    public class AlignmentAtom : AbstractAtom
    {

        public static AlignmentAtom Get()
        {
            return ObjPool<AlignmentAtom>.Get();
        }

        public static AlignmentAtom Get(string command, TexParserState state)
        {
            return ObjPool<AlignmentAtom>.Get();
        }

        public override Box CreateBox(TexBoxingState state)
        {
            return StrutBox.Empty;
        }

        public override void Flush()
        {
            ObjPool<AlignmentAtom>.Release(this);
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {

        }
    }
}
