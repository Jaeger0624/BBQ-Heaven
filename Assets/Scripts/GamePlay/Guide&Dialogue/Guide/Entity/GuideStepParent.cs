using System;
using UnityEngine;
using UniRx;
using QFramework;
using UnityEngine.UI;
using System.ComponentModel;
using Sirenix.OdinInspector;

/// <summary>
/// 教程步骤展示器 - 挂载在需要显示教程的UI元素上
/// 参考 TooltipParent 的设计思路，在编辑器中配置显示位置和排列方式
/// </summary>
public class GuideStepParent : MonoBehaviour, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [Header("配置")]
    [Tooltip("关联的教程步骤配置")]
    [SerializeField] private GuideStepInfo stepInfo;

    [Header("显示配置")]
    [Tooltip("文本显示方向")]
    [SerializeField] private GuideTextDirection textDirection = GuideTextDirection.右;
    
    [Tooltip("文本偏移量")]
    [SerializeField] private Vector2 textOffset = new Vector2(20, 0);

    private bool isActive = false;

    // ========== UniRx Subject ==========
    private Subject<Unit> _onShowSubject = new Subject<Unit>();
    private Subject<Unit> _onHideSubject = new Subject<Unit>();
    
    /// <summary>
    /// 显示事件 Observable
    /// </summary>
    public IObservable<Unit> OnShowAsObservable => _onShowSubject;
    
    /// <summary>
    /// 隐藏事件 Observable
    /// </summary>
    public IObservable<Unit> OnHideAsObservable => _onHideSubject;

    private CompositeDisposable _disposables = new CompositeDisposable();

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
        
        // 调用 GuidePanel 显示教程气泡
        if (GuidePanel.Instance != null && stepInfo != null)
        {
            GuidePanel.Instance.ShowGuideFocus(
                GetComponent<RectTransform>(), 
                stepInfo.guideText
            );
        }
        
        _onShowSubject.OnNext(Unit.Default);
        
        Debug.Log($"[GuideStepParent] 显示教程步骤: {stepInfo?.name}");
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
        
        _onHideSubject.OnNext(Unit.Default);
        
        Debug.Log($"[GuideStepParent] 隐藏教程步骤: {stepInfo?.name}");
    }
    /// <summary>
    /// 处理教程步骤显示事件
    /// </summary>
    private void OnStepShow(GuideStepShowEvent evt)
    {
        // 检查是否是当前步骤
        if (stepInfo != null && evt.stepInfo == stepInfo)
        {
            // 延迟显示
            ShowStep();
        }
    }

    /// <summary>
    /// 处理教程步骤隐藏事件
    /// </summary>
    private void OnStepHide(GuideStepHideEvent evt)
    {
        if (stepInfo != null && evt.stepInfo == stepInfo)
        {
            HideStep();
        }
    }

    /// <summary>
    /// 设置点击监听（如果步骤需要等待点击）
    /// </summary>
    private void SetupClickListener()
    {
        if (stepInfo == null) return;
        if (stepInfo.triggerAction == PlayerActionType.点击按钮)
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

    // ========== 测试方法 ==========

    [Button("预览教程步骤")]
    private void PreviewStep()
    {

        if (Application.isPlaying)
        {
            ShowStep();
        }
        else
        {
            Debug.LogWarning("[GuideStepParent] 预览功能仅在运行时可用");
        }
    }
    [Button("隐藏教程步骤")]
    private void HideStepEditor()
    {
        if (Application.isPlaying)
        {
            HideStep();
        }
    }
}
