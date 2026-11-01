# LHGF 剧本与开发手册

> 适用项目：`LHGF2609`（Elephant Head）
> 引擎：LHGF 1.x（LaTeX-supported HOILAI Galgame Framework），Unity 2022.3.54f1c1 / URP
> 本文基于当前仓库代码实际核对，替换了旧版《LHGF使用说明.md》中已过时的描述（`TextType` 默认值、`Smart` 渲染、`Image` 缩放、`WebView` 状态等）。
>
> 全文分两部分：
>
> - **第一部分：只写 XML 的剧本作者** —— 不接触 C#、不改 Unity，只改剧本和素材。
> - **第二部分：二次开发者** —— 要在本项目基础上加功能、改框架、做发布。

---

# 第一部分 · 剧本作者手册（只改 XML）

## 1. 你需要知道的四件事

1. **游戏内容全在 `Assets/StreamingAssets/HGF/` 下**，改这里的东西不影响 C# 编译，改完重新 Play 即可；
2. **剧情写在 `ScriptSheets/*.xml`**，一个文件 = 一个剧本，文件里是一串「节点（PlotNode）」；
3. **每个节点 = 玩家按一次回车后发生的一屏演出**，节点用 `NextId` 串成一条链；
4. **入口** 由 `GameConfig.json` 的 `RootNodeId` 指定。

## 2. 目录速查

```
Assets/StreamingAssets/HGF/
├── ScriptSheets/            ★ 剧本 XML（写剧情的地方）
├── GameConfig.json          ★ 游戏从哪个节点开始
├── CharacterInfo.json       角色表（ID → 名字/立绘目录）
├── EvidenceInfo/            证据正文、人物描述
├── StartMenuSetting/        开始菜单背景/BGM/标题
├── WebPage/                 WebView 命令打开的本地网页
├── xLua/xLuaDOTween/        动画命令（.lua，高级用法，见 12 节）
├── Audio/
│   ├── BGM/                 背景音乐
│   ├── CharacterVoice/      角色语音（CVPath）
│   └── EffectiveSound/      音效
├── Texture2D/
│   ├── BackgroundImage/     背景图（含 chapterImage\、ch2cg\ 等子目录）
│   ├── Portrait/<角色>/     立绘（目录名见 CharacterInfo.json 的 ResourcesPath）
│   ├── FullbodyPortrait/    人物信息面板的全身立绘
│   ├── LongSpeakImage/      长对话/黑板插图
│   └── ImageAnimation/      序列帧动画
└── VideoResource/VideoAnimation/   视频
```

## 3. 写第一个剧本

### 3.1 最小可用文件

新建 `Assets/StreamingAssets/HGF/ScriptSheets/MyStory.xml`：

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Root>
  <MainPlot>

    <PlotNode Id="MyStory-MAINBRANCH-0" NextId="MyStory-MAINBRANCH-1">
      <ChangeImageBackground BgImage="Park.jpg"/>
      <Speak SpeakerName="象山" Content="第一句话。"/>
    </PlotNode>

    <PlotNode Id="MyStory-MAINBRANCH-1" NextId="GAMEFINISH">
      <Speak SpeakerName="象山" Content="第二句话，然后游戏结束。"/>
    </PlotNode>

  </MainPlot>
</Root>
```

### 3.2 让游戏从它开始

编辑 `Assets/StreamingAssets/HGF/GameConfig.json`：

```json
{
    "RootNodeId": "MyStory-MAINBRANCH-0"
}
```

### 3.3 运行

Unity 里打开 `Assets/Scenes/MainScenceNew.unity` → Play → 开始菜单 → 开始游戏。

## 4. 节点（PlotNode）规则

### 4.1 命名规则：`脚本名-分支名-编号`

```
MyStory-MAINBRANCH-0
│        │          └── 分支内编号，从 0 递增
│        └── 分支名，主线固定 MAINBRANCH
└── 剧本文件名（不含 .xml）
```

**必须遵守**，因为框架靠节点名反推「该去哪个 XML 里找这个节点」。

- **脚本名、分支名里不能出现 `-`**（框架按 `-` 切分节点名）；
- `Id` 全局唯一（跨剧本也不要重名）；
- 起始节点必须是 `脚本名-MAINBRANCH-0`，因为 `GameConfig` 只能指向这里（详见 4.4）。

### 4.2 `NextId`

- 普通节点：`NextId="MyStory-MAINBRANCH-1"`；
- 结束：`NextId="GAMEFINISH"`；
- **跨剧本**：直接写另一个剧本的节点名，框架会自动加载新文件：

```xml
<PlotNode Id="Ch2Part1-MAINBRANCH-9" NextId="Ch3Part1-MAINBRANCH-0">
```

- **多分支**（选项用）：用 `%%` 分隔，如 `NextId="A-0%%B-0%%C-0"`。

### 4.3 一个节点里可以放多条命令

节点里的命令**自上而下依次执行**，不同层的命令互不干扰，可以同时演出：

```xml
<PlotNode Id="MyStory-MAINBRANCH-3" NextId="MyStory-MAINBRANCH-4">
  <ImageCharacterAnimation CharacterID="4" DOTweenAnimation="SetCI" ImageCharacterIllustration="mask.png" Time="0.01"/>
  <Speak CharacterID="5" Content="爸爸？" CVPath="c5_01.wav"/>
  <CameraAnimation DOTweenAnimation="SetCamera" Size="540" PosX="0" PosY="0" Time="0.3"/>
