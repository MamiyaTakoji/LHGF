using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 聊天气泡外壳。挂在 TalkContent 上，只负责「与内容类型无关」的四件事：
///   1. 调度量测（向当前内容块问"你需要多大"）；
///   2. 决定气泡尺寸（= 内容尺寸 + padding，再套 minWidth/minHeight/maxWidth/maxHeight 约束）；
///   3. 按 keepEdge 定位（尺寸变化时保持指定的一条边不动）；
///   4. 新内容出现时播放弹出动画。
///
/// 具体内容由 TalkContentBase 的子类实现（TalkTmpContent / TalkTexContent / …），
/// 它们在自己的 OnEnable / OnDisable 里自注册，所以同一时刻只有一个内容块 ——
/// 互斥由 Unity 的激活语义保证，外壳不需要猜，也不再依赖 TMPro / TexDrawLib。
///
/// 换内容用 Show&lt;T&gt;()，它会激活 T 并关掉其它所有内容块。
///
/// 【最小/最大尺寸的口径】四个约束都是"含 padding 的气泡总尺寸"。
/// 最小约束只作用在气泡上，不作用在内容上 —— 内容始终保持自己的自然尺寸并被居中，
/// 所以气泡被撑到最小高度时，文字会在气泡里垂直居中（TMP 本身没有垂直居中能力）。
/// max 是硬约束：min 设得比 max 大时以 max 为准。
///
/// 【重要】排版只在 LateUpdate（以及显式 Refresh）里执行，不在 Awake / OnEnable / OnValidate 里。
/// 原因：TalkContent 是内容渲染器的父物体，Unity 初始化顺序是父先于子；而 TEXDraw 的 orchestrator
/// 要到它自己的 OnEnable 才创建（TEXDraw.cs:171），在它之前调用其布局接口会抛 NullReferenceException
/// （TEXDraw.CheckGraphicsDirty 不判空，TEXDraw.cs:275）。LateUpdate 在所有 OnEnable 之后运行，安全。
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class LHGFTalkContentControler : MonoBehaviour
{
    /// <summary>尺寸变化时保持不动的边 / 角。（枚举顺序不要调整：场景里存的是整数值）</summary>
    public enum KeepEdge
    {
        TopLeft, Top, TopRight,
        Left, Center, Right,
        BottomLeft, Bottom, BottomRight
    }

    [Header("尺寸规则")]
    [Tooltip("框体最大宽度（含左右 padding），内容在这个宽度内换行")]
    public float maxWidth = 500f;
    [Tooltip("框体最小宽度（含左右 padding）。0 = 不限制")]
    public float minWidth = 0f;
    [Tooltip("框体最大高度（含上下 padding）。0 = 不限")]
    public float maxHeight = 0f;
    [Tooltip("框体最小高度（含上下 padding）。0 = 不限制")]
    public float minHeight = 0f;
    [Tooltip("内边距：x = 左, y = 右, z = 上, w = 下")]
    public Vector4 padding = new Vector4(12f, 12f, 10f, 10f);

    [Header("位置规则")]
    [Tooltip("尺寸变化时保持不动的边。默认左上角固定，框体向右下方生长")]
    public KeepEdge keepEdge = KeepEdge.TopLeft;

    [Header("弹出动画")]
    [Tooltip("调用 Show<T>() 换内容时自动播放弹出动画")]
    public bool autoPlayShowAnimation = true;
    [Tooltip("弹出动画时长（秒）。UnityChatWindow 里用的是 0.3")]
    public float showDuration = 0.3f;

    // ---------------- 内部状态 ----------------
    private RectTransform rect;
    private CanvasGroup canvasGroup;

    private TalkContentBase current;        // 由注册得到，不再靠猜
    private Vector2 lastContentSize;        // 上次落下去的内容尺寸
    private Vector2 lastBoxSize;            // 上次落下去的气泡尺寸
    private bool hasApplied;

    /// <summary>当前激活的内容块（没有则为 null）。列表用它查这一行加载完没有。</summary>
    public TalkContentBase CurrentContent => current;

    private void Awake()
    {
        Cache();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        // 只置位，不在这里排版：本物体的 OnEnable 会早于子物体渲染器的 OnEnable。
        hasApplied = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Cache();
        // 同样不在这里排版：脚本重载时 OnValidate 可能早于所有 OnEnable 执行。
        hasApplied = false;
    }
#endif

    private void OnDestroy()
    {
        // 与 CustomTextBox 一致：对象销毁时掐断身上的所有动画
        DOTween.Kill(gameObject);
    }

    // ======================= 内容注册（互斥的唯一来源）=======================

    /// <summary>由 TalkContentBase 在 OnEnable 里调用。</summary>
    public void Register(TalkContentBase content)
    {
        if (content == null) return;

        if (current != null && current != content)
            Debug.LogWarning(
                $"[{name}] 同一时刻注册了两个内容块：{current.name} / {content.name}，已改用后者。" +
                "请用 Show<T>() 或 GameObject.SetActive 保证互斥（组件级 enabled 不会触发 OnDisable）。", this);

        current = content;
        hasApplied = false;
    }

    /// <summary>由 TalkContentBase 在 OnDisable 里调用。</summary>
    public void Unregister(TalkContentBase content)
    {
        if (current == content)
        {
            current = null;
            hasApplied = false;
        }
    }

    /// <summary>内容自己变了（换文本 / 图片异步加载完成）。</summary>
    public void NotifyContentChanged()
    {
        hasApplied = false;
    }

    /// <summary>
    /// 换内容类型的唯一入口：激活 T，关掉其它所有内容块，并（可选）播放弹出动画。
    /// </summary>
    public T Show<T>() where T : TalkContentBase
    {
        T picked = null;

        foreach (var content in GetComponentsInChildren<TalkContentBase>(true))
        {
            bool on = content is T;
            content.gameObject.SetActive(on);      // ← 必须 GameObject 级：组件级 enabled 不触发 OnDisable
            if (on) picked = (T)content;
        }

        if (picked == null)
        {
            Debug.LogWarning($"[{name}] 找不到类型为 {typeof(T).Name} 的内容块。", this);
            return null;
        }

        current = picked;                          // 目标本来就是激活状态时不会触发 Register，这里兜底
        hasApplied = false;

        if (autoPlayShowAnimation) PlayShowAnimation();

        return picked;
    }

    /// <summary>
    /// 按数据里的 kind 激活对应的内容组件，并把数据交给它自己解释。
    /// 这是 TalkBar 与外壳之间唯一的数据入口：外壳不认识具体的内容类型，
    /// 靠各内容组件声明的 Kind 分发，所以新增内容类型时这里一行都不用改。
    ///
    /// 只负责"分发 + 写数据"，不播动画 —— 动画时机由 TalkBar 决定
    /// （手动调用仍可用 Show&lt;T&gt;()，那个会自动播）。
    /// </summary>
    public TalkContentBase Show(TalkContentData data)
    {
        if (data == null) return null;

        TalkContentBase picked = null;

        foreach (var content in GetComponentsInChildren<TalkContentBase>(true))
        {
            bool on = content.Kind == data.kind;
            content.gameObject.SetActive(on);      // ← 必须 GameObject 级：组件级 enabled 不触发 OnDisable
            if (!on) continue;

            if (picked == null) picked = content;
            else Debug.LogWarning($"[{name}] Kind = {data.kind} 的内容组件有多个：{picked.name} / {content.name}", this);
        }

        if (picked == null)
        {
            Debug.LogWarning($"[{name}] 没有 Kind = {data.kind} 的内容组件。", this);
            return null;
        }

        current = picked;                          // 目标本来就是激活状态时不会触发 Register，这里兜底
        hasApplied = false;

        picked.ApplyData(data);                    // ← 数据怎么解释，交给内容组件自己

        return picked;
    }

    /// <summary>立即重排一次（外部改完内容后可以调用；不调用也会被 LateUpdate 捕捉）。</summary>
    public void Refresh() => Rebuild();

    // ============================== 排版 ==============================

    private void LateUpdate() => Rebuild();

    private void Rebuild()
    {
        if (current == null) return;

        float l = padding.x, r = padding.y, t = padding.z, b = padding.w;
        float innerW = Mathf.Max(1f, maxWidth - l - r);
        float innerH = maxHeight > 0.01f ? Mathf.Max(1f, maxHeight - t - b) : 100000f;
        Vector2 available = new Vector2(innerW, innerH);

        // 1) 内容只报"自然尺寸"。
        //    这里刻意不套最小尺寸 —— 否则内容 rect 被撑高，TMP 的文字会贴在 rect 顶部，
        //    而不是在更高的气泡里居中。
        Vector2 contentSize = current.Measure(available);

        // 2) 气泡尺寸 = 内容 + padding，再套最小 / 最大约束。
        //    先 Max 再 Min，保证 min 比 max 大时以 max 为准（Mathf.Clamp 在 min > max 时行为相反，所以不用它）。
        Vector2 boxSize = new Vector2(contentSize.x + l + r, contentSize.y + t + b);
        boxSize.x = Mathf.Min(Mathf.Max(boxSize.x, minWidth), maxWidth);
        boxSize.y = Mathf.Max(boxSize.y, minHeight);
        if (maxHeight > 0.01f) boxSize.y = Mathf.Min(boxSize.y, maxHeight);

        // 两个都要比：气泡尺寸没变、但内容变矮时，内容 rect 仍要收缩并重新居中
        if (hasApplied && boxSize == lastBoxSize && contentSize == lastContentSize) return;

        // 3) 内容落自己的自然尺寸，并在内容区里居中 —— 气泡被撑高时文字自然垂直居中
        RectTransform contentRect = current.Rect;
        NormalizeContentRect(contentRect);
        contentRect.anchoredPosition = new Vector2((l - r) * 0.5f, (t - b) * 0.5f);

        current.ApplySize(contentSize);

        ApplySize(boxSize);

        lastBoxSize = boxSize;
        lastContentSize = contentSize;
        hasApplied = true;

        // 4) 尺寸真的变了 → 立刻让"气泡之上的布局"按新尺寸重算一次。
        //    行的 LayoutGroup / ContentSizeFitter、列表的 VerticalLayoutGroup 本来要等本帧稍后的
        //    布局重建阶段才排；异步加载（GIF 解码完成、图片下载完成）这种情况下，就会表现为
        //    "内容都加载完了，但排版还是旧的"。这里同步重建，保证当帧就对上。
        //
        //    放在这里而不是放在内容的加载回调里，是因为那一刻外壳还没写出新尺寸（外壳在 LateUpdate 写），
        //    提前刷新只会按旧尺寸白算一遍。
        ForceRebuildParentLayout();
    }

    /// <summary>
    /// 从父级往上找最外层带 LayoutGroup 的节点，对它做一次同步布局重建 ——
    /// 行高、行在列表里的位置会在这一帧内全部按新尺寸算好。
    /// 上面没有布局组（纯手摆）时什么都不做。
    /// </summary>
    private void ForceRebuildParentLayout()
    {
        RectTransform top = null;
        for (var t = transform.parent as RectTransform; t != null; t = t.parent as RectTransform)
            if (t.GetComponent<LayoutGroup>() != null) top = t;

        if (top != null) LayoutRebuilder.ForceRebuildLayoutImmediate(top);
    }

    /// <summary>改气泡尺寸，并让 keepEdge 指定的那条边保持不动（公式由 pivot 推出，与 pivot 取值无关）。</summary>
    private void ApplySize(Vector2 newSize)
    {
        Vector2 oldSize = rect.sizeDelta;
        Vector2 pos = rect.anchoredPosition;
        Vector2 p = rect.pivot;

        rect.sizeDelta = newSize;

        float dw = newSize.x - oldSize.x;
        float dh = newSize.y - oldSize.y;
        if (Mathf.Approximately(dw, 0f) && Mathf.Approximately(dh, 0f)) return;

        switch (keepEdge)
        {
            case KeepEdge.TopLeft:
                pos.x += p.x * dw;
                pos.y -= (1f - p.y) * dh;
                break;
            case KeepEdge.Top:
                pos.y -= (1f - p.y) * dh;
                break;
            case KeepEdge.TopRight:
                pos.x -= (1f - p.x) * dw;
                pos.y -= (1f - p.y) * dh;
                break;
            case KeepEdge.Left:
                pos.x += p.x * dw;
                break;
            case KeepEdge.Right:
                pos.x -= (1f - p.x) * dw;
                break;
            case KeepEdge.Center:
                pos.x += (p.x - 0.5f) * dw;
                pos.y += (p.y - 0.5f) * dh;
                break;
            case KeepEdge.BottomLeft:
                pos.x += p.x * dw;
                pos.y += p.y * dh;
                break;
            case KeepEdge.Bottom:
                pos.y += p.y * dh;
                break;
            case KeepEdge.BottomRight:
                pos.x -= (1f - p.x) * dw;
                pos.y += p.y * dh;
                break;
        }

        rect.anchoredPosition = pos;
    }

    // ============================== 动画 ==============================

    /// <summary>
    /// 弹出动画是否还在播。列表/层靠它决定"这一行完成没有"。
    ///
    /// 直接把这件事交给 DOTween 查，不自己置标志位：
    /// 动画的 tween 挂在 canvasGroup / rect 上，另外还有 DOTween.Kill（连续换行时掐断旧动画，
    /// 此时旧动画的 OnComplete 不会触发）和失活暂停两种情况 —— 自己维护标志位很容易漏掉其中之一，
    /// 一旦漏掉就是 Finish() 永远返回 false、游戏卡死。
    /// </summary>
    public bool IsPlayingShowAnimation
    {
        get
        {
            if (!Application.isPlaying) return false;
            if (canvasGroup != null && DOTween.IsTweening(canvasGroup)) return true;
            if (rect != null && DOTween.IsTweening(rect)) return true;
            return false;
        }
    }

    /// <summary>
    /// 让正在播的弹出动画立刻结束到最终状态（对应 Skip()）。
    /// 只影响动画，不影响任何异步加载 —— 动图该等还是要等。
    /// </summary>
    public void CompleteShowAnimation()
    {
        if (rect == null) rect = (RectTransform)transform;
        EnsureCanvasGroup();

        DOTween.Kill(gameObject);
        DOTween.Kill(canvasGroup);
        DOTween.Kill(rect);

        canvasGroup.alpha = 1f;
        rect.localScale = Vector3.one;
    }

    /// <summary>
    /// 把气泡整个藏起来（alpha 0），但**不改变激活状态、也不碰排版**。
    /// 用来等"对方正在输入"那段：气泡其实已经建好了，只是先不让它露脸。
    /// 这样不会触发 OnDisable，头像/图片不会因为藏一下就被释放重载。
    /// </summary>
    public void HideBubbleVisual()
    {
        if (rect == null) rect = (RectTransform)transform;
        EnsureCanvasGroup();

        DOTween.Kill(gameObject);
        DOTween.Kill(canvasGroup);
        DOTween.Kill(rect);

        canvasGroup.alpha = 0f;
        // scale 归零：显形时从 0 弹出来才有"弹出"感。
        // （摆成 1 的话 Reveal 就只剩缩放从 1 到 1，看着什么动画都没有）
        rect.localScale = Vector3.zero;
    }

    /// <summary>
    /// 把之前用 HideBubbleVisual 藏起来的气泡显形。
    ///
    /// 和 PlayShowAnimation 的区别：**alpha 直接跳到 1，只做缩放弹出**。
    /// 因为"对方正在输入"那一等已经把气泡藏了 N 秒，玩家早就在盯着顶部状态了，
    /// 再叠一层 0.3 秒淡入会显得拖沓、像"慢慢变出来"而不是"弹出来"。
    /// </summary>
    public void RevealBubble(bool withAnimation)
    {
        if (rect == null) rect = (RectTransform)transform;
        EnsureCanvasGroup();

        DOTween.Kill(gameObject);
        DOTween.Kill(canvasGroup);
        DOTween.Kill(rect);

        canvasGroup.alpha = 1f;      // ★ 瞬间可见

        if (withAnimation && Application.isPlaying)
        {
            // 只弹缩放：从 0 弹到 1
            rect.localScale = Vector3.zero;
            rect.DOScale(Vector3.one, showDuration).SetEase(Ease.OutBack);
        }
        else
        {
            rect.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 弹出动画：淡入 + 从 0 缩放弹到 1（Ease.OutBack）。
    /// 对应 UnityChatWindow 里 CustomTextBox.PlayShowAnimation()。
    /// 要点：scale / alpha 都不参与布局，所以和排版互不干扰；
    /// 这里先把起始状态写好，本帧渲染就是不可见的，下一帧 DOTween 才开始推进。
    /// </summary>
    [ContextMenu("Play Show Animation")]
    public void PlayShowAnimation()
    {
        if (rect == null) rect = (RectTransform)transform;
        EnsureCanvasGroup();

        // 编辑器（非播放）下 DOTween 不会推进，直接摆成最终状态，
        // 否则对象会被留在 scale 0 / alpha 0，在 Scene 视图里直接看不见。
        if (!Application.isPlaying)
        {
            canvasGroup.alpha = 1f;
            rect.localScale = Vector3.one;
            return;
        }

        // 防止同一行连续换内容时动画叠加。Sequence 的 target 是 canvasGroup（见下面 SetTarget），
        // 按 gameObject 的 id 已经杀不到它了 —— 必须按 target 杀。
        DOTween.Kill(canvasGroup);
        DOTween.Kill(rect);

        canvasGroup.alpha = 0f;     // 先摆好起始状态：本帧渲染就是不可见的，不会闪一下原尺寸
        rect.localScale = Vector3.zero;

        /*        DOTween.Sequence()
                    .Append(canvasGroup.DOFade(1f, showDuration))
                    .Join(rect.DOScale(Vector3.one, showDuration).SetEase(Ease.OutBack));*/
        var seq = DOTween.Sequence();
        seq.SetTarget(canvasGroup);      // ← 让 Kill(canvasGroup) 杀得到
        seq.Append(canvasGroup.DOFade(1f, showDuration))
           .Join(rect.DOScale(Vector3.one, showDuration).SetEase(Ease.OutBack));
    }

    // ============================== 杂项 ==============================

    private void Cache()
    {
        rect = (RectTransform)transform;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    /// <summary>内容块的 anchor / pivot 归一到中心，ApplySize 里的 sizeDelta 才等于实际尺寸。</summary>
    private static void NormalizeContentRect(RectTransform rt)
    {
        Vector2 center = new Vector2(0.5f, 0.5f);
        if (rt.anchorMin != center) rt.anchorMin = center;
        if (rt.anchorMax != center) rt.anchorMax = center;
        if (rt.pivot != center) rt.pivot = center;
    }
}
