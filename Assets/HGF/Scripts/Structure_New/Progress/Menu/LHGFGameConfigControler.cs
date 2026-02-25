using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LHGFGameConfigControler : MonoBehaviour
{
    // Start is called before the first frame update
    public Slider BGMSlider;
    public Slider EffectSoundSlider;
    public Slider CharacterVoiceSlider;
    public Button IsSkipUnreadContent;

    public AudioSource BGMAudioSource;
    public AudioSource EffectSoundAudioSource;
    public AudioSource CharacterVoiceAudioSource;

    private float Counter = 0f;
    public float SaveTime = 1f;
    private bool IsSave = false;
    void Start()
    {
        var SettingData = LHGFGameMain.instance.gameConfigDataManager.data;
        BGMSlider.onValueChanged.AddListener(
                delegate
                {
                    SettingData.BGMVolume = BGMSlider.value;
                    BGMAudioSource.volume = BGMSlider.value;
                    IsSave = true;
                }
            );
        EffectSoundSlider.onValueChanged.AddListener(
                delegate
                {
                    SettingData.EffectSoundVolme = EffectSoundSlider.value;
                    EffectSoundAudioSource.volume = BGMSlider.value;
                    IsSave = true;
                }
            );
        CharacterVoiceSlider.onValueChanged.AddListener(
                delegate
                {
                    SettingData.CVVolme = CharacterVoiceSlider.value;
                    CharacterVoiceAudioSource.volume = CharacterVoiceSlider.value;
                    IsSave = true;
                }
            );
        IsSkipUnreadContent.onClick.AddListener(
                delegate
                {
                    SettingData.IsSkipUnreadContent = !SettingData.IsSkipUnreadContent;
                    if (SettingData.IsSkipUnreadContent)
                    {
                        IsSkipUnreadContent.GetComponentInChildren<TMP_Text>().color = Color.black;
                    }
                    else
                    {
                        IsSkipUnreadContent.GetComponentInChildren<TMP_Text>().color = Color.grey;
                    }
                    IsSave = true;
                }
            );
    }

    // Update is called once per frame
    void Update()
    {
        if (IsSave)
        {
            if (Counter > 1f)
            {
                LHGFGameMain.instance.gameConfigDataManager.SaveSettingData();
                Counter = 0;
                IsSave = false;
            }
            else
            {
                Counter += Time.deltaTime;
            }
        }
    }
    public void OnEnable()
    {
        var SettingData = LHGFGameMain.instance.gameConfigDataManager.data;
        BGMSlider.value = SettingData.BGMVolume;
        EffectSoundSlider.value = SettingData.EffectSoundVolme;
        CharacterVoiceSlider.value = SettingData.CVVolme;
        if (SettingData.IsSkipUnreadContent)
        {
            IsSkipUnreadContent.GetComponentInChildren<TMP_Text>().color = Color.black;
        }
        else
        {
            IsSkipUnreadContent.GetComponentInChildren<TMP_Text>().color = Color.grey;
        }
    }
}
