using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace LHGFData 
{
    /// <summary>
    /// 手机聊天层。PhoneData / SaveData 都嵌套在这里，
    /// 和 BlackboardLayer.BlackboardData、LongDialogLayer.LongSpeakData、LHGFWebViewLayer.WebViewData 一致
    /// （外部引用写成 PhoneChatLayer.PhoneData）。
    ///
    /// PhoneState 同黑板的 BlackboardState，Continue 同 LongSpeak 的 Continue。
    /// 用 ContentType 而不是 TextType：黑板那个是"文字/图片"二选一，手机有 6 种内容。
    /// </summary>
    public class PhoneChatLayer : ILayer
    {
        /// <summary>
        /// 一条手机聊天命令的数据（一次 Next 的输入）。
        /// PhoneData 是从 LayerCommand 解析出来的原始命令数据，参与存档，全是可序列化的 string/bool/float；
        /// 交给 TalkBar 渲染的 TalkContentData 由控制器从它转换而来，两者不合并
        /// （TalkContentData 为了兼容运行时调用保留了 Sprite 字段，Sprite 进不了 JSON）。
        /// </summary>
        [Serializable]
        public class PhoneData
        {
            public bool PhoneState = true;      // 显示/隐藏手机（对应黑板的 BlackboardState）
            public bool Continue = true;        // true 续写，false 先清空聊天框（对应 LongSpeak 的 Continue）
            public string TalkerName = null;    // 聊天对象名字；空 = 不改变
            public TalkContentKind ContentType = TalkContentKind.TmpText;
            public string Content = null;       // 正文/时间戳/图片路径/动图路径/公式；File 类型时是文件名；null 或空 = 没有消息，只做控制
            public bool RightSide = false;      // true 自己发的（靠右），false 对方（靠左）
            public bool Skip = false;           // 跳过弹出动画；不影响动图加载，Finish() 依然看加载状态
            public string AvatarPath = null;    // 头像路径；空 = 不改变
            // 模拟对方输入
            public float TypingTime = 0f;       // 气泡出现前的"正在输入"等待秒数；0 = 不等待
            public string TypingText = null;    // 等待期间 TalkerState 显示的文字；空 = 用默认"对方正在输入"
            // 下面是分类型专属
            public string Icon = null;          // File：图标路径
            public string FileSize = null;      // File：大小文字
            public float FontSize = 0f;         // TexDraw：字号，0 = 不改

            public PhoneData() { }

            /// <summary>
            /// 从一条命令解析。refdata 是上一次的数据，用来继承没写的字段
            /// （和黑板 BlackboardData(command, refdata) 同一套路）。
            /// 取值用 DictionaryExtensions（LHGFScriptData.cs 末尾）；
            /// 它的 GetString 默认值是 ""，想区分"没写"就得显式传 null。
            /// </summary>
            public PhoneData(LayerCommand command, PhoneData refdata)
            {
                var dic = command?.CommandConfig;
                if (dic == null) return;

                // 显隐和清空是每次命令的独立动作，不从 refdata 继承
                PhoneState = dic.GetBool("PhoneState", true);
                Continue   = dic.GetBool("Continue", true);

                // 空 = 不改变，所以继承上一次
                TalkerName = dic.GetString("TalkerName", null);
                if (string.IsNullOrEmpty(TalkerName) && refdata != null)
                    TalkerName = refdata.TalkerName;

                Content = dic.GetString("Content", null);   // null = 没有消息，只做控制

                // 缺失或认不出 → 默认 TMP
                string typeStr = dic.GetString("ContentType", null);
                ContentType = (!string.IsNullOrEmpty(typeStr) && TryParseKind(typeStr, out var kind))
                    ? kind
                    : TalkContentKind.TmpText;

                RightSide = dic.GetBool("RightSide", false);
                Skip      = dic.GetBool("Skip", false);

                AvatarPath = dic.GetString("AvatarPath", null);
                if (string.IsNullOrEmpty(AvatarPath) && refdata != null)
                    AvatarPath = refdata.AvatarPath;

                // 每次命令的独立动作，不从 refdata 继承
                TypingTime = dic.GetFloat("TypingTime", 0f);
                TypingText = dic.GetString("TypingText", null);

                Icon     = dic.GetString("Icon", null);
                FileSize = dic.GetString("FileSize", null);
                FontSize = dic.GetFloat("FontSize", 0f);
            }

            /// <summary>这条命令是否带消息（Content 非空）。false 表示只做控制。</summary>
            public bool HasMessage => !string.IsNullOrEmpty(Content);

            /// <summary>是否需要先等一段"对方正在输入"。Content 为空时也照样等 —— 用来演"打了字又没发"。</summary>
            public bool HasTyping => TypingTime > 0f;

            /// <summary>
            /// ContentType 字符串 → 枚举，大小写不敏感：
            /// TMP / TmpText / TexDraw / File / TimeStamp / Image / Gif
            /// </summary>
            public static bool TryParseKind(string s, out TalkContentKind kind)
            {
                kind = TalkContentKind.TmpText;
                if (string.IsNullOrEmpty(s)) return false;

                switch (s.Trim().ToLowerInvariant())
                {
                    case "tmp":
                    case "text":
                    case "tmptext":   kind = TalkContentKind.TmpText;   return true;
                    case "texdraw":
                    case "tex":       kind = TalkContentKind.TexDraw;   return true;
                    case "file":      kind = TalkContentKind.File;      return true;
                    case "timestamp":
                    case "time":      kind = TalkContentKind.TimeStamp; return true;
                    case "image":
                    case "img":       kind = TalkContentKind.Image;     return true;
                    case "gif":       kind = TalkContentKind.Gif;       return true;
                    default:          return false;
                }
            }
        }

        /// <summary>
        /// 存档数据。Save() 返回、Load() 接收，
        /// 框架会把它塞进 GameSaveData.layerDatas[层名] 一起 JSON 序列化。
        /// </summary>
        [Serializable]
        public class SaveData
        {
            public List<List<PhoneData>> Contents = new List<List<PhoneData>>();         // 按节点分组
            public List<List<PhoneData>> HistoryContents = new List<List<PhoneData>>();  // 回撤用的历史快照
            public string TalkerName = null;    // 不存的话读档后顶部名字会丢
            public bool PhoneState = false;
        }

        // ILayer 实现

        /// <summary>表现层控制器。由 LHGFGameMain.SetLayers 注入。</summary>
        public LHGFChatLayerControler PhoneControler;

        /// <summary>按节点分组的消息内容。Continue=false 时新起一组（和黑板同构）。</summary>
        public List<List<PhoneData>> Contents = new List<List<PhoneData>>();

        /// <summary>回撤用的历史快照，每个节点一组。</summary>
        public List<List<PhoneData>> HistoryContents = new List<List<PhoneData>>();

        /// <summary>当前聊天对象名字。空 = 还没有名字。</summary>
        public string TalkerName = null;

        /// <summary>手机当前是否显示。</summary>
        public bool PhoneState = false;

        /// <summary>是否本节点的第一条命令（BeforeNextStart 置 true，Next 里用完置 false）。</summary>
        public bool IsNewNodeStart = true;

        /// <summary>上一次的数据，用来继承没写的字段（名字、头像）。不参与存档，可由 Contents 推出。</summary>
        public PhoneData currentData = null;

        public string LayerName()
        {
            return "PhoneChatLayer";
        }

        /// <summary>
        /// 返回手机根节点（Phone.prefab 实例）。
        /// 注意：目前 LayerShower 里没有 PhoneChat 分支，不会对它 SetActive；
        /// 若将来加了分支，SetActive(false) 会绕过 PhoneState，要想清楚。
        /// </summary>
        public GameObject GetControler()
        {
            return PhoneControler != null ? PhoneControler.phoneRoot : null;
        }

        public void OnStart()
        {
            PhoneState = false;
            PhoneControler?.CancelTyping();
            PhoneControler?.Hide();
        }

        /// <summary>驱动"对方正在输入"的计时。计时走完由控制器负责让气泡显形。</summary>
        public void OnUpdate()
        {
            PhoneControler?.UpdateTyping();
        }

        public void OnLoadFinish() { }

        /// <summary>节点切换钩子：标记本节点第一条命令，HistoryContents 要新起一组。</summary>
        public void BeforeNextStart()
        {
            IsNewNodeStart = true;
        }

        public void BeforeNextOnLoadStart()
        {
            IsNewNodeStart = true;
        }

        /// <summary>手机层不进对话历史。</summary>
        public Dictionary<string, string> Log(LayerCommand command)
        {
            return null;
        }

        /// <summary>
        /// 处理一条 PhoneChat 命令：显隐 → 清空 → 改名 → 加消息 → 记录分组。
        /// 控制器可能还没注入，所以一律判空。
        /// </summary>
        public void Next(LayerCommand command)
        {
            if (command == null) return;

            PhoneData data = new PhoneData(command, currentData);

            PhoneState = data.PhoneState;
            if (PhoneState) PhoneControler?.Show();
            else PhoneControler?.Hide();

            // 清空必须在加消息之前
            if (!data.Continue) PhoneControler?.ClearAll();

            if (!string.IsNullOrEmpty(data.TalkerName))
            {
                TalkerName = data.TalkerName;
                PhoneControler?.SetTalkerName(TalkerName);
            }

            // 控制器返回"新增数 - 被 maxBars 裁掉数"。
            // 裁剪删的是最旧的那几条，所以两个列表最前面的记录都要跟着删，
            // 否则 bars 和 Contents 会对不上（回撤时会删错条目）。
            //
            // 有 TypingTime 时先不显示：气泡建好但 alpha 0 藏着，
            // 排版/加载可以提前做，等控制器计时结束再显形（alpha 直接跳 1 + 播弹出动画）。
            int delta = data.HasMessage
                ? (PhoneControler?.AddMessage(data, hidden: data.HasTyping) ?? 0)
                : 0;

            // 起"对方正在输入"的计时。Content 为空也要起 —— 用来演"打了字又没发"。
            // bar 直接传引用（AddMessage 刚建出来的那条），等完精确显形这一条；
            // 没消息就传 null，等完什么都不发。
            if (data.HasTyping)
            {
                var bar = PhoneControler == null ? null : PhoneControler.LastCreatedBar;
                PhoneControler?.BeginTyping(data.TypingTime,
                                            data.HasMessage ? bar : null,
                                            data.Skip,
                                            data.TypingText);
            }

            // 分组记录（和黑板一致）
            if (!data.Continue || Contents.Count < 1)
                Contents.Add(new List<PhoneData>() { data });
            else
                Contents[Contents.Count - 1].Add(data);

            if (IsNewNodeStart)
            {
                HistoryContents.Add(new List<PhoneData>() { data });
                IsNewNodeStart = false;
            }
            else
            {
                HistoryContents[HistoryContents.Count - 1].Add(data);
            }

            // delta < 1 说明要么没加成、要么被裁了。
            // 两个列表都要从最旧的开始对齐，否则 HistoryContents 会比 Contents 长，
            // 回撤次数按节点命令数算的时候就会错位。
            for (int i = 0; i < 1 - delta; i++)
            {
                PopFirst(Contents);
                PopFirst(HistoryContents);
            }

            currentData = data;
        }

        /// <summary>
        /// 删掉最旧的一条【有消息】的记录（maxBars 裁剪时用来对齐 bars 和 Contents）。
        ///
        /// 只删有消息的那条：控制命令（Content 为空，如"清空/显隐"）不占 bar，
        /// 删了它并不会让界面少一个气泡，反而会把对齐错开一位。
        /// </summary>
        private static void PopFirst(List<List<PhoneData>> list)
        {
            if (list == null || list.Count == 0) return;

            // 从头找到第一条真正产生过 bar 的记录
            for (int g = 0; g < list.Count; g++)
            {
                var group = list[g];
                for (int i = 0; i < group.Count; i++)
                {
                    if (group[i] == null || !group[i].HasMessage) continue;

                    group.RemoveAt(i);
                    if (group.Count == 0) list.RemoveAt(g);
                    return;
                }
            }
        }

        /// <summary>
        /// 动画播完 **且** 动图加载完 **且** 不在"对方正在输入"的等待中，才算完成。
        /// 没有控制器时不阻塞（否则开局即被卡死）。
        /// </summary>
        public bool Finish()
        {
            if (PhoneControler == null) return true;
            return !PhoneControler.IsAnimating
                   && !PhoneControler.IsLoading
                   && !PhoneControler.IsTyping;
        }

        /// <summary>
        /// 跳过：让弹出动画立刻结束，并结束"对方正在输入"的等待（气泡马上出现）。
        /// 动图该等还是要等 —— Finish() 依然看加载状态。
        /// </summary>
        public void Skip()
        {
            PhoneControler?.CompleteAnimations();
            PhoneControler?.FinishTypingNow();
        }

        /// <summary>
        /// 回撤一条命令。框架按被回撤节点的命令条数逐条调用（和 WebViewLayer 一致）。
        ///
        /// ⚠ 两个列表都要弹【一条元素】，不能弹【一个组】：
        ///   Contents 按 Continue 分组、HistoryContents 按节点分组，两者组数不同，
        ///   所以只能保证"元素总数"同步。之前 HistoryContents 用 RemoveAt(Count-1) 一次删掉整组，
        ///   调用次数一多就先把 History 掏空、后续 Withdraw 直接 return，Contents 便撤不干净。
        ///
        /// 【增量还是重建】和黑板一样分两种情况：
        ///   · 撤掉这条后最后一组还在 → 只需删掉最后一个 bar（常见，便宜，不会重建图片）
        ///   · 撤掉这条后整组消失（跨过 Continue=0 的边界）→ 得清空重建（少见）
        /// </summary>
        public bool Withdraw()
        {
            if (HistoryContents.Count == 0) return false;

            // 先判断最后一组撤掉这条后会不会消失。必须在 PopLast 之前算。
            bool groupWillVanish = Contents.Count == 0
                                   || Contents[Contents.Count - 1].Count <= 1;

            PopLast(HistoryContents);
            PopLast(Contents);

            // 回撤时把还没走完的"输入中"一并取消，否则状态文字会留在屏幕上
            PhoneControler?.CancelTyping();

            currentData = FindLastData();
            SyncStateFromContents();

            if (groupWillVanish)
            {
                // 整组没了：屏幕上的内容要换成更早那一组的，只能重建
                Rebuild();
            }
            else
            {
                // 只少了一条：删掉最后一个 bar 就够，不碰其它（图片/动图不会被重新加载）
                PhoneControler?.RemoveLastBar();
                ApplyStateToControler();
            }
            return true;
        }

        /// <summary>把 TalkerName / PhoneState 落到控制器上（不重建内容）。</summary>
        private void ApplyStateToControler()
        {
            if (PhoneControler == null) return;

            if (!string.IsNullOrEmpty(TalkerName)) PhoneControler.SetTalkerName(TalkerName);
            if (PhoneState) PhoneControler.Show(); else PhoneControler.Hide();
        }

        /// <summary>弹掉最后一个组里的最后一条；组因此空了就把组也移除。</summary>
        private static void PopLast(List<List<PhoneData>> list)
        {
            if (list == null || list.Count == 0) return;

            var last = list[list.Count - 1];
            if (last.Count > 0) last.RemoveAt(last.Count - 1);
            if (last.Count == 0) list.RemoveAt(list.Count - 1);
        }

        private PhoneData FindLastData()
        {
            if (Contents.Count == 0) return null;
            var last = Contents[Contents.Count - 1];
            return last.Count > 0 ? last[last.Count - 1] : null;
        }

        /// <summary>
        /// 把 TalkerName / PhoneState 回退成"剩下这些数据"所表达的状态。
        ///
        /// 为什么要这样做：这两个字段是层的当前状态，Next() 会更新它们，但回撤时如果不管，
        /// 它们会停在"被撤掉的那条命令"的样子 —— 例如改名命令被撤了，顶部名字却还是新的。
        /// Contents 里每条 PhoneData 都完整记录了当时的命令，所以从头扫一遍就能还原。
        /// </summary>
        private void SyncStateFromContents()
        {
            string name = null;
            bool state = false;

            foreach (var group in Contents)
            {
                foreach (var item in group)
                {
                    if (item == null) continue;
                    if (!string.IsNullOrEmpty(item.TalkerName)) name = item.TalkerName;
                    state = item.PhoneState;
                }
            }

            TalkerName = name;
            PhoneState = state;
        }

        public void Reset()
        {
            Contents.Clear();
            HistoryContents.Clear();
            currentData = null;
            TalkerName = null;
            PhoneState = false;
            IsNewNodeStart = true;
            PhoneControler?.CancelTyping();
            PhoneControler?.ResetAll();
            PhoneControler?.Hide();
        }

        /// <summary>
        /// 按当前 Contents 重建整个聊天框（读档 / 回撤后用）。
        /// 不重放动画，所以每条都按 skip 处理。
        ///
        /// 从头把所有组都重放一遍，而不是只建最后一组：
        /// 回撤可能跨过 Continue=0 的边界（某次清空之后一条消息都没剩），
        /// 这时"最后一组"已经不存在，只看最后一组会错误地显示出更早的内容。
        /// 重放时按 Continue 清空，最终屏幕上留下的就是正确的那批。
        /// </summary>
        private void Rebuild()
        {
            if (PhoneControler == null) return;

            PhoneControler.ResetAll();

            // ★ 先让手机可见再建 bar。
            //   bar 是 Instantiate 到 content 下的，如果此刻 Phone 还是关着的，
            //   新建出来的 bar 也是 inactive —— 头像/图片的加载协程在 inactive 的
            //   GameObject 上起不来，会报 "Coroutine couldn't be started"。
            //   建完再按真实状态收尾（该关就关）。
            PhoneControler.Show();

            for (int g = 0; g < Contents.Count; g++)
            {
                var group = Contents[g];
                for (int i = 0; i < group.Count; i++)
                {
                    var item = group[i];
                    if (item == null) continue;

                    // Continue=false 表示这条之前先清空，重放时要还原这个动作
                    if (!item.Continue) PhoneControler.ClearAll();
                    if (item.HasMessage) PhoneControler.AddMessage(item, forceSkip: true);
                }
            }

            ApplyStateToControler();
        }

        public void Load(object saveData)
        {
            if (saveData == null) return;
            SaveData data;
            data = saveData as SaveData;
            if (data == null)
            {
                // 存档反序列化回来是 JObject，需要 JSON 往返一次（项目里的既定做法）
                string json = JsonConvert.SerializeObject(saveData);
                data = JsonConvert.DeserializeObject<SaveData>(json);
            }
            if (data == null) return;

            // 清空当前内容
            Contents.Clear();
            HistoryContents.Clear();

            // 恢复数据（深拷贝外层和内层列表）
            if (data.Contents != null)
            {
                foreach (var innerList in data.Contents)
                {
                    Contents.Add(new List<PhoneData>(innerList));
                }
            }
            if (data.HistoryContents != null)
            {
                foreach (var innerList in data.HistoryContents)
                {
                    HistoryContents.Add(new List<PhoneData>(innerList));
                }
            }

            TalkerName = data.TalkerName;
            PhoneState = data.PhoneState;
            currentData = FindLastData();

            // 恢复显示（读档后当前节点的命令不会重放，得自己重建）
            Rebuild();
        }

        public object Save()
        {
            var saveData = new SaveData();

            // 深拷贝外层和内层列表（和黑板一致，避免存档与运行期共享引用）
            if (Contents != null)
            {
                saveData.Contents = new List<List<PhoneData>>();
                foreach (var innerList in Contents)
                {
                    saveData.Contents.Add(new List<PhoneData>(innerList));
                }
            }
            if (HistoryContents != null)
            {
                saveData.HistoryContents = new List<List<PhoneData>>();
                foreach (var innerList in HistoryContents)
                {
                    saveData.HistoryContents.Add(new List<PhoneData>(innerList));
                }
            }

            saveData.TalkerName = TalkerName;
            saveData.PhoneState = PhoneState;
            return saveData;
        }
    }
}
