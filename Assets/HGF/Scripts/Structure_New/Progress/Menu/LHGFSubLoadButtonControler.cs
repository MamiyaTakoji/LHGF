using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LHGFData.GameSaveDataManager;
public class LHGFSubLoadButtonControler : MonoBehaviour
{
    //image代表缩略图
    public Image image;
    private float ClickTime = 0.05f;
    private float Counter = 0;
    //SaveID代表存档编号
    public TEXDraw SaveID;
    //Data代表存档日期
    public TEXDraw Data;
    public TEXDraw Abstract;
    public void SetContent(Texture2D texture, int saveID, string data, string abstractInfo)
    {
        var sprite = Sprite.Create(texture,
        new Rect(0, 0, texture.width, texture.height),
        new Vector2(0.5f, 0.5f), // 轴心点
        100, // 像素每单位
        0,
        SpriteMeshType.Tight);
        image.sprite = sprite;
        image.preserveAspect = true; // 保持宽高比

        SaveID.text = "Save "+ saveID.ToString();
        Data.text = data;
        Abstract.text = abstractInfo;
    }
    public void SetContentDefault(int saveID)
    {
        //白色对应的Base64
        var TextureBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5+AAAAFUlEQVQIHWP8DwQMSIAJiQ1mEhYAAAZdBAQjjGwcAAAAAElFTkSuQmCC";
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(System.Convert.FromBase64String(TextureBase64));
        string data = string.Empty;
        string abstractInfo = string.Empty;
        SetContent(texture, saveID, data, abstractInfo);
    }
    public void SetContent(int SaveId, GameSaveData gameSaveData)
    {
        int saveId = SaveId;
        string data = gameSaveData.SaveTime;
        string saveAbstract = gameSaveData.SaveAbstract;
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(gameSaveData.image);
        SetContent(tex, saveId, data, saveAbstract);
    }
}
