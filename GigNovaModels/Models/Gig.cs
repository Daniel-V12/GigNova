using GigNovaModels.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
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
        bool is_publish;
        bool has_revisions;

        public string Gig_id
        {
            get { return gig_id; }
            set { gig_id = value; }
        }

        [FirstLetterCaps(ErrorMessage = "Gig title must start with a capital letter")]
        [Required(ErrorMessage = "Gig title is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Gig title must be no longer than 20 characters and no less than 2")]
        public string Gig_name
        {
            get { return gig_name; }
            set { gig_name = value; }
        }
        [Required(ErrorMessage = "Gig description is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Gig description must be no longer than 250 characters and no less than 2")]
        public string Gig_description
        {
            get { return gig_description; }
            set { gig_description = value; }
        }
        [Required(ErrorMessage = "Gig delivery time is required")]
        public int Gig_delivery_time
        {
            get { return gig_delivery_time; }
            set { gig_delivery_time = value; }
        }
        public string Gig_date
        {
            get { return gig_date; }
            set { gig_date = value; }
        }
        [Required(ErrorMessage = "Language is required")]
        public int Language_id
        {
            get { return language_id; }
            set { language_id = value; }
        }
        public string Gig_photo
        {
            get { return gig_photo; }
            set { gig_photo = value; }
        }

        [Required(ErrorMessage = "Gig price is required")]
        [IsDigits(ErrorMessage = "Must be a number only")]
        public double Gig_price
        {
            get { return gig_price; }
            set { gig_price = value; }
        }

        public int Seller_id
        {
            get { return seller_id; }
            set { seller_id = value; }
        }
        public bool Is_publish
        {
            get { return is_publish; }
            set { is_publish = value; }
        }
        public bool Has_revisions
        {
            get { return has_revisions; }
            set { has_revisions = value; }
        }
    }
}
