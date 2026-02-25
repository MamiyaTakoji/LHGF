#if UNITY_EDITOR

using System.Linq;
using TexDrawLib;
using UnityEditor;
using UnityEngine;

//using System;

//TO DO: Add Search Feature & Filter by Type
[CustomEditor(typeof(TEXPreference))]
public class TEXPreferenceEditor : Editor
{
    static internal class Styles
    {
        public static GUIContent none = GUIContent.none;

        public static GUIContent[] HeaderUpdate = new GUIContent[]
        {
            new GUIContent("Auto Refresh"),
            new GUIContent("Auto Refresh"),
            new GUIContent("Refresh Now")
        };

        public static GUIStyle[] HeaderStyles = new GUIStyle[]
        {
            new GUIStyle(EditorStyles.miniButtonLeft),
            new GUIStyle(EditorStyles.miniButtonMid),
            new GUIStyle(EditorStyles.miniButtonMid),
            new GUIStyle(EditorStyles.miniButtonRight)
        };

        public static GUIStyle ManagerFamily = new GUIStyle(EditorStyles.boldLabel);
        public static GUIStyle ManagerChild = new GUIStyle(EditorStyles.miniButton);
        public static GUIStyle FontPreviewSymbols = new GUIStyle(EditorStyles.objectFieldThumb);
        public static GUIStyle FontPreviewRelation = new GUIStyle(EditorStyles.textArea);
        public static GUIStyle FontPreviewEnabled = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle FontPreviewDisabled = new GUIStyle(EditorStyles.label);

        public static GUIStyle[] ManagerHeader = new GUIStyle[]
        {
            new GUIStyle(EditorStyles.miniButtonLeft),
            new GUIStyle(EditorStyles.miniButtonMid),
            new GUIStyle(EditorStyles.miniButtonRight)
        };

        public static GUIContent[] ManagerHeaderContent = new GUIContent[]
        {
            new GUIContent("Fonts"),
            new GUIContent("Options"),
            new GUIContent("Character")
        };

        public static GUIContent ImporterOptionFontMessage = new GUIContent(
                                                                "So far there's nothing to customize for importing a font.");

        public static GUIStyle ImporterOptionFontStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
        public static GUIStyle ImporterPresetArea = new GUIStyle(EditorStyles.textArea);

        public static GUIStyle[] SetterHeader = new GUIStyle[]
        {
            new GUIStyle(EditorStyles.miniButtonLeft),
            new GUIStyle(EditorStyles.miniButtonRight)
        };

        public static GUIContent[] SetterHeaderContent = new GUIContent[]
        {
            new GUIContent("Properties"),
            new GUIContent("Relations")
        };

        public static GUIStyle SetterPreview = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterNextLarger = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterExtendTop = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterExtendMiddle = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterExtendBottom = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterExtendRepeat = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterTitle = new GUIStyle(EditorStyles.label);
        public static GUIStyle SetterFont = new GUIStyle(EditorStyles.label);
        public static GUIStyle GlueLabelH = new GUIStyle(EditorStyles.label);
        public static GUIStyle GlueLabelV = new GUIStyle(EditorStyles.label);
        public static GUIStyle GlueProgBack;
        public static GUIStyle GlueProgBar;
        public static GUIStyle GlueProgText;

        public static GUIContent[] CharMapContents = new GUIContent[0xffff];

        public static GUIContent GetCharMapContent(char c)
        {
            return CharMapContents[c] ?? (CharMapContents[c] = new GUIContent(new string(c, 1)));
        }

        public static GUIContent[] SetterCharMap = new GUIContent[33];

        public static string[] DefaultTypes = new string[]
        {
            ("Numbers"),
            ("Capitals"),
            ("Small"),
            ("Commands"),
            ("Text"),
            ("Unicode")
        };

        public static GUIContent[] CharTypes = new GUIContent[]
        {
            new GUIContent("Ordinary"),
            new GUIContent("Geometry"),
            new GUIContent("Operator"),
            new GUIContent("Relation"),
            new GUIContent("Punctuation"),
            new GUIContent("Open Delimiter"),
            new GUIContent("Close Delimiter"),
            new GUIContent("Big Operator"),
            new GUIContent("Inner"),
        };

        //public static int[] SetterCharMapInt = new int[33];

        public static GUIContent[] HeaderTitles = new GUIContent[3];

        //public static GUIContent[] fontSettings;
        public static GUIStyle Buttons = new GUIStyle(EditorStyles.miniButton);

