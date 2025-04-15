using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TexAnimation : MonoBehaviour
{
    public TEXDraw tEXDraw;
    public float duration = 2f;
    private string targetString;
    // Start is called before the first frame update
    void Start()
    {
        tEXDraw = GetComponent<TEXDraw>();
        targetString = tEXDraw.text;
        DOTween.To(
            () => 0,                        // 起始值（0字符）
            x => tEXDraw.text = targetString.Substring(0, x), // 截取前x个字符
            targetString.Length,                // 目标字符数
            duration
        ).SetEase(Ease.Linear);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
