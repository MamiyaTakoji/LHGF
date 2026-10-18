using System.Collections.Generic;
using UnityEngine;

namespace TexDrawLib
{
    // Box containing horizontal stack of child boxes.
    public class HorizontalBox : LayoutBox
    {
        public float totalWidth = 0f;
        private float leftMostPos = 0;
        private float rightMostPos = 0;

        public override Rect getBoundingRect => new Rect(leftMostPos, -height, rightMostPos - leftMostPos, depth + height);

        public static HorizontalBox Get(Box box, float width, TexAlignment alignment)
        {
            var Box = Get();
            if (box.width >= width)
            {
                Box.Add(box);
                return Box;
            }
            var extrawidth = Mathf.Max(width - box.width, 0);
            if (alignment == TexAlignment.Center)
            {
                var strutBox = StrutBox.Get(extrawidth / 2f, 0, 0);
                Box.Add(strutBox);
                Box.Add(box);
                Box.Add(strutBox);
            }
            else if (alignment == TexAlignment.Left)
            {
                Box.Add(box);
                Box.Add(StrutBox.Get(extrawidth, 0, 0));
            }
            else if (alignment == TexAlignment.Right)
            {
                Box.Add(StrutBox.Get(extrawidth, 0, 0));
                Box.Add(box);
            }
            return Box;
        }

        public static HorizontalBox Get(Box box, float width, float alignment)
        {
            var Box = Get();
            if (box.width == width)
            {
                Box.Add(box);
                return Box;
            }
            else
            {
                var excess = width - box.width;
                Box.Add(StrutBox.Get(excess * alignment, 0, 0));
                Box.Add(box);
                Box.Add(StrutBox.Get(excess * (1 - alignment), 0, 0));
                return Box;
            }
        }

        public static HorizontalBox Get(Box box, float shift = 0)
        {
            var Box = ObjPool<HorizontalBox>.Get();
            Box.shift = shift;
            Box.Add(box);
            return Box;
        }

        public static HorizontalBox Get()
        {
            return ObjPool<HorizontalBox>.Get();
        }

        public static HorizontalBox Get(IList<Box> box, int start = 0, int length = int.MaxValue)
        {
            var Box = ObjPool<HorizontalBox>.Get();
            var len = Mathf.Min(box.Count, length);
            var end = start + len;
            Box.children.EnsureCapacity(len);
            for (int i = start; i < end; i++)
                Box.Add(box[i]);
            return Box;
        }

        //Specific for DrawingParams
        public static HorizontalBox Get(List<Box> box)
        {
            var Box = ObjPool<HorizontalBox>.Get();
            Box.children.EnsureCapacity(box.Count);

            for (int i = 0; i < box.Count; i++)
                Box.Add(box[i]);
            ListPool<Box>.Release(box);
            return Box;
        }

        //Specific for DrawingParams
        public void AddRange(List<Box> box)
        {
            for (int i = 0; i < box.Count; i++)
                Add(box[i]);

            ListPool<Box>.Release(box);
        }

        public void AddRange(HorizontalBox box)
        {
            var ch = box.children;
            for (int i = 0; i < ch.Count; i++)
            {
                Add(ch[i]);
            }
        }

        public void AddRange(HorizontalBox box, int position)
        {
            var ch = box.children;
            for (int i = 0; i < ch.Count; i++)
            {
                Add(position++, ch[i]);
            }
        }

        private void AddToMetric(Box box)
        {
            if (children.Count == 0)
            {
                height = float.NegativeInfinity;
                depth = float.NegativeInfinity;
                leftMostPos = 0;
                rightMostPos = 0;
            }

            var xmax = box.getBoundingRect.xMax;
            leftMostPos = Mathf.Min(leftMostPos, totalWidth + box.getBoundingRect.xMin);
            rightMostPos = Mathf.Max(rightMostPos + Mathf.Min(0, xmax), totalWidth + xmax);
            totalWidth += box.width;

            height = Mathf.Max(height, box.height - box.shift);
            depth = Mathf.Max(depth, box.depth + box.shift);
            width = Mathf.Max(width + Mathf.Min(0, xmax), totalWidth);
        }

        public void Add(Box box)
        {
            AddToMetric(box);
            children.Add(box);
        }

        public void Add(int position, Box box)
        {
            AddToMetric(box);
            children.Insert(position, box);
        }

        public void Recalculate()
        {
            totalWidth = 0;
            width = 0;
            leftMostPos = 0;
            rightMostPos = 0;
            height = children.Count == 0 ? 0 : float.NegativeInfinity;
            depth = children.Count == 0 ? 0 : float.NegativeInfinity;
            for (int i = 0; i < children.Count; i++)
            {
                var box = children[i];
                var xmax = box.getBoundingRect.xMax;
                leftMostPos = Mathf.Min(leftMostPos, totalWidth + box.getBoundingRect.xMin);
                rightMostPos = Mathf.Max(rightMostPos + Mathf.Min(0, xmax), totalWidth + xmax);
                totalWidth += box.width;
                height = Mathf.Max(height, box.height - box.shift);
                depth = Mathf.Max(depth, box.depth + box.shift);
            }
            width = rightMostPos;
        }

        public override void Draw(TexRendererState state)
        {
            base.Draw(state);

            state.Push();

            for (int i = 0; i < children.Count; i++)
            {
                Box box = children[i];
                state.y -= box.shift;
                box.Draw(state);
                state.y += box.shift;
                state.x += box.width;
            }

            state.Pop();
        }

        public override void Flush()
        {
            ObjPool<HorizontalBox>.Release(this);
            totalWidth = 0;
            leftMostPos = 0;
            rightMostPos = 0;
            base.Flush();
        }
    }
}
