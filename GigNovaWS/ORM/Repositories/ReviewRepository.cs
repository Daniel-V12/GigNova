using GigNovaModels;
using GigNovaModels.Models;
using System.Data;
using System.Reflection;

namespace GigNovaWS
{
    public class ReviewRepository : Repository, IRepository<Review>
    {
        public ReviewRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Review model)
        {
            string sql = @$"Insert into Reviews (review_rating, review_comment,review_creation_date)
            values ( @review_rating , @review_comment , @review_creation_date)";
            this.dbHelperOledb.AddParameter("@review_rating", model.Review_rating);
            this.dbHelperOledb.AddParameter("@review_comment", model.Review_comment);
            this.dbHelperOledb.AddParameter("@Review_creation_date", DateTime.Now.ToShortDateString());
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Reviews where review_id = @review_id";
            this.dbHelperOledb.AddParameter("@review_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public List<Review> GetAll()
        {
            string sql = "Select * from Reviews";
            List<Review> reviews = new List<Review>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    reviews.Add(this.modelCreators.ReviewCreator.CreateModel(reader));
                }
            }
            return reviews;
        }

        public Review GetById(string id)
        {
            string sql = "Select * from Reviews where review_id = @review_id";
            this.dbHelperOledb.AddParameter("@review_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.ReviewCreator.CreateModel(reader);
            }
        }

        public bool Update(Review model)
        {
            string sql = @"Update Reviews set 
            review_rating = @review_rating ,
            review_comment = @review_comment ";
            this.dbHelperOledb.AddParameter("@review_rating", model.Review_rating);
            this.dbHelperOledb.AddParameter("@review_comment", model.Review_comment);
            return this.dbHelperOledb.Update(sql) > 0;
        }

        public double GetReviewBySeller(string  sellerId)
        {
            string sql = @"SELECT Avg(Reviews.review_rating) AS [Avg]
                           FROM Reviews
                           GROUP BY Reviews.seller_id
                           HAVING (((Reviews.seller_id)=@SellerId));";
              this.dbHelperOledb.AddParameter("@SellerId", sellerId);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return Convert.ToDouble(reader["Avg"]);
            }

        }
    }
}
