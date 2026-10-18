using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace LHGFData
{
    public class ChapterLayer : ILayer
    {
        public ChapterLayerControler ChapterLayerControler;
        public void BeforeNextOnLoadStart()
        {
            
        }

        public void BeforeNextStart()
        {

        }

        public bool Finish()
        {
            return ChapterLayerControler.IsAnimationFinished;
        }

        public GameObject GetControler()
        {
            return ChapterLayerControler.gameObject;
        }

        public string LayerName()
        {
            return "ChapterLayer";
        }

        public void Load(object SaveData)
        {

        }

        public Dictionary<string, string> Log(LayerCommand command)
        {
            return null;
        }

        public void Next(LayerCommand command)
        {
            //动画播放中不重复触发，防止与LayerShower重播冲突
            if (!ChapterLayerControler.IsAnimationFinished)
            {
                return;
            }
            string _ImagePath = command.CommandConfig["ImagePath"];
            string ImagePath = Path.Combine(Utils.ResoucePaths.BackgroundPath, _ImagePath);
            ChapterLayerControler.ShowChapter(ImagePath);
        }

        public void OnLoadFinish()
        {

        }

        public void OnStart()
        {

        }

        public void OnUpdate()
        {

        }

        public void Reset()
        {

        }

        public object Save()
        {
            return null;
        }

        public void Skip()
        {

        }

        public bool Withdraw()
        {
            //回撤时立即停止并隐藏章节动画，避免残留覆盖回撤后的内容
            ChapterLayerControler.KillAnimation();
            return true;
        }
    }
}

