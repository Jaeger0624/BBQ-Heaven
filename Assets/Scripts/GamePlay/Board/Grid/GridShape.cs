using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using UnityEngine;

[Serializable]
public class GridShape{
    public List<Vector2Int> cells;
    private int rotate = 0;
    public int Rotate{
        get{
            return rotate;
        }
        set{
            rotate = value % 4;
        }
    }
    public GridShape(List<Vector2Int> cells)
    {
        this.cells = cells.ToList();
    }
    public List<Vector2Int> GetRotatedCells(int rotation)
    {
        // 只能是0,1,2,3
        if (rotation < 0 || rotation > 3)
        {
            Debug.LogError("GetRotatedCells: rotation 只能是1,2,3");
            return cells;
        }

        var result = new List<Vector2Int>();
        foreach (var p in cells)
        {
            // 2D 网格旋转公式 (顺时针 90度: (x,y) -> (y, -x))
            int x = p.x;
            int y = p.y;
            
            for (int i = 0; i < rotation; i++)
            {
                int temp = x;
                x = y;
                y = -temp;
            }
            result.Add(new Vector2Int(x, y));
        }
        return result;
    }

    public Vector2Int GetRotatedLength()
    {
        int originWidth = cells.Max(x => x.x) - cells.Min(x => x.x);
        int originHeight = cells.Max(x => x.y) - cells.Min(x => x.y);

        if (rotate == 1 || rotate == 3){
            return new Vector2Int(originHeight, originWidth);
        }
        return new Vector2Int(originWidth, originHeight);
    }


    // ============== 静态方法 ==============
    public static GridShape Get(string id)
    {
        var data = GameArchitecture.Interface.GetSystem<IDataSystem>().GetGridShapeData(id);
        return CreateGridShape(data);
    }
    public static GridShape CreateGridShape(GridShapeData data)
    {
        return CreateGridShape(data.Type, data.Pattern);
    }
    public static GridShape CreateGridShape(GridShapeType type, string pattern)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        switch (type)
        {
            case GridShapeType.标准:
                string[] points = pattern.Split(';');
                // 每个点是"(x,y)"
                foreach (var point in points)
                {
                    var p = point.Trim('(', ')').Split(',');
                    cells.Add(new Vector2Int(int.Parse(p[0]), int.Parse(p[1])));
                }
                return new GridShape(cells);
            case GridShapeType.矩形:
                string[] rect = pattern.Split(';');
                string offset = rect[0];
                string size = rect[1];
                var o = offset.Split(',');
                var s = size.Split(',');
                for (int i = 0; i < int.Parse(s[0]); i++)
                {
                    for (int j = 0; j < int.Parse(s[1]); j++)
                    {
                        cells.Add(new Vector2Int(int.Parse(o[0]) + i, int.Parse(o[1]) + j));
                    }
                }
                return new GridShape(cells);
            default:
                Debug.LogError($"CreateGridShape: 不支持的网格形状类型: {type}");
                return null;
        }
    }
}