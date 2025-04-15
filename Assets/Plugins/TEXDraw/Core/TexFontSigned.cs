using UnityEngine;
#if TEXDRAW_TMP
using System.Collections.Generic;
using System.IO;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
#endif

namespace TexDrawLib
{
    public class TexFontSigned : TexAsset
    {
        public override TexAssetType type { get { return TexAssetType.FontSigned; } }

        public string rawpath;

#if TEXDRAW_TMP

        public override float LineHeight() { return asset.faceInfo.lineHeight / asset.faceInfo.pointSize; }

        public override float SpaceWidth() { return asset.faceInfo.meanLine / asset.faceInfo.pointSize; }

        public override Texture2D Texture() { return asset.atlasTexture; }
        public Texture2D Texture(int index) { return index < asset.atlasTextures.Length ? asset.atlasTextures[index] : null; }

        public TMP_FontAsset asset;

        protected Dictionary<char, SpriteMetrics> assetmetrices = new Dictionary<char, SpriteMetrics>();

        public SpriteMetrics GenerateMetric(char ch)
        {
            if (asset.atlasPopulationMode == AtlasPopulationMode.Dynamic)
            {
                if (asset.characterLookupTable == null)
                    asset.ReadFontAssetDefinition();

                if (!asset.HasCharacter(ch, false, true))
                {
                    Debug.LogWarningFormat("Character {0} is not found on {1}. Please explicitly tell which font supports this character.\n" +
                        "Have a look at https://fonts.google.com then download the font of your choosing, put it in TEXDraw font user catalog, click reimport font.\n" +
                        "You may also want to set that font to be the default typeface in TEXDraw Configuration.", ch, name);
                    return new SpriteMetrics();
                }

                var c =  asset.glyphLookupTable[asset.characterLookupTable[(uint)ch].glyphIndex];
                var factor = c.scale / asset.faceInfo.pointSize;
                var padding = asset.atlasPadding;
                float invW = 1.0f / asset.atlasWidth, invH = 1.0f / asset.atlasHeight;
                
                return new SpriteMetrics()
                {
                    size = new Vector4()
                    {
                        x = (-c.metrics.horizontalBearingX + 0) * factor,
                        y = (c.metrics.height - c.metrics.horizontalBearingY + 0) * factor,
                        z = (c.metrics.width + c.metrics.horizontalBearingX + 0) * factor,
                        w = (c.metrics.horizontalBearingY + 0) * factor,
                    },
                    advance = c.metrics.horizontalAdvance * factor,
                    uv = new Rect()
                    {
                        x = (c.glyphRect.x - padding) * invW,
                        y = (c.glyphRect.y - padding) * invH,
                        width = (c.glyphRect.width + 2 * padding) * invW,
                        height = (c.glyphRect.height + 2 * padding) * invH
                    },
                    index = (int)c.atlasIndex,
                };
            }


            return assetmetrices[ch];
        }

        public override void ImportDictionary()
        {
            base.ImportDictionary();

            if (!asset) return;

            // sanitize input

            assetmetrices.Clear();

            asset.ReadFontAssetDefinition();

            if (asset.atlasPopulationMode == AtlasPopulationMode.Dynamic) return;

            var info = asset.faceInfo;

            var padding = asset.atlasPadding;

            float invW = 1.0f / asset.atlasWidth, invH = 1.0f / asset.atlasHeight;

            for (int i = 0; i < editorMetadata.catalogs.Length; i++)
            {
                var ch = editorMetadata.catalogs[i];
                if (!asset.characterLookupTable.ContainsKey(ch))
                {
                    assetmetrices[ch] = new SpriteMetrics();
                    continue;
                }

                var c = asset.glyphLookupTable[asset.characterLookupTable[(uint)ch].glyphIndex];

                var factor = c.scale / info.pointSize;

                assetmetrices[ch] = new SpriteMetrics()
                {
                    size = new Vector4()
                    {
                        x = (-c.metrics.horizontalBearingX + 0) * factor,
                        y = (c.metrics.height - c.metrics.horizontalBearingY + 0) * factor,
                        z = (c.metrics.width + c.metrics.horizontalBearingX + 0) * factor,
                        w = (c.metrics.horizontalBearingY + 0) * factor,
                    },
                    advance = c.metrics.horizontalAdvance * factor,
                    uv = new Rect()
                    {
                        x = (c.glyphRect.x - padding) * invW,
                        y = (c.glyphRect.y - padding) * invH,
                        width = (c.glyphRect.width + 2 * padding) * invW,
                        height = (c.glyphRect.height + 2 * padding) * invH
                    }
                };
            }
        }

#else

        public override float LineHeight() { return 0; }

        public override float SpaceWidth() { return 0; }

        public override Texture2D Texture() { return null; }

#endif

#if UNITY_EDITOR

        public override void ImportAsset(string path)
        {
#if TEXDRAW_TMP
            if (path.EndsWith(".asset"))
            {
                asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
#if TEXDRAW_TMP_DYNAMIC
                if (asset && asset.atlasPopulationMode == AtlasPopulationMode.Static)
#else
                if (asset && asset.atlasPopulationMode == AtlasPopulationMode.Dynamic)
#endif
                {
                    AssetDatabase.DeleteAsset(path);
                    DestroyImmediate(asset);
                }
     
                if (!asset)
                {
                    var font = AssetDatabase.LoadAssetAtPath<Font>(rawpath);
#if TEXDRAW_TMP_DYNAMIC
                    asset = TMP_FontAsset.CreateFontAsset(font); // dynamic
#else
                    asset = TMP_FontAsset.CreateFontAsset(font, 16, 2, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Static);
#endif
                    AssetDatabase.CreateAsset(asset, path);

                    AssetDatabase.AddObjectToAsset(asset.material, asset);
                    AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
                }

            }
            else if (path.EndsWith(".ttf") || path.EndsWith(".otf"))
            {
                rawpath = path;
                path = Path.GetDirectoryName(Path.GetDirectoryName(path));
                path += "/TMPro/" + this.name + ".asset";
                ImportAsset(path);
            }
#endif
            }

#endif
    }
}
