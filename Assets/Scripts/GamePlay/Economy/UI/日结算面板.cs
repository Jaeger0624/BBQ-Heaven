using System.Linq;
using DG.Tweening;
using QFramework;
using TMPro;
using UnityEngine;

public class 日结算面板 : MonoBehaviour, IController
{
    [SerializeField] private TextMeshProUGUI _scoreText_基础得分;
    [SerializeField] private TextMeshProUGUI _scoreText_利润得分;
    [SerializeField] private TextMeshProUGUI _scoreText_奖金得分;
    [SerializeField] private TextMeshProUGUI _scoreText_总得分;
    [SerializeField] private TextMeshProUGUI _text_交易次数;
    [SerializeField] private TextMeshProUGUI _text_最佳交易;
    [SerializeField] private TextMeshProUGUI _text_食材销量冠军;
    [SerializeField] private TextMeshProUGUI _text_遭遇信息;
    private DailyInfo dailyInfo;
    [SerializeField] private CanvasGroup canvasGroup;
    void Awake()
    {
        OnHide(true);
    }
    void OnEnable()
    {
        this.RegisterEvent<CreateDailyEconomyPanelEvent>(OnCreateDailyEconomyPanelEvent);
        this.RegisterEvent<CloseDailyEconomyPanelEvent>(OnCloseDailyEconomyPanelEvent);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<CreateDailyEconomyPanelEvent>(OnCreateDailyEconomyPanelEvent);
        this.UnRegisterEvent<CloseDailyEconomyPanelEvent>(OnCloseDailyEconomyPanelEvent);
    }
    private void OnCreateDailyEconomyPanelEvent(CreateDailyEconomyPanelEvent evt)
    {
        SetInfo(evt.dailyInfo);
        OnShow();
    }
    private void OnCloseDailyEconomyPanelEvent(CloseDailyEconomyPanelEvent evt)
    {
        OnHide();
    }
    private void SetInfo(DailyInfo dailyInfo){
        this.dailyInfo = dailyInfo;
        UpdateScoreText();
        UpdateText();
    }
    public void UpdateScoreText(){
        if (dailyInfo == null) return;
        _scoreText_基础得分.text = $"{dailyInfo.DailyEconomy.baseScore}<color=yellow>Q</color>";
        _scoreText_利润得分.text = $"{dailyInfo.DailyEconomy.interestScore}<color=yellow>Q</color>";
        _scoreText_奖金得分.text = $"{dailyInfo.DailyEconomy.profitScore}<color=yellow>Q</color>";
        _scoreText_总得分.text = $"{dailyInfo.DailyEconomy.totalScore}<color=yellow>Q</color>";
    }
    public void ResetText(){
        _text_交易次数.text = "0";
        _text_最佳交易.text = "无";
        _text_食材销量冠军.text = "无";
        _text_遭遇信息.text = "多么平淡的一天！";
    }
    public void UpdateText(){
        if (dailyInfo == null) return;
        ResetText();

        _text_交易次数.text = $"{dailyInfo.dealCount}";
        if (dailyInfo.BestDeal != null){
            _text_最佳交易.text = $"{dailyInfo.BestDeal.price}";
        }
        if (dailyInfo.FoodSales.Count > 0){
            string maxFoodSalesKey = dailyInfo.FoodSales.OrderByDescending(x => x.Value).First().Key;
            string name = this.GetSystem<IDataSystem>().GetFoodData(maxFoodSalesKey).Name;
            int maxFoodSales = dailyInfo.FoodSales.OrderByDescending(x => x.Value).First().Value;
            float size = _text_食材销量冠军.fontSize;
            _text_食材销量冠军.text = $"{name} <size={size/2}>x{maxFoodSales}</size>";
        }
        if (dailyInfo.EncounterNames.Count > 0){
            _text_遭遇信息.text = $"{string.Join(", ", dailyInfo.EncounterNames)}";
        }
    }

    void OnShow(){
        canvasGroup.DOFade(1, 0.5f);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    void OnHide(bool instant = false){
        if (instant){
            canvasGroup.alpha = 0;
        }
        else{
            canvasGroup.DOFade(0, 0.5f);
        }
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}

#region 事件
public class CreateDailyEconomyPanelEvent : AbstractEvent{
    public DailyInfo dailyInfo;
    public CreateDailyEconomyPanelEvent(DailyInfo dailyInfo){
        this.dailyInfo = dailyInfo;
    }
}

public class CloseDailyEconomyPanelEvent : AbstractEvent{}
#endregion