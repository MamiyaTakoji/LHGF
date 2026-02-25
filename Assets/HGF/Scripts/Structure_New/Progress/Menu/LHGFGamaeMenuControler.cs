using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static LHGFData.GameSaveDataManager;
public class LHGFGamaeMenuControler : MonoBehaviour
{
    public Button SaveButton;
    public Button LoadButton;
    public Button BackButton;
    public Button ExitButton;
    public Button ConfigButton;
    public Button ReturnToStartMenuButton;
    public LHGFSLPanelControler SLPanel;
    private List<GameObject> panels = new() { };
    public LHGFMainScenceControler mainScence;
    public GameObject StartMenu;
    public MenuState menuState;
    public LHGFGameConfigControler ConfigPanel;
    public enum MenuState
    {
        StartMenu,
        MainScence
    }

    public void SetMainScenceState()
    {
        menuState = MenuState.MainScence;
        StartMenu.SetActive(false);
    }
    void Start()
    {
        panels.Add(SLPanel.gameObject);
        panels.Add(ConfigPanel.gameObject);
        BackButton.onClick.AddListener(delegate
        {
            BackButtonControler();
        });
        SaveButton.onClick.AddListener(delegate
        {
            ShowSavePanel();
        });
        LoadButton.onClick.AddListener(delegate
        {
            ShowLoadPanel();
        });
        ExitButton.onClick.AddListener(delegate
        {
            ExitGame();
        });
        ConfigButton.onClick.AddListener(delegate
        {
            ConfigButtonControler();
        });
        ReturnToStartMenuButton.onClick.AddListener(delegate
        {
            ReturnToStartMenu();
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (menuState== MenuState.StartMenu)
        {
            ReturnToStartMenuButton.interactable = false;
        }
        else
        {
            ReturnToStartMenuButton.interactable = true;
        }
    }
    public void SetStartMenuActive()
    {
        this.gameObject.SetActive(true);
        StartMenu.SetActive(true);
    }
    public void ShowSavePanel()
    {
        CloseAllPanel();
        SLPanel.gameObject.SetActive(true);
        SLPanel.SetSLState(LHGFSLPanelControler.SLPanelState.Save);
    }
    public void ShowLoadPanel()
    {
        CloseAllPanel();
        SLPanel.gameObject.SetActive(true);
        SLPanel.SetSLState(LHGFSLPanelControler.SLPanelState.Load);
    }
    public void ShowStartMenu()
    {
        if (StartMenu.activeSelf == false)
        {
            StartMenu.SetActive(true);
        }
    }
    public void CloseAllPanel()
    {
        foreach(GameObject g in panels)
        {
            g.SetActive(false);
        }
    }
    public void StartGameOnGameMenu()
    {
        LHGFGameMain.instance.OnGameStart();
        gameObject.SetActive(false);
        SetMainScenceState();
        LHGFGameMain.instance.MainScence.gameObject.SetActive(true);
    }
/*    public void LoadGameOnGameMenu(string SaveId)
    {
        LHGFGameMain.instance.OnGameLoad(SaveId);
        gameObject.SetActive(false);
        SetMainScenceState();
        LHGFGameMain.instance.MainScence.gameObject.SetActive(true);
    }*/
    public void LoadGameOnGameMenu(GameSaveData data)
    {
        LHGFGameMain.instance.ResetGame();
        LHGFGameMain.instance.gameProgress.Load(
            data.NodeId,
            LHGFGameMain.instance.gameData,
            data.layerDatas
            );
        gameObject.SetActive(false);
        SetMainScenceState();
        LHGFGameMain.instance.MainScence.gameObject.SetActive(true);
    }
    public void BackButtonControler()
    {
        if(menuState == MenuState.StartMenu)
        {
            ShowStartMenu();
        }
        else
        {
            gameObject.SetActive(false);
            mainScence.gameObject.SetActive(true);
        }
    }
    public void ConfigButtonControler()
    {
        CloseAllPanel();
        ConfigPanel.gameObject.SetActive(true);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void ReturnToStartMenu()
    {
        LHGFGameMain.instance.ResetGame();
        menuState = MenuState.StartMenu;
        ShowStartMenu();
    }
    public void ApplyGlobalSetting()
    {
        ConfigPanel.BGMAudioSource.volume = LHGFGameMain.instance.gameConfigDataManager.data.BGMVolume;
        ConfigPanel.EffectSoundAudioSource.volume = LHGFGameMain.instance.gameConfigDataManager.data.EffectSoundVolme;
        ConfigPanel.CharacterVoiceAudioSource.volume = LHGFGameMain.instance.gameConfigDataManager.data.CVVolme;
    }
}
