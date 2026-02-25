using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LHGFData;
public class LHGFImageBgLayerControler : MonoBehaviour
{
    public Image _background;
    public Image defaultBackGround;
    public Image background
    {
        get
        {
            if(_background == null)
            {
                _background = GetComponent<Image>();
            }
            return _background;
        }
        set
        {
            _background = value;
        }
    }
    public ImageBgLayer bgLayer;
    public float ImageBgHight
    {
        get 
        {
            //º«µ√…æ
            /*            gameObject.AddComponent<HorizontalLayoutGroup>();
                        gameObject.GetComponent<HorizontalLayoutGroup>().childControlWidth = true;
                        gameObject.transform.parent = null;*/
            //gameObject.GetComponent<RectTransform>().anchoredPosition3D.z
            gameObject.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 90, 0);
            //
            return background.GetComponent<RectTransform>().sizeDelta.y;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void _Reset()
    {
        background.sprite = null;
        background.color = Color.gray;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
