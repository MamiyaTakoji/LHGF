using Common.Game;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class StarMenuControler : MonoBehaviour
{
    // Start is called before the first frame update
    public GameConfig TitleSetting;
    public TextComponent titleTextComponent;
    public RawImage BackGround;
    public AudioSource BGMAudioSource;
    public AudioSource VoiceAudioSource;
    public AudioSource EffectAudioSource;
    void Start()
    {
        SetTitle();
        SetBackGround();
        //audioSource = GetComponent<AudioSource>();
        StartCoroutine(LoadMusic());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadMenuConfig()
    {

    }
    public void SetTitle()
    {
        TitleSetting = new GameConfig(Application.streamingAssetsPath + "/HGF/StartMenuSetting/StartMenuSetting.ini");
        titleTextComponent.ResetText();
        titleTextComponent.current_Text_Type = TitleSetting.GetValue("Title", "TextType");
        titleTextComponent.text = TitleSetting.GetValue("Title", "Content");
    }
    public void SetBackGround()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "HGF/StartMenuSetting/Background.png");
        Debug.Log(filePath);
        if (File.Exists(filePath))
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes); // 自动解析PNG数据
            BackGround.texture = texture;
        }
    }
    IEnumerator LoadMusic()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "HGF/StartMenuSetting/BackgroundMusic.mp3");
        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            BGMAudioSource.clip = clip;
            BGMAudioSource.loop = true;
            BGMAudioSource.Play();
        }
        else
        {
            Debug.LogError("加载失败: " + request.error);
        }
    }
}
