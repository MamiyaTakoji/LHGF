using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceButtonControler : MonoBehaviour
{
    public string JumpID;
    // Start is called before the first frame update
    public void Init(string _JumpID, string Title, string TextType)
    {
        JumpID = _JumpID;
        if (TextType == "TMP")
        {
            var textComponent = this.GetComponentInChildren<TMPro.TMP_Text>();
            textComponent.text = Title;
        }
        else
        {
            var textComponent = this.GetComponentInChildren<TEXDraw>();
            textComponent.text = Title;
        }
    }
}
