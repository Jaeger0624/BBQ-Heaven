using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public interface ISelectionRequest : ICanGetSystem{
    string Title { get; }
    int Amount { get; set; }
    Action<string> OnSelect { get; set; }
    SelectRequest Form();
}
public abstract class AbstractSelectionRequest : ISelectionRequest{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract string Title { get; }
    public abstract int Amount { get; set; }
    public abstract Action<string> OnSelect { get; set; }
    public abstract SelectRequest Form();
}

public class SelectionRequest_随机卡牌 : AbstractSelectionRequest{
    public override string Title => "选择一张卡牌";
    public override int Amount { get; set; } = 3;
    public override Action<string> OnSelect { get; set; } = null;
    public SelectionRequest_随机卡牌(int amount, Action<string> onSelect = null){
        this.Amount = amount;
        this.OnSelect = onSelect;
    }
    public override SelectRequest Form(){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICardSystem>();
        // 获取3张随机卡牌
        List<CardData> cardDatas = rng.PickMany<CardData>(this.GetSystem<IDataSystem>().GetAllCardData(), 3);
        // 构建回调
        Action<string> onSelect = (cardId) =>
        {
            Debug.Log("【SelectionRequest】选择卡牌：" + cardId);
            this.GetSystem<ICardSystem>().AddCardToRepository(cardId);
        };
        if (OnSelect != null){
            onSelect = OnSelect;
        }
        return new SelectRequest(cardDatas.Select(x => x.ID).ToList(), Title, SelectionType.Card, onSelect);
    }
}

public class SelectionRequest_随机食材 : AbstractSelectionRequest{
    public override string Title => "选择一种食材";
    public override int Amount { get; set; } = 3;
    public override Action<string> OnSelect { get; set; } = null;
    public SelectionRequest_随机食材(int amount, Action<string> onSelect = null){
        this.Amount = amount;
        this.OnSelect = onSelect;
    }
    public override SelectRequest Form(){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();
        // 获取随机食材
        List<FoodData> foodDatas = rng.PickMany<FoodData>(this.GetSystem<IDataSystem>().GetAllFoodData(), Amount);
        // 构建回调
        Action<string> onSelect = (foodId) =>
        {
            Debug.Log("【SelectionRequest】选择食材：" + foodId);
            this.GetSystem<IFoodSystem>().AddFoodToRepository(new List<FoodPack>(){new FoodPack(foodId, 1)});
        };
        if (OnSelect != null){
            onSelect = OnSelect;
        }
        return new SelectRequest(foodDatas.Select(x => x.ID).ToList(), Title, SelectionType.Food, onSelect);
    }
}