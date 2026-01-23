using System;
using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;
public enum BoardEntityType{
    食材,
    非食材,
}
public interface ICellPosition{
    Vector2Int GetCellPosition();
}
// 负责处理运动、图像、显示等
public abstract class BoardEntity : ICanGetSystem, ITooltipData, ICellPosition{
    public string guid { get; private set; }
    public abstract string name { get; }
    public abstract object data { get;}
    public Vector2Int position;
    public abstract BoardEntityType type { get; }
    public Vector2Int lastMoveDirection = Vector2Int.zero;

    public BoardEntity(){
        this.guid = Guid.NewGuid().ToString();
        this.position = new Vector2Int(-1, -1);
    }
    public void UpdateLastMoveDirection(Vector2Int direction){
        if (direction != Vector2Int.zero){
            this.lastMoveDirection = direction;
        }
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract List<TooltipInfo> GetTooltipInfos();


    // Helper methods
    public abstract Sprite GetSprite();
    public Vector2Int GetCellPosition() => position;
}
