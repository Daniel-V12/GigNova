using GigNovaModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.ViewModels
{
    public class SellerProfileViewModel
    {
        public Seller seller { get; set; }
        public Person seller_person { get; set; }
    }
}
