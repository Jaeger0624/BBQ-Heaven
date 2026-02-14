# 《串串天国》Demo V1 食材技术规范

> **版本**: v1.0
> **创建日期**: 2026-02-14
> **作者**: 主架构师
> **来源设计**: [`DESIGN_Food_DemoV1.md`](DESIGN_Food_DemoV1.md)
> **状态**: 待主程实现

---

## 1. 需求分析

### 1.1 来源设计文档
- 设计文档：[`Assets/Docs/Designs/DESIGN_Food_DemoV1.md`](DESIGN_Food_DemoV1.md)
- 核心需求：实现16种食材的完整效果，覆盖4种食材类型（海鲜、水果、蔬菜、零食）

### 1.2 食材清单

| 类型 | 食材 | 触发时机 | 实现状态 |
|------|------|----------|----------|
| 海鲜 | 🦐 跳跳虾 | 烤串构建后 | 可复用 |
| 海鲜 | 🦑 章鱼丸子 | 放上烤串前 | **需新增** |
| 海鲜 | 🐟 滑溜溜泥鳅 | 被选中时 | 可复用 |
| 海鲜 | 🦀 横行螃蟹 | 放上棋盘时 + 碰撞时 | 可复用 |
| 水果 | 🍋 柠檬 | 被选中时 | 可复用 |
| 水果 | 🍇 葡萄串 | 烤串构建后 | 可复用 |
| 水果 | 🍑 水蜜桃 | 放上烤串前 | 可复用 |
| 水果 | 🍉 西瓜 | 烤串构建后 | **需新增Info** |
| 蔬菜 | 🥕 胡萝卜 | 被选中时 | 可复用 |
| 蔬菜 | 🌱 豆芽 | 放上棋盘时 | **需新增** |
| 蔬菜 | 🥔 土豆 | 烤串构建后 | 可复用 |
| 蔬菜 | 🌽 玉米 | 被碰撞时 + 放上烤串前 | **需新增** |
| 零食 | 🥨 椒盐卷饼 | 无 | 无需实现 |
| 零食 | 🍬 幸运糖果 | 放上烤串前 | 可复用 |
| 零食 | 🍪 曲奇饼干 | 放上烤串前 | 已实现 |
| 零食 | 🍫 黑巧克力 | 放上烤串前 | 已实现 |
| 零食 | 🍒 樱桃炸弹 | 放上烤串前 | 已实现 |
| 零食 | 🍬 QQ糖 | 碰撞时 + 被碰撞时 | 已实现 |

---

## 2. 已有实现复用清单

### 2.1 可直接复用的GA

| 需求 | 复用方案 | 参考配置 |
|------|----------|----------|
| 跳跳虾：移动+成长 | `GA_方向位移` + `GA_为食材加属性` | `fooddatatable.json` - shrimp |
| 滑溜泥鳅：滑行 | `GA_滑行` | `GA_滑行.cs` |
| 横行螃蟹：选择方向滑行 | `GA_效果选择` + `GA_滑行` | `GA_效果选择.cs` |
| 柠檬：相邻同类加成 | `GA_重复执行GA` + `DV_周围食材数` | `fooddatatable.json` - lemon |
| 葡萄串：数量检定 | `Condition_DV检测` + `DV_串上某食材数量` | `fooddatatable.json` - potato |
| 水蜜桃：获得Buff | `GA_获得Buff` | `fooddatatable.json` - chocolate |
| 胡萝卜：空位条件成长 | `Condition_食材周围空位` | `fooddatatable.json` - beer |
| 土豆：唯一性检定 | `Condition_DV检测` + `DV_串上某食材数量` | `fooddatatable.json` - potato |
| 幸运糖果：随机效果 | `GA_随机触发` | `fooddatatable.json` - apple |

### 2.2 已实现的食材（无需修改）

| 食材ID | 食材名称 | 状态 |
|--------|----------|------|
| cookie | 曲奇饼干 | ✅ 已实现 |
| chocolate | 黑巧克力 | ✅ 已实现 |
| cherry_bomb | 樱桃炸弹 | ✅ 已实现 |
| QQ_sugar | QQ糖 | ✅ 已实现 |

---

## 3. 新增内容

### 3.1 新增GA

#### 3.1.1 GA_聚拢食材

**用途**：章鱼丸子 - 将周围食材向自己方向移动

**Luban配置字段**：
```yaml
GA_聚拢食材:
  info: GetFoodInstancesInfo    # 目标食材选择策略
  range: int                    # 聚拢范围（默认1）
```

