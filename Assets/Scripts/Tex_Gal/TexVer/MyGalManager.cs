using Common.Game;
using ScenesScripts.GalPlot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.IO;
using System.Xml.Linq;
using UnityEngine.Networking;
using UnityEngine.UI;
using static ScenesScripts.GalPlot.GalManager.Struct_PlotData;
using DG.Tweening;
using static MyGalManager;
using UnityEngine.TextCore.Text;

public class MyGalManager : GalManager
{
    public GalManager_LongSpeak Gal_LongSpeak;
    public AudioSource Gal_Voice;
    public AudioSource BGM;
    public GameObject MainCanvas;
    public GameObject MainCanvasCopy;
    public CurrentData currentData = new CurrentData();
    public string currentContent;
    public string currentId;
    public string currentTextType;
    public bool IsShowingChioce = false;
    public LoggerControler loggerControler;
    private void Start()
    {
/*        Gal_Voice = this.gameObject.GetComponent<AudioSource>();
        ResetPlotData();
        StartCoroutine(LoadPlot(() => { SetPlotData("S1-1", null); LoadPlot();
        }));
        Application.targetFrameRate = 60;
        return;*/
    }
    private void ResetPlotData()
    {   
        PlotData = new Struct_PlotData();
        return;
    }
    private class CharacterConfig
    {
        public static GameConfig CharacterInfo;
        public static GameConfig Department;

