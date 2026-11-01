using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>一条对话的内容类型。</summary>
public enum TalkContentKind
{
    None,
    TmpText,    // 普通文本（TMPText + TalkTmpContent）
    TexDraw,    // 公式（TEXDraw + TalkTexContent）
    Image,      // 图片（TalkImageContent）
    File,       // 文件（TalkFileContent）
    TimeStamp,  // 时间戳（TalkTimeStampContent —— 特殊的 TalkTmpContent）
    Gif         // 动图（TalkGifContent，StreamingAssets 里的 GIF）
    // ★ 新值一律追加在末尾：kind 是序列化的整数，插在中间会让已有 prefab 里存的值错位
}

/// <summary>
/// 一条对话行的全部数据。
///
/// 内容信息统一放在 values 字典里（key → 值），具体怎么解释由内容组件自己决定：
///   TalkTmpContent   : text
///   TalkTexContent   : tex, size(可选)
///   TalkImageContent : sprite 或 path, caption(可选)
///   TalkGifContent   : path
///   TalkFileContent  : icon, name, size
/// 新类型只要约定自己的 key，本文件和 LHGFTalkContentControler 都不用改。
///
/// 序列化说明（重要）：
///   · Newtonsoft（JsonConvert）能直接序列化 Dictionary → 存档没问题；
///   · Unity 自带序列化不支持 Dictionary → 它不会显示在 Inspector，也不会随场景/prefab 保存；
///   · ⚠ 不要加 [NonSerialized]：Json.NET 也认这个特性，会把整个字典一起忽略掉；
///   · Sprite 是 Unity 对象引用，不能进 JSON —— 存档时放资源路径（path）或 id，运行时再解析成 Sprite。
/// </summary>
[Serializable]
public class TalkContentData
{
    // ---------------- key 常量：避免字符串打错 ----------------
    public const string KeyText    = "text";
    public const string KeyTex     = "tex";
    public const string KeySize    = "size";      // TexDraw = 字号；File = 大小文字
    public const string KeySprite  = "sprite";
    public const string KeyCaption = "caption";
    public const string KeyIcon    = "icon";
    public const string KeyName    = "name";
    public const string KeyPath    = "path";      // 图片 / 动图 / 文件图标 / 头像：完整路径 / URL，或 StreamingAssets 相对路径

    public TalkContentKind kind = TalkContentKind.TmpText;

    /// <summary>所有内容信息。字符串 / 数字 / Sprite 都能放。</summary>
    public Dictionary<string, object> values = new Dictionary<string, object>();

    // ---------------- 行级显示属性（不属于"内容"）----------------
    [Tooltip("靠右：先对话框后头像；靠左：先头像后对话框")]
    public bool isRightSide;

    [Tooltip("这一行是否显示头像（连续同一个说话人时常关掉）")]
    public bool showAvatar = true;

    [Tooltip("这一行的头像。留空 = 不改变当前头像")]
    public Sprite avatar;

    /// <summary>
    /// 这一行的头像路径（完整路径 / URL / StreamingAssets 相对路径）。
    ///
    /// 为什么要有它：XML 剧本只能传字符串，而 <see cref="avatar"/> 是 Sprite —— 脚本驱动的场景下
    /// 那个字段永远是 null。所以脚本走 avatarPath，运行时代码走 avatar，两者谁非空用谁。
    /// 留空 = 沿用上一条的头像（与 avatar 的语义一致）。
    /// </summary>
    [Tooltip("头像路径（完整路径 / URL / StreamingAssets 相对路径）。留空 = 不改变当前头像")]
    public string avatarPath;

    // ---------------- 取值：越界 / 类型不符都返回默认值 ----------------

    public string GetString(string key, string def = "")
        => values != null && values.TryGetValue(key, out var v) && v != null ? v.ToString() : def;

    /// <summary>取数字。从 JSON 读回来可能是 long / double / JValue，统一走 Convert。</summary>
    public float GetFloat(string key, float def = 0f)
    {
        if (values == null || !values.TryGetValue(key, out var v) || v == null) return def;
        try { return Convert.ToSingle(v); }
        catch { return def; }
    }

