using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
namespace LHGFData
{
    public class LHGFWebViewLayer : ILayer
    {
        // ==================== 数据与常量 ====================
        // 本地网页放置目录(相对 StreamingAssets/HGF/)
        public const string LocalWebPageFolder = "WebPage";

        // 运行期状态(命令数据之外,由 Layer/Controller 维护)
        public WebViewData currentWebViewData;
        public List<WebViewData> HistoryWebViewData = new() { };
        // 页面加载完成标记:Controller 收到 UWB OnLoadFinish 后置位,供 Finish() 使用
        public bool IsLoaded = false;

        // 中间层控制器:负责定位/开关/加载(对应黑板 Layer 持有的 blackboardLayerControler)
        public LHGFWebViewLayerControler webViewLayerControler;

        public class WebViewData
        {
            public string Url;              // 最终传给 LoadUrl 的完整地址(本地已拼成 file:// 或保持外部 URL)
            public bool State = true;       // 开(1)/关(0),默认打开
            public string PosInfo;          // 定位关键词:UpperLeft | UpperCenter | UpperRight
            public Vector2 Pos = new Vector2(0.5f, 0.5f);   // 计算后的锚点(0~1)
            public Vector2 Size = new Vector2(0.6f, 0.6f);  // 相对画布尺寸(0~1)

            public WebViewData() { }
            public WebViewData(LayerCommand command, WebViewData refdata)
            {
                // URL:本地优先,外部兜底
                string rawUrl = Utils.GetDicValue(command.CommandConfig, "Url", null);
                Url = ResolveUrl(rawUrl);

                // 开关状态:默认打开
                State = Utils.string2bool(Utils.GetDicValue(command.CommandConfig, "State", "1"));

                // 尺寸:默认参考上一次数据(与黑板 BlackboardData 的默认值继承风格一致)
                float defaultSizeX = (refdata != null) ? refdata.Size.x : 0.6f;
                float defaultSizeY = (refdata != null) ? refdata.Size.y : 0.6f;
                float sizeX = float.Parse(Utils.GetDicValue(command.CommandConfig, "SizeX", defaultSizeX.ToString()));
                float sizeY = float.Parse(Utils.GetDicValue(command.CommandConfig, "SizeY", defaultSizeY.ToString()));
                Size = new Vector2(sizeX, sizeY);

                // 位置关键词:默认参考上一次
                string defaultPosInfo = (refdata != null) ? refdata.PosInfo : "UpperCenter";
                PosInfo = Utils.GetDicValue(command.CommandConfig, "PosInfo", defaultPosInfo);
                if (PosInfo == "UpperLeft")
                {
                    Pos = new Vector2(Size.x / 2, 1 - Size.y / 2);
                }
                else if (PosInfo == "UpperCenter")
                {
                    Pos = new Vector2(1f / 2, 1 - Size.y / 2);
                }
                else if (PosInfo == "UpperRight")
                {
                    Pos = new Vector2(1 - Size.x / 2, 1 - Size.y / 2);
                }
                else
                {
                    // 未匹配关键词:使用手动锚点
                    float posX = float.Parse(Utils.GetDicValue(command.CommandConfig, "PosX", "0.7"));
                    float posY = float.Parse(Utils.GetDicValue(command.CommandConfig, "PosY", "0.7"));
                    Pos = new Vector2(posX, posY);
                }
            }

            /// <summary>
            /// URL 解析:本地优先,外部兜底。
            /// ① 带协议(http:// / https:// / file://)→ 原样返回(外部地址);
            /// ② 无协议 → 视为 StreamingAssets/HGF/WebPage/ 下的相对路径,本地文件存在则拼成 file:// URL;
            /// ③ 本地文件不存在 → 原样返回,交给 LoadUrl 作为外部地址兜底(加载失败由引擎报错)。
            /// </summary>
            public static string ResolveUrl(string raw)
            {
                if (string.IsNullOrEmpty(raw))
                {
                    return raw;
                }
                if (raw.StartsWith("http://") || raw.StartsWith("https://") || raw.StartsWith("file://"))
                {
                    return raw;
                }

                // 本地优先:StreamingAssets/HGF/WebPage/<raw>(允许 raw 带子目录,如 web/help.html)
                string localDir = Path.Combine(Application.streamingAssetsPath, "HGF", LocalWebPageFolder);
                string localPath = Path.Combine(localDir, raw);
                if (File.Exists(localPath))
                {
                    // 生成 file:///... 格式的标准 URL(自动处理空格与转义)
                    return new Uri(localPath).AbsoluteUri;
                }

                // 外部兜底:本地文件不存在,原样返回
                return raw;
            }

