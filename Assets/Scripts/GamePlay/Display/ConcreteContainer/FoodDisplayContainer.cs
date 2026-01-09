using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public enum FoodContainerType{
    单选删除,
    多选删除,
    商店,
    构筑展示
}

public class FoodDisplayContainer : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private Transform foodContainer;
    [SerializeField] private DisplayFoodView foodItemPrefab;
    private DisplayItemGenerator<FoodUIContext, DisplayFoodView> _generator;
    void Start()
    {
        Init();
    }

    private void Init(){
        _generator = new DisplayItemGenerator<FoodUIContext, DisplayFoodView>(foodContainer, foodItemPrefab);
    }
    public void RefreshUI(List<FoodUIContext> foodInstances, IItemInteractStrategy<FoodUIContext, DisplayFoodView> itemInteractStrategy){
        _generator.Generate(foodInstances, itemInteractStrategy);
    }
}