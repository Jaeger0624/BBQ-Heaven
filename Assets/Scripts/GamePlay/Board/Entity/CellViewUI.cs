
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellViewUI : MonoBehaviour, IPointerClickHandler, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public BoardCell cell;
    [SerializeField] private Image image;
    [SerializeField] private Image highlightImage;
    [SerializeField] private TextMeshProUGUI tileNameText;
    void Start()
    {
           
        Unhighlight();
    }
    public void Init(BoardCell cell){
        this.cell = cell;
        UpdateVisual();
    }
    public void UpdateVisual(){
        
        tileNameText.text = cell.TileData.Name;
        if (cell.TileData.ID == "Default"){
            tileNameText.gameObject.SetActive(false);
        }
        else{
            tileNameText.gameObject.SetActive(true);
        }
    }
    public void Highlight(){
        // Debug.Log($"Highlight: {cell.position}");
        highlightImage.gameObject.SetActive(true);
    }
    public void Unhighlight(){
        highlightImage.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.GetSystem<IBoardSystem>().ClickCell(cell.position);
        // Debug.Log($"棋盘格子被点击: {cell.position}");
    }
}