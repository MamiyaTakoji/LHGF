#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace TexDrawLib
{
    public partial class TEXPreference : ScriptableObject
    {
        private void OnEnable()
        {
            try
            {
                if (!main)
                {
                    main = this;
                    Initialize();
                }
                if (symbols.Count == 0)
                    PushToDictionaries();
            }
            catch (System.Exception e)
            {
                // I got infinite loop if any of the import throws error.
                Debug.LogError("TEXPreference import failed! Please try again later");
                Debug.LogException(e);
            }
        }

        public bool PushToDictionaries()
        {
            symbols.Clear();
            fontnames.Clear();

            for (int i = 0; i < fonts.Length; i++)
            {
                var font = fonts[i];
                font.assetIndex = i;
                fontnames.Add(font.name, font);
            }

            foreach (var font in fonts)
            {
                if (font.metadata.style != TexAssetStyle.Normal)
                    continue;

                font.ImportDictionary();
            }
            foreach (var font in fonts)
            {
                if (!configuration.GlobalSymbolFonts.Contains(font.name))
                    continue;

                foreach (var item in font.symbols)
                    if (!symbols.ContainsKey(item.Key))
                        symbols[item.Key] = item.Value;
            }


            return true;
        }


#if UNITY_EDITOR

        [ContextMenu("Rebuild Font Data")]
        public void Reload()
        {
            FirstInitialize(MainFolderPath);
        }

        [ContextMenu("Wipe All Data")]
        public void ResetWholeData()
        {
            var respond = EditorUtility.DisplayDialogComplex("Confirm Reset",
                "Do you really want to reset all symbol setups from beginning?\nThis is different than just reset as this deletes all generated stuff and regain data from beginning\n(WARNING: will erase anything in TexFontMetadata and can't be undone)",
                "YES", "Yes, don't pick up from XML", "Cancel");
            if (respond == 2)
                return;
            foreach (var f in fonts)
            {
                DestroyImmediate(f, true);
            }
            DestroyImmediate(configuration, true);
            FirstInitialize(MainFolderPath);
            configuration = TEXConfiguration.main;
            if (respond == 1)
                return;
            ResetWholeDataConfirmed();
        }

        private void ResetWholeDataConfirmed()
        {
            //var user = AssetDatabase.LoadAssetAtPath<TextAsset>(MainFolderPath + "/XMLs/TexSymbolDefinitions.xml").text;
            //TexImporterUtility.ReadLegacyXMLSymbols(this, false, user);
            //var math = AssetDatabase.LoadAssetAtPath<TextAsset>(MainFolderPath + "/XMLs/TexMathDefinitions.xml").text;
            //TexImporterUtility.ReadLegacyXMLSymbols(this, true, math);
            //var config = AssetDatabase.LoadAssetAtPath<TextAsset>(MainFolderPath + "/XMLs/TEXConfigurations.xml").text;
            //var preset = AssetDatabase.LoadAssetAtPath<TextAsset>(MainFolderPath + "/XMLs/TexFontDefinitions.xml").text;
            //TexImporterUtility.ReadLegacyPreferences(this, preset, config);
        }

        [ContextMenu("Transfer from Legacy XML Data")]
        public void ReloadLegacy()
        {
            var respond = EditorUtility.DisplayDialog("Confirm Transfer",
                "Are you sure you want to transfer symbols from XML data? Use this for maintaining projects which using TEXDraw prior to 3.0 or reset Symbols, Configs and Glue Matrix",
                "Yes", "Cancel");
            if (!respond)
                return;
            ResetWholeDataConfirmed();
        }

        public void FirstInitialize(string mainPath)
        {
            try
            {
                editorReloading = true;
                MainFolderPath = mainPath;

                EditorUtility.DisplayProgressBar("Reloading TEXDraw", "Reading Font Data...", 0f);

                TexImporterUtility.ReadFromResources(this);
#if TEXDRAW_TMP
                EditorUtility.DisplayProgressBar("Reloading", "Writing Font Atlases...", .95f);

                TexImporterSDF.DoBatchRendering(false);
#endif
                EditorUtility.DisplayProgressBar("Reloading", "Reading Configurations...", .95f);

                PaintFontList();
                PushToDictionaries();

                EditorUtility.DisplayProgressBar("Reloading", "Refreshing Instances...", .95f);

                editorReloading = false;
                CallRedraw();

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError("Failed to Rebuilding TEXDraw's Font Data");
            }
            EditorUtility.ClearProgressBar();
        }

        public void CallRedraw()
        {
            TexOrchestrator.InvalidateParser();
            Component[] tex = FindObjectsOfType<Component>();
            for (int i = 0; i < tex.Length; i++)
            {
                if (tex[i] is ITEXDraw)
                {
                    ((ITEXDraw)tex[i]).SetTextDirty();
                    EditorUtility.SetDirty(tex[i]);
                }
            }
            SceneView.RepaintAll();
        }

        public string[] ConfigIDs;
        public string[] FontIDs;
        public GUIContent[] FontIDsGUI;
        public int[] FontIndexs;

        public void PaintFontList()
        {
            List<string> s = new List<string>();
            List<string> t = new List<string>();
            List<int> n = new List<int>();
            n.Add(-1);
            t.Add("-1 (Use Default Typefaces)");
            for (int i = 0; i < fonts.Length; i++)
            {
                t.Add(string.Format("{0} - {1}.ttf", i, fonts[i].id));
                n.Add(i);
                s.Add(fonts[i].id);
            }
            ConfigIDs = s.ToArray();
            FontIDs = t.ToArray();
            FontIndexs = n.ToArray();
            FontIDsGUI = t.ConvertAll(x => new GUIContent(x)).ToArray();
        }

#endif
    }
}
