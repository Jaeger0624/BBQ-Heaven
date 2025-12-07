using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TargetingArrowManager : MonoBehaviour
{
    public static TargetingArrowManager Instance => _instance;
    private static TargetingArrowManager _instance;

    [Header("Assets")]
    public Sprite bodySprite; 
    public Sprite headSprite; 

    [Header("Settings")]
    public int nodeCount = 15;
    public float controlHeight = 600f;
    public float scaleFactor = 0.5f; 

    [Header("Container")]
    public Transform arrowContainer; 

    private List<RectTransform> bodyNodes = new List<RectTransform>();
    private RectTransform headNode;
    [ShowInInspector]
    private bool isActive = true;

    private void Awake()
    {
        if (_instance != null) { Destroy(_instance.gameObject); }
        _instance = this;
        
        InitializePool();
        Hide(); 
    }

    private void InitializePool()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            GameObject go = new GameObject("ArrowBody_" + i);
            go.transform.SetParent(arrowContainer, false);
            Image img = go.AddComponent<Image>();
            img.sprite = bodySprite;
            img.raycastTarget = false; 
            bodyNodes.Add(go.GetComponent<RectTransform>());
        }

        GameObject head = new GameObject("ArrowHead");
        head.transform.SetParent(arrowContainer, false);
        Image headImg = head.AddComponent<Image>();
        headImg.sprite = headSprite;
        headImg.raycastTarget = false;
        headNode = head.GetComponent<RectTransform>();
        headNode.pivot = new Vector2(0.5f, 0.5f); 
    }

    public void Show(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        if (!isActive)
        {
            arrowContainer.gameObject.SetActive(true);
            isActive = true;
        }
        UpdateCurve(startWorldPos, endWorldPos);
    }

    public void Hide()
    {
        if (isActive)
        {
            arrowContainer.gameObject.SetActive(false);
            isActive = false;
        }
    }

    private void UpdateCurve(Vector3 p0, Vector3 p2)
    {
        // 关键点：将世界坐标转换为 ArrowContainer 的局部坐标
        // 这样无论 Canvas 是 Overlay 还是 Camera 模式都能正常工作
        
        Vector2 startPos, endPos;
        
        // 注意：如果 Canvas 是 Screen Space - Overlay，cam 参数传 null
        // 如果是 Screen Space - Camera，传 Camera.main
        Camera cam = null;
        if (GetComponentInParent<Canvas>().renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = Camera.main;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)arrowContainer, 
            RectTransformUtility.WorldToScreenPoint(cam, p0), 
            cam, 
            out startPos
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)arrowContainer, 
            RectTransformUtility.WorldToScreenPoint(cam, p2), 
            cam, 
            out endPos
        );

        // 计算控制点 P1 (中点上抬)
        Vector2 midPoint = (startPos + endPos) / 2f;
        Vector2 controlPoint = new Vector2(midPoint.x, midPoint.y + controlHeight);

        // 分布节点
        for (int i = 0; i < nodeCount; i++)
        {
            float t = i / (float)nodeCount; 
            Vector2 pos = CalculateBezierPoint(t, startPos, controlPoint, endPos);
            bodyNodes[i].anchoredPosition = pos;

            Vector2 nextPos = (i < nodeCount - 1) 
                ? CalculateBezierPoint((i + 1) / (float)nodeCount, startPos, controlPoint, endPos) 
                : endPos;
            
            Vector2 dir = nextPos - pos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bodyNodes[i].rotation = Quaternion.Euler(0, 0, angle - 90); 

            float scale = Mathf.Lerp(scaleFactor, 1f, t);
            bodyNodes[i].localScale = Vector3.one * scale;
        }

        // 设置箭头
        headNode.anchoredPosition = endPos;
        Vector2 headDir = endPos - bodyNodes[nodeCount - 1].anchoredPosition;
        float headAngle = Mathf.Atan2(headDir.y, headDir.x) * Mathf.Rad2Deg;
        headNode.rotation = Quaternion.Euler(0, 0, headAngle - 90); 
    }

    private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return (uu * p0) + (2 * u * t * p1) + (tt * p2);
    }
}