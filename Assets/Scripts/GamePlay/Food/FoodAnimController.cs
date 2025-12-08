using System.Collections.Generic;
using QFramework;
using Reflex.Attributes;
using UnityEngine;

public class FoodAnimController : MonoBehaviour, IController
{
    private IAudioService audioService => SettingManager.Instance.audioService;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private FoodController foodController;
    void Awake()
    {
        foodController = gameObject.GetComponent<FoodController>();
    }
    void OnEnable()
    {
        this.RegisterEvent<FoodInstanceViewAnimEvent>(OnFoodInstanceViewAnim).UnRegisterWhenDisabled(this);
        this.RegisterEvent<FoodInstanceAddBaseValueEvent>(OnFoodInstanceAddBaseValueAnim).UnRegisterWhenDisabled(this);
        this.RegisterEvent<FoodInstanceExecuteActionEvent>(OnFoodInstanceExecuteAction).UnRegisterWhenDisabled(this);
    }


    /// <summary>
    /// 食材实例视图动画事件
    /// </summary>
    /// <param name="e"></param>
    void OnFoodInstanceViewAnim(FoodInstanceViewAnimEvent e)
    {
        FoodInstanceView foodInstanceView = foodController.GetFoodInstanceView(e.guid);
        if (foodInstanceView == null) return;
        
        var scaleTween = new ScaleAnimationTask(foodInstanceView.transform, 1.6f,0.15f);
        var rotateTween = new RotateAnimationTask(foodInstanceView.transform, 0.25f);
        var parallelAnimTask = new ParallelAnimTask(new List<IAnimTask>{ scaleTween, rotateTween });
        this.GetSystem<IAnimationSystem>().DirectlyPlay(parallelAnimTask);
    }

    /// <summary>
    /// 食材实例添加基础值动画事件
    /// </summary>
    /// <param name="e"></param>
    void OnFoodInstanceAddBaseValueAnim(FoodInstanceAddBaseValueEvent e)
    {
        FoodInstanceView foodInstanceView = foodController.GetFoodInstanceView(e.guid);
        if (foodInstanceView == null) return;

        // FoodInstanceView 添加基础值动画
        var scaleTween = new ScaleAnimationTask(foodInstanceView.transform, 1.6f,0.15f);
        var rotateTween = new RotateAnimationTask(foodInstanceView.transform, 0.25f);

        // 飘数字
        string rarityText = e.rarity > 0 ? $"+{e.rarity}" : e.rarity.ToString();
        string tasteText = e.taste > 0 ? $"+{e.taste}" : e.taste.ToString();

        var spawnTextAnimTask1 = new SpawnTextAnimationTask(rarityText, 4f, 
        SettingManager.Instance.DevSettings.AddRarityTextColor, AnimUtility.GetTextSpawnPosition(foodInstanceView.transform, true));

        var spawnTextAnimTask2 = new SpawnTextAnimationTask(tasteText, 4f, 
        SettingManager.Instance.DevSettings.AddTasteTextColor, AnimUtility.GetTextSpawnPosition(foodInstanceView.transform, false));

        foodInstanceView.foodInstance.status.Value = new FoodInstanceViewStatus(true);

        var SFXAnimTask = new PlaySFXAnimationTask("Score 2", 0.02f, 0.2f).SetVolume(0.5f);
        // 执行并行动画
        var parallelAnimTask = new ParallelAnimTask(new List<IAnimTask>{ scaleTween, rotateTween, spawnTextAnimTask1, spawnTextAnimTask2, SFXAnimTask });
        this.GetSystem<IAnimationSystem>().DirectlyPlay(parallelAnimTask);
    }

    /// <summary>
    /// 食材实例执行动作动画事件
    /// 食材实例执行动作时，执行缩放动画和旋转动画
    /// </summary>
    /// <param name="e"></param>
    void OnFoodInstanceExecuteAction(FoodInstanceExecuteActionEvent e)
    {
        FoodInstanceView foodInstanceView = foodController.GetFoodInstanceView(e.guid);
        if (foodInstanceView == null) return;
        
        // var scaleTween = new ElasticScaleAnimTask(foodInstanceView.transform, 0.7f, 1.7f);
        var scaleTween = new ScaleAnimationTask(foodInstanceView.transform, 1.6f,0.15f);
        var rotateTween = new ElasticRotationAnimTask(foodInstanceView.transform, 0.6f, 180f);
        var parallelAnimTask = new ParallelAnimTask(new List<IAnimTask>{ scaleTween, rotateTween });
        this.GetSystem<IAnimationSystem>().DirectlyPlay(parallelAnimTask);
    }
}

public class FoodInstanceViewAnimEvent : IEvent{
    public string guid;
    public FoodInstanceViewAnimEvent(string guid){
        this.guid = guid;
    }
}

public class FoodInstanceAddBaseValueEvent : IEvent{
    public string guid;
    public int rarity;
    public int taste;
    public FoodInstanceAddBaseValueEvent(string guid, int rarity, int taste){
        this.guid = guid;
        this.rarity = rarity;
        this.taste = taste;
    }
}

public class FoodInstanceExecuteActionEvent : IEvent{
    public string guid;
    public FoodInstanceExecuteActionEvent(string guid){
        this.guid = guid;
    }
}