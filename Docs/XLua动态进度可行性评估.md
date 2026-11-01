# 可行性评估:通过 XLua 访问 LHGFGameMain 实现动态剧情(好感度系统)

> 目标:用 XLua 直接访问 `LHGFGameMain` 单例,实现「根据此前选项影响后续剧情走向」的好感度/变量系统。
> 状态:**仅评估,未修改任何代码**。本文件基于当前仓库(LHGF2608)源码逐条核验。

---

## 1. 结论摘要

| 问题 | 结论 |
| --- | --- |
| 技术上能否实现? | ✅ 完全可行。框架的进度数据(`NextNodeId` / `CurrentNodeId`)是 public 字段,天然可写;选项跳转发生在 C# 单点(`LHGFChoiceLayerControler.SetChoiceButton`),便于拦截。 |
| 当前能否「零修改」用 Lua 访问 `LHGFGameMain.instance`? | ❌ 不能直接使用。XLua 只对「已生成 wrap 的类型」提供 `CS.` 访问;当前 `LHGFGameMain` 未导出(见 §2.1)。需把相关类型加入导出配置并重新生成代码。 |
| 是否需要改框架核心逻辑? | 只需要**少量、可回退**的桥接(导出配置 + 1~2 个 hook 点),不需要推翻现有分层。 |
| 推荐路线 | **C# 侧轻量桥接 + Lua 决策函数**(§4.2),而不是让 Lua 直接操盘全部流程。 |

