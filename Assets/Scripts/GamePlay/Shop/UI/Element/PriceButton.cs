using System;
using QFramework;
using TMPro;
using UniRx;
using UnityEngine;
[RequireComponent(typeof(ButtonUI))]
// 简单的封装类，需要显示价格的交互按钮
public class PriceButton : MonoBehaviour, IController, ICanSendEvent{
    [SerializeField] private TextMeshProUGUI priceText;
    [HideInInspector]
    public ButtonUI button;
    private int price;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    void Awake()
    {
        button = GetComponent<ButtonUI>();

        if (button == null){
            Debug.LogError("PriceButton 的 button 为空");
            return;
        }
        if (priceText == null){
            Debug.LogError("PriceButton 的 priceText 为空");
            return;
        }
    }
    public void Bind(int price, Action onClick){
        this.price = price;
        button.OnClick.RemoveAllListeners();
        button.OnClick.AddListener(() => {
            if (this.GetSystem<IEconomySystem>().coin.Value < price){
                Debug.Log("金币不足，无法购买");
                this.SendEvent(new MoneyDontEnoughEvent(price));
                return;
            }

            this.GetSystem<IEconomySystem>().CostCoin(price);
            onClick?.Invoke();
        });
        UpdateText(price);
        
    }
    void Start()
    {
        this.GetSystem<IEconomySystem>().coin.Subscribe(x => {
            UpdateState(price);
        }).AddTo(this);
    }
    private void UpdateText(int price){
        if (price == 0){
            priceText.text = "获取";
            return;
        }
        priceText.text = $"{price}<color=yellow>Q</color>";
    }

    private void UpdateState(int currentCoin){

        // 必须要金币足够，才能点击
        button.SetInteractable(currentCoin >= price);
    }
}