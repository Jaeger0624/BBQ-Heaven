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
        currentSeed = UnityEngine.Random.Range(0, 1000000);
        seedInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        seedInputField.text = currentSeed.ToString();
        randomButton.onClick.AddListener(OnRandomButtonClick);
        seedInputField.onValueChanged.AddListener(OnValueChanged);
    }
    private void OnDestroy()
    {
        seedInputField.onValueChanged.RemoveListener(OnValueChanged);
        randomButton.onClick.RemoveListener(OnRandomButtonClick);
    }
    private void OnRandomButtonClick() 
    {
        currentSeed = UnityEngine.Random.Range(0, 1000000);
        seedInputField.text = currentSeed.ToString();
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