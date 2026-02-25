using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LHGFData
{
    public class BlackboardLayer : ILayer
    {
        //和LongDialog类似，但是可以设置在任意位置，
        //规定任意大小，并且和Speak不冲突
        public LHGFBlackboardLayerControler blackboardLayerControler;
        public Sequence TextTweener;
        public BlackboardData currentBlackboardData;
        public List<List<BlackboardData>> Contents = new() { };
        public List<List<BlackboardData>> HistoryContents = new() { };
        public bool IsNewNodeStart = true;
        public class BlackboardData
        {
            public bool Continue = true;
            public string Content = null;
            public string TextType = "TexDraw";
            //public bool End = false;
            public bool Skip = false;
            //下面是图片类型才可能有的属性
            public float Rate=0.3f;
            //下面是关于位置的信息
            //PosInfo目前只包含UpperLeft，UpperCenter，UpperRight
            //三种情况，因为其他位置感觉不是很好看
            //只能通过自行设置PosX和PosY来完成
            public string PosInfo;
            public Vector2 Pos = new Vector2(1, 1);
            public Vector2 Size;
            public float FontSize;
            public bool BlackboardState = true;
            public BlackboardData() { }
            public BlackboardData(LayerCommand command, BlackboardData refdata)
            {
                float defaultFontSize;
                string defaultTextType;
                //字体大小和类型默认参考数据
                if (refdata != null)
                {
                    defaultFontSize = refdata.FontSize;
                    if (refdata.TextType != "Image")
                    {
                        defaultTextType = refdata.TextType;
                    }
                    else
                    {
                        defaultTextType = "TexDraw";
                    }
                }
                else
                {
                    defaultFontSize = 36;
                    defaultTextType = "TexDraw";
                }
                var dic = command.CommandConfig;
                Continue = Utils.string2bool(Utils.GetDicValue(dic, "Continue", "1"));
                Content = Utils.GetDicValue(dic, "Content", null);
                TextType = Utils.GetDicValue(dic, "TextType", defaultTextType);
                //End = Utils.string2bool(Utils.GetDicValue(dic, "End", "0"));
                Skip = Utils.string2bool(Utils.GetDicValue(dic, "Skip", "0"));
                string _Rate = "0";
                //TextType的类型为Image时，command才会具有如下属性
                if (command.CommandConfig.ContainsKey("Rate"))
                {
                    _Rate = command.CommandConfig["Rate"];
                }
                Rate = float.Parse(_Rate);
                string FontSizeStr = Utils.GetDicValue(dic, "FontSize", defaultFontSize.ToString());
                FontSize = float.Parse(FontSizeStr);
                string SizeXStr = Utils.GetDicValue(dic, "SizeX", "0.6");
                string SizeYStr = Utils.GetDicValue(dic, "SizeY", "0.6");
                float sizeX = float.Parse(SizeXStr);
                float sizeY = float.Parse(SizeYStr);
                Size = new Vector2(sizeX, sizeY);
                PosInfo = Utils.GetDicValue(dic, "PosInfo", string.Empty);
                if (PosInfo == "UpperLeft")
                {
                    Pos = new Vector2(Size.x/2, 1-Size.y/2);
                }
                else if (PosInfo == "UpperCenter")
                {
                    Pos = new Vector2(0, 1 - Size.y / 2);
                }
                else if (PosInfo == "UpperRight")
                {
                    Pos = new Vector2(1 - Size.x / 2, 1 - Size.y / 2);
                }
                else
                {
                    string PosXStr = Utils.GetDicValue(dic, "PosX", "0.7");
                    string PosYStr = Utils.GetDicValue(dic, "PosY", "0.7");
                    float posX = float.Parse(PosXStr);
                    float posY = float.Parse(PosYStr);
                    Pos = new Vector2(posX, posY);
                }
                BlackboardState = Utils.string2bool(Utils.GetDicValue(dic, "BlackboardState", "1"));
            }
        }
        public void BeforeNextStart()
        {
            /*            if (currentBlackboardData!=null&&currentBlackboardData.BlackboardState == false)
                        {
                            blackboardLayerControler.CloseBlackboard();
                        }*/
            IsNewNodeStart = true;
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

        public GameObject GetControler()
        {
            return blackboardLayerControler.gameObject;
        }

        public string LayerName()
        {
            return "BlackboardLayer";
        }

        public Dictionary<string, string> Log(LayerCommand command)
        {
            Dictionary<string, string> Content = new();
            string info = currentBlackboardData.Content;
            if (info == null)
            {
                return null;
            }
            string ContentType = currentBlackboardData.TextType;
            if (ContentType == "Image")
            {
                string Rate = currentBlackboardData.Rate.ToString();
                Content.Add("TextInfo", info);
                Content.Add("ContentType", "Image");
                Content.Add("Rate", Rate);
            }
            else
            {
                string TextType = currentBlackboardData.TextType;
                Content.Add("TextType", TextType);
                Content.Add("TextInfo", info);
                Content.Add("ContentType", "Text");
            }
            return Content;
        }

        //如果command没有Content，则不设置新的内容
        public void Next(LayerCommand command)
        {
            //关闭黑板还是得在Next这里，不然回撤无法检测到关闭命令
            BlackboardData data = new(command, currentBlackboardData);
            string TextType = data.TextType;
            string Content = data.Content;
            //如果Content为null，则仅是关闭或者打开黑板，不更新内容
            bool BlackboardState = data.BlackboardState;
            if (BlackboardState)
            {
                blackboardLayerControler.OpenBlackboard(data);
            }
            else
            {
                blackboardLayerControler.CloseBlackboard();
            }
            //如果Content不为空，则
            if (Content != null)
            {
                if (TextType == "TexDraw" || TextType == "TMP")
                {
                    Content = BuildStringContent(Content, TextType);
                    if (TextType == "TexDraw")
                    {
                        Content = Utils.TexDarwContentWrapper(Content);
                    }
                }
                else if (TextType == "Image")
                {
                    Content = Path.Combine(Utils.ResoucePaths.LongSpeakImagePath, Content);
                }
                data.Content = Content;
                if (TextTweener == null || !TextTweener.IsActive() || !TextTweener.IsPlaying() || TextTweener.IsComplete())
                {
                    TextTweener = DOTween.Sequence();
                    TextTweener.Append(blackboardLayerControler.AddContent(data, false));
                }
                else
                {
                    TextTweener.Append(blackboardLayerControler.AddContent(data, false));
                }
            }
            
            if (!data.Continue || Contents.Count < 1)
            {
                Contents.Add(new List<BlackboardData>() { data });
            }
            else
            {
                Contents[Contents.Count - 1].Add(data);
            }
            if (IsNewNodeStart)
            {
                HistoryContents.Add(new List<BlackboardData>() { data });
                IsNewNodeStart = false;
            }
            else
            {
                HistoryContents[HistoryContents.Count - 1].Add(data);
            }
            //更新黑板状态
/*            if (data.BlackboardState)
            {
                blackboardLayerControler.OpenBlackboard(data);
            }
            else
            {
                blackboardLayerControler.CloseBlackboard();
                return;
            }*/

            currentBlackboardData = data;
        }

        public void Next_OnLoad(LayerCommand command)
        {
            BlackboardData data = new(command, currentBlackboardData);
            string TextType = data.TextType;
            string Content = data.Content;
            if (TextType == "TexDraw" || TextType == "TMP")
            {
                Content = BuildStringContent(Content, TextType);
                if (TextType == "TexDraw")
                {
                    Content = Utils.TexDarwContentWrapper(Content);
                }
            }
            else if (TextType == "Image")
            {
                Content = Path.Combine(Utils.ResoucePaths.LongSpeakImagePath, Content);
            }
            data.Content = Content;
            if (!data.Continue || Contents.Count < 1)
            {
                Contents.Add(new List<BlackboardData>() { data });
            }
            else
            {
                Contents[Contents.Count - 1].Add(data);
            }
            //更新黑板状态
            if (data.BlackboardState)
            {
                blackboardLayerControler.OpenBlackboard(data);
            }
            else
            {
                blackboardLayerControler.CloseBlackboard();
                return;
            }
            if (IsNewNodeStart)
            {
                HistoryContents.Add(new List<BlackboardData>() { data });
                IsNewNodeStart = false;
            }
            else
            {
                HistoryContents[HistoryContents.Count - 1].Add(data);
            }
            if (TextTweener == null || !TextTweener.IsActive() || !TextTweener.IsPlaying() || TextTweener.IsComplete())
            {
                //TextTweener = DOTween.Sequence();
                TextTweener.Append(blackboardLayerControler.AddContent(data, true));
            }
            else
            {
                TextTweener.Append(blackboardLayerControler.AddContent(data, true));
            }
            currentBlackboardData = data;
        }

        public void OnLoadFinish(){}
        public void OnStart()
        {
            blackboardLayerControler.CloseBlackboard();
        }

        public void OnUpdate()
        {
            
        }

        public void Reset()
        {
            blackboardLayerControler.Clear();
            blackboardLayerControler.CloseBlackboard();
        }

        public void Skip()
        {
            TextTweener.Complete(true);
        }

        public bool Withdraw()
        {
            //回撤还要控制黑板是否显示
            //回撤每一个History的Content
            //如果没有Contents，则不回撤内容
            if (Contents.Count == 0)
            {
                return false;
            }
            int WithdrawContentCount = HistoryContents[HistoryContents.Count - 1].Count;
            HistoryContents.RemoveAt(HistoryContents.Count - 1);
            bool IsClear = false;
            //否则，先移除上一个节点的Content
            for (int i = 0; i < WithdrawContentCount; i++)
            {
                var temp = Contents[Contents.Count - 1];
                if (temp[temp.Count - 1].BlackboardState)
                {
                    IsClear = (temp.Count == 1);
                    if (IsClear)
                    {
                        Contents.RemoveAt(Contents.Count - 1);
                    }
                    else
                    {
                        temp.RemoveAt(temp.Count - 1);
                    }
                    //如果Content不为空，则执行撤回
                    if (temp[temp.Count - 1].Content!=null)
                    {
                        if (Contents.Count > 0)
                        {
                            blackboardLayerControler.OpenBlackboard(temp[temp.Count - 1]);
                            blackboardLayerControler.WithdrawContent(Contents[Contents.Count - 1], IsClear);
                            if(temp[temp.Count - 1].BlackboardState == false)
                            {
                                blackboardLayerControler.CloseBlackboard();
                            }
                        }
                        else
                        {
                            blackboardLayerControler.Clear();
                            blackboardLayerControler.CloseBlackboard();
                        }
                    }
                }
                else
                {
                    IsClear = (temp.Count == 1);
                    if (IsClear)
                    {
                        Contents.RemoveAt(Contents.Count - 1);
                    }
                    else
                    {
                        temp.RemoveAt(temp.Count - 1);
                    }
                }
            }
            //如果后撤后已经没有内容，则关闭黑板
            if (Contents.Count == 0)
            {
                blackboardLayerControler.Clear();
                blackboardLayerControler.CloseBlackboard();
            }
            //否则，根据最后一条内容显示
            else
            {
                var temp = Contents[Contents.Count - 1];
                currentBlackboardData = temp[temp.Count - 1];
                if (currentBlackboardData.BlackboardState)
                {
                    blackboardLayerControler.OpenBlackboard(currentBlackboardData);
                }
                else
                {
                    blackboardLayerControler.CloseBlackboard();
                }
            }
            return true;
        }
        public string BuildStringContent(string text, string TextType)
        {
            if (TextType == "TexDraw")
            {
                text = text.Replace("@@", "\n\n");
            }
            else
            {
                text = text.Replace("@@", "\n");
            }
            return text;
        }
        public void BeforeNextOnLoadStart() 
        {
            IsNewNodeStart = true;
        }
        [System.Serializable]
        public class SaveData
        {
            public List<List<BlackboardData>> Contents;
            public List<List<BlackboardData>> HistoryContents;
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
            HistoryContents.Clear();

            // 恢复数据（深拷贝外层和内层列表）
            if (data.Contents != null)
            {
                foreach (var innerList in data.Contents)
                {
                    Contents.Add(new List<BlackboardData>(innerList));
                }
            }
            if (data.HistoryContents != null)
            {
                foreach (var innerList in data.HistoryContents)
                {
                    HistoryContents.Add(new List<BlackboardData>(innerList));
                }
            }

            // 停止所有动画
            if (TextTweener != null && TextTweener.IsActive())
            {
                TextTweener.Kill();
                TextTweener = null;
            }

            // 清空控制器显示
            blackboardLayerControler.Clear();
            //blackboardLayerControler.CloseBlackboard();
            if(Contents.Count> 0) 
            {
                // 重建黑板显示（模拟执行历史，但不播放动画）
                var lastContent = Contents[Contents.Count - 1];
                blackboardLayerControler.OpenBlackboard(lastContent[lastContent.Count - 1]);
                foreach (var blackboardData in lastContent)
                {
                    if(blackboardData.Content != null)
                    {
                        blackboardLayerControler.AddContent(blackboardData, true);
                    }
                }
                //如果黑板是关闭的，需要关闭它
                var lastdata = lastContent[lastContent.Count - 1];
                if (lastdata.BlackboardState == false)
                {
                    blackboardLayerControler.CloseBlackboard();
                }
            }
            // 设置当前数据为最后一个有效内容（如果有）
            if (Contents.Count > 0 && Contents[Contents.Count - 1].Count > 0)
            {
                var lastPage = Contents[Contents.Count - 1];
                currentBlackboardData = lastPage[lastPage.Count - 1];
            }
            else
            {
                currentBlackboardData = null;
            }
            IsNewNodeStart = true;
        }

        public object Save()
        {
            var saveData = new SaveData();

            // 深拷贝 Contents 和 HistoryContents
            if (Contents != null)
            {
                saveData.Contents = new List<List<BlackboardData>>();
                foreach (var innerList in Contents)
                {
                    saveData.Contents.Add(new List<BlackboardData>(innerList));
                }
            }
            if (HistoryContents != null)
            {
                saveData.HistoryContents = new List<List<BlackboardData>>();
                foreach (var innerList in HistoryContents)
                {
                    saveData.HistoryContents.Add(new List<BlackboardData>(innerList));
                }
            }
            return saveData;
        }
    }
}
