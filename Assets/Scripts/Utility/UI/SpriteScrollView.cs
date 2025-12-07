using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 2D世界坐标Sprite滚动视图组件
/// 适用于不使用UI Canvas的2D Sprite滚动场景
/// </summary>
[ExecuteAlways]
public class SpriteScrollView : MonoBehaviour
{
    [Header("视图设置")]
    [Tooltip("视口区域（内容显示的区域），如果为空则使用当前Transform的Bounds")]
    public Transform viewport;
    
    [Tooltip("内容容器（包含所有可滚动子物体的父对象），如果为空则使用当前Transform")]
    public Transform content;
    
    [Tooltip("视口尺寸（当viewport为空时使用此值）")]
    public Vector2 viewportSize = new Vector2(5f, 5f);
    
    [Header("滚动设置")]
    [Tooltip("是否启用水平滚动")]
    public bool horizontal = true;
    
    [Tooltip("是否启用垂直滚动")]
    public bool vertical = true;
    
    [Tooltip("滚动阻尼系数（值越大滚动越快）")]
    [Range(0.1f, 10f)]
    public float scrollDamping = 5f;
    
    [Tooltip("惯性滚动阻尼（值越大停止越快，0表示无惯性）")]
    [Range(0f, 20f)]
    public float inertiaDamping = 10f;
    
    [Tooltip("是否在边界处弹性回弹")]
    public bool elasticBounds = true;
    
    [Tooltip("弹性回弹强度")]
    [Range(0f, 1f)]
    public float elasticStrength = 0.3f;
    
    [Header("交互设置")]
    [Tooltip("鼠标拖拽灵敏度")]
    public float dragSensitivity = 1f;
    
    [Tooltip("滚轮缩放（Unity的滚轮输入倍数）")]
    public float scrollWheelMultiplier = 0.5f;
    
    [Tooltip("触摸拖拽灵敏度（移动设备）")]
    public float touchSensitivity = 1f;
    
    [Header("裁剪设置")]
    [Tooltip("是否裁剪视口外的子物体（通过设置GameObject.activeSelf）")]
    public bool clipChildren = false;
    
    [Tooltip("裁剪时的额外边距（可以显示部分超出边界的物体）")]
    public float clipMargin = 0.5f;
    
    [Header("调试")]
    [Tooltip("显示调试信息")]
    public bool showDebug = false;

