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
        GameMain.gameScriptId2Load = "0";
        GameMain.gameScriptName2Load = "FirstScript";
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    public void LoadButtonControler()
    {
        sLPanelControler.gameObject.SetActive( true );
        sLPanelControler.ShowCurrentPage();
    }
    public void ContinueButtonControler() 
    {
        sLPanelControler.GetCurrentData();
        var newestData = sLPanelControler.newestSaveData;
        GameMain.instance.galManager_Saver.Load(newestData.Id, newestData.ScriptName);
    }
    public void ConfigButtonControler()
    {
        settingControler.gameObject.SetActive ( true );
        settingControler.SetPanel();
    }

}
