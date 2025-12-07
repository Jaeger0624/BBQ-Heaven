using QFramework;
using UnityEngine;

public class ShopController : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    void OnEnable()
    {
        // this.RegisterEvent<CreateShopEvent_日间商店>(OnCreateShopEvent_日间商店);
    }
    void OnDisable()
    {
        // this.UnRegisterEvent<CreateShopEvent_日间商店>(OnCreateShopEvent_日间商店);
    }

}

