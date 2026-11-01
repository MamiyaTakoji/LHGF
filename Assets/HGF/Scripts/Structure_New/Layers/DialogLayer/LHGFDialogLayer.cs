using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHGFData
{
    public class DialogLayer : ILayer
    {
        public Tweener TextTweener;
        public string layerName = "DialogLayer";
        public DataLayer dataLayer;
        public LHGFDialogLayerControler SpeakLayerControler;
        public List<SpeakData> Contents = new() { };
        public class SpeakData
        {
            public string Content;
            public string TextType;
            public bool Skip;
            public string SpeakerName;
            public string DepartmentName;
            public Color DialogBarColor = new Color(0, 0, 0, 0.8313726f);
            public SpeakData() { }
            public SpeakData(LayerCommand command, DataLayer dataLayer)
            {
                Color c = new Color(0, 0, 0, 0.8313726f);
                if (command.CommandConfig.ContainsKey("DialogBarColor"))
                {
                    string colorStr = command.CommandConfig["DialogBarColor"];
                    string[] colorStrs = colorStr.Split(',');
                    if (colorStrs.Length == 3)
                    {
                        c = new Color(float.Parse(colorStrs[0]), float.Parse(colorStrs[1]), float.Parse(colorStrs[2]), 0.8313726f);
                    }
                    else if (colorStrs.Length == 4)
                    {
                        c = new Color(float.Parse(colorStrs[0]), float.Parse(colorStrs[1]), float.Parse(colorStrs[2]), float.Parse(colorStrs[3]));
                    }
                }
                DialogBarColor = c;
                Content = command.CommandConfig["Content"];

                // 显式指定优先；没写 TextType 时按内容自动判断：
                // 只有出现成对的 $...$ / $$...$$ 才用 TexDraw，其余一律 TMP。
                TextType = Utils.GetDicValue(command.CommandConfig, "TextType", "");
                TextType = TextType.Trim();
                if (TextType.Length == 0)
                {
                    TextType = Utils.ContainsFormula(Content) ? "TexDraw" : "TMP";
                }
                else if (string.Equals(TextType, "TMP", System.StringComparison.OrdinalIgnoreCase))
                {
                    TextType = "TMP";
                }
                else if (string.Equals(TextType, "TexDraw", System.StringComparison.OrdinalIgnoreCase))
                {
                    TextType = "TexDraw";
                }

                // 统一处理 @@、中文包装（仅 TexDraw）、TMP 下的 \\ 换行
                Content = Utils.PrepareContent(Content, TextType);
                Skip = command.CommandConfig.ContainsKey("Skip")
                    ? Utils.string2bool(command.CommandConfig["Skip"]): false;
                if (command.CommandConfig.ContainsKey("SpeakerName"))
                {
                    SpeakerName = command.CommandConfig["SpeakerName"];
                }
                else if(command.CommandConfig.ContainsKey("CharacterID"))
                {
                    string characterID = command.CommandConfig["CharacterID"];
                    var charInfo = dataLayer.characterData.GetCharacterInfo(characterID);
                    SpeakerName = charInfo.Name;
                }
                else
                {
                    SpeakerName = "";
                }

                if (command.CommandConfig.ContainsKey("DepartmentName"))
                {
                    DepartmentName = command.CommandConfig["DepartmentName"];
                }
                else if (command.CommandConfig.ContainsKey("CharacterID"))
                {
                    string characterID = command.CommandConfig["CharacterID"];
                    var charInfo = dataLayer.characterData.GetCharacterInfo(characterID);
                    DepartmentName = charInfo.Affiliation;
                }
                else
                {
                    DepartmentName = "";
                }
            }
        }
        public bool Finish()
        {
            if (TextTweener == null || !TextTweener.IsActive() || !TextTweener.IsPlaying() || TextTweener.IsComplete())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string LayerName()
        {
            return layerName;
        }

        public void Log()
        {

        }

        public void Next(LayerCommand command)
        {
            SpeakData SpeakData = new(command, dataLayer);
            SpeakLayerControler.Speak(SpeakData);
            Contents.Add(SpeakData);
        }

        public void Next_OnLoad(LayerCommand command)
        {
            SpeakData SpeakData = new(command, dataLayer);
            SpeakLayerControler.Speak(SpeakData,true);
            Contents.Add(SpeakData);
        }

        public void OnStart()
        {

        }

        public void OnUpdate()
        {

        }

        public void Skip()
        {
            if (TextTweener != null)
            {
                TextTweener.Complete();
            }
        }

        public bool Withdraw()
        {
            if(Contents.Count < 2)
            {
                return false;
            }
            SpeakData SpeakData = Contents[Contents.Count-2];
            SpeakLayerControler.Speak(SpeakData, true);
            Contents.RemoveAt(Contents.Count - 1);
            return true;
        }

        public GameObject GetControler()
        {
            return SpeakLayerControler.gameObject;
        }

        public void OnLoadFinish()
        {

        }

        public void BeforeNextStart()
        {

        }

        public Dictionary<string, string> Log(LayerCommand command)
        {
            Dictionary<string, string> Content = new();
            SpeakData SpeakData = new(command, dataLayer);
            string SpeakerName = SpeakData.SpeakerName;
            string SpeakContent = SpeakData.Content;
            string TextType = SpeakData.TextType;
            string info = SpeakerName + ":" + SpeakContent;
            Content.Add("TextType", TextType);
            Content.Add("TextInfo", info);
            Content.Add("ContentType", "Text");
            return Content;
        }

        public void Reset()
        {
            Contents = new() { };
        }
        //如果输入的内容中含有中文，需要把每一个中文字符用\text{}包裹
        public void BeforeNextOnLoadStart() { }
        [System.Serializable]
        public class SaveData
        {
            public List<SpeakData> Contents;
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

            // 清空当前内容
            Contents.Clear();

            // 恢复历史数据（深拷贝列表，元素直接引用）
            if (data.Contents != null)
            {
                Contents.AddRange(data.Contents);
            }

            // 停止任何正在播放的文本动画
            if (TextTweener != null && TextTweener.IsActive())
            {
                TextTweener.Kill();
                TextTweener = null;
            }
            // 刷新 UI：显示最新一条对话（不播放动画）
            if (Contents.Count > 0)
            {
                SpeakLayerControler.Speak(Contents[Contents.Count - 1], IsOnLoad: true);
            }
        }

        public object Save()
        {
            var saveData = new SaveData
            {
                // 创建 Contents 的副本（浅拷贝列表）
                Contents = new List<SpeakData>(this.Contents)
            };
            return saveData;
        }
    }
}
