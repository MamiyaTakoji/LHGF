using System.Collections.Generic;
using LHGFData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 手机聊天层的表现 + 列表管理。
///
/// 【挂载位置】挂在外壳物体上（常驻，从不失活），Phone.prefab 实例是它的子物体。
///   和黑板同构：BlackboardLayer（常驻控制器）→ Blackboard（prefab 实例，开关它）。
///   控制器不失活，IsLoading / IsAnimating 才能随时被查询。
///
///   PhoneChatLayer            [本组件]        ← 常驻，显隐的是子物体
///   └── Phone                 [Phone.prefab]  ← 实际开关它
///       ├── UpperPanel/Center/TalkerName
///       ├── Scroll View/Viewport/Content
///       └── LowerPanel
///
/// 【职责】显隐手机、改名字、按类型选预制体建/删 bar、汇总加载与动画状态。
///   解析命令、分组、回撤、存档都在 PhoneChatLayer 里。
///
/// 【Finish 的两个输入】
///   IsAnimating —— 还有弹出动画在播
///   IsLoading   —— 还有图片/动图/图标/头像在加载（**动图必须等它**）
/// </summary>
public class LHGFChatLayerControler : MonoBehaviour
{
    [Header("引用（留空按名字自动查找）")]
    [Tooltip("手机根节点，显隐的就是它。留空 = 找子物体 Phone")]
    public GameObject phoneRoot;
    [Tooltip("消息列表容器。留空按名字找 Content")]
    public Transform content;
    [Tooltip("滚动组件。留空从 content 往上找")]
    public ScrollRect scrollRect;
    [Tooltip("顶部聊天对象名字。留空按名字找 TalkerName")]
    public TMP_Text talkerName;
    [Tooltip("顶部状态文字。平时显示在线状态，输入时显示对方正在输入。留空按名字找 TalkerState")]
    public TMP_Text talkerState;

    [Header("各类型的预制体")]
    public LHGFTalkBarControler tmpBar;         // TmpText
    public LHGFTalkBarControler texBar;         // TexDraw（暂未实现，可留空；留空时按文本渲染）
    public LHGFTalkBarControler fileBar;        // File
    public LHGFTalkBarControler timeStampBar;   // TimeStamp
    public LHGFTalkBarControler imageBar;       // Image
    public LHGFTalkBarControler gifBar;         // Gif

    [Header("行为")]
    public bool autoScrollToBottom = true;
    [Tooltip("列表最多保留多少条，超出时从最旧的开始销毁。0 = 不限")]
    public int maxBars = 50;

    [Header("状态文字")]
    [Tooltip("平时显示的状态（TalkerState 的默认内容）")]
    public string onlineText = "在线";
    [Tooltip("等待气泡出现时显示的状态")]
    public string typingText = "对方正在输入";

    /// <summary>当前列表里所有活着的气泡（按加入顺序）。</summary>
    private readonly List<LHGFTalkBarControler> bars = new List<LHGFTalkBarControler>();

    // ==================== 模拟对方输入 ====================

    private float typingRemain;                         // 剩余等待秒数，>0 表示正在"输入"
    private LHGFTalkBarControler pendingBar;            // 等完要显形的 bar；null = 只是吊胃口，不发消息
    private bool pendingSkipAnimation;
    private int lastTypingFrame = -1;                   // 上一次 BeginTyping 所在帧，用来识别"同一批的多条"

    /// <summary>是否正在等"对方正在输入"。Finish() 要把它算进去，否则玩家能直接跳过这段等待。</summary>
    public bool IsTyping => typingRemain > 0f;

    /// <summary>
    /// 最近一次 AddMessage 真正建出来的 bar。
    /// 层靠它把 TypingTime 和具体哪条气泡挂上，不靠 bars[last] 猜。
    /// </summary>
    public LHGFTalkBarControler LastCreatedBar { get; private set; }

