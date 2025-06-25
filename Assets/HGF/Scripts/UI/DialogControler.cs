using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using TMPro;


public class DialogControler : MonoBehaviour
{
    public TextComponent textComponent;
    public GameObject SkipButton;
    public GameObject NextButton;
    private string targetString;
    public float MaxTime = 2f;
    public string TextType;
    public static float skipSpeed = 0.2f;
    public TMP_Text SpeakInfo;
    public float AnimationSpeed = 0.08f;
    public static Tweener TextAnimateEvent;
    public AudioSource CharacterVoice;
    //必须给出两个版本，其中一个版本播放动画，另一个版本直接更新文本
    public Tweener StartTextContent(string TextContent, string CharacterName,
        string _TextType, string CharacterIdentity,bool IsSkip ,UnityAction CallBack = null)
    {
        TextType = _TextType;
        textComponent.ResetText();
        textComponent.current_Text_Type = TextType;
        SetText_CharacterName(CharacterName, CharacterIdentity);
        targetString = TextContent;
        int InitialLen = 0;
        float FinishTime = Mathf.Min(TextContent.Length * AnimationSpeed, MaxTime);
        if (IsSkip) { InitialLen = targetString.Length; FinishTime = 0.1f; }
        NextButton.SetActive(false);
        TextAnimateEvent = DOTween.To(
            () => InitialLen,                        // 起始值（0字符）
            x =>
            {
                textComponent.text = targetString.Substring(0, x);
                //Debug.Log($"Current Progress: {x}/{targetString.Length}");
                //Debug.Log($"Rendered Text: {tEXDraw.text}");
            }, // 截取前x个字符
            targetString.Length,                // 目标字符数
            FinishTime
        ).SetEase(Ease.Linear);
        TextAnimateEvent.onComplete += (() => { NextButton.SetActive(true); });
        return TextAnimateEvent;
    }
    public void QuickSetTextContent(string TextContent, string CharacterName,
        string _TextType, string CharacterIdentity)
    {
        TextType = _TextType;
        textComponent.ResetText();
        textComponent.current_Text_Type = TextType;
        SetText_CharacterName(CharacterName, CharacterIdentity);
        targetString = TextContent;
        SetText_Content(targetString);
    }
    public void SetText_CharacterName(string CharacterName, string CharacterIdentity)
    {

        SpeakInfo.text = $"<b><size=+120%>{CharacterName}</size></b>    <color=#F684EE>{CharacterIdentity}</color>";
    }
    public void SetText_Content(string TextContent)
    {
        textComponent.current_Text_Type = TextType;
        textComponent.text = TextContent;
    }
    public void Skip()
    {
        TextAnimateEvent.Kill();
        SetText_Content(targetString);
    }
}