    public Sprite GetSprite(string key)
        => values != null && values.TryGetValue(key, out var v) ? v as Sprite : null;

    public bool Has(string key) => values != null && values.ContainsKey(key);

    // ---------------- 写入 ----------------

    public void Set(string key, object value)
    {
        if (values == null) values = new Dictionary<string, object>();
        values[key] = value;
    }

    /// <summary>换一个内容类型并清空旧内容。</summary>
    public void Reset(TalkContentKind newKind)
    {
        kind = newKind;
        values?.Clear();
    }

    public void ClearContent() => values?.Clear();
}

/// <summary>
/// 一条对话行（TalkBar）的控制器。挂在 TalkBar 上，只负责三件事：
///
/// 1) 定义这一行的数据类型（TalkContentKind / TalkContentData 就在本文件里）
/// 2) 排列头像与对话框的关系
///      靠左：[头像][对话框]        靠右：[对话框][头像]
///    实现方式 = 层级顺序（谁在前）+ 横向布局组的 childAlignment（整组贴哪边 + 头像竖直对齐）。
///    layout group 本来就是按 sibling index 排的，所以"先头像后对话框"直接映射成 SetSiblingIndex。
/// 3) 把数据交给外壳，由外壳去解释
///    Restore() = 排列 → bubble.Show(data) →（可选）播放动画。
///    本组件完全不认识具体的内容组件（TmpText / TexDraw / Image / File 都不认识）。
///
/// 【场景侧要求】TalkBar 的布局组件请这样配置，否则会出现位置跳动：
///      · 用 HorizontalLayoutGroup（不是 Vertical），Spacing 自定；
///      · Child Control Width / Height      → 都关闭（尺寸由 TalkContent 自己算）
///      · Child Force Expand Width / Height → 都关闭（否则子物体被拉伸）
///      · Child Scale Width / Height        → 都关闭（它会让 localScale 参与位置计算，
///        而弹出动画正好在改 localScale，会导致动画期间位置漂移）
///      · 保留 ContentSizeFitter（Vertical = PreferredSize）→ 行高自动 = max(头像高, 气泡高)
///    另外建议把 TalkContent 上的 autoPlayShowAnimation 关掉，动画统一由本组件控制。
/// </summary>
[DisallowMultipleComponent]
public class LHGFTalkBarControler : MonoBehaviour
{
    /// <summary>头像在这一行里的竖直对齐方式。</summary>
    public enum VerticalAnchor { Upper, Middle, Lower }

    [Header("引用（留空自动查找）")]
    [Tooltip("气泡外壳，一般就在子物体 TalkContent 上")]
    public LHGFTalkContentControler bubble;
    [Tooltip("头像。留空则自动取 TalkBar 下除气泡之外的第一个子物体")]
    public RectTransform avatar;
    [Tooltip("横向布局组。留空则自动取自身的 HorizontalLayoutGroup")]
    public HorizontalLayoutGroup rowLayout;

    [Header("排列规则")]
    [Tooltip("头像的竖直对齐。聊天框常用 Upper（头像顶部对齐气泡顶部）")]
    public VerticalAnchor verticalAnchor = VerticalAnchor.Upper;
    [Tooltip("自动把布局组的 childAlignment 设置成和左右侧一致")]
    public bool driveLayoutAlignment = true;

    [Header("内容数据")]
    [SerializeField] private TalkContentData data = new TalkContentData();

    [Header("动画")]
    [Tooltip("显示内容时是否播放弹出动画")]
    public bool playAnimation = true;

    /// <summary>当前这一行的数据（可读）。</summary>
    public TalkContentData Data => data;

    public bool IsRightSide => data.isRightSide;

    // 头像的路径加载器（懒建）。只在本组件自己创建的纹理上负责销毁。
    private RemoteImageLoader avatarLoader;
    private RemoteImageLoader AvatarLoader => avatarLoader ??= new RemoteImageLoader(this);

    // 真正显示头像的 Image（Avata 的子物体，不是 Avata 自己的遮罩图）。缓存避免反复查找。
    private Image avatarImage;

