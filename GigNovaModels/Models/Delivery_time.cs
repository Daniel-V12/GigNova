using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Delivery_time : Model
    {
        string delivery_time_id;
        string delivery_time_name;

        public Delivery_time()
        {

        }

        // d231231dawdsa
        public string Delivery_time_id
        {
            get { return delivery_time_id; }
            set { delivery_time_id = value; }
        }
        
        public string Delivery_time_name
        {
            get { return delivery_time_name; }
            set { delivery_time_name = value; }
        }
    }
}
