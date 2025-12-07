using QFramework;
using TMPro;
using UniRx;
using UnityEngine;

public class CoinView : MonoBehaviour, IController
{
    [SerializeField] private TextMeshProUGUI _coinText;

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    void Start()
    {
        this.GetSystem<IEconomySystem>().coin.Subscribe(x => _coinText.text = x.ToString()).AddTo(this);
    }
}
