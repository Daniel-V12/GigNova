using GigNovaModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.ViewModels
{
    public class ManageGigsViewModel
    {
        public List<Gig> Gigs { get; set; }

        public Gig SelectedGig { get; set; }

        public List<Delivery_time> DeliveryTimes { get; set; }
    }
}
