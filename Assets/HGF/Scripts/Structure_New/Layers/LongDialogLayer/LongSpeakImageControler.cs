using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LHGFData;
public class LongSpeakImageControler : MonoBehaviour
{
    //LongSpeakImage的工作原理为：
    //通过控制父对象的Image的高度来控制总图像的高度
    public float rate = 0.2f;
    public float ParentHigh;
    public Image ContentImage;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Set(float h, float _rate)
    {
        ParentHigh = h;
        rate = _rate;
    }
    public void SetTexture()
    {
        Texture2D texture = new Texture2D(2, (int)(ParentHigh * rate));
        var s = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        var Img = GetComponent<Image>();
        var c = Img.color;
        c.a = 0;
        Img.color = c;
        Img.sprite = s;
    }
    public void SetContentTexture(string imagePath)
    {
        var s = Utils.LoadTextureByIO(imagePath);
        ContentImage.sprite = s;
    }
}
