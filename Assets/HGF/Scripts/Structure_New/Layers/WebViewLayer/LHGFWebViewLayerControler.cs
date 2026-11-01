using LHGFData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LHGFData.LHGFWebViewLayer;

/// <summary>
/// WebView 中间层控制器:连接逻辑层(LHGFWebViewLayer)与底层(LHGFWebViewControler)。
/// 职责:定位网页面板、封装「打开/关闭/加载」、把底层网页加载完成事件转发为 Layer.IsLoaded。
/// 对应黑板层的 LHGFBlackboardLayerControler(后者做定位并调用底层 Blackboard 控制器)。
///
/// 挂载约定:
///     挂在 Canvas 下的任意管理节点上(不一定在 WebView 本体上),
///     通过 Inspector 拖引用 WebView 本体上的 LHGFWebViewControler 与逻辑层 LHGFWebViewLayer。
/// </summary>
public class LHGFWebViewLayerControler : MonoBehaviour
{
    /// <summary>
    /// 底层控制器(挂在 WebView 本体上),Inspector 中拖引用。
    /// </summary>
    [Tooltip("底层控制器(WebView 本体上的 LHGFWebViewControler)")]
    public LHGFWebViewControler webViewControler;

    /// <summary>
    /// 逻辑层引用,用于回传 IsLoaded(框架惯例:ChoiceLayerControler 亦有 choiceLayer 字段互指)。
    /// </summary>
    [Tooltip("逻辑层(LHGFWebViewLayer)")]
    public LHGFWebViewLayer webViewLayer;

    /// <summary>
    /// 定位用的 RectTransform;未设置时取 webViewControler 所在物体。
    /// </summary>
    [Tooltip("定位用的 RectTransform;留空则用 webViewControler 所在物体")]
    public RectTransform PanelRect;

    /// <summary>
    /// 网页所在根 Canvas(即 FreeCanvas)。菜单打开时把它压到负数排序层,让 StaticCanvas 的 UI 优先命中;
    /// 菜单关闭时恢复 0,网页回到原有 index 竞争并恢复交互。
    /// </summary>
    [Tooltip("网页所在根 Canvas(FreeCanvas)。留空则自动取 webViewControler 的根 Canvas")]
    public Canvas freeCanvas;

    /// <summary>
    /// 菜单打开时 FreeCanvas 的临时排序(负数,低于 StaticCanvas 的 0)。
    /// </summary>
    public int menuClosedSortingOrder = 0;
    public int menuOpenSortingOrder = -10;

    private void Awake()
    {
        // 订阅底层「网页加载完成」事件(与底层自身 Start 的引擎订阅无顺序竞争,事件对象始终存在)
        if (webViewControler != null)
        {
            webViewControler.OnPageLoadFinished += OnPageLoaded;
        }
    }

    private void Start()
    {
        // 初始隐藏面板:WebView 本体在场景中默认激活(保证 UWB 引擎能启动),
        // 这里在运行开始时关闭显示,直到剧情 <WebView State="1"/> 才打开。
        // (黑板由 Layer.OnStart 关闭;此处 WebView 在中间层保证,二者等效)
        Close();
    }

    private void OnDestroy()
    {
        if (webViewControler != null)
        {
            webViewControler.OnPageLoadFinished -= OnPageLoaded;
        }
    }

    /// <summary>
    /// 打开网页面板并定位、加载 URL(供 LHGFWebViewLayer.Next 调用)。
    /// </summary>
    public void Open(WebViewData data)
    {
        if (webViewControler == null)
        {
            Debug.LogError("[LHGFWebViewLayerControler] webViewControler 引用未设置!", this);
            return;
        }

        // 1. 显示面板
        webViewControler.OpenWebView();

        // 2. 定位与尺寸(与黑板 LHGFBlackboardLayerControler.OpenBlackboard 相同的写法)
        PlacePanel(data);

        // 3. 加载 URL(底层处理引擎未就绪的挂起;URL 为空则只打开面板不加载)
        if (!string.IsNullOrEmpty(data.Url))
        {
            webViewControler.LoadUrl(data.Url);
        }
        else
        {
            Debug.LogWarning("[LHGFWebViewLayerControler] <WebView State=\"1\"/> 未提供 Url,仅打开面板!", this);
        }
    }

