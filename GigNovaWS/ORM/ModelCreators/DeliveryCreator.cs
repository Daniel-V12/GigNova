using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS.ORM.ModelCreators
{
    public class DeliveryCreator : IModelCreator<Delivery>
    {
        public Delivery CreateModel(IDataReader dataReader)
        {
            Delivery delivery = new Delivery();
            delivery.Delivery_id = Convert.ToString(dataReader["delivery_id"]);
            delivery.Delivery_text = Convert.ToString(dataReader["delivery_text"]);
            delivery.Delivery_file = Convert.ToString(dataReader["delivery_file"]);
            delivery.Order_id = Convert.ToString(dataReader["order_id"]);
            return delivery;
        }
    }
}
