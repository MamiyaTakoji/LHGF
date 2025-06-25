using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InputFieldControler : MonoBehaviour
{
    //应该要支持Ctrl+Z和Ctrl+Y
    public int MaxStep = 1000;//支持撤回的最大步数
    public SListNode<string> currentText;
    public TMP_InputField inputField;
    public TEXDraw tEX;
    public bool IsAdd = true;
    void Start()
    {
        currentText = new SListNode<string>(string.Empty);
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(delegate
        {
            OnValueChanged(inputField.text,IsAdd);
            tEX.text = inputField.text;
        });
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
              Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) &&
             Input.GetKeyDown(KeyCode.Z))
        {
            IsAdd = false;
            Undo();
            IsAdd = true;
        }
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
      Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) &&
     Input.GetKeyDown(KeyCode.Y))
        {
            IsAdd = false;
            Forward();
            IsAdd = true;
        }
    }
    public void Undo()
    {
        if(currentText.last != null)
        {
            currentText = currentText.last;
            inputField.text = currentText.value;
            Debug.Log(inputField.text);
        }

    }
    public void Forward()
    {
        if (currentText.next!= null)
        {
            currentText = currentText.next;
            inputField.text = currentText.value;
        }
    }
    public void OnValueChanged(string value, bool IsAdd)
    {
        if (IsAdd)//如果是外部修改才添加
        {
            currentText.Add(value);
            currentText = currentText.next;
        }

    }
    //用双向链表的方式保存
    public class SListNode<T>
    {
        public SListNode<T> last = null;
        public SListNode<T> next = null;
        public T value;
        public void Add(T item)
        {
            next = new SListNode<T>(item);
            next.last = this;
        }
        public SListNode(T item)
        {
            value = item;
        }
        public void Remove()
        {
            next = null;
        }
    }
}
