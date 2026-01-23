using System.Collections.Generic;
using UnityEngine;

namespace cfg{
    public partial class GA_增加顾客等待值 : GameAction
    {
        public GA_增加顾客等待值(DynamicValue value, GetCustomerInfo info)
        {
            this.Value = value;
            this.Info = info;
        }
        public override GameAction Clone()
        {
            return new GA_增加顾客等待值(Value, Info);
        }

        public override void Execute(object sender, List<object> param)
        {
            List<Customer> customers = Info.GetCustomers(sender, param);
            if (customers == null || customers.Count == 0){
                Debug.LogError("【GA_增加顾客等待值】没有顾客");
                return;
            }
            customers.ForEach(customer => {
                customer.PatienceNow.Value += Value.GetValue(sender, param);
            });
        }

        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
    }
}
