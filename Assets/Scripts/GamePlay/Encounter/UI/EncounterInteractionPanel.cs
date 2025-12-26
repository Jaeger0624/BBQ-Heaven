using System.Collections.Generic;
using cfg;
using DG.Tweening;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncounterInteractionPanel : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    [Header("UI引用")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionViewPrefab;
    private List<OptionView> optionViews = new List<OptionView>();

    // 缓存当前正在处理的遭遇数据
    private InstantEncounterData currentData;

    void Start()
    {
        Hide(true);
        this.RegisterEvent<StartInstantEncounterEvent>(OnStartInstantEncounter);
    }

    void OnDestroy()
    {
        this.UnRegisterEvent<StartInstantEncounterEvent>(OnStartInstantEncounter);
    }

    private void OnStartInstantEncounter(StartInstantEncounterEvent evt)
    {
        currentData = evt.data;
        UpdateView();
        Show();
    }

    private void UpdateView()
    {
        if (currentData == null) return;

        // 1. 设置文本
        titleText.text = currentData.Name;
        descriptionText.text = currentData.Description;

        // 2. 生成选项
        RefreshOptions();
    }

    public void Show()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.DOFade(1, 0.3f).SetUpdate(true); // 忽略TimeScale，因为通常此时游戏会暂停
    }

    public void Hide(bool instant = false)
    {
        canvasGroup.blocksRaycasts = false;
        if (instant)
        {
            canvasGroup.alpha = 0;
        }
        else
        {
            canvasGroup.DOFade(0, 0.2f).SetUpdate(true);
        }
    }

    private void RefreshOptions()
    {
        // 清理旧按钮
        foreach (var optionView in optionViews)
        {
            Destroy(optionView.gameObject);
        }
        optionViews.Clear();

        if (currentData.Options == null || currentData.Options.Count == 0)
        {
            // 如果没有配置选项，生成一个默认的"确定"按钮
            CreateOptionView(this.GetSystem<IDataSystem>().GetOptionData("默认"));
            return;
        }

        foreach (var option in currentData.Options)
        {
            // 获取选项配置
            OptionData optionData = this.GetSystem<IDataSystem>().GetOptionData(option);

            // 创建按钮
            CreateOptionView(optionData);
        }
    }

    private void CreateOptionView(OptionData optionData)
    {
        // 1. 创建选项视图
        OptionView optionView = Instantiate(optionViewPrefab, optionsContainer).GetComponent<OptionView>();

        // 2. 绑定数据
        optionView.Bind(optionData);
        
        // 3. 添加到列表
        optionViews.Add(optionView);
    }
}