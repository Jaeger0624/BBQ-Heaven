using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UniRx;
using UnityEngine;

namespace cfg{

    # region 功能性GA，本身不带有意义
    public partial class GA_Action : GameAction
    {
        Action action;
        public GA_Action(Action action){
            this.action = action;
        }
        public override GameAction Clone()
        {
            return new GA_Action(action);
        }

        public override void Execute(object sender, List<object> param)
        {
            action?.Invoke();
        }

        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }

        public override int GetTypeId()
        {
            return 124532355;
        }
    }

    public class GA_WaitForEvent : GameAction
    {
        // 这是一个 Subject，充当信号枪
        // 你可以在 UI 按钮点击时调用 trigger.OnNext(Unit.Default)
        private Subject<Unit> _trigger = new Subject<Unit>();

        // 监听事件
        IUnRegister eventUnRegister;
        public void Trigger(string triggerName)
        {
            Debug.Log($"<color=yellow>【GA_WaitForEvent】触发事件: {triggerName}</color>");
            eventUnRegister.UnRegister();
            _trigger.OnNext(Unit.Default);
            _trigger.OnCompleted();
        }
        public override IObservable<GAResult> ExecuteAsync(object sender, List<object> param)
        {
            // 这里我们把 trigger 暴露出去，或者注册到某个系统里让别人能访问到
            // 比如 UIManager.ShowConfirmPanel(this.Trigger);

            eventUnRegister = this.GetArchitecture().RegisterEvent<TriggerGAEvent>(evt => Trigger(evt.triggerName));
            return _trigger
                .Take(1) //以此确保只触发一次
                .Select(_ => GAResult.Empty);
        }
        public override void Execute(object sender, List<object> param) { }
        public override IAnimTask GetAnimTask() => new EmptyAnimTask();
        public override GameAction Clone() => new GA_WaitForEvent();
        public override int GetTypeId()
        {
            return 124532356;
        }
    }



    #endregion
    
    // 注意GA原子化，串串顺序执行其实应该拆成基础值逐一增加
    public partial class GA_添加单个食材基础值 : GameAction
    {
        private FoodInstance foodInstance;
        private FoodInstanceViewAnimEvent foodInstanceViewAnimEvent;
        private AddBBQPropertyAnimEvent addBBQPropertyEvent;
        private PlaceFoodInstanceEvent placeFoodInstanceEvent;
        public GA_添加单个食材基础值(FoodInstance foodInstance){
            this.foodInstance = foodInstance;
        }
        public override GameAction Clone() => new GA_添加单个食材基础值(foodInstance);
        public override void Execute(object sender, List<object> param)
        {
            // 读取上下文
            BBQProcessContext context = param?.FirstOrDefault() as BBQProcessContext;
            if (context == null){
                Debug.LogError("上下文为空");
                return;
            }
            BBQ currentBBQ = context.targetBBQ;
            if (currentBBQ == null) {Debug.LogError("当前烧烤为空"); return;}

            int foodBaseRarity = (int)foodInstance.rarity;
            int foodBaseTaste = (int)foodInstance.taste;

            // 4. 判断是否暴击
            float multiplier = 1f;
            var rng = this.GetSystem<IRngSystem>().GetSubRng<IBBQSystem>();
            if (rng.NextFloat() < foodInstance.baseCritRate){
                multiplier = foodInstance.baseCritMultiplier;
                Debug.Log($"【GA_添加单个食材基础值】食材实例{foodInstance.name}暴击，倍率：{multiplier}\n原值: {foodBaseRarity}|{foodBaseTaste} -> 新值: {foodBaseRarity * multiplier}|{foodBaseTaste * multiplier}");
            }


            currentBBQ.SetTotalRarity((int)(currentBBQ.totalRarity.Value + foodBaseRarity * multiplier));
            currentBBQ.SetTotalTaste((int)(currentBBQ.totalTaste.Value + foodBaseTaste * multiplier));

            foodInstanceViewAnimEvent = new FoodInstanceViewAnimEvent(foodInstance.guid);
            addBBQPropertyEvent = new AddBBQPropertyAnimEvent(currentBBQ.totalRarity.Value, currentBBQ.totalTaste.Value, foodBaseRarity, foodBaseTaste, $"{foodInstance.food.foodData.Name}");
            placeFoodInstanceEvent = new PlaceFoodInstanceEvent(foodInstance.foodInstanceView, currentBBQ.foodInstances.IndexOf(foodInstance));
        }

        public override IAnimTask GetAnimTask()
        {
            IAnimTask animTask = new SequenceAnimTask(new List<IAnimTask>{
                new ActionAnimTask(() => this.SendEvent(placeFoodInstanceEvent)),
                new DelayAnimTask(0.1f),
                new ActionAnimTask(() => this.SendEvent(foodInstanceViewAnimEvent)),
                new ActionAnimTask(() => this.SendEvent(addBBQPropertyEvent)),
                new DelayAnimTask(SettingManager.Instance.AnimSettings.foodInstanceOnStickAnimInterval, true),

            });
            IAnimTask anim = new SequenceAnimTask(new List<IAnimTask>{
                new ActionAnimTask(() => this.SendEvent(placeFoodInstanceEvent)),
                new DelayAnimTask(0.1f),
                new ActionAnimTask(() => this.SendEvent(foodInstanceViewAnimEvent)),
                new ActionAnimTask(() => this.SendEvent(addBBQPropertyEvent)),
                new DelayAnimTask(SettingManager.Instance.AnimSettings.foodInstanceOnStickAnimInterval, true),
            });
            return anim;
        }

        public override int GetTypeId()
        {
            return 124532353;
        }
    }

    public partial class GA_添加所有食材基础值 : GameAction
    {
        private AddBBQPropertyAnimEvent addBBQPropertyEvent;
        private List<FoodInstance> foodInstances;
        public GA_添加所有食材基础值(List<FoodInstance> foodInstances){
            this.foodInstances = foodInstances;
        }
        public override GameAction Clone() => new GA_添加所有食材基础值(foodInstances);
        public override void Execute(object sender, List<object> param)
        {
            // 读取上下文
            BBQProcessContext context = param?.FirstOrDefault() as BBQProcessContext;
            if (context == null){
                Debug.LogError("上下文为空");
                return;
            }
            BBQ currentBBQ = context.targetBBQ;
            if (currentBBQ == null) {Debug.LogError("当前烧烤为空"); return;}

            int raritySum = foodInstances.Sum(x => (int)x.food.foodData.Rarity);
            int tasteSum = foodInstances.Sum(x => (int)x.food.foodData.Taste);
            currentBBQ.SetTotalRarity(currentBBQ.totalRarity.Value + raritySum);
            currentBBQ.SetTotalTaste(currentBBQ.totalTaste.Value + tasteSum);

            addBBQPropertyEvent = new AddBBQPropertyAnimEvent(currentBBQ.totalRarity.Value, currentBBQ.totalTaste.Value, raritySum, tasteSum, $"所有食材基础值");
        }

        public override IAnimTask GetAnimTask()
        {
            IAnimTask anim = new SequenceAnimTask(new List<IAnimTask>{
                new ActionAnimTask(() => this.SendEvent(addBBQPropertyEvent)),
                new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
            });
            return anim;
        }

        public override int GetTypeId()
        {
            return 124532354;
        }
    }
}