**运行时伪代码**：
```csharp
public partial class GA_聚拢食材 : GameAction
{
    public override void Execute(object sender, List<object> param)
    {
        // 1. 获取发起者
        FoodInstance origin = sender as FoodInstance;
        if (origin == null) return;
        
        // 2. 获取周围食材
        var surroundingFoods = origin.GetSurroundingFoodInstances(Range);
        
        // 3. 将每个食材向origin方向移动1格
        foreach (var food in surroundingFoods)
        {
            // 计算从food到origin的方向
            var direction = GetDirectionToward(food, origin);
            // 移动食材
            this.GetSystem<IBoardEntitySystem>().Mover.MoveEntity(food, direction, 1);
        }
    }
}
```

---

#### 3.1.2 GA_概率触发

**用途**：豆芽 - 50%概率触发效果

**Luban配置字段**：
```yaml
GA_概率触发:
  probability: int              # 概率百分比（0-100）
  action: GameAction            # 成功时执行的GA
```

**运行时伪代码**：
```csharp
public partial class GA_概率触发 : GameAction
{
    public override void Execute(object sender, List<object> param)
    {
        // 使用System.Random或Unity.Random
        int roll = UnityEngine.Random.Range(1, 101);
        if (roll <= Probability)
        {
            // 触发子动作
            this.GetSystem<IGASystem>().TriggerReaction(Action, sender, param);
        }
    }
}
```

---

#### 3.1.3 GA_添加状态层

**用途**：玉米 - 被碰撞时添加"玉米粒"层数

**Luban配置字段**：
```yaml
GA_添加状态层:
  stateID: string               # 状态ID（如 "corn_kernel"）
  maxLayers: int                # 最大层数（如 3）
  info: GetFoodInstancesInfo    # 目标食材
```

**运行时伪代码**：
```csharp
public partial class GA_添加状态层 : GameAction
{
    public override void Execute(object sender, List<object> param)
    {
        FoodInstance origin = sender as FoodInstance;
        List<FoodInstance> targets = Info.GetFoodInstances(origin, param);
        
        foreach (var food in targets)
        {
            // 获取或创建状态层数
            int currentLayers = food.GetStateLayers(StateID);
            if (currentLayers < MaxLayers)
            {
                food.SetStateLayers(StateID, currentLayers + 1);
            }
        }
    }
}
```

---

#### 3.1.4 GA_根据状态层加属性

**用途**：玉米 - 根据玉米粒层数加属性

**Luban配置字段**：
```yaml
GA_根据状态层加属性:
  stateID: string               # 状态ID
  rarityPerLayer: DynamicValue  # 每层珍稀值
  tastePerLayer: DynamicValue   # 每层美味值
  info: GetFoodInstancesInfo    # 目标食材
```

**运行时伪代码**：
```csharp
public partial class GA_根据状态层加属性 : GameAction
{
    public override void Execute(object sender, List<object> param)
    {
        FoodInstance origin = sender as FoodInstance;
        List<FoodInstance> targets = Info.GetFoodInstances(origin, param);
        
        int rarityPerLayer = RarityPerLayer.GetValue(sender, param);
        int tastePerLayer = TastePerLayer.GetValue(sender, param);
        
        foreach (var food in targets)
        {
            int layers = food.GetStateLayers(StateID);
            food.rarity += rarityPerLayer * layers;
            food.taste += tastePerLayer * layers;
        }
    }
}
```

---

### 3.2 新增DV

#### 3.2.1 DV_食材状态层数

**用途**：获取食材的某个状态层数

**Luban配置字段**：
```yaml
DV_食材状态层数:
  stateID: string               # 状态ID
```

**运行时伪代码**：
```csharp
public partial class DV_食材状态层数 : DynamicValue
{
    public override int GetValue(object target, List<object> param)
    {
        if (target is FoodInstance food)
        {
            return food.GetStateLayers(StateID);
        }
        return 0;
    }
}
```

---

### 3.3 新增Info策略

#### 3.3.1 GetFoodInstancesInfo 扩展 - 按类型筛选

**用途**：西瓜 - 选择串上所有水果类食材

**现有Strategy枚举**：
- 0: 指定位置
- 1: 周围食材
- 2: 串上食材
- 3: 自身

**新增Strategy**：
- 4: 串上某类型食材

**配置示例**：
```json
{
  "strategy": 4,
  "value": { "$type": "DV_值", "number": 1 },
  "types": [2]  // FoodType.Fruit = 2
}
```

**运行时伪代码**：
```csharp
// 在 GetFoodInstancesInfo.GetFoodInstances() 中添加
case 4: // 串上某类型食材
{
    BBQProcessContext context = param?.FirstOrDefault() as BBQProcessContext;
    if (context == null) return new List<FoodInstance>();
    
    return context.targetBBQ.foodInstances
        .Where(f => Types.Contains(f.food.foodData.Type))
        .ToList();
}
```

---

