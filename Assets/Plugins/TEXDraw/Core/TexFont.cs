using UnityEngine;
using System;
#pragma warning disable CS0618

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace TexDrawLib
{
    public class TexFont : TexAsset
    {
        public override TexAssetType type => TexAssetType.Font;

        public override float LineHeight() => (float)asset.lineHeight / (asset.fontSize == 0 ? 16 :  asset.fontSize);

        [NonSerialized]
        CharacterInfo space;

        public override float SpaceWidth() => SpaceWidth(16);

        public override float SpaceWidth(float atSize)
        {

            space = GenerateFont(' ', (int)atSize, FontStyle.Normal);

            if (space.width > 100)
            {
                // Need correction. Really.
                space.width = (TEXPreference.main.configuration.Typeface.blankSpaceWidth * space.size);
            }

            return space.width / space.size;
        }

        public override Texture2D Texture() { return asset?.material.mainTexture as Texture2D; }

        public Font asset;

        public CharacterInfo GenerateFont(char c, int size, FontStyle style)
        {
            var a = asset;

            a.RequestCharactersInTexture(TexParserUtility.Char2Str(c), size, style);

            a.GetCharacterInfo(c, out CharacterInfo o, size, style);

            return o;
        }

#if UNITY_EDITOR

        public override void ImportAsset(string path)
        {
            asset = AssetDatabase.LoadAssetAtPath<Font>(this.editorMetadata.assetPath = path);
        }

#endif
    }
}
