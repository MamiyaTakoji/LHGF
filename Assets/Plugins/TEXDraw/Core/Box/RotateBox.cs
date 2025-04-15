using UnityEngine;
using System.Collections.Generic;

namespace TexDrawLib
{
    public class RotateBox : Box
    {
        public float rotation;
        public Box box;
        public List<int> counts = new List<int>();

        public static LinkBox Get(Box baseBox, string key, float margin)
        {
            var box = ObjPool<LinkBox>.Get();
            box.key = key;
            box.box = baseBox;
            box.margin = margin;

            box.Set(baseBox.width, baseBox.height, baseBox.depth, 0);

            return box;
        }

        public override void Draw(TexRendererState state)
        {
            counts.Clear();
            for (int i = 0; i < state.vertexes.Count; i++)
            {
                counts.Add(state.vertexes[i].m_Colors.Count);
            }
            box.Draw(state);
            // count the diff
            for (int i = 0; i < state.vertexes.Count; i++)
            {
                if (i >= counts.Count)
                    counts.Add(state.vertexes[i].m_Colors.Count);
                else
                    counts[i] = state.vertexes[i].m_Colors.Count - counts[i];
            }

            // mix colors from cache
            Debug.Assert(state.vertexes.Count == counts.Count);
            for (int i = 0; i < state.vertexes.Count; i++)
            {
                var count = state.vertexes[i].m_Colors.Count;
                for (int k = count - counts[i]; k < count; k++)
                {
                    //state.vertexes[i].m_Colors[k] = TexUtility.MultiplyColor(state.vertexes[i].m_Colors[k], tint);
                }
            }
        }

        public override void Flush()
        {
            ObjPool<RotateBox>.Release(this);
            counts.Clear();
            rotation = 0;
            if (box != null)
            {
                box.Flush();
                box = null;
            }
        }
    }
}
