using System;
using UnityEngine;

namespace TexDrawLib
{
    // Box representing single character.
    public class CharBox : Box
    {
        // Unicode character (where it didn't inside our library)
        public static CharBox Get(int font, char ch, float scale, float resolution, Color32 color)
        {
            var C = TEXPreference.main.GetChar(font, ch);
            if (C == null)
            {
                var f = TEXPreference.main.fonts[font];

                if (f.type == TexAssetType.Font)
                {
                    // unicode
                    var box = ObjPool<CharBox>.Get();
                    box.type = f.type;
                    box.font = font;
                    box.color = color;

                    var c = box.c = ((TexFont)f).GenerateFont(ch,
                        (int)(resolution * scale) + 1, FontStyle.Normal);
                    float r = scale / c.size;
                    box.Set(-c.minY, c.maxY, -c.minX, c.maxX, c.advance, r);
                    return box;
                }
#if TEXDRAW_TMP
                if (f.type == TexAssetType.FontSigned && ((TexFontSigned)f).asset.atlasPopulationMode == TMPro.AtlasPopulationMode.Dynamic)
                {
                    // unicode
                    var box = ObjPool<CharBox>.Get();
                    box.type = f.type;
                    box.font = font;
                    box.color = color;
                    var asset = ((TexFontSigned)f).asset;
                    var b = ((TexFontSigned)f).GenerateMetric(ch);
                    box.uv = b.uv; var s = b.size;
                    box.Set(s.y, s.w, s.x, s.z, b.advance, scale);
                    if (asset.characterLookupTable.TryGetValue(ch, out TMPro.TMP_Character ccc))
                        box.coeff = ccc.scale; // aliasing coefficient?
                    box.coeff2 = (float)asset.atlasPadding / asset.faceInfo.pointSize * scale;
                    return box;
                }
#endif
                else if (ch == ' ')
                {
                    // the space character is a faux, just guess closest metrics

                    var box = ObjPool<CharBox>.Get();
                    box.type = f.type;
                    var offset = 0f;
#if TEXDRAW_TMP
                    if (f is TexFontSigned ff)
                    {
                        offset = -ff.asset.faceInfo.descentLine / ff.asset.faceInfo.pointSize;
                    }
#endif
                    box.Set(offset, f.LineHeight() - offset, 0, 0, f.SpaceWidth(), scale);
                    return box;
                }
                else
                    // a sprite. simply no way to fix this!
                    throw new InvalidOperationException("Illegal Character! '" + ch + "' doesn't exist in " + TEXPreference.main.fonts[font].name);
            }
            else
                return Get(C, scale, resolution, color);
        }

        public static CharBox Get(TexChar ch, float scale, float resolution, Color32 color)
        {
            var box = ObjPool<CharBox>.Get();
            var font = ch.Font;
            box.ch = ch;
            box.font = font.assetIndex;
            box.color = color;

            switch (box.type = font.type)
            {
                case TexAssetType.Font:
                    {
                        var c = box.c = ((TexFont)font).GenerateFont(ch.characterIndex,
                            (int)(resolution * scale) + 1, FontStyle.Normal);
                        float r = scale / c.size;
                        box.Set(-c.minY, c.maxY, -c.minX, c.maxX, c.advance, r);
                    }

                    return box;
                case TexAssetType.Sprite:
                    {
                        var b = ((TexSprite)font).GenerateMetric(ch.characterIndex);
                        box.uv = b.uv; var s = b.size;
                        box.Set(s.y, s.w, s.x, s.z, s.x + s.z, scale);
                    }
                    return box;
#if TEXDRAW_TMP
                case TexAssetType.FontSigned:
                    {
                        var asset = ((TexFontSigned)font).asset;
                        var b = ((TexFontSigned)font).GenerateMetric(ch.characterIndex);
                        box.font += b.index * 1024;
                        box.uv = b.uv; var s = b.size;
                        box.Set(s.y, s.w, s.x, s.z, b.advance, scale);
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

        private Rect uv;

        private Color32 color;

        public float bearing, italic;

        public TexAssetType type;

#if TEXDRAW_TMP
        private float coeff;
        private float coeff2;
#endif

        private void Set(float depth, float height, float bearing, float italic, float width, float scale)
        {
            this.depth = depth * scale;
            this.height = height * scale;
            this.bearing = bearing * scale;
            this.italic = italic * scale;
            this.width = width * scale;
            this.shift = 0;
            Debug.Assert(scale != float.NaN && !float.IsInfinity(scale));
        }

        public override void Draw(TexRendererState state)
        {
            // Draw character at given position.
            var rect = new Rect((state.x - bearing), (state.y - depth), (bearing + italic), (depth + height));
#if TEXDRAW_DEBUG
            // Cool debugging feature
            if (TEXConfiguration.main.Document.debug)
                state.Draw(new TexRendererState.QuadState(TexUtility.frontBlockIndex, rect, new Rect(), new Color(1f, 0.2f, 0.7f, 0.2f)));
#endif
            switch (type)
            {
                case TexAssetType.Font:
                    state.Draw(new TexRendererState.FlexibleUVQuadState(font, rect, c, color));
                    break;
                case TexAssetType.Sprite:
                    state.Draw(new TexRendererState.QuadState(font, rect, uv, color));
                    break;
#if TEXDRAW_TMP
                case TexAssetType.FontSigned:
                    rect.x -= coeff2;
                    rect.y -= coeff2;
                    rect.width += 2 * coeff2;
                    rect.height += 2 * coeff2;
                    state.signedCoeff = coeff;
                    state.Draw(new TexRendererState.QuadState(font, rect, uv, color));
                    break;
#endif
            }
        }

        public override string ToString()
        {
            return base.ToString() + " " + new string(ch?.characterIndex ?? '#', 1);
        }
        public override void Flush()
        {
            ObjPool<CharBox>.Release(this);
        }
    }
}
