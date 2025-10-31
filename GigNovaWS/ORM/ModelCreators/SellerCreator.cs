using GigNovaModels.Models;
using System;
using System.Data;

namespace GigNovaWS
{
    public class SellerCreator: IModelCreator<Seller>
    {
        public Seller CreateModel(IDataReader dataReader)
        { 
            Seller seller = new Seller();
            seller.Seller_id = Convert.ToString(dataReader["seller_id"]);
            seller.Seller_display_name = Convert.ToString(dataReader["seller_display_name"]);
            seller.Seller_description = Convert.ToString(dataReader["seller_description"]);
            seller.Seller_avatar = Convert.ToString(dataReader["seller_avatar"]);

            return seller;
        }
    }
}
