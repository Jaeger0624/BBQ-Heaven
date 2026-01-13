using System.Collections.Generic;
using cfg;
using QFramework;
using UniRx;
using UnityEngine;
public class PreviewManager : MonoBehaviour,IController, ICanGetSystem, ICanSendEvent{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    void Start()
    {
        this.GetSystem<IBoardSystem>().OnBoardStateChanged
            .ThrottleFrame(1) 
            .Subscribe(_ => RefreshAllFoods())
            .AddTo(this);
    }

    private void RefreshAllFoods(){
        IGASystem GASystem = this.GetSystem<IGASystem>();
        // 获取所有食材
        Dictionary<string, FoodInstance> foodInstances = this.GetSystem<IFoodSystem>().GetFoodInstances();
        // 更新所有食材的预览
        foreach (FoodInstance foodInstance in foodInstances.Values){
            bool check = GASystem.CheckFoodPreview(foodInstance, FoodGAType.放上烤串前) || GASystem.CheckFoodPreview(foodInstance, FoodGAType.被选中时);
            if (check){
                foodInstance.foodInstanceView.SetPreviewState(PreviewState_EntityView.Active);
            }
            else{
                foodInstance.foodInstanceView.SetPreviewState(PreviewState_EntityView.Normal);
            }
        }
    }


}