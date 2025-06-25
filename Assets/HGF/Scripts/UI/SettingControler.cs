using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingControler : MonoBehaviour
{
    public Slider BGMSlider;
    public Slider VoiceSlider;
    public Slider EffectSoundSlider;
    public Slider SpeedSlider;
    public Button CloseButton;
    // Start is called before the first frame update
    void Start()
    {
        SetPanel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        OnPanelActived();
    }
    public void OnPanelActived()
    {
        BGMSlider.value = GameMain.instance.gameSetting.BGMAudioSource.volume;
        VoiceSlider.value = GameMain.instance.gameSetting.VoiceAudioSource.volume;
        EffectSoundSlider.value = GameMain.instance.gameSetting.EffectAudioSource.volume;
        SpeedSlider.value = GameMain.instance.gameSL.saveDatas.baseSetting.AutoSpeed;
    }
    public void SetPanel()
    {
        if (GameMain.instance.gameSL.saveDatas.baseSetting == null)
        {
            GameMain.instance.gameSL.saveDatas.baseSetting = new
                (
                    0.6f, 1f, 1f, 1f
                );
        }
        CloseButton.onClick.AddListener(delegate
        {
            gameObject.SetActive(false);
            GameMain.instance.gameSL.saveDatas.baseSetting.BGMLoudness = BGMSlider.value;
            GameMain.instance.gameSL.saveDatas.baseSetting.VoiceLoudness = VoiceSlider.value;
            GameMain.instance.gameSL.saveDatas.baseSetting.EffectLoudness = EffectSoundSlider.value;
            GameMain.instance.gameSL.SaveGameData();
        });
        BGMSlider.onValueChanged.AddListener(
        delegate
        {
            GameMain.instance.gameSetting.BGMAudioSource.volume = BGMSlider.value;
        });

        VoiceSlider.onValueChanged.AddListener(
        delegate
        {
            GameMain.instance.gameSetting.VoiceAudioSource.volume = VoiceSlider.value;
        });

        EffectSoundSlider.onValueChanged.AddListener(
        delegate
        {
            GameMain.instance.gameSetting.EffectAudioSource.volume = EffectSoundSlider.value;
        });

    }
}
