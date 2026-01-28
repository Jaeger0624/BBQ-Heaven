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
}

public class FoodInstanceView : MonoBehaviour, IController, IPointerEnterHandler, IEntityView{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public BoardEntity Entity => foodInstance;
    private FoodInstance foodInstance;
    [ReadOnly]
    [SerializeField] private Image foodImage;
    [SerializeField] private Image newCreatedInfo;
    [SerializeField] private ParticleSystem buffedParticle;
    public UnityEvent OnPointerEnterEvent;
    private bool canShowPointerEnterAnim => foodInstance.state == FoodInstanceState.棋盘上;

    private void Start() {
        RandomizeTransform();
    }
    private void RandomizeTransform(){
        // 随机决定朝向（左或者右，水平翻转）
        bool isHorizontalFlip = Random.Range(0, 2) == 0;

        foodImage.transform.localScale = new Vector3(Random.Range(0.95f, 1.05f), Random.Range(0.95f, 1.05f), 1);
        // 正态分布随机角度，范围-6到6        
        foodImage.transform.localRotation = Quaternion.Euler(0, 0, Distribution.BoxMullerNormal(0, 5));

    }
    public void ResetTransform(){
        foodImage.transform.localScale = new Vector3(1,1,1);
        foodImage.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
    public void Bind(BoardEntity entity){
        if (entity is not FoodInstance foodInstance) {Debug.LogError("FoodInstanceView 只能绑定 FoodInstance");return;}
        this.foodInstance = foodInstance;
        //TODO: 技术债，数据层最好不要持有视图层，但快速开发期，先这样处理
        this.foodInstance.foodInstanceView = this;

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
        transform.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canShowPointerEnterAnim){
            OnPointerEnterEvent.Invoke();
        }
    }
    public GameObject GO() => gameObject;





}

