using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using TMPro;
public class TextTweener : MonoBehaviour
{
    // Start is called before the first frame update
    public enum TextType {TexDraw = 0, TMPText = 1 }
    public TextType type;
    public Dictionary<int, string> TextTypeDic = new Dictionary<int, string>() { { 0, "TexDraw" }, { 1, "TMP" } };
    private TEXDraw tEXDraw;
    private TMP_Text tMP_Text;
    private string Text_Type;
    public float MaxTime = 1.7f;
    public const float Speed = 100f;
    public Tweener SetTextTweener(string TextContent, bool Skip, UnityAction CallBack = null)
    {
        if(Skip) 
        {
            var TextAnimateEvent = DOTween.To(() => 0, x => { }, 0, 0);
            SetTextContent(TextContent, 0, TextContent.Length);
            return TextAnimateEvent;
        }
        else
        {
            float time = Mathf.Min(TextContent.Length / Speed, MaxTime);
            var TextAnimateEvent = DOTween.To(
                () => 0, 
                x => { SetTextContent(TextContent, 0, x); },
                TextContent.Length,
                time).SetEase(Ease.Linear);
            return TextAnimateEvent;
        }
    }
    public void SetTextContent(string TargetString, int L1, int L2)
    {
        if(type == TextType.TexDraw)
        {
            if (tEXDraw == null)
            {
                tEXDraw = GetComponent<TEXDraw>();
            }
            tEXDraw.text = TargetString.Substring(L1, L2);
        }
        else if(type == TextType.TMPText)
        {
            if (tMP_Text == null)
            {
                tMP_Text = GetComponent<TMP_Text>();
            }
            tMP_Text.text = TargetString.Substring(L1, L2);
        }
    }
    void Awake()
    {
        if(type == TextType.TexDraw)
        {
            tEXDraw = GetComponent<TEXDraw>();
        }
        else if(type == TextType.TMPText)
        {
            tMP_Text = GetComponent<TMP_Text>();
        }
        else
        {
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
