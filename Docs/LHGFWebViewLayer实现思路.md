# LHGFWebViewLayer 实现思路(UWB 2.2.8 适配版)

> 目标:在 LHGF 框架中新增 `LHGFWebViewLayer`(ILayer),实现类似黑板的网页展示层 ——
> 任意位置/任意大小显示网页、支持命令驱动、存读档、回撤,并预留「网页 ↔ 剧情」双向通信。
> 数据来源:`Library/PackageCache/dev.voltstro.unitywebbrowser@2.2.8`(Voltstro UnityWebBrowser,以下简称 UWB)。
> 状态:**仅设计思路,未实现**。

---

## 1. UWB 2.2.8 组件模型(与本框架对接的核心)

UWB 的核心是「一个挂到 GameObject 上的管理器组件,自动拉起浏览器引擎子进程,渲染到 RawImage,自带输入处理」:

```
[AddComponentMenu("UWB/Web Browser Basic")]
WebBrowserUIBasic  (sealed, 直接用)
  └─ RawImageUwbClientInputHandler  (abstract, 鼠标/键盘输入转发)
       └─ RawImageUwbClientManager   (abstract, 渲染到 RawImage + 生命周期)
            └─ BaseUwbClientManager  (abstract MonoBehaviour)
                 ├─ public WebBrowserClient browserClient  ← 所有控制/事件都在这
                 ├─ Start() → browserClient.Init()          (引擎进程启动)
                 └─ OnDestroy() → browserClient.Dispose()
```

- `WebBrowserUIBasic`、`WebBrowserUIFull`(带全屏)已提供;`WebBrowserUIControls` 是带地址栏的 UI 版,我们不需要。
- **我们的 Controller 只需持有 `WebBrowserUIBasic`(或其 `browserClient`)引用**,所有操作都走 `browserClient`。

## 2. 用到的 UWB API 清单与职责映射

| UWB API | 签名/类型 | 在本层的用途 |
| --- | --- | --- |
| `LoadUrl(string)` | `void` | 加载剧情指定的网址(命令 `Url` 属性) |
| `LoadHtml(string)` | `void` | 加载本地/内联 HTML(可选,支持本地内容) |
| `browserClient.BrowserTexture` | `Texture2D` | 渲染到 RawImage 的纹理(UWB 自动处理,仅需了解) |
| `ExecuteJs(string)` | `void` | C# → 网页:注入 JS(可选:改页面内容、触发页面逻辑) |
| `RegisterJsMethod<T...>(name, Action<T...>)` | `void` | 网页 → C#:注册回调,网页里 `window.xxx(...)` 可调用 C#(互动剧情的关键通路) |
| `OnLoadFinish(url)` | `event OnLoadFinishDelegate` | 页面加载完成 → 决定层 `Finish()`(WaitForSignal=0 模式) |
| `OnLoadStart(url)` / `OnUrlChanged(url)` | `event` | 状态追踪、历史记录 |
| `OnInputFocus(bool)` | `event` | 检测玩家是否正在操作网页(避免与剧情按钮抢输入) |
| `SetZoomLevelPercent(double)` | `void` | 网页缩放(可选命令属性) |
| `IsConnected` | `bool` | 引擎是否就绪,容量/加载的防御判断 |
| `Init()` / `Dispose()` | `void` | 生命周期由 UWB 组件自动管(挂组件即自动 Init),不用手动 |

> 关键结论:UWB 把「引擎进程、纹理渲染、鼠标键盘输入」都封装好了;
> 我们的层**只需要管「什么时候加载哪个 URL、什么时候显示/隐藏、什么时候算完成」**。

## 3. XML 命令设计(仿 `Blackboard` 语法)

```xml
<!-- 打开并加载网页(位置关键词:UpperLeft/UpperCenter/UpperRight) -->
<WebView Url="help.html" State="1" PosInfo="UpperCenter" SizeX="0.8" SizeY="0.6"/>
<!-- 关闭 -->
<WebView State="0"/>
```

