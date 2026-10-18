using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHGFData;
using System.IO;
using Newtonsoft.Json;

namespace LHGFData
{
    public class EvidenceLayer : ILayer
    {
        public Dictionary<string, Dictionary<string, string>> AllEvidences = new() { };
        public Dictionary<string, Dictionary<string, string>> CurrentEvidences = new() { };
        public Dictionary<string, Dictionary<string, string>> VisitedEvidences = new() { };
        public List<Evidence> HistoryEvidence = new() { };
        public List<Evidence> NewEvidence = new() { };
        public EvidencePanelControler EvidencePanelControler;
        public string Spliter = "|||";
        public class Evidence
        {
            public string chName;
            public string eviName;
            public string eviContent;
            public string eviAbstract;
            public Evidence() { }
            public Evidence(string chName, string eviName, string eviContent, string eviAbstract)
            {
                this.chName = chName;
                this.eviName = eviName;
                this.eviContent = eviContent;
                this.eviAbstract = eviAbstract;
            }
            public Evidence(int errorId)
            {
                if(errorId == 1)
                {
                    this.chName = "出现错误";
                    this.eviName = "错误信息：";
                    this.eviContent = "使用命令AddEvidence时，未添加所需命令";
                }
                else if(errorId == 2)
                {
                    this.chName = "出现错误";
                    this.eviName = "错误信息：";
                    this.eviContent = "添加了文件中未记录的线索";
                }
            }
        }
    public void LoadAllEvidence()
        {
            string path = Utils.ResoucePaths.EvidenceContentPath;
            string FileName = "EvidenceInfo.json";
            path = Path.Combine(path, FileName);
            AllEvidences = new Utils.GameConfig<Dictionary<string, string>>(path).dataDict;
        }
        public Evidence AddEvidence(string ch, string evidenceName)
        {
            var temp = AllEvidences[ch][evidenceName].Split(Spliter);
            string eviContent = temp[0];
            string eviAbstract = temp[1];
            var evi = new Evidence(ch, evidenceName, eviContent, eviAbstract);
            NewEvidence.Add(evi);
            return evi;
        }
        public void ReadEvidence(string ch, string evidenceName)
        {
            var temp = AllEvidences[ch][evidenceName].Split(Spliter);
            string eviContent = temp[0];
            string eviAbstract = temp[1];
            var evi = new Evidence(ch, evidenceName, eviContent, eviAbstract);
            if (!CurrentEvidences.ContainsKey(evi.chName))
            {
                VisitedEvidences[evi.chName] = new Dictionary<string, string>();
            }
            VisitedEvidences[evi.chName][evi.eviName] = AllEvidences[evi.chName][evi.eviName];
        }
        public bool IsEvidenceVisited(string chapterName, string evidenceName)
        {
            if (string.IsNullOrEmpty(chapterName) || string.IsNullOrEmpty(evidenceName))
            {
                return false;
            }

            // 检查章节是否存在
            if (!VisitedEvidences.ContainsKey(chapterName))
            {
                return false;
            }

            // 检查该章节下是否存在该证据
            return VisitedEvidences[chapterName].ContainsKey(evidenceName);
        }
        public bool HasUnvisitedEvidenceInChapter(string chapterName)
        {
            // 1. 如果玩家当前没有持有该章节的任何证据，则不存在“未读”的概念（或者说没有可读的）
            if (!CurrentEvidences.ContainsKey(chapterName))
            {
                return false;
            }

            var currentChapterData = CurrentEvidences[chapterName];

            // 2. 如果玩家持有了证据，但从未访问过该章节的任何证据（Visited 中无此章节或为空）
            if (!VisitedEvidences.ContainsKey(chapterName) || VisitedEvidences[chapterName].Count == 0)
            {
                return currentChapterData.Count > 0;
            }

            var visitedChapterData = VisitedEvidences[chapterName];

            // 3. 快速比对：如果数量都不一样，肯定有未读的（因为 Visited 只能是 Current 的子集或相等）
            // 注意：这里假设 VisitedEvidences 不会包含 CurrentEvidences 中没有的证据。
            // 如果 Visited 的数量小于 Current 的数量，说明有 Current 里的证据没被 Visit。
            if (visitedChapterData.Count < currentChapterData.Count)
            {
                return true;
            }

            // 4. 数量相同的情况下，比对具体的 Key 是否一致
            // 只要有一个 Key 在 Current 中存在但在 Visited 中不存在，即为未读
            foreach (var evidenceName in currentChapterData.Keys)
            {
                if (!visitedChapterData.ContainsKey(evidenceName))
                {
                    return true;
                }
            }

            // 5. 所有持有的证据都已访问过
            return false;
        }
        public void BeforeNextOnLoadStart()
        {
            throw new System.NotImplementedException();
        }

        public void BeforeNextStart()
        {
            NewEvidence = new List<Evidence>() { };
        }

