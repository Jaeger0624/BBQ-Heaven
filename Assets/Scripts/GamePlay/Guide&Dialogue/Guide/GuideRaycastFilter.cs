using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class GuideRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    public RectTransform TargetHollow; // 拖入那个高亮框

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        // 如果没有目标，全屏拦截 (返回 true)
        if (!TargetHollow.gameObject.activeInHierarchy) return true;

        // 如果点击点在 TargetHollow 矩形内，返回 false (不拦截，穿透下去)
        // 从而点到下面的游戏按钮
        bool isInside = RectTransformUtility.RectangleContainsScreenPoint(TargetHollow, sp, eventCamera);
        return !isInside;
    }
}