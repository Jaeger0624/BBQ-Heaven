using System.Collections.Generic;
using cfg;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SelectionType
{
    卡牌,
    食材,
    选项,
    自定义
}
public class SelectionView : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI effectDescriptionText;
    [SerializeField] private Image Image;
    [SerializeField] private ButtonUI button;
    public void Bind(SelectionBuildContext context)
    {
        button.OnClick.RemoveAllListeners();
        switch (context.SelectionType)
        {
            case SelectionType.卡牌:
                CardData cardData = this.GetSystem<IDataSystem>().GetCardData(context.Id);
                if (cardData == null){
                    Debug.LogError("卡牌数据不存在：" + context.Id);
                    return;
                }
                if (nameText != null){
                    nameText.text = cardData.Name;
                }
                if (descriptionText != null){
                    descriptionText.text = cardData.Description;
                }
                if (effectDescriptionText != null){
                    effectDescriptionText.text = cardData.Description;
                }
                break;
            case SelectionType.食材:
                FoodData foodData = this.GetSystem<IDataSystem>().GetFoodData(context.Id);
                if (foodData == null){
                    Debug.LogError("食材数据不存在：" + context.Id);
                    return;
                }
                if (nameText != null){
                    nameText.text = foodData.Name;
                }
                if (descriptionText != null){
                    descriptionText.text = foodData.Description;
                }
                if (effectDescriptionText != null){
                    effectDescriptionText.text = foodData.EffectDescription;
                }
                if (Image != null){
                    Image.sprite = Resources.Load<Sprite>("Sprites/" + foodData.Sprite);
                }
                break;
            case SelectionType.选项:
                OptionData optionData = this.GetSystem<IDataSystem>().GetOptionData(context.Id);
                if (optionData == null){
                    Debug.LogError("选项数据不存在：" + context.Id);
                    return;
                }
                if (nameText != null){
                    nameText.text = optionData.Name;
                }
                if (descriptionText != null){
                    descriptionText.text = "";
                }
                if (effectDescriptionText != null){
                    effectDescriptionText.text = optionData.EffectDescription;
                }
                break;
            case SelectionType.自定义:
                if (nameText != null){
                    nameText.text = context.Name;
                }
                if (descriptionText != null){
                    descriptionText.text = context.Description;
                }
                if (effectDescriptionText != null){
                    effectDescriptionText.text = context.EffectDescription;
                }
                break;
            default:
                Debug.LogError("不支持的选择类型：" + context.SelectionType);
                break;
        }
    }
}


public class SelectionBuildContext{
    public SelectionType SelectionType;
    public string Id;
    public string Name;
    public string Description;
    public string EffectDescription;

    public SelectionBuildContext(SelectionType selectionType, string id){
        this.SelectionType = selectionType;
        this.Id = id;

    }
}