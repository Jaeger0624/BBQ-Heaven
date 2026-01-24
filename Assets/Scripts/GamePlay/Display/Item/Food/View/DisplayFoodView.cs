using System;
using UnityEngine;
using QFramework;
using UnityEngine.UI;
using TMPro;
using cfg;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class DisplayFoodView : MonoBehaviour, IDisplayItemView<FoodUIContext>, IShowTooltip{
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
    [SerializeField] private TextMeshProUGUI foodPriceText;
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
        if (foodPriceText!=null){
            if (context.Price != -1){
                foodPriceText.text = $"{context.Price}<color=yellow>Q</color>";
            }
            else{
                foodPriceText.text = "";
            }
        }

        if (enhancementParent!=null && enhancementViewPrefab!=null){
            ClearEnhancementViews();
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


    #region 强化槽
    private void GenerateEnhancementViews(){
        if (_context.RuntimeFood == null){
            Debug.LogError("RuntimeFood is null");
            return;
        }
        // ClearEnhancementViews();
        List<Image> enhancementViews = new List<Image>();

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
        // 强制刷新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(enhancementParent as RectTransform);

        int amount = _context.RuntimeFood.Enhancements.Count;
        // Debug.Log($"有{enhancementViews.Count}个强化槽，但需要绑定{amount}个强化槽");
        // 绑定强化槽
        for (int i = 0; i < amount; i++){
            if (i >= amount){
                Debug.LogError($"强化槽数量不足，需要 {amount} 个，只有 {enhancementViews.Count} 个");
                break;
            }
            enhancementViews[i].sprite = SettingManager.Instance.ArtSettings.EnhancementSprites.GetSprite($"食材强化图标_{_context.RuntimeFood.Enhancements[i].SpriteName}");   
        }

        for (int i = 0; i < enhancementViews.Count; i++){
            enhancementViews[i].enabled = true;
        }
    }

    private void ClearEnhancementViews(){
        if (enhancementParent == null || enhancementViewPrefab == null) return;
        foreach (Transform child in enhancementParent){
            Destroy(child.gameObject);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(enhancementParent as RectTransform);
    }

    public List<TooltipInfo> GetTooltipInfo()
    {
        List<TooltipInfo> tooltipInfos = new List<TooltipInfo>();
        string description = $"<size=48>{_context.ConfigData.Name}</size>\n<size=36>{_context.ConfigData.EffectDescription}</size>";
        TooltipInfo tooltipInfo = new TooltipInfo(description);
        tooltipInfos.Add(tooltipInfo);

        // 添加强化信息
        foreach (var enhancement in _context.RuntimeFood.Enhancements){
            TooltipInfo enhancementTooltipInfo = new TooltipInfo($"<size=50>{enhancement.Name}</size>\n<size=36>{enhancement.Description}</size>");
            tooltipInfos.Add(enhancementTooltipInfo);
        }
        return tooltipInfos;
    }
    #endregion
}