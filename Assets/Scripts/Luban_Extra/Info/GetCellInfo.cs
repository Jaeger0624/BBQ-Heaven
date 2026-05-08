using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UniRx;
using UnityEngine;

namespace cfg{
    public partial class GetCellInfo : ICanGetSystem{
        public IArchitecture GetArchitecture() => GameArchitecture.Interface;
        private Rng rng => this.GetSystem<IRngSystem>().GetSubRng<IBoardSystem>();
        public GetCellInfo(GetCellStrategy strategy, bool isRandom){
            this.Strategy = strategy;
            this.IsRandom = isRandom;
        }
        public IObservable<BoardCell> GetCell(object origin, List<object> param){
            List<BoardCell> cells = GetCells(origin, param);
            if (cells == null || cells.Count == 0){
                Debug.LogWarning($"GetCellInfo: 没有目标格子");
                return Observable.Return<BoardCell>(null);
            }
            if (IsRandom){
                BoardCell cell = rng.PickOne(cells);
                if (cell == null){
                    Debug.LogWarning($"GetCellInfo: 随机选择目标格子失败");
                    return Observable.Return<BoardCell>(null);
                }
                return Observable.Return(cell);
            }
            else{
                return this.GetSystem<ISelectionSystem>().SelectCell(cells);
            }
        }
        
        public List<BoardCell> GetCells(object sender, List<object> param){

            ICellPosition center = null;

            // 获取中心
            if (Strategy == GetCellStrategy.自己){
                center = sender as ICellPosition;
            }
            else if (Strategy == GetCellStrategy.选取){
                center = param.FirstOrDefault(x => x is ICellPosition) as ICellPosition;
            }
            else{
                Debug.LogError($"GetCellInfo: 不支持的策略: {Strategy}");
                return null;
            }
            if (center == null){
                Debug.LogError($"GetCellInfo: 没有目标格子");
                return null;
            }

            // 获取网格形状
            GridShape shape = GridShape.Get(ShapeID);
            if (shape == null){
                Debug.LogError($"GetCellInfo: 没有找到网格形状: {ShapeID}");
                return null;
            }
    
            // 获取格子
            List<Vector2Int> originCells = shape.GetRotatedCells(0);
            List<BoardCell> cells = this.GetSystem<IBoardSystem>().GetCenteredCells(center.GetCellPosition(), originCells);

            // 筛选格子
            switch (Type){
                case CellType.空格:
                    cells = cells.Where(x => x.IsEmpty()).ToList();
                    break;
                case CellType.有实体:
                    cells = cells.Where(x => !x.IsEmpty()).ToList();
                    break;
                case CellType.任意:
                    break;
                default:
                    Debug.LogError($"GetCellInfo: 不支持的格子类型: {Type}");
                    return null;
            }
            return cells;
        }
    }
}