using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class OrderCreator : IModelCreator<Order>
    {
        public Order CreateModel(IDataReader dataReader)
        {
            Order order = new Order();
            order.Order_id = Convert.ToString(dataReader["order_id"]);
            order.Order_status_id = Convert.ToUInt16(dataReader["order_status_id"]);
            order.Order_creation_date = Convert.ToString(dataReader["order_creation_date"]);
            order.Seller_id = Convert.ToUInt16(dataReader["seller_id"]);
            order.Gig_id = Convert.ToUInt16(dataReader["gig_id"]);
            order.Buyer_id = Convert.ToUInt16(dataReader["buyer_id"]);
            order.Is_payment = Convert.ToBoolean(dataReader["is_payment"]);
            order.Order_requirements = Convert.ToString(dataReader["order_requirements"]);

            return order;
        }
    }
}
