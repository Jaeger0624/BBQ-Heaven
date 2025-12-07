using QFramework;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SupplyUI : MonoBehaviour, IController  {
    [SerializeField] private Button supplyButton;
    [SerializeField] private TextMeshProUGUI supplyCountText;
    public string supplyLabelText = "Supply:";

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    void Awake()
    {

    }

    void Start()
    {
        supplyButton.onClick.AddListener(OnSupplyButtonClick);
        ReactiveProperty<int> currentSupplyCount = this.GetSystem<IFoodSystem>().GetCurrentSupplyCount();
        currentSupplyCount.Subscribe(x => {
            supplyCountText.text = $"{supplyLabelText}{x}";
        }).AddTo(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)){
            this.GetSystem<IFoodSystem>().SupplyFood(this.GetSystem<IFoodSystem>().foodSupplyer, true);
        }
    }

    private void OnSupplyButtonClick()
    {
        this.GetSystem<IFoodSystem>().SupplyFood(this.GetSystem<IFoodSystem>().foodSupplyer, true);
    }
}