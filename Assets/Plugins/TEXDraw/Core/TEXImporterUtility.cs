#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System.Xml.Linq;
using System;

namespace TexDrawLib
{
    /// Contains some utility codes to manage the automatic importing processes.
    public class TexImporterUtility
    {
        public static void ReadFromResources(TEXPreference pref)
        {
            var fontList = new List<TexAsset>();

            EnsureFolderExist(pref.MainFolderPath + "/Resources", "TexFontMetaData");
            EnsureFolderExist(pref.MainFolderPath, "Fonts");
#if TEXDRAW_TMP
            EnsureFolderExist(pref.MainFolderPath + "/Fonts", "TMPro");
            LoadPrimaryDefinitionSubset<TexFontSigned>(pref, fontList, pref.MainFolderPath + "/Fonts/Math", "t:Font", 0);
            LoadPrimaryDefinitionSubset<TexFontSigned>(pref, fontList, pref.MainFolderPath + "/Fonts/Extras", "t:Font", 1);
            LoadPrimaryDefinitionSubset<TexFontSigned>(pref, fontList, pref.MainFolderPath + "/Fonts/User", "t:Font", 2);
            LoadPrimaryDefinitionSubset<TexSprite>(pref, fontList, pref.MainFolderPath + "/Fonts/Sprites", "t:Sprite", 3);
#else
            LoadPrimaryDefinitionSubset<TexFont>(pref, fontList, pref.MainFolderPath + "/Fonts/Math", "t:Font", 0);
            LoadPrimaryDefinitionSubset<TexFont>(pref, fontList, pref.MainFolderPath + "/Fonts/Extras", "t:Font", 1);
            LoadPrimaryDefinitionSubset<TexFont>(pref, fontList, pref.MainFolderPath + "/Fonts/User", "t:Font", 2);
            LoadPrimaryDefinitionSubset<TexSprite>(pref, fontList, pref.MainFolderPath + "/Fonts/Sprites", "t:Sprite", 3);
#endif
            EditorUtility.DisplayProgressBar("Reloading", "Preparing Stuff...", .93f);

            pref.fonts = fontList.OrderBy(x => x.editorMetadata.order).ToArray();
        }

        private static void EnsureFolderExist(string path, string name)
        {
            if (!AssetDatabase.IsValidFolder(path + "/" + name))
                AssetDatabase.CreateFolder(path, name);
        }

        private const string resourceFontMetaPath = "/Resources/TexFontMetaData/";

        private static void LoadPrimaryDefinitionSubset<T>(TEXPreference pref, List<TexAsset> fontList, string folderPath, string typeStr, int mode) where T : TexAsset
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] assetGUIDs = AssetDatabase.FindAssets(typeStr, new string[] { folderPath });

            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                string assetName = Path.GetFileNameWithoutExtension(assetPath).ToLower();

                if (fontList.Any((x) => x.id == assetName)) continue;

                if (!isNameValid(assetName))
                {
                    // We show this for information purpose, since *-Regular or *_ is very common mistake
                    Debug.LogWarningFormat("Skipping font {0} because of invalid file name. Please only use letters.", assetName);
                    continue;
                }


                UpdateProgress(mode, assetName, i, assetGUIDs.Length);

                var metaPath = pref.MainFolderPath + resourceFontMetaPath + assetName + ".asset";
                TexAsset metadata = AssetDatabase.LoadAssetAtPath<T>(metaPath);

                if (!metadata)
                {
                    metadata = CreateAndRecover<T>(metaPath);
                    AssetDatabase.CreateAsset(metadata, metaPath);
                    if (mode >= 2)
                        metadata.metadata.preferUnicode = true;
                }


                metadata.name = assetName;
                metadata.assetIndex = fontList.Count;
                metadata.ImportAsset(assetPath);
                metadata.ImportDictionary();

                var mod = Path.GetDirectoryName(assetPath) + "/.ttx/" + Path.ChangeExtension(assetName, "ttx");
                if (File.Exists(mod))
                {
                    using (var reader = XmlReader.Create(new FileStream(mod, FileMode.Open)))
                    {
                        var doc = XDocument.Load(reader);
                        var root = doc.Element("ttFont");
                        foreach (var item in root.Element("cmap").Element("cmap_format_4").Elements("map"))
                        {
                            var chr = (char)Convert.ToInt32((string)item.Attribute("code"), 16);
                            var ch = metadata.indexes.TryGetValue(chr, out TexChar val) ? val : null;
                            if (ch != null)
                                ch.symbol = (string)item.Attribute("name");
                        }
                    }
                }

                EditorUtility.SetDirty(metadata);

                fontList.Add(metadata);
            }
        }

        private static TexAsset CreateAndRecover<T>(string metaPath) where T : TexAsset
        {
            TexAsset metadata = ScriptableObject.CreateInstance<T>();
            {
                // it may saved in another format. try to recover.
                var mt2 = AssetDatabase.LoadAssetAtPath<TexAsset>(metaPath);
                if (mt2)
                {
                    EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(mt2), metadata);
                    AssetDatabase.DeleteAsset(metaPath);
                }
                else
                    metadata.ImportCharacters(TexCharPresets.legacyChars);
            }
            return metadata;
        }

        private static void UpdateProgress(int phase, string name, int idx, int total)
        {
            float prog = (phase / 3f) + idx / (total * 3f);
            EditorUtility.DisplayProgressBar("Reloading", "Reading " + name + "...", prog);
        }

        private static bool isNameValid(string name)
        {
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsLetter(name[i]))
                    continue;
                else
                    return false;
            }
            return true;
        }

    }
}

#endif
