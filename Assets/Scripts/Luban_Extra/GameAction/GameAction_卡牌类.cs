using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace cfg{
    public partial class GA_抽牌 : GameAction
    {
        IAnimPlayer animPlayer;
        public GA_抽牌(DynamicValue value){
            this.Value = value;
        }
        public override GameAction Clone() => new GA_抽牌(Value);
        public override void Execute(object sender, List<object> param)
        {
            animPlayer = null;

            this.GetSystem<ICardSystem>().DrawCard(Value.GetValue(sender, param));
        
            if (sender is IAnimPlayer newAnimPlayer){
                this.animPlayer = newAnimPlayer;
            }
        }
        public override IAnimTask GetAnimTask()
        {
            IAnimTask animTask = animPlayer != null ? new SequenceAnimTask(new List<IAnimTask>{
                AnimationConverter.Convert(animPlayer, "common"),
            }) : new EmptyAnimTask();
            return animTask;
        }
    }


}