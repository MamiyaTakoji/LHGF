using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 静态图片内容块：一张 <see cref="Image"/>，按比例适配气泡可用尺寸。
///
/// 数据来源（二选一）：
///   · values["sprite"] —— 运行时直接给 Sprite（不会释放它的纹理）
///   · values["path"]   —— 完整路径 / URL / StreamingAssets 相对路径，自己读进来解码（**由本组件创建的纹理会释放**）
///
/// 挂载位置：TalkContent 下的一个容器物体上，和 Image 同一个 GameObject。
///   ⚠ 不要和 TalkGifContent 挂在同一个物体上（两者 Kind 不同，外壳切换时会互相取消激活）。
///
/// 尺寸：MeasureCore 只缩不放 —— 原图比可用区域大就等比缩到放得下，比它小就保持原尺寸。
/// 加载与释放统一交给 <see cref="RemoteImageLoader"/>。
/// </summary>
[DisallowMultipleComponent]
public class TalkImageContent : TalkContentBase
{
    [Header("零件（留空自动查找）")]
    public Image image;

    [Tooltip("等比适配的上限尺寸（0 = 用外壳的 maxWidth/maxHeight 决定的可用区域）")]
    public Vector2 maxFit = Vector2.zero;

    public override TalkContentKind Kind => TalkContentKind.Image;

    private RemoteImageLoader loader;

    /// <summary>图片是否还在加载（列表可用它决定这一行算不算完成）。</summary>
    public override bool IsLoading => loader != null && loader.IsLoading;

    protected override void Awake()
    {
        base.Awake();
        if (image == null) image = GetComponentInChildren<Image>(true);
        loader = new RemoteImageLoader(this);
    }

    public override void ApplyData(TalkContentData data)
    {
        EnsureLoader();

        // ① 直接给 Sprite 的情况（所有权不在自己手里）
        Sprite sprite = data.GetSprite(TalkContentData.KeySprite);
        if (sprite != null)
        {
            loader.SetSprite(sprite);
            ApplySpriteToImage(loader.Current);
            MarkDirty();
            return;
        }

        // ② 给资源路径的情况（完整路径 / URL / StreamingAssets 相对路径都行）
        string path = data.GetString(TalkContentData.KeyPath);
        if (string.IsNullOrEmpty(path))
        {
            loader.Release();
            ApplySpriteToImage(null);
            MarkDirty();
            return;
        }

        // 加载期间先清空显示，避免残留上一条消息的图
        ApplySpriteToImage(null);
        loader.Load(path, _ =>
        {
            ApplySpriteToImage(loader.Current);
            MarkDirty();          // ★ 尺寸到手，让外壳按真实比例重排
        });
    }

    private void EnsureLoader()
    {
        if (loader == null) loader = new RemoteImageLoader(this);
    }

    private void ApplySpriteToImage(Sprite sprite)
    {
        if (image == null) return;
        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    // ==================== 量测 ====================

    protected override Vector2 MeasureCore(Vector2 available)
    {
        Vector2 nativeSize = loader != null ? loader.NativeSize : Vector2.zero;
        if (nativeSize.x <= 0.01f || nativeSize.y <= 0.01f)
            return new Vector2(120f, 120f);      // 还没加载完：先给个占位，加载完 MarkDirty 再重排

        Vector2 limit = available;
        if (maxFit.x > 0.01f) limit.x = Mathf.Min(limit.x, maxFit.x);
        if (maxFit.y > 0.01f) limit.y = Mathf.Min(limit.y, maxFit.y);

        float k = Mathf.Min(1f, limit.x / nativeSize.x, limit.y / nativeSize.y);   // 只缩不放
        return nativeSize * k;
    }

    // ==================== 释放 ====================

    /// <summary>只有当前激活的内容持有纹理：切走 / 失活就释放，再激活时重新加载。</summary>
    protected override void OnDisable()
    {
        base.OnDisable();      // ★ 必须先调基类：外壳靠它 Unregister，否则 current 会一直指向已失活的内容
        loader?.Release();
        ApplySpriteToImage(null);
    }

    private void OnDestroy()
    {
        loader?.Release();
    }
}
