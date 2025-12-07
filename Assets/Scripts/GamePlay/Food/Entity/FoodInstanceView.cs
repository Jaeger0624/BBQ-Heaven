using System.Collections.Generic;
using cfg;
using QFramework;
using UniRx;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public enum PreviewState_FoodInstanceView{
    Normal,
    Selected,
    Active,
    ActiveAndSelected,
}
public class FoodInstanceView : MonoBehaviour, IShowTooltip, IController, IPointerEnterHandler{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public FoodInstance foodInstance;
    [ReadOnly]
    public PreviewState_FoodInstanceView previewState = PreviewState_FoodInstanceView.Normal;
    [SerializeField] private Image foodImage;
    [SerializeField] private ParticleSystem buffedParticle;
    public UnityEvent OnPointerEnterEvent;
    private bool canShowPointerEnterAnim => foodInstance.state == FoodInstanceState.棋盘上;
    public void Bind(FoodInstance foodInstance){
        this.foodInstance = foodInstance;
        //TODO: 技术债，数据层最好不要持有视图层，但快速开发期，先这样处理
        this.foodInstance.foodInstanceView = this;
        // 观察者
        foodInstance.status.Subscribe(status => {
            UpdateVisual();
        }).AddTo(this);

        UpdateVisual();
        ResetScale();
    }

    public List<TooltipInfo> GetTooltipInfo()
    {
        List<TooltipInfo> tooltipInfos = new List<TooltipInfo>();
        Color rarityColor = SettingManager.Instance.DevSettings.AddRarityTextColor;
        Color tasteColor = SettingManager.Instance.DevSettings.AddTasteTextColor;
        TooltipInfo tooltipInfo = new TooltipInfo(
            $"<size=36>{foodInstance.food.foodData.Name}  "+
                $"<color=#{rarityColor.ToHexString()}>{foodInstance.rarity}</color> " + 
                $"<color=#{tasteColor.ToHexString()}>{foodInstance.taste}</color></size>" + 
                $"\n<size=24>{foodInstance.state.ToString()}</size>" +
                $"\n<size=24>{foodInstance.food.foodData.EffectDescription}</size>" +
                $"\n<size=20><color=grey>{foodInstance.food.foodData.Description}</color></size>");
        tooltipInfos.Add(tooltipInfo);
        return tooltipInfos;
    }

    void UpdateVisual()
    {
        if (foodImage == null) return;
        if (foodInstance == null) return;
        Sprite sprite = Resources.Load<Sprite>("Sprites/" + foodInstance.food.foodData.Sprite);


        if (sprite == null){
            Debug.LogError($"食材资源不存在: {foodInstance.food.foodData.Sprite}");
            return;
        }
        foodImage.sprite = sprite;

        if (foodInstance.status.Value.isBuffed){
            buffedParticle.gameObject.SetActive(true);
        }
        else{
            buffedParticle.gameObject.SetActive(false);
        }
    }

    public void SetPreviewState(PreviewState_FoodInstanceView previewState){
        switch (previewState){
            // 正常状态
            case PreviewState_FoodInstanceView.Normal:
                foodImage.color = Color.white;
                break;
            case PreviewState_FoodInstanceView.Selected:
                break;
            case PreviewState_FoodInstanceView.Active:
                foodImage.color = Color.green;
                break;
        }
    }
    public void ResetScale(){
        transform.localScale = new Vector3(1,1,1);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canShowPointerEnterAnim){
            OnPointerEnterEvent.Invoke();
        }
    }

}

