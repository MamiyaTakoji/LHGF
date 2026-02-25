using System.Collections.Generic;

using UnityEngine;

namespace TexDrawLib
{
    public abstract class LayoutBox : Box
    {
        public List<Box> children = new List<Box>();

        public override void Flush()
        {
            for (int i = children.Count; i-- > 0;)
                children[i].Flush();
            children.Clear();
            Set(0, 0, 0);
        }

        public override void Draw(TexRendererState state)
        {

        #if TEXDRAW_DEBUG
                // Cool debugging feature
                if (TEXConfiguration.main.Document.debug)
                    state.Draw(new TexRendererState.QuadState(TexUtility.frontBlockIndex, new Rect(state.x, state.y - depth, width, depth + height), new Rect(), new Color(1, 1, 0, 0.07f)));
        #endif
        }
    }
}
