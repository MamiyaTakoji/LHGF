using LHGFData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanelControler : MonoBehaviour
{
    public GameObject CharacterButtonContainer;
    public Button CloseButton;
    public Button CharacterButtonPerfab;
    public Image CharacterImage;
    public TMP_Text CharacterName;
    public TMP_Text CharacterDescription;
    public GameObject GameMenu;
    public CharacterInfoLayer data;
    public Dictionary<string, Button> buttons = new() { };
    public Dictionary<string, CharacterInfoLayer.CharacterInfoData> cidData = new() { };
    void Start()
    {
        CloseButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            GameMenu.SetActive(false);
            LHGFGameMain.instance.MainScence.gameObject.SetActive(true);
        });
        //buttons = new() { };
        //cidData = new() { };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetButtons()
    {
        //首先初始化设置
        Clear();
        foreach (var cid in data.CharacterInfoDataList)
        {
            var button = Instantiate(CharacterButtonPerfab, CharacterButtonContainer.transform);
            button.transform.Find("ButtonName").GetComponent<TMP_Text>().text = cid.Value.CharacterName;
            button.onClick.AddListener(() => OnCharacterButtonClick(cid.Value));
            buttons.Add(cid.Value.CharacterName, button);
            cidData.Add(cid.Value.CharacterName, cid.Value);
        }
    }
    public void Clear()
    {
        foreach (var button in buttons)
        {
            Destroy(button.Value.gameObject);
        }
        buttons.Clear();
        cidData.Clear();
        CharacterImage.gameObject.SetActive(false);
        CharacterName.text = "";
        CharacterDescription.text = "";
    }
    public void OnCharacterButtonClick(CharacterInfoLayer.CharacterInfoData cid)
    { 
        CharacterImage.gameObject.SetActive(true);
        var Texture = Utils.LoadTextureByIO(cid.CharacterImagePath);
        CharacterImage.sprite = Texture;
        CharacterName.text = cid.CharacterName;
        CharacterDescription.text = cid.CharacterDescription;
    }
}
