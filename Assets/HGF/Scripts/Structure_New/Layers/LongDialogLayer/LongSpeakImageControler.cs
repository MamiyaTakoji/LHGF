using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LHGFData;
public class LongSpeakImageControler : MonoBehaviour
{
    //LongSpeakImage的工作原理为：
    //  · rate：图片区域高度 = 可用高度 × rate（rate 只控制"高度上限"）
    //  · 图片区域宽度 = 可用宽度
    //  · 真实图片按原始宽高比等比缩放后放进这个区域，取其中最紧凑的一种，短边不浪费
    //  · 根物体上挂 LayoutElement，让 VerticalLayoutGroup 按这个高度排版，
    //    而不是被子物体 ContentImage 的高度反过来撑开（旧做法会导致竖图一路撑出屏幕）
    public float rate = 0.2f;
    public float ParentHigh;
    public float ParentWidth;
    public Image ContentImage;

    public void Set(float parentHigh, float parentWidth, float _rate)
    {
        ParentHigh = parentHigh;
        ParentWidth = parentWidth;
        rate = _rate;
    }

    /// <summary>
    /// 按 rate 和父级尺寸算出本节点的显示尺寸，写进 RectTransform 与 LayoutElement。
    /// 必须在 Set 之后、SetContentTexture 之前调用。
    /// </summary>
    public void SetTexture()
    {
        float maxWidth = ParentWidth > 0f ? ParentWidth : 600f;
        float maxHeight = ParentHigh * (rate > 0f ? rate : 1f);
        if (maxHeight <= 0f) maxHeight = 300f;

        var rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.sizeDelta = new Vector2(maxWidth, maxHeight);

        // 让父级的 VerticalLayoutGroup 按这个高度排版（否则会被子物体重新撑高）
        var layout = GetComponent<LayoutElement>();
        if (layout == null) layout = gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = maxWidth;
        layout.preferredHeight = maxHeight;
        layout.flexibleHeight = 0f;

        // 根物体只做占位，不需要显示任何贴图
        var img = GetComponent<Image>();
        if (img != null)
        {
            var c = img.color;
            c.a = 0f;
            img.color = c;
            img.sprite = null;
        }

        ApplyContentSize(maxWidth, maxHeight);
    }

    public void SetContentTexture(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            Debug.LogError("[LongSpeakImageControler] 图片路径为空");
            return;
        }
        if (ContentImage == null)
        {
            Debug.LogError($"[LongSpeakImageControler] 预制体未绑定 ContentImage，无法显示：{imagePath}");
            return;
        }

        var s = Utils.LoadTextureByIO(imagePath);
        if (s == null)
        {
            Debug.LogError($"[LongSpeakImageControler] 图片加载失败：{imagePath}");
            return;
        }

        ContentImage.sprite = s;
        ContentImage.preserveAspect = true;

        float maxWidth = ParentWidth > 0f ? ParentWidth : 600f;
        float maxHeight = ParentHigh * (rate > 0f ? rate : 1f);
        if (maxHeight <= 0f) maxHeight = 300f;
        ApplyContentSize(maxWidth, maxHeight);
    }

    /// <summary>把子物体 ContentImage 缩放到不超过可用区域的最大等比尺寸。</summary>
    private void ApplyContentSize(float maxWidth, float maxHeight)
    {
        if (ContentImage == null) return;
        var sprite = ContentImage.sprite;
        if (sprite == null) return;

        float spriteW = sprite.rect.width;
        float spriteH = sprite.rect.height;
        if (spriteW <= 0f || spriteH <= 0f) return;

        float scale = Mathf.Min(maxWidth / spriteW, maxHeight / spriteH);
        var contentRt = ContentImage.rectTransform;
        contentRt.sizeDelta = new Vector2(spriteW * scale, spriteH * scale);
        contentRt.anchorMin = new Vector2(0.5f, 0.5f);
        contentRt.anchorMax = new Vector2(0.5f, 0.5f);
        contentRt.anchoredPosition = Vector2.zero;
    }
}
