using DG.Tweening;
using LHGFData;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static LHGFData.ImageCILayer;
using static LHGFData.ImageLayer;
using static LHGFData.Utils;
//CI是character illustration的缩写
namespace LHGFData
{
    public class ImageCILayer : ImageLayer, ILayer
    {
        public Dictionary<string, ImageCIData> ImageDataDic = new() { };
        public Dictionary<string, string> DOTweenLuaCommandDic;
        public List<string> imageHistoryCharacterId = new() { };
        //public List<(ImageData data, string CharacterId)> imageHistoryData = new() { };
        public Dictionary<string, List<ImageData>> imageHistoryDataDic = new() { };
        public Sequence imageAnimations = null;
        public LHGFImageCILayerControler ImageCILayerControler;
        public DataLayer dataLayer;
        //public float defaultWideth = 700;
        public class ImageCIData
        {
            public ImageData imageData;
            public string DOTweenAnimation;
            // 必须添加的无参构造函数
            public ImageCIData() { }
            public ImageCIData(Dictionary<string, string> config, DataLayer dataLayer)
            {
                string characterFrom = config["From"];
                string CIName = string.Empty;
                if (config.ContainsKey("ImageCharacterIllustration"))
                {
                    CIName = config["ImageCharacterIllustration"];
                }
                else
                {
                    CIName = dataLayer.characterData.CharacterInfo.dataDict[characterFrom]["Portrait-Normall"];
                }
                string ResourcePath = dataLayer.characterData.CharacterInfo.dataDict[characterFrom]["ResourcesPath"];
                string CIPath = Path.Combine(LHGFData.Utils.ResoucePaths.PortraitPath, ResourcePath, CIName);
                config["texturePath"] = CIPath;
                imageData = new(config);
                DOTweenAnimation = config.ContainsKey("DOTweenAnimation") ? config["DOTweenAnimation"] : "SetAt";
            }
        }
        public bool Finish()
        {
            if (imageAnimations == null || !imageAnimations.IsActive())
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
            return ImageCILayerControler.gameObject;
        }

        public string LayerName()
        {
            return "ImageCILayer";
        }

        //在Next这里需要同时处理CharacterAnimate和AddCharacter两个命令
        public void Next(LayerCommand command)
        {
            string characterId = command.CommandConfig.ContainsKey("CharacterID") ?
         command.CommandConfig["CharacterID"] : command.CommandConfig["From"];
            if (!ImageDataDic.ContainsKey(characterId))
            {
                ImageCIData _imageCIData = new(command.CommandConfig, dataLayer);
                ImageDataDic.Add(characterId, _imageCIData);
            }
            var imageCIData = ImageDataDic[characterId];
            string DOTweenAnimation = command.CommandConfig.ContainsKey("DOTweenAnimation") ? command.CommandConfig["DOTweenAnimation"] : "SetAt";
            string DOTweenAnimationCommand = GetDOTweenLuaCommandDic()[DOTweenAnimation];
            //Debug.Log(LHGFData.Utils.DOTweenLuaCommandDic);
            GameObject G = null;
            if (!ImageCILayerControler.CharacterImageDic.ContainsKey(characterId))
            {
                G = SetImage(imageCIData.imageData, ImageCILayerControler.transform, ImageCILayerControler.CharacterImagePerfab);
                //Vector2 SizeDelta = imageCIData.imageData.sprite.rect.size;
                //G.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultWideth, SizeDelta.y);
                ImageCILayerControler.CharacterImageDic.Add(characterId, G);
            }
            else
            {
                G = ImageCILayerControler.CharacterImageDic[characterId];
            }
            //Debug.Log(G.GetComponent<RectTransform>().sizeDelta);
            var imageAnimationPlayer = new ImageAnimationPlayer
                (DOTweenAnimationCommand, new Dictionary<string, string>(command.CommandConfig), G);
            imageAnimationPlayer.AddScope("ImageCILayer", this);
            imageAnimationPlayer.Config.Add("Mode", "OnPlay");
            imageAnimationPlayer.Play();
            //imageAnimationPlayer.sequence.OnComplete(()=> { } );
            imageAnimationPlayer.sequence.onComplete +=
                () =>
                {
                    ImageData d = new ImageData(imageAnimationPlayer.G);
                    if (imageHistoryDataDic.ContainsKey(characterId))
                    {
                        imageHistoryDataDic[characterId].Add(d);
                    }
                    else
                    {
                        imageHistoryDataDic[characterId] = new List<ImageData>() { d };
                    }
                    //如果角色立绘发生了变化，则需要记录变化后立绘的路径
                    if (command.CommandConfig.ContainsKey("ImageCharacterIllustration"))
                    {
                        var fromDic = dataLayer.characterData.CharacterID2CharacterInfo;
                        var fromInfo = fromDic[characterId];
                        var from = fromInfo.From;
                        var resourceInfo = dataLayer.characterData.CharacterInfo.dataDict[from];
                        var resourcePath = resourceInfo["ResourcesPath"];
                        var ciName = command.CommandConfig["ImageCharacterIllustration"];
                        var ciPath = Path.Combine(Utils.ResoucePaths.PortraitPath, resourcePath, ciName);
                        d.texturePath = ciPath;
                        //同时也要更改游戏对象的texturePath路径
                        imageAnimationPlayer.G.GetComponent<GameObjectAdditionalInfo>().AdditionalInfo["texturePath"] = ciPath;
                    }
                };
            imageHistoryCharacterId.Add(characterId);
            //获取一下和上一个动画之间的连接方式
            string ConnectMethod = command.CommandConfig.ContainsKey("ConnectMethod") ?
         command.CommandConfig["ConnectMethod"] : "Append";
            if (imageAnimations == null || !imageAnimations.IsActive())
            {
                imageAnimations = imageAnimationPlayer.sequence;
            }
            else
            {
                if (ConnectMethod == "Append")
                {
                    imageAnimations.Append(imageAnimationPlayer.sequence);
                }
                else if (ConnectMethod == "Join")
                {
                    imageAnimations.Join(imageAnimationPlayer.sequence);
                }
                else
                {
                    throw new ArgumentException($"无效的ConnectMethod值: {ConnectMethod}。" +
                     $"只支持'Append'或'Join'。", nameof(ConnectMethod));
                }
            }
        }
        

