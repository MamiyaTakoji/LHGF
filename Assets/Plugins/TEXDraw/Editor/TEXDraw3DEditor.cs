using TexDrawLib;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TEXDraw3D))]
[CanEditMultipleObjects]
public class TEXDraw3DEditor : Editor
{
    private SerializedProperty m_Text;
    private SerializedProperty m_Size;
    private SerializedProperty m_Align;
    private SerializedProperty m_Color;
    private SerializedProperty m_Padding;
    // private SerializedProperty m_ScrollArea;
    private SerializedProperty m_Overflow;
    private SerializedProperty m_Material;
    private SerializedProperty m_PixelsPerUnit;

    static bool foldExpand = false;

    // Use this for initialization
    private void OnEnable()
    {
        m_Text = serializedObject.FindProperty("m_Text");
        m_Size = serializedObject.FindProperty("m_Size");
        m_Color = serializedObject.FindProperty("m_Color");
        m_Align = serializedObject.FindProperty("m_Alignment");
        m_Padding = serializedObject.FindProperty("m_Padding");
        // m_ScrollArea = serializedObject.FindProperty("m_ScrollArea");
        m_Overflow = serializedObject.FindProperty("m_Overflow");
        m_Material = serializedObject.FindProperty("m_Material");
        m_PixelsPerUnit = serializedObject.FindProperty("m_PixelsPerUnit");
        Undo.undoRedoPerformed += Redraw;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= Redraw;
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        //TEXBoxHighlighting.DrawText(m_Text);
        EditorGUILayout.PropertyField(m_Text);

        EditorGUILayout.PropertyField(m_Size);
        foldExpand = EditorGUILayout.Foldout(foldExpand, "More Properties");
        if (foldExpand)
        {
            EditorGUI.indentLevel++;
            TEXSharedEditor.DoTextAligmentControl(EditorGUILayout.GetControlRect(), m_Align);
            // EditorGUILayout.PropertyField(m_ScrollArea);
            EditorGUILayout.PropertyField(m_Padding, true);
            EditorGUILayout.PropertyField(m_Overflow);
            EditorGUILayout.PropertyField(m_Color);
            TEXSharedEditor.DoMaterialGUI(m_Material, (ITEXDraw)target);
#if !TEXDRAW_TMP
            EditorGUILayout.PropertyField(m_PixelsPerUnit);
#endif
            EditorGUI.indentLevel--;
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            Redraw();
        }

    }

    public void Redraw()
    {
        foreach (TEXDraw3D i in (serializedObject.targetObjects))
        {
            i.Redraw();
            i.Repaint();
        }
    }

    [MenuItem("GameObject/3D Object/TEXDraw 3D", false, 3300)]
    public static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        TEXPreference.Initialize();
        // Create a custom game object
        GameObject go = new GameObject("TEXDraw 3D");
        go.AddComponent<TEXDraw3D>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
}