        static Styles()
        {
            ManagerFamily.alignment = TextAnchor.MiddleCenter;

            ManagerChild.fontSize = 10;
            ManagerChild.fixedHeight = 20;
            foreach (var gui in ManagerHeader)
            {
                gui.fontSize = 10;
            }
            ImporterPresetArea.wordWrap = true;
            ImporterOptionFontStyle.alignment = TextAnchor.MiddleCenter;
            FontPreviewEnabled.alignment = TextAnchor.MiddleCenter;
            FontPreviewSymbols.alignment = TextAnchor.MiddleCenter;
            FontPreviewRelation.alignment = TextAnchor.MiddleCenter;
            FontPreviewDisabled.alignment = TextAnchor.MiddleCenter;
            FontPreviewRelation.fixedHeight = 0;
            FontPreviewRelation.onActive = FontPreviewEnabled.onActive;
            FontPreviewRelation.onNormal = FontPreviewRelation.focused;
            FontPreviewRelation.focused = FontPreviewEnabled.focused;

            SetterTitle.fontStyle = FontStyle.Bold;
            SetterTitle.fontSize = 16;
            SetterTitle.fixedHeight = 25;
            SetterFont.richText = true;
            SetterPreview.fontSize = 34;
            SetterPreview.alignment = TextAnchor.MiddleCenter;
            SetterNextLarger.fontSize = 24;
            SetterNextLarger.alignment = TextAnchor.MiddleCenter;

            SetterExtendTop.fontSize = 24;
            SetterExtendTop.alignment = TextAnchor.MiddleCenter;
            SetterExtendMiddle.fontSize = 24;
            SetterExtendMiddle.alignment = TextAnchor.MiddleCenter;
            SetterExtendBottom.fontSize = 24;
            SetterExtendBottom.alignment = TextAnchor.MiddleCenter;
            SetterExtendRepeat.fontSize = 24;
            SetterExtendRepeat.alignment = TextAnchor.MiddleCenter;

            HeaderTitles = new GUIContent[3]
            {
                new GUIContent("Characters"),
                new GUIContent("Configurations"),
                new GUIContent("Glue Matrix")
            };
            for (int i = 0; i < 3; i++)
            {
                HeaderStyles[i].fontSize = 12;
                HeaderStyles[i].fixedHeight = 24;
            }

            GlueLabelH.alignment = TextAnchor.MiddleRight;
            GlueLabelV.alignment = TextAnchor.MiddleLeft;

            GlueProgBack = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ProgressBarBack");
            GlueProgBar = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ProgressBarBar");
            GlueProgText = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ProgressBarText");
            GlueProgText.alignment = TextAnchor.MiddleCenter;
            Buttons.alignment = TextAnchor.MiddleCenter;
            Buttons.fontSize = 11;
        }
    }

    #region Base of all GUI Renderings

    private static TEXPreference targetPreference;

    //0 = Auto Update; 1 = Manual, No Change Applied; 2 = Manual, Pending Change
    [SerializeField]
    private int changeState = 0;

    [SerializeField]
    private int managerState = 0;

    static public bool willFocusOnImport = false;

    static public Material alphaOnlyMat = null;

    private void OnEnable()
    {
        TEXPreference.Initialize();
        Undo.undoRedoPerformed += RecordRedrawCallback;
        configEditor = CreateEditor(TEXConfiguration.main);
        if (willFocusOnImport)
        {
            FocusOnImporter();
            willFocusOnImport = false;
        }
        if (!alphaOnlyMat)
        {
            alphaOnlyMat = Resources.Load<Material>("TEX-AlphaOnly");
        }
    }

    private void OnDisable()
    {
        if (targetPreference)
        {
            targetPreference.PushToDictionaries();
        }
        DestroyImmediate(configEditor);
        Undo.undoRedoPerformed -= RecordRedrawCallback;
    }

    protected override void OnHeaderGUI()
    {
        base.OnHeaderGUI();
        Rect r = new Rect(46, 24, 146, 16);
        if (headerActive > 0)
        {
            if (GUI.Toggle(r, changeState == 0, Styles.HeaderUpdate[changeState], Styles.Buttons))
            {
                if (changeState == 1)
                {
                    RecordRedraw();
                    changeState = 0;
                }
                else if (changeState == 2)
                {
                    targetPreference.PushToDictionaries();
                    targetPreference.CallRedraw();
                    changeState = 1;
                }
            }
            else
                changeState = changeState == 0 ? 1 : changeState;
        }
        else
        {
            if (GUI.Button(r, Styles.HeaderUpdate[2], Styles.Buttons))
            {
                targetPreference.PushToDictionaries();
                targetPreference.CallRedraw();
                EditorUtility.SetDirty(targetPreference);
            }
        }
    }

