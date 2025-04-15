using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextComponent : MonoBehaviour
{
    public TEXDraw texDraw;
    public TMP_Text textTMP;
    public string text { 
        get 
        {  
            if (current_Text_Type == "TexDraw")
            {
                return texDraw.text;
            }
            else if(current_Text_Type == "TMP")
            {
                return textTMP.text;
            }
            else
            {
                //Ĭ�Ϸ���TexDraw��
                return texDraw.text;
            }
        }
        set
        {
            if (current_Text_Type == "TexDraw")
            {
                texDraw.text = value;
                textTMP.text = string.Empty;
            }
            else if (current_Text_Type == "TMP")
            {
                textTMP.text = value;
                texDraw.text = string.Empty;
            }
            else
            {
                //Ĭ�Ϸ���TexDraw��
                texDraw.text = value;
                textTMP.text = string.Empty;
            }
        }
    }
    public string current_Text_Type;//��ѡ��ĿǰΪTexDraw��TMP
    public void ResetText()
    {
        texDraw.text = string.Empty;
        textTMP.text = string.Empty;
    }
    public void ResetTransform(bool IsFresh = true)
    {
        if (current_Text_Type == "TexDraw")
        {
            //��������TMP��λ��
            var newPos = texDraw.GetComponent<RectTransform>().anchoredPosition;
            newPos.y = -texDraw.GetComponent<RectTransform>().rect.height / 2;
            texDraw.GetComponent<RectTransform>().anchoredPosition = newPos;//����TexDraw��λ��
            //LayoutRebuilder.ForceRebuildLayoutImmediate(texDraw.GetComponent<RectTransform>());
            // ��ȡ��ǰ����� RectTransform
            RectTransform currentRect = GetComponent<RectTransform>();
            // ����Ŀ�� RectTransform �����Ե���ǰ����
            currentRect.anchoredPosition = texDraw.rectTransform.anchoredPosition;
            currentRect.sizeDelta = texDraw.rectTransform.sizeDelta;
            currentRect.rotation = texDraw.rectTransform.rotation;
            if (IsFresh)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(texDraw.GetComponent<RectTransform>());
            }
            
        }
        else if (current_Text_Type == "TMP")
        {
            RectTransform currentRect = GetComponent<RectTransform>();

            // ����Ŀ�� RectTransform �����Ե���ǰ����
            currentRect.anchoredPosition = textTMP.rectTransform.anchoredPosition;
            currentRect.sizeDelta = textTMP.rectTransform.sizeDelta;
            currentRect.rotation = textTMP.rectTransform.rotation;
        }
        else
        {
            // ��ȡ��ǰ����� RectTransform
            RectTransform currentRect = GetComponent<RectTransform>();

            // ����Ŀ�� RectTransform �����Ե���ǰ����
            currentRect.anchoredPosition = texDraw.rectTransform.anchoredPosition;
            currentRect.sizeDelta = texDraw.rectTransform.sizeDelta;
            currentRect.rotation = texDraw.rectTransform.rotation;
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
