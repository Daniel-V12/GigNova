using GigNovaModels.Attribute;
using System.ComponentModel.DataAnnotations;

namespace GigNovaModels.ViewModels
{
    public class BuyerProfileUpdateViewModel
    {
        public string Person_id { get; set; }

        [Required(ErrorMessage = "Display name is required")]
        [FirstLetterCaps(ErrorMessage = "Each word in display name must start with a capital letter")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Display name must be between 2 and 30 characters")]
        public string Buyer_display_name { get; set; }

        [StringLength(250, ErrorMessage = "Description must be no longer than 250 characters")]
        public string Buyer_description { get; set; }
    }
}
