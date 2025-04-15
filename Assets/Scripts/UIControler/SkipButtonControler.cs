using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class SkipButtonControler : MonoBehaviour
{
    // Start is called before the first frame update
    //�������ctrl���������Ƿ��Ѷ�������
    //�������Z����ֻ�����Ѷ�
    public Button LongSpeakSkipButton;
    public Button SpeakSkipButton;
    public float Counter = 0;
    public float CounterZ = 0;
    //����Z��ʱ��Ҫ�����Ѷ��ı���״̬����
    public Dictionary<string,List<string>> _readState = new Dictionary<string,List<string>>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
/*        CtrlSkip();
        ZSkip();*/
    }
    public void CtrlSkip()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Counter += Time.deltaTime;
            if (Counter > 0.2f)
            {
                Debug.Log("Ctrl������ס");
                GameMain.instance.gal.Button_Click_NextPlot();
                LongSpeakSkipButton.onClick.Invoke();
                SpeakSkipButton.onClick.Invoke();
                Counter = 0;
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            Counter = 0;
        }
    }
    public void ZSkip()
    {
        GalManager_Text_TexVer.skipSpeed = 0.2f;
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Z��������");
            //_readState = new Dictionary<string,List<string>>(GameMain.instance.galManager_Saver.saveDatas.readState);
            _readState = GameMain.instance.galManager_Saver.saveDatas.readState.ToDictionary(
                kvp => kvp.Key,          // ��ֱ�Ӹ��ƣ�string ��ֵ���壬���������
                kvp => new List<string>(kvp.Value) // ֵ������ List������Ԫ��
            );
        }

        if (Input.GetKey(KeyCode.Z))
        {
            var currentScriptName = GameMain.gameScriptName2Load;
            var currentScriptId = GameMain.instance.gal.currentId;
            if(_readState.ContainsKey(currentScriptName)) 
            {
                if (_readState[currentScriptName].Contains(currentScriptId))
                {
                    CounterZ += Time.deltaTime;
                    if (CounterZ > 0.05f)
                    {
                        Debug.Log("Z������ס");
                        Debug.Log(_readState[currentScriptName].Count);
                        GameMain.instance.gal.Button_Click_NextPlot();
                        LongSpeakSkipButton.onClick.Invoke();
                        SpeakSkipButton.onClick.Invoke();
                        CounterZ = 0;
                    }
                }
            }
            
        }
        if (Input.GetKeyUp(KeyCode.Z))
        {
            CounterZ = 0;
            GalManager_Text_TexVer.skipSpeed = 0.2f;
        }
    }
}
