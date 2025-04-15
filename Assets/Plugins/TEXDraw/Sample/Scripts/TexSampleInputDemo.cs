using UnityEngine;
using UnityEngine.UI;

public class TexSampleInputDemo : MonoBehaviour
{
    public TEXInput texInput;
    public InputField uiInput;

    void Start()
    {
        uiInput.text = texInput.text;
    }

    public void UpdateFromTEXInput(string str)
    {
        uiInput.text = texInput.text;
    }

    public void UpdateFromUIInput(string str)
    {
        texInput.text = uiInput.text;
    }
}

