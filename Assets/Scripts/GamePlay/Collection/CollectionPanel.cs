using System.Collections.Generic;
using cfg;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;

public class CollectionPanel : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private CollectionType currentCollectionType = CollectionType.Food;
    private int currentParam = 0;

    void Start()
    {
    }
    public void OnShow(){
        SelectCollection(currentCollectionType);
        RefreshCollection(currentParam);
    }
    private void SelectCollection(CollectionType collectionType){
        // 1. 更新当前类型
        currentCollectionType = collectionType;
        // 2. 刷新UI（重置一下参数）
        currentParam = 0;
    }
    private void RefreshCollection(int param)
    {
        Dictionary<string, CollectionState> itemStates = this.GetSystem<ICollectionSystem>().GetItemStates(currentCollectionType);
        CollectionSelector collectionSelector = new CollectionSelector();
        CloseAll();
        switch (currentCollectionType)
        {
            case CollectionType.Food:
                FoodDisplayContainer foodDisplayContainer = GetComponentInChildren<FoodDisplayContainer>(true);
                if (foodDisplayContainer == null) {Debug.LogError("FoodDisplayContainer is not found"); return;}
                List<FoodUIContext> foodUIContexts = collectionSelector.GetFoodUIContexts(param);
                // 1. 刷新UI
                foodDisplayContainer.RefreshUI(foodUIContexts, new CollectionFoodStrategy());
                // 2. 显示UI
                foodDisplayContainer.GetGameObject().SetActive(true);
                break;
            case CollectionType.Stick:
                StickDisplayContainer stickDisplayContainer = GetComponentInChildren<StickDisplayContainer>();
                if (stickDisplayContainer == null) {Debug.LogError("StickDisplayContainer is not found"); return;}
                break;
            case CollectionType.Card:

                break;
            default:
            Debug.LogError("Unknown collection type: " + currentCollectionType);
            break;
        }
    }

    private void CloseAll(){
        IDisplayContainer[] displayContainers = GetComponentsInChildren<IDisplayContainer>();
        foreach (var displayContainer in displayContainers)
        {
            displayContainer.GetGameObject().SetActive(false);
        }
    }
    public void ChangeCollectionType(int collectionTypeIndex){
        CollectionType collectionType = (CollectionType)collectionTypeIndex;
        if (collectionType == currentCollectionType) return;
        SelectCollection(collectionType);
    }
}
