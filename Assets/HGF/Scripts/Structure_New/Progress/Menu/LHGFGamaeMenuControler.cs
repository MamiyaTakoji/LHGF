using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public Button ShowEvidenceButton;
    public Button ShowCharacterInfoButton;

    public LHGFSLPanelControler SLPanel;
    private List<GameObject> panels = new() { };
    public LHGFMainScenceControler mainScence;
    public GameObject StartMenu;
    public MenuState menuState;
    public LHGFGameConfigControler ConfigPanel;
    public EvidencePanelControler EvidencePanelControler;
    public CharacterPanelControler CharacterPanelControler;
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
        panels.Add(EvidencePanelControler.gameObject);
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
        ShowEvidenceButton.onClick.AddListener(delegate
        {
            ShowEvidenceButtonControler();
        });
        ShowCharacterInfoButton.onClick.AddListener(delegate
        {
            ShowCharacterInfoButtonControler();
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (menuState== MenuState.StartMenu)
        {
            //设置返回菜单按钮无法被点击
            ReturnToStartMenuButton.interactable = false;
            ReturnToStartMenuButton.GetComponentInChildren<TMP_Text>().color = Color.gray;
            //设置保存按钮无法被点击
            SaveButton.interactable = false;
            SaveButton.GetComponentInChildren<TMP_Text>().color = Color.gray;
            ShowEvidenceButton.gameObject.SetActive(false);       
            ShowCharacterInfoButton.gameObject.SetActive(false);
        }
        else
        {
            //设置返回菜单按钮可以被点击
            ReturnToStartMenuButton.interactable = true;
            ReturnToStartMenuButton.GetComponentInChildren<TMP_Text>().color = Color.black;
            //设置保存按钮可以被点击
            SaveButton.interactable = true;
            SaveButton.GetComponentInChildren<TMP_Text>().color = Color.black;
            ShowEvidenceButton.gameObject.SetActive(true);        
            ShowCharacterInfoButton.gameObject.SetActive(true);
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
        var G = SetBlackMask();
        Sequence seq = DOTween.Sequence();
        float BgmVolume = LHGFGameMain.instance.gameConfigDataManager.data.BGMVolume;
        //把背景音乐的音量调到0
        ConfigPanel.BGMAudioSource.volume = 0;
        seq.Append(G.GetComponent<Image>().DOFade(1f, 2f).OnComplete(() =>
        {
            LHGFGameMain.instance.OnGameStart();
            gameObject.SetActive(false);
            SetMainScenceState();
            LHGFGameMain.instance.MainScence.gameObject.SetActive(true);
            ConfigPanel.BGMAudioSource.volume = BgmVolume;
        }));
        seq.Append(G.GetComponent<Image>().DOFade(0f, 3f).OnComplete(() =>
        {
            Destroy(G);
        }));
        
    }
    /*    public void LoadGameOnGameMenu(string SaveId)
        {
            LHGFGameMain.instance.OnGameLoad(SaveId);
            gameObject.SetActive(false);
            SetMainScenceState();
            LHGFGameMain.instance.MainScence.gameObject.SetActive(true);
        }*/
    /*    public void LoadGameOnGameMenu(GameSaveData data)
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
        }*/
    public void LoadGameOnGameMenu(GameSaveData data)
    {
        var G = SetBlackMask();
        Sequence seq = DOTween.Sequence();
        float BgmVolume = LHGFGameMain.instance.gameConfigDataManager.data.BGMVolume;
        ConfigPanel.BGMAudioSource.volume = 0;
        seq.Append(G.GetComponent<Image>().DOFade(1f, 2f).OnComplete(() =>
        {
            LHGFGameMain.instance.ResetGame();
            // 卸载不再使用的资源并强制GC，防止读档时内存持续增长
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            LHGFGameMain.instance.gameProgress.Load(
                data.NodeId,
                LHGFGameMain.instance.gameData,
                data.layerDatas
                );
            gameObject.SetActive(false);
            SetMainScenceState();
            LHGFGameMain.instance.MainScence.gameObject.SetActive(true);
            ConfigPanel.BGMAudioSource.volume = BgmVolume;
            LHGFGameMain.instance.OnGameLoad();
        }));
        seq.Append(G.GetComponent<Image>().DOFade(0f, 3f).OnComplete(() =>
        {
            Destroy(G);
        }));
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
    /*    public IEnumerator ConfigButtonControler()
        {
            CloseAllPanel();
            ConfigPanel.gameObject.SetActive(true);
            // 暂停一帧
            yield return null;

            // 如果需要在一帧后执行其他逻辑，可以写在这里
            // 例如：ApplyGlobalSetting(); 
        }*/
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
        var G = SetBlackMask();
        Sequence seq = DOTween.Sequence();
        float BgmVolume = LHGFGameMain.instance.gameConfigDataManager.data.BGMVolume;
        ConfigPanel.BGMAudioSource.volume = 0;
        seq.Append(G.GetComponent<Image>().DOFade(1f, 2f).OnComplete(() =>
        {
            LHGFGameMain.instance.ResetGame();
            menuState = MenuState.StartMenu;
            ShowStartMenu();
            ConfigPanel.BGMAudioSource.volume = BgmVolume;
        }));
        seq.Append(G.GetComponent<Image>().DOFade(0f, 2f).OnComplete(() =>
        {
            Destroy(G);
        }));
        /*        LHGFGameMain.instance.ResetGame();
                menuState = MenuState.StartMenu;
                ShowStartMenu();*/
    }
    public void ApplyGlobalSetting()
    {
        ConfigPanel.BGMAudioSource.volume = LHGFGameMain.instance.gameConfigDataManager.data.BGMVolume;
        ConfigPanel.EffectSoundAudioSource.volume = LHGFGameMain.instance.gameConfigDataManager.data.EffectSoundVolme;
        ConfigPanel.CharacterVoiceAudioSource.volume = LHGFGameMain.instance.gameConfigDataManager.data.CVVolme;
    }
    public void ShowEvidenceButtonControler()
    {
        CloseAllPanel();
        EvidencePanelControler.CurrentMenuState = EvidencePanelControler.MenuState.Review;
        EvidencePanelControler.gameObject.SetActive(true);
    }
    public void ShowCharacterInfoButtonControler()
    {
        CloseAllPanel();
        CharacterPanelControler.gameObject.SetActive(true);
        CharacterPanelControler.SetButtons();
    }
    public GameObject SetBlackMask()
    {
        var canvas = GetComponentInParent<Canvas>();
        var overlayGo = new GameObject("BlackOverlay");
        overlayGo.transform.SetParent(canvas.transform, false);

        var image = overlayGo.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0);   // 初始透明
        //image.raycastTarget = false;            // 不阻挡点击

        // 拉伸到全屏
        var rt = overlayGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return overlayGo;
    }
}
