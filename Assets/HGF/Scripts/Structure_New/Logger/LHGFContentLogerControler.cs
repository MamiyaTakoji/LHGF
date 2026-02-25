using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using LHGFData;
using UnityEngine.UI;

public class LHGFContentLogerControler : MonoBehaviour
{
    public TEXDraw texdraw;
    public TMP_Text TMP;
    public LongSpeakImageControler image;
    public GameObject Content;
    public List<GameObject> Contents = new() { };
    public int MaxContentLength = 100;
    public Button CloesButton;
    private List<int> PageCounts = new() { };
    //记录未改动的所有记录
    public List<Dictionary<string, string>> OriginContents = new() { };
    public void ResetPageCount()
    {
        PageCounts.Add(0);
    }
    private void Start()
    {
        CloesButton.onClick.AddListener(
            delegate 
            {
                Close();
            }
            );
    }
    public void Open()
    {
        var pos = GetComponent<RectTransform>().localPosition;
        pos.z = 0;
        GetComponent<RectTransform>().localPosition = pos;
    }
    public void Close()
    {
        var pos = GetComponent<RectTransform>().localPosition;
        pos.z = -1;
        GetComponent<RectTransform>().localPosition = pos;
    }
    public void Clear()
    {
        // 销毁所有实例化的内容对象
        foreach (var item in Contents)
        {
            if (item != null)
                Destroy(item);
        }

        // 清空所有列表
        Contents.Clear();
        OriginContents.Clear();
        PageCounts.Clear();

        // 强制刷新布局，使内容区域收缩并正确显示
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.GetComponent<RectTransform>());
    }
    public void LogContent(Dictionary<string, string> content)
    {
        if (PageCounts.Count == 0)
        {
            ResetPageCount();
        }
        if (content == null)
        {
            return;
        }
        OriginContents.Add(content);
        string contentType = LHGFData.Utils.GetDicValue(content, "ContentType", "Text");
        GameObject G = null;
        if (contentType == "Text")
        {
            string textType = LHGFData.Utils.GetDicValue(content, "TextType", "TextDraw");
            string textInfo = LHGFData.Utils.GetDicValue(content, "TextInfo", "");
            G = LogTextInfo(textType, textInfo);
        }
        else if(contentType == "Image")
        {
            string imagePath = content["TextInfo"];
            string rate = content["Rate"];
            G = LogImageInfo(imagePath, rate);
        }
        Add(G);
        PageCounts[PageCounts.Count - 1] += 1;
    }
    public void Add(GameObject G)
    {
        if(Contents.Count> MaxContentLength)
        {
            Contents.Add(G);
            var firstItem = Contents[0];
            Destroy(firstItem);
            Contents.RemoveAt(0);
        }
        else
        {
            Contents.Add(G);
        }
    }
    public void WithDraw()
    {
        if (PageCounts.Count > 0)
        {
            int PageCount = PageCounts[PageCounts.Count - 1];
            for (int i = 0; i < PageCount; i++)
            {
                if (Contents.Count > 0)
                {
                    var lastItem = Contents[Contents.Count - 1];
                    Destroy(lastItem);
                    Contents.RemoveAt(Contents.Count - 1);
                    OriginContents.RemoveAt(OriginContents.Count - 1);
                }
            }
            PageCounts.RemoveAt(PageCounts.Count - 1);
        }
    }
    public GameObject LogTextInfo(string textType, string content)
    {
        GameObject g;
        if (textType == "TexDraw")
        {
            var _texDraw = Instantiate(texdraw, Content.transform);
            _texDraw.text = content;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_texDraw.GetComponent<RectTransform>());
            g = _texDraw.gameObject;
        }
        else
        {
            var _TMP = Instantiate(TMP, Content.transform);
            _TMP.text = content;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_TMP.GetComponent<RectTransform>());
            g = _TMP.gameObject;
        }
        return g;
    }
    public GameObject LogImageInfo(string imagePath, string ratestr)
    {
        GameObject g;
        var _speakImage = Instantiate(image, Content.transform);
        var rate = float.Parse(ratestr);
        _speakImage.Set(Content.transform.parent.GetComponent<RectTransform>().rect.height, rate);
        _speakImage.SetTexture();
        _speakImage.SetContentTexture(imagePath);
        g = _speakImage.gameObject;
        return g;
    }
}
