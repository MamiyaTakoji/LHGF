using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceButtonControler : MonoBehaviour
{
    public string JumpID;
    public TextComponent textComponent;
    // Start is called before the first frame update
    public void Init(string _JumpID, string Title, string TextType)
    {
        JumpID = _JumpID;
        textComponent.current_Text_Type = TextType;
        textComponent.text = Title;
    }
    /// <summary>
    /// 当玩家按下了选项
    /// </summary>

    public void Button_Click_JumpTo()
    {
        GameMain.instance.gameData.PlotData.SetNowJumpID(JumpID);
        GameMain.instance.gameData.PlotData.IsBranch = true;

        if (JumpID == "-1")
        {
            return;
        }

        //this.gameObject.transform.parent.GetComponent<SetChoice>().Button_Click_Choice();
        //GameObject.Find("EventSystem").GetComponent<MyGalManager>().IsShowingChioce = false;
        //GameObject.Find("EventSystem").GetComponent<GalManager>().Button_Click_NextPlot();
        return;
    }
}
