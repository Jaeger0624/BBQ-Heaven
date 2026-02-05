using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class CustomerRecordController : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private GameObject recordViewPrefab;
    [SerializeField] private Transform recordContainer;
    private List<CustomerRecordView> customerRecordViews = new List<CustomerRecordView>();
    void Start()
    {
        this.RegisterEvent<CreateMetaCustomerEvent>(OnCreateMetaCustomerEvent).UnRegisterWhenGameObjectDestroyed(this);
    }
    private void OnCreateMetaCustomerEvent(CreateMetaCustomerEvent e) => CreateRecordView(e.record);

    private CustomerRecordView CreateRecordView(CustomerRecord customerRecord){
        CustomerRecordView view = Instantiate(recordViewPrefab, recordContainer).GetComponent<CustomerRecordView>();
        view.Bind(customerRecord);
        customerRecordViews.Add(view);
        return view;
    }
}