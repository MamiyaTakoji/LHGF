using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;
using QuikGraph;
using System.Linq;
using QuikGraph.Algorithms.ShortestPath;
using System;
using System.IO;
using QuikGraph.Algorithms;
using Newtonsoft.Json;
using System.Collections;
using XLua;
using System.Text;

namespace LHGFData
{
    public static class Utils
    {
        public static class ResoucePaths
        {
            public static string PortraitPath = Path.Combine(Application.streamingAssetsPath, "HGF", "Texture2D", "Portrait");
            public static string CharacterJsonPath = Path.Combine(Application.streamingAssetsPath, "HGF", "CharacterInfo.json");
            //public static string SavePath = Path.Combine(Application.persistentDataPath, "savegame.sav");
            public static string BackgroundPath = Path.Combine(Application.streamingAssetsPath, "HGF", "Texture2D", "BackgroundImage");
            public static string AudioPath = Path.Combine(Application.streamingAssetsPath, "HGF", "Audio");
            public static string BgmPath = Path.Combine(Application.streamingAssetsPath, "HGF", "Audio", "BGM");
            public static string CVPath = Path.Combine(Application.streamingAssetsPath, "HGF", "Audio", "CharacterVoice");
            public static string EffectiveSoundPath = Path.Combine(Application.streamingAssetsPath, "HGF", "Audio", "EffectiveSound");
            public static string ImageAnimationPath = Path.Combine(Application.streamingAssetsPath, "HGF", "Texture2D", "ImageAnimation");
            public static string VideoAnimationPath = Path.Combine(Application.streamingAssetsPath, "HGF", "VideoResource", "VideoAnimation");
            public static string LongSpeakImagePath = Path.Combine(Application.streamingAssetsPath, "HGF", "Texture2D", "LongSpeakImage");
            public static string DOTweenLuaCommandPath = Path.Combine(Application.streamingAssetsPath, "HGF", "xLua", "xLuaDOTween");
            public static string ScriptPath = Path.Combine(Application.streamingAssetsPath, "HGF", "ScriptSheets");
            public static string StartMenuConfigPath = Path.Combine(Application.streamingAssetsPath, "HGF", "StartMenuSetting");
            public static string EvidenceContentPath = Path.Combine(Application.streamingAssetsPath, "HGF", "EvidenceInfo");
            public static string FullBodyImagePath = Path.Combine(Application.streamingAssetsPath, "HGF", "Texture2D", "FullbodyPortrait");
            public static string GameRootPath
            {
                get
                {
                #if UNITY_ANDROID
                                    return Application.persistentDataPath;
                #else
                        // Windows, macOS, Linux 等平台使用原来的逻辑
                        return Directory.GetParent(Application.dataPath).FullName;
                #endif
                }
            }
            public static string SaveDataPath = Path.Combine(GameRootPath, "SaveData");
        }
        public class GameConfig<T>
        //一般T=Dictionary<string, string>
        {
            public Dictionary<string, T> dataDict = new() { };
            public GameConfig(string jsonPath)
            {
                string jsonContent = ReadTextSync(jsonPath);
                 dataDict = JsonConvert.DeserializeObject<Dictionary<string, T>>(jsonContent);
            }            
        }
        public static T DeepClone<T>(T source)
        {
            if (source == null)
                return default(T);

            // 1. 序列化
            string json = JsonConvert.SerializeObject(source);

            // 2. 反序列化
            return JsonConvert.DeserializeObject<T>(json);
        }
        public static bool string2bool(string s)
        {
            if (s == "0")
            {
                return false;
            }
            else if (s == "1")
            {
                return true;
            }
            else
            {
                throw new ArgumentException(
                $"Input must be '0' or '1'. Received: '{s}'.",
                nameof(s)
                );
            }
        }
        public static string ReadTextSync(string path)
        {
#if UNITY_ANDROID
            // Android 平台：使用 UnityWebRequest 同步读取
            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                request.SendWebRequest();
                while (!request.isDone) { } // 同步等待完成
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    Debug.LogError($"ReadTextSync 失败 (Android): {request.error} 路径: {path}");
                    return null;
                }
            }
#else
        // Windows / macOS / Linux 等平台：直接使用 File.ReadAllText
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception e)
        {
            Debug.LogError($"ReadTextSync 失败: {e.Message} 路径: {path}");
            return null;
        }
#endif
        }
