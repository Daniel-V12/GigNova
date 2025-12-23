using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS.ORM.ModelCreators
{
    public class DeliveryTimeCreator : IModelCreator<Delivery_time>
    {
        public Delivery_time CreateModel(IDataReader dataReader)
        {
            Delivery_time delivery_time = new Delivery_time();
            delivery_time.Delivery_time_id = Convert.ToString(dataReader["delivery_time_id"]);
            delivery_time.Delivery_time_name = Convert.ToString(dataReader["delivery_time_name"]);
            return delivery_time;
        }
    }
}
