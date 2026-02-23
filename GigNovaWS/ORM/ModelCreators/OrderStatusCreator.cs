using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class OrderStatusCreator : IModelCreator<Order_status>
    {
        public Order_status CreateModel(IDataReader dataReader)
        {
            Order_status order_status = new Order_status();
            order_status.Order_status_id = Convert.ToString(dataReader["order_status_id"]);
            order_status.Status_name = Convert.ToString(dataReader["status_name"]);
            return order_status;
        }
    }
}