    /// <summary>
    /// 开始一段"对方正在输入"。bar 是等完要显形的气泡（null = 只等，不发消息）。
    ///
    /// ⚠ Forward 会把一个节点的所有命令在一帧内跑完，层分不出"我是本节点第几条"，
    ///   所以用"同一帧里第二条 TypingTime"来识别一批多条。
    /// 剧本作者定的规则：同一批多条 → 无视 TypingTime —— 本次清掉等待直接显形，
    ///   这条也不等（消息按命令顺序一帧出齐）；单独一条在自己的节点里时正常等。
    /// </summary>
    public void BeginTyping(float seconds, LHGFTalkBarControler bar, bool skipAnimation, string text = null)
    {
        if (seconds <= 0f) return;

        bool sameFrameBatch = typingRemain > 0f || lastTypingFrame == Time.frameCount;
        lastTypingFrame = Time.frameCount;

        if (sameFrameBatch)
        {
            FinishTypingNow();                          // 前一条立刻显形（可能已被上一条冲掉，幂等）
            if (bar != null) bar.Reveal(!skipAnimation); // 这条也不等
            return;
        }

        typingRemain = seconds;
        pendingBar = bar;
        pendingSkipAnimation = skipAnimation;
        SetTalkerState(string.IsNullOrEmpty(text) ? typingText : text);
    }

    /// <summary>由层每帧驱动。计时走完就把藏着的气泡显出来。</summary>
    public void UpdateTyping()
    {
        if (typingRemain <= 0f) return;

        typingRemain -= Time.deltaTime;
        if (typingRemain > 0f) return;

        FinishTypingNow();
    }

    /// <summary>立刻结束等待（Skip / 多条插队用）：状态文字复位，藏着的气泡立刻显形。</summary>
    public void FinishTypingNow()
    {
        if (typingRemain <= 0f && pendingBar == null) return;

        typingRemain = 0f;
        SetTalkerState(onlineText);

        var bar = pendingBar;
        var skip = pendingSkipAnimation;
        pendingBar = null;
        pendingSkipAnimation = false;

        if (bar != null)
        {
            bar.Reveal(!skip);
            if (autoScrollToBottom) ScrollToBottom();
        }
    }

    /// <summary>清掉等待状态，不显形（Reset / 回撤时用）。状态文字一并复位，否则会一直停在"正在输入"。</summary>
    public void CancelTyping()
    {
        typingRemain = 0f;
        pendingBar = null;
        pendingSkipAnimation = false;
        if (talkerState != null) SetTalkerState(onlineText);
    }

    /// <summary>设置顶部状态文字。传空 = 显示 onlineText。</summary>
    public void SetTalkerState(string text)
    {
        if (talkerState == null) return;
        talkerState.text = string.IsNullOrEmpty(text) ? onlineText : text;
    }

    // ==================== 状态上报（给 PhoneChatLayer.Finish）====================

    /// <summary>还有弹出动画在播吗。遍历所有 bar。</summary>
    public bool IsAnimating
    {
        get
        {
            for (int i = 0; i < bars.Count; i++)
            {
                var b = bars[i];
                if (b != null && b.IsPlayingShowAnimation) return true;
            }
            return false;
        }
    }

    /// <summary>还有图片/动图/图标/头像在加载吗。遍历所有 bar。</summary>
    public bool IsLoading
    {
        get
        {
            for (int i = 0; i < bars.Count; i++)
            {
                var b = bars[i];
                if (b == null) continue;
                if (b.IsAvatarLoading || b.IsContentLoading) return true;
            }
            return false;
        }
    }

    // ==================== 生命周期 ====================

    /// <summary>
    /// 常驻实例。本物体从不失活（失活的是子物体 Phone），所以适合给
    /// RemoteImageLoader 这类需要跑协程的组件当宿主 ——
    /// 消息所在的 bar 可能是 inactive 的（手机隐藏时、或读档瞬间还没 Show），
    /// 在它身上 StartCoroutine 会直接报 "game object is inactive"。
    /// </summary>
    public static LHGFChatLayerControler Instance { get; private set; }

    /// <summary>
    /// 给外部要跑协程的组件取一个"一定处于激活状态"的宿主。
    /// 优先用常驻的 Instance；没有就退回自己（此时至少行为可预期）。
    /// </summary>
    public static MonoBehaviour ResolveCoroutineHost(MonoBehaviour fallback)
    {
        if (Instance != null && Instance.isActiveAndEnabled) return Instance;
        return fallback;
    }

