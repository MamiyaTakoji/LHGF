using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TexAnimation : MonoBehaviour
{
    public TEXDraw tEXDraw;
    public float duration = 2f;
    private string targetString;
    // Start is called before the first frame update
    void Start()
    {
        tEXDraw = GetComponent<TEXDraw>();
        targetString = tEXDraw.text;
        DOTween.To(
            () => 0,                        // ��ʼֵ��0�ַ���
            x => tEXDraw.text = targetString.Substring(0, x), // ��ȡǰx���ַ�
            targetString.Length,                // Ŀ���ַ���
            duration
        ).SetEase(Ease.Linear);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
