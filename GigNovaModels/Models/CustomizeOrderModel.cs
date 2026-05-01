using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class CustomizeOrderModel
    {
        public int Gig_id {  get; set; }
        public string? Buyer_id { get; set; }
        public string requirements { get; set; } = "";
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
