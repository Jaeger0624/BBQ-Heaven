using QFramework;
using TMPro;
using UnityEngine;

public class TopBlankController : MonoBehaviour, IController
{
    [SerializeField] private TextMeshProUGUI coinText;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private void Update()
    {
        UpdateInfo();
    }


    private void UpdateInfo(){
        coinText.text = $"<color=yellow>Q:</color>{this.GetSystem<IEconomySystem>().coin.Value}";
    }
}
