# 技术方案：FloatingText 跳字系统拓展 & Bug修复

## 📋 需求概述

### 需求1: 拓展跳字动画模板
- **现状**: 系统已有 `FloatingAnimType.Normal` 和 `FloatingAnimType.只放大不缩小` 枚举定义
- **问题**: `只放大不缩小` 动画类型未在 [`FloatingTextInstance.cs`](Assets/Scripts/UI/FloatingTextNew/FloatingTextInstance.cs) 中实现
- **目标**: 实现 "只放大不缩小" 动画效果

### 需求2: 修复 GA_为食材加属性 跳字不显示 Bug
- **现象**: 食材添加属性时，应该在食材 View 上方跳出属性值，但现在看不到
- **根因**: [`SpawnTextAnimationTask.Play()`](Assets/Scripts/GamePlay/Animation/IAnimTask.cs:174) 中，当 `lifetime` 为 null 时不调用 `FloatingTextManager.Instance.Show()`

---

## 🔍 问题分析

### Bug 定位

**调用链路:**
```
GA_为食材加属性.Execute() 
  → SendAnimEvent() 发送 FoodInstanceAddBaseValueEvent
    → FoodAnimController.OnFoodInstanceAddBaseValueAnim() 
      → new SpawnTextAnimationTask(rarityText, 36f, color, position) // 没传 lifetime!
        → SpawnTextAnimationTask.Play() 
          → if (lifetime.HasValue) { Show() } else { 什么都不做! }
```

**关键代码 ([`IAnimTask.cs:174-182`](Assets/Scripts/GamePlay/Animation/IAnimTask.cs:174)):**
```csharp
public override IObservable<Unit> Play()
{
    if (lifetime.HasValue){
        FloatingTextInfo info = new FloatingTextInfo(text, lifetime.Value, color.color, FloatingAnimType.Normal);
        FloatingTextManager.Instance.Show(position, text, color.color, info);
        return Observable.ReturnUnit();
    }
    return Observable.ReturnUnit();  // ❌ Bug: lifetime 为 null 时直接返回，不显示跳字
}
```

---

## 🛠️ 技术方案

### 方案A: 修复 SpawnTextAnimationTask (推荐)

**修改文件:** [`Assets/Scripts/GamePlay/Animation/IAnimTask.cs`](Assets/Scripts/GamePlay/Animation/IAnimTask.cs)

**修改内容:**
```csharp
public class SpawnTextAnimationTask : AbstractAnimTask
{
    // 新增: 默认生命周期
    private const float DEFAULT_LIFETIME = 1.0f;
    
    public override IObservable<Unit> Play()
    {
        // 使用 lifetime ?? DEFAULT_LIFETIME 确保始终有值
        float actualLifetime = lifetime ?? DEFAULT_LIFETIME;
        FloatingTextInfo info = new FloatingTextInfo(text, actualLifetime, color.color, FloatingAnimType.Normal);
        FloatingTextManager.Instance.Show(position, text, color.color, info);
        return Observable.ReturnUnit();
    }
}
```

**优点:**
- 最小改动，只修改一处
- 向后兼容，不影响其他调用

---

### 方案B: 实现 "只放大不缩小" 动画类型

**修改文件:** [`Assets/Scripts/UI/FloatingTextNew/FloatingTextInstance.cs`](Assets/Scripts/UI/FloatingTextNew/FloatingTextInstance.cs)

**修改内容:**

1. 在 `PlayAnim()` 方法中根据 `FloatingAnimType` 选择不同的缩放动画:

```csharp
private void PlayAnim(FloatingTextMode mode, Vector3? targetPos, ScatterMode scatterMode, Action onArrive, FloatingTextInfo info)
{
    // ... 现有代码 ...
    
    switch (mode)
    {
        case FloatingTextMode.Normal:
            Vector2 direction = info?.direction ?? Vector2.up;
            Vector3 t = _rectTransform.position + new Vector3(direction.x * _floatHeight, direction.y * _floatHeight, 0);
            _seq.Join(_rectTransform.DOMove(t, duration).SetEase(Ease.OutSine, 10f, 0.5f).SetLink(gameObject));
            
            // 根据 animType 选择缩放动画
            DoScaleByType(_seq, info?.animType ?? FloatingAnimType.Normal);
            break;
            
        // ... 其他 case ...
    }
}

// 新增方法: 根据类型执行不同的缩放动画
private void DoScaleByType(Sequence seq, FloatingAnimType animType)
{
    switch (animType)
    {
        case FloatingAnimType.Normal:
            DoScalePunch(seq); // 现有: 放大后缩小
            break;
            
        case FloatingAnimType.只放大不缩小:
            // 只放大，保持不放缩
            seq.Insert(0, transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack).SetLink(gameObject));
            // 不添加缩小动画，最终保持 1.5 倍直到淡出
            break;
    }
}
```

