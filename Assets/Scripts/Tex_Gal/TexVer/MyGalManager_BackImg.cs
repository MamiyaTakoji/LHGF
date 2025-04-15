using DG.Tweening;
using ScenesScripts.GalPlot;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyGalManager_BackImg : GalManager_BackImg
{
    private Image BackImg;
    private Image TempBackImg;
    // Start is called before the first frame update
    private void Start()
    {
        BackImg = transform.Find("BackImg").gameObject.GetComponent<Image>();
        var color = BackImg.color;
        color.a = 0;
        TempBackImg = transform.Find("TempBackImg").gameObject.GetComponent<Image>();
        TempBackImg.color = color;
    }

    // Update is called once per frame
    public override Tweener SetImage(Sprite ImgSprite)
    {
        TempBackImg.sprite = BackImg.sprite;
        var color = TempBackImg.color;
        color.a = 1;
        TempBackImg.color = color;
        BackImg.sprite = ImgSprite;
        //var color = TempBackImg.color;
        /*        var BackImgAnimateEvent = DOTween.To(
                    () => 0,                        // 起始值（0字符）
                    x =>
                    {
                        Color tempColor = TempBackImg.color;
                        tempColor.a = 1 - x; // 淡出效果
                        TempBackImg.color = tempColor;
                    },
                    0.5,
                    1//时间
                );*/
        var BackImgAnimateEvent = TempBackImg.DOFade(0f, 1f);
        return BackImgAnimateEvent;
    }
}
