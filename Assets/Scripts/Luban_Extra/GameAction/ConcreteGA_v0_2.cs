using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UniRx;
using UnityEngine;

namespace cfg{
    public partial class GA_食材换位 : GameAction, ICanGetSystem
    {
        public GA_食材换位(GetFoodInstancesInfo foodInfo, GetCellInfo cellInfo)
        {
            this.FoodInfo = foodInfo;
            this.CellInfo = cellInfo;
        }
        public override GameAction Clone() => new GA_食材换位(FoodInfo, CellInfo);

        public override IObservable<GAResult> ExecuteAsync(object sender, List<object> param){
            return Observable.Create<GAResult>(observer =>
            {
                // 1. 准备局部变量
                var moveEvents = new List<MoveEntityEvent>();

                // 2. 获取食材目标列表
                FoodInstance origin = null;
                if (param != null)
                {
                    origin = param.FirstOrDefault(x => x is FoodInstance) as FoodInstance;
                }
                if (origin == null)
                {
                    Debug.LogWarning($"[GA_食材换位] 没有食材目标，使用发送者作为食材目标: {sender}");
                    origin = sender as FoodInstance;
                }

                List<FoodInstance> targets = FoodInfo.GetFoodInstances(origin, param);
                if (targets == null || targets.Count == 0)
                {
                    BuildAndReturnResult(sender, moveEvents, observer);
                    return Disposable.Empty;
                }

                IFoodSystem foodSystem = this.GetSystem<IFoodSystem>();

                // 3. 串行异步选择目标格子
                var selectionObservables = targets.Select(foodInstance => 
                    CellInfo.GetCell(foodInstance, param)
                );

                // 使用 Concat 将所有异步选择串联起来（一个接一个）
                var subscription = Observable.Concat(selectionObservables)
                    .Where(x => x != null) // 过滤掉选择失败的
                    .Subscribe(
                        x => 
                        {
                            Debug.Log($"[GA_食材换位] 选择到格子: {x.position}");
                            BoardCell secondCell = this.GetSystem<IBoardSystem>().GetCell(x.position);
                            if (secondCell.instanceGuid == null)
                            {
                                observer.OnError(new Exception($"[GA_食材换位] 格子上没有食材实例: {x.position}"));
                            }
                            FoodInstance secondFoodInstance = foodSystem.GetFoodInstance(secondCell.instanceGuid);
                            if (secondFoodInstance == null)
                            {
                                observer.OnError(new Exception($"[GA_食材换位] 格子上没有食材实例: {secondCell.instanceGuid}"));
                            }
                            this.GetSystem<IBoardEntitySystem>().Mover.SwapEntities(origin, secondFoodInstance);
                        },
                        ex => 
                        {
                            if (ex is OperationCanceledException)
                            {
                                Debug.Log($"[GA_食材换位] 用户取消选择");
                            }
                            else
                            {
                                Debug.LogError($"[GA_食材换位] 选择出错: {ex}");
                            }
                            observer.OnError(ex);
                        },
                        () => 
                        {
                            BuildAndReturnResult(sender, moveEvents, observer);
                        }
                    );
                return subscription;
            });
        }

        private void BuildAndReturnResult(object sender, List<MoveEntityEvent> moveEvents, IObserver<GAResult> observer){
            var animSequence = new List<IAnimTask>();
            if (sender is IAnimPlayer animPlayer)
            {
                animSequence.Add(AnimationConverter.Convert(animPlayer, "common"));
            }
            if (moveEvents.Count > 0)
            {
                animSequence.Add(new ActionAnimTask(() =>
                {
                    foreach (var evt in moveEvents)
                    {
                        this.GetArchitecture().SendEvent(evt);
                    }
                }));
            }

            // 缓冲时间
            animSequence.Add(new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval));

