using TMPro;
using UnityEngine;

/// <summary>
/// TMP 文本内容块。挂在和 TextMeshProUGUI 同一个 GameObject 上。
///
/// 量测用 GetPreferredValues(文本, 最大宽度, 0)：按给定宽度换行并同步算出高度，
/// 不依赖上一帧的网格缓存，所以不会"慢一帧"，也不需要 ForceRefreshSize 那套东西。
///
/// 渲染宽度必须等于量测宽度：fillBubbleWidth 打开时返回 available.x，
/// 这样 TMP 折行用的宽度和实际渲染宽度严格一致，不会因为浮点误差在渲染时多折一行。
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TalkTmpContent : TalkContentBase
{
    [Header("宽度策略")]
    [Tooltip("勾选：文本块撑满气泡可用宽度（气泡宽度恒定，只有高度随文字变）——聊天框常用\n" +
             "取消：文本块收紧到最长一行的宽度（气泡跟着文字变宽）")]
    public bool fillBubbleWidth = true;

    private TextMeshProUGUI label;
    private string lastSeenText;

    public TextMeshProUGUI Label => label != null ? label : (label = GetComponent<TextMeshProUGUI>());

    /// <summary>推荐入口：写文本会立刻让量测缓存失效。</summary>
    public string Text
    {
        get => Label.text;
        set
        {
            if (Label.text == value) return;
            Label.text = value;
            MarkDirty();
        }
    }

    /// <summary>等价于 Text 属性，方便挂给 UnityEvent / Lua。</summary>
    public void SetText(string value) => Text = value;

    protected override void Awake()
    {
        base.Awake();
        label = GetComponent<TextMeshProUGUI>();
        lastSeenText = label.text;
    }

    protected override Vector2 MeasureCore(Vector2 available)
    {
        Vector2 pv = Label.GetPreferredValues(Label.text, available.x, 0f);

        float width = fillBubbleWidth ? available.x : Mathf.Min(pv.x, available.x);
        return new Vector2(width, pv.y);
    }

    // ==================== 内容类型 + 数据处理 ====================

    /// <summary>本组件负责普通文本。</summary>
    public override TalkContentKind Kind => TalkContentKind.TmpText;

    /// <summary>从数据里取 text 作为正文。</summary>
    public override void ApplyData(TalkContentData data)
        => Text = data.GetString(TalkContentData.KeyText);

    private void LateUpdate()
    {
        // 兜底：捕获任何绕过 Text 属性的写入（TextTweener / 本地化组件 / Lua 直接写 label.text）。
        // 代价是每帧一次字符串引用比较，可以忽略。
        if (Label.text != lastSeenText)
        {
            lastSeenText = Label.text;
            MarkDirty();
        }
    }
}
