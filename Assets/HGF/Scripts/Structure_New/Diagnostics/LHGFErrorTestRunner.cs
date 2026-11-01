using System;
using UnityEngine;

namespace LHGFDiagnostics
{
    /// <summary>
    /// 错误窗口的独立测试脚本。
    ///
    /// 用法：
    ///   1. 新建空场景（ErrorTest），把本脚本挂到任意 GameObject 上；
    ///   2. Play，按提示按键触发各类错误；
    ///   3. 观察：弹窗是否出现、游戏是否冻结、点「忽略并继续」是否恢复、
    ///      根目录 ErrorLog\ 下是否生成日志。
    ///
    /// 本脚本不依赖 LHGFGameMain，可以在完全空的场景里单独运行。
    /// 兜底 UI 使用英文标签，因此不需要配置中文字体。
    /// </summary>
    public class LHGFErrorTestRunner : MonoBehaviour
    {
        [Header("Test Keys / 测试按键")]
        [Tooltip("Trigger a single error (Debug.LogError)")]
        public KeyCode keySingleError = KeyCode.Alpha1;
        [Tooltip("Trigger 5 identical errors in a row (checks suppression)")]
        public KeyCode keyRepeatedError = KeyCode.Alpha2;
        [Tooltip("Throw a real exception (LogType.Exception)")]
        public KeyCode keyException = KeyCode.Alpha3;
        [Tooltip("Throw every frame (simulates an Update bug); press again to stop")]
        public KeyCode keyStorm = KeyCode.Alpha4;

        private bool stormActive;
        private int frameCounter;

        private void Start()
        {
            Debug.Log("[LHGFErrorTest] Test started. 1=single error  2=repeated errors  3=exception  4=error every frame (toggle)");
            DumpWindowHierarchy();
        }

        /// <summary>把错误窗口的完整层级与组件打印到 Console，用于排查按钮是否真的创建。</summary>
        private void DumpWindowHierarchy()
        {
            var window = FindObjectOfType<LHGFErrorWindow>(true);
            if (window == null)
            {
                Debug.LogWarning("[LHGFErrorTest] No LHGFErrorWindow found in scene.");
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("[LHGFErrorTest] === Error window hierarchy ===\n");
            sb.Append("root: ").Append(window.gameObject.name)
              .Append("  activeSelf=").Append(window.gameObject.activeSelf)
              .Append("  activeInHierarchy=").Append(window.gameObject.activeInHierarchy).Append('\n');
            sb.Append("  titleText=").Append(window.titleText != null ? "OK" : "NULL")
              .Append("  contextText=").Append(window.contextText != null ? "OK" : "NULL")
              .Append("  logText=").Append(window.logText != null ? "OK" : "NULL").Append('\n');
            sb.Append("  copyButton=").Append(window.copyButton != null ? "OK" : "NULL")
              .Append("  ignoreButton=").Append(window.ignoreButton != null ? "OK" : "NULL")
              .Append("  quitButton=").Append(window.quitButton != null ? "OK" : "NULL")
              .Append("  suppressedHint=").Append(window.suppressedHint != null ? "OK" : "NULL").Append('\n');

            DumpRecursive(window.transform, 1, sb);
            Debug.Log(sb.ToString());
        }

        private void DumpRecursive(Transform t, int depth, System.Text.StringBuilder sb)
        {
            var comps = new System.Collections.Generic.List<string>();
            foreach (var c in t.GetComponents<Component>())
            {
                if (c != null) comps.Add(c.GetType().Name);
            }
            sb.Append(new string(' ', depth * 2)).Append(t.name)
              .Append(" [").Append(string.Join(", ", comps)).Append("]\n");

            for (int i = 0; i < t.childCount; i++)
            {
                DumpRecursive(t.GetChild(i), depth + 1, sb);
            }
        }

        private void Update()
        {
            // 心跳放在 gate 之前，保证「忽略并继续」后能立即看到主循环恢复
            frameCounter++;
            if (frameCounter % 45 == 0)
            {
                Debug.Log($"[LHGFErrorTest] heartbeat frame={frameCounter} timeScale={Time.timeScale} totalErrors={LHGFErrorGuard.TotalErrors} windowOpen={LHGFErrorGuard.IsOpen}");
            }

            // 错误窗口打开时，本测试脚本也应停下来（与游戏主循环行为一致）
            if (LHGFErrorGuard.IsOpen) return;

            if (Input.GetKeyDown(keySingleError))
            {
                Debug.LogError("[LHGFErrorTest] Single test error: the error window should appear.");
            }

            if (Input.GetKeyDown(keyRepeatedError))
            {
                for (int i = 0; i < 5; i++)
                {
                    Debug.LogError("[LHGFErrorTest] Repeated test error: only one window, others are suppressed.");
                }
            }

            if (Input.GetKeyDown(keyException))
            {
                TriggerException();
            }

            if (Input.GetKeyDown(keyStorm))
            {
                stormActive = !stormActive;
                Debug.Log($"[LHGFErrorTest] Error storm: {(stormActive ? "ON" : "OFF")}");
            }

            if (stormActive)
            {
                Debug.LogError("[LHGFErrorTest] Error storm: this should be suppressed while the window is open.");
            }
        }

        /// <summary>抛一个会被 Unity 记为 LogType.Exception 的真实异常。</summary>
        private void TriggerException()
        {
            // 故意在业务逻辑里抛出，不 catch，让 Unity 记录完整的 Exception 堆栈，
            // 验证错误窗口能否捕获 LogType.Exception（而不只是 Debug.LogError）。
            throw new InvalidOperationException(
                "Simulated exception thrown on purpose to verify the error window catches LogType.Exception.");
        }
    }
}
