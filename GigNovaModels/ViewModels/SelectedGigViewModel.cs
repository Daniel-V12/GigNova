using GigNovaModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.ViewModels
{
    public class SelectedGigViewModel
    {
        public Gig gig{ get; set; }
        public Seller seller { get; set; }
    }
}
