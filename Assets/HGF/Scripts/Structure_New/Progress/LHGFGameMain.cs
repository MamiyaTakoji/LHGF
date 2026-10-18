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
    public EvidencePanelControler EvidencePanelControler;
    public CharacterPanelControler CharacterPanelControler;
    public ChapterLayerControler ChapterLayerControler;

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

    // 推进时周期性回收：连续游玩时历史快照已不再持有纹理，被替换下来的旧立绘/背景会变成“无引用”孤儿，
    // 仅靠 Resources.UnloadUnusedAssets() 才能释放原生 Texture2D（GC 无法回收）。
    // 读档/开始时已有该回收，推进时补这一步，避免孤儿纹理一路堆积。
    private int forwardStepCount = 0;
    private const int UnloadEveryNForwards = 20;

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
        Application.targetFrameRate = 45;
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
        // 先 Reset（层 Reset 经 DestroyResources 释放历史/当前 ImageData 持有的 Sprite+Texture，
        // 并清掉对旧纹理的引用），再 UnloadUnusedAssets 释放已成孤儿的原生纹理，顺序不能反。
        ResetGame();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
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
        MainScence.gameObject.SetActive(false);  // 加这行
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
            TryUnloadUnusedAssets();
        }
    }
    // 节流回收：每 UnloadEveryNForwards 步前进触发一次。
    // 不仅回收被替换下来且已无引用的立绘/背景纹理（UnloadUnusedAssets），
    // 还排空 xLua 的 refQueue 并做全量 Lua GC——此前每个动画创建却未 Dispose 的 LuaTable 包装器
    // 其终结器会把“待释放的 Lua 引用”入队 refQueue，若从不 Tick 排空，Lua 端的表/编译块会一直被钉住，
    // Lua 堆只涨不落（GC.Collect 与 UnloadUnusedAssets 都管不到 Lua 堆），表现为峰值逐次抬高。
    // 频率可在实测后调整：过密会卡顿，过疏则孤儿纹理/Lua 堆堆积。
    private void TryUnloadUnusedAssets()
    {
        forwardStepCount++;
        if (forwardStepCount < UnloadEveryNForwards) return;
        forwardStepCount = 0;
        // 1) 回收 C# 端已无引用对象（含未 Dispose 的 LuaTable 包装器），其终结器把待释放的 Lua 引用入队 refQueue。
        //    WaitForPendingFinalizers 等待终结器跑完，确保 refQueue 已填满再排空。
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        // 2) 排空 refQueue，真正释放对应 Lua 端引用，使 Lua 端的表/编译块变为垃圾。
        Utils.xLuaEnv.luaEnv.Tick();
        // 3) 全量 Lua GC，把已成垃圾的表/编译块即时回收，避免靠增量 GC 慢慢追、堆栈爬升。
        Utils.xLuaEnv.luaEnv.FullGc();
        // 4) 释放已成孤儿的原生纹理等 Unity 资产（零引用资产，不影响屏幕上正在显示的纹理）。
        Resources.UnloadUnusedAssets();
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
        
        //设置证据层
        var EvidenceLayer = new EvidenceLayer();
        EvidenceLayer.EvidencePanelControler = EvidencePanelControler;
        EvidencePanelControler.data = EvidenceLayer;
        layers.Add("EvidenceLayer", EvidenceLayer);

        //设置证据出示层
        var EvidenceChoiceLayer = new EvidenceChoiceLayer();
        EvidenceChoiceLayer.EvidencePanelControler = EvidencePanelControler;
        EvidencePanelControler.choiceData = EvidenceChoiceLayer;
        layers.Add("EvidenceChoiceLayer", EvidenceChoiceLayer);

        //设置角色信息层
        var CharacterInfoLayer = new CharacterInfoLayer();
        CharacterInfoLayer.CharacterInfoPanelControler = CharacterPanelControler;
        CharacterPanelControler.data = CharacterInfoLayer;
        layers.Add("CharacterInfoLayer", CharacterInfoLayer);

        //设置章节层
        var ChapterLayer = new ChapterLayer();
        ChapterLayer.ChapterLayerControler = ChapterLayerControler;
        layers.Add("ChapterLayer", ChapterLayer);

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
        objectCommand2layer.Add("AddEvidence", new List<string>() { "EvidenceLayer", "EffectiveSoundLayer" });
        objectCommand2layer.Add("EvidenceChoice", new List<string>() { "EvidenceChoiceLayer"});
        objectCommand2layer.Add("CharacterDescribe", new List<string>() { "CharacterInfoLayer" });
        objectCommand2layer.Add("ChapterImage", new List<string>() { "ChapterLayer" });
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
        //证据层默认是关闭的
        CloseEvidencePanel();
        choiceControler.SetActive(false);
        foreach (var command in commands)
        {
            if (command.CommandName == "ChapterImage")
            {
                //回撤/读档/前进到章节节点时，重新触发章节动画
                gameData.layers["ChapterLayer"].Next(command);
            }
            else
            {
                LayerShower(command.CommandName);
            }
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
        if(commandName == "EvidenceChoice")
        {
            OpenEvidencePanel();
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
    public void OpenEvidencePanel()
    {
        EvidencePanelControler.GameMenu.SetActive(true);
        EvidencePanelControler.gameObject.SetActive(true);
        EvidencePanelControler.ActiveEviddencePanel(EvidencePanelControler.MenuState.Show);
    }
    public void CloseEvidencePanel()
    {
        EvidencePanelControler.GameMenu.SetActive(false);
    }
}
