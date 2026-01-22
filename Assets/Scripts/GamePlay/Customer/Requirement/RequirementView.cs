using QFramework;
using TMPro;
using UnityEngine;

public class RequirementView : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI startAmountText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    private CustomerRequirement requirement;
    public void Bind(CustomerRequirement requirement){
        this.requirement = requirement;
        UpdateVisual();
    }

    private void UpdateVisual(){
        startAmountText.text = (requirement.StarAmount / 2f).ToString("F1");
        descriptionText.text = requirement.GetDescription();
    }
}