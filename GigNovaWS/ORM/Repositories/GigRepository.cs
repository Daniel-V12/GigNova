using GigNovaModels;
using GigNovaModels.Models;
using System.Data;
using System.Text;

namespace GigNovaWS
{
    public class GigRepository : Repository, IRepository<Gig>
    {
        public GigRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }

        public bool Create(Gig model)
        {
            //string sql = @$"Insert into Gigs (gig_name, gig_description, gig_date, gig_price)
            //values ( '{model.Gig_name}' , '{model.Gig_description}', '{model.Gig_date}', '{model.Gig_price}' )";
            string sql = @$"Insert into Gigs (gig_name, gig_description, gig_date, gig_price , is_publish, has_revisions)
            values ( @gig_name , @gig_description , @gig_date , @gig_price, @is_publish, @has_revisions )";
            this.dbHelperOledb.AddParameter("@gig_name", model.Gig_name);
            this.dbHelperOledb.AddParameter("@gig_description", model.Gig_description);
            this.dbHelperOledb.AddParameter("@gig_date", DateTime.Now.ToShortDateString());
            this.dbHelperOledb.AddParameter("@gig_price", model.Gig_price);
            this.dbHelperOledb.AddParameter("@is_publish", false);
            this.dbHelperOledb.AddParameter("@has_revisions", false);
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
            this.dbHelperOledb.AddParameter("@gig_price", model.Gig_price);
            this.dbHelperOledb.AddParameter("@is_publish", model.Is_publish);
            this.dbHelperOledb.AddParameter("@is_publish", model.Has_revisions);
            return this.dbHelperOledb.Update(sql) > 0;
        }

        //public List<Gig> GetGigByPrice(string price)
        //{
        //    string sql = "Select * from Gigs";
        //    this.dbHelperOledb.AddParameter("@gig_price", model.Gig_price.ToString());
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

        public List<Gig> GetGigByCategories(List<string>  categories)
        {
           
          
            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT Gigs.gig_id, Gigs.gig_name, Gigs.gig_description, Gigs.gig_delivery_time, 
                          Gigs.language_id, Gigs.gig_date, Gigs.gig_photo, Gigs.gig_price, Gigs.seller_id, 
                          Gigs.is_publish, Gigs.has_revisions, [Gigs - Categories].category_id\r\nFROM Gigs 
                          INNER JOIN [Gigs - Categories] ON Gigs.gig_id = [Gigs - Categories].gig_id");
            if (categories != null && categories.Count > 0)
            {
                sb.Append(" WHERE ");
                int i = 0;
                foreach( string category in categories)
                {
                    sb.Append($"category_id ={category}");
                    i++;
                    if (i != categories.Count - 1)
                        sb.Append(" Or ");
                }

            }
            List<Gig> gigs = new List<Gig>();
            using (IDataReader reader = this.dbHelperOledb.Select(sb.ToString())) 
            {
                while (reader.Read())
                {
                    gigs.Add(this.modelCreators.GigCreator.CreateModel(reader));
                }
            }
            return gigs;

        }
        public List<Gig> GetGigsByPage(int page)
        {
            int gigsperpage = 5;
            List<Gig> gigs = this.GetAll();
            return gigs.Skip(gigsperpage * (page - 1)).Take(gigsperpage).ToList();
        }
    }
}
