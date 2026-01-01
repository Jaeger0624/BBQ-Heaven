using System.Collections.Generic;
using System.Linq;
using QFramework;
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
        }
        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }
}