</PlotNode>
```

同层命令会排队播放；**所有层都演出完（`Finish()` 返回 true）才能按回车继续**。
注意：文本类命令（`Speak`/`LongSpeak`/`Blackboard`）的打字机动画会阻塞前进，动画类（立绘位移、镜头）不会。

### 4.4 关于「起点只能是 MAINBRANCH-0」

框架用「从 `脚本名-MAINBRANCH-0` 出发的最短路径」来定位节点，所以：

- 每个剧本的入口节点都应该是 `XXX-MAINBRANCH-0`；
- 如果想从中途开始测试，可以临时把 `GameConfig.RootNodeId` 指向 `XXX-MAINBRANCH-N`（只要该节点在从 0 号出发的链上就能解析），但**发布时请指回主线入口**。

## 5. 文本渲染：TMP 与 TEXDraw（最重要的规则）

引擎里有两套文本渲染器：

| 渲染器 | 特点 | 适合 |
| --- | --- | --- |
| **TMP**（TextMeshPro） | 快、排版稳、字距正常 | 普通中文对白、选项、菜单 |
| **TEXDraw** | 能渲染 `$...$` LaTeX；但中文必须逐字包 `$\text{字}$`，长文本性能差 | 含公式、表格、数学符号的段落 |

### 5.1 自动判断（默认行为）

**不写 `TextType` 时**，框架会看文本内容：

- 出现**成对、且中间有内容**的 `$...$` 或 `$$...$$` → 用 **TEXDraw**；
- 其余一律 → 用 **TMP**。

判定细节：

| 写法 | 结果 |
| --- | --- |
| `今天天气不错` | TMP |
| `当 $x=1$ 时` | TEXDraw |
| `$$a^2+b^2=c^2$$` | TEXDraw |
| `价格 $5`（只有一个 `$`） | TMP，原样显示 `$5` |
| `\$5`（转义） | TMP，显示字面 `$5` |
| `$ $`（只有空白） | TMP |

### 5.2 手动指定

| 写法 | 效果 |
| --- | --- |
| `TextType="TEXDraw"` / `"TexDraw"` | 强制 TEXDraw |
| `TextType="TMP"` | 强制 TMP（`$` 原样显示） |
| `TextType="tmp"`、`" texdraw "` | 大小写、空格都不影响 |
| 其他值 | 忽略，退回自动判断 |

> 老剧本里常见的写法 `TextType="TexDraw%%TexDraw"` 仍然有效。

### 5.3 换行与段落

| 写法 | TMP 下 | TEXDraw 下 |
| --- | --- | --- |
| `\\`（两个反斜杠） | 换行 | 原样交给 TEXDraw |
| `@@` | 换行 | 两个换行（空一行） |

对话框、长对话、黑板三处行为一致。XML 属性里注意转义 `&` → `&amp;`、`<` → `&lt;`、`>` → `&gt;`、引号 → `&quot;`。

### 5.4 公式写法

```xml
<Speak SpeakerName="老师" Content="勾股定理：$a^2+b^2=c^2$"/>
<Speak SpeakerName="老师" Content="$$\int_0^1 x^2\,dx=\frac{1}{3}$$"/>
<Blackboard Content="\begin{tabular}{| l | x |} \hline 项目&amp; 数值 \\ \hline 甲&amp; 1 \\ \hline \end{tabular}" TextType="TexDraw"/>
```

TEXDraw 支持的指令请参考 TEXDraw 插件文档。推荐只把公式放进 `$...$`，其余文字留给 TMP。

### 5.5 性能建议（写给作者）

- 大段纯中文对白**不要**写 `TextType="TexDraw"`；
- 一句里混少量公式没问题（整句走 TEXDraw），但整页长文本 + TEXDraw 会明显变慢；
- 长对话/黑板里插图 + 公式一起用时，尽量拆成多个节点。

## 6. 命令参考

> 通用：属性值都是字符串；布尔用 `0`/`1`；时间单位秒。
> **带 ★ 的是必填**。

### 6.1 `ChangeImageBackground` 背景

```xml
<ChangeImageBackground BgImage="Park.jpg"/>
<ChangeImageBackground BgImage="ch2cg/cg1.png" DOTweenAnimation="SetBgBlinds" Time="0.7"/>
```

| 属性 | 说明 |
| --- | --- |
| ★ `BgImage` | 相对 `Texture2D/BackgroundImage/` 的文件名，子目录用 `\` 或 `/` |
| `DOTweenAnimation` | 转场动画：`SetBg`（默认，淡入）/ `SetBgBlinds`（百叶窗）/ `Null` |
| `Time` | 时长 |
| `colorR/G/B/A` | 淡入起始色（可选） |

### 6.2 `Speak` 对话框

```xml
<Speak SpeakerName="象山" Content="你好。" DialogBarColor="0,0,0,0.83"/>
<Speak CharacterID="5" Content="爸爸？" CVPath="c5_01.wav"/>
```

| 属性 | 说明 |
| --- | --- |
| ★ `Content` | 正文 |
| `SpeakerName` | 说话人；不写且给了 `CharacterID` 时自动取角色名 |
| `CharacterID` | 立绘实例 ID（见 `AddCharacter`），用于自动取名 |
| `DepartmentName` | 说话人下方的小字（如部门/职位） |
| `CVPath` | `Audio/CharacterVoice/` 下的语音文件 |
| `TextType` | 见第 5 节，可省略 |
| `Skip` | `1` = 允许被跳过 |
| `DialogBarColor` | 对话条底色 `r,g,b` 或 `r,g,b,a` |

### 6.3 `LongSpeak` 长对话（整页滚动）

```xml
<LongSpeak Continue="0" Content="第一段文字。@@第二段文字。"/>
<LongSpeak Content="含公式的段落：$E=mc^2$"/>
<LongSpeak TextType="Image" Content="fig11.png" Rate="0.6"/>
```

| 属性 | 说明 |
| --- | --- |
| ★ `Content` | 长文本；`TextType="Image"` 时是 `LongSpeakImage/` 下的文件名 |
| `TextType` | 省略=自动；`TMP` / `TexDraw` / `Image` |
| `Continue` | `0`=先清空再显示（默认 `1` 追加到上一段后面） |
| `Rate` | 仅图片：**图片区域高度 = 内容区高度 × Rate**，默认 `0`（不写会不可见！） |
| `Skip` | 允许跳过文字动画 |

**`Rate` 用法**：图片按原始宽高比缩放到「内容区宽 × (内容区高×Rate)」的框内并居中。
竖图（如 2048×3056）用 0.5~0.7，横图（如 2158×1333）用 0.3~0.5。

### 6.4 `Blackboard` 黑板（讲解/插图）

```xml
<Blackboard Content="一段讲解" PosInfo="UpperCenter" SizeX="0.6" SizeY="0.4" Continue="0"/>
<Blackboard Content="fig11.png" TextType="Image" Rate="0.6" PosInfo="UpperCenter" SizeX="0.4" SizeY="0.6"/>
<Blackboard BlackboardState="0"/>
```

| 属性 | 说明 |
| --- | --- |
| `Content` | 文本或图片文件名；**不写 = 只做开关** |
| `TextType` | 省略=自动；`TMP` / `TexDraw` / `Image`（**不再继承上一条**） |
| `BlackboardState` | `1` 打开（默认）/ `0` 关闭 |
| `PosInfo` | `UpperLeft` / `UpperCenter` / `UpperRight` |
| `PosX` / `PosY` | `PosInfo` 没匹配时用的锚点（0~1） |
| `SizeX` / `SizeY` | 相对画面尺寸（0~1），默认 0.6 |
| `FontSize` | 字号，默认 36（会继承上一条） |
| `Rate` | 图片高度占比，同 `LongSpeak` |
| `Continue` | `0` 先清空黑板 |

### 6.5 `Choice` 选项

```xml
<PlotNode Id="MyStory-MAINBRANCH-10" NextId="MyStory-MAINBRANCH-11%%MyStory-MAINBRANCH-12">
  <Choice NextId="MyStory-MAINBRANCH-11%%MyStory-MAINBRANCH-12"
          Content="选项一%%选项二：含公式 $x^2$"/>
