using cfg;
using QFramework;
using TMPro;
using UnityEngine;

public class OptionView : MonoBehaviour, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI effectDescriptionText;
    [SerializeField] private ButtonUI button;

    public void Bind(OptionData optionData)
    {
        // 1. 更新UI
        nameText.text = optionData.Name;
        descriptionText.text = optionData.Description;
        effectDescriptionText.text = optionData.EffectDescription;

        // 2. 绑定点击事件
        button.OnClick.AddListener(() =>
        {
            this.SendEvent(new SelectOptionEvent(optionData));
        });

        // 3. 设置按钮文本
        button.SetText(optionData.Name);
    }
}