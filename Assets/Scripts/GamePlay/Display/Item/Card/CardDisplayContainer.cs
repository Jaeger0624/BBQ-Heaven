using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CardDisplayContainer : MonoBehaviour, IController, IDisplayContainer{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private Transform cardContainer;
    DisplayCardView cardItemPrefab => SettingManager.Instance.PrefabSettings.cardItemPrefab;
    private DisplayItemGenerator<CardUIContext, DisplayCardView> _generator;
    [LabelText("是否控制网格布局")]
    [SerializeField] private bool controlGridLayout = false;
    [ShowIf("controlGridLayout")]
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [ShowIf("controlGridLayout")]
    [SerializeField] private Vector2 cellSize = new Vector2(100, 100);
    [ShowIf("controlGridLayout")]
    [SerializeField] private Vector2 cellSpacing = new Vector2(10, 10);
    private IDisplayTask<CardUIContext, DisplayCardView> _displayTask;
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
        _generator = new DisplayItemGenerator<CardUIContext, DisplayCardView>(cardContainer, cardItemPrefab);
    }
    public void RefreshUI(List<CardUIContext> cardInstances,
    IItemInteractStrategy<CardUIContext, DisplayCardView> itemInteractStrategy){
        _generator.Generate(cardInstances, itemInteractStrategy);
    }
    public void RefreshUI(){
        if (_displayTask == null) {Debug.LogError("DisplayTask 是空的，但尝试无参数刷新UI"); return;}
        List<CardUIContext> cardUIContexts = _displayTask.GetList().ToList();
        RefreshUI(cardUIContexts, _displayTask.GetStrategy());
    }
    public void SetDisplayTask(IDisplayTask<CardUIContext, DisplayCardView> displayTask){
        _displayTask = displayTask;
    }
    public GameObject GetGameObject() => gameObject;
}