using UnityEngine;

public class BoardCell : IGridComponent{
    public Vector2Int position { get; private set; } = new Vector2Int(-1, -1);
    public string instanceGuid { get; private set; } // 实例GUID，不一定是食材
    public bool Abandoned;  // 是否被放弃
    public BoardCell(string instanceGuid){
        this.instanceGuid = instanceGuid;
    }
    public void SetInstance(string instanceGuid) => this.instanceGuid = instanceGuid;
    public string DebugInfo() => $"{instanceGuid}";
    public void SetPosition(Vector2Int position) => this.position = position;
    public bool IsEmpty() => instanceGuid == null;

}