using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Delivery : Model
    {
        string delivery_id;
        string delivery_text;
        string delivery_file;
        string order_id;

        public Delivery()
        {

        }

        public string Delivery_id
        {
            get { return delivery_id; }
            set { delivery_id = value; }
        }

        public string Delivery_text
        {
            get { return delivery_text; }
            set { delivery_text = value; }
        }

        public string Delivery_file
        {
            get { return delivery_file; }
            set { delivery_file = value; }
        }

        public string Order_id
        {
            get { return order_id; }
            set { order_id = value; }
        }
    }
}
