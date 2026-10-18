using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class LHGFShowEdge : MonoBehaviour
{
    //这个脚本用来调整边框
    //两个方向边框的宽度应该相同
    public RectTransform SubRectTransform;
    //边的Size由x方向的边来确定
    public float EdgeSize;
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnRectTransformDimensionsChange()
    {
        float xSize = GetComponent<RectTransform>().rect.width;
        float ySize = GetComponent<RectTransform>().rect.height;
        float x_anchorMin = EdgeSize / 2;
        float x_anchorMax = 1 - x_anchorMin;
        float y_anchorMin = xSize / ySize * x_anchorMin;
        float y_anchorMax = 1 - y_anchorMin;
        SubRectTransform.anchorMin = new Vector2(x_anchorMin, y_anchorMin);
        SubRectTransform.anchorMax = new Vector2(x_anchorMax, y_anchorMax);
    }
}
