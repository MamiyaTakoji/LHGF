using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Video;

namespace LHGFDiagnostics
{
    /// <summary>
    /// 运行时错误守卫：捕获 Error / Exception / Assert，写入根目录日志，并弹出错误窗口。
    /// 本类与游戏逻辑无耦合，只向外暴露一个只读状态 IsOpen 供主循环 gate 使用。
    ///
    /// 启动时机：
    ///   BeforeSceneLoad  —— 只注册监听并缓存错误（此时场景尚未创建，不能建 UI）
    ///   AfterSceneLoad   —— 创建宿主、冲刷早期错误、挂上弹窗
    /// </summary>
    public static class LHGFErrorGuard
    {
        /// <summary>错误窗口是否正在显示。游戏主循环应在此为 true 时跳过本帧推进。</summary>
        public static bool IsOpen => window != null && window.IsOpen;

        /// <summary>是否启用错误提示（可由 DebugConfig.json 的 ShowErrorWindow 关闭）。</summary>
        public static bool Enabled = true;

        /// <summary>本次运行累计捕获的错误条数（含弹窗打开期间被抑制的）。</summary>
        public static int TotalErrors => totalErrors;
        private static int totalErrors;

        // ---- 内部状态 ----
        private static LHGFErrorWindow window;
        private static bool hooked;
        private static bool ready;   // AfterSceneLoad 之后为 true，才允许弹 UI
        private static float startTime;

        // 弹窗打开期间到来的错误只计数、不弹窗，避免刷屏；关闭时把这些数字一并写进日志。
        private static int suppressedCount;

        // 早期错误缓存：BeforeSceneLoad 到 AfterSceneLoad 之间无法弹窗，先存下来。
        private struct PendingError
        {
            public string Condition;
            public string Stack;
            public LogType Type;
        }
        private static readonly List<PendingError> pending = new List<PendingError>();

        // ---------------------------------------------------------------- 启动

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            startTime = Time.realtimeSinceStartup;
            Hook();
            Application.quitting += OnApplicationQuitting;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            ready = true;
            EnsureHost();
            FlushPending();
        }

        private static void Hook()
        {
            if (hooked) return;
            hooked = true;
            Application.logMessageReceived += OnLogMessage;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private static void Unhook()
        {
            if (!hooked) return;
            hooked = false;
            Application.logMessageReceived -= OnLogMessage;
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        }

        private static void OnApplicationQuitting()
        {
            Unhook();
        }

        // ---------------------------------------------------------------- 事件处理

        private static void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;

            HandleError(condition, stackTrace, type);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // 注意：这是非主线程回调，不能碰 UI / 不能碰 Unity API。
            // 只把内容写进日志（文件 IO 是线程安全的），UI 部分交给主线程的下一帧。
            Exception ex = e.ExceptionObject as Exception;
            string text = ex != null ? ex.ToString() : (e.ExceptionObject?.ToString() ?? "<unknown>");
            string condition = "[UnhandledException (non-main thread)] " + (ex?.Message ?? text);
            AppendToFile(condition, ex?.StackTrace ?? text, LogType.Exception);
        }

