using UnityEngine;

namespace ScenesScripts.GalPlot
{
    public class MyGalManager_Choice : MonoBehaviour
    {
        private GameObject GameObject_Choice;

        private void Start()
        {
            GameObject_Choice = Resources.Load<GameObject>("HGF/MyButton-Choice");
        }
        [SerializeField]
        public void CreatNewChoice(string JumpID, string Title)
        {
            var _ = GameObject_Choice;
            _.GetComponent<MyGalComponent_Choice>().Init(JumpID, Title);
            Instantiate(_, this.transform);
            return;
        }
        public void Button_Click_Choice()
        {


            for (int i = 0; i < this.transform.childCount; i++)
            {
                //������DestroyImmediate
                //ԭ��DestroyImmediate��ͬ���ģ����ʹ����ᵼ��ÿ�λ�ȡ�Ķ���0���޷�ɾ����
                Destroy(this.transform.GetChild(i).gameObject);
            }
            return;
        }
    }
}