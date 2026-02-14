# 串串天国 (Skewers Heaven) - 技术架构文档

> **文档版本**: v1.1
> **最后更新**: 2026-02-14
> **维护者**: 架构师

---

## 📋 目录

1. [技术栈概览](#1-技术栈概览)
2. [核心框架](#2-核心框架)
3. [架构分层](#3-架构分层)
4. [系统清单](#4-系统清单)
5. [数据流与通信](#5-数据流与通信)
6. [代码规范要点](#6-代码规范要点)
7. [开发工作流](#7-开发工作流)

---

## 1. 技术栈概览

### 1.1 核心框架

| 框架 | 用途 | 文档链接 |
|------|------|----------|
| **QFramework** | 架构框架 (MVC/Architecture) | [官方文档](https://qframework.cn) |
| **UniRx** | 响应式编程/异步处理 | [GitHub](https://github.com/neuecc/UniRx) |
| **Reflex** | 依赖注入容器 | [GitHub](https://github.com/gustavopsantos/reflex) |
| **Luban** | 配置表生成工具 | [官方文档](https://luban.doc.code-philosophy.com/) |

### 1.2 UI与动画

| 插件 | 用途 | 文档链接 |
|------|------|----------|
| **Odin Inspector** | Inspector增强 & 序列化 | [官方文档](https://odininspector.com/) |
| **DOTween** | 动画插件 (主力) | [官方网站](http://dotween.demigiant.com/) |
| **PrimeTween** | 高性能动画库 (备选) | [GitHub](https://github.com/KyryloKuzyk/PrimeTween) |
| **UI Effect** | UI特效 (灰度/模糊/发光) | [GitHub](https://github.com/mob-sakai/UIEffect) |
| **UI Particle** | UI粒子特效 | [GitHub](https://github.com/mob-sakai/ParticleEffectForUGUI) |

### 1.3 开发工具

| 工具 | 用途 |
|------|------|
| **Hot Reload** | 运行时代码热更新 |
| **Console Pro** | 增强版控制台 |
| **vHierarchy** | Hierarchy窗口增强 |
| **Unity Skills** | 技能系统框架 |

### 1.4 开发环境

- **引擎**: Unity 2022.3+ (URP)
- **语言**: C# (.NET Standard 2.1)
- **IDE**: Visual Studio / VS Code + ReSharper/Rider

### 1.5 项目特性

- **无头模式 (Headless Mode)**: 支持脱离View快速运行对局逻辑
- **热重载**: 使用 HotReload 插件支持运行时代码更新

> 📖 **详细插件文档**: [PLUGINS_AND_TOOLS.md](PLUGINS_AND_TOOLS.md)

---

## 2. 核心框架

### 2.1 QFramework 架构体系

本项目采用 QFramework 的 Architecture 模式，核心概念：

```
GameArchitecture (架构入口)
    ├── System (系统层 - 业务逻辑)
    ├── Controller (控制器层 - View与System桥梁)
    ├── Model (模型层 - 数据存储) [本项目未使用]
    ├── Command (命令层) [本项目未使用]
    └── Utility (工具层)
```

**本项目特点**:
- 聚焦使用 **System** 和 **Controller**
- Controller 可直接访问 System
- System 与 Controller 通过 **事件** 通信
- 不涉及 Model 和 Command 的使用

### 2.2 UniRx 响应式编程

**替代协程**: 本项目禁止使用 Unity 协程，所有异步操作使用 UniRx

```csharp
// ✅ 正确 - 使用 UniRx
Observable.Timer(TimeSpan.FromSeconds(1f))
    .Subscribe(_ => DoSomething());

// ❌ 错误 - 禁止使用协程
StartCoroutine(MyCoroutine());
```

### 2.3 Luban 配置系统

**数据加载流程**:
```
Excel/JSON 配置 → Luban 生成 → cfg 命名空间 → DataSystem 加载
```

所有配置数据必须通过 `IDataSystem` 访问，禁止直接读取配置文件。

### 2.4 DOTween 动画系统

用于制作游戏内所有动画效果，配合 AnimationSystem 统一管理。

---

## 3. 架构分层

### 3.1 分层原则

```
┌─────────────────────────────────────────┐
│              View (Unity)               │  ← MonoBehaviour, UI
├─────────────────────────────────────────┤
│           Controller 层                  │  ← 处理用户输入，调用System
├─────────────────────────────────────────┤
│             System 层                    │  ← 纯逻辑层，禁止引用 UnityEngine
├─────────────────────────────────────────┤
│           Data (Luban)                  │  ← 配置数据
└─────────────────────────────────────────┘
```

### 3.2 层级职责

| 层级 | 职责 | 可访问 | 禁止访问 |
|------|------|--------|----------|
| **View** | 显示、用户交互 | Controller | 直接访问 System |
| **Controller** | 输入处理、视图更新 | System, View | 业务逻辑实现 |
| **System** | 核心业务逻辑 | 其他 System, Data | UnityEngine (Transform, GameObject 等) |

### 3.3 无头模式 (Headless Mode)

System 层禁止引用 UnityEngine 的原因：
- 支持无渲染环境下的快速对局模拟
- 便于单元测试
- 保持逻辑层纯净

```csharp
// GameArchitecture.cs
public static bool HeadlessMode = false;
```

---

## 4. 系统清单

### 4.1 核心系统

| 系统 | 接口 | 职责 |
|------|------|------|
| **GameSystem** | `IGameSystem` | 单局游戏进程管理（日、月、局结算） |
| **ProcessSystem** | `IProcessSystem` | 流程管理 |
| **TimeSystem** | `ITimeSystem` | 时间推动、事件发布 |
| **ScoreSystem** | `IScoreSystem` | 分数记录与结算 |
| **RandomSystem** | `IRandomSystem` | 随机池系统 |
| **RngSystem** | `IRngSystem` | 随机数生成（种子管理） |

### 4.2 游戏玩法系统

| 系统 | 接口 | 职责 |
|------|------|------|
| **FoodSystem** | `IFoodSystem` | 食材仓储、实例补充 |
| **CardSystem** | `ICardSystem` | 卡牌系统 |
| **BoardSystem** | `IBoardSystem` | 棋盘控制、地块效果 |
| **BoardEntitySystem** | `IBoardEntitySystem` | 棋盘实体管理 |
| **BBQSystem** | `IBBQSystem` | 烧烤构造、属性计算 |
| **StickSystem** | `IStickSystem` | 烤串补充与使用 |
| **CustomerSystem** | `ICustomerSystem` | 顾客来去、Tag、要求刷新 |
| **DealSystem** | `IDealSystem` | 交易结算、单次得分 |

### 4.3 玩家与效果系统

| 系统 | 接口 | 职责 |
|------|------|------|
| **PCSystem** | `IPCSystem` | 玩家角色、主动/被动技能 |
| **MascotSystem** | `IMascotSystem` | 吉祥物系统、全局效果 |
| **GASystem** | `IGASystem` | GameAction 效果触发链管理 |
| **BuffSystem** | `IBuffSystem` | Buff 管理 |

### 4.4 经济与商店系统

| 系统 | 接口 | 职责 |
|------|------|------|
| **EconomySystem** | `IEconomySystem` | 游戏货币管理 |
| **ShopSystem** | `IShopSystem` | 商店内容刷新 |

### 4.5 内容系统

| 系统 | 接口 | 职责 |
|------|------|------|
| **RecipeSystem** | `IRecipeSystem` | 配方提醒、执行与获取 |
| **EncounterSystem** | `IEncounterSystem` | 经营事件管理 |
| **CollectionSystem** | `ICollectionSystem` | 图鉴系统 |

### 4.6 UI与交互系统

| 系统 | 接口 | 职责 |
|------|------|------|
| **SelectorSystem** | `ISelectorSystem` | 高亮、选中、选项创建 |
| **DialogueSystem** | `IDialogueSystem` | 对话系统 |
| **GuideSystem** | `IGuideSystem` | 引导/教程系统 |
| **AnimationSystem** | `IAnimationSystem` | 动画管理 |

### 4.7 基础设施系统

| 系统 | 接口 | 职责 |
|------|------|------|
| **DataSystem** | `IDataSystem` | Luban 配置数据加载 |
| **SaveSystem** | `ISaveSystem` | 存档管理 |
| **BlackboardSystem** | - | 全局黑板（状态共享） |
| **ProxySystem** | `IProxySystem` | 无头模式代理 |

---

## 5. 数据流与通信

### 5.1 事件系统

**事件发送** (System → Controller/其他System):
```csharp
this.SendEvent(new MascotAddedEvent(mascot));
```

**事件监听** (Controller):
```csharp
this.RegisterEvent<MascotAddedEvent>(OnMascotAdded)
    .UnRegisterWhenDisabled(this);
```

### 5.2 事件阶段 (EventStage)

对于复杂流程事件，使用阶段划分：

```csharp
public enum EventStage {
    Before,   // 前置处理
    System,   // 系统处理
    After     // 后置处理
}

// 示例：一天结束事件
this.SendEvent(new EndDayEvent(month, day, EventStage.Before));
this.SendEvent(new EndDayEvent(month, day, EventStage.System));
this.SendEvent(new EndDayEvent(month, day, EventStage.After));
```

### 5.3 数据流向图

```
用户输入
    ↓
Controller (处理输入)
    ↓
System (执行业务逻辑)
    ↓
TypeEventSystem (发送事件)
    ↓
Controller (接收事件，更新View)
    ↓
View (显示变化)
```

---

## 6. 代码规范要点

### 6.1 命名约定

| 类型 | 规范 | 示例 |
|------|------|------|
| 公有属性 | PascalCase | `Health`, `MaxDay` |
| UniRx Subject | 以 `Subject` 结尾 | `OnHitSubject` |
| UI 组件 | 以组件类型结尾 | `StartGameButton`, `PlayerNameText` |
| 事件类 | 以 `Event` 结尾 | `MascotAddedEvent` |

### 6.2 注释规范

```csharp
/// <summary>
/// 添加吉祥物到系统中
/// </summary>
/// <param name="mascotID">吉祥物配置ID</param>
public void AddMascot(string mascotID)
{
    // 检查是否已存在（是否堆叠）
    // 这里使用 TryGetValue 避免二次查找
    if (mascots.TryGetValue(mascotID, out Mascot mascot))
    {
        // ... 逻辑说明
    }
}
```

### 6.3 禁止事项

| 禁止 | 原因 | 替代方案 |
|------|------|----------|
| `SendMessage` | 性能差、难以追踪 | 使用 TypeEventSystem |
| `GameObject.Find` | 性能差、耦合高 | 使用架构绑定或引用 |
| Unity 协程 | 不符合项目规范 | 使用 UniRx |
| System 层引用 `Transform`/`GameObject` | 破坏无头模式 | 通过事件通知 Controller |

### 6.4 代码风格

- **括号**: 即使单行语句也必须加 `{}`
- **接口**: 所有 System 必须定义接口 (如 `IGameSystem`)
- **存档**: 实现 `ISavable` 接口

---

## 7. 开发工作流

### 7.1 新增系统流程

1. 在 `GameArchitecture.cs` 中注册系统
2. 创建接口 `IXxxSystem : ISystem`
3. 实现类 `XxxSystem : AbstractSystem, IXxxSystem`
4. 如需存档，实现 `ISavable`

### 7.2 新增 Controller 流程

1. 创建 MonoBehaviour 类实现 `IController`
2. 实现 `GetArchitecture()` 返回 `GameArchitecture.Interface`
3. 在 `OnEnable` 中注册事件
4. 使用 `UnRegisterWhenDisabled(this)` 自动注销

### 7.3 配置表修改流程

1. 修改 `Config/Datas/` 下的 Excel 文件
2. 运行 `Config/gen.bat` 生成代码
3. 在 `Luban_Extra/` 中编写扩展方法
4. 通过 `DataSystem` 访问数据

### 7.4 文件结构

```
Assets/Scripts/GamePlay/
├── BBQ/           # 烧烤系统
├── Board/         # 棋盘系统
├── Card/          # 卡牌系统
├── Customer/      # 顾客系统
├── Deal/          # 交易系统
├── Economy/       # 经济系统
├── Food/          # 食材系统
├── Game/          # 游戏进程
├── GameAction/    # GameAction 效果
├── Guide&Dialogue/# 引导与对话
├── Mascot/        # 吉祥物
├── PC/            # 玩家角色
├── Process/       # 流程管理
├── Recipe/        # 配方
├── Score/         # 分数
├── Seed/          # 种子
├── Shop/          # 商店
├── Stick/         # 烤串
├── Time/          # 时间
├── UI/            # UI控制器
└── ...
```

---

## 附录

### A. 相关文档

- [`.cursorrules`](../.cursorrules) - 代码规范完整版
- [`CODING_STYLE_SAMPLE.md`](./CODING_STYLE_SAMPLE.md) - 代码模板示例
- [`GAME_DESIGN.md`](./GAME_DESIGN.md) - 游戏设计文档
- [`EDITOR_AUTOMATION.md`](./EDITOR_AUTOMATION.md) - 编辑器自动化

### B. 快速参考

**获取系统**:
```csharp
this.GetSystem<IGameSystem>()
```

**发送事件**:
```csharp
this.SendEvent(new MyEvent());
```

**注册事件**:
```csharp
this.RegisterEvent<MyEvent>(OnMyEvent);
```

**获取配置数据**:
```csharp
this.GetSystem<IDataSystem>().GetXxxData(id);
```

---

## C. 关键资源位置

### 配置与数据

| 资源 | 位置 | 说明 |
|------|------|------|
| **Luban配置表** | `Config/Datas/*.xlsx` | Excel格式的数据配置 |
| **Luban生成脚本** | `Config/gen.bat` | 执行后生成CS代码和JSON |
| **生成的CS类** | `Assets/Luban/*.cs` | Luban自动生成，**禁止手动修改** |
| **运行时扩展** | `Assets/Scripts/Luban_Extra/` | 对生成类的逻辑扩展 |
| **配置JSON** | `Assets/Resources/Config/*.json` | 运行时读取的配置数据，可查阅已配置内容 |

### 核心扩展目录

| 目录 | 说明 |
|------|------|
| `Assets/Scripts/Luban_Extra/GameAction/` | GA运行时实现 |
| `Assets/Scripts/Luban_Extra/Info/` | 获取食材/格子/顾客信息的策略实现 |
| `Assets/Scripts/Luban_Extra/Other/` | 其他扩展（Direction、Dialogue等） |

### 设计文档

| 目录 | 说明 |
|------|------|
| `Assets/Docs/Designs/` | 具体功能/内容的设计文档 |
| `Assets/Docs/` | 项目整体文档（架构、规范、工作流等） |

### 查阅已配置内容

要了解项目中已配置的GA、Condition、DV等，可以查看：
1. **JSON配置文件**: `Assets/Resources/Config/*.json` - 包含所有已配置的数据
2. **Luban生成类**: `Assets/Luban/GA_*.cs`, `Assets/Luban/Condition_*.cs`, `Assets/Luban/DV_*.cs`
3. **运行时实现**: `Assets/Scripts/Luban_Extra/GameAction/GameActionExtra.cs`

> 📖 **详细工作流**: [WORKFLOW.md](WORKFLOW.md)

---

*本文档由架构师维护，如有疑问请联系技术负责人。*
