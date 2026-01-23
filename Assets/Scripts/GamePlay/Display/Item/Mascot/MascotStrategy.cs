using UnityEngine;
using QFramework;

public class BuyMascotStrategy : ItemInteractStrategyBase<MascotUIContext, DisplayMascotView>{
    public override void OnBind(DisplayMascotView itemView, MascotUIContext data){
    }
    public override void OnClick(MascotUIContext data, DisplayMascotView itemView){
        if (data == null) {Debug.LogError("BuyMascotStrategy 的 data 为空"); return;}


        if (data.RuntimeMascot == null) {Debug.LogError("BuyMascotStrategy 的 data.RuntimeMascot 为空"); return;}

        // 若价格不够
        if (this.GetSystem<IEconomySystem>().coin.Value < data.Price){
            Debug.Log("金币不足，无法购买");
            return;
        }
        // 1. 获得吉祥物到仓库
        this.GetSystem<IMascotSystem>().AddMascot(data.RuntimeMascot.ID);

        // 2. 扣除金币
        this.GetSystem<IEconomySystem>().CostCoin(data.Price);

        // 3. 隐藏自身
        itemView.gameObject.SetActive(false);
    }
}