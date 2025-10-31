using GigNovaModels.Models;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Data;

namespace GigNovaWS
{
    public class MessageCreator : IModelCreator<Message>
    {
        public Message CreateModel(IDataReader dataReader)
        {
            Message message = new Message();
            message.Message_id = Convert.ToString(dataReader["message_id"]);
            message.Message_text = Convert.ToString(dataReader["message_text"]);
            message.Message_date = Convert.ToString(dataReader["message_date"]);
            message.Message_type_id = Convert.ToUInt16(dataReader["message_type_id"]);
            message.Order_id = Convert.ToUInt16(dataReader["order_id"]);
            message.Reciever_id = Convert.ToUInt16(dataReader["reciever_id"]);
            message.Sender_id = Convert.ToUInt16(dataReader["sender_id"]);
            return message;
        }
    }
}
