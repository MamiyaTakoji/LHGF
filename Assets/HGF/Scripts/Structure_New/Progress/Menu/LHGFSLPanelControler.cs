using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHGFData;
using static LHGFData.GameSaveDataManager;
using System;
using UnityEngine.UI;
using System.Globalization;
using TMPro;

public class LHGFSLPanelControler : MonoBehaviour
{
    public int CurrntPageId;
    public string CurrentMode;
    public RenderTexture image;
    public GameObject PageSelection;
    public GameObject Loaders;
    public SLPanelState state;
    public TMP_Text showSLState;
    public LHGFGamaeMenuControler gameMenu;
    private List<string> SLStateStr = new List<string>() {"正在加载","正在保存" };
    public enum SLPanelState
    {
        Load,
        Save
    }
    public void SetSLState(SLPanelState _state)
    {
        state = _state;
        SetPanel();
        showSLState.text = SLStateStr[(int)_state];
    }
    private void Start()
    {
        foreach (Transform child in PageSelection.transform)
        {
            GameObject childGO = child.gameObject;
            childGO.GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    CurrntPageId = int.Parse(childGO.name);
                    SetPageContent(CurrntPageId);
                }
                );
        }
        foreach (Transform child in Loaders.transform)
        {
            GameObject childGO = child.gameObject;
            childGO.GetComponent<Button>().onClick.AddListener(
            delegate
            {
                int saveId = CurrntPageId * 4 + int.Parse(childGO.name);
                if (state == SLPanelState.Load)
                {
                    var SaveDic = LHGFGameMain.instance.gameSaveDataManager.gameSaveDatas;
                    if (!SaveDic.ContainsKey(saveId))
                    {
                        return;
                    }
                    else
                    {
                        var SaveData = SaveDic[saveId];
                        //LHGFGameMain.instance.GameMenu.LoadGameOnGameMenu(SaveData.NodeId);
                        LHGFGameMain.instance.GameMenu.LoadGameOnGameMenu(SaveData);
                        //LHGFGameMain.instance.OnGameLoad();
                    }  
                }
                else
                {
                    LHGFGameMain.instance.OnGameSave();
                    var SaveData = GetSaveData();
                    LHGFGameMain.instance.gameSaveDataManager.gameSaveDatas[saveId] = SaveData;
                    LHGFGameMain.instance.gameSaveDataManager.WriteSaveDataToSavePath(SaveData, saveId.ToString());
                    SetPageContent(CurrntPageId);
                }
            }
        );
        }
    }
    public void SetPanel()
    {
        int NewestSaveId = GetNewestSaveId();
        CurrntPageId = (NewestSaveId - 1) / 4;
        SetPageContent(CurrntPageId);
    }
    //这里组装SaveData
    public GameSaveData GetSaveData()
    {

        var gameMain = LHGFGameMain.instance;
        //获取NodeId
        string NodeId = gameMain.gameProgress.CurrentNodeId;
        //Abstract以Log的最后一条为准
        //摆了，不识别类型吧
        string SaveAbstract = string.Empty;
        var ContentLogger = gameMain.ContentLogerControler;
        var OriginContents = ContentLogger.OriginContents;
        if (OriginContents.Count > 0)
        {
            var lastContent = OriginContents[OriginContents.Count - 1];
            SaveAbstract = LHGFData.Utils.GetDicValue(lastContent, "TextInfo", string.Empty);
        }
        var texture = ConvertRenderTextureToTexture2D(image);
        //压缩图片
        Texture2D resizedTexture = ResizeTexture(texture, 1980/2, 1080/2);
        var texturebyte = resizedTexture.EncodeToPNG();
        // 销毁临时创建的 Texture2D，防止内存泄漏
        Destroy(texture);
        Destroy(resizedTexture);
        var gameSaveData = new GameSaveData();
        gameSaveData.image = texturebyte;
        gameSaveData.NodeId = NodeId;
        gameSaveData.SaveAbstract = SaveAbstract;
        DateTime now = DateTime.Now;
        gameSaveData.SaveTime = now.ToString("yyyy-MM-dd HH:mm:ss");
        var layerDatas = gameMain.gameProgress.OnGameSave(gameMain.gameData);
        gameSaveData.layerDatas = layerDatas;
        return gameSaveData;
    }
    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }
    public Texture2D ConvertRenderTextureToTexture2D(RenderTexture renderTexture)
    {
        // 创建与Render Texture相同尺寸的Texture2D
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height,
                                     TextureFormat.ARGB32, true);

        // 保存当前活动的Render Texture
        RenderTexture previous = RenderTexture.active;

        // 设置要读取的Render Texture为活动状态
        RenderTexture.active = renderTexture;

        // 读取像素数据
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        // 恢复之前的活动Render Texture
        RenderTexture.active = previous;

        return tex;
    }
    public void SetPageContent(int PageNum)
    {
        foreach (Transform child in Loaders.transform)
        {
            var Go = child.gameObject;
            int saveId = PageNum * 4 + int.Parse(Go.name);
            var SaveDataDic = LHGFGameMain.instance.gameSaveDataManager.gameSaveDatas;
            if (SaveDataDic.ContainsKey(saveId))
            {
                child.GetComponent<Button>().interactable = true;
                var SaveData = SaveDataDic[saveId];
                child.GetComponent<LHGFSubLoadButtonControler>().SetContent(saveId, SaveData);
            }
            else
            {
                child.GetComponent<LHGFSubLoadButtonControler>().SetContentDefault(saveId);
                //child.GetComponent<Button>().interactable = false;
                child.GetComponent<Button>().interactable = (state == SLPanelState.Save);
            }
        }
    }
    public int GetNewestSaveId()
    {
        var SaveDatas = LHGFGameMain.instance.gameSaveDataManager.gameSaveDatas;
        int NewestSaveId = -1;
        string newestTime = null;
        foreach (var Key in SaveDatas.Keys)
        {
            // SaveTime 为固定格式 "yyyy-MM-dd HH:mm:ss"（定长零填充），
            // 字符串比较结果与时间先后完全一致，无需 DateTime.ParseExact 解析
            string DataStr = SaveDatas[Key].SaveTime;
            if (newestTime == null || string.CompareOrdinal(DataStr, newestTime) > 0)
            {
                newestTime = DataStr;
                NewestSaveId = Key;
            }
        }
        return NewestSaveId;
    }
}
