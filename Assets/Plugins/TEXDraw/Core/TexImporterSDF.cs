#if TEXDRAW_TMP
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore;
using UnityEngine;
using TMPro;
using Object = UnityEngine.Object;
using static ReflectionUtility;
#if UNITY_EDITOR
using UnityEditor;
using TMPro.EditorUtilities;
#endif
#endif

namespace TexDrawLib
{
    public static class TexImporterSDF
    {
#if TEXDRAW_TMP
        private static readonly string[] FontResolutionLabels = { "16", "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };
        private static readonly int[] FontAtlasResolutions = { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        // Status Progresses

        internal static float m_AtlasGenerationProgress;
        internal static string m_AtlasGenerationProgressLabel = string.Empty;
        internal static bool m_IsRenderingDone;
        internal static bool m_IsProcessing;

        internal static bool m_IsGenerationCancelled;

        private enum GlyphRasterModes
        {
            RASTER_MODE_8BIT = 1,
            RASTER_MODE_MONO = 2,
            RASTER_MODE_NO_HINTING = 4,
            RASTER_MODE_HINTED = 8,
            RASTER_MODE_BITMAP = 0x10,
            RASTER_MODE_SDF = 0x20,
            RASTER_MODE_SDFAA = 0x40,
            RASTER_MODE_MSDF = 0x100,
            RASTER_MODE_MSDFA = 0x200,
            RASTER_MODE_1X = 0x1000,
            RASTER_MODE_8X = 0x2000,
            RASTER_MODE_16X = 0x4000,
            RASTER_MODE_32X = 0x8000
        }
#if !TEXDRAW_TMP_DYNAMIC
        private static int currentBatchState = -1;
#endif

        public static void DoBatchRendering(bool forceRerenderAll = false)
        {
#if !TEXDRAW_TMP_DYNAMIC
            var cap = TEXPreference.main.fonts.Length;
            for (currentBatchState++; currentBatchState < cap; currentBatchState++)
            {
                var font = TEXPreference.main.fonts[currentBatchState];
                if (font.type == TexAssetType.FontSigned)
                {
                    if (forceRerenderAll || ((TexFontSigned)font).asset.atlasTexture.width == 0)
                    {
                        break;
                    }
                }
            }
            if (currentBatchState == cap)
            {
                currentBatchState = -1;
            }
            else
            {
                var font = TEXPreference.main.fonts[currentBatchState] as TexFontSigned;
                DoDirectRendering(font.asset, font.rawpath, font.editorMetadata.catalogs.Select(x => (uint)x).ToArray(), x => DoBatchRendering());
            }
#endif
        }

        public static void DoDirectRendering(TMP_FontAsset asset, string fontPath, uint[] characterSet, Action<TMP_FontAsset> finished)
        {
#if UNITY_EDITOR
            int m_PointSize = 16;
            int m_Padding = 5;

            System.Diagnostics.Stopwatch m_StopWatch;
            double m_GlyphPackingGenerationTime;
            double m_GlyphRenderingGenerationTime;

            GlyphPackingMode m_PackingMode = GlyphPackingMode.BestShortSideFit;
            int m_PointSizeSamplingMode = 0; // { "Auto Sizing", "Custom Size" }

            GlyphRenderMode m_GlyphRenderMode = GlyphRenderMode.SDFAA;
            int m_AtlasWidth = 512;
            int m_AtlasHeight = 512;
            byte[] m_AtlasTextureBuffer;
            var m_SourceFontFile = fontPath;
            var tex_FileName = asset.name;

            List<Glyph> m_GlyphsToPack = new List<Glyph>();
            List<Glyph> m_GlyphsPacked = new List<Glyph>();
            List<GlyphRect> m_FreeGlyphRects = new List<GlyphRect>();
            List<GlyphRect> m_UsedGlyphRects = new List<GlyphRect>();
            List<Glyph> m_GlyphsToRender = new List<Glyph>();
            List<uint> m_AvailableGlyphsToAdd = new List<uint>();
            List<uint> m_MissingCharacters = new List<uint>();
            List<uint> m_ExcludedCharacters = new List<uint>();
            List<Glyph> m_FontGlyphTable = new List<Glyph>();
            List<TMP_Character> m_FontCharacterTable = new List<TMP_Character>();
            Dictionary<uint, uint> m_CharacterLookupMap = new Dictionary<uint, uint>();
            Dictionary<uint, List<uint>> m_GlyphLookupMap = new Dictionary<uint, List<uint>>();

            Texture2D m_FontAtlasTexture = null;
            FaceInfo m_FaceInfo = default;

            var fontAsset = asset;

            {
                // Initialize font engine
                FontEngineError errorCode = FontEngine.InitializeFontEngine();
                if (errorCode != FontEngineError.Success)
                {
                    Debug.Log("Font Asset Creator - Error [" + errorCode + "] has occurred while Initializing the FreeType Library.");
                }

                if (errorCode == FontEngineError.Success)
                {
                    errorCode = FontEngine.LoadFontFace(fontPath);

                    if (errorCode != FontEngineError.Success)
                    {
                        Debug.Log("Font Asset Creator - Error Code [" + errorCode + "] has occurred trying to load the [" + fontPath + "] font file. This typically results from the use of an incompatible or corrupted font file.");
                    }
                }


                // Define an array containing the characters we will render.
                if (errorCode == FontEngineError.Success)
                {

                    m_AtlasGenerationProgress = 0;
                    m_IsProcessing = true;
                    m_IsGenerationCancelled = false;

                    GlyphLoadFlags glyphLoadFlags = ((GlyphRasterModes)m_GlyphRenderMode & GlyphRasterModes.RASTER_MODE_HINTED) == GlyphRasterModes.RASTER_MODE_HINTED ? GlyphLoadFlags.LOAD_RENDER : GlyphLoadFlags.LOAD_RENDER | GlyphLoadFlags.LOAD_NO_HINTING;

                    //
                    AutoResetEvent autoEvent = new AutoResetEvent(false);

                    // Worker thread to pack glyphs in the given texture space.
                    ThreadPool.QueueUserWorkItem(PackGlyphs =>
                    {
                        // Start Stop Watch
                        m_StopWatch = System.Diagnostics.Stopwatch.StartNew();

                        // Clear the various lists used in the generation process.
                        m_AvailableGlyphsToAdd.Clear();
                        m_MissingCharacters.Clear();
                        m_ExcludedCharacters.Clear();
                        m_CharacterLookupMap.Clear();
                        m_GlyphLookupMap.Clear();
                        m_GlyphsToPack.Clear();
                        m_GlyphsPacked.Clear();

                        // Check if requested characters are available in the source font file.
                        for (int i = 0; i < characterSet.Length; i++)
                        {
                            uint unicode = characterSet[i];

                            if (FontEngine.TryGetGlyphIndex(unicode, out uint glyphIndex))
                            {
                                // Skip over potential duplicate characters.
                                if (m_CharacterLookupMap.ContainsKey(unicode))
                                    continue;

                                // Add character to character lookup map.
                                m_CharacterLookupMap.Add(unicode, glyphIndex);

                                // Skip over potential duplicate glyph references.
                                if (m_GlyphLookupMap.ContainsKey(glyphIndex))
                                {
                                    // Add additional glyph reference for this character.
                                    m_GlyphLookupMap[glyphIndex].Add(unicode);
                                    continue;
                                }

                                // Add glyph reference to glyph lookup map.
                                m_GlyphLookupMap.Add(glyphIndex, new List<uint>() { unicode });

                                // Add glyph index to list of glyphs to add to texture.
                                m_AvailableGlyphsToAdd.Add(glyphIndex);
                            }
                            else
                            {
                                // Add Unicode to list of missing characters.
                                m_MissingCharacters.Add(unicode);
                            }
                        }

                        // Pack available glyphs in the provided texture space.
                        if (m_AvailableGlyphsToAdd.Count > 0)
                        {
                            int packingModifier = ((GlyphRasterModes)m_GlyphRenderMode & GlyphRasterModes.RASTER_MODE_BITMAP) == GlyphRasterModes.RASTER_MODE_BITMAP ? 0 : 1;

                            if (m_PointSizeSamplingMode == 0) // Auto-Sizing Point Size Mode
                            {
                                // Estimate min / max range for auto sizing of point size.
                                int minPointSize = 0;
                                int maxPointSize = (int)Mathf.Sqrt((m_AtlasWidth * m_AtlasHeight) / m_AvailableGlyphsToAdd.Count) * 3;

                                m_PointSize = (maxPointSize + minPointSize) / 2;

                                bool optimumPointSizeFound = false;
                                for (int iteration = 0; iteration < 15 && optimumPointSizeFound == false; iteration++)
                                {
                                    m_AtlasGenerationProgressLabel = "Packing glyphs - Pass (" + iteration + ")";

                                    FontEngine.SetFaceSize(m_PointSize);

                                    m_GlyphsToPack.Clear();
                                    m_GlyphsPacked.Clear();

                                    m_FreeGlyphRects.Clear();
                                    m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - packingModifier, m_AtlasHeight - packingModifier));
                                    m_UsedGlyphRects.Clear();

                                    for (int i = 0; i < m_AvailableGlyphsToAdd.Count; i++)
                                    {
                                        uint glyphIndex = m_AvailableGlyphsToAdd[i];

                                        if (FontEngine.TryGetGlyphWithIndexValue(glyphIndex, glyphLoadFlags, out Glyph glyph))
                                        {
                                            if (glyph.glyphRect.width > 0 && glyph.glyphRect.height > 0)
                                            {
                                                m_GlyphsToPack.Add(glyph);
                                            }
                                            else
                                            {
                                                m_GlyphsPacked.Add(glyph);
                                            }
                                        }
                                    }

                                    IStaticCall(typeof(FontEngine), "TryPackGlyphsInAtlas", m_GlyphsToPack, m_GlyphsPacked, m_Padding, m_PackingMode, m_GlyphRenderMode, m_AtlasWidth, m_AtlasHeight, m_FreeGlyphRects, m_UsedGlyphRects
                                        );


                                    if (m_IsGenerationCancelled)
                                    {
                                        Object.DestroyImmediate(m_FontAtlasTexture);
                                        m_FontAtlasTexture = null;
                                        return;
                                    }

                                    //Debug.Log("Glyphs remaining to add [" + m_GlyphsToAdd.Count + "]. Glyphs added [" + m_GlyphsAdded.Count + "].");

                                    if (m_GlyphsToPack.Count > 0)
                                    {
                                        if (m_PointSize > minPointSize)
                                        {
                                            maxPointSize = m_PointSize;
                                            m_PointSize = (m_PointSize + minPointSize) / 2;

                                            //Debug.Log("Decreasing point size from [" + maxPointSize + "] to [" + m_PointSize + "].");
                                        }
                                    }
                                    else
                                    {
                                        if (maxPointSize - minPointSize > 1 && m_PointSize < maxPointSize)
                                        {
                                            minPointSize = m_PointSize;
                                            m_PointSize = (m_PointSize + maxPointSize) / 2;
                                        }
                                        else
                                        {
                                            optimumPointSizeFound = true;
                                        }
                                    }
                                }
                            }
                            else // Custom Point Size Mode
                            {
                                m_AtlasGenerationProgressLabel = "Packing glyphs...";

                                // Set point size
                                FontEngine.SetFaceSize(m_PointSize);

                                m_GlyphsToPack.Clear();
                                m_GlyphsPacked.Clear();

                                m_FreeGlyphRects.Clear();
                                m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - packingModifier, m_AtlasHeight - packingModifier));
                                m_UsedGlyphRects.Clear();

                                for (int i = 0; i < m_AvailableGlyphsToAdd.Count; i++)
                                {
                                    uint glyphIndex = m_AvailableGlyphsToAdd[i];

                                    if (FontEngine.TryGetGlyphWithIndexValue(glyphIndex, glyphLoadFlags, out Glyph glyph))
                                    {
                                        if (glyph.glyphRect.width > 0 && glyph.glyphRect.height > 0)
                                        {
                                            m_GlyphsToPack.Add(glyph);
                                        }
                                        else
                                        {
                                            m_GlyphsPacked.Add(glyph);
                                        }
                                    }
                                }

                                typeof(FontEngine).InvokeMember("TryPackGlyphsInAtlas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, null, new object[] {
                                        m_GlyphsToPack, m_GlyphsPacked, m_Padding, (GlyphPackingMode)m_PackingMode,     m_GlyphRenderMode, m_AtlasWidth, m_AtlasHeight, m_FreeGlyphRects, m_UsedGlyphRects
                                    });


                                if (m_IsGenerationCancelled)
                                {
                                    Object.DestroyImmediate(m_FontAtlasTexture);
                                    m_FontAtlasTexture = null;
                                    return;
                                }
                                //Debug.Log("Glyphs remaining to add [" + m_GlyphsToAdd.Count + "]. Glyphs added [" + m_GlyphsAdded.Count + "].");
                            }

                        }
                        else
                        {
                            int packingModifier = ((GlyphRasterModes)m_GlyphRenderMode & GlyphRasterModes.RASTER_MODE_BITMAP) == GlyphRasterModes.RASTER_MODE_BITMAP ? 0 : 1;

                            FontEngine.SetFaceSize(m_PointSize);

                            m_GlyphsToPack.Clear();
                            m_GlyphsPacked.Clear();

                            m_FreeGlyphRects.Clear();
                            m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - packingModifier, m_AtlasHeight - packingModifier));
                            m_UsedGlyphRects.Clear();
                        }

