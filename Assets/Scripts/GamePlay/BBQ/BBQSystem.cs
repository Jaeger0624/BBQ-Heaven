using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using Reflex.Attributes;
using UnityEngine;

public interface IBBQSystem : ISystem{
    BBQPreview PreviewFoodInstances { get;}
    bool IsBBQing { get; }
    BBQ GetCurrentBBQ();
    void SetPreview(BBQPreview preview);
    void FinishBBQ(Stick stick, List<FoodInstance> foodInstances);
    void SetCalculator(IBBQCalculator calculator);
}

/// <summary>
/// 烧烤系统 - 系统层
/// 用于处理烧烤相关的逻辑
/// </summary>
public class BBQSystem : AbstractSystem, IBBQSystem
{
    // 存储的所有烧烤实例
    private List<BBQ> BBQRepository = new List<BBQ>();
    // 当前在运算，待存储的烧烤实例
    private BBQ currentBBQ;
    private IBBQCalculator calculator = new BBQCalculator_食材基础值逐个加();
    private bool isBBQing = false;
    public bool IsBBQing => isBBQing;

    public BBQPreview PreviewFoodInstances { get; private set; }
    public void SetPreview(BBQPreview preview) => PreviewFoodInstances = preview;

    public BBQ GetCurrentBBQ() => currentBBQ;
    
