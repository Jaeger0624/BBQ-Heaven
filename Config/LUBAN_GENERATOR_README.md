# Luban Configuration Table Generator

## 简介

这是一个用于生成和更新符合 Luban 配置表规范的 Excel 文件的 Python 脚本。通过 JSON Schema 定义数据结构，脚本会自动生成或增量更新所有必要的配置文件。

## 功能特性

- ✅ 支持生成 Bean 定义文件 (`__beans__.xlsx`)
- ✅ 支持生成 Enum 定义文件 (`__enums__.xlsx`)
- ✅ 支持生成 Table 定义文件 (`__tables__.xlsx`)
- ✅ 支持生成数据表文件（包含实际数据的 Excel 文件）
- ✅ **支持增量更新现有数据表（新增、修改、删除记录）**
- ✅ 自动处理 Luban 特有的 Excel 格式规范
- ✅ 支持复杂的嵌套数据结构
- ✅ 提供示例 Schema 生成功能

## 两种运行模式

### 1. 生成模式（默认）
完全覆盖现有文件，生成全新的配置表。适用于：
- 首次创建配置表
- 需要完全重建表结构

### 2. 更新模式（推荐）
在现有表基础上进行增量修改，保留未修改的数据。适用于：
- 修改现有配置表
- 添加新记录
- 更新特定字段
- 删除不需要的记录

## 安装依赖

脚本依赖 `openpyxl` 库来操作 Excel 文件：

```bash
pip install openpyxl
```

## 使用方法

### 1. 生成示例 Schema

首先，你可以生成一个示例 Schema 文件来了解格式：

```bash
python Config/luban_table_generator.py --sample my_schema.json
```

这会创建一个包含示例 Bean、Enum 和 Table 定义的 JSON 文件。

### 2. 编写 Schema 文件

Schema 文件是一个 JSON 文件，包含以下三个主要部分：

#### 2.1 Beans（数据结构定义）

```json
{
  "beans": [
    {
      "name": "CardData",
      "comment": "卡牌数据结构",
      "parent": null,           // 可选：父类名称
      "valueType": null,        // 可选：值类型
      "sep": null,              // 可选：分隔符
      "alias": null,            // 可选：别名
      "group": null,            // 可选：分组
      "tags": null,             // 可选：标签
      "fields": [
        {
          "name": "ID",
          "type": "string",
          "comment": "唯一标识符",
          "alias": null,        // 可选
          "group": null,        // 可选
          "tags": null,         // 可选
          "variants": null      // 可选
        },
        {
          "name": "name",
          "type": "string",
          "comment": "卡牌名称"
        },
        {
          "name": "cost",
          "type": "int",
          "comment": "费用"
        }
      ]
    }
  ]
}
```

#### 2.2 Enums（枚举定义）

```json
{
  "enums": [
    {
      "name": "CardTargetType",
      "comment": "卡牌目标类型",
      "alias": null,            // 可选
      "tags": null,             // 可选
      "items": [
        {
          "name": "None",
          "value": 0,
          "comment": "无目标",
          "alias": null,        // 可选
          "tags": null          // 可选
        },
        {
          "name": "Single",
          "value": 1,
          "comment": "单体目标"
        },
        {
          "name": "All",
          "value": 2,
          "comment": "全体目标"
        }
      ]
    }
  ]
}
```

#### 2.3 Tables（数据表定义）

```json
{
  "tables": [
    {
      "name": "TbCard",
      "value": "CardData",          // 关联的 Bean 类型
      "index": "ID",                // 主键字段
      "mode": "map",                // 表模式: one, map, list
      "comment": "卡牌配置表",
      "group": null,                // 可选：分组
      "tags": null,                 // 可选：标签
      "inputFile": "CardDatas/CardData.xlsx",  // 数据文件路径
      "fields": [                   // 字段定义（可选，如果不提供会从数据推断）
        {
          "name": "ID",
          "type": "string",
          "comment": "卡牌ID"
        },
        {
          "name": "name",
          "type": "string",
          "comment": "卡牌名称"
        }
      ],
      "data": [                     // 实际数据
        {
          "ID": "card_001",
          "name": "火球术",
          "cost": 3
        },
        {
          "ID": "card_002",
          "name": "治愈术",
          "cost": 2
        }
      ]
    }
  ]
}
```

### 3. 生成配置表

