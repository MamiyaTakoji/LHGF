using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace TexDrawLib
{

    public abstract class TexAsset : ScriptableObject
    {
        /// <summary>
        /// type of this TexFont
        /// </summary>
        public abstract TexAssetType type { get; }
        public string id => name;
        public int assetIndex;
        public EditorMetadata editorMetadata = new EditorMetadata();
        public FontMetadata metadata = new FontMetadata();

        [Serializable]
        public class EditorMetadata
        {
            public char[] catalogs = new char[0];
            public string catalogsToken;
            public string assetPath;
            public string category;
            public int order = 100;
            public string[] variantAssetNames = new string[0];
        }

        [Serializable]
        public class FontMetadata
        {
            public float height;
            public float depth;
            public float width; // 0 if not monospaced
            public bool preferUnicode = false;

            public TexAssetStyle style;
            public TexAssetCategory category;
            public TexAsset baseAsset = null;
            public TexAsset[] variantAssets = new TexAsset[0];
        }

        public TexChar[] chars = new TexChar[0];

        public readonly Dictionary<char, TexChar> indexes = new Dictionary<char, TexChar>();

        public readonly Dictionary<string, TexChar> symbols = new Dictionary<string, TexChar>();

        // Also called by TexPreference's OnEnable
        public virtual void ImportDictionary()
        {
            indexes.Clear();
            symbols.Clear();

            if (metadata.baseAsset == null)
            {
                metadata.variantAssets = editorMetadata.variantAssetNames.Select(x => {

                    if (TEXPreference.main.fontnames.TryGetValue(x, out TexAsset y))
                        return y;
                    else return null;
                }).Where(x => x != null).ToArray();
                foreach (var item in metadata.variantAssets)
                {
                    Debug.Assert(item != this);
                    item.metadata.baseAsset = this;
                }
            }

            int i = 0;
            foreach (var item in chars)
            {
                // these three are not serialized
                item.index = i++;
                item.fontIndex = assetIndex;
                item.fontName = name;
                indexes[item.characterIndex] = item;

                if (!string.IsNullOrEmpty(item.symbol))
                    symbols[item.symbol] = item;
            }

            if (metadata.style == TexAssetStyle.Normal)
            {
                foreach (var item in metadata.variantAssets)
                {
                    item?.ImportDictionary();
                }
            }
            else if (metadata.baseAsset)
            {
                foreach (var item in metadata.baseAsset.symbols)
                    if (!symbols.ContainsKey(item.Key))
                        symbols[item.Key] = item.Value;

                //foreach (var item in symbols)
                //    if (!metadata.baseAsset.symbols.ContainsKey(item.Key))
                //        metadata.baseAsset.symbols[item.Key] = item.Value;
            }
        }

        public TexChar this[char c] => indexes.TryGetValue(c, out TexChar ch) ? ch : null;

        public abstract float LineHeight();

        public virtual float SpaceWidth(float atSize) => SpaceWidth();

        public abstract float SpaceWidth();

        public abstract Texture2D Texture();

        public virtual Material Material() => TEXPreference.main.defaultMaterial;


#if UNITY_EDITOR

        [ContextMenu("Export as JSON")]
        public void Export()
        {
            TEXPreference.Initialize();
            var dir = TEXPreference.main.MainFolderPath + "/Core/Editor/Resources/";
            var path = dir + id + ".json";
            var json = JsonUtility.ToJson(this);

            Directory.CreateDirectory(dir);
            File.WriteAllText(path, json);
            Debug.Log("Successfully written to " + path);
        }

        public void ImportCatalog(string raw)
        {
            editorMetadata.catalogsToken = string.IsNullOrEmpty(raw) ? TexCharPresets.legacyChars : raw;
            editorMetadata.catalogs = TexCharPresets.CharsFromString(editorMetadata.catalogsToken);
        }

        public void ImportCharacters(string newcatalog)
        {
            // map from old (existing catalog) to newer one.
            // preserve data from each characterindex

            ImportDictionary();
            editorMetadata.catalogsToken = newcatalog;
            editorMetadata.catalogs = TexCharPresets.CharsFromString(newcatalog);

            var old = chars; chars = new TexChar[editorMetadata.catalogs.Length];

            for (int i = 0; i < chars.Length; i++)
            {
                var cc = editorMetadata.catalogs[i];
                chars[i] = old.FirstOrDefault(x => x.characterIndex == cc) ?? new TexChar();
                chars[i].characterIndex = cc;
            }
            ImportDictionary();
        }

        public abstract void ImportAsset(string path);

#endif
    }
}
