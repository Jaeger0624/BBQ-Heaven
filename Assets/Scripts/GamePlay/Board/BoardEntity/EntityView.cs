using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntityView : MonoBehaviour, IController,IShowTooltip, IPointerEnterHandler{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public BoardEntity entity;
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
    private void ResetScale(){
        transform.localScale = Vector3.one;
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
}