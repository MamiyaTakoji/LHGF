using TexDrawLib;

using UnityEditor;
using UnityEngine;

public class TEXBoxHighlighting : ScriptableObject
{
    public static class Styles
    {
        public static GUIStyle style = new GUIStyle(EditorStyles.textArea);
        public static GUIStyle styleBg = new GUIStyle(GUI.skin.label);
        public static GUIStyle textArea = new GUIStyle(GUI.skin.textArea);

        static Styles()
        {
            styleBg.font = style.font = textArea.font = Font.CreateDynamicFontFromOSFont(
#if UNITY_EDITOR_WIN
            "Consolas" 
#else
            "Courier New"
#endif
            , textArea.fontSize);
            textArea.padding = new RectOffset(4, 4, 2, 2);
            styleBg.normal.background = EditorGUIUtility.whiteTexture;
            styleBg.padding = styleBg.margin = new RectOffset();
            styleBg.normal.textColor = Color.black;
        }
    }

    public GUIContent textContent = new GUIContent();


    public TextEditor editor;
    public int controlID;
    public string text;
    public int cursorIndex, selectIndex;
    public EditorWindow attachedWindow;


    private void OnEnable()
    {
        Undo.undoRedoPerformed += UndoRedoPerformed;

    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= UndoRedoPerformed;

    }

    private void UndoRedoPerformed()
    {
        if (editor != null)
        {
            editor.text = text;
            editor.cursorIndex = cursorIndex;
            editor.selectIndex = selectIndex;
            if (attachedWindow)
                attachedWindow.Repaint();
        }
    }

    public void SaveBackup()
    {
        text = editor.text;
        selectIndex = editor.selectIndex;
        cursorIndex = editor.cursorIndex;
    }

    private bool ShouldByPassEvent(Event ev, ref string text)
    {
        var evt = ev.type;
        if (evt == EventType.KeyDown)
        {
            if ((ev.modifiers & ~EventModifiers.Shift) != 0)
            {
                // everything except arrow keys, copy&paste, quit.
                switch (ev.keyCode)
                {
                    case KeyCode.UpArrow:
                    case KeyCode.DownArrow:
                    case KeyCode.LeftArrow:
                    case KeyCode.RightArrow:
                    case KeyCode.PageUp:
                    case KeyCode.PageDown:
                    case KeyCode.Home:
                    case KeyCode.End:
                    case KeyCode.A:
                    case KeyCode.C:
                    case KeyCode.X:
                    case KeyCode.V:
                    case KeyCode.Backspace:
                    case KeyCode.Delete:
                        return false;
                    default:
                        return true;
                }
            }
            else if (editor != null)
            {
                if (ev.keyCode == KeyCode.Tab)
                {
                    var cursorpos = editor.cursorIndex;
                    editor.MoveLineStart();
                    if (ev.shift)
                        editor.Delete();
                    else
                        editor.ReplaceSelection("\t");
                    editor.cursorIndex = cursorpos + (ev.shift ? -1 : 1);
                    ev.Use();
                    if (editor.text != text)
                    {
                        text = editor.text;
                        GUI.changed = true;
                    }
                    return true;
                }
            }
        } 
        else if (evt == EventType.KeyUp)
        {
            if ((ev.modifiers & ~EventModifiers.Shift) == 0 && ev.keyCode == KeyCode.Tab)
            {
                ev.Use();
                GUIUtility.keyboardControl = controlID;
                return true;
            }    
        }
        return false;
    }

    public (string open, string close)[] displayPairs = new(string open, string close)[]
    {
        ("{", "}"),
        ("\\begin", "\\end"),
        ("\\left", "\\right"),
        ("\\[", "\\]"),
        ("\\(", "\\)"),
    };

