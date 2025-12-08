using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ShopView_日前提升 : MonoBehaviour, IController, ICanSendEvent
{
    [SerializeField] private FoodItemGenerator foodItemGenerator;
    [SerializeField] private Button refreshButton;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private int maxChooseTime = 2;
    private int currentChooseTime = 0;
    private void OnEnable() 
    {
        this.RegisterEvent<CreateShop_日前提升>(OnCreateShop_日前提升);
        this.RegisterEvent<HideShop_日前提升>(OnHideShop_日前提升);
        foodItemGenerator.OnClickEvent += OnBuyButtonClick;
    }
    private void OnDisable() 
    {
        this.UnRegisterEvent<CreateShop_日前提升>(OnCreateShop_日前提升);
        this.UnRegisterEvent<HideShop_日前提升>(OnHideShop_日前提升);
        foodItemGenerator.OnClickEvent -= OnBuyButtonClick;
    }
    private void OnBuyButtonClick(){
        currentChooseTime++;
        if (currentChooseTime >= maxChooseTime){
            this.SendEvent(new ProcessMoveNextEvent());
        }
    }
    private void OnCreateShop_日前提升(CreateShop_日前提升 evt){
        UpdateView(evt.shopItems);
        currentChooseTime = 0;
        OnShow();
    }
    private void OnHideShop_日前提升(HideShop_日前提升 evt){
        OnHide();
    }

    private void UpdateView(List<IShopItem> shopItems){
        // 1. 重置食材生成器
        foodItemGenerator.Reset();

        // 2. 生成食材
        foodItemGenerator.GenerateItem(shopItems);
    }
    [Button("Test")]
    private void Test(){
        List<ShopTask> shopTasks = new List<ShopTask>{
            new ShopTask(ShopItemType.Food, 3, new Price_免费()),
        };
        List<IShopItem> shopItemsList = this.GetSystem<IShopSystem>()
            .BuildConcreteShopItem(shopTasks, ShopItemType.Food, new Random_纯随机());
        UpdateView(shopItemsList);
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

public class CreateShop_日前提升 : AbstractEvent{
    public List<IShopItem> shopItems;
    public CreateShop_日前提升(List<IShopItem> shopItems){
        this.shopItems = shopItems;
    }
}
public class HideShop_日前提升 : AbstractEvent{
    public HideShop_日前提升(){}
}