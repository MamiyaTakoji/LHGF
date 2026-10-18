using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace LHGFData 
{
    public class VideoAnimationLayer : VideoLayer, ILayer
    {
        //处于性能考虑，生成Controler后不Destory而是Active设为false
        //一个VideoAnimation可以持续多个节点，回撤时动画会继续播放
        //直到动画播放完成为止才结束
        public LHGFVideoAnimationLayerControler videoAnimationLayerControler;
        public LHGFVideoAnimationPlayer currentPlayer = null;
        //public Dictionary<string, LayerCommand> HistoryCommands;
        //在加载完成后会执行一次Forward，在那时释放保存好的视频数据
        public Dictionary<string, List<LayerCommand>> OnLoadVideoAnimation = new() { };
        public Dictionary<string, List<LayerCommand>> HistoryVideoAnimation = new() { };
        public List<string> HistoryVideoAnimationNames = new() { };
        //按道理不终止就播放动画有点奇怪，总之这里和CILayer的逻辑类似
        public class VideoAnimationData
        {
            public VideoData videoData;
            public Vector2 Pos = new Vector2(0.5f, 0.5f);
            public float Size = 1f;
            public float Rotation = 0;
            public VideoAnimationData(Dictionary<string, string> Config)
            {
                if (!Config.ContainsKey("VideoName"))
                {
                    Debug.LogError("未输入必要参数VideoName");
                    return;
                }
                string videoName = Config["VideoName"];
                string videoPath = Path.Combine(Utils.ResoucePaths.VideoAnimationPath, videoName);
                string IsLoopStr = Utils.GetDicValue(Config, "IsLoop", "0");
                bool IsLoop = Utils.string2bool(IsLoopStr);
                videoData = new VideoData(videoPath, IsLoop);
                string SizeStr = Utils.GetDicValue(Config, "Size", "1");
                Size = float.Parse(SizeStr);
                string RotationStr = Utils.GetDicValue(Config, "Rotation", "0");
                Rotation = float.Parse(RotationStr);
                string PosXStr = Utils.GetDicValue(Config, "PosX", "0.5");
                string PosYStr = Utils.GetDicValue(Config, "PosY", "0.5");
                float posX = float.Parse(PosXStr);
                float posY = float.Parse(PosYStr);
                Pos = new Vector2(posX, posY);
            }
        }
        public bool Finish()
        {
            return true;
        }

        public GameObject GetControler()
        {
            return videoAnimationLayerControler.gameObject;
        }

        public string LayerName()
        {
            throw new System.NotImplementedException();
        }

        public void Log()
        {
            throw new System.NotImplementedException();
        }

        public void Next(LayerCommand command)
        {
/*            if(OnLoadVideoAnimation != null)
            {
                HistoryVideoAnimation = new(OnLoadVideoAnimation);
                OnLoadVideoAnimation = null;
            }*/
            VideoAnimationData data = new(command.CommandConfig);
            string path = data.videoData.VideoPath;
            HistoryVideoAnimationNames.Add(path);
            if (HistoryVideoAnimation.ContainsKey(path))
            {
                HistoryVideoAnimation[path].Add(command);
            }
            else
            {
                HistoryVideoAnimation[path] = new() { command };
            }
            PlayVideoAnimation(command);
        }
        public void PlayVideoAnimation(LayerCommand command, bool IsLoad=false) 
        {
            //目前不支持同时播放多个具有相同名字的视频动画
            VideoAnimationData data = new(command.CommandConfig);
            var Players = videoAnimationLayerControler.videoAnimationPlayers;
            var path = data.videoData.VideoPath;
            if (Players.ContainsKey(path))
            {
                currentPlayer = Players[path];
                currentPlayer.gameObject.SetActive(true);
                currentPlayer.IsOnUsing = true;
                if (command.CommandConfig.ContainsKey("Finish"))
                {
                    if (command.CommandConfig["Finish"] == "1")
                    {
                        currentPlayer.IsOnUsing = false;
                        currentPlayer.videoPlayer.Stop();
                        currentPlayer.gameObject.SetActive(false);
                        return;
                    }
                }
            }
            if (currentPlayer == null)
            {
                foreach (string key in Players.Keys)
                {
                    if (!Players[key].IsOnUsing)
                    {
                        Players.Remove(key, out currentPlayer);
                        Players.Add(path, currentPlayer);
                        currentPlayer.gameObject.SetActive(true);
                        currentPlayer.IsOnUsing = true;
                    }
                }
            }
            if (currentPlayer == null)
            {
                var G = Object.Instantiate(videoAnimationLayerControler.videoAnimationPlayer);
                currentPlayer = G.GetComponent<LHGFVideoAnimationPlayer>();
                G.transform.parent = videoAnimationLayerControler.transform;
                Utils.SetSingleAnchorPoint(G.GetComponent<RectTransform>(), data.Pos);
                Utils.SetRelativeSize(G.GetComponent<RectTransform>(), data.Size);
                var lp = G.GetComponent<RectTransform>().localPosition;
                lp.z = 0;
                G.GetComponent<RectTransform>().localPosition = lp;
                videoAnimationLayerControler.videoAnimationPlayers.Add(path, currentPlayer);
            }
            currentPlayer.IsLoadFinish = false;
            currentPlayer.IsOnUsing = true;
            if (!IsLoad)
            {
                PlayVideo(data.videoData, currentPlayer.videoPlayer, onVideoFinish);
            }
        }
        public void onVideoFinish(VideoPlayer video)
        {
            currentPlayer.IsLoadFinish = true;
            video.Play();
        }
        public void Next_OnLoad(LayerCommand command)
        {
            VideoAnimationData data = new(command.CommandConfig);
            string path = data.videoData.VideoPath;
            HistoryVideoAnimationNames.Add(path);
            if (OnLoadVideoAnimation.ContainsKey(path))
            {
                OnLoadVideoAnimation[path].Add(command);
            }
            else
            {
                OnLoadVideoAnimation[path] = new() {command};
            }
            PlayVideoAnimation(command,IsLoad:true);
        }
        public void PlayVideoAnimations(List<LayerCommand> layerCommands)
        {
            foreach(var command in layerCommands)
            {
                //如果已经Finish则不播放
                if (!command.CommandConfig.ContainsKey("Finish")|| !(command.CommandConfig["Finish"] == "1"))
                {
                    PlayVideoAnimation(command);
                }
            }
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
            //删除最后一个元素并播放
            if (HistoryVideoAnimationNames.Count > 0)
            {
                var LastVideoAnimationName = HistoryVideoAnimationNames[HistoryVideoAnimationNames.Count - 1];
                var Commands = HistoryVideoAnimation[LastVideoAnimationName];
                if (Commands.Count == 1&& videoAnimationLayerControler.videoAnimationPlayers.ContainsKey(LastVideoAnimationName))
                {
                    var currentPlayer = videoAnimationLayerControler.videoAnimationPlayers[LastVideoAnimationName];
                    //如果已经只剩下最后一个元素了，则停止播放动画
                    currentPlayer.IsOnUsing = false;
                    currentPlayer.videoPlayer.Stop();
                    currentPlayer.gameObject.SetActive(false);
                }
                else
                {
                    var lastCommand = Commands[Commands.Count - 2];
                    PlayVideoAnimation(lastCommand);
                }
                Commands.RemoveAt(Commands.Count - 1);
                return true;
            }
            else
            {
                return false;
            }
        }
        public void OnLoadFinish()
        {
            var OnLoadFinalVideoAnimation = new List<LayerCommand>() { };
            foreach (string key in OnLoadVideoAnimation.Keys)
            {
                var vals = OnLoadVideoAnimation[key];
                var val = vals[vals.Count - 1];
                OnLoadFinalVideoAnimation.Add(val);
            }
            PlayVideoAnimations(OnLoadFinalVideoAnimation);
            HistoryVideoAnimation = new Dictionary<string, List<LayerCommand>>(OnLoadVideoAnimation);
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
            foreach(var Player in videoAnimationLayerControler.videoAnimationPlayers.Values)
            {
                Player.IsOnUsing = false;
                Player.videoPlayer.Stop();
                Player.gameObject.SetActive(false);
            }
        }
        public void BeforeNextOnLoadStart() { }
        [System.Serializable]
        public class SaveData
        {
            public List<string> HistoryVideoAnimationNames = new() { };
            public Dictionary<string, List<LayerCommand>> HistoryVideoAnimation;
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

            // 重置当前状态：停止所有视频，清空历史
            Reset(); // 假设 Reset 方法已经能停止所有视频播放器并清空自身状态

            // 恢复历史数据
            HistoryVideoAnimationNames = new List<string>(data.HistoryVideoAnimationNames);
            HistoryVideoAnimation = new Dictionary<string, List<LayerCommand>>();
            foreach (var kvp in data.HistoryVideoAnimation)
            {
                HistoryVideoAnimation[kvp.Key] = new List<LayerCommand>(kvp.Value);
            }

            // 恢复视频播放状态：为每个视频路径播放最后一次命令
            var finalCommands = new List<LayerCommand>();
            foreach (string path in HistoryVideoAnimationNames) // 按顺序？但是每个视频可能有多个命令，我们只需每个视频的最后一条
            {
                if (HistoryVideoAnimation.ContainsKey(path) && HistoryVideoAnimation[path].Count > 0)
                {
                    var lastCommand = HistoryVideoAnimation[path][HistoryVideoAnimation[path].Count - 1];
                    // 避免重复添加同一个视频的最后一条命令（如果不同路径最后命令可能是同一个，但没关系）
                    finalCommands.Add(lastCommand);
                }
            }
            // 播放这些命令（每个视频一次）
            PlayVideoAnimations(finalCommands);
        }

        public object Save()
        {
            var saveData = new SaveData
            {
                HistoryVideoAnimationNames = new List<string>(this.HistoryVideoAnimationNames),
                HistoryVideoAnimation = new Dictionary<string, List<LayerCommand>>()
            };
            foreach (var kvp in this.HistoryVideoAnimation)
            {
                saveData.HistoryVideoAnimation[kvp.Key] = new List<LayerCommand>(kvp.Value);
            }
            return saveData;
        }
    }
}

