# 教程引导系统技术方案 (Technical Specification)

> **版本**: v1.0  
> **创建日期**: 2026-02-14  
> **架构师**: The Architect  
> **状态**: 待实现

---

## 📋 目录

1. [需求概述](#1-需求概述)
2. [架构设计](#2-架构设计)
3. [数据结构设计](#3-数据结构设计)
4. [系统层设计](#4-系统层设计)
5. [组件层设计](#5-组件层设计)
6. [事件系统](#6-事件系统)
7. [UniRx 流设计](#7-unirx-流设计)
8. [编辑器配置](#8-编辑器配置)
9. [外部接口](#9-外部接口)
10. [实现清单](#10-实现清单)

---

## 1. 需求概述

### 1.1 核心需求

1. **启动入口**: 提供外部调用的教程启动方法，暂不实现自动触发（如首次游戏）
2. **线性/非线性引导**: 支持两种引导模式
   - **线性**: 步骤按顺序执行，必须完成当前步骤才能进入下一步
   - **非线性**: 玩家可以跳过某些步骤，或按任意顺序完成
3. **配置驱动**: 通过 ScriptableObject 配置教程内容
   - `text`: 教程气泡显示的文字
   - `PlayerActionType`: 监听的玩家行为，触发后推进教程
4. **参考 Tooltip**: 借鉴 [`TooltipParent`](Assets/Scripts/UI/Tooltip/TooltipParent.cs:7) 的组织方式，在编辑器中配置显示位置和排列方式

### 1.2 设计原则

- **高内聚、低耦合**: 遵循 QFramework 架构分层
- **响应式编程**: 使用 UniRx 替代协程
- **配置与代码分离**: 通过 SO 配置教程流程
- **复用现有系统**: 复用 [`GuideTarget`](Assets/Scripts/GamePlay/Guide&Dialogue/Guide/GuideTarget.cs:9)、[`GuidePanel`](Assets/Scripts/GamePlay/Guide&Dialogue/Guide/GuidePanel.cs:6) 等现有组件

---

## 2. 架构设计

### 2.1 系统架构图

```
┌─────────────────────────────────────────────────────────┐
│                    View Layer (Unity)                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │GuideStepParent│  │  GuidePanel  │  │  GuideTarget │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓ ↑
┌─────────────────────────────────────────────────────────┐
│                 Controller Layer (QFramework)           │
│  ┌──────────────────────────────────────────────────┐  │
│  │         GuideController (IController)             │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓ ↑
┌─────────────────────────────────────────────────────────┐
│                   System Layer (QFramework)             │
│  ┌──────────────────────────────────────────────────┐  │
│  │          IGuideSystem (AbstractSystem)            │  │
│  │  - 流程管理 (StartGuide/StopGuide)                │  │
│  │  - 步骤推进 (NextStep/SkipStep)                   │  │
│  │  - 状态查询 (IsGuideActive/CurrentStep)           │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓ ↑
┌─────────────────────────────────────────────────────────┐
│                   Data Layer (ScriptableObject)         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ GuideFlow SO │  │GuideStepInfo │  │ PlayerAction │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### 2.2 数据流

```
外部调用 StartGuide(flowID)
    ↓
GuideSystem 加载 GuideFlow 配置
    ↓
初始化当前步骤索引 = 0
    ↓
发送 GuideStepShowEvent (显示当前步骤)
    ↓
GuideStepParent 接收事件 → 显示教程气泡
    ↓
监听 PlayerActionEvent
    ↓
匹配成功 → GuideSystem.NextStep()
    ↓
发送 GuideStepAdvanceEvent
    ↓
重复直到所有步骤完成
    ↓
发送 GuideCompleteEvent
```

---

## 3. 数据结构设计

### 3.1 [`GuideStepInfo`](Assets/Scripts/GamePlay/Guide&Dialogue/Guide/Entity/GuideInfo.cs:5) - 教程步骤配置

**文件路径**: `Assets/Scripts/GamePlay/Guide&Dialogue/Guide/Entity/GuideStepInfo.cs`

```csharp
using System;
using UnityEngine;

/// <summary>
/// 教程步骤配置 - 单个教程步骤的数据
/// </summary>
[CreateAssetMenu(fileName = "GuideStepInfo", menuName = "Tutorial/GuideStep")]
public class GuideStepInfo : ScriptableObject
{
    [Header("基础信息")]
    [TextArea(3, 10)]
    [Tooltip("教程气泡中显示的文字")]
    public string guideText = "这是教程文本";

    [Header("触发条件")]
    [Tooltip("监听的玩家行为类型，触发后推进教程")]
    public PlayerActionType triggerAction = PlayerActionType.无;

    [Header("高亮目标（可选）")]
    [Tooltip("对应 GuideTarget.TargetID，留空则不高亮")]
    public string targetID = "";

    [Header("显示配置")]
    [Tooltip("气泡文本显示方向")]
    public GuideTextDirection textDirection = GuideTextDirection.右;
    
    [Tooltip("是否需要玩家点击才能推进（不监听 PlayerAction）")]
    public bool waitForClick = false;
    
    [Tooltip("显示前的延迟时间（秒）")]
    public float delayBeforeShow = 0f;

    [Header("非线性专用")]
    [Tooltip("步骤组ID，同组步骤可任意顺序完成")]
    public string groupID = "";
    
    [Tooltip("是否为可选步骤（跳过不影响流程）")]
    public bool isOptional = false;
}
```

### 3.2 [`GuideFlow`](Assets/Scripts/GamePlay/Guide&Dialogue/Guide/Entity/GuideFlow.cs) - 教程流程配置

**文件路径**: `Assets/Scripts/GamePlay/Guide&Dialogue/Guide/Entity/GuideFlow.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 教程流程配置 - 管理一组教程步骤
/// </summary>
[CreateAssetMenu(fileName = "GuideFlow", menuName = "Tutorial/GuideFlow")]
public class GuideFlow : ScriptableObject
{
    [Header("流程信息")]
    [Tooltip("唯一标识符")]
    public string flowID;

    [Tooltip("是否为线性流程（true=按顺序执行，false=可跳步）")]
    public bool isLinear = true;

    [Header("步骤列表")]
    [Tooltip("教程步骤列表")]
    public List<GuideStepInfo> steps = new List<GuideStepInfo>();

    [Header("流程配置")]
    [Tooltip("是否允许跳过整个教程")]
    public bool canSkip = false;
    
    [Tooltip("是否暂停游戏时间（调用 TimeSystem）")]
    public bool pauseGame = false;

    [Header("完成奖励（可选）")]
    [Tooltip("完成教程后触发的奖励")]
    public string rewardID = "";
}
```

### 3.3 [`PlayerActionType`](Assets/Scripts/GamePlay/Guide&Dialogue/Guide/Events/PlayerActionEvent.cs:10) - 扩展玩家动作枚举

**文件路径**: `Assets/Scripts/GamePlay/Guide&Dialogue/Guide/Events/PlayerActionEvent.cs`

```csharp
/// <summary>
/// 玩家动作类型枚举 - 用于教程监听
/// </summary>
public enum PlayerActionType
{
    无,
    选择串签,
    移动串签,
    确认制作,
    抽取食材卡,
    出售串串,
    使用卡牌,
    点击UI,
    // 后续根据游戏需求扩展...
}
```

---

## 4. 系统层设计

### 4.1 [`IGuideSystem`](Assets/Scripts/GamePlay/Guide&Dialogue/Guide/GuideSystem.cs:5) - 接口扩展

**文件路径**: `Assets/Scripts/GamePlay/Guide&Dialogue/Guide/GuideSystem.cs`

```csharp
/// <summary>
/// 教程系统接口 - 扩展现有接口
/// </summary>
public interface IGuideSystem : ISystem 
{
    // ========== 现有方法 ==========
    void RegisterTarget(GuideTarget target);
    void UnregisterTarget(GuideTarget target);
    GuideTarget GetTarget(string targetID);

    // ========== 新增方法 - 流程控制 ==========
    /// <summary>
    /// 启动指定教程
    /// </summary>
    void StartGuide(string flowID);

    /// <summary>
    /// 停止当前教程
    /// </summary>
    void StopGuide();

    /// <summary>
    /// 暂停教程（保留当前状态）
    /// </summary>
    void PauseGuide();

    /// <summary>
    /// 恢复教程
    /// </summary>
    void ResumeGuide();

    // ========== 新增方法 - 步骤控制 ==========
    /// <summary>
    /// 推进到下一步
    /// </summary>
    void NextStep();

    /// <summary>
    /// 跳过当前步骤（仅非线性流程）
    /// </summary>
    void SkipStep();

    // ========== 状态查询 ==========
    /// <summary>
    /// 当前是否有教程在运行
    /// </summary>
    bool IsGuideActive { get; }

    /// <summary>
    /// 当前教程流程ID
    /// </summary>
    string CurrentFlowID { get; }

    /// <summary>
    /// 当前步骤索引
    /// </summary>
    int CurrentStepIndex { get; }

    /// <summary>
    /// 当前步骤信息
    /// </summary>
    GuideStepInfo CurrentStep { get; }
}
```

### 4.2 [`GuideSystem`](Assets/Scripts/GamePlay/Guide&Dialogue/Guide/GuideSystem.cs:11) - 实现类

**关键实现要点**:

```csharp
public class GuideSystem : AbstractSystem, IGuideSystem
{
    // ========== 依赖注入 ==========
    private Dictionary<string, GuideTarget> _targets = new Dictionary<string, GuideTarget>();
    
    // ========== 教程状态 ==========
    private GuideFlow _currentFlow;
    private int _currentStepIndex = -1;
    private bool _isPaused = false;
    private CompositeDisposable _guideDisposables = new CompositeDisposable();

    // ========== 属性实现 ==========
    public bool IsGuideActive => _currentFlow != null;
    public string CurrentFlowID => _currentFlow?.flowID;
    public int CurrentStepIndex => _currentStepIndex;
    public GuideStepInfo CurrentStep => 
        (_currentFlow != null && _currentStepIndex >= 0 && _currentStepIndex < _currentFlow.steps.Count) 
            ? _currentFlow.steps[_currentStepIndex] 
            : null;

    protected override void OnInit()
    {
        // 监听玩家动作事件
        this.RegisterEvent<PlayerActionEvent>(OnPlayerAction)
            .AddTo(_guideDisposables);
    }

    // ========== 核心方法实现 ==========
    public void StartGuide(string flowID)
    {
        // 1. 加载 GuideFlow 配置（通过 Resources 或 Addressables）
        // 2. 初始化状态
        // 3. 发送 GuideStartEvent
        // 4. 显示第一步
    }

    public void NextStep()
    {
        // 1. 检查是否为线性流程
        // 2. 推进索引
        // 3. 发送 GuideStepAdvanceEvent
        // 4. 检查是否完成
    }

    private void OnPlayerAction(PlayerActionEvent evt)
    {
        if (!IsGuideActive || _isPaused) return;

        var currentStep = CurrentStep;
        if (currentStep == null) return;

        // 匹配玩家动作
        if (currentStep.triggerAction == evt.actionType)
        {
            NextStep();
        }
    }
}
```

---

## 5. 组件层设计

### 5.1 [`GuideStepParent`](Assets/Scripts/GamePlay/Guide&Dialogue/Guide/Entity/GuideShower.cs) - 教程步骤展示器

**设计思路**: 参考 [`TooltipParent`](Assets/Scripts/UI/Tooltip/TooltipParent.cs:7) 的组织方式

**文件路径**: `Assets/Scripts/GamePlay/Guide&Dialogue/Guide/Entity/GuideStepParent.cs`

```csharp
using System;
using UnityEngine;
using UniRx;
using QFramework;

/// <summary>
/// 教程步骤展示器 - 挂载在需要显示教程的UI元素上
/// 参考 TooltipParent 的设计思路
/// </summary>
public class GuideStepParent : MonoBehaviour
{
    [Header("配置")]
    [Tooltip("关联的教程步骤配置")]
    [SerializeField] private GuideStepInfo stepInfo;

    [Header("显示配置")]
    [Tooltip("文本显示方向")]
    [SerializeField] private GuideTextDirection textDirection = GuideTextDirection.右;
    
    [Tooltip("文本偏移量")]
    [SerializeField] private Vector2 textOffset = new Vector2(20, 0);

    [Header("运行时状态（只读）")]
    [SerializeField] private bool isActive = false;

    // ========== UniRx Subject ==========
    private Subject<Unit> _onShowSubject = new Subject<Unit>();
    private Subject<Unit> _onHideSubject = new Subject<Unit>();
    
    public IObservable<Unit> OnShowAsObservable => _onShowSubject;
    public IObservable<Unit> OnHideAsObservable => _onHideSubject;

    private CompositeDisposable _disposables = new CompositeDisposable();

    private void Start()
    {
        // 监听教程步骤显示事件
        this.RegisterEvent<GuideStepShowEvent>(OnStepShow)
            .AddTo(_disposables);
        
        // 监听教程步骤隐藏事件
        this.RegisterEvent<GuideStepHideEvent>(OnStepHide)
            .AddTo(_disposables);
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
    }

    // ========== 事件处理 ==========
    private void OnStepShow(GuideStepShowEvent evt)
    {
        // 检查是否是当前步骤
        if (stepInfo != null && evt.stepInfo == stepInfo)
        {
            // 延迟显示
            if (stepInfo.delayBeforeShow > 0)
            {
                Observable.Timer(TimeSpan.FromSeconds(stepInfo.delayBeforeShow))
                    .Subscribe(_ => ShowStep())
                    .AddTo(_disposables);
            }
            else
            {
                ShowStep();
            }
        }
    }

    private void OnStepHide(GuideStepHideEvent evt)
    {
        if (stepInfo != null && evt.stepInfo == stepInfo)
        {
            HideStep();
        }
    }
}
```

---

## 6. 事件系统

### 6.1 新增事件类型

**文件路径**: `Assets/Scripts/GamePlay/Guide&Dialogue/Guide/Events/GuideEvents.cs`

```csharp
using QFramework;

/// <summary>
/// 教程开始事件
/// </summary>
public class GuideStartEvent : AbstractEvent
{
    public string flowID;
    public GuideFlow flow;

    public GuideStartEvent(string flowID, GuideFlow flow)
    {
        this.flowID = flowID;
        this.flow = flow;
    }
}

/// <summary>
/// 教程步骤显示事件
/// </summary>
public class GuideStepShowEvent : AbstractEvent
{
    public string flowID;
    public int stepIndex;
    public GuideStepInfo stepInfo;

    public GuideStepShowEvent(string flowID, int stepIndex, GuideStepInfo stepInfo)
    {
        this.flowID = flowID;
        this.stepIndex = stepIndex;
        this.stepInfo = stepInfo;
    }
}

/// <summary>
/// 教程步骤隐藏事件
/// </summary>
public class GuideStepHideEvent : AbstractEvent
{
    public string flowID;
    public int stepIndex;
    public GuideStepInfo stepInfo;

    public GuideStepHideEvent(string flowID, int stepIndex, GuideStepInfo stepInfo)
    {
        this.flowID = flowID;
        this.stepIndex = stepIndex;
        this.stepInfo = stepInfo;
    }
}

/// <summary>
/// 教程步骤推进事件
/// </summary>
public class GuideStepAdvanceEvent : AbstractEvent
{
    public string flowID;
    public int previousStepIndex;
    public int currentStepIndex;
    public GuideStepInfo previousStep;
    public GuideStepInfo currentStep;

    public GuideStepAdvanceEvent(string flowID, int prevIndex, int currIndex, 
                                  GuideStepInfo prevStep, GuideStepInfo currStep)
    {
        this.flowID = flowID;
        this.previousStepIndex = prevIndex;
        this.currentStepIndex = currIndex;
        this.previousStep = prevStep;
        this.currentStep = currStep;
    }
}

/// <summary>
/// 教程完成事件
/// </summary>
public class GuideCompleteEvent : AbstractEvent
{
    public string flowID;
    public GuideFlow flow;

    public GuideCompleteEvent(string flowID, GuideFlow flow)
    {
        this.flowID = flowID;
        this.flow = flow;
    }
}

/// <summary>
/// 教程停止事件
/// </summary>
public class GuideStopEvent : AbstractEvent
{
    public string flowID;
    public int stoppedAtStepIndex;

    public GuideStopEvent(string flowID, int stoppedAtStepIndex)
    {
        this.flowID = flowID;
        this.stoppedAtStepIndex = stoppedAtStepIndex;
    }
}
```

---

## 7. UniRx 流设计

### 7.1 玩家动作监听流

```csharp
// 在 GuideSystem.OnInit() 中
this.RegisterEvent<PlayerActionEvent>(OnPlayerAction)
    .AddTo(_guideDisposables);

// 处理逻辑
private void OnPlayerAction(PlayerActionEvent evt)
{
    if (!IsGuideActive || _isPaused) return;

    var currentStep = CurrentStep;
    if (currentStep == null) return;

    // 线性流程：严格匹配当前步骤
    if (_currentFlow.isLinear)
    {
        if (currentStep.triggerAction == evt.actionType)
        {
            NextStep();
        }
    }
    // 非线性流程：检查所有未完成步骤
    else
    {
        CheckNonLinearProgress(evt.actionType);
    }
}
```

### 7.2 延迟显示流

```csharp
// 在 GuideStepParent 中
private void OnStepShow(GuideStepShowEvent evt)
{
    if (stepInfo != null && evt.stepInfo == stepInfo)
    {
        if (stepInfo.delayBeforeShow > 0)
        {
            Observable.Timer(TimeSpan.FromSeconds(stepInfo.delayBeforeShow))
                .Subscribe(_ => ShowStep())
                .AddTo(_disposables);
        }
        else
        {
            ShowStep();
        }
    }
}
```

### 7.3 点击推进流

```csharp
// 在 GuideStepParent 中，处理 waitForClick 的情况
private void SetupClickListener()
{
    if (stepInfo == null || !stepInfo.waitForClick) return;

    var button = GetComponent<UnityEngine.UI.Button>();
    if (button != null)
    {
        button.OnClickAsObservable()
            .Where(_ => isActive)
            .Subscribe(_ => 
            {
                // 发送点击事件，推进教程
                this.SendEvent(new PlayerActionEvent(PlayerActionType.点击UI));
            })
            .AddTo(_disposables);
    }
}
```

---

## 8. 编辑器配置

### 8.1 创建教程流程

```
Assets/Create/Tutorial/GuideFlow
```

配置项：
- `flowID`: 唯一标识（如 "tutorial_basic", "tutorial_combat"）
- `isLinear`: 是否线性流程
- `steps`: 步骤列表（拖入 GuideStepInfo 资源）
- `canSkip`: 是否允许跳过
- `pauseGame`: 是否暂停游戏时间

### 8.2 创建教程步骤

```
Assets/Create/Tutorial/GuideStep
```

配置项：
- `guideText`: 教程文本
- `triggerAction`: 触发动作
- `targetID`: 高亮目标ID（对应 GuideTarget.TargetID）
- `textDirection`: 文本显示方向
- `waitForClick`: 是否等待点击
- `delayBeforeShow`: 显示延迟

### 8.3 场景配置流程

1. **添加高亮目标**:
   - 在需要高亮的UI元素上添加 [`GuideTarget`](Assets/Scripts/GamePlay/Guide&Dialogue/Guide/GuideTarget.cs:9) 组件
   - 设置 `TargetID`（如 "Btn_SelectStick"）

2. **添加教程展示器**:
   - 在需要显示教程文本的位置添加 [`GuideStepParent`](Assets/Scripts/GamePlay/Guide&Dialogue/Guide/Entity/GuideShower.cs) 组件
   - 关联 `GuideStepInfo` 资源
   - 配置显示方向和偏移

3. **启动教程**:
   ```csharp
   this.GetSystem<IGuideSystem>().StartGuide("tutorial_basic");
   ```

---

## 9. 外部接口

### 9.1 启动教程（唯一入口）

```csharp
// 方式1: 通过 System 直接调用
this.GetSystem<IGuideSystem>().StartGuide("tutorial_basic");

// 方式2: 通过 Controller 调用（推荐）
public class TutorialController : AbstractController
{
    public void StartBasicTutorial()
    {
        this.GetSystem<IGuideSystem>().StartGuide("tutorial_basic");
    }
}
```

### 9.2 停止教程

```csharp
this.GetSystem<IGuideSystem>().StopGuide();
```

### 9.3 查询状态

```csharp
var guideSystem = this.GetSystem<IGuideSystem>();

if (guideSystem.IsGuideActive)
{
    Debug.Log($"当前教程: {guideSystem.CurrentFlowID}");
    Debug.Log($"当前步骤: {guideSystem.CurrentStepIndex}");
    Debug.Log($"步骤内容: {guideSystem.CurrentStep?.guideText}");
}
```

### 9.4 监听教程事件

```csharp
// 监听教程开始
this.RegisterEvent<GuideStartEvent>(evt => 
{
    Debug.Log($"教程开始: {evt.flowID}");
}).AddTo(this);

// 监听步骤推进
this.RegisterEvent<GuideStepAdvanceEvent>(evt => 
{
    Debug.Log($"步骤推进: {evt.previousStepIndex} -> {evt.currentStepIndex}");
}).AddTo(this);

// 监听教程完成
this.RegisterEvent<GuideCompleteEvent>(evt => 
{
    Debug.Log($"教程完成: {evt.flowID}");
}).AddTo(this);
```

---

## 10. 实现清单

### 10.1 需要创建的脚本

| 文件路径 | 类名 | 继承/实现 | 说明 |
|---------|------|----------|------|
| `Entity/GuideStepInfo.cs` | `GuideStepInfo` | `ScriptableObject` | 教程步骤配置 |
| `Entity/GuideFlow.cs` | `GuideFlow` | `ScriptableObject` | 教程流程配置 |
| `Events/GuideEvents.cs` | 多个事件类 | `AbstractEvent` | 教程相关事件 |
| `GuideSystem.cs` | `IGuideSystem` | `AbstractSystem` | 扩展系统接口和实现 |
| `Entity/GuideStepParent.cs` | `GuideStepParent` | `MonoBehaviour` | 教程步骤展示器 |

### 10.2 需要修改的脚本

| 文件路径 | 修改内容 |
|---------|---------|
| `Events/PlayerActionEvent.cs` | 扩展 `PlayerActionType` 枚举 |
| `GuideSystem.cs` | 实现新增的接口方法 |
| `GuidePanel.cs` | 确保支持动态文本和位置设置 |

### 10.3 需要创建的配置资源

| 资源类型 | 数量 | 说明 |
|---------|------|------|
| `GuideFlow` | 1+ | 教程流程配置 |
| `GuideStepInfo` | 5+ | 教程步骤配置 |

---

## 11. 技术要点总结

### 11.1 QFramework 规范

- ✅ System 继承 `AbstractSystem`
- ✅ 事件继承 `AbstractEvent`
- ✅ Controller 实现 `IController`
- ✅ 通过 `this.GetSystem<T>()` 访问系统
- ✅ 通过 `this.RegisterEvent<T>()` 监听事件
- ✅ 通过 `this.SendEvent<T>()` 发送事件

### 11.2 UniRx 规范

- ✅ 使用 `Observable.Timer` 替代协程
- ✅ 使用 `Subject` 暴露 Observable
- ✅ 使用 `CompositeDisposable` 管理订阅
- ✅ 使用 `.AddTo(this)` 绑定生命周期
- ✅ Subject 变量以 `Subject` 结尾

### 11.3 命名规范

- ✅ public 属性使用 PascalCase
- ✅ UI 组件以类型结尾（如 `GuideStepParent`）
- ✅ 事件以 `Event` 结尾
- ✅ 接口以 `I` 开头

### 11.4 禁忌

- ❌ 禁止使用 `SendMessage`
- ❌ 禁止使用 `GameObject.Find`
- ❌ 禁止在 System 中使用 `UnityEngine.Transform` 等
- ❌ 禁止使用 Unity 协程

---

## 12. 后续扩展点

1. **自动触发**: 实现首次游戏自动启动教程
2. **教程存档**: 记录玩家已完成的教程
3. **多语言支持**: 集成本地化系统
4. **教程回放**: 允许玩家重新查看教程
5. **条件触发**: 基于游戏进度自动触发特定教程

---

**文档结束** - 请主程根据此技术方案实现代码