    /// <summary>
    /// 仅显示并定位网页面板,不触发加载(供回撤恢复同一 URL 时使用,避免重复加载导致闪烁)。
    /// </summary>
    public void ShowWithoutReload(WebViewData data)
    {
        if (webViewControler == null)
        {
            Debug.LogError("[LHGFWebViewLayerControler] webViewControler 引用未设置!", this);
            return;
        }

        webViewControler.OpenWebView();
        PlacePanel(data);
    }

    /// <summary>
    /// 按数据定位与缩放面板。
    /// </summary>
    private void PlacePanel(WebViewData data)
    {
        RectTransform rect = PanelRect != null ? PanelRect : webViewControler.transform as RectTransform;
        if (rect != null)
        {
            LHGFData.Utils.SetSingleAnchorPoint(rect, data.Pos);   // 锚点(PosInfo 关键词已在 WebViewData 换算)
            LHGFData.Utils.SetRelativeSize(rect, data.Size);      // 相对画布尺寸
        }
        else
        {
            Debug.LogWarning("[LHGFWebViewLayerControler] 定位对象不是 RectTransform,跳过定位!", this);
        }
    }

    /// <summary>
    /// 关闭网页面板(供 LHGFWebViewLayer.Next 的 State=0 或 Reset 调用)。
    /// </summary>
    public void Close()
    {
        if (webViewControler == null)
        {
            return;
        }
        webViewControler.CloseWebView();
    }

    /// <summary>
    /// 面板可见时每帧调用:把当前鼠标位置同步给底层浏览器(透传,补偿 UWB 拖拽不跟手)。
    /// </summary>
    public void SyncMousePositionToBrowser()
    {
        if (webViewControler != null)
        {
            webViewControler.SyncMousePositionToBrowser();
        }
    }

    /// <summary>
    /// 鼠标是否停在"可见的"网页面板上(用于决定滚轮等输入是否交给网页,而非触发全局快捷键)。
    /// 面板不可见时始终返回 false(网页已关,滚轮应照常打开历史记录)。
    /// </summary>
    public bool IsMouseOverWebView()
    {
        RectTransform rect = PanelRect != null
            ? PanelRect
            : webViewControler != null ? webViewControler.transform as RectTransform : null;
        if (rect == null || !rect.gameObject.activeInHierarchy)
        {
            return false;
        }

        // 与 UWB 同一套屏幕→世界换算:camera 取面板所在 Canvas 的 worldCamera(= Main Camera,已验证)
        Camera cam = rect.GetComponentInParent<Canvas>()?.worldCamera;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, cam);
    }

    /// <summary>
    /// 每帧自检:GameMenu(含其下全部面板)激活时,把 FreeCanvas sortingOrder 压成负数,
    /// 使 StaticCanvas 的 UI(按钮/面板)在射线排序中优先于网页;
    /// 菜单关闭时恢复 0,网页回到原有排序并恢复交互。
    /// 不改菜单控制器、不改任何 UI 组件——只切 FreeCanvas 的排序层。
    /// </summary>
    public void UpdateWebViewBlocking()
    {
        if (freeCanvas == null)
        {
            // 未在 Inspector 指定时,自动取网页所在根 Canvas(FreeCanvas)
            freeCanvas = webViewControler != null
                ? webViewControler.GetComponentInParent<Canvas>()
                : null;
            if (freeCanvas == null)
            {
                return;
            }
        }

        bool menuOpen = LHGFGameMain.instance?.GameMenu != null
                        && LHGFGameMain.instance.GameMenu.gameObject.activeInHierarchy;

        int target = menuOpen ? menuOpenSortingOrder : menuClosedSortingOrder;
        if (freeCanvas.sortingOrder != target)
        {
            freeCanvas.sortingOrder = target;
        }
    }

    /// <summary>
    /// 直接加载 URL(用于读档恢复等场景;需要时上层自行先 Open)。
    /// </summary>
    public void LoadUrl(string url)
    {
        if (webViewControler == null)
        {
            return;
        }
        webViewControler.LoadUrl(url);
    }

    /// <summary>
    /// 底层网页加载完成 → 置位 Layer.IsLoaded,驱动 Finish() 放行剧情。
    /// </summary>
    private void OnPageLoaded(string url)
    {
        if (webViewLayer != null)
        {
            webViewLayer.IsLoaded = true;
        }
    }
}