</PlotNode>
```

| 属性 | 说明 |
| --- | --- |
| ★ `NextId` | 每个选项的去向，`%%` 分隔 |
| ★ `Content` | 每个选项的文字，`%%` 分隔，数量必须与 `NextId` 一致 |
| `TextType` | 可省略；写了就要与选项一一对应（`TMP%%TexDraw`），某项留空则该选项自动判断 |

**重要限制：不要放两个相邻的选项节点。**
当前框架 `LayerShower` 会先关闭选项层再按命令打开，而点击回调在 `Forward` 之后又会关一次；
因此「本节点有 Choice → 下一个节点也有 Choice」时，第二组选项不会显示。
需要连续选择时，中间插一个普通 `Speak` 节点。

### 6.6 `AddCharacter` 添加立绘

```xml
<AddCharacter CharacterID="4" From="IZUMI" posX="-2000" Time="0.01" sizeY="800"/>
<AddCharacter From="WD" CharacterID="5" DOTweenAnimation="SetAtRight" posX="-2000" posY="default" Time="0.01" sizeY="800"/>
```

| 属性 | 说明 |
| --- | --- |
| ★ `CharacterID` | 本场景内的实例 ID（数字字符串），后续动画/说话都用它 |
| ★ `From` | `CharacterInfo.json` 里的角色 key |
| `posX` / `posY` | 初始位置；`posY="default"` 表示默认高度 |
| `sizeY` | 立绘高度，常用 800~900 |
| `DOTweenAnimation` | 入场动画，默认 `SetAt` |
| `Time` | 入场时长 |

### 6.7 `ImageCharacterAnimation` 立绘动画

```xml
<ImageCharacterAnimation CharacterID="4" DOTweenAnimation="SetCI" ImageCharacterIllustration="mask.png" Time="0.01"/>
<ImageCharacterAnimation CharacterID="5" DOTweenAnimation="MoveToLeft" Time="0.3"/>
<ImageCharacterAnimation CharacterID="5" DOTweenAnimation="ShakeX" Time="0.5" ShakeTime="3" Delta="20"/>
<ImageCharacterAnimation CharacterID="6" DOTweenAnimation="Quit" Time="1"/>
```

| 属性 | 说明 |
| --- | --- |
| ★ `CharacterID` | 立绘实例 ID |
| ★ `DOTweenAnimation` | `xLuaDOTween/` 里的动画名：`SetAt` `SetAtLeft` `SetAtRight` `SetAtFirst` `SetCI` `MoveTo` `MoveToLeft` `MoveToRight` `MoveToLeftOutside` `MoveToRightOutside` `ShakeX` `Quit` `Null` |
| `ImageCharacterIllustration` | 切换表情/服装的图片名（`SetCI` 用） |
| `posX` / `posY` | 目标位置 |
| `ShakeTime` / `Delta` | `ShakeX` 的次数/幅度 |
| `Time` | 时长 |
| `ConnectMethod` | `Append`（排队，默认）/ `Join`（与上一条并行） |

### 6.8 `CameraAnimation` 镜头

```xml
<CameraAnimation DOTweenAnimation="SetCamera" Size="250" posX="0" posY="-250"/>
<CameraAnimation DOTweenAnimation="SetCamera" Size="540" PosX="0" PosY="0" rotation="0" Time="1" IsFade="1"/>
```

| 属性 | 说明 |
| --- | --- |
| `DOTweenAnimation` | `SetCamera`（默认） |
| `Size` / `posX` / `posY` / `rotation` | 取景参数 |
| `Time` | 时长 |
| `IsFade` | `1` 渐变过渡 |

### 6.9 `VideoAnimation` 视频

```xml
<VideoAnimation VideoName="op.mp4" IsLoop="0" Size="1" Finish="1"/>
```

| 属性 | 说明 |
| --- | --- |
| ★ `VideoName` | `VideoResource/VideoAnimation/` 下文件名 |
| `IsLoop` | 循环 |
| `Size` / `Rotation` / `PosX` / `PosY` | 位置与大小（默认居中 0.5） |
| `Finish` | `1` = 播完才允许继续 |

### 6.10 `ChapterImage` 章节图

```xml
<ChapterImage ImagePath="chapterImage\Ch1.png"/>
```

| 属性 | 说明 |
| --- | --- |
| ★ `ImagePath` | 相对 `BackgroundImage/` 的路径 |

### 6.11 音频

```xml
<SetBgm BgmName="cool_vibes.mp3"/>
<SetBgm BgmName="STOPBGM"/>
<SetBgm BgmName="theme.ogg" Volume="0.3" IsLoopStr="0"/>
<EffectiveSound EffectiveSoundPath="yingqingsheng.wav"/>
```

| 命令 | 属性 |
| --- | --- |
| `SetBgm` | ★`BgmName`（`STOPBGM` = 停止）、`Volume`(0~1，默认 0.5)、`IsLoopStr`(默认 1) |
| `EffectiveSound` | ★`EffectiveSoundPath` |
| 语音 | 不单独使用，挂在 `Speak`/`LongSpeak` 的 `CVPath` 上 |

### 6.12 证据与人称面板（推理玩法）

```xml
<!-- 获得证据 -->
<AddEvidence ChapterName="一/前兆" EvidenceName="里岛一年的证词1" EffectiveSoundPath="GetEvidence.wav"/>

