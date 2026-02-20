using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GigNovaModels.Models;

namespace GigNovaModels.ViewModels
{
    public class CatalogViewModel
    {
        public List<Gig> Gigs { get; set; }
        public List<Category> Categories { get; set; }
        public List<Language> Languages { get; set; }
        public List<Delivery_time> Delivery_Times { get; set; }

        public int Page { get; set; } = 0;
        public int TotalPages { get; set; } = 0;
        public int GigsPerPageCount { get; set; } = 9;

        public string GigCategories { get; set; } = "";
        public List<string> GigCategoryNames { get; set; } = new List<string>();

        public double min_price { get; set; } = 0;
        public double max_price { get; set; } = 0;
        public int delivery_time_id { get; set; } = 0;
        public int language_id { get; set; } = 0;
        public double min_rating { get; set; } = 0;
    }
}
