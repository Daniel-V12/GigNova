using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class GigCreator : IModelCreator<Gig>
    {
        public Gig CreateModel(IDataReader dataReader)
        {
            Gig gig = new Gig();
            gig.Gig_name = Convert.ToString(dataReader["gig_name"]);
            gig.Gig_description = Convert.ToString(dataReader["gig_description"]);
            gig.Gig_id = Convert.ToString(dataReader["gig_id"]);
            gig.Language_id = Convert.ToUInt16(dataReader["language_id"]);
            gig.Gig_date = Convert.ToString(dataReader["gig_date"]);
            gig.Gig_price = Convert.ToUInt32(dataReader["gig_price"]);
            gig.Gig_photo = Convert.ToString(dataReader["gig_photo"]);
            gig.Delivery_time_id = Convert.ToUInt16(dataReader["delivery_time_id"]);
            gig.Seller_id = Convert.ToUInt16(dataReader["seller_id"]);
            gig.Is_publish = Convert.ToBoolean(dataReader["is_publish"]);
            gig.Has_revisions = Convert.ToBoolean(dataReader["has_revisions"]);
            gig.Category_id = TryGetCategoryId(dataReader);

            return gig;
        }

        private string TryGetCategoryId(IDataReader dataReader)
        {
            try
            {
                object value = dataReader["category_id"];
                if (value == null)
                {
                    return "";
                } 
                return Convert.ToString(value);
            }
            catch
            {
                return "";
            }
        }
    }
}
