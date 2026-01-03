using System;
using QFramework;
using UnityEngine;
public enum BoardEntityType{
    食材,
    非食材,
}
public abstract class BoardEntity : ICanGetSystem{
    public string guid { get; private set; }
    public abstract string name { get; }
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
}
