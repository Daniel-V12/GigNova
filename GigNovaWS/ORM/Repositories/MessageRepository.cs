using GigNovaModels.Models;

namespace GigNovaWS
{
    public class MessageRepository : Repository, IRepository<Message>
    {
        public bool Create(Message model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            throw new NotImplementedException();
        }

        public List<Message> GetAll()
        {
            throw new NotImplementedException();
        }

        public Message GetById(string id)
        {
            throw new NotImplementedException();
        }

        public bool Update(Message model)
        {
            throw new NotImplementedException();
        }
    }
}
