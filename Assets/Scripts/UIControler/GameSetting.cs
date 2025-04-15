using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetting : MonoBehaviour
{
    public static float BGMLoudness;
    public static float VoiceLoudness;
    public static float EffectLoudness;
    public static float AutoSpeed;
    public AudioSource BGMAudioSource;
    public AudioSource VoiceAudioSource;
    public AudioSource EffectAudioSource;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetBaseSetting(BaseSetting baseSetting)
    {
        BGMLoudness = baseSetting.BGMLoudness;
        VoiceLoudness = baseSetting.VoiceLoudness;
        EffectLoudness = baseSetting.EffectLoudness;
        AutoSpeed = baseSetting.AutoSpeed;
        BGMAudioSource.volume = BGMLoudness;
        VoiceAudioSource.volume = VoiceLoudness;
        EffectAudioSource.volume = EffectLoudness;
    }
}
