using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatBarTestScript : MonoBehaviour
{
    // Start is called before the first frame update
    public LHGFTalkBarControler bar;
    public float Time_Sleep = 1f;
    public float CurrentTime = 0f;
    public bool Finish = false;
    public string Content = "测试";
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Finish)
        {
            CurrentTime += Time.deltaTime;
            if (CurrentTime > Time_Sleep)
            {
                bar.SetText(Content);
                Finish = true;
            }
        }
    }
}
