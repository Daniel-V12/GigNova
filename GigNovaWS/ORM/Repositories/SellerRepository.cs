using GigNovaModels.Models;

namespace GigNovaWS
{
    public class SellerRepository : Repository, IRepository<Seller>
    {
        public SellerRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Seller model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            throw new NotImplementedException();
        }

        public List<Seller> GetAll()
        {
            throw new NotImplementedException();
        }

        public Seller GetById(string id)
        {
            throw new NotImplementedException();
        }

        public bool Update(Seller model)
        {
            throw new NotImplementedException();
        }
    }
}
