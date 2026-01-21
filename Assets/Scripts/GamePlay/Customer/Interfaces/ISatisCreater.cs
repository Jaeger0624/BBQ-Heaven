using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public interface ISatisCreater{
    public string Name { get; }
    bool hasUsed { get; set; }
    float Calculate(DealContext context);
    IAnimTask GetAnimTask();
}

public abstract class AbstractSatisCreater : ISatisCreater, ICanGetSystem{
    public abstract string Name { get; }
    public bool hasUsed { get; set; } = true;
    public abstract float Calculate(DealContext context);
    public abstract IAnimTask GetAnimTask();

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}

public class SatisCreater_测试 : AbstractSatisCreater
{
    private List<string> foodInstancesGuids = new List<string>();
    public override string Name => "测试";
    private IAnimTask animTask;
    public override float Calculate(DealContext context)
    {
        foodInstancesGuids.Clear();
        foodInstancesGuids.AddRange(context.BBQ.foodInstances.Select(x => x.guid));
        float res = 1f;
        List<IAnimTask> animTasks = new List<IAnimTask>();

        foodInstancesGuids.ForEach(x => {
            // 1. 执行乘区（真正乘到满意度上的）
            GameAction ga = new GA_创建满意度乘区(1.1f, Name, false);
            this.GetSystem<IGASystem>().ApplyGA(context.Customer, ga, new List<object>{context});
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

public class SatisHolder : ICanGetSystem, ICanSendEvent{
    public readonly List<ISatisCreater> satisCreaters;
    public SatisHolder(){
        this.satisCreaters = new List<ISatisCreater>{};
    }
    public void Calculate(DealContext context){

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