using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Message
    {
        string message_id;
        int sender_id;
        int reciever_id;
        string message_text;
        int message_type_id;
        string message_date;
        int order_id;

        public string Message_id
        {
            get { return message_id; }
            set { message_id = value; }
        }
        public int sender_id
        {
            get { return sender_id; }
            set { sender_id = value; }
        }
        public int reciever_id
        {
            get { return reciever_id; }
            set { reciever_id = value; }
        }

        [Required(ErrorMessage = "Message text is required")]
        public string Message_text
        {
            get { return message_text;}
            set { message_text = value; }
        }

        [Required(ErrorMessage = "Message type is required")]
        public int Message_type_id
        {
            get { return message_type_id; }
            set { message_type_id = value; }
        }
        public string Message_date
        {
            get { return message_date; }
            set { message_date = value; }
        }
        public int Order_id
        {
            get { return order_id; }
            set { order_id = value; }
        }

    }
}