    private void Awake()
    {
        Instance = this;
        Resolve();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        Hide();     // 初始关闭，等剧本命令决定
    }

    private void Resolve()
    {
        if (phoneRoot == null)
        {
            var t = FindDeep(transform, "Phone");
            phoneRoot = t != null ? t.gameObject : gameObject;
        }
        if (content == null) content = FindDeep(transform, "Content");
        if (talkerName == null)
        {
            var t = FindDeep(transform, "TalkerName");
            if (t != null) talkerName = t.GetComponent<TMP_Text>();
        }
        if (talkerState == null)
        {
            var t = FindDeep(transform, "TalkerState");
            if (t != null) talkerState = t.GetComponent<TMP_Text>();
        }
        if (scrollRect == null && content != null) scrollRect = content.GetComponentInParent<ScrollRect>();
    }

    /// <summary>按名字深度优先查找子物体（含未激活的）。</summary>
    private static Transform FindDeep(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;
        foreach (Transform child in root)
        {
            var found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }

    // ==================== 显隐 / 名字 ====================

    public void Show()
    {
        if (phoneRoot != null && !phoneRoot.activeSelf) phoneRoot.SetActive(true);
    }

    public void Hide()
    {
        if (phoneRoot != null && phoneRoot.activeSelf) phoneRoot.SetActive(false);
    }

    /// <summary>改顶部聊天对象名字。传空 = 不改。</summary>
    public void SetTalkerName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (talkerName == null)
        {
            Debug.LogWarning($"[{nameof(LHGFChatLayerControler)}] 找不到 TalkerName 文本，名字无法设置。", this);
            return;
        }
        talkerName.text = name;
    }

    // ==================== 列表管理 ====================

    /// <summary>清空聊天框里的所有消息（对应 Continue=false）。名字和手机显隐不受影响。</summary>
    public void ClearAll()
    {
        for (int i = 0; i < bars.Count; i++)
        {
            if (bars[i] != null) Destroy(bars[i].gameObject);
        }
        bars.Clear();
    }

    /// <summary>重置整个列表（读档 / Reset 用）：清空消息 + 清空名字 + 结束输入状态。</summary>
    public void ResetAll()
    {
        CancelTyping();
        ClearAll();
        if (talkerName != null) talkerName.text = string.Empty;
        SetTalkerState(null);
    }

    /// <summary>列表里当前的 bar 数量。层靠它判断增删是否对齐。</summary>
    public int BarCount => bars.Count;

    /// <summary>
    /// 删掉最后一个 bar（回撤时用）。
    /// 返回是否真的删掉了一个 —— 返回 false 说明列表本来就空，层的记录和界面对不上了。
    /// </summary>
    public bool RemoveLastBar()
    {
        if (bars.Count == 0) return false;
        var last = bars[bars.Count - 1];
        bars.RemoveAt(bars.Count - 1);
        if (last != null) Destroy(last.gameObject);
        return true;
    }

    /// <summary>
    /// 追加一条消息。forceSkip = true 时强制跳过弹出动画（读档重建时用）。
    /// Skip 由本类解释（Next 只负责把数据交过来）。
    /// hidden = true 时先不显示（配合"对方正在输入"，等计时结束再显形）。
    ///
    /// 返回本次真正新增的 bar 数减去被 maxBars 裁掉的数量：
    ///   · 返回 1  → 正常加了一条；
    ///   · 返回 0  → 没加成（预制体缺失等），或被裁掉一条正好抵掉；
    ///   · 返回负数 → 裁掉的比加的多（不该发生，但层那边按这个数删记录也安全）。
    /// 层用它把 Contents 里的记录和界面上的 bar 保持一一对应。
    /// </summary>
    public int AddMessage(PhoneChatLayer.PhoneData data, bool forceSkip = false, bool hidden = false)
    {
        if (data == null || !data.HasMessage) return 0;

        // 等待期间又来一条消息（比如节点里"带 TypingTime 的"后面跟了条普通的）：
        // 把挂起的立刻显出来 —— 不然普通的先出现在屏幕上、带等待的那条反而晚到，顺序会乱。
        if (IsTyping) FinishTypingNow();

        if (content == null)
        {
            Debug.LogWarning($"[{nameof(LHGFChatLayerControler)}] content 没设置，消息无法显示。", this);
            return 0;
        }

        var prefab = PickPrefab(data.ContentType);
        if (prefab == null)
        {
            Debug.LogWarning($"[{nameof(LHGFChatLayerControler)}] 类型 {data.ContentType} 没有配预制体，这条消息被跳过。", this);
            return 0;
        }

        var bar = Instantiate(prefab, content);
        bar.name = $"PhoneBar_{data.ContentType}_{bars.Count}";
        bars.Add(bar);
        LastCreatedBar = bar;      // 层靠它把 TypingTime 挂到这一条上

        if (!string.IsNullOrEmpty(data.AvatarPath)) bar.SetAvatarPath(data.AvatarPath);

        // hidden 时不起弹出动画：马上就要被藏起来，起了也白起（还会被 Kill 掉再重算）
        bool withAnimation = !(forceSkip || data.Skip) && !hidden;
        ApplyToBar(bar, data, withAnimation);

        // 等"对方正在输入"期间：先把气泡藏起来（alpha 0）。
        // 不改变激活状态，所以头像/图片该加载照旧加载，显形时不用重来。
        if (hidden) bar.HideVisual();

        int trimmed = TrimOverflow();
        if (autoScrollToBottom) ScrollToBottom();
        return 1 - trimmed;
    }

