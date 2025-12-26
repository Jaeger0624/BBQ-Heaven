using cfg;
using QFramework;
using TMPro;
using UnityEngine;

public class OptionView : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI effectDescriptionText;
    [SerializeField] private ButtonUI button;


    public void Bind(OptionData optionData)
    {
        descriptionText.text = optionData.Description;
        effectDescriptionText.text = optionData.EffectDescription;
        CGA cga = new CGA(optionData.Action);
        button.OnClick.AddListener(() =>
        {
            this.GetSystem<IGASystem>().ApplyCGA(this, cga, null);
        });
        button.SetText(optionData.Name);
    }
}