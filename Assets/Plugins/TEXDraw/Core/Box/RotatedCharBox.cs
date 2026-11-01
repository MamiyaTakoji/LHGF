using UnityEngine;

namespace TexDrawLib
{
    public class RotatedCharBox : Box
    {

        public static RotatedCharBox Get(TexChar ch, float scale, float resolution, Color32 color)
        {
            var box = ObjPool<RotatedCharBox>.Get();
            var font = ch.Font;
            box.ch = ch;
            box.font = font.assetIndex;
            box.color = color;


            switch (box.type = font.type)
            {
                case TexAssetType.Font:
                    var c = box.c = ((TexFont)font).GenerateFont(ch.characterIndex,
                        (int)(resolution * scale) + 1, FontStyle.Normal);
                    float ratio = scale / c.size;
                    box.Set(c.maxX, (-c.minX), 0, (c.maxY - c.minY), ratio);
                    return box;
                case TexAssetType.Sprite:
                    {
                        var b = (box.o = (TexSprite)font).GenerateMetric(ch.characterIndex);
                        box.uv = b.uv; var s = b.size;
                        box.Set(s.z, s.x, s.w, s.y);
                    }
                    return box;
#if TEXDRAW_TMP
                case TexAssetType.FontSigned:
                    {
                        var asset = ((TexFontSigned)font).asset;
                        var b = ((TexFontSigned)font).GenerateMetric(ch.characterIndex);
                        box.font += b.index * 1024;
                        box.uv = b.uv; var s = b.size;
                        box.Set(s.z, s.x, s.w, s.y, scale);
                        box.coeff = asset.characterLookupTable[(uint)ch.characterIndex].scale;
                        box.coeff2 = (float)asset.atlasPadding / asset.faceInfo.pointSize * scale;
                    }
                    return box;
#endif
                default:
                    return null;
            }
        }

        public TexChar ch;

        private int font;

        private CharacterInfo c;

        private TexSprite o;

        private Rect uv;

        private Color32 color;

        public float bearing, italic;

        public TexAssetType type;


#if TEXDRAW_TMP
        private float coeff;
        private float coeff2;
#endif

        private void Set(float depth, float height, float bearing, float italic, float scale)
        {
            this.depth = depth * scale;
            this.height = height * scale;
            this.bearing = bearing * scale;
            this.italic = italic * scale;
            this.width = (italic + bearing) * scale;
            this.shift = 0;
            Debug.Assert(scale != float.NaN && !float.IsInfinity(scale));
        }

        public override void Draw(TexRendererState state)
        {
            // Draw character at given position.
            var rect = new Rect((state.x - bearing), (state.y - depth), (bearing + italic), (depth + height));

            switch (type)
            {
                case TexAssetType.Font:
#pragma warning disable CS0618 // Type or member is obsolete
                    state.Draw(new TexRendererState.FlexibleUVQuadState(font, rect, c.uvBottomRight, c.uvTopRight, c.uvTopLeft, c.uvBottomLeft, color, !c.flipped));
#pragma warning restore CS0618 // Type or member is obsolete

                    break;
                case TexAssetType.Sprite:
                    state.Draw(new TexRendererState.FlexibleUVQuadState(font, rect,
                        new Vector2(uv.xMax, uv.y), uv.max,
                        new Vector2(uv.x, uv.yMax), uv.min,
                        color, true));
                    break;
#if TEXDRAW_TMP
                case TexAssetType.FontSigned:
                    rect.x -= coeff2;
                    rect.y -= coeff2;
                    rect.width += 2 * coeff2;
                    rect.height += 2 * coeff2;
                    state.signedCoeff = coeff;
                    state.Draw(new TexRendererState.FlexibleUVQuadState(font, rect,
                        new Vector2(uv.xMax, uv.y), uv.max,
                        new Vector2(uv.x, uv.yMax), uv.min,
                        color, true));
                    break;
#endif
            }
        }

        public override void Flush()
        {
            ObjPool<RotatedCharBox>.Release(this);
        }
    }
}
