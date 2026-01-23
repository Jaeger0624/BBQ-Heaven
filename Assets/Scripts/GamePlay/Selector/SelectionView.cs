using System.Collections.Generic;
using cfg;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SelectionType
{
    Card,
    Food,
    Choices
}
public class SelectionView : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI effectDescriptionText;
    [SerializeField] private Image Image;
    [SerializeField] private ButtonUI button;
    public void Bind(string Id, SelectionType selectionType)
    {
        button.OnClick.RemoveAllListeners();
        switch (selectionType)
        {
            case SelectionType.Card:
                CardData cardData = this.GetSystem<IDataSystem>().GetCardData(Id);
                if (cardData == null){
                    Debug.LogError("卡牌数据不存在：" + Id);
                    return;
                }
                nameText.text = cardData.Name;
                descriptionText.text = cardData.Description;
                effectDescriptionText.text = cardData.Description;
                // Image.sprite = Resources.Load<Sprite>("Sprites/" + cardData.Sprite);
                break;
            case SelectionType.Food:
                FoodData foodData = this.GetSystem<IDataSystem>().GetFoodData(Id);
                if (foodData == null){
                    Debug.LogError("食材数据不存在：" + Id);
                    return;
                }
                nameText.text = foodData.Name;
                descriptionText.text = foodData.Description;
                effectDescriptionText.text = foodData.EffectDescription;
                Image.sprite = Resources.Load<Sprite>("Sprites/" + foodData.Sprite);
                break;
            case SelectionType.Choices:
                OptionData optionData = this.GetSystem<IDataSystem>().GetOptionData(Id);
                if (optionData == null){
                    Debug.LogError("选项数据不存在：" + Id);
                    return;
                }
                nameText.text = optionData.Name;
                descriptionText.text = "";
                effectDescriptionText.text = optionData.EffectDescription;
                // Image.sprite = Resources.Load<Sprite>("Sprites/" + optionData.Sprite);
                break;
            default:
                Debug.LogError("不支持的选择类型：" + selectionType);
                break;
        }
    }
}