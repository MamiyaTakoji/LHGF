using DG.Tweening;
using LHGFData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChapterLayerControler : MonoBehaviour
{
    public float ShowTime = 2f;       // 图片完全显示后停留的时间
    public float FadeInDuration = 0.1f; // 淡入时长
    public float FadeOutDuration = 1f;// 淡出时长
    public Image ChapterImage;
    public bool IsAnimationFinished = true;

    private DG.Tweening.Sequence currentSequence;

    public void Start()
    {
        ChapterImage = GetComponent<Image>();
    }
    public void KillAnimation()
    {
        //停止正在播放的章节动画，立即隐藏并恢复可推进状态
        currentSequence.Kill();
        IsAnimationFinished = true;
        gameObject.SetActive(false);
    }
    public void ShowChapter(string imagePath)
    {
        if(ChapterImage == null)
        {
            ChapterImage = GetComponent<Image>();
        }
        var s = Utils.LoadTextureByIO(imagePath);
        ChapterImage.sprite = s;
        IsAnimationFinished = false;

        ChapterImage.color = new Color(1, 1, 1, 0);
        ChapterImage.gameObject.SetActive(true);

        // 3. 停止之前的动画（如果有）
        currentSequence.Kill();

        // 4. DOTween 序列：淡入 → 停留 → 淡出
        currentSequence = DOTween.Sequence();
        currentSequence
            .Append(ChapterImage.DOFade(1f, FadeInDuration))       // 逐渐显现
            .AppendInterval(ShowTime)                               // 停留
            .AppendCallback(() =>
            {
                //淡出开始：标记动画完成并自动跳转下一节点，实现无缝衔接
                IsAnimationFinished = true;
                if (LHGFGameMain.instance != null)
                {
                    LHGFGameMain.instance.Forward();
                }
            })
            .Append(ChapterImage.DOFade(0f, FadeOutDuration))      // 逐渐消失
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}
