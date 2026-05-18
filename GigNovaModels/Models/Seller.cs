using GigNovaModels.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Seller : Model
    {

        string seller_id;
        string seller_description;
        string seller_display_name;
        string seller_avatar;
        bool seller_is_linked;

        public Seller()
        {

        }
        public string Seller_id
        {
            get { return seller_id; }
            set { seller_id = value; }
        }
        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters")]
        public string Seller_description
        {
            get { return seller_description; }
            set { seller_description = value; }
        }
        [Required(ErrorMessage = "Display name is required")]
        [FirstLetterCaps(ErrorMessage = "Each word in display name must start with a capital letter")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Display name must be between 2 and 30 characters")]
        public string Seller_display_name
        {
            get { return seller_display_name; }
            set { seller_display_name = value; }
        }
        public string Seller_avatar
        {
            get { return seller_avatar; }
            set { seller_avatar = value; } //ValidateProperty(value, "seller_avatar");
        }

        public bool Seller_is_linked
        {
            get { return seller_is_linked; }
            set { seller_is_linked = value; }
        }

    }
}