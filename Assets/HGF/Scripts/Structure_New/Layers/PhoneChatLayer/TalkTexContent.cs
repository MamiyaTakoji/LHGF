using TexDrawLib;
using UnityEngine;

/// <summary>
/// TEXDraw 公式内容块。挂在和 TEXDraw 同一个 GameObject 上。
///
/// 量测方式：TEXDraw 实现了 ILayoutElement，调用它的布局接口后再读 preferredWidth / preferredHeight。
/// 必须先 CalculateLayoutInputHorizontal 再 Vertical —— 只有 Horizontal 内部的 CheckGraphicsDirty
/// 才会把 m_xLayoutDirty / m_yLayoutDirty 置位，Vertical 才能拿到新的高度。
///
/// 初始化保护：TEXDraw 的 orchestrator 只在它自己的 OnEnable 里创建（TEXDraw.cs:171），
/// 而 CheckGraphicsDirty 直接解引用它、不判空（TEXDraw.cs:275）。
/// 所以这里在 orchestrator 为 null 时返回零尺寸，并由 LateUpdate 轮询到它就绪后再重算。
/// </summary>
[RequireComponent(typeof(TEXDraw))]
public class TalkTexContent : TalkContentBase
{
    [Header("宽度策略")]
    [Tooltip("公式是否撑满可用宽度。公式一般保持自身宽度居中即可，所以默认关闭。")]
    public bool fillBubbleWidth = false;

    [Header("调试")]
    [Tooltip("公式宽于可用宽度时打印一次警告")]
    public bool warnOnOverflow = true;

    private TEXDraw tex;
    private string lastSeenText;
    private float lastSeenSize = -1f;
    private bool warnedOverflow;
    private bool ready;

    public TEXDraw Tex => tex != null ? tex : (tex = GetComponent<TEXDraw>());

    public string Text
    {
        get => Tex.text;
        set
        {
            if (Tex.text == value) return;
            Tex.text = value;
            MarkDirty();
        }
    }

    public float Size
    {
        get => Tex.size;
        set
        {
            if (Mathf.Approximately(Tex.size, value)) return;
            Tex.size = value;
            MarkDirty();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        tex = GetComponent<TEXDraw>();
        lastSeenText = tex.text;
        lastSeenSize = tex.size;
    }

    protected override Vector2 MeasureCore(Vector2 available)
    {
        if (Tex.orchestrator == null) return Vector2.zero;   // 还没初始化好，等 LateUpdate 轮询

        Tex.CalculateLayoutInputHorizontal();
        Tex.CalculateLayoutInputVertical();

        float nativeW = Tex.preferredWidth;
        float nativeH = Tex.preferredHeight;

        if (nativeW > available.x + 0.5f && warnOnOverflow && !warnedOverflow)
        {
            warnedOverflow = true;
            Debug.LogWarning(
                $"[{nameof(TalkTexContent)}] TEXDraw 内容宽度 {nativeW:F1} 超过可用宽度 {available.x:F1}。" +
                "Overflow = Hidden 时超出部分会被裁掉，把 TEXDraw 的 Overflow 改成 DownScale 可让它自动缩小。", this);
        }

        float width = fillBubbleWidth ? available.x : Mathf.Min(nativeW, available.x);
        return new Vector2(width, nativeH);
    }

    // ==================== 内容类型 + 数据处理 ====================

    /// <summary>本组件负责公式。</summary>
    public override TalkContentKind Kind => TalkContentKind.TexDraw;

    /// <summary>从数据里取 tex（公式）与 size（可选字号）。</summary>
    public override void ApplyData(TalkContentData data)
    {
        Text = data.GetString(TalkContentData.KeyTex);

        float size = data.GetFloat(TalkContentData.KeySize, 0f);
        if (size > 0f) Size = size;
    }

    private void LateUpdate()
    {
        // TEXDraw 就绪状态变化 → 补一次重算（启动阶段外壳可能先量到过零尺寸）
        bool nowReady = Tex.orchestrator != null;
        if (nowReady != ready)
        {
            ready = nowReady;
            MarkDirty();
        }

        // 兜底：捕获绕过 Text / Size 属性的写入
        if (Tex.text != lastSeenText || !Mathf.Approximately(Tex.size, lastSeenSize))
        {
            lastSeenText = Tex.text;
            lastSeenSize = Tex.size;
            MarkDirty();
        }
    }
}
