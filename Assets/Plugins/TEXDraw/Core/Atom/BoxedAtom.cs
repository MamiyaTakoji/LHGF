using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{
    public class BoxedAtom : InlineAtom
    {
        public string mode;
        public float size;
        public float median;
        public bool relative = true;

        public static BoxedAtom Get()
        {
            return ObjPool<BoxedAtom>.Get();
        }

        public static BoxedAtom Get(Atom a)
        {
            var b = ObjPool<BoxedAtom>.Get();
            b.atom = a;
            return b;
        }
        float mix(float boxSize)
        {
            if (relative)
                return size + boxSize;
            else
                return System.Math.Max(boxSize, size);
        }

        public override Box CreateBox(TexBoxingState state)
        {
            switch (mode)
            {
                case "hbox":
                    state.Push();
                    state.restricted = relative;
                    if (size > 0)
                        state.width = size;
                    var box = atom.CreateBox(state);
                    var hbox = HorizontalBox.Get(box, mix(box.width), TexAlignment.Left);
                    state.Pop();
                    return hbox;
                case "vbox":
                case "vcenter":
                case "vtop":
                default:
                    var vbox = VerticalBox.Get();
                    state.Push();
                    state.interned = relative;
                    if (size > 0)
                        state.height = size;
                    box = atom.CreateBox(state);
                    vbox.Add(box);
                    if (mode == "vbox")
                        vbox.height = mix(vbox.TotalHeight) - median;
                    else if (mode == "vcenter")
                    {
                        vbox.height = mix(vbox.TotalHeight) - vbox.depth - median;
                        TexUtility.CentreBox(vbox, median);
                    }
                    else if (mode == "vtop") 
                        vbox.depth = mix(vbox.TotalHeight) - vbox.height;
                    state.Pop();
                    return vbox;
            }
        }

        public override void Flush()
        {
            size = 0;
            relative = true;
            median = 0;
            ObjPool<BoxedAtom>.Release(this);
            atom?.Flush();
            atom = null;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            mode = command;
            median = state.Typeface.lineMedian * state.Ratio;
            if (value == null) return;
            SkipWhiteSpace(value, ref position);
            if (position < value.Length && value[position] != beginGroupChar)
            {
                var key = LookForAWord(value, ref position);
                if (key == "by" || key == "spread")
                {
                    relative = key == "spread";
                    SkipWhiteSpace(value, ref position);
                    size = TexUtility.ParseUnit(value, ref position, state);
                    SkipWhiteSpace(value, ref position);
                }
            }
            atom = state.parser.Parse(ReadGroup(value, ref position), state, true);
        }
    }

    public class RaiseAtom : BoxedAtom
    {
        public float? depth, height;

        public static new RaiseAtom Get()
        {
            return ObjPool<RaiseAtom>.Get();
        }

        public override Box CreateBox(TexBoxingState state)
        {
            var b = HorizontalBox.Get(atom?.CreateBox(state) ?? StrutBox.Empty);
            b.shift = size;
            if (height != null)
                b.height = (float)height;
            if (depth != null)
                b.depth = (float)depth;
            return b;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            SkipWhiteSpace(value, ref position);
            if (position < value.Length && value[position] == '{')
            {
                // note the minus! 
                size = -TexUtility.ParseUnit(ReadGroup(value, ref position), state);
                SkipWhiteSpace(value, ref position);
            }
            if (position < value.Length && value[position] == '[')
            {
                height = TexUtility.ParseUnit(ReadGroup(value, ref position), state);
                SkipWhiteSpace(value, ref position);
            }
            if (position < value.Length && value[position] == '[')
            {
                depth = TexUtility.ParseUnit(ReadGroup(value, ref position), state);
                SkipWhiteSpace(value, ref position);
            }
            if (position < value.Length && value[position] == '{')
            {
                atom = state.parser.ParseToken(value, state, ref position);
            }
        }

        public override void Flush()
        {
            ObjPool<RaiseAtom>.Release(this);
            depth = height = null;
            base.Flush();
        }
    }
}
