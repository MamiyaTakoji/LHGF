using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHGFData;
using System.Linq;

namespace LHGFData
{
    public class EvidenceChoiceLayer : ILayer
    {
        public EvidencePanelControler EvidencePanelControler;
        public bool IsChoiceSelected = true;
        //写法类似 ChoiceLayer，但是没有选项Content
        //执行相关命令时，显示EvidencePanel，但是不显示退出键
        //选出正确的证据和错误的证据会走向不同的分支
        //正确的证据可以有多个，错误的同样可以有多个
        //因此，EvidenceChoiceLayer需要使用的参数为
        //正确的章节名，正确的证据名，选择错误后走向的剧情节点
        //以及选择正确后走向的剧情节点，它们分别用%%分割
        public void BeforeNextOnLoadStart()
        {
            throw new System.NotImplementedException();
        }

        public void BeforeNextStart()
        {

        }

        public bool Finish()
        {
            return IsChoiceSelected;
        }

        public GameObject GetControler()
        {
            return EvidencePanelControler.gameObject;
        }

        public string LayerName()
        {
            return "EvidenceChoiceLayer";
        }

        public void Load(object SaveData)
        {
             
        }

        public Dictionary<string, string> Log(LayerCommand command)
        {
            return null;
        }
        public Dictionary<string, string> SetLogInfo(string EvidenceName)
        {
            Dictionary<string, string> Content = new() { };
            string info = "你出示了证据：" + EvidenceName;
            Content.Add("TextType", "TexDraw");
            Content.Add("TextInfo", info);
            Content.Add("ContentType", "Text");
            return Content;
        }
        public void Next(LayerCommand command)
        {
            IsChoiceSelected = false;
            var NextId = command.CommandConfig["NextId"].Split("%%").ToList();
            var ChapterName = command.CommandConfig["ChapterName"].Split("%%").ToList();
            var EvidenceName = command.CommandConfig["EvidenceName"].Split("%%").ToList();
            //第一个节点对应的是默认的正确节点
            Dictionary<string, Dictionary<string, string>> evidences2nextId = new() { };
            for (int i = 0; i < NextId.Count; i++)
            {
                if (!evidences2nextId.ContainsKey(ChapterName[i]))
                {
                    evidences2nextId[ChapterName[i]] = new() { };

                }
                evidences2nextId[ChapterName[i]][EvidenceName[i]] = NextId[i];
            }
            EvidencePanelControler.GameMenu.SetActive(true);
            EvidencePanelControler.gameObject.SetActive(true);
            EvidencePanelControler.choiceEvidences = evidences2nextId;
            EvidencePanelControler.RightNextId = NextId[0];
            EvidencePanelControler.DefaultNextId = NextId[NextId.Count-1];
            EvidencePanelControler.RightEvidence = EvidenceName[0];
            EvidencePanelControler.ActiveEviddencePanel(EvidencePanelControler.MenuState.Show);
        }

        public void OnLoadFinish()
        {
            throw new System.NotImplementedException();
        }

        public void OnStart()
        {

        }

        public void OnUpdate()
        {

        }

        public void Reset()
        {
    
        }

        public object Save()
        {
            return null;
        }

        public void Skip()
        {

        }

        public bool Withdraw()
        {
            IsChoiceSelected = true;
            return true;
        }
    }
}
