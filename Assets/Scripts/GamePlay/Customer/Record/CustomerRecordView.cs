using System.Linq;
using TMPro;
using UnityEngine;

public class CustomerRecordView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI timeText;
    private CustomerRecord customerRecord;
    public void Bind(CustomerRecord customerRecord){
        this.customerRecord = customerRecord;
        UpdateVisual();
    }

    private void UpdateVisual(){
        nameText.text = customerRecord.metaCustomer.name;
        descriptionText.text = string.Join(", ", customerRecord.metaCustomer.customerTags.Select(x => x.name));
        timeText.text = $"{customerRecord.visitCount}次";
    }
}
