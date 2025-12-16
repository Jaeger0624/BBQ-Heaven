using System.Collections.Generic;
using System.Linq;
using QFramework;

namespace cfg{
    // RecipeRule只能允许存储逻辑，具有幂等性，不能有副作用
    public partial class RecipeRule : ICanGetSystem{
        public bool EvaluatePreview(List<object> param)
        {
            return this.GetSystem<IGASystem>().EvaluateConditions(null, Action.Conditions, param);
        }
        public bool Evaluate(BBQ bbq, List<object> param)
        {
            return this.GetSystem<IGASystem>().EvaluateConditions(bbq, Action.Conditions, param);
        }
        public void Execute(BBQ bbq, List<object> param)
        {
            this.GetSystem<IGASystem>().ApplyCGA(bbq, Action, param);
        }
        public IArchitecture GetArchitecture()
        {
            return GameArchitecture.Interface;
        }
    }
}