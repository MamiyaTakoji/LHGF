using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace LHGFData
{
    public class BgmLayer : AudioLayer, ILayer
    {
        public LHGFBgmLayerControler BgmLayerControler;
        public string stopBgmSign = "STOPBGM";
        private Coroutine currentLoadCoroutine;
        public List<AudioData> audioDatas = new() { };
        public bool Finish()
        {
            //BGM放不放完都无所谓吧
            return true;
        }

        public GameObject GetControler()
        {
            return BgmLayerControler.gameObject;
        }

        public string LayerName()
        {
            return "BgmLayer";
        }

        public void PlayeBGM(AudioData audioData)
        {
            string BgmPath = audioData.AudioPath;
            bool IsLoop = audioData.IsLoop;
            //float Volume = audioData.Volume;

            // 停止任何正在进行的加载协程
            if (currentLoadCoroutine != null)
            {
                BgmLayerControler.StopCoroutine(currentLoadCoroutine);
                currentLoadCoroutine = null;
            }

            if (BgmPath == stopBgmSign)
            {
                BgmLayerControler.audioSource.Stop();
                return;
            }

            // 启动新协程并保存引用
            currentLoadCoroutine = BgmLayerControler.StartCoroutine(LoadAudioSource(BgmPath, (clip) =>
            {
                if (clip != null)
                {
                    BgmLayerControler.audioSource.clip = clip;
                    BgmLayerControler.audioSource.loop = IsLoop;
                    //BgmLayerControler.audioSource.volume = Volume;
                    BgmLayerControler.audioSource.Play();
                }
                // 加载完成（无论成功与否）清除引用
                currentLoadCoroutine = null;
            }));
        }
        public void Next(LayerCommand command)
        {
            string BgmName = command.CommandConfig.ContainsKey("BgmName") ?
                 command.CommandConfig["BgmName"] : null;
            if (BgmName == null) { Debug.LogWarning("缺少参数BgmName"); }
            string VolumeStr = command.CommandConfig.ContainsKey("Volume") ?
     command.CommandConfig["Volume"] : "0.5";
            string IsLoopStr = command.CommandConfig.ContainsKey("IsLoopStr") ?
command.CommandConfig["IsLoopStr"] : "1";
            bool IsLoop = Utils.string2bool(IsLoopStr);
            bool success = float.TryParse(VolumeStr, out float Volume);
            if (!success)
            {
                Volume = 0.5f;
            }
            string BgmPath = (BgmName == stopBgmSign) ? stopBgmSign :
            Path.Combine(Utils.ResoucePaths.BgmPath, BgmName);
            var AudioData = new AudioData(BgmPath, Volume, IsLoop);
            audioDatas.Add(AudioData);
            PlayeBGM(AudioData);

        }
        public void Next_OnLoad(LayerCommand command)
        {
            string BgmName = command.CommandConfig.ContainsKey("BgmName") ?
                 command.CommandConfig["BgmName"] : null;
            if (BgmName == null) { Debug.LogWarning("缺少参数BgmName"); }
            string VolumeStr = command.CommandConfig.ContainsKey("Volume") ?
            command.CommandConfig["Volume"] : "0.5";
            string IsLoopStr = command.CommandConfig.ContainsKey("IsLoopStr") ?
            command.CommandConfig["IsLoopStr"] : "1";
            bool IsLoop = Utils.string2bool(IsLoopStr);
            bool success = float.TryParse(VolumeStr, out float Volume);
            if (!success)
            {
                Volume = 0.5f;
            }
            string BgmPath = (BgmName == stopBgmSign)?stopBgmSign:
                Path.Combine(Utils.ResoucePaths.BgmPath, BgmName);
            var AudioData = new AudioData(BgmPath, Volume, IsLoop);
            audioDatas.Add(AudioData);
            PlayeBGM(AudioData);
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
            if(audioDatas.Count == 0)
            {
                return false;
            }
            //最初是没有背景音乐的
            if (audioDatas.Count == 1)
            {
                audioDatas.RemoveAt(0);
                BgmLayerControler.audioSource.Stop();
            }
            else if (audioDatas.Count>1)
            {
                audioDatas.RemoveAt(audioDatas.Count-1);
                PlayeBGM(audioDatas[audioDatas.Count - 1]);
            }
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
            //停止播放音乐并清空历史记录
            audioDatas = new() { };
            if (currentLoadCoroutine != null)
            {
                BgmLayerControler.StopCoroutine(currentLoadCoroutine);
                currentLoadCoroutine = null;
            }
            BgmLayerControler.audioSource.Stop();
        }
        public void BeforeNextOnLoadStart() { }

        public class SaveData
        {
            public List<AudioData> audioDatas = new() { };
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
            audioDatas.Clear();

            // 恢复历史音频数据（深拷贝）
            if (data.audioDatas != null)
            {
                audioDatas.AddRange(data.audioDatas);
            }

            // 停止当前正在进行的加载协程和音频播放
            if (currentLoadCoroutine != null)
            {
                BgmLayerControler.StopCoroutine(currentLoadCoroutine);
                currentLoadCoroutine = null;
            }
            BgmLayerControler.audioSource.Stop();

            // 如果恢复后有音频数据，播放最后一个（即最新状态）
            if (audioDatas.Count > 0)
            {
                PlayeBGM(audioDatas[audioDatas.Count - 1]);
            }
        }
        public object Save()
        {
            var saveData = new SaveData();
            // 保存 audioDatas 的副本（元素引用相同，但 AudioData 通常不可变）
            saveData.audioDatas = new List<AudioData>(this.audioDatas);
            return saveData;
        }
    }
}
