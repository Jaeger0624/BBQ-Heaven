using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class FoodInstanceMover : ICanGetSystem, ICanSendEvent
{
    private Rng rng => this.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();
    public IArchitecture GetArchitecture() =>
        GameArchitecture.Interface;
    
    public MoveFoodInstanceEvent MoveFoodInstanceTo(Vector2Int newPos, FoodInstance foodInstance, bool showAnimDirect = false){
        // Debug.Log($"【FoodInstanceMover】移动食材实例到指定位置: {newPos}，食材实例GUID: {foodInstance.guid}");
        // 1. 获取食材实例
        Vector2Int originPosition = foodInstance.position;

        BoardCell cell = this.GetSystem<IBoardSystem>().GetCell(newPos);
        if (!cell.IsEmpty()){
            Debug.LogError($"【FoodSystem】移动食材实例到指定位置失败: {newPos}，已有实例: {cell.instanceGuid}");
            return null;
        }

        // 2. 从棋盘上移除
        this.GetSystem<IBoardSystem>().SetCellInstance(originPosition, null);

        // 3. 移到指定位置
        return this.MoveTo(foodInstance, cell, showAnimDirect);
    }

    // 先将食材实例从棋盘上移除，然后随机移动到棋盘上的空位
    public List<MoveFoodInstanceEvent> RandomMoveFoodInstances(List<FoodInstance> foodInstances, bool showAnimDirect = false){
        Debug.Log($"【FoodInstanceMover】随机移动食材实例: {foodInstances.Count}");
        // 1. 先将选中食材实例从棋盘上移除
        foodInstances.ForEach(foodInstance => {
            Vector2Int originPosition = foodInstance.position;
            this.GetSystem<IBoardSystem>().SetCellInstance(originPosition, null);
        });

        // 2. 获取棋盘上的空位
        List<BoardCell> emptyCells = this.GetSystem<IBoardSystem>().GetEmptyCells();
        List<MoveFoodInstanceEvent> moveEvents = new List<MoveFoodInstanceEvent>();
        // 3. 随机移动到棋盘上的空位
        foreach (var foodInstance in foodInstances){
            BoardCell emptyCell = rng.PickOne(emptyCells);
            moveEvents.Add(this.MoveTo(foodInstance, emptyCell, showAnimDirect));
            emptyCells.Remove(emptyCell);
        }
        return moveEvents;
    }

    /// <summary>
    /// 前提是食材实例已经被清理
    /// </summary>
    /// <param name="foodInstance"></param>
    /// <param name="cell"></param>
    private MoveFoodInstanceEvent MoveTo(FoodInstance foodInstance, BoardCell cell, bool showAnimDirect = false){
        foodInstance.position = cell.position;
        cell.SetInstance(foodInstance.guid);

        return new MoveFoodInstanceEvent(cell.position, foodInstance.position, foodInstance.guid);
    }



    public MoveFoodInstanceEvent FoodInstanceLineMove(FoodInstance foodInstance, Vector2Int direction, bool showAnimDirect = false){
        BoardCell stopPosition = this.GetSystem<IBoardSystem>().GetStopPosition(foodInstance.position, direction);
        if (stopPosition.position == foodInstance.position){
            return null;
        }
        int distance = (int)Vector2Int.Distance(foodInstance.position, stopPosition.position);
        return MoveTo(foodInstance, stopPosition, showAnimDirect);
    }

    public List<MoveFoodInstanceEvent> ExchangeFoodInstances(FoodInstance first, FoodInstance second, bool showAnimDirect = false){
        List<MoveFoodInstanceEvent> moveEvents = new List<MoveFoodInstanceEvent>();
        
        // 1. 分别从棋盘上移除
        this.GetSystem<IBoardSystem>().SetCellInstance(first.position, null);
        this.GetSystem<IBoardSystem>().SetCellInstance(second.position, null);
        // 2. 交换位置
        Vector2Int firstPosition = first.position;
        Vector2Int secondPosition = second.position;
        first.position = secondPosition;
        second.position = firstPosition;
        BoardCell firstCell = this.GetSystem<IBoardSystem>().GetCell(firstPosition);
        BoardCell secondCell = this.GetSystem<IBoardSystem>().GetCell(secondPosition);
        // 3. 分别移到新的位置
        moveEvents.Add(MoveTo(first, secondCell, showAnimDirect));
        moveEvents.Add(MoveTo(second, firstCell, showAnimDirect));
        return moveEvents;
    }
}