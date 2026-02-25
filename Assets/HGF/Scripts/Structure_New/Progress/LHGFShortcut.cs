using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LHGFShortcut : MonoBehaviour
{
    //我没招了，限制一下操作的频率吧
    //所有的按键操作，以及鼠标的点击，都会重置计时器
    public float MinOperationInterval = 0.1f;
    private float CurrentTime = 0f;
    public KeyCode Withdraw = KeyCode.Backspace;
    public KeyCode Forward = KeyCode.Return;
    public Button Next;
    public LHGFContentLogerControler logger;
    // Start is called before the first frame update
    void Start()
    {
        Next.onClick.AddListener(delegate 
        {
            if (CurrentTime > MinOperationInterval)
            {
                LHGFGameMain.instance.Forward();
                CurrentTime = 0;
            }
        }
        );
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime += Time.deltaTime;
        if (CurrentTime < MinOperationInterval)
        {
            return;
        }
        if (Input.GetKeyDown(Withdraw))
        {
            LHGFGameMain.instance.Withdraw();
            CurrentTime = 0;
            return;
        }
        if (Input.GetKeyDown(Forward))
        {
            LHGFGameMain.instance.Forward();
            CurrentTime = 0;
            return;
        }
        if (Input.mouseScrollDelta.y>0)
        {
            logger.Open();
        }
    }
}
