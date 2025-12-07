using UnityEngine;

/// <summary>
/// AI生成的，臭
/// </summary>
public class ParallelCanvas : MonoBehaviour
{
    [Header("视差设置")]
    [Tooltip("视差移动的强度倍数")]
    public float parallaxStrength = 1f;
    
    [Tooltip("最大视差范围（0-1，限制鼠标偏移的最大值，防止穿帮）")]
    [Range(0f, 1f)]
    public float maxParallaxRange = 0.5f;
    
    [Tooltip("平滑时间（达到目标位置所需的大致时间，值越大越平滑）")]
    [Range(0.01f, 2f)]
    public float smoothTime = 0.2f;
    
    [Tooltip("是否仅在鼠标移动时更新")]
    public bool updateOnlyOnMouseMove = false;
    
    private Transform[] childTransforms;
    private Vector3[] childStartPositions;
    private Vector3[] childVelocities; // 用于 SmoothDamp 的速度缓存
    private int middleIndex;
    private Vector2 lastMousePosition;
    
    void Start()
    {
        // 只在播放模式下初始化
        if (!Application.isPlaying)
            return;
            
        // 获取所有子对象
        int childCount = transform.childCount;
        if (childCount == 0)
        {
            Debug.LogWarning("ParallelCanvas: 没有找到子对象！");
            return;
        }
        
        childTransforms = new Transform[childCount];
        childStartPositions = new Vector3[childCount];
        childVelocities = new Vector3[childCount];
        
        // 存储子对象和初始位置
        for (int i = 0; i < childCount; i++)
        {
            childTransforms[i] = transform.GetChild(i);
            childStartPositions[i] = childTransforms[i].localPosition;
            childVelocities[i] = Vector3.zero; // 初始化速度
        }
        
        // 计算中间索引（向下取整）
        // 注意：对于奇数个，中间索引会有移动；对于偶数个，中间索引为 childCount/2 - 1
        middleIndex = (childCount - 1) / 2;
        
        // 初始化鼠标位置（仅在播放模式下）
        if (Application.isPlaying)
        {
            lastMousePosition = GetMouseScreenPosition();
        }
    }

    void Update()
    {
        // 只在播放模式下更新
        if (!Application.isPlaying)
            return;
            
        if (childTransforms == null || childTransforms.Length == 0)
            return;
        
        Vector2 currentMousePosition = GetMouseScreenPosition();
        
        // 如果设置了仅在鼠标移动时更新，检查鼠标是否移动
        if (updateOnlyOnMouseMove && currentMousePosition == lastMousePosition)
            return;
        
        lastMousePosition = currentMousePosition;
        
        // 获取鼠标相对于屏幕中心的偏移（归一化到 -1 到 1）
        Vector2 mouseOffset = GetNormalizedMouseOffset(currentMousePosition);
        
        // 更新每个子对象的位置
        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i] == null)
                continue;
            
            // 计算相对于中间索引的偏移
            float indexOffset = i - middleIndex;
            
            // 计算移动幅度：索引越大，同向移动幅度越大；索引越小，反向移动幅度越大
            // 如果正好是中间索引（indexOffset == 0），给它一个小的基准移动值
            float moveMultiplier;
            if (Mathf.Approximately(indexOffset, 0f))
            {
                // 中间索引给一个小的基准值（0.5），确保它也会移动
                moveMultiplier = 0.5f * parallaxStrength;
            }
            else
            {
                moveMultiplier = indexOffset * parallaxStrength;
            }
            
            // 计算目标位置
            Vector3 targetPosition = childStartPositions[i] + new Vector3(
                mouseOffset.x * moveMultiplier,
                mouseOffset.y * moveMultiplier,
                0f
            );
            
            // 使用 SmoothDamp 实现更自然的平滑移动（指数阻尼平滑）
            Vector3 currentPosition = childTransforms[i].localPosition;
            Vector3 newPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref childVelocities[i], smoothTime);
            childTransforms[i].localPosition = newPosition;
        }
    }
    
    /// <summary>
    /// 获取鼠标屏幕位置
    /// </summary>
    Vector2 GetMouseScreenPosition()
    {
        // 确保只在播放模式下访问输入系统
        if (!Application.isPlaying)
            return Vector2.zero;
            
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            return UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        }
        return Vector2.zero;
        #else
        return Input.mousePosition;
        #endif
    }
    
    /// <summary>
    /// 获取归一化的鼠标偏移（相对于屏幕中心，范围限制在 maxParallaxRange 内）
    /// </summary>
    Vector2 GetNormalizedMouseOffset(Vector2 mousePosition)
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 offset = mousePosition - screenCenter;
        
        // 归一化到 -1 到 1 的范围
        float normalizedX = offset.x / (Screen.width / 2f);
        float normalizedY = offset.y / (Screen.height / 2f);
        
        // 限制在最大视差范围内（防止穿帮）
        float magnitude = Mathf.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
        if (magnitude > maxParallaxRange)
        {
            float scale = maxParallaxRange / magnitude;
            normalizedX *= scale;
            normalizedY *= scale;
        }
        
        return new Vector2(normalizedX, normalizedY);
    }
    
    /// <summary>
    /// 重置所有子对象到初始位置
    /// </summary>
    public void ResetPositions()
    {
        // 只在播放模式下重置
        if (!Application.isPlaying)
            return;
            
        if (childTransforms == null || childStartPositions == null)
            return;
        
        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i] != null)
            {
                childTransforms[i].localPosition = childStartPositions[i];
                if (childVelocities != null && i < childVelocities.Length)
                {
                    childVelocities[i] = Vector3.zero; // 重置速度
                }
            }

        }
    }
}
