using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.ViewModels
{
    public class OrderSubmitResultViewModel
    {
        public bool IsSuccess { get; set; }
        public string OrderId { get; set; }
        public string Message { get; set; }
    }
}
