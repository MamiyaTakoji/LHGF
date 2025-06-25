using System;
using TexDrawLib;
using UnityEditor;
using UnityEngine;

public class EditorObjectPool : EditorWindow
{
    private Vector2 scr;
#if TEXDRAW_DEBUG
    private static readonly string helpInfo =
        "This is a tool for previewing how many times a class that implements TexDrawLib.IFlushable be instantianted, and preserved in memory." +
        "\nYou can also implement IFlushable and use TEXDrawLib.ObjPool as part of optimizing your in-game performance." +
        "\nIt's very recommended to implement this into non-serializable classes. See TEXDraw/Internal/ListPool.cs for the example of implementation";
#else
    private static readonly string helpInfo = "Add compilation TEXDRAW_DEBUG to enable this feature";

#endif
    private void OnGUI()
    {
        if (Screen.width > 350)
            EditorGUILayout.HelpBox(helpInfo, MessageType.Info);
        else
            EditorGUILayout.Space();
        scr = EditorGUILayout.BeginScrollView(scr, GUILayout.ExpandHeight(true));
#if TEXDRAW_DEBUG
        var l = ObjectPoolShared.objectPools;
        for (int i = 0; i < l.Count; i++)
        {
            EditorGUILayout.LabelField(GetTypeName(l[i].GetType().GetGenericArguments()[0]), string.Format("Total {0} Idle {1}", l[i].countAll, l[i].countInactive), EditorStyles.helpBox);
        }
#endif
        EditorGUILayout.EndScrollView();
    }

    private string GetTypeName(Type t)
    {
        string n = t.Name;
        var arg = t.GetGenericArguments();
        if (arg.Length > 0)
            n = string.Format("{0}<{1}>", n.Substring(0, n.Length - 2), GetTypeName(arg[0]));
        return n;
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }
}
