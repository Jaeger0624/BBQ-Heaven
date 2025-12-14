using cfg;
using QFramework;
using UnityEngine;

public class BoardCell : IGridComponent, ICanGetSystem{
    public Vector2Int position { get; private set; } = new Vector2Int(-1, -1);
    public string instanceGuid { get; private set; } // 实例GUID，不一定是食材

    // 地块信息
    public string TileID {get; private set; }
    public TileData TileData => this.GetSystem<IDataSystem>().GetTileData(TileID);
    public BoardCell(string instanceGuid, string tileID = null){
        this.instanceGuid = instanceGuid;
        this.TileID = tileID;
    }
    public void SetInstance(string instanceGuid) => this.instanceGuid = instanceGuid;
    public string DebugInfo() => $"{instanceGuid}";
    public void SetPosition(Vector2Int position) => this.position = position;
    public bool IsEmpty() => instanceGuid == null;

    // 修改地块信息
    public void SetTile(string tileID){
        this.TileID = tileID;
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}