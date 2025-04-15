namespace TexDrawLib
{
    public class SpaceAtom : AbstractAtom
    {
        public float width, height, depth;

        public static SpaceAtom Empty => ObjPool<SpaceAtom>.Get();

        public static SpaceAtom Get()
        {
            return Empty;
        }

        public static SpaceAtom Get(string command, TexParserState state)
        {
            var atom = ObjPool<SpaceAtom>.Get();
            atom.ProcessParameters(command, state);
            return atom;
        }

        public static SpaceAtom Get(float width, float height, float depth)
        {
            var atom = ObjPool<SpaceAtom>.Get();
            atom.width = width;
            atom.height = height;
            atom.depth = depth;
            return atom;
        }


        public override Box CreateBox(TexBoxingState state)
        {
            return StrutBox.Get(width, height, depth);
        }

        public override void Flush()
        {

            width = height = depth = 0;
            ObjPool<SpaceAtom>.Release(this);
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            var ratio = state.Ratio;
            
            width = DetermineWidth(command, state) * ratio;
            height = state.Typeface.lineAscent * ratio;
            depth = state.Typeface.lineDescent * ratio;
        }

        float DetermineWidth(string command, TexParserState state)
        {
            switch (command)
            {
                case "qquad":
                    return (state.Typeface.lineAscent) * 2f;
                case "quad":
                    return (state.Typeface.lineAscent);
                case "enskip":
                    return state.Typeface.blankSpaceWidth * 2f;
                case ",":
                    return state.Math.thinSpace;
                case ":":
                    return state.Math.mediumSpace;
                case ";":
                    return state.Math.thickSpace;
                case "!":
                    return -state.Math.thinSpace;
                case "w":
                    return state.Typeface.blankSpaceWidth;
                case "none":
                    return 0;
                case " ":
                default:
                    return state.Environment.current.IsMathMode() ? 0 : state.Typeface.blankSpaceWidth;
            }
        }

    }

    public class KernAtom : SpaceAtom
    {
        public static new KernAtom Get()
        {
            return ObjPool<KernAtom>.Get();
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            var ratio = state.Ratio;

            TexParserUtility.SkipWhiteSpace(value, ref position);
            width = TexUtility.ParseUnit(value, ref position, state);
            height = state.Typeface.lineAscent * ratio;
            depth = state.Typeface.lineDescent * ratio;
        }
        public override void Flush()
        {
            ObjPool<KernAtom>.Release(this);
            base.Flush();
        }
    }
}
