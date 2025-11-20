using GigNovaModels.Models;

namespace GigNovaWS
{
    public class Message_typeRepository : Repository, IRepository<Message_type>
    {
        public Message_typeRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Message_type model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            throw new NotImplementedException();
        }

        public List<Message_type> GetAll()
        {
            throw new NotImplementedException();
        }

        public Message_type GetById(string id)
        {
            throw new NotImplementedException();
        }

        public bool Update(Message_type model)
        {
            throw new NotImplementedException();
        }
    }
}
