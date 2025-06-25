using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetChoice : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject Choice;
    void Start()
    {
        Choice = Resources.Load<GameObject>("HGF/Button-Choice");
    }
    public void SetChoiceButton(string ChoiceID, string ChoiceContent, string ChoiceContentType)
    {
        GameObject Button = Instantiate(Choice, transform);
        Button.GetComponent<ChoiceButtonControler>().Init(ChoiceID, ChoiceContent, ChoiceContentType);
        Button.name = ChoiceID;
        Button.GetComponent<Button>().onClick.AddListener(
            delegate
            {
                GameMain.instance.gameProgress.OnChoiceSelected(Button.name);
                Button_Click_Choice();
            }
            );
    }

    // Update is called once per frame
    public void Button_Click_Choice()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            //不可用DestroyImmediate
            //原因：DestroyImmediate是同步的，如果使用则会导致每次获取的都是0，无法删除，
            Destroy(this.transform.GetChild(i).gameObject);
        }
        return;
    }
}
