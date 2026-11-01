using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 头像路径加载的验证脚本。
///
/// 目的：确认 TalkBar.SetAvatarPath() 这条链路真的能把图加载出来。
/// 之前 ChatBarTestScript2 只测了 SetGif / SetImagePath，头像路径这条**零验证**。
///
/// 用法：
///   1. 新建一个空场景，随便放个 Canvas；
///   2. 把 TalkBar 预制体（TMPTalkBar / ImageTalkBar / GIFTalkBar 都行）拖到 Canvas 下；
///   3. 新建空物体挂上本脚本，把这些预制体按顺序拖进 bars 数组；
///   4. 把 Content 拖进 content（留空则用自身）；
///   5. 按需改 avatarPaths —— 每一项对应 bars 里同一位置的 bar。
///   6. 运行，看 Console 和各自头像是否出现。
///
/// 头像路径支持三种写法（RemoteImageLoader.ResolveStreamingAssets）：
///   · StreamingAssets 相对路径："HGF/Texture2D/Portrait/Z2/Normal.png"
///   · 绝对路径：              "C:/Users/xxx/Desktop/1.png"
///   · 完整 URL：              "http://.../a.png"
///
/// 资源现实（写本脚本时核对过）：
///   · StreamingAssets/HGF/Texture2D/Portrait/Z2/ 下有 Normal.png / smile.png / scary.png 等
///   · CharacterInfo.json 里那个 "Portrait-Normall": "Portrait-Normall.png" 是坏数据，
///     对应文件并不存在，别照抄。
/// </summary>
public class AvatarPathTestScript : MonoBehaviour
{
    [Header("场景引用")]
    [Tooltip("聊天列表容器；留空则用本物体自身的 transform")]
    public Transform content;

    [Tooltip("要测试的 TalkBar 预制体，顺序与 avatarPaths 一一对应")]
    public List<LHGFTalkBarControler> bars = new List<LHGFTalkBarControler>();

    [Header("测试参数")]
    [Tooltip("启动后等多久再跑（给 Start/Resolve 留时间），单位秒")]
    public float delay = 1f;

    [Tooltip("每条消息的文本")]
    public string message = "头像测试";

    [Tooltip("每一项对应 bars 里同一位置的 bar 的头像路径。留空字符串 = 测\"不改变头像\"的分支")]
    public List<string> avatarPaths = new List<string>();

    [Tooltip("跑完后是否再等一会儿、把头像路径清空再设一次（验证重复设置不会泄漏/不生效）")]
    public bool alsoTestReapply = true;

    private float timer;
    private bool done;

    private void Start()
    {
        if (content == null) content = transform;
    }

    private void Update()
    {
        if (done) return;

        timer += Time.deltaTime;
        if (timer < delay) return;

        done = true;
        RunTest();
    }

    private void RunTest()
    {
        if (bars == null || bars.Count == 0)
        {
            Debug.LogError("[AvatarPathTest] bars 没配，无法测试。把 TalkBar 预制体拖进 bars。", this);
            return;
        }

        Debug.Log($"[AvatarPathTest] 开始，共 {bars.Count} 个 bar。");

        for (int i = 0; i < bars.Count; i++)
        {
            var prefab = bars[i];
            if (prefab == null)
            {
                Debug.LogWarning($"[AvatarPathTest] bars[{i}] 是空的，跳过。", this);
                continue;
            }

            // 1) 实例化到列表容器里
            var bar = Instantiate(prefab, content);
            bar.name = $"AvatarTest_{i}";

            // 2) 先设内容（顺便验证 SetText 与头像设置不冲突）
            bar.SetText(message, withAnimation: false);

            // 3) 再设头像路径
            string path = (avatarPaths != null && i < avatarPaths.Count) ? avatarPaths[i] : null;

            if (string.IsNullOrEmpty(path))
            {
                // 空字符串 = 测"不改变头像"分支：应当既不改图也不报错
                bar.SetAvatarPath(path);
                Debug.Log($"[AvatarPathTest] #{i} 传空路径 → 应走\"不改变头像\"分支（保持 prefab 里的默认图）。");
            }
            else
            {
                bar.SetAvatarPath(path);
                Debug.Log($"[AvatarPathTest] #{i} 设置头像路径：{path}");
            }
        }

        if (alsoTestReapply)
        {
            Invoke(nameof(Reapply), delay + 1f);
        }
    }

    /// <summary>再设一次同样的路径，确认重复调用不会叠加加载、也不会把图弄丢。</summary>
    private void Reapply()
    {
        Debug.Log("[AvatarPathTest] 重复设置一次相同路径（验证幂等）。");

        var found = content.GetComponentsInChildren<LHGFTalkBarControler>(true);
        foreach (var bar in found)
        {
            if (bar == null || !bar.name.StartsWith("AvatarTest_")) continue;
            if (string.IsNullOrEmpty(bar.Data.avatarPath)) continue;
            bar.SetAvatarPath(bar.Data.avatarPath);
        }

        // 一轮结束：把所有 bar 的头像状态打出来，方便肉眼核对
        Invoke(nameof(Report), 2f);
    }

    /// <summary>把每个 bar 的头像实际状态打出来 —— 这是判断"到底加载成功没"的直接证据。</summary>
    private void Report()
    {
        var found = content.GetComponentsInChildren<LHGFTalkBarControler>(true);
        foreach (var bar in found)
        {
            if (bar == null || !bar.name.StartsWith("AvatarTest_")) continue;

            // ⚠ Avata 自己挂着 Mask 用的底图 Image，用 GetComponentInChildren 会先拿到它。
            //    必须按"名为 Image 的子物体"取，才是真正的头像图。
            UnityEngine.UI.Image image = null;
            if (bar.avatar != null)
            {
                var child = bar.avatar.Find("Image");
                image = child != null
                    ? child.GetComponent<UnityEngine.UI.Image>()
                    : bar.avatar.GetComponentInChildren<UnityEngine.UI.Image>(true);
            }
            bool hasSprite = image != null && image.sprite != null;

            Debug.Log(
                $"[AvatarPathTest] {bar.name}: " +
                $"avatar引用={(bar.avatar != null ? bar.avatar.name : "null")} " +
                $"image={(image != null ? image.name : "null")} " +
                $"sprite={(hasSprite ? image.sprite.name : "null")} " +
                $"enabled={(image != null && image.enabled)} " +
                $"stillLoading={bar.IsAvatarLoading} " +
                $"path={bar.Data.avatarPath}");
        }
    }
}
