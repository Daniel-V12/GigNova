using GigNovaModels.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Review:Model
    {
        string review_id;
        int review_rating;
        string review_comment;
        string review_creation_date;
        int buyer_id;
        int seller_id;
        int gig_id;

        public Review()
        {

        }
        public string Review_id
        {
            get { return review_id; }
            set { review_id = value; }
        }
        //[Required(ErrorMessage = "Review rating is required")]
        //[IsDigits(ErrorMessage = "Must be a number only")]
        public int Review_rating
        {
            get { return review_rating; }
            set { review_rating = value; }
        }
        //[Required(ErrorMessage = "Review comment is required")]
        public string Review_comment
        {
            get { return review_comment; }
            set { review_comment = value; }
        }
        public string Review_creation_date
        {
            get { return review_creation_date; }
            set { review_creation_date = value; }
        }
        public int Buyer_id
        {
            get { return buyer_id; }
            set { buyer_id = value; }
        }
        public int Seller_id
        {
            get { return seller_id; }
            set { seller_id = value; }
        }
        public int Gig_id
        {
            get { return gig_id; }
            set { gig_id = value; }
        }
    }
}
