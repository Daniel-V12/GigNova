using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class OrderFilesCreator : IModelCreator<Order_file>
    {
        public Order_file CreateModel(IDataReader dataReader)
        {
            Order_file order_file_name = new Order_file();
            order_file_name.Order_file_id = Convert.ToString(dataReader["order_file_id"]);
            order_file_name.Order_file_name = Convert.ToString(dataReader["order_file_name"]);
            return order_file_name;
        }
    }
}