        private static void OnUnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            AppendToFile("[UnobservedTaskException]", ex != null ? ex.ToString() : "<unknown>", LogType.Error);
            e.SetObserved();
        }

        private static void HandleError(string condition, string stackTrace, LogType type)
        {
            totalErrors++;

            // 1) 日志永远写全（不受弹窗状态影响）
            AppendToFile(condition, stackTrace, type);

            // 2) 场景还没准备好（早期启动），先缓存，等 AfterSceneLoad 再处理
            if (!ready)
            {
                pending.Add(new PendingError { Condition = condition, Stack = stackTrace, Type = type });
                return;
            }

            // 3) 弹窗打开期间：只计数，不再弹、不叠文本
            if (window != null && window.IsOpen)
            {
                suppressedCount++;
                window.SetSuppressedHint(suppressedCount);
                return;
            }

            // 4) 弹窗关闭状态：正常弹出
            Show(condition, stackTrace, type);
        }

        private static void FlushPending()
        {
            if (pending.Count == 0) return;
            var first = pending[0];
            pending.Clear();
            // 只弹最早那一条（其余都已写进日志），符合“一次只显示一个错误”的语义
            if (Enabled) Show(first.Condition, first.Stack, first.Type);
        }

        private static void Show(string condition, string stackTrace, LogType type)
        {
            if (!Enabled) return;

            EnsureHost();
            if (window == null) return;

            if (window.IsOpen)
            {
                // 理论上不会走到这里，双保险
                suppressedCount++;
                window.SetSuppressedHint(suppressedCount);
                return;
            }

            suppressedCount = 0;
            try
            {
                window.Show(condition, stackTrace, type, totalErrors, BuildContext());
            }
            catch (Exception ex)
            {
                // 弹窗自身出错时不能递归进 HandleError，直接落到日志
                AppendToFile("[LHGFErrorWindow internal error]", ex.ToString(), LogType.Error);
            }
        }

        // ---------------------------------------------------------------- 宿主

        /// <summary>
        /// 确保存在一个 DontDestroyOnLoad 的弹窗宿主。
        /// 优先在场景/Resources 中查找预制体，找不到时退化为代码构建的兜底 UI。
        /// </summary>
        private static void EnsureHost()
        {
            if (window != null) return;
            if (!ready) return;

            try
            {
                // 1) 场景中已放置的弹窗（推荐：和 Logger.prefab 一样挂在 MainScence 里）
                var existing = UnityEngine.Object.FindObjectOfType<LHGFErrorWindow>(true);
                if (existing != null)
                {
                    window = existing;
                }
                else
                {
                    // 2) Resources 中的预制体
                    var prefab = Resources.Load<GameObject>("LHGFErrorWindow");
                    if (prefab != null)
                    {
                        var go = UnityEngine.Object.Instantiate(prefab);
                        go.name = "LHGFErrorWindow";
                        window = go.GetComponent<LHGFErrorWindow>();
                    }
                }

                // 3) 兜底：代码构建最小可用界面
                if (window == null)
                {
                    window = LHGFErrorWindow.CreateFallback();
                }

                if (window != null)
                {
                    UnityEngine.Object.DontDestroyOnLoad(window.gameObject);
                    window.gameObject.SetActive(false);
                }
            }
            catch (Exception ex)
            {
                AppendToFile("[LHGFErrorGuard failed to create error window]", ex.ToString(), LogType.Error);
            }
        }

        // ---------------------------------------------------------------- 上下文

        /// <summary>拼装报错时的游戏状态，方便测试同学定位，例如“节点 / 剧本名 / 最近历史”。</summary>
        private static string BuildContext()
        {
            try
            {
                var main = LHGFGameMain.instance;
                if (main == null) return "(game not initialized)";

                string node = main.gameProgress?.CurrentNodeId;
                string script = main.gameData?.CurrentScriptName;
                if (string.IsNullOrEmpty(node) && string.IsNullOrEmpty(script)) return "(game not started)";

                var sb = new StringBuilder();
                sb.Append("Script: ").Append(string.IsNullOrEmpty(script) ? "<unknown>" : script);
                sb.Append("    Node: ").Append(string.IsNullOrEmpty(node) ? "<unknown>" : node);

                var history = main.gameData?.historyNodes;
                if (history != null && history.Count > 0)
                {
                    int from = Math.Max(0, history.Count - 5);
                    sb.Append("\nRecent nodes: ");
                    for (int i = from; i < history.Count; i++)
                    {
                        if (i > from) sb.Append(" -> ");
                        sb.Append(history[i]);
                    }
                }
                return sb.ToString();
            }
            catch
            {
                return "(failed to read context)";
            }
        }

        // ---------------------------------------------------------------- 日志落盘

        private static string logFilePath;
        private static string logDirUsed;
        private static bool logWriteFailed;
        private static int logWriteFailCount;

        /// <summary>日志文件完整路径（用于在弹窗中展示）。</summary>
        public static string LogFilePath
        {
            get
            {
                EnsureLogPath();
                return logFilePath;
            }
        }

        private static void EnsureLogPath()
        {
            if (logFilePath != null) return;
            try
            {
                string root = GetRootPath();
                string dir = Path.Combine(root, "ErrorLog");
                Directory.CreateDirectory(dir);
                logDirUsed = dir;
                logFilePath = Path.Combine(dir, $"error_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                // 写入运行环境头，方便对照
                File.AppendAllText(logFilePath, BuildHeader(), new UTF8Encoding(true));
            }
            catch (Exception ex)
            {
                // 根目录不可写 → 回退到 persistentDataPath
                try
                {
                    string dir2 = Path.Combine(Application.persistentDataPath, "ErrorLog");
                    Directory.CreateDirectory(dir2);
                    logDirUsed = dir2;
                    logFilePath = Path.Combine(dir2, $"error_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                    File.AppendAllText(logFilePath,
                        BuildHeader() + $"[Warning] Root directory is not writable, log redirected here. Reason: {ex.Message}\n\n",
                        new UTF8Encoding(true));
                }
                catch
                {
                    logFilePath = null;
                }
            }
        }

        private static string GetRootPath()
        {
            // 与 Utils.ResoucePaths.GameRootPath 保持一致：Windows 下为 exe 所在目录
            return Directory.GetParent(Application.dataPath).FullName;
        }

        private static string BuildHeader()
        {
            var sb = new StringBuilder();
            sb.Append("================ LHGF Error Log ================\n");
            sb.Append("Time:    ").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append('\n');
            sb.Append("Version: ").Append(Application.version).Append('\n');
            sb.Append("Unity:   ").Append(Application.unityVersion).Append('\n');
            sb.Append("Platform:").Append(Application.platform).Append('\n');
            sb.Append("OS:      ").Append(SystemInfo.operatingSystem).Append('\n');
            sb.Append("Log dir: ").Append(logDirUsed ?? "<unknown>").Append('\n');
            sb.Append("=================================================\n\n");
            return sb.ToString();
        }

        /// <summary>
        /// 追加写入日志。整个方法自带 try/catch，写失败不会再次触发错误监听。
        /// 使用 AppendAllText 而非长期持有 StreamWriter，避免崩溃时缓冲区内容丢失。
        /// </summary>
        private static void AppendToFile(string condition, string stackTrace, LogType type)
        {
            if (logWriteFailed) return;
            EnsureLogPath();
            if (logFilePath == null)
            {
                logWriteFailed = true;
                return;
            }

            try
            {
                var sb = new StringBuilder();
                sb.Append('[').Append(DateTime.Now.ToString("HH:mm:ss.fff")).Append("][")
                  .Append(type).Append("][+")
                  .Append((Time.realtimeSinceStartup - startTime).ToString("F2")).Append("s] ");
                sb.Append(condition).Append('\n');
                if (!string.IsNullOrEmpty(stackTrace))
                {
                    sb.Append(stackTrace.TrimEnd()).Append('\n');
                }
                sb.Append("-----------------------------------------------\n");
                File.AppendAllText(logFilePath, sb.ToString(), new UTF8Encoding(false));
            }
            catch
            {
                // 连续失败若干次后停止尝试，避免卡在磁盘错误上
                logWriteFailCount++;
                if (logWriteFailCount > 3) logWriteFailed = true;
            }
        }
    }
}
