# 串串天国 (Skewers Heaven) - 插件与工具文档

> **文档版本**: v1.0  
> **最后更新**: 2026-02-14  
> **维护者**: 架构师

---

## 📋 目录

1. [核心框架](#1-核心框架)
2. [UI与动画插件](#2-ui与动画插件)
3. [开发工具](#3-开发工具)
4. [视觉增强插件](#4-视觉增强插件)
5. [Unity内置模块](#5-unity内置模块)
6. [使用规范](#6-使用规范)

---

## 1. 核心框架

### 1.1 QFramework
| 属性 | 值 |
|------|-----|
| **路径** | `Assets/Plugins/QFramework/` |
| **用途** | 架构框架 (MVC/Architecture) |
| **文档** | [官方文档](https://qframework.cn) |

**核心用法**:
- [`AbstractSystem`](Assets/Plugins/QFramework/Framework/Scripts/QFramework.cs) - 业务逻辑层
- [`AbstractController`](Assets/Plugins/QFramework/Framework/Scripts/QFramework.cs) - 控制器层
- [`this.SendEvent<T>()`](Assets/Plugins/QFramework/Framework/Scripts/QFramework.cs) - 事件发送
- [`this.RegisterEvent<T>()`](Assets/Plugins/QFramework/Framework/Scripts/QFramework.cs) - 事件注册

### 1.2 UniRx
| 属性 | 值 |
|------|-----|
| **路径** | `Assets/Plugins/UniRx/` |
| **用途** | 响应式编程/异步处理 |
| **文档** | [GitHub](https://github.com/neuecc/UniRx) |

**核心用法**:
```csharp
// 替代协程 - 延迟执行
Observable.Timer(TimeSpan.FromSeconds(1f))
    .Subscribe(_ => DoSomething())
    .AddTo(gameObject); // 自动销毁

// 事件流
Observable.EveryUpdate()
    .Where(_ => Input.GetKeyDown(KeyCode.Space))
    .Subscribe(_ => OnSpacePressed());

// AsyncSubject - 异步结果
AsyncSubject<Unit> subject = new AsyncSubject<Unit>();
subject.OnNext(Unit.Default);
subject.OnCompleted();
```

**⚠️ 禁止事项**: 本项目禁止使用 Unity 协程 (`StartCoroutine`)，必须使用 UniRx。

### 1.3 Reflex (DI容器)
| 属性 | 值 |
|------|-----|
| **路径** | Package: `com.gustavopsantos.reflex` |
| **用途** | 依赖注入容器 |
| **文档** | [GitHub](https://github.com/gustavopsantos/reflex) |

**核心用法**:
```csharp
// 安装器 - 注册依赖
public class ProjectInstaller : MonoBehaviour, IInstaller
{
    public void Install(ContainerDescriptor descriptor)
    {
        descriptor.AddSingleton<IGameService, GameService>();
        descriptor.AddTransient<IEnemy, Enemy>();
    }
}

// 注入依赖
public class PlayerController : MonoBehaviour
{
    [Inject] private IGameService _gameService;
    
    // 或通过构造函数注入
    [Inject]
    public void Construct(IGameService gameService)
    {
        _gameService = gameService;
    }
}
```

**项目中的使用**:
- [`ProjectInstaller`](Assets/Scripts/Base/Installers/ProjectInstaller.cs) - 全局依赖注册
- `[Inject]` 属性 - 字段/方法注入
- [`ContainerDescriptor`](Assets/Scripts/Base/Installers/ProjectInstaller.cs) - 服务注册

### 1.4 Luban
| 属性 | 值 |
|------|-----|
| **路径** | `Config/` (配置源), `Assets/Luban/` (生成输出) |
| **用途** | 配置表生成工具 |
| **文档** | [官方文档](https://luban.doc.code-philosophy.com/) |

**数据流**:
```
Excel/JSON → Luban生成 → cfg命名空间 → DataSystem加载
```

---

## 2. UI与动画插件

### 2.1 Odin Inspector & Serializer
| 属性 | 值 |
|------|-----|
| **路径** | `Assets/Plugins/Sirenix/Odin Inspector/` |
| **用途** | Inspector增强 & 序列化 |
| **文档** | [官方文档](https://odininspector.com/) |

**核心特性**:

#### Inspector增强
```csharp
using Sirenix.OdinInspector;

public class Example : MonoBehaviour
{
    [Title("基础设置")]
    [LabelText("玩家名称")]
    public string playerName;
    
    [Button("执行测试")]
    private void TestButton()
    {
        Debug.Log("Button clicked!");
    }
    
    [ShowInInspector]
    private int readOnlyValue => CalculateValue();
    
    [FoldoutGroup("高级设置")]
    [PropertyOrder(1)]
    public float advancedSetting;
    
    [EnumToggleButtons]
    public GameState currentState;
}
```

#### 序列化增强
```csharp
using Sirenix.Serialization;

// 支持序列化Dictionary、HashSet等Unity原生不支持的类型
[OdinSerialize]
public Dictionary<string, int> itemDictionary;

// 注意：使用Odin序列化的类需要继承SerializedMonoBehaviour
public class CustomClass : SerializedMonoBehaviour
{
    [OdinSerialize]
    private Dictionary<Material, int> materialCount;
}
```

**项目中的常用属性**:
| 属性 | 用途 |
|------|------|
| `[Button]` | Inspector中添加按钮 |
| `[ShowInInspector]` | 显示私有字段/属性 |
| `[LabelText]` | 自定义标签文本 |
| `[Title]` | 添加标题 |
| `[FoldoutGroup]` | 折叠分组 |
| `[PropertyOrder]` | 属性显示顺序 |
| `[EnumToggleButtons]` | 枚举显示为按钮组 |

### 2.2 DOTween
| 属性 | 值 |
|------|-----|
| **路径** | `Assets/Plugins/Demigiant/DOTween/` |
| **用途** | 动画插件 |
| **文档** | [官方网站](http://dotween.demigiant.com/) |

**核心用法**:
```csharp
using DG.Tweening;

// 基础动画
transform.DOMove(targetPos, 1f).SetEase(Ease.OutSine);
transform.DOScale(1.5f, 0.5f).SetLoops(2, LoopType.Yoyo);
canvasGroup.DOFade(0, 0.3f);

// Sequence动画
Sequence seq = DOTween.Sequence();
seq.Append(transform.DOScale(1.2f, 0.2f));
seq.Join(transform.DORotate(new Vector3(0, 180, 0), 0.2f));
seq.AppendCallback(() => Debug.Log("Complete"));

// 链式调用与安全设置
transform.DOScale(1f, 0.12f)
    .SetEase(Ease.OutSine)
    .SetUpdate(true)           // 忽略TimeScale
    .SetLink(gameObject);      // 自动销毁绑定

// 数值动画
DOTween.To(() => currentValue, x => currentValue = x, targetValue, duration);
```

**项目扩展方法** (见 [`DOTweenExtentions.cs`](Assets/Scripts/Utility/Animation/DOTweenExtentions.cs)):
- `.UnScaledKill(gameObject)` - 忽略TimeScale的Kill

### 2.3 PrimeTween
| 属性 | 值 |
|------|-----|
| **路径** | `Assets/Plugins/PrimeTween/` |
| **用途** | 高性能动画库 (DOTween替代品) |
| **文档** | [GitHub](https://github.com/KyryloKuzyk/PrimeTween) |

**特点**:
- 性能优于DOTween
- 结构体设计，零GC分配
- 可作为DOTween的高频动画替代

**注意**: 项目中主要使用DOTween，PrimeTween作为备选方案。

### 2.4 UI Effect (Coffee UI Effect)
| 属性 | 值 |
|------|-----|
| **路径** | Package: `com.coffee.ui-effect` |
| **用途** | UI特效 (灰度、模糊、发光等) |
| **文档** | [GitHub](https://github.com/mob-sakai/UIEffect) |

**常用效果**:
- 灰度效果 (技能冷却/不可用状态)
- 模糊效果
- 发光效果

### 2.5 UI Particle (Coffee UI Particle)
| 属性 | 值 |
|------|-----|
| **路径** | Package: `com.coffee.ui-particle` |
| **用途** | UI粒子特效 |
| **文档** | [GitHub](https://github.com/mob-sakai/ParticleEffectForUGUI) |

**用途**: 在UI Canvas中正确渲染粒子特效。

---

## 3. 开发工具

### 3.1 Hot Reload
| 属性 | 值 |
|------|-----|
| **路径** | Package: `com.singularitygroup.hotreload` |
| **用途** | 运行时代码热更新 |
| **文档** | [官方网站](https://hotreload.net/) |

**功能**: 无需重新编译即可在运行时更新代码逻辑。

### 3.2 Console Pro
| 属性 | 值 |
|------|-----|
| **路径** | `Assets/Plugins/ConsolePro/` |
| **用途** | 增强版控制台 |
| **文档** | [Asset Store](https://assetstore.unity.com/packages/tools/utilities/console-pro-41318) |

**特性**:
- 日志过滤
- 远程调试
- 性能分析

### 3.3 vHierarchy
| 属性 | 值 |
|------|-----|
| **路径** | `Assets/Plugins/vHierarchy/` |
| **用途** | Hierarchy窗口增强 |
| **文档** | [Asset Store](https://assetstore.unity.com/packages/tools/utilities/vhierarchy-265326) |

**特性**:
- 自定义Hierarchy图标
- 快速组件操作
- 颜色分组

### 3.4 Unity Skills
| 属性 | 值 |
|------|-----|
| **路径** | Package: `com.besty.unity-skills` |
| **用途** | 技能系统框架 |
| **文档** | [GitHub](https://github.com/Besty0728/Unity-Skills) |

**相关文档**: [`Assets/Docs/UnitySkill/SKILL.md`](Assets/Docs/UnitySkill/SKILL.md)

### 3.5 Cursor IDE Support
| 属性 | 值 |
|------|-----|
| **路径** | Package: `com.boxqkrtm.ide.cursor` |
| **用途** | Cursor IDE集成 |

---

## 4. 视觉增强插件

### 4.1 All In 1 Sprite Shader
| 属性 | 值 |
|------|-----|
| **路径** | `Assets/Plugins/AllIn1SpriteShader/` |
| **用途** | Sprite着色器效果集 |
| **文档** | [Asset Store](https://assetstore.unity.com/packages/vfx/shaders/all-in-1-sprite-shader-262860) |

**效果**: 发光、描边、扭曲、颜色调整等。

### 4.2 Feel (Nice Vibrations)
| 属性 | 值 |
|------|-----|
| **路径** | `Assets/Plugins/Feel/NiceVibrations/` |
| **用途** | 移动设备触觉反馈 |
| **文档** | [Asset Store](https://assetstore.unity.com/packages/tools/particles-effects/feel-183450) |

**用途**: 震动反馈、触觉效果。

### 4.3 True Shadow
| 属性 | 值 |
|------|-----|
| **路径** | `Assets/Plugins/Le Tai's Asset/TrueShadow/` |
| **用途** | UI真实阴影效果 |
| **文档** | [Asset Store](https://assetstore.unity.com/packages/tools/particles-effects/true-shadow-173722) |

### 4.4 Colourful Hierarchy
| 属性 | 值 |
|------|-----|
| **路径** | `Assets/Plugins/M Studio/Colourful Hierarchy Category GameObject/` |
| **用途** | Hierarchy颜色分组 |

---

## 5. Unity内置模块

### 5.1 核心模块
| 模块 | 用途 |
|------|------|
| **Input System** | 新输入系统 |
| **Cinemachine** | 智能相机系统 |
| **AI Navigation** | 导航网格 |
| **URP** | 通用渲染管线 |
| **2D Feature** | 2D游戏功能集 |
| **Test Framework** | 单元测试框架 |

### 5.2 多人游戏
| 模块 | 用途 |
|------|------|
| **Multiplayer Center** | 多人游戏中心 |
| **Gameplay Storytelling** | 故事叙述工具 |

---

## 6. 使用规范

### 6.1 插件选择优先级

```
动画: DOTween (主) > PrimeTween (高频场景备选)
序列化: Odin Serializer (复杂类型) > Unity原生 (简单类型)
DI: Reflex
异步: UniRx (禁止协程)
配置: Luban
```

### 6.2 Odin使用规范

**推荐**:
```csharp
// ✅ 使用Button标记测试方法
[Button("测试")]
private void TestFunction() { }

// ✅ 使用ShowInInspector显示计算属性
[ShowInInspector]
public int CalculatedValue => _value * 2;

// ✅ 使用OdinSerialize序列化复杂类型
[OdinSerialize]
public Dictionary<string, List<int>> complexData;
```

**不推荐**:
```csharp
// ❌ 过度使用Odin属性导致Inspector臃肿
[FoldoutGroup("A")]
[FoldoutGroup("A.B")]
[FoldoutGroup("A.B.C")]
public int tooDeep;

// ❌ 简单类型使用Odin序列化 (增加开销)
[OdinSerialize]
public int simpleValue; // 应使用 public int simpleValue;
```

### 6.3 DOTween使用规范

**必须**:
```csharp
// ✅ 设置SetLink确保对象销毁时动画也销毁
transform.DOScale(1f, 0.5f).SetLink(gameObject);

// ✅ UI动画使用SetUpdate(true)忽略TimeScale
canvasGroup.DOFade(0, 0.3f).SetUpdate(true);

// ✅ 使用SetEase指定缓动曲线
transform.DOMove(pos, 1f).SetEase(Ease.OutSine);
```

**禁止**:
```csharp
// ❌ 忘记SetLink导致内存泄漏
transform.DOScale(1f, 0.5f); // 危险！

// ❌ 不指定Ease使用默认线性
transform.DOMove(pos, 1f); // 应指定Ease
```

## 📚 相关文档

- [技术架构文档](ARCHITECTURE.md)
- [代码规范](CODING_STYLE.md)
- [代码示例](CODING_STYLE_SAMPLE.md)
- [游戏设计文档](GAME_DESIGN.md)

---

> **更新日志**:
> - 2026-02-14: 初始版本，整理所有插件和工具
