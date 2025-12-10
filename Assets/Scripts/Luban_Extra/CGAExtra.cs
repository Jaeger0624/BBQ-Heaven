using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using Sirenix.Serialization;
namespace cfg{
public partial class CGA : ICanGetSystem, IHaveAnim{
    [NonSerialized]
    private List<IHaveAnim> animTasks = new List<IHaveAnim>();
    public CGA(CGA cga){
        this.ID = cga.ID;
        this.Conditions = new List<Condition>(cga.Conditions);
        this.Actions = new List<GameAction>(cga.Actions.Select(x => x.Clone()));
    }
    public bool Execute(object sender, List<object> param)
    {
        // 只要有一个条件不满足，就跳过执行
        if (!this.GetSystem<IGASystem>().EvaluateConditions(sender, Conditions, param)){
            return false;
        }
        foreach (var action in Actions){
            //TODO: 让其越过IGASystem？
            // 如果每个GA的执行都单独再通过IGASystem，则需要在这里添加动画任务，但CGA的动画应该是组合动画
            action.Execute(sender, param);
            animTasks.Add(action);
        }
        return true;
    }
        public IAnimTask GetAnimTask()
        {
            if (animTasks.Count == 0) return new EmptyAnimTask();
            // TODO: 显示CGA的组合动画
            List<IAnimTask> animTasksList = new List<IAnimTask>();
            foreach (var action in Actions){
                animTasksList.Add(action.GetAnimTask());
            }
            IAnimTask animTask = new SequenceAnimTask(animTasksList);
            return animTask;
        }

        public IArchitecture GetArchitecture()
        {
            return GameArchitecture.Interface;
        }

        public void ApplyMultiplier(int multiplier)
        {
            foreach (var action in Actions)
            {
                action.ApplyMultiplier(multiplier);
            }
        }
    }


    public partial class TagCGA{
        public TagCGA(TagCGA tagCGA){
            this.GADescription = tagCGA.GADescription;
            this.CDDescription = tagCGA.CDDescription;
            this.Type = tagCGA.Type;
            this.Cga = new CGA(tagCGA.Cga);
        }
    }
}