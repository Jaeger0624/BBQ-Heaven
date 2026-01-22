using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class SmartCatPaw : MonoBehaviour
{
    [Header("设置")]
    [SerializeField] private float _moveDuration = 0.4f;
    [SerializeField] private float _pressDuration = 0.2f;
    
    [Header("微调")]
    [Tooltip("指尖距离目标点的距离（避免完全遮挡目标）")]
    [SerializeField] private float _tipGap = 20f; 
    
    [Tooltip("猫爪图片的实际长度（像素），建议比图片略短一点点以留出安全余量")]
    [SerializeField] private float _pawLength = 0f; 

    [Tooltip("安全边距：让手腕必须缩进屏幕边缘多少像素才算安全")]
    [SerializeField] private float _safeMargin = 50f;
    [LabelText("动画弹跳幅度")]
    [SerializeField] private float _retractBounce = 0.8f;

    [Header("必须引用")]
    [Tooltip("渲染UI的相机，Screen Space-Camera模式必填！")]
    public Camera UICamera; 

    private RectTransform _rect;
    private RectTransform _parentRect; 
    private Sequence _seq;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _parentRect = transform.parent as RectTransform;

        if (UICamera == null) 
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null) UICamera = canvas.worldCamera;
            if (UICamera == null) UICamera = Camera.main; 
        }

        if (_pawLength <= 0) _pawLength = _rect.rect.height;
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // 初始隐藏
        VisualHide(); 
    }

    private void Update() 
    {
        // 测试代码
        if (Input.GetMouseButtonDown(0)) PointAt(Input.mousePosition);
        if (Input.GetMouseButtonDown(1)) Hide();
    }

    public void PointAt(Vector2 targetScreenPos)
    {
        VisualShow();
        if (_seq != null) _seq.Kill();

        // 1. 坐标转换
        Vector2 localTargetPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentRect, 
            targetScreenPos, 
            UICamera, 
            out localTargetPos
        );

        // =========================================================
        // 策略选择算法
        // =========================================================

        // 策略A：默认使用径向（从中心指向目标）
        Vector2 vectorOut = localTargetPos.normalized;
        if (vectorOut == Vector2.zero) vectorOut = Vector2.down; // 兜底

        // 检查策略A是否会导致穿帮
        if (!IsLengthEnough(localTargetPos, vectorOut))
        {
            // 策略B：如果径向不够长，则寻找最近的边缘垂直伸入
            // 因为通常屏幕宽 > 高，所以往往会变成从上下伸入，这样路径最短
            vectorOut = GetShortestEdgeDirection(localTargetPos);
        }

        // =========================================================
        // 动画计算
        // =========================================================

        // 2. 计算旋转 (Sprite UP 指向 ↙, 位置在 ↗)
        float angle = Mathf.Atan2(-vectorOut.y, -vectorOut.x) * Mathf.Rad2Deg - 90f;
        _rect.localRotation = Quaternion.Euler(0, 0, angle);

        // 3. 计算关键位置
        // 指尖位置
        Vector2 finalTipPos = localTargetPos; 
        // 修正：我们希望指尖停在 gap 处，而不是直接重合
        // 由于 vectorOut 是指向屏幕外的，所以指尖应该是在 localTargetPos 往屏幕内缩一点？
        // 不，PointAt 意图是指向 localTargetPos。
        // TipGap 是指尖和目标的距离。
        // 指尖实际位置 = 目标位置 + (指向屏幕外方向 * Gap)
        Vector2 actualTipPos = localTargetPos + (vectorOut * _tipGap);

        // 手腕最终位置 (伸进来后的位置) = 指尖实际位置 + (指向屏幕外方向 * 猫爪长度)
        Vector2 finalWristPos = actualTipPos + (vectorOut * _pawLength);
        
        // 手腕起始位置 (屏幕外) = 最终位置再往外退一段距离
        // 屏幕对角线长度作为安全距离
        float safeDist = _parentRect.rect.width + _parentRect.rect.height;
        Vector2 startWristPos = finalWristPos + (vectorOut * safeDist);

        // =========================================================
        // 执行动画
        // =========================================================
        
        _rect.anchoredPosition = startWristPos;
        
        _seq = DOTween.Sequence();
        _seq.SetUpdate(true);
        
        // 阶段A: 快速伸出
        // 使用 OutBack 制造惯性冲击感
        _seq.Append(_rect.DOAnchorPos(finalWristPos, _moveDuration).SetEase(Ease.OutBack, _retractBounce, 1f));
        
        // 阶段B: 悬停按压 (Loop)
        // 沿着反方向(指向目标)向前戳
        // Vector2 pressOffset = -vectorOut * 20f; 
        // _seq.Append(_rect.DOAnchorPos(finalWristPos + pressOffset, _pressDuration)
        //     .SetLoops(-1, LoopType.Yoyo)
        //     .SetEase(Ease.InOutSine));
    }

    public void Hide()
    {
        if (!gameObject.activeSelf) return;
        if (_seq != null) _seq.Kill();

        // 缩回动画：往手腕当前朝向的反方向（即屏幕外）退
        Vector2 vectorOut = (Vector2)_rect.up * -1f; 
        Vector2 currentPos = _rect.anchoredPosition;
        
        // 为了保险，直接把当前位置往外推
        Vector2 retractPos = currentPos + (vectorOut.normalized * 1500f); 
        
        _rect.DOAnchorPos(retractPos, 0.3f).SetUpdate(true)
            .SetEase(Ease.InBack, _retractBounce, 1f)
            .OnComplete(() => {
                VisualHide();
            });
    }

    // --- 核心辅助方法 ---

    /// <summary>
    /// 检查按照当前方向伸入，猫爪是否足够长覆盖到边缘（不穿帮）
    /// </summary>
    private bool IsLengthEnough(Vector2 targetPos, Vector2 dir)
    {
        // 预测手腕的位置：目标 + 方向 * (长度 + Gap)
        // 这里稍微减去一点 _safeMargin，确保不仅是刚好的，还要留有余地
        Vector2 wristPos = targetPos + (dir * (_pawLength + _tipGap - _safeMargin));

        // 检查这个手腕位置是否还在父容器的 Rect 范围内
        // 如果在范围内，说明猫爪还没伸出屏幕就已经到头了 -> 穿帮
        return !_parentRect.rect.Contains(wristPos);
    }

    /// <summary>
    /// 获取距离目标点最近的边缘方向（上、下、左、右）
    /// </summary>
    private Vector2 GetShortestEdgeDirection(Vector2 targetPos)
    {
        Rect r = _parentRect.rect;
        
        // 计算到四边的距离
        float distLeft = Mathf.Abs(targetPos.x - r.xMin);
        float distRight = Mathf.Abs(targetPos.x - r.xMax);
        float distBottom = Mathf.Abs(targetPos.y - r.yMin);
        float distTop = Mathf.Abs(targetPos.y - r.yMax);

        // 找到最小值
        float min = Mathf.Min(distLeft, distRight, distBottom, distTop);

        // 返回对应的法线方向（指向屏幕外）
        if (Mathf.Approximately(min, distBottom)) return Vector2.down;
        if (Mathf.Approximately(min, distTop)) return Vector2.up;
        if (Mathf.Approximately(min, distLeft)) return Vector2.left;
        return Vector2.right;
    }

    private void VisualShow(){
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
    }

    private void VisualHide(){
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }
}