    // prefab 里预置的那张头像图。没给 AvatarPath 时用它；加载失败时回退到它。
    // 不记下来的话，一旦被"加载失败"清空就再也回不来了（后续消息走"不改变头像"分支，不会重设）。
    private Sprite defaultAvatarSprite;
    private bool defaultAvatarCaptured;

    /// <summary>头像是否还在加载（列表可用它决定这一行算不算完成）。</summary>
    public bool IsAvatarLoading => avatarLoader != null && avatarLoader.IsLoading;

    /// <summary>气泡外壳。列表用它查动画状态、转发跳过动画。</summary>
    public LHGFTalkContentControler Bubble => bubble != null ? bubble : (bubble = GetComponentInChildren<LHGFTalkContentControler>(true));

    /// <summary>当前激活的内容块是否还在异步加载（图片 / 动图 / 文件图标）。动图必须等它变 false。</summary>
    public bool IsContentLoading
    {
        get
        {
            var b = Bubble;
            return b != null && b.CurrentContent != null && b.CurrentContent.IsLoading;
        }
    }

    /// <summary>这一行的弹出动画是否还在播。</summary>
    public bool IsPlayingShowAnimation => Bubble != null && Bubble.IsPlayingShowAnimation;

    /// <summary>让这一行的弹出动画立刻结束（转发给外壳，对应 Skip()）。</summary>
    public void CompleteShowAnimation()
    {
        Bubble?.CompleteShowAnimation();
    }

    /// <summary>整行是否被藏起来（等待"对方正在输入"期间）。置 true 时头像也强制隐藏。</summary>
    private bool visualHidden;

    /// <summary>
    /// 把这一行整个藏起来，用于等"对方正在输入"：
    /// 气泡走外壳的 alpha=0（不改激活状态，排版/头像照常加载）；
    /// 头像不归外壳的 CanvasGroup 管（它是 TalkContent 的兄弟节点），单独隐藏。
    /// </summary>
    public void HideVisual()
    {
        visualHidden = true;
        Bubble?.HideBubbleVisual();
        ResolveAvatarImage();               // 确保 avatar 的引用已解析（隐藏父链下 Awake 会推迟）
        SetAvatarVisible(false);
    }

    /// <summary>
    /// 把藏起来的一行显形，withAnimation 决定要不要播弹出动画。
    /// 头像的显隐按当前数据重新判定（已加载完就显示，还没加载完就保持隐藏等回调）。
    /// </summary>
    public void Reveal(bool withAnimation)
    {
        visualHidden = false;
        Bubble?.RevealBubble(withAnimation);
        SetAvatarVisible(data.showAvatar && ResolveAvatarImageIsSet());
    }

    /// <summary>头像上是不是已经有一张能显示的图了。</summary>
    private bool ResolveAvatarImageIsSet()
    {
        var image = ResolveAvatarImage();
        return image != null && image.enabled && image.sprite != null;
    }

    // ==================== 生命周期 ====================

    private void Awake()
    {
        Resolve();
        CaptureDefaultAvatar();
    }

    /// <summary>
    /// 记下 prefab 预置的头像图（只记一次）。
    /// 必须在任何人改它之前调用 —— 所以放在 Awake，且 Resolve 之后（要先找到那个 Image）。
    /// </summary>
    private void CaptureDefaultAvatar()
    {
        if (defaultAvatarCaptured) return;
        defaultAvatarCaptured = true;

        var image = ResolveAvatarImage();
        if (image != null) defaultAvatarSprite = image.sprite;
    }

    // ⚠ 这里曾经有个 Start() → Restore(playAnimation)：
    //   每个 bar 实例化后一帧会【自己再播一遍弹出动画】，把"对方正在输入"期间
    //   刚藏好的气泡又亮出来（后来有意等它弹出的 Reveal 就变成了提前偷跑）。
    //   bar 的全部内容由 AddMessage → Restore 同步填好，这个重放没有意义，故删除。
    //   在隐藏的 Phone 下实例化导致 Awake 推迟的情况，由 Restore 开头的 Resolve + CaptureDefaultAvatar 兜住。

#if UNITY_EDITOR
    private void OnValidate()
    {
        Resolve();
        if (Application.isPlaying && isActiveAndEnabled) ApplyOrder();
    }
#endif

