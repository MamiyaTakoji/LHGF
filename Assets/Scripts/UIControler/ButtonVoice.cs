using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonVoice : MonoBehaviour
{
    public AudioSource buttonSound;
    // Start is called before the first frame update
    void Start()
    {
        //��ȡ��ť����Դ
        var G = GameObject.Find("ButtonEffectSound");
        buttonSound = G.GetComponent<AudioSource>();
        GetComponent<Button>().onClick.AddListener
            (
            delegate
            {
                buttonSound.Play();
            }
            );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
