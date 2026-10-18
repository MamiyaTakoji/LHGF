using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace LHGFData
{
    public class AudioLayer
    {
        public class AudioData
        {
            public bool IsLoop;
            public float Volume;
            public string AudioPath;
            public AudioData() { }
            public AudioData(string _AudioPath, float _Volume, bool _IsLoop)
            {
                AudioPath = _AudioPath;
                Volume = _Volume;
                IsLoop = _IsLoop;
            }
        }
        public static AudioType GetAudioTypeFromPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return AudioType.UNKNOWN;

            string extension = Path.GetExtension(filePath).ToLower();

            switch (extension)
            {
                case ".mp3":
                case ".mpeg":
                    return AudioType.MPEG;

                case ".wav":
                    return AudioType.WAV;

                case ".ogg":
                    return AudioType.OGGVORBIS;

                case ".aiff":
                case ".aif":
                    return AudioType.AIFF;

                case ".acc":
                case ".m4a":
                    return AudioType.ACC;
                case ".mod":
                    return AudioType.MOD;

                case ".it":
                    return AudioType.IT;

                case ".s3m":
                    return AudioType.S3M;

                case ".xm":
                    return AudioType.XM;

                case ".xma":
                    return AudioType.XMA;

                case ".vag":
                    return AudioType.VAG;

                case ".au":
                    return AudioType.AUDIOQUEUE;

                default:
                    Debug.LogWarning($"未知的音频格式: {extension}，使用默认格式: MPEG");
                    return AudioType.MPEG;
            }
        }
        public static IEnumerator LoadAudioSource(string AudioPath, System.Action<AudioClip> onComplete)
        {
            // 获取音频类型
            AudioType audioType = GetAudioTypeFromPath(AudioPath);

            // 构建完整路径
            //string FilePath = Path.Combine(Utils.ResoucePaths.BgmPath, BgmName);

            Debug.Log($"正在加载音频: {AudioPath}, 类型: {audioType}");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(AudioPath, audioType))
            {
                // 发送请求并等待
                yield return www.SendWebRequest();

                // 检查结果
                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    Debug.Log($"音频加载成功: {AudioPath}, 长度: {clip.length}秒");
                    onComplete?.Invoke(clip);
                }
                else
                {
                    Debug.LogError($"音频加载失败: {AudioPath}, 错误: {www.error}");
                    onComplete?.Invoke(null);
                }
            }
        }
    }
}
