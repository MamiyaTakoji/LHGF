using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using static ScenesScripts.GalPlot.GalManager;

public class GameMain : MonoBehaviour
{
    // Start is called before the first frame update
    public static GameMain instance;
    public MyGalManager gal;
    public GalManager_Saver galManager_Saver;
    public GameSetting gameSetting;
    public static string gameScriptId2Load = "0";
    public static string gameScriptName2Load = "FirstScript";
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        gal = GetComponent<MyGalManager>();
        galManager_Saver = GetComponent<GalManager_Saver>(); 
        gameSetting = GetComponent<GameSetting>();
        galManager_Saver.LoadData();
        if(gal != null)
        {
            gal.Load(gameScriptId2Load, gameScriptName2Load);
        }
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
}
