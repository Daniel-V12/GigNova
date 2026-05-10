using System.Collections.Generic;
using GigNovaModels.Models;

namespace GigNovaModels.ViewModels
{
    public class BlockedListViewModel
    {
        public List<Gig> BlockedGigs { get; set; } = new List<Gig>();
        public List<Category> BlockedCategories { get; set; } = new List<Category>();
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; } = 0;
        public int ItemsPerPageCount { get; set; } = 10;
        public string Search { get; set; } = "";
    }
}