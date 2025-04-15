using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputTex : MonoBehaviour
{
    // Start is called before the first frame update
    public TEXDraw Long;
    public TEXDraw Short;
    public InputField inputField;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnInputFieldValueChange()
    {
        Long.text = inputField.text;
        Short.text = inputField.text;
    }
}
