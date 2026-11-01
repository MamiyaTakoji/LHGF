using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class ResizeBgImage : MonoBehaviour
{
    //不知道取什么名字好了，总之用来判断是高控制宽还是宽控制高
    public float ControlRate;
    private float rate;
    public float Rate
    {
        get
        {
            if (GetComponent<Image>().sprite == null) { return 1f; }
            else
            {
                var Image = GetComponent<Image>();
                var tempRate = Image.sprite.texture.width / (float)Image.sprite.texture.height;
                if (tempRate != rate)
                {
                    _ResizeImage();
                    rate = tempRate;
                }
                return rate;
            }

        }
    }
    public void _ResizeImage()
    {
        if (GetComponent<Image>().sprite == null) return;
        var _AspectRatioFitter = GetComponent<AspectRatioFitter>();
        if (_AspectRatioFitter == null) _AspectRatioFitter = this.gameObject.AddComponent<AspectRatioFitter>();
        var Image = GetComponent<Image>();
        var imageRealRate = Image.sprite.texture.width / (float)Image.sprite.texture.height;
        _AspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        if (imageRealRate < ControlRate)//如果图片较高，需要适当拉伸图片，使其的宽高比接近屏幕的宽高比
        {
            _AspectRatioFitter.aspectRatio = ControlRate;
        }
        else
        {
            _AspectRatioFitter.aspectRatio = Image.sprite.texture.width / (float)Image.sprite.texture.height;
        }
        
    }
    private void LateUpdate()
    {
        var _ = Rate;
    }
}
