using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    // Start is called before the first frame update
    public GameProgress gameProgress;
    public GameData gameData;
    public GameSL gameSL;
    public GameSetting gameSetting;
    public static GameMain instance;
    public static string ScriptName = "FirstScript";
    public static string ScriptId = "0";
    public static int ScenceId = 0;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        gameProgress = GetComponent<GameProgress>();
        gameData = GetComponent<GameData>();
        gameSL = GetComponent<GameSL>();
        gameSetting = GetComponent<GameSetting>();

        gameSL.LoadGameData();
        if(ScenceId == 1)
        {
            gameProgress.OnGameStart();
        }
        gameSetting.SetBaseSetting(gameSL.saveDatas.baseSetting);
        //gameProgress.img_CharactersShower.InitCharacter("D:\\Unity\\LHGF\\Assets\\StreamingAssets\\HGF\\Texture2D\\Portrait\\Maya Takoji\\1.png", "0");
    }

    // Update is called once per frame
    void Update()
    {
        gameProgress.OnGaneProgressUpdata(Time.deltaTime);
    }
    public void SetScenceId(int scenceId)
    {
        ScenceId = scenceId;
    }
}
