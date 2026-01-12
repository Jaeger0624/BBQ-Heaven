using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UniRx;
using UnityEngine;

/// <summary>
/// 食材实例类
/// </summary>
public partial class FoodInstance : BoardEntity, IAnimPlayer{  
    public override BoardEntityType type => BoardEntityType.食材;
    public override string name => food.name;
    public Food food;
    /// <summary>实例状态，主要用于程序逻辑判断</summary>
    public FoodInstanceState state {get; private set;} = FoodInstanceState.无;

    // 示例数据
    public int rarity;
    public int taste;
    public float baseCritRate = 0.1f; // 基础暴击率
    public float baseCritMultiplier = 1.5f; // 基础暴击倍率
    public int foodSize = 1;

    /// <summary>实例状态，主要决定动画和View的显示</summary>
    public readonly ReactiveProperty<FoodInstanceViewStatus> status = new ReactiveProperty<FoodInstanceViewStatus>(new FoodInstanceViewStatus(false));
    public FoodInstanceView foodInstanceView;
    // Runtime部分：不保存
    public List<SustainEffect> sustainEffects = new List<SustainEffect>();
    public Action OnViewStatusChanged;
    public FoodInstance(Food food, Vector2Int position) : base(){
        this.food = food;
        this.position = position;
        //TODO: 可能之后会增加初始化逻辑，比如CGA等，先这样处理
        this.rarity = (int)food.foodData.Rarity;
        this.taste = (int)food.foodData.Taste;
        List<CGA> cgas = new List<CGA>();
        // 每一个CGA都深拷贝一份
        foreach (var cga in food.foodGAs.Values.SelectMany(x => x)){
            cgas.Add(new CGA(cga));
        }
        this.sustainEffects = food.sustainEffects.Select(x => x.Clone()).ToList();
    }
    public void SetState(FoodInstanceState state){
        if (this.state == state){
            return;
        }
        this.state = state;
        if (state == FoodInstanceState.棋盘上){
            OnAddSE();
        }else if (state == FoodInstanceState.烤串上){
            OnRemoveSE();
        }
    }

    public void OnAddSE(){
        if (sustainEffects == null || sustainEffects.Count == 0) return;
        // Debug.Log($"【FoodInstance】添加食材实例SE: {sustainEffects.Count}");
        foreach (var se in sustainEffects){
            this.GetSystem<IGASystem>().ApplySE(this, se);
        }
    }
    public void OnRemoveSE(){
        if (sustainEffects == null || sustainEffects.Count == 0) return;
        // Debug.Log($"【FoodInstance】移除食材实例SE: {sustainEffects.Count}");
        foreach (var se in sustainEffects){
            this.GetSystem<IGASystem>().RemoveSE(this, se);
        }
    }
}


/// <summary>
/// 食材实例状态，主要决定动画和View的显示
/// </summary>
[Serializable]
public class FoodInstanceViewStatus
{
    public readonly bool isBuffed = false;
    public FoodInstanceViewStatus(bool isBuffed){
        this.isBuffed = isBuffed;
    }
}