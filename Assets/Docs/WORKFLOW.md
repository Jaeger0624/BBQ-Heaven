# 《串串天国》开发工作流规范

## 概述

本文档定义了项目内容开发的标准工作流程，确保策划、架构师、主程三个角色之间的高效协作。

---

## 角色职责

### 🎨 主策划 (The Designer)
- **职责**：负责玩法设计和内容设计
- **输出**：设计文档（包含用户故事、核心循环、数值预估）
- **关注点**：趣味性、自洽性、用户体验
- **不负责**：技术实现细节、代码编写

### 🏗️ 主架构师 (The Architect)
- **职责**：将策划设计转化为技术方案
- **输出**：技术规范文档（包含Luban配置、伪代码、集成点）
- **关注点**：系统架构、代码规范、可维护性
- **必须熟悉**：
  - 项目结构（[`ARCHITECTURE.md`](ARCHITECTURE.md)）
  - 代码规范（[`.cursorrules`](../.cursorrules)）
  - 已有的Luban配置（`Assets/Luban/`下的生成类）
  - 已有的GA实现（`Assets/Scripts/Luban_Extra/GameAction/`）
  - 已有的配置数据（`Assets/Resources/Config/*.json`）

### 💻 主程 (The Coder)
- **职责**：根据技术方案实现代码
- **输出**：
  1. Luban配置表（`Config/Datas/`下的Excel文件）
  2. 执行Luban生成脚本（`Config/gen.bat`）
  3. 运行时逻辑实现（`Assets/Scripts/Luban_Extra/`）
- **关注点**：代码质量、性能、正确性

---

## 标准工作流

```
┌─────────────────┐
│   主策划 (Designer)   │
│  设计玩法/内容    │
└────────┬────────┘
         │ 设计文档
         ▼
┌─────────────────┐
│  主架构师 (Architect)  │
│  分析已有实现     │
│  设计技术方案     │
└────────┬────────┘
         │ 技术规范
         ▼
┌─────────────────┐
│   主程 (Coder)      │
│  配置Luban表      │
│  执行生成脚本     │
│  实现运行时逻辑   │
└─────────────────┘
```

---

## 关键资源位置

### 配置相关
| 资源 | 位置 | 说明 |
|------|------|------|
| Luban配置表 | `Config/Datas/*.xlsx` | Excel格式的数据配置 |
| Luban生成脚本 | `Config/gen.bat` | 执行后生成CS代码 |
| 生成的CS类 | `Assets/Luban/*.cs` | Luban自动生成，不要手动修改 |
| 运行时扩展 | `Assets/Scripts/Luban_Extra/` | 对生成类的逻辑扩展 |
| 配置JSON | `Assets/Resources/Config/*.json` | 运行时读取的配置数据 |

### 文档相关
| 文档 | 位置 | 说明 |
|------|------|------|
| 游戏设计 | `Assets/Docs/GAME_DESIGN.md` | 整体玩法设计 |
| 架构文档 | `Assets/Docs/ARCHITECTURE.md` | 技术架构说明 |
| 代码规范 | `Assets/.cursorrules` | 编码规范和禁忌 |
| 代码示例 | `Assets/Docs/CODING_STYLE_SAMPLE.md` | 代码风格参考 |

---

## 已实现的GA清单

> ⚠️ 架构师在设计技术方案时，必须先查阅此清单，优先复用已有GA

### 游戏动作 (GameAction)

