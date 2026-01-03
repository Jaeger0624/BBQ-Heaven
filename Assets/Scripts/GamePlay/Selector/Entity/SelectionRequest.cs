using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;

public interface ISelectionRequest : ICanGetSystem{
    string Title { get; }
    RequestSelectionEvent Form();
}
public abstract class AbstractSelectionRequest : ISelectionRequest{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract string Title { get; }
    public abstract RequestSelectionEvent Form();
}

public class SelectionRequest_随机卡牌 : AbstractSelectionRequest{
    public override string Title => "选择一张卡牌";
    public override RequestSelectionEvent Form(){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICardSystem>();
        // 获取3张随机卡牌
        List<CardData> cardDatas = rng.PickMany<CardData>(this.GetSystem<IDataSystem>().GetAllCardData(), 3);
        // 构建回调
        Action<string> onSelect = (cardId) =>
        {
            this.GetSystem<ICardSystem>().AddCardToRepository(cardId);
        };
        return new RequestSelectionEvent(cardDatas.Select(x => x.ID).ToList(), Title, SelectionType.Card, onSelect);
    }
}

public class SelectionRequest_随机食材 : AbstractSelectionRequest{
    public override string Title => "选择一种食材";
    public override RequestSelectionEvent Form(){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();
        // 获取3种随机食材
        List<FoodData> foodDatas = rng.PickMany<FoodData>(this.GetSystem<IDataSystem>().GetAllFoodData(), 3);
        // 构建回调
        Action<string> onSelect = (foodId) =>
        {
            this.GetSystem<IFoodSystem>().AddFoodToRepository(new List<FoodPack>(){new FoodPack(foodId, 1)});
        };
        return new RequestSelectionEvent(foodDatas.Select(x => x.ID).ToList(), Title, SelectionType.Food, onSelect);
    }
}