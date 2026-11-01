using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 远程 / 本地图片加载器。把「路径 → Sprite」这一段抽出来，
/// 供 TalkImageContent、TalkFileContent 的图标、以及 TalkBar 的头像三处复用。
///
/// 为什么需要它：
///   这三处的加载流程完全一样（解析 URL → UnityWebRequest 下载 → Texture2D.LoadImage 解码 → 建 Sprite），
///   而释放规则也一样（**只有自己建的纹理才负责销毁**，外部塞进来的 Sprite 不能动）。
///   复制三份迟早会在某一处漏掉释放，所以集中在这里。
///
/// 用法（挂载在任意 MonoBehaviour 上，由它驱动协程）：
///   loader.Load(path, sprite => { image.sprite = sprite; });
///   loader.Release();        // 换图 / 失活 / 销毁时调用
///
/// 生命周期约定：
///   · 同一个 Loader 同时只持有一张"自己创建的"Sprite，Load 新的会自动释放旧的；
///   · SetSprite(sprite, owns:false) 用于外部直接给 Sprite（自己不是所有者，Release 不会销毁它）；
///   · 调用方必须在 OnDisable / OnDestroy 里调 Release()，否则纹理泄漏。
/// </summary>
public class RemoteImageLoader
{
    /// <summary>当前持有的 Sprite（无论是自己加载的还是外部给的）。</summary>
    public Sprite Current { get; private set; }

    /// <summary>当前 Sprite 的原始像素尺寸；没有图时为 Vector2.zero。</summary>
    public Vector2 NativeSize { get; private set; }

    /// <summary>是否正在加载。可用于把加载状态纳入"这一行是否完成"的判定。</summary>
    public bool IsLoading { get; private set; }

    private readonly MonoBehaviour host;    // 用来跑协程（由调用方保证是激活的）
    private Coroutine loading;
    private bool ownsTexture;               // 纹理是不是自己建的

    public RemoteImageLoader(MonoBehaviour host)
    {
        this.host = host;
    }

    /// <summary>
    /// 真正用来跑协程的对象。
    /// 优先借用 PhoneChatLayer 的常驻控制器 —— 本 Loader 的宿主可能是某个 bar，
    /// 而 bar 在手机隐藏时（或读档还没 Show 的那一瞬间）是 inactive 的，
    /// 在它身上 StartCoroutine 会直接抛 "Coroutine couldn't be started ... is inactive"。
    /// </summary>
    private MonoBehaviour ActiveHost
    {
        get
        {
            var persistent = LHGFChatLayerControler.ResolveCoroutineHost(host);
            return persistent != null ? persistent : host;
        }
    }

    // ==================== 对外接口 ====================

    /// <summary>
    /// 按路径加载。onDone 在加载结束（成功或失败）后回调，参数为 Sprite 或 null。
    /// 会先释放上一张自己创建的纹理。
    /// </summary>
    public void Load(string nameOrPath, Action<Sprite> onDone = null)
    {
        if (host == null) return;

        if (string.IsNullOrEmpty(nameOrPath))
        {
            Release();
            onDone?.Invoke(null);
            return;
        }

        Stop();

        var runner = ActiveHost;
        if (runner == null || !runner.isActiveAndEnabled)
        {
            // 连常驻宿主都不可用（例如还没进场景）——不报错，直接当作加载失败
            Debug.LogWarning($"[RemoteImageLoader] 没有可用的协程宿主，图片未加载: {nameOrPath}");
            onDone?.Invoke(null);
            return;
        }

        IsLoading = true;
        loading = runner.StartCoroutine(LoadCoroutine(ResolveStreamingAssets(nameOrPath), onDone));
    }

    /// <summary>外部已经有 Sprite 时直接设置（owns=false 表示所有权不在自己手里，Release 不会销毁它）。</summary>
    public void SetSprite(Sprite sprite, bool owns = false)
    {
        Stop();
        ReleaseCurrent();

        Current = sprite;
        ownsTexture = owns && sprite != null;
        NativeSize = sprite != null ? sprite.rect.size : Vector2.zero;
        IsLoading = false;
    }

