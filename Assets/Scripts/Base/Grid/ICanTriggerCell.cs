using UnityEngine;

public interface ICanTriggerCell<T> where T : IGridComponent{
    public void OnTriggerCell(Grid<T> grid, Vector2Int cellPos);
}