using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHGFData;
using System.Linq;
namespace LHGFGameProgress
{
    //虽然不打算专门定义这些接口，但是大体上Layer会分为六种
    //第一种是MainInformationLayer，一般一个场景下只能有一个这种Layer是激活的
    //例如LongSpeak和Speak
    //第二种是ViewLayer，这种Layer一般用来展示图片以及视频动画
    //第三种是SoundLayer，这种Layer一般用来播放音频
    //第四种是BranchLayer，这种Layer一般需要负责选项的跳转
    //第五种是DataLayer，这种Layer一般只负责记录和更新数据
    //如果还有其他没办法归类的就是第六种
    public class LHGF_GameData
    {
        public Dictionary<string, ILayer> layers;
        public Dictionary<string, List<string>> layerCommand2layer;
        public string CurrentScriptName;
        public ScriptGraph scriptGraph;
        public List<string> historyNodes = new() { };


        //单例，方便Layer访问
        //public static LHGF_GameData instance;
        public LHGF_GameData(Dictionary<string, ILayer>  _layers, Dictionary<string, List<string>> _objectCommand2layer)
        {
            layers = _layers;
            layerCommand2layer = _objectCommand2layer;
            //instance = this;
        }
    }
    
    public class LHGF_GameProgress
    {
        public string CurrentNodeId;
        public string NextNodeId;
        public List<Dictionary<string, string>> LogContents;
        //指定游戏开始的节点ID，默认为"FirstScript-MAINBRANCH-0"
        public static string StartNodeId = "FirstScript-MAINBRANCH-0";
        //public static LHGF_GameProgress instance;
        public LHGF_GameProgress(string _CurrentNodeId, string _NextNodeId)
        {
            CurrentNodeId = _CurrentNodeId;
            NextNodeId = _NextNodeId;
        }
        public LHGF_GameProgress()
        {
            //instance = this;
        }
/*        public List<ScriptData> Load(string NodeName, LHGF_GameData data)
        {
            //解析节点名，获取脚本名称
            var NodeInfo = ScriptDatasOperator.GetScriptInfo(NodeName);
            data.CurrentScriptName = NodeInfo["ScriptName"];
            data.scriptGraph = new(data.CurrentScriptName);
            //到达NodeName
            var Nodes = data.scriptGraph.GetShortestPath(NodeName);
            List<ScriptData> CommandsOnLoad = new() { };
            foreach (var Node in Nodes)
            {
                //直到Node==NodeName时停下
                if(Node != NodeName) 
                {
                    //var Time1 = System.DateTime.Now;
                    ForwardOnLoad(data, Node); 
                    //var Time2 = System.DateTime.Now;
                    //Debug.Log("Load Node "+Node+" Time: "+(Time2-Time1).TotalSeconds);
                }
            }
            CurrentNodeId = NodeName;
            NextNodeId = NodeName;
            return CommandsOnLoad;
        }*/
        public string Next(string Node, LHGF_GameData data)
        {
            //默认的下一个顶点为图的下一个顶点的第一个顶点
            var OutEdges = data.scriptGraph.OutEdges(Node).ToList();
            var NextNodes = new List<string>() { };
            foreach (var OutEdge in OutEdges)
            {
                NextNodes.Add(OutEdge.Target);
            }
            NextNodeId = NextNodes[0];
            return NextNodeId;
        }
        public void OnGlobalGameStart(LHGF_GameData data)
        {
            foreach (string LayerName in data.layers.Keys)
            {
                data.layers[LayerName].OnStart();
            }
        }
        public void OnGameStart(LHGF_GameData data)
        {
            var NodeInfo = ScriptDatasOperator.GetScriptInfo(StartNodeId);
            data.CurrentScriptName = NodeInfo["ScriptName"];
            data.scriptGraph = new(data.CurrentScriptName);
            CurrentNodeId = StartNodeId;
            NextNodeId = StartNodeId;
        }
        public void OnGameUpdata(LHGF_GameData data)
        {
            foreach (string LayerName in data.layers.Keys)
            {
                data.layers[LayerName].OnUpdate();
            }
        }
        public void OnGameLoadFinish(LHGF_GameData data)
        {
            foreach (string LayerName in data.layers.Keys)
            {
                data.layers[LayerName].OnLoadFinish();
            }
        }
        public Dictionary<string, object> OnGameSave(LHGF_GameData data)
        {
            Dictionary<string, object> LayerDatas = new Dictionary<string, object>();
            foreach(string LayerName in data.layers.Keys)
            {
               object layerData = data.layers[LayerName].Save();
                LayerDatas.Add(LayerName, layerData);
            }
            return LayerDatas;
        }
        public void Load(string NodeName, LHGF_GameData data, Dictionary<string, object> LayerDatas)
        {
            var NodeInfo = ScriptDatasOperator.GetScriptInfo(NodeName);
            data.CurrentScriptName = NodeInfo["ScriptName"];
            data.scriptGraph = new(data.CurrentScriptName);
            //加载层的数据
            foreach (string LayerName in data.layers.Keys)
            {
                object layerData = LayerDatas[LayerName];
                data.layers[LayerName].Load(layerData);
            }
            //记录历史节点
            var Nodes = data.scriptGraph.GetShortestPath(NodeName);
            data.historyNodes.AddRange(Nodes);
            CurrentNodeId = NodeName;
            NextNodeId = Next(CurrentNodeId, data);
        }
/*        public void ForwardOnLoad(LHGF_GameData data, string NextNodeId)
        {
            foreach (string layerName in data.layers.Keys)
            {
                data.layers[layerName].BeforeNextOnLoadStart();
            }
            CurrentNodeId = NextNodeId;
            data.historyNodes.Add(CurrentNodeId);
            var ScriptData = data.scriptGraph[CurrentNodeId];
            foreach (var command in ScriptData.LayerCommands)
            {
                //根据command的名字寻找command需要作用的层
                var LayNames = data.layerCommand2layer[command.CommandName];
                foreach (string LayerName in LayNames)
                {
                    data.layers[LayerName].Next_OnLoad(command);
                }
            }
        }*/
        //游戏流程：更新当前顶点->通过Next设置NextNodeID->
        //在执行Layer的更新时还有可能更新NextNodeID
        //
        public void Forward(LHGF_GameData data)
        {
            //在执行Next前，先对每个层执行BeforeNextStart
            foreach(string layerName in data.layers.Keys)
            {
                data.layers[layerName].BeforeNextStart();
            }
            CurrentNodeId = NextNodeId;
            //检查NextNodeId是否还在当前脚本中
            var NodeInfo = ScriptDatasOperator.GetScriptInfo(CurrentNodeId);
            if(data.CurrentScriptName != NodeInfo["ScriptName"])
            {
                data.CurrentScriptName = NodeInfo["ScriptName"];
                data.scriptGraph = new(data.CurrentScriptName);
            }
            data.historyNodes.Add(CurrentNodeId);
            NextNodeId = Next(CurrentNodeId, data);
            var ScriptData = data.scriptGraph[CurrentNodeId];
            LogContents = new();
            foreach (var command in ScriptData.LayerCommands)
            {
                //根据command的名字寻找command需要作用的层
                var LayNames = data.layerCommand2layer[command.CommandName];
                foreach(string LayerName in LayNames)
                {
                    data.layers[LayerName].Next(command);
                    LogContents.Add(data.layers[LayerName].Log(command));
                }
            }
        }
        public void Withdraw(LHGF_GameData data)
        {
            Skip(data);
            var historyNodes = data.historyNodes;
            int lastIndex = historyNodes.Count-1;
            //只有lastIndex中出现的命令才会执行回撤
            if (historyNodes.Count > 1)
            {
                NextNodeId = historyNodes[lastIndex];
                CurrentNodeId = historyNodes[lastIndex-1];
                historyNodes.RemoveAt(lastIndex);
                var commands = data.scriptGraph[NextNodeId].LayerCommands;
                foreach(var command in commands)
                {
                    var layerNames = data.layerCommand2layer[command.CommandName];
                    foreach (string LayerName in layerNames)
                    {
                        data.layers[LayerName].Withdraw();
                    }
                }
            }
        }
        public bool IsFinish(LHGF_GameData data)
        {
            bool isFinish = true;
            foreach(var layer in data.layers.Values)
            {
                isFinish = isFinish&&layer.Finish();
            }
            return isFinish;
        }
        public void Skip(LHGF_GameData data)
        {
            foreach (var layer in data.layers.Values)
            {
                layer.Skip();
            }
        }
        public void Clear(LHGF_GameData data)
        {
            foreach(var layer in data.layers.Values)
            {
                layer.Reset();
            }
        }
        
/*        public string GetMainInformationLayerType(LHGF_GameData data, string nodeId)
        {
            List<string> MainInformationLayerTypeList =
                new List<string> {"LongSpeak", "Speak" };
            string MainInformationLayerType = "Speak";
            var Commands = data.scriptGraph[nodeId].ObjectCommands;
            foreach(var command in Commands)
            {
                if (MainInformationLayerTypeList.Contains(command.CommandName))
                {
                    MainInformationLayerType = command.CommandName;
                }
            }
            return MainInformationLayerType;
        }*/
    }
}
