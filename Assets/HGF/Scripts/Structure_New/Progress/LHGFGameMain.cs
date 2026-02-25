using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHGFGameProgress;
using LHGFData;
public class LHGFGameMain : MonoBehaviour
{
    //这个文件应该能够描述整个游戏的过程

    //设置层的控制器
    public LHGFChoiceLayerControler ChoiceLayerControler;
    public LHGFLongDialogLayerControler LongDialogLayerControler;
    public LHGFDialogLayerControler DialogLayerControler;
    public LHGFImageCILayerControler ImageCILayerControler;
    public LHGFImageBgLayerControler ImageBgLayerControler;
    public LHGFBgmLayerControler BgmLayerControler;
    public LHGFCVLayerControler CVLayerControler;
    public LHGFVideoAnimationLayerControler VideoAnimationLayerControler;
    public LHGFCameraLayerControler CameraLayerControler;
    public LHGFEffectiveSoundLayerControler EffectiveSoundLayerControler;
    public LHGFBlackboardLayerControler BlackboardLayerControler;

    //设置历史记录
    public LHGFContentLogerControler ContentLogerControler;

    public LHGFGameProgress.LHGF_GameData gameData;
    public LHGFGameProgress.LHGF_GameProgress gameProgress;
    public float waitTime;

    //游戏数据
    public GameGlobaDataManager gameGlobaDataManager;
    public GameConfigDataManager gameConfigDataManager;
    public GameSaveDataManager gameSaveDataManager;

    //private float Count = 0;
    //private bool IsOperatable = false;
    
    public static LHGFGameMain instance;

    //游戏菜单
    public LHGFGamaeMenuControler GameMenu;
    public LHGFMainScenceControler MainScence;
    //public string NodeId;
    public bool IsFinish { get => gameProgress.IsFinish(gameData);  }
    public void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        OnGlobalGameStart();
    }

    // Update is called once per frame
    void Update()
    {
        OnGameUpdata();
    }
    //整个游戏开始时调用，并不是点击开始游戏时调用
    public void OnGlobalGameStart()
    {
        GameMenu.gameObject.SetActive(true);
        GameMenu.SetStartMenuActive();
        ContentLogerControler.gameObject.SetActive(true);
        SetLayers();
        LongDialogLayerControler.gameObject.SetActive(true);
        gameProgress = new();
        //LHGFGameProgress.LHGF_GameProgress.instance = new();
        /*gameData = LHGFGameProgress.LHGF_GameData.instance;*/
        //gameProgress = LHGFGameProgress.LHGF_GameProgress.instance;
        gameProgress.OnGlobalGameStart(gameData);
        gameGlobaDataManager = new();
        gameConfigDataManager = new();
        gameSaveDataManager = new();
        GameMenu.ApplyGlobalSetting();
    }
    public void OnGameStart()
    {
        //点击开始游戏时调用
        ResetGame();
        gameProgress.OnGameStart(gameData);
        Forward();
    }
/*    public void OnGameLoad(string NodeId)
    {
        ResetGame();
        ContentLogerControler.Clear();
        gameProgress.Load(NodeId, gameData);
        gameProgress.OnGameLoadFinish(gameData);
        LayerShower(gameData, gameProgress.NextNodeId);
        gameProgress.Forward(gameData);
        LogContent();
    }*/
    public void OnGameLoad()
    {
        //在数据加载完成后调用
        //var commands = gameData.scriptGraph[gameProgress.CurrentNodeId].LayerCommands;
        LayerShower(gameData, gameProgress.CurrentNodeId);
    }
    public void OnGameSave()
    {
        //游戏保存时，要结束层的表现，保存数据
        gameProgress.Skip(gameData);
    }
    public void OnGameUpdata()
    {
        gameProgress.OnGameUpdata(gameData);
    }
    public void ResetGame()
    {
        ContentLogerControler.Clear();
        gameProgress.Clear(gameData);
    }
    public void OnGameFinish()
    {
        //清除已有数据
        ResetGame();
        ContentLogerControler.Clear();
        GameMenu.SetStartMenuActive();
    }
    //游戏前进时调用
    public void Forward()
    {
/*        if (!IsOperatable)
        {
            return;
        }*/
        if (!IsFinish)
        {
            gameProgress.Skip(gameData);
        }
        else if(gameProgress.NextNodeId == ScriptData.FinishFlag)
        {
            OnGameFinish();
        }
        else 
        {
            gameGlobaDataManager.UpdataGlobalData(gameProgress.CurrentNodeId);
            LayerShower(gameData, gameProgress.NextNodeId);
            gameProgress.Forward(gameData);
            LogContent();
            Debug.Log(gameProgress.CurrentNodeId);
        }
    }
    public void SetLayers()
    {
        Dictionary<string, List<string>> layerCommand2layer = SetLayerCommand2layer();
        Dictionary<string, ILayer> layers = new() { };

        //设置选项层
        var ChoiceLayer = new ChoiceLayer();
        ChoiceLayer.LHGFchoiceLayerControler = ChoiceLayerControler;
        ChoiceLayerControler.choiceLayer = ChoiceLayer;
        layers.Add("ChoiceLayer", ChoiceLayer);

        //设置长对话层
        var LongSpeakLayer = new LongDialogLayer();
        LongSpeakLayer.LHGFlongDialogLayerControler = LongDialogLayerControler;
        LongDialogLayerControler.longDialogLayer = LongSpeakLayer;
        layers.Add("LongSpeakLayer", LongSpeakLayer);

        //设置数据层
        var DataLayer = new DataLayer();
        layers.Add("DataLayer", DataLayer);

        //设置对话层
        var SpeakLayer = new DialogLayer();
        SpeakLayer.SpeakLayerControler = DialogLayerControler;
        SpeakLayer.dataLayer = DataLayer;
        DialogLayerControler.dialogLayer = SpeakLayer;
        layers.Add("SpeakLayer", SpeakLayer);

        //设置图片立绘层
        var ImageCILayer = new ImageCILayer();
        ImageCILayer.ImageCILayerControler = ImageCILayerControler;
        ImageCILayer.dataLayer = DataLayer;
        layers.Add("ImageCILayer", ImageCILayer);

        //设置图片背景层
        var ImageBgLayer = new ImageBgLayer();
        ImageBgLayer.ImageBgLayerControler = ImageBgLayerControler;
        layers.Add("ImageBgLayer", ImageBgLayer);

        //设置背景音乐层
        var BgmLayer = new BgmLayer();
        BgmLayer.BgmLayerControler = BgmLayerControler;
        layers.Add("BgmLayer", BgmLayer);

        //设置角色语音层
        var CVLayer = new CVLayer();
        CVLayer.CVLayerControler = CVLayerControler;
        layers.Add("CVLayer", CVLayer);

        //设置视频动画层
        var VideoAnimationLayer = new VideoAnimationLayer();
        VideoAnimationLayer.videoAnimationLayerControler = VideoAnimationLayerControler;
        layers.Add("VideoAnimationLayer", VideoAnimationLayer);

        //设置摄像机动画层
        var CameraLayer = new CameraLayer();
        CameraLayer.CameraLayerControler = CameraLayerControler;
        layers.Add("CameraLayer", CameraLayer);

        //设置音效层
        var EffectiveSoundLayer = new EffectiveSoundLayer();
        EffectiveSoundLayer.EffectiveSoundLayerControler = EffectiveSoundLayerControler;
        layers.Add("EffectiveSoundLayer", EffectiveSoundLayer);

        //设置黑板层
        var BlackboardLayer = new BlackboardLayer();
        BlackboardLayer.blackboardLayerControler = BlackboardLayerControler;
        layers.Add("BlackboardLayer", BlackboardLayer);
        
        gameData = new(_layers: layers, _objectCommand2layer: layerCommand2layer);
        //gameData = LHGFGameProgress.LHGF_GameData.instance;
    }
