using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResizeImage : MonoBehaviour
{
    public void _ResizeImage()
    {
        if (GetComponent<Image>().sprite == null) return;
        var _AspectRatioFitter = GetComponent<AspectRatioFitter>();
        if (_AspectRatioFitter == null) _AspectRatioFitter = this.gameObject.AddComponent<AspectRatioFitter>();
        _AspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        var Image = GetComponent<Image>();
        _AspectRatioFitter.aspectRatio = Image.sprite.texture.width / (float)Image.sprite.texture.height;
    }
}