            IAnimTask finalTask = new SequenceAnimTask(animSequence);
            observer.OnNext(GAResult.FromAnim(finalTask));
            observer.OnCompleted();
        }
        public override void Execute(object sender, List<object> param){}
        public override IAnimTask GetAnimTask(){return null;}
    }

    public partial class GA_方向位移 : GameAction
    {
        List<MoveEntityEvent> moveEvents = new List<MoveEntityEvent>();
        public GA_方向位移(Direction direction, DynamicValue value, GetFoodInstancesInfo info)
        {
            this.Dir = direction;
            this.Value = value;
            this.Info = info;
        }

        public override GameAction Clone()
        {
            return new GA_方向位移(Dir, Value, Info);
        }

        public override void Execute(object sender, List<object> param)
        {
            moveEvents.Clear();
            // 1. 处理方向
            if (Dir == Direction.无)
            {
                Debug.LogError("【GA_方向位移】方向不能为无");
                return;
            }
            Direction finalDirection = Dir;
            if (Dir == Direction.上下文)
            {
                DirectionContext directionContext = param.FirstOrDefault(x => x is DirectionContext) as DirectionContext;
                if (directionContext == null)
                {
                    Debug.LogError("【GA_方向位移】没有方向上下文");
                    return;
                }
                finalDirection = directionContext.direction;
            }

            // 2. 获取食材
            FoodInstance origin = param.FirstOrDefault(x => x is FoodInstance) as FoodInstance;
            if (origin == null)
            {
                origin = sender as FoodInstance;
                Debug.LogWarning($"[GA_方向位移] 没有食材，使用发送者作为食材: {sender}");
            }
            List<FoodInstance> targets = Info.GetFoodInstances(origin, param);
            if (targets == null || targets.Count == 0)
            {
                Debug.LogWarning($"[GA_方向位移] 没有目标食材");
                return;
            }


            // 3. 实际执行位移
            foreach (var target in targets)
            {
                int steps = Value.GetValue(target, param);
                Vector2Int direction = finalDirection.ToVector2Int();
                this.GetSystem<IBoardEntitySystem>().Mover.DirectionalMove(target, direction, steps);
            }
        }
        private void SendAnims(){
            foreach (var evt in moveEvents)
            {
                if (evt == null) continue;
                this.SendEvent(evt);
            }
        }

        public override IAnimTask GetAnimTask()
        {
            return new SequenceAnimTask(new List<IAnimTask>{
                new ActionAnimTask(() => SendAnims()),
                new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
            });
        }
    }

    public partial class GA_滑行 : GameAction
    {
        public GA_滑行(DynamicValue value)
        {
            this.Value = value;
        }
        public override GameAction Clone() => new GA_滑行(Value);
        public override void Execute(object sender, List<object> param){

            BoardEntity entity = param.FirstOrDefault(x => x is BoardEntity) as BoardEntity;
            if (entity == null)
            {
                Debug.LogWarning($"[GA_滑行] 没有食材，使用发送者作为食材: {sender}");
            }
            entity = sender as BoardEntity;
            if (entity == null)
            {
                Debug.LogError($"[GA_滑行] 没有食材: {sender}");
                return;
            }

            int steps = Value.GetValue(entity, param);

            // 2. 检查是否有方向记录
            if (entity.lastMoveDirection == Vector2Int.zero){
                Debug.LogError($"[GA_滑行] 没有方向记录: {entity.name}");
                return;
            }

            Debug.Log($"[GA_滑行] 滑行: {entity.name} 方向: {entity.lastMoveDirection}");

            this.GetSystem<IBoardEntitySystem>().Mover.DirectionalMove(entity, entity.lastMoveDirection, steps);
        }
        public override IAnimTask GetAnimTask(){return null;}
    }


    public partial class GA_修改地块 : GameAction
    {
        public GA_修改地块(string tileId)
        {
            this.TileId = tileId;   
        }
        public override GameAction Clone() => new GA_修改地块(TileId);

        public override void Execute(object sender, List<object> param){}
        public override IObservable<GAResult> ExecuteAsync(object sender, List<object> param){

            BoardCell targetCell = param.FirstOrDefault(x => x is BoardCell) as BoardCell;
            if (targetCell == null)
            {
                Debug.LogError($"[GA_修改地块] 没有目标地块: {sender}");
                return Observable.Return<GAResult>(GAResult.Empty);
            }
            TileData tileData = this.GetSystem<IDataSystem>().GetTileData(TileId);
            TileData oldTileData = this.GetSystem<IDataSystem>().GetTileData(targetCell.TileID);
            if (tileData == null)
            {
                Debug.LogError($"[GA_修改地块] 地块数据不存在: {TileId}");
                return Observable.Return<GAResult>(GAResult.Empty);
            }
            Debug.Log($"[GA_修改地块] 修改地块: {targetCell.position} 旧地块：{oldTileData.Name} 新地块：{tileData.Name}");
            this.GetSystem<IBoardSystem>().SetCellTile(targetCell.position, TileId);
            return Observable.Return(GAResult.Empty);
        }
        public override IAnimTask GetAnimTask(){return null;}
    }


    public partial class GA_创建实体 : GameAction
    {
        public GA_创建实体(string entityId)
        {
            this.EntityID = entityId;
        }
        public override GameAction Clone() => new GA_创建实体(EntityID);
        public override IObservable<GAResult> ExecuteAsync(object sender, List<object> param){
            BoardCell targetCell = param.FirstOrDefault(x => x is BoardCell) as BoardCell;
            if (targetCell == null)
            {
                Debug.LogError($"[GA_创建实体] 没有目标地块: {sender}");
                return Observable.Return<GAResult>(GAResult.Empty);
            }
            Debug.Log($"[GA_创建实体] 创建实体: {EntityID} 到地块: {targetCell.position}");
            this.GetSystem<IBoardEntitySystem>().CreateEntity(EntityID, targetCell.position);
            return Observable.Return<GAResult>(GAResult.Empty);
        }
        public override void Execute(object sender, List<object> param){
            Debug.LogError($"[GA_创建实体] 创建实体: {EntityID}，目前没做同步逻辑");
        }
        public override IAnimTask GetAnimTask(){return null;}
    }
}