    private void OnDestroy()
    {
        // 头像是本组件自己加载的纹理，实例销毁时必须释放，否则列表反复增删会一路泄漏
        avatarLoader?.Release();
    }

    // ==================== 数据入口 ====================
    // 这里只做"写数据 + 转发"，key 的具体含义由各内容组件自己解释。

    /// <summary>一条普通文本。rightSide 传 null 表示沿用当前的左右侧。</summary>
    public void SetText(string text, bool? rightSide = null, bool withAnimation = true)
    {
        data.Reset(TalkContentKind.TmpText);
        data.Set(TalkContentData.KeyText, text);
        if (rightSide.HasValue) data.isRightSide = rightSide.Value;
        Restore(withAnimation);
    }

    /// <summary>
    /// 一条时间戳。和普通文本共用同一套量测（TalkTimeStampContent 继承自 TalkTmpContent），
    /// 这里只是把 kind 换成 TimeStamp、并默认关掉头像 ——
    /// 时间戳一般是"居中、无头像、无气泡底"的一行，对应一个单独的预制体。
    /// </summary>
    public void SetTimeStamp(string time, bool withAnimation = true)
    {
        data.Reset(TalkContentKind.TimeStamp);
        data.Set(TalkContentData.KeyText, time);
        data.showAvatar = false;
        Restore(withAnimation);
    }

    /// <summary>一条公式。fontSize 传 null 表示不改字号。</summary>
    public void SetFormula(string tex, float? fontSize = null, bool? rightSide = null, bool withAnimation = true)
    {
        data.Reset(TalkContentKind.TexDraw);
        data.Set(TalkContentData.KeyTex, tex);
        if (fontSize.HasValue) data.Set(TalkContentData.KeySize, fontSize.Value);
        if (rightSide.HasValue) data.isRightSide = rightSide.Value;
        Restore(withAnimation);
    }

    /// <summary>一条图片。caption 可省。</summary>
    public void SetImage(Sprite sprite, string caption = null, bool? rightSide = null, bool withAnimation = true)
    {
        data.Reset(TalkContentKind.Image);
        data.Set(TalkContentData.KeySprite, sprite);
        if (!string.IsNullOrEmpty(caption)) data.Set(TalkContentData.KeyCaption, caption);
        if (rightSide.HasValue) data.isRightSide = rightSide.Value;
        Restore(withAnimation);
    }

    /// <summary>一条图片：给完整路径 / URL，或 StreamingAssets 下的相对路径。</summary>
    public void SetImagePath(string path, string caption = null, bool? rightSide = null, bool withAnimation = true)
    {
        data.Reset(TalkContentKind.Image);
        data.Set(TalkContentData.KeyPath, path);
        if (!string.IsNullOrEmpty(caption)) data.Set(TalkContentData.KeyCaption, caption);
        if (rightSide.HasValue) data.isRightSide = rightSide.Value;
        Restore(withAnimation);
    }

    /// <summary>一条动图（GIF）：给完整路径 / URL，或 StreamingAssets 下的相对路径。</summary>
    public void SetGif(string path, bool? rightSide = null, bool withAnimation = true)
    {
        data.Reset(TalkContentKind.Gif);
        data.Set(TalkContentData.KeyPath, path);
        if (rightSide.HasValue) data.isRightSide = rightSide.Value;
        Restore(withAnimation);
    }

    /// <summary>一条文件卡片：图标 + 文件名 + 大小。icon 直接给 Sprite（运行时代码用）。</summary>
    public void SetFile(Sprite icon, string fileName, string sizeText, bool? rightSide = null, bool withAnimation = true)
    {
        data.Reset(TalkContentKind.File);
        data.Set(TalkContentData.KeyIcon, icon);
        data.Set(TalkContentData.KeyName, fileName);
        data.Set(TalkContentData.KeySize, sizeText);
        if (rightSide.HasValue) data.isRightSide = rightSide.Value;
        Restore(withAnimation);
    }

