using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

public partial class TEXInput : Selectable, IUpdateSelectedHandler, IDragHandler
{
    private void Append(char ch)
    {
        var s = new string(ch, 1);
        selectedText = s;
        selectionBegin += s.Length;
        selectionReversed = false;
    }

    readonly static char[] brackets = new char[] { '{', '}' };

    private void Backspace()
    {
        if (hasSelection)
        {
            selectedText = "";
            selectionBegin = selectionStart;
        }
        else if (selectionStart > 0)
        {
            text = text.Remove(selectionBegin - 1, 1);
            selectionBegin--;
        }
    }

    private void Delete()
    {
        if (hasSelection)
        {
            selectedText = "";
            selectionBegin = selectionStart;
        }
        else if (selectionStart < text.Length)
        {
            text = text.Remove(selectionBegin, 1);
        }
    }

    private void Move(bool shift, Func<int, int> mover)
    {
        var x = shift | hasSelection ? selectionEnd : selectionBegin;

        if (shift)
            selectionEnd = mover(x);
        else
            selectionBegin = mover(x);
    }

    private int MoveLeft(int x)
    {
        return Math.Max(x - 1, 0);
    }

    private int MoveRight(int x)
    {
        return Math.Min(x + 1, text.Length);
    }

    private int MoveUp(int x)
    {
        var up = text.Take(x);
        var line = up.Count(c => c == '\n');
        if (line == 0) return x;

        var lines = up.Select((c, i) => new { c, i }).Where(cc => cc.c == '\n').Select(cc => cc.i);
        var dist = x - lines.LastOrDefault();
        var pos = lines.ElementAtOrDefault(line - 2) + dist;
        return pos;
    }

    private int MoveDown(int x)
    {
        var down = text.Skip(x);
        var line = down.Count(c => c == '\n');
        if (line == 0) return x;

        var lines = text.Take(x).Select((c, i) => new { c, i }).Where(cc => cc.c == '\n').Select(cc => cc.i);
        var dist = x - (lines.LastOrDefault());
        var pos = x + down.Select((c, i) => new { c, i }).Where(cc => cc.c == '\n').First().i + dist;
        return pos;
    }

    private struct UndoState
    {
        public string text;
        int selectionStart;
        int selectionLength;

        public UndoState(TEXInput input)
        {
            text = input.text;
            selectionStart = input.selectionStart;
            selectionLength = input.selectionLength;
        }

        public void Apply(TEXInput input)
        {
            input.text = text;
            input.selectionStart = selectionStart;
            input.selectionLength = selectionLength;
        }

        public static bool IsEqual(UndoState l, UndoState r)
        {
            return l.text == r.text && l.selectionLength == r.selectionLength
                && l.selectionStart == r.selectionStart;
        }
    }

    [NonSerialized]
    private List<UndoState> undoStack = new List<UndoState>();

    /// <summary>
    /// Record undo
    /// </summary>
    public void RecordUndo()
    {
        var st = new UndoState(this);
        if (undoStack.Count == 0 || !UndoState.IsEqual(st, undoStack[undoStack.Count - 1]))
        {
            undoStack.Add(st);
            // maybe the cap is too subjective
            if (undoStack.Count > 15)
                undoStack.RemoveAt(0);
        }
    }

    private bool Undo()
    {
        if (undoStack.Count > 0)
        {
            var now = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);

            if (now.text == text)
                return Undo();
            else
                now.Apply(this);
            return true;
        }
        return false;
    }

    private static Regex _escaper = new Regex(@"([\\{}^_])");

    public static string Escape(string s)
    {
        return _escaper.Replace(s, "\\$1");
    }

    public static string Escape(char s)
    {
        return
        // TexFormulaParser.IsParserReserved(s) ? "\\" + s :
        new string(s, 1);
    }

    static readonly Regex _commandPattern = new Regex(@"(?:\\[A-Za-z]+|[\^_]{1,3})?(?:\[[^\[\]]*?\])?$");
    static readonly Regex _commandPattern2 = new Regex(@"\G(?:\\[A-Za-z]+|[\^_]{1,3})?(?:\[[^\[\]]*?\])?{");

    private bool AttemptToMatchBrackets(bool backspace)
    {
        CheckSelection();

        if (backspace && selectionStart > 0)
        {
            var c = text[selectionStart - 1];
            if (c == '{')
            {
                var m = LookupBracePairAhead(selectionStart - 1);
                if (m != selectionStart - 1)
                {
                    var s = selectionStart > 1 ? _commandPattern.Match(text, 0, selectionStart - 1).Length : 0;
                    var d = m - selectionStart;
                    selectionBegin -= s + 1;
                    selectionLength = d + 1 + s;
                }
                else
                {
                    selectionBegin--;
                    text = text.Remove(selectionBegin, 1);
                }
                return true;
            }
            else if (c == '}')
            {
                var m = LookupBracePairBehind(selectionStart - 1);
                if (m != selectionStart - 1)
                {
                    var s = m > 0 ? _commandPattern.Match(text, 0, m).Length : 0;
                    var d = selectionStart - m;
                    selectionBegin -= d + s;
                    selectionLength = d + s;
                }
                else
                {
                    selectionBegin--;
                    text = text.Remove(selectionBegin, 1);
                }
                return true;
            }
        }
        else if (!backspace && selectionStart < text.Length)
        {
            var c = text[selectionStart];
            if (c == '}')
            {
                var m = LookupBracePairBehind(selectionStart);
                if (m != selectionStart)
                {
                    var s = m > 0 ? _commandPattern.Match(text, 0, m).Length : 0;
                    var d = selectionStart - m;
                    selectionBegin -= d + s;
                    selectionLength = d + s + 1;
                }
                else
                {
                    text = text.Remove(selectionBegin, 1);
                }
                return true;
            }
            else if (c == '{' || c == '\\' || c == '^' || c == '_')
            {
                var q = 0;
                if (c != '{')
                {
                    var g = _commandPattern2.Match(text, selectionStart);
                    if (!g.Success) return false;
                    q = g.Length - 1;
                }
                var m = LookupBracePairAhead(selectionStart + q);
                if (m != selectionStart + q)
                {
                    var s = selectionStart > 2 ? _commandPattern.Match(text, 0, selectionStart - 1).Length : 0;
                    var d = m - selectionStart;
                    selectionBegin -= s;
                    selectionLength = d + s;
                }
                else
                {
                    text = text.Remove(selectionBegin, 1);
                }
                return true;
            }
        }
        return false;
    }

    private int LookupBracePairAhead(int position)
    {
        var t = text;
        var start = position;
        var shift = 0;
        do
        {
            if (position >= t.Length) return start;
            var c = t[position++];
            if (c == '\\') position++;
            else if (c == '{') shift++;
            else if (c == '}') shift--;
            else if (c == '\n') return start;
        } while (shift != 0);
        return position;
    }

    private int LookupBracePairBehind(int position)
    {
        var t = text;
        var start = position;
        var shift = 0;
        do
        {
            if (position < 0) return start;
            var c = t[position--];
            if (c == '\n') return start;
            else if (position >= 0 && t[position] == '\\')
            {
                var cont = 0;
                while (position > 0 && t[--position] == '\\')
                    cont++;
                if (t[position] == '\n')
                    return start;
                if (cont % 2 == 1) continue;
            }
            else if (c == '{') shift++;
            else if (c == '}') shift--;
        } while (shift != 0);
        return position + 1;
    }

}

