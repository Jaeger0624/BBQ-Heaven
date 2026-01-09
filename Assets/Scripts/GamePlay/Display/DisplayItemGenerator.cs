using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;


public class DisplayItemGenerator<TData, TView> where TView : MonoBehaviour, IDisplayItemView<TData>{
    private TView _prefab;
    private Transform _container;
    private List<TView> _items = new List<TView>();
    public IItemInteractStrategy<TData, TView> CurrentStrategy { get; set; }
    public DisplayItemGenerator(Transform container, TView prefab){
        _prefab = prefab;
        _container = container;
    }
    public void Generate(IEnumerable<TData> dataList, IItemInteractStrategy<TData, TView> itemIneractStrategy){
        if (dataList == null || itemIneractStrategy == null) {Debug.LogError("数据或策略为空"); return;}
        CurrentStrategy = itemIneractStrategy;
        int dataIndex = 0;
        
        // 1. 遍历数据
        foreach (var data in dataList)
        {
            TView view;

            // 2. 对象池逻辑：如果当前位置已经有View，就复用；否则实例化新的
            if (dataIndex < _items.Count)
            {
                view = _items[dataIndex];
                view.gameObject.SetActive(true);
            }
            else
            {
                view = Object.Instantiate(_prefab, _container);
                view.gameObject.SetActive(true);
                _items.Add(view);
            }

            // 3. 视图初始化 (Dumb View 逻辑)
            view.Bind(data);

            // 4. 绑定策略 (Strategy 逻辑)
            // 先清理旧的监听（防止闭包陷阱）
            view.SetInteraction(null); 
            
            // 绑定点击逻辑
            view.SetInteraction((d) => CurrentStrategy.OnClick(d, view));
            
            // 触发表现逻辑
            CurrentStrategy.OnBind(view, data);

            dataIndex++;
        }

        // 5. 隐藏多余的 View (而不是 Destroy，简单的缓存池)
        for (int i = dataIndex; i < _items.Count; i++)
        {
            _items[i].gameObject.SetActive(false);
        }
    }


    #region Helper Methods
    public List<TView> GetItems(){
        return _items.FindAll(item => item.gameObject.activeSelf);
    }
    #endregion
}