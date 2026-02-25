using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class SellerRepository : Repository, IRepository<Seller>
    {
        public SellerRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Seller model)
        {
            string sql = "Insert into Sellers (seller_id, seller_description, seller_display_name, seller_avatar, is_linked) values (@seller_id, @seller_description, @seller_display_name, @seller_avatar, @is_linked)";
            this.dbHelperOledb.AddParameter("@seller_id", model.Seller_id);
            this.dbHelperOledb.AddParameter("@seller_description", model.Seller_description);
            this.dbHelperOledb.AddParameter("@seller_display_name", model.Seller_display_name);
            this.dbHelperOledb.AddParameter("@seller_avatar", model.Seller_avatar);
            this.dbHelperOledb.AddParameter("@is_linked", model.Seller_is_linked);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Sellers where seller_id = @seller_id";
            this.dbHelperOledb.AddParameter("seller_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public List<Seller> GetAll()
        {
            string sql = "Select * from Sellers";
            List<Seller> sellers = new List<Seller>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    sellers.Add(this.modelCreators.SellerCreator.CreateModel(reader));
                }
            }
            return sellers;
        }

        public Seller GetById(string id)
        {
            string sql = "Select * from Sellers where seller_id = @seller_id";
            this.dbHelperOledb.AddParameter("@seller_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == false)
                {
                    return null;
                }
                return this.modelCreators.SellerCreator.CreateModel(reader);
            }
        }

        public bool Update(Seller model)
        {
            string sql = @"Update Sellers set 
            seller_description = @seller_description,
            seller_display_name = @seller_display_name,
            seller_avatar = @seller_avatar,
            is_linked = @is_linked
            where seller_id = @seller_id";
            this.dbHelperOledb.AddParameter("@seller_description", model.Seller_description);
            this.dbHelperOledb.AddParameter("@seller_display_name", model.Seller_display_name);
            this.dbHelperOledb.AddParameter("@seller_avatar", model.Seller_avatar);
            this.dbHelperOledb.AddParameter("@is_linked", model.Seller_is_linked);
            this.dbHelperOledb.AddParameter("@seller_id", model.Seller_id);
            return this.dbHelperOledb.Update(sql) > 0;
        }
    }
}
