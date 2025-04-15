using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TexDrawLib;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class GalManager_LongSpeak : MonoBehaviour
{
    public const float DefalutSpeed = 0.045f;
    public const float FastSpeend = 0.02f;
    public static float skipSpeed = 0.2f;
    public float MaxTime = 5f;//�������5����Ҫ������
    public TMP_Text TMP_Text_Content;
    public TMP_Text TMP_Text_Transformer;
    public TEXDraw tEXDraw_Content;
    public TEXDraw tEXDraw_Transformer;
    public int CurrentLen = 0;
    public string targetString = "";
    private string _IsEnd;
    public string TextType;
    /// <summary>
    /// ��ǰ�Ƿ�������
    /// </summary>
    public static bool IsFastMode;
    public GameObject SkipButton;
    /// <summary>
    /// ��ǰ�Ƿ����ڷ���
    /// ���Ϊ������Կ�ʼ��һ��
    /// ������ı��������ʱ��ҲΪTrue
    /// </summary>
    public static bool IsSpeak;

    /// <summary>
    /// �ı����ݴ��ֻ������¼�
    /// </summary>
    public static Tweener TextAnimateEvemt;

    /// <summary>
    ///�Ƿ�������� 
    /// </summary>
    public static bool IsCanJump = true;

    /// <summary>
    /// �Ի������½ǵ���һ����ʾ
    /// </summary>
    public GameObject Button_Next;
    public GameObject Button_Contiune;
    /// <summary>
    /// �Ի����Ƿ�ɼ�
    /// </summary>
    public void SetDialogHide(bool value = false)
    {
        this.gameObject.SetActive(value);
    }
    /// <summary>
    /// ���öԻ�����
    /// </summary>
    /// <param name="TextContent"></param>
    public virtual void SetText_Content()
    {
        if(TextType == "TexDraw")
        {
            tEXDraw_Content.text = targetString;
            tEXDraw_Transformer.text = targetString;
            ResetTransformer(tEXDraw_Transformer.gameObject);
        }
        else
        {
            TMP_Text_Content.text = targetString;
            TMP_Text_Transformer.text = targetString;
            ResetTransformer(TMP_Text_Transformer.gameObject);
        }
    }
    public void SetText_Content_Animation(int L1, int L2)
    {
        if (TextType == "TexDraw")
        {
            tEXDraw_Content.text = targetString.Substring(L1, L2);
            tEXDraw_Transformer.text  = targetString.Substring(L1, L2);
            ResetTransformer(tEXDraw_Transformer.gameObject);
        }
        else
        {
            TMP_Text_Transformer.text = targetString.Substring(L1, L2);
            TMP_Text_Content.text = targetString.Substring(L1, L2);
            ResetTransformer(TMP_Text_Transformer.gameObject);
        }
    }
    /// <summary>
    /// ���÷����˵�����
    /// </summary>
/*    public void SetText_CharacterName(string CharacterName, string CharacterIdentity)
    {

        Text_CharacterName.text = $"<b>{CharacterName}</b><size=45>     <color=#F684EE>{CharacterIdentity}</color></size>";
    }*/
    /// <summary>
    /// ��ʼ����
    /// </summary>
    /// <param name="TextContent">�ı�����</param>
    /// <param name="CharacterName">����������</param>
    /// <param name="CharacterIdentity">����������</param>
    /// <param name="CallBack">�ص��¼�</param>
    /// <returns></returns>
    public virtual Tweener StartTextContent(string TextContent,string IsContiune, string IsEnd,
        string _TextType, string Skip, out string FinalTextContent, UnityAction CallBack = null)
    {
        //100  60   40
        /*        void Alwayls()
                {

                    SetText_CharacterName(CharacterName, CharacterIdentity);

                }*/
        TextType = _TextType;
        _IsEnd = IsEnd;
        SkipButton.SetActive(true);
        if(TextType == "TexDraw") { TextContent = "\n\n" + TextContent; }
        if(TextType == "TMP"&&IsContiune == "1") { TextContent = "\n" + TextContent; }
        //TextContent = TextContent.Replace("\r\n", "\n").Replace("\r", "\n");
        if (TextType == "TexDraw") { TextContent = TextContent.Replace("@@", "\n\n"); }
        if (TextType == "TMP") { TextContent = TextContent.Replace("@@", "\n"); }
        FinalTextContent = TextContent;
        if (IsSpeak) return TextAnimateEvemt;
        IsSpeak = true;
        ChangeButtonNext();
        if(IsContiune == "0")
        {
            TMP_Text_Content.text = string.Empty;//���������
            TMP_Text_Transformer.text = string.Empty;
            tEXDraw_Content.text = string.Empty;
            tEXDraw_Transformer.text = string.Empty;
            targetString = string.Empty;
            CurrentLen = 0;
            SetButtonNext();
        }
        ResetNextButton();
        targetString += TextContent;
        //SetText_Content(targetString);
        
        Button_Next.SetActive(false);
        Button_Contiune.SetActive(false);
        //Alwayls();

        /*        TexDraw.text = targetString;
                IsSpeak = false;
                CallBack?.Invoke();
                Button_Next.SetActive(true);*/
        int startValue = 0;
        float time = Mathf.Min(TextContent.Length * (IsFastMode ? FastSpeend : DefalutSpeed), MaxTime);
        if (Skip == "1")
        {
            startValue = TextContent.Length;
            time = 0.1f;
        }
        

        TextAnimateEvemt = DOTween.To(
            () => startValue,                        // ��ʼֵ��0�ַ���
            x =>
            {
/*                textComponent.text = targetString.Substring(0, x+ CurrentLen);// ��ȡǰx���ַ�
*//*                var newPos = textComponent.GetComponent<RectTransform>().anchoredPosition;
                newPos.y = -textComponent.GetComponent<RectTransform>().rect.height/2;
                textComponent.GetComponent<RectTransform>().anchoredPosition = newPos;//����TexDraw��λ��*//*
                textComponent.ResetTransform(false);
                //Debug.Log(TexDraw.GetComponent<RectTransform>().rect.height);*/
                SetText_Content_Animation(0,x+CurrentLen);
                //��Ҫ����Transform��λ��
            },
            TextContent.Length,                // Ŀ���ַ���
            time
        ).SetEase(Ease.Linear).OnComplete(() =>
        {
            IsSpeak = false;
            CallBack?.Invoke();
            if (IsEnd == "0") 
            { 
                Button_Contiune.SetActive(true);
                Button_Next.SetActive(false);
            }
            else
            {
                Button_Next.SetActive(true);
                Button_Contiune.SetActive(false);
            }
            CurrentLen = CurrentLen + TextContent.Length;
            SkipButton.SetActive(false);
            ResetTransformer(TMP_Text_Transformer.gameObject);
            ResetTransformer(tEXDraw_Transformer.gameObject);
        });
        return TextAnimateEvemt;

    }
    public void SkipButtonControler()
    {
        if (IsSpeak && Mathf.Max(TMP_Text_Content.text.Length,tEXDraw_Content.text.Length
            ) >= targetString.Length * skipSpeed && IsCanJump)
        {
            TextAnimateEvemt.Kill();
            SetText_Content();
            IsSpeak = false;
            //Button_Next.SetActive(true);
            SkipButton.SetActive(false);
            //LayoutRebuilder.ForceRebuildLayoutImmediate(textComponent.GetComponent<RectTransform>());
            /*        var newPos = textComponent.GetComponent<RectTransform>().anchoredPosition;
                    newPos.y = -textComponent.GetComponent<RectTransform>().rect.height / 2;
                    textComponent.GetComponent<RectTransform>().anchoredPosition = newPos;//����TexDraw��λ��*/
            //�������ù��λ��
            //textComponent.ResetTransform();
            //LayoutRebuilder.ForceRebuildLayoutImmediate(textComponent.GetComponent<RectTransform>());
            CurrentLen = targetString.Length;
            if (_IsEnd == "0") { Button_Contiune.SetActive(true); }
            else { Button_Next.SetActive(true); }
            ResetTransformer(TMP_Text_Transformer.gameObject);
            ResetTransformer(tEXDraw_Transformer.gameObject);
        }
    }
    public void ResetTransformer(GameObject g)
    {
        var newPos = g.GetComponent<RectTransform>().anchoredPosition;
        newPos.y = -g.GetComponent<RectTransform>().rect.height / 2;
        g.GetComponent<RectTransform>().anchoredPosition = newPos;//����TexDraw��λ��
        LayoutRebuilder.ForceRebuildLayoutImmediate(g.GetComponent<RectTransform>());
    }
    public void SetButtonNext()
    {
        ResetNextButton();
        ChangeButtonNext();
    }
    public void ChangeButtonNext()
    {
        if (TextType == "TexDraw")
        {
            Button_Next = tEXDraw_Transformer.transform.Find("Button_Next").gameObject;
            Button_Contiune = tEXDraw_Transformer.transform.Find("Button_Continue").gameObject;
        }
        else
        {
            Button_Next = TMP_Text_Transformer.transform.Find("Button_Next").gameObject;
            Button_Contiune = TMP_Text_Transformer.transform.Find("Button_Continue").gameObject;
        }
    }
    public void ResetNextButton()
    {
        tEXDraw_Transformer.transform.Find("Button_Next").gameObject.SetActive(false);
        tEXDraw_Transformer.transform.Find("Button_Continue").gameObject.SetActive(false);
        TMP_Text_Transformer.transform.Find("Button_Next").gameObject.SetActive(false);
        TMP_Text_Transformer.transform.Find("Button_Continue").gameObject.SetActive(false);
    }
}