/*    public void Counter()
    {
        Count += Time.deltaTime;
        if (Count > waitTime)
        {
            Count = 0;
            IsOperatable = true;
        }
    }*/
    public Dictionary<string, List<string>> SetLayerCommand2layer()
    {
        Dictionary<string, List<string>> objectCommand2layer = new() { };
        objectCommand2layer.Add("Choice", new List<string>() { "ChoiceLayer" });
        objectCommand2layer.Add("LongSpeak", new List<string>() { "LongSpeakLayer", "CVLayer" });
        objectCommand2layer.Add("AddCharacter", new List<string>() { "DataLayer", "ImageCILayer" });
        objectCommand2layer.Add("Speak", new List<string>() { "SpeakLayer", "CVLayer"});
        objectCommand2layer.Add("ImageCharacterAnimation", new List<string>() { "ImageCILayer" });
        objectCommand2layer.Add("ChangeImageBackground", new List<string>() { "ImageBgLayer" });
        objectCommand2layer.Add("SetBgm", new List<string>() { "BgmLayer" });
        objectCommand2layer.Add("VideoAnimation", new List<string>() { "VideoAnimationLayer" });
        objectCommand2layer.Add("CameraAnimation", new List<string>() { "CameraLayer" });
        objectCommand2layer.Add("EffectiveSound", new List<string>() { "EffectiveSoundLayer"});
        objectCommand2layer.Add("Blackboard", new List<string>() { "BlackboardLayer" });
        return objectCommand2layer;
    }
    public bool Withdraw()
    {
        if (gameData.historyNodes.Count < 2)
        {
            return false;
        }
        else
        {
            gameProgress.Withdraw(gameData);
            LayerShower(gameData, gameData.historyNodes[gameData.historyNodes.Count - 1]);
            ContentLogerControler.WithDraw();
            Debug.Log(gameProgress.CurrentNodeId);
            return true;
        }
    }
    public void LayerShower(LHGFGameProgress.LHGF_GameData gameData, string nextNode)
    {
        var commands = gameData.scriptGraph[nextNode].LayerCommands;
        //选项层默认是关闭的
        var choiceControler = gameData.layers["ChoiceLayer"].GetControler();
        choiceControler.SetActive(false);
        foreach (var command in commands)
        {
            LayerShower(command.CommandName);
        }
    }
    public void LayerShower(string commandName)
    {
        //展示Speak层时，不显示LongSpeak层
        //展示LongSpeak层时，不显示Speak层
        if (commandName == "LongSpeak")
        {
            var speakControler = gameData.layers["SpeakLayer"].GetControler();
            speakControler.SetActive(false);
            var longSpeakControler = gameData.layers["LongSpeakLayer"].GetControler();
            longSpeakControler.SetActive(true);
        }
        if (commandName == "Speak")
        {
            var speakControler = gameData.layers["SpeakLayer"].GetControler();
            speakControler.SetActive(true);
            var longSpeakControler = gameData.layers["LongSpeakLayer"].GetControler();
            longSpeakControler.SetActive(false);
        }
        if(commandName == "Choice")
        {
            var choiceControler = gameData.layers["ChoiceLayer"].GetControler();
            choiceControler.SetActive(true);
        }
    }
    public void LogContent()
    {
        ContentLogerControler.ResetPageCount();
        var contents = gameProgress.LogContents;
        foreach(var content in contents)
        {
            ContentLogerControler.LogContent(content);
        }
    }
}