    protected override void OnInit()
    {
        // Debug.Log("【BBQSystem】注册事件"); 
        BBQRepository = new List<BBQ>();
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDayEvent);
        this.RegisterEvent<DealCompletedEvent>(OnDealCompletedEvent);
    }
    protected override void OnDeinit()
    {
        this.UnRegisterEvent<StartNewDayEvent>(OnStartNewDayEvent);
        this.UnRegisterEvent<DealCompletedEvent>(OnDealCompletedEvent);
    }
    private void OnStartNewDayEvent(StartNewDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;

        // 清除实例
        List<BBQ> tempBBQRepository = new List<BBQ>();
        tempBBQRepository.AddRange(BBQRepository);
        tempBBQRepository.ForEach(bbq => RemoveBBQFromRepository(bbq));
    }
    // 完成一次烧烤
    public void FinishBBQ(Stick stick, List<FoodInstance> foodInstances){
        isBBQing = true;
        this.GetSystem<IStickSystem>().UseCurrentStick(stick);  // 会同时取消选中烤串
        
        BBQ bbq = new BBQ(stick, foodInstances);    // 创建烧烤实例
        
        SetCurrentBBQ(bbq);  // 设置当前烧烤实例

        //TODO: 如果有监听Combine的条件且在Combine之前的事件，则在这里执行


        if (!this.GetSystem<IProxySystem>().isTesting){
            this.SendEvent(new CombineBBQEvent(bbq));

            this.GetSystem<IGASystem>().SendAction(bbq, () => {
                this.SendEvent(new CombineBBQEvent_动画());
            });
        }

        BBQProcessContext context = new BBQProcessContext(currentBBQ);

        // 0.检测时间是否足够
        int timePoint = this.GetSystem<ITimeSystem>().GetCostTime(foodInstances, stick);
        this.GetSystem<ITimeSystem>().PushTimePoint(timePoint);
        Debug.Log($"【BBQSystem】制作烧烤耗时：{timePoint}");

        foodInstances.ForEach(x => 
        {
            this.GetSystem<IGASystem>().SendAction(x, () => {
                x.SetState(FoodInstanceState.被选中);
            });

            // 触发被选中时的GA
            if (x.food.foodGAs.ContainsKey(FoodGAType.被选中时)){
                // 执行食材实例的CGA
                foreach (var cga in x.food.foodGAs[FoodGAType.被选中时]){
                    this.GetSystem<IGASystem>().TriggerReaction(cga, x, new List<object>{context});
                }
            }
        });

        // 计算并得出当前烧烤实例的最终结果
        this.GetSystem<IGASystem>().SendAction(bbq, () => {
            CalculateCurrentBBQ(context);
        });
    }
    // 计算并得出当前烧烤实例的最终结果
    private void CalculateCurrentBBQ(BBQProcessContext context){
        // 1. 重置烧烤计算器用于执行计算过程
        BBQCalculator_食材基础值逐个加 calculator = new BBQCalculator_食材基础值逐个加();
        calculator.Calculate(context);

        // 2.1 在计算完成后，添加动画暂停
        // 2.2 清除高亮显示
        if (!this.GetSystem<IProxySystem>().isTesting){
            this.GetSystem<IGASystem>().SendAction(context.targetBBQ, () => {
                this.GetSystem<IAnimationSystem>().Append(new DelayAnimTask(0.3f, true));
                this.SendEvent(new ClearAllBoardsHighlight());
                Debug.Log("【BBQSystem】清除高亮显示");
                this.GetSystem<IAnimationSystem>().Append(new DelayAnimTask(0.5f, true));
            });
        }


        this.GetSystem<IGASystem>().SendAction(context.targetBBQ, () => {
            // 4. 检测配方触发情况
            this.GetSystem<IRecipeSystem>().MatchRecipe(new List<object>{context});
        });

        this.GetSystem<IGASystem>().SendAction(context.targetBBQ, () => {
            this.SendEvent(new AfterCalculateBBQEvent(context.targetBBQ, context));
        });

        if (!this.GetSystem<IProxySystem>().isTesting){
            // this.GetSystem<IGASystem>().SetTrigger(context.targetBBQ);   
        }
        //

        this.GetSystem<IGASystem>().SendAction(context.targetBBQ, () => {
            this.SendEvent(new FinishCombineBBQEvent(context.targetBBQ, context));
        });

        if (!this.GetSystem<IProxySystem>().isTesting){
        this.GetSystem<IGASystem>().SendAction(context.targetBBQ, () => {
            // 7. 等待0.3s
            this.GetSystem<IAnimationSystem>().Append(new DelayAnimTask(0.3f, true));
            // 8. 播放完成烧烤动画
            this.GetSystem<IAnimationSystem>().Append(new ActionAnimTask(() => {
                this.SendEvent(new FinishCombineBBQEvent_动画());
            }));
        });
        }
        this.GetSystem<IGASystem>().SendAction(context.targetBBQ, () => {
            // 5. 将当前烧烤实例存储到烧烤仓库中
            AddBBQToRepository(new List<object>{context});
        });
    }
    private void SetCurrentBBQ(BBQ bbq){
        if (bbq == null) return;
        currentBBQ = bbq;
        // Debug.Log($"【BBQSystem】设置当前烧烤实例: {bbq.guid}");
    }
    private void AddBBQToRepository(List<object> param){
        BBQProcessContext context = param?.FirstOrDefault() as BBQProcessContext;
        if (context == null){
            Debug.LogError("上下文为空");
            return;
        }
        BBQ bbq = context.targetBBQ;
        if (bbq == null) {Debug.LogError("当前烧烤为空"); return;}

        BBQRepository.Add(bbq);

        if (!this.GetSystem<IProxySystem>().isTesting){
        // 延时提醒动画发生
            this.GetSystem<IAnimationSystem>().Append(new SequenceAnimTask(new List<IAnimTask>{
                new ActionAnimTask(() => {
                    isBBQing = false;
                }),
                new DelayAnimTask(0.3f, true),
                new ActionAnimTask(() => {
                    this.SendEvent(new AddBBQToRepositoryEvent(bbq));
                }),
            }));
        }
        else{
            isBBQing = false;
            this.SendEvent(new AddBBQToRepositoryEvent(bbq));
        }
    }
    private void OnDealCompletedEvent(DealCompletedEvent evt)
    {
        RemoveBBQFromRepository(evt.result.bbq);
    }
    private void RemoveBBQFromRepository(BBQ bbq){
        BBQRepository.Remove(bbq);
        this.SendEvent(new RemoveBBQFromRepositoryEvent(bbq));
    }
    public void SetCalculator(IBBQCalculator calculator){
        this.calculator = calculator;
        Debug.Log($"【BBQSystem】设置烧烤计算器为{calculator.GetType().Name}");
    }
}