using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class LongDialogControler : MonoBehaviour
{
    public TMP_Text TMP_Text_Content;
    public TMP_Text TMP_Text_Transformer;
    public TEXDraw tEXDraw_Content;
    public TEXDraw tEXDraw_Transformer;
    public int CurrentLen = 0;
    public string TextType = "TexDraw";
    public string targetString = "";
    public GameObject Button_Next;
    public GameObject Button_Contiune;
    public static Tweener TextAnimateEvent;
    public float MaxTime = 1.7f;
    public const float Speed = 0.045f;
    public virtual void SetText_Content()
    {
        if (TextType == "TexDraw")
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
            tEXDraw_Transformer.text = targetString.Substring(L1, L2);
            ResetTransformer(tEXDraw_Transformer.gameObject);
        }
        else
        {
            TMP_Text_Transformer.text = targetString.Substring(L1, L2);
            TMP_Text_Content.text = targetString.Substring(L1, L2);
            ResetTransformer(TMP_Text_Transformer.gameObject);
        }
    }
    public Tweener StartTextContent(string TextContent, string IsContiune, string IsEnd,
    string _TextType, bool Skip, List<string> TotalContent,out string FinalTextContent, UnityAction CallBack = null)
    {
        TextType = _TextType;
        var _FinalTextContent = BuildStringContent(TextContent,_TextType);
        FinalTextContent = _FinalTextContent;
        ChangeButtonNext();
        if (IsContiune == "0")
        {
            TMP_Text_Content.text = string.Empty;//先清空内容
            TMP_Text_Transformer.text = string.Empty;
            tEXDraw_Content.text = string.Empty;
            tEXDraw_Transformer.text = string.Empty;
            targetString = string.Empty;
            CurrentLen = 0;
            SetButtonNext();
        }
        ResetNextButton();
        targetString = BuildStringContent(TotalContent, TextType);
        if(Skip)
        {
            SetText_Content();
            TextAnimateEvent = DOTween.To(() => 0, x => { }, 0, 0);
            TextAnimateEvent.onComplete +=
            (() => {
                if (IsEnd == "1")
                {
                    Button_Next.SetActive(true);
                }
                else
                {
                    Button_Contiune.SetActive(true);
                }
                ResetTransformer(tEXDraw_Transformer.gameObject);
                ResetTransformer(TMP_Text_Transformer.gameObject);
            });
            return TextAnimateEvent;
        }
        float time = Mathf.Min(TextContent.Length * Speed,MaxTime);
        int temp = targetString.Length - TextContent.Length;
        TextAnimateEvent = DOTween.To(
            () => 0,                        // 起始值（0字符）
            x =>
            {
                SetText_Content_Animation(0, Mathf.Min(x + temp , targetString.Length));
            },
            TextContent.Length,                // 目标字符数
            time
        ).SetEase(Ease.Linear);
        TextAnimateEvent.onComplete += 
        (() => {
            if(IsEnd == "1")
            {
                Button_Next.SetActive(true);
            }
            else
            {
                Button_Contiune.SetActive(true);
            }
        });
        return TextAnimateEvent;
    }
    public string BuildStringContent(List<string> texts, string TextType)
    {
        string _targetString = string.Empty;
        if (TextType == "TexDraw")
        {
            foreach (string text in texts)
            {
                _targetString += "\n\n" + text;
            }
            _targetString = _targetString.Replace("@@", "\n\n");
        }
        else
        {
            foreach (string text in texts)
            {
                _targetString += "\n" + text;
            }
            _targetString = _targetString.Replace("@@", "\n");
        }
        return _targetString;
    }
    public string BuildStringContent(string text, string TextType)
    {
        if (TextType == "TexDraw")
        {
             text = "\n\n" + text;
             text = text.Replace("@@", "\n\n");
        }
        else
        {
            text = "\n" + text;
            text = text.Replace("@@", "\n");
        }
        return text;
    }
    public void ResetNextButton()
    {
        tEXDraw_Transformer.transform.Find("Button_Next").gameObject.SetActive(false);
        tEXDraw_Transformer.transform.Find("Button_Continue").gameObject.SetActive(false);
        TMP_Text_Transformer.transform.Find("Button_Next").gameObject.SetActive(false);
        TMP_Text_Transformer.transform.Find("Button_Continue").gameObject.SetActive(false);
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
    public void ResetTransformer(GameObject g)
    {
        var newPos = g.GetComponent<RectTransform>().anchoredPosition;
        newPos.y = -g.GetComponent<RectTransform>().rect.height / 2;
        g.GetComponent<RectTransform>().anchoredPosition = newPos;//更新TexDraw的位置
        LayoutRebuilder.ForceRebuildLayoutImmediate(g.GetComponent<RectTransform>());
    }
}
