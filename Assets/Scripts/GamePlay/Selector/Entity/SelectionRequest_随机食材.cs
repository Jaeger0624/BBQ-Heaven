using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;


public class SelectionRequest_随机食材 : AbstractSelectionRequest{
    public override string Title => "选择一种食材";
    public override int Amount { get; set; } = 3;
    public override Action<SelectionBuildContext> OnSelect { get; set; } = null;
    public SelectionRequest_随机食材(int amount, Action<SelectionBuildContext> onSelect = null){
        this.Amount = amount;
        this.OnSelect = onSelect;
    }
    public override SelectRequest Create(){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();
        // 获取随机食材
        List<FoodData> foodDatas = rng.PickMany<FoodData>(this.GetSystem<IDataSystem>().GetAllFoodData(), Amount);
        // 构建回调
        Action<SelectionBuildContext> onSelect = (foodContext) =>
        {
            Debug.Log("【SelectionRequest】选择食材：" + foodContext.Id);
            this.GetSystem<IFoodSystem>().AddFoodToRepository(new List<FoodPack>(){new FoodPack(foodContext.Id, 1, new List<FoodCardEnhancement>())});
        };
        if (OnSelect != null){
            onSelect = OnSelect;
        }
        return new SelectRequest(foodDatas.Select(x => new SelectionBuildContext(SelectionType.食材, x.ID)).ToList(), Title, onSelect);
    }
}
