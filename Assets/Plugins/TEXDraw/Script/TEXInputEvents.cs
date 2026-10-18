using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public partial class TEXInput : Selectable, IUpdateSelectedHandler, IDragHandler
{

    // EVENTS =========

    public override void OnSelect(BaseEventData eventData)
    {
        allowInput = true;
        base.OnSelect(eventData);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        allowInput = false;
        base.OnDeselect(eventData);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        MousePointed(true, eventData.position);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        MousePointed(false, eventData.position);
        base.OnPointerUp(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MousePointed(false, eventData.position);
    }

    public virtual void OnUpdateSelected(BaseEventData eventData)
    {

        if (allowInput && interactable)
        {
            var lastText = text;
            while (Event.PopEvent(m_ProcessingEvent))
            {
                EventType type = m_ProcessingEvent.type;
                if (m_ProcessingEvent.rawType == EventType.KeyDown)
                {
                    if (!KeyPressed(m_ProcessingEvent))
                    {
                        allowInput = false;
                        var x = FindSelectableOnRight() ?? FindSelectableOnDown();
                        if (x)
                            EventSystem.current.SetSelectedGameObject(x.gameObject);
                        break;
                    }
                    else
                    {
                        cursor.hotState = cursor.hotState;
                        tex.SetTextDirty();
                    }
                }
                else if (type == EventType.ValidateCommand || type == EventType.ExecuteCommand)
                {
                    string commandName = m_ProcessingEvent.commandName;
                    switch (commandName)
                    {
                        case "SelectAll":
                            selectionBegin = 0;
                            selectionEnd = text.Length;
                            tex.SetTextDirty();
                            break;
                        case "UndoRedoPerformed":
                            if (Undo())
                                tex.SetTextDirty();
                            break;
                        default:
                            break;
                    }
                }
            }
            if (text != lastText)
            {
                m_OnChange.Invoke(text);
            }
            eventData.Use();
        }
    }

    public void MousePointed(bool begin, Vector2 p)
    {
        if (logger.blocks.Count == 0) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, p, null, out p);

        var x = logger.GetNearestPosition(p);

        if (begin)
            selectionBegin = x;
        else
            selectionEnd = x;

        tex.SetTextDirty();
    }

    // true if continue
    protected bool KeyPressed(Event evt)
    {
        var shift = evt.shift;
        if (evt.control | evt.command)
        {
            switch (evt.keyCode)
            {
                case KeyCode.A:
                    RecordUndo();
                    selectionBegin = 0;
                    selectionEnd = selectionLength;
                    return true;
                case KeyCode.C:
                    GUIUtility.systemCopyBuffer = selectedText;
                    return true;
                case KeyCode.X:
                    GUIUtility.systemCopyBuffer = selectedText;
                    if (!readOnly)
                    {
                        RecordUndo();
                        selectedText = "";
                    }
                    return true;
                case KeyCode.V:
                    if (!readOnly)
                    {
                        RecordUndo();
                        selectedText = GUIUtility.systemCopyBuffer;
                    }
                    return true;
                default:
                    return true;
            }
        }
        switch (evt.keyCode)
        {
            case KeyCode.Backspace:
                if (!readOnly)
                {
                    RecordUndo();
                    Backspace();
                }
                return true;
            case KeyCode.Delete:
                if (!readOnly)
                {
                    RecordUndo();
                    Delete();
                }
                return true;
            case KeyCode.LeftArrow:
                Move(shift, MoveLeft);
                return true;
            case KeyCode.RightArrow:
                Move(shift, MoveRight);
                return true;
            case KeyCode.UpArrow:
                Move(shift, MoveUp);
                return true;
            case KeyCode.DownArrow:
                Move(shift, MoveDown);
                return true;
            case KeyCode.Tab:
            case KeyCode.Escape:
                return false;
            default:
                if (!readOnly)
                {
                    char c = evt.character;
                    if (c == '\0')
                    {
                        return true;
                    }
                    if (c == '\r' || c == '\u0003')
                    {
                        c = '\n';
                    }
                    RecordUndo();
                    Append(c);
                }
                return true;
        }
    }


}

