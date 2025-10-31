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
    }
}
