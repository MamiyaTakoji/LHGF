using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public static class GenCodeConfig
{
    // CustomGenConfig.cs
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
{
    // DOTween核心类
    typeof(DG.Tweening.DOTween),
    typeof(DG.Tweening.Tween),
    typeof(DG.Tweening.Sequence),
    typeof(DG.Tweening.TweenParams),
    typeof(DG.Tweening.Ease),
    
    // 常用扩展类
    typeof(DG.Tweening.ShortcutExtensions),
    
    // UI相关扩展
    typeof(DG.Tweening.DOTweenModuleUI),
    typeof(DG.Tweening.TweenSettingsExtensions),
    typeof(SequenceExtensions),
    
    // 其他你可能需要使用的类
    typeof(DG.Tweening.PathType),
    typeof(DG.Tweening.PathMode),
    typeof(DG.Tweening.RotateMode),
    typeof(DG.Tweening.Core.DOGetter<UnityEngine.Vector2>),
    typeof(DG.Tweening.Core.DOSetter<UnityEngine.Vector2>),

        // 可选：如果你还用其他类型，也加上
    typeof(DG.Tweening.Core.DOGetter<float>),
    typeof(DG.Tweening.Core.DOSetter<float>),
    typeof(DG.Tweening.Core.DOGetter<UnityEngine.Vector3>),
    typeof(DG.Tweening.Core.DOSetter<UnityEngine.Vector3>),
    typeof(DG.Tweening.TweenExtensions),
    typeof(Utils),
    //typeof(DOTweenAnimationManager),
    typeof(TypeTransform),
    typeof(LHGFData.ImageLayer.ImageAnimationPlayer)
};
    [CSharpCallLua]
    public static List<System.Type> CSharpCallLua = new List<System.Type>()
    {
        typeof(DG.Tweening.Core.DOGetter<Vector2>),
        typeof(DG.Tweening.Core.DOSetter<Vector2>),
        typeof(DG.Tweening.Core.DOGetter<float>),
        typeof(DG.Tweening.Core.DOSetter<float>),
    };
}