#if UNITY_ANDROID
        // ------------------- 纹理加载优化：缓存 + 降采样 + 不可读（仅 Android） -------------------
        // 缓存上限：视觉小说中活跃的背景/立绘通常不超过二三十张，48 张足够覆盖常用资源
        public const int TextureCacheCapacity = 48;
        // 纹理最大边长：手机屏幕通常不超过 1440p，2560 已足够清晰，同时大幅降低显存占用
        public const int MaxTextureSize = 2560;

        private static readonly Dictionary<string, Sprite> textureCache = new Dictionary<string, Sprite>(StringComparer.Ordinal);
        // 记录缓存使用顺序，索引 0 为最近使用，用于 LRU 淘汰
        private static readonly List<string> cacheOrder = new List<string>();

        private static void TouchCache(string path)
        {
            cacheOrder.Remove(path);
            cacheOrder.Insert(0, path);
        }

        private static void CacheTexture(string path, Sprite sprite)
        {
            if (textureCache.ContainsKey(path))
            {
                textureCache[path] = sprite;
                TouchCache(path);
                return;
            }
            textureCache[path] = sprite;
            cacheOrder.Insert(0, path);
            // 超出容量时淘汰最久未使用的条目。
            // 不主动 Destroy：若场景仍在引用则保留，否则由引用计数在 Resources.UnloadUnusedAssets 时释放
            while (cacheOrder.Count > TextureCacheCapacity)
            {
                string last = cacheOrder[cacheOrder.Count - 1];
                cacheOrder.RemoveAt(cacheOrder.Count - 1);
                textureCache.Remove(last);
            }
        }

        /// <summary>
        /// 清除纹理缓存（可在章节切换时调用，释放不再需要的纹理）
        /// </summary>
        public static void ClearTextureCache()
        {
            textureCache.Clear();
            cacheOrder.Clear();
        }

        /// <summary>
        /// 将超尺寸纹理降采样到 MaxTextureSize 以内，减少 GPU 显存与带宽占用。
        /// 若尺寸未超限则原样返回；若发生缩放，源纹理会被销毁，请使用返回的新纹理。
        /// </summary>
        private static Texture2D DownscaleTexture(Texture2D source, int maxSize)
        {
            int sw = source.width;
            int sh = source.height;
            if (sw <= maxSize && sh <= maxSize)
            {
                return source;
            }

            float scale = Mathf.Min(1f, (float)maxSize / Mathf.Max(sw, sh));
            int tw = Mathf.Max(1, Mathf.RoundToInt(sw * scale));
            int th = Mathf.Max(1, Mathf.RoundToInt(sh * scale));

            RenderTexture rt = RenderTexture.GetTemporary(tw, th, 0, RenderTextureFormat.ARGB32);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            Texture2D result = new Texture2D(tw, th, TextureFormat.RGBA32, false);
            result.filterMode = FilterMode.Bilinear;
            result.ReadPixels(new Rect(0, 0, tw, th), 0, 0);
            result.Apply(false, true);

            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(rt);
            UnityEngine.Object.Destroy(source);
            return result;
        }

        /// <summary>
        /// 将图片字节解码为 Texture2D：标记不可读（不保留 CPU 端副本，显著降低内存占用），超尺寸自动降采样
        /// </summary>
        private static Texture2D DecodeTextureFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("读取到的字节数组为空");
                return null;
            }

            Texture2D texture = new Texture2D(2, 2);
            texture.filterMode = FilterMode.Bilinear;
            if (!texture.LoadImage(bytes, true)) // markNonReadable=true：纹理仅保留 GPU 副本
            {
                Debug.LogError("LoadImage 失败，数据可能不是有效图片");
                return null;
            }

            return DownscaleTexture(texture, MaxTextureSize);
        }
#endif

        /// <summary>
        /// 同步加载图片。
        /// Android：使用优化方案（缓存 + 降采样 + 不可读），同一路径只解码一次，之后直接命中缓存；
        /// Windows 及其他平台：保持原始逻辑，行为与修改前完全一致。
        /// </summary>
        public static Sprite LoadTextureByIO(string path)
        {
#if UNITY_ANDROID
            // ==================== Android：优化方案（缓存 + 降采样 + 不可读） ====================
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (textureCache.TryGetValue(path, out Sprite cached))
            {
                if (cached != null)
                {
                    TouchCache(path);
                    return cached;
                }
                // 缓存对象已被外部销毁，移除后重新加载
                textureCache.Remove(path);
                cacheOrder.Remove(path);
            }

            byte[] bytes = null;
            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                request.SendWebRequest();
                // 同步等待（首次加载会阻塞主线程，命中缓存后不再阻塞）
                // Sleep(1) 让出 CPU 时间片，避免空转占满核心
                while (!request.isDone) { System.Threading.Thread.Sleep(1); }
                if (request.result == UnityWebRequest.Result.Success)
                {
                    bytes = request.downloadHandler.data;
                }
                else
                {
                    Debug.LogError($"Android 读取失败: {request.error} 路径: {path}");
                    return null;
                }
            }

            Texture2D texture = DecodeTextureFromBytes(bytes);
            if (texture == null)
            {
                return null;
            }

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            CacheTexture(path, sprite);
            return sprite;
