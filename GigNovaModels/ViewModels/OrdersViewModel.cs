using GigNovaModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.ViewModels
{
    public class OrdersViewModel
    {
        public List<Order> Orders { get; set; }
        public List<Buyer> Buyers { get; set; }
    }
}
