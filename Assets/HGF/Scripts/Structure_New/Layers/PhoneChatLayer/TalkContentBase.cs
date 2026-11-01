using UnityEngine;

/// <summary>
/// 聊天气泡里「内容块」的基类。挂在和内容渲染器同一个 GameObject 上。
///
/// 分工：
///   · 基类负责所有内容类型共有的部分 —— 自注册（互斥）、脏标记、量测缓存、尺寸钳制、通用落尺寸；
///   · 子类声明自己负责哪种 TalkContentKind（外壳按它分发），
///     实现 MeasureCore()（在给定可用尺寸内我原始需要多大），
///     并按需重写 ApplyData()（把这一行的数据解释成自己的显示内容）。
///
/// 互斥：内容在 OnEnable 时向父级 LHGFTalkContentControler 注册，OnDisable 时注销。
/// 同一时刻只会有一个内容处于激活状态，所以"用哪个"由 Unity 的激活语义保证，不需要外壳去猜。
///
/// 注意：互斥必须用 GameObject 的 SetActive 控制，不要用组件 enabled ——
/// 组件级禁用不会触发 OnDisable，内容会照样注册，外壳就会去量测一个不渲染的渲染器。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public abstract class TalkContentBase : MonoBehaviour
{
    /// <summary>父级气泡外壳。为 null 说明这个内容没挂在 TalkContent 下，不会参与排版。</summary>
    public LHGFTalkContentControler Bubble { get; private set; }

    /// <summary>自己的 RectTransform。外壳读它来摆位置，ApplySize 也写它。</summary>
    public RectTransform Rect => (RectTransform)transform;

    // ---------------- 所有类型共用的状态 ----------------
    private bool dirty = true;              // true 表示必须重算
    private Vector2 lastAvailable;          // 上次量测用的可用尺寸
    private Vector2 lastSize;               // 上次量测的结果

    protected virtual void Awake()
    {
        Bubble = GetComponentInParent<LHGFTalkContentControler>();
        if (Bubble == null)
            Debug.LogWarning($"[{GetType().Name}] 父级找不到 LHGFTalkContentControler，这个内容块不会参与排版。", this);
    }

    protected virtual void OnEnable()
    {
        dirty = true;
        Bubble?.Register(this);
    }

    protected virtual void OnDisable()
    {
        Bubble?.Unregister(this);
    }

    /// <summary>内容变了就喊一声（换文本、换 sprite、异步加载完成…）。</summary>
    public void MarkDirty()
    {
        dirty = true;
        Bubble?.NotifyContentChanged();
    }

    // ==================== 内容类型 + 数据处理（子类实现）====================

    /// <summary>
    /// 这个内容组件负责哪种 TalkContentKind。外壳的 Show(data) 按它分发 ——
    /// 所以新增内容类型时只需要新建一个声明了 Kind 的组件，外壳和 TalkBar 都不用改。
    /// </summary>
    public abstract TalkContentKind Kind { get; }

    /// <summary>
    /// 把这一行的数据解释成自己的显示内容。子类按自己的 key 约定取值
    /// （key 常量定义在 TalkContentData 上）。不需要数据的类型可以不重写。
    /// </summary>
    public virtual void ApplyData(TalkContentData data) { }

    // ==================== 资源路径 ====================

    /// <summary>
    /// 把资源路径解析成 UnityWebRequest 能直接取用的 URL。
    /// 实现已移到 <see cref="RemoteImageLoader.ResolveStreamingAssets"/>（图片 / 图标 / 头像三处共用），
    /// 这里保留一个转发入口，避免各内容块到处写 Loader 的类名。
    /// </summary>
    protected static string ResolveStreamingAssets(string nameOrPath)
        => RemoteImageLoader.ResolveStreamingAssets(nameOrPath);

    // ==================== 加载状态 ====================

    /// <summary>
    /// 这个内容块是否正在异步加载（图片 / 动图 / 文件图标）。
    /// 默认 false；需要等待加载的类型（如 TalkGifContent）重写它，
    /// 供列表或外壳决定"这一行算不算完成"。
    /// </summary>
    public virtual bool IsLoading => false;

    // ==================== 量测（子类实现）====================

    /// <summary>
    /// 在 available 内我需要多大（不含气泡 padding）。
    /// 带缓存：内容和可用尺寸都没变时直接返回上次结果，所以外壳可以每帧无脑调用。
    /// </summary>
    public Vector2 Measure(Vector2 available)
    {
        available = Vector2.Max(available, Vector2.one);                    // 防 0 / 负
        if (!dirty && available == lastAvailable) return lastSize;          // 缓存命中

        Vector2 size = MeasureCore(available);                              // ← 子类算法

        if (float.IsNaN(size.x) || float.IsNaN(size.y)) size = Vector2.zero;
        size = Vector2.Min(Vector2.Max(size, Vector2.zero), available);     // 统一钳制

        dirty = false;
        lastAvailable = available;
        lastSize = size;
        return size;
    }

    /// <summary>
    /// 落尺寸。前提：外壳已把我的 anchor/pivot 归一到中心，所以 sizeDelta 就等于实际尺寸。
    /// 绝大多数类型不需要重写这个方法。
    /// </summary>
    public virtual void ApplySize(Vector2 size)
    {
        Rect.sizeDelta = size;
    }

    /// <summary>
    /// 在 available 内我原始需要多大。不用自己钳制、不用管缓存，基类会做。
    /// </summary>
    protected abstract Vector2 MeasureCore(Vector2 available);
}