        static CharacterConfig()
        {
            CharacterInfo = new GameConfig(GameAPI.GetWritePath() + "/HGF/CharacterInfo.ini");
            Department = new GameConfig(GameAPI.GetWritePath() + "/HGF/Department.ini");
        }
    }
    public void Load(string id, string scriptName)
    {
        //Gal_Voice = this.gameObject.GetComponent<AudioSource>();
        ResetPlotData();
        currentData.reset();
        GameMain.instance.galManager_Saver.ResetCharacterImg();
        StartCoroutine(LoadPlot(() =>
        {
            SetPlotData(id); _LoadPlot(); Button_Click_NextPlot();
        }, scriptName));
        Application.targetFrameRate = 60;
        
    }
    public string GetAudioPath(string AudioType, string fileName, string characterName = "")
    {
        //如果是BGM，返回StreamingAssets\HGF\Audio\BGM里面的文件
        //如果是角色语音，返回StreamingAssets\HGF\Audio\scriptName里面的文件
        //其中scriptName代表脚本名字
        if(AudioType == "Voice")
        {
            string currentScriptName = GameMain.gameScriptName2Load;
            string path = $"{GameAPI.GetWritePath()}/HGF/Audio/{currentScriptName}/{characterName}/{fileName}";
            return path;
        }
        else if(AudioType == "BGM")
        {
            string path = $"{GameAPI.GetWritePath()}/HGF/Audio/BGM/{fileName}";
            return path;
        }
        else
        {
            Debug.LogError("错误的AudioType");
            return null;
        }
    }
    public string GetBackGroundImagePath(string BackGroundImageName)
    {
        string path = $"{GameAPI.GetWritePath()}/HGF/Texture2D/BackgroundImage/{BackGroundImageName}";
        return path;
    }
    private IEnumerator PlayAudio(AudioSource audioSource, string filePath)
    {
        //获取.wav文件，并转成AudioClip
        GameAPI.Print(filePath);
        AudioType audioType = Path.GetExtension(filePath).ToLower() switch
        {
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            _ => AudioType.UNKNOWN
        };
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType);
        //等待转换完成
        yield return www.SendWebRequest();
        //获取AudioClip
        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
        //设置当前AudioSource组件的AudioClip
        audioSource.clip = audioClip;
        //播放声音
        audioSource.Play();
    }
    public override void Button_Click_NextPlot()
    {
        if (IsShowingChioce)
        {
            return;
        }

        if (PlotData.MainPlot.Count == 0)
        {
            GameAPI.Print("游戏结束!");
            returnMainMenu();
            return;
        }

        //IsCanJump这里有问题，如果一直点击会为false，而不是说true，这是因为没有点击按钮 ，没有添加按钮
        if (GalManager_Text.IsSpeak || !GalManager_Text.IsCanJump) { return; }
        if (!PlotData.IsBranch)
        {
            PlotData.NowPlotDataNode = PlotData.MainPlot.Dequeue();//队列出队+内联 出一个temp节点
                                                                   //PlotData.MainPlot.TryDequeue(out PlotData.NowPlotDataNode);
            PlotData.BranchPlotInfo.Clear();
        }
        else//当前为分支节点
        {
            //这块得妥善处理
            PlotData.NowPlotDataNode = GetBranchByID(PlotData.NowJumpID);
        }

        PlotData.ChoiceText.Clear();
        if (PlotData.NowPlotDataNode == null)
        {
            GameAPI.Print("无效的剧情结点", "error");
            return;
        }
        Debug.Log(PlotData.NowPlotDataNode);
        switch (PlotData.NowPlotDataNode.Name.ToString())
        {
            case "AddCharacter"://处理添加角色信息的东西
                {
                    var _ = new Struct_CharacterInfo();
                    var _From = PlotData.NowPlotDataNode.Attribute("From").Value;
                    var _CharacterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                    _.Name = CharacterConfig.CharacterInfo.GetValue(_From, "Name");
                    _.CharacterID = _CharacterId;
                    _.Affiliation = CharacterConfig.Department.GetValue(CharacterConfig.CharacterInfo.GetValue(_From, "Department"), "Name");

                    var _CameObj = Resources.Load<GameObject>("HGF/Img-Character");
                    _CameObj.GetComponent<Image>().sprite = GameAPI.LoadTextureByIO($"{GameAPI.GetWritePath()}/HGF/Texture2D/Portrait/{CharacterConfig.CharacterInfo.GetValue(_From, "ResourcesPath")}/{CharacterConfig.CharacterInfo.GetValue(_From, "Portrait-Normall")}");
                    _.CharacterGameObject = Instantiate(_CameObj, Gal_CharacterImg.gameObject.transform);
                    _.CharacterGameObject.name = _.CharacterID;

                    if (PlotData.NowPlotDataNode.Attributes("SendMessage").Count() != 0)
                    {
                        _.CharacterGameObject.GetComponent<GalManager_CharacterAnimate>().Animate_StartOrOutside 
                            = PlotData.NowPlotDataNode.Attribute("SendMessage").Value;
                    }

                    PlotData.CharacterInfo.Add(_);

                    Button_Click_NextPlot();
                    break;
                }
            case "Speak":  //处理发言
                {
                    Gal_Text.gameObject.SetActive(true);
                    Gal_CharacterImg.gameObject.SetActive(true);
                    Gal_Choice.gameObject.SetActive(true);
                    Gal_LongSpeak.gameObject.SetActive(false);
                    var _nodeinfo = GetCharacterObjectByName(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                    if (PlotData.NowPlotDataNode.Elements().Count() != 0) //有选项，因为他有子节点数目了
                    {
                        IsShowingChioce = true;
                        foreach (var ClildItem in PlotData.NowPlotDataNode.Elements())
                        {
                            if (ClildItem.Name.ToString() == "Choice")
                                PlotData.ChoiceText.Add(new Struct_Choice(ClildItem.Value, ClildItem.Attribute("JumpID").Value, ClildItem.Attribute("TextType").Value));
                        }
                        Gal_Text.StartTextContent(PlotData.NowPlotDataNode.Attribute("Content").Value,
                             _nodeinfo.Name, PlotData.NowPlotDataNode.Attribute("TextType").Value, _nodeinfo.Affiliation, () =>
                        {

                            foreach (var ClildItem in GalManager.PlotData.ChoiceText)
                            {
                                Gal_Choice.CreatNewChoice(ClildItem.JumpID, ClildItem.Title, ClildItem.TextType);
                            }
                        });
                    }

                    else
                    {
                        Gal_Text.StartTextContent(PlotData.NowPlotDataNode.Attribute("Content").Value,
                         _nodeinfo.Name, PlotData.NowPlotDataNode.Attribute("TextType").Value, _nodeinfo.Affiliation);
                    }
                    loggerControler.LogContent(
                        "Speak",
                        PlotData.NowPlotDataNode.Attribute("TextType").Value,
                        PlotData.NowPlotDataNode.Attribute("Content").Value,
                        _nodeinfo.Name
                        );
                    //处理消息
                    if (PlotData.NowPlotDataNode.Attributes("SendMessage").Count() != 0)
                        SendCharMessage(_nodeinfo.CharacterID, PlotData.NowPlotDataNode.Attribute("SendMessage").Value);
                    if (PlotData.NowPlotDataNode.Attributes("AudioPath").Count() != 0)
                    {
                        string CharacterName = _nodeinfo.Name;
                        string VoicePath = GetAudioPath("Voice", PlotData.NowPlotDataNode.Attribute("AudioPath").Value,CharacterName);
                        StartCoroutine(PlayAudio(Gal_Voice, VoicePath));
                    }
                        
                    currentId = PlotData.NowPlotDataNode.Attribute("Id").Value;
                    var currentGameScriptName = GameMain.gameScriptName2Load;
                    GameMain.instance.galManager_Saver.saveDatas.SetReadState(currentId, currentGameScriptName);
                    currentContent = PlotData.NowPlotDataNode.Attribute("Content").Value;
                    currentTextType = PlotData.NowPlotDataNode.Attribute("TextType").Value;
                    break;
                }
            case "LongSpeak":
                {
                    Gal_Text.gameObject.SetActive(false);
                    Gal_CharacterImg.gameObject.SetActive(false);
                    Gal_Choice.gameObject.SetActive(false);
                    Gal_LongSpeak.gameObject.SetActive(true);
                    //var _nodeinfo = GetCharacterObjectByName(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                    string LoggerContent = string.Empty;
                       Gal_LongSpeak.StartTextContent(PlotData.NowPlotDataNode.Attribute("Content").Value,
                        PlotData.NowPlotDataNode.Attribute("Continue").Value,
                        PlotData.NowPlotDataNode.Attribute("End").Value,
                        PlotData.NowPlotDataNode.Attribute("TextType").Value,
                        PlotData.NowPlotDataNode.Attribute("Skip").Value,
                        out LoggerContent
                        );
                    loggerControler.LogContent(
                        "LongSpeak",
                        PlotData.NowPlotDataNode.Attribute("TextType").Value,
                        LoggerContent
                        );
                    currentId = PlotData.NowPlotDataNode.Attribute("Id").Value;
                    var currentGameScriptName = GameMain.gameScriptName2Load;
                    GameMain.instance.galManager_Saver.saveDatas.SetReadState(currentId, currentGameScriptName);
                    currentContent = PlotData.NowPlotDataNode.Attribute("Content").Value;
                    currentTextType = PlotData.NowPlotDataNode.Attribute("TextType").Value;
                    break;
                }
            case "ChangeBackImg"://更换背景图片
                {
                    var _Path = PlotData.NowPlotDataNode.Attribute("BackImgName").Value;
                    string path = GetBackGroundImagePath(_Path);
                    Gal_BackImg.SetImage(GameAPI.LoadTextureByIO(path));
                    Button_Click_NextPlot();
                    break;
                }
            case "DeleteCharacter":
                {
                    DestroyCharacterByID(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                    break;
                }
            case "NextScript"://加载后继脚本
                {
                    var _NextScriptName = PlotData.NowPlotDataNode.Attribute("NextScriptName").Value;
                    GameMain.gameScriptId2Load = "0";
                    GameMain.gameScriptName2Load = _NextScriptName;
                    GameMain.instance.galManager_Saver.Load(
                        GameMain.gameScriptId2Load, 
                        GameMain.gameScriptName2Load);
                    break;
                }
            //TODO:这里实现添加BGM
            case "SetBGM":
                {
                    string BGMName = PlotData.NowPlotDataNode.Attribute("BGMName").Value;
                    if (BGMName == "Stop")
                    {
                        BGM.Stop();
                    }
                    else
                    {
                        string BGMPath = GetAudioPath("BGM", BGMName);
                        StartCoroutine(PlayAudio(BGM, BGMPath));
                        Debug.Log(BGMPath);
                    }
                    Button_Click_NextPlot();
                    break;
                }
            case "CharacterAnimate":
                {
                    string AnimateType = PlotData.NowPlotDataNode.Attribute("SendMessage").Value; 
                    string CharacterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                    var Character = Gal_CharacterImg.transform.Find(CharacterId).gameObject;
                    Character.GetComponent<GalManager_CharacterAnimate>().Animate_type = AnimateType;
                    Character.GetComponent<GalManager_CharacterAnimate>().HandleMessgae();
                    Button_Click_NextPlot();
                    break;
                }
        }
        if (PlotData.BranchPlotInfo.Count == 0)
        {
            PlotData.IsBranch = false;
        }
        return;
    }
    public class CurrentData
    {
        //CurrentData中应该包含如下信息
        //如果是短对话，就应该包含此时人物立绘的位置信息，以及当前对话的内容
        //如果有选项，还要加载出选项
        //如果是长对话，就应该包含本个对话框中的所有内容
        public string currentSpeakType;//当前的对话类型，LongSpeak或者Speak
        public Dictionary<string , (string pos,string from)> characterInfo;//只保存位置信息，其他的不用这个保存
        public string backGroundPath;
        public string SpeakContent;
        public string currentLongSpeakContent;
        public string currentBGM;
        public string currentLongSpeakTextType;
        public Dictionary<string, List<string>> characterAnimations;
        public CurrentData()
        {
            currentSpeakType = null;
            backGroundPath = null;
            characterInfo = new();
            currentLongSpeakContent = "";
            currentLongSpeakTextType = "TexDraw";
            currentBGM = null;
            characterAnimations = new Dictionary<string, List<string>>();
        }
         public  static (string selection, string id) GetId(string id)
        {
            //判断输入id的类型，例如
            //"A-B"返回("A","B")
            //"C"返回(null,"C")
            string[] parts = id.Split(new[] { '-' }, 2);
            return parts.Length > 1 ? (parts[0], parts[1]) : (null, id);
        }
        public void setLongSpeakContent(string IsContiune,string content)
        {
            content = "\n\n" + content;
            //设置长对话的内容
            if (IsContiune == "0")
            {
                currentLongSpeakContent = content;
            }
            else
            {
                currentLongSpeakContent += content;
            }
        }
        public void reset()
        {
            currentSpeakType = null;
            backGroundPath = null;
            characterInfo = new();
            currentLongSpeakContent = "";
        }
    }

    public void _LoadPlot()
    {
        //在知道了PlotData以及CurrentData时，正确布置场景

        if (currentData.backGroundPath != null)
        {
            Gal_BackImg.SetImage(GameAPI.LoadTextureByIO(currentData.backGroundPath));
        }
        //加载BGM
        if (currentData.currentBGM != null&& currentData.currentBGM != "Stop")
        {
            StartCoroutine(PlayAudio(BGM, currentData.currentBGM));
        }
        foreach (var _ in PlotData.CharacterInfo)
        {
            var _CameObj = Resources.Load<GameObject>("HGF/Img-Character");
            var _moreinfo = currentData.characterInfo[_.CharacterID];
            string from = _moreinfo.from; string pos = _moreinfo.pos;
            _CameObj.GetComponent<Image>().sprite = GameAPI.LoadTextureByIO($"{GameAPI.GetWritePath()}/HGF/Texture2D/Portrait/{CharacterConfig.CharacterInfo.GetValue(from, "ResourcesPath")}/{CharacterConfig.CharacterInfo.GetValue(from, "Portrait-Normall")}");
            _.CharacterGameObject = Instantiate(_CameObj, Gal_CharacterImg.gameObject.transform);
            _.CharacterGameObject.name = _.CharacterID;
            if (pos != null)
            {
                _.CharacterGameObject.GetComponent<GalManager_CharacterAnimate>().Animate_StartOrOutside
                = _moreinfo.pos;
            }
            _.CharacterGameObject.GetComponent<GalManager_CharacterAnimate>().IsKill = true;
        }
        foreach (var _ in PlotData.CharacterInfo)
        {
            string CharacterId = _.CharacterID;
            var Character = Gal_CharacterImg.transform.Find(_.CharacterID).gameObject;
            
            if (currentData.characterAnimations.ContainsKey(CharacterId) ) 
            {
                string FinalAnimateType = string.Empty;
                foreach (string AnimateType in currentData.characterAnimations[CharacterId])
                {
                    FinalAnimateType = AnimateType;
                }
                Character.GetComponent<GalManager_CharacterAnimate>().Animate_type = FinalAnimateType;
                var animate = Character.GetComponent<GalManager_CharacterAnimate>().HandleMessgae();
                //animate.Complete(true);
                //animate.Kill();
            }

            //string AnimateType = PlotData.NowPlotDataNode.Attribute("SendMessage").Value;
            //string CharacterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
            //var Character = Gal_CharacterImg.transform.Find(CharacterId).gameObject;
            //Character.GetComponent<GalManager_CharacterAnimate>().Animate_type = AnimateType;
            //Character.GetComponent<GalManager_CharacterAnimate>().HandleMessgae();
            
            //break;
        }
        //如果是短对话，把人物放上去后点击一下
        if (currentData.currentSpeakType == "Speak")
        {
            
        }
        else if(currentData.currentSpeakType == "LongSpeak")
        {
           //Gal_LongSpeak.SetText_Content(currentData.currentLongSpeakContent);
            //Gal_LongSpeak.textComponent.ResetText();
            Gal_LongSpeak.TextType = currentData.currentLongSpeakTextType;
            Gal_LongSpeak.targetString = currentData.currentLongSpeakContent;
            Gal_LongSpeak.CurrentLen = currentData.currentLongSpeakContent.Length;
        }
        else
        {

        }
        //Button_Click_NextPlot();
        //加载场景
    }
    public void SetPlotData(string id)
    {
        //在知道了scriptName以及id后，正确修改PlotData文件
        bool IsFinish = false;
        while (!IsFinish)
        {
            IsFinish = Check(id);
            IsFinish = IsFinish || PlotData.MainPlot.Count == 0;
        }
    }
    public bool Check(string id)
    {
        var _id = CurrentData.GetId(id);
        string selection = _id.Item1;
        string selectedid = _id.Item2;
        if (!PlotData.IsBranch)
        {
            PlotData.NowPlotDataNode = PlotData.MainPlot.Peek();//队列出队+内联 出一个temp节点
                                                                   //PlotData.MainPlot.TryDequeue(out PlotData.NowPlotDataNode);
            PlotData.BranchPlotInfo.Clear();
        }
        else//当前为分支节点
        {
            //这块得妥善处理
            PlotData.NowPlotDataNode = GetBranchByID(PlotData.NowJumpID);
        }

        PlotData.ChoiceText.Clear();
        if (PlotData.NowPlotDataNode == null)
        {
            GameAPI.Print("无效的剧情结点", "error");
            return false; 
        }
        switch (PlotData.NowPlotDataNode.Name.ToString())
        {
            case "AddCharacter"://处理添加角色信息的东西
                {
                    var _ = new Struct_CharacterInfo();
                    var _From = PlotData.NowPlotDataNode.Attribute("From").Value;
                    var _CharacterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                    _.Name = CharacterConfig.CharacterInfo.GetValue(_From, "Name");
                    _.CharacterID = _CharacterId;
                    _.Affiliation = CharacterConfig.Department.GetValue(CharacterConfig.CharacterInfo.GetValue(_From, "Department"), "Name");

                    PlotData.CharacterInfo.Add(_);
                    string pos = null;
                    if (PlotData.NowPlotDataNode.Attributes("SendMessage").Count() != 0)
                    {
                        pos = PlotData.NowPlotDataNode.Attribute("SendMessage").Value;
                    }

                    currentData.characterInfo.Add(_.CharacterID, (pos, _From));
                    break;
                }
            case "Speak":  //处理发言
                {
                    var _nodeinfo = GetCharacterObjectByName(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                    if (PlotData.NowPlotDataNode.Attribute("Id").Value == id)
                    {
                        currentData.SpeakContent = PlotData.NowPlotDataNode.Attribute("Content").Value;
                        currentData.currentSpeakType = "Speak";
                        return true;
                    }
                    if (PlotData.NowPlotDataNode.Elements().Count() != 0) //有选项，因为他有子节点数目了
                    //如果有选项，要判断是否跳转选项         
                    {
                        //如果要加载选项
                        if (selection != null)
                        {
                            foreach (var ClildItem in PlotData.NowPlotDataNode.Elements())
                            {
                                if (ClildItem.Name.ToString() == "Choice")
                                {
                                    string choiceId = ClildItem.Attribute("JumpID").Value;
                                    if (choiceId == selection)
                                    {

                                        PlotData.NowJumpID = choiceId;
                                        PlotData.IsBranch = true;
                                        if (choiceId == "-1")
                                        {
                                            break;
                                        }
                                        PlotData.NowPlotDataNode = GetBranchByID(PlotData.NowJumpID);
                                        PlotData.MainPlot.Dequeue();
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            case "LongSpeak":
                {
                    //longspeak不加载选项，但是这里需要额外记录一下实际需要放的内容
                    if (PlotData.NowPlotDataNode.Attribute("Id").Value == id)
                    {
                        currentData.currentSpeakType = "LongSpeak";
                        return true;
                    }
                    string Iscontinue = PlotData.NowPlotDataNode.Attribute("Continue").Value;
                    string content = PlotData.NowPlotDataNode.Attribute("Content").Value;
                    currentData.setLongSpeakContent(Iscontinue, content);
                    currentData.currentLongSpeakTextType = PlotData.NowPlotDataNode.Attribute("TextType").Value;
                    break;
                }
            case "ChangeBackImg"://更换背景图片
                {
                    var _Path = PlotData.NowPlotDataNode.Attribute("BackImgName").Value;
                    string path = GetBackGroundImagePath(_Path);
                    currentData.backGroundPath = path;
                    break;
                }
            case "DeleteCharacter":
                {
                    string characterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                    var _ = PlotData.CharacterInfo.Find(t => t.CharacterID == characterId);
                    currentData.characterInfo.Remove(characterId);
                    PlotData.CharacterInfo.Remove(_);
                    break;
                }
            case "SetBGM":
                {
                    string BGMName = PlotData.NowPlotDataNode.Attribute("BGMName").Value;
                    if (BGMName != "Stop")
                    {
                        string BGMPath = GetAudioPath("BGM", BGMName);
                        //StartCoroutine(PlayAudio(BGM, BGMPath));
                        //Debug.Log(BGMPath);
                        //Button_Click_NextPlot();
                        currentData.currentBGM = BGMPath;
                    }
                    else
                    {
                        currentData.currentBGM = "Stop";
                    }
                    break;
                }
            case "CharacterAnimate":
                {
                    string AnimateType = PlotData.NowPlotDataNode.Attribute("SendMessage").Value;
                    string CharacterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                    if (currentData.characterAnimations.ContainsKey(CharacterId))
                    {
                        currentData.characterAnimations[CharacterId].Add(AnimateType);
                    }
                    else
                    {
                        currentData.characterAnimations[CharacterId] = new List<string>() { AnimateType };
                    }
                    
                    break;
                }
        }
        if (PlotData.BranchPlotInfo.Count == 0)
        {
            PlotData.IsBranch = false;
        }
        PlotData.MainPlot.Dequeue();
        return false;
    }
    public override IEnumerator LoadPlot(Action onComplete = null, string ScriptName = null)
    {
        yield return null;
        string _PlotText = string.Empty;
        string SavePath = "HGF/ScriptSheets/" + ScriptName + ".xml";
        //string filePath = Path.Combine(Application.streamingAssetsPath, "HGF/ScriptSheets/Test.xml");
        string filePath = Path.Combine(Application.streamingAssetsPath, SavePath);
        if (Application.platform == RuntimePlatform.Android)
        {

            filePath = "jar:file://" + Application.dataPath + "!/assets/HGF/ScriptSheets/Test.xml";
        }
        UnityWebRequest www = UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();

        if (www.isDone && !www.isNetworkError && !www.isHttpError)
        {
            _PlotText = www.downloadHandler.text;
        }
        else
        {
            Debug.Log("Error: " + www.error);
        }
        try
        {

            GameAPI.Print($"游戏剧本：{_PlotText}");
            PlotxDoc = XDocument.Parse(_PlotText);

            //-----开始读取数据

            foreach (var item in PlotxDoc.Root.Elements())
            {
                switch (item.Name.ToString())
                {
                    case "title":
                        {
                            PlotData.Title = item.Value;
                            break;
                        }
                    case "Synopsis":
                        {
                            PlotData.Synopsis = item.Value;
                            break;
                        }
                    case "BranchPlot":
                        {
                            foreach (var BranchItem in item.Elements())
                            {
                                PlotData.BranchPlot.Add(BranchItem);
                            }
                            break;
                        }
                    case "MainPlot":
                        {
                            foreach (var MainPlotItem in item.Elements())
                            {
                                PlotData.MainPlot.Enqueue(MainPlotItem);
                            }
                            break;
                        }
                    default:
                        {
                            throw new Exception("无法识别的根标签");

                        }
                }
            }
        }
        catch (Exception ex)
        {
            if (ex.Message != "无法识别的根标签")
            {

                GameAPI.Print(ex.Message, "error");
            }
        }
        //Button_Click_NextPlot();
        onComplete?.Invoke();  // 触发回调
        yield break;  // 确保协程正确结束
    }
    public void returnMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
