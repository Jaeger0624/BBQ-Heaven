using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionPanel : MonoBehaviour, IController
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup choiceCanvasGroup;
    [SerializeField] private ButtonUI changeVisibleButton;
    [SerializeField] private Transform selectionContainer;
    [SerializeField] private GameObject choicePrefab;
    private bool isChoicesVisible = false;

    // 刷新次数
    [SerializeField] private TextMeshProUGUI refreshAmountText;
    [SerializeField] private ButtonUI refreshButton;
    private int refreshAmount = 0;
    private List<SelectionView> selectionViews = new List<SelectionView>();
    private ISelectionRequest currentRequest = null;
    void Start()
    {

        // 1. 绑定按钮点击事件
        changeVisibleButton.OnClick.AddListener(OnChangeVisibleButtonClick);
        refreshButton.OnClick.AddListener(OnRefreshButtonClick);
        refreshAmountText.text = $"刷新：{refreshAmount}";

        // 2. 隐藏面板
        Hide(true);
        ShowChoices();
        this.RegisterEvent<RequestSelectionEvent>(OnRequestSelection);
    }
    void OnDestroy()
    {
        this.UnRegisterEvent<RequestSelectionEvent>(OnRequestSelection);
    }
    private void OnChangeVisibleButtonClick(){
        if (isChoicesVisible){
            HideChoices();
        }
        else{
            ShowChoices();
        }
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
        RequestSelectionEvent evt = currentRequest.Form();

        // 2. 设置标题
        titleText.text = evt.Title;

        if (evt.Choices.Count != selectionViews.Count) {Debug.LogError("选择数量不一致"); return;}
        // 3. 重置选择
        for (int i = 0; i < evt.Choices.Count; i++)
        {
            var choice = evt.Choices[i];
            SelectionView selectionView = selectionViews[i];
            selectionView.Bind(choice, evt.SelectionType);
            selectionView.GetComponent<ButtonUI>().OnClick.AddListener(() =>
            {
                // 触发回调，通知 GA 恢复执行
                evt.OnSelect?.Invoke(choice);
                
                // 关闭面板
                Hide();
            });
        }
    }

    // 处理一个选择请求
    private void OnRequestSelection(RequestSelectionEvent evt)
    {
        // 1. 重置选择
        ResetSelection();
        refreshAmount = evt.refreshAmount;

        

        // 2. 设置标题
        titleText.text = evt.Title;

        // 3. 生成选项
        foreach (var choice in evt.Choices)
        {
            var go = Instantiate(choicePrefab, selectionContainer);
            SelectionView selectionView = go.GetComponent<SelectionView>();
            selectionViews.Add(selectionView);
            selectionView.Bind(choice, evt.SelectionType);
            
            // 绑定点击事件
            go.GetComponent<ButtonUI>().OnClick.AddListener(() =>
            {
                // A. 触发回调，通知 GA 恢复执行
                evt.OnSelect?.Invoke(choice);
                
                // B. 关闭面板
                Hide();
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
        if (instant){
            canvasGroup.alpha = 0;
        }
        else{
            canvasGroup.DOFade(0, 0.3f).SetEase(Ease.OutSine).SetUpdate(true);
        }
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
    [Button("显示")]
    private void Show(){
        canvasGroup.DOFade(1, 0.3f).SetEase(Ease.OutSine).SetUpdate(true);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    private void ShowChoices(){
        choiceCanvasGroup.DOFade(1, 0.3f).SetEase(Ease.OutSine).SetUpdate(true);
        choiceCanvasGroup.blocksRaycasts = true;
        choiceCanvasGroup.interactable = true;
        isChoicesVisible = true;
    }
    private void HideChoices(){
        choiceCanvasGroup.DOFade(0, 0.3f).SetEase(Ease.OutSine).SetUpdate(true);
        choiceCanvasGroup.blocksRaycasts = false;
        choiceCanvasGroup.interactable = false;
        isChoicesVisible = false;
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    [Button("测试：卡牌选择")]
    private void Test_CardSelection()
    {
        List<CardData> cardDatas = new List<CardData>();
        // 随机选取3张卡牌
        cardDatas = this.GetSystem<IDataSystem>().GetAllCardData().OrderBy(x => Guid.NewGuid()).Take(3).ToList();
        // 构建回调
        Action<string> onSelect = (cardId) =>
        {
            Debug.Log("选择了一张卡牌：" + cardId);
            this.GetSystem<ICardSystem>().AddCardToRepository(cardId);
        };
        this.GetSystem<ISelectorSystem>().RequestSelection(new SelectionRequest_随机卡牌());
    }
    [Button("测试：选择食材")]
    private void Test_FoodSelection()
    {
        List<FoodData> foodDatas = new List<FoodData>();
        foodDatas = this.GetSystem<IDataSystem>().GetAllFoodData().OrderBy(x => Guid.NewGuid()).Take(3).ToList();
        Action<string> onSelect = (foodId) =>
        {
            Debug.Log("选择了一种食材：" + foodId);
            this.GetSystem<IFoodSystem>().AddFoodToRepository(new List<FoodPack>(){new FoodPack(foodId, 1)});
        };
        this.GetSystem<ISelectorSystem>().RequestSelection(new SelectionRequest_随机食材());
    }
}