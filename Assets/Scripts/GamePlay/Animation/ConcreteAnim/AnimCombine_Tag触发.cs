using System.Collections.Generic;
using System.Numerics;
using QFramework;
using UnityEngine;

public class AnimCombine_顾客Tag{
    // 都是原子级的动画结合，不需要在意逻辑，只需要满足实现动画的必须参数
    public static IAnimTask Anim_Tag触发_逐一突出(List<string> foodInstancesGuids, List<float> multipliers){
        ICanSendEvent sender = SettingManager.Instance;
        List<IAnimTask> animTasks = new List<IAnimTask>();

        // 食材实例视图动画事件
        for (int i = 0; i < foodInstancesGuids.Count; i++)
        {
            Debug.Log($"【AnimCombine_Tag触发】逐一突出：食材实例GUID: {foodInstancesGuids[i]}，满意度乘区: {multipliers[i]}X");
            string guid = foodInstancesGuids[i];
            float multiplier = multipliers[i];
            //TODO: 改成另一个动画
            string output = $"满意乘区：{multiplier}X";
            UnityEngine.Vector3 position = SettingManager.Instance.SatisfactionTextParent.position;
            position.z = -16;
            IAnimTask anim = new SequenceAnimTask(new List<IAnimTask>{
                new SpawnTextAnimationTask(output, 5f, Color.white, position),
                new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval),
            });
            animTasks.Add(anim);
            animTasks.Add(new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval));
        }
        return new SequenceAnimTask(animTasks);
    }

    public static IAnimTask Anim_Tag触发_单一突出(string foodInstanceGuid, float multiplier, string name, Transform targetTransform){
            ICanSendEvent sender = SettingManager.Instance;
            List<IAnimTask> animTasks = new List<IAnimTask>();
            

            // 1. 食材实例视图动画事件
            animTasks.Add(new ActionAnimTask(() => sender.SendEvent(new FoodInstanceViewAnimEvent(foodInstanceGuid))));
            // 2. 满意度乘区文本动画事件
            animTasks.Add(Anim_Tag触发_满意度乘区文本(multiplier, name, targetTransform));
            // 3. 更新满意度乘区
            UpdateStatisEvent updateStatisEvent = new UpdateStatisEvent(multiplier, null, null, false);
            animTasks.Add(new ActionAnimTask(() => sender.SendEvent(updateStatisEvent)));
            
            // 4. 等待默认间隔
            animTasks.Add(new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval));
            
            return new SequenceAnimTask(animTasks);
    }

    public static IAnimTask Anim_Tag触发_整体突出(List<string> foodInstancesGuids, float multiplier){
        ICanSendEvent sender = SettingManager.Instance;
        List<IAnimTask> animTasks = new List<IAnimTask>();
        string output = $"满意乘区：{multiplier}X";
        UnityEngine.Vector3 position = SettingManager.Instance.SatisfactionTextParent.position;
        position.z = -16;
        List<IAnimTask> tasks = new List<IAnimTask>();

        // 1. 食材实例视图动画事件（全体）
        foodInstancesGuids.ForEach(x => {
            tasks.Add(new ActionAnimTask(() => sender.SendEvent(new FoodInstanceViewAnimEvent(x))));
        });
        tasks.Add(new ParallelAnimTask(tasks));

        // 2. 满意度乘区文本动画事件
        tasks.Add(new SpawnTextAnimationTask(output, 5f, Color.white, position));

        // 3. 更新满意度乘区
        UpdateStatisEvent updateStatisEvent = new UpdateStatisEvent(multiplier, null, null, false);
        tasks.Add(new ActionAnimTask(() => sender.SendEvent(updateStatisEvent)));

        // 4. 等待默认间隔
        tasks.Add(new DelayAnimTask(SettingManager.Instance.DefaultAnimInterval));
        return new SequenceAnimTask(tasks);
    }

    public static IAnimTask Anim_Tag触发_满意度乘区文本(float multiplier, string name, Transform targetTransform){
        string output = $"{name}： {multiplier}X";
        UnityEngine.Vector3 position = AnimUtility.GetTextSpawnPosition(targetTransform.position);
        return new SpawnTextAnimationTask(output, 5f, Color.white, position);
    }

    public static IAnimTask Anim_顾客Tag_隐藏标签视图(){
		return new SequenceAnimTask(new List<IAnimTask>{
            new ActionAnimTask(() => {
                GameArchitecture.Interface.SendEvent(new HideTagViewEvent());
            }),
            new DelayAnimTask(0.15f, true),
        });
    }
    public static IAnimTask Anim_顾客Tag_显示标签视图(){
        return new SequenceAnimTask(new List<IAnimTask>{
            new ActionAnimTask(() => {
                GameArchitecture.Interface.SendEvent(new ShowTagViewEvent());
            }),
            new DelayAnimTask(0.15f, true),
        });
    }
}