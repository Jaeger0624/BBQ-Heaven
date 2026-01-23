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
    SelectRequest Create();
}
public abstract class AbstractSelectionRequest : ISelectionRequest{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract string Title { get; }
    public abstract int Amount { get; set; }
    public abstract Action<string> OnSelect { get; set; }
    public abstract SelectRequest Create();
}

public class SelectionRequest_随机卡牌 : AbstractSelectionRequest{
    public override string Title => "选择一张卡牌";
    public override int Amount { get; set; } = 3;
    public override Action<string> OnSelect { get; set; } = null;
    public SelectionRequest_随机卡牌(int amount, Action<string> onSelect = null){
        this.Amount = amount;
        this.OnSelect = onSelect;
    }
    public override SelectRequest Create(){
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
    public override SelectRequest Create(){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();
        // 获取随机食材
        List<FoodData> foodDatas = rng.PickMany<FoodData>(this.GetSystem<IDataSystem>().GetAllFoodData(), Amount);
        // 构建回调
        Action<string> onSelect = (foodId) =>
        {
            Debug.Log("【SelectionRequest】选择食材：" + foodId);
            this.GetSystem<IFoodSystem>().AddFoodToRepository(new List<FoodPack>(){new FoodPack(foodId, 1, new List<FoodCardEnhancement>())});
        };
        if (OnSelect != null){
            onSelect = OnSelect;
        }
        return new SelectRequest(foodDatas.Select(x => x.ID).ToList(), Title, SelectionType.Food, onSelect);
    }
}

public class SelectionRequest_效果选项 : AbstractSelectionRequest{
    public override string Title => "选择一个效果";
    public override int Amount { get; set; } = 1;
    private List<OptionData> Options;
    public override Action<string> OnSelect { get; set; } = null;
    private object Sender;
    private List<object> Param;
    public SelectionRequest_效果选项(List<OptionData> options, object sender, List<object> param){
        this.Options = options;
        this.OnSelect = OnSelection;
        this.Sender = sender;
        this.Param = param;
    }
    public override SelectRequest Create(){
        return new SelectRequest(Options.Select(x => x.ID).ToList(), Title, SelectionType.Choices, OnSelect);
    }

    private void OnSelection(string optionID){
        OptionData optionData = Options.FirstOrDefault(x => x.ID == optionID);
        if (optionData == null){
            Debug.LogError("【SelectionRequest】选择的效果不存在：" + optionID);
            return;
        }
        foreach (var cga in optionData.Actions){
            this.GetSystem<IGASystem>().ApplyCGA(Sender, new CGA(cga), Param);
        }
    }
}