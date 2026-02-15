using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class GridBase
{
    public int width;
    public int height;
    public abstract string OnShow(int x, int y);
    public abstract void ChangeGrid(int width, int height);
}
// 网格系统
public class Grid<T> : GridBase where T : IGridComponent
{
    private T[,] cells;
    private Func<T> createCell;
    public Grid(int width, int height, Func<T> createCell)
    {
        base.width = width;
        base.height = height;
        this.createCell = createCell;
        this.cells = new T[width, height];
        // 初始化每个cell
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                this.cells[i, j] = CreateNewCell(i, j);
                this.cells[i, j].SetPosition(new Vector2Int(i, j));
            }
        }
    }
    public override void ChangeGrid(int width, int height)
    {
        int oldWidth = this.width;
        int oldHeight = this.height;
        this.width = width;
        this.height = height;
        // 默认的cell都是null

        T[,] newCells = new T[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                newCells[i, j] = (i < oldWidth && j < oldHeight) ? cells[i, j] : CreateNewCell(i, j);    
            }
        }
        cells = newCells;
    }
    public void SetCell(int x, int y, T value)
    {
        cells[x, y] = value;
    }
    public T GetCell(int x, int y)
    {
        if (!IsValidPosition(x, y))
        {
            Debug.LogError($"【Grid】获取格子失败: {x},{y} 不存在");
            return default;
        }
        return cells[x, y];
    }
    private T CreateNewCell(int x, int y) => this.createCell();
    public List<T> GetAllCells() => cells.Cast<T>().ToList();
    public override string OnShow(int x, int y){
        return $"({x},{y})\n{cells[x, y].DebugInfo()}";
    }

    private bool IsValidPosition(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

}