    /// <summary>
    /// 一条文件卡片：图标给路径（XML 剧本走这个）。
    /// iconPath 支持完整路径 / URL / StreamingAssets 相对路径，留空则显示无图标的卡片。
    /// </summary>
    public void SetFilePath(string iconPath, string fileName, string sizeText, bool? rightSide = null, bool withAnimation = true)
    {
        data.Reset(TalkContentKind.File);
        data.Set(TalkContentData.KeyPath, iconPath);
        data.Set(TalkContentData.KeyName, fileName);
        data.Set(TalkContentData.KeySize, sizeText);
        if (rightSide.HasValue) data.isRightSide = rightSide.Value;
        Restore(withAnimation);
    }

    /// <summary>直接塞一整份数据（对话系统一般走这个）。</summary>
    public void SetContent(TalkContentData newData, bool withAnimation = true)
    {
        if (newData == null) return;
        data = newData;
        Restore(withAnimation);
    }

    /// <summary>只切换左右侧（不换内容），立即重新排列。</summary>
    public void SetSide(bool rightSide)
    {
        data.isRightSide = rightSide;
        ApplyOrder();
    }

    /// <summary>换头像。传 null 表示不改。</summary>
    public void SetAvatar(Sprite sprite)
    {
        data.avatar = sprite;
        data.avatarPath = null;      // 两个来源互斥：给了 Sprite 就清掉路径，避免旧路径把人覆盖回去
        ApplyAvatar();
    }

