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
public enum PreviewState_EntityView{
    Normal,
    Selected,
    Active,
    ActiveAndSelected,
}
public class FoodInstanceView : MonoBehaviour, IShowTooltip, IController, IPointerEnterHandler{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public FoodInstance foodInstance;
    [ReadOnly]
    public PreviewState_EntityView previewState = PreviewState_EntityView.Normal;
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

    public List<TooltipInfo> GetTooltipInfo() => foodInstance.GetTooltipInfos();

    void UpdateVisual()
    {
        if (foodImage == null) return;
        if (foodInstance == null) return;
        Sprite sprite = foodInstance.GetSprite();

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

    public void SetPreviewState(PreviewState_EntityView previewState){
        switch (previewState){
            // 正常状态
            case PreviewState_EntityView.Normal:
                foodImage.color = Color.white;
                break;
            case PreviewState_EntityView.Selected:
                break;
            case PreviewState_EntityView.Active:
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

