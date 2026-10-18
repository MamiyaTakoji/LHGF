using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHGFData
{
    public class CameraLayer : ImageLayer, ILayer
    {
        public LHGFCameraLayerControler CameraLayerControler;
        public Dictionary<string, string> DOTweenLuaCommandDic;
        public List<CameraData> cameraDatas = new() { };
        public Sequence cameraAnimations = null;
        public CameraData DefaultCameraData;
        public class CameraData
        {
            public CameraData() { }
            //Camera目前只记录这三个状态
            public Vector2 pos = new Vector2(0, 0);
            public float size = 540;
            public float rotation = 0;
            public CameraData(GameObject G)
            {
                pos = G.GetComponent<Transform>().position;
                rotation = G.GetComponent<Transform>().eulerAngles.z;
                size = G.GetComponent<LHGFCameraLayerControler>().camera.orthographicSize;
            }
        }
        public void BeforeNextStart()
        {
            
        }

        public bool Finish()
        {
            if (cameraAnimations == null || !cameraAnimations.IsActive())
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
            throw new System.NotImplementedException();
        }

        public string LayerName()
        {
            return "CameraLayer";
        }

        public void Next(LayerCommand command)
        {
            string DOTweenAnimation = Utils.GetDicValue(command.CommandConfig, "DOTweenAnimation", "SetAt");
            string DOTweenAnimationCommand = DOTweenLuaCommandDic[DOTweenAnimation];
            var imageAnimationPlayer = new ImageAnimationPlayer
             (DOTweenAnimationCommand, 
             new Dictionary<string, string>(command.CommandConfig),
             CameraLayerControler.gameObject);
            imageAnimationPlayer.AddScope("CameraLayer", this);
            imageAnimationPlayer.Config.Add("Mode", "OnPlay");
            imageAnimationPlayer.Play();
            imageAnimationPlayer.sequence.onComplete +=
                () =>
                {
                    CameraData d = new CameraData(imageAnimationPlayer.G);
                    cameraDatas.Add(d);
                };
            string ConnectMethod = command.CommandConfig.ContainsKey("ConnectMethod") ?
         command.CommandConfig["ConnectMethod"] : "Append";
            if (cameraAnimations == null || !cameraAnimations.IsActive())
            {
                cameraAnimations = imageAnimationPlayer.sequence;
            }
            else
            {
                if (ConnectMethod == "Append")
                {
                    cameraAnimations.Append(imageAnimationPlayer.sequence);
                }
                else if (ConnectMethod == "Join")
                {
                    cameraAnimations.Join(imageAnimationPlayer.sequence);
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
            string DOTweenAnimation = Utils.GetDicValue(command.CommandConfig, "DOTweenAnimation", "SetCamera");
            string DOTweenAnimationCommand = DOTweenLuaCommandDic[DOTweenAnimation];
            var imageAnimationPlayer = new ImageAnimationPlayer
             (DOTweenAnimationCommand,
             new Dictionary<string, string>(command.CommandConfig),
             CameraLayerControler.gameObject);
            imageAnimationPlayer.AddScope("CameraLayer", this);
            imageAnimationPlayer.Config.Add("Mode", "OnLoad");
            imageAnimationPlayer.Play();
            CameraData d = new CameraData(imageAnimationPlayer.G);
            cameraDatas.Add(d);
        }

        public void OnLoadFinish()
        {
            
        }

        public void OnStart()
        {
            Utils.SetDOTweenLuaCommandDic();
            DOTweenLuaCommandDic = Utils.DOTweenLuaCommandDic;
            //cameraDatas.Add(new CameraData(CameraLayerControler.gameObject));
            DefaultCameraData = new CameraData(CameraLayerControler.gameObject);
        }

        public void OnUpdate()
        {

        }

        public void Skip()
        {
            if (cameraAnimations != null)
            {
                cameraAnimations.Complete();
            }
        }
        public void SetCamera(CameraData cameraData)
        {
            CameraLayerControler.GetComponent<Transform>().position = cameraData.pos;
            CameraLayerControler.GetComponent<LHGFCameraLayerControler>().camera.orthographicSize
                = cameraData.size;
            CameraLayerControler.GetComponent<Transform>().eulerAngles = new Vector3(0, 0, cameraData.rotation);
        }

        public bool Withdraw()
        {
            cameraDatas.RemoveAt(cameraDatas.Count - 1);
            if (cameraDatas.Count == 0)
            {
                SetCamera(DefaultCameraData);
            }
            if (cameraDatas.Count > 0)
            {
                var lastData = cameraDatas[cameraDatas.Count - 1];
                SetCamera(lastData);
            }
            return true;
        }
        public static Texture2D CopyTextureGPU(Texture2D source)
        {
            if (source == null) return null;

            Texture2D dest = new Texture2D(
                source.width,
                source.height,
                source.format,
                source.mipmapCount > 1
            );

            Graphics.CopyTexture(source, dest);
            return dest;
        }

        public Dictionary<string, string> Log(LayerCommand command)
        {
            return null;
        }

        public void Reset()
        {
            CameraLayerControler.Reset();
            cameraDatas = new() { };
        }
        public void BeforeNextOnLoadStart() { }

        [System.Serializable]
        public class SaveData
        {
            public List<CameraData> cameraDatas = new() { };
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


            // 恢复历史数据
            cameraDatas.Clear();
            if (data.cameraDatas != null)
            {
                cameraDatas.AddRange(data.cameraDatas);
            }

            // 重置运行时动画（避免残留）
            if (cameraAnimations != null && cameraAnimations.IsActive())
            {
                cameraAnimations.Kill();
                cameraAnimations = null;
            }

            // 确保 DOTween 命令字典已初始化（类似 OnStart 的逻辑）
            if (DOTweenLuaCommandDic == null)
            {
                Utils.SetDOTweenLuaCommandDic();
                DOTweenLuaCommandDic = Utils.DOTweenLuaCommandDic;
            }
            if (cameraDatas.Count > 0)
            {
                SetCamera(cameraDatas[cameraDatas.Count - 1]);
            }
        }

        public object Save()
        {
            var saveData = new SaveData();
            // 复制列表，避免与原对象共享引用
            saveData.cameraDatas = new List<CameraData>(this.cameraDatas);
            return saveData;
        }
    }
}

