using UnityEngine;

namespace TexDrawLib
{
    public class TEXConfiguration : ScriptableObject
    {
        static public TEXConfiguration main;

        public static void Initialize()
        {
            if (!main)
            {
                // The only thing that we can found is in the Resource folder
                if (TEXPreference.main)
                    main = TEXPreference.main.configuration;
                if (!main)
                {
                    main = TEXPreference.main.configuration = (TEXConfiguration)Resources.Load("TEXDrawConfiguration");
#if UNITY_EDITOR
                    if (!main)
                    {
                        main = TEXPreference.main.configuration = CreateInstance<TEXConfiguration>();
                        UnityEditor.AssetDatabase.CreateAsset(main, TEXPreference.main.MainFolderPath + "/Resources/TEXDrawConfiguration.asset");
                    }
                    UnityEditor.EditorUtility.SetDirty(TEXPreference.main);
#endif
                }
            }
        }

        public DocumentState Document = new DocumentState() { pixelsPerInch = 96 };

        public ParagraphState Paragraph = new ParagraphState();

        public TypefaceState Typeface = new TypefaceState();

        public MathModeState Math = new MathModeState();

        public int[] GlueTable = new int[100];

        public StringPair[] InlineMacros = new StringPair[0];

        public StringPair[] BlockMacros = new StringPair[0];

        public StringPair[] SymbolAliases = new StringPair[0];

        public string[] GlobalSymbolFonts = new string[0];

    }
}
