using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
public static class SequenceExtensions
{
    public static Sequence SetSeqLoops(Sequence sequence, int loops)
    {
        sequence.SetLoops(loops);
        return sequence;
    }
    public static Sequence Append(Sequence s, Tween t)
    {
        s.Append(t);
        return s;
    }
    public static Sequence Join(Sequence s, Tween t)
    {
        s.Join(t);
        return s;
    }
    public static Sequence DOFade(Graphic graphic, float endValue, float duration) 
    {
        Sequence sequence = DOTween.Sequence();
        var animation = graphic.DOFade(endValue, duration);
        sequence.Append(animation);
        return sequence;
    }
}
public static class TypeTransform
{
    public static int string2int(string str)
    {
        return int.Parse(str);
    }
    public static float string2float(string str)
    {
        return float.Parse(str);
    }
}