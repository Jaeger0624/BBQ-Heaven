using System.Collections.Generic;
using QFramework;

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
}