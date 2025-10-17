using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Message_type
    {
        string message_type_id;
        string message_type_name;

        public string Message_type_id
        {
            get { return message_type_id; }
            set { message_type_id = value; }
        }

        public string Message_type_name
        {
            get { return message_type_name; }
            set { message_type_name = value; }
        }
    }
}
