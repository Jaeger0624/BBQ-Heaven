using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

namespace cfg{
public abstract partial class Condition : ICanGetSystem, IHaveAnim{

    public abstract bool Evaluate(object sender, List<object> param);

    public abstract IAnimTask GetAnimTask();
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}

public partial class Condition_DV检测 {

    public override bool Evaluate(object sender, List<object> param)
    {
        return ExtraTool.BoolValue(Sign, DV1.GetValue(sender, param), DV2.GetValue(sender, param));
    }
    public override IAnimTask GetAnimTask()
    {
        return new EmptyAnimTask();
    }
}



public partial class Condition_当前交易烧烤食材数量
{
    public override bool Evaluate(object sender, List<object> param)
    {
        return false;
    }
    public override IAnimTask GetAnimTask()
    {
        return new EmptyAnimTask();
    }
}

public partial class Condition_食材周围空位
{
    public override bool Evaluate(object sender, List<object> param)
    {
        if (sender == null || !(sender is FoodInstance foodInstance)) return false;

        if (foodInstance.state != FoodInstanceState.棋盘上 && foodInstance.state != FoodInstanceState.被选中){
            // Debug.Log($"【Condition_食材周围空位】食材不在棋盘上: {foodInstance.name}, 当前状态：{foodInstance.state}");
            return false;
        }

        // 检测食材周围是否有空位
        Vector2Int instancePos = foodInstance.position;

        // 获取周围4个位置
        List<Vector2Int> surroundingPositions = new List<Vector2Int>{
            new Vector2Int(instancePos.x + 1, instancePos.y),
            new Vector2Int(instancePos.x - 1, instancePos.y),
            new Vector2Int(instancePos.x, instancePos.y + 1),
            new Vector2Int(instancePos.x, instancePos.y - 1)};
        int emptyCount = 0;
        foreach (Vector2Int surroundingPos in surroundingPositions){
            BoardCell surroundingCell = this.GetSystem<IBoardSystem>().GetCell(surroundingPos);
            if (surroundingCell == null) continue;
            if (surroundingCell.instanceGuid == null) emptyCount++;
        }

        return ExtraTool.BoolValue(Sign, emptyCount, Amount);
    }
    public override IAnimTask GetAnimTask()
    {
        return new EmptyAnimTask();
    }

}


public partial class Condition_位于首尾
{
    public override bool Evaluate(object sender, List<object> param)
    {
        if (!(sender is FoodInstance foodInstance)) {Debug.LogError($"【Condition_位于首尾】该条件只能由FoodInstance触发");return false;}
        if (foodInstance.state != FoodInstanceState.烤串上){
            Debug.LogError($"【Condition_位于首尾】食材不在烤串上: {foodInstance.name}, 当前状态：{foodInstance.state}");
            return false;
        }
        BBQProcessContext context = param?.FirstOrDefault() as BBQProcessContext;
        if (context == null) {Debug.LogError("上下文为空");return false;}
        List<FoodInstance> foodInstances = context.targetBBQ.foodInstances;
        if (!foodInstances.Contains(foodInstance)) {Debug.LogError($"【Condition_位于首尾】食材不在当前烧烤中: {foodInstance.name}");return false;}
        int index = foodInstances.IndexOf(foodInstance);

        if (Type == 0){
            return index == 0;
        }
        else if (Type == 1){
            return index == foodInstances.Count - 1;
        }
        else if (Type == 2){
            return index == 0 || index == foodInstances.Count - 1;
        }
        else{
            Debug.LogError($"【Condition_位于首尾】类型错误: {Type}");
            return false;
        }
    }
    public override IAnimTask GetAnimTask()
    {
        return new EmptyAnimTask();
    }
}

public partial class Condition_当前顾客要求全满足
{
    public override bool Evaluate(object sender, List<object> param)
    {
        DealContext context = param?.FirstOrDefault() as DealContext;
        if (context == null) {Debug.LogError("上下文为空");return false;}
        ReviewResult reviewResult = context.Customer.Review(context);
        // 检查要求是否全满足
        return reviewResult.Records.All(x => x.IsMet);
    }
    public override IAnimTask GetAnimTask()
    {
        return new EmptyAnimTask();
    }
}
}
