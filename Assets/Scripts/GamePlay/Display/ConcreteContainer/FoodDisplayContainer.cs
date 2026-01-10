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
public class FoodDisplayContainer : MonoBehaviour, IController, IDisplayContainer{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private Transform foodContainer;
    [SerializeField] private DisplayFoodView foodItemPrefab;
    private DisplayItemGenerator<FoodUIContext, DisplayFoodView> _generator;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private Vector2 cellSize = new Vector2(100, 100);
    [SerializeField] private Vector2 cellSpacing = new Vector2(10, 10);
    void Start()
    {
        Init();
    }
    void Update()
    {
        if (gridLayoutGroup == null) return;
        gridLayoutGroup.cellSize = cellSize;
        gridLayoutGroup.spacing = cellSpacing;
    }
    private void Init(){
        _generator = new DisplayItemGenerator<FoodUIContext, DisplayFoodView>(foodContainer, foodItemPrefab);
    }
    public void RefreshUI(List<FoodUIContext> foodInstances, IItemInteractStrategy<FoodUIContext, DisplayFoodView> itemInteractStrategy){
        _generator.Generate(foodInstances, itemInteractStrategy);
    }
    public GameObject GetGameObject() => gameObject;
}