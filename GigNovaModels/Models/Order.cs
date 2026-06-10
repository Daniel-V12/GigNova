using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Order : Model
    {

        string order_id;
        int order_status_id;
        string order_requirements;
        string order_creation_date;
        int gig_id;
        int buyer_id;
        int seller_id;
        bool is_payment;


        public Order()
        {

        }

        public string Order_id
        {
            get { return order_id; }
            set { order_id = value; }
        }

        [Required(ErrorMessage = "Order status is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid order status")]
        public int Order_status_id
        {
            get { return order_status_id; }
            set { order_status_id = value; }
        }
        [Required(ErrorMessage = "Order requirements are required")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Requirements must be between 5 and 2000 characters")]
        public string Order_requirements
        {
            get { return order_requirements; }
            set { order_requirements = value; }
        }

        public string Order_creation_date
        {
            get { return order_creation_date; }
            set { order_creation_date = value; }
        }

        public int Gig_id
        {
            get { return gig_id; }
            set { gig_id = value; }
        }

        public int Buyer_id
        {
            get { return buyer_id; }
            set { buyer_id = value; }
        }

        public int Seller_id
        {
            get { return seller_id; }
            set { seller_id = value; }
        }

        public bool Is_payment
        {
            get { return is_payment; }
            set { is_payment = value; }
        }
    }
}
