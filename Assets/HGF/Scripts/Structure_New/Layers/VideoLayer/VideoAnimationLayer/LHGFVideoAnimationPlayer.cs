using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LHGFVideoAnimationPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public bool IsOnUsing;
    public bool IsLoadFinish = true;
    void Awake()
    {
        var targetRenderer = new RenderTexture(1980, 1080, 24);
        rawImage = GetComponent<RawImage>();
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.source = VideoSource.Url;
        rawImage.texture = targetRenderer;
        videoPlayer.targetTexture = targetRenderer;
        videoPlayer.playOnAwake = false;
        videoPlayer.targetTexture.Release();
        videoPlayer.targetTexture.MarkRestoreExpected();
        IsOnUsing = false;
    }
}
