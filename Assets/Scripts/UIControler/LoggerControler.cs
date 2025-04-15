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
        gameObject.SetActive(false);
    }
    public void LogContent(string contentType, string textType, string content, string speaker = "")
    {
        //如果是Speak，要加上说话的人
        if (contentType == "Speak")
        {
            content = speaker + ":" + content;
        }
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
    }
}
