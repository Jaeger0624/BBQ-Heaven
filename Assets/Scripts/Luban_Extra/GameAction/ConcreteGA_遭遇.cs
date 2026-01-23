using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UniRx;
using UnityEngine;

namespace cfg{
    public partial class GA_修改得分乘区 : GameAction
    {
        public GA_修改得分乘区(float value, string sender, bool isAdd)
        {
            this.Value = value;
            this.Sender = sender;
            this.IsAdd = isAdd;
        }
        public override GameAction Clone()
        {
            return new GA_修改得分乘区(Value, Sender, IsAdd);
        }
        public override void Execute(object sender, List<object> param)
        {
            if (IsAdd){
                this.GetSystem<IDealSystem>().AddScoreMultiplier(Sender, Value);
            }
            else{
                this.GetSystem<IDealSystem>().RemoveScoreMultiplier(Sender);
            }
        }
        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }

    public partial class GA_创建临时乘区 : GameAction
    {
        public GA_创建临时乘区(float value, string name)
        {
            this.Value = value;
            this.Name = name;
        }
        public override GameAction Clone()
        {
            return new GA_创建临时乘区(Value, Name);
        }
        public override void Execute(object sender, List<object> param)
        {
            DealContext context = param.First(x => x is DealContext) as DealContext;

            if (context == null)
            {
                Debug.LogError("【GA_创建临时乘区】没有DealContext");
                return;
            }

            context.OtherMultipliers.Add(Name, Value);
            Debug.Log($"【GA_创建临时乘区】创建临时乘区: {Name}, {Value}");
        }
        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }


    #region 遭遇GA
    public partial class GA_触发遭遇 : GameAction
    {
        public GA_触发遭遇(string ID)
        {
            this.ID = ID;
        }
        public override GameAction Clone()
        {
            return new GA_触发遭遇(ID);
        }
        public override void Execute(object sender, List<object> param)
        {
            this.GetSystem<IEncounterSystem>().StartEncounter(ID);
        }
        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }

    public partial class GA_触发瞬时遭遇 : GameAction
    {
        public GA_触发瞬时遭遇(string ID)
        {
            this.ID = ID;
        }

        public override GameAction Clone()
        {
            return new GA_触发瞬时遭遇(ID);
        }

        public override IObservable<GAResult> ExecuteAsync(object sender, List<object> param)
        {
            // 返回一个Observable, 当遭遇结束时发射 GAResult
            return Observable.Create<GAResult>(observer =>
            {
                this.GetSystem<IEncounterSystem>().TriggerInstantEncounter(ID, param).Subscribe(result =>
                {
                    observer.OnNext(GAResult.Empty);
                    observer.OnCompleted();
                });
                return Disposable.Empty;
            });
        }
        public override void Execute(object sender, List<object> param)
        {
            Debug.LogError("【GA_触发瞬时遭遇】不能直接执行，请使用 ExecuteAsync");
        }
        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }
    #endregion
}