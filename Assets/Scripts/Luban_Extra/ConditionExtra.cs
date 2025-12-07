using System.Collections.Generic;
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

        if (foodInstance.state != FoodInstanceState.棋盘上) return false;

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
}
