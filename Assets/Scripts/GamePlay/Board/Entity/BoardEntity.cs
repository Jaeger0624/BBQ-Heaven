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
    public BoardEntity(){
        this.guid = Guid.NewGuid().ToString();
        this.position = new Vector2Int(-1, -1);
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}
