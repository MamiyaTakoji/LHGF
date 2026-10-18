using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LHGFData.BlackboardLayer;
using LHGFData;
using TMPro;
using DG.Tweening;

public class LHGFBlackboardControler : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text TMPPerfab;
    public TEXDraw TexDrawPerfab;
    public GameObject ImagePerfab;
    public GameObject Content;
    public GameObject Button_Continue;
    public GameObject Button_Next;
    public List<GameObject> contents;
    void Start()
    {
        
    }
    public Tweener AddTextContent(BlackboardData data, bool IsOnLoad)
    {
        string textType = data.TextType;
        //…Ë÷√◊÷ÃÂ
        TMPPerfab.fontSize = data.FontSize;
        TexDrawPerfab.size = data.FontSize;
        GameObject G = null;
        if (!data.Continue)
        {
            Clear();
        }
        bool IsSkip;
        if (IsOnLoad) { IsSkip = true; }
        else
        {
            IsSkip = data.Skip;
        }
        if (textType == "TexDraw")
        {
            G = Instantiate(TexDrawPerfab.gameObject, Content.transform);
        }
        else if (textType == "TMP")
        {
            G = Instantiate(TMPPerfab.gameObject, Content.transform);
        }
        else if (textType == "Image")
        {
            G = Instantiate(ImagePerfab, Content.transform);
        }
        contents.Add(G);
        if (textType == "Image")
        {
            string path = data.Content;
            float rate = data.Rate;
            var ImageControler = G.GetComponent<LongSpeakImageControler>();
            ImageControler.Set(Content.transform.parent.GetComponent<RectTransform>().rect.height, rate);
            ImageControler.SetTexture();
            ImageControler.SetContentTexture(path);
            return DOTween.To(() => 0, x => { }, 0, 0);
        }
        else
        {
            return  G.GetComponent<TextTweener>().SetTextTweener(data.Content, IsSkip);
        }
    }
    public void WithdrawTextContent(List<BlackboardData> datas, bool IsClear)
    {
        if (IsClear)
        {
            Clear();
            foreach (var data in datas)
            {
                //bool IsSkip = longSpeakData.Skip;
                if (data.Content != null)
                {
                    AddTextContent(data, true);
                }
            }
        }
        else
        {
            if (datas[datas.Count - 1] != null)
            {
                Destroy(contents[contents.Count - 1].gameObject);
                contents.RemoveAt(contents.Count - 1);
            }
        }
    }
    public void Clear()
    {
        int L = contents.Count;
        for (int i = 0; i < L; i++)
        {
            Destroy(contents[i].gameObject);
        }
        contents = new() { };
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
