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
        this.RegisterEvent<ClearCustomerRecordViewsEvent>(OnClearCustomerRecordViewsEvent).UnRegisterWhenGameObjectDestroyed(this);
    }

    private void OnClearCustomerRecordViewsEvent(ClearCustomerRecordViewsEvent _){
        foreach (var view in customerRecordViews){
            if (view != null && view.gameObject != null){
                Destroy(view.gameObject);
            }
        }
        customerRecordViews.Clear();
    }
    private void OnCreateMetaCustomerEvent(CreateMetaCustomerEvent e) => CreateRecordView(e.record);

    private CustomerRecordView CreateRecordView(CustomerRecord customerRecord){
        CustomerRecordView view = Instantiate(recordViewPrefab, recordContainer).GetComponent<CustomerRecordView>();
        view.Bind(customerRecord);
        customerRecordViews.Add(view);
        return view;
    }
}