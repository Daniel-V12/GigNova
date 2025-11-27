using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class MessageRepository : Repository, IRepository<Message>
    {

        public MessageRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Message model)
        {
            string sql = @$"Insert into Messages (message_text, message_type_id, message_date)
            values ( @message_text , @message_type_id , @message_date)";
            this.dbHelperOledb.AddParameter("@message_text", model.Message_text);
            this.dbHelperOledb.AddParameter("@message_type_id", model.Message_type_id);
            this.dbHelperOledb.AddParameter("@message_date", DateTime.Now.ToShortDateString());
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Messages where message_id = @message_id";
            this.dbHelperOledb.AddParameter("@message_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public List<Message> GetAll()
        {
            string sql = "Select * from Messages";
            List<Message> gigs = new List<Message>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    gigs.Add(this.modelCreators.MessageCreator.CreateModel(reader));
                }
            }
            return gigs;
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

        public bool Update(Message model)
        {
            throw new NotImplementedException();
        }
    }
}
