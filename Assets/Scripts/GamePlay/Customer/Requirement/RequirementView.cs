using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RequirementView : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI startAmountText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image awardIcon;
    private CustomerRequirement requirement;
    public void Bind(CustomerRequirement requirement){
        this.requirement = requirement;
        UpdateVisual();
    }

    private void UpdateVisual(){
        startAmountText.text = (requirement.StarAmount / 2f).ToString("F1");
        descriptionText.text = requirement.GetDescription();
        switch (requirement.AwardType){
            case RequirementAwardType.倍率:
                awardIcon.color = Color.yellow;
                break;
            case RequirementAwardType.声望:
                awardIcon.color = Color.blue;
                break;
            case RequirementAwardType.金币:
                awardIcon.color = Color.red;
                break;
            default:
                Debug.LogError($"【RequirementView】未知奖励类型: {requirement.AwardType}");
                break;
        }
    }
}