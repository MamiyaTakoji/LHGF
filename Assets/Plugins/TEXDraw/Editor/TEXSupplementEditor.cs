//using TexDrawLib;
//using UnityEditor;

//[CustomEditor(typeof(TEXDrawSupplementBase), true)]
//[CanEditMultipleObjects]
//public class TEXSupplementEditor : Editor
//{
//    [System.NonSerialized]
//    protected string helpGUI;

//    [System.NonSerialized]
//    protected MessageType helpType;

//    public static bool isHelpShown = true;

//    protected virtual void OnEnable()
//    {
//        var att = target.GetType().GetCustomAttributes(typeof(TEXSupHelpTip), true);
//        if (att.Length > 0)
//        {
//            var attr = (TEXSupHelpTip)att[0];
//            helpGUI = attr.text;
//            helpType = attr.beta ? MessageType.Warning : MessageType.Info;
//            if (attr.beta)
//                helpGUI = "[This supplement is in development stage]\n" + helpGUI;
//        }
//        else
//        {
//            helpGUI = "[This supplement doesn't have any additional info]";
//            helpType = MessageType.None;
//        }
//    }

//    private string[] excludedProps = new string[] { "m_Script" };

//    public override void OnInspectorGUI()
//    {
//        if (isHelpShown)
//        {
//            EditorGUILayout.HelpBox(helpGUI, helpType);
//        }
//        serializedObject.Update();
//        DrawPropertiesExcluding(serializedObject, excludedProps);
//        serializedObject.ApplyModifiedProperties();
//        //base.OnInspectorGUI();
//    }
//}

//[CustomEditor(typeof(TEXDrawMeshEffectBase), true)]
//[CanEditMultipleObjects]
//public class TEXPostEffectEditor : TEXSupplementEditor
//{
//    protected override void OnEnable()
//    {
//        base.OnEnable();
//        if (helpGUI[0] == '[')
//            helpGUI = "[Post Effect] " + helpGUI;
//        else
//            helpGUI = "[Post Effect]\n" + helpGUI;
//    }
//}
