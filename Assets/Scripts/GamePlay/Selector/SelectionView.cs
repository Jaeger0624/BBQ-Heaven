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
    [SerializeField] private Image cardImage;
    [SerializeField] private ButtonUI button;
    public void Bind(CardData cardData)
    {
        nameText.text = cardData.Name;
        descriptionText.text = cardData.Description;
        // cardImage.sprite = Resources.Load<Sprite>("Sprites/" + cardData.Sprite);
    }
}