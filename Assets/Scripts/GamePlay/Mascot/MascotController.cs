using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class MascotController : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private GameObject mascotViewPrefab;
    [SerializeField] private Transform mascotContainer;
    private Dictionary<string, MascotView> mascotViews = new Dictionary<string, MascotView>();
    void OnEnable()
    {
        this.RegisterEvent<MascotAddedEvent>(OnMascotAddedEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<MascotRemovedEvent>(OnMascotRemovedEvent).UnRegisterWhenDisabled(this);
    }
    private void OnMascotAddedEvent(MascotAddedEvent e){
        // 如果已存在，则更新视觉
        if (mascotViews.TryGetValue(e.mascot.ID, out MascotView mascotView))
        {
            mascotView.UpdateVisual();
            return;
        }
        // 否则创建新视图
        else{
            CreateMascotView(e.mascot);
        }
    }
    private void OnMascotRemovedEvent(MascotRemovedEvent e) => RemoveMascotView(e.mascot);
    private MascotView CreateMascotView(Mascot mascot)
    {
        MascotView mascotView = Instantiate(mascotViewPrefab, mascotContainer, false).GetComponent<MascotView>();
        mascotView.Init(mascot);
        mascotViews.Add(mascot.data.ID, mascotView);
        return mascotView;
    }
    private void RemoveMascotView(Mascot mascot)
    {
        if (mascotViews.TryGetValue(mascot.data.ID, out MascotView mascotView))
        {
            Destroy(mascotView.gameObject);
            mascotViews.Remove(mascot.data.ID);
        }
        else
        {
            Debug.LogWarning($"吉祥物 {mascot.data.ID} 不存在,但尝试移除");
        }
    }
}