                        //Stop StopWatch
                        m_StopWatch.Stop();
                        m_GlyphPackingGenerationTime = m_StopWatch.Elapsed.TotalMilliseconds;
                        Debug.Log("Glyph packing completed in: " + m_GlyphPackingGenerationTime.ToString("0.000 ms."));
                        m_StopWatch.Reset();

                        m_FontCharacterTable.Clear();
                        m_FontGlyphTable.Clear();
                        m_GlyphsToRender.Clear();

                        // Add glyphs and characters successfully added to texture to their respective font tables.
                        foreach (Glyph glyph in m_GlyphsPacked)
                        {
                            uint glyphIndex = glyph.index;

                            m_FontGlyphTable.Add(glyph);

                            // Add glyphs to list of glyphs that need to be rendered.
                            if (glyph.glyphRect.width > 0 && glyph.glyphRect.height > 0)
                                m_GlyphsToRender.Add(glyph);

                            foreach (uint unicode in m_GlyphLookupMap[glyphIndex])
                            {
                                // Create new Character
                                m_FontCharacterTable.Add(new TMP_Character(unicode, glyph));
                            }
                        }

                        //
                        foreach (Glyph glyph in m_GlyphsToPack)
                        {
                            foreach (uint unicode in m_GlyphLookupMap[glyph.index])
                            {
                                m_ExcludedCharacters.Add(unicode);
                            }
                        }

