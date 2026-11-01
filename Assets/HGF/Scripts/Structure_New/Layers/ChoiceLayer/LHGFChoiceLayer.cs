using LHGFGameProgress;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace LHGFData
{
    public class ChoiceLayer : ILayer
    {
        private string layerName = "ChoiceLayer";
        public bool IsChoiceSelected = true;
        public LHGFChoiceLayerControler LHGFchoiceLayerControler;
        public List<LayerCommand> HistoryChoice = new() { };
        public bool Finish()
        {
            if(IsChoiceSelected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Next(LayerCommand command)
        {
            BuildChoices(command);
            HistoryChoice.Add(command);
            IsChoiceSelected = false;
        }

        /// <summary>
        /// 解析选项命令并创建按钮。TextType 可选：
        /// 显式写了就按显式值（大小写不敏感），没写则按该选项内容是否含公式自动判断。
        /// </summary>
        private void BuildChoices(LayerCommand command)
        {
            var NextId = command.CommandConfig["NextId"].Split("%%").ToList();
            var Content = command.CommandConfig["Content"].Split("%%").ToList();

            string rawTypes = Utils.GetDicValue(command.CommandConfig, "TextType", "");
            var TextTypes = rawTypes.Split("%%").ToList();

            int choiceNum = NextId.Count;
            LHGFchoiceLayerControler.ResetButton();

            for (int i = 0; i < choiceNum; i++)
            {
                string content = i < Content.Count ? Content[i] : "";
                string type = i < TextTypes.Count ? TextTypes[i].Trim() : "";

                if (string.Equals(type, "TMP", System.StringComparison.OrdinalIgnoreCase))
                {
                    type = "TMP";
                }
                else if (string.Equals(type, "TexDraw", System.StringComparison.OrdinalIgnoreCase))
                {
                    type = "TexDraw";
                }
                else
                {
                    // 没写或未知类型：按内容自动判断
                    type = Utils.ContainsFormula(content) ? "TexDraw" : "TMP";
                }

                content = Utils.PrepareContent(content, type);
                LHGFchoiceLayerControler.SetChoiceButton(NextId[i], content, type);
            }
        }
        public void Next_OnLoad(LayerCommand command)
        {
            LHGFchoiceLayerControler.gameObject.SetActive(true);
            Next(command);
            IsChoiceSelected = true;
            LHGFchoiceLayerControler.gameObject.SetActive(false);
        }

        public void OnStart()
        {
            
        }

        public void OnUpdate()
        {
            
        }
        public void Skip()
        {
            
        }
        public bool Withdraw()
        {
/*            var command = HistoryChoice[HistoryChoice.Count - 1];
            var NextId = command.CommandConfig["NextId"].Split("%%").ToList();
            var TextType = command.CommandConfig["TextType"].Split("%%").ToList();
            var Content = command.CommandConfig["Content"].Split("%%").ToList();
            int ChoiceNum = NextId.Count;
            LHGFchoiceLayerControler.ResetButton();
            for (int i = 0; i < ChoiceNum; i++)
            {
                LHGFchoiceLayerControler.SetChoiceButton(NextId[i], Content[i], TextType[i]);
            }
            HistoryChoice.RemoveAt(HistoryChoice.Count - 1);
            IsChoiceSelected = false;*/
            IsChoiceSelected = true;  // 添加这一行
            return true;
        }

        public string LayerName()
        {
            return layerName;
        }

        public GameObject GetControler()
        {
            return LHGFchoiceLayerControler.gameObject;
        }

        public void OnLoadFinish()
        {
            
        }

        public void BeforeNextStart()
        {

        }

        public Dictionary<string, string> Log(LayerCommand command)
        {
            return null;
        }
        //Log的文本在这里设置，在控制器中实现消息的记录
        public Dictionary<string, string> SetLogInfo(string ChoiceText, string TextType)
        {
            Dictionary<string, string> Content = new() { };
            // ChoiceText 已被 BuildChoices 按 TextType 处理过；
            // TexDraw 时前缀也要包装，否则历史记录里前缀会是裸中文
            string info = "你选择了：" + ChoiceText;
            if (string.Equals(TextType, "TexDraw", System.StringComparison.OrdinalIgnoreCase))
            {
                info = Utils.TexDarwContentWrapper("你选择了：") + ChoiceText;
            }
            Content.Add("TextType", TextType);
            Content.Add("TextInfo", info);
            Content.Add("ContentType", "Text");
            return Content;
        }

        public void Reset()
        {
            LHGFchoiceLayerControler.ResetButton();
            HistoryChoice = new() { };
        }
        public void BeforeNextOnLoadStart() { }
        [System.Serializable]
        public class SaveData
        {
            public bool IsChoiceSelected = true;
            public List<LayerCommand> HistoryChoice = new() { };
        }
        public void Load(object saveData)
        {
            if (saveData == null) return;
            SaveData data;
            data = saveData as SaveData;
            if (data == null)
            {
                string json = JsonConvert.SerializeObject(saveData);
                data = JsonConvert.DeserializeObject<SaveData>(json);
            }

            // 恢复基础数据
            IsChoiceSelected = data.IsChoiceSelected;

            // 恢复历史命令列表（创建新列表避免引用共享）
            HistoryChoice.Clear();
            if (data.HistoryChoice != null)
            {
                HistoryChoice.AddRange(data.HistoryChoice);
            }
            //无论选项是否被选择，都布置选项
            //解码命令并设置选项
            if (HistoryChoice.Count > 0)
            {
                var lastCommand = HistoryChoice[HistoryChoice.Count - 1];
                LHGFchoiceLayerControler.gameObject.SetActive(true);
                BuildChoices(lastCommand);
                if (IsChoiceSelected)
                {
                    LHGFchoiceLayerControler.gameObject.SetActive(false);
                }
            }
        }

        public object Save()
        {
            var saveData = new SaveData
            {
                IsChoiceSelected = this.IsChoiceSelected,
                // 创建新列表，元素引用相同（假设 LayerCommand 在保存后不会修改）
                HistoryChoice = new List<LayerCommand>(this.HistoryChoice)
            };
            return saveData;
        }
    }
}

