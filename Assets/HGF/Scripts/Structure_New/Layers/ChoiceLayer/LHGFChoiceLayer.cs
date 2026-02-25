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
            //解码命令
            var NextId = command.CommandConfig["NextId"].Split("%%").ToList();
            var TextType = command.CommandConfig["TextType"].Split("%%").ToList();
            var Content = command.CommandConfig["Content"].Split("%%").ToList();
            int ChoiceNum = NextId.Count;
            LHGFchoiceLayerControler.ResetButton();
            for (int i = 0; i < ChoiceNum; i++)
            {
                LHGFchoiceLayerControler.SetChoiceButton(NextId[i], Content[i], TextType[i]);
            }
            HistoryChoice.Add(command);
            IsChoiceSelected = false;
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
            string info = "你选择了：" + ChoiceText;
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
                //解码命令
                var NextId = lastCommand.CommandConfig["NextId"].Split("%%").ToList();
                var TextType = lastCommand.CommandConfig["TextType"].Split("%%").ToList();
                var Content = lastCommand.CommandConfig["Content"].Split("%%").ToList();
                int ChoiceNum = NextId.Count;
                LHGFchoiceLayerControler.ResetButton();
                LHGFchoiceLayerControler.gameObject.SetActive(true);
                for (int i = 0; i < ChoiceNum; i++)
                {
                    LHGFchoiceLayerControler.SetChoiceButton(NextId[i], Content[i], TextType[i]);
                }
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

