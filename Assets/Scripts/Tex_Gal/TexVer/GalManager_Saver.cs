using DG.Tweening.Plugins.Core.PathCore;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class GalManager_Saver : MonoBehaviour
{
    // Start is called before the first frame update
    public SaveDatas saveDatas;
    public _SaveDatas _saveDatas;
    public GameObject img_character;
    void Start()
    {
        //LoadData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ResetCharacterImg()
    {
        List<GameObject> gs = new List<GameObject>();
        foreach (Transform g in img_character.transform)
        {
            gs.Add(g.gameObject);
        }
        for(int i = 0; i < gs.Count; i++)
        {
            Destroy(gs[i]);
        }
    }
    public void Save(int SaveId, SaveData saveData)
    {
        saveDatas.datas[SaveId] = saveData;
        SaveData();
    }
    public void SaveData()
    {
        _saveDatas = saveDatas2_saveDatas(saveDatas);
        string json = JsonConvert.SerializeObject(_saveDatas, Formatting.Indented);
        string path = System.IO.Path.Combine(Application.persistentDataPath, "savegame.sav");
        File.WriteAllText(path, json);
        Debug.Log("存档成功，路径：" + path);
    }
    private string TextureToBase64(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        return System.Convert.ToBase64String(bytes);
    }
    public Texture2D GetTexture(string TextureBase64)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(System.Convert.FromBase64String(TextureBase64));
        return texture;
    }
    public _SaveData saveData2_saveData(SaveData sd)
    {
        _SaveData _sd = new _SaveData();
        _sd.Data = sd.Data;
        _sd.SaveInfoText = sd.SaveInfoText;
        _sd.SaveInfoType = sd.SaveInfoType;
        _sd.ScriptName = sd.ScriptName;
        _sd.Id = sd.Id;
        string TextureInfo = TextureToBase64(sd.texture);
        _sd.TextureBase64 = TextureInfo;
        return _sd;
    }
    public _SaveDatas saveDatas2_saveDatas(SaveDatas sds)
    {
        _SaveDatas _sds = new _SaveDatas();
        foreach(var Key in sds.datas.Keys)
        {
            _sds.datas[Key] = saveData2_saveData(sds.datas[Key]);
        }
        _sds.readState = sds.readState;
        _sds.baseSetting = sds.baseSetting;
        return _sds;
    }
    public SaveData _saveData2saveData(_SaveData _sd)
    {
        SaveData sd = new SaveData();
        sd.Data = _sd.Data;
        sd.SaveInfoText = _sd.SaveInfoText;
        sd.SaveInfoType = _sd.SaveInfoType;
        sd.ScriptName = _sd.ScriptName;
        sd.Id = _sd.Id;
        sd.texture = GetTexture(_sd.TextureBase64);
        return sd;
    }
    public SaveDatas _saveDatas2saveDatas(_SaveDatas _sds)
    {
        SaveDatas sds = new SaveDatas();
        foreach (var Key in _sds.datas.Keys)
        {
            sds.datas[Key] = _saveData2saveData(_sds.datas[Key]);
        }
        sds.readState = _sds.readState;
        sds.baseSetting = _sds.baseSetting;
        return sds;
    }
    public void Load(string scriptId,string scriptName)
    {
        GameMain.gameScriptId2Load = scriptId;
        GameMain.gameScriptName2Load = scriptName;
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    public void LoadData()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "savegame.sav");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            // 反序列化
            _saveDatas = JsonConvert.DeserializeObject<_SaveDatas>(json);
            saveDatas = _saveDatas2saveDatas(_saveDatas);
            if (saveDatas.baseSetting != null)
            {
                GameMain.instance.gameSetting.SetBaseSetting(saveDatas.baseSetting);
            }
            
        }
        else
        {
            Debug.Log("无存档文件");
        }
    }
}
