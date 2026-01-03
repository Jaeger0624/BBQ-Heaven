using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

public class TileHandler : ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public void EntityMove(MoveEntityEvent e){
        BoardCell oldCell = this.GetSystem<IBoardSystem>().GetCell(e.originPosition);
        BoardCell newCell = this.GetSystem<IBoardSystem>().GetCell(e.newPos);
        BoardEntity entity = e.entity;
        Direction direction = e.direction.ToDirection();
        TileContext context = new TileContext(entity, newCell, direction);
        TriggerTileEffect(context, TileEffectType.实体移动进入);

        TileContext oldContext = new TileContext(entity, oldCell, direction);
        TriggerTileEffect(oldContext, TileEffectType.实体移动离开);
    }

    public void EntityPlaced(PlaceEntityEvent e){
        BoardCell cell = this.GetSystem<IBoardSystem>().GetCell(e.newPos);
        BoardEntity entity = e.entity;
        TileContext context = new TileContext(entity, cell, Direction.无);
        TriggerTileEffect(context, TileEffectType.实体被放置);

    }
    private void TriggerTileEffect(TileContext context, TileEffectType type){
        BoardCell cell = context.boardCell;
        if (cell == null){ Debug.LogError($"【TileHandler】地块不存在"); return; }
        if (cell.TileID == null){ Debug.LogError($"【TileHandler】地块没有地块ID"); return; }
        TileData tileData = this.GetSystem<IDataSystem>().GetTileData(cell.TileID);
        if (tileData == null){ Debug.LogError($"【TileHandler】地块数据不存在: {cell.TileID}"); return; }


        switch (type){
            case TileEffectType.实体被放置:
                // Debug.Log($"【TileHandler】实体被放置: {context.boardEntity.name} 放置 {context.boardCell.position}");
                break;
            case TileEffectType.实体移动进入:
                Debug.Log($"【TileHandler】实体移动进入: {context.boardEntity.name} 进入 {context.boardCell.position}");
                foreach (var cga in tileData.CGAs){
                    if (cga.Type == TileEffectType.实体移动进入){
                        this.GetSystem<IGASystem>().ApplyCGA(context.boardEntity, cga.Action, new List<object>{context});
                    }
                }
                break;
            case TileEffectType.实体移动离开:
                Debug.Log($"【TileHandler】实体移动离开: {context.boardEntity.name} 离开 {context.boardCell.position}");
                break;
        }
    }
}


