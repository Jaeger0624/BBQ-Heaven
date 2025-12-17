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
    void Start()
    {
        Hide(true);
        changeVisibleButton.OnClick.AddListener(OnChangeVisibleButtonClick);
        ShowChoices();
        this.RegisterEvent<RequestCardSelectionEvent>(OnRequestSelection);
    }
    void OnDestroy()
    {
        this.UnRegisterEvent<RequestCardSelectionEvent>(OnRequestSelection);
    }
    private void OnChangeVisibleButtonClick(){
        if (isChoicesVisible){
            HideChoices();
        }
        else{
            ShowChoices();
        }
    }
    [Button("测试")]
    private void Test()
    {
        List<CardData> cardDatas = new List<CardData>();

        // 随机选取3张卡牌
        cardDatas = this.GetSystem<IDataSystem>().GetAllCardData().OrderBy(x => Guid.NewGuid()).Take(3).ToList();

        // 构建回调
        Action<CardData> onSelect = (cardData) =>
        {
            Debug.Log("选择了一张卡牌：" + cardData.Name);
            this.GetSystem<ICardSystem>().AddCardToRepository(cardData.ID);
        };

        OnRequestSelection(new RequestCardSelectionEvent(cardDatas, "测试：请选择一张卡牌", onSelect));
    }
    private void OnRequestSelection(RequestCardSelectionEvent evt)
    {
        // 1. 重置选择
        ResetSelection();

        // 2. 设置标题
        titleText.text = evt.Title;

        // 3. 显示选择
        // 3. 生成选项
        foreach (var cardData in evt.Choices)
        {
            var go = Instantiate(choicePrefab, selectionContainer);

            SelectionView selectionView = go.GetComponent<SelectionView>();
            selectionView.Bind(cardData);
            
            // 绑定点击事件
            go.GetComponent<ButtonUI>().OnClick.AddListener(() =>
            {
                // A. 触发回调，通知 GA 恢复执行
                evt.OnSelect?.Invoke(cardData);
                
                // B. 关闭面板
                Hide();
            });
        }
        Show();
    }

    private void ResetSelection(){
        foreach (Transform child in selectionContainer) Destroy(child.gameObject);
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
}