using QFramework;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

public class ShopController : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private Transform shopPanelParent;
    [SerializeField] private GameObject shopPanel_普通商店_Prefab;
    [SerializeField] private GameObject shopPanel_批发商店_Prefab;
    void OnEnable()
    {

    }
    void OnDisable()
    {
    }
    private void CreateShopPanel(ShopInitContext shopInitContext){
        ShopPanel shopPanel = null;

        switch (shopInitContext.shopType){
            case ShopType.普通:
                shopPanel = Instantiate(shopPanel_普通商店_Prefab, shopPanelParent).GetComponent<ShopPanel>();

                break;
            case ShopType.批发:
                shopPanel = Instantiate(shopPanel_批发商店_Prefab, shopPanelParent).GetComponent<ShopPanel>();
                break;
        }
        shopPanel.uiPanel.ForceHide();


        // 过2帧后初始化
        Observable.NextFrame().Subscribe(_ => {
            shopPanel.Init(shopInitContext);
            shopPanel.uiPanel.Show();
        }).AddTo(this);
    }
    [Button]
    public void Test_标准商店(){
        CreateShopPanel(new ShopInitContext(){
            shopType = ShopType.普通,
                shopName = "普通商店",
                shopDescription = "普通商店",
            });
    }
    [Button]
    public void Test_批发商店(){
        CreateShopPanel(new ShopInitContext(){
            shopType = ShopType.批发,
            shopName = "批发商店",
            shopDescription = "批发商店",
        });
    }
}

