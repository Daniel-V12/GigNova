using GigNovaModels.Models;

namespace GigNovaWS
{
    public class ReviewRepository : Repository, IRepository<Review>
    {
        public ReviewRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Review model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            throw new NotImplementedException();
        }

        public List<Review> GetAll()
        {
            throw new NotImplementedException();
        }

        public Review GetById(string id)
        {
            throw new NotImplementedException();
        }

        public bool Update(Review model)
        {
            throw new NotImplementedException();
        }
    }
}
