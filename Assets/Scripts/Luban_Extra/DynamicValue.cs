using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

namespace cfg{
    public abstract partial class DynamicValue : ICanGetSystem{
        public DynamicValue(){}
        public abstract int GetValue(object target, List<object> param);
        public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    }

    public partial class DV_值 : DynamicValue {
        public DV_值(int number){
            this.Number = number;
        }
        public override int GetValue(object target, List<object> param)
        {
            return Number;
        }
    }
    public partial class DV_周围食材数 : DynamicValue {
        public override int GetValue(object target, List<object> param)
        {
            if (target == null || !(target is FoodInstance foodInstance)){
                Debug.LogError($"DV_周围食材数只能用于FoodInstance，当前类型为{target?.GetType()}");
                return 0;
            }   

            if (string.IsNullOrEmpty(Id))
            {
                int res = foodInstance.GetSurroundingFoodInstances().Count;
                // Debug.Log($"【DV_周围食材数】食材实例: {foodInstance.name}, 周围食材数: {res}");
                return res;
            }
            else{
                // 获取指定ID的食材周围食材数
                List<FoodInstance> surroundingFoodInstances = foodInstance.GetSurroundingFoodInstances().Where(x => x.food.foodData.ID == Id).ToList();   
                int res = surroundingFoodInstances.Count;
                // Debug.Log($"【DV_周围食材数】食材实例: {foodInstance.name}, 周围{Id}食材数: {res}");
                return res;
            }
        }
    }

    public partial class DV_周围空位数 : DynamicValue {
        public override int GetValue(object target, List<object> param)
        {
            if (target == null || !(target is FoodInstance foodInstance)){
                Debug.LogError($"DV_周围空位数只能用于FoodInstance，当前类型为{target?.GetType()}");
                return 0;
            }

            int res = foodInstance.GetSurroundingEmptyCellsCount();
            // Debug.Log($"【DV_周围空位数】食材实例: {foodInstance.name}, 周围空位数: {res}");
            return res;
        }
    }

    public partial class DV_当前构建烤串食材数量 : DynamicValue {
        public override int GetValue(object target, List<object> param)
        {
            
            // 读取上下文
            BBQProcessContext context = param?.FirstOrDefault() as BBQProcessContext;
            BBQPreview preview = param?.FirstOrDefault() as BBQPreview;
            if (context == null && preview == null){
                Debug.LogError("上下文或预览为空");
                return 0;
            }

            if (context != null){
                BBQ currentBBQ = context.targetBBQ;
                if (currentBBQ == null) {Debug.LogError("当前烧烤为空"); return 0;}
                return currentBBQ.foodInstances.Count;
            }
            else{
                int res = 0;
                foreach (BoardCell boardCell in preview.boardCells){
                    if (boardCell.instanceGuid != null){
                        BoardEntity boardEntity = this.GetSystem<IBoardEntitySystem>().GetEntity(boardCell.instanceGuid);
                        if (boardEntity != null && boardEntity is FoodInstance foodInstance){
                            res++;
                        }
                    }
                }
                return res;
            }
        }
    }

    public partial class DV_食材实例数量 : DynamicValue {
        public override int GetValue(object target, List<object> param)
        {
            
            int res = this.GetSystem<IFoodSystem>().GetFoodInstances().Where(x => x.Value.state == FoodInstanceState.棋盘上).Count();
            return res;
        }
    }

    public partial class DV_售卖食材数量 : DynamicValue {
        public override int GetValue(object target, List<object> param)
        {
            // 读取上下文
            DealContext context = param?.FirstOrDefault() as DealContext;
            if (context == null){
                Debug.LogError("上下文为空");
                return 0;
            }
            return context.targetBBQ.foodInstances.Count;
        }
    }
}