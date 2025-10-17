using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Order_status
    {
        string order_status_id;
        string status_name;
        public string Order_status_id
        {
            get { return order_status_id; }
            set { order_status_id = value; }
        }
        public string Status_name
        {
            get { return status_name; }
            set { status_name = value; }
        }

    }
}