        public bool Finish()
        {
            return true;
        }

        public GameObject GetControler()
        {
            return EvidencePanelControler.gameObject;
        }

        public string LayerName()
        {
            return "EvidenceLayer";
        }
        public class SaveData
        {
            public Dictionary<string, Dictionary<string, string>> Evidences = new() { };
            public Dictionary<string, Dictionary<string, string>> VisitedEvidences = new() { };
            public List<Evidence> HistoryEvidence = new() { };
        }
        public void Load(object saveData)
        {
            //存档数据缺失时，重置为空状态而不是跳过加载，避免后续状态污染导致证据不显示
            if (saveData == null)
            {
                CurrentEvidences = new Dictionary<string, Dictionary<string, string>>();
                VisitedEvidences = new Dictionary<string, Dictionary<string, string>>();
                HistoryEvidence = new List<Evidence>() { };
                EvidencePanelControler.SetEvidencePanel(CurrentEvidences);
                return;
            }
            SaveData data;
            data = saveData as SaveData;
            if (data == null)
            {
                string json = JsonConvert.SerializeObject(saveData);
                data = JsonConvert.DeserializeObject<SaveData>(json);
            }
            //防御式恢复：数据为 null 时用空字典兜底，避免 CurrentEvidences 被置 null
            CurrentEvidences = data.Evidences ?? new Dictionary<string, Dictionary<string, string>>();
            VisitedEvidences = data.VisitedEvidences ?? new Dictionary<string, Dictionary<string, string>>();
            HistoryEvidence = data.HistoryEvidence ?? new List<Evidence>() { };
            EvidencePanelControler.SetEvidencePanel(CurrentEvidences);
        }

        public Dictionary<string, string> Log(LayerCommand command)
        {
            return null;
        }

        public void Next(LayerCommand command)
        {
            var config = command.CommandConfig;
            Evidence evi = null;
            //如果不存在需要的键，则加入特定的信息提示报错
            if (!config.ContainsKey("ChapterName") || !config.ContainsKey("EvidenceName"))
            {
                evi = new Evidence(1);
                NewEvidence.Add(evi);
            }
            else 
            {
                string chName = config["ChapterName"];
                string eviName = config["EvidenceName"];
                //如果allEvidence中不存在需要的键，则加入特定的报错信息
                if (!AllEvidences.ContainsKey((chName)) || !AllEvidences[chName].ContainsKey(eviName))
                {
                    evi = new Evidence(2);
                    NewEvidence.Add(evi);
                }
                else
                {
                    evi = AddEvidence(chName, eviName);
                    NewEvidence.Add(evi);
                }
            }
            if (!CurrentEvidences.ContainsKey(evi.chName))
            {
                CurrentEvidences[evi.chName] = new Dictionary<string, string>();
            }
            CurrentEvidences[evi.chName][evi.eviName] = AllEvidences[evi.chName][evi.eviName];
            //执行控制器的操作
            EvidencePanelControler.AddEvidence(evi);
            HistoryEvidence.Add(evi);
        }

        public void OnLoadFinish()
        {

        }

        public void OnStart()
        {
            LoadAllEvidence();
        }

        public void OnUpdate()
        {

        }

        public void Reset()
        {
            CurrentEvidences.Clear();
            EvidencePanelControler.ResetEvidencePanel();
        }

        public object Save()
        {
            var saveData = new SaveData();
            saveData.Evidences = Utils.DeepClone(CurrentEvidences);
            saveData.VisitedEvidences = Utils.DeepClone(VisitedEvidences);
            saveData.HistoryEvidence = Utils.DeepClone(HistoryEvidence); 
            return saveData;
        }

        public void Skip()
        {
            
        }

        public bool Withdraw()
        {
            //如果把回撤功能去掉，工作量立减80%，别做这个比回撤功能了我说。
            //还是做吧（叹气）
            //回撤主要的操作对象有两个，第一个是当前的Layer，第二个是控制器
            //其中，对于当前的Layer，主要回撤上一个evidence
            //对于控制器，如果一个章节的证据为空，则需要删除章节对应的按钮
            if(HistoryEvidence.Count == 0)
            {
                return false;
            }
            var historyEvi = HistoryEvidence[HistoryEvidence.Count-1];
            HistoryEvidence.RemoveAt(HistoryEvidence.Count-1);
            string ch = historyEvi.chName;
            string eviName = historyEvi.eviName;
            CurrentEvidences[ch].Remove(eviName);
            if(CurrentEvidences[ch].Count == 0)
            {
                CurrentEvidences.Remove(ch);
                GameObject ButtonGameObject = EvidencePanelControler.ChapterButtons[ch].gameObject;
                GameObject.Destroy(ButtonGameObject);
                EvidencePanelControler.ChapterButtons.Remove(ch);
            }
            return true;
        }
    }
}

