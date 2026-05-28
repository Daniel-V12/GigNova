using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class OrderFileCreator : IModelCreator<Order_file>
    {
        public Order_file CreateModel(IDataReader dataReader)
        {
            Order_file order_file = new Order_file();
            order_file.Order_file_id = Convert.ToString(dataReader["order_file_id"]);
            order_file.Order_file_path = Convert.ToString(dataReader["order_file_path"]);
            return order_file;
        }
    }
}