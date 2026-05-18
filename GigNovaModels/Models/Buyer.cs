using GigNovaModels.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Buyer : Person
    {
        string buyer_description;
        string buyer_display_name;

        [StringLength(250, ErrorMessage = "Description must be no longer than 250 characters")]
        public string Buyer_description
        {
            get { return buyer_description; }
            set { buyer_description = value; }
        }
        [Required(ErrorMessage = "Display name is required")]
        [FirstLetterCaps(ErrorMessage = "Each word in display name must start with a capital letter")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Display name must be between 2 and 30 characters")]
        public string Buyer_display_name
        {
            get { return buyer_display_name; }
            set { buyer_display_name = value; }
        }
    }
}