using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Reflection; // 引用 MMTools

public class UIFloatingTextFixer : MonoBehaviour
{
    private void Start()
    {
        // 1. 获取对象池组件
        // MMFloatingTextSpawner 通常会自动添加一个 MMSimpleObjectPooler
        var pooler = GetComponent<MMFloatingTextSpawner>();


        // 2. 获取场景中的池对象
        var poolObjects = FindObjectsByType<MMObjectPool>(FindObjectsSortMode.None);

        

        foreach (var poolObject in poolObjects)
        {
            poolObject.gameObject.AddComponent<RectTransform>();
            poolObject.transform.SetParent(this.transform);

            poolObject.transform.localScale = Vector3.one;
            poolObject.transform.localPosition = Vector3.zero;
        }
    }
    
    // 额外的保险：每当对象生成时，再次按住它
    // 如果上面的 Start 不起作用，可以用这个 Update 暴力修正（通常不需要）
    /*
    private void Update()
    {
        foreach (Transform child in transform)
        {
            // 确保所有子物体缩放正确（防止被 Canvas 缩放搞乱）
            if (child.localScale != Vector3.one)
            {
                child.localScale = Vector3.one;
            }
        }
    }
    */
}