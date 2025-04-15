using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Net.WebSockets;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UI;

public class SLPanelControler : MonoBehaviour
{
    public Texture2D screenImage;
    public Texture2D cameraImage;
    public Canvas MainCanva;
    public GameObject Loaders;
    public GameObject PageSelection;
    public int CurrntPageId;
    public string CurrentModel;
    public GameObject ShowCurrentState;
    public Texture2D nullSaveImage;
    public TMP_Text saveDataPath;
    public SaveData newestSaveData = null;
    public Button BackButton;
    // 捕获全屏（包括UI）
    public void Awake()
    {

    }
    public void Start()
    {
        saveDataPath.text = "存档文件所在位置：" + Application.persistentDataPath;
        foreach (Transform child in PageSelection.transform)
        {
            GameObject childGO = child.gameObject;
            childGO.GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    CurrntPageId = int.Parse(childGO.name);
                    SetLoaderContent(CurrntPageId);
                    SetPage(CurrntPageId);
                }
                );
        }
        BackButton.onClick.AddListener(
            delegate { gameObject.SetActive(false); }
            );
    }
    public void CaptureFullScreen()
    {
        
        // 在渲染帧结束后执行（确保所有UI元素已绘制）
        StartCoroutine(CaptureScreenRoutine());
    }


    public IEnumerator CaptureScreenRoutine(Action onComplete = null)
    {
        yield return new WaitForEndOfFrame();

        var _screenImage1 = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        _screenImage1.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        _screenImage1.Apply();
        //Camera.main.targetTexture = Texture2DToRenderTexture(_screenImage1);



        var p = gameObject.transform.position;
        gameObject.transform.position = new Vector3(10000, 10000, 10000);
        //Camera.main.targetTexture = Texture2DToRenderTexture(_screenImage1);
        yield return new WaitForEndOfFrame(); // 等待本帧渲染完成
        Camera.main.targetTexture = null;
       var _screenImage = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        _screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        _screenImage.Apply();
        screenImage = _screenImage;
        gameObject.transform.position = p;
        // 执行回调（安全调用）
        onComplete?.Invoke();
    }
    public void SetPage(int PageNum)
    {
        foreach(Transform child in Loaders.transform)
        {
            var Go = child.gameObject;
            string saveId = (PageNum * 4 + int.Parse(Go.name)).ToString();
            saveId = "Save" + saveId;
            child.GetComponent<LoaderControler>().SaveID.text = saveId;
        }
    }
    public void SetLoaderContent(int PageNum)
    {
        foreach (Transform child in Loaders.transform)
        {
            var Go = child.gameObject;
            int saveId = PageNum * 4 + int.Parse(Go.name);
/*            string saveId = (PageNum * 4 + int.Parse(Go.name)).ToString();
            saveId = "Save" + saveId;
            child.GetComponent<LoaderControler>().SaveID.text = saveId;*/
            var saveDatas = GameMain.instance.galManager_Saver.saveDatas;
            if (saveDatas.datas.ContainsKey(saveId)){
                Go.GetComponent<LoaderControler>().SetSaveContent(saveDatas.datas[saveId]);
            }
            else
            {
                var nullSave = new SaveData();
                nullSave.texture = Texture2D.whiteTexture;
                nullSave.Data = string.Empty;
                nullSave.SaveInfoText = string.Empty;
                nullSave.ScriptName = null;
                nullSave.Id = null;
                Go.GetComponent<LoaderControler>().SetSaveContent(nullSave,TextureSize: false);
            }
        }
    }
    public RenderTexture Texture2DToRenderTexture(Texture2D tex)
    {
        RenderTexture rt = new RenderTexture(tex.width, tex.height, 0);
        Graphics.SetRenderTarget(rt);
        GL.Clear(true, true, Color.clear); // 清空原有内容
        Graphics.Blit(tex, rt); // 或使用Material进行复杂复制
        return rt;
    }
    public void ShowCurrentPage()
    {
        //展示最近的日期
        string NewestDatastr = "1900-10-05 08:30:00";
        var NewestData = DateTime.ParseExact(NewestDatastr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var SaveDatas = GameMain.instance.galManager_Saver.saveDatas;
        int NewestDataPage = 0;
        foreach(var Key in SaveDatas.datas.Keys)
        {
            string DataStr = SaveDatas.datas[Key].Data;
            var Data = DateTime.ParseExact(DataStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            if (Data > NewestData)
            {
                NewestDataPage = (Key - 1) / 4;
            }
        }
        CurrntPageId = NewestDataPage;
        SetLoaderContent(NewestDataPage);
        SetPage(NewestDataPage);   
    }
    public void GetCurrentData()
    {
        string NewestDatastr = "1900-10-05 08:30:00";
        var NewestData = DateTime.ParseExact(NewestDatastr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var SaveDatas = GameMain.instance.galManager_Saver.saveDatas;
        //int NewestDataPage = 1;
        foreach (var Key in SaveDatas.datas.Keys)
        {
            string DataStr = SaveDatas.datas[Key].Data;
            var Data = DateTime.ParseExact(DataStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            if (Data > NewestData)
            {
                newestSaveData = SaveDatas.datas[Key];
            }
        }
        //CurrntPageId = NewestDataPage;
        //SetLoaderContent(NewestDataPage);
        //SetPage(NewestDataPage);
    }
}
