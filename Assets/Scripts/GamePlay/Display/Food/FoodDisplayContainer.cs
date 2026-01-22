using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum FoodContainerType{
    单选删除,
    多选删除,
    商店,
    构筑展示
}
public interface IDisplayContainer{
    GameObject GetGameObject();
}
public enum FoodDisplayType{
    Item,
    Card
}
public class FoodDisplayContainer : MonoBehaviour, IController, IDisplayContainer{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private Transform foodContainer;
    DisplayFoodView foodItemPrefab => SettingManager.Instance.PrefabSettings.foodItemPrefab;
    DisplayFoodView foodCardItemPrefab => SettingManager.Instance.PrefabSettings.foodCardItemPrefab;
    [SerializeField] private FoodDisplayType displayType;
    private DisplayItemGenerator<FoodUIContext, DisplayFoodView> _generator;
    [LabelText("是否控制网格布局")]
    [SerializeField] private bool controlGridLayout = false;
    [ShowIf("controlGridLayout")]
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [ShowIf("controlGridLayout")]
    [SerializeField] private Vector2 cellSize = new Vector2(100, 100);
    [ShowIf("controlGridLayout")]
    [SerializeField] private Vector2 cellSpacing = new Vector2(10, 10);
    private IDisplayTask<FoodUIContext, DisplayFoodView> _displayTask;
    void Start()
    {
        Init();
    }
    void Update()
    {
        if (gridLayoutGroup == null || !controlGridLayout) return;
        gridLayoutGroup.cellSize = cellSize;
        gridLayoutGroup.spacing = cellSpacing;
    }
    private void Init(){
        if (displayType == FoodDisplayType.Item){
            _generator = new DisplayItemGenerator<FoodUIContext, DisplayFoodView>(foodContainer, foodItemPrefab);
        }else if (displayType == FoodDisplayType.Card){
            _generator = new DisplayItemGenerator<FoodUIContext, DisplayFoodView>(foodContainer, foodCardItemPrefab);
        }
    }
    public void RefreshUI(List<FoodUIContext> foodInstances,
    IItemInteractStrategy<FoodUIContext, DisplayFoodView> itemInteractStrategy){
        _generator.Generate(foodInstances, itemInteractStrategy);
    }
    public void RefreshUI(){
        if (_displayTask == null) {Debug.LogError("DisplayTask 是空的，但尝试无参数刷新UI"); return;}
        List<FoodUIContext> foodUIContexts = _displayTask.GetList().ToList();
        RefreshUI(foodUIContexts, _displayTask.GetStrategy());
    }
    public void SetDisplayTask(IDisplayTask<FoodUIContext, DisplayFoodView> displayTask){
        _displayTask = displayTask;
    }
    public GameObject GetGameObject() => gameObject;
}


