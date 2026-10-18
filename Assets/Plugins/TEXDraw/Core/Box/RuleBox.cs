using System;
using UnityEngine;

namespace TexDrawLib
{
    public class RuleBox : StrutBox
    {

        public static RuleBox Get(Color32 color, float width, float height, float depth)
        {
            var box = ObjPool<RuleBox>.Get();
            box.color = color;
            box.Set(width, height, depth);
            return box;
        }

        public static void PutBorderVertical(HorizontalBox box, Color32 color, float thickness, int times, bool forleft)
        {
            Action<Box> add = forleft ? (Action<Box>)((item) => box.Add(0, item)) : ((item) => box.Add(item));
            for (int i = 0; i < times; i++)
            {
                add(Get(thickness, box.height, box.depth));
                add(Get(color, thickness, box.height, box.depth));
            }
        }

        public static void PutBorderHorizontal(VerticalBox box, Color32 color, float thickness, int times, bool fortop)
        {
            Action<Box> add = fortop ? (Action<Box>)((item) => box.Add(0, item)) : ((item) => box.Add(item));
            for (int i = 0; i < times; i++)
            {
                if (i > 0)
                add(Get(box.width, thickness, 0));
                add(Get(color, box.width, thickness, 0));
            }
        }

        public Color32 color;

        public override void Flush()
        {
            ObjPool<RuleBox>.Release(this);
            base.Flush();
        }

        public override void Draw(TexRendererState state)
        {
#if TEXDRAW_TMP
            state.signedCoeff = 1;
#endif
            state.Draw(new TexRendererState.QuadState(TexUtility.frontBlockIndex, new Rect(state.x, state.y - depth, width, height + depth), new Rect(), color));
        }
    }
}
