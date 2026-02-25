//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace TexDrawLib
//{
//    public class TEXDatasets : ScriptableObject
//    {
//        static public TEXDatasets main;

//        public static void Initialize()
//        {
//            if (!main)
//            {
//                // The only thing that we can found is in the Resource folder
//                if (TEXPreference.main)
//                    main = TEXPreference.main.datasets;
//                if (!main)
//                {
//                    main = TEXPreference.main.datasets = (TEXDatasets)Resources.Load("TEXDrawDatasets");
//#if UNITY_EDITOR
//                    if (!main)
//                    {
//                        main = TEXPreference.main.datasets = CreateInstance<TEXDatasets>();
//                        UnityEditor.AssetDatabase.CreateAsset(main, TEXPreference.main.MainFolderPath + "/Resources/TEXDrawDatasets.asset");
//                    }
//                    UnityEditor.EditorUtility.SetDirty(TEXPreference.main);
//#endif
//                }
//            }
//        }

//        public struct TexSymbol
//        {
//            public int id;
//            public string symbol;
//            public TexCharKind kind;
//        }

//        public struct TexLigature
//        {
//            public int id;
//            public string ligature;
//        }

//        public struct TexRelations
//        {
//            public int id;
//            public int larger;
//            public bool extendable;
//        }

//        public struct TexExtendable
//        {
//            public int id;
//            public int top;
//            public int bottom;
//            public int tiled;
//            public int isHorizontal;
//        }

//        public string[] fontMappings;

//        public TexSymbol[] symbols;

//        public TexLigature[] ligatures;



//    }
//}
