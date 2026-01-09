using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
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