using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;
using UnityEngine.Rendering;
[Serializable]
public class FoodPile : ICanGetSystem, ICanSendEvent{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    /// <summary>
    /// 当前食材仓库
    /// </summary>
    public List<FoodCard> FoodSet = new List<FoodCard>();
    public List<FoodCard> DrawFoodPile = new List<FoodCard>();
    public bool JustRefreshed { get; private set; } = false;
    public FoodPile(List<FoodCard> foodInventory){
        FoodSet = foodInventory.ToList();
        DrawFoodPile = new List<FoodCard>();
    }
    public void RefreshFoodPile(){
        // 1. 补充抽牌堆
        DrawFoodPile = FoodSet.ToList();
        JustRefreshed = true;

        // 2. 消耗时间
        this.GetSystem<ITimeSystem>().PushTimePoint(this.GetSystem<IFoodSystem>().RefreshFoodCost);

        this.SendEvent(new FoodCardPileUpdateEvent());
    }
    public void LoadFoodPile(FoodPile foodPile){
        // 1. 设置FoodSet
        FoodSet = foodPile.FoodSet.ToList();
        this.SendEvent(new FoodCardPileUpdateEvent());
    }
    public void Init(){
        DrawFoodPile = FoodSet.ToList();
        JustRefreshed = true;
        this.SendEvent(new FoodCardPileUpdateEvent());
    }
    public void AddFoodToSet(FoodCard food, bool alsoToDrawPile){
        FoodSet.Add(food);
        if (alsoToDrawPile){
            DrawFoodPile.Add(food);
        }
        this.SendEvent(new FoodCardPileUpdateEvent());
    }
    public List<FoodCard> DrawFoodCard(int count){
        JustRefreshed = false;
        List<FoodCard> foodCards = new List<FoodCard>();
        if (DrawFoodPile.Count < count){
            foodCards.AddRange(DrawFoodPile);
            DrawFoodPile.Clear();
        }
        else{
            Rng rng = this.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();
            // 抽出后要删除对应的食物卡牌
            foodCards = rng.PickMany<FoodCard>(DrawFoodPile, count).ToList();
            DrawFoodPile.RemoveAll(foodCard => foodCards.Contains(foodCard));
        }
        this.SendEvent(new FoodCardPileUpdateEvent());
        this.SendEvent(new DrawFoodCardEvent(foodCards));
        return foodCards;
    }

    public void Reset(List<FoodCard> foodCards){
        FoodSet = foodCards.ToList();
        DrawFoodPile = new List<FoodCard>();
        JustRefreshed = false;
    }
}

public class FoodCardPileUpdateEvent : AbstractEvent{

}


public class DrawFoodCardEvent : AbstractEvent{
    public List<FoodCard> foodCards;
    public DrawFoodCardEvent(List<FoodCard> foodCards){
        this.foodCards = foodCards;
    }
}