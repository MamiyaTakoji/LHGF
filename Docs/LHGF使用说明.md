# LHGF 使用说明与二次开发指南

> LHGF（LaTeX-supported HOILAI Galgame Framework）是一个基于 Unity 的视觉小说（Galgame）开发框架，
> 在原版 [HOILAI-Galgame-Framework](https://github.com/Empty-ZZJ/HOILAI-Galgame-Framework) 基础上扩展而来。
> 本文面向希望在 LHGF 上进行二次开发的开发者，涵盖环境搭建、架构理解、剧情脚本编写、动画扩展与代码级定制。

---

## 目录

1. [项目简介与特性](#1-项目简介与特性)
2. [环境要求](#2-环境要求)
3. [目录结构](#3-目录结构)
4. [快速开始](#4-快速开始)
5. [核心架构](#5-核心架构)
6. [剧情脚本（XML）编写](#6-剧情脚本xml编写)
7. [命令参考（层命令大全）](#7-命令参考层命令大全)
8. [资源配置与约定](#8-资源配置与约定)
9. [配置文件](#9-配置文件)
10. [动画系统（XLua + DOTween）](#10-动画系统xlua--dotween)
11. [存读档与设置](#11-存读档与设置)
12. [二次开发专题](#12-二次开发专题)
13. [常见问题 FAQ](#13-常见问题-faq)

---

## 1. 项目简介与特性

LHGF 在原框架基础上实现了 Galgame 常见功能：

| 功能 | 说明 |
| --- | --- |
| 存/读档 | 多槽位 JSON 存档，保存节点位置与各层状态 |
| 长对话 / 黑板 | 整页滚动文本、可插入图片与 LaTeX 公式 |
| 动画 | 立绘动画、背景动画、摄像机动画、视频动画 |
| 自动 / 跳过 | 自动播放、跳过已读、跳过未读 |
| 对话记录 / 回撤 | 历史记录面板（Backspace 回撤） |
| 设置菜单 | BGM、按键音效、角色语音音量调节 |
| LaTeX 支持 | 通过 TEXDraw 渲染 `$...$` 公式 |

框架亮点：**剧情脚本是纯数据（XML），动画命令是可热更的 Lua 文件（XLua）**，
因此不需要懂 Unity 也能编写剧情，二次开发者则可以在 C# 层扩展新功能。

---

## 2. 环境要求

| 项目 | 要求 |
| --- | --- |
| Unity | **2022.3.54f1c1**（`ProjectSettings/ProjectVersion.txt`） |
| 渲染管线 | URP（Universal Render Pipeline） |
| 目标平台 | Windows（主）、Android（内置内存优化） |
| 第三方依赖 | 见 `Assets/packages.config`：Newtonsoft.Json 13.0.3、QuikGraph 2.5.0、AutoMapper 14.0.0 |
| 内置插件 | XLua、DOTween、TEXDraw（均已在 `Assets/` 下，无需额外安装） |

> 提示：如果项目缺少 NuGet 包，可通过 Unity 菜单 `NuGet/Update NuGet Packages` 恢复
> （项目已集成 [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)）。

---

## 3. 目录结构

```
LHGF2608/
├── Assets/
│   ├── HGF/                          # ★ 框架本体（二次开发主要区域）
│   │   ├── Scripts/
│   │   │   ├── Structure_New/        # 新版核心脚本
│   │   │   │   ├── Layers/           # 各游戏"层"（Layer）的实现
│   │   │   │   │   ├── AudioLayer/          # BGM / 语音 / 音效
│   │   │   │   │   ├── BlackboardLayer/     # 黑板（长文本/图片/公式）
│   │   │   │   │   ├── CameraLayer/         # 摄像机动画
│   │   │   │   │   ├── ChapterLayer/        # 章节动画（ChapterImage）
│   │   │   │   │   ├── ChoiceLayer/         # 选项
│   │   │   │   │   ├── DataLayer/           # 角色/证据数据
│   │   │   │   │   │   ├── CharacterInfoLayer/   # 人物信息面板
│   │   │   │   │   │   └── EvidenceLayer/        # 证据收集/查看
│   │   │   │   │   ├── DialogLayer/         # 对话框（Speak）
│   │   │   │   │   ├── EvidenceChoiceLayer/ # 出示证据（指证）
│   │   │   │   │   ├── ImageLayer/          # 图片背景 / 立绘
│   │   │   │   │   │   ├── ImageBgLayer/    # 背景图
│   │   │   │   │   │   └── ImageCILayer/    # 立绘（Character Illustration）
│   │   │   │   │   ├── LongDialogLayer/     # 长对话
│   │   │   │   │   └── VideoLayer/          # 视频
│   │   │   │   ├── Logger/            # 对话历史记录
│   │   │   │   ├── MainCamera/        # LetterBox（黑边）
│   │   │   │   ├── Progress/          # 游戏主流程
│   │   │   │   │   ├── LHGFGameMain.cs        # 总控（场景入口组件）
│   │   │   │   │   ├── LHGFGameProgress.cs    # 进度/前进/回撤/跳转
│   │   │   │   │   ├── LHGFGameSaveData.cs    # 存档/设置/全局数据
│   │   │   │   │   ├── LHGFScriptData.cs      # XML 解析 / 图结构 / Utils
│   │   │   │   │   ├── LHGFShortcut.cs        # 快捷键
│   │   │   │   │   └── Menu/                  # 开始菜单/游戏菜单/存读档面板
│   │   │   │   └── StartMenu/         # 开始菜单 Canvas
│   │   │   ├── XLua/                  # BuildLuaFileList（生成 lua_files.txt）
│   │   │   └── UI/                    # ButtonEffect 等 UI 辅助
│   │   ├── Perfab/                    # 预制体（Canvas/Menu/Setting/Dialog...）
│   │   ├── Fonts/                     # 字体（含 SDF、TEXDraw 字体）
│   │   └── Texture2D/                 # 内置贴图/渲染纹理
│   ├── StreamingAssets/
│   │   └── HGF/                       # ★ 游戏数据（剧情/资源/配置，发布后仍可改）
│   │       ├── ScriptSheets/          # 剧情脚本 XML（★ 写剧情的地方）
│   │       ├── Audio/
│   │       │   ├── BGM/               # 背景音乐
│   │       │   ├── CharacterVoice/    # 角色语音（CV）
│   │       │   └── EffectiveSound/    # 按键/演出音效
│   │       ├── Texture2D/
│   │       │   ├── BackgroundImage/   # 背景图（含章节图 chapterImage\、CG ch2cg\ 等子目录）
│   │       │   ├── Portrait/          # 立绘（按角色目录组织，如 Z\、WD\）
│   │       │   ├── FullbodyPortrait/  # 人物信息面板全身立绘
│   │       │   ├── LongSpeakImage/    # 长对话插图
│   │       │   └── ImageAnimation/    # 图片序列动画帧
│   │       ├── VideoResource/VideoAnimation/  # 视频动画
│   │       ├── xLua/xLuaDOTween/      # ★ 动画命令 Lua 文件（可热更）
│   │       ├── StartMenuSetting/      # 开始菜单背景/BGM/标题
│   │       ├── EvidenceInfo/          # 证据与人物描述 JSON
│   │       ├── GameConfig.json        # 游戏起始节点配置
│   │       ├── CharacterInfo.json     # 角色 ID → 角色配置
│   │       └── RequiredAssets.txt     # 资源引用说明（辅助）
│   ├── Scenes/                        # 场景（构建启用 MainScenceNew.unity）
│   ├── XLua/                          # XLua 框架本体（含 Gen 生成代码）
│   └── Plugins/                       # TEXDraw、DOTween 等
└── SaveData/                          # 运行时生成的存档（Windows 下位于项目根目录）
```

---

## 4. 快速开始

### 4.1 打开并运行

1. 用 Unity 2022.3.54f1c1（或兼容版本）打开项目根目录（`LHGF2608`）。
2. 首次导入会进行 XLua 代码生成（`Assets/XLua/Gen/*`），等待编译完成后可运行。
3. 打开场景 `Assets/Scenes/MainScenceNew.unity`（Build Settings 中唯一启用的场景）。
4. 点击 Play。默认会进入**开始菜单**（背景/BGM 由 `StartMenuSetting/StartMenuConfig.json` 配置）。
5. 点击「开始游戏」，从 `GameConfig.json` 的 `RootNodeId` 指定的节点开始播放剧情。

### 4.2 常用快捷键

| 按键 | 功能 |
| --- | --- |
| `Enter`（Return） | 前进到下一句/下一节点 |
| `Space` | 前进（游戏内主控制器）；长按/点击跳过 |
| `Backspace` | 回撤到上一句 |
| `Ctrl`（LeftControl） | 跳过（未读也跳） |
| 鼠标滚轮向上 | 打开对话历史记录 |

### 4.3 角色与证据

- 游戏中可打开**证据列表**（`AddEvidence` / `EvidenceChoice` 命令相关）；
- 人物信息面板（`CharacterDescribe` 命令）展示角色全身立绘、姓名、描述。

> 快速验证剧情编写是否成功：在 `ScriptSheets/FirstScript.xml` 中修改文本，
> 并把 `GameConfig.json` 的 `RootNodeId` 指到该脚本的某个节点，重新运行即可看到效果。

---

## 5. 核心架构

LHGF 的核心思想是 **「剧情驱动 + 分层响应」**：

```
剧情 XML ──解析──▶ ScriptGraph（有向图，QuikGraph）
                    │
            PlotNode（节点）＝ 一串 LayerCommand（命令）
                    │  按命令名路由 layerCommand2layer
                    ▼
              各 Layer（逻辑层, 实现 ILayer）
                    │
              各 Controller（MonoBehaviour，操作 UI/GameObject）
```

### 5.1 游戏主流程（`LHGFGameMain`）

挂在主场景的 `LHGFGameMain` 是总控组件：

- `SetLayers()`：创建全部 Layer 实例并注册进 `LHGF_GameData.layers`；
- `SetLayerCommand2layer()`：定义「命令名 → 哪些层处理」的路由表（见第 7 节）；
- `Forward()`：推进游戏。当前节点所有命令分发到对应层执行，全部 `Finish()` 后才能再前进；
- `Withdraw()`：回撤到上一个节点（Backspace 触发）；
- `OnGameLoad()` / `OnGameSave()`：读档后重放各层 / 存档前快进各层动画。

### 5.2 数据与进度（`LHGFScriptData.cs`）

- `Utils.ResoucePaths`：所有资源路径约定（见第 8 节）；
- `Utils.xLuaEnv`：全局唯一的 `LuaEnv`（XLua 环境）；
- `ScriptGraph`：有向图。每个 `PlotNode` 是一个顶点，`NextId` 是边；
- `ScriptData`：一个节点包含的 `List<LayerCommand>`；
- `LayerCommand`：`{ CommandName, CommandConfig(Dictionary<string,string>) }`，XML 标签名 → 命令名，属性 → 配置键值；
- `LHGF_GameProgress`：维护 `CurrentNodeId` / `NextNodeId` / `historyNodes` / `LogContents`，
  提供 `Forward / Next / Withdraw / Skip / Load / Save`。

节点命名规则：**`脚本名-分支名-编号`**，例如 `Ch2Part1-MAINBRANCH-0`。
- `脚本名` = XML 文件名（不含扩展名），如 `Ch2Part1`；
- `分支名`：主分支用 `MAINBRANCH`，错误选项分支可自定义（如 `WrongAnswer`）；
- `编号`：分支内从 0 递增。

> 跨脚本跳转：`NextId` 允许指向其他脚本的节点（如 `Ch3Part1_Scene1-MAINBRANCH-0`），
> 框架会按节点名解析脚本文件并**自动加载新脚本**（`ScriptGraph` 的惰性加载）。

### 5.3 层接口（`ILayer`）

每个功能模块实现 `ILayer`（`LHGFScriptData.cs` 中定义）：

```csharp
public interface ILayer
{
    string LayerName();                 // 层名
    void OnStart();                     // 游戏开始
    void OnUpdate();                    // 每帧
    bool Withdraw();                    // 回撤
    void Next(LayerCommand command);    // 收到一个命令
    bool Finish();                      // 该层是否已完成演出（决定能否继续前进）
    void Skip();                        // 跳过演出
    Dictionary<string,string> Log(LayerCommand command); // 记录到历史
    GameObject GetControler();          // 返回对应 UI 控制器
    void OnLoadFinish();
    void BeforeNextStart();
    void BeforeNextOnLoadStart();
    void Reset();                       // 重置
    void Load(object SaveData);         // 读档
    object Save();                      // 存档
}
```

典型实现模式（如 `ChapterLayer.cs`）：

```csharp
public class ChapterLayer : ILayer
{
    public ChapterLayerControler ChapterLayerControler;   // UI 控制器
    public void Next(LayerCommand command)
    {
        string _ImagePath = command.CommandConfig["ImagePath"];
        string ImagePath = Path.Combine(Utils.ResoucePaths.BackgroundPath, _ImagePath);
        ChapterLayerControler.ShowChapter(ImagePath);     // 调用控制器播放章节动画
    }
    public bool Finish() => ChapterLayerControler.IsAnimationFinished;
    // ... 其余接口按需实现
}
```

> **Layer 与 Controller 分离**：Layer 是纯逻辑（不挂 GameObject），
> Controller 是挂在预制体上的 `MonoBehaviour`，负责实际操作 UI。
> 新增功能时建议遵循同样的「逻辑层 + 控制器」两件套结构。

### 5.4 命令路由表

`LHGFGameMain.SetLayerCommand2layer()` 中的默认路由（新增自定义命令时需要在这里注册）：

| 命令名 | 处理层 |
| --- | --- |
| `Choice` | ChoiceLayer |
| `LongSpeak` | LongSpeakLayer, CVLayer |
| `AddCharacter` | DataLayer, ImageCILayer |
| `Speak` | SpeakLayer, CVLayer |
| `ImageCharacterAnimation` | ImageCILayer |
| `ChangeImageBackground` | ImageBgLayer |
| `SetBgm` | BgmLayer |
| `VideoAnimation` | VideoAnimationLayer |
| `CameraAnimation` | CameraLayer |
| `EffectiveSound` | EffectiveSoundLayer |
| `Blackboard` | BlackboardLayer |
| `AddEvidence` | EvidenceLayer, EffectiveSoundLayer |
| `EvidenceChoice` | EvidenceChoiceLayer |
| `CharacterDescribe` | CharacterInfoLayer |
| `ChapterImage` | ChapterLayer |

---

## 6. 剧情脚本(XML)编写

### 6.1 文件组织

- 位置：`Assets/StreamingAssets/HGF/ScriptSheets/*.xml`
- 每个 `.xml` 文件是一个「剧本」（如 `Ch2Part1.xml`、`FirstScript.xml`）；
- 剧情按节点组织，节点之间用 `NextId` 链接成图；
- 游戏入口节点由 `GameConfig.json` 的 `RootNodeId` 指定。

### 6.2 基本结构

```xml
<?xml version="1.0" ?>
<Root>
  <MainPlot>
    <PlotNode Id="Ch2Part1-MAINBRANCH-0" NextId="Ch2Part1-MAINBRANCH-1">
      <ChapterImage ImagePath="chapterImage\Ch1.png"/>
      <ChangeImageBackground BgImage="Park.jpg"/>
      <Speak SpeakerName=" " Content="这里写对话"/>
      <SetBgm BgmName="STOPBGM"/>
    </PlotNode>

    <PlotNode Id="Ch2Part1-MAINBRANCH-1" NextId="Ch2Part1-MAINBRANCH-2">
      <Speak SpeakerName=" " Content="下一句对话"/>
      <SetBgm BgmName="cool_vibes.mp3"/>
    </PlotNode>
  </MainPlot>
</Root>
```

- `<MainPlot>` 下可以有任意多个 `<PlotNode>`；
- `PlotNode` 的 `Id` 必须全局唯一（跨脚本也建议唯一，按命名规则天然唯一）；
- 节点内自上而下执行命令；多个命令可写在同一节点，按顺序执行（分别为不同层处理）；
- `NextId` 指向下一个节点；**多个候选用 `%%` 分隔**（用于选项/指证，见 6.5）。

### 6.3 一个节点内的多命令

一个 `PlotNode` 可以同时包含多个命令，例如同时说话、换立绘、换镜头：

```xml
<PlotNode Id="Ch2Part1-MAINBRANCH-14" NextId="Ch2Part1-MAINBRANCH-15">
  <ImageCharacterAnimation CharacterID="4" DOTweenAnimation="Quit" Time="0.3"/>
  <Speak CharacterID="5" Content="爸爸？"/>
  <CameraAnimation DOTweenAnimation="SetCamera" Size="540" PosX="0" PosY="0" Time="0.3"/>
  <ImageCharacterAnimation DOTweenAnimation="SetAtLeft" CharacterID="3" Time="0.3"/>
  <ImageCharacterAnimation DOTweenAnimation="ShakeX" CharacterID="5" Time="0.5" ShakeTime="3" Delta="20"/>
</PlotNode>
```

> 同一层收到多个命令时会**排队/追加**（`ConnectMethod` 参数默认 `Append`），
> 依次演出；全部完成后节点才 `Finish`。

### 6.4 选项（Choice）

选项通过 **`NextId` 用 `%%` 分隔多个目标**，同时 `Content` / `TextType` 用 `%%` 分隔一一对应：

```xml
<PlotNode Id="Ch2Part2-MAINBRANCH-100" NextId="Ch2Part2-WrongAnswer1-0%%Ch2Part2-WrongAnswer2-0%%Ch2Part2-RightAnswer1-0">
  <Choice NextId="Ch2Part2-WrongAnswer1-0%%Ch2Part2-WrongAnswer2-0%%Ch2Part2-RightAnswer1-0"
          TextType="TexDraw%%TexDraw%%TexDraw"
          Content="选项一%%选项二%%选项三"/>
</PlotNode>
```

- `Content`：按钮上显示的文本（`%%` 分隔，数量 = 选项数）；
- `TextType`：每个选项的文本渲染方式（`TexDraw` 支持公式，`TMP` 普通文本）；
- 选中的分支成为下一个节点；错误分支写回正确节点的套路是：错误分支的 `NextId` 指回题目节点或直接继续。

### 6.5 出示证据（EvidenceChoice / 指证）

类似选项，但按钮来自证据列表：

```xml
<PlotNode Id="Ch10_Scene2-MAINBRANCH-50" NextId="Ch10_Scene2-MAINBRANCH-51%%Ch10_Scene2-WrongAnswer-8%%Ch10_Scene2-WrongAnswer-8">
  <EvidenceChoice NextId="Ch10_Scene2-MAINBRANCH-51%%Ch10_Scene2-WrongAnswer-8%%Ch10_Scene2-WrongAnswer-8"
                  ChapterName="五/发病%%一/前兆%%一/前兆"
                  EvidenceName="演唱会当天的状况%%夏天的天气%%彩夏的晒痕"/>
</PlotNode>
```

- `ChapterName` / `EvidenceName` 用 `%%` 与 `NextId` 对齐；
- 玩家选择（「出示」）某个证据后跳转到对应 `NextId`；
- 前提：证据需先用 `AddEvidence` 加入（否则不在候选列表）。

### 6.6 内容中的换行与转义

- 对话/黑板文本中换行使用 `\\`（两个反斜杠）表示；示例见 `Blackboard` 节点；
- XML 属性中需转义 `&`（`&amp;`）、`<`（`&lt;`）等；
- LaTeX 公式直接写在 `$...$` 中，如 `$x^2$`（需 `TextType="TexDraw"`）。

---

## 7. 命令参考(层命令大全)

> 通用约定：未给出的属性名与 Lua 动画参数对应；`Time` 为演出秒数；
> 坐标/尺寸等数值以字符串形式写在属性中。

### 7.1 ChapterImage —— 章节动画

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `ImagePath` | ✔ | 相对 `BackgroundImage` 目录的路径，如 `chapterImage\Ch1.png` |

```xml
<ChapterImage ImagePath="chapterImage\Ch1.png"/>
```

### 7.2 ChangeImageBackground —— 切换背景图

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `BgImage` | ✔ | 相对 `BackgroundImage` 目录，如 `Park.jpg` / `ch2cg/cg1diff1.png` |
| `DOTweenAnimation` | | 转场动画，默认 `SetBg`（见 Lua 动画） |
| `Time` | | 转场时长（秒） |

```xml
<ChangeImageBackground BgImage="Park.jpg"/>
<ChangeImageBackground BgImage="ch2cg/cg1diff1.png" Time="0.7"/>
```

### 7.3 Speak —— 对话（对话框）

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `Content` | ✔ | 对话文本（可用 `\\` 换行，`$...$` 写公式） |
| `SpeakerName` | | 说话人名；缺省时若给 `CharacterID` 则自动取角色名 |
| `CharacterID` | | 说话人立绘实例 ID（须先 `AddCharacter`） |
| `TextType` | | `TexDraw`（默认，支持公式）/ `TMP` |
| `Skip` | | `0`/`1`，是否可被「跳过」 |
| `DialogBarColor` | | 对话框颜色（进阶） |

```xml
<Speak SpeakerName="象山" Content="你好，$x^2+y^2=z^2$"/>
<Speak CharacterID="5" Content="爸爸？"/>
```

### 7.4 LongSpeak —— 长对话（整页滚动）

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `Content` | ✔ | 长文本（`\\` 换行） |
| `Continue` | | `0`/`1`，是否可继续翻页/结束 |
| `Skip` | | `0`/`1` |
| `TextType` | | `TexDraw`（默认）/ `TMP` / `Image` |

```xml
<LongSpeak Content="穿过玄关门廊时，象山的目光被停在玄关门旁的公路车吸引了。……" Continue="0"/>
```

长对话插图：`TextType="Image"`，`Content` 填 `LongSpeakImage` 目录下的文件名。

### 7.5 SetBgm —— 背景音乐

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `BgmName` | ✔ | `BGM` 目录下的文件名；填 `STOPBGM` 停止当前 BGM |
| `Volume` | | 音量 0~1，默认 0.5 |
| `IsLoopStr` | | `0`/`1` 是否循环，默认 1 |

```xml
<SetBgm BgmName="STOPBGM"/>
<SetBgm BgmName="cool_vibes.mp3"/>
```

### 7.6 EffectiveSound —— 音效

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `EffectiveSoundPath` | ✔ | `EffectiveSound` 目录下的文件名 |

```xml
<EffectiveSound EffectiveSoundPath="yingqingsheng.wav"/>
```

### 7.7 AddCharacter —— 添加角色立绘

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `CharacterID` | ✔ | 该实例在当前立绘层的 ID（字符串数字，如 `"4"`）；同一角色可多次 Add 获得不同 ID |
| `From` | ✔ | `CharacterInfo.json` 中的角色 key（如 `IZUMI`、`WD`、`YM`） |
| `posX` / `posY` | | 初始位置，`posY="default"` 表示默认 |
| `Time` | | 出现演出时长 |
| `sizeY` | | 立绘高度（如 800/864，控制缩放） |
| `DOTweenAnimation` | | 入场动画（如 `SetAtRight`、`SetAt`），默认 `SetAt` |

```xml
<AddCharacter CharacterID="4" From="IZUMI" posX="-2000" Time="0.01" sizeY="800"/>
<AddCharacter From="WD" CharacterID="5" DOTweenAnimation="SetAtRight" posX="-2000" posY="default" Time="0.01" sizeY="800"/>
```

### 7.8 ImageCharacterAnimation —— 立绘动画

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `CharacterID` | ✔ | 立绘实例 ID |
| `DOTweenAnimation` | ✔ | 动画命令名（对应 `xLuaDOTween/*.lua`）：`SetCI`/`SetAt`/`MoveTo`/`MoveToLeft`/`MoveToRight`/`ShakeX`/`Quit` 等 |
| `ImageCharacterIllustration` | | 立绘图文件名（切换表情/服装），如 `mask.png`、`Surprise.png` |
| `Time` | | 演出时长 |
| `posX` / `posY` | | 目标位置 |
| `ShakeTime` / `Delta` | | `ShakeX` 专用：抖动次数/幅度 |
| `ConnectMethod` | | `Append`（排队，默认）/ `Join`（并行） |

```xml
<ImageCharacterAnimation DOTweenAnimation="SetCI" CharacterID="4" ImageCharacterIllustration="mask.png" Time="0.01"/>
<ImageCharacterAnimation CharacterID="5" DOTweenAnimation="ShakeX" Time="0.5" ShakeTime="3" Delta="20"/>
<ImageCharacterAnimation CharacterID="6" DOTweenAnimation="Quit" Time="1"/>
```

### 7.9 CameraAnimation —— 摄像机（镜头）动画

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `DOTweenAnimation` | ✔ | 通常 `SetCamera` |
| `Size` / `posX` / `posY` / `rotation` | | 目标取景参数 |
| `Time` | | 移动时长 |
| `IsFade` | | `0`/`1` 是否渐变过渡 |

```xml
<CameraAnimation DOTweenAnimation="SetCamera" Size="250" posX="0" posY="-250"/>
<CameraAnimation DOTweenAnimation="SetCamera" Size="540" PosX="0" PosY="0" rotation="0" Time="1" IsFade="1"/>
```

### 7.10 VideoAnimation —— 视频动画

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `VideoName` | ✔ | `VideoResource/VideoAnimation/` 下的文件名 |
| `IsLoop` | | `0`/`1` 循环 |
| `Size` / `Rotation` / `PosX` / `PosY` | | 显示参数（Pos 默认 0.5 中心） |
| `Finish` | | `1` 播完后结束本节点（用于等待视频完成再继续） |

```xml
<VideoAnimation VideoName="op.mp4" IsLoop="0" Size="1" Finish="1"/>
```

### 7.11 Blackboard —— 黑板（重点讲解/插图）

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `Content` | ✔ | 文本（`\\` 换行）或 `LongSpeakImage` 图片名（配 `TextType="Image"`） |
| `TextType` | | `TexDraw`（默认，支持表格/公式）/ `Image` / `TMP` |
| `BlackboardState` | | `1` 打开 / `0` 关闭（在后续节点关闭黑板） |
| `PosInfo` | | 位置：`UpperCenter` / `UpperLeft` 等 |
| `SizeX` / `SizeY` | | 相对画面尺寸（0~1） |
| `Rate` | | 图片缩放 |
| `Continue` / `Skip` | | 翻页/跳过控制 |

```xml
<!-- 打开黑板并显示文本 -->
<Blackboard Content="象山在便利商店买了雨衣穿上……" PosInfo="UpperCenter" SizeX="0.5" Continue="0" SizeY="0.4"/>
<!-- 显示插图 -->
<Blackboard Content="fig11.png" TextType="Image" PosInfo="UpperCenter" Rate="1" SizeX="0.4" Continue="0" SizeY="0.57"/>
<!-- 关闭黑板 -->
<Blackboard BlackboardState="0"/>
```

黑板支持 LaTeX 表格，例如：

```xml
<Blackboard Content="\begin{tabular}{| l | x |} \hline 幸运者&amp; \text{内容} \\ \hline \end{tabular}" PosInfo="UpperLeft" SizeY="0.6" Continue="0" SizeX="0.4"/>
```

### 7.12 Choice —— 选项（见 6.4）

### 7.13 AddEvidence —— 获得证据

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `ChapterName` | ✔ | `EvidenceInfo.json` 中的章节分类 |
| `EvidenceName` | ✔ | 证据名 |
| `EffectiveSoundPath` | | 获得证据时的音效 |

```xml
<AddEvidence ChapterName="一/前兆" EvidenceName="里岛一年的证词1" EffectiveSoundPath="GetEvidence.wav"/>
```

### 7.14 EvidenceChoice —— 出示证据（见 6.5）

### 7.15 CharacterDescribe —— 人物信息面板

| 属性 | 必填 | 说明 |
| --- | --- | --- |
| `CharacterID` | ✔ | `CharacterDescribeInfo.json` 中的角色 key（如 `Z`、`WD`） |
| `Info` | ✔ | `"姓名索引%立绘索引%描述索引"`，对应 `CharacterDescribeInfo.json` 中该角色的三个数组下标 |

```xml
<CharacterDescribe CharacterID="Z" Info="0%0%0"/>
<CharacterDescribe CharacterID="Z" Info="1%0%2"/>
```

---

## 8. 资源配置与约定

所有路径均相对 `StreamingAssets/HGF/`（`Utils.ResoucePaths`，见 `LHGFScriptData.cs`）：

| 资源 | 目录 | 说明 |
| --- | --- | --- |
| 背景图 | `Texture2D/BackgroundImage/` | 含子目录：`chapterImage\`（章节图）、`ch2cg\` 等 CG 目录 |
| 立绘 | `Texture2D/Portrait/<角色目录>/` | 目录名 = `CharacterInfo.json` 的 `ResourcesPath` |
| 全身立绘 | `Texture2D/FullbodyPortrait/` | 人物信息面板用 |
| 长对话图 | `Texture2D/LongSpeakImage/` | `Blackboard TextType="Image"` 用 |
| 序列帧动画 | `Texture2D/ImageAnimation/` | 图片动画帧（进阶） |
| BGM | `Audio/BGM/` | 支持 ogg/mp3/wav |
| 语音 | `Audio/CharacterVoice/` | CV 层播放 |
| 音效 | `Audio/EffectiveSound/` | 按键音、演出音效 |
| 视频 | `VideoResource/VideoAnimation/` | VideoPlayer |
| 动画命令 | `xLua/xLuaDOTween/*.lua` | **可热更的动画** |
| 剧情 | `ScriptSheets/*.xml` | 剧本 |
| 开始菜单 | `StartMenuSetting/` | `StartMenuConfig.json` + 背景图 + BGM |
| 证据/人物 | `EvidenceInfo/` | `EvidenceInfo.json`、`CharacterDescribeInfo.json` |

> 新增资源：直接把文件丢进对应目录（Unity 会自动生成 `.meta`），剧情里用文件名引用即可。
> 子目录路径用 `\` 分隔（如 `chapterImage\Ch1.png`），XML 中无需转义（不是属性分隔符）。

---

## 9. 配置文件

### 9.1 `GameConfig.json` —— 起始节点

```json
{ "RootNodeId": "FirstScript-MAINBRANCH-0" }
```

`RootNodeId` 决定点「开始游戏」后从哪个节点进入剧情。

### 9.2 `CharacterInfo.json` —— 角色

```json
{
  "IZUMI": { "Name": "和泉", "ResourcesPath": "IZUMI", "Portrait-Normall": "Portrait-Normall.png" },
  "WD":   { "Name": "舞冬", "ResourcesPath": "WD",   "Portrait-Normall": "Portrait-Normall.png" }
}
```

- key（如 `IZUMI`）：`AddCharacter From=...` 与 `Speak CharacterID` 查找使用；
- `ResourcesPath`：立绘目录名（`Texture2D/Portrait/<ResourcesPath>/`）；
- `Portrait-Normall`：默认立绘文件名。

### 9.3 `StartMenuSetting/StartMenuConfig.json` —— 开始菜单

```json
{ "BGM": "G9Fried@GRM.mp3", "Background": "Untitled.jpg", "Title": "" }
```

- `BGM`：`StartMenuSetting/` 目录下音乐文件；
- `Background`：`StartMenuSetting/` 目录下背景图；
- `Title`：主菜单标题文本（TEXDraw）。

### 9.4 `EvidenceInfo/EvidenceInfo.json` —— 证据内容

```json
{
  "序章": {
    "里岛一年的证词1": "「证词正文……」|||摘要：里岛称八月二十六日……",
    "测试证据1": "1|||234。"
  },
  "一/前兆": { "...": "..." }
}
```

- 外层 key = `ChapterName`（与 `AddEvidence`/`EvidenceChoice` 对应）；
- 内层 key = `EvidenceName`；
- 值 = `证词正文|||摘要`（`|||` 分隔正文与摘要，证据面板分栏显示）。

### 9.5 `EvidenceInfo/CharacterDescribeInfo.json` —— 人物信息

```json
{
  "Z": {
    "Name": ["象山晴太", "象山晴太（幸运者）"],
    "Portrait": ["3.png", "3.png"],
    "Describe": ["描述一", "描述二"]
  }
}
```

- `CharacterID` 对应 `CharacterDescribe` 命令；
- `Info="a%b%c"` 分别索引 `Name[a]`、`Portrait[b]`、`Describe[c]`（即 9.15 节的三个下标）。

---

## 10. 动画系统(XLua + DOTween)

动画命令**不在 C# 里硬编码**，而是由 XLua 执行 `xLuaDOTween/*.lua` 文件。
编写新动画 = 新增一个 Lua 文件（无需改 C#、无需重新编译，甚至可热更）。

### 10.1 Lua 动画文件结构

每个文件实现三个函数 + 末尾的分发：

```lua
-- 初始化：给 _self.Config 补默认参数
function Set(_self)
    local dict = {}
    _self.Config:set_Item("Time", "1")   -- 默认值
    for k,v in pairs(_self.Config) do end -- 遍历实际配置
end

-- 播放动画（进入节点时调用）
function OnPlay(_self)
    local G = _self.G                     -- 目标 GameObject
    local Config = _self.Config            -- 命令参数字典
    -- 用 DOTween 创建 Sequence：
    local sequence = CS.DG.Tweening.DOTween.Sequence()
    local animation = G:GetComponent(typeof(CS.UnityEngine.RectTransform)):DOAnchorPos(targetPos, Time)
    CS.SequenceExtensions.Append(sequence, animation)
    _self.sequence = sequence              -- 保存给框架管理
end

-- 读档/加载时直接设置最终状态（不播放动画）
function OnLoad(_self)
    -- 直接设置位置/颜色/精灵等最终值
end

Set(self)
local Config = self.Config
local success, Value = Config:TryGetValue("Mode")
if success then
    if Value == "OnPlay" then OnPlay(self)
    elseif Value == "OnLoad" then OnLoad(self) end
end
```

`self` 对象（C# 侧 `ImageAnimationPlayer`）字段：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `self.G` | `UnityEngine.GameObject` | 动画作用对象（立绘/背景/摄像机） |
| `self.Config` | `Dictionary<string,string>` | 该命令的全部 XML 属性 |
| `self.sequence` | `DG.Tweening.Sequence` | 赋给框架以管理生命周期 |

在 Lua 中调用 C# 的统一入口是 `CS.命名空间.类名`，例如：

```lua
CS.UnityEngine.Vector2(x, y)
CS.System.IO.Path.Combine(CS.LHGFData.Utils.ResoucePaths.BackgroundPath, path)
CS.LHGFData.ImageLayer.LoadTextureByIO(path)
CS.TypeTransform.string2float(str)     -- 字符串转 float（SequenceExtendtion.cs 提供）
CS.SequenceExtensions.Append(seq, tween)
```

### 10.2 内置动画命令（`xLuaDOTween/`）

| Lua 文件 | 用途 | 常用参数 |
| --- | --- | --- |
| `SetAt.lua` | 设置到指定位置 | posX, posY, Time |
| `SetAtFirst.lua` | 设置到最开始位置 | — |
| `SetAtLeft.lua` / `SetAtRight.lua` | 靠左 / 靠右 | Time |
| `SetCI.lua` | 切换立绘图 | ImageCharacterIllustration |
| `MoveTo.lua` | 移动到指定位置 | posX, posY, Time |
| `MoveToLeft.lua` / `MoveToRight.lua` | 移向左 / 移向右 | Time |
| `MoveToLeftOutside.lua` / `MoveToRightOutside.lua` | 移出屏幕外 | Time |
| `ShakeX.lua` | 水平抖动 | ShakeTime, Delta, Time |
| `SetBg.lua` | 切换背景图（淡入） | BgImage, Time, colorR/G/B/A |
| `SetBgBlinds.lua` | 百叶窗切换背景 | BgImage, Time |
| `SetCamera.lua` | 摄像机缩放/移动 | Size, posX, posY, rotation |
| `Quit.lua` | 淡出移除 | Time |
| `Null.lua` | 空操作 | — |

`lua_files.txt`：列出目录下所有 Lua 文件名（供 Android 端读取清单）。新增/删除 Lua 文件后，
在 Unity 中执行菜单 **`Tools/Generate Lua File List`**（`BuildLuaFileList.cs`）重新生成。

> Windows 下实际按目录枚举，`lua_files.txt` 主要用于 Android 平台，但仍建议保持同步。

### 10.3 新增一个动画（三步）

1. 在 `Assets/StreamingAssets/HGF/xLua/xLuaDOTween/` 新建 `MyAnim.lua`，按 10.1 结构实现；
2. Unity 菜单 `Tools/Generate Lua File List` 更新 `lua_files.txt`；
3. 剧本中引用：`<ImageCharacterAnimation CharacterID="1" DOTweenAnimation="MyAnim" Time="0.5"/>`
   （或 `ChangeImageBackground` 的 `DOTweenAnimation="MyAnim"`）。

> 进阶：若 Lua 需要调用**新的 C# 方法**，需要 XLua 生成代码。
> 编辑 `Xlua` 配置（`Assets/XLua/Editor/XLuaUnityDefaultConfig.cs` 等）后，
> 通过菜单 `XLua/Generate Code` 重新生成 `Assets/XLua/Gen/`。C# 侧也可仿照
> `SequenceExtendtion.cs` 为 Lua 提供薄封装（扩展方法 + `TypeTransform`）。

---

## 11. 存读档与设置

### 11.1 存档

- 位置：`SaveData/` 目录（Windows 下 = 项目根目录 `SaveData/`；Android 下 = `persistentDataPath`）。
- 文件名：`save_<id>.sav`（JSON，`Newtonsoft.Json` 序列化）。
- 内容：`GameSaveData`（节点 ID + 各层 `ILayer.Save()` 数据）。
- 管理类：`GameSaveDataManager`（`LHGFGameSaveData.cs`）。
- 读档流程：`LHGFGameMain.ResetGame()` → 载入 `scriptGraph` → 各层 `Load()` → `OnGameLoad()` 重放。

> 层数据（`Save()`/`Load()`）：需要持久化的层（如立绘层）自行实现。新增状态时记得实现这两个方法。

### 11.2 设置（音量等）

- `LHGFGameConfigControler`：BGM / 音效 / 语音三个 Slider；
- 数据持久化于 `GameConfigDataManager`（JSON 写入 `SaveData/` 或持久化路径）；
- `LHGFGameMain.gameConfigDataManager` 全局访问。

### 11.3 全局数据/标记（AddCharacter 追踪等）

- `GameGlobaDataManager`：跨存档的全局记录（如章节解锁、立绘/角色状态）；
- `Blackboard`/`ChapterImage` 等层各有 `Save/Load` 实现，回撤/读档自动还原。

---

## 12. 二次开发专题

### 12.1 新增一个「层」（功能模块）

以「新增一个天气层」为例：

1. **实现逻辑层**：新建 `WeatherLayer.cs`，实现 `ILayer`（参考 `ChapterLayer.cs`）；
   - `Next(command)` 中读取 `command.CommandConfig`（如 `WeatherType="rain"`）并调用控制器；
   - `Finish()` 返回演出是否结束；
   - `Save()/Load()/Reset()/Withdraw()` 按需实现。
2. **实现控制器**：新建 `WeatherLayerControler.cs`（`MonoBehaviour`），
   挂到预制体上（如 `Assets/HGF/Perfab/` 下的 UI 预制体），负责实际 UI/粒子效果。
3. **注册**：
   - 场景中把控制器组件拖到 `LHGFGameMain` 的对应字段；
   - `LHGFGameMain.SetLayers()` 中 `new WeatherLayer()` 并塞入 `layers`；
   - `SetLayerCommand2layer()` 中注册：`"Weather" -> new List<string>(){"WeatherLayer"}`。
4. **剧情中使用**：`<Weather WeatherType="rain" Time="1"/>`。

> XLua 侧如需在 Lua 调用该层，可仿照 `ImageLayer`（`ImageAnimationPlayer`）暴露入口，
> 或通过 XLua 配置生成 C# 包装（新增类需 `[LuaCallCSharp]` 并重新生成代码）。

### 12.2 新增剧情命令

1. XML 中定义新标签（如 `<FadeOutScreen Time="1"/>`）；
2. 映射到层：`SetLayerCommand2layer()` 添加 `"FadeOutScreen" -> ...`；
3. 在目标层的 `Next()` 中读取 `CommandConfig["Time"]` 处理。

### 12.3 修改 UI / 外观

- 所有 UI 预制体在 `Assets/HGF/Perfab/` 与 `Assets/HGF/Scripts/Structure_New/*/...*.prefab`：
  - `Canvas.prefab`（总 UI 根）、`Menu.prefab`（菜单）、`Setting.prefab`（设置）、
    `Dialog.prefab`/`Text-SpeakContent.prefab`（对话框）、`LongDialog.prefab`（长对话）、
    `ButtonShowMenu.prefab`、`SLPanel.prefab`（存读档面板）等；
- 字体：`Assets/HGF/Fonts/`（含 TextMeshPro SDF 与 TEXDraw 字体）；
- 图片：`Assets/HGF/Texture2D/` 与 `StreamingAssets/HGF/Texture2D/`；
- 按钮音效：`ButtonVoice.cs` 挂到按钮上，音效文件在 `Audio/EffectiveSound/`。

### 12.4 新增角色

| 步骤 | 操作 |
| --- | --- |
| 1 | `CharacterInfo.json` 增加条目（key、Name、ResourcesPath、默认立绘） |
| 2 | `Texture2D/Portrait/<ResourcesPath>/` 放入立绘（含 `Portrait-Normall.png`） |
| 3 | 需要人物信息面板时，在 `CharacterDescribeInfo.json` 增加 Name/Portrait/Describe 数组 |
| 4 | 剧情中 `AddCharacter` + `Speak CharacterID` 使用 |

### 12.5 视频 / 声音 / 背景

- 直接把文件放入对应目录即可被引用（运行期从 StreamingAssets 读取）；
- 注意视频编解码：Unity VideoPlayer 支持 mp4（H.264）；网页/移动端注意格式兼容。

### 12.6 Android 适配

- 项目已内置 Android 优化（`LHGFScriptData.cs` 中 `#if UNITY_ANDROID`）：
  纹理缓存（LRU 48 张）+ 降采样（≤2560）+ 不可读纹理，显著降内存；
- 路径差异：`Utils.ResoucePaths.GameRootPath` 在 Android 返回 `persistentDataPath`，
  存档/StreamingAssets 读取自动适配；
- 发布前建议开启 `Android Logcat` 查看运行日志（项目已带 com.unity.mobile.android-logcat）。

### 12.7 性能与内存

- 推进剧情时每 20 步或关键节点会触发 `Resources.UnloadUnusedAssets()` + Lua GC（`LHGFGameMain.TryUnloadUnusedAssets`），
  不要删除该逻辑，否则纹理与 Lua 堆会持续增长；
- 大量长对话/黑板页面注意 `TexDarwContentWrapper` 对中文逐字 `\text{}` 包裹的开销；
- 自定义动画尽量复用 Sequence 并在 `OnComplete` 中 `Destroy` 临时对象（参考 `SetBg.lua`）。

---

## 13. 常见问题 FAQ

**Q1：运行时报「找不到脚本 / Missing Script」**
> XLua 首次导入需要生成代码：`Assets/XLua/Gen/` 是否已生成完整；另确认
> 场景中 `LHGFGameMain` 的各控制器字段都已拖好（`Menu.prefab`、`Layer` 控制器等）。

**Q2：剧情改了不生效**
> 剧情在 `StreamingAssets`，若在编辑器运行，直接保存后重进 Play 即可；
> 若发布后修改，需重新导出 StreamingAssets 或走热更通道。确认 `GameConfig.json` 的 `RootNodeId` 指向正确节点。

**Q3：新动画不执行**
> 检查：1) `DOTweenAnimation` 名 = Lua 文件名（不含 `.lua`）；2) `lua_files.txt` 已重新生成；
> 3) Lua 语法错误看 Console（`Tools/Generate Lua File List` 只列清单，不编译）。

**Q4：`AddCharacter` 后立绘不显示**
> 确认 `From` 在 `CharacterInfo.json` 中存在、`ResourcesPath` 目录下有图；
> `sizeY` 决定缩放，给 800~900 之间；初始 `posX="-2000"` 表示从屏幕外移入。

**Q5：选项点了没反应 / 跳错节点**
> 确认 `Choice` 的 `NextId` 用 `%%` 分隔且数量与 `Content` 一致；
> 目标节点 Id 必须真实存在（含分支名与编号）。错误分支的节点也要写 `NextId` 闭环。

**Q6：存读档后状态不对**
> 检查该功能的 Layer 是否实现了 `Save()/Load()`，并在 `LHGFGameMain.SetLayers()` 中注册；
> 读档会走 `Load(SaveData)` + `OnLoadFinish()`，动画层走 Lua 的 `OnLoad` 分支（`Mode="OnLoad"`）。

**Q7：想复用现有立绘层做其他图（道具、CG）**
> 立绘层 `ImageCILayer` 是通用图片层，`AddCharacter From=角色Key` 即可摆放任意图片；
> 也可新增一个 Layer 克隆 `ImageCILayer` 改路径常量。

**Q8：LaTeX 公式/表格不显示**
> 文本所在命令需 `TextType="TexDraw"`（对话框/长对话/黑板默认即 TexDraw）；
> 黑板表格用 `\begin{tabular}`，注意 XML 转义 `&` → `&amp;`、`\` 保持不变、换行 `\\`（XML 中两个反斜杠）。

**Q9：如何改游戏标题/图标**
> 标题在 `StartMenuConfig.json` 的 `Title`；图标/产品名在
> `ProjectSettings/PlayerSettings`（Product Name、Icon）。

**Q10：多个剧本之间如何衔接**
> `NextId` 直接指向另一剧本的节点即可（如 `Ch2Part2-MAINBRANCH-0`），
> 框架按节点名自动加载对应 XML；`GameConfig.json` 的 `RootNodeId` 指向起始剧本起始节点。

---

## 附录：常用代码入口（二次开发速查）

| 需求 | 位置 |
| --- | --- |
| 全局总控 | `LHGFGameMain.instance`（`Progress/LHGFGameMain.cs`） |
| 进度/前进/回撤 | `LHGFGameMain.instance.gameProgress` |
| 全局数据 | `LHGFGameMain.instance.gameGlobaDataManager` |
| 设置数据 | `LHGFGameMain.instance.gameConfigDataManager` |
| 存档数据 | `LHGFGameMain.instance.gameSaveDataManager` |
| 资源路径 | `LHGFData.Utils.ResoucePaths.*` |
| 读取文本 | `LHGFData.Utils.ReadTextSync(path)` |
| 读取图片 | `LHGFData.ImageLayer.LoadTextureByIO(path)` |
| XLua 环境 | `LHGFData.Utils.xLuaEnv.luaEnv` |
| 命令路由表 | `LHGFGameMain.SetLayerCommand2layer()` |
| 层接口 | `ILayer`（`LHGFScriptData.cs`） |
| Lua 动画加载 | `LHGFData.Utils.LoadDOTweenLuaCommandsAsync()` |
| XLua C# 封装示例 | `Assets/HGF/Scripts/XLua/SequenceExtendtion.cs` |

---

*文档基于当前仓库（LHGF2608）实际代码编写，随着框架演进请以代码为准。*