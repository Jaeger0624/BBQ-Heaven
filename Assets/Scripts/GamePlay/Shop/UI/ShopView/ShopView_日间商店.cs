using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

/// <summary>
/// 日间商店视图
/// </summary>
public class ShopView_日间商店 : MonoBehaviour, IController
{
    [LabelText("吉祥物商品生成器")]
    public ShopItemGenerator shopItemGenerator_吉祥物;


    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    void Awake()
    {
    }

    void OnEnable()
    {
        this.RegisterEvent<CreateShopEvent_日间商店>(OnCreateShopEvent_日间商店);
        this.RegisterEvent<HideShopEvent_日间商店>(OnHideShopEvent_日间商店);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<CreateShopEvent_日间商店>(OnCreateShopEvent_日间商店);
        this.UnRegisterEvent<HideShopEvent_日间商店>(OnHideShopEvent_日间商店);
    }
    private void OnCreateShopEvent_日间商店(CreateShopEvent_日间商店 evt){
        UpdateView(evt.shopItems);
        OnShow();
    }
    private void OnHideShopEvent_日间商店(HideShopEvent_日间商店 evt){
        OnHide();
    }

    public void UpdateView(Dictionary<ShopItemType, List<IShopItem>> shopItems){
        shopItemGenerator_吉祥物.Reset();
        shopItems.ForEach(shopItemType => {
            switch (shopItemType.Key){
                case ShopItemType.Food:
                    break;
                case ShopItemType.Mascot:
                    shopItemGenerator_吉祥物.GenerateItem(shopItemType.Value);
                    break;
                case ShopItemType.Recipe:
                    break;
                default:
                    break;
            }
        });
    }

    [Button("Test")]
    private void Test(){
        List<ShopTask> shopTasks = new List<ShopTask>(){
            new ShopTask(ShopItemType.Food, 3, new PriceStrategy_原值()),
            new ShopTask(ShopItemType.Mascot, 1, new PriceStrategy_原值()),
        };
        Dictionary<ShopItemType, List<IShopItem>> shopItems = this.GetSystem<IShopSystem>().BuildShopItems(shopTasks, new Random_纯随机());
        UpdateView(shopItems);
    }
    [Button("OnShow")]
    public void OnShow(){
        IAnimTask animTask = new TweenAnimTask(this.transform.DOLocalMoveY(0f, 1.5f).SetEase(Ease.OutSine).SetEase(Ease.OutBack,0.5f,0.3f).SetUpdate(true));
        this.GetSystem<IAnimationSystem>().Append(animTask);
        this.GetSystem<IAnimationSystem>().Play();
    }


    [Button("OnHide")]
    public void OnHide(){
        IAnimTask animTask = new TweenAnimTask(this.transform.DOLocalMoveY(Screen.height, 1.5f).SetEase(Ease.OutSine).SetEase(Ease.OutBack,0.5f,0.3f).SetUpdate(true));
        this.GetSystem<IAnimationSystem>().Append(animTask);
        this.GetSystem<IAnimationSystem>().Play();
    }
}

#region 事件
public class CreateShopEvent_日间商店{
    public Dictionary<ShopItemType, List<IShopItem>> shopItems;
    public CreateShopEvent_日间商店(Dictionary<ShopItemType, List<IShopItem>> shopItems){
        this.shopItems = shopItems;
    }
}
public class HideShopEvent_日间商店{
    public HideShopEvent_日间商店(){}
}
#endregion