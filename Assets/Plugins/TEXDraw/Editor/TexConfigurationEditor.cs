using TexDrawLib;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TEXConfiguration))]
[CanEditMultipleObjects]
public class TEXConfigurationEditor : Editor
{

    private static GUIContent[] fontSets;


    private static int[] fontValues;

    private void OnEnable()
    {
        Undo.undoRedoPerformed += Repaint;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= Repaint;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (OnInspectorGUI_Draw())
            TEXPreference.main.CallRedraw();
    }

    public bool OnInspectorGUI_Draw()
    {
        EditorGUI.BeginChangeCheck();
        DrawPropertiesExcluding(serializedObject);
        var obj = (TEXConfiguration)target;

        if (fontSets == null)
        {
            fontSets = new GUIContent[TEXPreference.main.fonts.Length];
            fontValues = new int[fontSets.Length];
            for (int i = 0; i < fontSets.Length; i++)
            {
                fontSets[i] = new GUIContent(TEXPreference.main.fonts[i].id);
                fontValues[i] = i;
            }
        }
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        return EditorGUI.EndChangeCheck();
    }
}
