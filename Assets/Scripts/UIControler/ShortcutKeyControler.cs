using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortcutKeyControler : MonoBehaviour
{
    // Start is called before the first frame update
    private float scroll;
    private bool IsAuto;
    private float Counter;
    public GameObject ShowAuto;
    public GameObject Logger;
    public GameObject SLPanel;
    public GameObject Setting;
    public SkipButtonControler Skip;
    void Start()
    {
        Skip = GetComponent<SkipButtonControler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Logger.activeSelf && !SLPanel.activeSelf &&!Setting.activeSelf)
        {
            Skip.CtrlSkip();
            Skip.ZSkip();
            SetLogger();
            SetAuto();
            if (IsAuto)
            {
                Counter += Time.deltaTime;
                Auto();
            }
        }

    }
    void SetLogger()
    {
        scroll = Input.GetAxis("Mouse ScrollWheel");

        // 滚轮向上滚动时返回正值（通常 >0.1）
        if (scroll > 0)
        {
            Logger.SetActive(true);
            IsAuto = false;
        }
    }
    void SetAuto()
    {
        if (Input.anyKeyDown)
        {
            Debug.Log("检测到任意按键被按下");
            IsAuto = false;
            Counter = 0;
            ShowAuto.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            IsAuto = true;
            ShowAuto.SetActive(true);
        }
    }
    void Auto()
    {
        if(Counter> 4.5-GameMain.instance.galManager_Saver.saveDatas.baseSetting.AutoSpeed) 
        { 
            GameMain.instance.gal.Button_Click_NextPlot();
            Counter = 0;
        }
    }

}
