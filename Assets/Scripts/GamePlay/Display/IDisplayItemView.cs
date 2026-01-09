using System;

public interface IDisplayItemView<TData>{
    void Bind(TData data);
    void SetInteraction(Action<TData> onClick);
    void SetSelectedState(bool isSelected); // 是否选中
}


