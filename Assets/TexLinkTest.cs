using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexLinkTest : MonoBehaviour
{
    public TEXLink tEXLink;
    public TEXDraw tEXDraw;
    public void Start()
    {
        tEXLink = GetComponent<TEXLink>();
        tEXDraw = GetComponent<TEXDraw>();
        tEXDraw.text = @"Try \cmbx \link{Click on Here}\\  
              
            You can \ulink{click the link}\ here.\\  
              
            \color{#39f} \item \link{Color tweening}\\  
            \color{#3f3} \item \link{Mouse \& Touchpad support}";
        tEXLink.onClick.AddListener(OnLinkClicked);
    }
    void OnLinkClicked(string linkKey)
    {
        Debug.Log($"链接被点击: {linkKey}");

        // 根据不同的链接键执行不同操作  
        switch (linkKey)
        {
            case "Click on Here":
                Debug.Log("点击了主链接");
                break;
            case "click the link":
                Debug.Log("点击了下划线链接");
                break;
            case "Color tweening":
                Debug.Log("颜色过渡功能");
                break;
            case "Mouse & Touchpad support":
                Debug.Log("鼠标和触摸板支持");
                break;
        }
    }

}
