using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class SoundsControler : MonoBehaviour
{
    public AudioSource Voice;
    public AudioSource BGMPlayer;
    public AudioSource EffectSoundPlayer;
    public int IsVoiceFinish = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private IEnumerator PlayAudio(AudioSource audioSource, string filePath, UnityAction callback = null)
    {
        //获取.wav文件，并转成AudioClip
        AudioType audioType = Path.GetExtension(filePath).ToLower() switch
        {
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            _ => AudioType.UNKNOWN
        };
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType);
        //等待转换完成
        yield return www.SendWebRequest();
        //获取AudioClip
        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
        Debug.Log(www);
        //设置当前AudioSource组件的AudioClip
        audioSource.clip = audioClip;
        //播放声音
        audioSource.Play();
        StartCoroutine(AudioPlayFinished(audioSource.clip.length, callback));
    }
    private IEnumerator AudioPlayFinished(float time, UnityAction callback)
    {
        yield return new WaitForSeconds(time);
        callback.Invoke();
    }
    public IEnumerator PlayVoice(string filePath)
    {
        if(IsVoiceFinish < 0) 
        {
            IsVoiceFinish = 0;
        }
        IsVoiceFinish += 1;
        yield return PlayAudio(Voice, filePath, () => { IsVoiceFinish += -1; });
    }
    public IEnumerator PlayBGM(string filePath)
    {
        yield return PlayAudio(BGMPlayer, filePath);
    }
    public void StopVoice()
    {
        Voice.Stop();
    }
    public void StopBGM()
    {
        BGMPlayer.Stop();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
