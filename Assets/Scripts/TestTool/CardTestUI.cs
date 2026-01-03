using System.Linq;
using cfg;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class CardTestUI : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public InputField inputField;
    public Button ButtonID;
    public Button ButtonName;
    void Start()
    {
        ButtonID.onClick.AddListener(OnButtonIDClick);
        ButtonName.onClick.AddListener(OnButtonNameClick);
    }
    void OnButtonIDClick()
    {
        string id = inputField.text;
        CardData cardData = this.GetSystem<IDataSystem>().GetCardData(id);
        if (cardData == null)
        {
            Debug.LogError("卡牌数据不存在");
        }
        this.GetSystem<ICardSystem>().AddC(cardData.ID);
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