    /// <summary>
    /// 换头像：给路径（完整路径 / URL / StreamingAssets 相对路径）。传空表示不改。
    /// 这是 XML 剧本的入口 —— avatar 字段是 Sprite，脚本传不了。
    /// </summary>
    public void SetAvatarPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return;    // 空 = 沿用当前头像，别把已有头像清掉
        data.avatarPath = path;
        data.avatar = null;
        ApplyAvatar();
    }

    /// <summary>
    /// 按当前 data 重建这一行：排列 → 把数据交给外壳解释 →（可选）播放动画。
    /// </summary>
    public void Restore(bool withAnimation)
    {
        Resolve();
        // Awake 在被隐藏的父链下实例化时会推迟到激活，这里补一次默认头像抓取（幂等）
        CaptureDefaultAvatar();

        ApplyOrder();
        ApplyAvatar();

        TalkContentBase content = bubble != null ? bubble.Show(data) : null;

        if (withAnimation && playAnimation && content != null)
            bubble.PlayShowAnimation();
    }

    // ==================== 内部实现 ====================

    private void Resolve()
    {
        if (bubble == null) bubble = GetComponentInChildren<LHGFTalkContentControler>(true);
        if (rowLayout == null) rowLayout = GetComponent<HorizontalLayoutGroup>();

        if (avatar == null)
        {
            // 取 TalkBar 下"不是气泡"的第一个子物体当作头像
            foreach (Transform child in transform)
            {
                if (bubble != null && child == bubble.transform) continue;
                if (child is RectTransform rt) { avatar = rt; break; }
            }
        }
    }

    /// <summary>排列：层级顺序决定谁在前，childAlignment 决定整组贴哪边 + 头像竖直对齐。</summary>
    private void ApplyOrder()
    {
        if (bubble != null && avatar != null)
        {
            var bubbleRect = (RectTransform)bubble.transform;

            // 层级顺序：靠左 [头像, 对话框]，靠右 [对话框, 头像]
            int baseIndex = Mathf.Min(avatar.GetSiblingIndex(), bubbleRect.GetSiblingIndex());
            if (data.isRightSide)
            {
                bubbleRect.SetSiblingIndex(baseIndex);
                avatar.SetSiblingIndex(baseIndex + 1);
            }
            else
            {
                avatar.SetSiblingIndex(baseIndex);
                bubbleRect.SetSiblingIndex(baseIndex + 1);
            }
        }

        // 整组贴左 / 贴右 + 头像竖直对齐
        // TextAnchor 正好是 3×3：UpperLeft=0, UpperCenter=1, UpperRight=2, MiddleLeft=3, ...
        if (driveLayoutAlignment && rowLayout != null)
        {
            int row = (int)verticalAnchor;          // Upper=0, Middle=1, Lower=2
            int col = data.isRightSide ? 2 : 0;     // Left=0, Right=2
            rowLayout.childAlignment = (TextAnchor)(row * 3 + col);
        }
    }

    /// <summary>
    /// 取真正用来显示头像的那个 Image，并缓存下来。
    ///
    /// ⚠ 不能用 avatar.GetComponentInChildren&lt;Image&gt;()：
    ///   预制体里的结构是「Avata（挂 Mask + 一张圆角底图）→ Image（子物体，真正的头像）」。
    ///   Avata 自己身上就有一个 Image（用作 Mask 的遮罩图形，m_ShowMaskGraphic = 1），
    ///   而 GetComponentInChildren 是深度优先、**会先返回 Avata 自己那个**，
    ///   结果就是"设置头像"改到遮罩图上去了：头像纹丝不动，遮罩形状还被破坏。
    ///
    /// 所以这里优先找名为 "Image" 的子物体；找不到再退回"除自己以外"的第一个 Image。
    /// </summary>
    private Image ResolveAvatarImage()
    {
        if (avatar == null) return null;
        if (avatarImage != null) return avatarImage;

        // ① 首选：名为 Image 的直接子物体（预制体的既定结构）
        var child = avatar.Find("Image");
        if (child != null)
        {
            avatarImage = child.GetComponent<Image>();
            if (avatarImage == null) avatarImage = child.GetComponentInChildren<Image>(true);
        }

        // ② 兜底：Avata 自己身上的（万一预制体改成把头像图挂在 Avata 上）
        if (avatarImage == null) avatarImage = avatar.GetComponent<Image>();

        // ③ 再兜底：任意子物体里的第一个
        if (avatarImage == null) avatarImage = avatar.GetComponentInChildren<Image>(true);

        if (avatarImage == null)
            Debug.LogWarning($"[{nameof(LHGFTalkBarControler)}] 头像下找不到 Image 组件，头像无法显示。", this);

        return avatarImage;
    }

    private void ApplyAvatar()
    {
        if (avatar == null) return;

        var image = ResolveAvatarImage();
        if (image == null) return;

        // ① 直接给 Sprite（运行时代码路径）：同步设置，所有权不在我们手里
        if (data.avatar != null)
        {
            avatarLoader?.Release();
            SetAvatarSprite(data.avatar);
            return;
        }

        // ② 给路径（XML 剧本路径）：异步加载，纹理由 loader 负责销毁
        if (!string.IsNullOrEmpty(data.avatarPath))
        {
            // 关键：加载完成前先把整个头像藏起来。
            // 否则会先露出 Avata 的圆角底图（一个没有图的空框），等图片到了才被盖住 —— 看着像"白框闪一下"。
            SetAvatarVisible(false);

            AvatarLoader.Load(data.avatarPath, sprite =>
            {
                if (this == null) return;
                // 加载失败（sprite == null）就回退到 prefab 默认图，避免留下空框
                SetAvatarSprite(sprite != null ? sprite : defaultAvatarSprite);
            });
            return;
        }

        // ③ 都没给 = 用 prefab 里预置的那张图（同步，不需要等待）
        SetAvatarSprite(defaultAvatarSprite);
    }

    /// <summary>
    /// 把图设到真正的头像 Image 上，并按 showAvatar 决定整块头像的显隐。
    /// sprite 为 null 表示没有可显示的图 —— 这时整个头像不显示，而不是留一个空框。
    /// </summary>
    private void SetAvatarSprite(Sprite sprite)
    {
        var image = ResolveAvatarImage();
        if (image == null) return;

        image.sprite = sprite;
        image.enabled = sprite != null;

        // 没有图就别显示头像框（否则露出 Avata 的圆角底图，看着是个白框）
        SetAvatarVisible(data.showAvatar && sprite != null);
    }

    /// <summary>开关整个头像物体。加载中也会用它先藏起来。</summary>
    private void SetAvatarVisible(bool visible)
    {
        if (avatar == null) return;
        // 整行被 HideVisual 藏起来（等待"对方正在输入"）时，头像不许单独漏出来
        if (visualHidden) visible = false;
        if (avatar.gameObject.activeSelf != visible) avatar.gameObject.SetActive(visible);
    }
}

// visualHidden 字段声明在下面带 HideVisual / Reveal 的位置（见文件前部）
