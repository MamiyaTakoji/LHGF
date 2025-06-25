using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameProgress : MonoBehaviour
{
    public DialogControler dialogControler;
    public LongDialogControler longDialogControler;
    public Img_CharactersShower img_CharactersShower;
    public SetChoice ChoiceSetter;
    public BackImgControler backImgControler;
    public Tweener CurrentTextTweener;
    public Sequence CurrentCharacterTweeners;
    public Tweener CurrentBackgroundTweener;
    public LoggerControler loggerControler;
    public bool IsSkipable = true;
    public bool IsTextTweenerFinish = false;
    public bool IsCharacterTweenerFinish = false ;
    public bool IsOnChoice = false;
    public bool IsBGMOnPlay = false;
    public bool IsVoiceOnPlay = false;
    public float WaitTime = 0.1f;
    public float WaitTime2Auto = 3f;
    public float Count = 0f;
    public float Count2Auto = 0f;
    public GameObject Img_Character;
    public SoundsControler soundsControler;
    public void OnGaneProgressUpdata(float deltaTime)
    {
        if (!IsSkipable)
        {
           SetCounter(deltaTime);
        }
    }
    public void SetCounter(float deltaTime)
    {
        Count += deltaTime;
        if(Count>WaitTime) 
        {
            Count = 0f;
            IsSkipable = true;
        }
    }
    public void OnGameStart()
    {
        string ScriptId = GameMain.ScriptId;
        string ScriptName = GameMain.ScriptName;
        GameMain.instance.gameData.SetCurrentInfo(ScriptId: ScriptId, ScriptName: ScriptName);
        GameMain.instance.gameData.LoadGameScript(
            onComplete: () => 
            {
                GameMain.instance.gameData.currentData.SetPlotData
                (
                    GameMain.instance.gameData.PlotData,
                    GameMain.instance.gameData.CurrentGameScriptId
                );
                SetScence(IsOnLoad:true);
            }
            );
    }
    public void SetScence(bool IsOnLoad = false)
    {
        var CurrentData = GameMain.instance.gameData.currentData;

        if (IsOnLoad)
        {
            CharacterAnimation_OnLoad();
            if(CurrentData.currentBackground != null)
            {
                backImgControler.SetImage_OnLoad(CurrentData.currentBackground);
                CurrentData.currentBackground = null;
            }
        }
        else
        {
            CharacterAnimation_OnPlay();
            if (CurrentData.currentBackground != null)
            {
                backImgControler.SetImage(CurrentData.currentBackground);
                CurrentData.currentBackground = null;
            }
        }
        if (CurrentData.currentBGM == "Stop")
        {
            soundsControler.StopBGM();
            IsBGMOnPlay = false;
        }
        else
        {
            if (IsBGMOnPlay == false && CurrentData.currentBGM != null)
            {
                StartCoroutine(soundsControler.PlayBGM(CurrentData.currentBGM));
                Debug.Log(CurrentData.currentBGM);
                IsBGMOnPlay = true;
            }
        }
        if (CurrentData.currentDialogType == "Speak")
        {
            longDialogControler.gameObject.SetActive(false);
            dialogControler.gameObject.SetActive(true);
            IsTextTweenerFinish = false;
            var currentSpeakData = CurrentData.currentSpeakData;
            loggerControler.LogSpeakContent(
                currentSpeakData.currentSpeakTextType,
                currentSpeakData.currentSpeakContent,
                currentSpeakData.currentSpeaker);
            CurrentTextTweener = dialogControler.StartTextContent
            (CurrentData.currentSpeakData.currentSpeakContent,
            CurrentData.currentSpeakData.currentSpeaker,
            CurrentData.currentSpeakData.currentSpeakTextType,
            CurrentData.currentSpeakData.currentDepartment,
            CurrentData.currentSpeakData.IsSkip
            );
            CurrentTextTweener.onComplete += (() => 
            { IsTextTweenerFinish = true;
              var ChoiceInfo = CurrentData.currentChoiceData.ChoiceInfo;
              foreach (var Key in ChoiceInfo.Keys)
                {
                    IsOnChoice = true;
                    ChoiceSetter.SetChoiceButton(Key, ChoiceInfo[Key].ChoiceContent, ChoiceInfo[Key].ChoiceContentType);
                }
            });
            if(CurrentData.currentSpeakData.currentVoicePath != null)
            {
                Debug.Log(CurrentData.currentSpeakData.currentVoicePath);
                StartCoroutine(soundsControler.PlayVoice(CurrentData.currentSpeakData.currentVoicePath));
            }
            return;
        }
        else if (CurrentData.currentDialogType == "LongSpeak")
        {
            longDialogControler.gameObject.SetActive(true);
            dialogControler.gameObject.SetActive(false);
            string FinalContent;
            IsTextTweenerFinish = false;
            var currentLongSpeakData = CurrentData.currentLongSpeakData;
            if (CurrentData.currentLongSpeakData.currentVoicePath != null)
            {
                var path = CurrentData.currentLongSpeakData.currentVoicePath;
                Debug.Log(path);
                StartCoroutine(soundsControler.PlayVoice(path));
            }
            CurrentTextTweener = longDialogControler.StartTextContent
            (
                TextContent: currentLongSpeakData.currentLongSpeakContent,
                IsContiune: currentLongSpeakData.isContinue,
                IsEnd: currentLongSpeakData.isEnd,
                _TextType: currentLongSpeakData.currentLongSpeakTextType,
                Skip: currentLongSpeakData.isSkip,
                TotalContent: currentLongSpeakData.totalContent,
                out FinalContent
            );
            CurrentTextTweener.onComplete += (() => 
            { IsTextTweenerFinish = true;
                loggerControler.LogLongSpeakContent
                  (currentLongSpeakData.currentLongSpeakTextType,
                  FinalContent);
            });
            return;
        }
    }
    public void ButtonClickNext()
    {
        string ScriptName = GameMain.instance.gameData.currentData.currentScriptName;
        string ScriptID = GameMain.instance.gameData.currentData.currentID;
        var CurrentData = GameMain.instance.gameData.currentData;
        GameMain.instance.gameSL.saveDatas.UpdataReadedId(ScriptName, ScriptID);
        if (!IsSkipable|IsOnChoice) 
        {
            return;
        }
        soundsControler.StopVoice();
        if(!IsTextTweenerFinish||!IsCharacterTweenerFinish||soundsControler.IsVoiceFinish>0)
        {
            CurrentCharacterTweeners.Complete();
            CurrentTextTweener.Complete();
            IsTextTweenerFinish = true;
            soundsControler.IsVoiceFinish = 0;
            return;
        }
        var PlotData = GameMain.instance.gameData.PlotData;
        
        if (CurrentData.IsGameFinish)
        {
            GameMain.ScenceId = 0;
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            return;
        }
        CurrentData.Check(PlotData, "$#@-2");
        if (CurrentData.IsLoadNextScript == true)
        {
            CurrentData.IsLoadNextScript = false;
            LoadNextScript(CurrentData.NextScriptName);
        }
        while (CurrentData.currentID == "-1"&&!CurrentData.IsGameFinish)
        {
            CurrentData.Check(PlotData, "-1");
        }

        //通过CurrentData布置场景
        SetScence();
    }
    public void CharacterAnimation_OnLoad()
    {
        //分别执行加载角色,角色动画,角色退场
        var currentData = GameMain.instance.gameData.currentData;
        var CharacterInfo = currentData.currentcCharactersInfo.charactersNodeinfo;
        var Character2Add = currentData.currentcCharactersInfo.CharacterAnimation2add_OnPlay;
        var Charater2Show = currentData.currentcCharactersInfo.CharacterAnimation2show_OnPlay;
        var CharacterPortrait2Change = currentData.currentcCharactersInfo.CharacterPortrait2Change_OnPlay;
        var _img_CharactersShower = img_CharactersShower.GetComponent<Img_CharactersShower>();
        CurrentCharacterTweeners = DOTween.Sequence();
        GameConfig characterConfig = new GameConfig(Utils.ResoucePaths.CharacterIniPath);
        //注意这样设计其实是有问题的，无法体现脚本中真正的顺序，不过先凑合吧
        foreach (string characterId in Character2Add.Keys)
        {
            var Character = CharacterInfo[characterId];
            string ResourePath = Path.Combine(Utils.ResoucePaths.PortraitPath,
                characterConfig.GetValue(Character.From, "ResourcesPath"),
                characterConfig.GetValue(Character.From, "Portrait-Normall"));
            img_CharactersShower.InitCharacterOnLoad(ResourePath, characterId, Character2Add[characterId]);
        }
        var img_Characters = _img_CharactersShower.characterImgs;
        foreach (string characterId in Charater2Show.Keys)
        {
            var img_Character = _img_CharactersShower.characterImgs[characterId].GetComponent<Img_Character_AnimationControler>();
            img_Character.HandleMessageOnLoad(Charater2Show[characterId]);
        }
        //角色立绘的切换按道理也应该是动画，但是先简单用直接变凑合一下
        foreach (string characterId in CharacterPortrait2Change.Keys)
        {
            var Character = CharacterInfo[characterId];
            string ResourePath = Path.Combine(Utils.ResoucePaths.PortraitPath,
                characterConfig.GetValue(Character.From, "ResourcesPath"),
                characterConfig.GetValue(Character.From, CharacterPortrait2Change[characterId]));
            img_CharactersShower.SetPortrait(ResourePath, characterId);
        }
        CurrentCharacterTweeners.onComplete += (() =>
        {
            currentData.currentcCharactersInfo.OnCharacterRemove();
            currentData.currentcCharactersInfo.Reset();
        });
    }
    public void CharacterAnimation_OnPlay()
    {
        //分别执行加载角色,角色动画,角色退场
        IsCharacterTweenerFinish = false;
        var currentData = GameMain.instance.gameData.currentData;
        var CharacterInfo = currentData.currentcCharactersInfo.charactersNodeinfo;
        var Character2Add = currentData.currentcCharactersInfo.CharacterAnimation2add_OnPlay;
        var Charater2Show = currentData.currentcCharactersInfo.CharacterAnimation2show_OnPlay;
        var Charater2Quit = currentData.currentcCharactersInfo.CharacterAnimation2delete_OnPlay;
        var CharacterPortrait2Change = currentData.currentcCharactersInfo.CharacterPortrait2Change_OnPlay;
        var _img_CharactersShower = img_CharactersShower.GetComponent<Img_CharactersShower>();
        CurrentCharacterTweeners = DOTween.Sequence();
        GameConfig characterConfig = new GameConfig(Utils.ResoucePaths.CharacterIniPath);
        //注意这样设计其实是有问题的，无法体现脚本中真正的顺序，不过先凑合吧
        foreach (string characterId in Character2Add.Keys)
        {
            var Character = CharacterInfo[characterId];
            string ResourePath =  Path.Combine(Utils.ResoucePaths.PortraitPath, 
                characterConfig.GetValue(Character.From, "ResourcesPath"), 
                characterConfig.GetValue(Character.From, "Portrait-Normall"));
            CurrentCharacterTweeners.Append
            (img_CharactersShower.InitCharacter(ResourePath, characterId, Character2Add[characterId]));
        }
        var img_Characters = _img_CharactersShower.characterImgs;
        foreach (string characterId in Charater2Show.Keys)
        {
            var img_Character = _img_CharactersShower.characterImgs[characterId].GetComponent<Img_Character_AnimationControler>();
            CurrentCharacterTweeners.Append(img_Character.HandleMessage(Charater2Show[characterId]));
        }
        foreach (string characterId in Charater2Quit.Keys)
        {
            var img_Character = _img_CharactersShower.characterImgs[characterId].GetComponent<Img_Character_AnimationControler>();
            CurrentCharacterTweeners.Append(img_Character.HandleMessage(Charater2Quit[characterId]));
        }
        //角色立绘的切换按道理也应该是动画，但是先简单用直接变凑合一下
        foreach(string characterId in CharacterPortrait2Change.Keys)
        {
            var Character = CharacterInfo[characterId];
            string ResourePath = Path.Combine(Utils.ResoucePaths.PortraitPath,
                characterConfig.GetValue(Character.From, "ResourcesPath"),
                characterConfig.GetValue(Character.From, CharacterPortrait2Change[characterId]));
            img_CharactersShower.SetPortrait(ResourePath, characterId);
        }
        CurrentCharacterTweeners.onComplete += (() => 
        { currentData.currentcCharactersInfo.OnCharacterRemove();
            currentData.currentcCharactersInfo.Reset();
            IsCharacterTweenerFinish = true;
        });
    }
    //选项被点击后执行
    public void OnChoiceSelected(string SelectionID)
    {
        var currentChoiceData = GameMain.instance.gameData.currentData.currentChoiceData;
        GameMain.instance.gameData.PlotData.IsBranch = true;
        GameMain.instance.gameData.PlotData.NowJumpID = SelectionID;
        loggerControler.LogChoiceContent(
            currentChoiceData.ChoiceInfo[SelectionID].ChoiceContentType,
            currentChoiceData.ChoiceInfo[SelectionID].ChoiceContent);
        IsOnChoice = false;
        ButtonClickNext();
        currentChoiceData.ResetChoiceInfo();
        GameMain.instance.gameData.PlotData.BranchPlotInfo.Dequeue();
        if(GameMain.instance.gameData.PlotData.BranchPlotInfo.Count == 0)
        {
            GameMain.instance.gameData.PlotData.IsBranch = false;
        };
    }
/*    public void Load(string ScriptName, string ScriptId)
    {
        var gameData = GameMain.instance.gameData;
        gameData.SetCurrentInfo(ScriptId, ScriptName);
        //这里注意改
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }*/
/*    public void Load()
    {
        //这里注意改
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }*/
    public void Skip()
    {
        //快速跳过剧情，实现方法为模拟按键快速点击
        //具体在按键的Update方法中调用，但是还是另外定义一个方法出来
        ButtonClickNext();
    }
    public void SkipReadedContent()
    {
        string ScriptName = GameMain.instance.gameData.currentData.currentScriptName;
        string ScriptID = GameMain.instance.gameData.currentData.currentID;
        if(GameMain.instance.gameSL.saveDatas.ReadedId.ContainsKey(ScriptName)&&
            GameMain.instance.gameSL.saveDatas.ReadedId[ScriptName].Contains(ScriptID))
        {
            ButtonClickNext();
        }
    }
    public void Auto()
    {
        if (IsCharacterTweenerFinish && IsTextTweenerFinish&&soundsControler.IsVoiceFinish == 0)
        {
            Count2Auto += Time.deltaTime;
            if (Count2Auto > WaitTime2Auto)
            {
                ButtonClickNext();
                Count2Auto = 0;
            }
        }
    }
    public void LoadNextScript(string ScriptName)
    {
        GameMain.ScriptName = ScriptName;
        GameMain.ScriptId = "0";
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    public void ReturnStartMenu()
    {
        GameMain.ScenceId = 0;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
