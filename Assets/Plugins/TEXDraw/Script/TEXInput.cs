using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[AddComponentMenu("TEXDraw/TEXInput"), SelectionBase]
public partial class TEXInput : Selectable, IUpdateSelectedHandler, IDragHandler
{

    /// <summary>
    /// Starting range of selection. Changing this require texdraw.SetTextDirty() to be called
    /// </summary>
#if !TEXDRAW_DEBUG
   [HideInInspector]
#endif
    public int selectionStart;

    /// <summary>
    /// Length range of selection. Changing this require texdraw.SetTextDirty() to be called
    /// </summary>
#if !TEXDRAW_DEBUG
   [HideInInspector]
#endif
    public int selectionLength;

    [NonSerialized]
    public bool selectionReversed;

    /// <summary>
    /// Internal selection, refer when 'MouseDown'
    /// </summary>
    protected int selectionBegin
    {
        get
        {
            return selectionReversed ? selectionStart + selectionLength : selectionStart;
        }
        set
        {
            selectionStart = value;
            // this is beginning, right? so...
            selectionLength = 0;
            selectionReversed = false;
            cursor.hotState = false;
            PropagateCursor();
        }
    }

    /// <summary>
    /// Internal selection, refer during 'MouseMove'
    /// </summary>
    protected int selectionEnd
    {
        get
        {
            return selectionReversed ? selectionStart : selectionStart + selectionLength;
        }
        set
        {
            if ((value >= selectionBegin) ^ (selectionEnd >= selectionBegin))
                selectionReversed = !selectionReversed;

            if (selectionReversed)
            {
                selectionLength = selectionBegin - value;
                selectionStart = value;
            }
            else
            {
                selectionLength = value - selectionStart;
            }
            cursor.hotState = selectionLength > 0;
            PropagateCursor();
        }
    }

    /// <summary>
    /// Is the content can't be modified by user?
    /// </summary>
    [Tooltip("Is the content can't be modified?")]
    public bool readOnly = false;

    [SerializeField]
    private TEXInputChangeEvent m_OnChange = new TEXInputChangeEvent();

    [SerializeField, TextArea(5, 10)]
    private string m_Text = "TEXDraw";

    [SerializeField]
    private TEXDraw m_TEXDraw = null;

    [SerializeField]
    private TEXInputCursor m_Cursor = null;

    [SerializeField]
    private TEXInputLogger m_Logger = null;

    /// <summary>
    /// Event to whenever the text has changed by user
    /// </summary>
    public TEXInputChangeEvent onChange
    {
        get { return m_OnChange; }
        set { m_OnChange = value; }
    }

    /// <summary>
    /// Text that displayed, which is exact same as TEXDraw
    /// </summary>
    public string text
    {
        get => m_Text; set
        {
            m_Text = value;
            PropagateText();
            PropagateCursor();
        }
    }

    /// <summary>
    /// Portion of text that displayed
    /// </summary>
    public string selectedText
    {
        get
        {
            CheckSelection();
            return text.Substring(selectionStart, selectionLength);
        }
        set
        {
            CheckSelection();
            var start = selectionStart;
            var end = selectionLength;
            selectionLength = start + value.Length;
            selectionReversed = false;
            text = text.Substring(0, start) + value + text.Substring(start + end);
        }
    }

    /// <summary>
    /// Is selectionLength > 0?
    /// </summary>
    public bool hasSelection => selectionLength > 0;

    internal TEXInputCursor cursor => m_Cursor;

    internal TEXInputLogger logger => m_Logger;

    internal TEXDraw tex => m_TEXDraw;

    private bool allowInput = false;

    /// <summary>
    /// Does the Event System put its focus to this input?
    /// </summary>
    public bool hasFocus => allowInput;

    private Event m_ProcessingEvent = new Event();

    private void CheckSelection()
    {
        if (selectionStart + selectionLength > text.Length)
        {
            selectionStart = Math.Min(text.Length, selectionStart);
            selectionLength = Math.Min(text.Length, selectionStart + selectionLength) - selectionStart;
        }
    }

    private void PropagateText()
    {
        tex.text = logger.ReplaceString(text);
    }

    private void PropagateCursor()
    {
        cursor.SetVerticesDirty();
    }


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        PropagateText();
    }

    protected override void Reset()
    {
        base.Reset();
        transition = Transition.None;
    }
#endif

    [Serializable]
    internal struct Block
    {
        public int index;
        public int start;
        public int length;
        public int group;

        public override string ToString()
        {
            return index + ": " + start + "-" + length;
        }

        public int end => start + length;

        public bool isInside(int pos) => pos > start && pos - start <= length;
    }
}

[Serializable]
public class TEXInputChangeEvent : UnityEvent<string>
{
}
