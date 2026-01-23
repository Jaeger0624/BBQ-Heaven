using System;
using UnityEngine;
using QFramework;
using UnityEngine.UI;
using TMPro;
using cfg;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class DisplayMascotView : MonoBehaviour, IDisplayItemView<MascotUIContext>{
    private MascotUIContext _context;
    private Action<MascotUIContext> _onClick;
    [SerializeField] private Image mascotImage;
    [SerializeField] private Image mascotRank;
    [SerializeField] private TextMeshProUGUI mascotStackNumberText;
    [SerializeField] private TextMeshProUGUI mascotNameText;
    [SerializeField] private TextMeshProUGUI mascotPriceText;
    public void Bind(MascotUIContext context){
        if (context == null){
            Debug.LogError("MascotUIContext is null");
            return;
        }
        _context = context;

        if (mascotImage!=null){
            // mascotImage.sprite = Resources.Load<Sprite>("Sprites/Mascots/" + context.ConfigData.Image);
        }
        if (mascotRank!=null){
            switch (context.ConfigData.Rank){
                case Rank.普通:
                    mascotRank.color = Color.green;
                    break;
                case Rank.稀有:
                    mascotRank.color = Color.blue;
                    break;
                case Rank.史诗:
                // 史诗颜色为紫色
                    mascotRank.color = new Color(0.5f, 0f, 0.5f); // Purple
                    break;
                case Rank.传说:
                    mascotRank.color = Color.yellow;
                    break;
            }
        }
        if (mascotPriceText!=null && context.Price != -1){
            mascotPriceText.text = $"{context.Price}<color=yellow>Q</color>";
            mascotPriceText.gameObject.SetActive(true);
        }
        else{
            mascotPriceText.gameObject.SetActive(false);
        }
        if (mascotStackNumberText!=null){
            if (context.ConfigData.Stackable){
                // mascotStackNumberText.text = context.Count.ToString();
                mascotStackNumberText.text = "";
            }
            else{
                mascotStackNumberText.text = "";
            }
        }

        if (mascotNameText!=null){
            mascotNameText.text = context.ConfigData.Name;
        }
    }
    public void SetInteraction(Action<MascotUIContext> onClick)
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
            mascotImage.color = Color.red;
        }else{
            mascotImage.color = Color.white;
        }
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}