            /// <summary>
            /// 深拷贝(存档/回撤用):复制一份独立数据,避免历史与当前共享引用。
            /// </summary>
            public WebViewData DeepClone()
            {
                WebViewData clone = new WebViewData
                {
                    Url = Url,
                    State = State,
                    PosInfo = PosInfo,
                    Pos = Pos,
                    Size = Size
                };
                return clone;
            }
        }

        /// <summary>
        /// 存档数据(ILayer.Save 的返回值,由框架 Newtonsoft.Json 序列化)。
        /// 保存当前页与回撤历史 —— 两者都是完整 WebViewData 快照(含 Url/State/PosInfo/Pos/Size),
        /// 因此位置与尺寸随存档自动保存,回撤也可恢复完整状态。
        /// </summary>
        public class SaveData
        {
            public WebViewData currentWebViewData;
            public List<WebViewData> historyWebViewData = new() { };
        }

        public void BeforeNextOnLoadStart()
        {
            // 读档路径的节点切换钩子;WebView 无每节点分组逻辑,空实现(与黑板保持同构但无需 IsNewNodeStart)
        }

        public void BeforeNextStart()
        {
            // 前进路径的节点切换钩子;WebView 无每节点分组逻辑,空实现
        }

        public bool Finish()
        {
            // 从未收到过 WebView 命令:不阻塞游戏(否则开局即被卡死)
            if (currentWebViewData == null)
            {
                return true;
            }
            // 关闭命令:关闭面板是即时动作,不阻塞
            if (!currentWebViewData.State)
            {
                return true;
            }
            // 打开命令:页面加载完成(OnLoadFinish 置位 IsLoaded)才允许继续前进
            return IsLoaded;
        }

        public GameObject GetControler()
        {
            return webViewLayerControler != null ? webViewLayerControler.gameObject : null;
        }

        public string LayerName()
        {
            return "WebViewLayer";
        }

        public Dictionary<string, string> Log(LayerCommand command)
        {
            // 记录到对话历史:网页本身不便摘要,记当前 URL;无当前页则不记
            if (currentWebViewData == null || string.IsNullOrEmpty(currentWebViewData.Url))
            {
                return null;
            }
            Dictionary<string, string> content = new()
            {
                { "TextInfo", currentWebViewData.Url },
                { "ContentType", "Text" }
            };
            return content;
        }

        public void Next(LayerCommand command)
        {
            // 解析命令为 WebViewData(URL 解析/State/定位与尺寸继承上一次,与黑板 BlackboardData 同构)
            WebViewData data = new(command, currentWebViewData);

            if (data.State)
            {
                // 打开面板:先重置加载完成标记,避免旧的 IsLoaded 让 Finish() 误放行
                IsLoaded = false;
                webViewLayerControler.Open(data);
            }
            else
            {
                // 关闭面板(即时动作,不阻塞)
                webViewLayerControler.Close();
            }

            // 更新当前数据并记录历史(供 Withdraw 回撤/恢复)
            currentWebViewData = data;
            HistoryWebViewData.Add(data);
        }

        public void OnLoadFinish()
        {
            // 读档流程最后阶段回调;显示恢复已在 Load() 里处理,此处空实现
        }

        public void OnStart()
        {
            // 游戏开始时初始关闭面板(中间层 LayerControler.Start 也已保证,双保险)
            if (webViewLayerControler != null)
            {
                webViewLayerControler.Close();
            }
        }

        public void OnUpdate()
        {
            // 菜单/面板打开时让网页让位(射线穿透到下层 UI)
            webViewLayerControler?.UpdateWebViewBlocking();

            // 面板可见时每帧把鼠标位置同步给浏览器(补偿 UWB 协程未启动导致的拖拽不跟手)
            if (currentWebViewData != null && currentWebViewData.State)
            {
                webViewLayerControler?.SyncMousePositionToBrowser();
            }
        }

