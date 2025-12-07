using UnityEngine;

public interface IGridComponent{
    public Vector2Int position { get; }
    string DebugInfo();
    void SetPosition(Vector2Int position);
}

public class Integer : IGridComponent{
    public Vector2Int position { get; private set; }
    public int value { get; set; }
    public Integer(int value){
        this.value = value;
    }
    public void SetPosition(Vector2Int position){
        this.position = position;
    }
    public string DebugInfo(){
        return $"{value}";
    }
}