# 架构设计规范：FloatingText 跳字系统拓展 & Bug修复

> **文档类型**: 架构设计规范 (Architecture Specification)  
> **目标读者**: 主程序 (The Coder)  
> **遵循框架**: QFramework + UniRx + DOTween

---

## 📋 任务概述

| 任务ID | 描述 | 优先级 | 修改文件 |
|--------|------|--------|----------|
| TASK-1 | 修复 `SpawnTextAnimationTask.Play()` lifetime 为 null 时不显示跳字的 Bug | 🔴 高 | [`IAnimTask.cs`](../Scripts/GamePlay/Animation/IAnimTask.cs) |
| TASK-2 | 实现 `FloatingAnimType.只放大不缩小` 动画类型 | 🟡 中 | [`FloatingTextInstance.cs`](../Scripts/UI/FloatingTextNew/FloatingTextInstance.cs) |
| TASK-3 | (可选) 优化食材属性跳字使用新动画类型 | 🟢 低 | [`FoodAnimController.cs`](../Scripts/GamePlay/Food/FoodAnimController.cs) |

---

## 🔍 问题根因分析

### Bug 定位: [`SpawnTextAnimationTask.Play()`](../Scripts/GamePlay/Animation/IAnimTask.cs:174)

**当前代码 (有问题):**
```csharp
public override IObservable<Unit> Play()
{
    if (lifetime.HasValue)  // ❌ 当 lifetime 为 null 时，整个 if 块被跳过
    {
        FloatingTextInfo info = new FloatingTextInfo(text, lifetime.Value, color.color, FloatingAnimType.Normal);
        FloatingTextManager.Instance.Show(position, text, color.color, info);
        return Observable.ReturnUnit();
    }
    return Observable.ReturnUnit();  // ❌ 什么都不做，跳字不显示
}
```

**调用链路:**
```
GA_为食材加属性.Execute() 
  → SendAnimEvent(FoodInstanceAddBaseValueEvent)
    → FoodAnimController.OnFoodInstanceAddBaseValueAnim()
      → new SpawnTextAnimationTask(rarityText, 36f, color, position)  // ⚠️ 没有传 lifetime!
        → SpawnTextAnimationTask.Play() 
          → lifetime.HasValue == false → 直接返回，不显示跳字 ❌
```

**根因**: 构造函数中 `lifetime` 参数默认值为 `null`，而 [`FoodAnimController.cs:54-58`](../Scripts/GamePlay/Food/FoodAnimController.cs:54) 调用时没有传递该参数。

---

## 🛠️ 技术方案

### TASK-1: 修复 SpawnTextAnimationTask Bug

**修改文件**: [`Assets/Scripts/GamePlay/Animation/IAnimTask.cs`](../Scripts/GamePlay/Animation/IAnimTask.cs)  
**修改位置**: `SpawnTextAnimationTask` 类 (Line 158-183)

#### 伪代码规范

```csharp
public class SpawnTextAnimationTask : AbstractAnimTask
{
    private string text;
    private float size;
    private (Color color, bool useColor) color;
    private Vector3 position;
    private float? lifetime;
    
    // ✅ 新增: 默认生命周期常量
    private const float DEFAULT_LIFETIME = 1.0f;
    
    public SpawnTextAnimationTask(string text, float size, Color color, Vector3 position, float? lifetime = null)
    {
        this.text = text;
        this.size = size;
        this.color = (color, true);
        this.position = position;
        this.lifetime = lifetime;  // 保持可空语义
    }

    public override IObservable<Unit> Play()
    {
        // ✅ 修复: 使用 null 合并运算符，确保始终有值
        float actualLifetime = lifetime ?? DEFAULT_LIFETIME;
        
        FloatingTextInfo info = new FloatingTextInfo(
            text, 
            actualLifetime, 
            color.color, 
            FloatingAnimType.Normal  // 默认使用 Normal 动画
        );
        
        FloatingTextManager.Instance.Show(position, text, color.color, info);
        
        return Observable.ReturnUnit();
    }
}
```

#### 设计要点

| 要点 | 说明 |
|------|------|
| **默认值策略** | 使用 `DEFAULT_LIFETIME = 1.0f` 作为默认生命周期 |
| **空值处理** | 使用 `??` 运算符，避免 `if-else` 分支 |
| **向后兼容** | 不修改构造函数签名，保持现有调用方无需修改 |
| **UniRx 规范** | 返回 `Observable.ReturnUnit()` 符合 `IObservable<Unit>` 接口 |