        public void Reset()
        {
            // 重置层数据并关闭面板(与黑板 Reset:Clear + CloseBlackboard 对应)
            currentWebViewData = null;
            HistoryWebViewData.Clear();
            IsLoaded = false;
            if (webViewLayerControler != null)
            {
                webViewLayerControler.Close();
            }
        }

        public object Save()
        {
            SaveData saveData = new SaveData();

            // 深拷贝当前页与历史(避免存档与运行期共享引用,Json 序列化前数据被外部修改)
            if (currentWebViewData != null)
            {
                saveData.currentWebViewData = currentWebViewData.DeepClone();
            }
            if (HistoryWebViewData != null)
            {
                saveData.historyWebViewData = new List<WebViewData>();
                foreach (WebViewData data in HistoryWebViewData)
                {
                    saveData.historyWebViewData.Add(data.DeepClone());
                }
            }

            return saveData;
        }

        public void Load(object SaveData)
        {
            if (SaveData == null) return;

            // 兼容两种情况:直接传入 SaveData 实例,或传入 Json 反序列化出的中间对象
            SaveData data;
            data = SaveData as SaveData;
            if (data == null)
            {
                string json = JsonConvert.SerializeObject(SaveData);
                data = JsonConvert.DeserializeObject<SaveData>(json);
            }

            // 恢复数据(深拷贝)
            currentWebViewData = data.currentWebViewData != null ? data.currentWebViewData.DeepClone() : null;
            HistoryWebViewData = new List<WebViewData>();
            if (data.historyWebViewData != null)
            {
                foreach (WebViewData item in data.historyWebViewData)
                {
                    HistoryWebViewData.Add(item.DeepClone());
                }
            }

            // 读档后当前节点命令不会重放,这里主动重建显示(与黑板 Load 重建黑板显示一致):
            // 打开状态 → 显示并重新加载该 URL;关闭状态 → 仅隐藏。
            // 注意:IsLoaded 不随存档保存,重载后回到 false,等 OnLoadFinish 再置位。
            if (webViewLayerControler != null)
            {
                if (currentWebViewData != null && currentWebViewData.State)
                {
                    IsLoaded = false;
                    webViewLayerControler.Open(currentWebViewData);
                }
                else
                {
                    webViewLayerControler.Close();
                }
            }
        }

        public void Skip()
        {
            // 强制放行:不等页面加载完成(加载慢时不至于永久卡死)
            // 注意:只影响"能否继续"的标记,不影响网页在屏幕上是否显示
            IsLoaded = true;
        }

        public bool Withdraw()
        {
            // 无历史可撤
            if (HistoryWebViewData.Count == 0)
            {
                return false;
            }

            // 撤掉当前这条命令的记录(框架会按被回撤节点的命令条数逐条调用 Withdraw)
            HistoryWebViewData.RemoveAt(HistoryWebViewData.Count - 1);

            if (HistoryWebViewData.Count > 0)
            {
                // 恢复到上一命令的最终状态
                WebViewData last = HistoryWebViewData[HistoryWebViewData.Count - 1];
                RestoreDisplay(last);
                currentWebViewData = last.DeepClone();
            }
            else
            {
                // 撤回"从未有过网页"的初始状态
                currentWebViewData = null;
                if (webViewLayerControler != null)
                {
                    webViewLayerControler.Close();
                }
            }

            return true;
        }

        /// <summary>
        /// 回撤时的显示恢复:目标是打开状态 → 显示(URL 相同则不重载,避免闪烁;不同才重新加载);
        /// 目标是关闭状态 → 隐藏。
        /// </summary>
        private void RestoreDisplay(WebViewData target)
        {
            if (webViewLayerControler == null)
            {
                Debug.LogError("[LHGFWebViewLayer] webViewLayerControler 引用未设置,回撤无法恢复显示!");
                return;
            }

            if (!target.State)
            {
                webViewLayerControler.Close();
                return;
            }

            // URL 相同(回到同一个页面):只重定位不重载,IsLoaded 保持原值(页面仍在)
            if (currentWebViewData != null && currentWebViewData.Url == target.Url)
            {
                webViewLayerControler.ShowWithoutReload(target);
                return;
            }

            // URL 不同(回到另一个页面):重新加载,等新页面 OnLoadFinish 再置位 IsLoaded
            IsLoaded = false;
            webViewLayerControler.Open(target);
        }
    }
}