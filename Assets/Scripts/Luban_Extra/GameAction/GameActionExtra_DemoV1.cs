// // Demo V1 食材相关的 GameAction 扩展
// // 包含：GA_聚拢食材、GA_概率触发、GA_添加状态层、GA_根据状态层加属性

// using System.Collections.Generic;
// using System.Linq;
// using cfg;
// using QFramework;
// using UniRx;
// using UnityEngine;

// namespace cfg
// {
//     #region Demo V1 食材 GA

//     /// <summary>
//     /// GA_聚拢食材：章鱼丸子使用
//     /// 将周围食材向自己方向移动
//     /// </summary>
//     public partial class GA_聚拢食材 : GameAction
//     {
//         private IAnimPlayer animPlayer = null;
//         private List<MoveEntityEvent> moveEvents = new List<MoveEntityEvent>();

//         public GA_聚拢食材(GetFoodInstancesInfo info, int range)
//         {
//             this.Info = info;
//             this.Range = range;
//         }

//         public override GameAction Clone() => new GA_聚拢食材(Info, Range);

//         public override void Execute(object sender, List<object> param)
//         {
//             animPlayer = null;
//             moveEvents.Clear();

//             // 1. 获取发起者（章鱼丸子自己）
//             FoodInstance origin = sender as FoodInstance;
//             if (origin == null)
//             {
//                 Debug.LogError("[GA_聚拢食材] 发送者不是FoodInstance");
//                 return;
//             }

//             // 2. 获取周围食材（使用Info策略获取目标）
//             List<FoodInstance> surroundingFoods = Info.GetFoodInstances(origin, param);
//             if (surroundingFoods == null || surroundingFoods.Count == 0)
//             {
//                 // Debug.Log("[GA_聚拢食材] 周围没有食材");
//                 return;
//             }

//             // 3. 将每个食材向origin方向移动1格
//             IBoardSystem boardSystem = this.GetSystem<IBoardSystem>();
//             foreach (var food in surroundingFoods)
//             {
//                 // 计算从food到origin的方向
//                 Vector2Int direction = GetDirectionToward(food.position, origin.position);
//                 if (direction == Vector2Int.zero) continue; // 已经相邻，跳过

//                 // 获取目标格子（向origin方向移动1格）
//                 BoardCell targetCell = boardSystem.GetCell(origin.position - direction);
//                 if (targetCell == null || targetCell.instanceGuid != null) continue; // 格子不存在或已被占用

//                 // 执行移动
//                 this.GetSystem<IBoardEntitySystem>().Mover.PlaceEntity(food, targetCell);
//                 moveEvents.Add(new MoveEntityEvent(targetCell.position, food.position, food, direction));
//             }

//             // 4. 设置动画播放器
//             if (sender is IAnimPlayer newAnimPlayer)
//             {
//                 this.animPlayer = newAnimPlayer;
//             }
//         }

//         /// <summary>
//         /// 计算从from到to的方向向量（单位向量）
//         /// </summary>
//         private Vector2Int GetDirectionToward(Vector2Int from, Vector2Int to)
//         {
//             Vector2Int diff = to - from;
//             // 归一化为单位方向（只取一个方向）
//             if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
//             {
//                 return new Vector2Int((int)Mathf.Sign(diff.x), 0);
//             }
//             else if (diff.y != 0)
//             {
//                 return new Vector2Int(0, (int)Mathf.Sign(diff.y));
//             }
//             return Vector2Int.zero;
//         }

//         private void SendAnims()
//         {
//             foreach (var evt in moveEvents)
//             {
//                 if (evt == null) continue;
//                 this.SendEvent(evt);
//             }
//         }

//         public override IAnimTask GetAnimTask()
//         {
//             if (moveEvents.Count == 0) return new EmptyAnimTask();

//             IAnimTask anim = animPlayer != null ? new SequenceAnimTask(new List<IAnimTask>{
//                 AnimationConverter.Convert(animPlayer, "common"),
//                 new ActionAnimTask(() => SendAnims()),
//                 new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
//             }) : new SequenceAnimTask(new List<IAnimTask>{
//                 new ActionAnimTask(() => SendAnims()),
//                 new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
//             });
//             return anim;
//         }
//     }

//     /// <summary>
//     /// GA_概率触发：豆芽使用
//     /// 按指定概率触发子动作
//     /// </summary>
//     public partial class GA_概率触发 : GameAction
//     {
//         private bool triggered = false;

//         public GA_概率触发(int probability, GameAction action)
//         {
//             this.Probability = probability;
//             this.Action = action != null ? action.Clone() : null;
//         }

//         public override GameAction Clone() => new GA_概率触发(Probability, Action);

//         public override void Execute(object sender, List<object> param)
//         {
//             triggered = false;

//             if (Action == null)
//             {
//                 Debug.LogError("[GA_概率触发] Action为空");
//                 return;
//             }

//             // 使用随机数判断是否触发
//             int roll = UnityEngine.Random.Range(1, 101); // 1-100
//             if (roll <= Probability)
//             {
//                 triggered = true;
//                 // 触发子动作
//                 this.GetSystem<IGASystem>().TriggerReaction(Action, sender, param);
//                 // Debug.Log($"[GA_概率触发] 触发成功 (roll={roll}, probability={Probability})");
//             }
//             // else
//             // {
//             //     Debug.Log($"[GA_概率触发] 触发失败 (roll={roll}, probability={Probability})");
//             // }
//         }

//         public override IAnimTask GetAnimTask()
//         {
//             // 概率触发本身没有动画，由子Action的动画组成
//             return new EmptyAnimTask();
//         }
//     }

