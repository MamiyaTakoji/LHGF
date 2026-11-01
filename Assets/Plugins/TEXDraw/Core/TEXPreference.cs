#if UNITY_EDITOR

using UnityEditor;
using System.IO;

#endif

using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace TexDrawLib
{
    public partial class TEXPreference : ScriptableObject
    {
        /// <summary>
        /// Main & Shared access to TEXDraw Preference
        /// </summary>
        static public TEXPreference main;

        static public void Initialize()
        {
#if UNITY_EDITOR
            if (!main)
            {
                //Get the Preference
                string[] targetData = AssetDatabase.FindAssets("t:TEXPreference");
                if (targetData.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(targetData[0]);
                    main = AssetDatabase.LoadAssetAtPath<TEXPreference>(path);
                    main.MainFolderPath = Path.GetDirectoryName(path);
                    // TEXDraw preference now put into resources files after v3.0
                    if (main.MainFolderPath.Contains("Resources"))
                        main.MainFolderPath = Path.GetDirectoryName(main.MainFolderPath);
                    if (targetData.Length > 1)
                        Debug.LogWarning("You have more than one TEXDraw preference file, ensure that only one TexPreference exist in your Project");
                }
                else
                {
                    //Create New One
                    main = CreateInstance<TEXPreference>();
                    if (AssetDatabase.IsValidFolder(DefaultTexFolder))
                    {
                        AssetDatabase.CreateAsset(main, DefaultTexFolder + "/Resources/TEXDrawPreference.asset");
                        main.FirstInitialize(DefaultTexFolder);
                    }
                    else
                    {
                        //Find alternative path to the TEXPreference, that's it: Parent path of TEXPreference script.
                        string AlternativePath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(main));
                        AlternativePath = Directory.GetParent(AlternativePath).Parent.FullName;
                        AssetDatabase.CreateAsset(main, AlternativePath + "/Resources/TEXDrawPreference.asset");
                        main.FirstInitialize(AlternativePath);
                    }
                }
            }
#else
             if (!main)
                    // The only thing that we can found at runtime build is in the Resource folder
                    main = (TEXPreference)Resources.Load("TEXDrawPreference");
#endif
            // also init the neighborhood
            TEXConfiguration.Initialize();
            //TEXDatasets.Initialize();
        }

        // This editor only MainFolderPath is auto reloaded
        public string MainFolderPath = "Assets/Plugins/TEXDraw";

        private const string DefaultTexFolder = "Assets/Plugins/TEXDraw";

        /// Check if we are on importing process.
        /// This solves issue where TEXDraw component
        /// tries to render in the middle of importing process..
        public bool editorReloading = false;

        #region Runtime Utilities

        public Material defaultMaterial;

        public Material spriteMaterial;

        [FormerlySerializedAs("fontData")]
        public TexAsset[] fonts = new TexAsset[0];

        // Dictionaries for faster lookups
        [NonSerialized]
        public Dictionary<string, TexCharRef> symbols = new Dictionary<string, TexCharRef>();

        [NonSerialized]
        public Dictionary<string, TexAsset> fontnames = new Dictionary<string, TexAsset>();

        public TEXConfiguration configuration;

        public TexAsset GetFontByID(string id)
        {
            return fontnames.TryGetValue(id, out TexAsset val) ? val : null;
        }

        public int GetFontIndexByID(string id)
        {
            return fontnames.TryGetValue(id, out TexAsset f) ? f.assetIndex : -1;
        }

        public TexChar GetChar(int font, char ch) => fonts[font][ch];

        public TexChar GetChar(string font, char ch) => string.IsNullOrEmpty(font) ? null : (fontnames[font]?[ch]);

        public TexChar GetChar(TexCharRef hash) => hash.Get;

        public TexChar GetChar(string symbol) => GetChar(symbols.TryGetValue(symbol, out TexCharRef val) ? val : new TexCharRef());

        public TexChar GetChar(string symbol, int font)
        {
            if (font >= 0)
            {
                var f = fonts[font];
                if (f.symbols.TryGetValue(symbol, out TexChar ch))
                    return ch;
            }

            return GetChar(symbol);
        }

        public bool IsCharAvailable(int font, char ch) => fonts[font][ch] != null;

        public int GetGlue(CharType leftType, CharType rightType)
        {
            if (leftType < 0 || rightType < 0)
                return 0;
            return configuration.GlueTable[(int)leftType * 10 + (int)rightType];
        }

        [Obsolete]
        static public TexCharRef CharToHash(TexChar ch)
        {
            return ch;
        }

        static public int CharToHash(int font, int ch)
        {
            return ch | font << 8;
        }

        #endregion
    }
}
