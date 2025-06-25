using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewControler : MonoBehaviour
{
    // Start is called before the first frame update
    public Button Switcher;
    public TEXDraw Long;
    public TEXDraw Short;
    public InputFieldControler fieldControler;
    public string TexDrawType = "Long";
    void Start()
    {
        Switcher.onClick.AddListener(delegate { Switch(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Switch()
    {
        if(TexDrawType == "Long")
        {
            TexDrawType = "Short";
            fieldControler.tEX = Short;
            Long.text = string.Empty;
        }
        else
        {
            TexDrawType = "Long";
            fieldControler.tEX = Long;
            Short.text = string.Empty;
        }
    }
}
