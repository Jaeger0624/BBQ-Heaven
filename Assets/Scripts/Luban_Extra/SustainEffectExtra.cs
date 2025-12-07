using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

namespace cfg{
    [Serializable]
    // 因为SustainEffect是持续性的，可能会在存在过程中改变层数而修改其行为，所以需要一个方法来处理这种情况
    // 不是是同一种效果就能叠加，而且必须要是同源的
    public abstract partial class SustainEffect : ICanGetSystem, ICanRegisterEvent {
        public string guid = Guid.NewGuid().ToString(); // 持续性效果的唯一标识
        public IArchitecture GetArchitecture() => GameArchitecture.Interface;
        public int StackNumber { get; protected set; } = 1;
        public SustainEffect(){}
        public abstract void OnAdd(object sender);
        public abstract void OnRemove(object sender);
        public abstract void OnChangeStack(object sender, int newStackNumber);
        public abstract SustainEffect Clone();
    }

    // 主要用于监听事件类型的吉祥物
    [Serializable]
    public partial class SE_监听事件 : SustainEffect{
        private IUnRegister eventUnRegister;

        public SE_监听事件(SE_监听事件 se){
            this.Evt = se.Evt;
            this.Actions = new List<CGA>(se.Actions.Select(x => new CGA(x)));
        }
        public override void OnAdd(object sender)
        {
            eventUnRegister = EventBinder.Convert(Evt, this, () => ApplyActions(sender));
        }
        public override void OnRemove(object sender)
        {
            eventUnRegister.UnRegister();
        }
        public override void OnChangeStack(object sender, int newStackNumber)
        {
            int diff = newStackNumber - StackNumber;
            StackNumber = newStackNumber;
        }
        private void ApplyActions(object sender)
        {
            this.GetSystem<IGASystem>().SendAction(sender, () => {
                foreach (var action in Actions){
                    action.ApplyMultiplier(StackNumber);
                    this.GetSystem<IGASystem>().ApplyCGA(sender, action, null);
                }
            });
        }
        public override SustainEffect Clone() => new SE_监听事件(this);
    }

    // 主要用于被动类的吉祥物（例如：最大补充数+2）
    public partial class SE_基于线性GA : SustainEffect{
        public SE_基于线性GA(SE_基于线性GA se){
            this.Actions = new List<GameAction>(se.Actions.Select(x => x.Clone()));
            this.OnRemoveAction = new List<GameAction>(se.OnRemoveAction.Select(x => x.Clone()));
        }
        public override void OnAdd(object sender)
        {
            foreach (var action in Actions){
                action.ApplyMultiplier(StackNumber);
                this.GetSystem<IGASystem>().ApplyGA(sender, action, null);
            }
        }
        public override void OnChangeStack(object sender, int newStackNumber)
        {
            // 2. 再添加新的效果
            int diff = newStackNumber - StackNumber;
            StackNumber = newStackNumber;
            foreach (var action in Actions){
                action.ApplyMultiplier(diff);
                this.GetSystem<IGASystem>().ApplyGA(sender, action, null);
            }
        }
        public override void OnRemove(object sender)
        {
            foreach (var action in OnRemoveAction){
                action.ApplyMultiplier(StackNumber);
                this.GetSystem<IGASystem>().ApplyGA(sender, action, null);
            }
        }
        public override SustainEffect Clone() => new SE_基于线性GA(this);
    }
}