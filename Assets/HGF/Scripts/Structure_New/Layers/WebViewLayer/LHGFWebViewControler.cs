using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoltstroStudios.UnityWebBrowser;
using VoltstroStudios.UnityWebBrowser.Core;
using VoltstroStudios.UnityWebBrowser.Helper;

/// <summary>
/// WebView 底层控制器:挂在 WebView 本体上,直接操作 UWB(WebBrowserUIBasic)的网页控制方法。
/// 职责:显示/隐藏网页面板(操作自身 gameObject,与黑板 LHGFBlackboardControler 被 LayerControler
/// 直接 SetActive 的方式一致)、加载 URL(引擎未就绪时挂起)、把 UWB 事件转发给上层。
///
/// 挂载约定(重要):
///     本组件与 WebBrowserUIBasic + RawImage 挂在同一个 GameObject(即 WebView 本体)上,
///     显示/隐藏由上层(LHGFWebViewLayerControler)对本组件所在物体 SetActive。
///
/// 引擎启动提示:UWB 引擎进程在物体激活时才会 Init() 启动。因此本物体(或其常驻父物体)
/// 需要保证在游戏开始时激活一次;之后 SetActive(false) 只是暂停画面刷新,引擎进程仍在,
/// 重新 SetActive(true) 后画面刷新自动恢复。
/// </summary>
public class LHGFWebViewControler : MonoBehaviour
{
    /// <summary>
    /// UWB 浏览器组件(WebBrowserUIBasic),与本组件挂在同一物体上,Inspector 中拖引用。
    /// </summary>
    [Tooltip("UWB 浏览器组件(WebBrowserUIBasic),与本组件挂在同一物体上")]
    public WebBrowserUIBasic Browser;

    /// <summary>
    /// 网页加载完成事件(参数:加载完成的 URL)。
    /// 上层(LHGFWebViewLayerControler / Layer)通过它把 IsLoaded 置位,驱动 Finish()。
    /// </summary>
    public event Action<string> OnPageLoadFinished;

    /// <summary>
    /// 引擎就绪前暂存的 URL:引擎连接完成后自动补加载,避免剧情先于引擎执行时的时序问题。
    /// </summary>
    private string pendingUrl;

    private bool eventSubscribed;

    private void Start()
    {
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    // ==================== 对上层开放的网页控制方法 ====================

    /// <summary>
    /// 打开并显示网页面板(显示本物体;引擎已在激活状态下启动过,重新激活后画面刷新自动恢复)。
    /// </summary>
    public void OpenWebView()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 关闭并隐藏网页面板(隐藏本物体;引擎进程仍在,下次 OpenWebView 后画面刷新自动恢复)。
    /// </summary>
    public void CloseWebView()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 加载指定 URL(调用方传入的应是 LHGFData.LHGFWebViewLayer.WebViewData.ResolveUrl 处理后的完整地址)。
    /// 引擎未就绪时挂起等待,就绪后自动加载。
    /// </summary>
    public void LoadUrl(string url)
    {
        if (Browser == null)
        {
            Debug.LogError($"[LHGFWebViewControler] Browser(WebBrowserUIBasic) 引用未设置,无法加载 URL: {url}", this);
            return;
        }
        if (Browser.browserClient == null || !Browser.browserClient.IsConnected)
        {
            // 引擎尚未就绪:挂起,等 OnClientInitialized 后补加载
            pendingUrl = url;
            return;
        }
        pendingUrl = null;
        Browser.browserClient.LoadUrl(url);
    }

    /// <summary>
    /// 面板可见时每帧调用:将当前鼠标位置同步给浏览器。
    /// 补偿 UWB "OnPointerEnter 后才启动移动协程" 的固有限制(鼠标本就停在面板上时无 Enter 事件,拖拽/滚轮不跟手)。
    /// 鼠标位置与滚轮换算复用 UWB 自身的工具/口径(与它内部同一套,保证坐标系与方向一致)。
    /// </summary>
    public void SyncMousePositionToBrowser()
    {
        if (Browser == null || Browser.browserClient == null || !Browser.browserClient.IsConnected)
        {
            return;
        }
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        RawImage img = Browser.Image;
        if (img == null || img.texture == null)
        {
            return;
        }

        // 屏幕坐标 → 面板本地比例(0~1,左上原点);camera 取自 canvas.worldCamera(= Main Camera,已验证换算正确)
        if (!WebBrowserUtils.GetScreenPointToLocalPositionDeltaOnImage(img, Input.mousePosition, out Vector2 delta))
        {
            return;
        }

        // 比例 → 浏览器像素坐标
        Vector2 px = new(delta.x * img.texture.width, delta.y * img.texture.height);
        Browser.browserClient.SendMouseMove(px);

        // 滚轮(与 UWB 协程同款换算):滚动量 × 纹理高度;方向与 GetAxis("Mouse ScrollWheel") 一致
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            Browser.browserClient.SendMouseScroll(px, (int)(scroll * Browser.browserClient.BrowserTexture.height));
        }
    }

    // ==================== UWB 事件订阅与转发 ====================

    private void SubscribeEvents()
    {
        if (eventSubscribed || Browser == null || Browser.browserClient == null)
        {
            return;
        }
        Browser.browserClient.OnLoadFinish += OnLoadFinished;
        Browser.browserClient.OnClientConnected += OnClientConnected;
        eventSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!eventSubscribed || Browser == null || Browser.browserClient == null)
        {
            return;
        }
        Browser.browserClient.OnLoadFinish -= OnLoadFinished;
        Browser.browserClient.OnClientConnected -= OnClientConnected;
        eventSubscribed = false;
    }

    /// <summary>
    /// 页面加载完成(OnLoadFinish = "URL finished",不代表加载成功,见设计文档)。
    /// </summary>
    private void OnLoadFinished(string url)
    {
        OnPageLoadFinished?.Invoke(url);
    }

    /// <summary>
    /// 引擎连接完成(ReadySignalReceived 已置位,引擎真正就绪):若此前有挂起的 URL,补加载。
    /// 注意:不要用 OnClientInitialized —— 它只是"Unity 客户端已初始化",引擎进程那时尚未启动,
    /// 此时 LoadUrl 会抛 UwbIsNotReadyException。
    /// </summary>
    private void OnClientConnected()
    {
        if (!string.IsNullOrEmpty(pendingUrl))
        {
            string url = pendingUrl;
            pendingUrl = null;
            Browser.browserClient.LoadUrl(url);
        }
    }
}