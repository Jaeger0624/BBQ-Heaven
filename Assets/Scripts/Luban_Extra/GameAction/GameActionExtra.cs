
using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UniRx;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace cfg{

public struct GAResult{
    public IAnimTask AnimTask;
    public static GAResult Empty => new GAResult{ AnimTask = new EmptyAnimTask() };
    public static GAResult FromAnim(IAnimTask animTask) => new GAResult{ AnimTask = animTask };
}

public abstract partial class GameAction : ICanGetSystem, IHaveAnim, ICanSendEvent
{
    public GameAction(){}
    // 默认实现：如果子类没重写 ExecuteAsync，就跑同步逻辑 + 获取 GetAnimTask
    public virtual IObservable<GAResult> ExecuteAsync(object sender, List<object> param)
    {
        return Observable.Create<GAResult>(observer =>
        {
            try
            {
                // 1. 跑你原来的同步逻辑 (Execute)
                this.Execute(sender, param); 
                
                // 2. 拿你原来的动画 (GetAnimTask)
                var anim = this.GetAnimTask();
                
                // 3. 发送结果并结束
                observer.OnNext(GAResult.FromAnim(anim));
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
            return Disposable.Empty;
        });
    }
    public abstract void Execute(object sender, List<object> param);
    public virtual void ApplyMultiplier(int multiplier){}
    public abstract IAnimTask GetAnimTask();
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
    public abstract GameAction Clone();
    public virtual void SetRelation(object target){}
    
}

#region 配置GA

public partial class GA_直接修改当前烧烤值 : GameAction
{
    private IAnimTask senderAnimTask;
    private AddBBQPropertyAnimEvent addBBQPropertyAnimEvent;
    public override void Execute(object sender, List<object> param)
    {
        // 读取上下文
        BBQProcessContext context = param?.FirstOrDefault() as BBQProcessContext;


        BBQ currentBBQ = null;
        if (context == null){
            Debug.LogWarning("上下文为空，使用当前烧烤");
            currentBBQ = this.GetSystem<IBBQSystem>().GetCurrentBBQ();
        }
        else{
            currentBBQ = context.targetBBQ;
        }
        if (currentBBQ == null) {Debug.LogError("当前烧烤为空"); return;}


        currentBBQ.SetTotalRarity((int)(currentBBQ.totalRarity.Value + RarityValue.GetValue(sender, param)));
        currentBBQ.SetTotalTaste((int)(currentBBQ.totalTaste.Value + TasteValue.GetValue(sender, param)));
        int currentRarity = currentBBQ.totalRarity.Value;
        int currentTaste = currentBBQ.totalTaste.Value;
        if (sender is IAnimPlayer animPlayer){
            senderAnimTask = AnimationConverter.Convert(animPlayer, "common");
        }
        addBBQPropertyAnimEvent = new AddBBQPropertyAnimEvent(currentRarity, currentTaste, (int)RarityValue.GetValue(sender, param), (int)TasteValue.GetValue(sender, param ), $"", false);
    }
    public override IAnimTask GetAnimTask()
    {
        
        IAnimTask anim = senderAnimTask != null ? new SequenceAnimTask(new List<IAnimTask>{
            senderAnimTask,
            new ActionAnimTask(() => this.SendEvent(addBBQPropertyAnimEvent)),
            new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval, true),
        }) : new SequenceAnimTask(new List<IAnimTask>{
            new ActionAnimTask(() => this.SendEvent(addBBQPropertyAnimEvent)),
            new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval, true),
        });
        return anim;
    }
    public override GameAction Clone() => new GA_直接修改当前烧烤值(RarityValue, TasteValue);
    public GA_直接修改当前烧烤值(DynamicValue rarityValue, DynamicValue tasteValue)
    {
        this.RarityValue = rarityValue;
        this.TasteValue = tasteValue;
    }
}