### 3.4 FoodInstance 扩展 - 状态层数系统

**用途**：支持玉米等食材的状态层数机制

**扩展内容**：
```csharp
// 在 FoodInstance 类中添加
private Dictionary<string, int> stateLayers = new Dictionary<string, int>();

public int GetStateLayers(string stateID)
{
    return stateLayers.TryGetValue(stateID, out int layers) ? layers : 0;
}

public void SetStateLayers(string stateID, int layers)
{
    stateLayers[stateID] = layers;
}
```

---

## 4. Luban配置清单

### 4.1 需要配置的Excel表

| 表名 | 路径 | 配置内容 |
|------|------|----------|
| 食材表 | `Config/Datas/food_data.xlsx` | 16种食材的基础数据和CGA配置 |
| GA定义 | `Config/Datas/__beans__.xlsx` | 新增4个GA的定义 |

### 4.2 新增GA的Luban Bean定义

```yaml
# 在 __beans__.xlsx 中添加以下Bean定义

GA_聚拢食材:
  parent: GameAction
  fields:
    - name: info
      type: GetFoodInstancesInfo
    - name: range
      type: int

GA_概率触发:
  parent: GameAction
  fields:
    - name: probability
      type: int
    - name: action
      type: GameAction

GA_添加状态层:
  parent: GameAction
  fields:
    - name: stateID
      type: string
    - name: maxLayers
      type: int
    - name: info
      type: GetFoodInstancesInfo

GA_根据状态层加属性:
  parent: GameAction
  fields:
    - name: stateID
      type: string
    - name: rarityPerLayer
      type: DynamicValue
    - name: tastePerLayer
      type: DynamicValue
    - name: info
      type: GetFoodInstancesInfo
```

### 4.3 新增DV的Luban Bean定义

```yaml
DV_食材状态层数:
  parent: DynamicValue
  fields:
    - name: stateID
      type: string
```

---

## 5. 食材配置示例

### 5.1 跳跳虾 (shrimp)

```json
{
  "ID": "shrimp",
  "name": "跳跳虾",
  "type": 8,
  "rarity": 4,
  "taste": 4,
  "sprite": "Shrimp",
  "effectDescription": "完成串串后往随机方向移动并+2/+2",
  "CGAs": [],
  "SEs": [
    {
      "$type": "SE_监听事件",
      "evt": 3,
      "actions": [
        {
          "ID": "1",
          "conditions": [],
          "actions": [
            {
              "$type": "GA_方向位移",
              "info": { "strategy": 3, "value": { "$type": "DV_值", "number": 1 }, "types": [1] },
              "dir": 6,
              "value": { "$type": "DV_值", "number": 1 }
            }
          ]
        },
        {
          "ID": "2",
          "conditions": [],
          "actions": [
            {
              "$type": "GA_为食材加属性",
              "rarity": { "$type": "DV_值", "number": 2 },
              "taste": { "$type": "DV_值", "number": 2 },
              "info": { "strategy": 3, "value": { "$type": "DV_值", "number": 1 }, "types": [1] }
            }
          ]
        }
      ]
    }
  ],
  "rank": 1,
  "timeCost": 1
}
```

### 5.2 章鱼丸子 (octopus_ball)

```json
{
  "ID": "octopus_ball",
  "name": "章鱼丸子",
  "type": 8,
  "rarity": 5,
  "taste": 5,
  "sprite": "OctopusBall",
  "effectDescription": "放上烤串前，将周围1格内食材向自己聚拢",
  "CGAs": [
    {
      "type": 1,
      "action": {
        "ID": "1",
        "conditions": [],
        "actions": [
          {
            "$type": "GA_聚拢食材",
            "info": { "strategy": 3, "value": { "$type": "DV_值", "number": 1 }, "types": [1] },
            "range": 1
          }
        ]
      }
    }
  ],
  "SEs": [],
  "rank": 1,
  "timeCost": 2
}
```

### 5.3 豆芽 (bean_sprout)

```json
{
  "ID": "bean_sprout",
  "name": "豆芽",
  "type": 1,
  "rarity": 1,
  "taste": 1,
  "sprite": "BeanSprout",
  "effectDescription": "放上棋盘时，50%概率在相邻空位生成一个豆芽",
  "CGAs": [
    {
      "type": 0,
      "action": {
        "ID": "1",
        "conditions": [],
        "actions": [
          {
            "$type": "GA_概率触发",
            "probability": 50,
            "action": {
              "$type": "GA_创建实体",
              "foodID": "bean_sprout",
              "info": { "strategy": 0, "value": { "$type": "DV_值", "number": 1 }, "types": [0] }
            }
          }
        ]
      }
    }
  ],
  "SEs": [],
  "rank": 1,
  "timeCost": 1
}
```

