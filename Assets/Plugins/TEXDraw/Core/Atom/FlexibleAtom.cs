using System;
using UnityEngine;

namespace TexDrawLib
{
    public readonly struct FlexibleMetric
    {
        public readonly float size, plus, minus;
        public readonly int plusLevel, minusLevel;

        public FlexibleMetric(float size, float plus, float minus, int plusLevel, int minusLevel)
        {
            this.size = size;
            this.plus = plus;
            this.minus = minus;
            this.plusLevel = plusLevel;
            this.minusLevel = minusLevel;
        }
    }

    public class FlexibleBox : StrutBox
    {
        public FlexibleMetric x, y;

        public static FlexibleBox Get()
        {
            return ObjPool<FlexibleBox>.Get();
        }

        public static FlexibleBox Get(FlexibleMetric x, FlexibleMetric y)
        {
            var obj = Get();
            obj.x = x;
            obj.y = y;
            obj.width = x.size;
            obj.height = y.size;
            return obj;
        }

        public override void Flush()
        {
            ObjPool<FlexibleBox>.Release(this);
            x = y = default;
            width = height = depth = shift = 0;
        }

    }

    public sealed class FlexibleAtom : AbstractAtom
    {
        public FlexibleMetric x, y;

        public static FlexibleAtom Get()
        {
            return ObjPool<FlexibleAtom>.Get();
        }

        public static FlexibleAtom Get(FlexibleMetric x, FlexibleMetric y = default)
        {
            var atom = ObjPool<FlexibleAtom>.Get();
            atom.x = x;
            atom.y = y;
            return atom;
        }

        public override Box CreateBox(TexBoxingState state)
        {
            return FlexibleBox.Get(x, y);
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            switch (command)
            {
                case "hskip":
                case "vskip":
                case "hfil":
                    x = new FlexibleMetric(0, 1, 0, 1, 0);
                    break;
                case "vfil":
                    y = new FlexibleMetric(0, 1, 0, 1, 0);
                    break;
                case "hss":
                    x = new FlexibleMetric(0, 1, 1, 1, 1);
                    break;
                case "vss":
                    y = new FlexibleMetric(0, 1, 1, 1, 1);
                    break;
                case "hfill":
                    x = new FlexibleMetric(0, 1, 0, 2, 0);
                    break;
                case "vfill":
                    y = new FlexibleMetric(0, 1, 0, 2, 0);
                    break;
                case "hfilneg":
                case "vfilneg":
                    break;
            }
        }

        public static bool HandleFlexiblesHorizontal(HorizontalBox box, float desiredSize)
        {
            float excess = desiredSize - box.width;
            // Check applicable levels & total distribution
            int targetLevel = 0;
            float distribution = 0;
            for (int i = 0; i < box.children.Count; i++)
            {
                if (box.children[i] is FlexibleBox flex)
                {
                    int thisLevel = excess > 0 ? flex.x.plusLevel : flex.x.minusLevel;
                    float thisAmount = excess > 0 ? flex.x.plus : flex.x.minus;
                    if (targetLevel < thisLevel) {
                        targetLevel = thisLevel;
                        distribution = 0;
                    }
                    distribution += thisAmount;
                }
            }
            if (distribution <= 0) return false;
            // Go nuke
            for (int i = 0; i < box.children.Count; i++)
            {
                if (box.children[i] is FlexibleBox flex && (excess > 0 ? flex.x.plusLevel : flex.x.minusLevel) == targetLevel)
                {
                    float add = (excess > 0 ? flex.x.plus : flex.x.minus) / distribution * excess;
                    (box.children[i] as StrutBox).width += add;
                }
            }
            box.Recalculate();
            return true;
        }

        public static void HandleFlexiblesVertical(LayoutAtom atom, VerticalBox box, float desiredSize)
        {
            Debug.Assert(atom.children.Count == box.children.Count);
            float excess = desiredSize - box.width;
            // Check applicable levels
            int targetLevel = 0;
            for (int i = 0; i < atom.children.Count; i++)
            {
                if (atom.children[i] is FlexibleAtom flex)
                {
                    targetLevel = Math.Max(targetLevel, excess > 0 ? flex.y.plusLevel : flex.y.minusLevel);
                }
            }
            // Check total distribution
            float distribution = 0;
            for (int i = 0; i < atom.children.Count; i++)
            {
                if (atom.children[i] is FlexibleAtom flex && (excess > 0 ? flex.y.plusLevel : flex.y.minusLevel) == targetLevel)
                {
                    distribution += excess > 0 ? flex.y.plus : flex.y.minus;
                }
            }
            if (distribution == 0) return;
            // Go nuke
            for (int i = 0; i < atom.children.Count; i++)
            {
                if (atom.children[i] is FlexibleAtom flex && (excess > 0 ? flex.y.plusLevel : flex.y.minusLevel) == targetLevel)
                {
                    (box.children[i] as StrutBox).height += (excess > 0 ? flex.y.plus : flex.y.minus) / distribution * excess;
                }
            }
        }

        public override void Flush()
        {
            ObjPool<FlexibleAtom>.Release(this);
            x = y = default;
        }
    }
}
