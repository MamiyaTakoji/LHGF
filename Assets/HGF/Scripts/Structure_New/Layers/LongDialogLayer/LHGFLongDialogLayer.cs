using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LHGFData
{
    public class LongDialogLayer : ILayer
    {
        //public LongSpeakData CurrentLongSpeakData;
        public Tweener TextTweener;
        public LHGFLongDialogLayerControler LHGFlongDialogLayerControler;
        //public bool IsShowingLongSpeak;
        public string layerName = "LongDialogLayer";
        public LongSpeakData currentLongSpeakData;
        public List<List<LongSpeakData>> Contents = new() { };


        public class LongSpeakData
        {
            public bool Continue;
            public string Content;
            public string TextType;
            private string defaultTextType = "TexDraw";
            private string defaultContinue = "1";
            private string defaultSkip = "0";
            //public bool End;
            public bool Skip;
            //下面是图片类型才可能有的属性
            public float Rate;
            public LongSpeakData() { }
            public LongSpeakData(LayerCommand command)
            {
                var dic = command.CommandConfig;
                Continue = Utils.string2bool(Utils.GetDicValue(dic, "Continue", defaultContinue));
                Content = command.CommandConfig["Content"];
                TextType = Utils.GetDicValue(dic, "TextType", defaultTextType);
                Skip = Utils.string2bool(Utils.GetDicValue(dic, "Skip", defaultSkip));
                //TextType的类型为Image时，command才会具有如下属性
                string _Rate = "0";
                if (command.CommandConfig.ContainsKey("Rate"))
                {
                    _Rate = command.CommandConfig["Rate"];
                }
                Rate = float.Parse(_Rate);
            }
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
        public bool Finish()
        {
            if (TextTweener == null || !TextTweener.IsActive()|| !TextTweener.IsPlaying()||TextTweener.IsComplete())
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
            LongSpeakData LongSpeakData = new(command);
            bool Continue = LongSpeakData.Continue ;
            string Content = LongSpeakData.Content;
            string TextType = LongSpeakData.TextType;
            //string End = command.CommandConfig["End"];
            if (TextType == "TexDraw"||TextType == "TMP")
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
            LongSpeakData.Content = Content;
            if (!Continue || Contents.Count < 1)
            {
                Contents.Add( new List<LongSpeakData>() { LongSpeakData });
            }
            else
            {
                Contents[Contents.Count - 1].Add(LongSpeakData);
            }
            LHGFlongDialogLayerControler.AddTextContent(Contents);
            currentLongSpeakData = LongSpeakData;
        }

        public void Next_OnLoad(LayerCommand command)
        {
            LongSpeakData LongSpeakData = new(command);
            bool Continue = LongSpeakData.Continue;
            string Content = LongSpeakData.Content;
            string TextType = LongSpeakData.TextType;
            //string End = command.CommandConfig["End"];
            Content = BuildStringContent(Content, TextType);
            if (TextType == "TexDraw" || TextType == "TMP")
            {
                Content = BuildStringContent(Content, TextType);
                if(TextType == "TexDraw")
                {
                    Content = Utils.TexDarwContentWrapper(Content);
                }
            }
            else if (TextType == "Image")
            {
                Content = Path.Combine(Utils.ResoucePaths.LongSpeakImagePath, Content);
            }
            LongSpeakData.Content = Content;
            if (!Continue || Contents.Count < 1)
            {
                Contents.Add(new List<LongSpeakData>() { LongSpeakData});
            }
            else
            {
                Contents[Contents.Count - 1].Add(LongSpeakData);
            }
            LHGFlongDialogLayerControler.AddTextContent(Contents, IsOnLoad:true);
            currentLongSpeakData = LongSpeakData;
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
            //判断是否加载上一页的内容
            if (Contents.Count == 0)
            {
                return false;
            }
            var temp = Contents[Contents.Count - 1];
            bool IsClear = (temp.Count == 1);
            //如果temp的长度1，则删除Contents的最后一个元素
            if(IsClear)
            {
                Contents.RemoveAt(Contents.Count - 1);
            }
            else
            {
                temp.RemoveAt(temp.Count - 1);
            }
            if (Contents.Count > 0)
            {
                LHGFlongDialogLayerControler.WithdrawTextContent(Contents[Contents.Count - 1], IsClear);
            }
            else if (Contents.Count == 0)
            {
                LHGFlongDialogLayerControler.Clear();
            }
            return true;
        }
        public string LayerName()
        {
            return layerName;
        }

        public GameObject GetControler()
        {
            return LHGFlongDialogLayerControler.gameObject;
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
            string info = currentLongSpeakData.Content;
            string ContentType = currentLongSpeakData.TextType;
            if(ContentType == "Image")
            {
                string Rate = currentLongSpeakData.Rate.ToString();
                Content.Add("TextInfo", info);
                Content.Add("ContentType", "Image");
                Content.Add("Rate", Rate);
            }
            else
            {
                string TextType = currentLongSpeakData.TextType;
                Content.Add("TextType", TextType);
                Content.Add("TextInfo", info);
                Content.Add("ContentType", "Text");
            }
            return Content;
        }

        public void Reset()
        {
            LHGFlongDialogLayerControler.Clear();
            // 停止任何正在播放的文本动画
            if (TextTweener != null && TextTweener.IsActive())
            {
                TextTweener.Kill();
                TextTweener = null;
            }
            Contents = new() { };
        }
        public void BeforeNextOnLoadStart() { }

        public class SaveData
        {
            public List<List<LongSpeakData>> Contents = new() { };
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

            // 深拷贝数据：复制外层列表和内层列表，但 LongSpeakData 对象直接引用（其字段不可变）
            foreach (var innerList in data.Contents)
            {
                Contents.Add(new List<LongSpeakData>(innerList));
            }

            // 设置当前长段数据为最后一个（如果有）
            currentLongSpeakData = Contents.Count > 0 && Contents[Contents.Count - 1].Count > 0
                ? Contents[Contents.Count - 1][Contents[Contents.Count - 1].Count - 1]
                : null;

            // 刷新 UI：清除现有显示，然后重新显示加载后的内容（使用 OnLoad 模式）
            LHGFlongDialogLayerControler.gameObject.SetActive(true);
            LHGFlongDialogLayerControler.Clear();
            if (Contents.Count > 0)
            {
                //LHGFlongDialogLayerControler.AddTextContent(Contents, IsOnLoad: true);
                LHGFlongDialogLayerControler.AddTextContents(Contents[Contents.Count-1]);
            }

            // 停止任何正在播放的文本动画
            if (TextTweener != null && TextTweener.IsActive())
            {
                TextTweener.Kill();
                TextTweener = null;
            }
            LHGFlongDialogLayerControler.gameObject.SetActive(false);
        }

        public object Save()
        {
            var saveData = new SaveData();
            // 深拷贝 Contents：复制外层列表和内层列表（LongSpeakData 直接引用）
            foreach (var innerList in Contents)
            {
                saveData.Contents.Add(new List<LongSpeakData>(innerList));
            }
            return saveData;
        }
    }
}
