using QFramework;
using UnityEngine;

public class ShopPanel : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    [SerializeField] private FoodDisplayContainer foodDisplayContainer;
    // [SerializeField] private ButtonUI 


    // public void Init()

}


// 不同商店刷新价格不一样也很关键
public enum ShopType{
    普通,
    批发,
    折扣,
    收藏,
    黑市,
    升级,
    盲盒
}

public class ShopInitContext{

}