#else
            // ==================== Windows 及其他平台：原始逻辑（与修改前一致） ====================
            byte[] bytes = null;

#if UNITY_STANDALONE_WIN
            // ------------------- Windows 平台：使用 FileStream 直接读取 -------------------
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Windows 读取失败: {e.Message} 路径: {path}");
                return null;
            }
#else
            // 其他平台（可选）可自行补充，这里简单用 UnityWebRequest 兜底
            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                request.SendWebRequest();
                while (!request.isDone) { }
                if (request.result == UnityWebRequest.Result.Success)
                    bytes = request.downloadHandler.data;
                else
                    return null;
            }
#endif

            // ------------------- 公共部分：将字节数组转换为 Sprite -------------------
            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("读取到的字节数组为空");
                return null;
            }

            Texture2D texture = new Texture2D(2, 2);
            texture.filterMode = FilterMode.Bilinear;
            if (texture.LoadImage(bytes))
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogError("LoadImage 失败，数据可能不是有效图片");
                return null;
            }
#endif
        }

#if UNITY_ANDROID
        /// <summary>
        /// 异步加载图片（协程），用于预加载：不阻塞主线程，加载完成后回调返回 Sprite 并写入缓存。
        /// 示例：StartCoroutine(Utils.LoadTextureByIOAsync(path, sprite => { img.sprite = sprite; }));
        /// </summary>
        public static IEnumerator LoadTextureByIOAsync(string path, Action<Sprite> onComplete = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(null);
                yield break;
            }
            if (textureCache.TryGetValue(path, out Sprite cached))
            {
                if (cached != null)
                {
                    TouchCache(path);
                    onComplete?.Invoke(cached);
                    yield break;
                }
                textureCache.Remove(path);
                cacheOrder.Remove(path);
            }

            byte[] bytes = null;
            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    bytes = request.downloadHandler.data;
                }
                else
                {
                    Debug.LogError($"异步读取失败: {request.error} 路径: {path}");
                    onComplete?.Invoke(null);
                    yield break;
                }
            }

            Texture2D texture = DecodeTextureFromBytes(bytes);
            if (texture == null)
            {
                onComplete?.Invoke(null);
                yield break;
            }

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            CacheTexture(path, sprite);
            onComplete?.Invoke(sprite);
        }
