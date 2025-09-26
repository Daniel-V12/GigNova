using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels
{
    public class Gig
    {
        string gig_id;
        string gig_name;
        string gig_description;
        int gig_delivery_time;
        int language_id;
        string gig_date;
        string gig_photo;
        double gig_price;
        int gig_rating;
        int seller_id;

        public string Gig_id
        {
            get { return this.gig_id; }
            set { this.gig_id = value; }
        }
        [Required(ErrorMessage = " Gig Name cannot contain numbers")]
        [StringLength(20,MinimumLength = 3, ErrorMessage = " Gig Name cannot contain more than 20 lettes")]
        [FirstLetterCaps(ErrorMessage = "First letter should be capital and rest small")]
        public string Gig_name
        {
            get { return this.gig_name; }
            set { this.gig_name = value; }
        }

        public string Gig_description
        {
            get { return this.gig_description; }
            set { this.gig_description = value; }
        }
        public int Gig_delivery_time
        {
            get { return this.gig_delivery_time; }
            set { this.gig_delivery_time = value; }
        }
        public string Gig_date
        {
            get { return this.gig_date; }
            set { this.gig_date = value; }
        }

        public int Language_id
        {
            get { return this.language_id; }
            set { this.language_id = value; }
        }
        public string Gig_photo
        {
            get { return this.gig_photo; }
            set { this.gig_photo = value; }
        }

        public double Gig_price
        {
            get { return this.gig_price; }
            set { this.gig_price = value; }
        }

        public int Seller_id
        {
            get { return this.seller_id; }
            set { this.seller_id = value; }
        }






    }
}
