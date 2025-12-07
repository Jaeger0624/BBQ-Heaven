using System;
using UniRx;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 弹性旋转动画
/// </summary>
public class ElasticRotationAnimTask : IAnimTask
{
    private Transform transform;
    private float duration;
    private float rotation;
    public ElasticRotationAnimTask(Transform transform, float duration, float rotation)
    {
        this.transform = transform;
        this.duration = duration;
        this.rotation = rotation;
    }
    public IObservable<Unit> Play()
    {
        // 从 rotation 度回弹到 0 度，使用弹性缓动
        var tween = transform.DOLocalRotate(new Vector3(0, 0, 0), duration)
            .From(new Vector3(0, 0, rotation))
            .SetEase(Ease.OutElastic, 1.70158f, 0.3f);
        return new TweenAnimTask(tween).Play();
    }
}


public class ElasticScaleAnimTask : IAnimTask
{
    private Transform transform;
    private float duration;
    private float scaleRatio;
    public ElasticScaleAnimTask(Transform transform, float duration, float scaleRatio)
    {
        this.transform = transform;
        this.duration = duration;
        this.scaleRatio = scaleRatio;
    }
     public IObservable<Unit> Play()
     {
         var originScale = transform.localScale.x;
         var from = originScale * scaleRatio;
         var target = originScale;
         var tween = transform.DOScale(target, duration)
             .From(from)
             .SetEase(Ease.OutElastic, 1.70158f, 0.3f);
         return new TweenAnimTask(tween).Play();
     }    
}


public class NextFrameAnimTask : IAnimTask
{
    public IObservable<Unit> Play()
    {
        return Observable.TimerFrame(1).AsUnitObservable();
    }
}