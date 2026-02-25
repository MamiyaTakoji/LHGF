
using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace LHGFData {
    public class ImageBgLayer : ImageLayer, ILayer
    {
        //完全类似ImageCILayer的写法
        public LHGFImageBgLayerControler ImageBgLayerControler;
        public List<ImageBgData> imageBgDatas = new() { };
        public Sequence imageAnimations = null;
        public class ImageBgData
        {
            public ImageData imageData;
            public ImageBgData() { }
            public ImageBgData(Dictionary<string, string> config, LHGFImageBgLayerControler c)
            {
                string ResourcePath = config["BgImage"];
                string BgPath = Path.Combine(Utils.ResoucePaths.BackgroundPath, ResourcePath);
                config["texturePath"] = BgPath;
                float hight = c.ImageBgHight;
                imageData = new(config);
                imageData.size.y = hight;
            }
            public ImageBgData(GameObject g)
            {
                imageData = new ImageData(g);
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
            return ImageBgLayerControler.gameObject;
        }

        public string LayerName()
        {
            return "ImageBgLayer";
        }

        public void Log()
        {
            throw new System.NotImplementedException();
        }


        public void Next(LayerCommand command)
        {
            //var ImageBgData = new ImageBgData(command.CommandConfig, ImageBgLayerControler);
            //ImageBgLayerControler.background.sprite = ImageBgData.imageData.sprite;
            string DOTweenAnimation = command.CommandConfig.ContainsKey("DOTweenAnimation") ? command.CommandConfig["DOTweenAnimation"] : "SetBg";
            string DOTweenAnimationCommand = GetDOTweenLuaCommandDic()[DOTweenAnimation];
            var imageAnimationPlayer = new ImageAnimationPlayer
                (DOTweenAnimationCommand, new Dictionary<string, string>(command.CommandConfig), ImageBgLayerControler.gameObject);
            //在这里把新的背景加进去
            //这样写真是唐完了
            string resourcePath = Path.Combine(Utils.ResoucePaths.BackgroundPath, command.CommandConfig["BgImage"]);
            if (ImageBgLayerControler.gameObject.GetComponent<GameObjectAdditionalInfo>() == null)
            {
                ImageBgLayerControler.gameObject.AddComponent<GameObjectAdditionalInfo>();
            }
            ImageBgLayerControler.gameObject.GetComponent<GameObjectAdditionalInfo>().AdditionalInfo["texturePath"] = resourcePath;
            imageAnimationPlayer.AddScope("ImageBgLayer", this);
            imageAnimationPlayer.Config.Add("Mode", "OnPlay");
            imageAnimationPlayer.Play();
            imageAnimationPlayer.sequence.onComplete +=
                () =>
                {
                    ImageBgData d = new ImageBgData(imageAnimationPlayer.G);
                    imageBgDatas.Add(d);
                };
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
            string DOTweenAnimation = command.CommandConfig.ContainsKey("DOTweenAnimation") ? command.CommandConfig["DOTweenAnimation"] : "SetBg";
            string DOTweenAnimationCommand = GetDOTweenLuaCommandDic()[DOTweenAnimation];
            var imageAnimationPlayer = new ImageAnimationPlayer
                (DOTweenAnimationCommand, new Dictionary<string, string>(command.CommandConfig), ImageBgLayerControler.gameObject);
            imageAnimationPlayer.AddScope("ImageBgLayer", this);
            imageAnimationPlayer.Config.Add("Mode", "OnLoad");
            imageAnimationPlayer.Play();
            ImageBgData d = new ImageBgData(imageAnimationPlayer.G);
            imageBgDatas.Add(d);
        }

        public void OnStart()
        {
            //游戏启动时，记录默认背景
            ImageBgData d = new ImageBgData(ImageBgLayerControler.background.gameObject);
            imageBgDatas.Add(d);
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
            if (imageBgDatas.Count == 0)
            {
                return false;
            }
            if (imageBgDatas.Count > 1)
            {
                imageBgDatas.RemoveAt(imageBgDatas.Count - 1);
                var bgData = imageBgDatas[imageBgDatas.Count - 1];
                SetImage(bgData.imageData, ImageBgLayerControler.gameObject);
            }
            return true;
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
            //使用默认背景
            ImageBgLayerControler._Reset();
        }
        public void BeforeNextOnLoadStart() { }
        [System.Serializable]
        public class SaveData
        {
            public List<ImageBgData> imageBgDatas;
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

            // 清空当前数据
            imageBgDatas.Clear();

            // 恢复历史数据（深拷贝列表，元素直接引用）
            if (data.imageBgDatas != null)
            {
                imageBgDatas.AddRange(data.imageBgDatas);
            }

            // 停止所有动画
            if (imageAnimations != null && imageAnimations.IsActive())
            {
                imageAnimations.Kill();
                imageAnimations = null;
            }

            // 恢复背景显示：应用最后一个 ImageBgData（如果存在）
            if (imageBgDatas.Count > 0)
            {
                var lastBg = imageBgDatas[imageBgDatas.Count - 1];
                SetImage(lastBg.imageData, ImageBgLayerControler.gameObject);
            }
            else
            {
                // 如果没有数据，重置为默认背景（调用控制器的 Reset 或 SetDefault）
                ImageBgLayerControler._Reset();
            }
        }

        public object Save()
        {
            var saveData = new SaveData
            {
                // 创建 imageBgDatas 的副本（浅拷贝列表）
                imageBgDatas = new List<ImageBgData>(this.imageBgDatas)
            };
            return saveData;
        }
    }
}
