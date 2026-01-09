using QFramework;
using TMPro;
using UniRx;
using UnityEngine;

public class TopBlankController : MonoBehaviour, IController
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI reputationText;
    [SerializeField] private TextMeshProUGUI levelText;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    
    void Start()
    {
        this.GetSystem<IEconomySystem>().coin.Subscribe((coin) => {
            UpateCoinInfo(coin);
        }).AddTo(this);
        this.GetSystem<IPCSystem>().Reputation.Subscribe((reputation) => {
            UpdateReputationInfo(reputation, this.GetSystem<IPCSystem>().NextLevelReputation.Value);
        }).AddTo(this);

        this.GetSystem<IPCSystem>().Level.Subscribe((level) => {
            UpdateLevelInfo(level);
        }).AddTo(this);
        
        this.GetSystem<IPCSystem>().NextLevelReputation.Subscribe((nextLevelReputation) => {
            UpdateReputationInfo(this.GetSystem<IPCSystem>().Reputation.Value, nextLevelReputation);
        }).AddTo(this);

        // 初始化
        UpateCoinInfo(this.GetSystem<IEconomySystem>().coin.Value);
        UpdateReputationInfo(this.GetSystem<IPCSystem>().Reputation.Value, this.GetSystem<IPCSystem>().NextLevelReputation.Value);
        UpdateLevelInfo(this.GetSystem<IPCSystem>().Level.Value);
    }

    private void UpateCoinInfo(int value) => coinText.text = $"<color=yellow>Q:</color>{value}";
    private void UpdateReputationInfo(int value, int nextLevelReputation) => reputationText.text = $"<color=yellow>口碑:</color>{value}/{nextLevelReputation}";
    private void UpdateLevelInfo(int value) => levelText.text = $"<color=yellow>等级:</color>{value}";

}
