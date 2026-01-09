using QFramework;
using UnityEngine;

public interface IItemInteractStrategy<TData, TView> : IController where TView : MonoBehaviour, IDisplayItemView<TData>{
    // 定制显示
    void OnBind(TView itemView, TData data);
    void OnClick(TData data, TView itemView);  // 定制点击
}

public abstract class ItemInteractStrategyBase<TData, TView> : IItemInteractStrategy<TData, TView> where TView : MonoBehaviour, IDisplayItemView<TData>
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract void OnClick(TData data, TView itemView);
    public abstract void OnBind(TView itemView, TData data);
}
