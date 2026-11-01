using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using static UnityEngine.Video.VideoPlayer;

namespace LHGFData
{
    public class VideoLayer
    {
        public class VideoData
        {
            public string VideoPath;
            public bool IsLoop;
            public VideoData(string _VideoPath, bool _IsLoop)
            {
                VideoPath = _VideoPath;
                IsLoop = _IsLoop;
            }
        }
        public void PlayVideo(VideoData videoData, VideoPlayer videoPlayer, EventHandler  onFinish)
        {
            videoPlayer.isLooping = videoData.IsLoop;
            videoPlayer.url = videoData.VideoPath;
            videoPlayer.prepareCompleted -= onFinish;
            videoPlayer.prepareCompleted += onFinish;
            videoPlayer.Prepare();
        }
    }
}
