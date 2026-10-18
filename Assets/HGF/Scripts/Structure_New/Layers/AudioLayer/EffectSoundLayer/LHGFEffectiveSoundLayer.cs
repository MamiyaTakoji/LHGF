using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace LHGFData
{
    public class EffectiveSoundLayer : AudioLayer, ILayer
    {
        public bool IsVoiceFinish = true;
        public LHGFEffectiveSoundLayerControler EffectiveSoundLayerControler;
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
                EffectiveSoundLayerControler.EffectiveSoundPlayer.clip = clip;
                EffectiveSoundLayerControler.EffectiveSoundPlayer.Play();

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
            return EffectiveSoundLayerControler.gameObject;
        }

        public string LayerName()
        {
            return "EffectiveSoundLayer";
        }

        public void Next(LayerCommand command)
        {
            if (!command.CommandConfig.ContainsKey("EffectiveSoundPath"))
            {
                return;
            }
            string _EffectiveSoundPath = command.CommandConfig["EffectiveSoundPath"];
            string EffectiveSoundPath = Path.Combine(Utils.ResoucePaths.EffectiveSoundPath, _EffectiveSoundPath);
            EffectiveSoundLayerControler.StartCoroutine(PlayVoice(EffectiveSoundPath));
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
                EffectiveSoundLayerControler.EffectiveSoundPlayer.Stop();
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
