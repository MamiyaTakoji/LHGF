using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 动图（GIF）内容块：用 UniGif 把 GIF 解成帧纹理，再用 <see cref="RawImage"/> 逐帧播放。
///
/// 数据来源：values["path"] —— 完整路径 / URL，或 StreamingAssets 下的相对路径（推荐；只有路径能存档）。
///
/// 挂载位置：TalkContent 下的一个容器物体上，和 RawImage 同一个 GameObject。
///   ⚠ 不要和 TalkImageContent 挂在同一个物体上（两者 Kind 不同，外壳切换时会互相取消激活）。
///
/// 三个关键点：
///   1. 帧延迟用每帧自己的 m_delaySec —— GIF 各帧延迟可以不同，不能用固定 fps；
///   2. 尺寸用 UniGif 回调里的逻辑宽高（不是等第一帧）—— 通过 MeasureCore 等比适配；
///   3. 只有当前激活的内容持有帧纹理 —— OnDisable 全部销毁，再激活时重新解码。
/// </summary>
[DisallowMultipleComponent]
public class TalkGifContent : TalkContentBase
{
    [Header("零件（留空自动查找）")]
    public RawImage rawImage;

    [Tooltip("等比适配的上限尺寸（0 = 用外壳的 maxWidth/maxHeight 决定的可用区域）")]
    public Vector2 maxFit = Vector2.zero;

    public override TalkContentKind Kind => TalkContentKind.Gif;

    /// <summary>解码是否还在进行（外壳/列表用它决定这一行算不算完成 —— 你要求"动图加载完"才算完成）。</summary>
    public override bool IsLoading => loading != null;

    private List<UniGif.GifTexture> frames;
    private int index;
    private float nextTime;
    private Vector2 nativeSize;
    private Coroutine loading;

    protected override void Awake()
    {
        base.Awake();
        if (rawImage == null) rawImage = GetComponentInChildren<RawImage>(true);
    }

    // ==================== 数据处理 ====================

    public override void ApplyData(TalkContentData data)
    {
        string path = data.GetString(TalkContentData.KeyPath);
        if (string.IsNullOrEmpty(path))
        {
            ClearFrames();
            MarkDirty();
            return;
        }

        Load(path);
    }

    // ==================== 加载与解码 ====================

    private void Load(string nameOrPath)
    {
        if (loading != null) StopCoroutine(loading);
        ClearFrames();
        loading = StartCoroutine(LoadCoroutine(RemoteImageLoader.ResolveStreamingAssets(nameOrPath)));
    }

    private IEnumerator LoadCoroutine(string url)
    {
        byte[] bytes = null;

        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogError($"[{nameof(TalkGifContent)}] GIF 读取失败: {url} ({request.error})", this);
            else
                bytes = request.downloadHandler.data;
        }

        if (bytes == null)
        {
            loading = null;
            yield break;
        }

        // 回调参数：(帧列表, 循环次数, GIF 逻辑宽, GIF 逻辑高)
        yield return UniGif.GetTextureListCoroutine(bytes, (list, loopCount, width, height) =>
        {
            frames = list;
            nativeSize = new Vector2(width, height);
        });

        if (frames == null || frames.Count == 0)
        {
            Debug.LogError($"[{nameof(TalkGifContent)}] GIF 解码失败: {url}", this);
            loading = null;
            yield break;
        }

        index = 0;
        var first = frames[0];
        if (rawImage != null)
        {
            rawImage.texture = first.m_texture2d;
            rawImage.enabled = true;
        }
        nextTime = Time.unscaledTime + first.m_delaySec;

        loading = null;
        MarkDirty();          // ★ 尺寸到手，让外壳按真实比例重排
    }

    // ==================== 播放 ====================

    private void Update()
    {
        if (frames == null || frames.Count < 2 || rawImage == null) return;
        if (Time.unscaledTime < nextTime) return;

        index++;
        if (index >= frames.Count) index = 0;

        var frame = frames[index];
        rawImage.texture = frame.m_texture2d;
        nextTime = Time.unscaledTime + frame.m_delaySec;   // ★ 用帧自带延迟，不是 1/fps
    }

    // ==================== 量测 ====================

    protected override Vector2 MeasureCore(Vector2 available)
    {
        if (nativeSize.x <= 0.01f || nativeSize.y <= 0.01f)
            return new Vector2(120f, 120f);      // 还没解码完：先给个占位，解码完 MarkDirty 再重排

        Vector2 limit = available;
        if (maxFit.x > 0.01f) limit.x = Mathf.Min(limit.x, maxFit.x);
        if (maxFit.y > 0.01f) limit.y = Mathf.Min(limit.y, maxFit.y);

        float k = Mathf.Min(1f, limit.x / nativeSize.x, limit.y / nativeSize.y);   // 只缩不放
        return nativeSize * k;
    }

    // ==================== 释放 ====================

    /// <summary>销毁所有帧纹理（一帧一张 Texture2D，不销毁会一直占内存）。</summary>
    private void ClearFrames()
    {
        if (frames != null)
        {
            foreach (var frame in frames)
                if (frame != null && frame.m_texture2d != null) Destroy(frame.m_texture2d);
            frames = null;
        }

        index = 0;
        nativeSize = Vector2.zero;

        if (rawImage != null)
        {
            rawImage.texture = null;
            rawImage.enabled = false;
        }
    }

    /// <summary>只有当前激活的内容持有帧纹理：切走 / 失活就释放，再激活时重新解码。</summary>
    protected override void OnDisable()
    {
        base.OnDisable();      // ★ 必须先调基类：外壳靠它 Unregister，否则 current 会一直指向已失活的内容
        if (loading != null)
        {
            StopCoroutine(loading);
            loading = null;
        }
        ClearFrames();
    }

    private void OnDestroy()
    {
        ClearFrames();
    }
}
