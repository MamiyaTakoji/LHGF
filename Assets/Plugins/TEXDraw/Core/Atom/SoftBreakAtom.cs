using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{
    public interface ISoftBreak
    {
    }

    public class HyphenBreak : CharAtom
    {
        public bool show = false;

        public static HyphenBreak Get()
        {
            return ObjPool<HyphenBreak>.Get();
        }

        public void Copy(CharAtom ch, bool flush = true)
        {
            this.character = ch.character;
            this.color = ch.color;
            this.fontIndex = ch.fontIndex;
            this.resolution = ch.resolution;
            this.size = ch.size;
            if (flush)
            {
                ch.Flush();
            }
        }

        public override Box CreateBox(TexBoxingState state)
        {
            return show ? base.CreateBox(state) : StrutBox.Empty;
        }

        public Box CreateBoxEnforcedShow(TexBoxingState state)
        {
            return base.CreateBox(state);
        }

        public override void Flush()
        {
            ObjPool<HyphenBreak>.Release(this);
            show = false;
            character = default;
            fontIndex = -1;
            size = 0;
            resolution = 1;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            SkipWhiteSpace(value, ref position);
            if (this.character == default)
            {
                Copy(Get('-', state));
            }
        }
    }

    public class SoftBreakAtom : AbstractAtom, ISoftBreak
    {
        public float extraSpace;

        public static SoftBreakAtom Get()
        {
            return ObjPool<SoftBreakAtom>.Get();
        }

        public override Box CreateBox(TexBoxingState state)
        {
            return StrutBox.Empty;
        }

        public override void Flush()
        {
            extraSpace = 0;
            ObjPool<SoftBreakAtom>.Release(this);
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            SkipWhiteSpace(value, ref position);
            if (position < value.Length && value[position] == '[')
            {
                extraSpace = TexUtility.ParseUnit(ReadGroup(value, ref position, '[', ']'), state);
            }
        }
    }

    public class ParagraphBreakAtom : SoftBreakAtom
    {
        public static new ParagraphBreakAtom Get()
        {
            return ObjPool<ParagraphBreakAtom>.Get();
        }

        public override void Flush()
        {
            extraSpace = 0;
            ObjPool<ParagraphBreakAtom>.Release(this);
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            SkipWhiteSpace(value, ref position);
        }
    }

    public class VerticalSkipAtom : SoftBreakAtom
    {

        public static new VerticalSkipAtom Get()
        {
            return ObjPool<VerticalSkipAtom>.Get();
        }

        public override Box CreateBox(TexBoxingState state)
        {
            return StrutBox.Get(0, extraSpace, 0);
        }

        public override void Flush()
        {
            extraSpace = 0;
            ObjPool<VerticalSkipAtom>.Release(this);
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            SkipWhiteSpace(value, ref position);
            if (position < value.Length)
            {
                extraSpace = TexUtility.ParseUnit(value, ref position, state);
            }
        }
    }
}