| 属性 | 必填 | 默认 | 说明 |
| --- | --- | --- | --- |
| `Url` | State=1 时必填 | 无 | 网页地址。**本地优先**:`Url` 未带协议(`http://`/`https://`/`file://`)时视为 **StreamingAssets 下的相对路径**,层内拼成本地 file:// URL;若该本地文件不存在(或带了协议),直接按外部 URL 交给 `LoadUrl`(选项 B:本地优先、外部兜底) |
| `State` | ✔ | — | `1` 打开面板并加载 / `0` 关闭面板 |
| `PosInfo` | | `UpperCenter` | 定位关键词:`UpperLeft` / `UpperCenter` / `UpperRight`(按黑板现有关键词集合);未匹配则用 `PosX/PosY` |
| `PosX` / `PosY` | | `0.7, 0.7` | 手动锚点(0~1),`PosInfo` 未匹配时使用 |
| `SizeX` / `SizeY` | | `0.6, 0.6` | 相对画布尺寸(0~1),复用 `Utils.SetRelativeSize` |

> 本地网页推荐放置目录:`Assets/StreamingAssets/HGF/WebPage/`(可新增,需在 `Utils.ResoucePaths` 加一个常量或直接拼 `ScriptPath` 同级目录)。

## 4. 数据类:`WebViewData`(仿 `BlackboardData`,可放 `LHGFWebViewLayer.cs` 内)

```
Url          string   // 最终要传给 LoadUrl 的完整地址(本地已拼成 file:// 或保持外部 URL)
State        bool     // 开/关
PosInfo      string   // UpperLeft | UpperCenter | UpperRight(3 个关键词)
Pos          Vector2  // 计算后的锚点(0~1),由 PosInfo 或 PosX/PosY 得出
Size         Vector2  // 相对画布尺寸

// 运行期状态(Layer 内部,不进存档的命令数据)
IsLoaded     bool     // 收到 UWB OnLoadFinish 置位,供 Finish()
```

- 构造:`WebViewData(LayerCommand command, WebViewData refdata)` —— 解析 `CommandConfig`;若 `PosInfo` 是关键词按黑板的三段计算算出 `Pos`,否则用 `PosX/PosY`。
- **URL 解析函数(定稿)**:`ResolveUrl(raw)` →
  ① `raw` 以 `http://` / `https://` / `file://` 开头 → 直接返回原值(外部地址);
  ② 否则视为本地相对路径:`path = StreamingAssets/HGF/WebPage/raw`,若 `File.Exists` 返回 `file://` + 该路径;
  ③ 本地文件不存在 → 退回把 `raw` 原样当 URL 交给 `LoadUrl`(外部兜底,加载失败由引擎报错)。

## 5. ILayer 各方法实现思路(填满现有 16 个空方法)

| 方法 | 思路 |
| --- | --- |
| `LayerName()` | 返回 `"WebViewLayer"` |
| `OnStart()` | 面板初始隐藏;引擎由 UWB 组件自动 Init(若组件常驻场景) |
| `Next(command)` | 解析 `WebViewData`(含 URL 解析);`State=1` → Controller.Open(定位+显示) → `browserClient.LoadUrl(ResolveUrl(Url))` 并重置 `IsLoaded=false`;`State=0` → Controller.Close;记录 History 与 currentWebViewData |
| `Finish()` | `State=0` 返回 true;**`State=1` 时返回 `IsLoaded`**(页面加载完成才放行,已定稿) |
| `Skip()` | 置 `IsLoaded=true`(跳过等待),与黑板 `Skip` 语义一致 |
| `Log(command)` | 记录 URL 文本到历史(最简版只记 URL) |
| `GetControler()` | 返回 Controller GameObject |
| `Withdraw()` | 从 History 弹回上一个 `WebViewData` 并恢复(回到上一 URL 或关闭) |
| `Save()` / `Load()` | Save:序列化 currentWebViewData 的 Url/State/Pos/Size;Load:反序列化并**重新 ResolveUrl + LoadUrl**(异步恢复) |
| `Reset()` | 清 History、关闭面板;不 Dispose 引擎(引擎归 UWB 组件管,面板可复用) |
| `BeforeNextStart()` / `BeforeNextOnLoadStart()` / `OnUpdate()` / `OnLoadFinish()` | 常规空实现或轻量处理 |

## 6. Controller:`LHGFWebViewLayerControler : MonoBehaviour`(新增)

- 字段:持有一个 **`WebBrowserUIBasic`** 引用(预制体上拖好),外加面板的 `RectTransform`。
- 方法:
  - `Open(WebViewData)` / `Close()`:SetActive + `Utils.SetSingleAnchorPoint` + `Utils.SetRelativeSize`(复制 `LHGFBlackboardLayerControler` 的定位写法,关键词定位即在此生效);
  - `LoadUrl(url)`:防呆(`browserClient != null && browserClient.IsConnected`)后 `browserClient.LoadUrl(url)`;
  - 事件转发:`browserClient.OnLoadFinish += ...` 置层内 `IsLoaded`(最简版只挂这个)。

