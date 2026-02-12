using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using Sirenix.Serialization;
namespace cfg{


    public partial class TagCGA{
        public TagCGA(TagCGA tagCGA){
            this.GADescription = tagCGA.GADescription;
            this.CDDescription = tagCGA.CDDescription;
            this.Type = tagCGA.Type;
            this.Cga = new CGA(tagCGA.Cga);
        }
    }

    public partial class CustomerCGA{
        public CustomerCGA(CustomerCGA customerCGA){
            this.Cga = new CGA(customerCGA.Cga);
            this.Type = customerCGA.Type;
        }
    }
}