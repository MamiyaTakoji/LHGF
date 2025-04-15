using System;
using System.Collections.Generic;
using TexDrawLib;
using UnityEngine;

[AddComponentMenu("TEXDraw/TEXDraw 3D", 2), ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[HelpURL("https://willnode.gitlab.io/texdraw/")]
#if TEXDRAW_DEBUG
[SelectionBase]
#endif
public class TEXDraw3D : MonoBehaviour, ITEXDraw
{
    
    public TEXPreference preference { get { return TEXPreference.main; } }

    [NonSerialized]
    private List<TEXDraw3DRenderer> m_Renderers = new List<TEXDraw3DRenderer>();

    [SerializeField, TextArea(5, 10)]
    private string m_Text = "$$TEXDraw$$";

    [SerializeField]
    private float m_Size = 36f;

    [SerializeField]
    private float m_PixelsPerUnit = 1f;

    [SerializeField]
    private Color m_Color = Color.white;

    [SerializeField]
    private Vector2 m_Alignment = Vector2.one * 0.5f;

    [SerializeField]
    private Rect m_ScrollArea = new Rect();

    [SerializeField]
    private TexRectOffset m_Padding = new TexRectOffset(2, 2, 2, 2);

    [SerializeField]
    private Overflow m_Overflow = Overflow.Hidden;

    [SerializeField]
    private Material m_Material;

    public virtual string text
    {
        get
        {
            return m_Text;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                if (string.IsNullOrEmpty(m_Text))
                    return;
                m_Text = "";
                Redraw();
            }
            else if (m_Text != value)
            {
                m_Text = value;
                Redraw();
            }
        }
    }

    public virtual float size
    {
        get
        {
            return m_Size;
        }
        set
        {
            if (m_Size != value)
            {
                m_Size = Mathf.Max(value, 0f);
                Redraw();
            }
        }
    }

    public virtual Color color
    {
        get
        {
            return m_Color;
        }
        set
        {
            if (m_Color != value)
            {
                m_Color = value;
                Redraw();
            }
        }
    }

    public virtual Material material
    {
        get
        {
            return m_Material;
        }
        set
        {
            if (m_Material != value)
            {
                m_Material = value;
                Redraw();
            }
        }
    }

    public virtual Vector2 alignment
    {
        get => m_Alignment;
        set
        {
            if (m_Alignment != value)
            {
                m_Alignment = value;
                SetTextDirty();
            }
        }
    }

    public virtual TexRectOffset padding
    {
        get => m_Padding;
        set
        {
            m_Padding = value;
            Redraw();
        }
    }

    public virtual Rect scrollArea
    {
        get => m_ScrollArea;
        set
        {
            if (m_ScrollArea != value)
            {
                m_ScrollArea = value;
                Redraw();
            }
        }
    }


    public virtual Overflow overflow
    {
        get => m_Overflow;
        set
        {
            if (m_Overflow != value)
            {
                m_Overflow = value;
                Redraw();
            }
        }
    }

#if UNITY_EDITOR

    [ContextMenu("Open Preference")]
    private void OpenPreference()
    {
        UnityEditor.Selection.activeObject = preference;
    }

    [ContextMenu("Trace Output")]
    private void TraceOutput()
    {
        UnityEditor.EditorGUIUtility.systemCopyBuffer = orchestrator.Trace();
        Debug.Log("The trace output has been copied to clipboard.");
    }

#endif

    #region Engine

    private void OnEnable()
    {
        if (!preference)
        {
            TEXPreference.Initialize();
        }

        orchestrator = new TexOrchestrator();
        GetComponentsInChildren<TEXDraw3DRenderer>(true, m_Renderers);

        Font.textureRebuilt += OnFontRebuild;
        Redraw();
    }

    public void SetTextDirty()
    {
        Redraw();
    }

    public void SetTextDirty(bool now)
    {
        Redraw();
    }

    private void LateUpdate()
    {
        if (transform.hasChanged)
        {
            Redraw();
            transform.hasChanged = false;
        }
    }

    private bool _onRendering = false;

    private TexOrchestrator m_orchestrator;

    public TexOrchestrator orchestrator { get => m_orchestrator; private set => m_orchestrator = value; }

    public void Redraw()
    {
        // Multi-threading issue with OnFontRebuilt
        if (_onRendering)
            return;
        if (isActiveAndEnabled)
        {
            _onRendering = true;
            try
            {
#if UNITY_EDITOR
                if (preference.editorReloading)
                {
                    return;
                }
#endif
                orchestrator.initialColor = color;
                orchestrator.initialSize = size;
                orchestrator.pixelsPerInch = TEXConfiguration.main.Document.pixelsPerInch;
                orchestrator.alignment = m_Alignment;

                //orchestrator.Parse(m_Text);
                orchestrator.ResetParser();
                orchestrator.parserState.Document.retinaRatio = Mathf.Max(orchestrator.parserState.Document.retinaRatio, m_PixelsPerUnit);
                orchestrator.latestAtomCache = orchestrator.parser.Parse(m_Text, orchestrator.parserState);
                orchestrator.InputCanvasSize(rectTransform.rect, new Rect(), new TexRectOffset(), Vector2Int.zero, m_Overflow);
                orchestrator.Box();
                orchestrator.Render();

                var vertexes = orchestrator.rendererState.vertexes;
                for (int i = vertexes.Count; i < m_Renderers.Count; i++)
                {
                    if (m_Renderers[i])
                    {
                        m_Renderers[i].FontMode = -1;
                        m_Renderers[i].ForceRender();
                    }
                }
                for (int i = 0; i < vertexes.Count; i++)
                {
                    if (i >= m_Renderers.Count)
                        m_Renderers.Add(CreateNewRenderer());
                    m_Renderers[i].FontMode = vertexes[i].m_Font;
                    m_Renderers[i].Redraw();
                    m_Renderers[i].Repaint();
                }
            }
            finally
            {
                _onRendering = false;
            }
        }
    }

    public void Repaint()
    {

    }

    private RectTransform _cRT;

    public RectTransform rectTransform => _cRT ?? (_cRT = GetComponent<RectTransform>());


    private TEXDraw3DRenderer CreateNewRenderer()
    {
        var g = new GameObject("TEXDraw 3D Renderer");
#if TEXDRAW_DEBUG
        g.hideFlags = HideFlags.DontSaveInEditor;
#else
        g.hideFlags = HideFlags.HideAndDontSave;
#endif
        var r = g.AddComponent<RectTransform>();
        r.SetParent(transform, false);
        r.anchorMax = Vector2.one;
        r.anchorMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        r.offsetMin = Vector2.zero;
        var rr = g.AddComponent<TEXDraw3DRenderer>();
        rr.m_TEXDraw = this;
        return rr;
    }


    private void OnDisable()
    {
        Font.textureRebuilt -= OnFontRebuild;
        foreach (var item in m_Renderers)
            if (item)
            {
                item.FontMode = -1;
                item.Repaint();
            }
    }

    private void OnFontRebuild(Font f)
    {
        Redraw();
    }

    public void GenerateParam()
    {
        throw new NotImplementedException();
    }

    #endregion

    //    #region Supplements

    //    [NonSerialized]
    //    private List<TEXDrawSupplementBase> supplements = new List<TEXDrawSupplementBase>();

    //    private bool supplementIsDirty = false;

    //    [NonSerialized]
    //    private List<BaseMeshEffect> postEffects = new List<BaseMeshEffect>();

    //    public void SetSupplementDirty()
    //    {
    //        supplementIsDirty = true;
    //        SetTextDirty();
    //    }

    //    private void UpdateSupplements()
    //    {
    //        GetComponents(supplements);
    //        GetComponents(postEffects);

    //        supplementIsDirty = false;
    //    }

    //    private string PerformSupplements(string original)
    //    {
    //        if (supplements == null)
    //            return original;
    //        for (int i = 0; i < supplements.Count; i++)
    //        {
    //            if (supplements[i] && supplements[i].enabled)
    //                original = supplements[i].ReplaceString(original);
    //        }
    //        return original;
    //    }

    //    private void PerformMeshEffect(Mesh m)
    //    {
    //        if (postEffects.Count == 0)
    //            return;
    //#if UNITY_EDITOR
    //        if (!Application.isPlaying)
    //            GetComponents(postEffects);
    //#endif
    //        for (int i = 0; i < postEffects.Count; i++)
    //        {
    //            if (postEffects[i] && postEffects[i].enabled)
    //                postEffects[i].ModifyMesh(m);
    //        }
    //    }

    //    #endregion
}
