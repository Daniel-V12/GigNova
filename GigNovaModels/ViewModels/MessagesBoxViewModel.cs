using GigNovaModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.ViewModels
{
    public class MessagesBoxViewModel
    {
        public List<Message> Messages { get; set; }
        public List<Person> Senders { get; set; }
    }
}
