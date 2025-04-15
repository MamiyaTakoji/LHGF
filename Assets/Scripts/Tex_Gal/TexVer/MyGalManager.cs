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
        //�����BGM������StreamingAssets\HGF\Audio\BGM������ļ�
        //����ǽ�ɫ����������StreamingAssets\HGF\Audio\scriptName������ļ�
        //����scriptName����ű�����
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
            Debug.LogError("�����AudioType");
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
        //��ȡ.wav�ļ�����ת��AudioClip
        GameAPI.Print(filePath);
        AudioType audioType = Path.GetExtension(filePath).ToLower() switch
        {
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            _ => AudioType.UNKNOWN
        };
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType);
        //�ȴ�ת�����
        yield return www.SendWebRequest();
        //��ȡAudioClip
        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
        //���õ�ǰAudioSource�����AudioClip
        audioSource.clip = audioClip;
        //��������
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
            GameAPI.Print("��Ϸ����!");
            returnMainMenu();
            return;
        }

        //IsCanJump���������⣬���һֱ�����Ϊfalse��������˵true��������Ϊû�е����ť ��û����Ӱ�ť
        if (GalManager_Text.IsSpeak || !GalManager_Text.IsCanJump) { return; }
        if (!PlotData.IsBranch)
        {
            PlotData.NowPlotDataNode = PlotData.MainPlot.Dequeue();//���г���+���� ��һ��temp�ڵ�
                                                                   //PlotData.MainPlot.TryDequeue(out PlotData.NowPlotDataNode);
            PlotData.BranchPlotInfo.Clear();
        }
        else//��ǰΪ��֧�ڵ�
        {
            //�������ƴ���
            PlotData.NowPlotDataNode = GetBranchByID(PlotData.NowJumpID);
        }

        PlotData.ChoiceText.Clear();
        if (PlotData.NowPlotDataNode == null)
        {
            GameAPI.Print("��Ч�ľ�����", "error");
            return;
        }
        Debug.Log(PlotData.NowPlotDataNode);
        switch (PlotData.NowPlotDataNode.Name.ToString())
        {
            case "AddCharacter"://������ӽ�ɫ��Ϣ�Ķ���
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
            case "Speak":  //������
                {
                    Gal_Text.gameObject.SetActive(true);
                    Gal_CharacterImg.gameObject.SetActive(true);
                    Gal_Choice.gameObject.SetActive(true);
                    Gal_LongSpeak.gameObject.SetActive(false);
                    var _nodeinfo = GetCharacterObjectByName(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                    if (PlotData.NowPlotDataNode.Elements().Count() != 0) //��ѡ���Ϊ�����ӽڵ���Ŀ��
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
                    //������Ϣ
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
            case "ChangeBackImg"://��������ͼƬ
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
            case "NextScript"://���غ�̽ű�
                {
                    var _NextScriptName = PlotData.NowPlotDataNode.Attribute("NextScriptName").Value;
                    GameMain.gameScriptId2Load = "0";
                    GameMain.gameScriptName2Load = _NextScriptName;
                    GameMain.instance.galManager_Saver.Load(
                        GameMain.gameScriptId2Load, 
                        GameMain.gameScriptName2Load);
                    break;
                }
            //TODO:����ʵ�����BGM
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
        //CurrentData��Ӧ�ð���������Ϣ
        //����Ƕ̶Ի�����Ӧ�ð�����ʱ���������λ����Ϣ���Լ���ǰ�Ի�������
        //�����ѡ���Ҫ���س�ѡ��
        //����ǳ��Ի�����Ӧ�ð��������Ի����е���������
        public string currentSpeakType;//��ǰ�ĶԻ����ͣ�LongSpeak����Speak
        public Dictionary<string , (string pos,string from)> characterInfo;//ֻ����λ����Ϣ�������Ĳ����������
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
            //�ж�����id�����ͣ�����
            //"A-B"����("A","B")
            //"C"����(null,"C")
            string[] parts = id.Split(new[] { '-' }, 2);
            return parts.Length > 1 ? (parts[0], parts[1]) : (null, id);
        }
        public void setLongSpeakContent(string IsContiune,string content)
        {
            content = "\n\n" + content;
            //���ó��Ի�������
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
        //��֪����PlotData�Լ�CurrentDataʱ����ȷ���ó���

        if (currentData.backGroundPath != null)
        {
            Gal_BackImg.SetImage(GameAPI.LoadTextureByIO(currentData.backGroundPath));
        }
        //����BGM
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
        //����Ƕ̶Ի������������ȥ����һ��
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
        //���س���
    }
    public void SetPlotData(string id)
    {
        //��֪����scriptName�Լ�id����ȷ�޸�PlotData�ļ�
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
            PlotData.NowPlotDataNode = PlotData.MainPlot.Peek();//���г���+���� ��һ��temp�ڵ�
                                                                   //PlotData.MainPlot.TryDequeue(out PlotData.NowPlotDataNode);
            PlotData.BranchPlotInfo.Clear();
        }
        else//��ǰΪ��֧�ڵ�
        {
            //�������ƴ���
            PlotData.NowPlotDataNode = GetBranchByID(PlotData.NowJumpID);
        }

        PlotData.ChoiceText.Clear();
        if (PlotData.NowPlotDataNode == null)
        {
            GameAPI.Print("��Ч�ľ�����", "error");
            return false; 
        }
        switch (PlotData.NowPlotDataNode.Name.ToString())
        {
            case "AddCharacter"://������ӽ�ɫ��Ϣ�Ķ���
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
            case "Speak":  //������
                {
                    var _nodeinfo = GetCharacterObjectByName(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                    if (PlotData.NowPlotDataNode.Attribute("Id").Value == id)
                    {
                        currentData.SpeakContent = PlotData.NowPlotDataNode.Attribute("Content").Value;
                        currentData.currentSpeakType = "Speak";
                        return true;
                    }
                    if (PlotData.NowPlotDataNode.Elements().Count() != 0) //��ѡ���Ϊ�����ӽڵ���Ŀ��
                    //�����ѡ�Ҫ�ж��Ƿ���תѡ��         
                    {
                        //���Ҫ����ѡ��
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
                    //longspeak������ѡ�����������Ҫ�����¼һ��ʵ����Ҫ�ŵ�����
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
            case "ChangeBackImg"://��������ͼƬ
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

            GameAPI.Print($"��Ϸ�籾��{_PlotText}");
            PlotxDoc = XDocument.Parse(_PlotText);

            //-----��ʼ��ȡ����

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
                            throw new Exception("�޷�ʶ��ĸ���ǩ");

                        }
                }
            }
        }
        catch (Exception ex)
        {
            if (ex.Message != "�޷�ʶ��ĸ���ǩ")
            {

                GameAPI.Print(ex.Message, "error");
            }
        }
        //Button_Click_NextPlot();
        onComplete?.Invoke();  // �����ص�
        yield break;  // ȷ��Э����ȷ����
    }
    public void returnMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