<!-- 出示证据：第一个是"正确答案"，最后一个是"默认（错）"分支 -->
<EvidenceChoice NextId="Ch10-Scene2-51%%Ch10-WrongAnswer-8%%Ch10-WrongAnswer-8"
                ChapterName="五/发病%%一/前兆%%一/前兆"
                EvidenceName="演唱会当天的状况%%夏天的天气%%彩夏的晒痕"/>

<!-- 人物信息面板 -->
<CharacterDescribe CharacterID="Z" Info="0%0%0"/>
```

| 命令 | 属性 |
| --- | --- |
| `AddEvidence` | ★`ChapterName`、★`EvidenceName`、`EffectiveSoundPath` |
| `EvidenceChoice` | ★`NextId`（`%%` 分隔）、★`ChapterName`、★`EvidenceName`，三者数量一致 |
| `CharacterDescribe` | ★`CharacterID`（`CharacterDescribeInfo.json` 的 key）、★`Info`=`"姓名下标%立绘下标%描述下标"`（分隔符是单个 `%`） |

### 6.13 `PhoneChat` 手机聊天

同一节点的多条 `PhoneChat` 会依次执行，所以一段聊天可以写在一个节点里：

```xml
<PlotNode Id="MyStory-MAINBRANCH-20" NextId="MyStory-MAINBRANCH-21">
  <PhoneChat PhoneState="1" Continue="0" TalkerName="象山"/>
  <PhoneChat Content="在吗？"/>
  <PhoneChat Content="在。" RightSide="1"/>
  <PhoneChat Content="算了。" TypingTime="1.5"/>
  <PhoneChat TypingTime="2"/>
