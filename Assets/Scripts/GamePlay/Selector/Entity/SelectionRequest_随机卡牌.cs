using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public class SelectionRequest_随机卡牌 : AbstractSelectionRequest{
    public override string Title => "选择一张卡牌";
    public override int Amount { get; set; } = 3;
    public override Action<SelectionBuildContext> OnSelect { get; set; } = null;
    public SelectionRequest_随机卡牌(int amount, Action<SelectionBuildContext> onSelect = null){
        this.Amount = amount;
        this.OnSelect = onSelect;
    }
    public override SelectRequest Create(){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICardSystem>();
        // 获取3张随机卡牌
        List<CardData> cardDatas = rng.PickMany<CardData>(this.GetSystem<IDataSystem>().GetAllCardData(), 3);
        // 构建回调
        Action<SelectionBuildContext> onSelect = (cardContext) =>
        {
            Debug.Log("【SelectionRequest】选择卡牌：" + cardContext.Id);
            this.GetSystem<ICardSystem>().AddCardToRepository(cardContext.Id);
        };
        if (OnSelect != null){
            onSelect = OnSelect;
        }
        return new SelectRequest(cardDatas.Select(x => new SelectionBuildContext(SelectionType.卡牌, x.ID)).ToList(), Title, onSelect);
    }
}
