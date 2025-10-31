﻿using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class GigCreator:IModelCreator<Gig>
    {
        public Gig CreateModel(IDataReader dataReader)
        {
            Gig gig = new Gig();
            gig.Gig_name = Convert.ToString(dataReader["gig_name"]);
            gig.Gig_description = Convert.ToString(dataReader["gig_description"]);
            gig.Gig_id = Convert.ToString(dataReader["gig_id"]);
            gig.Gig_delivery_time = Convert.ToUInt16(dataReader["gig_delivery_time"]);
            gig.Language_id = Convert.ToUInt16(dataReader["language_id"]);
            gig.Gig_date = Convert.ToString(dataReader["gig_date"]);
            gig.Gig_price = Convert.ToUInt32(dataReader["gig_price"]);
            gig.Gig_price = Convert.ToUInt32(dataReader["gig_price"]);
            gig.Gig_photo = Convert.ToString(dataReader["gig_price"]);
            gig.Seller_id = Convert.ToUInt16(dataReader["seller_id"]);
            gig.Is_publish = Convert.ToBoolean(dataReader["is_publish"]);
            gig.Has_revisions = Convert.ToBoolean(dataReader["has_revisions"]);

            return gig;
        }
    }
}
