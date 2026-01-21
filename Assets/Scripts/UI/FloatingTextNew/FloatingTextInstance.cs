using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;
public enum FloatingTextMode
{
    Normal,     // 普通上飘 (伤害)
    Physics,    // 物理下坠 (爆金币/爆物品效果)
    Gather      // 物理下坠 -> 飞向目标 (收集金币/经验)
}

[RequireComponent(typeof(CanvasGroup))]
public class FloatingTextInstance : MonoBehaviour
{
    [Header("--- 组件引用 ---")]
    [SerializeField] private TextMeshProUGUI _textComp;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;

    [Header("--- 默认手感参数 ---")]
    [SerializeField] private float _duration = 1.0f;
    [SerializeField] private bool _useUnscaledTime = true; // 忽略 TimeScale

    [Header("物理下坠模式参数")]
    [SerializeField] private float _jumpPower = 100f;   // 跳跃高度
    [SerializeField] private float _dropHeight = 150f;  // 下坠深度

    // 内部状态
    private Sequence _seq;
    private Action<FloatingTextInstance> _recycleCallback;
    private FloatingTextSettings _settings => SettingManager.Instance.FloatingTextSettings;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Init(string content, Vector3 startPos, FloatingTextMode mode, Action<FloatingTextInstance> onRecycle, 
                     Vector3? targetPos = null, ScatterMode scatterMode = ScatterMode.Both, Action onArrive = null, Color? color = null, float sizeScale = 1f)
    {
        _recycleCallback = onRecycle;

        if (_seq != null) _seq.Kill();
        
        _textComp.text = content;
        _textComp.color = color ?? Color.white;
        _rectTransform.position = startPos;
        _canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one * sizeScale; 
        
        gameObject.SetActive(true);

        PlayAnim(mode, targetPos, scatterMode, onArrive);
    }

    private void PlayAnim(FloatingTextMode mode, Vector3? targetPos, ScatterMode scatterMode, Action onArrive)
    {
        _seq = DOTween.Sequence();
        _seq.SetUpdate(_useUnscaledTime);

        // --- 第一阶段：出现动画 ---
        switch (mode)
        {
            case FloatingTextMode.Normal:
                // 普通上飘
                _seq.Join(_rectTransform.DOMoveY(_rectTransform.position.y + 100f, _duration).SetEase(Ease.OutQuad).SetLink(gameObject));
                DoScalePunch(_seq); // Q弹
                break;

            case FloatingTextMode.Physics:
                // 物理下坠 (Drop)
                DoPhysicsSpread(_seq);
                break;

            case FloatingTextMode.Gather:
                // 简单的向上扇形散射 (Scatter)
                DoFanScatter(_seq, scatterMode);
                break;
        }

        // --- 第二阶段：结束或飞行 ---
        if (mode == FloatingTextMode.Gather && targetPos.HasValue)
        {
            // 停顿 -> 飞向目标
            _seq.AppendInterval(_settings.GatherDelay);
            _seq.AppendCallback(() => DoFlyToTarget(targetPos.Value, onArrive));
        }
        else
        {
            // 淡出销毁
            _seq.Insert(_duration * 0.8f, _canvasGroup.DOFade(0f, _duration * 0.2f).SetLink(gameObject));
            _seq.OnComplete(() => _recycleCallback?.Invoke(this));
        }
    }

    // 逻辑：简单的向上扇形散射 (无重力，无大幅度跳跃)
    private void DoFanScatter(Sequence seq, ScatterMode scatterMode)
    {
        // 1. 计算随机角度 (以竖直向上 Vector3.up 为中心)
        float randomAngle = 0;
        switch (scatterMode)
        {
            case ScatterMode.Both:
                randomAngle = Random.Range(-_settings.SpreadAngle / 2f, _settings.SpreadAngle / 2f);
                break;
            case ScatterMode.Left:
                randomAngle = Random.Range(-_settings.SpreadAngle / 6f, _settings.SpreadAngle / 2f);
                break;
            case ScatterMode.Right:
                randomAngle = Random.Range(-_settings.SpreadAngle / 2f, _settings.SpreadAngle / 6f);
                break;
        }
        // 使用四元数旋转 "向上" 向量
        Quaternion rotation = Quaternion.Euler(0, 0, randomAngle);
        Vector3 dir = rotation * Vector3.up; 

        // 2. 计算终点 (方向 * 距离)
        // 增加一点距离的随机性，让它们参差不齐
        
        float dist = Random.Range(_settings.ScatterDistance * _settings.ScatterMultiplierMin, _settings.ScatterDistance * _settings.ScatterMultiplierMax);
        Vector3 endPos = _rectTransform.position + (dir * dist);

        // 3. 快速移动到位 (比如 0.3秒)
        float scatterTime = _settings.ScatterTime;
        seq.Join(_rectTransform.DOMove(endPos, scatterTime).SetEase(_settings.ScatterEase, _settings.ScatterAmplitude, _settings.ScatterPeriod).SetLink(gameObject));
        
        // // 4. 配合缩放效果
        // seq.Join(transform.DOScale(0.5f, 0f))
        //    .Join(transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack))
        //    .Join(transform.DOScale(1.0f, scatterTime - 0.2f));
    }

    // 逻辑：物理下坠 (旧逻辑，用于掉落物品)
    private void DoPhysicsSpread(Sequence seq)
    {
        float angle = Random.Range(-90f, 90f) * Mathf.Deg2Rad; // 左右横跳
        Vector3 dir = new Vector3(Mathf.Sin(angle), 0, 0); 
        float force = Random.Range(50f, 150f);
        
        Vector3 endPos = _rectTransform.position + (dir * force) + (Vector3.down * _dropHeight);
        
        seq.Join(_rectTransform.DOJump(endPos, _jumpPower, 1, _duration).SetEase(Ease.Linear).SetLink(gameObject));
        DoScalePunch(seq);
    }

    // 逻辑：S型曲线飞向目标
    private void DoFlyToTarget(Vector3 target, Action onArrive)
    {
        Vector3 start = transform.position;
        Vector3 mid = (start + target) / 2f;
        mid += (Vector3)Random.insideUnitCircle * _settings.CurveDeviation; // 随机弯曲控制点

        Vector3[] path = new Vector3[] { mid, target };

        transform.DOPath(path, _settings.FlightDuration, PathType.CatmullRom).SetLink(gameObject)
            .SetEase(_settings.FlightMotionEase)
            .SetUpdate(_useUnscaledTime)
            .OnComplete(() => {
                onArrive?.Invoke(); 
                _recycleCallback?.Invoke(this); 
            });
            
        transform.DOScale(_settings.FlightScale, _settings.FlightDuration).SetUpdate(_useUnscaledTime).SetEase(_settings.FlightScaleEase).SetLink(gameObject); // 飞行变小
    }

    private void DoScalePunch(Sequence seq)
    {
        seq.Insert(0, transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack).SetLink(gameObject))
           .Insert(0.2f, transform.DOScale(1f, 0.5f).SetLink(gameObject));
    }

    private void OnDisable() => _seq?.Kill();
}