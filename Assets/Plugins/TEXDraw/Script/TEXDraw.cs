using System;
using System.Collections;
using System.Collections.Generic;
using TexDrawLib;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("TEXDraw/TEXDraw UI", 1), ExecuteAlways]
[RequireComponent(typeof(RectTransform)), RequireComponent(typeof(CanvasRenderer))]
[HelpURL("https://willnode.gitlab.io/texdraw/")]
#if TEXDRAW_DEBUG
[SelectionBase]
#endif
public class TEXDraw : MaskableGraphic, ITEXDraw, ILayoutElement, ILayoutSelfController
{
    public TEXPreference preference => TEXPreference.main;

    [NonSerialized]
    private List<TEXDrawRenderer> m_Renderers = new List<TEXDrawRenderer>();

    [NonSerialized]
    private bool m_TextDirty = true;

    [NonSerialized]
    private bool m_BoxDirty = true;

    [NonSerialized]
    private bool m_xLayoutDirty = true;

    [NonSerialized]
    private bool m_yLayoutDirty = true;

    [NonSerialized]
    private bool m_OutputDirty = true;

    [SerializeField]
    private Vector2 m_Alignment = Vector2.one * 0.5f;

    [SerializeField, TextArea(5, 10)]
    private string m_Text = "$$TEXDraw$$";

    [SerializeField]
    private float m_Size = 36f;

    [SerializeField]
    private Rect m_ScrollArea = new Rect();

    [SerializeField]
    private TexRectOffset m_Padding = new TexRectOffset(2, 2, 2, 2);

    [SerializeField]
    private Overflow m_Overflow = Overflow.Hidden;

    public UnityEngine.Events.UnityAction<DirtyLevel> hook;

