using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class CustomizeOrderModel
    {
        public int Gig_id { get; set; }
        public string? Buyer_id { get; set; }

        [Required(ErrorMessage = "Please describe what you need")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Requirements must be between 10 and 2000 characters")]
        public string requirements { get; set; } = "";

        [JsonIgnore]
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}