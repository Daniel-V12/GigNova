using GigNovaModels;
using GigNovaModels.Models;
using GigNovaModels.ViewModels;
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

        public List<Gig> GetGigByPrice(double price)
        {
            string sql = "Select * from Gigs where gig_price = @gig_price";
            this.dbHelperOledb.AddParameter("@gig_price", price.ToString());
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

        public List<Gig> GetGigByCategories(string[] categories)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT Gigs.gig_id, Gigs.gig_name, Gigs.gig_description, Gigs.gig_delivery_time, 
                          Gigs.language_id, Gigs.gig_date, Gigs.gig_photo, Gigs.gig_price, Gigs.seller_id, 
                          Gigs.is_publish, Gigs.has_revisions, [Gigs - Categories].category_id
                          FROM Gigs 
                          INNER JOIN [Gigs - Categories] ON Gigs.gig_id = [Gigs - Categories].gig_id");
            if (categories != null && categories.Length > 0)
            {
                sb.Append(" WHERE ");
                int i = 0;
                foreach (string category in categories)
                {
                    sb.Append($"category_id ={category} ");
                   
                    if (i != categories.Length - 1)
                        sb.Append(" Or ");
                    i++;
                }

            }
            List<Gig> gigs = new List<Gig>();
            using (IDataReader reader = this.dbHelperOledb.Select(sb.ToString()))
            {
                while (reader.Read())
                {
                    Gig gig = this.modelCreators.GigCreator.CreateModel(reader);
                    if(IfGigExist(gig, gigs)==false)
                        gigs.Add(gig);
                }
            }
            return gigs;

        }

        private bool IfGigExist(Gig gig, List<Gig> gigs)
        {
            foreach (Gig thisGig in gigs)
            {
                
                    if (thisGig.Gig_id == gig.Gig_id) 
                    {
                        return true;
                    }
                
            }
            return false;
        }
        public List<Gig> GetGigsByPage(int page)
        {
            int gigsperpage = 5;
            List<Gig> gigs = this.GetAll();
            return gigs.Skip(gigsperpage * (page - 1)).Take(gigsperpage).ToList();
        }


        public List<Gig> GetGigsByLanguage(string language)
        {
            string sql = @"Select * from Gigs where language_id = @language_id ";
            this.dbHelperOledb.AddParameter("@language_id", language);
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

        public List<Gig> GetGigsBySeller(string sellerId)
        {
            string sql = @"Select * from Gigs where seller_id = @seller_id ";
            this.dbHelperOledb.AddParameter("@seller_id", sellerId);
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

        public List<Gig> GetGigsBySellerByPage(string sellerId, int page)
        {
            int gigsperpage = 5;
            List<Gig> gigs = GetGigsBySeller(sellerId);
            return gigs.Skip(gigsperpage * (page - 1)).Take(gigsperpage).ToList();
        }

        public List<Gig> GetGigsByPageAndCategories(string[] strings, int page)
         {
            int gigsperpage = 5;
            List<Gig> gigs = GetGigByCategories(strings);
            return gigs.Skip(gigsperpage * (page - 1)).Take(gigsperpage).ToList();
         }


        //public List<Gig> GetGigsByRating(string rating)
        //{
        //    string sql = @"Select * from Gigs where review_rating = @review_rating ";
        //    this.dbHelperOledb.AddParameter("@review_rating", review_rating);
        //    List<Gig> gigs = new List<Gig>();
        //    using (IDataReader reader = this.dbHelperOledb.Select(sql))
        //    {
        //        while (reader.Read())
        //        {
        //            gigs.Add(this.modelCreators.GigCreator.CreateModel(reader));
        //        }
        //    }
        //    return gigs;
        //}
    }
}
