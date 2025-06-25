using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShortCutControler : MonoBehaviour
{
    // Start is called before the first frame update
    public KeyCode SkipButton1 = KeyCode.LeftControl;
    public KeyCode SkipButton2 = KeyCode.Z;
    public KeyCode AutoButton = KeyCode.A;
    public LoggerControler loggerControler;
    public float waitTime = 0.5f;
    public TMP_Text ShowingIsAuto;
    public bool IsAuto = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(SkipButton1)) 
        {
            GameMain.instance.gameProgress.Skip();
        }
        if (Input.GetKey(SkipButton2))
        {
            GameMain.instance.gameProgress.SkipReadedContent();
        }
        if (Input.anyKeyDown && IsAuto)
        {
            IsAuto = false;
            ShowingIsAuto.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(AutoButton))
        {
            IsAuto = true;
            ShowingIsAuto.gameObject.SetActive(true);
        }
        if (IsAuto)
        {
            GameMain.instance.gameProgress.Auto();
        }
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta > 0f)
        {
            loggerControler.Open();
        }
    }
}
