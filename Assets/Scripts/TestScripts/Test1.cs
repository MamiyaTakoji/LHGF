using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//这个脚本用来测试能否正确截图
public class Test1 : MonoBehaviour
{
    // Start is called before the first frame update
    public Image image;
    public SLPanelControler sLPanelControler;
    private float ClickTime = 1f;
    private float Counter = 0;
    void Start()
    {
        Application.targetFrameRate = 30;
        GetComponent<Button>().onClick.AddListener
            (
            delegate
            {
                StartCoroutine(sLPanelControler.CaptureScreenRoutine
                    (
                        () => { ApplyToImage(image, sLPanelControler.screenImage);
                            sLPanelControler.gameObject.SetActive(false); }
                        
                    ));
                GetComponent<Button>().interactable = false;
                Counter = 0;
                
            }
            );
    }

    // Update is called once per frame
    void Update()
    {
        Counter += Time.deltaTime;
        if (Counter > ClickTime) 
        {
            GetComponent<Button>().interactable = true;
        }
    }
    void ApplyToImage(Image uiImageComponent , Texture2D texture)
    {
        // 创建Sprite（需要纹理设置为Sprite模式）
        Sprite sprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), // 轴心点
            100, // 像素每单位
            0,
            SpriteMeshType.Tight);

        uiImageComponent.sprite = sprite;
        uiImageComponent.preserveAspect = true; // 保持宽高比
    }
}
