using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SaveDatas
{
    public Dictionary<int, SaveData> datas= new Dictionary<int, SaveData>{};
    //readState用来记录某一页是否被读到过
    public Dictionary<string, List<string>> readState = new Dictionary<string, List<string>>() { };
    public BaseSetting baseSetting = null;
    public SaveDatas() { }
    public void SetReadState(string Id, string ScriptName)
    {
        //记录阅读状态
        if (readState.ContainsKey(ScriptName))
        {
            if (!readState[ScriptName].Contains(Id))
            {
                readState[ScriptName].Add(Id);
            }
        }
        else
        {
            readState[ScriptName] = new List<string>{Id};
        }
    }
}
[System.Serializable]
public class BaseSetting
{
    public float BGMLoudness;
    public float VoiceLoudness;
    public float EffectLoudness;
    public float AutoSpeed;
    public BaseSetting(float _BGMLoudness, float _VoiceLoudness, float _EffectLoudness, float _AutoSpeed)
    {
        BGMLoudness = _BGMLoudness;
        VoiceLoudness = _VoiceLoudness;
        EffectLoudness = _EffectLoudness;
        AutoSpeed = _AutoSpeed;
    }
}
public class _SaveDatas
{
    public Dictionary<int, _SaveData> datas = new Dictionary<int, _SaveData> { };
    public Dictionary<string, List<string>> readState = new Dictionary<string, List<string>>();
    public BaseSetting baseSetting = null;
    public _SaveDatas() { 
        datas = new Dictionary<int, _SaveData> { };
        readState = new Dictionary<string, List<string>>() { };
    }
}
[System.Serializable]
public class SaveData
{
    public Texture2D texture;
    public string Data;
    public string SaveInfoText;
    public string SaveInfoType;
    public string ScriptName;//剧本的名字
    public string Id;//对应剧本的Id
    public SaveData(Texture2D texture, int saveId, 
        string data, string saveInfoText, 
        string saveInfoType,
        string scriptName, string id)
    {
        this.texture = texture;
        Data = data;
        SaveInfoText = saveInfoText;
        SaveInfoType = saveInfoType;
        ScriptName = scriptName;
        Id = id;
    }
    public SaveData() { }
}
public class _SaveData
{
    public string TextureBase64;
    public string Data;
    public string SaveInfoText;
    public string SaveInfoType;
    public string ScriptName;//剧本的名字
    public string Id;//对应剧本的Id
    public _SaveData(string texture,
        string data, string saveInfoText,
        string saveInfoType,
        string scriptName, string id)
    {
        this.TextureBase64 = texture;
        Data = data;
        SaveInfoText = saveInfoText;
        SaveInfoType = saveInfoType;
        ScriptName = scriptName;
        Id = id;
    }
    public _SaveData() { }
}




