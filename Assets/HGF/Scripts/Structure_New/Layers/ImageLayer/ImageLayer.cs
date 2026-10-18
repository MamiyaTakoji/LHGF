using DG.Tweening;
using LHGFData;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using XLua;
namespace LHGFData
{
    [LuaCallCSharp]
    public class ImageLayer
    {
        //给点了Image的大小，位置，颜色，纹理
        //就能确定一张图片
        public class ImageData
        {
            public Vector2 size = new Vector2(300, 300);
            public Vector2 pos = new Vector2(0, 0);
            public Color color = Color.white;
            [JsonIgnore]
            public Sprite _sprite = null;
            public float rotation = 0;
            public string texturePath = string.Empty;
            [JsonIgnore]
            public Sprite sprite
            {
                get
                {
                    if (_sprite == null && !string.IsNullOrEmpty(texturePath))
                    {
                        _sprite = LoadTextureByIO(texturePath);
                    }
                    return _sprite;
                }
                set
                {
                    _sprite = value;
                }
            }
            public ImageData() { }
            /// <summary>
            /// 销毁该 ImageData 持有的 Sprite 和 Texture2D 资源，防止内存泄漏
            /// </summary>
            public void DestroyResources()
            {
                if (_sprite != null)
                {
#if UNITY_ANDROID
                    // Android 平台：Sprite/Texture 由 Utils 的 LRU 纹理缓存统一管理，
                    // 同一路径可能被多个 ImageData 共享（如立绘历史、背景历史）。
                    // 这里只释放引用，不销毁对象。原因：
                    // 1. 直接 Destroy 会破坏缓存中其他 ImageData 对该 Sprite 的引用，导致画面异常；
                    // 2. Destroy 延迟到帧末才生效，读档时同一帧内先 Reset 销毁、后从缓存重新取回，
                    //    会拿到即将被销毁的 Sprite，导致立绘/背景无法正常显示。
                    // 对象最终由缓存 LRU 淘汰 + Resources.UnloadUnusedAssets() 释放。
                    _sprite = null;
#else
                    var tex = _sprite.texture;
                    UnityEngine.Object.Destroy(_sprite);
                    _sprite = null;
                    if (tex != null)
                    {
                        UnityEngine.Object.Destroy(tex);
                    }
#endif
                }
            }
            public ImageData(Dictionary<string, string> config)
            {
                size.x = config.GetFloat("sizeX", 700f);
                size.y = config.GetFloat("sizeY", 700f);
                pos.x = config.GetFloat("posX", 0f);
                pos.y = config.GetFloat("posY", 0f);
                color.r = config.GetFloat("colorR", 1f);
                color.g = config.GetFloat("colorG", 1f);
                color.b = config.GetFloat("colorB", 1f);
                color.a = config.GetFloat("colorA", 1f);
                texturePath = config.GetString("texturePath", string.Empty);
                // 不在此立即加载：交给 sprite 属性按需懒加载。
                // 这样历史快照只持有路径字符串、不钉住纹理，避免正常推进时历史列表长期持有 Sprite 导致内存只增不减。
                // 行为与“退出游戏→读档”路径一致（读档反序列化重建的 ImageData 同样只有 texturePath、_sprite 为空）。
                rotation = config.GetFloat("rotation", 0f);
            }
            public ImageData(GameObject G)
            {
                size = G.GetComponent<RectTransform>().sizeDelta;
                pos = G.GetComponent<RectTransform>().anchoredPosition;
                color = G.GetComponent<MaskableGraphic>().color;
                rotation = G.GetComponent<RectTransform>().eulerAngles.z;
                // 先读取 texturePath：历史快照只保留路径，不持有 Sprite（与读档路径一致），
                // 由 sprite 属性按需懒加载，避免历史列表长期钉住纹理导致内存只增不减。
                if (G.GetComponent<GameObjectAdditionalInfo>() != null)
                {
                    if (G.GetComponent<GameObjectAdditionalInfo>().AdditionalInfo.ContainsKey("texturePath"))
                    {
                        texturePath = G.GetComponent<GameObjectAdditionalInfo>().AdditionalInfo["texturePath"];
                    }
                }
                // 仅当没有可用路径（如默认背景）时才保留当前 Image 上的 Sprite；
                // 有路径的快照保持 _sprite=null，交给懒加载管理，回退时由 sprite getter 按需加载。
                if (string.IsNullOrEmpty(texturePath))
                {
                    sprite = G.GetComponent<Image>().sprite;
                }
            }
            //这个方法只在调试代码时有用
            public void ShowImageInfo(string Name)
            {
                Debug.Log($"{Name}的size是{size}");
            }
        }
        public Dictionary<string, string > GetDOTweenLuaCommandDic()
        {
            if(Utils.DOTweenLuaCommandDic == null)
            {
                Utils.SetDOTweenLuaCommandDic();
            }
            return Utils.DOTweenLuaCommandDic;
        }
        public GameObject SetImage(ImageData imageData, Transform parent, GameObject imageAnimationPerfab)
        {
            var G = UnityEngine.Object.Instantiate(imageAnimationPerfab, parent);
            if (G.GetComponent<GameObjectAdditionalInfo>() == null)
            {
                G.AddComponent<GameObjectAdditionalInfo>(); 
            }
            G.GetComponent<GameObjectAdditionalInfo>().AdditionalInfo["texturePath"] = imageData.texturePath;
            G.GetComponent<RectTransform>().anchoredPosition = imageData.pos;
            G.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, imageData.rotation);
            Sprite sprite = imageData.sprite;
            G.GetComponent<Image>().sprite = sprite;
            //G的宽度统一固定吧
            RectTransform rect = G.GetComponent<RectTransform>();
            //LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            G.GetComponent<RectTransform>().sizeDelta = imageData.size;
            G.GetComponent<MaskableGraphic>().color = imageData.color;
            if(G.GetComponent<ResizeImage>() == null) { 
                G.AddComponent<ResizeImage>();
            }
            var _resizeImage = G.GetComponent<ResizeImage>();
            _resizeImage._ResizeImage();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            //Debug.Log(G.GetComponent<RectTransform>().sizeDelta);
            return G;
        }
        public void SetImage(ImageData imageData, GameObject TargetGameObject)
        {
            Sprite sprite = imageData.sprite;
            TargetGameObject.GetComponent<Image>().sprite = sprite;
            TargetGameObject.GetComponent<MaskableGraphic>().color = imageData.color;
        }
        public static Sprite LoadTextureByIO(string Path)
        {
            /*            FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
                        fs.Seek(0, SeekOrigin.Begin);//游标的操作，可有可无
                        byte[] bytes = new byte[fs.Length];//生命字节，用来存储读取到的图片字节
                        try
                        {
                            fs.Read(bytes, 0, bytes.Length);//开始读取，这里最好用trycatch语句，防止读取失败报错
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.Log(e);
                        }
                        fs.Close();//切记关闭
                        int width = 2;//图片的宽（这里两个参数可以提到方法参数中）
                        int height = 2;//图片的高（这里说个题外话，pico相关的开发，这里不能大于4k×4k不然会显示异常，当时开发pico的时候应为这个问题找了大半天原因，因为美术给的图是6000*3600，导致出现切几张图后就黑屏了。。。
                        Texture2D texture = new Texture2D(width, height);
                        if (texture.LoadImage(bytes))
                        {
                            var s = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));//将生成的texture2d返回，到这里就得到了外部的图片，可以使用了;
                            return s;
                        }
                        else
                        {
                            return null;
                        }*/
            return Utils.LoadTextureByIO(Path);
        }
        public class ImageAnimationPlayer
        {
            public string Commnad;
            public Dictionary<string, string> Config;
            public GameObject G;
            public Sequence sequence = null;
            private LuaTable scriptScopeTable;
            public ImageAnimationPlayer(string commnad, Dictionary<string, string> config, GameObject g)
            {
                Commnad = commnad;
                Config = config;
                G = g;
                scriptScopeTable = LHGFData.Utils.xLuaEnv.luaEnv.NewTable();
                using (LuaTable meta = LHGFData.Utils.xLuaEnv.luaEnv.NewTable())
                {
                    meta.Set("__index", LHGFData.Utils.xLuaEnv.luaEnv.Global);
                    scriptScopeTable.SetMetaTable(meta);
                }
                scriptScopeTable.Set("self", this);
            }
            public void AddScope<T>(string ClassName, T Class)
            {
                scriptScopeTable.Set(ClassName, Class);
            }
            public void Play()
            {
                Utils.xLuaEnv.luaEnv.DoString(Commnad, "", scriptScopeTable);
            }
        }
    }
}
