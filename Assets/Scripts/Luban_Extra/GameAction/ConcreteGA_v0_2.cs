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
                var moveEvents = new List<MoveFoodInstanceEvent>();

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
                            var evt = foodSystem.foodInstanceMover.ExchangeFoodInstances(origin, secondFoodInstance, false);
                            moveEvents.AddRange(evt);
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

        private void BuildAndReturnResult(object sender, List<MoveFoodInstanceEvent> moveEvents, IObserver<GAResult> observer){
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
}