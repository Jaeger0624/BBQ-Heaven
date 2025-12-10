using System.Collections.Generic;

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

        }
        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }
}