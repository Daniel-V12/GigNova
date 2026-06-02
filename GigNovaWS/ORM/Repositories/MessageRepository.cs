using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class MessageRepository : Repository, IRepository<Message>
    {
        public MessageRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {
        }


        // ============================== Create / Update / Delete ==============================

        public bool Create(Message model)
        {
            string sql = @$"Insert into Messages (sender_id, reciever_id, message_text, message_date, order_id)
            values ( @sender_id , @reciever_id , @message_text , @message_date, @order_id)";
            this.dbHelperOledb.AddParameter("@sender_id", model.Sender_id);
            this.dbHelperOledb.AddParameter("@reciever_id", model.Reciever_id);
            this.dbHelperOledb.AddParameter("@message_text", model.Message_text);
            this.dbHelperOledb.AddParameter("@message_date", DateTime.Now.ToShortDateString());
            this.dbHelperOledb.AddParameter("@order_id", model.Order_id);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        // We don't edit messages once they're sent. Update is required by IRepository<T> but unused.
        public bool Update(Message model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Messages where message_id = @message_id";
            this.dbHelperOledb.AddParameter("@message_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }


        // ============================== Read (single + lists) ==============================

        public List<Message> GetAll()
        {
            string sql = "Select * from Messages";
            List<Message> messages = new List<Message>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    messages.Add(this.modelCreators.MessageCreator.CreateModel(reader));
                }
            }
            return messages;
        }

        public Message GetById(string id)
        {
            string sql = "Select * from Messages where message_id = @message_id";
            this.dbHelperOledb.AddParameter("@message_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.MessageCreator.CreateModel(reader);
            }
        }

        // All messages a person is involved in (either as sender OR receiver), newest first.
        // Used by MessagingBox when no specific order is selected.
        public List<Message> GetByPersonId(string personId)
        {
            string sql = "Select * from Messages where sender_id = @sender_id or reciever_id = @reciever_id order by message_id desc";
            this.dbHelperOledb.AddParameter("@sender_id", personId);
            this.dbHelperOledb.AddParameter("@reciever_id", personId);
            List<Message> messages = new List<Message>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    messages.Add(this.modelCreators.MessageCreator.CreateModel(reader));
                }
            }
            return messages;
        }

        // Same as GetByPersonId but narrowed to a single order. Used by MessagingBox when scoped to one order.
        public List<Message> GetByPersonAndOrderId(string personId, string orderId)
        {
            string sql = "Select * from Messages where (sender_id = @sender_id or reciever_id = @reciever_id) and order_id = @order_id order by message_id desc";
            this.dbHelperOledb.AddParameter("@sender_id", personId);
            this.dbHelperOledb.AddParameter("@reciever_id", personId);
            this.dbHelperOledb.AddParameter("@order_id", orderId);
            List<Message> messages = new List<Message>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    messages.Add(this.modelCreators.MessageCreator.CreateModel(reader));
                }
            }
            return messages;
        }
    }
}
