using GigNovaModels.Models;

namespace GigNovaWS
{
    public class Order_filesRepository : Repository, IRepository<Order_file>
    {
        public Order_filesRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Order_file model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            throw new NotImplementedException();
        }

        public List<Order_file> GetAll()
        {
            throw new NotImplementedException();
        }

        public Order_file GetById(string id)
        {
            throw new NotImplementedException();
        }

        public bool Update(Order_file model)
        {
            throw new NotImplementedException();
        }
    }
}
