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

    }
}
