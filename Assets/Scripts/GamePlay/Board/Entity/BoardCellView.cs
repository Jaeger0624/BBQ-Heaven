using UnityEngine;

public class BoardCellView : MonoBehaviour{
    public BoardCell cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject selectedView;

    void Awake()
    {
        selectedView.SetActive(false);
    }
    public void Init(BoardCell cell){
        this.cell = cell;
    }

    public void OnSelect(){
        // 显示选中视图
        // 半透明绿色
        // spriteRenderer.color = new Color(0f, 1f, 0f, 0.5f);
        selectedView.SetActive(true);
    }
    public void OnUnselect(){
        // 隐藏选中视图
        // 半透明白色
        // spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        selectedView.SetActive(false);
    }
}