| GA名称 | 说明 | 配置文件参考 |
|--------|------|--------------|
| `GA_直接修改当前烧烤值` | 修改串串的美味/珍稀值 | `fooddatatable.json` - apple |
| `GA_为食材加属性` | 为指定食材加属性 | `fooddatatable.json` - cookie, QQ_sugar |
| `GA_随机触发` | 随机执行一个子动作 | `fooddatatable.json` - apple |
| `GA_重复执行GA` | 重复执行子动作N次 | `fooddatatable.json` - cookie, lemon |
| `GA_方向位移` | 使食材向指定方向移动 | `fooddatatable.json` - shrimp |
| `GA_定点爆破` | 将周围实体弹开 | `fooddatatable.json` - cherry_bomb |
| `GA_获得Buff` | 为食材添加Buff | `fooddatatable.json` - chocolate |
| `GA_补充食材` | 抽取食材卡 | - |
| `GA_添加食材到烤串` | 将食材添加到烤串 | - |
| `GA_使食材获得GA` | 动态为食材添加GA | - |
| `GA_滑行` | 使食材滑行直到遇到障碍 | `GA_滑行.cs` |
| `GA_食材位移` | 使食材移动到指定位置 | `GA_食材位移.cs` |
| `GA_食材换位` | 交换两个食材位置 | `GA_食材换位.cs` |
| `GA_食材随机冲锋` | 随机方向冲锋 | `GA_食材随机冲锋.cs` |
| `GA_创建实体` | 创建新的食材实例 | `GA_创建实体.cs` |
| `GA_效果选择` | 提供选项供玩家选择 | `GA_效果选择.cs` |

### 条件 (Condition)

| 条件名称 | 说明 | 配置文件参考 |
|----------|------|--------------|
| `Condition_食材周围空位` | 检测周围空位数量 | `fooddatatable.json` - beer |
| `Condition_位于首尾` | 检测是否在串首/尾 | `fooddatatable.json` - orio |
| `Condition_DV检测` | 通用动态值比较 | `fooddatatable.json` - potato |

### 动态值 (DynamicValue)

| DV名称 | 说明 | 配置文件参考 |
|--------|------|--------------|
| `DV_值` | 固定数值 | 通用 |
| `DV_周围食材数` | 周围指定食材数量 | `fooddatatable.json` - lemon |
| `DV_串上某食材数量` | 串上指定食材数量 | `fooddatatable.json` - potato |
| `DV_当前串空位数` | 当前串的空位数量 | - |
| `DV_当前构建烤串食材数量` | 当前串上食材总数 | - |
| `DV_周围空位数` | 周围空位数量 | - |
| `DV_周围食材数` | 周围食材数量 | - |
| `DV_回文串` | 回文检测 | - |
| `DV_食材实例数量` | 棋盘上食材实例数量 | - |
| `DV_售卖食材数量` | 售卖的食材数量 | - |

### 食材GA触发时机 (FoodGAType)

| 触发时机 | 值 | 说明 |
|----------|-----|------|
| `放上棋盘时` | 0 | 食材被放置到棋盘时触发 |
| `放上烤串前` | 1 | 食材被添加到串串之前触发 |
| `烤串构建后` | 2 | 整个串串构建完成后触发 |
| `被选中时` | 3 | 食材被玩家选中时触发 |
| `碰撞时` | 4 | 食材主动碰撞其他食材时触发 |
| `被碰撞时` | 5 | 食材被其他食材碰撞时触发 |

---

## 设计文档模板

策划在设计新内容时，应使用以下模板：

```markdown
# [功能名称]设计方案

## 用户故事
> 作为一名[角色]，我希望[功能]，以便[收益]

## 核心循环
1. [步骤1]
2. [步骤2]
3. [步骤3]

## 数值预估
| 属性 | 数值 |
|------|------|
| 基础值 | X |
| 成长值 | Y |

## 效果描述
- **【触发时机】**：效果描述

## GA需求（供架构师参考）
| 触发时机 | 效果描述 |
|----------|----------|
| XXX时 | XXX |
```

---

## 技术规范模板

架构师在输出技术方案时，应使用以下模板：