---

### 方案C: 为食材加属性使用 "只放大不缩小" 效果 (增强体验)

**修改文件:** [`Assets/Scripts/GamePlay/Food/FoodAnimController.cs`](Assets/Scripts/GamePlay/Food/FoodAnimController.cs)

**修改内容:**

将 `SpawnTextAnimationTask` 替换为直接调用 `FloatingTextManager`:

```csharp
void OnFoodInstanceAddBaseValueAnim(FoodInstanceAddBaseValueEvent e)
{
    IEntityView foodInstanceView = foodController.GetEntityView(e.guid);
    if (foodInstanceView == null) return;

    // 缩放旋转动画
    var scaleTween = new ScaleAnimationTask(foodInstanceView.GO().transform, 1.6f, 0.15f);
    var rotateTween = new RotateAnimationTask(foodInstanceView.GO().transform, 0.25f);

    // 使用 FloatingTextManager 直接显示跳字，使用 "只放大不缩小" 效果
    string rarityText = e.rarity > 0 ? $"+{e.rarity}" : e.rarity.ToString();
    string tasteText = e.taste > 0 ? $"+{e.taste}" : e.taste.ToString();

    Vector3 rarityPos = AnimUtility.GetTextSpawnPosition(foodInstanceView.GO().transform, true);
    Vector3 tastePos = AnimUtility.GetTextSpawnPosition(foodInstanceView.GO().transform, false);

    FloatingTextInfo info = new FloatingTextInfo(rarityText, 1.2f, 
        SettingManager.Instance.DevSettings.AddRarityTextColor, 
        FloatingAnimType.只放大不缩小);

    FloatingTextManager.Instance.Show(rarityPos, rarityText, 
        SettingManager.Instance.DevSettings.AddRarityTextColor, info);

    // taste 同理...
    
    // SFX
    var SFXAnimTask = new PlaySFXAnimationTask("Score 2", 0.02f, 0.2f).SetVolume(0.5f);
    
    var parallelAnimTask = new ParallelAnimTask(new List<IAnimTask>{ scaleTween, rotateTween, SFXAnimTask });
    this.GetSystem<IAnimationSystem>().DirectlyPlay(parallelAnimTask);
}
```

---

## 📊 数值预估

| 参数 | 建议值 | 说明 |
|------|--------|------|
| DEFAULT_LIFETIME | 1.0f | 跳字默认显示时长 |
| 只放大不缩小_最终缩放 | 1.5f | 保持 1.5 倍大小直到淡出 |
| 只放大不缩小_动画时长 | 0.2f | 放大动画时长 |

---

## ✅ 验收标准

### 需求1 验收
- [ ] `FloatingAnimType.只放大不缩小` 动画效果正确实现
- [ ] 跳字放大后保持大小，不缩小，直到淡出消失

### 需求2 验收
- [ ] `GA_为食材加属性` 执行时，食材 View 上方正确显示属性变化值
- [ ] 珍稀度 (rarity) 显示在左上方，美味度 (taste) 显示在右上方
- [ ] 正值显示 `+N`，负值显示 `-N`

---

## 📝 任务清单 (给架构师)

- [ ] 1. 修复 [`SpawnTextAnimationTask.Play()`](Assets/Scripts/GamePlay/Animation/IAnimTask.cs:174) 的 lifetime 为 null 时不显示问题
- [ ] 2. 在 [`FloatingTextInstance.cs`](Assets/Scripts/UI/FloatingTextNew/FloatingTextInstance.cs) 中实现 `FloatingAnimType.只放大不缩小` 动画
- [ ] 3. (可选) 优化 [`FoodAnimController.cs`](Assets/Scripts/GamePlay/Food/FoodAnimController.cs) 使用新的动画类型

---

## 🎯 用户故事

**作为** 玩家  
**我希望** 在食材获得属性加成时，能清晰看到数值变化  
**以便于** 理解游戏机制，获得即时反馈的满足感

**作为** 游戏设计师  
**我希望** 有多种跳字动画模板可选  
**以便于** 为不同场景 (伤害/治疗/增益) 设计差异化的视觉反馈
