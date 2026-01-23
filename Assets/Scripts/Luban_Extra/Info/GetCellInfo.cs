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
            ICellPosition cellPosition = param.FirstOrDefault(x => x is ICellPosition) as ICellPosition;
            if (cellPosition == null){
                cellPosition = origin as ICellPosition;
                if (cellPosition == null){
                    Debug.LogError($"GetCellInfo: 没有目标格子");
                    return Observable.Return<BoardCell>(null);
                }
            }
            if (Strategy == GetCellStrategy.周围空位){
                List<BoardCell> adjacentCells = this.GetSystem<IBoardSystem>().GetAdjacentCells(cellPosition.GetCellPosition());
                List<BoardCell> emptyCells = adjacentCells.Where(x => x.IsEmpty()).ToList();
                if (emptyCells.Count == 0){
                    Debug.LogWarning($"GetCellInfo: 周围没有空位");
                    return Observable.Return<BoardCell>(null);
                }
                if (IsRandom){
                    BoardCell randomCell = rng.PickOne(emptyCells);
                    return Observable.Return(randomCell);
                }
                else{
                    return this.GetSystem<ISelectorSystem>().SelectCell(emptyCells)
                        .Do(cell => {
                            if (cell == null){
                                Debug.LogError($"GetCellInfo: 选择空位失败");
                            }
                        });
                }
            }
            else if (Strategy == GetCellStrategy.周围食材){
                List<BoardCell> adjacentCells = this.GetSystem<IBoardSystem>().GetAdjacentCells(cellPosition.GetCellPosition());
                List<BoardCell> foodCells = adjacentCells.Where(x => x.instanceGuid != null).ToList();
                if (foodCells.Count == 0){
                    Debug.LogWarning($"GetCellInfo: 周围没有食材");
                    return Observable.Return<BoardCell>(null);
                }
                if (IsRandom){
                    BoardCell randomCell = rng.PickOne(foodCells);
                    return Observable.Return(randomCell);
                }
                else{
                    return this.GetSystem<ISelectorSystem>().SelectCell(foodCells)
                        .Do(cell => {
                            if (cell == null){
                                Debug.LogError($"GetCellInfo: 选择食材失败");
                            }
                        });
                }
            }
            else if (Strategy == GetCellStrategy.选取){
                BoardCell cell = param.FirstOrDefault(x => x is BoardCell) as BoardCell;
                if (cell == null){
                    Debug.LogError($"GetCellInfo: 没有目标格子");
                    return Observable.Return<BoardCell>(null);
                }
                return Observable.Return(cell);
            }
            else{
                Debug.LogError($"GetCellInfo: 不支持的策略: {Strategy}");
                return Observable.Return<BoardCell>(null);
            }
        }
        
        public List<BoardCell> GetCells(object sender, List<object> param){
            ICellPosition cellPosition = param.FirstOrDefault(x => x is ICellPosition) as ICellPosition;

            if (cellPosition == null){
                cellPosition = sender as ICellPosition;
                if (cellPosition == null){
                    Debug.LogError($"GetCellInfo: 没有目标格子");
                    return null;
                }
            }
            else{
                Debug.Log($"GetCellInfo: 目标格子: {cellPosition.GetCellPosition()}");
            }
            if (Strategy == GetCellStrategy.选取){
                ICellPosition cellPos = param.FirstOrDefault(x => x is ICellPosition) as ICellPosition;
                if (cellPos == null){
                    Debug.LogError($"GetCellInfo: 没有目标格子");
                    return null;
                }
                return new List<BoardCell>{this.GetSystem<IBoardSystem>().GetCell(cellPos.GetCellPosition())};
            }
            else if (Strategy == GetCellStrategy.周围空位){
                
                List<BoardCell> adjacentCells = this.GetSystem<IBoardSystem>().GetAdjacentCells(cellPosition.GetCellPosition());
                List<BoardCell> emptyCells = adjacentCells.Where(x => x.IsEmpty()).ToList();
                if (emptyCells.Count == 0){
                    Debug.LogWarning($"GetCellInfo: 周围没有空位");
                    return null;
                }
                return emptyCells;
            }
            else if (Strategy == GetCellStrategy.周围食材){
                List<BoardCell> adjacentCells = this.GetSystem<IBoardSystem>().GetAdjacentCells(cellPosition.GetCellPosition());
                List<BoardCell> foodCells = adjacentCells.Where(x => x.instanceGuid != null).ToList();
                if (foodCells.Count == 0){
                    Debug.LogWarning($"GetCellInfo: 周围没有食材");
                    return null;
                }
                return foodCells;
            }
            else{
                Debug.LogError($"GetCellInfo: 不支持的策略: {Strategy}");
                return null;
            }
        }
    }
}