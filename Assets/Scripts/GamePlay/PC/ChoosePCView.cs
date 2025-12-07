using cfg;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePCView : MonoBehaviour{
    public string pcID => pcData.ID;
    private PCData pcData;
    private ChoosePCPanel choosePCPanel;
    [SerializeField] private Button choosePCButton;
    [SerializeField] private TextMeshProUGUI nameText;

    void Awake()
    {
        choosePCButton.onClick.AddListener(OnChoosePCButtonClick);
    }

    public void Init(PCData pcData, ChoosePCPanel choosePCPanel){
        this.pcData = pcData;
        this.choosePCPanel = choosePCPanel;
        UpdateVisual();
    }
    private void UpdateVisual(){
        nameText.text = pcData.Name;
    }

    private void OnChoosePCButtonClick()
    {
        // Debug.Log("【ChoosePCView】选择玩家角色");
        choosePCPanel.OnChoosePC(pcData);
    }

    public void SetButtonInteractable(bool interactable){
        choosePCButton.interactable = interactable;
    }
}