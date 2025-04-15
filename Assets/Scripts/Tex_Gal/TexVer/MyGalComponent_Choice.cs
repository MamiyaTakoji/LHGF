using UnityEngine;
using UnityEngine.UI;

namespace ScenesScripts.GalPlot
{
    /// <summary>
    /// ѡ����
    /// </summary>
    public class MyGalComponent_Choice : MonoBehaviour
    {
        /// <summary>
        /// ���ѡ��Ҫ��ת����ID
        /// </summary>
        public string _JumpID;

        /// <summary>
        /// ��ʾ���ı�
        /// </summary>
        public Text _Title;
        public void Init(string JumpID, string Title)
        {
            _JumpID = JumpID;
            _Title.text = Title;
        }
        /// <summary>
        /// ����Ұ�����ѡ��
        /// </summary>
        public void Button_Click_JumpTo()
        {

            GalManager.PlotData.NowJumpID = _JumpID;
            GalManager.PlotData.IsBranch = true;
            GalManager_Text.IsCanJump = true;
            if (_JumpID == "-1")
            {
                return;
            }
            this.gameObject.transform.parent.GetComponent<MyGalManager_Choice>().Button_Click_Choice();
            GameObject.Find("EventSystem").GetComponent<MyGalManager>().Button_Click_NextPlot();

            return;
        }
    }
}