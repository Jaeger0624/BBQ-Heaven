using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetSeedPanel : MonoBehaviour, IController{
    [SerializeField] private TMP_InputField seedInputField;
    [SerializeField] private Button randomButton;
    private int currentSeed;
    void Awake()
    {
        seedInputField = GetComponentInChildren<TMP_InputField>();
        randomButton = GetComponentInChildren<Button>();
    }
    private void Start()
    {

        seedInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        GenerateRandomSeed();
        randomButton.onClick.AddListener(OnRandomButtonClick);
        seedInputField.onValueChanged.AddListener(OnValueChanged);
    }
    private void OnDestroy()
    {
        seedInputField.onValueChanged.RemoveListener(OnValueChanged);
        randomButton.onClick.RemoveListener(OnRandomButtonClick);
    }

    private void GenerateRandomSeed()
    {
        currentSeed = UnityEngine.Random.Range(0, 1000000);
        seedInputField.text = currentSeed.ToString();

        this.GetSystem<BlackboardSystem>().newGameInfo.seed = currentSeed;
    }
    private void OnRandomButtonClick() 
    {
        GenerateRandomSeed();
    }
    private void OnValueChanged(string text)
    {
        if (int.TryParse(text, out int seed))
        {
            currentSeed = seed;
        }
    }

    public int GetSeed() => currentSeed;
    public void Show()
    {
        this.gameObject.SetActive(true);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}