```markdown
# [功能名称]技术规范

## 1. 需求分析
- 来源设计文档：[链接]
- 核心需求：[摘要]

## 2. 已有实现复用
| 需求 | 复用方案 | 来源 |
|------|----------|------|
| XXX | 使用GA_XXX | fooddatatable.json - xxx |

## 3. 新增内容

### 3.1 新增GA
| GA名称 | Luban配置 | 运行时类 |
|--------|-----------|----------|
| GA_XXX | 字段说明 | 伪代码 |

### 3.2 新增Condition
| 条件名称 | Luban配置 | 运行时类 |
|----------|-----------|----------|

### 3.3 新增DV
| DV名称 | Luban配置 | 运行时类 |
|--------|-----------|----------|

## 4. Luban配置清单
- [ ] 配置表：`Config/Datas/XXX.xlsx`
- [ ] 字段：XXX, YYY, ZZZ

## 5. 实现步骤
1. **使用配置表生成器**创建Luban配置（见下方说明）
2. 执行`Config/gen.bat`生成代码
3. 在`Assets/Scripts/Luban_Extra/GameAction/`实现XXX
4. 测试验证
```

---

## Luban配置表生成工具

> ⚠️ **重要**：所有Luban配置表的修改和创建**必须**使用配置表生成工具，禁止手动编辑Excel文件！

### 工具位置
- **脚本路径**：`Config/luban_table_generator.py`
- **文档说明**：`Config/LUBAN_GENERATOR_README.md`

### 使用流程

1. **创建Schema文件**（JSON格式）
   ```bash
   # 生成示例Schema
   python Config/luban_table_generator.py --sample my_schema.json
   ```

2. **编辑Schema文件**
   - 定义Beans（数据结构）
   - 定义Enums（枚举）
   - 定义Tables（数据表）
   - 填充数据或更新操作

3. **生成或更新配置表**
   
   **推荐：使用更新模式（增量修改）**
   ```bash
   # 在原表基础上增量修改（安全，不会丢失数据）
   python Config/luban_table_generator.py --update my_schema.json Config/Datas
   ```
   
   **仅在需要时：使用生成模式（完全覆盖）**
   ```bash
   # 生成新表（会覆盖现有文件，需要用户批准）
   python Config/luban_table_generator.py my_schema.json Config/Datas
   ```

### 更新模式操作类型

在Schema中可以通过 `operations` 字段定义增量操作：

```json
{
  "tables": [
    {
      "name": "TbCard",
      "index": "ID",
      "operations": [
        {
          "action": "update",
          "key": "card_001",
          "data": {"cost": 4}
        },
        {
          "action": "delete",
          "key": "card_002"
        }
      ],
      "data": [
        {"ID": "card_new", "name": "新卡牌"}
      ]
    }
  ]
}
```

**支持的操作：**
- `update`: 更新现有记录的指定字段
- `delete`: 删除指定记录
- `data` 数组: 新增记录

### 为什么必须使用工具？

1. **格式规范**：自动保证Excel格式符合Luban规范
2. **避免错误**：防止手动编辑导致的格式错误
3. **版本控制**：JSON Schema更容易进行版本对比
4. **可追溯性**：清晰记录每次配置变更的内容
5. **安全性**：更新模式不会意外丢失数据

### 注意事项

- 🔴 **禁止**直接编辑 `Config/Datas/` 下的Excel文件
- ✅ **必须**通过工具生成或更新配置表
- 📝 **建议**将Schema文件也提交到版本控制
- ⚠️ **推荐**优先使用 `--update` 模式，避免覆盖现有数据
- ⚠️ **警告**：生成模式（不带 `--update`）会覆盖现有文件，需要用户批准

---

## 注意事项

1. **策划不写代码**：策划只负责内容和玩法设计，不涉及技术实现
2. **架构师必须查重**：在设计技术方案前，必须先查阅已有实现，避免重复开发
3. **主程按规范实现**：主程必须按照架构师的技术规范实现，如有问题及时反馈
4. **文档同步更新**：任何变更都需要同步更新相关文档

---

*文档版本：v1.0*
*创建日期：2026-02-14*
