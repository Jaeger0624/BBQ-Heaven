using UnityEngine;
using TMPro;
using QFramework;
using System.Collections.Generic;

public class CustomerTimeView : MonoBehaviour, IShowTooltip, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI customerTimeText;
    void Start()
    {
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDay).UnRegisterWhenDisabled(this.gameObject);
    }
    private void OnStartNewDay(StartNewDayEvent evt)
    {
        if (evt.StageMeet(EventStage.System)){
            UpdateCustomerTimeInfo();
        }
    }
    public List<TooltipInfo> GetTooltipInfo()
    {
        return new List<TooltipInfo>{
            new TooltipInfo("经营时间"),
        };
    }
    public string GetTooltipText()
    {
        string displayName = this.GetSystem<ICustomerSystem>().GetCustomerTimeDisplayName();
        float chance = this.GetSystem<ICustomerSystem>().GetOldCustomerChance();
        return $"{displayName} 老客:{(int)(chance * 100)}%";
    }

    /// <summary>
    /// 更新时段显示信息
    /// </summary>
    private void UpdateCustomerTimeInfo(){
        customerTimeText.text = GetTooltipText();
    }
}