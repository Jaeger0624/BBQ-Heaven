using UnityEngine;
using DG.Tweening;

public class CatPawPointer : MonoBehaviour
{
    [Header("设置")]
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private float _moveDuration = 0.5f; // 伸爪子的速度
    [SerializeField] private float _pressInterval = 0.6f; // 按压频率
    
    [Header("偏移修正")]
    // 因为爪子本身有大小，不能让指尖刚好挡住按钮中心，需要一点偏移
    // 假设爪子图片是食指指向左上，这里可能需要调整
    public Vector2 TipOffset = new Vector2(20, -20); 

    private Sequence _animSeq;
    private Vector2 _screenSize;

    private void Awake()
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        _screenSize = new Vector2(Screen.width, Screen.height);
    }

    /// <summary>
    /// 让猫爪指向某个屏幕坐标
    /// </summary>
    /// <param name="targetScreenPos">目标的屏幕坐标</param>
    public void ShowAndPoint(Vector2 targetScreenPos)
    {
        gameObject.SetActive(true);
        KillAnim();

        // 1. 计算偏移后的实际目标点 (让爪尖指着目标，而不是手掌中心盖住目标)
        Vector2 finalPos = targetScreenPos + TipOffset;

        // 2. 智能计算“出生点” (Off-Screen Position)
        // 逻辑：猫爪应该从离目标最近的屏幕边缘伸出来，或者固定从底部伸出来
        Vector2 startPos = CalculateSpawnPos(targetScreenPos);

        // 3. 设置初始状态
        _rectTransform.position = startPos;
        // 简单的朝向计算：让爪子稍微旋转指向目标 (可选)
        // float angle = Mathf.Atan2(finalPos.y - startPos.y, finalPos.x - startPos.x) * Mathf.Rad2Deg;
        // _rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90取决于素材朝向

        // 4. 构建动画序列
        _animSeq = DOTween.Sequence();
        
        // Phase A: 伸爪子 (OutExpo 模拟生物快速伸出后减速的动量)
        _animSeq.Append(_rectTransform.DOMove(finalPos, _moveDuration).SetEase(Ease.OutExpo));

        // Phase B: 循环按压 (模拟点击动作)
        // 组合缩放和微小位移
        _animSeq.AppendCallback(() => 
        {
            // 循环动画：按下去 -> 弹起来
            transform.DOScale(0.9f, 0.2f).SetLoops(-1, LoopType.Yoyo); 
        });
    }

    public void Hide()
    {
        KillAnim();
        // 缩回动画：往后退一点然后消失
        Vector2 retractPos = _rectTransform.position - (_rectTransform.up * 100); 
        _rectTransform.DOMove(retractPos, 0.2f).OnComplete(() => 
        {
            gameObject.SetActive(false);
        });
    }

    private void KillAnim()
    {
        if (_animSeq != null) _animSeq.Kill();
        transform.DOKill();
        transform.localScale = Vector3.one; // 重置缩放
    }

    // 计算猫爪从哪里伸出来最自然
    private Vector2 CalculateSpawnPos(Vector2 target)
    {
        // 方案 A：永远从屏幕底部伸出来 (最像玩手机/电脑时的猫)
        // 给X轴一点随机偏移，不要每次都直直的
        // return new Vector2(target.x + Random.Range(-50f, 50f), -200f);

        // 方案 B：根据象限决定 (左下、右下)
        if (target.x < _screenSize.x / 2)
        {
            // 目标在左边 -> 从左下角伸出来
            return new Vector2(-100f, -100f);
        }
        else
        {
            // 目标在右边 -> 从右下角伸出来
            return new Vector2(_screenSize.x + 100f, -100f);
        }
    }
}