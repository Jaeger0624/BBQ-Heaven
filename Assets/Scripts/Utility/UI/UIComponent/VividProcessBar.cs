using System;
using DG.Tweening;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 带有弹动、闪烁等动效的进度条组件，可通过 Inspector 配置不同的表现。
/// </summary>
public class VividProcessBar : MonoBehaviour, IController
{
    [Serializable]
    public class ProgressEvent : UnityEvent<float> { }

    public enum ValueDisplayMode
    {
        None,
        Normalized,
        Percentage,
        Absolute
    }

    [Header("引用")]
    [SerializeField] private RectTransform barRoot;
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform fillTransform;
    [SerializeField] private TextMeshProUGUI valueLabel;

    [Header("初始值")]
    [SerializeField, Range(0f, 1f)] private float initialNormalizedValue = 0f;
    [SerializeField] private bool applyInitialOnEnable = true;

    [Header("取值设置")]
    [SerializeField] private bool clampValue = true;
    [SerializeField, Min(0.0001f)] private float maxValue = 100f;
    [SerializeField] private bool useUnscaledTime = false;
    [SerializeField] private ValueDisplayMode valueDisplay = ValueDisplayMode.Percentage;
    [SerializeField] private string customLabelFormat = "0.0";

    [Header("动效设置")]
    [SerializeField] private bool animateValueChange = true;
    [SerializeField, Min(0.01f)] private float animationDuration = 0.35f;
    [SerializeField] private Ease animationEase = Ease.OutBack;
    [SerializeField] private float easeOvershootOrAmplitude = 1.15f;
    [SerializeField] private float easePeriod = 0f;

    [Header("弹动效果")]
    [SerializeField] private bool punchOnIncrease = true;
    [SerializeField] private bool punchOnDecrease = false;
    [SerializeField] private Vector3 punchScale = new Vector3(0.08f, 0.08f, 0f);
    [SerializeField, Min(0.01f)] private float punchDuration = 0.3f;
    [SerializeField] private int punchVibrato = 6;
    [SerializeField, Range(0f, 1f)] private float punchElasticity = 0.6f;

