using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static GameData.Struct_PlotData;
using static GameData;
using System.Linq;

public class GameData : MonoBehaviour
{
    public string CurrentGameScriptId = "0";
    private string GameScriptContent = string.Empty;
    public string CurrentScriptName = "FirstScript";
    public XDocument PlotxDoc;
    public Struct_PlotData PlotData;
    public CurrentData currentData;
    public void SetCurrentInfo(string  ScriptId, string ScriptName)
    {
        CurrentGameScriptId = ScriptId;
       CurrentScriptName = ScriptName;
    }

    public void LoadGameScript(Action onComplete = null)
    {
          StartCoroutine(LoadXML(
            onComplete: (Content) =>
            {
                GameScriptContent = Content;
                GetScriptDoc(GameScriptContent);
                InitCurrentData();
                onComplete.Invoke();
                currentData.currentScriptName = CurrentScriptName;
                currentData.currentID = CurrentGameScriptId;
            },
            ScriptName: CurrentScriptName
            ));
    }
    public void InitCurrentData()
    {
        currentData = new CurrentData();
    }
    public void GetScriptDoc(string Content)
    {
        PlotxDoc = XDocument.Parse(Content);
        PlotData = new Struct_PlotData();
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
    public IEnumerator LoadXML(Action<string> onComplete = null, string ScriptName = null, string FilePath = "HGF/ScriptSheets/")
    {
        yield return null;
        string SavePath = "HGF/ScriptSheets/" + ScriptName + ".xml";
        //string SavePath = FilePath + ScriptName + ".xml";
        //string filePath = Path.Combine(Application.streamingAssetsPath, "HGF/ScriptSheets/Test.xml");
        //string filePath = Path.Combine(Application.streamingAssetsPath, SavePath);
        string filePath = Application.streamingAssetsPath + '/' + SavePath;
        //UnityWebRequest www = UnityWebRequest.Get("D:/Unity/GameRebuilded/Assets/HGF/StreamingAssets/HGF/ScriptSheets/FirstScript.xml");
        UnityWebRequest www = UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();
        Debug.Log(SavePath);
        Debug.Log(filePath);
        Debug.Log(www.responseCode);
        if (www.isDone && !www.isNetworkError && !www.isHttpError)
        {
            onComplete?.Invoke(www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Error: " + www.error);
            onComplete?.Invoke(null);
        }
        yield break;  // 确保协程正确结束
    }
    private class CharacterConfig
    {
        public static GameConfig CharacterInfo;
        public static GameConfig Department;

        static CharacterConfig()
        {
            CharacterInfo = new GameConfig(Utils.GetWritePath() + "/HGF/CharacterInfo.ini");
            Department = new GameConfig(Utils.GetWritePath() + "/HGF/Department.ini");
        }
    }
    public class Struct_PlotData
    {
        public string Title;
        public string Synopsis;
        public List<XElement> BranchPlot = new List<XElement>();
        public Queue<XElement> BranchPlotInfo = new Queue<XElement>();
        public Queue<XElement> MainPlot = new Queue<XElement>();
        public class Struct_Choice
        {
            public Struct_Choice(string Title, string JumpID, string textType)
            {
                this.Title = Title;
                this.JumpID = JumpID;
                TextType = textType;
            }
            public string Title;
            public string JumpID;
            public string TextType;
        }
        //public List<Struct_CharacterInfo> CharacterInfo = new List<Struct_CharacterInfo>();
        public List<Struct_Choice> ChoiceText = new List<Struct_Choice>();
        /// <summary>
        /// 当前的剧情节点
        /// </summary>
        public XElement NowPlotDataNode;

        /// <summary>
        /// 当前是否为分支剧情节点
        /// </summary>
        public bool IsBranch = false;
        public string NowJumpID;
        public void Struct_PlotDataProgress()
        {
            if (!IsBranch)
            {
                NowPlotDataNode = MainPlot.Dequeue();//队列出队+内联 出一个temp节点
                                                                       //PlotData.MainPlot.TryDequeue(out PlotData.NowPlotDataNode);
                BranchPlotInfo.Clear();
            }
            else
            {
                GetBranchByID(NowJumpID);
            }
        }
        public XElement GetBranchByID(string ID)
        {
            if (BranchPlotInfo.Count == 0)
            {
                foreach (var item in BranchPlot.Find(t => t.Attribute("ID").Value == ID).Elements())
                {
                    BranchPlotInfo.Enqueue(item);
                }
                return BranchPlotInfo.Peek();
            }
            else
            {
                //PlotData.BranchPlotInfo.TryDequeue(out XElement t);
                return BranchPlotInfo.Dequeue();
            }
        }
        public void SetNowJumpID(string JumpID)
        {
            NowJumpID = JumpID;
        }
    }
    public string GetNodeValue(XElement Node ,string Arg)
    {
        return Node.Attribute(Arg).Value;
    }
    public class CurrentData
    {
        //CurrentData中应该包含如下信息
        //如果是短对话，就应该包含此时人物立绘的位置信息，以及当前对话的内容
        //如果有选项，还要加载出选项
        //如果是长对话，就应该包含本个对话框中的所有内容
        public Dictionary<string, string> characterStartInfo;
        //只保存位置信息，其他的不用这个保存
        public CurrentSpeakData currentSpeakData;
        public CurrentLongSpeakData currentLongSpeakData;
        public CurrentcCharactersInfo currentcCharactersInfo = new();
        public CurrentChoiceData currentChoiceData = new();
        public string currentDialogType;
        public string currentBackground;
        public string currentBGM = string.Empty;
        public string currentScriptName;
        public string currentID;
        public string NextScriptName = string.Empty;
        public bool IsGameFinish = false;
        public bool IsLoadNextScript = false;
        public class Struct_CharacterInfo
        {
            public string CharacterID;
            public GameObject CharacterGameObject;
            public string Name;
            public string Affiliation;
            public string From;
        }
        public class CurrentChoiceData
        {
            //需要保存选项的文本，以及选项跳转的Id
            public Dictionary<string ,(string ChoiceContent, string ChoiceContentType)> ChoiceInfo = new();
            public void SetChoiceInfo(string ChoiceId, string ChoiceContent, string ChoiceContentType)
            {
                ChoiceInfo[ChoiceId] = (ChoiceContent, ChoiceContentType);
            }
            public void ResetChoiceInfo()
            {
                ChoiceInfo = new();
            }
        }
        public class CurrentSpeakData
        {
            //这里存放Speak类型数据需要的数据
            public string currentSpeakContent = null;
            public string currentSpeakTextType = null;
            public string currentVoicePath = null;
            public string currentSpeaker = null;
            public string currentDepartment = null;
            public bool IsSkip = false;
        }
        public class CurrentcCharactersInfo
        {
            //加载存档与游戏运行时逻辑可能是不一样的,需要分别实现

            //所有角色加载时需要使用的动画
            public Dictionary<string, string> charactersAnimation_OnLoad = new();

            //所有角色的角色信息
            public Dictionary<string, Struct_CharacterInfo> charactersNodeinfo = new();

            //下一个命令需要播放的角色动画
            public Dictionary<string,string> CharacterAnimation2show_OnPlay = new();

            //下一个命令需要添加的角色及其对应动画
            public Dictionary<string, string> CharacterAnimation2add_OnPlay = new();

            //下一个命令需要删除的角色及其对应的动画
            public Dictionary<string, string> CharacterAnimation2delete_OnPlay = new();

            //下一个命令需要更换的立绘
            public Dictionary<string, string> CharacterPortrait2Change_OnPlay = new();

            //添加角色的信息以及动画信息
            public void AddCharacter(string _addCharacterId, Struct_CharacterInfo NodeInfo, string _addCharacterAnimation)
            {
                charactersAnimation_OnLoad[_addCharacterId] = _addCharacterAnimation;
                CharacterAnimation2add_OnPlay[_addCharacterId] = _addCharacterAnimation;
                charactersNodeinfo[_addCharacterId] = NodeInfo;
            }

            //记录需要移除角色的信息
            public void RemoveCharacter_OnPlay(string deleteCharacterId)
            {
                CharacterAnimation2delete_OnPlay[deleteCharacterId] = "Quit";
            }

            public void OnCharacterRemove()
            {
                foreach(var character in CharacterAnimation2delete_OnPlay.Keys)
                {
                    charactersNodeinfo.Remove(character);
                }
            }

            //加载时通过这个移除角色
            public void RemoveCharacter_OnLoad(string deleteCharacterId)
            {
                charactersNodeinfo.Remove(deleteCharacterId);
                CharacterAnimation2show_OnPlay.Remove(deleteCharacterId);
                CharacterAnimation2add_OnPlay.Remove(deleteCharacterId);
                CharacterPortrait2Change_OnPlay.Remove(deleteCharacterId);
                CharacterAnimation2delete_OnPlay.Remove(deleteCharacterId);
            }

            //添加角色动画
            public void AddCharacterAnimation_OnLoad(string CharacterId, string Animation)
            {
                charactersAnimation_OnLoad[CharacterId] = Animation;
            }

            public void AddCharacterAnimation_OnPlay(string CharacterId, string Animation)
            {
                CharacterAnimation2show_OnPlay[CharacterId] = Animation;
            }

            public void SetCharacterPortrait_OnPlay(string CharacterId, string PortraitName)
            {
                CharacterPortrait2Change_OnPlay[CharacterId] = PortraitName;
            }

            public void Reset()
            {
                CharacterAnimation2show_OnPlay = new();
                CharacterAnimation2add_OnPlay = new();
                CharacterAnimation2delete_OnPlay = new();
                CharacterPortrait2Change_OnPlay = new();
            }

            
        }
        public class CurrentLongSpeakData
        {
            //这里存放LongSpeak类型数据需要的数据
            public string currentLongSpeakContent = null;
            public string currentLongSpeakTextType = null;
            public string currentVoicePath = null;
            public string isContinue = null;
            public string isEnd = null;
            public bool isSkip = false;
            public List<string> totalContent = new List<string>();
        }
        public CurrentData()
        {
            currentScriptName = "FirstScript"; 
            currentID = "-1";
            Reset();
        }
        public void Reset()
        {
            currentSpeakData = null;
            if (currentLongSpeakData!=null&&currentLongSpeakData.isEnd == "1")
            {
                currentLongSpeakData = null;
            }
            currentBackground = null;
            currentBGM = null;
        }
        public static (string selection, string id) GetId(string id)
        {
            //判断输入id的类型，例如
            //"A-B"返回("A","B")
            //"C"返回(null,"C")
            string[] parts = id.Split(new[] { '-' }, 2);
            return parts.Length > 1 ? (parts[0], parts[1]) : (null, id);
        }

        public Struct_CharacterInfo GetCharacterObjectByName(List<Struct_CharacterInfo> CharacterInfos, string ID)
        {
            return CharacterInfos.Find(t => t.CharacterID == ID);
        }
        public string GetBackGroundImagePath(string BackGroundImageName)
        {
            //string path = $"{Utils.GetWritePath()}/HGF/Texture2D/BackgroundImage/{BackGroundImageName}";
            string path = Path.Combine(Utils.ResoucePaths.BackgroundPath, BackGroundImageName);
            return path;
        }
        public string GetAudioPath(string AudioType, string fileName, string characterName = "", string ScriptName = "FirstScript")
        {
            //如果是BGM，返回StreamingAssets\HGF\Audio\BGM里面的文件
            //如果是角色语音，返回StreamingAssets\HGF\Audio\scriptName里面的文件
            //其中scriptName代表脚本名字
            if (AudioType == "Voice")
            {
                //string path = $"{Utils.GetWritePath()}/HGF/Audio/{ScriptName}/{characterName}/{fileName}";
                string path = Path.Combine(Utils.ResoucePaths.VoicePath, ScriptName, characterName, fileName);
                return path;
            }
            else if (AudioType == "BGM")
            {
                //string path = $"{Utils.GetWritePath()}/HGF/Audio/BGM/{fileName}";
                string path = Path.Combine(Utils.ResoucePaths.VoicePath, "BGM", fileName);
                return path;
            }
            else
            {
                Debug.LogError("错误的AudioType");
                return null;
            }
        }
        public void SetPlotData(Struct_PlotData _PlotData, string id)
        {
            //在知道了scriptName以及id后，正确修改PlotData文件
            bool IsFinish = false;
            while (!IsFinish)
            {
                IsFinish = Check(_PlotData,id,IsOnPlay:false);
                IsFinish = IsFinish || _PlotData.MainPlot.Count == 0;
            }
        }
        
        public bool Check(Struct_PlotData _PlotData ,string id, bool IsOnPlay = true)
        {
            currentChoiceData.ResetChoiceInfo();
            var _id = GetId(id);
            string selection = _id.Item1;
            string selectedid = _id.Item2;
            if (!_PlotData.IsBranch)
            {
                _PlotData.NowPlotDataNode = _PlotData.MainPlot.Dequeue();//队列出队+内联 出一个temp节点
                                                                      //PlotData.MainPlot.TryDequeue(out PlotData.NowPlotDataNode);
                                                                      //更新剧本的ID
                currentID = _PlotData.NowPlotDataNode.Attribute("Id")?.Value ?? "-1";
                _PlotData.BranchPlotInfo.Clear();
                if (_PlotData.MainPlot.Count == 0)
                {
                    IsGameFinish = true;
                }
                //Debug.Log(_PlotData.NowPlotDataNode.ToString());
            }
            else//当前为分支节点
            {
                //这块得妥善处理
                _PlotData.NowPlotDataNode = _PlotData.GetBranchByID(_PlotData.NowJumpID);
            }
            switch (_PlotData.NowPlotDataNode.Name.ToString())
            {
                case "AddCharacter"://处理添加角色信息的东西
                    {
                        var _ = new Struct_CharacterInfo();
                        var _From = _PlotData.NowPlotDataNode.Attribute("From").Value;
                        var _CharacterId = _PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                        _.From = _From;
                        _.Name = CharacterConfig.CharacterInfo.GetValue(_From, "Name");
                        _.CharacterID = _CharacterId;
                        _.Affiliation = CharacterConfig.Department.GetValue(CharacterConfig.CharacterInfo.GetValue(_From, "Department"), "Name");
                        string Message = null;
                        if (_PlotData.NowPlotDataNode.Attributes("SendMessage").Count() != 0)
                        {
                            Message = _PlotData.NowPlotDataNode.Attribute("SendMessage").Value;
                        }
                        currentcCharactersInfo.AddCharacter(_CharacterId, _, Message);
                        break;
                    }
                case "Speak":  //处理发言
                    {
                        currentDialogType = "Speak";
                        currentSpeakData = new();
                        currentID = _PlotData.NowPlotDataNode.Attribute("Id").Value;
                        //var _nodeinfo = GetCharacterObjectByName(_PlotData.CharacterInfo ,_PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                        var _nodeinfo = currentcCharactersInfo.charactersNodeinfo[_PlotData.NowPlotDataNode.Attribute("CharacterID").Value];
                        string voice = _PlotData.NowPlotDataNode.Attribute("AudioPath")?.Value ?? null;
                        if (voice != null)
                        {
                            currentSpeakData.currentVoicePath = GetAudioPath("Voice", voice, _nodeinfo.From, GameMain.ScriptName);
                        }
                        currentSpeakData.currentSpeakContent = _PlotData.NowPlotDataNode.Attribute("Content").Value;
                        currentSpeakData.currentSpeakTextType = _PlotData.NowPlotDataNode.Attribute("TextType").Value;
                        if (_PlotData.NowPlotDataNode.Attribute("Skip") != null)
                        {
                            if (_PlotData.NowPlotDataNode.Attribute("Skip").Value == "1")
                            {
                                currentSpeakData.IsSkip = true;
                            }
                        }
                        currentSpeakData.currentSpeaker = _nodeinfo.Name;
                        currentSpeakData.currentDepartment = _nodeinfo.Affiliation;
                        if (_PlotData.NowPlotDataNode.Elements().Count() != 0) //有选项，因为他有子节点数目了
                                                                               //如果有选项，要判断是否跳转选项
                                                                               //OnLoad和OnPlay对于选项的情况要分开来处理
                        {
                            foreach (var ClildItem in _PlotData.NowPlotDataNode.Elements())
                            {
                                if (ClildItem.Name.ToString() == "Choice")
                                {
                                    string choiceId = ClildItem.Attribute("JumpID").Value;
                                    string choiceContent = ClildItem.Value;
                                    string choiceContentType = ClildItem.Attribute("TextType").Value;
                                    currentChoiceData.SetChoiceInfo(choiceId, choiceContent, choiceContentType);
                                }
                            }
                            //如果要加载选项
                            if (selection != null)
                            {
                                foreach (var ClildItem in _PlotData.NowPlotDataNode.Elements())
                                {
                                    if (ClildItem.Name.ToString() == "Choice")
                                    {
                                        string choiceId = ClildItem.Attribute("JumpID").Value;
                                        if (choiceId == selection)
                                        {

                                            _PlotData.NowJumpID = choiceId;
                                            _PlotData.IsBranch = true;
                                            if (choiceId == "-1")
                                            {
                                                break;
                                            }
                                            _PlotData.NowPlotDataNode = _PlotData.GetBranchByID(_PlotData.NowJumpID);
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                        if (_PlotData.NowPlotDataNode.Attribute("Id").Value == id)
                        {
                            return true;
                        }
                        break;
                    }
                case "LongSpeak":
                    {
                        //longspeak不加载选项，但是这里需要额外记录一下实际需要放的内容
                        currentDialogType = "LongSpeak";
                        currentID = _PlotData.NowPlotDataNode.Attribute("Id").Value;
                        if (currentLongSpeakData == null)
                        {
                            currentLongSpeakData = new();
                        }
                        currentLongSpeakData.isContinue = _PlotData.NowPlotDataNode.Attribute("Continue").Value;
                        if (currentLongSpeakData.isContinue == "0")
                        {
                            currentLongSpeakData = new();
                        }
                        string voice = _PlotData.NowPlotDataNode.Attribute("AudioPath")?.Value ?? null;
                        if (voice != null)
                        {
                            currentLongSpeakData.currentVoicePath = GetAudioPath("Voice", voice, "LongSpeak", GameMain.ScriptName);
                        }
                        currentLongSpeakData.isContinue = _PlotData.NowPlotDataNode.Attribute("Continue").Value;
                        currentLongSpeakData.currentLongSpeakContent = _PlotData.NowPlotDataNode.Attribute("Content").Value;
                        currentLongSpeakData.isEnd = _PlotData.NowPlotDataNode.Attribute("End").Value;
                        currentLongSpeakData.currentLongSpeakTextType = _PlotData.NowPlotDataNode.Attribute("TextType").Value;
                        currentLongSpeakData.totalContent.Add(currentLongSpeakData.currentLongSpeakContent);
                        currentLongSpeakData.isSkip = false;
                        if (_PlotData.NowPlotDataNode.Attribute("Skip") != null)
                        {
                            if (_PlotData.NowPlotDataNode.Attribute("Skip").Value == "1")
                            {
                                currentLongSpeakData.isSkip = true;
                            }
                        }

                        if (_PlotData.NowPlotDataNode.Attribute("Id").Value == id)
                        {
                            return true;
                        }
                        break;
                    }
                case "ChangeBackImg"://更换背景图片
                    {
                        var _Path = _PlotData.NowPlotDataNode.Attribute("BackImgName").Value;
                        string path = GetBackGroundImagePath(_Path);
                        currentBackground = path;
                        break;
                    }
                case "DeleteCharacter":
                    {
                        string characterId = _PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                        if (IsOnPlay)
                        {
                            currentcCharactersInfo.RemoveCharacter_OnPlay(characterId);
                        }
                        else
                        {
                            currentcCharactersInfo.RemoveCharacter_OnLoad(characterId);
                        }
                        break;
                    }
                case "SetBGM":
                    {
                        string BGMName = _PlotData.NowPlotDataNode.Attribute("BGMName").Value;
                        if (BGMName != "Stop")
                        {
                            string BGMPath = GetAudioPath("BGM", BGMName, ScriptName: currentScriptName);
                            //StartCoroutine(PlayAudio(BGM, BGMPath));
                            //Debug.Log(BGMPath);
                            //Button_Click_NextPlot();
                            currentBGM = BGMPath;
                        }
                        else
                        {
                            currentBGM = "Stop";
                        }
                        break;
                    }
                case "CharacterAnimate":
                    {
                        string AnimateType = _PlotData.NowPlotDataNode.Attribute("SendMessage").Value;
                        string CharacterId = _PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                        currentcCharactersInfo.AddCharacterAnimation_OnPlay(CharacterId, AnimateType);
                        break;
                    }
                case "CharacterPortrait":
                    {
                        string CharacterPortrait = _PlotData.NowPlotDataNode.Attribute("CharacterPortrait").Value;
                        string CharacterId = _PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                        currentcCharactersInfo.CharacterPortrait2Change_OnPlay[CharacterId] = CharacterPortrait;
                        break;
                    }
                case "NextScript":
                    {
                        NextScriptName = _PlotData.NowPlotDataNode.Attribute("NextScriptName").Value;
                        IsLoadNextScript = true;
                        break;
                    }
            }
            if (_PlotData.BranchPlotInfo.Count == 0)
            {
                _PlotData.IsBranch = false;
            }
            return false;
        }
    }
}
