using LHGFData;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
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
    public AudioSource EffectSoundPlayer;
    public TEXDraw Title;
    public RawImage Background;
    public AudioClip Clicked;
    public AudioClip Clicked1;
    private bool _lastHasSave = true;   // 缓存"是否有存档"状态，仅在状态变化时更新 UI
    private TMP_Text _continueText;     // 缓存子物体引用，避免每帧 Find + GetComponent
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
            //LHGFGameMain.instance.OnGameLoad();
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

        //为每个按钮添加点击音效
        /*List<Button> ButtonList1 = new List<Button>() { StartButton, ContinueButton};
        List<Button> ButtonList2 = new List<Button>() { LoadButton, ConfigButton, ExitButton};
        foreach (var button in ButtonList1)
        {
            button.onClick.AddListener(delegate
            {
                EffectiveSoundLayer esl = (EffectiveSoundLayer)LHGFGameMain.instance.gameData.layers["EffectiveSoundLayer"];
                esl.EffectiveSoundLayerControler.StartCoroutine(PlayVoice(Clicked));
            });
        }
        foreach (var button in ButtonList2)
        {
            button.onClick.AddListener(delegate
            {
                EffectiveSoundLayer esl = (EffectiveSoundLayer)LHGFGameMain.instance.gameData.layers["EffectiveSoundLayer"];
                esl.EffectiveSoundLayerControler.StartCoroutine(PlayVoice(Clicked1));
            });
        }*/

        string jsonContent = Utils.ReadTextSync(Path.Combine(LHGFData.Utils.ResoucePaths.StartMenuConfigPath, StartMenuConfigPath));
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
        // 缓存"继续游戏"按钮的文本引用，供 Update 使用（替代每帧 Find + GetComponent）
        _continueText = ContinueButton.transform.Find("Text (TMP)").GetComponent<TMP_Text>();
    }
    private void OnEnable()
    {
        string jsonContent = Utils.ReadTextSync(Path.Combine(LHGFData.Utils.ResoucePaths.StartMenuConfigPath, StartMenuConfigPath));
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
        bool hasSave = LHGFGameMain.instance.GameMenu.SLPanel.GetNewestSaveId() != -1;
        // 存档状态没有变化时直接跳过，避免每帧执行 UI 更新
        if (hasSave == _lastHasSave)
        {
            return;
        }
        _lastHasSave = hasSave;

        ContinueButton.interactable = hasSave;
        _continueText.color = hasSave ? Color.black : Color.gray;
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
    IEnumerator PlayVoice(AudioClip clip)
    {
        if (clip != null)
        {
            // 播放音频
            EffectSoundPlayer.clip = clip;
            EffectSoundPlayer.Play();

            // 等待播放完成
            yield return new WaitForSeconds(clip.length);
        }
    }
}
