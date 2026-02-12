using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using cfg;
using QFramework;
using Sirenix.Serialization;
using UnityEngine;

/// <summary>
/// 卡牌系统 - 实例层
/// </summary>
[Serializable]
public class Card : ICanGetSystem{
    [OdinSerialize]
    public string guid { get; private set; }
    [OdinSerialize]
    public string id { get; private set; }
    public string name;
    public string description;
    public int cost;
    public CardTargetType targetType => cardData.Target;
    [NonSerialized]
    private CardData cardData;
    public CardData CardData => cardData;
    public Card(CardData cardData){
        this.guid = Guid.NewGuid().ToString();
        this.id = cardData.ID;
        this.cardData = cardData;
        this.name = cardData.Name;
        this.description = cardData.Description;
        this.cost = cardData.Cost;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context){
        if (string.IsNullOrEmpty(id)){
            Debug.LogError("卡牌ID为空");
            return;
        }
        CardData cardData = this.GetSystem<IDataSystem>().GetCardData(id);
        if (cardData == null){
            Debug.LogError($"卡牌数据不存在: {id}");
            return;
        }
        this.cardData = cardData;
        // Debug.Log($"加载卡牌数据: {id} - {name}");
    }

    public void OnUse(List<object> param){
        this.GetSystem<ICardSystem>().State = CardSystemState.卡牌使用中;
        // 1. 提取出使用时效果
        List<CGA> useCGAs = cardData.CGAs.Where(x => x.Type == CardGAType.使用时).Select(x => new CGA(x.Action)).ToList();
        // 2. 执行使用时效果
        foreach (var cga in useCGAs){
            this.GetSystem<IGASystem>().TriggerReaction(cga, this, param);
        }

        this.GetSystem<IGASystem>().SendAction(this, () => {
            
            this.GetSystem<ICardSystem>().State = CardSystemState.正常;
        });

        // 3. 消耗时间
        this.GetSystem<ITimeSystem>().PushTimePoint(cost);
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}