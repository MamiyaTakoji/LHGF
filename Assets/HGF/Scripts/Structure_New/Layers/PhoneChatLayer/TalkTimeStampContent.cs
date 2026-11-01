using UnityEngine;

/// <summary>
/// 时间戳内容块 —— 就是"特殊的 TalkTmpContent"。
///
/// 除了把 Kind 换成 TimeStamp，其余全部继承自 TalkTmpContent：
///   · MeasureCore()  按可用宽度算出文字尺寸（气泡尺寸照旧由外壳算）
///   · Text 属性      写入 TMP 并 MarkDirty()
///   · ApplyData()    从 values["text"] 取名（和普通文本同一个 key）
///   · 量测缓存 / 脏标记 / LateUpdate 轮询兜底
/// 所以换掉 Kind 之后，外壳就能把它和普通文本区分开（可以用不同的预制体、不同的样式），
/// 而 LHGFTalkContentControler 一行都不用改。
///
/// 挂载位置和 TalkTmpContent 完全一样：**和 TextMeshProUGUI 同一个 GameObject**，
/// 该物体是 TalkContent 的子物体。同一个物体上不要再挂 TalkTmpContent（二选一）。
///
/// 典型做法是单独做一个"时间戳 bar"预制体（例如 TimeStampTalkBar）：
///   · TalkBar 下不放 Avata（SetTimeStamp 里也已经把 showAvatar 置 false）
///   · TalkContent 水平居中摆放；Image 背景去掉 sprite 或把 alpha 设为 0
///   · TalkContent 外壳的 KeepEdge 设成 Top —— 只钉住上边、不动 x，宽度变化时才能保持居中
///   · 本组件上的 Fill Bubble Width 建议**取消勾选**，让它收缩到文字宽度（一小条灰字）
///   · TMP 字号调小、颜色调灰
///
/// 数据来源：TalkBar 的 SetTimeStamp(time) → values["text"]
/// </summary>
[DisallowMultipleComponent]
public class TalkTimeStampContent : TalkTmpContent
{
    public override TalkContentKind Kind => TalkContentKind.TimeStamp;
}
