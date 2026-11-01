using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EditorTEXUIQuickEditors : EditorWindow
{
    public TEXDraw root;
    public Vector2 scr;
    public Vector2 size;
    public double coolDownUndo = 0;
    public TEXBoxHighlighting highligter;



    public const string helpInfo = "This editor helps you to edit latex text easily on selected TEXDraw object.";

    private void OnGUI()
    {
        root = (TEXDraw)EditorGUILayout.ObjectField(root, typeof(TEXDraw), true);
        if (root)
        {
            var r = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            EditorGUI.BeginChangeCheck();
            var text = highligter.DrawText(root.text, r);
            if (EditorGUI.EndChangeCheck())
            {
                if (coolDownUndo < EditorApplication.timeSinceStartup)
                {
                    Undo.RecordObjects(new UnityEngine.Object[] { root, highligter }, "Editing TEXDraw");
                    coolDownUndo = EditorApplication.timeSinceStartup + 1;
                }
                highligter.SaveBackup();
                root.text = text;
                EditorUtility.SetDirty(root);
            }
            var rr = EditorGUILayout.GetControlRect();
            var cur = highligter.GetCursorPos();
            var cur2 = highligter.GetCursorPosVector();
            GUI.Label(rr, string.Format("Ln: {0}, Col: {1}, Pos: {2}", cur2.y, cur2.x, cur));
        }
        else
        {
            EditorGUILayout.HelpBox(helpInfo, MessageType.Info);
        }
    }



    private void OnEnable()
    {
        highligter = CreateInstance<TEXBoxHighlighting>();
        highligter.hideFlags = HideFlags.HideAndDontSave;
        highligter.attachedWindow = this;
        OnSelectionChange();
    }
    private void OnDisable()
    {
        if (highligter)
            DestroyImmediate(highligter);
    }

    private void OnSelectionChange()
    {
        var g = Selection.activeObject as GameObject;
        root = g && Selection.objects.Length == 1 ? g.GetComponent<TEXDraw>() : null;
        coolDownUndo = 0;
        Repaint();
    }
}
