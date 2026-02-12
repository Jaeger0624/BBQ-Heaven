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

            bool isCritical = rng.NextFloat() < foodInstance.baseCritRate;

            multiplier = isCritical ? foodInstance.baseCritMultiplier : 1f;
            // 向上取整
            int newFoodBaseRarity = (int)Mathf.Ceil(foodBaseRarity * multiplier);
            int newFoodBaseTaste = (int)Mathf.Ceil(foodBaseTaste * multiplier);

            if (isCritical){
                Debug.Log($"【GA_添加单个食材基础值】食材实例{foodInstance.name}暴击，倍率：{multiplier}\n原值: {foodBaseRarity}|{foodBaseTaste} -> 新值: {newFoodBaseRarity}({foodBaseRarity*multiplier})|{newFoodBaseTaste}({foodBaseTaste*multiplier})");
            }

            currentBBQ.SetTotalRarity((int)(currentBBQ.totalRarity.Value + newFoodBaseRarity));
            currentBBQ.SetTotalTaste((int)(currentBBQ.totalTaste.Value + newFoodBaseTaste));

            foodInstanceViewAnimEvent = new FoodInstanceViewAnimEvent(foodInstance.guid);
            addBBQPropertyEvent = new AddBBQPropertyAnimEvent(currentBBQ.totalRarity.Value, currentBBQ.totalTaste.Value, newFoodBaseRarity, newFoodBaseTaste, $"{foodInstance.food.foodData.Name}", isCritical);
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

            addBBQPropertyEvent = new AddBBQPropertyAnimEvent(currentBBQ.totalRarity.Value, currentBBQ.totalTaste.Value, raritySum, tasteSum, $"所有食材基础值", false);
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