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
    private DailyEconomyInfo dailyEconomyInfo;
    [SerializeField] private CanvasGroup canvasGroup;
    void Awake()
    {
        OnHide();
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
        SetInfo(evt.dailyEconomyInfo);
        OnShow();
    }
    private void OnCloseDailyEconomyPanelEvent(CloseDailyEconomyPanelEvent evt)
    {
        OnHide();
    }
    private void SetInfo(DailyEconomyInfo dailyEconomyInfo){
        this.dailyEconomyInfo = dailyEconomyInfo;
        UpdateScoreText();
    }
    public void UpdateScoreText(){
        if (dailyEconomyInfo == null) return;
        _scoreText_基础得分.text = $"{dailyEconomyInfo.baseScore}<color=yellow>Q</color>";
        _scoreText_利润得分.text = $"{dailyEconomyInfo.interestScore}<color=yellow>Q</color>";
        _scoreText_奖金得分.text = $"{dailyEconomyInfo.profitScore}<color=yellow>Q</color>";
        _scoreText_总得分.text = $"{dailyEconomyInfo.totalScore}<color=yellow>Q</color>";
    }

    void OnShow(){
        canvasGroup.DOFade(1, 0.5f);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    void OnHide(){
        canvasGroup.DOFade(0, 0.5f);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}

#region 事件
public class CreateDailyEconomyPanelEvent : AbstractEvent{
    public DailyEconomyInfo dailyEconomyInfo;
    public CreateDailyEconomyPanelEvent(DailyEconomyInfo dailyEconomyInfo){
        this.dailyEconomyInfo = dailyEconomyInfo;
    }
}

public class CloseDailyEconomyPanelEvent : AbstractEvent{}
#endregion