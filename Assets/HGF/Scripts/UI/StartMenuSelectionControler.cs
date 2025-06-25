using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuSelectionControler : MonoBehaviour
{
    // Start is called before the first frame update
    public Button StartButton;
    public Button LoadButton;
    public Button ContinueButton;
    public Button ConfigButton;
    public Button QuitButton;
    public SLPanelControler sLPanelControler;
    public SettingControler settingControler;
    void Start()
    {
        StartButton.onClick.AddListener
            (
            delegate { StartButtonControler(); }
            );
        LoadButton.onClick.AddListener
            (
            delegate { LoadButtonControler(); }
            );
        ContinueButton.onClick.AddListener
            (
            delegate { ContinueButtonControler(); }
            );
        ConfigButton.onClick.AddListener
            (
            delegate { ConfigButtonControler(); }
            );
        QuitButton.onClick.AddListener
            (
            delegate { Application.Quit(); }
            );
    }       

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartButtonControler()
    {
        GameMain.ScriptId = "0";
        GameMain.ScriptName = "FirstScript";
        GameMain.ScenceId = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    public void LoadButtonControler()
    {
        sLPanelControler.gameObject.SetActive( true );
        sLPanelControler.ShowCurrentPage();
        GameMain.ScenceId = 1;
        sLPanelControler.CurrentMode = "Load";

    }
    public void ContinueButtonControler() 
    {
        sLPanelControler.GetCurrentData();
        var newestData = sLPanelControler.newestSaveData;
        GameMain.ScenceId = 1;
        GameMain.instance.gameSL.LoadGameData(sLPanelControler.newestSaveDataId);
    }
    public void ConfigButtonControler()
    {
        settingControler.gameObject.SetActive ( true );
        settingControler.SetPanel();
    }

}