//     /// <summary>
//     /// GA_添加状态层：玉米使用
//     /// 被碰撞时添加状态层数（如"玉米粒"）
//     /// </summary>
//     public partial class GA_添加状态层 : GameAction
//     {
//         private IAnimPlayer animPlayer = null;
//         private List<FoodInstance> affectedFoods = new List<FoodInstance>();

//         public GA_添加状态层(string stateID, int maxLayers, GetFoodInstancesInfo info)
//         {
//             this.StateID = stateID;
//             this.MaxLayers = maxLayers;
//             this.Info = info;
//         }

//         public override GameAction Clone() => new GA_添加状态层(StateID, MaxLayers, Info);

//         public override void Execute(object sender, List<object> param)
//         {
//             animPlayer = null;
//             affectedFoods.Clear();

//             // 1. 获取发起者
//             FoodInstance origin = sender as FoodInstance;

//             // 2. 获取目标食材
//             List<FoodInstance> targets = Info.GetFoodInstances(origin, param);
//             if (targets == null || targets.Count == 0) return;

//             // 3. 为每个目标添加状态层
//             foreach (var food in targets)
//             {
//                 int currentLayers = food.GetStateLayers(StateID);
//                 if (currentLayers < MaxLayers)
//                 {
//                     food.SetStateLayers(StateID, currentLayers + 1);
//                     affectedFoods.Add(food);
//                     // Debug.Log($"[GA_添加状态层] {food.name} 获得状态层 {StateID}: {currentLayers + 1}/{MaxLayers}");
//                 }
//             }

//             // 4. 设置动画播放器
//             if (sender is IAnimPlayer newAnimPlayer)
//             {
//                 this.animPlayer = newAnimPlayer;
//             }
//         }

//         private void SendAnims()
//         {
//             foreach (var food in affectedFoods)
//             {
//                 this.SendEvent(new FoodInstanceViewAnimEvent(food.guid));
//             }
//         }

//         public override IAnimTask GetAnimTask()
//         {
//             if (affectedFoods.Count == 0) return new EmptyAnimTask();

//             IAnimTask anim = animPlayer != null ? new SequenceAnimTask(new List<IAnimTask>{
//                 AnimationConverter.Convert(animPlayer, "common"),
//                 new ActionAnimTask(() => SendAnims()),
//                 new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
//             }) : new SequenceAnimTask(new List<IAnimTask>{
//                 new ActionAnimTask(() => SendAnims()),
//                 new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
//             });
//             return anim;
//         }
//     }

//     /// <summary>
//     /// GA_根据状态层加属性：玉米使用
//     /// 根据状态层数增加食材的珍稀值和美味值
//     /// </summary>
//     public partial class GA_根据状态层加属性 : GameAction
//     {
//         private IAnimPlayer animPlayer = null;
//         private List<FoodInstanceAddBaseValueEvent> evts = new List<FoodInstanceAddBaseValueEvent>();

//         public GA_根据状态层加属性(string stateID, DynamicValue rarityPerLayer, DynamicValue tastePerLayer, GetFoodInstancesInfo info)
//         {
//             this.StateID = stateID;
//             this.RarityPerLayer = rarityPerLayer;
//             this.TastePerLayer = tastePerLayer;
//             this.Info = info;
//         }

//         public override GameAction Clone() => new GA_根据状态层加属性(StateID, RarityPerLayer, TastePerLayer, Info);

//         public override void Execute(object sender, List<object> param)
//         {
//             animPlayer = null;
//             evts.Clear();

//             // 1. 获取发起者
//             FoodInstance origin = sender as FoodInstance;

//             // 2. 获取目标食材
//             List<FoodInstance> targets = Info.GetFoodInstances(origin, param);
//             if (targets == null || targets.Count == 0) return;

//             // 3. 获取每层属性值
//             int rarityPerLayer = RarityPerLayer.GetValue(sender, param);
//             int tastePerLayer = TastePerLayer.GetValue(sender, param);

//             // 4. 根据层数为每个目标加属性
//             foreach (var food in targets)
//             {
//                 int layers = food.GetStateLayers(StateID);
//                 if (layers > 0)
//                 {
//                     int rarityBonus = rarityPerLayer * layers;
//                     int tasteBonus = tastePerLayer * layers;

//                     food.rarity += rarityBonus;
//                     food.taste += tasteBonus;

//                     evts.Add(new FoodInstanceAddBaseValueEvent(food.guid, rarityBonus, tasteBonus));
//                     // Debug.Log($"[GA_根据状态层加属性] {food.name} 层数={layers}, +{rarityBonus}/+{tasteBonus}");
//                 }
//             }

//             // 5. 设置动画播放器
//             if (sender is IAnimPlayer newAnimPlayer)
//             {
//                 this.animPlayer = newAnimPlayer;
//             }
//         }

//         private void SendAnims()
//         {
//             foreach (var evt in evts)
//             {
//                 this.SendEvent(evt);
//             }
//         }

//         public override IAnimTask GetAnimTask()
//         {
//             if (evts.Count == 0) return new EmptyAnimTask();

//             IAnimTask anim = animPlayer != null ? new SequenceAnimTask(new List<IAnimTask>{
//                 AnimationConverter.Convert(animPlayer, "common"),
//                 new ActionAnimTask(() => SendAnims()),
//                 new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
//             }) : new SequenceAnimTask(new List<IAnimTask>{
//                 new ActionAnimTask(() => SendAnims()),
//                 new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
//             });
//             return anim;
//         }
//     }

//     #endregion
// }
