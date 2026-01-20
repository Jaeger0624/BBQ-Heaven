using System;
using System.Linq;
using cfg;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TestType{
    卡牌,
    吉祥物,
}
public class CardTestUI : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public TextMeshProUGUI testTypeText;
    public TMP_InputField inputField;
    public ButtonUI ButtonID;
    public ButtonUI ButtonName;
    public ButtonUI ButtonTestType;
    public TestType testType = TestType.卡牌;
    void Start()
    {
        ButtonID.OnClick.AddListener(OnButtonIDClick);
        ButtonName.OnClick.AddListener(OnButtonNameClick);
        ButtonTestType.OnClick.AddListener(OnButtonTestTypeClick);
        testTypeText.text = testType.ToString();
    }
    void OnButtonTestTypeClick()
    {
        testType = (TestType)((int)(testType + 1) % Enum.GetValues(typeof(TestType)).Length);
        testTypeText.text = testType.ToString();
    }
    void OnButtonIDClick()
    {
        string id = inputField.text;
        switch (testType)
        {
            case TestType.卡牌:
                CardData cardData = this.GetSystem<IDataSystem>().GetCardData(id);
                if (cardData == null)
                {
                    Debug.LogError("卡牌数据不存在");
                }
                this.GetSystem<ICardSystem>().AddCardToHand(cardData.ID);
                break;
            case TestType.吉祥物:
                MascotData mascotData = this.GetSystem<IDataSystem>().GetMascotData(id);
                if (mascotData == null)
                {
                    Debug.LogError("吉祥物数据不存在");
                }
                this.GetSystem<IMascotSystem>().AddMascot(mascotData.ID);
                break;
                
        }

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