## 7. 与框架的集成点

1. `LHGFGameMain.SetLayers()`:`new WebViewLayer()` + 把 Controller 赋给 `webViewLayer.LHGFWebViewLayerControler` + `layers.Add("WebViewLayer", ...)`;
2. `SetLayerCommand2layer()`:加 `"WebView" → new List<string>() { "WebViewLayer" }`;
3. 场景 `MainScenceNew.unity`:
   - Canvas 下建「WebViewPanel」子物体(带 `RawImage` + **`WebBrowserUIBasic`** 组件 + 我们新增的 Controller);
   - 初始隐藏(不隐藏 UWB 组件所在物体!见 §8 注意 1),Controller 拖到 `LHGFGameMain` 的字段。

## 8. 必须注意的坑(UWB 特性相关)

1. **引擎子进程不能随便销毁**:`WebBrowserUIBasic` 挂载的物体被 `SetActive(false)` 时,`BaseUwbClientManager.Start()` 已跑过、引擎仍在后台;但**面板物体若被 SetActive(false),UWB 的 Update(取像素/LoadTextureData)会停**。
   - 对策:**面板根物体常驻激活**,显示/隐藏用内部子物体(如 RawImage 所在节点)或 CanvasGroup.alpha + 关闭射线;保证浏览器在隐藏时也保持连接,避免反复断开/重启引擎(引擎进程启动很慢,约 1~2s)。
2. **引擎进程数量**:每个 `WebBrowserUIBasic` 实例 = 一个浏览器子进程,内存不小;**全游戏建议只放一个实例**,WebViewLayer 复用它,不做「每个 URL 一个新浏览器」。
3. **输入冲突**:`RawImageUwbClientInputHandler` 实现 `IPointerDownHandler` 等,网页打开时会拦截该区域所有输入;注意与选项按钮、菜单的层级关系(网页面板在菜单之下即可)。
4. **本地 HTML / 文件**:`file://` 路径可用,但 WebGL/移动端与桌面端行为不同(桌面 Windows 没问题);本地内容更推荐 `LoadHtml` 或打包进 StreamimgAssets 后用 `Application.streamingAssetsPath` 拼 file:// URL。
5. **平台支持**:UWB 2.2.8 主要支持桌面(Windows/macOS/Linux);发布到其他平台前确认引擎支持(包内 `Plugins/` 有各平台引擎,WebGL 不适用)。这是 Galgame 常用的 Windows 目标,通常没问题。
6. **暂停/自动模式**:播放器开网页时若开了「自动」,`Finish()` 不会等玩家看网页 —— 建议 `WaitForSignal=1` 时由网页自行决定节奏,或在剧情里把自动模式提示暂停(可选,后续再议)。
7. **存读档的页面状态**:网页内部状态(滚动、表单、缓存)无法随存档恢复;`Load()` 只保证「重新加载同一 URL」。在文档里写明这个语义,避免玩家/策划误解。

## 9. 实施顺序建议

1. 先做「手工验证」:在空场景放 `UnityWebBrowser.prefab`(官方 Sample)或手挂 `WebBrowserUIBasic` + RawImage,确认引擎能启动、能 LoadUrl、能点击;**这步排除 UWB 本身的问题**;
2. 实现 `WebViewData` + 命令解析(`Next`) + Controller 开/关/定位/加载 —— 用一条 `<WebView Url=... State=1/>` 命令打通「剧情 → 网页显示」;
3. 补 `Finish/Skip/Withdraw/Log` —— 打通「网页加载完 → 能继续剧情 → 回撤」;
4. 补 `Save/Load/Reset` —— 打通存读档;
5. (可选)加 `WaitForSignal` + `RegisterJsMethod` —— 网页按钮通知剧情继续(互动玩法);
6. 注册进 `SetLayers`/`SetLayerCommand2layer`、接进场景,端到端测试。

---

*文档基于 UWB 2.2.8 实际源码(DLL 中的类与事件名已核对)编写;UWB API 若未来升级,以 「§2 职责映射」 为准即可,层内其余设计不受影响。*