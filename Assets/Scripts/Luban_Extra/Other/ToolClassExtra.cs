// 工具类扩展

using System.Collections.Generic;

namespace cfg{
    public partial class GetFoodInstancesInfo{
        public GetFoodInstancesInfo(GetFoodInstanceStrategy strategy, DynamicValue value, List<FoodInstanceState> types){
            this.Strategy = strategy;
            this.Value = value;
            this.Types = types;
        }
        public List<FoodInstance> GetFoodInstances(object sender, List<object> param){

            
            return ExtraTool.GetFoodInstances(Types, Strategy, Value.GetValue(sender, param), sender, param);
        }
    }
}