#endif
        public static void SetRelativeSize(RectTransform rt, float relativeSize)
        {
            // 获取父对象尺寸（若父对象是Canvas）
            RectTransform parent = rt.parent as RectTransform;
            float parentWidth = parent.rect.width;
            float parentHeight = parent.rect.height;
            float Width = rt.rect.width;
            float Height = rt.rect.height;
            // 计算子对象尺寸（基于父对象的比例）
            //按宽度放缩
            Vector2 size = new Vector2(parentWidth * relativeSize,
                                      parentWidth * relativeSize / Width * Height);

            // 设置尺寸
            rt.sizeDelta = size;
        }
        public static void SetRelativeSize(RectTransform rt, Vector2 relativeSize)
        {
            // 获取父对象尺寸（若父对象是Canvas）
            RectTransform parent = rt.parent as RectTransform;
            float parentWidth = parent.rect.width;
            float parentHeight = parent.rect.height;
/*            float Width = rt.rect.width;
            float Height = rt.rect.height;*/
            // 计算子对象尺寸（基于父对象的比例）
            Vector2 size = new Vector2(parentWidth * relativeSize[0],
                                      parentHeight * relativeSize[1]);

            // 设置尺寸
            rt.sizeDelta = size;
        }
        public static void SetSingleAnchorPoint(RectTransform rt, Vector2 relativePosition)
        {
            // 保存当前轴点设置
            Vector2 originalPivot = rt.pivot;

            // 设置锚点为同一点，表示定位基于此点
            rt.anchorMin = relativePosition;
            rt.anchorMax = relativePosition;

            // 重置位置偏移
            rt.anchoredPosition = Vector2.zero;

            // 恢复原始轴点
            rt.pivot = originalPivot;
        }
        public static class xLuaEnv
        {
            private static LuaEnv lua_env;
            public static LuaEnv luaEnv
            {
                get
                {
                    if (lua_env == null)
                    {
                        lua_env = new LuaEnv();
                        return lua_env;
                    }
                    else
                    {
                        return lua_env;
                    }
                }
            }
        }
        public static IEnumerator LoadDOTweenLuaCommandsAsync(System.Action onComplete = null)
        {
            DOTweenLuaCommandDic = new Dictionary<string, string>();

#if UNITY_ANDROID
            // Android: 读取文件清单
            string listPath = Path.Combine(ResoucePaths.DOTweenLuaCommandPath, "lua_files.txt");
            using (UnityWebRequest listRequest = UnityWebRequest.Get(listPath))
            {
                yield return listRequest.SendWebRequest();
                if (listRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"无法读取lua文件清单: {listRequest.error}");
                    onComplete?.Invoke();
                    yield break;
                }

                string[] fileNames = listRequest.downloadHandler.text.Split('\n');
                foreach (string fileName in fileNames)
                {
                    string trimmed = fileName.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    string filePath = Path.Combine(ResoucePaths.DOTweenLuaCommandPath, trimmed);
                    using (UnityWebRequest fileRequest = UnityWebRequest.Get(filePath))
                    {
                        yield return fileRequest.SendWebRequest();
                        if (fileRequest.result == UnityWebRequest.Result.Success)
                        {
                            string content = fileRequest.downloadHandler.text;
                            string key = Path.GetFileNameWithoutExtension(trimmed);
                            DOTweenLuaCommandDic[key] = content;
                        }
                        else
                        {
                            Debug.LogError($"加载lua文件失败: {filePath}, 错误: {fileRequest.error}");
                        }
                    }
                }
            }
#else
        // Windows / 其他平台：直接枚举目录
        string[] txtFiles = Directory.GetFiles(
            ResoucePaths.DOTweenLuaCommandPath,
            "*.lua",
            SearchOption.AllDirectories
        );
        foreach (string txtFile in txtFiles)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(txtFile);
            string content = File.ReadAllText(txtFile);
            DOTweenLuaCommandDic.Add(fileNameWithoutExtension, content);
        }