</PlotNode>
```

| 属性 | 说明 |
| --- | --- |
| `PhoneState` | `1` 显示手机 / `0` 隐藏 |
| `Continue` | `1` 续写（默认）/ `0` 先清空 |
| `TalkerName` | 顶部聊天对象名，不写=不变 |
| `ContentType` | `TMP`（默认）/ `File` / `TimeStamp` / `Image` / `Gif` / `TexDraw` |
| `Content` | 正文/时间戳/文件名/图片路径；不写 = 只做控制（如纯等待） |
| `RightSide` | `1` 自己（靠右） |
| `AvatarPath` | 头像路径 |
| `TypingTime` / `TypingText` | 弹出前的「对方正在输入」秒数/文字 |
| `Icon` / `FileSize` | `File` 类型专用 |
| `Skip` | 跳过弹出动画 |

> 注意：`TexDraw` 类型目前控制器里没有接 texBar，公式会按普通文本显示，建议手机上先用 `TMP`。

### 6.14 `WebView` 内嵌网页

```xml
<WebView Url="help.html" State="1" PosInfo="UpperCenter" SizeX="0.8" SizeY="0.6"/>
<WebView State="0"/>
```

| 属性 | 说明 |
| --- | --- |
| `Url` | 带 `http://`/`https://` 视为外部地址；否则找 `HGF/WebPage/` 下的本地文件 |
| `State` | `1` 打开（默认）/ `0` 关闭 |
| `PosInfo` | `UpperLeft` / `UpperCenter` / `UpperRight` |
| `PosX` / `PosY` | 未匹配关键词时的锚点 |
| `SizeX` / `SizeY` | 相对画布尺寸 |

## 7. 配置文件

### 7.1 `GameConfig.json`

```json
{ "RootNodeId": "MyStory-MAINBRANCH-0" }
```

### 7.2 `CharacterInfo.json`

```json
{
  "IZUMI": { "Name": "和泉", "ResourcesPath": "IZUMI", "Portrait-Normall": "Portrait-Normall.png" }
}
```

- key：`AddCharacter From` 用的 ID；
- `ResourcesPath`：`Texture2D/Portrait/<ResourcesPath>/` 目录名；
- `Portrait-Normall`：默认立绘文件名（注意原字段拼写是 `Normall`）。

### 7.3 `EvidenceInfo/EvidenceInfo.json`

```json
{
  "一/前兆": {
    "里岛一年的证词1": "正文……|||摘要……"
  }
}
```

外层 = `ChapterName`，内层 = `EvidenceName`，值用 `|||` 分隔正文与摘要。

### 7.4 `EvidenceInfo/CharacterDescribeInfo.json`

```json
{
  "Z": {
    "Name": ["象山晴太", "象山晴太（幸运者）"],
    "Portrait": ["3.png", "3.png"],
    "Describe": ["描述一", "描述二"]
  }
}
```

`Portrait` 里的文件名相对 `Texture2D/FullbodyPortrait/`。

### 7.5 `StartMenuSetting/StartMenuConfig.json`

```json
{ "BGM": "G9Fried@GRM.mp3", "Background": "Untitled.jpg", "Title": "标题" }
```

## 8. 操作与快捷键

| 按键 | 功能 |
| --- | --- |
| `Enter` / 空格 | 前进 |
| `Backspace` | 回撤到上一节点 |
| `Ctrl` | 跳过 |
| 鼠标滚轮上 | 打开对话记录 |
| Esc / 菜单按钮 | 游戏菜单（存档/读档/设置） |