    public virtual string text
    {
        get => m_Text;
        set
        {
            if (m_Text != value)
            {
                m_Text = value;
                SetTextDirty();
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
            SetVerticesDirty();
        }
    }

    public virtual Rect scrollArea
    {
        get => m_ScrollArea;
        set
        {
            if (m_ScrollArea != value)
            {
                if (m_ScrollArea.size != value.size)
                    m_BoxDirty = true;
                else
                {
                    // need to inject manually cause scroll position is not injected on render
                    var rect = new Rect(Vector2.zero, rectTransform.rect.size);
                    rect.center = Vector2.zero;
                    orchestrator.InputCanvasSize(rectTransform.rect, value, m_Padding, new Vector2Int(), m_Overflow);
                }
                m_ScrollArea = value;
                m_OutputDirty = true;
                SetVerticesDirty();
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

    public override Color color
    {
        get => base.color;
        set
        {
            if (base.color != value)
            {
                base.color = value;
                SetTextDirty();
            }
        }
    }
    public Overflow overflow
    {
        get => this.m_Overflow;
        set
        {
            if (this.m_Overflow != value)
            {
                this.m_Overflow = value;
                SetVerticesDirty();
            }
        }
    }

    private static void FixCanvas(Canvas c)
    {
        if (c)
            c.additionalShaderChannels
            |= AdditionalCanvasShaderChannels.TexCoord1
            | AdditionalCanvasShaderChannels.TexCoord2;
    }

    protected override void OnEnable()
    {
        if (!preference)
        {
            TEXPreference.Initialize();
        }
        base.OnEnable();
        orchestrator = new TexOrchestrator();
        GetComponentsInChildren(true, m_Renderers);
        FixCanvas(canvas);
        Font.textureRebuilt += TextureRebuilted;
        m_OutputDirty = m_BoxDirty = m_TextDirty = true;
        StartDelayDirtyCallback();
    }

    protected override void OnDisable()
    {
        Font.textureRebuilt -= TextureRebuilted;
        foreach (var item in m_Renderers)
            if (item)
            {
                item.FontMode = -1;
                item.ForceRender();
            }
        m_Renderers.Clear();
        base.OnDisable();
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        FixCanvas(canvas);
    }

    private void TextureRebuilted(Font obj)
    {
        StartDelayDirtyCallback();
    }

    public void RegisterRenderer(TEXDrawRenderer renderer)
    {
        for (int i = 0; i < m_Renderers.Count; i++)
        {
            if (m_Renderers[i] == null)
            {
                m_Renderers[i] = renderer;
                return;
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

    protected override void OnValidate()
    {
        base.OnValidate();
    }

#endif

    #region Engine

    private TexOrchestrator m_orchestrator;

    public TexOrchestrator orchestrator { get => m_orchestrator; private set => m_orchestrator = value; }

    public void SetTextDirty()
    {
        m_OutputDirty = true;
        m_BoxDirty = true;
        m_TextDirty = true;

        SetLayoutDirty();
        SetVerticesDirty();
        SetMaterialDirty();
    }

    Vector2 _lastSavedPreferredSize;
    Vector2 _lastSavedAtSize;

    Rect _internalRect
    {
        get
        {
            var sz = rectTransform.rect.size;
            return new Rect(sz * -0.5f, sz);
        }
    }

    internal void CheckGraphicsDirty(bool render = true)
    {
#if UNITY_EDITOR
        if (preference.editorReloading)
            return;
#endif
        if (!isActiveAndEnabled)
            return;

        if (_internalRect != orchestrator.drawingRect)
        {
            m_BoxDirty = true;
            m_yLayoutDirty = true;
        }

        int dirtylevel = 0;
        // Three main stages of rendering: Parse, Box, Render.
        if (m_TextDirty)
        {
#if UNITY_EDITOR && TEXDRAW_DEBUG
            UnityEngine.Profiling.Profiler.BeginSample("TexParsing");
#endif
            orchestrator.initialColor = color;
            orchestrator.initialSize = size;
            orchestrator.pixelsPerInch = TEXConfiguration.main.Document.pixelsPerInch;
            orchestrator.alignment = m_Alignment;
            orchestrator.Parse(m_Text);
            m_TextDirty = false;
            m_BoxDirty = true;
            m_xLayoutDirty = true;
            m_yLayoutDirty = true;

            // calculate preferred size for layout
            dirtylevel = Math.Max(dirtylevel, 3);

#if UNITY_EDITOR && TEXDRAW_DEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }
        if (m_BoxDirty && render)
        {
#if UNITY_EDITOR && TEXDRAW_DEBUG
            UnityEngine.Profiling.Profiler.BeginSample("TexBoxing");
#endif

            orchestrator.InputCanvasSize(_internalRect, m_ScrollArea, m_Padding, new Vector2Int(), m_Overflow);
            orchestrator.Box();
            m_OutputDirty = true;
            m_BoxDirty = false;
            dirtylevel = Math.Max(dirtylevel, 2);

#if UNITY_EDITOR && TEXDRAW_DEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif       
        }
        if (m_OutputDirty && render)
        {
#if UNITY_EDITOR && TEXDRAW_DEBUG
            UnityEngine.Profiling.Profiler.BeginSample("TexRender");
#endif

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
                {
                    m_Renderers.Add(null);
                    CreateNewRenderer();
                }
                else if (m_Renderers[i])
                {
                    var item = m_Renderers[i];
                    item.FontMode = vertexes[i].m_Font;
                    item.ForceRender();
                }
            }
            m_OutputDirty = false;
            dirtylevel = Math.Max(dirtylevel, 1);

#if UNITY_EDITOR && TEXDRAW_DEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }
        if (dirtylevel > 0 && hook != null)
            StartDelayHookDirtyCallback((DirtyLevel)dirtylevel);
    }

    private void CreateNewRenderer()
    {
        var g = new GameObject("TEXDraw Renderer");
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
        r.pivot = Vector2.one * 0.5f;
        g.AddComponent<TEXDrawRendererFactory>();
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.EditorApplication.delayCall += () =>
            {
                UnityEditor.SceneView.RepaintAll();
            };
        }
#endif
        StartDelayDirtyCallback();
    }

    public void StartDelayDirtyCallback(bool dirtytext = true)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this)
                {
                    if (dirtytext)
                        SetTextDirty();
                    else
                    {
                        SetMaterialDirty();
                        SetLayoutDirty();
                    }
                    UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                }
            };
        }
        else
#endif
            StartCoroutine(DelayDirtyCallback(dirtytext));
    }
    protected void StartDelayHookDirtyCallback(DirtyLevel dirtyLevel)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this)
                {
                    hook?.Invoke(dirtyLevel);
                    UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                }
            };
        }
        else
#endif
            StartCoroutine(DelayHookDirtyCallback(dirtyLevel));
    }

    private IEnumerator DelayDirtyCallback(bool dirtytext = true)
    {
        yield return null;
        if (dirtytext)
            SetTextDirty();
        else
        {
            SetLayoutDirty();
            SetMaterialDirty();
        }
    }

    private IEnumerator DelayHookDirtyCallback(DirtyLevel dirtyLevel)
    {
        yield return null;
        hook?.Invoke(dirtyLevel);
    }

    public override void SetLayoutDirty()
    {
        m_BoxDirty = true;
        m_OutputDirty = true;
        base.SetLayoutDirty();
    }

    public override Material defaultMaterial => preference.defaultMaterial;


    #endregion
    // these functions are called in order by uGUI

    #region Layout

    public virtual void CalculateLayoutInputHorizontal()
    {
        CheckGraphicsDirty(false);
        if (m_xLayoutDirty)
        {
#if UNITY_EDITOR && TEXDRAW_DEBUG
            UnityEngine.Profiling.Profiler.BeginSample("TexLayoutX");
#endif
            orchestrator.InputCanvasSize(_internalRect, m_ScrollArea, m_Padding, Vector2Int.one, m_Overflow);
            orchestrator.Box();
            _lastSavedPreferredSize.x = orchestrator.outputNativeCanvasSize.x;
            _lastSavedAtSize.x = orchestrator.outputNativeCanvasSize.y;
            m_xLayoutDirty = false;
#if UNITY_EDITOR && TEXDRAW_DEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }
    }

    public virtual void SetLayoutHorizontal() { }

    public virtual void CalculateLayoutInputVertical()
    {
        if (m_yLayoutDirty)
        {

#if UNITY_EDITOR && TEXDRAW_DEBUG
            UnityEngine.Profiling.Profiler.BeginSample("TexLayoutY");
#endif

            orchestrator.InputCanvasSize(_internalRect, m_ScrollArea, m_Padding, Vector2Int.up, m_Overflow);
            orchestrator.Box();
            _lastSavedPreferredSize.y = orchestrator.outputNativeCanvasSize.y;
            _lastSavedAtSize.y = orchestrator.outputNativeCanvasSize.x;
            m_yLayoutDirty = false;
#if UNITY_EDITOR && TEXDRAW_DEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }
    }

    public virtual void SetLayoutVertical() { }

    protected override void UpdateGeometry()
    {
        CheckGraphicsDirty();
    }


    public virtual float minWidth => 0;

    public virtual float preferredWidth => _lastSavedPreferredSize.x * (m_Overflow >= Overflow.DownScale ? Math.Min(1, rectTransform.rect.height / _lastSavedAtSize.x) : 1);

    public virtual float flexibleWidth => -1;

    public virtual float minHeight => 0;

    public virtual float preferredHeight => _lastSavedPreferredSize.y * (m_Overflow >= Overflow.DownScale ? Math.Min(1, rectTransform.rect.width / _lastSavedAtSize.y) : 1);

    public virtual float flexibleHeight => -1;

    public virtual int layoutPriority => -1;

    #endregion

}
