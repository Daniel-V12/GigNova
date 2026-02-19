using GigNovaModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.ViewModels
{
    public class SellerPublicProfileViewModel
    {
        public Seller seller { get; set; }
        public Person seller_person { get; set; }
        public List<Gig> gigs { get; set; }
        public double average_rating { get; set; }
    }
}