#endif

            onComplete?.Invoke();
            yield return null;
        }
        public static void SetDOTweenLuaCommandDic()
        {
            /*            DOTweenLuaCommandDic = new() { };
                        string[] txtFiles = Directory.GetFiles(
                                ResoucePaths.DOTweenLuaCommandPath,
                                "*.lua",
                                SearchOption.AllDirectories
                            );
                        foreach (string txtFile in txtFiles)
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(txtFile);
                            string Content = File.ReadAllText(txtFile);
                            DOTweenLuaCommandDic.Add(fileNameWithoutExtension, Content);
                        }*/
            var routine = LoadDOTweenLuaCommandsAsync();
            var coroutine = LHGFGameMain.instance.StartCoroutine(routine); // 假设有启动协程的辅助方法
        }
        public static Dictionary<string, string> DOTweenLuaCommandDic;
        public static string ShowListInfo<T>(List<T> list)
        {
            string info = "";
            foreach (var item in list)
            {
                info += (" " + item.ToString());
            }
            return info;
        }
        public static Value GetDicValue<Key,Value>(Dictionary<Key, Value> dic, Key key,Value defaultValue)
        {
            return dic.ContainsKey(key) ? dic[key] : defaultValue;
        }
        public static string TexDarwContentWrapper(string input)
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
        private static bool IsChineseChar(char c)
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
    public class LayerCommand
    {
        public string CommandName;
        public Dictionary<string, string> CommandConfig = new() { };
        public LayerCommand() { }
        public LayerCommand(string _CommandName)
        {
            CommandName = _CommandName;
        }
    }
    //这个类负责保存脚本有关的信息
    public class ScriptData
    {
        public List<LayerCommand> LayerCommands = new List<LayerCommand>() { };
        public static string FinishFlag = "GAMEFINISH";
        public ScriptData(XElement PlotNode)
        {
            SetCommands(PlotNode);
        }
        public void SetCommands(XElement PlotNode)
        {
            foreach(var cmd in PlotNode.Elements()) 
            {
                LayerCommands.Add(GetObjectCommand(cmd));
            }
        }
        public LayerCommand GetObjectCommand(XElement command)
        {
            string commandname = command.Name.ToString();
            LayerCommand commandObj = new LayerCommand(commandname);
            foreach (var commandConfig in command.Attributes())
            {
                commandObj.CommandConfig[commandConfig.Name.ToString()] = commandConfig.Value;
                //Debug.Log(commandConfig.Name.ToString() + ":" + commandConfig.Value);
            }
            return commandObj;
        }
    }
    public class stringPairEdge : IEdge<string>
    {
        public stringPairEdge(string _source, string _target)
        {
            source = _source;target = _target;
        }
        public string Source { get { return source; }set { source = value; } }

        public string Target { get { return target; } set { target = value; } }
        public string source;
        public string target;
    }
    public class ScriptGraph : AdjacencyGraph<string, stringPairEdge>
    {
        public string ScriptName;
        public Dictionary<string, ScriptData> ScriptDatas = new() { };
        public ScriptData this[string nodeId]
        {
            //如果nodeId不在ScriptDatas中，则重新加载ScriptGraph
            get 
            {
                if (!ScriptDatas.ContainsKey(nodeId))
                {
                    var NodeInfo = ScriptDatasOperator.GetScriptInfo(nodeId);
                    string CurrentScriptName = NodeInfo["ScriptName"];
                    string ScriptPath = Path.Combine(Utils.ResoucePaths.ScriptPath, CurrentScriptName + ".xml");
                    string XMLContent = XMLUtil.SyncLoadXML(ScriptPath);
                    ScriptDatasOperator.LoadScriptDatas(this, XMLContent);
                }
                return ScriptDatas[nodeId]; 
            }
            set => ScriptDatas[nodeId] = value;
        }
        public ScriptGraph(string _ScriptName)
        {
            ScriptName = _ScriptName;
            string ScriptPath = Path.Combine(Utils.ResoucePaths.ScriptPath, ScriptName + ".xml");
            string XMLContent = XMLUtil.SyncLoadXML(ScriptPath);
            ScriptDatasOperator.LoadScriptDatas(this, XMLContent);
        }
        public List<string> GetShortestPath(string TargetNode)
        {
            Func<stringPairEdge, double> edgeWeights = edge => 1;
            string StartNode = ScriptDatasOperator.StartNode(ScriptName);
            var tryGetPath = this.ShortestPathsDijkstra(edgeWeights, StartNode);
            List<string> pathNode = new List<string>() { StartNode };
            //如果StartNode就是TargetNode，那么path就是自身
            tryGetPath(TargetNode, out var path);
            if (path != null) 
            {
                foreach (var _path in path)
                {
                    pathNode.Add(_path.Target);
                }
            }
/*            foreach(var Node in pathNode)
            {
                Debug.Log(Node);
            }*/
            return pathNode;
        }
        public List<ScriptData> Load(string TargetNode)
        {
            var pathNode = GetShortestPath(TargetNode);
            List<ScriptData> scriptDatas = new List<ScriptData>();
            foreach(var node in pathNode)
            {
                scriptDatas.Add(ScriptDatas[node]);
            }
            return scriptDatas;
        }
    }
    //还是xml吧   
    //处理脚本数据的具体方法在这里实现
    public static class ScriptDatasOperator
    {
        public static string StartNodeId = "0";
        public static string StartBranchName = "MAINBRANCH";
        public static string StartNode(string ScriptName)
        {
            return ScriptName + "-" + StartBranchName + "-" + StartNodeId;
        }
        public static void LoadScriptDatas(ScriptGraph scriptGraph, string XMLContent)
        {
            var ScriptDoc = XDocument.Parse(XMLContent);
            foreach (var item in ScriptDoc.Root.Elements())
            {
                if(item.Name.ToString() == "MainPlot")
                {
                    foreach (var MainPlotItem in item.Elements())
                    {
                        if(MainPlotItem.Name.ToString() == "PlotNode")
                        {
                            var ScriptData = new ScriptData(MainPlotItem);
                            var CurrentNodeId = MainPlotItem.Attribute("Id").Value;
                            scriptGraph.ScriptDatas.Add
                                (key: CurrentNodeId,
                                value: ScriptData);
                            //注意，这种添加方式可能会导致图结构中存在某个顶点
                            //但是字典中并不存在这个顶点，这种情况下需要另外处理
                            if (!scriptGraph.ContainsVertex(CurrentNodeId))
                            {
                                scriptGraph.AddVertex(CurrentNodeId);
                            }
                            string NextIDs = MainPlotItem.Attribute("NextId").Value;
                            var NextIDList = NextIDs.Split("%%").ToList();
                            foreach(string stringNode in NextIDList)
                            {
                                if (!scriptGraph.ContainsVertex(stringNode))
                                {
                                    scriptGraph.AddVertex(stringNode);
                                }
                                stringPairEdge stringPairEdge =
                                    new(_source: CurrentNodeId, _target: stringNode);
                                scriptGraph.AddEdge(stringPairEdge);
                            }
                        }
                    }
                }
            }
        }
        //目前，剧本的节点的命名方式为
        //剧本名-分支名-分支名中的编号
        public static Dictionary<string,string> GetScriptInfo(string NodeName)
        {
            List<string> ScriptInfo = NodeName.Split("-").ToList();
            Dictionary<string, string> ScriptInfoDic = new Dictionary<string,string>();
            ScriptInfoDic["ScriptName"] = ScriptInfo[0];
            ScriptInfoDic["BranchName"] = ScriptInfo[1];
            string BranchID = string.Empty;
            for (int i = 2;i<ScriptInfo.Count;i++)
            {
                if (i == 2) 
                {
                    BranchID += ScriptInfo[i]; 
                }
                else
                {
                    BranchID += "-"+ScriptInfo[i];
                }
                
            }
            ScriptInfoDic["BranchID"] = BranchID;
            return ScriptInfoDic;
        }
    }
    public static class XMLUtil
    {
        public static string SyncLoadXML(string path)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(path))
            {
                www.SendWebRequest();
                while (!www.isDone) { }
                return www.downloadHandler.text;
            }
        }
    }
    public interface ILayer
    {
        //可能不应该把这个设成接口
        public string LayerName();
        //在游戏开始时调用
        public void OnStart();
        //在游戏更新时调用
        public void OnUpdate();
        //在回撤时调用
        public bool Withdraw();
        //在跳转到下一个节点时调用
        public void Next(LayerCommand command);

        //在加载模式下跳转到下一个节点时调用
        //这个方法暂时是不需要的不过先留着吧，万一以后需要在加载模式下跳转到下一个节点时做一些特殊处理呢
        /*public void Next_OnLoad(LayerCommand command);*/

        //是否播放或者加载完成
        public bool Finish();
        //跳过
        public void Skip();
        //记录信息
        public Dictionary<string,string> Log(LayerCommand command);
        //返回控制器
        public GameObject GetControler();
        //游戏加载完成后调用
        public void OnLoadFinish();
        //Next开始前调用
        public void BeforeNextStart();
        //Next_OnLoad开始前调用
        public void BeforeNextOnLoadStart();
        //重置层现有的数据和游戏对象
        public void Reset();
        //根据层的存档数据重现层的数据和控制器状态
        public void Load(object SaveData);
        //保存层的数据和控制器的状态
        public object Save();
    }
    public static class DictionaryExtensions
    {
        public static float GetFloat(this Dictionary<string, string> dict, string key, float defaultValue = 0f)
        {
            return dict.ContainsKey(key) && float.TryParse(dict[key], out float result) ? result : defaultValue;
        }

        public static int GetInt(this Dictionary<string, string> dict, string key, int defaultValue = 0)
        {
            return dict.ContainsKey(key) && int.TryParse(dict[key], out int result) ? result : defaultValue;
        }

        public static bool GetBool(this Dictionary<string, string> dict, string key, bool defaultValue = false)
        {
            if (!dict.ContainsKey(key)) return defaultValue;

            string value = dict[key].ToLower();
            return value == "true" || value == "1" || value == "yes";
        }

        public static string GetString(this Dictionary<string, string> dict, string key, string defaultValue = "")
        {
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }
    }
    public class Buffer<T>
    {
        public List<T> list;
        public int BufferSize;
        public int CurrentIndex;
        public T LastItem
        {
            get
            {
                return list[CurrentIndex % BufferSize];
            }
        }
        public Buffer(int _BufferSize)
        {
            BufferSize = _BufferSize;
            CurrentIndex = 0;
        }
        public void Add(T item)
        {
            if (list.Count < BufferSize)
            {
                list.Add(item);
            }
            else
            {
                list[CurrentIndex % BufferSize] = item;
            }
            CurrentIndex += 1;
        }
        public void RemoveLast()
        {
            CurrentIndex -= 1;
        }
    }
}
