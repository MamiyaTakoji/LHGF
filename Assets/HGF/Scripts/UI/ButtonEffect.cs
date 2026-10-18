using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//[RequireComponent(typeof(Button))]
public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum ButtonSetected
    {
        //按钮被选择时的表现形式
        Outline,
        Underline
    }
    public bool Interactable
    {
        get
        {
            if(GetComponent<Button>() != null)
            {
                return GetComponent<Button>().interactable;
            }
            else
            {
                return true;
            }
            //return GetComponent<Button>().interactable;
        }
    }
    public ButtonSetected ButtonSelectedEffect;
    public AudioSource EffectSoundPlayer;
    public AudioClip ButtonClicked;
    public AudioClip ButtonSelected;
    [ColorUsage(false, true)] public Color outlineColor = Color.red;
    public float outlineWidth = 0.2f;

    private TMP_Text[] texts;
    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
    private static readonly int UnderlayColorId = Shader.PropertyToID("_UnderlayColor");
    private static readonly int UnderlayOffsetId = Shader.PropertyToID("_UnderlayOffset");
    void Start()
    {
        EffectSoundPlayer = GameObject.Find("EffectSound").GetComponent<AudioSource>();
        if (GetComponent<Button>() != null)
        {
            GetComponent<Button>().onClick.AddListener(delegate
            {
                EffectSoundPlayer.PlayOneShot(ButtonClicked);
            });
        }
        texts = GetComponentsInChildren<TMP_Text>();
        if (ButtonSelectedEffect == ButtonSetected.Outline)
        {
            foreach (var t in texts)
            {
                t.raycastTarget = false;
                t.fontMaterial = new Material(t.fontMaterial);
                t.fontMaterial.EnableKeyword("OUTLINE_ON");    // 启用描边
                t.fontMaterial.SetFloat(OutlineWidthId, 0f);   // 初始无描边
                t.fontMaterial.EnableKeyword("UNDERLAY_ON");
            }
        }
/*        else if(ButtonSelectedEffect == ButtonSetected.Underline)
        {
            foreach (var t in texts)
            {
                t.fontMaterial = new Material(t.fontMaterial);
                t.fontMaterial.EnableKeyword("UNDERLAY_ON");
            }
        }*/

    }
/*    public void OnEnable()
    {
        if (texts == null) return;
        foreach (var t in texts)
        {
            t.fontMaterial = new Material(t.fontMaterial);
            t.fontMaterial.EnableKeyword("OUTLINE_ON");
            t.fontMaterial.SetFloat(OutlineWidthId, 0f);
        }
    }*/
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactable) { return; }
        foreach (var t in texts)
        {
            Debug.Log($"Shader: {t.fontMaterial.shader.name}");
            Debug.Log($"OUTLINE_ON enabled: {t.fontMaterial.IsKeywordEnabled("OUTLINE_ON")}");
        }
        EffectSoundPlayer.PlayOneShot(ButtonSelected);
        if(ButtonSelectedEffect == ButtonSetected.Outline)
        {
            foreach (var t in texts)
            {
                t.fontMaterial.SetFloat(OutlineWidthId, outlineWidth);
                t.fontMaterial.SetColor(OutlineColorId, outlineColor);
            }
        }
        else if(ButtonSelectedEffect == ButtonSetected.Underline)
        {
            foreach (var t in texts)
            {
                t.fontStyle |= FontStyles.Underline;
/*                t.fontMaterial.SetFloat(UnderlayOffsetId, 2f);
                t.fontMaterial.SetColor(UnderlayColorId, outlineColor);*/
            }
        }

    }

        // Update is called once per frame
        void Update()
    {
        
    }
    public void OnEnable()
    {
        if (texts == null) { return; }
        if (ButtonSelectedEffect == ButtonSetected.Outline)
        {
            foreach (var t in texts)
                t.fontMaterial.SetFloat(OutlineWidthId, 0f);
        }
        else
        {
            foreach (var t in texts)
            {
                t.fontStyle &= ~FontStyles.Underline;
                /*            t.fontMaterial.SetFloat(UnderlayOffsetId, 0f);*/
            }
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Interactable) { return; }
        if (ButtonSelectedEffect == ButtonSetected.Outline)
        {
            foreach (var t in texts)
                t.fontMaterial.SetFloat(OutlineWidthId, 0f);
        }
        else
        {
            foreach (var t in texts) { 
            t.fontStyle &= ~FontStyles.Underline;
/*            t.fontMaterial.SetFloat(UnderlayOffsetId, 0f);*/
        }
        }
    }
}
