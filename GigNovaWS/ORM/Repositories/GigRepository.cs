using GigNovaModels;
using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class GigRepository : Repository, IRepository<Gig>
    {
        public bool Create(Gig model)
        {
            //string sql = @$"Insert into Gigs (gig_name, gig_description, gig_date, gig_price)
            //values ( '{model.Gig_name}' , '{model.Gig_description}', '{model.Gig_date}', '{model.Gig_price}' )";
            string sql = @$"Insert into Gigs (gig_name, gig_description, gig_date, gig_price , is_publish)
            values ( @gig_name , @gig_description , @gig_date , @gig_price, @is_publish )";
            this.dbHelperOledb.AddParameter("@gig_name", model.Gig_name);
            this.dbHelperOledb.AddParameter("@gig_description", model.Gig_description);
            this.dbHelperOledb.AddParameter("@gig_date", DateTime.Now.ToShortDateString());
            this.dbHelperOledb.AddParameter("@gig_price", model.Gig_price.ToString());
            this.dbHelperOledb.AddParameter("@is_publish", "false");
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Gigs where gig_id = @gig_id";
            this.dbHelperOledb.AddParameter("@gig_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public List<Gig> GetAll()
        {
            string sql = "Select * from Gigs";
            List<Gig> gigs = new List<Gig>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    gigs.Add(this.modelCreators.GigCreator.CreateModel(reader));
                }
            }
            return gigs;
        }

        public Gig GetById(string id)
        {
            string sql = "Select * from Gigs where gig_id = @gig_id";
            this.dbHelperOledb.AddParameter("@gig_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.GigCreator.CreateModel(reader);
            }
        }

        public bool Update(Gig model)
        {
            string sql = @"Update Gigs set 
            gig_name = @gig_name ,
            gig_description = @gig_description ,
            gig_price = @gig_price,
            gig_description = @gig_description";
            this.dbHelperOledb.AddParameter("@gig_name", model.Gig_name);
            this.dbHelperOledb.AddParameter("@gig_description", model.Gig_description);
            this.dbHelperOledb.AddParameter("@gig_price", model.Gig_price.ToString());
            this.dbHelperOledb.AddParameter("@is_publish", model.Is_publish.ToString());
            return this.dbHelperOledb.Update(sql) > 0;
        }

        //public List<Gig> GetGigByPrice(string price)
        //{
        //    string sql = "Select * from Gigs";
        //this.dbHelperOledb.AddParameter("@gig_price", model.Gig_price.ToString());
        //    List<Gig> gigs = new List<Gig>();
        //    using (IDataReader reader = this.dbHelperOledb.Select(sql))
        //    {
        //        while (reader.Read())
        //        {
        //            gigs.Add(this.modelCreators.GigCreator.CreateModel(price));
        //        }
        //    }
        //    return gigs;
        //}
    }
}
