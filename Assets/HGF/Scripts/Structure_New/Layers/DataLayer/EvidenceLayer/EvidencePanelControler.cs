using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LHGFData;

public class EvidencePanelControler : MonoBehaviour
{
    public Button ChapterButtonPerfab;
    public GameObject SectionSelect;
    public Button EvidenceButtonPerfab;
    public GameObject EvidenceSelect;

    //下面的内容关于EvidenceContent
    public TMP_Text EvidenceContent;
    public TMP_Text EvidenceAbstract;
    public List<GameObject> CiteSymbols;
    public VerticalLayoutGroup EvidenceContentGroups;
    public Color ClickButtonColor;



    public Button CloseButton;
    //出示证据时，首先要激活游戏菜单
    //这里有一个很蛋疼的情况，通过按钮激活菜单后，
    //是通过按钮禁用快捷键的，而这里是通过游戏流程禁用的。
    public GameObject GameMenu;

    public Dictionary<string, Button> ChapterButtons = new() { };
    public Dictionary<string, Button> EvidenceButtons = new() { };
    public EvidenceLayer data;
    public EvidenceChoiceLayer choiceData;
    public Dictionary<string, Dictionary<string, string>> choiceEvidences;

    public Button ShowEvidenceButton;
    public Button SkipButton;

    private string CurrentChapter;
    private string CurrentEvidence;
    public MenuState CurrentMenuState;

    public Dictionary<string, Dictionary<string, string>> chapterEvidenceNextIdPair = new() { };
    public string DefaultNextId;
    public string RightNextId;
    public string RightEvidence;
    public enum MenuState
    {
        Review,
        Show
    }
    public void Start()
    {
        CloseButton.onClick.AddListener(delegate
        {
            //清除所有证据按钮
            foreach (var button in EvidenceButtons.Values)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            //关闭EvidenceContentGroups
            CloseEvidenceContent();
            gameObject.SetActive(false);
            GameMenu.SetActive(false);
            LHGFGameMain.instance.MainScence.gameObject.SetActive(true);
        });
    }

