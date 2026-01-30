using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class BuyerCreator : IModelCreator<Buyer>
    {
        public Buyer CreateModel(IDataReader dataReader)
        {
            Buyer buyer = new Buyer();
            buyer.Buyer_display_name = Convert.ToString(dataReader["buyer_display_name"]);
            buyer.Buyer_description = Convert.ToString(dataReader["buyer_description"]);

            return buyer;
        }
    }
}
