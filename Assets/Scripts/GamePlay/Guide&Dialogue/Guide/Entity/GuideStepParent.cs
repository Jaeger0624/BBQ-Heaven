using System;
using UnityEngine;
using UniRx;
using QFramework;
using UnityEngine.UI;
using System.ComponentModel;
using Sirenix.OdinInspector;
using cfg;

/// <summary>
/// 教程步骤展示器 - 挂载在需要显示教程的UI元素上
/// 参考 TooltipParent 的设计思路，在编辑器中配置显示位置和排列方式
/// </summary>
public class GuideStepParent : MonoBehaviour, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [Header("显示配置")]
    [Tooltip("文本显示方向")]
    [SerializeField] private GuideTextDirection textDirection = GuideTextDirection.右;
    [SerializeField] private GuideTargetType targetType = GuideTargetType.无;
    [SerializeField] private Transform targetTransform;
    private bool isActive = false;

    // ========== UniRx Subject ==========
    private Subject<Unit> _onShowSubject = new Subject<Unit>();
    private Subject<Unit> _onHideSubject = new Subject<Unit>();
    private CompositeDisposable _disposables = new CompositeDisposable();
    private GuideStepInfo _stepInfo;
    private void Start()
    {
        // 监听教程步骤显示事件
        this.RegisterEvent<GuideStepShowEvent>(OnStepShow).UnRegisterWhenGameObjectDestroyed(this.gameObject);
        
        // 监听教程步骤隐藏事件
        this.RegisterEvent<GuideStepHideEvent>(OnStepHide).UnRegisterWhenGameObjectDestroyed(this.gameObject);

        // 设置点击监听（如果需要等待点击）
        SetupClickListener();
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
        _onShowSubject?.Dispose();
        _onHideSubject?.Dispose();
    }

    /// <summary>
    /// 显示教程步骤
    /// </summary>
    public void ShowStep()
    {
        if (isActive) return;
        
        isActive = true;

        bool isShow = _stepInfo.target != GuideTargetType.简单对话;
        
        // 调用 GuidePanel 显示教程气泡
        if (GuidePanel.Instance != null)
        {
            GuidePanel.Instance.ShowGuideFocus(
                GetComponent<RectTransform>(), 
                _stepInfo.guideText,
                targetTransform,
                textDirection,
                _stepInfo.isConcerned,
                isShow
            );
        }
        
        Debug.Log($"[GuideStepParent] 显示教程步骤: {_stepInfo?.name}");
    }

    /// <summary>
    /// 隐藏教程步骤
    /// </summary>
    public void HideStep()
    {
        if (!isActive) return;
        
        isActive = false;
        
        if (GuidePanel.Instance != null)
        {
            GuidePanel.Instance.HideGuide();
        }
        
        Debug.Log($"[GuideStepParent] 隐藏教程步骤: {_stepInfo?.name}");

        // 清空步骤信息
        _stepInfo = null;
    }
    /// <summary>
    /// 处理教程步骤显示事件
    /// </summary>
    private void OnStepShow(GuideStepShowEvent evt)
    {
        if (evt.stepInfo.target != targetType) return;

        _stepInfo = evt.stepInfo;
        ShowStep();
    }

    /// <summary>
    /// 处理教程步骤隐藏事件
    /// </summary>
    private void OnStepHide(GuideStepHideEvent evt)
    {
        HideStep();
    }

    /// <summary>
    /// 设置点击监听（如果步骤需要等待点击）
    /// </summary>
    private void SetupClickListener()
    {
        if (_stepInfo == null) return;
        if (_stepInfo.triggerAction == PlayerActionType.点击按钮)
        {
            // 如果有按钮组件，则设置点击监听
            var button = GetComponent<ButtonUI>();
            if (button != null)
            {
                button.OnClick.AddListener(() => {
                    this.SendEvent(new PlayerActionEvent(PlayerActionType.点击按钮));
                });
            }
        }
    }
}