        public void Next_OnLoad(LayerCommand command)
        {
            string characterId = command.CommandConfig.ContainsKey("CharacterID") ?
command.CommandConfig["CharacterID"] : command.CommandConfig["From"];
            if (!ImageDataDic.ContainsKey(characterId))
            {
                ImageCIData _imageCIData = new(command.CommandConfig, dataLayer);
                ImageDataDic.Add(characterId, _imageCIData);
            }
            var imageCIData = ImageDataDic[characterId];
            string DOTweenAnimation = command.CommandConfig.ContainsKey("DOTweenAnimation") ? command.CommandConfig["DOTweenAnimation"] : "SetAt";
            //Debug.Log(DOTweenAnimation);
            string DOTweenAnimationCommand = GetDOTweenLuaCommandDic()[DOTweenAnimation];
            GameObject G = null;
            if (!ImageCILayerControler.CharacterImageDic.ContainsKey(characterId) ||
                ImageCILayerControler.CharacterImageDic[characterId] == null)
            {
                G = SetImage(imageCIData.imageData, ImageCILayerControler.transform, ImageCILayerControler.CharacterImagePerfab);
                //Vector2 SizeDelta = imageCIData.imageData.sprite.rect.size;
                //G.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultWideth, SizeDelta.y);
                ImageCILayerControler.CharacterImageDic[characterId] = G;
                //ImageCILayerControler.CharacterImageDic.Add(characterId, G);
            }
            else
            {
                G = ImageCILayerControler.CharacterImageDic[characterId];
            }
            var imageAnimationPlayer = new ImageAnimationPlayer
                (DOTweenAnimationCommand, new Dictionary<string, string>(command.CommandConfig), G);
            imageAnimationPlayer.AddScope("ImageCILayer", this);
            imageAnimationPlayer.Config.Add("Mode", "OnLoad");
            imageAnimationPlayer.Play();
            ImageData d = new ImageData(imageAnimationPlayer.G);
            if (imageHistoryDataDic.ContainsKey(characterId))
            {
                imageHistoryDataDic[characterId].Add(d);
            }
            else
            {
                imageHistoryDataDic[characterId] = new List<ImageData>() { d };
            }
            imageHistoryCharacterId.Add(characterId);
        }

        public void OnStart()
        {
            Utils.SetDOTweenLuaCommandDic();
            DOTweenLuaCommandDic = Utils.DOTweenLuaCommandDic;
        }

        public void OnUpdate()
        {

        }

        public void Skip()
        {
            if (imageAnimations != null)
            {
                imageAnimations.Complete();
            }
        }

