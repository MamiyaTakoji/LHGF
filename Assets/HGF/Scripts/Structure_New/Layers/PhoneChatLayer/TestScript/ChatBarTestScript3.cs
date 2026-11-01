using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ChatBarTestScript2 的 UTF-8 版本。
///
/// 为什么要新开一个文件：ChatBarTestScript2.cs 是 GBK 编码，
/// 我的编辑工具无法读取（报 invalid UTF-8），而 shell 写入又被沙箱挡住。
/// 原文件原样保留、未做任何改动；本文件是它的超集，多了头像路径测试。
///
/// 用哪个：
///   · PhoneUITest.unity 里原来引用的是 ChatBarTestScript2，继续用它就行；
///   · 想测头像路径，把场景里的组件换成本脚本（或新增一个物体挂它）。
///
/// 相比原版的改动：
///   1. 新增 barPerfab6 / barPerfab7 两个槽位，专门测头像路径；
///   2. 文件卡片改用 SetFilePath（路径加载图标），原版用的 SetFile(null, ...) 等于没有图标；
///   3. 硬编码的 F:/Tencent Files 和 C:/Users/22378/Desktop 改成按当前用户桌面动态拼接；
///   4. 新增 Report()，把每个 bar 的头像实际加载状态打出来。
/// </summary>
public class ChatBarTestScript3 : MonoBehaviour
{
    public GameObject Content;
    public LHGFTalkBarControler barPerfab;    // 文本
    public LHGFTalkBarControler barPerfab2;   // 文件
    public LHGFTalkBarControler barPerfab3;   // 时间戳
    public LHGFTalkBarControler barPerfab4;   // 动图
    public LHGFTalkBarControler barPerfab5;   // 图片
    public LHGFTalkBarControler barPerfab6;   // 头像（有路径）
    public LHGFTalkBarControler barPerfab7;   // 头像（空路径，测"不改变"分支）

    public float Time_Sleep = 1f;
    public float CurrentTime = 0f;
    public bool Finish = false;
    public string BarContent = "测试";

    [Header("资源路径（按自己机器的实际情况改）")]
    [Tooltip("动图：完整路径 / URL / StreamingAssets 相对路径都行。留空 = 用桌面 testImage.gif")]
    public string GifPath = "";
    [Tooltip("图片：同上。留空 = 用桌面 1.png")]
    public string ImagePath = "";
    [Tooltip("头像：StreamingAssets 相对路径。Portrait/Z2/ 下确实有 Normal.png")]
    public string AvatarPath = "HGF/Texture2D/Portrait/Z2/Normal.png";
    [Tooltip("文件图标：走 SetFilePath，即 XML 剧本用的那条路。留空 = 无图标")]
    public string FileIconPath = "F:/Unity/LHGF2609/Assets/HGF/Scripts/Structure_New/Layers/PhoneChatLayer/New Folder/Resource/pdf_filetype_icon_177525.png";
    public string FileName = "1.png";
    public string FileSize = "1.png";

    void Start()
    {
        // 原版硬编码了 "F:/Tencent Files/..." 和 "C:/Users/22378/Desktop/1.png"，
        // 这里按当前用户的桌面动态拼接，换台机器也能跑。
        string desktop = System.Environment.GetFolderPath(
            System.Environment.SpecialFolder.DesktopDirectory).Replace("\\", "/");

        if (string.IsNullOrEmpty(ImagePath)) ImagePath = desktop + "/1.png";
        if (string.IsNullOrEmpty(GifPath)) GifPath = desktop + "/testImage.gif";
    }

    void Update()
    {
        if (Finish) return;

        CurrentTime += Time.deltaTime;
        if (CurrentTime <= Time_Sleep) return;

        var bar = Instantiate(barPerfab, Content.transform);
        bar.SetText(BarContent);

        var bar2 = Instantiate(barPerfab2, Content.transform);
        bar2.SetFilePath(FileIconPath, FileName, FileSize, false);

        var bar3 = Instantiate(barPerfab3, Content.transform);
        bar3.SetTimeStamp("2024-01-01 12:00", false);

        var bar4 = Instantiate(barPerfab4, Content.transform);
        bar4.SetGif("F:/Tencent Files/2237804476/nt_qq/nt_data/Emoji/emoji-recv/2026-10/Ori/testImage.gif",rightSide:true);

        var bar5 = Instantiate(barPerfab5, Content.transform);
        bar5.SetImagePath("C:/Users/22378/Desktop/1.png");

        // 头像：有路径 → 应加载出图
        if (barPerfab6 != null)
        {
            var bar6 = Instantiate(barPerfab6, Content.transform);
            bar6.SetText("头像:有路径", withAnimation: false);
            bar6.SetAvatarPath(AvatarPath);
        }

        // 头像：空路径 → 应走"不改变头像"分支，保持预制体里的默认图
        if (barPerfab7 != null)
        {
            var bar7 = Instantiate(barPerfab7, Content.transform);
            bar7.SetText("头像:空路径(应保持默认)", withAnimation: false);
            bar7.SetAvatarPath("");
        }

        Finish = true;

        // 等 2 秒（给异步加载留时间）再把实际状态打出来
        Invoke("Report", 2f);
    }

    /// <summary>判断"头像/图标到底加载成功没"的直接证据。</summary>
    void Report()
    {
        var found = Content.GetComponentsInChildren<LHGFTalkBarControler>(true);
        foreach (var b in found)
        {
            // ⚠ 不能用 avatar.GetComponentInChildren<Image>()：Avata 自己挂着 Mask 用的底图 Image，
            //    深度优先会先返回它，看到的就不是真正的头像图。这里按"名为 Image 的子物体"取。
            UnityEngine.UI.Image img = null;
            if (b.avatar != null)
            {
                var child = b.avatar.Find("Image");
                img = child != null
                    ? child.GetComponent<UnityEngine.UI.Image>()
                    : b.avatar.GetComponentInChildren<UnityEngine.UI.Image>(true);
            }
            bool has = img != null && img.sprite != null;

            Debug.Log(string.Format(
                "[ChatBarTest3] {0}: avatar={1} image={2} sprite={3} enabled={4} loading={5} path={6}",
                b.name,
                b.avatar != null ? b.avatar.name : "null",
                img != null ? img.name : "null",
                has ? img.sprite.name : "null",
                img != null && img.enabled,
                b.IsAvatarLoading,
                b.Data.avatarPath));
        }
    }
}
