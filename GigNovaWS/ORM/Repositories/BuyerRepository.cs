using GigNovaModels.Models;

namespace GigNovaWS
{
    public class BuyerRepository : Repository, IRepository<Buyer>
    {
        public BuyerRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Buyer model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            throw new NotImplementedException();
        }

        public List<Buyer> GetAll()
        {
            throw new NotImplementedException();
        }

        public Buyer GetById(string id)
        {
            throw new NotImplementedException();
        }

        public bool Update(Buyer model)
        {
            throw new NotImplementedException();
        }
    }
}
