using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UniRx;
using UnityEngine;

namespace cfg{
    public partial class GA_增加顾客等待值 : GameAction
    {
        public GA_增加顾客等待值(DynamicValue value, GetCustomerInfo info)
        {
            this.Value = value;
            this.Info = info;
        }
        public override GameAction Clone()
        {
            return new GA_增加顾客等待值(Value, Info);
        }

        public override void Execute(object sender, List<object> param)
        {
            Debug.Log($"【GA_增加顾客等待值】执行: {Value.GetValue(sender, param)}");
            List<Customer> customers = Info.GetCustomers(sender, param);
            if (customers == null || customers.Count == 0){
                Debug.LogError("【GA_增加顾客等待值】没有顾客");
                return;
            }
            int value = Value.GetValue(sender, param);
            customers.ForEach(customer => {
                customer.ChangePatience(value);
            });
        }

        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }


    public partial class GA_定点爆破 : GameAction
    {
        public GA_定点爆破(GetCellInfo info)
        {
            this.Info = info;
        }
        public override GameAction Clone() => new GA_定点爆破(Info);

        public override void Execute(object sender, List<object> param){
            
            // 获取目标地块
            List<BoardCell> cells = Info.GetCells(sender, param);

            foreach (var cell in cells){

            // 选取cell周围的食材，让其向反方向移动
            BoardCell upCell = this.GetSystem<IBoardSystem>().GetCell(cell.position + Vector2Int.up);
            BoardCell downCell = this.GetSystem<IBoardSystem>().GetCell(cell.position + Vector2Int.down);
            BoardCell leftCell = this.GetSystem<IBoardSystem>().GetCell(cell.position + Vector2Int.left);
            BoardCell rightCell = this.GetSystem<IBoardSystem>().GetCell(cell.position + Vector2Int.right);

            if (upCell != null){
                FoodInstance foodInstance = this.GetSystem<IBoardEntitySystem>().GetEntity(upCell.instanceGuid) as FoodInstance;
                if (foodInstance != null){
                    this.GetSystem<IBoardEntitySystem>().Mover.DirectionalMove(foodInstance, Vector2Int.up, 1);
                }
            }
            if (downCell != null){
                FoodInstance foodInstance = this.GetSystem<IBoardEntitySystem>().GetEntity(downCell.instanceGuid) as FoodInstance;
                if (foodInstance != null){
                    this.GetSystem<IBoardEntitySystem>().Mover.DirectionalMove(foodInstance, Vector2Int.down, 1);
                }
            }
            if (leftCell != null){
                FoodInstance foodInstance = this.GetSystem<IBoardEntitySystem>().GetEntity(leftCell.instanceGuid) as FoodInstance;
                if (foodInstance != null){
                    this.GetSystem<IBoardEntitySystem>().Mover.DirectionalMove(foodInstance, Vector2Int.left, 1);
                }
            }
            if (rightCell != null){
                FoodInstance foodInstance = this.GetSystem<IBoardEntitySystem>().GetEntity(rightCell.instanceGuid) as FoodInstance;
                if (foodInstance != null){
                    this.GetSystem<IBoardEntitySystem>().Mover.DirectionalMove(foodInstance, Vector2Int.right, 1);
                }
            }
            }
        }
        public override IAnimTask GetAnimTask() => new EmptyAnimTask();
    }


    public partial class GA_使顾客离开 : GameAction
    {
        public GA_使顾客离开(GetCustomerInfo info)
        {
            this.Info = info;
        }
        public override GameAction Clone()
        {
            return new GA_使顾客离开(Info);
        }

        public override void Execute(object sender, List<object> param)
        {
            List<Customer> customers = Info.GetCustomers(sender, param);
            if (customers == null || customers.Count == 0){
                Debug.LogError("【GA_使顾客离开】没有顾客");
                return;
            }
            this.GetSystem<ICustomerSystem>().LeaveCustomer(customers);
        }

        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }


    public partial class GA_效果选择 : GameAction
    {
        public GA_效果选择(List<string> effectIDs)
        {
            this.Options = effectIDs;
        }
        public override GameAction Clone() => new GA_效果选择(Options);
        public override void Execute(object sender, List<object> param)
        {
            List<OptionData> options = Options.Select(x => this.GetSystem<IDataSystem>().GetOptionData(x)).ToList();
            this.GetSystem<ISelectionSystem>().RequestSelection(new SelectionRequest_效果选项(options, sender, param), 0, SelectionPanelType.通用选择);
        }
        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }

    public partial class GA_修改稀有度概率 : GameAction
    {
        public GA_修改稀有度概率(RankProbability probs)
        {
            this.Probs = probs;
        }
        public override GameAction Clone() => new GA_修改稀有度概率(Probs);

        public override void Execute(object sender, List<object> param)
        {
            Dictionary<Rank, float> probabilities = new Dictionary<Rank, float>();
            probabilities.Add(Rank.普通, Probs.Probability普通);
            probabilities.Add(Rank.稀有, Probs.Probability稀有);
            probabilities.Add(Rank.史诗, Probs.Probability史诗);
            probabilities.Add(Rank.传说, Probs.Probability传说);
            ProbabilityInfo probabilityInfo = new ProbabilityInfo(probabilities);
            this.GetSystem<IRandomSystem>().SetProbability(probabilityInfo);
        }
        public override IAnimTask GetAnimTask() => new EmptyAnimTask();
    }
    public partial class GA_获得收入 : GameAction
    {
        public GA_获得收入(DynamicValue value)
        {
            this.Value = value;
        }
        public override GameAction Clone() => new GA_获得收入(Value);

        public override void Execute(object sender, List<object> param)
        {
            this.GetSystem<IEconomySystem>().ChangeIncome(Value.GetValue(sender, param));
        }

        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }

    public partial class GA_随机触发 : GameAction
    {
        public GA_随机触发(List<GameAction> actions)
        {
            this.Actions = actions;
        }
        public override GameAction Clone() => new GA_随机触发(Actions);
        public override void Execute(object sender, List<object> param)
        {
            if (Actions == null || Actions.Count == 0) return;

            Rng rng = this.GetSystem<IRngSystem>().GetSubRng<IGASystem>();
            GameAction randomAction = rng.PickOne(Actions);
            this.GetSystem<IGASystem>().TriggerReaction(randomAction, sender, param);
        }
        public override IAnimTask GetAnimTask() => new EmptyAnimTask();
    }

    public partial class GA_随机选卡 : GameAction
    {
        public GA_随机选卡(DynamicValue value)
        {
            this.Value = value;
        }
        public override GameAction Clone() => new GA_随机选卡(Value);
        public override void Execute(object sender, List<object> param)
        {
            int count = Value.GetValue(sender, param);
            if (count <= 0){
                Debug.LogError("【GA_随机选卡】选卡数量不能小于等于0");
                return;
            }

            // 发起选择请求
            this.GetSystem<ISelectionSystem>().RequestSelection(new SelectionRequest_随机卡牌(count), count, SelectionPanelType.通用选择);
        }
        public override IAnimTask GetAnimTask() => new EmptyAnimTask();
    }
}