#### 3.1 生成模式（完全覆盖）

使用 Schema 文件生成全新的配置表：

```bash
# 使用默认输出目录 (Config/Datas)
python Config/luban_table_generator.py my_schema.json

# 指定输出目录
python Config/luban_table_generator.py my_schema.json Config/MyOutput
```

#### 3.2 更新模式（增量修改，推荐）

在现有表基础上进行增量修改：

```bash
# 更新现有配置表
python Config/luban_table_generator.py --update my_schema.json Config/Datas
```

**更新模式的优势：**
- ✅ 保留未修改的数据
- ✅ 只更新指定的记录
- ✅ 可以新增、修改、删除记录
- ✅ 更安全，不会意外丢失数据

#### 3.3 更新操作类型

在 Schema 的 `tables` 中，可以通过 `operations` 字段定义更新操作：

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
          "comment": "更新火球术的费用",
          "data": {
            "cost": 4,
            "description": "对目标造成150点伤害"
          }
        },
        {
          "action": "delete",
          "key": "card_002",
          "comment": "删除治愈术"
        }
      ],
      "data": [
        {
          "ID": "card_003",
          "name": "冰冻术",
          "cost": 2
        }
      ]
    }
  ]
}
```

**操作类型说明：**

| 操作 | 说明 | 必需字段 |
|------|------|----------|
| `update` | 更新现有记录 | `action`, `key`, `data` |
| `delete` | 删除记录 | `action`, `key` |
| `data` 数组 | 新增记录 | 包含主键的完整记录 |

**更新示例：**

```json
{
  "action": "update",
  "key": "card_001",
  "comment": "更新火球术（可选）",
  "data": {
    "cost": 4,
    "description": "新的描述"
  }
}
```

**删除示例：**

```json
{
  "action": "delete",
  "key": "card_002",
  "comment": "删除治愈术（可选）"
}
```

**新增示例（通过 data 数组）：**

```json
{
  "data": [
    {
      "ID": "card_new",
      "name": "新卡牌",
      "cost": 1
    }
  ]
}
```

### 4. 生成的文件结构

脚本会生成以下文件：

```
Config/Datas/
├── __beans__.xlsx          # Bean 定义文件
├── __enums__.xlsx          # Enum 定义文件
├── __tables__.xlsx         # Table 定义文件
├── TbCard/                 # 数据表目录（以表名命名）
│   └── TbCard.xlsx        # 实际数据表
└── TbFood/
    └── TbFood.xlsx
```

## Luban Excel 格式说明

### Bean 定义格式 (`__beans__.xlsx`)

| 行号 | 内容 |
|------|------|
| 1 | `##var` \| `full_name` \| `parent` \| `valueType` \| `sep` \| `alias` \| `comment` \| `group` \| `tags` \| `#` \| `*fields`... |
| 2 | `##var` \| ... \| ... \| ... \| ... \| ... \| ... \| ... \| ... \| ... \| `name` \| `alias` \| `type` \| `group` \| `comment` \| `tags` \| `variants` |
| 3 | `##` \| 中文说明行 |
| 4+ | Bean 定义（每个 Bean 包含多行：声明行 + 字段行） |

### Enum 定义格式 (`__enums__.xlsx`)

| 行号 | 内容 |
|------|------|
| 1 | `##var` \| `name` \| `alias` \| `comment` \| `tags` \| `*items`... |
| 2 | `##var` \| ... \| ... \| ... \| ... \| `name` \| `alias` \| `value` \| `comment` \| `tags` |
| 3 | `##` \| 中文说明行 |
| 4+ | Enum 定义（每个 Enum 包含多行：声明行 + 枚举项行） |

### Table 定义格式 (`__tables__.xlsx`)

| 行号 | 内容 |
|------|------|
| 1 | `##var` \| `name` \| `value` \| `index` \| `mode` \| `group` \| `comment` \| `tags` \| `*inputFiles` |
| 2 | `##var` \| ... \| ... \| ... \| ... \| ... \| ... \| ... \| `文件路径` |
| 3 | `##` \| 中文说明行 |
| 4+ | Table 定义（每行一个 Table） |

### 数据表格式