    public string DrawText(string text, Rect bounds)
    {
        textContent.text = text;
        var ev = Event.current;
        var evt = ev.type;
        if (evt == EventType.Layout)
            return text;
        if (ShouldByPassEvent(ev, ref text))
            return text;

        // extra space for scrollbars
        bounds.width -= GUI.skin.verticalScrollbar.fixedWidth;
        GUIUtility.GetControlID(FocusType.Passive);
        text = GUI.TextArea(bounds, text, Styles.textArea);
        controlID = GUIUtility.GetControlID(FocusType.Passive) - 1;
        var id = GUIUtility.keyboardControl;
        if (id != controlID) return text;
        editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), id);

        HandleScrolling(bounds, editor, ev, Styles.textArea);
        if (evt == EventType.Repaint)
        {

            int pos = editor.cursorIndex;

            //Find & Highlight Left Group
            int first, last, iter = 0;
            bool redrawCursor = false;

            do
            {
                var open = displayPairs[iter].open;
                var close = displayPairs[iter].close;
                first = FindBeforeGroup(text, pos, open, close);
                last = FindAfterGroup(text, pos, open, close);
                if (first >= 0)
                {
                    DrawHighlight(Styles.style, editor, text, first, first + open.Length, Color.green);
                }
                if (last < text.Length)
                {
                    DrawHighlight(Styles.style, editor, text, last, last + close.Length, Color.green);
                }
            } while (++iter < displayPairs.Length && first < 0 && last >= text.Length);


            //Do we need to redraw cursor?
            if (editor.cursorIndex != editor.selectIndex)
                redrawCursor = false;
            else
            {
                //Find & Highlight Escapes
                int res = FindPossibleEscape(text, pos, out first, out last);
                if (res > 0)
                {
                    Color scheme;
                    switch (res)
                    {
                        case 1:
                            scheme = Color.green;
                            break;
                        case 2:
                            scheme = Color.cyan;
                            break;
                        case 3:
                            scheme = new Color(0.8f, 1f, 0);
                            break;
                        default:
                            scheme = new Color(0.7f, 0.7f, 0.7f);
                            break;
                    }
                    DrawHighlight(Styles.style, editor, text, first, last + 1, scheme);
                    redrawCursor = true;
                }
            }
            if (redrawCursor)
            {
                bounds.position -= Vector2.Max(Vector2.zero, editor.scrollOffset);
                Styles.style.DrawCursor(bounds, textContent, id, editor.cursorIndex);
            }
        }

        return text;
    }

    private void HandleScrolling(Rect r, TextEditor editor, Event ev, GUIStyle style)
    {
        var contentHeight = style.CalcHeight(textContent, r.width);
        if (contentHeight < r.height) return;
        var vBarRect = new Rect(r.xMax, r.y, GUI.skin.verticalScrollbar.fixedWidth, r.height);
        float scroll = GUI.VerticalScrollbar(vBarRect, editor.scrollOffset.y, r.height, 0, contentHeight);

        if (ev.type == EventType.ScrollWheel)
        {
            float num4 = scroll + (ev.delta.y * 10f);
            scroll = Mathf.Clamp(num4, 0f, contentHeight - r.height);
            Event.current.Use();
        }
        if (editor.scrollOffset.y != scroll)
        {
            if (attachedWindow)
                attachedWindow.Repaint();
            editor.scrollOffset = new Vector2(editor.scrollOffset.x, scroll);
        }
    }


    private void DrawHighlight(GUIStyle style, TextEditor editor, string text, int charFrom, int charTo, Color tint)
    {
        string word = text.Substring(charFrom, charTo - charFrom);
        var rect = new Rect(style.GetCursorPixelPosition(editor.position, textContent, charFrom)
            , style.GetCursorPixelPosition(editor.position, textContent, charTo));
        rect.size -= rect.position;
        if (rect.height > 0)
            return;
        rect.position -= Vector2.Max(Vector2.zero, editor.scrollOffset);
        rect.height += GUI.skin.label.fontSize;
        //rect.y -= 4; // fontSize is 12

        var c = GUI.backgroundColor;
        GUI.backgroundColor = tint;
        GUI.Label(rect, word, Styles.styleBg);
        GUI.backgroundColor = c;
    }

    private static bool IsStringMatchAt(string haystack, string needle, int pos)
    {
        if (pos < 0 || pos >= haystack.Length - needle.Length)
            return false;
        for (int i = 0; i < needle.Length; i++)
        {
            if (haystack[i + pos] != needle[i])
                return false;
        }
        return true;
    }

    private static int FindBeforeGroup(string text, int pos, string open = "{", string close = "}")
    {
        pos--;
        if (text.Length <= 0 || pos < 0 || pos >= text.Length)
            return -1;
        var group = 0;
        while (pos >= 0)
        {
            if (pos > 0 && text[pos - 1] == '\\')
            {
            }
            else if (IsStringMatchAt(text, open, pos))
            {
                if (group <= 0)
                    return pos;
                else
                    group--;
            }
            else if (IsStringMatchAt(text, close, pos))
            {
                group++;
            }
            pos--;
        }
        return -1;
    }

    private static int FindAfterGroup(string text, int pos, string open = "{", string close = "}")
    {
        if (text.Length <= 0 || pos < 0 || pos >= text.Length)
            return text.Length;
        var group = 0;
        while (pos < text.Length)
        {
            if (pos > 0 && text[pos - 1] == '\\')
            {
            }
            else if (IsStringMatchAt(text, close, pos))
            {
                if (group <= 0)
                    return pos;
                else
                    group--;
            }
            else if (IsStringMatchAt(text, open, pos))
            {
                group++;
            }
            pos++;
        }
        return pos;
    }

    ///Returning: 0-Not found, 1-Symbol, 2-Command, 3-Font, 4-Misc
    private static int FindPossibleEscape(string text, int pos, out int start, out int end)
    {
        start = 0;
        end = 0;
        pos--;
        if (text.Length <= 0 || pos < 0 || pos >= text.Length)
            return 0;
        if (TexParserUtility.reservedChars.Contains(text[pos]))
        {
            if (pos == 0 || text[pos - 1] != '\\')
            {
                if (pos == 0 || text[pos] != '\\')
                    return 0;
                else if (pos < text.Length - 1)
                    pos++;
                else
                {
                    start = pos - 1;
                    end = pos;
                    return 4;
                }
            }
            else
            {
                start = pos - 1;
                end = pos;
                return 4;
            }
        }
        end = pos;
        while (true)
        {
            if (char.IsLetter(text[pos]))
            {
                pos--;
                if (pos >= 0)
                    continue;
                else
                    return 0;
            }
            if (text[pos] == '\\')
            {
                if (pos == 0 || text[pos - 1] != '\\')
                {
                    start = pos;
                    break;
                }
            }
            return 0;
        }
        while (end < text.Length && char.IsLetter(text[end]))
            end++;
        end--;
        var s = text.Substring(start + 1, end - start);
        if (TEXPreference.main.GetFontIndexByID(s) >= 0)
            return 3;
        if (TexOrchestrator.staticParser != null && (TexOrchestrator.staticParser.aliasCommands.ContainsKey(s) || TexOrchestrator.staticParser.generalCommands.ContainsKey(s)))
            return 2;
        if (TEXPreference.main.GetChar(s) != null)
            return 1;
        return 4;
    }

    public int GetCursorPos() => editor == null ? 0 : editor.cursorIndex + 1;
    public Vector2Int GetCursorPosVector()
    {
        if (editor == null) return Vector2Int.zero;
        var text = editor.text;
        var pos = Mathf.Min(editor.cursorIndex, text.Length - 1);
        while (pos >= 0 && text[pos] != '\n')
            pos--;
        var col = editor.cursorIndex - pos;
        var row = 1;
        while (pos >= 0)
        {
            if (text[pos] == '\n') row++;
            pos--;
        }
        return new Vector2Int(col, row);

    }
}