    /// <summary>停止加载并释放自己创建的纹理。外部给的 Sprite 只丢引用，不销毁。</summary>
    public void Release()
    {
        Stop();
        ReleaseCurrent();
    }

    // ==================== 内部 ====================

    private void Stop()
    {
        // 必须在"启动它的那个宿主"上停，否则 StopCoroutine 找不到协程、静默失效。
        // 加载总是用 ActiveHost 启动，这里也用同一个（常驻宿主通常是稳定的同一个实例）。
        if (loading != null)
        {
            var runner = ActiveHost;
            if (runner != null) runner.StopCoroutine(loading);
        }
        loading = null;
        IsLoading = false;
    }

    private void ReleaseCurrent()
    {
        // 只销毁自己建的纹理；外部传进来的 Sprite（资源、共享贴图）动了会连累别人
        if (ownsTexture && Current != null && Current.texture != null)
        {
            UnityEngine.Object.Destroy(Current.texture);
        }

        Current = null;
        ownsTexture = false;
        NativeSize = Vector2.zero;
    }

    private IEnumerator LoadCoroutine(string url, Action<Sprite> onDone)
    {
        byte[] bytes = null;

        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogError($"[RemoteImageLoader] 图片读取失败: {url} ({request.error})");
            else
                bytes = request.downloadHandler.data;
        }

        if (bytes == null)
        {
            loading = null;
            IsLoading = false;
            onDone?.Invoke(null);
            yield break;
        }

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes))
        {
            UnityEngine.Object.Destroy(texture);
            loading = null;
            IsLoading = false;
            onDone?.Invoke(null);
            yield break;
        }

        var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                                   new Vector2(0.5f, 0.5f));

        ReleaseCurrent();
        Current = sprite;
        ownsTexture = true;                          // 这个纹理是我们建的 → 由我们负责销毁
        NativeSize = sprite.rect.size;

        loading = null;
        IsLoading = false;
        onDone?.Invoke(sprite);
    }

    /// <summary>
    /// 把资源路径解析成 UnityWebRequest 能直接取用的 URL。
    ///   · 已经是完整 URL（含 "://"，例如 Android 上 streamingAssetsPath 本身就是 jar:file://...）→ 原样返回；
    ///   · 绝对路径（D:\a\b.png、/storage/emulated/0/a.png）→ 原样使用，只补 file:/// 前缀；
    ///   · 无根的相对路径（chat/cat.png）→ 拼到 Application.streamingAssetsPath 下。
    /// 本地路径一律做 URL 转义 —— 中文 / 空格不转义的话 UnityWebRequest 会直接失败。
    /// 注意：Android 上 StreamingAssets 在 APK 里，不能用 File.ReadAllBytes，必须走 UnityWebRequest。
    /// </summary>
    public static string ResolveStreamingAssets(string nameOrPath)
    {
        if (string.IsNullOrEmpty(nameOrPath)) return null;
        if (nameOrPath.Contains("://")) return nameOrPath;

        // 有根路径（完整路径）直接使用；只有无根的相对路径才拼 StreamingAssets
        string path = Path.IsPathRooted(nameOrPath)
            ? nameOrPath
            : Path.Combine(Application.streamingAssetsPath, nameOrPath);

        path = path.Replace("\\", "/");
        if (path.Contains("://")) return path;      // Android/WebGL 上 streamingAssetsPath 本身就是 URL

        // 本地路径 → 合法的 file URL（中文、空格等自动转义）
        if (Uri.TryCreate("file:///" + path, UriKind.Absolute, out var uri))
            return uri.AbsoluteUri;

        return "file:///" + path;
    }

    // ==================== 便捷：直接挂到 Image 上 ====================

    /// <summary>
    /// 给一个 Image 加载并显示图片（加载完自动设上去，失败则隐藏）。
    /// 适合"一个 Image 配一个 Loader"的简单场景；需要参与排版量测时请用 Load 自行处理回调。
    /// </summary>
    public void LoadInto(Image image, string path)
    {
        Load(path, sprite =>
        {
            if (image == null) return;
            image.sprite = sprite;
            image.enabled = sprite != null;
        });
    }
}
