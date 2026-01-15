using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public partial class AnimCombine_放置烤串食材{
    public static IAnimTask Anim_放置单个食材(IEntityView view, int index, Transform waitingPlace, BBQView bbqView){
        Transform slotTransform = bbqView.GetSlotTransform(index);
        IAnimTask animTask_SetParent = new ActionAnimTask(() => {
            if(view == null) return;
            view.GO().transform.SetParent(slotTransform, true);
        });
        Transform transform = view.GO().transform;
        Vector3 waitingPlacePosition = waitingPlace.position + new Vector3(0, 0, -4f);
        // 1. 先放置在等待位置
        IAnimTask animTask_MoveToWaitingPlace = new RelativeMoveAnimationTask(
            view.GO().transform,
            0.5f,
            waitingPlace,
            new Vector3(0, 0, -4f),
            new Vector3(0, 0, -1f),
            false).SetEase(Ease.OutSine);

        // 2. 等待0.5秒
        IAnimTask animTask_Delay = new DelayAnimTask(0.2f);
        
        // 3. 再放置到目标插槽
        IAnimTask animTask_MoveToSlot = new RelativeMoveAnimationTask(
            view.GO().transform,
            0.5f,
            slotTransform,
            new Vector3(0, 0, -1f),
            new Vector3(0, 0, -1f),
            false).SetEase(Ease.OutBack);



        IAnimTask animTask = new SequenceAnimTask(new List<IAnimTask>
        {
            animTask_SetParent,
            animTask_MoveToWaitingPlace,
            animTask_Delay,
            animTask_MoveToSlot
        });
        
        return animTask;
    }
}