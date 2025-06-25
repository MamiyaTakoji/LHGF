using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ButtonVoice : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //获取按钮声音源
        var G = GameObject.Find("EffectSound");
        var buttonSound = G.GetComponent<AudioSource>();
        GetComponent<Button>().onClick.AddListener
            (
            delegate
            {
                buttonSound.Play();
            }
            );
    }
}
