using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class ReviewCreator : IModelCreator<Review>
    {
        public Review CreateModel(IDataReader dataReader)
        {
            Review review = new Review();
            review.Review_id = Convert.ToString(dataReader["review_id"]);
            review.Review_comment = Convert.ToString(dataReader["review_comment"]);
            review.Review_rating = Convert.ToUInt16(dataReader["review_rating"]);
            review.Review_creation_date = Convert.ToString(dataReader["review_creation_date"]);
            review.Gig_id = Convert.ToUInt16(dataReader["gig_id"]);
            review.Buyer_id = Convert.ToUInt16(dataReader["buyer_id"]);
            review.Seller_id = Convert.ToUInt16(dataReader["seller_id"]);

            return review;
        }
    }
}
