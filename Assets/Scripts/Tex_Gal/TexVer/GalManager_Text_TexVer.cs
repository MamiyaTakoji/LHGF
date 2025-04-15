using DG.Tweening;
using ScenesScripts.GalPlot;
using System.Collections;
using System.Collections.Generic;
using TexDrawLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GalManager_Text_TexVer : GalManager_Text
{
    public TextComponent textComponent;
    public GameObject SkipButton;
    private string targetString;
    public float MaxTime = 2f;
    public string TextType;
    public static float skipSpeed = 0.2f;
    public override void SetText_Content(string TextContent)
    {
        textComponent.current_Text_Type = TextType;
        textComponent.text = TextContent;
    }
    public override Tweener StartTextContent(string TextContent, string CharacterName, 
        string _TextType, string CharacterIdentity, UnityAction CallBack = null)
    {
        TextType = _TextType;
        SkipButton.SetActive(true);
        void Alwayls()
        {
            SetText_CharacterName(CharacterName, CharacterIdentity);
        }
/*        if (IsSpeak && TexDraw.text.Length >= TextContent.Length * 0.75f && IsCanJump)//当前还正在发言
        {
            //但是 ，如果当前到了总文本的三分之二，也可以下一句
            SetText_Content(TextContent);
            IsSpeak = false;
            TextAnimateEvemt.Kill();
            Button_Next.SetActive(true);
            Alwayls();
            return TextAnimateEvemt;
        }*/
        if (IsSpeak) return TextAnimateEvemt;
        IsSpeak = true;
        //SetText_Content(string.Empty);//先清空内容
        textComponent.ResetText();
        textComponent.current_Text_Type = TextType;
        Button_Next.SetActive(false);
        Alwayls();
        targetString = TextContent;
        /*        TexDraw.text = targetString;
                IsSpeak = false;
                CallBack?.Invoke();
                Button_Next.SetActive(true);*/
        TextAnimateEvemt = DOTween.To(
            () => 0,                        // 起始值（0字符）
            x =>
            {
                textComponent.text = targetString.Substring(0, x);
                //Debug.Log($"Current Progress: {x}/{targetString.Length}");
                //Debug.Log($"Rendered Text: {tEXDraw.text}");
            }, // 截取前x个字符
            targetString.Length,                // 目标字符数
            Mathf.Min(TextContent.Length * (IsFastMode ? FastSpeend : DefalutSpeed), MaxTime)
        ).SetEase(Ease.Linear).OnComplete(() =>
        {
            IsSpeak = false;
            CallBack?.Invoke();
            Button_Next.SetActive(true);
            SkipButton.SetActive(false);
        });
        return TextAnimateEvemt;


    }
    public void SkipButtonControler()
    {
        if (IsSpeak && textComponent.text.Length >= targetString.Length * skipSpeed && IsCanJump&&
            !GameMain.instance.gal.IsShowingChioce)
        {
            TextAnimateEvemt.Kill();
            SetText_Content(targetString);
            IsSpeak = false;
            Button_Next.SetActive(true);
            SkipButton.SetActive(false);
        }


    }
}
