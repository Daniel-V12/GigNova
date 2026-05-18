using GigNovaModels.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Gig : Model
    {

        string gig_id;
        string gig_name;
        string gig_description;
        int delivery_time_id;
        int language_id;
        string gig_date;
        string gig_photo;
        double gig_price;
        int seller_id;
        bool is_publish;
        bool has_revisions;
        bool is_blocked;
        string category_id;
        List<string> category_ids = new List<string>();

        public Gig()
        {

        }
        public string Gig_id
        {
            get { return gig_id; }
            set { gig_id = value; }
        }

        [Required(ErrorMessage = "Gig title is required")]
        [FirstLetterCaps(ErrorMessage = "Each word in gig title must start with a capital letter")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Gig title must be between 5 and 50 characters")]
        public string Gig_name
        {
            get { return gig_name; }
            set
            {
                gig_name = value;
                ValidateProperty(value, "Gig_name");
            }
        }
        [Required(ErrorMessage = "Gig description is required")]
        [StringLength(500, MinimumLength = 20, ErrorMessage = "Gig description must be between 20 and 500 characters")]
        public string Gig_description
        {
            get { return gig_description; }
            set { gig_description = value; ValidateProperty(value, "Gig_description"); }
        }
        [Required(ErrorMessage = "Gig delivery time is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a delivery time")]
        public int Delivery_time_id
        {
            get { return delivery_time_id; }
            set { delivery_time_id = value; ValidateProperty(value, "Delivery_time_id"); }
        }
        public string Gig_date
        {
            get { return gig_date; }
            set { gig_date = value; ValidateProperty(value, "Gig_date"); }
        }
        [Required(ErrorMessage = "Language is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a language")]
        public int Language_id
        {
            get { return language_id; }
            set { language_id = value; }
        }

        public string Gig_photo
        {
            get { return gig_photo; }
            set
            {
                if (value == null)
                {
                    gig_photo = "none";
                }
                else
                {
                    gig_photo = value;
                }

                ValidateProperty(value, "Gig_photo");
            }
        }

        [Required(ErrorMessage = "Gig price is required")]
        [Range(1, 10000, ErrorMessage = "Price must be between 1 and 10000")]
        public double Gig_price
        {
            get { return gig_price; }
            set { gig_price = value; ValidateProperty(value, "Gig_price"); }
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

        public bool Is_blocked
        {
            get { return is_blocked; }
            set { is_blocked = value; }
        }

        public string Category_id
        {
            get { return category_id; }
            set { category_id = value; }
        }

        public List<string> Category_ids
        {
            get { return category_ids; }
            set
            {
                if (value == null)
                    category_ids = new List<string>();
                else
                    category_ids = value;
            }
        }
    }
}