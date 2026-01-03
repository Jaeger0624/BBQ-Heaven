using System.Linq;
using cfg;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardTestUI : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public TMP_InputField inputField;
    public ButtonUI ButtonID;
    public ButtonUI ButtonName;
    void Start()
    {
        ButtonID.OnClick.AddListener(OnButtonIDClick);
        ButtonName.OnClick.AddListener(OnButtonNameClick);
    }
    void OnButtonIDClick()
    {
        string id = inputField.text;
        CardData cardData = this.GetSystem<IDataSystem>().GetCardData(id);
        if (cardData == null)
        {
            Debug.LogError("卡牌数据不存在");
        }
        this.GetSystem<ICardSystem>().AddCardToHand(cardData.ID);
    }
    void OnButtonNameClick()
    {
        string name = inputField.text;
        CardData cardData = this.GetSystem<IDataSystem>().GetAllCardData().FirstOrDefault(x => x.Name == name);
        if (cardData == null)
        {
            Debug.LogError("卡牌数据不存在");
        }

    }
}
