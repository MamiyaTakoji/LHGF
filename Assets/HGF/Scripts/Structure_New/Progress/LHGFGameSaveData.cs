using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LHGFData 
{
    public class UnityContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            // 忽略 Vector2.normalized
            if (member.DeclaringType == typeof(Vector2) && member.Name == "normalized")
            {
                property.Ignored = true;
            }
            // 忽略 Vector3.normalized 和 Vector3.magnitude 等可能导致递归的属性
            else if (member.DeclaringType == typeof(Vector3) && (member.Name == "normalized" || member.Name == "magnitude" || member.Name == "sqrMagnitude"))
            {
                property.Ignored = true;
            }
            // 忽略 Color.linear 和 Color.gamma
            else if (member.DeclaringType == typeof(Color) && (member.Name == "linear" || member.Name == "gamma"))
            {
                property.Ignored = true;
            }
            // 忽略 Quaternion.eulerAngles（它返回 Vector3，但本身也可能递归？不过 eulerAngles 是属性，返回 Vector3，可能安全，但如果你遇到问题可以加）
            // else if (member.DeclaringType == typeof(Quaternion) && member.Name == "eulerAngles")
            // {
            //     property.Ignored = true;
            // }

            return property;
        }
    }
    public class GameSaveDataManager
    {
        //和之前的设计不同，这里只保存存档数据
        //音量等其他设置放在其他地方
        public GameSaveDataManager() { _ = gameSaveDatas; }
        public Dictionary<int, GameSaveData> _gameSaveDatas;
        public Dictionary<int, GameSaveData> gameSaveDatas
        {
            get
            {
                if (_gameSaveDatas == null)
                {
                    _gameSaveDatas = new Dictionary<int, GameSaveData>();
                    GetGameSaveDatas();
                }
                return _gameSaveDatas;
            }
            set
            {
                _gameSaveDatas = value;
            }
        }
/*        private static GameSaveDataManager _instance;
        public static GameSaveDataManager instance 
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new();
                    _instance.init();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public void init()
        {
            GetGameSaveDatas();
        }*/
        [System.Serializable]
        public class GameSaveData
        {
            public string NodeId;
            public string SaveAbstract;
            public string SaveAbstractTextType;
            public string SaveTime;
            public byte[] image;
            public Dictionary<string, object> layerDatas;
        }
        public void Save(int saveDataId, GameSaveData gameSaveData)
        {
            gameSaveDatas[saveDataId] = gameSaveData;
            WriteSaveDataToSavePath(gameSaveData, saveDataId.ToString());
        }
        public void Load(int saveDataId)
        {
            //这里最后实现吧
            //貌似不需要实现了
        }
        //根据json文件读取数据
        public static GameSaveData LoadSaveData(string saveDataPath)
        {
            if (File.Exists(saveDataPath))
            {
                string json = File.ReadAllText(saveDataPath);
                // 反序列化
                var saveData = JsonConvert.DeserializeObject<GameSaveData>(json);
                return saveData;
            }
            else
            {
                Debug.Log("无存档文件");
                return null;
            }
        }
        public void WriteSaveDataToSavePath(GameSaveData gameSaveData, string saveid)
        {
            string SavePath = Utils.ResoucePaths.SaveDataPath;
            string SaveName = $"save_{saveid}.sav";
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new UnityContractResolver()
            };
            string json = JsonConvert.SerializeObject(gameSaveData, Formatting.Indented,settings:settings);

            string path = Path.Combine(SavePath, SaveName);
            File.WriteAllText(path, json);
            Debug.Log("存档成功，路径：" + path);
        }
        public void GetGameSaveDatas()
        {
            gameSaveDatas.Clear(); // 清空现有数据

            string savePath = Utils.ResoucePaths.SaveDataPath;

            // 检查存档目录是否存在
            if (!Directory.Exists(savePath))
            {
                Debug.Log("存档目录不存在: " + savePath);
                return;
            }

            try
            {
                // 获取所有存档文件
                var saveFiles = Directory.GetFiles(savePath, "save_*.sav");

                Debug.Log($"找到 {saveFiles.Length} 个存档文件");

                foreach (var filePath in saveFiles)
                {
                    try
                    {
                        // 从文件名中提取存档ID
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        if (int.TryParse(fileName.Replace("save_", ""), out int saveId))
                        {
                            // 加载存档数据
                            GameSaveData saveData = LoadSaveData(filePath);
                            if (saveData != null)
                            {
                                gameSaveDatas[saveId] = saveData;
                                Debug.Log($"成功加载存档 ID: {saveId}, 文件: {fileName}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"无法从文件名解析存档ID: {fileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"加载存档文件时出错: {filePath}, 错误: {ex.Message}");
                    }
                }

                Debug.Log($"总共加载了 {gameSaveDatas.Count} 个存档");
            }
            catch (Exception ex)
            {
                Debug.LogError($"读取存档文件列表时出错: {ex.Message}");
            }
        }
    }
    public class GameConfigDataManager
    {
        private GameConfigData _data;

        public GameConfigData data
        {
            get
            {
                if (_data == null)
                {
                    init();
                }
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        public class GameConfigData
        {
            public float BGMVolume = 0.5f;
            public float EffectSoundVolme = 0.5f;
            public float CVVolme = 0.5f;
            public bool IsSkipUnreadContent = false;
        }

        private const string SETTING_DATA_FILE_NAME = "game_settings.sav";

        public void init()
        {
            LoadSettingData();
        }

        // 保存设置数据
        // 每隔一秒钟最多保存一次
        public void SaveSettingData()
        {
            string savePath = Utils.ResoucePaths.SaveDataPath;
            string SaveName = SETTING_DATA_FILE_NAME;
            if (!Directory.Exists(savePath))
            {
               Directory.CreateDirectory(savePath);
            }
            string json = JsonConvert.SerializeObject(_data, Formatting.Indented);
            string path = Path.Combine(savePath, SaveName);
            File.WriteAllText(path, json);
            Debug.Log("游戏设置保存成功，路径：" + path);
         }

        // 读取设置数据
        public void LoadSettingData()
        {
            string savePath = Utils.ResoucePaths.SaveDataPath;
            string filePath = Path.Combine(savePath, SETTING_DATA_FILE_NAME);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var loadedData = JsonConvert.DeserializeObject<GameConfigData>(json);

                if (loadedData != null)
                {
                    _data = loadedData;
                    Debug.Log("游戏设置加载成功");
                }
                else
                {
                    Debug.LogWarning("游戏设置文件为空，使用默认数据");
                    _data = new GameConfigData();
                }
            }
            else
            {
                Debug.Log("无游戏设置文件，使用默认数据");
                _data = new GameConfigData();
            }
        }
    }
    public class GameGlobaDataManager
    {
        //这里存放整个游戏共用的数据，例如已读顶点数据，周目数据（未实现）
        public DateTime LastSaveTime;
        private GameGlobalData _data;
        public void UpdataGlobalData(string NodeId)
        {
            data.AddVisitedNode(NodeId);
            SaveGlobalData();
        }
        public GameGlobalData data
        {
            get
            {
                if (_data == null)
                {
                    init();
                }
                return _data;
            }
            set
            {
                _data = value;
            }
        }
        public class GameGlobalData
        {
            public List<string> VisitedNode = new() { };
            public bool AddVisitedNode(string nodeId)
            {
                if (VisitedNode.Contains(nodeId))
                {
                    return false; // 已存在，不添加
                }
                else
                {
                    VisitedNode.Add(nodeId);
                    return true; // 成功添加
                }
            }
        }
        //public GameGlobalData gameGlobalData = new() { };
        //private static GameGlobaDataManager _instance;
        private const string GLOBAL_DATA_FILE_NAME = "global_data.sav";
        public void init()
        {
            LoadGlobalData();
            LastSaveTime = DateTime.Now;
        }

        // 保存全局数据
        //每隔一秒钟最多存档一次
        public void SaveGlobalData()
        {
            var CurrentTime = DateTime.Now;
            if ((CurrentTime - LastSaveTime).TotalSeconds<1f)
            {
                return;
            }
            else
            {
                LastSaveTime = CurrentTime;
                string savePath = Utils.ResoucePaths.SaveDataPath;
                string SaveName = GLOBAL_DATA_FILE_NAME;

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                string json = JsonConvert.SerializeObject(_data, Formatting.Indented);
                string path = Path.Combine(savePath, SaveName);

                File.WriteAllText(path, json);
                Debug.Log("全局数据保存成功，路径：" + path);
            }
        }

        // 读取全局数据
        public void LoadGlobalData()
        {
            string savePath = Utils.ResoucePaths.SaveDataPath;
            string filePath = Path.Combine(savePath, GLOBAL_DATA_FILE_NAME);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var loadedData = JsonConvert.DeserializeObject<GameGlobalData>(json);

                if (loadedData != null)
                {
                    _data = loadedData;
                    Debug.Log("全局数据加载成功");
                }
                else
                {
                    Debug.LogWarning("全局数据文件为空，使用默认数据");
                    _data = new GameGlobalData();
                }
            }
            else
            {
                Debug.Log("无全局数据文件，使用默认数据");
                _data = new GameGlobalData();
            }
        }
    }
}



