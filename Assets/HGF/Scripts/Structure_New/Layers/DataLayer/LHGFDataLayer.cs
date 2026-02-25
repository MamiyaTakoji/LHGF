using LHGFData;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DataLayer.LHGF_MoreGameData;
public class DataLayer : ILayer
{
    public string layerName = "DataLayer";
    public LHGF_MoreGameData characterData = new();
    //这个类用于保存额外需要的游戏数据
    public class LHGF_MoreGameData
    {
        public LHGF_MoreGameData() {}
        //目前，这里只保存CharacterInfo.ini和Department.ini的数据
        //以及通过AddCharacter加入的数据
        public Dictionary<string, Struct_CharacterInfo> CharacterID2CharacterInfo = new() { };
        public LHGFData.Utils.GameConfig CharacterInfo = new(LHGFData.Utils.ResoucePaths.CharacterJsonPath);
        public void AddCharacterInfo(string CharacterID, string CharacterFrom)
        {
            RemoveCharacterInfo(CharacterID);
            Struct_CharacterInfo newCharInfo = new(CharacterFrom, CharacterInfo.dataDict[CharacterFrom]);
            CharacterID2CharacterInfo.Add(CharacterID, newCharInfo);
        }
        public void RemoveCharacterInfo(string CharacterID)
        {
            if (CharacterID2CharacterInfo.ContainsKey(CharacterID))
            {
                CharacterID2CharacterInfo.Remove(CharacterID);
            }
        }
        public void ResetCharacterInfo()
        {
            CharacterID2CharacterInfo = new() { };
        }
        public Struct_CharacterInfo GetCharacterInfo(string CharacterID)
        {
            return CharacterID2CharacterInfo[CharacterID];
        }
        public class Struct_CharacterInfo
        {
            public Struct_CharacterInfo() { }
            public Struct_CharacterInfo(string _From, Dictionary<string, string> CharacterInfo)
            {
                From = _From;
                Affiliation = CharacterInfo.ContainsKey("Department") ? CharacterInfo["Department"] : "";
                Name = CharacterInfo.ContainsKey("Name") ? CharacterInfo["Name"] : "";
            }
            public string Name;
            public string Affiliation;
            public string From;
        }
    }
    public bool Finish()
    {
        return true;
    }

    public GameObject GetControler()
    {
        return null;
    }

    public string LayerName()
    {
        return layerName;
    }

    public void Log()
    {
        
    }

    public void Next(LayerCommand command)
    {
        string CharacterID = command.CommandConfig["CharacterID"];
        string CharacterFrom = command.CommandConfig["From"];
        characterData.AddCharacterInfo(CharacterID, CharacterFrom);
    }

    public void Next_OnLoad(LayerCommand command)
    {
        string CharacterID = command.CommandConfig["CharacterID"];
        string CharacterFrom = command.CommandConfig["From"];
        characterData.AddCharacterInfo(CharacterID, CharacterFrom);
    }

    public void OnStart()
    {
        
    }

    public void OnUpdate()
    {
        
    }

    public void Skip()
    {
        
    }

    public bool Withdraw()
    {
        return true;
    }

    public void OnLoadFinish()
    {

    }

    public void BeforeNextStart()
    {

    }

    public Dictionary<string, string> Log(LayerCommand command)
    {
        return null;
    }

    public void Reset()
    {
        characterData.ResetCharacterInfo();
    }

    public void BeforeNextOnLoadStart(){}

    public class SaveData
    {
        public Dictionary<string, Struct_CharacterInfo> CharacterID2CharacterInfo = new();
    }
    public void Load(object saveData)
    {
        if (saveData == null) return;
        SaveData data;
        data = saveData as SaveData;
        if (data == null)
        {
            string json = JsonConvert.SerializeObject(saveData);
            data = JsonConvert.DeserializeObject<SaveData>(json);
        }

        // 清空当前数据
        characterData.CharacterID2CharacterInfo.Clear();

        // 恢复数据
        if (data.CharacterID2CharacterInfo != null)
        {
            foreach (var kvp in data.CharacterID2CharacterInfo)
            {
                characterData.CharacterID2CharacterInfo[kvp.Key] = kvp.Value;
            }
        }
    }

    public object Save()
    {
        var saveData = new SaveData();
        // 复制字典，避免与原对象共享引用
        foreach (var kvp in characterData.CharacterID2CharacterInfo)
        {
            saveData.CharacterID2CharacterInfo[kvp.Key] = kvp.Value; // Struct_CharacterInfo 是引用类型，但内容不可变（字符串），直接赋值没问题
        }
        return saveData;
    }
}
