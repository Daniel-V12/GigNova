using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class Message_typeRepository : Repository, IRepository<Message_type>
    {
        public Message_typeRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Message_type model)
        {
            string sql = @$"Insert into Message_type (message_type_name)
            values ( @message_type_name)";
            this.dbHelperOledb.AddParameter("@message_type_name", model.Message_type_name);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Message_Types where message_type_id = @message_type_id";
            this.dbHelperOledb.AddParameter("@message_type_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public List<Message_type> GetAll()
        {
            string sql = "Select * from Message_Types";
            List<Message_type> message_types = new List<Message_type>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    message_types.Add(this.modelCreators.MessageTypeCreator.CreateModel(reader));
                }
            }
            return message_types;
        }

        public Message_type GetById(string id)
        {
            string sql = "Select * from Message_types where message_type_id = @message_type_id";
            this.dbHelperOledb.AddParameter("@message_type_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.MessageTypeCreator.CreateModel(reader);
            }
        }

        public bool Update(Message_type model)
        {
            string sql = @"Update Message_Types set 
            message_type_name = @message_type_name";
            this.dbHelperOledb.AddParameter("@message_type_name", model.Message_type_name);
            return this.dbHelperOledb.Update(sql) > 0;
        }
    }
}
