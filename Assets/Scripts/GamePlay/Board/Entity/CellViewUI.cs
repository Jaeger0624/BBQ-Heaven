
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellViewUI : MonoBehaviour, IPointerClickHandler, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public BoardCell cell;
    [SerializeField] private Image image;
    [SerializeField] private Image highlightImage;
    private bool isHighlighted = false;
    void Start()
    {
        
        Unhighlight();
    }
    public void Init(BoardCell cell){
        this.cell = cell;
    }

    public void Highlight(){
        // Debug.Log($"Highlight: {cell.position}");
        isHighlighted = true;
        highlightImage.gameObject.SetActive(true);
    }
    public void Unhighlight(){
        isHighlighted = false;
        highlightImage.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.GetSystem<IBoardSystem>().ClickCell(cell.position);
        // Debug.Log($"棋盘格子被点击: {cell.position}");
    }
}