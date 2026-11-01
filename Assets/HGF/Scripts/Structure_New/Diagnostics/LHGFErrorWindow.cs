using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace LHGFDiagnostics
{
    /// <summary>
    /// 错误窗口：显示报错信息，暂停游戏，提供「复制 / 忽略并继续 / 退出游戏」。
    ///
    /// 两种构建方式：
    ///   1. 预制体（推荐）：场景中放置或放到 Resources，Inspector 里绑定字段；
    ///   2. 兜底：预制体不存在时调用 CreateFallback() 纯代码构建最小可用界面。
    ///
    /// 暂停策略（立场 2）：Time.timeScale = 0 + AudioListener.pause + 暂停 VideoPlayer，
    /// 同时对外暴露 IsOpen，由 LHGFGameMain.Update 的 gate 冻结主循环。
    /// </summary>
    public class LHGFErrorWindow : MonoBehaviour
    {
        [Header("预制体字段（兜底 UI 不需要绑定）")]
        [Tooltip("标题文本，例如“游戏发生了错误”")]
        public TMP_Text titleText;
        [Tooltip("节点/剧本等上下文信息")]
        public TMP_Text contextText;
        [Tooltip("错误正文（condition + stack）")]
        public TMP_Text logText;
        [Tooltip("滚动视图根节点，用于自动滚动/重建")]
        public ScrollRect logScrollRect;
        [Tooltip("复制错误信息")]
        public Button copyButton;
        [Tooltip("忽略并继续（恢复游戏）")]
        public Button ignoreButton;
        [Tooltip("退出游戏")]
        public Button quitButton;
        [Tooltip("被抑制的错误数量提示（弹窗打开期间新增的错误）")]
        public TMP_Text suppressedHint;

        /// <summary>窗口是否正在显示。游戏主循环据此冻结。</summary>
        public bool IsOpen { get; private set; }

        // ---- 暂停前的现场，恢复时原样还原 ----
        private float savedTimeScale = 1f;
        private bool savedAudioPause;
        private bool savedTimeScaleCaptured;
        private readonly List<VideoPlayer> pausedVideos = new List<VideoPlayer>();

        private string lastFullLog = string.Empty;
        private int suppressed = 0;
        private bool boundCopy, boundIgnore, boundQuit;
        private bool bindLogged;

        // ================================================================ 生命周期

        private void Awake()
        {
            // 场景中预先放置的弹窗对象通常是 SetActive(false) 状态，Awake 不会执行。
            // 注意：CreateFallback() 里 AddComponent 会立刻触发 Awake，而此时按钮字段还没赋值，
            // 所以这里不能依赖 listenersBound 判重，改成按按钮实例逐个判重（见 BindListeners）。
            if (suppressedHint != null) suppressedHint.gameObject.SetActive(false);
        }

        /// <summary>
        /// 幂等绑定按钮。按「每个按钮是否已绑定」判重，而不是一个全局 bool——
        /// 因为 CreateFallback 会在字段赋值前先触发一次 Awake。
        /// </summary>
        private void BindListeners()
        {
            if (copyButton != null && !boundCopy)
            {
                boundCopy = true;
                copyButton.onClick.AddListener(CopyToClipboard);
            }
            if (ignoreButton != null && !boundIgnore)
            {
                boundIgnore = true;
                ignoreButton.onClick.AddListener(OnIgnoreClicked);
            }
            if (quitButton != null && !boundQuit)
            {
                boundQuit = true;
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            // 首次绑定后打一条诊断日志，方便确认绑定是否真的发生
            if (!bindLogged)
            {
                bindLogged = true;
                Debug.Log($"[LHGFErrorWindow] BindListeners: copy={(copyButton != null)} ignore={(ignoreButton != null)} quit={(quitButton != null)} " +
                          $"| bound copy={boundCopy} ignore={boundIgnore} quit={boundQuit}");
            }
        }

        private void OnDestroy()
        {
            if (copyButton != null) copyButton.onClick.RemoveListener(CopyToClipboard);
            if (ignoreButton != null) ignoreButton.onClick.RemoveListener(OnIgnoreClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);

            // 被销毁时务必还原，避免 timeScale 卡在 0
            if (IsOpen) Restore();
        }        // ================================================================ 显示 / 关闭

        /// <summary>由 LHGFErrorGuard 调用。</summary>
        public void Show(string condition, string stackTrace, LogType type, int totalErrors, string context)
        {
            if (IsOpen) return;

            IsOpen = true;
            suppressed = 0;

            // 对象此前可能处于非激活状态（Awake 未执行），先激活再补绑按钮
            gameObject.SetActive(true);
            BindListeners();

            lastFullLog = BuildBody(condition, stackTrace, type);

            if (titleText != null)
            {
                titleText.text = "Runtime Error";
            }
            if (contextText != null)
            {
                string ctx = string.IsNullOrEmpty(context) ? "" : context;
                contextText.text = $"{ctx}\nTotal errors this session: {totalErrors}";
                contextText.gameObject.SetActive(true);
            }
            if (logText != null)
            {
                logText.richText = false;   // 防止堆栈里的 '<' 被当成富文本吞掉
                logText.enableWordWrapping = true; // 防止长堆栈不换行撑出屏幕
                logText.text = lastFullLog;
            }
            if (suppressedHint != null) suppressedHint.gameObject.SetActive(false);

            transform.SetAsLastSibling();

            Pause();

            // 布局刷新后再滚到顶部，让玩家先看到错误类型
            Canvas.ForceUpdateCanvases();
            if (logScrollRect != null) logScrollRect.verticalNormalizedPosition = 1f;
        }

        /// <summary>弹窗打开期间又有错误到来时，只更新“还有 N 条”的计数提示。</summary>
        public void SetSuppressedHint(int count)
        {
            suppressed = count;
            if (suppressedHint != null)
            {
                suppressedHint.gameObject.SetActive(true);
                suppressedHint.text = $"+{count} more error(s) suppressed (already saved to log)";
            }
        }

        private void OnIgnoreClicked()
        {
            Restore();
            gameObject.SetActive(false);
        }

        private void OnQuitClicked()
        {
            Restore();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void CopyToClipboard()
        {
            string text = lastFullLog;
            if (suppressed > 0) text += $"\n\n[{suppressed} more error(s) suppressed, see log file]";
            text += $"\n\nLog file: {LHGFErrorGuard.LogFilePath}";
            GUIUtility.systemCopyBuffer = text;
        }

        // ================================================================ 暂停 / 恢复

        private void Pause()
        {
            if (!savedTimeScaleCaptured)
            {
                savedTimeScale = Time.timeScale;
                savedAudioPause = AudioListener.pause;
                savedTimeScaleCaptured = true;
            }

            Time.timeScale = 0f;
            AudioListener.pause = true;

            // 视频播放器不受 timeScale 影响，需显式暂停；只记录原本在播的，恢复时不能误播
            pausedVideos.Clear();
            try
            {
                var players = FindObjectsOfType<VideoPlayer>();
                for (int i = 0; i < players.Length; i++)
                {
                    var v = players[i];
                    if (v == null) continue;
                    if (v.isPlaying)
                    {
                        pausedVideos.Add(v);
                        v.Pause();
                    }
                }
            }
            catch (Exception)
            {
                // 暂停视频失败不影响主流程
            }
        }

        private void Restore()
        {
            if (!savedTimeScaleCaptured)
            {
                IsOpen = false;
                return;
            }

            Time.timeScale = savedTimeScale;
            AudioListener.pause = savedAudioPause;

            for (int i = 0; i < pausedVideos.Count; i++)
            {
                var v = pausedVideos[i];
                if (v != null) v.Play();
            }
            pausedVideos.Clear();

            savedTimeScaleCaptured = false;
            IsOpen = false;
        }

        // ================================================================ 文本

        private static string BuildBody(string condition, string stackTrace, LogType type)
        {
            var sb = new StringBuilder();
            sb.Append('[').Append(type).Append("] ");
            sb.Append(condition);
            if (!string.IsNullOrEmpty(stackTrace))
            {
                sb.Append("\n\n");
                sb.Append(stackTrace.TrimEnd());
            }
            return sb.ToString();
        }

        // ================================================================ 兜底 UI

        /// <summary>
        /// 预制体缺失时，用代码构建一个最小可用的错误窗口。
        /// 不做样式，只保证：能看到文本、能点忽略、能退出。
        /// </summary>
        public static LHGFErrorWindow CreateFallback()
        {
            var root = new GameObject("LHGFErrorWindow");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;   // 盖住所有游戏 UI

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            // UGUI 点击检测依赖 EventSystem，空场景里通常没有。
            // 缺了它按钮看得见却点不动，所以这里必须兜底创建。
            EnsureEventSystem();

            var window = root.AddComponent<LHGFErrorWindow>();

            // ---- 遮罩：挡住游戏输入 ----
            var blocker = CreateImage(root.transform, "Blocker", new Color(0f, 0f, 0f, 0.85f));
            Stretch(blocker.rectTransform);
            blocker.raycastTarget = true;

            // ---- 面板 ----
            var panel = CreateImage(root.transform, "Panel", new Color(0.12f, 0.12f, 0.14f, 1f));
            var prt = panel.rectTransform;
            prt.anchorMin = new Vector2(0.1f, 0.1f);
            prt.anchorMax = new Vector2(0.9f, 0.9f);
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;

            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 20, 20);
            layout.spacing = 12f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            // ---- 标题（英文：兜底 UI 不依赖中文字体，保证任何环境都能看清按钮） ----
            var title = CreateText(panel.transform, "TitleText", "Runtime Error", 40f, FontStyles.Bold);
            title.color = new Color(1f, 0.45f, 0.4f, 1f);
            window.titleText = title;
            var titleLe = title.gameObject.AddComponent<LayoutElement>();
            titleLe.minHeight = 56f;
            titleLe.preferredHeight = 56f;

            // ---- 上下文 ----
            var ctx = CreateText(panel.transform, "ContextText", "", 24f, FontStyles.Normal);
            ctx.color = new Color(0.75f, 0.78f, 0.85f, 1f);
            window.contextText = ctx;
            var ctxLe = ctx.gameObject.AddComponent<LayoutElement>();
            ctxLe.minHeight = 40f;
            ctxLe.preferredHeight = 40f;

            // ---- 日志区（ScrollRect） ----
            var scrollGo = new GameObject("LogArea", typeof(RectTransform));
            scrollGo.transform.SetParent(panel.transform, false);
            var scroll = scrollGo.AddComponent<ScrollRect>();
            var scrollLe = scrollGo.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1f;
            scrollLe.minHeight = 200f;

            var viewport = CreateImage(scrollGo.transform, "Viewport", new Color(0f, 0f, 0f, 0.35f));
            Stretch(viewport.rectTransform);
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var log = content.AddComponent<TextMeshProUGUI>();
            log.fontSize = 22f;
            log.richText = false;
            log.enableWordWrapping = true;
            log.color = new Color(0.92f, 0.92f, 0.95f, 1f);
            log.margin = new Vector4(16f, 16f, 16f, 16f);
            window.logText = log;

            scroll.viewport = viewport.rectTransform;
            scroll.content = contentRt;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            window.logScrollRect = scroll;

            // ---- 按钮行 ----
            var row = new GameObject("ButtonRow", typeof(RectTransform));
            row.transform.SetParent(panel.transform, false);
            var hLayout = row.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 12f;
            hLayout.childAlignment = TextAnchor.MiddleRight;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;   // 高度撑满，避免被 VerticalLayoutGroup 压成 0
            var rowLe = row.AddComponent<LayoutElement>();
            rowLe.minHeight = 64f;
            rowLe.preferredHeight = 64f;
            rowLe.flexibleHeight = 0f;               // 不参与纵向弹性分配，固定 64

            window.copyButton = CreateButton(row.transform, "CopyButton", "Copy");
            window.ignoreButton = CreateButton(row.transform, "IgnoreButton", "Ignore & Continue");
            window.quitButton = CreateButton(row.transform, "QuitButton", "Quit");

            // ---- 抑制提示 ----
            var hint = CreateText(panel.transform, "SuppressedHint", "", 20f, FontStyles.Italic);
            hint.color = new Color(1f, 0.8f, 0.4f, 1f);
            window.suppressedHint = hint;
            hint.gameObject.SetActive(false);

            return window;
        }

        /// <summary>
        /// 确保场景中存在 EventSystem（UGUI 点击检测的前提）。
        /// 空场景里通常没有 EventSystem，缺了它按钮会显示却点不动。
        /// </summary>
        private static void EnsureEventSystem()
        {
            // 注意不能只用 EventSystem.current 判断：它在 OnEnable 之前可能还是 null。
            // 直接查找已存在的组件更可靠（包含非激活对象）。
#if UNITY_2023_1_OR_NEWER
            var existing = UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include);
#else
            var existing = UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>(true);
#endif
            if (existing != null) return;

            var esGo = new GameObject("LHGFErrorEventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // 本项目 activeInputHandler=0（旧输入系统），用 StandaloneInputModule 即可。
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            UnityEngine.Object.DontDestroyOnLoad(esGo);
        }

        private static Image CreateImage(Transform parent, string name, Color color) { 
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string content, float size, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.richText = false;
            t.enableWordWrapping = true;
            return t;
        }

        private static Button CreateButton(Transform parent, string name, string label)
        {
            var btnImg = CreateImage(parent, name, new Color(0.25f, 0.27f, 0.32f, 1f));
            var btn = btnImg.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.35f, 0.38f, 0.45f, 1f);
            colors.pressedColor = new Color(0.18f, 0.2f, 0.24f, 1f);
            btn.colors = colors;

            var text = CreateText(btnImg.transform, "Label", label, 24f, FontStyles.Normal);
            text.alignment = TextAlignmentOptions.Center;
            text.richText = false;
            Stretch(text.rectTransform);

            var le = btnImg.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 200f;
            le.preferredWidth = 220f;
            le.minHeight = 56f;
            le.preferredHeight = 56f;
            return btn;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
