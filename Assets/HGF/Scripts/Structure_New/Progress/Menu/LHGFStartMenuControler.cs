using LHGFData;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LHGFStartMenuControler : MonoBehaviour
{
    public Button StartButton;
    public Button ContinueButton;
    public Button LoadButton;
    public Button ConfigButton;
    public Button ExitButton;
    public string StartId = "FirstScript-MAINBRANCH-0";
    public string StartMenuConfigPath = "StartMenuConfig.json";
    public AudioSource BGMPlayer;
    public TEXDraw Title;
    public RawImage Background;
    void Start()
    {
        StartButton.onClick.AddListener(delegate
        {
           LHGFGameMain.instance.GameMenu.StartGameOnGameMenu();
        });
        LoadButton.onClick.AddListener(delegate
        {
            LHGFGameMain.instance.GameMenu.ShowLoadPanel();
            gameObject.SetActive(false);
        });
        ContinueButton.onClick.AddListener(delegate
        {
            int newestSaveId = LHGFGameMain.instance.GameMenu.SLPanel.GetNewestSaveId();
            var SaveDic = LHGFGameMain.instance.gameSaveDataManager.gameSaveDatas;
            var SaveData = SaveDic[newestSaveId]; 
            LHGFGameMain.instance.GameMenu.LoadGameOnGameMenu(SaveData);
            LHGFGameMain.instance.OnGameLoad();
        });
        ConfigButton.onClick.AddListener(delegate
        {
            LHGFGameMain.instance.GameMenu.ConfigButtonControler();
            gameObject.SetActive(false);
        });
        ExitButton.onClick.AddListener(delegate
        {
            Application.Quit();
        });
        string jsonContent = File.ReadAllText(Path.Combine(LHGFData.Utils.ResoucePaths.StartMenuConfigPath, StartMenuConfigPath));
        var dataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
        if (dataDict.ContainsKey("BGM"))
        {
            BGMPlayer.loop = true;
            string BGMPath = Path.Combine(LHGFData.Utils.ResoucePaths.StartMenuConfigPath, dataDict["BGM"]);
            StartCoroutine(PlayVoice(BGMPath));
        }
        if (dataDict.ContainsKey("Background"))
        {
            string BackgroundPath = Path.Combine(LHGFData.Utils.ResoucePaths.StartMenuConfigPath, dataDict["Background"]);
            var sprite = LHGFData.Utils.LoadTextureByIO(BackgroundPath);
            Background.texture = sprite.texture;
        }
        if (dataDict.ContainsKey("Title"))
        {
            Title.text = dataDict["Title"];
        }
    }
    private void OnEnable()
    {
        string jsonContent = File.ReadAllText(Path.Combine(LHGFData.Utils.ResoucePaths.StartMenuConfigPath, StartMenuConfigPath));
        var dataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
        if (dataDict.ContainsKey("BGM"))
        {
            BGMPlayer.loop = true;
            string BGMPath = Path.Combine(LHGFData.Utils.ResoucePaths.StartMenuConfigPath, dataDict["BGM"]);
            StartCoroutine(PlayVoice(BGMPath));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator PlayVoice(string VoicePath)
    {
        bool loading = true;
        AudioClip clip = null;
        yield return LHGFData.AudioLayer.LoadAudioSource(VoicePath, (loadedClip) =>
        {
            clip = loadedClip;
            loading = false;
        });

        while (loading) yield return null;

        if (clip != null)
        {
            // 播放音频
            BGMPlayer.clip = clip;
            BGMPlayer.Play();

            // 等待播放完成
            yield return new WaitForSeconds(clip.length);

            // 播放完成后的逻辑
            Debug.Log($"音频播放完成: {VoicePath}");
        }

    }
}
