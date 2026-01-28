using System.Collections.Generic;
using QFramework;
using UnityEngine;

public abstract class ShopInitContext{
    public ShopType shopType;
    public string shopName;
    public string shopDescription;
    /// <summary>
    /// 初始化商店商品
    /// </summary>
    /// <param name="shopPanel">商店面板</param>
    public abstract void InitShopItems(ShopPanel shopPanel);

    public static ShopInitContext Get(ShopType shopType){
        switch (shopType){
            case ShopType.普通:
                return new ShopInitContext_普通商店();
            case ShopType.批发:
                return new ShopInitContext_批发商店();
            default:
                Debug.LogError("不支持的商店类型：" + shopType);
                return null;
        }
    }
    public static ShopInitContext GetRandom(Rng rng){
        List<ShopInitContext> shopInitContexts = new List<ShopInitContext>
        {
            new ShopInitContext_普通商店(),
            new ShopInitContext_批发商店()
        };
        return rng.PickOne(shopInitContexts);
    }
}