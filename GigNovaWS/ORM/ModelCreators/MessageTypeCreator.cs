using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class MessageTypeCreator : IModelCreator<Message_type>
    {
        public Message_type CreateModel(IDataReader dataReader)
        {
            Message_type message_type = new Message_type();
            message_type.Message_type_id = Convert.ToString(dataReader["message_type_id"]);
            message_type.Message_type_name = Convert.ToString(dataReader["message_type_name"]);
            return message_type;
        }
    }
}