### 5.4 玉米 (corn)

```json
{
  "ID": "corn",
  "name": "玉米",
  "type": 1,
  "rarity": 3,
  "taste": 3,
  "sprite": "Corn",
  "effectDescription": "被碰撞时获得玉米粒(最多3层)，放上烤串前每层+3/+2",
  "CGAs": [
    {
      "type": 5,
      "action": {
        "ID": "1",
        "conditions": [],
        "actions": [
          {
            "$type": "GA_添加状态层",
            "stateID": "corn_kernel",
            "maxLayers": 3,
            "info": { "strategy": 3, "value": { "$type": "DV_值", "number": 1 }, "types": [1] }
          }
        ]
      }
    },
    {
      "type": 1,
      "action": {
        "ID": "2",
        "conditions": [],
        "actions": [
          {
            "$type": "GA_根据状态层加属性",
            "stateID": "corn_kernel",
            "rarityPerLayer": { "$type": "DV_值", "number": 2 },
            "tastePerLayer": { "$type": "DV_值", "number": 3 },
            "info": { "strategy": 3, "value": { "$type": "DV_值", "number": 1 }, "types": [1] }
          }
        ]
      }
    }
  ],
  "SEs": [],
  "rank": 3,
  "timeCost": 2
}
```

### 5.5 西瓜 (watermelon)

```json
{
  "ID": "watermelon",
  "name": "西瓜",
  "type": 2,
  "rarity": 8,
  "taste": 12,
  "sprite": "Watermelon",
  "effectDescription": "烤串构建后，串上所有水果+4/+2",
  "CGAs": [
    {
      "type": 2,
      "action": {
        "ID": "1",
        "conditions": [],
        "actions": [
          {
            "$type": "GA_为食材加属性",
            "rarity": { "$type": "DV_值", "number": 2 },
            "taste": { "$type": "DV_值", "number": 4 },
            "info": { "strategy": 4, "value": { "$type": "DV_值", "number": 1 }, "types": [2] }
          }
        ]
      }
    }
  ],
  "SEs": [],
  "rank": 3,
  "timeCost": 3
}
```

---

## 6. 实现步骤

### 6.1 主程实现顺序

1. **扩展FoodInstance** - 添加状态层数系统
2. **扩展GetFoodInstancesInfo** - 添加Strategy=4（按类型筛选）
3. **实现新增GA** - 按以下顺序：
   - `GA_概率触发`（豆芽依赖）
   - `GA_聚拢食材`（章鱼丸子依赖）
   - `GA_添加状态层`（玉米依赖）
   - `GA_根据状态层加属性`（玉米依赖）
4. **实现新增DV** - `DV_食材状态层数`
5. **配置Luban表** - 在Excel中配置所有食材
6. **执行生成脚本** - 运行 `Config/gen.bat`
7. **配置JSON数据** - 在 `fooddatatable.json` 中添加食材配置
8. **测试验证** - 逐一测试每种食材效果

### 6.2 文件修改清单

| 文件路径 | 修改内容 |
|----------|----------|
| `Config/Datas/__beans__.xlsx` | 添加4个新GA和1个新DV的Bean定义 |
| `Config/Datas/food_data.xlsx` | 配置16种食材数据 |
| `Assets/Scripts/Luban_Extra/GameAction/GameActionExtra.cs` | 实现4个新GA |
| `Assets/Scripts/Luban_Extra/DynamicValue.cs` | 实现1个新DV |
| `Assets/Scripts/Luban_Extra/Info/GetFoodInstancesInfo.cs` | 扩展Strategy=4 |
| `Assets/Scripts/GamePlay/Food/FoodInstance.cs` | 添加状态层数系统 |
| `Assets/Resources/Config/fooddatatable.json` | 添加食材配置 |

---

## 7. 注意事项

### 7.1 技术约束
- 所有GA必须继承自 `GameAction` 基类
- 所有DV必须继承自 `DynamicValue` 基类
- 遵循 [`.cursorrules`](../../.cursorrules) 中的编码规范
- 参考 [`CODING_STYLE_SAMPLE.md`](../CODING_STYLE_SAMPLE.md) 的代码风格

### 7.2 复用优先
- 优先使用已实现的GA，避免重复开发
- `GA_创建实体` 已存在，豆芽可直接复用
- `GA_滑行` 已存在，泥鳅和螃蟹可直接复用

### 7.3 测试要点
- 章鱼丸子的聚拢效果需要测试边界情况
- 豆芽的复制需要标记"复制品无效果"
- 玉米的层数上限需要正确限制
- 西瓜的类型筛选需要正确识别水果类型

---

*文档版本：v1.0*
*创建日期：2026-02-14*
*作者：主架构师*
