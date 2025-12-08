using System.Collections.Generic;
using QFramework;

namespace cfg{
    public partial class GA_显示对话 : GameAction, ICanGetSystem{
        public Dialogue dialogue;
        public GA_显示对话(Dialogue dialogue){
            this.dialogue = dialogue;
        }

        public override GameAction Clone()
        {
            return new GA_显示对话(dialogue);
        }

        public override void Execute(object sender, List<object> param)
        {
            this.GetSystem<IDialogueSystem>().ShowDialogues(new List<Dialogue>{dialogue});
        }

        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }

        public override int GetTypeId()
        {
            return 124532357;
        }
    }
}