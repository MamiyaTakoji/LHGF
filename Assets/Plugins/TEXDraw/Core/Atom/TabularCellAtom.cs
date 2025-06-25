using UnityEngine;

namespace TexDrawLib
{
    public class TabularCellAtom : BlockAtom
    {
        public (float borderWidth, int borderStyle, Color32 color, float padding) top, left, bottom, right;
        public (int span, float alignment, float size) x = (1, 0, 0), y = (1, 0, 0);
        public Color32 background;


        public override CharType Type => CharTypeInternal.Inner;
        public override CharType LeftType => Type;
        public override CharType RightType => Type;

        Box cacheBox;
        Vector3 cacheMetric;
        Vector3 computedMetric;
        public Vector2? overrideMetric;

        public override Box CreateBox(TexBoxingState state)
        {
            if (cacheBox == null)
                cacheBox = StrutBox.Empty;
            cacheBox.width = Mathf.Min(cacheBox.width, cacheMetric[0]);
            cacheBox.height = Mathf.Min(cacheBox.height, cacheMetric[1]);
            cacheBox.depth = Mathf.Min(cacheBox.depth, cacheMetric[2]);
            var hbox = HorizontalBox.Get(cacheBox ?? StrutBox.Empty, cacheMetric.x, x.alignment);
            // Horizontal
            hbox.Add(0, StrutBox.Get(left.padding, 0, 0));
            hbox.Add(StrutBox.Get(right.padding, 0, 0));
            var vbox = VerticalBox.Get(hbox, cacheMetric.y, cacheMetric.z);
            vbox.Add(0, StrutBox.Get(0, top.padding, 0));
            vbox.Add(StrutBox.Get(0, bottom.padding, 0));

            var hhbox = HorizontalBox.Get(vbox);
            RuleBox.PutBorderVertical(hhbox, left.color, left.borderWidth, left.borderStyle, true);
            RuleBox.PutBorderVertical(hhbox, right.color, right.borderWidth, right.borderStyle, false);
            var vvbox = VerticalBox.Get(hhbox);
            RuleBox.PutBorderHorizontal(vvbox, top.color, top.borderWidth, top.borderStyle, true);
            RuleBox.PutBorderHorizontal(vvbox, bottom.color, bottom.borderWidth, bottom.borderStyle, false);
            if (background.a > 0)
            {
                hhbox.Add(0, StrutBox.Get(-hhbox.width, 0, 0));
                hhbox.Add(0, RuleBox.Get(background, hhbox.width, hhbox.height, hhbox.depth));
            }
            return vvbox;
        }

        public bool AssignFinalMetric(Vector3 size)
        {
            cacheMetric = size - excessSize;
            if (cacheBox != null)
            {
                var compare = computedMetric;
                if (compare.x > size.x || compare.y > size.y || compare.z > size.z)
                {
                    overrideMetric = new Vector2(size.x, size.y);
                    return true; // the old is overflowing, recalculation needed
                }
            }
            return false;
        }

        public Vector3 PrecomputeBox(TexBoxingState state)
        {
            state.Push();

            if (overrideMetric.HasValue)
            {
                state.width = overrideMetric.Value.x;
                state.height = overrideMetric.Value.y;
                state.restricted = false;
                state.interned = false;
            } else
            {
                if (x.size > float.Epsilon)
                {
                    state.width = x.size;
                    state.restricted = false;
                }
                if (y.size > float.Epsilon)
                {
                    state.height = y.size;
                    state.interned = false;
                }
            }
            cacheBox = atom?.CreateBox(state);
            state.Pop();
            computedMetric = (cacheBox is null ? Vector3.zero : new Vector3(cacheBox.width, cacheBox.height, cacheBox.depth));
            return computedMetric + excessSize;
        }

        Vector3 excessSize => new Vector3(
            left.padding + right.padding + left.borderWidth * left.borderStyle * 2 + right.borderWidth * right.borderStyle * 2,
            top.padding + top.borderWidth * (top.borderStyle * 2 - 1), bottom.padding + bottom.borderWidth * (bottom.borderStyle * 2 - 1));

        public override void Flush()
        {
            ObjPool<TabularCellAtom>.Release(this);
            background.a = 0;
            top = left = bottom = right = default;
            x = y = (1, 0, 0);
            cacheBox?.Flush();
            cacheBox = null;
            overrideMetric = null;
            cacheMetric = Vector3.zero;
            base.Flush();
        }

        internal static TabularCellAtom Get(Atom container)
        {
            var atom = ObjPool<TabularCellAtom>.Get();
            atom.atom = container;
            return atom;
        }

        internal static TabularCellAtom Get()
        {
            var atom = ObjPool<TabularCellAtom>.Get();
            return atom;
        }

        internal static Vector2 currentTableSpace;
        internal static Vector2 currentAlignment;

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            if (state.Metadata.TryGetValue("columncolor", out Atom catom) && catom is ColorAtom ccatom)
            {
                background = ccatom.color;
            }
            left.padding = right.padding = currentTableSpace.x;
            top.padding = bottom.padding = currentTableSpace.y;
            x.alignment = currentAlignment.x;
            y.alignment = currentAlignment.y;
        }
    }
}
