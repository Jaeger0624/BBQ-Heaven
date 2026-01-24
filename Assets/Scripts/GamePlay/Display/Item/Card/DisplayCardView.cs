using System;
using UnityEngine;
using QFramework;
using UnityEngine.UI;
using TMPro;
using cfg;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class DisplayCardView : MonoBehaviour, IDisplayItemView<CardUIContext>{
    private CardUIContext _context;
    private Action<CardUIContext> _onClick;
    [SerializeField] private Image cardImage;
    [SerializeField] private Image backgroundImage;
    // [SerializeField] private Image cardRank;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardCostText;
    [SerializeField] private TextMeshProUGUI cardPriceText;
    [SerializeField] private TextMeshProUGUI cardDescriptionText;
    public void Bind(CardUIContext context){
        _context = context;
        cardNameText.text = context.ConfigData.Name;

        if (cardImage!=null){
            // cardImage.sprite = Resources.Load<Sprite>("Sprites/" + context.ConfigData.Sprite);
        }
        // if (cardRank!=null){
        //     switch (context.ConfigData.Rank){
        //         case Rank.普通:
        //             cardRank.color = Color.green;
        //             break;
        //         case Rank.稀有:
        //             cardRank.color = Color.blue;
        //             break;
        //         case Rank.史诗:
        //         // 史诗颜色为紫色
        //             cardRank.color = new Color(0.5f, 0f, 0.5f); // Purple
        //             break;
        //         case Rank.传说:
        //             cardRank.color = Color.yellow;
        //             break;
        //     }
        // }

        if (cardPriceText!=null){
            if (context.Price != -1){
                cardPriceText.text = $"{context.Price}<color=yellow>Q</color>";
            }
            else{
                cardPriceText.text = "";
            }
        }
        if (cardCostText!=null){
            cardCostText.text = $"{context.ConfigData.Cost}";
        }

        if (cardDescriptionText!=null){
            cardDescriptionText.text = context.ConfigData.Description;
        }
    }
    public void SetInteraction(Action<CardUIContext> onClick)
    {
        _onClick = onClick;
    }
    public void OnClick()
    {
        _onClick?.Invoke(_context);
    }
    public void SetSelectedState(bool isSelected)
    {
        if (isSelected){
            backgroundImage.color = Color.red;
        }else{
            backgroundImage.color = Color.white;
        }
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}