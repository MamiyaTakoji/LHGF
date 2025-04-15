
using Common.Game;
using DG.Tweening;
using UnityCustom;
using UnityEngine;
using UnityEngine.UI;

namespace ScenesScripts.GalPlot
{
    public class GalManager_CharacterAnimate : MonoBehaviour
    {
        /// <summary>
        /// 出入场出场动画
        /// </summary>
        [StringInList("ToShow", "Outside-ToLeft", "Outside-ToRight")] public string Animate_StartOrOutside = "ToShow";
        /// <summary>
        /// 动画
        /// <para>Shake：颤抖</para>
        /// <para>Shake-Y-Once：向下抖动一次</para>
        /// <para>ToGrey：变灰</para>
        /// <para>To - ：不解释了，移动到指定位置</para>
        /// </summary>
        [StringInList("Shake", "Shake-Y-Once", "ToLeft", "ToCenter", "ToRight")] public string Animate_type = "Shake";
        /// <summary>
        /// 角色立绘
        /// </summary>
        public Image CharacterImg;
        public Tweener tweener;
        public Canvas MainCanvas;
        public bool IsKill = false;
        private void Awake ()
        {
            CharacterImg = this.gameObject.GetComponent<Image>();
            if (MainCanvas == null) MainCanvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
        }

        private void Start ()
        {
            tweener =  HandleInOrOutsideMessgae(Animate_StartOrOutside);
            if(IsKill)
            {
                tweener.Complete(true);
                tweener.Kill();
            }
        }

        public Tweener HandleMessgae ()
        {
            var _rect = CharacterImg.GetComponent<RectTransform>();
            Tweener Animate = null;
            switch (Animate_type)
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
                    Animate= CharacterImg.DOFade(0, 0.7f).OnComplete(() =>
                    {
                        Destroy(this.gameObject);
                    });
                    break;
                }
                default:
                {
                    GameAPI.Print("当前剧情文本受损，请重新安装游戏尝试", "error");
                    break;
                }
            }
            return Animate;

        }
        /// <summary>
        /// 处理出场动画消息
        /// </summary>
        /// <param name="Messgae"></param>
        public Tweener HandleInOrOutsideMessgae (string Messgae)
        {

            CharacterImg.color = new Color32(255, 255, 255, 0);//完全透明
            var rect = this.gameObject.GetComponent<RectTransform>();
            Tweener Animate = null;
            switch (Messgae)
            {

                //逐渐显示
                case "ToShow":
                {
                        
                    PositionImageOutside(this.gameObject.GetComponent<RectTransform>(), 0);
                    Animate = CharacterImg.DOFade(1, 0.7f);
                    return Animate;
                }
                //从屏幕边缘滑到左侧
                case "Outside-ToLeft":
                {

                    PositionImageOutside(this.gameObject.GetComponent<RectTransform>(), -1);
                    Animate = DOTween.To(() => rect.anchoredPosition, x => rect.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(rect.anchoredPosition.x + CharacterImg.sprite.texture.width, rect.anchoredPosition.y), 1f);
                    break;
                }
                //从屏幕边缘滑到右侧
                case "Outside-ToRight":
                {
                    PositionImageOutside(this.gameObject.GetComponent<RectTransform>(), 1);
                    Animate=DOTween.To(() => rect.anchoredPosition, x => rect.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(rect.anchoredPosition.x - CharacterImg.sprite.texture.width, rect.anchoredPosition.y), 1f);
                    break;
                }
                default:
                {
                    GameAPI.Print("当前剧情文本受损，请重新安装游戏尝试", "error");
                    break;
                }

            }
            //都需要指定的
            {
                Animate = Animate.OnComplete(() => CharacterImg.DOFade(1, 0.7f));
            }
            return Animate;
        }
        /// <summary>
        /// 设置image的位置到屏幕之外
        /// </summary>
        /// <param name="ImageGameObject"></param>
        /// <param name="Position">-1：左侧 0：中间 1：右侧</param>
        private void PositionImageOutside (RectTransform ImageGameObject, int Position)
        {
            // 获取Image的Rect Transform
            switch (Position)
            {
                case -1:
                    this.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2((-MainCanvas.GetComponent<RectTransform>().sizeDelta.x / 2) - (ImageGameObject.gameObject.GetComponent<Image>().sprite.texture.width / 2), ImageGameObject.anchoredPosition.y);
                    break;
                case 1:

                    this.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2((MainCanvas.GetComponent<RectTransform>().sizeDelta.x / 2) + (ImageGameObject.gameObject.GetComponent<Image>().sprite.texture.width / 2), ImageGameObject.anchoredPosition.y);
                    break;
                case 0:
                    this.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, ImageGameObject.anchoredPosition.y);
                    break;
                default: break;
            }
        }
        /// <summary>
        /// 获取image的位置到屏幕之内的位置
        /// </summary>
        /// <param name="ImageGameObject"></param>
        /// <param name="Position">-1：左侧 0：中间 1：右侧</param>
        private Vector2 PositionImageInside (RectTransform ImageGameObject, int Position)
        {
            // 获取Image的Rect Transform

            switch (Position)
            {
                case -1:
                    return new Vector2((-MainCanvas.GetComponent<RectTransform>().sizeDelta.x / 2) + (ImageGameObject.gameObject.GetComponent<Image>().sprite.texture.width / 2), ImageGameObject.anchoredPosition.y);

                case 1:
                    return new Vector2((MainCanvas.GetComponent<RectTransform>().sizeDelta.x / 2) - (ImageGameObject.gameObject.GetComponent<Image>().sprite.texture.width / 2), ImageGameObject.anchoredPosition.y);

                case 0:
                    return new Vector2(0, ImageGameObject.anchoredPosition.y);

                default:
                {
                    GameAPI.Print("当前剧情文本受损，请重新安装游戏尝试", "error");
                    return new Vector2(0, 0);
                }
            }
        }
    }

}
