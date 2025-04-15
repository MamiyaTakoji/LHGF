using UnityEngine;

namespace TexDrawLib
{
    public class StrikeBox : Box
    {
        private const string NegateSkin = "negateskin";

        public enum StrikeMode
        {
            diagonal = 0,
            diagonalInverse = 1,
            horizontal = 2,
            doubleHorizontal = 3,
            underline = 4,
            overline = 5,
            vertical = 6,
            verticalInverse = 7
        }

        public float margin;
        public float thickness;
        public float offsetPlus;
        public float offsetMinus;
        public float underlineLevel;
        public float overlineLevel;
        public float midlineLevel;
        public Color32 color;
        public StrikeMode mode;

        public static StrikeBox Get(Color32 color, float Height, float Width, float Depth,
            float Margin, float Thickness, StrikeMode Mode,
            float OffsetM, float OffsetP,
            float underline, float overline, float midline)
        {
            var box = Get(Height, Width, Depth);
            box.color = color;
            box.margin = Margin;
            box.thickness = Thickness;
            box.mode = Mode;
            box.offsetPlus = OffsetP;
            box.offsetMinus = OffsetM;
            box.underlineLevel = underline;
            box.overlineLevel = overline;
            box.midlineLevel = midline;
            return box;
        }

        public static StrikeBox Get(float Height, float Width, float Depth)
        {
            var box = ObjPool<StrikeBox>.Get();
            box.Set(Width, Height, Depth, 0);
            return box;
        }

        public override void Draw(TexRendererState state)
        {

            //This is for people who are nerdy about math
            float op = Mathf.Max(0, offsetPlus), om = Mathf.Min(0, offsetMinus);
            float opm = op - om;
            float angle;
            float x = state.x;
            float y = state.y;
            switch (mode)
            {
                case StrikeMode.diagonal: angle = Mathf.Atan2(TotalHeight + margin * 2 - opm, width); break;
                case StrikeMode.diagonalInverse: angle = Mathf.Atan2(TotalHeight + margin * 2 - opm, width); break;
                case StrikeMode.vertical: angle = Mathf.Atan2(TotalHeight + margin * 2, width - opm); break;
                case StrikeMode.verticalInverse: angle = Mathf.Atan2(TotalHeight + margin * 2, width - opm); break;
                default: angle = 0; break;
            }
            float s = Mathf.Sin(angle) * thickness, c = Mathf.Cos(angle) * thickness;
            float w = width, wdhpmr = width + margin, h = TotalHeight / 2f + margin;
            TexRendererState.RawVertexQuadState v;
            y += TotalHeight / 2f - depth;
            if ((int)mode > 1 && (int)mode < 6)
                y += op + om;
            switch (mode)
            {
                case StrikeMode.diagonal:
                    v = new TexRendererState.RawVertexQuadState(
                        TexUtility.frontBlockIndex, color,
                        new Vector3((x + w + s), (y + h - c + om)),
                        new Vector2((x + w - s), (y + h + c + om)),
                        new Vector2((x - s), (y - h + c + op)),
                        new Vector2((x + s), (y - h - c + op))
                    );
                    break;
                case StrikeMode.diagonalInverse:
                    v = new TexRendererState.RawVertexQuadState(
                        TexUtility.frontBlockIndex, color,
                        new Vector2((x + s), (y + h + c + om)),
                        new Vector2((x - s), (y + h - c + om)),
                        new Vector2((x + w - s), (y - h - c + op)),
                        new Vector2((x + w + s), (y - h + c + op))
                    );
                    break;
                case StrikeMode.horizontal:
                    y += midlineLevel;
                    v = new TexRendererState.RawVertexQuadState(
                        TexUtility.frontBlockIndex, color,
                        new Vector2((x - margin), (y + thickness)),
                        new Vector2((x + wdhpmr), (y + thickness)),
                        new Vector2((x + wdhpmr), (y - thickness)),
                        new Vector2((x - margin), (y - thickness))
                    );
                    break;
                case StrikeMode.doubleHorizontal:
                    float doubleOffset = thickness;
                    y += midlineLevel;
                    v = new TexRendererState.RawVertexQuadState(
                        TexUtility.frontBlockIndex, color,
                        new Vector2((x - margin), (y + thickness + doubleOffset)),
                        new Vector2((x + wdhpmr), (y + thickness + doubleOffset)),
                        new Vector2((x + wdhpmr), (y - thickness + doubleOffset)),
                        new Vector2((x - margin), (y - thickness + doubleOffset))
                    );
                    state.Draw(v);

                    v = new TexRendererState.RawVertexQuadState(
                        TexUtility.frontBlockIndex, color,
                        new Vector2((x - margin), (y + thickness - doubleOffset)),
                        new Vector2((x + wdhpmr), (y + thickness - doubleOffset)),
                        new Vector2((x + wdhpmr), (y - thickness - doubleOffset)),
                        new Vector2((x - margin), (y - thickness - doubleOffset))
                    );
                    break;
                case StrikeMode.underline:
                    y -= TotalHeight / 2f - depth + thickness * 2 - underlineLevel;
                    v = new TexRendererState.RawVertexQuadState(
                        TexUtility.frontBlockIndex, color,
                        new Vector2((x - margin), (y + thickness)),
                        new Vector2((x + wdhpmr), (y + thickness)),
                        new Vector2((x + wdhpmr), (y - thickness)),
                        new Vector2((x - margin), (y - thickness))
                    );
                    break;
                case StrikeMode.overline:
                    y += TotalHeight / 2 + thickness * 2 + overlineLevel;
                    v = new TexRendererState.RawVertexQuadState(
                        TexUtility.frontBlockIndex, color,
                        new Vector2((x - margin), (y + thickness)),
                        new Vector2((x + wdhpmr), (y + thickness)),
                        new Vector2((x + wdhpmr), (y - thickness)),
                        new Vector2((x - margin), (y - thickness))
                    );
                    break;
                case StrikeMode.vertical:
                    v = new TexRendererState.RawVertexQuadState(
                        TexUtility.frontBlockIndex, color,
                        new Vector2((x + w + s + om), (y + h - c)),
                        new Vector2((x + w - s + om), (y + h + c)),
                        new Vector2((x - s + op), (y - h + c)),
                        new Vector2((x + s + op), (y - h - c))
                    );
                    break;
                case StrikeMode.verticalInverse:
                    v = new TexRendererState.RawVertexQuadState(
                        TexUtility.frontBlockIndex, color,
                        new Vector2((x + s + op), (y + h + c)),
                        new Vector2((x - s + op), (y + h - c)),
                        new Vector2((x + w - s + om), (y - h - c)),
                        new Vector2((x + w + s + om), (y - h + c))
                    );
                    break;
                default:
                    return;
            }
            state.Draw(v);
        }

        private Vector2[] uv;
        private int fontIdx;

        public override void Flush()
        {
            ObjPool<StrikeBox>.Release(this);
        }
    }
}