        public bool Withdraw()
        {
            //回撤时立即完成动画播放
            if (imageAnimations != null)
            {
                imageAnimations.onComplete += () => { imageAnimations = null; };
                imageAnimations.Complete();
            }
            //回撤,根据imageHistoryCharacterId的最后一个元素确定需要回撤的对象
            //移除imageHistoryDataDic的最后一个元素,如果此时列表为空则销毁游戏对象
            //并移除ImageCILayerControler.CharacterImageDic中的对应id
            if (imageHistoryCharacterId.Count > 0)
            {
                string lastId = imageHistoryCharacterId[imageHistoryCharacterId.Count - 1];
                var historydatas = imageHistoryDataDic[lastId];
                imageHistoryCharacterId.RemoveAt(imageHistoryCharacterId.Count - 1);
                if (historydatas.Count > 1)
                {
                    historydatas.RemoveAt(historydatas.Count - 1);
                    var imageData = historydatas[historydatas.Count - 1];
                    var G = ImageCILayerControler.CharacterImageDic[lastId];
                    UnityEngine.Object.Destroy(G);
                    var _G = SetImage(imageData, ImageCILayerControler.transform, ImageCILayerControler.CharacterImagePerfab);
                    ImageCILayerControler.CharacterImageDic[lastId] = _G;
                }
                else
                {
                    imageHistoryDataDic.Remove(lastId);
                    var G = ImageCILayerControler.CharacterImageDic[lastId];
                    UnityEngine.Object.Destroy(G);
                    ImageCILayerControler.CharacterImageDic.Remove(lastId);
                }
                return true;
            }
            else
            {
                return false;
            }
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

        public void Reset()
        {
            foreach(string GameObjectName in ImageCILayerControler.CharacterImageDic.Keys)
            {
                UnityEngine.Object.Destroy(ImageCILayerControler.CharacterImageDic[GameObjectName]);
            }
            ImageCILayerControler.CharacterImageDic = new() { };
            ImageDataDic = new() { };
            imageHistoryCharacterId = new() { };
            imageHistoryDataDic = new() { };
        }
        public void BeforeNextOnLoadStart() { }

        public class SaveData
        {
            public Dictionary<string, ImageCIData> ImageDataDic = new() { };
            public List<string> imageHistoryCharacterId = new() { };
            public Dictionary<string, List<ImageData>> imageHistoryDataDic = new() { };
        }
        public void Load(object saveData)
        {
            //加载SaveData到ImageCILayer自身
            if (saveData == null) return;
            SaveData data;
            data = saveData as SaveData;
            if (data == null)
            {
                string json = JsonConvert.SerializeObject(saveData);
                data = JsonConvert.DeserializeObject<SaveData>(json);
            }
            Reset();
            // 清空当前数据
            this.ImageDataDic.Clear();
            //this.DOTweenLuaCommandDic?.Clear();
            this.imageHistoryCharacterId.Clear();
            this.imageHistoryDataDic.Clear();

            // 恢复数据
            if (data.ImageDataDic != null)
            {
                foreach (var kvp in data.ImageDataDic)
                    this.ImageDataDic[kvp.Key] = kvp.Value;
            }

            if (data.imageHistoryCharacterId != null)
            {
                this.imageHistoryCharacterId.AddRange(data.imageHistoryCharacterId);
            }

            if (data.imageHistoryDataDic != null)
            {
                foreach (var kvp in data.imageHistoryDataDic)
                {
                    this.imageHistoryDataDic[kvp.Key] = new List<ImageData>(kvp.Value);
                }
            }
            this.imageAnimations = null;
            //根据恢复的数据重建场景
            foreach (string characterId in imageHistoryDataDic.Keys)
            {
                var imageDatas = imageHistoryDataDic[characterId];
                if (imageDatas.Count > 0)
                {
                    GameObject G = SetImage(imageDatas[imageDatas.Count-1], ImageCILayerControler.transform, ImageCILayerControler.CharacterImagePerfab);
                    ImageCILayerControler.CharacterImageDic.Add(characterId, G);
                }

            }
        }

        public object Save()
        {
            //保存ImageCILayer自身的数据到SaveData
            var saveData = new SaveData
            {
                // 创建副本，避免与原对象共享引用
                ImageDataDic = new Dictionary<string, ImageCIData>(this.ImageDataDic),
                imageHistoryCharacterId = new List<string>(this.imageHistoryCharacterId),
                imageHistoryDataDic = new Dictionary<string, List<ImageData>>()
            };

            // 深拷贝 imageHistoryDataDic 中的每个 List<ImageData>
            foreach (var kvp in this.imageHistoryDataDic)
            {
                saveData.imageHistoryDataDic[kvp.Key] = new List<ImageData>(kvp.Value);
            }

            return saveData;
        }
    }
}
