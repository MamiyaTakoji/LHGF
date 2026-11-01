using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class ResizeImage : MonoBehaviour
{
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
                if(tempRate != rate)
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
        _AspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        var Image = GetComponent<Image>();
        _AspectRatioFitter.aspectRatio = Image.sprite.texture.width / (float)Image.sprite.texture.height;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_AspectRatioFitter.GetComponent<RectTransform>());
    }
    private void Update()
    {
        var _ = Rate;
    }
}
