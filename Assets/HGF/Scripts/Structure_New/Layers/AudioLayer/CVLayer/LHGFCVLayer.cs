using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LHGFData
{
    public class CVLayer : AudioLayer, ILayer
    {
        public bool IsVoiceFinish = true;
        public LHGFCVLayerControler CVLayerControler;


        //这个层不需要写回撤功能（暂时）真是太好了
        //不好
        IEnumerator PlayVoice(string VoicePath)
        {
            bool loading = true;
            AudioClip clip = null;
            yield return LoadAudioSource(VoicePath, (loadedClip) =>
            {
                clip = loadedClip;
                loading = false;
            });

            while (loading) yield return null;

            if (clip != null)
            {
                // 播放音频
                IsVoiceFinish = false;
                CVLayerControler.CVPlayer.clip = clip;
                CVLayerControler.CVPlayer.Play();

                // 等待播放完成
                yield return new WaitForSeconds(clip.length);

                // 播放完成后的逻辑
                Debug.Log($"音频播放完成: {VoicePath}");
                IsVoiceFinish = true;
            }

        }
        public bool Finish()
        {
            return true;
        }

        public GameObject GetControler()
        {
            return CVLayerControler.gameObject;
        }

        public string LayerName()
        {
            return "CVLayer";
        }

        public void Next(LayerCommand command)
        {
            if (!command.CommandConfig.ContainsKey("CVPath"))
            {
                return;
            }
            string _CVPath = command.CommandConfig["CVPath"];
            string CVPath = Path.Combine(Utils.ResoucePaths.CVPath, _CVPath);
            CVLayerControler.StartCoroutine(PlayVoice(CVPath));
        }

        public void Next_OnLoad(LayerCommand command)
        {

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
            if (!IsVoiceFinish)
            {
                CVLayerControler.CVPlayer.Stop();
            }
        }

        public Dictionary<string, string> Log(LayerCommand command)
        {
            return null;
        }

        public void Reset()
        {
            
        }
        public void BeforeNextOnLoadStart() { }

        public void Load(object SaveData)
        {
            
        }

        public object Save()
        {
            return null;
        }
    }

}