public partial class GA_创建满意度乘区 : GameAction
{
    private string str;
    private bool showAnim = true;
    private List<bool> previewResults = new List<bool>();
    private UpdateStatisEvent e;
    public GA_创建满意度乘区(float value, string name, bool showAnim = true){
        this.Name = name;
        this.Value = value;
        this.showAnim = showAnim;
    }
    public override void Execute(object sender, List<object> param)
    {
        float multiplier = Value;
        if (Name == null) str = "未知";
        else if (String.IsNullOrEmpty(Name)) str = "未知";
        else str = Name;

        // 获取当前顾客实例
        this.GetSystem<ICustomerSystem>().Satisfaction.AddSatisfactionMultiplier(multiplier, str);
        e = new UpdateStatisEvent(multiplier, previewResults, null, false).SetName(str);
    }
    public override void SetRelation(object target){
        if (target is List<bool> previewResults){
            this.previewResults = previewResults;
        }
    }

    public override IAnimTask GetAnimTask()
    {
        if (!showAnim) return new EmptyAnimTask();
        IAnimTask anim = new ParallelAnimTask(new List<IAnimTask>{
            new ActionAnimTask(() => this.SendEvent(e))
        });
        return anim;
    }
    public override GameAction Clone()
    {
        return new GA_创建满意度乘区(Value, Name);
    }
}
    public partial class GA_测试文字 : GameAction
    {
        private int repeatCount = 1;
        public override void Execute(object sender, List<object> param)
        {
            for (int i = 0; i < repeatCount; i++){
            if (sender != null){
                    Debug.Log($"【GA_测试文字】【sender:{sender.GetType().Name}】: {Text}");
                }
                else{
                    Debug.Log($"【GA_测试文字】: {Text}");
                }
            }
        }
        public override void ApplyMultiplier(int multiplier) => repeatCount = multiplier;
        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }

        public override GameAction Clone()
        {
            return new GA_测试文字(Text);
        }
    }



    public partial class GA_为食材加属性 : GameAction
    {
        IAnimPlayer animPlayer;
        private List<FoodInstanceAddBaseValueEvent> evts = new List<FoodInstanceAddBaseValueEvent>();
        public GA_为食材加属性(DynamicValue rarity, DynamicValue taste, GetFoodInstancesInfo info){
            this.Rarity = rarity;
            this.Taste = taste;
            this.Info = info;
        }
        public override GameAction Clone() => new GA_为食材加属性(Rarity, Taste, Info);
        public override void Execute(object sender, List<object> param)
        {
            evts.Clear();
            this.animPlayer = null;

            FoodInstance origin = sender is FoodInstance ? sender as FoodInstance : null;

            List<FoodInstance> foodInstances = Info.GetFoodInstances(origin, param);

            int rarity = Rarity.GetValue(sender, param);
            int taste = Taste.GetValue(sender, param);
            
            if (foodInstances == null) return;
            foreach (FoodInstance foodInstance in foodInstances){

                evts.Add(new FoodInstanceAddBaseValueEvent(foodInstance.guid, rarity, taste));
            }
            // Debug.Log($"【GA_为食材加属性】: 为食材加属性: {rarity} {taste}, 为{foodInstances.Count}个食材加属性");
            foodInstances.ForEach(foodInstance => {
                foodInstance.rarity += rarity;
                foodInstance.taste += taste;
            });

            if (sender is IAnimPlayer newAnimPlayer){
                this.animPlayer = newAnimPlayer;
            }
        }
        private void SendAnimEvent()
        {
            evts.ForEach(evt => {
                this.SendEvent(evt);
            });
        }
        public override IAnimTask GetAnimTask()
        {
            if (evts.Count == 0) return new EmptyAnimTask();

            IAnimTask anim = animPlayer != null ? new SequenceAnimTask(new List<IAnimTask>{
                AnimationConverter.Convert(animPlayer, "common"),
                new ActionAnimTask(() => SendAnimEvent()),
                new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
            }) : new SequenceAnimTask(new List<IAnimTask>{
                new ActionAnimTask(() => SendAnimEvent()),
                new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
            });
            return anim;
        }
    }


    public partial class GA_补充食材 : GameAction
    {
        private int Multiplier = 1;
        public GA_补充食材(DynamicValue value) => this.Value = value;
        public override GameAction Clone() => new GA_补充食材(Value);
        public override void ApplyMultiplier(int multiplier) => Multiplier = multiplier;
        public override void Execute(object sender, List<object> param)
        {
            int value = Value.GetValue(sender, param) * Multiplier;
            if (value <= 0) return;

            (List<FoodCard> cards, List<FoodInstance> instances) = this.GetSystem<IFoodSystem>().DrawFoodCard(value);

            Debug.Log($"【GA_补充食材】: 抽取了{cards.Count}张食材卡牌，创建了{instances.Count}个食材实例");
        }

        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }


    public partial class GA_添加食材到烤串 : GameAction
    {
        // 暂存起来，生成动画时再获取
        private List<FoodInstance> foods = new List<FoodInstance>();
        public GA_添加食材到烤串(GetFoodInstancesInfo info){
            foods = new List<FoodInstance>();
            this.Info = info;
        }
        public override void Execute(object sender, List<object> param)
        {
            foods.Clear();
            FoodInstance origin = sender is FoodInstance ? sender as FoodInstance : null;

            foods = Info.GetFoodInstances(origin, param);
            if (foods == null) return;
            if (foods.Count == 0) return;
            string foodNames = string.Join(", ", foods.Select(x => x.name));
            Debug.Log($"【GA_添加食材到烤串】{Info.Strategy.ToString()}: 添加了食材: {foodNames}");
        }
                
        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
        public override GameAction Clone() => new GA_添加食材到烤串(Info);
    }

    public partial class GA_重复执行GA : GameAction
    {
        public GA_重复执行GA(DynamicValue value, GameAction action){
            this.Value = value;
            this.Action = action.Clone();
        }
        public override GameAction Clone() => new GA_重复执行GA(Value, Action);

         // 【核心修改】：重写 ExecuteAsync，而不是 Execute
        public override IObservable<GAResult> ExecuteAsync(object sender, List<object> param)
        {
            // 1. 获取重复次数 (同步计算)
            int count = Value.GetValue(sender, param);
            
            // Debug.Log($"【GA_重复执行GA】: 计划重复执行 {Action.GetType().Name} {count} 次");
            
            if (count <= 0) 
            {
                return Observable.Return(GAResult.Empty);
            }

            // 2. 构建执行流
            // 使用 Defer 确保流被订阅时才开始构建
            return Observable.Defer(() => 
            {
                var system = this.GetSystem<IGASystem>();
                var streams = new List<IObservable<Unit>>();

                for (int i = 0; i < count; i++)
                {
                    // 关键点：每次循环都克隆一个新的 Action 实例
                    GameAction subAction = Action.Clone();
                    
                    // 关键点：调用 System 的 Immediate 方法，而不是 ApplyGA
                    // 这样这些动作会串行链接在当前流中
                    streams.Add(system.ApplyGAImmediate(sender, subAction, param));
                }

                // 3. 串行执行所有子任务
                // Concat 保证了：第1次逻辑+动画完全结束 -> 第2次逻辑+动画 ...
                return Observable.Concat(streams)
                    .Select(_ => GAResult.Empty); // 所有子任务跑完后，返回 Empty 给外层
            });
        }
        public override void Execute(object sender, List<object> param) { }
        public override IAnimTask GetAnimTask()
        {
            // 本身没有动画，由Action的动画组成
            return new EmptyAnimTask();
        }
    }


    public partial class GA_使食材获得GA : GameAction
    {
        private IAnimPlayer animPlayer = null;
        private List<FoodInstance> foodInstances;
        public GA_使食材获得GA(List<GameAction> actions, GetFoodInstancesInfo info, string description, FoodGAType foodGAType){
            this.Actions = new List<GameAction>(actions.Select(x => x.Clone()));
            this.Info = info;
            this.Description = description;
            this.Type = foodGAType;
            this.foodInstances = new List<FoodInstance>();
        }
        public override GameAction Clone() => new GA_使食材获得GA(Actions, Info, Description, Type);

        public override void Execute(object sender, List<object> param)
        {
            // 0. 清空动画播放器
            animPlayer = null;
            if (foodInstances != null) foodInstances.Clear();
            else foodInstances = new List<FoodInstance>();

            // 1. 获取食材
            FoodInstance origin = sender is FoodInstance ? sender as FoodInstance : null;
            foodInstances = Info.GetFoodInstances(origin, param);

            // 2. 为食材添加GA
            foreach (FoodInstance foodInstance in foodInstances){
                // foodInstance.AddGA(Actions, Type);
            }

            // 3. 设置动画播放器
            if (sender is IAnimPlayer newAnimPlayer){
                this.animPlayer = newAnimPlayer;
            }
        }

        private void SendAnims(){
            if (foodInstances == null) return;
            foodInstances.ForEach(foodInstance => {
                this.SendEvent(new FoodInstanceViewAnimEvent(foodInstance.guid));
            });
        }

        public override IAnimTask GetAnimTask()
        {
            IAnimTask anim = animPlayer != null ? new SequenceAnimTask(new List<IAnimTask>{
                AnimationConverter.Convert(animPlayer, "common"),
                new ActionAnimTask(() => SendAnims()),
                new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
            }) : new SequenceAnimTask(new List<IAnimTask>{
                new ActionAnimTask(() => SendAnims()),
                new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
            });
            return anim;
        }
    }

    public partial class GA_随机移动食材 : GameAction
    {
        IAnimPlayer animPlayer = null;
        public GA_随机移动食材(GetFoodInstancesInfo info){
            this.Info = info;
            animPlayer = null;
        }
        public override GameAction Clone()=> new GA_随机移动食材(Info);
        public override void Execute(object sender, List<object> param)
        {
            animPlayer = null;

            FoodInstance origin = sender is FoodInstance ? sender as FoodInstance : null;
            List<FoodInstance> foodInstances = Info.GetFoodInstances(origin, param);
            if (foodInstances == null) {Debug.LogError($"[GA_随机移动食材] 没有食材: {sender}"); return;}
            if (foodInstances.Count == 0) {Debug.LogError($"[GA_随机移动食材] 没有食材: {sender}"); return;}

            List<BoardEntity> entities = foodInstances.Cast<BoardEntity>().ToList();
            this.GetSystem<IBoardEntitySystem>().Mover.RandomPlaceEntities(entities);

            if (sender is IAnimPlayer newAnimPlayer){
                this.animPlayer = newAnimPlayer;
            }
        }
        public override IAnimTask GetAnimTask()
        {
            IAnimTask anim = animPlayer != null ? new SequenceAnimTask(new List<IAnimTask>{
                AnimationConverter.Convert(animPlayer, "common"),
                new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
            }) : new SequenceAnimTask(new List<IAnimTask>{
                new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
            });
            return anim;
        }
    }
    public partial class GA_食材位移 : GameAction{
        public GA_食材位移(GetFoodInstancesInfo info, GetCellInfo cellInfo){
            this.Info = info;
            this.CellInfo = cellInfo;
        }
        public override GameAction Clone() => new GA_食材位移(Info, CellInfo);
        public override IObservable<GAResult> ExecuteAsync(object sender, List<object> param)
    {
        return Observable.Create<GAResult>(observer =>
        {
            // --- 1. 准备局部变量 ---
            var moveEvents = new List<MoveEntityEvent>();
            
            // --- 2. 获取食材目标列表 ---
            FoodInstance origin = null;
            if (param != null)
            {
                origin = param.FirstOrDefault(x => x is FoodInstance) as FoodInstance;
            }
            if (origin == null)
            {
                origin = sender as FoodInstance;

            }

            List<FoodInstance> targets = Info.GetFoodInstances(origin, param);
            
            if (targets == null || targets.Count == 0)
            {
                // 没有目标，直接返回空动画
                BuildAndReturnResult(sender, moveEvents, observer);
                return Disposable.Empty;
            }

            // --- 3. 串行异步选择目标格子 ---
            var boardSystem = this.GetSystem<IBoardSystem>();
            var foodSystem = this.GetSystem<IFoodSystem>();
            
            // 将每个 foodInstance 转换为一个异步选择流，然后串联执行
            var selectionObservables = targets.Select(foodInstance => 
                CellInfo.GetCell(foodInstance, param)
                    .Select(cell => new { foodInstance, cell })
            );

            // 使用 Concat 将所有异步选择串联起来（一个接一个）
            var subscription = Observable.Concat(selectionObservables)
                .Where(x => x.cell != null) // 过滤掉选择失败的
                .Subscribe(
                    x => 
                    {
                        // Debug.Log($"[GA_食材位移] 选择到格子: {x.cell.position}");
                        // 每次用户选择完一个格子，执行移动逻辑
                        this.GetSystem<IBoardEntitySystem>().Mover.PlaceEntity(x.foodInstance, x.cell);
                    },
                    ex => 
                    {
                        // 错误处理：用户取消或其他异常
                        if (ex is OperationCanceledException)
                        {
                            Debug.Log($"[GA_食材位移] 用户取消选择");
                        }
                        else
                        {
                            Debug.LogError($"[GA_食材位移] 选择出错: {ex}");
                        }
                        observer.OnError(ex);
                    },
                    () => 
                    {
                        // 所有选择完成，构建动画任务并返回
                        BuildAndReturnResult(sender, moveEvents, observer);
                    }
                );

            return subscription;
        });
    }

    private void BuildAndReturnResult(object sender, List<MoveEntityEvent> moveEvents, IObserver<GAResult> observer)
    {
        // 构建动画任务
        var animSequence = new List<IAnimTask>();

        // A. 发起者动画
        if (sender is IAnimPlayer animPlayer)
        {
            animSequence.Add(AnimationConverter.Convert(animPlayer, "common"));
        }

        // B. 位移动画
        if (moveEvents.Count > 0)
        {
            animSequence.Add(new ActionAnimTask(() => 
            {
                foreach (var evt in moveEvents)
                {
                    this.SendEvent(evt);
                }
            }));
        }

        // C. 缓冲时间
        animSequence.Add(new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval));

        // 返回结果
        IAnimTask finalTask = new SequenceAnimTask(animSequence);
        observer.OnNext(GAResult.FromAnim(finalTask));
        observer.OnCompleted();
    }
        public override void Execute(object sender, List<object> param){}
        public override IAnimTask GetAnimTask(){return null;}
    }
    public partial class GA_食材随机冲锋 : GameAction{
        private IAnimPlayer animPlayer = null;
        public override GameAction Clone() => new GA_食材随机冲锋(Info);
        private List<MoveEntityEvent> moveEvents = new List<MoveEntityEvent>();
        public GA_食材随机冲锋(GetFoodInstancesInfo info){
            this.Info = info;
            moveEvents = new List<MoveEntityEvent>();
        }
        public override void Execute(object sender, List<object> param)
        {
            animPlayer = null;
            moveEvents.Clear();

            FoodInstance origin = sender is FoodInstance ? sender as FoodInstance : null;

            List<FoodInstance> foodInstances = Info.GetFoodInstances(origin, param);
            if (foodInstances == null || foodInstances.Count == 0) return;
            
            foodInstances.ForEach(foodInstance => {
                
            });

            if (sender is IAnimPlayer newAnimPlayer){
                this.animPlayer = newAnimPlayer;
            }
        }
        private void SendAnims(){
            moveEvents.ForEach(evt => {
                this.SendEvent(evt);
            });
        }
        public override IAnimTask GetAnimTask()
        {
            IAnimTask anim = animPlayer != null ? new SequenceAnimTask(new List<IAnimTask>{
                new ActionAnimTask(() => SendAnims()),
                AnimationConverter.Convert(animPlayer, "common"),
                new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
            }) : new SequenceAnimTask(new List<IAnimTask>{
                new ActionAnimTask(() => SendAnims()),
                new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
            });
            return anim;
        }
    }

}


#endregion