| 行号 | 内容 |
|------|------|
| 1 | `##var` \| 字段名1 \| 字段名2 \| 字段名3 \| ... |
| 2 | `##var` \| 类型1 \| 类型2 \| 类型3 \| ... |
| 3 | `##` \| 注释1 \| 注释2 \| 注释3 \| ... |
| 4+ | 数据行 |

## 支持的数据类型

### 基础类型
- `int` - 整数
- `long` - 长整数
- `float` - 浮点数
- `double` - 双精度浮点数
- `bool` - 布尔值
- `string` - 字符串

### 复杂类型
- `list,<类型>` - 列表，例如 `list,int`
- `array,<类型>` - 数组
- `map,<键类型>,<值类型>` - 映射，例如 `map,string,int`
- 自定义 Bean 类型 - 直接使用 Bean 名称
- 自定义 Enum 类型 - 直接使用 Enum 名称

## 完整示例

### Schema 文件 (`game_config.json`)

```json
{
  "beans": [
    {
      "name": "FoodData",
      "comment": "食材数据",
      "fields": [
        {"name": "ID", "type": "string", "comment": "食材ID"},
        {"name": "name", "type": "string", "comment": "食材名称"},
        {"name": "tastiness", "type": "int", "comment": "美味度"},
        {"name": "rarity", "type": "int", "comment": "珍稀度"},
        {"name": "tags", "type": "list,string", "comment": "标签列表"}
      ]
    }
  ],
  "enums": [
    {
      "name": "FoodType",
      "comment": "食材类型",
      "items": [
        {"name": "Meat", "value": 1, "comment": "肉类"},
        {"name": "Vegetable", "value": 2, "comment": "蔬菜"},
        {"name": "Seafood", "value": 3, "comment": "海鲜"}
      ]
    }
  ],
  "tables": [
    {
      "name": "TbFood",
      "value": "FoodData",
      "index": "ID",
      "mode": "map",
      "comment": "食材配置表",
      "inputFile": "FoodDatas/FoodData.xlsx",
      "fields": [
        {"name": "ID", "type": "string", "comment": "食材ID"},
        {"name": "name", "type": "string", "comment": "食材名称"},
        {"name": "tastiness", "type": "int", "comment": "美味度"},
        {"name": "rarity", "type": "int", "comment": "珍稀度"},
        {"name": "tags", "type": "list,string", "comment": "标签"}
      ],
      "data": [
        {
          "ID": "food_001",
          "name": "牛肉",
          "tastiness": 80,
          "rarity": 3,
          "tags": ["Meat", "Fresh"]
        },
        {
          "ID": "food_002",
          "name": "青椒",
          "tastiness": 40,
          "rarity": 1,
          "tags": ["Vegetable"]
        }
      ]
    }
  ]
}
```

### 生成命令

```bash
python Config/luban_table_generator.py game_config.json Config/Datas
```

## 注意事项

1. **编码问题**：脚本使用 UTF-8 编码，确保你的 JSON 文件也是 UTF-8 编码
2. **字段顺序**：Bean 中的字段顺序会保留到生成的 Excel 中
3. **空值处理**：可选字段可以省略或设为 `null`
4. **复杂类型**：列表和字典类型会自动转换为 JSON 字符串存储
5. **文件覆盖**：如果输出文件已存在，会被覆盖

## 与 Luban 工作流集成

生成配置表后，你可以使用 Luban 工具生成代码：

```bash
# Windows
cd Config
gen.bat

# Linux/Mac
cd Config
./gen.sh
```

## 故障排除

### 问题：提示找不到 openpyxl 模块
**解决方案**：运行 `pip install openpyxl`

### 问题：生成的 Excel 文件格式不正确
**解决方案**：检查你的 Schema JSON 格式是否正确，特别是字段类型是否有效

### 问题：Luban 生成代码时报错
**解决方案**：
1. 检查 `luban.conf` 配置是否正确
2. 确保所有引用的类型（Bean、Enum）都已定义
3. 检查数据表中的数据是否符合字段类型定义

## 项目信息

- **项目**：串串天国 (Skewers Heaven)
- **作者**：BBQ Game Project
- **创建日期**：2026-02-15
- **版本**：1.0.0

## 更新日志

### v1.0.0 (2026-02-15)
- ✨ 初始版本发布
- ✅ 支持 Bean、Enum、Table 定义生成
- ✅ 支持数据表生成
- ✅ 提供示例 Schema 生成功能
- 📝 完整的文档和示例
