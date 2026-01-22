using System;
using UnityEngine;
using QFramework;
using UnityEngine.UI;
using TMPro;
using cfg;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class DisplayFoodView : MonoBehaviour, IDisplayItemView<FoodUIContext>{
    private FoodUIContext _context;
    private Action<FoodUIContext> _onClick;
    [SerializeField] private Image foodImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image foodRank;
    [SerializeField] private Transform foodSizeParent;
    [SerializeField] private TextMeshProUGUI foodNameText;
    [SerializeField] private TextMeshProUGUI foodCostText;
    [SerializeField] private TextMeshProUGUI foodRarityText;
    [SerializeField] private TextMeshProUGUI foodTasteText;
    [LabelText("强化父物体")]
    [SerializeField] private Transform enhancementParent;
    [LabelText("强化预制体")]
    [SerializeField] private GameObject enhancementViewPrefab;
    public void Bind(FoodUIContext context){
        _context = context;
        foodNameText.text = context.ConfigData.Name;

        if (foodImage!=null){
            foodImage.sprite = Resources.Load<Sprite>("Sprites/" + context.ConfigData.Sprite);
        }
        if (foodNameText!=null){
            foodNameText.text = context.ConfigData.Name;
        }
        if (backgroundImage!=null){
            // backgroundImage.sprite = Resources.Load<Sprite>("Sprites/" + context.ConfigData.Sprite);
        }
        if (foodRarityText!=null){
            foodRarityText.text = context.ConfigData.Rarity.ToString();
        }
        if (foodTasteText!=null){
            foodTasteText.text = context.ConfigData.Taste.ToString();
        }
        if (foodRank!=null){
            switch (context.ConfigData.Rank){
                case Rank.普通:
                    foodRank.color = Color.green;
                    break;
                case Rank.稀有:
                    foodRank.color = Color.blue;
                    break;
                case Rank.史诗:
                // 史诗颜色为紫色
                    foodRank.color = new Color(0.5f, 0f, 0.5f); // Purple
                    break;
                case Rank.传说:
                    foodRank.color = Color.yellow;
                    break;
            }
        }

        if (enhancementParent!=null && enhancementViewPrefab!=null){
            GenerateEnhancementViews();
        }
    }
    public void SetInteraction(Action<FoodUIContext> onClick)
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
            foodImage.color = Color.red;
        }else{
            foodImage.color = Color.white;
        }
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }

    private void GenerateEnhancementViews(){
        if (_context.RuntimeFood == null){
            Debug.LogError("RuntimeFood is null");
            return;
        }
        List<Image> enhancementViews = new List<Image>();
        // 先创建等于最大槽数的强化槽
        for (int i = 0; i < _context.RuntimeFood.MaxSlots; i++){
            GameObject enhancementView = Instantiate(enhancementViewPrefab, enhancementParent);
            enhancementView.SetActive(true);
            Image enhancementImage = enhancementView.GetComponent<Image>();
            enhancementImage.sprite = SettingManager.Instance.ArtSettings.EnhancementSprites.GetSprite($"食材强化图标_无");
            if (enhancementImage == null){
                Debug.LogError("EnhancementImage is null");
                continue;
            }
            enhancementViews.Add(enhancementImage);
        }
        Debug.Log($"创建了 {enhancementViews.Count} 个强化槽");
        // 强制刷新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(enhancementParent as RectTransform);

        // 绑定强化槽
        for (int i = 0; i < _context.RuntimeFood.Enhancements.Count; i++){
            enhancementViews[i].sprite = SettingManager.Instance.ArtSettings.EnhancementSprites.GetSprite($"食材强化图标_{_context.RuntimeFood.Enhancements[i].SpriteName}");
        }
    }
}