    private void SetPreviewFont(Font font)
    {
        if (!font)
            return;
        Styles.FontPreviewEnabled.font = font;
        Styles.FontPreviewSymbols.font = font;
        Styles.FontPreviewRelation.font = font;
        Styles.SetterPreview.font = font;
    }

    public void FocusOnImporter()
    {
        headerActive = 0;
        managerState = 1;
    }

    // Root of all GUI instruction begin here.
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (!targetPreference)
        {
            targetPreference = TEXPreference.main;
            if (selectedFont != null)
                SetPreviewFont(selectedFont.asset);
        }
        RecordUndo();
        DrawHeaderOption();
        if (headerActive == 0)
        {
            // Rect v = EditorGUILayout.GetControlRect(GUILayout.Height(5));
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.labelWidth));

                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < 3; i++)
                {
                    if (GUILayout.Toggle(i == managerState, Styles.ManagerHeaderContent[i], Styles.ManagerHeader[i]))
                        managerState = i;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth /= 2;
                if (managerState == 0) DrawManager();
                else if (managerState == 1) DrawImporter();
                else if (managerState == 2) DrawSetter();
                EditorGUIUtility.labelWidth *= 2;
                EditorGUILayout.EndVertical();
            }

            if (selectedAsset != null)
            {
                // v.xMin += EditorGUIUtility.labelWidth + 4;
                // v.height = Screen.height - ViewerHeight;
                Rect v = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                CheckEvent(false);
                switch (selectedAsset.type)
                {
                    case TexAssetType.Font:
                        DrawViewer(selectedFont, v, null);
                        break;
                    case TexAssetType.Sprite:
                        DrawViewer(selectedSprite, v, (r, ch) =>
                        {
                            var sprite = selectedSprite;
                            var sprt = sprite.GenerateMetric(ch.characterIndex);
                            Graphics.DrawTexture(r, sprite.assets.FirstOrDefault().texture, sprt.uv, 0,0,0,0, sprite.alphaOnly ? alphaOnlyMat : null);
                        });
                        break;
                    case TexAssetType.FontSigned:
                        DrawViewer(selectedSigned, v, (r, ch) =>
                        {
#if TEXDRAW_TMP
                            // Additional measurements for accurate display in TMP
                            var r2 = r;
                            var mx = selectedSigned.GenerateMetric(ch.characterIndex);
                            var ratio = Mathf.Min(1, (mx.size.w + mx.size.y) / selectedSigned.LineHeight());
                            r.height *= ratio;
                            r.width = (mx.size.x + mx.size.z) / (mx.size.y + mx.size.w) * r.height;
                            r.y += (r2.height - r.height) / 2f;
                            r.x += (r2.width - r.width) / 2f;
                            var sprt = selectedSigned.asset.atlasTextures[mx.index];
                            Graphics.DrawTexture(r, sprt, mx.uv, 0,0,0,0, alphaOnlyMat);
#endif
                        });
                        break;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

        }
        else if (headerActive == 1)
            DrawConfiguration();
        else if (headerActive == 2)
            DrawGlue();

        serializedObject.ApplyModifiedProperties();
    }

    [SerializeField]
    private int headerActive = 0;

    private void DrawHeaderOption()
    {
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < 3; i++)
        {
            if (GUILayout.Toggle(i == headerActive, Styles.HeaderTitles[i], Styles.HeaderStyles[i]))
                headerActive = i;
        }
        EditorGUILayout.EndHorizontal();
    }

    #endregion

    #region Character Management

    private const float ViewerHeight = 120f;

    [SerializeField]
    private Vector2 childSize;

    [SerializeField]
    private Vector2 ManagerScroll;

    [SerializeField]
    private Vector2 SetterScroll;

    [SerializeField]
    private int selectedFontIdx;

    [SerializeField]
    private int selectedCharIdx;

    private TexFont selectedFont
    {
        get { return selectedAsset as TexFont; }
        set { selectedFontIdx = value.assetIndex; SetPreviewFont(selectedFont.asset); }
    }

    private TexAsset selectedAsset
    {
        get
        {
            if (selectedFontIdx >= targetPreference.fonts.Length) selectedFontIdx = 0;
            return targetPreference.fonts[selectedFontIdx];
        }
    }

    private TexSprite selectedSprite
    {
        get { return selectedAsset as TexSprite; }
        set { selectedFontIdx = value.assetIndex; }
    }

    private TexFontSigned selectedSigned
    {
        get { return selectedAsset as TexFontSigned; }
        set { selectedFontIdx = value.assetIndex; }
    }

    private TexChar selectedChar
    {
        get
        {
            if (selectedCharIdx >= selectedAsset.chars.Length) selectedCharIdx = selectedAsset.chars.Length - 1;
            return selectedAsset.chars[selectedCharIdx];
        }
        set
        {
            selectedCharIdx = value.index;
            selectedFontIdx = value.fontIndex;
        }
    }

    private bool lastCharChanged = false;
    private int setterState = 0;

    private void DrawManager()
    {
        ManagerScroll = EditorGUILayout.BeginScrollView(ManagerScroll, false, false, GUILayout.ExpandHeight(true));
        int Total = targetPreference.fonts.Length;
        string lastCaption = null;
        for (int i = 0; i < Total; i++)
        {
            //Draw Headers First, if needed
            TexAsset d = targetPreference.fonts[i];
            if (lastCaption != d.editorMetadata.category)
            {
                GUILayout.Label(lastCaption = d.editorMetadata.category, Styles.ManagerFamily);
            }
            //Draw the font

            if ((selectedFontIdx == i) != GUILayout.Toggle(selectedFontIdx == i, d.id, Styles.ManagerChild))
            {
                selectedFontIdx = i;
                selectedCharIdx = Mathf.Clamp(selectedCharIdx, 0, d.chars.Length - 1);
                if (selectedFont != null)
                    SetPreviewFont(selectedFont.asset);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private GUIStyle SubDetermineStyle(TexChar c)
    {
        if (!string.IsNullOrEmpty(c.symbol))
            return Styles.FontPreviewSymbols;
        else if (c.larger.Has || c.extension.enabled)
            return Styles.FontPreviewRelation;
        else
            return Styles.FontPreviewEnabled;
    }

    private void DrawViewer(TexAsset asset, Rect drawRect, System.Action<Rect, TexChar> draw)
    {
        if (!asset || !asset.Texture())
        {
            // Something wrong?

            EditorGUI.LabelField(drawRect, "The Asset is NULL or missing. you should reimport again.", Styles.ImporterOptionFontStyle);
            return;
        }

        //Rect r;
        var chars = asset.chars;
        if (Event.current.type == EventType.Repaint)
        {
            childSize = new Vector2(drawRect.width / 8f - 2, asset.LineHeight() * (drawRect.width / 8f));
        }
        EditorGUILayout.GetControlRect(false, GUILayout.Height((childSize.y + 2) * Mathf.Ceil(chars.Length / 8f)));
        if (draw == null)
        {
            Styles.FontPreviewEnabled.fontSize = Styles.FontPreviewSymbols.fontSize = Styles.FontPreviewRelation.fontSize = (int)childSize.x / 2;
        }
        for (int i = 0; i < chars.Length; i++)
        {
            int x = i % 8, y = i / 8, l = selectedCharIdx;
            var r = new Rect(new Vector2((childSize.x + 2) * x + drawRect.x, (childSize.y + 2) * y + drawRect.y), childSize);
            var ch = chars[i];
            if (CustomToggle(r, selectedCharIdx == i, draw == null ? Styles.GetCharMapContent(ch.characterIndex) : Styles.none, SubDetermineStyle(ch)))
            {
                int newS = i + (selectedCharIdx - l);
                if (newS != selectedCharIdx && lastCharChanged)
                {
                    RecordDirty();
                    lastCharChanged = false;
                }
                selectedCharIdx = newS;
            }
            if (Event.current.type == EventType.Repaint && draw != null)
            {
                draw(r, ch);
            }
        }
    }

    private void DrawImporter()
    {
        //GUILayoutOption max = GUILayout.MaxWidth(EditorGUIUtility.labelWidth);

        GUILayout.Label(selectedAsset.id, Styles.SetterTitle);

        // The options for sprite assets
        if (selectedSprite != null)
        {
            EditorGUI.BeginChangeCheck();
            float w = EditorGUILayout.FloatField("Line Offset", selectedSprite.lineOffset);
            float h = EditorGUILayout.FloatField("Line Height", selectedSprite.lineHeight);
            bool v = EditorGUILayout.Toggle("Alpha Only", selectedSprite.alphaOnly);
            if (EditorGUI.EndChangeCheck())
            {
                RecordDirty();
                selectedSprite.lineOffset = w;
                selectedSprite.lineHeight = h;
                selectedSprite.alphaOnly = v;
            }
            if (GUILayout.Button("Apply"))
            {
                selectedSprite.ImportDictionary();
                targetPreference.CallRedraw();
            }
        }

        // The options for import presets
        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();
        var ctgImport = (ImportCharPresetsType)EditorGUILayout.EnumPopup("Import Preset", TexCharPresets.guessEnumPresets(selectedAsset.editorMetadata.catalogsToken));
        if (EditorGUI.EndChangeCheck())
        {
            RecordUndo();
            selectedAsset.editorMetadata.catalogsToken = TexCharPresets.charsFromEnum(ctgImport);
            selectedAsset.ImportCharacters(selectedAsset.editorMetadata.catalogsToken);
        }

        selectedAsset.editorMetadata.catalogsToken = GUILayout.TextArea(selectedAsset.editorMetadata.catalogsToken, Styles.ImporterPresetArea, GUILayout.Height(90));

        if (GUILayout.Button("Reimport"))
        {
            selectedAsset.ImportCharacters(selectedAsset.editorMetadata.catalogsToken);
        }

        EditorGUILayout.Space();

        if (ctgImport == ImportCharPresetsType.Custom)
        {
            GUILayout.Label("Preview:");
            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
            {
                var convers = TexCharPresets.CharsFromString(selectedAsset.editorMetadata.catalogsToken);
                GUILayout.Label(string.Join(", ", convers) + " (" + convers.Length.ToString() + " )", Styles.ImporterOptionFontStyle, GUILayout.ExpandHeight(true));

            }
            else
                GUILayout.Label("X");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Please note that characters outside from the list is still available. Only type on characters that need to be turn into symbols. Max allowed symbol count is 256 per font", Styles.ImporterOptionFontStyle);
        }

    }

    private void DrawSetter()
    {
        GUILayout.Label(selectedAsset.id, Styles.SetterTitle, GUILayout.MaxHeight(25));
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("ID");
            GUILayout.Label(string.Format("<b>{0}</b> (#{1:X})", selectedAsset.id, selectedAsset.assetIndex), Styles.SetterFont);
        }
        EditorGUILayout.EndHorizontal();
        TexChar c = selectedChar;
        //        if (c.supported)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < 2; i++)
            {
                if (GUILayout.Toggle(i == setterState, Styles.SetterHeaderContent[i], Styles.SetterHeader[i]))
                    setterState = i;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            if (setterState == 0)
            {
                // Create an thumbnail
                switch (selectedAsset.type)
                {
                    case TexAssetType.Font:
                        EditorGUILayout.LabelField(Styles.GetCharMapContent(selectedAsset.chars[selectedCharIdx].characterIndex),
                            Styles.SetterPreview, GUILayout.Height(selectedFont.asset.lineHeight * 2.2f));
                        break;
                    case TexAssetType.Sprite:
                        Rect r2 = EditorGUILayout.GetControlRect(GUILayout.Height(selectedSprite.lineHeight * EditorGUIUtility.labelWidth));
                        EditorGUI.LabelField(r2, GUIContent.none, Styles.SetterPreview);
                        r2.width *= .5f;
                        r2.x += r2.width * .5f;

                        var sprt = selectedSprite.GenerateMetric(selectedChar.characterIndex);
                        if (Event.current.type == EventType.Repaint)
                            Graphics.DrawTexture(r2, selectedSprite.Texture(), sprt.uv, 0, 0, 0, 0, selectedSprite.alphaOnly ? alphaOnlyMat : null);
                        break;
#if TEXDRAW_TMP
                    case TexAssetType.FontSigned:
                        sprt = selectedSigned.GenerateMetric(selectedChar.characterIndex);
                        var mh = selectedSigned.LineHeight();
                        var h = (sprt.size.y + sprt.size.w) / (sprt.size.x + sprt.size.z);
                        var s = Mathf.Min(1f, 1.5f / h);
                        r2 = EditorGUILayout.GetControlRect(GUILayout.Height(Mathf.Min(1.5f, mh) * EditorGUIUtility.labelWidth));
                        EditorGUI.LabelField(r2, GUIContent.none, Styles.SetterPreview);
                        var r3 = new Rect
                        {
                            size = new Vector2(EditorGUIUtility.labelWidth, h * EditorGUIUtility.labelWidth) * s,
                            center = r2.center
                        };

                        if (Event.current.type == EventType.Repaint)
                            Graphics.DrawTexture(r3, selectedSigned.asset.atlasTextures[sprt.index], sprt.uv, 0, 0, 0, 0, alphaOnlyMat);
                        break;
#endif
                    default:
                        break;
                }

                // Basic info stuff
                EditorGUILayout.LabelField("Index", string.Format("<b>{0}</b> (#{0:X2})", selectedCharIdx), Styles.SetterFont);
                EditorGUILayout.LabelField("Character Index", "<b>" +
                    selectedChar.characterIndex.ToString() + "</b> (#" + ((int)selectedChar.characterIndex).ToString("X2")
                    + ")", Styles.SetterFont);

                EditorGUILayout.LabelField("Symbol Definition");
                EditorGUILayout.BeginHorizontal();
                {
                    c.symbol = EditorGUILayout.TextField(c.symbol); //Primary
                }
                EditorGUILayout.EndHorizontal();

                c.type = (CharType)EditorGUILayout.EnumPopup("Symbol Type", c.type);
            }
            else
            {
                SetterScroll = EditorGUILayout.BeginScrollView(SetterScroll, GUILayout.ExpandHeight(true));
                {
                    EditorGUILayout.LabelField(string.Format("Hash \t : <b>{0}</b> (#{1:X1})", (int)c.characterIndex, (int)c.characterIndex), Styles.SetterFont);
                    c.larger = SubDrawThumbnail(c.larger, "Is Larger Character Exist?", Styles.SetterNextLarger);
                    EditorGUILayout.Space();
                    if (EditorGUILayout.ToggleLeft("Is Part of Extension?", c.extension.enabled))
                    {
                        EditorGUI.indentLevel++;
                        c.extension.enabled = true;
                        c.extension.horizontal = EditorGUILayout.ToggleLeft("Is This Horizontal?", c.extension.horizontal);

                        c.extension.top = SubDrawThumbnail(c.extension.top, c.extension.horizontal ? "Has Left Extension?" : "Has Top Extension?", Styles.SetterExtendTop);
                        c.extension.mid = SubDrawThumbnail(c.extension.mid, "Has Middle Extension?", Styles.SetterExtendMiddle);
                        c.extension.bottom = SubDrawThumbnail(c.extension.bottom, c.extension.horizontal ? "Has Right Extension?" : "Has Bottom Extension?", Styles.SetterExtendBottom);
                        c.extension.repeat = SubDrawThumbnail(c.extension.repeat, "Has Tiled Extension?", Styles.SetterExtendRepeat);

                        EditorGUI.indentLevel--;
                    }
                    else
                        c.extension.enabled = false;
                }
                EditorGUILayout.EndScrollView();
            }
            if (EditorGUI.EndChangeCheck())
            {
                RecordDirty();
                lastCharChanged = true;
            }
        }
    }

    private TexCharRef SubDrawThumbnail(TexCharRef hash, string confirmTxt, GUIStyle style)
    {
        if (EditorGUILayout.ToggleLeft(confirmTxt, hash.Has))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.indentLevel++;

            if (!hash.Has)
                hash = selectedChar;
            TexAsset font = hash.GetFont;
            char ch = hash.character;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            font = targetPreference.fonts[Mathf.Clamp(EditorGUILayout.IntField(font.assetIndex), 0, targetPreference.fonts.Length)];
            ch = (char)Mathf.Clamp(EditorGUILayout.IntField((int)ch), 0, 0xFFFF);
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
            if (font.type == TexAssetType.Font)
            {
                //r.height = targetFont.Font_Asset.lineHeight * 1.7f;
                style.font = ((TexFont)font).asset;
                EditorGUILayout.LabelField(Styles.GetCharMapContent(font.indexes.TryGetValue(ch, out TexChar chr) ? chr.characterIndex : '\0'), style);
            }
#if TEXDRAW_TMP
            else if (font.type == TexAssetType.FontSigned)
            {
                //r.height = targetFont.font_lineHeight * r.width;
                 var s = ((TexFontSigned)font).GenerateMetric(ch);
                var st = ((TexFontSigned)font).asset;
                if (st)
                {
                    var r = EditorGUILayout.GetControlRect(GUILayout.Height(40f * (s.size.y + s.size.w) / (s.size.x + s.size.z)), GUILayout.Width(40));
                    EditorGUI.LabelField(r, GUIContent.none, style);
                    Graphics.DrawTexture(r, st.atlasTextures[s.index], s.uv, 0, 0, 0, 0, alphaOnlyMat);
                }
            }
#endif
            else
            {
                //r.height = targetFont.font_lineHeight * r.width;
                var sprite = ((TexSprite)font);
                var sprt = sprite.GenerateMetric(ch);
                
                var r = EditorGUILayout.GetControlRect(GUILayout.Height(40), GUILayout.Width(40));
                EditorGUI.LabelField(r, GUIContent.none, style);
                Graphics.DrawTexture(r, sprite.assets.FirstOrDefault().texture, sprt.uv, 0, 0, 0, 0, sprite.alphaOnly ? alphaOnlyMat : null);
            }
            EditorGUILayout.EndHorizontal();
            //EditorGUILayout.GetControlRect(GUILayout.Height(Mathf.Max(r.height - 18, 36)));
            if (EditorGUI.EndChangeCheck())
                return new TexCharRef(font.name, ch);
            else
                return hash;
        }
        else
            return new TexCharRef();
    }

    private void CheckEvent(bool noCmd)
    {
        Event e = Event.current;
        if (headerActive == 0 && selectedAsset != null)
        {
            if (e.isKey & e.type != EventType.KeyUp)
            {
                if (e.control | noCmd)
                {
                    var length = selectedAsset.chars.Length;
                    int verticalJump = 8;
                    if (e.keyCode == KeyCode.UpArrow)
                        selectedCharIdx = (int)Mathf.Repeat(selectedCharIdx - verticalJump, length);
                    else if (e.keyCode == KeyCode.DownArrow)
                        selectedCharIdx = (int)Mathf.Repeat(selectedCharIdx + verticalJump, length);
                    else if (e.keyCode == KeyCode.LeftArrow)
                        selectedCharIdx = (int)Mathf.Repeat(selectedCharIdx - 1, length);
                    else if (e.keyCode == KeyCode.RightArrow)
                        selectedCharIdx = (int)Mathf.Repeat(selectedCharIdx + 1, length);
                    else if (e.keyCode == KeyCode.Home)
                        selectedAsset.chars[selectedCharIdx].type = (CharType)(int)Mathf.Repeat((int)selectedAsset.chars[selectedCharIdx].type - 1, 9);
                    else if (e.keyCode == KeyCode.End)
                        selectedAsset.chars[selectedCharIdx].type = (CharType)(int)Mathf.Repeat((int)selectedAsset.chars[selectedCharIdx].type + 1, 9);
                    else
                        goto skipUse;

                    //This is just estimation... maybe?
                    float ratio = selectedAsset.LineHeight() * ((Screen.width - EditorGUIUtility.labelWidth - 60) / 250) + 10;
                    childSize.y = Mathf.Clamp(childSize.y, (selectedCharIdx / verticalJump - 3) * ratio, (selectedCharIdx / verticalJump - 1) * ratio);
                    e.Use();
                skipUse:
                    return;
                }
            }
        }
    }

    #endregion

    #region Configuration

    private const float configHeight = 355;
    // Vector2 configScroll;

    private Editor configEditor;

    private void DrawConfiguration()
    {
        EditorGUI.BeginChangeCheck();
        targetPreference.defaultMaterial = (Material)EditorGUILayout.ObjectField("Default Material", targetPreference.defaultMaterial, typeof(Material), true);
        targetPreference.spriteMaterial = (Material)EditorGUILayout.ObjectField("Sprite Material", targetPreference.spriteMaterial, typeof(Material), true);
        configEditor.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            RecordDirty();
        }
    }

    #endregion

    #region Glue Management

    private void DrawGlue()
    {
        EditorGUILayout.HelpBox("This section configures how much additional space is applied between characters by looking on each types. If you looking for what characters is considered as the type then look at 'Characters' tab.", MessageType.Info);
        EditorGUILayout.Space();

        labelMatrixHeight = (Screen.width / (Screen.dpi / 96) - 150) / 9f;
        glueSimmetry = GUILayout.Toggle(glueSimmetry, "Edit Symmetrically", Styles.Buttons, GUILayout.Height(22));
        SubDrawMatrix();
    }

    private int GlueGet(int l, int r)
    {
        return targetPreference.configuration.GlueTable[l * 10 + r];
    }

    private void GlueSet(int l, int r, int v)
    {
        targetPreference.configuration.GlueTable[l * 10 + r] = v;
        EditorUtility.SetDirty(targetPreference.configuration);
        RecordDirty();
    }

    private const float labelMatrixWidth = 110;
    private float labelMatrixHeight = 38;

    [SerializeField]
    private bool glueSimmetry = true;

    private Vector2 scrollGlue;

    private void SubDrawMatrix()
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(labelMatrixWidth));
        r.height = labelMatrixHeight;
        GUI.matrix = Matrix4x4.TRS(new Vector2(labelMatrixWidth - r.x + 36, r.y + labelMatrixWidth - 16), Quaternion.Euler(0, 0, -90), Vector3.one);
        r.position = Vector2.zero;
        for (int i = 9; i-- > 0;)
        {
            EditorGUI.LabelField(r, Styles.CharTypes[i], Styles.GlueLabelV);
            r.y += labelMatrixHeight;
        }
        GUI.matrix = Matrix4x4.identity;
        EditorGUILayout.GetControlRect(GUILayout.Height(labelMatrixWidth - 36));
        scrollGlue = EditorGUILayout.BeginScrollView(scrollGlue, GUILayout.Height(Screen.height - 320));
        r = EditorGUILayout.GetControlRect(GUILayout.Width(labelMatrixWidth));
        r.height = labelMatrixHeight;
        float xx = r.x;
        int cur, now;
        for (int y = 0; y < 9; y++)
        {
            GUI.Label(r, Styles.CharTypes[y], Styles.GlueLabelH);
            r.x += labelMatrixWidth;
            r.width = labelMatrixHeight;
            for (int x = 9; x-- > 0;)
            {
                if (glueSimmetry)
                {
                    cur = GlueGet(y, x) != GlueGet(x, y) ? -10 : GlueGet(y, x);
                    now = CustomTuner(r, cur);
                    if (cur != now)
                    {
                        GlueSet(y, x, now);
                        GlueSet(x, y, now);
                    }
                }
                else
                {
                    cur = GlueGet(y, x);
                    now = CustomTuner(r, cur);
                    if (cur != now)
                        GlueSet(y, x, now);
                }
                r.x += labelMatrixHeight;
                if (glueSimmetry && ((8 - x) >= (8 - y)))
                    break;
            }
            r.x = xx;
            r.y += labelMatrixHeight;
            r.width = labelMatrixWidth;
        }
        EditorGUILayout.GetControlRect(GUILayout.Height(labelMatrixHeight * 9));
        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region Undo & Functionality

    private void RecordDirty()
    {
        if (headerActive == 0 && selectedAsset)
        {
            EditorUtility.SetDirty(selectedAsset);
        }
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(targetPreference);
        switch (changeState)
        {
            case 0:
                RecordRedraw();
                break;
            case 1:
                changeState = 2;
                break;
        }
    }

    private void RecordRedrawCallback()
    {
        switch (changeState)
        {
            case 0:
                RecordRedraw();
                break;
            case 1:
                changeState = 2;
                break;
        }
    }

    private void RecordRedraw()
    {
        if (headerActive == 1)
            targetPreference.PushToDictionaries();
        if (headerActive > 0)
            targetPreference.CallRedraw();
    }

    private void RecordUndo()
    {
        //   Undo.IncrementCurrentGroup();
        Undo.RecordObjects(new Object[] { targetPreference, this }, "Changes to TEXDraw Preference");
    }

    #endregion

    #region Custom GUI Controls

    private const int customToggleHash = 0x05f8;

    private bool CustomToggle(Rect r, bool value, GUIContent content, GUIStyle style)
    {
        //TO DO: Add functionality for Tab & Page Up/Down
        int controlID = GUIUtility.GetControlID(customToggleHash, FocusType.Passive);
        bool result = GUI.Toggle(r, value, content, style);
        if (value != result)
            GUIUtility.keyboardControl = controlID;
        if (GUIUtility.keyboardControl == controlID)
            CheckEvent(true);
        return result;
    }

    private const int customTunerHash = 0x08e3;

    private int CustomTuner(Rect r, int value)
    {
        int controlID = GUIUtility.GetControlID(customTunerHash, FocusType.Passive, r);
        Event current = Event.current;
        EventType typeForControl = current.GetTypeForControl(controlID);
        if (typeForControl == EventType.Repaint)
        {
            Styles.GlueProgBack.Draw(r, false, false, false, false);
            Rect r2 = new Rect(r);
            r2.yMin = Mathf.Lerp(r2.yMax, r2.yMin, value == 3 ? 1 : (value * 0.3f + 0.1f));
            if (value > 0)
                Styles.GlueProgBar.Draw(r2, false, false, false, false);
            Styles.GlueProgText.Draw(r, value == -10 ? "--" : value.ToString(), false, false, false, false);
        }
        else if (typeForControl == EventType.MouseDrag)
        {
            Vector2 mousePos = (current.mousePosition);
            if (!r.Contains(mousePos))
                return value;
            float normValue = Mathf.InverseLerp(r.yMin, r.yMax, mousePos.y);
            value = Mathf.Clamp(Mathf.FloorToInt((Mathf.Sqrt((1 - normValue)) / 0.6f - 0.4f) * 3f), -1, 10);
        }
        else if (typeForControl == EventType.MouseDown)
        {
            if (!r.Contains(current.mousePosition))
                return value;
            return (int)Mathf.Repeat(value + (current.shift ? -1 : 1) + 1, 5) - 1;
        }
        return value;
    }

    #endregion

#if TEXDRAW_TMP


#endif
}

#endif
