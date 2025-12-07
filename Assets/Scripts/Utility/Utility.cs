
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Utility{
    public static Vector3 GetMousePosition2D(){
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public static T RaycastHitComponent<T>(string layerName = "Default") where T : Component
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utility.GetMousePosition2D(), Vector2.zero, 100f, LayerMask.GetMask(layerName));
        foreach (RaycastHit2D hit in hits){
            if (hit.collider == null) continue;
            if (hit.collider.TryGetComponent(out T component)){
                return component;
            }

            T parentComponent = hit.collider.GetComponentInParent<T>();
            if (parentComponent != null){
                return parentComponent;
            }
        }
        return null;
    }
    // 获取UI组件，不用
    public static T RaycastGetUI<T>(this PointerEventData eventData, string layerName = "Default") where T : Component
    {
        // 如果射线有很多个UI组件，则需要从下往上遍历
        if (eventData.pointerCurrentRaycast.gameObject == null) return null;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        foreach (RaycastResult raycastResult in raycastResults){
            T component = raycastResult.gameObject.GetComponentInParent<T>();
            if (component != null){
                return component;
            }
        }
        return null;
    }
}