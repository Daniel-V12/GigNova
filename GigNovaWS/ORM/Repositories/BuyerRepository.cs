using GigNovaModels.Models;
using System;
using System.Data;

namespace GigNovaWS
{
    public class BuyerRepository : Repository, IRepository<Buyer>
    {
        public BuyerRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Buyer model)
        {
            string sql = "Insert into Buyers (buyer_description, buyer_display_name) values ( @buyer_description ,  @buyer_display_name)";
            this.dbHelperOledb.AddParameter("@buyer_description", model.Buyer_description);
            this.dbHelperOledb.AddParameter("@buyer_display_name", model.Buyer_display_name);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Buyers where buyer_id = @buyer_id";
            this.dbHelperOledb.AddParameter("buyer_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public List<Buyer> GetAll()
        {
            string sql = "Select * from Buyers";
            List<Buyer> buyers = new List<Buyer>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    buyers.Add(this.modelCreators.BuyerCreator.CreateModel(reader));
                }
            }
            return buyers;
        }

        public Buyer GetById(string id)
        {
            string sql = "Select * from Buyers where buyer_id = @buyer_id";
            this.dbHelperOledb.AddParameter("@buyer_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.BuyerCreator.CreateModel(reader);
            }
        }

        public bool Update(Buyer model)
        {
            string sql = @"Update Buyers set 
            buyer_description = @buyer_description ,
            buyer_display_name = @buyer_display_name
            where buyer_id = @buyer_id";
            this.dbHelperOledb.AddParameter("@buyer_description", model.Buyer_description);
            this.dbHelperOledb.AddParameter("@buyer_display_name", model.Buyer_display_name);
            this.dbHelperOledb.AddParameter("@buyer_id", model.Buyer_id);
            return this.dbHelperOledb.Update(sql) > 0;
        }
    }
}
