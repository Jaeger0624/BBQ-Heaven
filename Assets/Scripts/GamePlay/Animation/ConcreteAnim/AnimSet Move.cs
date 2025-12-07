
using System;
using DG.Tweening;
using UniRx;
using UnityEngine;

public class RelativeMoveAnimationTask : IAnimTask
{
    private Transform transform;
    private Transform targetTransform;
    private Vector3 relativeEnd;
    private Vector3 relativeStart;
    private float duration;
    private bool isFromRelativeStart = true;
    private bool useUnscaledTime = false;
    private Ease ease = Ease.Linear;
    public RelativeMoveAnimationTask(Transform transform, float duration, Transform targetTransform, Vector3 relativeEnd, Vector3 relativeStart, bool isFromRelativeStart = true)
    {
        this.transform = transform;
        this.relativeEnd = relativeEnd;
        this.relativeStart = relativeStart;
        this.duration = duration;
        this.targetTransform = targetTransform;
        this.isFromRelativeStart = isFromRelativeStart;
    }
    public RelativeMoveAnimationTask SetEase(Ease ease)
    {
        this.ease = ease;
        return this;
    }
    public RelativeMoveAnimationTask SetUseUnscaledTime(bool useUnscaledTime)
    {
        this.useUnscaledTime = useUnscaledTime;
        return this;
    }

    public IObservable<Unit> Play()
    {
        if (transform == null) {Debug.LogError("【RelativeMoveAnimationTask】transform 为空"); return Observable.ReturnUnit();}
        Vector3 from = isFromRelativeStart ? targetTransform.position + relativeStart : transform.position;
        Vector3 to = targetTransform.position + relativeEnd;
        var tween = transform.DOMove(to, duration).From(from).SetEase(ease).SetUpdate(useUnscaledTime);
        return new TweenAnimTask(tween).Play().AsUnitObservable();
    }
}

public class MoveAnimationTask : IAnimTask{
    private Transform transform;
    private Vector3 targetPosition;
    private Vector3 originPosition;
    private float duration;
    public MoveAnimationTask(Transform transform,float duration, Vector3 targetPosition, Vector3 originPosition)
    {
        this.transform = transform;
        this.targetPosition = targetPosition;
        this.duration = duration;
        this.originPosition = originPosition;
    }
    public IObservable<Unit> Play()
    {
        if (transform == null) {Debug.LogError("【MoveAnimationTask】transform 为空"); return Observable.ReturnUnit();}
        var tween = transform.DOMove(targetPosition, duration).From(originPosition);
        return new TweenAnimTask(tween).Play();
    }
}