using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public interface IEntityView : IShowTooltip{
    BoardEntity Entity { get; }
    void Bind(BoardEntity entity);
    void ResetScale();
    void SetPreviewState(PreviewState_EntityView previewState);
    GameObject GO();
}
public class EntityView : MonoBehaviour, IController, IEntityView, IPointerEnterHandler{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private BoardEntity entity;
    public BoardEntity Entity => entity;
    [SerializeField] private Image image;
    [SerializeField] private ParticleSystem buffedParticle;
    public UnityEvent OnPointerEnterEvent;
    public void Bind(BoardEntity entity){
        this.entity = entity;

        UpdateVisual();
        ResetScale();
    }
    private void UpdateVisual(){
        if (image == null) return;
        if (entity == null) return;
        Sprite sprite = entity.GetSprite();
        if (sprite == null){
            Debug.LogError($"实体资源不存在: {entity.name}");
            return;
        }
        image.sprite = sprite;
    }
    public void ResetScale(){
        transform.localScale = Vector3.one;
    }
    public void SetPreviewState(PreviewState_EntityView previewState){
        switch (previewState){
            case PreviewState_EntityView.Normal:
                break;
            case PreviewState_EntityView.Selected:
                break;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent.Invoke();
    }
    public List<TooltipInfo> GetTooltipInfo()
    {
        List<TooltipInfo> tooltipInfos = new List<TooltipInfo>();
        tooltipInfos.Add(new TooltipInfo(entity.name));
        return tooltipInfos;
    }
    public GameObject GO() => gameObject;
}