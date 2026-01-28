using System;
using System.Collections.Generic;
using System.Linq;

public class SelectionRequest_选择商店 : AbstractSelectionRequest{
    public override string Title => "选择一个商店";
    public override int Amount { get; set; } = 3;
    public List<ShopType> shopTypes;
    public override Action<SelectionBuildContext> OnSelect { get; set; } = null;
    public SelectionRequest_选择商店(List<ShopType> shopTypes, Action<SelectionBuildContext> onSelect = null){
        this.shopTypes = shopTypes;
        this.OnSelect = onSelect;
        this.OnSelect = OnSelection;
    }
    public override SelectRequest Create(){
        List<SelectionBuildContext> contexts = new List<SelectionBuildContext>();
        foreach (var shopType in shopTypes)
        {
            SelectionBuildContext context = new SelectionBuildContext(SelectionType.自定义, shopType.ToString());
            ShopInitContext shopInitContext = ShopInitContext.Get(shopType);
            context.Name = shopInitContext.shopName;
            context.Description = shopInitContext.shopDescription;
            context.EffectDescription = "";
            contexts.Add(context);
        }
        return new SelectRequest(contexts, Title, OnSelect);
    }
    private void OnSelection(SelectionBuildContext shopContext){
        ShopType shopTypeEnum = Enum.Parse<ShopType>(shopContext.Id);
        ShopInitContext shopInitContext = ShopInitContext.Get(shopTypeEnum);
        this.GetArchitecture().SendEvent(new CreateShopPanelEvent(shopInitContext));
    }
}