# 串串天国 (Skewers Heaven) 代码规范指南

## 1. 核心架构 (Architecture)
- **框架**: QFramework (必须严格遵守 MVC/Architecture 分层)
- **异步处理**: UniRx (禁止使用 Coroutine，除非设计到极复杂的帧操作)
- **数据表**: Luban (所有 Config 加载必须通过 DataSystem)
- **动画**：使用DOTween插件制作动画

## 2. 命名约定 (Naming Conventions)
- **变量**: public 属性使用 PascalCase (e.g., `Health`)。
- **Subject**: UniRx 的 Subject 变量必须以 `Subject` 结尾 (e.g., `OnHitSubject`)。
- **UI 组件**: 必须以组件类型结尾 (e.g., `StartGameButton`, `PlayerNameText`)。

## 3. QFramework 特别规定
- **事件发送**: 使用 `TypeEventSystem.Global.Send<T>()`。
- **对于Model与Command**：本项目中聚焦使用System和Controller，没有涉及到Model和Command的使用，Controller可以直接访问System，System与Controller通信通过事件
- ****

## 4. 偏好与习惯 (My Preferences)
- **注释**: 每个核心方法必须写 `<summary>` 注释。逻辑复杂的代码块内部需要写行内注释，解释“为什么这样做”。
- **括号**: 即使 `if` 只有一行，也必须加 `{ }`。

## 5. 禁忌 (Do Not Use)
- 禁止使用 `SendMessage`。
- 禁止使用 `GameObject.Find` (使用架构内的绑定或引用)。
- 非Controller层（如System）中不允许使用UnityEngine中的Transform、GameObject等依赖（为了Headless Mode，并保证代码整洁干净）
- 禁止使用 Unity协程，异步都使用UniRx
