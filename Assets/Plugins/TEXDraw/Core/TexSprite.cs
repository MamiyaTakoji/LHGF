using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace TexDrawLib
{
    public class TexSprite : TexAsset
    {
        public override TexAssetType type { get { return TexAssetType.Sprite; } }

        public Sprite[] assets = new Sprite[0];

        [FormerlySerializedAs("sprite_lineOffset")]
        public float lineOffset;

        [FormerlySerializedAs("font_lineHeight")]
        public float lineHeight = 1;

        [FormerlySerializedAs("sprite_alphaOnly")]
        public bool alphaOnly;

        public override float LineHeight() => lineHeight;

        public override float SpaceWidth() => 0;

        public override Texture2D Texture() => assets[0] ? assets[0].texture : null;

        public override Material Material() => alphaOnly ? TEXPreference.main.defaultMaterial : TEXPreference.main.spriteMaterial;


        public Dictionary<char, Sprite> assetindexes = new Dictionary<char, Sprite>();

        public Dictionary<char, SpriteMetrics> assetmetrices = new Dictionary<char, SpriteMetrics>();

        public SpriteMetrics GenerateMetric(char c)
        {
            return assetmetrices.TryGetValue(c, out SpriteMetrics val) ? val : default;
        }

        public override void ImportDictionary()
        {
            // sanitize input
            if (editorMetadata.catalogs.Length < assets.Length)
            {
                Array.Resize(ref editorMetadata.catalogs, assets.Length);
                Array.Resize(ref chars, assets.Length);
            }

            base.ImportDictionary();

            assetindexes.Clear();
            assetmetrices.Clear();

            for (int i = 0; i < assets.Length; i++)
            {
                var a = assets[i]; var sz = new Vector2(1f / a.texture.width, 1f / a.texture.height);
                var aspect = a.rect.width / a.rect.height; var pivot = Vector2.Scale(a.pivot, sz);
                assetindexes[editorMetadata.catalogs[i]] = a;
                assetmetrices[editorMetadata.catalogs[i]] = new SpriteMetrics()
                {
                    size = new Vector4()
                    {
                        x = aspect * lineHeight * pivot.x,
                        y = -lineOffset + lineHeight * pivot.y,
                        z = aspect * lineHeight * (1 - pivot.x),
                        w = lineOffset + lineHeight * (1 - pivot.y),
                    },
                    advance = aspect * lineHeight,
                    uv = new Rect()
                    {
                        min = Vector2.Scale(a.rect.min, sz),
                        max = Vector2.Scale(a.rect.max, sz),
                    }
                };
            }
        }

#if UNITY_EDITOR

        public override void ImportAsset(string path)
        {
            // we're skeptics with order
            assets = AssetDatabase.LoadAllAssetsAtPath(this.editorMetadata.assetPath = path)
                .OfType<Sprite>().ToArray();
        }

#endif
    }

    [Serializable]
    public struct SpriteMetrics
    {
        /// <summary>
        /// left, bottom, right, up. or bearing, depth, italic, height.
        /// </summary>
        public Vector4 size;

        /// <summary>
        /// width span
        /// </summary>
        public float advance;

        /// <summary>
        /// UV
        /// </summary>
        public Rect uv;

#if TEXDRAW_TMP
        /// <summary>
        /// Font Index 
        /// </summary>
        public int index;
#endif
    }
}
