using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LHGFData;
using LHGFGameProgress;
public class LHGFChoiceLayerControler : MonoBehaviour
{
    public ChoiceLayer choiceLayer;
    public GameObject Choice;
    public GameObject Choices;
    void Awake()
    {
/*        Choice = Resources.Load<GameObject>("LHGF/Button-Choice");
        Choices = GameObject.Find("Choices");*/
    }
    public void SetChoiceButton(string ChoiceID, string ChoiceContent, string ChoiceContentType)
    {
        GameObject Button = Instantiate(Choice, Choices.transform);
        Button.GetComponent<ChoiceButtonControler>().Init(ChoiceID, ChoiceContent, ChoiceContentType);
        Button.name = ChoiceID;
        Button.GetComponent<Button>().onClick.AddListener(
            delegate
            {
                choiceLayer.IsChoiceSelected = true;
                LHGFGameMain.instance.gameProgress.NextNodeId = Button.name; 
                //LHGFGameProgress.LHGF_GameProgress.instance.NextNodeId = Button.name;
                LHGFGameMain.instance.gameProgress.Skip(LHGFGameMain.instance.gameData);
                LHGFGameMain.instance.ContentLogerControler.LogContent
                (
                    choiceLayer.SetLogInfo(ChoiceContent, ChoiceContentType)
                ) ;
                LHGFGameMain.instance.Forward();
                Button_Click_Choice();
            }
        );
    }
    public void ResetButton()
    {
        if (Choices==null)
        {
            return;
        }
        for (int i = 0; i < Choices.transform.childCount; i++)
        {
            //不可用DestroyImmediate
            //原因：DestroyImmediate是同步的，如果使用则会导致每次获取的都是0，无法删除，
            Destroy(Choices.transform.GetChild(i).gameObject);
        }
    }
    public void Button_Click_Choice()
    {
/*        for (int i = 0; i < this.transform.childCount; i++)
        {
            //不可用DestroyImmediate
            //原因：DestroyImmediate是同步的，如果使用则会导致每次获取的都是0，无法删除，
            Destroy(this.transform.GetChild(i).gameObject);
        }*/
        gameObject.SetActive(false);
        return;
    }
}
