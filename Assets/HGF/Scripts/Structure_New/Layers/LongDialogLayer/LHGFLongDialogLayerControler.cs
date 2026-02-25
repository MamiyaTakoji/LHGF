using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHGFData;
using static TextTweener;
using DG.Tweening;
using static LHGFData.LongDialogLayer;
using System.IO;

public class LHGFLongDialogLayerControler : MonoBehaviour
{
    public GameObject TMPPerfab;
    public GameObject TexDrawPerfab;
    public GameObject ImagePerfab;
    public GameObject Content;
    public GameObject Button_Continue;
    public GameObject Button_Next;
    public LongDialogLayer longDialogLayer;
    public List<GameObject> contents;
    public void Awake()
    {
        contents = new() { };
    }
    public void _AddTextContent(LongSpeakData longSpeakData, bool IsSkip)
    {
        string textType = longSpeakData.TextType;
        GameObject G = null;
        if (textType == "TexDraw")
        {
            G = Instantiate(TexDrawPerfab, Content.transform);
        }
        else if (textType == "TMP")
        {
            G = Instantiate(TMPPerfab, Content.transform);
        }
        else if (textType == "Image")
        {
            G = Instantiate(ImagePerfab, Content.transform);
        }
        contents.Add(G);
        if (textType == "Image")
        {
            string path = longSpeakData.Content;
            float rate = longSpeakData.Rate;
            var ImageControler = G.GetComponent<LongSpeakImageControler>();
            ImageControler.Set(Content.transform.parent.GetComponent<RectTransform>().rect.height, rate);
            ImageControler.SetTexture();
            ImageControler.SetContentTexture(path);
            longDialogLayer.TextTweener = DOTween.To(() => 0, x => { }, 0, 0);
        }
        else
        {
            longDialogLayer.TextTweener =
                G.GetComponent<TextTweener>().SetTextTweener(longSpeakData.Content, IsSkip);
        }
    }
    public void AddTextContent(List<List<LongSpeakData>> longSpeakDatas, bool IsOnLoad = false)
    {
        //如果IsContinue才继续
        var temp = longSpeakDatas[longSpeakDatas.Count - 1];
        var longSpeakData = temp[temp.Count - 1];
        bool IsContinue = longSpeakData.Continue;
        if (!IsContinue)
        {
            Clear();
        }
        bool IsSkip;
        if (IsOnLoad) { IsSkip = true; }
        else
        {
            IsSkip = longSpeakData.Skip;
        }
        _AddTextContent(longSpeakData,IsSkip);
    }
    public void AddTextContents(List<LongSpeakData> longSpeakDatas)
    {
        foreach (var content in longSpeakDatas)
        {
            _AddTextContent(content, IsSkip:true);
        }
    }
    
    public void WithdrawTextContent(List<LongSpeakData> longSpeakDatas, bool IsClear)
    {
        if (IsClear)
        {
            Clear();
            foreach(var longSpeakData in longSpeakDatas)
            {
                //bool IsSkip = longSpeakData.Skip;
                _AddTextContent(longSpeakData, true);
            }
        }
        else
        {
            Destroy(contents[contents.Count-1].gameObject);
            contents.RemoveAt(contents.Count - 1);
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
}
