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
    void Awake()
    {
        // 测试用，只在编辑器下显示
        gameObject.SetActive(false);
        #if UNITY_EDITOR
        gameObject.SetActive(true);
        #endif
    }
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
            default:
                Debug.LogError($"不支持的测试类型：{testType}");
                break;
        }

    }
    void OnButtonNameClick()
    {
        string name = inputField.text;
        switch (testType)
        {
            case TestType.卡牌:
                CardData cardData = this.GetSystem<IDataSystem>().GetAllCardData().FirstOrDefault(x => x.Name == name);
                if (cardData == null)
                {
                    Debug.LogError("卡牌数据不存在");
                }
                this.GetSystem<ICardSystem>().AddCardToHand(cardData.ID);
                break;
            case TestType.吉祥物:
                MascotData mascotData = this.GetSystem<IDataSystem>().GetAllMascotData().FirstOrDefault(x => x.Name == name);
                if (mascotData == null)
                {
                    Debug.LogError("吉祥物数据不存在");
                }
                this.GetSystem<IMascotSystem>().AddMascot(mascotData.ID);
                break;
            default:
                Debug.LogError($"不支持的测试类型：{testType}");
                break;
        }
    }
}
