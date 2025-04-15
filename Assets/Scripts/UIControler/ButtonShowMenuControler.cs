using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonShowMenuControler : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject MenuGameObject;
    private bool IsMenuShowed = false;
    public SLPanelControler SLPanel;
    public SettingControler SettingPanel;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(
            delegate
            {
                if (IsMenuShowed)
                {
                    ClosePanel();
                }
                else
                {
                    ShowPanel();
                }
            }
            );
        GameObject SaveButton = MenuGameObject.transform.Find("Save").gameObject;
        SaveButton.GetComponent<Button>().onClick.AddListener(
            delegate { 
                SetSavePanel();
                SLPanel.ShowCurrentPage();
            }
            );
        GameObject LoadButton = MenuGameObject.transform.Find("Load").gameObject;
        LoadButton.GetComponent<Button>().onClick.AddListener(
            delegate {
                SetLoadPanel();
                SLPanel.ShowCurrentPage();
            }
            );
        GameObject SettingButton = MenuGameObject.transform.Find("Setting").gameObject;
        SettingButton.GetComponent<Button>().onClick.AddListener(
            delegate
            {
                SetSettingPanel();
            }
            );
        GameObject Back2MainMenuButton = MenuGameObject.transform.Find("Back2MainMenu").gameObject;
        Back2MainMenuButton.GetComponent<Button>().onClick.AddListener(
            delegate
            {
                GameMain.instance.gal.returnMainMenu();
            }
            );
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ClosePanel()
    {
        GetComponent<Button>().interactable = false;
        float parentHeight = MenuGameObject.GetComponent<RectTransform>().rect.height;

        // 设置子对象的锚点为父对象顶部中心
        var p = MenuGameObject.GetComponent<RectTransform>().anchoredPosition;
        p.y = p.y + parentHeight;
        //MenuGameObject.GetComponent<RectTransform>().anchoredPosition = p;
/*        MenuGameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
        MenuGameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f); // 轴心在底部中心

        // 计算目标位置：父对象顶部 + 子对象自身高度的一半（可选偏移）
        float offset = 10f; // 可根据需求调整间距
        float targetY = parentHeight * 0.5f + MenuGameObject.GetComponent<RectTransform>().rect.height * 0.5f + offset;*/

        // 移动子对象到正上方（使用DOTween动画）
        MenuGameObject.GetComponent<RectTransform>().DOAnchorPosY(p.y, 0.2f).SetEase(Ease.OutQuad).OnComplete(
            () => { IsMenuShowed = false; GetComponent<Button>().interactable = true; }
            ); ;
    }
    void ShowPanel()
    {
        GetComponent<Button>().interactable = false;
        float parentHeight = MenuGameObject.GetComponent<RectTransform>().rect.height;

        // 设置子对象的锚点为父对象顶部中心
        var p = MenuGameObject.GetComponent<RectTransform>().anchoredPosition;
        p.y = p.y - parentHeight;
        //MenuGameObject.GetComponent<RectTransform>().anchoredPosition = p;
        /*        MenuGameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
                MenuGameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f); // 轴心在底部中心

                // 计算目标位置：父对象顶部 + 子对象自身高度的一半（可选偏移）
                float offset = 10f; // 可根据需求调整间距
                float targetY = parentHeight * 0.5f + MenuGameObject.GetComponent<RectTransform>().rect.height * 0.5f + offset;*/

        // 移动子对象到正下方（使用DOTween动画）
        MenuGameObject.GetComponent<RectTransform>().DOAnchorPosY(p.y, 0.2f).SetEase(Ease.OutQuad).OnComplete(
            () => { IsMenuShowed = true; GetComponent<Button>().interactable = true; }
            );
    }
    void SetSavePanel()
    {
        SLPanel.gameObject.SetActive(true);
        SLPanel.CurrentModel = "Save";
        SLPanel.ShowCurrentState.GetComponent<TMP_Text>().text = "正在存档";
    }
    void SetLoadPanel()
    {
        SLPanel.gameObject.SetActive(true);
        SLPanel.CurrentModel = "Load";
        SLPanel.ShowCurrentState.GetComponent<TMP_Text>().text = "正在读档";
    }
    void SetSettingPanel()
    {
        SettingPanel.gameObject.SetActive(true);
        SettingPanel.SetPanel();
    }
}