                        // Get the face info for the current sampling point size.
                        m_FaceInfo = FontEngine.GetFaceInfo();

                        autoEvent.Set();
                    });

                    // Worker thread to render glyphs in texture buffer.
                    ThreadPool.QueueUserWorkItem(RenderGlyphs =>
                    {
                        autoEvent.WaitOne();

                        // Start Stop Watch
                        m_StopWatch = System.Diagnostics.Stopwatch.StartNew();

                        m_IsRenderingDone = false;

                        // Allocate texture data
                        m_AtlasTextureBuffer = new byte[m_AtlasWidth * m_AtlasHeight];

                        m_AtlasGenerationProgressLabel = "Rendering glyphs...";

                        // Render and add glyphs to the given atlas texture.
                        if (m_GlyphsToRender.Count > 0)
                        {
                            IStaticCall(typeof(FontEngine), "RenderGlyphsToTexture", m_GlyphsToRender, m_Padding, m_GlyphRenderMode, m_AtlasTextureBuffer, m_AtlasWidth, m_AtlasHeight);
                        }

                        m_IsRenderingDone = true;

                        // Stop StopWatch
                        m_StopWatch.Stop();
                        m_GlyphRenderingGenerationTime = m_StopWatch.Elapsed.TotalMilliseconds;
                        Debug.Log("Font Atlas generation completed in: " + m_GlyphRenderingGenerationTime.ToString("0.000 ms."));
                        m_StopWatch.Reset();

                        EditorApplication.delayCall += () =>
                        {
                            {
                                if (m_FontAtlasTexture != null)
                                    Object.DestroyImmediate(m_FontAtlasTexture);

                                m_FontAtlasTexture = new Texture2D(m_AtlasWidth, m_AtlasHeight, TextureFormat.Alpha8, false, true);

                                Color32[] colors = new Color32[m_AtlasWidth * m_AtlasHeight];

                                for (int i = 0; i < colors.Length; i++)
                                {
                                    byte c = m_AtlasTextureBuffer[i];
                                    colors[i] = new Color32(c, c, c, c);
                                }

                                // Clear allocation of
                                m_AtlasTextureBuffer = null;

                                if ((m_GlyphRenderMode & GlyphRenderMode.RASTER) == GlyphRenderMode.RASTER || (m_GlyphRenderMode & GlyphRenderMode.RASTER_HINTED) == GlyphRenderMode.RASTER_HINTED)
                                    m_FontAtlasTexture.filterMode = FilterMode.Point;

                                m_FontAtlasTexture.SetPixels32(colors, 0);
                                m_FontAtlasTexture.Apply(false, false);
                            }

                            if (fontAsset == null)
                            {
                                //Debug.Log("Creating TextMeshPro font asset!");
                                fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>(); // Create new TextMeshPro Font Asset.
                                                                                              //AssetDatabase.CreateAsset(fontAsset, tex_Path_NoExt + ".asset");

                                // Set version number of font asset
                                ISetProp(fontAsset, "version", "1.1.0");

                                // Reference to source font file GUID.
                                ISetProp(fontAsset, "m_SourceFontFileGUID", AssetDatabase.AssetPathToGUID(fontPath));

                                //Set Font Asset Type
                                ISetProp(fontAsset, "atlasRenderMode", m_GlyphRenderMode);

                                // Add FaceInfo to Font Asset
                                ISetProp(fontAsset, "faceInfo", m_FaceInfo);

                                // Add GlyphInfo[] to Font Asset
                                ISetProp(fontAsset, "glyphTable", m_FontGlyphTable);

                                // Add CharacterTable[] to font asset.
                                ISetProp(fontAsset, "characterTable", m_FontCharacterTable);

                                // Sort glyph and character tables.
                                ICall(fontAsset, "SortAllTables");

                                // Get and Add Kerning Pairs to Font Asset
                                //if (m_IncludeFontFeatures)
                                //    ISetProp(fontAsset, "fontFeatureTable", GetKerningTable());

                                // Add Font Atlas as Sub-Asset
                                fontAsset.atlasTextures = new Texture2D[] { m_FontAtlasTexture };
                                m_FontAtlasTexture.name = tex_FileName + " Atlas";
                                ISetProp(fontAsset, "atlasWidth", m_AtlasWidth);
                                ISetProp(fontAsset, "atlasHeight", m_AtlasHeight);
                                ISetProp(fontAsset, "atlasPadding", m_Padding);

                                AssetDatabase.AddObjectToAsset(m_FontAtlasTexture, fontAsset);

                                // Create new Material and Add it as Sub-Asset
                                Shader default_Shader = Shader.Find("TextMeshPro/Distance Field");
                                Material tmp_material = new Material(default_Shader);

                                tmp_material.name = tex_FileName + " Material";
                                tmp_material.SetTexture(ShaderUtilities.ID_MainTex, m_FontAtlasTexture);
                                tmp_material.SetFloat(ShaderUtilities.ID_TextureWidth, m_FontAtlasTexture.width);
                                tmp_material.SetFloat(ShaderUtilities.ID_TextureHeight, m_FontAtlasTexture.height);

                                int spread = m_Padding + 1;
                                tmp_material.SetFloat(ShaderUtilities.ID_GradientScale, spread); // Spread = Padding for Brute Force SDF.

                                tmp_material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
                                tmp_material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);

                                fontAsset.material = tmp_material;

                                AssetDatabase.AddObjectToAsset(tmp_material, fontAsset);

                            }
                            else
                            {

                                // Destroy Assets that will be replaced.
                                if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0)
                                    Object.DestroyImmediate(fontAsset.atlasTextures[0], true);

                                // Set version number of font asset
                                ISetProp(fontAsset, "version", "1.1.0");


                                //Set Font Asset Type
                                ISetProp(fontAsset, "atlasRenderMode", m_GlyphRenderMode);

                                // Add FaceInfo to Font Asset
                                ISetProp(fontAsset, "faceInfo", m_FaceInfo);

                                // Add GlyphInfo[] to Font Asset
                                ISetProp(fontAsset, "glyphTable", m_FontGlyphTable);

                                // Add CharacterTable[] to font asset.
                                ISetProp(fontAsset, "characterTable", m_FontCharacterTable);

                                // Sort glyph and character tables.
                                ICall(fontAsset, "SortAllTables");

                                // Get and Add Kerning Pairs to Font Asset
                                // TODO: Check and preserve existing adjustment pairs.
                                //if (m_IncludeFontFeatures)
                                //    ISetProp(fontAsset, "fontFeatureTable", GetKerningTable());

                                // Add Font Atlas as Sub-Asset
                                fontAsset.atlasTextures = new Texture2D[] { m_FontAtlasTexture };
                                m_FontAtlasTexture.name = tex_FileName + " Atlas";
                                ISetProp(fontAsset, "atlasWidth", m_AtlasWidth);
                                ISetProp(fontAsset, "atlasHeight", m_AtlasHeight);
                                ISetProp(fontAsset, "atlasPadding", m_Padding);

                                // Special handling due to a bug in earlier versions of Unity.
                                m_FontAtlasTexture.hideFlags = HideFlags.None;
                                fontAsset.material.hideFlags = HideFlags.None;

                                AssetDatabase.AddObjectToAsset(m_FontAtlasTexture, fontAsset);

                                // Assign new font atlas texture to the existing material.
                                fontAsset.material.SetTexture(ShaderUtilities.ID_MainTex, fontAsset.atlasTextures[0]);

                                // Find all Materials referencing this font atlas.
                                Material[] material_references = TMP_EditorUtility.FindMaterialReferences(fontAsset);

                                // Update the Texture reference on the Material
                                for (int i = 0; i < material_references.Length; i++)
                                {
                                    material_references[i].SetTexture(ShaderUtilities.ID_MainTex, m_FontAtlasTexture);
                                    material_references[i].SetFloat(ShaderUtilities.ID_TextureWidth, m_FontAtlasTexture.width);
                                    material_references[i].SetFloat(ShaderUtilities.ID_TextureHeight, m_FontAtlasTexture.height);

                                    int spread = m_Padding + 1;
                                    material_references[i].SetFloat(ShaderUtilities.ID_GradientScale, spread); // Spread = Padding for Brute Force SDF.

                                    material_references[i].SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
                                    material_references[i].SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
                                }
                            }
                            finished(fontAsset);
                        };
                    });
                }
            }
#endif
        }
#endif

    }
}
