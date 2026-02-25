using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
            public SpeakData() { }
            public SpeakData(LayerCommand command, DataLayer dataLayer)
            {
                Content = command.CommandConfig["Content"];
                TextType = command.CommandConfig.ContainsKey("TextType")
                    ? command.CommandConfig["TextType"]:"TexDraw";
                if(TextType== "TexDraw")
                {
                    Content = TexDarwContentWrapper(Content);
                }
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
            public string TexDarwContentWrapper(string input)
            {
                //检测每一个字符，如果是中文，则用\text{}包裹
                //例如文本$公式$就要替换成\text{文}\text{本}$\text{公}\text{式}$
                if (string.IsNullOrEmpty(input))
                    return input;

                StringBuilder result = new StringBuilder();

                foreach (char c in input)
                {
                    // 判断是否为中文字符
                    if (IsChineseChar(c))
                    {
                        // 每个中文字符单独用\text{}包裹
                        result.Append($@"$\text{{{c}}}$");
                    }
                    else
                    {
                        // 非中文字符直接添加
                        result.Append(c);
                    }
                }

                return result.ToString();
            }
            // 判断字符是否为中文字符
            private bool IsChineseChar(char c)
            {
                // CJK统一表意文字基本区
                if (c >= '\u4E00' && c <= '\u9FFF')
                    return true;

                // CJK统一表意文字扩展A区
                if (c >= '\u3400' && c <= '\u4DBF')
                    return true;

                // CJK统一表意文字扩展B-F区（代理对处理）
                // 注意：由于char只能表示BMP字符，扩展B-F区的字符需要特殊处理

                // 中日韩符号和标点
                if (c >= '\u3000' && c <= '\u303F')
                    return true;

                // 全角标点（部分）
                if (c >= '\uFF01' && c <= '\uFF5E')
                {
                    // 排除全角英文字母和数字
                    if (c >= '\uFF21' && c <= '\uFF3A') // 全角大写字母
                        return false;
                    if (c >= '\uFF41' && c <= '\uFF5A') // 全角小写字母
                        return false;
                    if (c >= '\uFF10' && c <= '\uFF19') // 全角数字
                        return false;
                    return true;
                }
                return false;
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
            if (Contents.Count > 0)
            {
                // 显示最新一条对话（不播放动画）
                SpeakLayerControler.Speak(Contents[Contents.Count - 1], IsOnLoad: true);
            }
        
            // 刷新 UI：显示最新一条对话（或清空）
            if (Contents.Count > 0)
            {
                SpeakLayerControler.Speak(Contents[Contents.Count - 1], IsOnLoad: true);
            }
            else
            {

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
