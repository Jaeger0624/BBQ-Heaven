using System.Collections.Generic;
using QFramework;
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
            List<Customer> customers = Info.GetCustomers(sender, param);
            if (customers == null || customers.Count == 0){
                Debug.LogError("【GA_增加顾客等待值】没有顾客");
                return;
            }
            customers.ForEach(customer => {
                customer.PatienceNow.Value += Value.GetValue(sender, param);
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
}
