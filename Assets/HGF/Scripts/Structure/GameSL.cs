using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using DG.Tweening.Plugins.Core.PathCore;
using System;
using static GameData;
using UnityEngine.SceneManagement;

public class GameSL:MonoBehaviour
{
    public string ScriptId = "0";
    public string ScriptName = "FirstScript";
    public SaveDatas saveDatas = new SaveDatas();

    public string TextureToBase64(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        return System.Convert.ToBase64String(bytes);
    }
    [System.Serializable]
    public class SaveDatas
    {
        public Dictionary<int, SaveData> saveDatas = new Dictionary<int, SaveData>();
        public GameBaseSetting baseSetting = new GameBaseSetting();
        //记录已经读过的部分
        public Dictionary<string, List<string>> ReadedId = new Dictionary<string, List<string>>();
        public void UpdataReadedId(string scriptName, string scriptId)
        {
            if (ReadedId.ContainsKey(scriptName))
            {
                if (!ReadedId[scriptName].Contains(scriptId))
                {
                    ReadedId[scriptName].Add(scriptId);
                }   
            }
            else
            {
                ReadedId[scriptName] = new List<string>(){scriptId};
            }
        }
    }

    [System.Serializable]
    public class GameBaseSetting
    {
        public float BGMLoudness = 0.7f;
        public float VoiceLoudness = 1f;
        public float EffectLoudness = 0.9f;
        public float AutoSpeed = 1f;
        public GameBaseSetting(float bGMLoudness, float voiceLoudness, float effectLoudness, float autoSpeed)
        {
            BGMLoudness = bGMLoudness;
            VoiceLoudness = voiceLoudness;
            EffectLoudness = effectLoudness;
            AutoSpeed = autoSpeed;
        }
        public GameBaseSetting() 
        {
            BGMLoudness = 0.7f;
            VoiceLoudness = 1f;
            EffectLoudness = 0.9f;
            AutoSpeed = 1f;
    }
    }

    [System.Serializable]
    public class SaveData
    {
        public string ScriptId;
        public string ScriptName;
        public string SaveTime;
        public string Abstract;
        public string TextureBase64;
        public string AbstractTextType;

        [JsonIgnore]
        public Texture2D Texture { get { return Base64ToTexture(TextureBase64); }  }
        public string TextureToBase64(Texture2D texture)
        {
            byte[] bytes = texture.EncodeToPNG();
            return System.Convert.ToBase64String(bytes);
        }
        public Texture2D Base64ToTexture(string TextureBase64)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(System.Convert.FromBase64String(TextureBase64));
            return texture;
        }
        public SaveData(CurrentData currentData, Texture2D texture2D)
        {
            ScriptId = currentData.currentID;
            ScriptName =  currentData.currentScriptName;
            DateTime now = DateTime.Now;
            string timeString = now.ToString("yyyy-MM-dd HH:mm:ss");
            SaveTime = timeString;
            if (currentData.currentDialogType == "Speak")
            {
                Abstract = currentData.currentSpeakData.currentSpeakContent;
                AbstractTextType = currentData.currentSpeakData.currentSpeakTextType;
            }
            else if (currentData.currentDialogType == "LongSpeak")
            {
                Abstract = currentData.currentLongSpeakData.currentLongSpeakContent;
                AbstractTextType = currentData.currentLongSpeakData.currentLongSpeakTextType;
            }
            else
            {
                Abstract = "";
            }
            TextureBase64 = TextureToBase64(texture2D);
        }
        public SaveData()
        {
            ScriptId = null;
            ScriptName = null;
            SaveTime = string.Empty;
            Abstract = string.Empty;
            AbstractTextType = "TexDraw";
            //白色对应的Base64
            TextureBase64 = 
            "iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5+AAAAFUlEQVQIHWP8DwQMSIAJiQ1mEhYAAAZdBAQjjGwcAAAAAElFTkSuQmCC";
        }

    }
    public void SaveGameData()
    {
        string json = JsonConvert.SerializeObject(saveDatas, Formatting.Indented);
        string path = System.IO.Path.Combine(Application.persistentDataPath, "savegame.sav");
        File.WriteAllText(path, json);
        Debug.Log("存档成功，路径：" + path);
    }
    public void LoadGameData()
    {
        var path = Utils.ResoucePaths.SavePath;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            // 反序列化
            saveDatas = JsonConvert.DeserializeObject<SaveDatas>(json);
/*            if (saveDatas.baseSetting != null)
            {
                GameMain.instance.gameSetting.SetBaseSetting(saveDatas.baseSetting);
            }*/
        }
        else
        {
            Debug.Log("无存档文件");
        }
    }
    public void LoadGameData(int id) 
    {
        var saveData = saveDatas.saveDatas[id];
        string scriptName = saveData.ScriptName;
        string scriptId = saveData.ScriptId;
        GameMain.ScriptId = scriptId; GameMain.ScriptName = scriptName;
        SceneManager.LoadScene(1);
    }
    public void SaveGameData(int id, Texture2D texture2D)
    {
        CurrentData currentData = GameMain.instance.gameData.currentData;
        SaveData saveData = new(currentData,texture2D);
        saveDatas.saveDatas[id] = saveData;
        SaveGameData();
    }
}
