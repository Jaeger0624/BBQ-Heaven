using DG.Tweening;
using UnityEngine;

public static class DOTweenExtentions{
    public static Tween UnScaledKill(this Tween tween, GameObject gameObject){
        tween.SetUpdate(true)
            .SetLink(gameObject);
        return tween;
    }
}   