## 9. 剧本作者 FAQ

**Q1：改了剧本没生效？**
确认 `GameConfig.RootNodeId` 指向正确节点；确认文件名与节点 id 前缀一致；重新 Play。

**Q2：报 `KeyNotFoundException` / 节点找不到？**
节点名必须严格是 `脚本名-分支名-编号`；`NextId` 指向的节点必须存在；脚本名里不能有 `-`。

**Q3：中文没显示 / 变成 `$\text{字}$`？**
说明这段走了 TEXDraw 但中文没被包装（通常是手写 `TextType="TexDraw"` 后又塞了不规范内容）。优先删掉 `TextType` 让它自动判断。

**Q4：公式不显示，显示成 `$x^2$`？**
说明走了 TMP。检查 `TextType="TMP"` 是否写多了，或者 `$` 只有一个/被 `\` 转义了。

**Q5：选项不弹出来？**
见 6.5 的「不要放两个相邻选项节点」。

**Q6：长对话/黑板图片看不见或巨大？**
`Rate` 默认 0 会导致高度为 0；竖图 `Rate` 别超过 0.7。

**Q7：用什么编码保存 XML？**
**UTF-8（建议无 BOM）**。项目已把所有 `.cs`、`.xml` 统一为 UTF-8。

---

# 第二部分 · 二次开发者手册

## 10. 工程与架构

### 10.1 环境

| 项 | 值 |
| --- | --- |
| Unity | 2022.3.54f1c1 |
| 渲染 | URP |
| 主场景 | `Assets/Scenes/MainScenceNew.unity`（构建里还有 StartMenu / MainScence / Preview 等） |
| 第三方 | XLua、DOTween、TEXDraw、TextMeshPro、Newtonsoft.Json、QuikGraph、UniGif、UnityWebBrowser |

### 10.2 核心流程

```
PlotNode(命令列表)
   │  LHGFGameMain.Forward()
   ▼
LHGF_GameProgress.Forward(data)
   ├─ 每层 BeforeNextStart()
   ├─ CurrentNodeId = NextNodeId
   ├─ NextNodeId = Next(CurrentNodeId)        // 取 NextId（可能是多分支）
   ├─ 遍历本节点命令:
   │     layerCommand2layer[命令名] → 各层 Next(command)
   │     收集各层 Log(command) 进 LogContents
   ▼
LHGFGameMain.LayerShower(nextNode)  // 决定 Speak/LongSpeak/Choice 等谁可见
```

按回车时若 `IsFinish == false` 先调 `gameProgress.Skip()` 跳完正在播放的演出。

### 10.3 `ILayer` 接口

```csharp
string LayerName();
void   Next(LayerCommand command);      // 收到命令
bool   Finish();                        // 本层演出是否结束（决定能否前进）
void   Skip();                          // 跳过演出
bool   Withdraw();                      // 回撤
object Save();  void Load(object data); // 存读档
void   Reset();
Dictionary<string,string> Log(LayerCommand command);  // 对话记录
GameObject GetControler();
void   BeforeNextStart();
void   BeforeNextOnLoadStart();
void   OnStart();  void OnUpdate();  void OnLoadFinish();
```

**约定**：Layer 是不挂 GameObject 的纯逻辑类；Controller 是 MonoBehaviour，负责 UI。两者一一对应。

### 10.4 命令路由表

[LHGFGameMain.SetLayerCommand2layer()](../Assets/HGF/Scripts/Structure_New/Progress/LHGFGameMain.cs)：

| 命令 | 层 |
| --- | --- |
| `Speak` | SpeakLayer, CVLayer |
| `LongSpeak` | LongSpeakLayer, CVLayer |
| `Blackboard` | BlackboardLayer |
| `Choice` | ChoiceLayer |
| `AddCharacter` | DataLayer, ImageCILayer |
| `ImageCharacterAnimation` | ImageCILayer |
| `ChangeImageBackground` | ImageBgLayer |
| `CameraAnimation` | CameraLayer |
| `VideoAnimation` | VideoAnimationLayer |
| `SetBgm` | BgmLayer |
| `EffectiveSound` | EffectiveSoundLayer |
| `AddEvidence` | EvidenceLayer, EffectiveSoundLayer |
| `EvidenceChoice` | EvidenceChoiceLayer |
| `CharacterDescribe` | CharacterInfoLayer |
| `ChapterImage` | ChapterLayer |
| `WebView` | WebViewLayer |
| `PhoneChat` | PhoneChatLayer |

### 10.5 文本渲染链路（本次重点改动）

```
命令属性 → XXLayer 的 Data 构造函数
   ├─ TextType = 显式值(Trim/大小写归一化) 或 ""
   ├─ 空 → Utils.ContainsFormula(Content) ? "TexDraw" : "TMP"
   └─ Content = Utils.PrepareContent(Content, TextType)
        ├─ TexDraw: @@→\n\n，然后 Utils.TexDarwContentWrapper（中文逐字 $\text{}$）
        └─ TMP:     @@→\n，\\→\n，中文不动
   ▼