    [Header("颜色效果")]
    [SerializeField] private bool useFillGradient = false;
    [SerializeField] private Gradient fillGradient;
    [SerializeField] private bool flashOnIncrease = true;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField, Min(0.01f)] private float flashDuration = 0.25f;

    [Header("事件")]
    [SerializeField] private ProgressEvent onNormalizedValueChanged;
    [SerializeField] private UnityEvent onAnimationCompleted;

    private float currentNormalizedValue;
    private Tween fillTween;
    private Tween punchTween;
    private Tween flashTween;
    private readonly ColorTweenCache colorCache = new();

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    private void Awake()
    {
        CacheInitialColor();
        currentNormalizedValue = Mathf.Clamp01(initialNormalizedValue);
        ApplyVisuals(currentNormalizedValue);
    }

    private void OnEnable()
    {
        if (applyInitialOnEnable)
        {
            SetNormalizedProgress(initialNormalizedValue, true);
        }
    }

    private void OnDisable()
    {
        KillTweens();
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    /// <summary>
    /// 设置归一化进度（0-1）。
    /// </summary>
    public void SetNormalizedProgress(float normalizedValue, bool instant = false)
    {
        var target = clampValue ? Mathf.Clamp01(normalizedValue) : normalizedValue;
        UpdateProgress(target, instant);
    }

    /// <summary>
    /// 输入绝对值（0 - maxValue），内部转换为归一化值。
    /// </summary>
    public void SetAbsoluteProgress(float absoluteValue, bool instant = false)
    {
        var normalized = maxValue <= 0f ? 0f : absoluteValue / maxValue;
        SetNormalizedProgress(normalized, instant);
    }

    /// <summary>
    /// 立即刷新当前进度，不触发动画。
    /// </summary>
    public void SnapTo(float normalizedValue)
    {
        SetNormalizedProgress(normalizedValue, true);
    }

    private void UpdateProgress(float targetNormalized, bool instant)
    {
        if (!animateValueChange || instant || !isActiveAndEnabled)
        {
            KillTweens();
            ApplyVisuals(targetNormalized);
            currentNormalizedValue = targetNormalized;
            onNormalizedValueChanged?.Invoke(currentNormalizedValue);
            onAnimationCompleted?.Invoke();
            return;
        }

        if (Mathf.Approximately(currentNormalizedValue, targetNormalized))
        {
            return;
        }

        KillFillTween();

        var from = currentNormalizedValue;
        var increasing = targetNormalized > from;

        fillTween = DOTween
            .To(() => from, x =>
            {
                from = x;
                currentNormalizedValue = x;
                ApplyVisuals(x);
                onNormalizedValueChanged?.Invoke(x);
            }, targetNormalized, animationDuration)
            .SetEase(animationEase, easeOvershootOrAmplitude, easePeriod)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() =>
            {
                currentNormalizedValue = targetNormalized;
                HandlePunch(increasing);
                HandleFlash(increasing);
                onAnimationCompleted?.Invoke();
            });
    }

    private void ApplyVisuals(float normalizedValue)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(normalizedValue);

            if (useFillGradient && fillGradient != null)
            {
                fillImage.color = fillGradient.Evaluate(Mathf.Clamp01(normalizedValue));
            }

            colorCache.BaseColor = fillImage.color;
            colorCache.HasCache = true;
        }

        if (valueLabel != null)
        {
            switch (valueDisplay)
            {
                case ValueDisplayMode.None:
                    valueLabel.gameObject.SetActive(false);
                    break;
                case ValueDisplayMode.Normalized:
                    valueLabel.gameObject.SetActive(true);
                    valueLabel.text = normalizedValue.ToString(customLabelFormat);
                    break;
                case ValueDisplayMode.Percentage:
                    valueLabel.gameObject.SetActive(true);
                    valueLabel.text = (normalizedValue * 100f).ToString(customLabelFormat) + "%";
                    break;
                case ValueDisplayMode.Absolute:
                    valueLabel.gameObject.SetActive(true);
                    valueLabel.text = (normalizedValue * maxValue).ToString(customLabelFormat);
                    break;
                default:
                    valueLabel.text = normalizedValue.ToString(customLabelFormat);
                    break;
            }
        }
    }

    private void HandlePunch(bool increasing)
    {
        if ((increasing && !punchOnIncrease) || (!increasing && !punchOnDecrease))
        {
            return;
        }

        var targetTransform = fillTransform != null ? fillTransform : barRoot;
        if (targetTransform == null)
        {
            return;
        }

        punchTween?.Kill();
        targetTransform.localScale = Vector3.one;
        punchTween = targetTransform
            .DOPunchScale(punchScale, punchDuration, punchVibrato, punchElasticity)
            .SetUpdate(useUnscaledTime);
    }

    private void HandleFlash(bool increasing)
    {
        if (!flashOnIncrease || fillImage == null || !increasing)
        {
            return;
        }

        if (!colorCache.HasCache && fillImage != null)
        {
            colorCache.BaseColor = fillImage.color;
        }

        flashTween?.Kill();
        var baseColor = colorCache.BaseColor;
        fillImage.color = baseColor;

        flashTween = fillImage
            .DOColor(flashColor, flashDuration * 0.5f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() =>
            {
                fillImage.color = baseColor;
            });
    }

    private void CacheInitialColor()
    {
        if (fillImage != null)
        {
            colorCache.BaseColor = fillImage.color;
            colorCache.HasCache = true;
        }
    }

    private void KillTweens()
    {
        KillFillTween();
        punchTween?.Kill();
        flashTween?.Kill();
        punchTween = null;
        flashTween = null;
    }

    private void KillFillTween()
    {
        fillTween?.Kill();
        fillTween = null;
    }

    [ContextMenu("测试：随机增加进度")]
    private void DebugIncrease()
    {
        SetNormalizedProgress(UnityEngine.Random.Range(currentNormalizedValue, 1f));
    }

    [ContextMenu("测试：随机减少进度")]
    private void DebugDecrease()
    {
        SetNormalizedProgress(UnityEngine.Random.Range(0f, currentNormalizedValue));
    }

    public float CurrentNormalizedValue => currentNormalizedValue;
    public float CurrentAbsoluteValue => currentNormalizedValue * maxValue;

    private class ColorTweenCache
    {
        public bool HasCache;
        public Color BaseColor = Color.white;
    }
}
