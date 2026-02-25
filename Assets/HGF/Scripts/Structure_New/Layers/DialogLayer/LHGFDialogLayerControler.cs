using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHGFData;
using TMPro;
using static LHGFData.DialogLayer;
public class LHGFDialogLayerControler : MonoBehaviour
{
    public GameObject TMPContent;
    public GameObject TexDrawContent;
    public DialogLayer dialogLayer;
    public TMP_Text SpeakerInfo;
    public void Speak(SpeakData speakData, bool IsOnLoad = false)
    {
        TMPContent.GetComponent<TMP_Text>().text = "";
        TexDrawContent.GetComponent<TEXDraw>().text = "";
        bool IsSkip = speakData.Skip;
        if (IsOnLoad) { IsSkip = true; }
        if(speakData.TextType == "TMP")
        {
            dialogLayer.TextTweener = TMPContent.GetComponent<TextTweener>().SetTextTweener(speakData.Content, IsSkip);
        }
        else
        {
            dialogLayer.TextTweener = TexDrawContent.GetComponent<TextTweener>().SetTextTweener(speakData.Content, IsSkip);
        }
        string SpeakerName = speakData.SpeakerName;
        string DepartmentName = speakData.DepartmentName;
        SpeakerInfoWrapper(SpeakerName, DepartmentName);
    }
    public void SpeakerInfoWrapper(string SpeakerName, string DepartmentName)
    {
        SpeakerInfo.GetComponent<TMP_Text>().text =
            $"<b><size=+120%>{SpeakerName}</size></b>    <color=#F684EE>{DepartmentName}</color>";
    }
}