---

### TASK-2: 实现 "只放大不缩小" 动画类型

**修改文件**: [`Assets/Scripts/UI/FloatingTextNew/FloatingTextInstance.cs`](../Scripts/UI/FloatingTextNew/FloatingTextInstance.cs)  
**修改位置**: `PlayAnim()` 方法 (Line 59-101) 和新增 `DoScaleByType()` 方法

#### 当前代码分析

[`FloatingTextInstance.cs:66-86`](../Scripts/UI/FloatingTextNew/FloatingTextInstance.cs:66) 中的 `PlayAnim()` 方法：

```csharp
switch (mode)
{
    case FloatingTextMode.Normal:
        Vector2 direction = info?.direction ?? Vector2.up;
        Vector3 t = _rectTransform.position + new Vector3(direction.x * _floatHeight, direction.y * _floatHeight, 0);
        _seq.Join(_rectTransform.DOMove(t, duration).SetEase(Ease.OutSine, 10f, 0.5f).SetLink(gameObject));
        DoScalePunch(_seq); // ← 当前固定调用 DoScalePunch (放大后缩小)
        break;
    // ...
}
```

当前 [`DoScalePunch()`](../Scripts/UI/FloatingTextNew/FloatingTextInstance.cs:173) 实现：
```csharp
private void DoScalePunch(Sequence seq)
{
    seq.Insert(0, transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack).SetLink(gameObject))
       .Insert(0.2f, transform.DOScale(1f, 0.5f).SetLink(gameObject));  // ← 会缩小回 1.0
}
```

#### 伪代码规范

**Step 1: 修改 `PlayAnim()` 方法**

```csharp
private void PlayAnim(FloatingTextMode mode, Vector3? targetPos, ScatterMode scatterMode, Action onArrive, FloatingTextInfo info)
{
    float duration = info?.duration ?? _duration;
    _seq = DOTween.Sequence();
    _seq.SetUpdate(_useUnscaledTime);

    // --- 第一阶段：出现动画 ---
    switch (mode)
    {
        case FloatingTextMode.Normal:
            Vector2 direction = info?.direction ?? Vector2.up;
            Vector3 t = _rectTransform.position + new Vector3(direction.x * _floatHeight, direction.y * _floatHeight, 0);
            _seq.Join(_rectTransform.DOMove(t, duration).SetEase(Ease.OutSine, 10f, 0.5f).SetLink(gameObject));
            
            // ✅ 修改: 根据 animType 选择缩放动画
            FloatingAnimType animType = info?.animType ?? FloatingAnimType.Normal;
            DoScaleByType(_seq, animType);
            break;

        case FloatingTextMode.Physics:
            DoPhysicsSpread(_seq);
            break;

        case FloatingTextMode.Gather:
            DoFanScatter(_seq, scatterMode);
            break;
    }

    // --- 第二阶段：结束或飞行 ---
    if (mode == FloatingTextMode.Gather && targetPos.HasValue)
    {
        _seq.AppendInterval(_settings.GatherDelay);
        _seq.AppendCallback(() => DoFlyToTarget(targetPos.Value, onArrive));
    }
    else
    {
        _seq.Insert(duration * 0.8f, _canvasGroup.DOFade(0f, duration * 0.2f).SetLink(gameObject));
        _seq.OnComplete(() => _recycleCallback?.Invoke(this));
    }
}
```

**Step 2: 新增 `DoScaleByType()` 方法**

```csharp
/// <summary>
/// 根据动画类型执行不同的缩放效果
/// </summary>
/// <param name="seq">DOTween Sequence</param>
/// <param name="animType">动画类型</param>
private void DoScaleByType(Sequence seq, FloatingAnimType animType)
{
    switch (animType)
    {
        case FloatingAnimType.Normal:
            // 原有行为: 放大后缩小 (Q弹效果)
            DoScalePunch(seq);
            break;

        case FloatingAnimType.只放大不缩小:
            // ✅ 新增: 只放大，保持不放缩
            // 放大到 1.5 倍，使用 OutBack 缓动产生弹性效果
            seq.Insert(0, transform.DOScale(1.5f, 0.2f)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject));
            // 不添加缩小动画，最终保持 1.5 倍直到淡出消失
            break;
    }
}
```

