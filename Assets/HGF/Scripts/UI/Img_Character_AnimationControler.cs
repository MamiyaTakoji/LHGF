using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Img_Character_AnimationControler : MonoBehaviour
{
    // 应该同时支持固定的动作以及更多自由的动作
    //自由的动作应该能够通过动画修改角色立绘的位置以及大小
    //同时涉及修改角色位置的动画应该包含一个直接修改位置的版本
    //如果为了高自由度，这里其实不应该这么写，但是先这样吧
    public GameObject MainCanvas;
    public string Animate_StartOrOutside = "Outside-ToLeft";
    public string Animate_type = "";
    public Image Img;
    void Start()
    {
        if (MainCanvas == null) MainCanvas = GameObject.Find("Img-Characters");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Tween HandleMessage(string message)
    {
        var _rect = GetComponent<RectTransform>();
        Tweener Animate = null;
        switch (message)
        {
            case "Shake":
                {
                    Animate = _rect.DOShakePosition(0.5f, 30f);
                    break;
                }
            case "Shake-Y-Once":
                {
                    Animate = _rect.DOAnchorPosY(_rect.anchoredPosition.y - 50f, 0.6f).OnComplete(() =>
                    {
                        _rect.DOAnchorPosY(_rect.anchoredPosition.y + 50f, 0.6f);
                    });
                    break;
                }
            case "ToLeft":
                {
                    Animate = DOTween.To(() => _rect.anchoredPosition, x => _rect.GetComponent<RectTransform>().anchoredPosition = x, PositionImageInside(_rect, -1), 1f);
                    break;
                }
            case "ToCenter":
                {
                    Animate = DOTween.To(() => _rect.anchoredPosition, x => _rect.GetComponent<RectTransform>().anchoredPosition = x, PositionImageInside(_rect, 0), 0.8f);
                    break;
                }
            case "ToRight":
                {
                    Animate = DOTween.To(() => _rect.anchoredPosition, x => _rect.GetComponent<RectTransform>().anchoredPosition = x, PositionImageInside(_rect, 1), 1f);
                    break;
                }
            case "Quit":
                {
                    Animate = Img.DOFade(0, 0.7f).OnComplete(() =>
                    {
                        Destroy(this.gameObject);
                    });
                    break;
                }
            default:
                {
                    //GameAPI.Print("当前剧情文本受损，请重新安装游戏尝试", "error");
                    break;
                }
        }
        return Animate;
    }
    public Tweener HandleInOrOutsideMessgae(string Messgae)
    {
        var CharacterImg = Img;
        CharacterImg.color = new Color32(255, 255, 255, 0);//完全透明
        var rect = this.gameObject.GetComponent<RectTransform>();
        Tweener Animate = null;
        switch (Messgae)
        {

            //逐渐显示
            case "ToShow":
                {
                    PositionImageOutside(this.gameObject.GetComponent<RectTransform>(), 0);
                    Animate = CharacterImg.DOFade(1, 0.3f);
                    return Animate;
                }
            //从屏幕边缘滑到左侧
            case "Outside-ToLeft":
                {

                    PositionImageOutside(rect, -1);
                    Animate = DOTween.To(() => rect.anchoredPosition, 
                        x => rect.GetComponent<RectTransform>().anchoredPosition = x, 
                        new Vector2(rect.anchoredPosition.x + CharacterImg.rectTransform.rect.width, 
                        rect.anchoredPosition.y),
                        0.5f);
                    CharacterImg.DOFade(1, 0.3f);
                    //Animate = Animate.OnComplete(() => CharacterImg.DOFade(1, 0.7f));
                    return Animate;
                }
            //从屏幕边缘滑到右侧
            case "Outside-ToRight":
                {
                    PositionImageOutside(this.gameObject.GetComponent<RectTransform>(), 1);
                    Animate = DOTween.To(() => rect.anchoredPosition, 
                        x => rect.GetComponent<RectTransform>().anchoredPosition = x, 
                        new Vector2(rect.anchoredPosition.x - CharacterImg.sprite.texture.width, 
                        rect.anchoredPosition.y), 0.5f);
                    Animate = Animate.OnComplete(() => CharacterImg.DOFade(1, 0.3f));
                    return Animate;
                }
            default:
                {
                    Debug.Log("不使用任何动画");
                    break;
                }
        }
        //都需要指定的
            Animate = Animate.OnComplete(() => CharacterImg.DOFade(1, 0.3f));
        return Animate;
    }
    //加载时调用
    public void HandleMessageOnLoad(string message)
    {
        var _rect = GetComponent<RectTransform>();
        switch (message)
        {
            case "ToLeft":
                {
                     PositionImageOutside(_rect, -1);
                    break;
                }
            case "ToCenter":
                {
                    PositionImageInside(_rect, 0);
                    break;
                }
            case "ToRight":
                {
                    PositionImageInside(_rect, 1);
                    break;
                }
/*            case "Quit":
                {
                    //在Check方法里就应该被删除
                    Animate = Img.DOFade(0, 0.7f).OnComplete(() =>
                    {
                        Destroy(this.gameObject);
                    });
                    break;
                }*/
            default:
                {
                    //GameAPI.Print("当前剧情文本受损，请重新安装游戏尝试", "error");
                    break;
                }
        }
    }
    public void HandleInOrOutsideMessgaeOnLoad(string Messgae)
    {
        var rect = this.gameObject.GetComponent<RectTransform>();
        var CharacterImg = Img;
        if (MainCanvas == null) MainCanvas = GameObject.Find("Img-Characters");
        switch (Messgae)
        {
            //逐渐显示
            case "ToShow":
                {
                    return;
                }
            //从屏幕边缘滑到左侧
            case "Outside-ToLeft":
                {
                    PositionImageOutside(rect, -1);
                    rect.anchoredPosition
                    = new Vector2(rect.anchoredPosition.x + CharacterImg.rectTransform.rect.width, rect.anchoredPosition.y);
                    return;
                }
            //从屏幕边缘滑到右侧
            case "Outside-ToRight":
                {
                    PositionImageOutside(rect, 1);
                    rect.anchoredPosition
                    = new Vector2(rect.anchoredPosition.x - CharacterImg.rectTransform.rect.width, rect.anchoredPosition.y);
                    return;
                }
            default:
                {
                    Debug.Log("不使用任何动画");
                    break;
                }
        }
        return;
    }

    private void PositionImageOutside(RectTransform ImageGameObject, int Position)
    {
        if (MainCanvas == null) MainCanvas = GameObject.Find("Img-Characters");
        // 获取Image的Rect Transform
        switch (Position)
        {
            case -1:
                this.gameObject.GetComponent<RectTransform>().anchoredPosition 
                = new Vector2((-MainCanvas.GetComponent<RectTransform>().rect.width / 2)
                - (ImageGameObject.gameObject.GetComponent<RectTransform>().rect.width / 2), 
                ImageGameObject.anchoredPosition.y);
                break;
            case 1:
                this.gameObject.GetComponent<RectTransform>().anchoredPosition
                = new Vector2((MainCanvas.GetComponent<RectTransform>().rect.width / 2)
                +(ImageGameObject.gameObject.GetComponent<RectTransform>().rect.width / 2),
                ImageGameObject.anchoredPosition.y);
                break;
            case 0:
                this.gameObject.GetComponent<RectTransform>().anchoredPosition 
                    = new Vector2(0, ImageGameObject.anchoredPosition.y);
                break;
            default: break;
        }
    }
    private Vector2 PositionImageInside(RectTransform ImageGameObject, int Position)
    {
        // 获取Image的Rect Transform

        switch (Position)
        {
            case -1://TODO 记得改回来
                return new Vector2((-MainCanvas.GetComponent<RectTransform>().sizeDelta.x / 2) - (ImageGameObject.gameObject.GetComponent<Image>().sprite.texture.width / 2), ImageGameObject.anchoredPosition.y);

            case 1:
                return new Vector2((MainCanvas.GetComponent<RectTransform>().sizeDelta.x / 2) + (ImageGameObject.gameObject.GetComponent<Image>().sprite.texture.width / 2), ImageGameObject.anchoredPosition.y);

            case 0:
                return new Vector2(0, ImageGameObject.anchoredPosition.y);

            default:
                {
                    //GameAPI.Print("当前剧情文本受损，请重新安装游戏尝试", "error");
                    return new Vector2(0, 0);
                }
        }
    }
}
