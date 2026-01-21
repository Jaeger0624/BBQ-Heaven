using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

namespace cfg{
    public class DynamicValue_当前售卖中食材数量 : DynamicValue {
        public override int GetTypeId() => 14214123;
        private FoodType foodType;
        public DynamicValue_当前售卖中食材数量(FoodType foodType){
            this.foodType = foodType;
        }
        public override int GetValue(object target, List<object> param)
        {
            DealContext context = param?.FirstOrDefault() as DealContext;
            if (context == null){
                Debug.LogError("上下文为空");
                return 0;
            }
            if (foodType == FoodType.无) return context.BBQ.foodInstances.Count;
            return context.BBQ.foodInstances.Count(x => x.food.foodType == foodType);
        }
    }
}