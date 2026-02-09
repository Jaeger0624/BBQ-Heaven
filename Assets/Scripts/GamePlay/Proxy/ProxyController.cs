using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using TMPro;
using UnityEngine;

public class ProxyController : MonoBehaviour, IController, ICanSendEvent
{
    [SerializeField] private TextMeshProUGUI _currentScoreText;
    [SerializeField] private TextMeshProUGUI _targetScoreText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _stickInfoText;
    [SerializeField] private TextMeshProUGUI _foodInfoText;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    void Update()
    {
        _currentScoreText.text = $"当前: {this.GetSystem<IScoreSystem>().Score}";
        _targetScoreText.text = $"目标: {this.GetSystem<IScoreSystem>().TargetScore}";
        UpdateTimeText();
        UpdateStickInfoText();
        UpdateFoodInfoText();
    }
    private void UpdateTimeText()
    {
        if (this.GetSystem<ITimeSystem>() == null){
            return;
        }
        if (this.GetSystem<ITimeSystem>().CurrentTime == null || this.GetSystem<ITimeSystem>().TargetTime == null){
            return;
        }
        int currentTimePoint = this.GetSystem<ITimeSystem>().CurrentTime.GetTotalTimePoint() - this.GetSystem<ITimeSystem>().CurrentTime.GetOriginalTimeInfo().GetTotalTimePoint();
        int targetTimePoint = this.GetSystem<ITimeSystem>().TargetTime.GetTotalTimePoint() - this.GetSystem<ITimeSystem>().CurrentTime.GetOriginalTimeInfo().GetTotalTimePoint();
        _timeText.text = $"时间: {currentTimePoint}/{targetTimePoint}";
    }
    private void UpdateStickInfoText()
    {
        if (this.GetSystem<IStickSystem>() == null){
            Debug.LogError("【ProxyController】StickSystem 为空");
            return;
        }
        if (this.GetSystem<IStickSystem>().CurrentSticks.Count == 0){
            _stickInfoText.text = "串: 无";
            return;
        }
        // 串联
        string stickInfo = String.Join(", ", this.GetSystem<IStickSystem>().CurrentSticks.Select(x => x.name));
        _stickInfoText.text = $"串: {stickInfo}";
    }
    private void UpdateFoodInfoText()
    {
        if (this.GetSystem<IFoodSystem>() == null){
            return;
        }
        if (this.GetSystem<IFoodSystem>().FoodPile == null){
            return;
        }
        List<FoodInstance> foodInstances = this.GetSystem<IFoodSystem>().GetFoodInstances().Values.ToList().Where(x => x.state == FoodInstanceState.棋盘上).ToList();
        _foodInfoText.text = $"食材: {foodInstances.Count}";
    }
}