using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TexSampleSimpleNote : MonoBehaviour
{
    public string notesFormat;
    public TEXDraw output;
    public InputField input;
    public Scrollbar scroll;

    [System.NonSerializedAttribute]
    private List<string> savedNotes = new List<string>();

    // Use this for initialization
    private void OnEnable()
    {
        savedNotes.Clear();
        savedNotes.AddRange(PlayerPrefs.GetString("TexSample_Note", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        RewriteOutput();
    }

    private void OnDisable()
    {
        PlayerPrefs.SetString("TexSample_Note", string.Join("\n", savedNotes.ToArray()));
    }

    private void RewriteOutput()
    {
        var s = new StringBuilder();
        for (int i = 0; i < savedNotes.Count; i++)
        {
            s.AppendLine(string.Format(notesFormat, savedNotes[i], i));
        }
        output.text = s.ToString();
    }

    public void DeleteText(string index)
    {
        var idx = int.Parse(index);
        savedNotes.RemoveAt(idx);
        RewriteOutput();
    }

    // Update is called once per frame
    public void SubmitText(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return;
        txt = StabilizeBracesCount(txt);
        var str = output.text;
        output.text = str + string.Format(notesFormat, txt, savedNotes.Count) + "\n";
        savedNotes.Add(txt);

        input.text = string.Empty;
        // scroll.value = 0;
        StartCoroutine(focus());
    }

    private static string StabilizeBracesCount(string src)
    {
        int l = 0, r = 0;
        for (int i = 0; i < src.Length; i++)
        {
            if (src[i] == '{')
                l++;
            else if (src[i] == '}')
                r++;
        }
        if (r > l)
        {
            return new string('{', r - l) + src;
        }
        else if (r < l)
            return src + new string('}', l - r);
        else
            return src;
    }

    private IEnumerator focus()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        EventSystem.current.SetSelectedGameObject(input.gameObject);
    }

    public void Import()
    {
        var str = GUIUtility.systemCopyBuffer;
        if (string.IsNullOrEmpty(str))
            return;
        savedNotes.AddRange(str.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        RewriteOutput();
    }

    public void Export()
    {
        GUIUtility.systemCopyBuffer = string.Join("\n", savedNotes.ToArray());
    }

    public void Clean()
    {
        savedNotes.Clear();
        RewriteOutput();
    }

    public void ZoomIn()
    {
        output.size = Mathf.Min(output.size + 2, 60);
    }

    public void ZoomOut()
    {
        output.size = Mathf.Max(output.size - 2, 8);
    }
}
