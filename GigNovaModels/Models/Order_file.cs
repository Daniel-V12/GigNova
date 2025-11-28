using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Order_file :Model
    {
        string order_file_id;
        string order_file_name;
        string order_id;

        public string Order_file_id
        {
            get { return order_file_id; }
            set { order_file_id = value; }
        }

        public string Order_file_name
        {
            get { return order_file_name; }
            set { order_file_name = value; }
        }
        public string Order_id
        {
            get { return order_id; }
            set { order_id = value; }
        }
    }
}
