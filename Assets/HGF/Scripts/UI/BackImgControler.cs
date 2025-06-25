using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackImgControler : MonoBehaviour
{
    public Image BackImg;
    public Image TempBackImg;
    // Start is called before the first frame update
    void Start()
    {
        BackImg = GetComponent<Image>();
        TempBackImg = transform.Find("Temp").gameObject.GetComponent<Image>();
        var c = BackImg.color;
        c.a = 0;
        TempBackImg.color = c;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Tweener SetImage(string BackgroundPath)
    {
        TempBackImg.sprite = BackImg.sprite;
        var color = TempBackImg.color;
        color.a = 1;
        TempBackImg.color = color;
        var ImgSprite = Utils.LoadTextureByIO(BackgroundPath);
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
    public void SetImage_OnLoad(string BackgroundPath)
    {
        var ImgSprite = Utils.LoadTextureByIO(BackgroundPath);
        BackImg.sprite = ImgSprite;
    }
}
