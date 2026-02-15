
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
            image.sprite = SettingManager.Instance.ArtSettings.DefaultTileSprite;
        }
        else{
            tileNameText.gameObject.SetActive(true);
            Sprite sprite = SettingManager.Instance.ArtSettings.TilesSprites.GetSprite(cell.TileData.SpriteName);
            if (sprite == null){
                Debug.LogError($"CellViewUI: 获取地块图标失败: {cell.TileData.SpriteName}");
                image.sprite = SettingManager.Instance.ArtSettings.DefaultTileSprite;
            }
            else{
                image.sprite = sprite;
            }
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