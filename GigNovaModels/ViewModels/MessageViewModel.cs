using GigNovaModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.ViewModels
{
    public class MessageViewModel
    {
        public Message message { get; set; }
        public Order order { get; set; }
    }
}
