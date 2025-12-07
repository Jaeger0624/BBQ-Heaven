using System.Collections.Generic;

namespace cfg{
    // RecipeRule只能允许存储逻辑，具有幂等性，不能有副作用
    public abstract partial class RecipeRule{
        public abstract bool Evaluate(BBQ bbq, List<object> param);
    }

    public partial class RecipeRule_食材共存 : RecipeRule{
        public override bool Evaluate(BBQ bbq, List<object> param)
        {
            return false;
        }
    }

    public partial class RecipeRule_类型共存 : RecipeRule{
        public override bool Evaluate(BBQ bbq, List<object> param)
        {
            return false;
        }
    }

    public partial class RecipeRule_动态值 : RecipeRule{
        public override bool Evaluate(BBQ bbq, List<object> param)
        {
            return ExtraTool.BoolValue(Sign, DV1.GetValue(bbq, param), DV2.GetValue(bbq, param));
        }
    }
}