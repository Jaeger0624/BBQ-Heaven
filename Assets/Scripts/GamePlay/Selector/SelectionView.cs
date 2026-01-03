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
    [SerializeField] private Image Image;
    [SerializeField] private ButtonUI button;
    public void Bind(string cardId, SelectionType selectionType)
    {
        switch (selectionType)
        {
            case SelectionType.Card:
                CardData cardData = this.GetSystem<IDataSystem>().GetCardData(cardId);
                if (cardData == null){
                    Debug.LogError("卡牌数据不存在：" + cardId);
                    return;
                }
                nameText.text = cardData.Name;
                descriptionText.text = cardData.Description;
                // Image.sprite = Resources.Load<Sprite>("Sprites/" + cardData.Sprite);
                break;
            default:
                Debug.LogError("不支持的选择类型：" + selectionType);
                break;
        }
    }
}