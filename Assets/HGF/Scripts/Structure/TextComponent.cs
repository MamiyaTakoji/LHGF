using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextComponent : MonoBehaviour
{
    public TEXDraw texDraw;
    public TMP_Text textTMP;
    public string current_Text_Type;//可选项目前为TexDraw和TMP
    // Start is called before the first frame update
    public string text
    {
        get
        {
            if (current_Text_Type == "TexDraw")
            {
                return texDraw.text;
            }
            else if (current_Text_Type == "TMP")
            {
                return textTMP.text;
            }
            else
            {
                //默认返回TexDraw吧
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
                //默认返回TexDraw吧
                texDraw.text = value;
                textTMP.text = string.Empty;
            }
        }
    }
    
    public void ResetText()
    {
        texDraw.text = string.Empty;
        textTMP.text = string.Empty;
    }
}
