using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using static GameSL;

public class LoaderControler : MonoBehaviour
{
    // Start is called before the first frame update
    public Image image;
    public SLPanelControler sLPanelControler;
    private float ClickTime = 0.05f;
    private float Counter = 0;
    public TEXDraw SaveID;
    public TEXDraw Data;
    public TextComponent Text;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener
            (
            delegate
            {
                if(sLPanelControler.CurrentMode == "Save")
                {
                    SaveGame();
                    GetComponent<Button>().interactable = false;
                    Counter = 0;
                }
                else if(sLPanelControler.CurrentMode == "Load")
                {
                    LoadGame();
                    GetComponent<Button>().interactable = false;
                    Counter = 0;
                }
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
    void ApplyToImage(Image uiImageComponent, Texture2D texture, bool TextureSize = true)
    {
        // 创建Sprite（需要纹理设置为Sprite模式）
        Sprite sprite;
        if (TextureSize) 
        {
        sprite = Sprite.Create(texture,
        new Rect(0, 0, texture.width, texture.height),
        new Vector2(0.5f, 0.5f), // 轴心点
        100, // 像素每单位
        0,
        SpriteMeshType.Tight);
        uiImageComponent.sprite = sprite;
         uiImageComponent.preserveAspect = true; // 保持宽高比
        }
        else
        {
           uiImageComponent.GetComponent<Image>().sprite=null;
        }
    }
    public void LoadGame()
    {
        int CurrentLoaderId = sLPanelControler.CurrntPageId * 4 + int.Parse(gameObject.name);
        var saveDatas = GameMain.instance.gameSL.saveDatas.saveDatas;
        if (saveDatas.ContainsKey(CurrentLoaderId)){
            GameMain.instance.gameSL.LoadGameData(CurrentLoaderId);
        }
        sLPanelControler.gameObject.SetActive(false);

    }
    public void SaveGame()
    {
        StartCoroutine(sLPanelControler.CaptureScreenRoutine
                    (
                        () => {
                            //ApplyToImage(image, sLPanelControler.screenImage);
                            Save();
                            //SetSaveContent(savdData);
                            sLPanelControler.gameObject.SetActive(false);
                        }
                    ));
    }
    public void Save()
    {
        int CurrentLoaderId = sLPanelControler.CurrntPageId * 4 + int.Parse(gameObject.name);
        GameMain.instance.gameSL.SaveGameData(CurrentLoaderId, sLPanelControler.screenImage);
    }
    public void SetSaveContent(SaveData saveData, bool TextureSize = true)
    {
        ApplyToImage(image, saveData.Texture,TextureSize);
        //SaveID.text = "Save" + saveData.SaveId.ToString();
        Data.text = saveData.SaveTime;
        Text.current_Text_Type = saveData.AbstractTextType;
        Text.ResetText();
        Text.text = saveData.Abstract;
    }
}