    // 私有变量
    private Camera targetCamera;
    private Vector3 contentStartPosition;
    private Vector3 lastMouseWorldPos;
    private bool isDragging = false;
    private Vector2 velocity = Vector2.zero;
    private Vector2 contentBoundsMin;
    private Vector2 contentBoundsMax;
    private Vector2 viewportBoundsMin;
    private Vector2 viewportBoundsMax;
    private bool hasInitialized = false;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        if (!hasInitialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        // 获取摄像机
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<Camera>();
            }
        }

        // 初始化视口和内容
        if (viewport == null)
        {
            viewport = transform;
        }

        if (content == null)
        {
            content = transform;
        }

        // 记录内容初始位置
        contentStartPosition = content.localPosition;
        
        hasInitialized = true;
        
        if (showDebug)
        {
            Debug.Log($"SpriteScrollView initialized - Viewport: {viewport.name}, Content: {content.name}");
        }
    }

    private void Update()
    {
        if (!hasInitialized)
        {
            Initialize();
        }

        UpdateBounds();
        HandleInput();
        UpdateScroll();
        ClampContent();
        
        if (clipChildren)
        {
            UpdateChildVisibility();
        }
    }

    private void UpdateBounds()
    {
        // 计算内容边界（相对于内容容器的本地空间）
        Bounds contentBounds = CalculateLocalBounds(content);
        contentBoundsMin = new Vector2(contentBounds.min.x, contentBounds.min.y);
        contentBoundsMax = new Vector2(contentBounds.max.x, contentBounds.max.y);

        // 计算视口边界（世界空间）
        Bounds viewportBoundsWorld = viewport != null ? CalculateWorldBounds(viewport) : 
            new Bounds(viewport != null ? viewport.position : transform.position, viewportSize);
        
        // 将视口边界转换为内容容器的本地空间
        Vector3 viewportMinWorld = viewportBoundsWorld.min;
        Vector3 viewportMaxWorld = viewportBoundsWorld.max;
        Vector3 viewportMinLocal = content.InverseTransformPoint(viewportMinWorld);
        Vector3 viewportMaxLocal = content.InverseTransformPoint(viewportMaxWorld);
        
        viewportBoundsMin = new Vector2(viewportMinLocal.x, viewportMinLocal.y);
        viewportBoundsMax = new Vector2(viewportMaxLocal.x, viewportMaxLocal.y);
    }

    private Bounds CalculateWorldBounds(Transform target)
    {
        Bounds bounds = new Bounds();
        bool hasBounds = false;

        foreach (Transform child in target)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null && sr.gameObject.activeSelf)
            {
                Bounds childBounds = sr.bounds;
                if (!hasBounds)
                {
                    bounds = childBounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(childBounds);
                }
            }
        }

        // 如果没有找到任何子物体，使用Transform的位置和默认大小
        if (!hasBounds)
        {
            bounds = new Bounds(target.position, viewportSize);
        }

        return bounds;
    }

    private Bounds CalculateLocalBounds(Transform target)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;

        foreach (Transform child in target)
        {
            if (!child.gameObject.activeSelf) continue;
            
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                // 获取子物体在世界空间中的边界
                Bounds childBoundsWorld = sr.bounds;
                
                // 转换为内容容器的本地空间
                Vector3 minLocal = target.InverseTransformPoint(childBoundsWorld.min);
                Vector3 maxLocal = target.InverseTransformPoint(childBoundsWorld.max);
                Vector3 centerLocal = target.InverseTransformPoint(childBoundsWorld.center);
                Vector3 sizeLocal = maxLocal - minLocal;
                
                Bounds childBoundsLocal = new Bounds(centerLocal, sizeLocal);
                
                if (!hasBounds)
                {
                    bounds = childBoundsLocal;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(childBoundsLocal);
                }
            }
        }

        // 如果没有找到任何子物体，返回零边界
        if (!hasBounds)
        {
            bounds = new Bounds(Vector3.zero, viewportSize);
        }

        return bounds;
    }

    private void HandleInput()
    {
        if (targetCamera == null) return;

        Vector3 currentMouseWorldPos = GetMouseWorldPosition();

        // 鼠标/触摸按下
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointInViewport(currentMouseWorldPos))
            {
                isDragging = true;
                lastMouseWorldPos = currentMouseWorldPos;
                velocity = Vector2.zero;
            }
        }
        // 鼠标/触摸拖拽
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 deltaPos = currentMouseWorldPos - lastMouseWorldPos;
            Vector2 delta = new Vector2(
                horizontal ? deltaPos.x * dragSensitivity : 0f,
                vertical ? deltaPos.y * dragSensitivity : 0f
            );

            // 更新内容位置
            Vector3 contentPos = content.localPosition;
            content.localPosition = new Vector3(
                contentPos.x + delta.x,
                contentPos.y + delta.y,
                contentPos.z
            );

            // 计算速度（用于惯性滚动）
            velocity = delta / Time.deltaTime;

            lastMouseWorldPos = currentMouseWorldPos;
        }
        // 鼠标/触摸释放
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // 滚轮输入
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector2 scrollDelta = new Vector2(
                horizontal ? scroll * scrollWheelMultiplier : 0f,
                vertical ? -scroll * scrollWheelMultiplier : 0f
            );
            
            Vector3 contentPos = content.localPosition;
            content.localPosition = new Vector3(
                contentPos.x + scrollDelta.x,
                contentPos.y + scrollDelta.y,
                contentPos.z
            );
        }

        // 触摸输入处理（多点触控）
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 touchWorldPos = GetTouchWorldPosition(touch.position);
                if (IsPointInViewport(touchWorldPos))
                {
                    isDragging = true;
                    lastMouseWorldPos = touchWorldPos;
                    velocity = Vector2.zero;
                }
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector3 touchWorldPos = GetTouchWorldPosition(touch.position);
                Vector3 deltaPos = touchWorldPos - lastMouseWorldPos;
                Vector2 delta = new Vector2(
                    horizontal ? deltaPos.x * touchSensitivity : 0f,
                    vertical ? deltaPos.y * touchSensitivity : 0f
                );

                Vector3 contentPos = content.localPosition;
                content.localPosition = new Vector3(
                    contentPos.x + delta.x,
                    contentPos.y + delta.y,
                    contentPos.z
                );

                velocity = delta / Time.deltaTime;
                lastMouseWorldPos = touchWorldPos;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }

    private void UpdateScroll()
    {
        // 惯性滚动
        if (!isDragging && velocity.magnitude > 0.01f)
        {
            Vector3 contentPos = content.localPosition;
            Vector2 delta = velocity * Time.deltaTime;
            
            content.localPosition = new Vector3(
                contentPos.x + delta.x,
                contentPos.y + delta.y,
                contentPos.z
            );

            // 应用阻尼
            velocity = Vector2.Lerp(velocity, Vector2.zero, inertiaDamping * Time.deltaTime);
        }
    }

    private void ClampContent()
    {
        Vector3 contentPos = content.localPosition;
        Vector2 contentSize = contentBoundsMax - contentBoundsMin;
        Vector2 viewportSize = viewportBoundsMax - viewportBoundsMin;

        // 计算内容相对于视口的大小差异
        Vector2 contentDelta = contentSize - viewportSize;

        float newX = contentPos.x;
        float newY = contentPos.y;

        if (horizontal)
        {
            if (contentDelta.x > 0)
            {
                // 内容大于视口，限制在边界内
                float minX = -contentDelta.x * 0.5f;
                float maxX = contentDelta.x * 0.5f;
                
                if (elasticBounds && (newX < minX || newX > maxX))
                {
                    // 弹性回弹
                    float targetX = Mathf.Clamp(newX, minX, maxX);
                    newX = Mathf.Lerp(newX, targetX, elasticStrength);
                    velocity.x *= 0.9f; // 降低速度
                }
                else
                {
                    newX = Mathf.Clamp(newX, minX, maxX);
                    if (newX == minX || newX == maxX)
                    {
                        velocity.x = 0f;
                    }
                }
            }
            else
            {
                // 内容小于视口，居中对齐
                newX = 0f;
                velocity.x = 0f;
            }
        }

        if (vertical)
        {
            if (contentDelta.y > 0)
            {
                // 内容大于视口，限制在边界内
                float minY = -contentDelta.y * 0.5f;
                float maxY = contentDelta.y * 0.5f;
                
                if (elasticBounds && (newY < minY || newY > maxY))
                {
                    // 弹性回弹
                    float targetY = Mathf.Clamp(newY, minY, maxY);
                    newY = Mathf.Lerp(newY, targetY, elasticStrength);
                    velocity.y *= 0.9f;
                }
                else
                {
                    newY = Mathf.Clamp(newY, minY, maxY);
                    if (newY == minY || newY == maxY)
                    {
                        velocity.y = 0f;
                    }
                }
            }
            else
            {
                // 内容小于视口，居中对齐
                newY = 0f;
                velocity.y = 0f;
            }
        }

        content.localPosition = new Vector3(newX, newY, contentPos.z);
    }

    private void UpdateChildVisibility()
    {
        if (content == null) return;

        // 计算裁剪边界（加上边距）
        Bounds viewportBounds = CalculateWorldBounds(viewport);
        viewportBounds.Expand(clipMargin * 2f);

        foreach (Transform child in content)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                bool shouldBeVisible = viewportBounds.Intersects(sr.bounds);
                child.gameObject.SetActive(shouldBeVisible);
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (targetCamera == null) return Vector3.zero;
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = targetCamera.nearClipPlane;
        
        // 对于正交摄像机，需要计算Z距离
        if (targetCamera.orthographic)
        {
            mousePos.z = Mathf.Abs(targetCamera.transform.position.z - viewport.position.z);
        }
        
        return targetCamera.ScreenToWorldPoint(mousePos);
    }

    private Vector3 GetTouchWorldPosition(Vector2 screenPosition)
    {
        if (targetCamera == null) return Vector3.zero;
        
        Vector3 touchPos = new Vector3(screenPosition.x, screenPosition.y, targetCamera.nearClipPlane);
        
        if (targetCamera.orthographic)
        {
            touchPos.z = Mathf.Abs(targetCamera.transform.position.z - viewport.position.z);
        }
        
        return targetCamera.ScreenToWorldPoint(touchPos);
    }

    private bool IsPointInViewport(Vector3 worldPoint)
    {
        if (viewport == null) return true;

        Bounds viewportBounds = CalculateWorldBounds(viewport);
        return viewportBounds.Contains(worldPoint);
    }

    /// <summary>
    /// 重置滚动位置到初始状态
    /// </summary>
    public void ResetPosition()
    {
        if (content != null)
        {
            content.localPosition = contentStartPosition;
            velocity = Vector2.zero;
            isDragging = false;
        }
    }

    /// <summary>
    /// 滚动到指定位置（平滑移动）
    /// </summary>
    public void ScrollTo(Vector2 targetPosition, float duration = 0.5f)
    {
        StartCoroutine(ScrollToCoroutine(targetPosition, duration));
    }

    private System.Collections.IEnumerator ScrollToCoroutine(Vector2 targetPosition, float duration)
    {
        Vector3 startPos = content.localPosition;
        Vector3 targetPos = new Vector3(targetPosition.x, targetPosition.y, startPos.z);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t); // 平滑插值
            
            content.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        content.localPosition = targetPos;
        velocity = Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        if (!showDebug) return;

        // 绘制视口边界
        if (viewport != null)
        {
            Bounds viewportBounds = CalculateWorldBounds(viewport);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(viewportBounds.center, viewportBounds.size);
        }

        // 绘制内容边界（世界空间）
        if (content != null)
        {
            Bounds contentBounds = CalculateWorldBounds(content);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(contentBounds.center, contentBounds.size);
        }
    }
}