一句话结论:**可行,建议走「导出 + 钩子」路线**:把 `LHGFGameMain`、进度类、好感度数据类导出给 Lua;在选项点击、节点前进两个单点留出「若 Lua 注册了决策函数则调用」的桥接;好感度数值由 Lua 读写(可持久化到 C# 的 GameGlobalData 或 Lua 自管 JSON)。

---

## 2. 现状盘点(为什么要改、改哪里)

### 2.1 XLua 当前导出了什么(决定 Lua 能碰什么)

- 生效的导出配置:`Assets/XLua/Editor/LuaCallCsharpe.cs` → `GenCodeConfig.LuaCallCSharp`
  (`ExampleConfig.cs`、`ExampleGenConfig.cs` 的导出配置全部被注释,仅为模板)。
- 已导出类型(对应 `Assets/XLua/Gen/*.cs`):DOTween 全家桶、`SequenceExtensions`、`TypeTransform`、`Utils`(`LHGFData.Utils`)、`LHGFData.ImageLayer.ImageAnimationPlayer`。
- **未导出**:`LHGFGameMain`、`LHGF_GameProgress`、`LHGF_GameData`、`GameGlobaDataManager` 等全部框架类型 —— 因此目前 Lua 中 `CS.LHGFGameMain` 会失败。

> XLua 细节:未生成 wrap 的公共类型,`CS.` 前缀访问可能走「反射查找」路径,但性能差、
> IL2CPP 裁剪下有风险,且依赖运行时程序集扫描;正式做法是加入导出配置后由
> 菜单 `XLua/Generate Code` 重新生成 `Gen/*.cs`。

### 2.2 现有 Lua 脚本的执行方式(决定「剧情逻辑脚本」挂在哪)

- 动画命令:`ImageLayer.ImageAnimationPlayer` 用 `luaEnv.DoString(命令, "", scriptScopeTable)` 执行
  `xLuaDOTween/*.lua`;`scriptScopeTable` 的 `__index` 指向 `luaEnv.Global`,所以动画 Lua 能访问 `CS` 表。
- 动画 Lua 只在「该动画命令被层执行」时运行一次,且 `self` 是动画播放器 —— 不适合承载常驻好感度逻辑。
- `Utils.xLuaEnv.luaEnv`(`LHGFScriptData.cs`)是全局唯一 `LuaEnv`,已存在,可复用作常驻环境。

### 2.3 选项跳转机制(核心拦截点)

`LHGFChoiceLayerControler.SetChoiceButton(...)`:

```
Button.onClick → { NextNodeId = Button.name;   // ← 硬编码:跳转目标 = 按钮绑定的节点
                  Skip(); LogContent(); Forward(); }
```

- 选项目前是「纯静态」:每个按钮 `JumpID = NextId[i]`,点击即跳该节点。
- **在这里插入「Lua 决策」即可实现好感度分流**:点击时先问 Lua「这个选择加多少好感度?最终跳哪个节点?」,
  再设置 `NextNodeId`。**一行 C# hook 就能接入,现有 XML 的 `%%` 静态分支仍可保留作为回退。**

### 2.4 进度数据是否可写(判断「动态修改游戏进度」的可行性)

- `LHGF_GameProgress`(`LHGFGameProgress.cs`):`CurrentNodeId`、`NextNodeId` 均为 `public string`,可写;
  `Forward()` 时 `CurrentNodeId = NextNodeId` 直接把下一个节点推进为当前节点 —— 改写 `NextNodeId` 即可改变走向。
- `LHGFGameMain` 是 `MonoBehaviour`,`public static LHGFGameMain instance`(`Awake()` 里赋值),全局单例。
- 层演出之间靠 `ILayer.Finish()` 门控,改 `NextNodeId` 不会破坏演出状态机。

### 2.5 持久化数据现状(好感度放哪)

- `GameGlobaDataManager`(`LHGFGameSaveData.cs`):全局数据,已有 `VisitedNode` 列表 +
  `SaveGlobalData()` / `LoadGlobalData()`(JSON,`SaveData/global_data.sav`),注释里明确预留了「周目数据(未实现)」。
- **好感度建议直接扩展 `GameGlobalData`**(加 `Dictionary<string,int> Affection`)或由 Lua 自管一份 JSON —— 两者都要加载/保存时机配合。

---

## 3. 三套方案的对比

### 方案 A:纯 Lua 直接操盘(Lua 全程控制)

```
Lua 持有 LHGFGameMain.instance → 每帧/每选项直接改 NextNodeId → 调用 Forward()
```

- 优点:改动最少(只要导出类型 + 一个常驻脚本)。
- 缺点:
  - 需要导出 `LHGFGameMain` 及 `gameProgress` 的字段类型(`LHGF_GameProgress`)给 Lua,导出面大;
  - Lua 直接调用 `Forward()`/`Withdraw()` 容易绕过层生命周期(如 `BeforeNextStart`、`LogContent`),造成演出状态错乱;
  - 回撤、读档、Skip、自动模式都要在 Lua 里重写一遍对进度结构的理解,维护成本高、易出 bug。

### 方案 B(推荐):C# 桥接 + Lua 决策函数

```
选项点击/节点前进/开局时 → C# 检查「Lua 是否注册了决策回调」→ 调用 Lua 函数
                                  ↑ 返回:好感度增减值、实际跳转节点、是否拦截
```

- 优点:
  - C# 只多几个「可选调用点」,现有流程不变、零行为回归;
  - Lua 只需写字面逻辑:`on_choose(id, nodeIds)`、`before_forward()` 等,不碰进度机内部;
  - 存档/回撤/跳过等仍由框架负责,Lua 只管「决策与数值」。
- 代价:需要 1~2 个 C# hook 点 + `[CSharpCallLua]` 委托导出(已有先例:配置文件里导出了 `DOGetter<float>` 等委托)。

### 方案 C:纯 XML 静态分支(不改动任何代码)

- 现有 `Choice` + `%%` 多分支只能做固定路径;好感度分流需运行时判断,纯 XML 无法满足。
- 结论:不满足需求,仅作为对照。

### 方案 D(用户提出,推荐):XLuaLayer 动态修改 scriptGraph

> 思路:新增一个实现 `ILayer` 的「XLuaLayer」,让它比其他层**优先执行逻辑判断**,
> 在层内**动态修改 scriptGraph**(增删边、替换节点命令、甚至动态新增节点),从而决定后续剧情走向。
> 下文逐条核验该想法在现有代码上的可行性。

#### D.1 结论:可行,且比方案 B 更贴合框架的分层哲学

核验了三个决定成败的机制后,结论是**完全可行**,并且比散落 hook 更内聚:

1. **图是可变图** ✅ — `ScriptGraph` 继承 `QuikGraph.AdjacencyGraph<string, stringPairEdge>`,
   `ScriptDatas` / `ScriptData.LayerCommands` / `LayerCommand.CommandConfig` 全是 public 可变容器;
   增删顶点/边/命令在 C# 层没有障碍。
2. **剧情走向正是由「边」决定** ✅ — `LHGF_GameProgress.Forward()` 里的
   `NextNodeId = Next(CurrentNodeId, data)` 取 `scriptGraph.OutEdges(CurrentNodeId)` 的**第一个 Target**。
   因此「改当前节点的出边」就是在图层面改变下一跳 —— 与"改 `NextNodeId` 字段"等价,但更符合你的直觉模型。
3. **ILayer 已有"优先判断"的天然时机** ✅ — `BeforeNextStart()` 在每节点前进前被所有层调用,
   且发生在 `Next()` 求下一跳 **之前**;不用每帧 `OnUpdate` 轮询,决策时机更精准(详见 D.3)。

#### D.2 三个必须知道的坑(核验自源码)

| # | 坑 | 说明与对策 |
| --- | --- | --- |
| 1 | **图会被整体重建** | 跨脚本跳转与读档都会 `data.scriptGraph = new(...)` 从 XML 重建,**动态修改全部丢失**。对策:**不要"一次性"改图**,而是让 XLuaLayer 在每次决策前基于当前图重新计算并应用(图重建后下次决策自动恢复,天然自愈)。 |
| 2 | **动态新增节点必须写入 `ScriptDatas`** | `ScriptGraph[nodeId]` 的 getter 在节点不存在时会按名字拆分去加载 XML;如果该节点不在任何 XML 中,`ScriptDatas[nodeId]` 会抛 `KeyNotFound`。对策:**候选分支节点全部静态预埋在 XML 里,只动态改边**(推荐),或将 `ScriptData` 同时 set 进字典(索引器有 setter)。 |
| 3 | **选项节点的跳转不经过「边」** | `ChoiceLayer.Next` 按 `CommandConfig["NextId"].Split("%%")` 生成按钮,点击后直接 `NextNodeId = Button.name`,**不查图**。要动态化选项本身,需改该 Choice 命令的 `NextId` 字符串(通过 `ScriptData.LayerCommands` 即可改),或用下方 D.4 的「隐藏判定节点」模式。 |

另外注意 `LHGFGameMain.Forward()` 中 `LayerShower(...)` 会先读一次图(`scriptGraph[nextNode].LayerCommands`)做 UI 预演,
随后 `gameProgress.Forward()` 再读一次图分发命令 —— **不要在两次读取之间改图**,否则预演与执行不一致(只改"边"则无此问题)。

#### D.3 推荐形态:决策时机用 `BeforeNextStart`,不用每帧 `OnUpdate`

- `BeforeNextStart()`:`ILayer` 现有接口,每节点前进前调用,早于 `Next()` 求下一跳 —— 是「逻辑判断」的正解;
- `OnUpdate()` 每帧轮询没必要,且字典遍历顺序不保证「优先」;若坚持在 `OnUpdate` 改图,
  需在 `SetLayers()` 中保证 XLuaLayer 在字典最前(字典按插入序,不严格保证)。
- 用户场景「好感度攒够了再开新路线」本质是事件驱动(节点进入时判断),`BeforeNextStart` 语义恰好匹配。

#### D.4 最推荐的剧情写法:「选项 → 隐藏判定节点 → 动态边分流」

```
<Choice NextId="JudgeNode-0%%JudgeNode-0%%JudgeNode-0" .../>  ← 选项(可静态,目标都是判定节点)
    ↓ 任选
<PlotNode Id="JudgeNode-0" NextId="Dummy"/>                    ← 隐藏判定节点(无 UI 命令)
    ↓ XLuaLayer.BeforeNextStart 按好感度把 JudgeNode-0 的出边 改为 → WDHigh-0 或 YGLow-0
<PlotNode Id="WDHigh-0" .../> / <PlotNode Id="YGLow-0" .../>   ← 候选分支(静态预埋)
```

- 优点:不改 ChoiceLayer 任何代码;候选分支全部预埋在 XML,避开 D.2 坑 #2;
  XLuaLayer 只需做「改出边」这一件事;图重建后每次决策重新计算,自动恢复。
- 这就是把 `scriptGraph` 当**运行时跳转表(jump table)** 用 —— 你的思路的直接落地。

#### D.5 Lua 与图的交互方式:建议 C# 薄桥 + Lua 决策

- XLua 目前**未导出** QuikGraph / `ScriptGraph` / `LHGF_GameData`(见 §2.1),Lua 直接操作图对象会很繁琐且导出面大;
- 推荐 `XLuaLayer` 作为 **C# 层**,把「改边 / 改 NextId / 改档好感度」做成易用方法(如
  `SetNodeNext(script, node, target)` / `GetAffection(name)` / `AddAffection(name, n)`),
  这些方法注册进 Lua 全局表(参考 `ImageAnimationPlayer` 注入 `self` 的模式,或 `new LuaEnv().Global.Set(...)`;
- Lua 脚本只写判断与数值(`if aff.wd >= 3 then return "WDHigh-0" end`),再由 C# 层执行改图 —— 职责清晰、导出面最小。

#### D.6 与方案 B 的关系

- 方案 B(hook 在选择按钮上)能动态化「选项按钮的跳转目标」,但散落、侵入现有点击回调;
- 方案 D 把判断收敛到一个层里,并与图的运行时语义(边=走向)对齐,扩展性更好(以后加"变量/条件/周目"都往这个层加);
- 两者可共存:选项仍用 B 的 hook 或 D 的判定节点,由你选择;D 建议作为主路线。

---

## 4. 建议的最小修改方案(分步落地,每步可独立验证)

### 步骤 0:只读验证(不动任何逻辑)

在某个已有动画 Lua 里临时打印,确认 `CS` 表与 `LuaEnv` 正常(当前项目已能跑动画,这一步通常跳过)。

### 步骤 1:导出所需类型(唯一「必须」的代码改动)

在 `Assets/XLua/Editor/LuaCallCsharpe.cs` 的 `GenCodeConfig.LuaCallCSharp` 中追加:

```csharp
typeof(LHGFGameMain),
typeof(LHGFGameProgress.LHGF_GameProgress),
// 好感度数据类(步骤 3 中新增,见下)
```

然后 Unity 菜单 `XLua → Generate Code` 重新生成 `Assets/XLua/Gen/`。
验证:Lua 中 `CS.LHGFGameMain.instance` 可取到单例(打印 `instance != nil`)。

### 步骤 2:新增常驻剧情逻辑 Lua 的加载入口

在现有 `Utils.xLuaEnv` 初始化处(或 `LHGFGameMain.Awake/Start`)追加一次加载:

```
加载 StreamingAssets/HGF/xLua/LHGFPlot/*.lua 到 luaEnv.Global(注册 on_choose / before_forward 等函数)
```

可复用 `LoadDOTweenLuaCommandsAsync` 的读取方式;`lua_files.txt` 清单机制也可复用。
> 若想「零新增 C# 加载代码」,也可以把剧情逻辑作为「每节点必执行的动画命令」注入(如黑板命令附带执行),
> 但这是 hack,不建议作为正式方案。

### 步骤 3:选项 hook —— 实现好感度分流(核心)

在 `LHGFChoiceLayerControler.SetChoiceButton` 的 onClick 中,`NextNodeId = Button.name` 之前插入:

```csharp
// 伪代码:若 Lua 注册了决策函数,则调用并获得「是否修改跳转」
if (LHGFPlotBridge.TryDecideChoice(Button.name, ChoiceContent, out string finalNode))
    NextNodeId = finalNode;
```

Lua 侧(示例,路线 B 风格):

```lua
-- LHGFPlot/main.lua
Affection = { WD = 0, YG = 0 }        -- 好感度表(可持久化)

function on_choose(btnId, content)
    if btnId:find("ask_wd") then Affection.WD = Affection.WD + 1 end
    if Affection.WD >= 3 then return "Ch2Part1-WDHigh-0" end -- 动态改写跳转
    return btnId                                          -- 默认仍跳按钮节点
end
```

### 步骤 4:好感度数据持久化(建议放在 C# 侧)

给 `GameGlobalData` 加字段(如 `public Dictionary<string,int> Affection = new()`),
复用现有 `SaveGlobalData()`/`LoadGlobalData()`;Lua 通过导出的桥接读写(如 `LHGFBridge.GetAffection(key)/AddAffection(key,n)`)。
> 这样好感度天然跟随全局存档、跨周目语义清晰;Lua 表自管的方案也可行,但需自行处理生命周期。

### 步骤 5:与既有系统对齐

- **回撤**(Backspace):回撤不应回滚好感度(它是「已发生事实」);若希望回撤同时撤销好感度,需在 `Withdraw()` 钩子中让 Lua 决策函数处理 —— 建议默认不回滚。
- **读档**:好感度在全局数据中,与剧情进度解耦,读档后仍保持一致(符合好感度「全局累计」定位)。
- **跳过/自动**:只影响层演出,不影响决策点。
- **折叠回退**:如果 Lua 未注册任何决策函数,`NextNodeId = Button.name` 原逻辑直接生效,保证老剧本零影响。

---

## 5. 风险与注意事项

| 风险 | 说明与对策 |
| --- | --- |
| 导出面膨胀 | 只导出「决策需要的窄接口」:`LHGFGameMain` + 一个自建的 `LHGFPlotBridge`(轻量静态类)优于导出 `LHGF_GameData` 内部结构。 |
| Lua 直接操纵进度导致状态机破坏 | 严格限制 Lua 只改 `NextNodeId` 与好感度,不暴露 `Forward/Withdraw/Skip`;跳转统一走 C# hook。 |
| IL2CPP / Android 反射 | 生成代码(wrap)在 IL2CPP 下安全;避免依赖反射路径访问未导出类型。 |
| 好感度何时持久化 | 每次 `on_choose` 后调用 `SaveGlobalData()`(该方法自带 1 秒节流),避免频繁写盘。 |
| 多表/复数角色好感度 | Lua 表或 C# Dictionary 均可;若需展示在 UI,建议 C# 侧持有并导出只读查询。 |
| 与现有 `%%` 静态分支共存 | 静态分支仍是「默认值」;动态 hook 只在其存在时覆盖,二者不冲突。 |

---

## 6. 涉及文件清单(实施时改动点定位)

| 文件 | 改动 | 类型 |
| --- | --- | --- |
| `Assets/XLua/Editor/LuaCallCsharpe.cs` | `GenCodeConfig.LuaCallCSharp` 增加 `LHGFGameMain` 等类型;`CSharpCallLua` 增加委托(如 `Func<string,string,string>`) | 必须 |
| `Assets/XLua/Gen/*` | 菜单重新生成(自动) | 生成物 |
| 新增 `LHGFPlotBridge.cs`(建议放 `Assets/HGF/Scripts/XLua/`) | 好感度读写 + `TryDecideChoice` 静态钩子,导出给 Lua | 新增 |
| `LHGFChoiceLayerControler.cs` | onClick 中调用 `LHGFPlotBridge.TryDecideChoice` | 1 处小改 |
| (可选)`LHGFGameMain.cs` | 入口处加载剧情逻辑 Lua | 1 处小改 |
| (可选)`LHGFGameSaveData.cs` | `GameGlobalData` 加好感度字段 | 小改 |
| 新增 `StreamingAssets/HGF/xLua/LHGFPlot/*.lua` | 常驻决策脚本(好感度规则) | 新增(纯数据/逻辑) |

---

## 7. 结论

- **可行性:高**。框架的结构(public 进度字段 + 单例 + 单点选项回调 + 现成全局数据持久化)
  恰好适合做「运行时动态改写剧情走向」,且不需要改动演出层与脚本图机制。
- **不要**让 Lua 直接接管 `Forward()` 等流程控制;而是通过 1~2 个 C# 钩子点,把「决策权」交给 Lua,
  「流程控制权」保留在框架 —— 这是对现有架构侵入最小、最可维护的方案。
- 若仅想快速验证,可先做步骤 1 + 步骤 3(导出 + 选项 hook),好感度先放在 Lua 表里,
  验证分流正确后再补持久化(步骤 4)。