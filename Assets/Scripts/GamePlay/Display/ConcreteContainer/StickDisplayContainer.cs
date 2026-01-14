using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class StickDisplayContainer : MonoBehaviour, IController, IDisplayContainer{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private Transform foodContainer;
    [SerializeField] private DisplayStickView displayStickViewPrefab;
    private DisplayItemGenerator<Stick, DisplayStickView> _generator;
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
        _generator = new DisplayItemGenerator<Stick, DisplayStickView>(foodContainer, displayStickViewPrefab);
    }
    public void RefreshUI(List<Stick> stickInstances, IItemInteractStrategy<Stick, DisplayStickView> itemInteractStrategy){
        _generator.Generate(stickInstances, itemInteractStrategy);
    }
    public GameObject GetGameObject() => gameObject;
}

