using UnityEngine;

namespace TexDrawLib
{
    public class StrutBox : Box
    {
        public static Box Empty => Get(0, 0, 0);

        public static StrutBox Get(float width, float height, float depth)
        {
            var box = ObjPool<StrutBox>.Get();
            box.Set(width, height, depth);
            return box;
        }

        public override void Flush()
        {
            ObjPool<StrutBox>.Release(this);
        }

        public override void Draw(TexRendererState state)
        {
#if TEXDRAW_DEBUG
            // Cool debugging feature
            if (TEXConfiguration.main.Document.debug && (width > 0 || TotalHeight > 0))
            {
                var w = Mathf.Max(width, TEXConfiguration.main.Math.lineThickness * state.scale);
                var h = Mathf.Max(height, TEXConfiguration.main.Math.lineThickness * state.scale);
                state.Draw(new TexRendererState.QuadState(TexUtility.frontBlockIndex, new Rect(state.x, state.y, w, h), new Rect(), new Color(1f, 0.7f, 0.2f, 1f)));
                state.Draw(new TexRendererState.QuadState(TexUtility.frontBlockIndex, new Rect(state.x, state.y - depth, w, depth), new Rect(), new Color(1f, 0.7f, 0.2f, 0.2f)));
            }
#endif
        }
    }
}
