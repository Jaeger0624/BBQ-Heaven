using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public interface ISatisCreater{
    public string Name { get; }
    bool hasUsed { get; set; }
    float Calculate(BBQ bbq, Customer customer);
    IAnimTask GetAnimTask();
}

public abstract class AbstractSatisCreater : ISatisCreater, ICanGetSystem{
    public abstract string Name { get; }
    public bool hasUsed { get; set; } = true;
    public abstract float Calculate(BBQ bbq, Customer customer);
    public abstract IAnimTask GetAnimTask();

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}

public class SatisCreater_测试 : AbstractSatisCreater
{
    private List<string> foodInstancesGuids = new List<string>();
    public override string Name => "测试";
    private IAnimTask animTask;
    public override float Calculate(BBQ bbq, Customer customer)
    {
        foodInstancesGuids.Clear();
        foodInstancesGuids.AddRange(bbq.foodInstances.Select(x => x.guid));
        float res = 1f;
        List<IAnimTask> animTasks = new List<IAnimTask>();
        // 0. 创建上下文
        CustomerSatisfaction satisfaction = this.GetSystem<ICustomerSystem>().Satisfaction;
        DealContext context = new DealContext(bbq, customer, satisfaction);
        foodInstancesGuids.ForEach(x => {
            // 1. 执行乘区（真正乘到满意度上的）
            GameAction ga = new GA_创建满意度乘区(1.1f, Name, false);
            this.GetSystem<IGASystem>().ApplyGA(customer, ga, new List<object>{context});
            // 2. 添加动画任务
            Transform target = SettingManager.Instance.SatisfactionTextParent;
            animTasks.Add(AnimCombine_顾客Tag.Anim_Tag触发_单一突出(x, 1.1f, Name, target));
            // 3. 更新局部总乘区
            res *= 1.1f;
        });
        animTask = new SequenceAnimTask(animTasks);
        return res;
    }
    public override IAnimTask GetAnimTask()
    {
        return animTask;
    }
}

public class SatisCreater_具体食材 : AbstractSatisCreater
{
    public override string Name => "具体食材";
    public override float Calculate(BBQ bbq, Customer customer)
    {
        
        List<Food> Foods = bbq.foodInstances.Select(x => x.food).ToList();
        if (customer.preferences == null || customer.preferences.foodIds == null || customer.preferences.foodIds.Count == 0){
            hasUsed = false;
            return 1f;
        }
        int matchedAmount = Foods.Count(x => customer.preferences.foodIds.Contains(x.guid));
        return matchedAmount * 0.1f + 1f;
    }
    public override IAnimTask GetAnimTask()
    {
        return new EmptyAnimTask();
    }
}

public class SatisCreater_食材类型 : AbstractSatisCreater
{
    public override string Name => "食材类型";
    private int amount = 0;
    private List<string> foodInstancesGuids = new List<string>();
    public override float Calculate(BBQ bbq, Customer customer)
    {
        if (customer.preferences == null || customer.preferences.foodTypes == null || customer.preferences.foodTypes.Count == 0){
            hasUsed = false;
            return 1f;
        }
        foodInstancesGuids.Clear();
        foodInstancesGuids.AddRange(bbq.foodInstances.Where(x => customer.preferences.foodTypes.Contains(x.food.foodType)).Select(x => x.guid));
        amount = foodInstancesGuids.Count;
        return amount * 0.1f + 1f;
    }
    public override IAnimTask GetAnimTask()
    {
        List<float> multipliers = new List<float>();
        foreach (var _ in foodInstancesGuids){
            multipliers.Add(1.1f);
        }
        return AnimCombine_顾客Tag.Anim_Tag触发_逐一突出(foodInstancesGuids.ToList(), multipliers);
    }
}

public class SatisCreater_配方喜好 : AbstractSatisCreater
{
    public override string Name => "配方喜好";
    public override float Calculate(BBQ bbq, Customer customer)
    {
        if (customer.preferences == null || customer.preferences.recipeIds == null || customer.preferences.recipeIds.Count == 0){
            hasUsed = false;
            return 1f;
        }
        int matchedAmount = bbq.recipeIds.Count(x => customer.preferences.recipeIds.Contains(x));
        return matchedAmount * 0.1f + 1f;
    }
    public override IAnimTask GetAnimTask()
    {
        return new EmptyAnimTask();
    }
}


public class SatisHolder : ICanGetSystem, ICanSendEvent{
    public readonly List<ISatisCreater> satisCreaters;
    public SatisHolder(){
        this.satisCreaters = new List<ISatisCreater>{};
    }
    public void Calculate(BBQ bbq, Customer customer){

        foreach (var satisCreater in satisCreaters){
            // 1. 执行计算（生成动画）

        }
    }
    public void AddSatisCreater(ISatisCreater satisCreater){
        this.satisCreaters.Add(satisCreater);
    }
    public void Reset(){
        this.satisCreaters.Clear();
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}