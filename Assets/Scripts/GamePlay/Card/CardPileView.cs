using QFramework;
using TMPro;
using UnityEngine;

public class CardPileView : MonoBehaviour, IController
{
    public TextMeshProUGUI cardAmountText;
    public CardPileType cardPileType;
    void Awake()
    {
        cardAmountText = GetComponentInChildren<TextMeshProUGUI>();
    }
    void OnEnable()
    {
        this.RegisterEvent<PileUpdateEvent>(OnPileUpdateEvent).UnRegisterWhenDisabled(this);
    }
    private void OnPileUpdateEvent(PileUpdateEvent evt){
        UpdateVisual();
    }
    public void UpdateVisual(){
        if (cardPileType == CardPileType.抽牌堆){
            cardAmountText.text = this.GetSystem<ICardSystem>().CardPile.drawPile.Count.ToString();
        }else if (cardPileType == CardPileType.弃牌堆){
            cardAmountText.text = this.GetSystem<ICardSystem>().CardPile.discardPile.Count.ToString();
        }
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}
public enum CardPileType{
    抽牌堆,
    弃牌堆,
}