    public void OnEnable()
    {
        if(CurrentMenuState == MenuState.Review)
        {
            ShowEvidenceButton.gameObject.SetActive(false);
            SkipButton.gameObject.SetActive(false);
        }
        foreach (var g in CiteSymbols)
        {
            g.SetActive(false);
        }
    }
    public void ResetEvidencePanel()
    {
        // 销毁章节按钮
        foreach (var button in ChapterButtons.Values)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        ChapterButtons.Clear();

        // 销毁证据按钮
        foreach (var button in EvidenceButtons.Values)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        EvidenceButtons.Clear();

        // 清空证据内容文本
        CloseEvidenceContent();
    }
    //证据面板被设置时，没有任何章节按钮被点击，因此证据名那里无内容
    public void SetEvidencePanel(Dictionary<string, Dictionary<string, string>> Evidences)
    { 
        //外层字典的key为章节名称，内层字典的key为证据名称，value为证据内容
        foreach (var chapter in Evidences)
        {
            Button chapterButton = Instantiate(ChapterButtonPerfab, SectionSelect.transform);
            chapterButton.GetComponentInChildren<TMP_Text>().text = chapter.Key;
            ChapterButtons[chapter.Key] = chapterButton;
            string chapterName = chapter.Key;
            chapterButton.onClick.AddListener(delegate
            {
                OnChapterButtonClick(chapterName);
            });
        }
        CloseEvidenceContent();
    }
    //章节按钮被点击时，显示该章节下的所有证据按钮
    public void OnChapterButtonClick(string chapterName)
    {
        var value = data.CurrentEvidences;
        var evidences = value[chapterName];
        //首先清除所有证据按钮
        foreach (var button in EvidenceButtons.Values)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        EvidenceButtons.Clear();
        foreach (var evidence in evidences)
        {
            Button evidenceButton = Instantiate(EvidenceButtonPerfab, EvidenceSelect.transform);
            evidenceButton.name = evidence.Key;

            // 创建局部变量捕获当前迭代的证据名称
            string evidenceName = evidence.Key;
            evidenceButton.onClick.AddListener(delegate
            {
                OnEvidenceButtonClick(evidenceName);
                evidenceButton.transform.Find("1").gameObject.SetActive(true);
                evidenceButton.GetComponent<Image>().color = ClickButtonColor;
            });

            evidenceButton.GetComponentInChildren<TMP_Text>().text = evidence.Key;
            EvidenceButtons[evidence.Key] = evidenceButton;
        }
        CurrentChapter = chapterName;
    }
    public void OnEvidenceButtonClick(string evidenceName)
    {
        var value = data.CurrentEvidences;
        var evidences = value[CurrentChapter];
        var temp = evidences[evidenceName].Split(data.Spliter);
        string evidenceContent = temp[0];
        string evidenceAbstract = temp[1];
        SetEvidenceContent(evidenceContent, evidenceAbstract);
        //EvidenceContent.text = evidence;
        CurrentEvidence = evidenceName;
        //被选中的按钮和未被选中的按钮的样式不同
        //先把所有按钮的样式设置为未选中
        foreach (var button in EvidenceButtons.Values)
        {
            button.GetComponent<Image>().color = Color.white;
            button.transform.Find("1").gameObject.SetActive(false);
        }

        if(CurrentMenuState == MenuState.Show)
        {
            string NextId = DefaultNextId;
            ShowEvidenceButton.gameObject.SetActive(true);
            SkipButton.gameObject.SetActive(true);
            if (choiceEvidences.ContainsKey(CurrentChapter))
            {
                var Dic = choiceEvidences[CurrentChapter];
                if (Dic.ContainsKey(CurrentEvidence))
                {
                    NextId = Dic[evidenceName];
                }
            }
            ShowEvidenceButton.onClick.RemoveAllListeners();
            ShowEvidenceButton.onClick.AddListener(delegate
            {
                Forward(NextId, evidenceName);
                GameMenu.SetActive(false);
                ActiveEviddencePanel(MenuState.Review);
            });
            SkipButton.onClick.RemoveAllListeners();
           SkipButton.onClick.AddListener(delegate
            {
                Forward(RightNextId, RightEvidence);
                GameMenu.SetActive(false);
                ActiveEviddencePanel(MenuState.Review);
            });
        }
    }
    public void AddEvidence(EvidenceLayer.Evidence evidence)
    {
        //如果是未出现过的章节，则要先添加该章节
        if (!ChapterButtons.ContainsKey(evidence.chName))
        {
            Button chapterButton = Instantiate(ChapterButtonPerfab, SectionSelect.transform);
            chapterButton.GetComponentInChildren<TMP_Text>().text = evidence.chName;
            ChapterButtons[evidence.chName] = chapterButton;
            string chapterName = evidence.chName;
            chapterButton.onClick.AddListener(delegate
            {
                OnChapterButtonClick(chapterName);
            });
        }
        //新证据加入后，用户打开时显示最新证据
        //暂时不做吧...
/*        CurrentMenuState = MenuState.Show;
        OnChapterButtonClick(evidence.chName);
        OnEvidenceButtonClick(evidence.eviName);*/
    }
    public void Forward(string NextNodeId, string evidenceName)
    {
        choiceData.IsChoiceSelected = true;
        LHGFGameMain.instance.gameProgress.NextNodeId = NextNodeId;
        LHGFGameMain.instance.gameProgress.Skip(LHGFGameMain.instance.gameData);
        LHGFGameMain.instance.ContentLogerControler.LogContent
        (
            choiceData.SetLogInfo(evidenceName)
        );
        LHGFGameMain.instance.Forward();
    }
    public void ActiveEviddencePanel(MenuState menuState)
    {
        //首先切换模式，如果是Show模式，则禁用关闭按钮
        CurrentMenuState = menuState;
        if (menuState == MenuState.Show)
        {
            CloseButton.gameObject.SetActive(false);
        }
        else
        {
            CloseButton.gameObject.SetActive(true);
        }
    }
    public void SetEvidenceContent(string EvidenceOriginalText, string EvidenceAbstractText)
    {
        EvidenceContentGroups.gameObject.SetActive(true);
        EvidenceContent.text = EvidenceOriginalText; 
        EvidenceAbstract.text = EvidenceAbstractText;
        foreach(var g in CiteSymbols)
        {
            g.SetActive(true);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(EvidenceContentGroups.GetComponent<RectTransform>());
    }
    public void CloseEvidenceContent()
    {
        EvidenceContentGroups.gameObject.SetActive(false);
    }
}
