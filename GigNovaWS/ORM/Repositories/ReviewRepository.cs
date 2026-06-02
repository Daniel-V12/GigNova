using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class ReviewRepository : Repository, IRepository<Review>
    {
        public ReviewRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {
        }


        // ============================== Create / Update / Delete ==============================

        public bool Create(Review model)
        {
            string sql = @$"Insert into Reviews (review_rating, review_comment, review_creation_date, buyer_id, seller_id, gig_id)
            values ( @review_rating , @review_comment , @review_creation_date, @buyer_id, @seller_id, @gig_id)";
            this.dbHelperOledb.AddParameter("@review_rating", model.Review_rating);
            this.dbHelperOledb.AddParameter("@review_comment", model.Review_comment);
            this.dbHelperOledb.AddParameter("@Review_creation_date", DateTime.Now.ToShortDateString());
            this.dbHelperOledb.AddParameter("@buyer_id", model.Buyer_id);
            this.dbHelperOledb.AddParameter("@seller_id", model.Seller_id);
            this.dbHelperOledb.AddParameter("@gig_id", model.Gig_id);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        // Reviews aren't edited - they're written once. Update is required by IRepository<T> but unused.
        // (The previous body had no WHERE clause which would have updated every row.)
        public bool Update(Review model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Reviews where review_id = @review_id";
            this.dbHelperOledb.AddParameter("@review_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }


        // ============================== Read (single + lists) ==============================

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

        // All reviews left on a specific gig.
        public List<Review> GetReviewsByGigId(string gigId)
        {
            string sql = "Select * from Reviews where gig_id = @gig_id";
            this.dbHelperOledb.AddParameter("@gig_id", gigId);
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


        // ============================== Average rating queries ==============================
        // SQL Avg() returns NULL when no rows match (no reviews yet). DBNull.Value is how OLEDB
        // surfaces SQL NULL to .NET. If we don't check for it, Convert.ToDouble(DBNull.Value)
        // throws. Both methods return 0 in that "no reviews" case.

        // Average review rating across all of one seller's gigs.
        public double GetReviewBySeller(string sellerId)
        {
            string sql = @"SELECT Avg(Reviews.review_rating) AS [Avg]
                           FROM Reviews
                           WHERE Reviews.seller_id = @SellerId";
            this.dbHelperOledb.AddParameter("@SellerId", sellerId);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == true && reader["Avg"] != DBNull.Value)
                {
                    return Convert.ToDouble(reader["Avg"]);
                }
                return 0;
            }
        }

        // Average review rating for a single gig.
        public double GetAverageRatingByGigId(string gigId)
        {
            string sql = @"SELECT Avg(Reviews.review_rating) AS [Avg]
                           FROM Reviews
                           WHERE Reviews.gig_id = @gig_id;";
            this.dbHelperOledb.AddParameter("@gig_id", gigId);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == true && reader["Avg"] != DBNull.Value)
                {
                    return Convert.ToDouble(reader["Avg"]);
                }
                return 0;
            }
        }
    }
}
