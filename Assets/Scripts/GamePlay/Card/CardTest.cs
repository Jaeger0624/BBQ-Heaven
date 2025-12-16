using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;

public class CardTest : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    private void Start() {
        for (int i = 0; i < 10; i++) {
            this.GetSystem<ICardSystem>().AddCardToRepository("1");
        }

        this.GetSystem<ICardSystem>().DrawCard(3);
    }

    [Button]
    private void Refresh() {
        // 1. 先弃掉所有手牌
        this.GetSystem<ICardSystem>().RefreshHandCards();

        // 2. 重新抽取3张
        this.GetSystem<ICardSystem>().DrawCard(3);
    }

    void OnGUI()
    {
        CardPile cardPile = this.GetSystem<ICardSystem>().CardPile;
        if (cardPile == null) return;
        GUILayout.Label($"手牌数量: {cardPile.handPile.Count}");
        GUILayout.Label($"抽牌堆数量: {cardPile.drawPile.Count}");
        GUILayout.Label($"弃牌堆数量: {cardPile.discardPile.Count}");
    }
}