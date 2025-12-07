using System.Net;
using DG.Tweening;
using UnityEngine;

public class StickPanel : MonoBehaviour
{
    [SerializeField]
    public Vector2 _OriginColliderSize;    
    [SerializeField]
    public Vector2 _hoveredColliderSize;    

    [SerializeField]
    private BoxCollider2D _boxCollider2D;
    [SerializeField]
    private Transform _BBQContainer;
    private bool _isHovered = false;
    private void Update() {
        // OnHovered();
    }

    private void OnHovered(){
        StickPanel stickPanel = Utility.RaycastHitComponent<StickPanel>() ?? null;
        if (stickPanel != null)
        {
            OnEnter();
        }
        else
        {
            OnExit();
        }
    }

    private void OnEnter(){
        if(_isHovered) return;
        _isHovered = true;
        // _boxCollider2D.size = _hoveredColliderSize;


        // 不受TimeScale影响
        _BBQContainer.DOLocalMoveY(1, 0.4f).SetEase(Ease.OutSine).SetUpdate(true);
    }

    private void OnExit(){
        if(!_isHovered) return;
        _isHovered = false;
        // _boxCollider2D.size = _OriginColliderSize;

        _BBQContainer.DOLocalMoveY(-2, 0.4f).SetEase(Ease.OutSine).SetUpdate(true);
    }
}
