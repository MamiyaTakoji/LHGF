using System.Collections;
using System.Collections.Generic;
using TexDrawLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoggerControler : MonoBehaviour
{
    public TEXDraw texdraw;
    public TMP_Text TMP;
    public Button CloesButton;
    public GameObject Content;
    public ScrollRect scrollRect;
    // Start is called before the first frame update
    void Start()
    {
        CloesButton.onClick.AddListener(delegate
        {
            Close();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Close()
    {
        var rect = GetComponent<RectTransform>();
        transform.position = new Vector3(1.5f*rect.rect.width, 1.5f*rect.rect.height, 0);
    }
    public void Open()
    {
        var rect = GetComponent<RectTransform>();
        transform.position = new Vector3(rect.rect.width / 2, rect.rect.height / 2, 0);
    }
    public void LogSpeakContent(string textType, string content, string speaker = "")
    {
        content = speaker + ":" + content;
        if (textType == "TexDraw")
        {
            var _texDraw = Instantiate(texdraw, Content.transform);
            _texDraw.text = content;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_texDraw.GetComponent<RectTransform>());
        }
        else
        {
            var _TMP = Instantiate(TMP, Content.transform);
            _TMP.text = content;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_TMP.GetComponent<RectTransform>());
        }
        var pos = Content.GetComponent<RectTransform>().position;
        pos.y = Content.GetComponent<RectTransform>().rect.height;
        Content.GetComponent<RectTransform>().position = pos;
        scrollRect.verticalNormalizedPosition = 0f;
    }
    public void LogLongSpeakContent(string textType, string content)
    {
        if(textType == "TexDraw")
        {
            var _texDraw = Instantiate(texdraw, Content.transform);
            _texDraw.text = content;
            //_texDraw.transform.parent = Content.transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_texDraw.GetComponent<RectTransform>());
        }
        else
        {
            var _TMP = Instantiate(TMP, Content.transform);
            _TMP.text = content;
            //_TMP.transform.parent = Content.transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_TMP.GetComponent<RectTransform>());
        }
        var pos = Content.GetComponent<RectTransform>().position;
        pos.y = Content.GetComponent<RectTransform>().rect.height;
        Content.GetComponent<RectTransform>().position = pos;
        scrollRect.verticalNormalizedPosition = 0f;
    }
    public void LogChoiceContent(string textType, string content)
    {
        if (textType == "TexDraw")
        {
            var _texDraw = Instantiate(texdraw, Content.transform);
            _texDraw.text = content;
            //_texDraw.transform.parent = Content.transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_texDraw.GetComponent<RectTransform>());
        }
        else
        {
            var _TMP = Instantiate(TMP, Content.transform);
            _TMP.text = content;
            //_TMP.transform.parent = Content.transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_TMP.GetComponent<RectTransform>());
        }
        var pos = Content.GetComponent<RectTransform>().position;
        pos.y = Content.GetComponent<RectTransform>().rect.height;
        Content.GetComponent<RectTransform>().position = pos;
    }
}
