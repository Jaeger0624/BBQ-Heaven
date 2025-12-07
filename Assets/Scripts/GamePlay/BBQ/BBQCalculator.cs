using System;
using System.Collections.Generic;
using cfg;
using DG.Tweening;
using JetBrains.Annotations;
using QFramework;
using UniRx;
using UnityEngine;

public interface IBBQCalculator{
    void Calculate(BBQProcessContext context);
}

public abstract class AbstractBBQCalculator : IBBQCalculator , ICanGetSystem, ICanSendEvent{
    public abstract void Calculate(BBQProcessContext context);
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}


// 虽然数据层不应该知道UI层的存在
public class BBQCalculator_食材基础值逐个加 : AbstractBBQCalculator{

    public override void Calculate(BBQProcessContext context){

        BBQ bbq = context.targetBBQ;

        var foodQueue = new Queue<FoodInstance>(bbq.foodInstances);
        // 执行所有食材的计算
        while (foodQueue.Count > 0){
            ExecuteSingleFoodInstance(foodQueue, context);
        }
        // 执行串效果的计算
        ExecuteStickStrategy();
        this.GetSystem<IAnimationSystem>().Play();
    }
    private void ExecuteSingleFoodInstance(Queue<FoodInstance> foodQueue, BBQProcessContext context){
        FoodInstance foodInstance = foodQueue.Dequeue();
        BBQ bbq = context.targetBBQ;

        // 2. 通过GA_Action执行食材实例的CGA
        this.GetSystem<IGASystem>().SendAction(foodInstance, () => {
            // 3. 设置食材实例状态为在烤串上
            foodInstance.SetState(FoodInstanceState.烤串上);
        });

        if (foodInstance.food.foodGAs.ContainsKey(FoodGAType.放上烤串前)){
            // 执行食材实例的CGA
            foreach (var cga in foodInstance.food.foodGAs[FoodGAType.放上烤串前]){
                // Debug.Log($"【BBQCalculator】执行食材实例的CGA：{cga.ID}");
                this.GetSystem<IGASystem>().ApplyCGA(foodInstance, cga, new List<object>{context});
            }
        }
        this.GetSystem<IGASystem>().SendAction(foodInstance, () => {
            this.GetSystem<IFoodSystem>().PutFoodInstanceToStick(foodInstance.guid);
        });

        // 执行食材实例的GA并执行
        this.GetSystem<IGASystem>().ApplyGA(foodInstance, new GA_添加单个食材基础值(foodInstance), new List<object>{context});
    }
    private void ExecuteStickStrategy(){

    }
}