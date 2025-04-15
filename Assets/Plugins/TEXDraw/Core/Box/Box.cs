using UnityEngine;

namespace TexDrawLib
{
    // Represents graphical box that is part of math expression, and can itself contain child boxes.
    public abstract class Box : IFlushable
    {
        protected Box() { }

        public float TotalHeight { get { return height + depth; } }

        public float width, height, depth, shift;

        public virtual Rect getBoundingRect => new Rect(0, -height, width, depth + height); // topleft to downright

        public void Set(float w, float h, float d, float s = 0)
        {
            width = w;
            height = h;
            depth = d;
            shift = s;
        }

        public abstract void Draw(TexRendererState state);

        public abstract void Flush();

        public bool IsFlushed { get; set; }

        public override string ToString()
        {
            return base.ToString().Replace("TexDrawLib.", string.Empty) +
                string.Format(" H:{0:F2} D:{1:F2} W:{2:F2} S:{3:F2}", height, depth, width, shift);
        }
    }
}