Controller 按 TextType 选用 TMP 预制体或 TEXDraw 预制体
   ▼
TextTweener（打字机）写入对应组件
```

相关工具方法都在 [`LHGFData.Utils`](../Assets/HGF/Scripts/Structure_New/Progress/LHGFScriptData.cs)：

```csharp
Utils.ContainsFormula(string)                 // 检测成对的 $...$ / $$...$$
Utils.PrepareContent(string, string textType) // 统一文本预处理
Utils.TexDarwContentWrapper(string)           // 中文逐字包装（仅 TEXDraw）
```

## 11. 二次开发：常见任务

### 11.1 新增一个命令（最常用）

以新增 `<Weather>` 为例：

1. **建 Layer**：`Assets/HGF/Scripts/Structure_New/Layers/WeatherLayer/WeatherLayer.cs`
   ```csharp
   public class WeatherLayer : ILayer
   {
       public WeatherLayerControler controler;
       public void Next(LayerCommand command)
       {
           string type = Utils.GetDicValue(command.CommandConfig, "Type", "rain");
           float time = float.Parse(Utils.GetDicValue(command.CommandConfig, "Time", "1"));
           controler.Play(type, time);
       }
       public bool Finish() => controler.IsFinished;
       // 其余接口按需实现（Save/Load/Withdraw/Reset 可先空实现）
   }
   ```
2. **建 Controller**：挂在场景/prefab 上的 MonoBehaviour。
3. **注册**：
   - `LHGFGameMain` 增加 `public WeatherLayerControler WeatherLayerControler;`（Inspector 拖引用）；
   - `SetLayers()` 里 `new WeatherLayer()` 并 `layers.Add("WeatherLayer", ...)`；
   - `SetLayerCommand2layer()` 里 `objectCommand2layer.Add("Weather", new List<string>{"WeatherLayer"});`
4. 剧本：`<Weather Type="rain" Time="1"/>`。

> 忘记注册会抛 `KeyNotFoundException`（`layerCommand2layer[命令名]`）。

### 11.2 修改文本类型判断规则

规则集中在 `Utils`，改一处全项目生效：

- `ContainsFormula`：改公式识别（比如要支持 `\(...\)`）；
- `PrepareContent`：改换行/包装规则；
- 各 Layer 的 `TryParse` 段（`DialogLayer` / `LongDialogLayer` / `BlackboardLayer` / `ChoiceLayer`）。

如果想加第三种渲染方式（比如「RichTMP」），按同样模式在 `TextType` 解析里加分支，并在各 Controller 的 `if/else` 里加预制体。

### 11.3 新增一个动画（XLua + DOTween）

1. 在 `Assets/StreamingAssets/HGF/xLua/xLuaDOTween/` 新建 `MyAnim.lua`：
   ```lua
   function Set(_self) end
   function OnPlay(_self)
       local G = _self.G
       local Time = CS.TypeTransform.string2float(_self.Config["Time"])
       local seq = CS.DG.Tweening.DOTween.Sequence()
       CS.SequenceExtensions.Append(seq, G.transform:DOMoveX(100, Time))
       _self.sequence = seq
   end
   function OnLoad(_self) end
   Set(self)
   local Config = self.Config
   local ok, mode = Config:TryGetValue("Mode")
   if ok then
       if mode == "OnPlay" then OnPlay(self) elseif mode == "OnLoad" then OnLoad(self) end
   end
   ```
2. Unity 菜单 `Tools/Generate Lua File List` 刷新 `lua_files.txt`（Android 需要）；
3. 剧本：`DOTweenAnimation="MyAnim"`。

Lua 调 C#：`CS.命名空间.类名`，参数类型要匹配（字符串转数字用 `CS.TypeTransform.string2float`）。

### 11.4 改 UI

| 目标 | 预制体位置 |
| --- | --- |
| 对话框 | `Assets/HGF/Scripts/Structure_New/Layers/DialogLayer/Dialog.prefab` |
| 长对话 | `.../LongDialogLayer/LongDialogLayer.prefab` |
| 黑板 | `.../BlackboardLayer/Blackboard.prefab` |
| 选项按钮 | `.../ChoiceLayer/Button-Choice.prefab` |
| 证据面板 | `.../DataLayer/EvidenceLayer/Perfab/*.prefab` |
| 人物面板 | `.../DataLayer/CharacterInfoLayer/CharacterPanel.prefab` |
| 手机聊天 | `.../PhoneChatLayer/New Folder/Perfab/*.prefab` |
| 对话记录 | `Assets/HGF/Scripts/Structure_New/Logger/Logger.prefab` |
| 菜单/存读档/设置 | `.../Progress/Menu/*.prefab`、`Assets/HGF/Perfab/Setting.prefab` |

字体在 `Assets/HGF/Fonts/`（TMP SDF 与 TEXDraw 字体需分别配置）。

### 11.5 图片类命令的尺寸约定

`LongSpeak`/`Blackboard` 的 `TextType="Image"` 共用 `LongSpeakImageControler`：

```csharp
// 调用方
var parentRt = Content.transform.parent.GetComponent<RectTransform>();
controler.Set(parentRt.rect.height, parentRt.rect.width, rate);
controler.SetTexture();               // 锁定根物体尺寸 + 加 LayoutElement
controler.SetContentTexture(path);    // 加载图片并按比例缩放到框内
```

- 根物体尺寸 = `(可用宽度, 可用高度 × rate)`，并加 `LayoutElement.preferredHeight`，防止被 `VerticalLayoutGroup` 反向撑高；
- `ContentImage` 子物体按图片原始宽高比等比缩放后居中；
- 预制体根物体**必须挂 `LongSpeakImageControler`**，子物体要有 `Image ContentImage` 引用。

### 11.6 存读档

- 存档目录 `SaveData/`，JSON（Newtonsoft）；
- 每层实现 `Save()`/`Load()`；不实现的层读档后会回到初始状态；
- 读档流程：`ResetGame()` → 重建 `scriptGraph` → 各层 `Load()` → `OnLoadFinish()`；
- 新增持久化字段时，注意旧存档反序列化后字段为默认值。

### 11.7 已知限制（改之前先看）

| 限制 | 说明 |
| --- | --- |
| 连续选项 | `LayerShower` 先关再开 + 点击后 `SetActive(false)`，导致相邻两个 Choice 节点第二组不显示；中间插一个 `Speak` 节点规避 |
| 手机聊天的 TEXDraw | `LHGFChatLayerControler.texBar` 未接，`TexDraw` 消息按 TMP 显示 |
| 长对话图片 | `Rate` 不写（=0）时高度为 0，图片不可见 |
| 跳转起点 | 节点解析依赖 `脚本名-MAINBRANCH-0` 出发的最短路径，`GameConfig` 只能从入口指向可解析节点 |
| `@@` 字面量 | 目前 `@@` 一律被当作段落标记，正文里无法直接显示这两个字符 |

## 12. 性能与内存

- **TEXDraw 是主要开销**：长文本尽量走 TMP，公式段落尽量短；
- `LHGFGameMain.TryUnloadUnusedAssets()` 每 N 步触发 `GC.Collect` + `Resources.UnloadUnusedAssets` + Lua GC，不要删；
- Android 下 `Utils.LoadTextureByIO` 有 LRU 缓存（48 张）与降采样（≤2560），注意缓存命中路径；
- 创建临时纹理/精灵记得 `Destroy`，`Sprite.Create` 出来的对象不会自动释放。

## 13. 编码与工程约定

- **所有源码与数据文件统一 UTF-8（无 BOM）**；历史上 GBK 文件已批量转换，新增文件请保持 UTF-8；
- 不要用「另存为 UTF-8」处理原有 GBK 文件（会双重编码）；
- `.meta` 文件必须随资源一起提交，否则引用会断；
- XML 剧本换行符保持 LF/CRLF 均可，但不要混用。

## 14. 调试

| 手段 | 用法 |
| --- | --- |
| Console | 命令解析错误（`KeyNotFoundException`）通常表示缺必填属性 |
| 节点日志 | `LHGFGameMain.Forward()` 末尾 `Debug.Log(CurrentNodeId)`，可直接看当前节点 |
| 错误窗口 | `Assets/HGF/Scripts/Structure_New/Diagnostics/LHGFErrorWindow.cs` 会把异常汇总到界面与 `ErrorLog/` |
| 测试剧本 | `ScriptSheets/TextTypeTest.xml` 覆盖了文本类型、换行、图片、选项各种情况，可当回归用例 |

## 15. 发布检查清单

1. `GameConfig.RootNodeId` 指回正式入口；
2. 场景 `MainScenceNew.unity` 在 Build Settings 且启用；
3. `Tools/Generate Lua File List` 已执行（Android）；
4. `CharacterInfo.json` / `EvidenceInfo` / 立绘目录齐全，无缺图；
5. 关闭调试用的测试剧本或将其移出 `ScriptSheets`；
6. Android：确认 StreamingAssets 读取路径与 `persistentDataPath` 逻辑未被破坏。

---

*本文档与代码同步维护：改动 `Utils`、各 Layer 的解析逻辑或命令属性后，请同步更新第 5、6、10 节。*
