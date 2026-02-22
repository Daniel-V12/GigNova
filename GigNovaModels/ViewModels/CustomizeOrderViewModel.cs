using GigNovaModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.ViewModels
{
    public class CustomizeOrderViewModel
    {
        public Order order { get; set; }
        public Order_file order_file { get; set; }
        public List<Order_file> order_files { get; set; }
        public Gig gig { get; set; }
        public Seller seller { get; set; }
        public Person seller_person { get; set; }
        public Delivery_time delivery_time { get; set; }
    }
}
