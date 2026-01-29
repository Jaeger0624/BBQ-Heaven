using System.Collections.Generic;
using QFramework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UniRx;
using UnityEngine;

public class ShopController : SerializedMonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private Transform shopPanelParent;
    [OdinSerialize]
    public Dictionary<ShopType, GameObject> shopPanelPrefabs = new Dictionary<ShopType, GameObject>();
    private ShopPanel currentShopPanel;
    void OnEnable()
    {
        this.RegisterEvent<CreateShopPanelEvent>(OnCreateShopPanelEvent);
        this.RegisterEvent<CloseShopPanelEvent>(OnCloseShopPanelEvent);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<CreateShopPanelEvent>(OnCreateShopPanelEvent);
        this.UnRegisterEvent<CloseShopPanelEvent>(OnCloseShopPanelEvent);
    }
    private void OnCreateShopPanelEvent(CreateShopPanelEvent evt){
        Debug.Log("【ShopController】创建商店面板: " + evt.shopInitContext.shopName);
        CreateShopPanel(evt.shopInitContext);
    }
    private void CreateShopPanel(ShopInitContext shopInitContext){
        ShopPanel shopPanel = null;

        if (shopPanelPrefabs.ContainsKey(shopInitContext.shopType)){
            shopPanel = Instantiate(shopPanelPrefabs[shopInitContext.shopType], shopPanelParent).GetComponent<ShopPanel>();
            shopPanel.uiPanel.ForceHide();
        }
        else{
            Debug.LogError("【ShopController】不支持的商店类型: " + shopInitContext.shopType);
            return;
        }


        // 过2帧后初始化
        Observable.NextFrame().Subscribe(_ => {
            shopPanel.Init(shopInitContext);
            shopPanel.uiPanel.Show();
        }).AddTo(this);

        // 设定当前商店面板
        currentShopPanel = shopPanel;
    }

    private void OnCloseShopPanelEvent(CloseShopPanelEvent evt){
        if (currentShopPanel != null){
            GameObject shopPanelGameObject = currentShopPanel.gameObject;
            currentShopPanel.uiPanel.Hide().Subscribe(_ => {
                Destroy(shopPanelGameObject);
            }).AddTo(this);
            currentShopPanel = null;
        }
        else{
            Debug.LogError("【ShopController】当前商店面板为空");
        }
    }

    [Button]
    public void Test_CreateShopPanel(ShopType shopType){
        CreateShopPanel(ShopInitContext.Get(shopType));
    }
}

