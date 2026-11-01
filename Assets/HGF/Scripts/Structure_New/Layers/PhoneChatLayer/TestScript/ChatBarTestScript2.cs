using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatBarTestScript2 : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Content;
    public LHGFTalkBarControler barPerfab;
    public LHGFTalkBarControler barPerfab2;
    public LHGFTalkBarControler barPerfab3;
    public LHGFTalkBarControler barPerfab4;
    public LHGFTalkBarControler barPerfab5;
    public float Time_Sleep = 1f;
    public float CurrentTime = 0f;
    public bool Finish = false;
    public string BarContent = "测试";
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
                var bar = Instantiate(barPerfab, Content.transform);
                bar.SetText(BarContent);
                var bar2 = Instantiate(barPerfab2, Content.transform);
                bar2.SetFile(null, "1.png", "1.png", false);
                var bar3 = Instantiate(barPerfab3, Content.transform);
                bar3.SetTimeStamp("2024-01-01 12:00",false);
                var bar4 = Instantiate(barPerfab4, Content.transform);
                bar4.SetGif("F:/Tencent Files/2237804476/nt_qq/nt_data/Emoji/emoji-recv/2026-10/Ori/testImage.gif");
                var bar5 = Instantiate(barPerfab5, Content.transform);
                bar5.SetImagePath("C:/Users/22378/Desktop/1.png");
                Finish = true;
            }
        }
    }
}
