using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LHGFMainScenceControler : MonoBehaviour
{
    //主菜单的快捷键以及按钮的调整都在这里实现 
    public float MinOperationInterval = 0.1f;
    private float CurrentTime = 0f;
    public KeyCode Withdraw = KeyCode.Backspace;
    public KeyCode Forward = KeyCode.Space;
    public KeyCode Skip = KeyCode.LeftControl;

    public Button Next;
    public Button WithdrawButton;

    public Button SkipButton;
    private bool IsOnSkip = false;
    public float SkipSpeed = 0.2f;

    public Button AutoButton;
    private bool IsOnAuto = false;
    public float AutoWaitTime = 1f;
    private float AutoWaitTimeCounter = 0f;

    public Button SaveButton;
    public Button LoadButton;
    //点击后访问设置菜单
    public Button MenuButton;

    public LHGFGamaeMenuControler Menu;
    public LHGFContentLogerControler logger;
    void Start()
    {
        Next.onClick.AddListener(delegate
        {
            StopSkipOrAuto();
            if (CurrentTime > MinOperationInterval)
            {
                LHGFGameMain.instance.Forward();
                CurrentTime = 0;
            }
        }
        );
        WithdrawButton.onClick.AddListener(delegate
        {
            StopSkipOrAuto();
            if (CurrentTime > MinOperationInterval)
            {
                LHGFGameMain.instance.Withdraw();
                CurrentTime = 0;
            }
        }
        );
        SkipButton.onClick.AddListener(delegate
        {
            bool _IsOnSkip = IsOnSkip;
            StopSkipOrAuto();
            IsOnSkip = !_IsOnSkip;
            ShowButtonState(SkipButton.gameObject, IsOnSkip);
        }
        );
        AutoButton.onClick.AddListener(delegate
        {
            bool _IsOnAuto = IsOnAuto;
            StopSkipOrAuto();
            IsOnAuto = !_IsOnAuto;
            ShowButtonState(AutoButton.gameObject, IsOnAuto);
        }
        );
        SaveButton.onClick.AddListener(delegate
        {
            gameObject.SetActive(false);
            Menu.gameObject.SetActive(true);
            Menu.ShowSavePanel();
        }
        );
        LoadButton.onClick.AddListener(delegate
        {
            gameObject.SetActive(false);
            Menu.gameObject.SetActive(true);
            Menu.ShowLoadPanel();
        });
        MenuButton.onClick.AddListener(delegate
        {
            gameObject.SetActive(false);
            Menu.gameObject.SetActive(true);
            Menu.ConfigButtonControler();
        });

    }

    // Update is called once per frame
    void Update()
    {
        //如果执行了其他操作，则停止自动和跳过
        CurrentTime += Time.deltaTime;
        if (CurrentTime < MinOperationInterval)
        {
            return;
        }
        if (Input.GetKeyDown(Withdraw))
        {
            StopSkipOrAuto();
            LHGFGameMain.instance.Withdraw();
            CurrentTime = 0;
            return;
        }
        else if (Input.GetKeyDown(Forward))
        {
            StopSkipOrAuto();
            LHGFGameMain.instance.Forward();
            CurrentTime = 0;
            return;
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            StopSkipOrAuto();
            logger.Open();
        }
        else if (Input.GetKey(Skip)||IsOnSkip)
        {
            if(CurrentTime>(1/SkipSpeed)*Time.deltaTime)
            {
                if(LHGFGameMain.instance.gameConfigDataManager.data.IsSkipUnreadContent||
                   LHGFGameMain.instance.gameGlobaDataManager.data.VisitedNode.Contains
                   (
                       LHGFGameMain.instance.gameProgress.CurrentNodeId
                    ))
                LHGFGameMain.instance.Forward();
                CurrentTime = 0;
                return;
            }
        }
        if (IsOnAuto)
        {
            //如果所有层都执行完成，则开始计算等待时间
            if (LHGFGameMain.instance.IsFinish)
            {
                AutoWaitTimeCounter += Time.deltaTime;
                if (AutoWaitTimeCounter > AutoWaitTime)
                {
                    LHGFGameMain.instance.Forward();
                    AutoWaitTimeCounter = 0;
                    return;
                }
            }
        }
    }
    public void StopSkipOrAuto()
    {
        IsOnSkip = false;
        IsOnAuto = false;
        ShowButtonState(SkipButton.gameObject, false);
        ShowButtonState(AutoButton.gameObject, false);
    }
    //假定游戏对象的子对象有一个TMP_Text组件
    public void ShowButtonState(GameObject G, bool IsOnUsing)
    {
        TMP_Text tmp = G.GetComponentInChildren<TMP_Text>();
        if (IsOnUsing)
        {
            tmp.fontStyle |= FontStyles.Underline;
        }
        else
        {
            tmp.fontStyle &= ~FontStyles.Underline;
        }
    }
}
