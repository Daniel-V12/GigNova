using GigNovaModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.ViewModels
{
    public class SelectedOrderViewModel
    {
        public Order Order { get; set; }
        public Buyer Buyer { get; set; }
    }
}
