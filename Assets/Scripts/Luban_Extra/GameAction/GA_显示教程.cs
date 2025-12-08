using System.Collections.Generic;
using QFramework;

namespace cfg{
    public partial class GA_显示教程 : GameAction{
        public string tutorial;
        public GA_显示教程(string tutorial){
            this.tutorial = tutorial;
        }

        public override GameAction Clone()
        {
            return new GA_显示教程(tutorial);
        }

        public override void Execute(object sender, List<object> param)
        {
            // this.GetSystem<IGuideSystem>().ShowTutorial(tutorial);
        }

        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }

        public override int GetTypeId()
        {
            return 124532356;
        }
    }
}