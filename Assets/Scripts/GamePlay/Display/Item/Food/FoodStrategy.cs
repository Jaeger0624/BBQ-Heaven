using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
public class CollectionFoodStrategy : ItemInteractStrategyBase<FoodUIContext, DisplayFoodView>
{
    public override void OnBind(DisplayFoodView itemView, FoodUIContext data)
    {
    }
    public override void OnClick(FoodUIContext data, DisplayFoodView itemView)
    {
    }
}
public class DeleteFoodStrategy : ItemInteractStrategyBase<FoodUIContext, DisplayFoodView>{
    private Action _onPicked; // 选中后的回调;
    public DeleteFoodStrategy(Action onPicked){
        _onPicked = onPicked;
    }
    public override void OnClick(FoodUIContext data, DisplayFoodView itemView)
    {
        this.GetSystem<IFoodSystem>().DeleteFoodFromRepository(data.RuntimeFood);
        itemView.gameObject.SetActive(false);
        _onPicked?.Invoke();
    }
    public override void OnBind(DisplayFoodView itemView, FoodUIContext data){
    }
}

public class MultiDeleteFoodStrategy : ItemInteractStrategyBase<FoodUIContext, DisplayFoodView>{
    private SelectionContext<FoodUIContext> _context;
    public MultiDeleteFoodStrategy(SelectionContext<FoodUIContext> context){
        _context = context;
    }
    public override void OnBind(DisplayFoodView itemView, FoodUIContext data){
        itemView.SetSelectedState(_context.SelectedItems.Contains(data));
    }
    public override void OnClick(FoodUIContext data, DisplayFoodView itemView)
    {
        bool isSelected = _context.Toggle(data);
        itemView.SetSelectedState(isSelected);
    }
}

public class BuyFoodStrategy : ItemInteractStrategyBase<FoodUIContext, DisplayFoodView>{
    private bool isMultiple;
    public BuyFoodStrategy(bool isMultiple){
        this.isMultiple = isMultiple;
    }
    public override void OnBind(DisplayFoodView itemView, FoodUIContext data){
    }
    public override void OnClick(FoodUIContext data, DisplayFoodView itemView){


        if (data == null) {Debug.LogError("BuyFoodStrategy 的 data 为空"); return;}
        if (data.RuntimeFood == null) {Debug.LogError("BuyFoodStrategy 的 data.RuntimeFood 为空"); return;}

        // 若价格不够
        if (this.GetSystem<IEconomySystem>().coin.Value < data.Price){
            Debug.Log("金币不足，无法购买");
            return;
        }

        // 1. 获得食材到仓库
        if (isMultiple){
            if (data.Count <= 0) {Debug.LogError("BuyFoodStrategy 的 data.Count 为0"); return;}
            List<FoodCard> foodCards = new List<FoodCard>();
            for (int i = 0; i < data.Count; i++){
                foodCards.Add(data.RuntimeFood.Clone());
            }
            this.GetSystem<IFoodSystem>().AddFoodToRepository(foodCards);
        } else {
            this.GetSystem<IFoodSystem>().AddFoodToRepository(new List<FoodCard>(){data.RuntimeFood});
        }

        // 2. 扣除金币
        this.GetSystem<IEconomySystem>().CostCoin(data.Price);

        // 3. 隐藏自身
        itemView.gameObject.SetActive(false);
    }
}