    /// <summary>按内容类型把数据填进 bar。</summary>
    private void ApplyToBar(LHGFTalkBarControler bar, PhoneChatLayer.PhoneData data, bool withAnimation)
    {
        switch (data.ContentType)
        {
            case TalkContentKind.TimeStamp:
                bar.SetTimeStamp(data.Content, withAnimation);
                break;

            case TalkContentKind.File:
                // File 的载荷是 Content(文件名) / FileSize / Icon；图标走路径，所以用 SetFilePath
                bar.SetFilePath(data.Icon, data.Content, data.FileSize, data.RightSide, withAnimation);
                break;

            case TalkContentKind.Image:
                bar.SetImagePath(data.Content, null, data.RightSide, withAnimation);
                break;

            case TalkContentKind.Gif:
                bar.SetGif(data.Content, data.RightSide, withAnimation);
                break;

            // TexDraw 暂未实现，当普通文本渲染（公式会显示成原始标记，等实现后改这里）
            case TalkContentKind.TexDraw:
            default:
                bar.SetText(data.Content, data.RightSide, withAnimation);
                break;
        }
    }

    private LHGFTalkBarControler PickPrefab(TalkContentKind kind)
    {
        switch (kind)
        {
            case TalkContentKind.TmpText:   return tmpBar;
            case TalkContentKind.TexDraw:   return texBar != null ? texBar : tmpBar;
            case TalkContentKind.File:      return fileBar;
            case TalkContentKind.TimeStamp: return timeStampBar != null ? timeStampBar : tmpBar;
            case TalkContentKind.Image:     return imageBar;
            case TalkContentKind.Gif:       return gifBar;
            default:                        return tmpBar;
        }
    }

    /// <summary>
    /// 超过上限时销毁最旧的几条，防止长对话把 UI 对象堆爆。
    /// 返回被裁掉的数量 —— 层那边要同步把 Contents 里对应的记录也删掉，
    /// 否则 bars 和 Contents 会对不上，回撤的增量删除就会删错条目。
    /// </summary>
    private int TrimOverflow()
    {
        if (maxBars <= 0) return 0;

        int trimmed = 0;
        while (bars.Count > maxBars)
        {
            var oldest = bars[0];
            bars.RemoveAt(0);
            if (oldest != null) Destroy(oldest.gameObject);
            trimmed++;
        }
        return trimmed;
    }

    // ==================== 动画 / 滚动 ====================

    /// <summary>让所有正在播的弹出动画立刻结束（对应 Skip()）。</summary>
    public void CompleteAnimations()
    {
        for (int i = 0; i < bars.Count; i++)
        {
            bars[i]?.CompleteShowAnimation();
        }
    }

    /// <summary>滚到底部。</summary>
    public void ScrollToBottom()
    {
        if (scrollRect == null) return;
        // 新条目的高度要等布局组重建后才算得出来，先强制刷一次再设位置
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