#### DOTween 动画参数说明

| 参数 | 值 | 说明 |
|------|-----|------|
| `targetScale` | 1.5f | 最终放大倍率 |
| `scaleDuration` | 0.2f | 放大动画时长 |
| `Ease.OutBack` | - | 超出目标后回弹的缓动曲线，产生"弹性"手感 |

#### 动画时序图

```
Normal 动画:
├── 0.0s: Scale 1.0 → 1.5 (OutBack)
├── 0.2s: Scale 1.5 → 1.0 (线性缩小)
├── 0.8 * duration: 开始淡出
└── duration: 回收

只放大不缩小 动画:
├── 0.0s: Scale 1.0 → 1.5 (OutBack)
├── 0.2s: 保持 1.5 (不缩小)
├── 0.8 * duration: 开始淡出
└── duration: 回收 (此时 Scale 仍为 1.5)
```

---

### TASK-3: (可选) 优化食材属性跳字

**修改文件**: [`Assets/Scripts/GamePlay/Food/FoodAnimController.cs`](../Scripts/GamePlay/Food/FoodAnimController.cs)  
**修改位置**: `OnFoodInstanceAddBaseValueAnim()` 方法 (Line 41-64)

#### 当前代码问题

当前实现使用 `SpawnTextAnimationTask`，该类不支持自定义 `FloatingAnimType`。

#### 伪代码规范 (可选优化)

```csharp
void OnFoodInstanceAddBaseValueAnim(FoodInstanceAddBaseValueEvent e)
{
    IEntityView foodInstanceView = foodController.GetEntityView(e.guid);
    if (foodInstanceView == null) return;

    // 缩放旋转动画
    var scaleTween = new ScaleAnimationTask(foodInstanceView.GO().transform, 1.6f, 0.15f);
    var rotateTween = new RotateAnimationTask(foodInstanceView.GO().transform, 0.25f);

    // ✅ 方案A: 直接使用 FloatingTextManager (推荐)
    string rarityText = e.rarity > 0 ? $"+{e.rarity}" : e.rarity.ToString();
    string tasteText = e.taste > 0 ? $"+{e.taste}" : e.taste.ToString();

    Vector3 rarityPos = AnimUtility.GetTextSpawnPosition(foodInstanceView.GO().transform, true);
    Vector3 tastePos = AnimUtility.GetTextSpawnPosition(foodInstanceView.GO().transform, false);

    // 珍稀度 - 使用 "只放大不缩小" 效果
    FloatingTextInfo rarityInfo = new FloatingTextInfo(
        rarityText, 
        1.2f, 
        SettingManager.Instance.DevSettings.AddRarityTextColor, 
        FloatingAnimType.只放大不缩小
    );
    FloatingTextManager.Instance.Show(rarityPos, rarityText, 
        SettingManager.Instance.DevSettings.AddRarityTextColor, rarityInfo);

    // 美味度 - 使用 "只放大不缩小" 效果
    FloatingTextInfo tasteInfo = new FloatingTextInfo(
        tasteText, 
        1.2f, 
        SettingManager.Instance.DevSettings.AddTasteTextColor, 
        FloatingAnimType.只放大不缩小
    );
    FloatingTextManager.Instance.Show(tastePos, tasteText, 
        SettingManager.Instance.DevSettings.AddTasteTextColor, tasteInfo);

    // SFX
    var SFXAnimTask = new PlaySFXAnimationTask("Score 2", 0.02f, 0.2f).SetVolume(0.5f);
    
    // 执行并行动画 (移除 spawnTextAnimTask)
    var parallelAnimTask = new ParallelAnimTask(new List<IAnimTask>{ scaleTween, rotateTween, SFXAnimTask });
    this.GetSystem<IAnimationSystem>().DirectlyPlay(parallelAnimTask);
}
```

#### 设计决策

| 方案 | 优点 | 缺点 |
|------|------|------|
| **方案A: 直接调用 FloatingTextManager** | 灵活控制动画类型，代码清晰 | 绕过 AnimationSystem 统一调度 |
| **方案B: 扩展 SpawnTextAnimationTask** | 保持 AnimationSystem 统一调度 | 需要修改构造函数，影响面大 |

**推荐**: 方案A，因为跳字显示是即时行为，不需要纳入动画队列管理。

---

## 📐 QFramework 规范检查

