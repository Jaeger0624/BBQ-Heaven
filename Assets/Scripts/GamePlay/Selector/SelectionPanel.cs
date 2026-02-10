using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using cfg;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
// 这种Type不会决定逻辑区别，只是用来导向不同的预制体而已
public enum SelectionPanelType{
    日间事件,
    食材,
    通用选择,
}
public class SelectionPanel : MonoBehaviour, IController
{
    public SelectionPanelType selectionPanelType;
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    private IUIPanel Panel;
    [SerializeField] private Transform selectionContainer;
    [SerializeField] private GameObject choicePrefab;
    // 刷新次数
    [SerializeField] private TextMeshProUGUI refreshAmountText;
    [SerializeField] private ButtonUI refreshButton;
    private int refreshAmount = 0;
    private List<SelectionView> selectionViews = new List<SelectionView>();
    private ISelectionRequest currentRequest = null;
    void Start()
    {
        Panel = GetComponent<IUIPanel>();
        if (Panel == null) {Debug.LogError("SelectionPanel: Panel is not set"); return;}
        // 1. 绑定按钮点击事件
        refreshButton.OnClick.AddListener(OnRefreshButtonClick);
        refreshAmountText.text = $"刷新：{refreshAmount}";

        // 2. 隐藏面板
        Hide(true);
        this.RegisterEvent<CreateSelectionEvent>(OnRequestSelection);
    }
    void OnDestroy()
    {
        this.UnRegisterEvent<CreateSelectionEvent>(OnRequestSelection);
    }

    private void OnRefreshButtonClick(){
        if (refreshAmount <= 0) return;
        refreshAmount--;
        refreshAmountText.text = $"刷新：{refreshAmount}";

        RefreshSelection();
    }
    private void RefreshSelection(){
        if (currentRequest == null) {Debug.LogError("当前没有选择请求"); return;}
        // 1. 重新生成选择请求
        SelectRequest evt = currentRequest.Create();

        // 2. 设置标题
        titleText.text = evt.Title;

        if (evt.Contexts.Count != selectionViews.Count) {Debug.LogError("选择数量不一致"); return;}
        // 3. 重置选择
        for (int i = 0; i < evt.Contexts.Count; i++)
        {
            var context = evt.Contexts[i];
            SelectionView selectionView = selectionViews[i];
            selectionView.Bind(context);
            selectionView.GetComponent<ButtonUI>().OnClick.AddListener(() =>
            {
                // 触发回调，通知 GA 恢复执行
                evt.OnSelect?.Invoke(context);
                
                // 关闭面板
                Hide();
            });
        }
    }

    // 处理一个选择请求
    private void OnRequestSelection(CreateSelectionEvent evt)
    {
        if (evt.SelectionPanelType != selectionPanelType) return;

        AsyncSubject<Unit> sub = evt.GetSubject();

        // 1. 重置选择与刷新次数
        ResetSelection();
        refreshAmount = evt.RefreshAmount;

        if (refreshAmount == 0)
        {
            refreshButton.gameObject.SetActive(false);
            refreshAmountText.text = "";
        }
        else
        {
            refreshButton.gameObject.SetActive(true);
            refreshAmountText.text = $"刷新：{refreshAmount}";
        }

        // 2. 设置当前请求
        currentRequest = evt.SelectionRequest;
        Debug.Log("【SelectionPanel】当前请求：" + currentRequest.Title + "，类型：" + evt.SelectionPanelType);
        // 3. 生成选项
        SelectRequest request = currentRequest.Create();
        titleText.text = request.Title;
        foreach (var context in request.Contexts)
        {
            var go = Instantiate(choicePrefab, selectionContainer);
            SelectionView selectionView = go.GetComponent<SelectionView>();
            selectionViews.Add(selectionView);
            selectionView.Bind(context);
            
            // 绑定点击事件
            go.GetComponent<ButtonUI>().OnClick.AddListener(() =>
            {
                // A. 触发回调，通知 GA 恢复执行
                request.OnSelect?.Invoke(context);
                
                // B. 关闭面板
                Hide();


                // C. 触发回调
                sub.OnNext(Unit.Default);
                sub.OnCompleted();
            });
        }
        Show();
    }

    private void ResetSelection(){
        foreach (var selectionView in selectionViews)
        {
            Destroy(selectionView.gameObject);
        }
        selectionViews.Clear();
    }

    [Button("隐藏")]
    private void Hide(bool instant = false){
        Panel.Hide();
    }
    [Button("显示")]
    private void Show(){
        Panel.Show();
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    [Button("测试：卡牌选择")]
    private void Test_CardSelection(SelectionPanelType selectionPanelType)
    {
        List<CardData> cardDatas = new List<CardData>();
        // 随机选取3张卡牌
        cardDatas = this.GetSystem<IDataSystem>().GetAllCardData().OrderBy(x => Guid.NewGuid()).Take(3).ToList();

        this.GetSystem<ISelectorSystem>().RequestSelection(new SelectionRequest_随机卡牌(3), 3, selectionPanelType);
    }
    [Button("测试：选择食材")]
    private void Test_FoodSelection(SelectionPanelType selectionPanelType)
    {
        List<FoodData> foodDatas = new List<FoodData>();
        foodDatas = this.GetSystem<IDataSystem>().GetAllFoodData().OrderBy(x => Guid.NewGuid()).Take(3).ToList();

        this.GetSystem<ISelectorSystem>().RequestSelection(new SelectionRequest_随机食材(3), 3, selectionPanelType);
    }
    [Button("测试：选择商店")]
    private void Test_ShopSelection(SelectionPanelType selectionPanelType){
        List<ShopType> shopTypes = new List<ShopType>();
        shopTypes.Add(ShopType.普通);
        shopTypes.Add(ShopType.批发);
        this.GetSystem<ISelectorSystem>().RequestSelection(new SelectionRequest_选择商店(shopTypes), 0, selectionPanelType);
    }
}