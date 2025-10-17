using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Buyer
    {
        string buyer_id;
        string buyer_description;
        string buyer_display_name;
        public string Buyer_id
        {
            get { return buyer_id; }
            set { buyer_id = value; }
        }
        [Required(ErrorMessage = "Description is required")]
        public string Buyer_description
        {
            get { return buyer_description; }
            set { buyer_description = value; }
        }
        [FirstLetterCaps(ErrorMessage = "Display name must start with a capital letter")]
        [Required(ErrorMessage = "Display name is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Display name must be no longer than 20 characters and no less than 2")]
        public string Buyer_display_name
        {
            get { return buyer_display_name; }
            set { buyer_display_name = value; }
        }
    }
}
