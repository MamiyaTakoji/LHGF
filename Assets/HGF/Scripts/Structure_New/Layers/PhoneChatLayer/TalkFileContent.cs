using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 文件卡片内容块：只负责把数据填进三个零件，不做任何排版。
///
/// 气泡尺寸是固定的 —— 本预制体（FileTalkBar）里 TalkContent 的外壳已经设成
/// minWidth = maxWidth = 500、minHeight = 200，所以气泡恒为 500×200。
/// 因此这里 MeasureCore() 只返回一个很小的值，让外壳的最小尺寸约束把气泡钉住；
/// ApplySize() 直接留空，内容块自己不落尺寸、也不摆零件位置，一切都用预制体里排好的样子。
///
/// 挂载位置：TalkContent 下的一个容器物体上（例：Content_File），
/// FileName / FileSize / Image 三个零件是它的子物体。
/// ⚠ 不能挂到 TalkContent 自己身上：外壳会把"内容块"的 anchor/pivot 归中并重新居中，
///    挂在气泡自己身上会让气泡的锚点被改掉、位置跳动。
///
/// 数据来源：TalkBar 的 SetFile / SetFilePath →
///   values["icon"]（Sprite，运行时代码）或 values["path"]（路径，XML 剧本）
///   加上 values["name"] / values["size"]
///
/// 图标为什么有两条路：XML 只能传字符串，而 Sprite 是对象引用 —— 脚本驱动的场景下
/// values["icon"] 永远是 null，所以脚本走 path，由 <see cref="RemoteImageLoader"/> 加载。
/// 两条路互斥，优先用 Sprite（谁非空用谁）。
/// </summary>
[DisallowMultipleComponent]
public class TalkFileContent : TalkContentBase
{
    [Header("零件（留空会按名字自动查找）")]
    [Tooltip("文件名")]
    public TextMeshProUGUI fileName;
    [Tooltip("文件大小")]
    public TextMeshProUGUI fileSize;
    [Tooltip("文件图标")]
    public Image icon;

    public override TalkContentKind Kind => TalkContentKind.File;

    private RemoteImageLoader iconLoader;

    /// <summary>图标是否还在加载（列表可用它决定这一行算不算完成）。</summary>
    public override bool IsLoading => iconLoader != null && iconLoader.IsLoading;

    protected override void Awake()
    {
        base.Awake();

        if (fileName == null) fileName = FindLabel("FileName");
        if (fileSize == null) fileSize = FindLabel("FileSize");
        if (icon == null) icon = GetComponentInChildren<Image>(true);

        iconLoader = new RemoteImageLoader(this);

        if (fileName == null || fileSize == null || icon == null)
            Debug.LogWarning($"[{nameof(TalkFileContent)}] 零件没配齐：FileName={fileName} FileSize={fileSize} Icon={icon}", this);
    }

    /// <summary>
    /// 不参与量测：返回一个很小的值，气泡尺寸由外壳的 minWidth / minHeight 钉住
    /// （FileTalkBar 里是 500×200），所以无论文本多长，文件气泡大小都恒定。
    /// 哪天想让高度跟着内容走，把这里改成返回实际需要的高度即可。
    /// </summary>
    protected override Vector2 MeasureCore(Vector2 available) => Vector2.one;

    /// <summary>
    /// 空实现：内容块尺寸保持不变（用预制体上排好的样子），外壳只负责气泡自己的尺寸。
    /// </summary>
    public override void ApplySize(Vector2 size) { }

    /// <summary>把这一行的数据填进三个零件。</summary>
    public override void ApplyData(TalkContentData data)
    {
        if (fileName != null) fileName.text = data.GetString(TalkContentData.KeyName);
        if (fileSize != null) fileSize.text = data.GetString(TalkContentData.KeySize);

        ApplyIcon(data);

        MarkDirty();   // 直接写 .text / .sprite 不经过包装属性，必须显式通知，否则外壳不会重排
    }

    private void ApplyIcon(TalkContentData data)
    {
        if (icon == null) return;
        if (iconLoader == null) iconLoader = new RemoteImageLoader(this);

        // ① 直接给 Sprite（运行时代码路径）：所有权不在自己手里，先掐掉可能存在的路径加载
        Sprite sprite = data.GetSprite(TalkContentData.KeyIcon);
        if (sprite != null)
        {
            iconLoader.Release();
            SetIconSprite(sprite);
            return;
        }

        // ② 给路径（XML 剧本路径）
        string path = data.GetString(TalkContentData.KeyPath);
        if (!string.IsNullOrEmpty(path))
        {
            SetIconSprite(null);        // 加载期间先清空，避免显示上一条消息的图标
            iconLoader.Load(path, loaded => SetIconSprite(loaded));
            return;
        }

        // ③ 都没有 → 无图标的文件卡片
        iconLoader.Release();
        SetIconSprite(null);
    }

    private void SetIconSprite(Sprite sprite)
    {
        if (icon == null) return;
        icon.sprite = sprite;
        icon.enabled = sprite != null;
    }

    /// <summary>图标是本组件加载的纹理，失活时释放，再激活时重新加载。</summary>
    protected override void OnDisable()
    {
        base.OnDisable();      // 必须先调基类：外壳靠它 Unregister
        iconLoader?.Release();
        SetIconSprite(null);
    }

    private void OnDestroy()
    {
        iconLoader?.Release();
    }

    private TextMeshProUGUI FindLabel(string childName)
    {
        foreach (var label in GetComponentsInChildren<TextMeshProUGUI>(true))
            if (label.name == childName) return label;
        return null;
    }
}