### 遵循原则

| 原则 | 检查项 | 状态 |
|------|--------|------|
| **高内聚** | 动画逻辑封装在 `FloatingTextInstance` 内部 | ✅ |
| **低耦合** | 通过 `FloatingTextInfo` 传递配置，不直接依赖具体实现 | ✅ |
| **单一职责** | `DoScaleByType()` 只负责缩放动画选择 | ✅ |
| **开闭原则** | 新增动画类型通过 `switch` 扩展，不修改现有逻辑 | ✅ |

### 类职责划分

```
┌─────────────────────────────────────────────────────────────┐
│                    FloatingTextManager                       │
│  职责: 对象池管理、API 门面、坐标转换                          │
│  实现: MonoBehaviour Singleton                               │
└───────────────────────────┬─────────────────────────────────┘
                            │ 调用
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    FloatingTextInstance                      │
│  职责: 动画播放、生命周期管理                                  │
│  实现: DOTween Sequence                                      │
│  方法: PlayAnim(), DoScaleByType(), DoScalePunch()          │
└───────────────────────────┬─────────────────────────────────┘
                            │ 读取配置
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                     FloatingTextInfo                         │
│  职责: 配置数据传输对象 (DTO)                                  │
│  字段: content, duration, color, direction, animType        │
└─────────────────────────────────────────────────────────────┘
```

---

## ✅ 验收标准

### TASK-1 验收

- [ ] `SpawnTextAnimationTask` 构造时不传 `lifetime`，跳字正常显示
- [ ] `SpawnTextAnimationTask` 构造时传入 `lifetime`，行为不变
- [ ] 食材添加属性时，属性值正确显示在食材 View 上方

### TASK-2 验收

- [ ] `FloatingAnimType.只放大不缩小` 动画效果正确实现
- [ ] 跳字放大后保持 1.5 倍大小，不缩小
- [ ] 淡出动画正常执行
- [ ] `FloatingAnimType.Normal` 行为不变

### TASK-3 验收 (可选)

- [ ] 珍稀度 (rarity) 显示在左上方，使用 "只放大不缩小" 效果
- [ ] 美味度 (taste) 显示在右上方，使用 "只放大不缩小" 效果
- [ ] 正值显示 `+N`，负值显示 `-N`

---

## 📝 实现清单 (给主程序)

```
[ ] 1. 修改 IAnimTask.cs - SpawnTextAnimationTask.Play()
      - 添加 DEFAULT_LIFETIME 常量
      - 使用 ?? 运算符处理 null lifetime

[ ] 2. 修改 FloatingTextInstance.cs - PlayAnim()
      - 提取 animType 到局部变量
      - 调用 DoScaleByType() 替代 DoScalePunch()

[ ] 3. 新增 FloatingTextInstance.cs - DoScaleByType()
      - 实现 switch 语句分发
      - 添加 FloatingAnimType.只放大不缩小 分支

[ ] 4. (可选) 修改 FoodAnimController.cs - OnFoodInstanceAddBaseValueAnim()
      - 替换 SpawnTextAnimationTask 为直接调用 FloatingTextManager
      - 使用 FloatingAnimType.只放大不缩小

[ ] 5. 编译验证 - 确保无编译错误

[ ] 6. 运行时测试 - 在游戏中触发食材属性变化，验证跳字显示
```

---

## 🔗 相关文件索引

| 文件 | 路径 | 修改类型 |
|------|------|----------|
| IAnimTask.cs | [`Assets/Scripts/GamePlay/Animation/IAnimTask.cs`](../Scripts/GamePlay/Animation/IAnimTask.cs) | 修改 |
| FloatingTextInstance.cs | [`Assets/Scripts/UI/FloatingTextNew/FloatingTextInstance.cs`](../Scripts/UI/FloatingTextNew/FloatingTextInstance.cs) | 修改 |
| FloatingTextManager.cs | [`Assets/Scripts/UI/FloatingTextNew/FloatingTextManager.cs`](../Scripts/UI/FloatingTextNew/FloatingTextManager.cs) | 只读参考 |
| FoodAnimController.cs | [`Assets/Scripts/GamePlay/Food/FoodAnimController.cs`](../Scripts/GamePlay/Food/FoodAnimController.cs) | 修改 (可选) |

---

*文档生成时间: 2026-02-14*  
*架构师: The Architect (GLM-5)*
