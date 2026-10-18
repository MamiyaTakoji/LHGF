using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LHGFData
{
    public class CharacterInfoLayer : ILayer
    {
        public CharacterPanelControler CharacterInfoPanelControler;
        public Dictionary<string, CharacterInfoData> CharacterInfoDataList = new() { };
        public Dictionary<string, Dictionary<string, List<string>>> AllCharacterDescribe;
        public string Spilter = "%";
        public List<Dictionary<string, CharacterInfoData>> HistoryCharacterInfoDataList = new() { };
        public class CharacterInfoData
        {
            public string CharacterName;
            public string CharacterImagePath;
            public string CharacterDescription;
            public CharacterInfoData(string CharacterName, string CharacterImagePath, string CharacterDescription)
            {
               this.CharacterName = CharacterName;
               this.CharacterImagePath = CharacterImagePath;
               this.CharacterDescription = CharacterDescription;
            }
        }
        public void LoadAllCharacterInfo()
        {
            string path = Utils.ResoucePaths.EvidenceContentPath;
            string FileName = "CharacterDescribeInfo.json";
            path = Path.Combine(path, FileName);
            AllCharacterDescribe = new Utils.GameConfig<Dictionary<string, List<string>>>(path).dataDict;
        }
        public void BeforeNextOnLoadStart()
        {
            
        }

        public void BeforeNextStart()
        {
           
        }

        public bool Finish()
        {
            return true;
        }

        public GameObject GetControler()
        {
            return CharacterInfoPanelControler.gameObject;
        }

        public string LayerName()
        {
            return "CharacterInfoLayerControler";
        }
        public class SaveData
        {
            public Dictionary<string, CharacterInfoData> CharacterInfoDataList = new() { };
            public List<Dictionary<string, CharacterInfoData>> HistoryCharacterInfoDataList = new() { };
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
            // Clear current contents
            CharacterInfoDataList.Clear();
            HistoryCharacterInfoDataList.Clear();
            // Restore data
            if (data.CharacterInfoDataList != null)
            {
                foreach (var kv in data.CharacterInfoDataList)
                {
                    CharacterInfoDataList[kv.Key] = kv.Value;
                }
            }
            if (data.HistoryCharacterInfoDataList != null)
            {
                foreach (var inner in data.HistoryCharacterInfoDataList)
                {
                    HistoryCharacterInfoDataList.Add(new Dictionary<string, CharacterInfoData>(inner));
                }
            }
        }

        public Dictionary<string, string> Log(LayerCommand command)
        {
            return null;
        }

        public void Next(LayerCommand command)
        {
            var config = command.CommandConfig;
            string CharacterID = config["CharacterID"];
            var CharacterInfo = AllCharacterDescribe[CharacterID];
            string CharacterInfoId = config["Info"];
            var CID = CharacterInfoId.Split(Spilter).ToList();
            //–’√˚%Õº∆¨%√Ë ˆ

            CharacterInfoDataList[CharacterID]=new CharacterInfoData(
                    CharacterInfo["Name"][int.Parse(CID[0])],
                    Path.Combine(Utils.ResoucePaths.FullBodyImagePath,CharacterInfo["Portrait"][int.Parse(CID[1])]),
                    CharacterInfo["Describe"][int.Parse(CID[2])]
                );
            HistoryCharacterInfoDataList.Add(Utils.DeepClone(CharacterInfoDataList));
        }

        public void OnLoadFinish()
        {

        }

        public void OnStart()
        {
            LoadAllCharacterInfo();
        }

        public void OnUpdate(){}

        public void Reset()
        {
            CharacterInfoDataList.Clear();
        }

        public object Save()
        {
            var saveData = new SaveData();
            // Deep-copy current character info and history snapshots to avoid sharing references with runtime data
            if (CharacterInfoDataList != null)
            {
                saveData.CharacterInfoDataList = new Dictionary<string, CharacterInfoData>();
                foreach (var kv in CharacterInfoDataList)
                {
                    saveData.CharacterInfoDataList[kv.Key] = kv.Value;
                }
            }
            if (HistoryCharacterInfoDataList != null)
            {
                saveData.HistoryCharacterInfoDataList = new List<Dictionary<string, CharacterInfoData>>();
                foreach (var inner in HistoryCharacterInfoDataList)
                {
                    saveData.HistoryCharacterInfoDataList.Add(new Dictionary<string, CharacterInfoData>(inner));
                }
            }
            return saveData;
        }

        public void Skip(){}

        public bool Withdraw()
        {
            if(HistoryCharacterInfoDataList.Count == 0)
            {
                return false;
            }
            else if(HistoryCharacterInfoDataList.Count == 1)
            {
                CharacterInfoDataList.Clear();
            }
            else
            {
                HistoryCharacterInfoDataList.RemoveAt(HistoryCharacterInfoDataList.Count - 1);
                CharacterInfoDataList = HistoryCharacterInfoDataList[HistoryCharacterInfoDataList.Count - 1];
            }
            return true;
        }
    }
}
