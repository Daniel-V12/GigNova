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


        // ============================== Create / Update / Delete ==============================

        public bool Create(Gig model)
        {
            string sql = @$"Insert into Gigs (gig_name, gig_description, language_id, gig_date, gig_photo, gig_price, seller_id, is_publish, delivery_time_id, is_blocked)
            values ( @gig_name , @gig_description , @language_id, @gig_date , @gig_photo, @gig_price, @seller_id , @is_publish, @delivery_time_id, @is_blocked )";
            this.dbHelperOledb.AddParameter("@gig_name", model.Gig_name);
            this.dbHelperOledb.AddParameter("@gig_description", model.Gig_description);
            this.dbHelperOledb.AddParameter("@language_id", model.Language_id);
            this.dbHelperOledb.AddParameter("@gig_date", DateTime.Now.ToShortDateString());
            this.dbHelperOledb.AddParameter("@gig_photo", model.Gig_photo);
            this.dbHelperOledb.AddParameter("@gig_price", model.Gig_price);
            this.dbHelperOledb.AddParameter("@seller_id", model.Seller_id);
            this.dbHelperOledb.AddParameter("@is_publish", false);
            this.dbHelperOledb.AddParameter("@delivery_time_id", model.Delivery_time_id);
            this.dbHelperOledb.AddParameter("@is_blocked", false);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Update(Gig model)
        {
            string sql = @"Update Gigs set
            gig_name = @gig_name,
            gig_description = @gig_description,
            gig_price = @gig_price,
            is_publish = @is_publish
            where gig_id = @gig_id";
            this.dbHelperOledb.AddParameter("@gig_name", model.Gig_name);
            this.dbHelperOledb.AddParameter("@gig_description", model.Gig_description);
            this.dbHelperOledb.AddParameter("@gig_price", model.Gig_price);
            this.dbHelperOledb.AddParameter("@is_publish", model.Is_publish);
            this.dbHelperOledb.AddParameter("@gig_id", model.Gig_id);
            return this.dbHelperOledb.Update(sql) > 0;
        }

        // Update used by the seller edit form (more fields than the generic Update).
        public bool UpdateBySeller(Gig model)
        {
            string sql = @"Update Gigs set
            gig_name = @gig_name,
            gig_description = @gig_description,
            gig_price = @gig_price,
            gig_photo = @gig_photo,
            language_id = @language_id,
            delivery_time_id = @delivery_time_id
            where gig_id = @gig_id and seller_id = @seller_id";
            this.dbHelperOledb.AddParameter("@gig_name", model.Gig_name);
            this.dbHelperOledb.AddParameter("@gig_description", model.Gig_description);
            this.dbHelperOledb.AddParameter("@gig_price", model.Gig_price);
            this.dbHelperOledb.AddParameter("@gig_photo", model.Gig_photo);
            this.dbHelperOledb.AddParameter("@language_id", model.Language_id);
            this.dbHelperOledb.AddParameter("@delivery_time_id", model.Delivery_time_id);
            this.dbHelperOledb.AddParameter("@gig_id", model.Gig_id);
            this.dbHelperOledb.AddParameter("@seller_id", model.Seller_id);
            return this.dbHelperOledb.Update(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Gigs where gig_id = @gig_id";
            this.dbHelperOledb.AddParameter("@gig_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        // Delete only succeeds if the gig belongs to the given seller.
        public bool DeleteBySeller(string gigId, string sellerId)
        {
            string sql = @"Delete from Gigs where gig_id = @gig_id and seller_id = @seller_id";
            this.dbHelperOledb.AddParameter("@gig_id", gigId);
            this.dbHelperOledb.AddParameter("@seller_id", sellerId);
            return this.dbHelperOledb.Delete(sql) > 0;
        }


        // ============================== Read (single + lists) ==============================

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
                if (reader.Read() == false)
                {
                    return null;
                }
                return this.modelCreators.GigCreator.CreateModel(reader);
            }
        }

        public List<Gig> GetGigsBySeller(string sellerId)
        {
            string sql = @"Select * from Gigs where seller_id = @seller_id";
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

        // Pagination for the seller's "Manage Gigs" page. 5 gigs per page.
        public List<Gig> GetGigsBySellerByPage(string sellerId, int page)
        {
            int gigsperpage = 5;
            List<Gig> gigs = GetGigsBySeller(sellerId);
            return gigs.Skip(gigsperpage * (page - 1)).Take(gigsperpage).ToList();
        }


        // ============================== Categories (filter + join + dedup) ==============================

        // Returns gigs that have ANY of the given category ids attached.
        // The SQL is built dynamically because the WHERE clause has one " ? " per category id, joined by OR.
        // A gig that's linked to multiple of the requested categories shows up in the result more than once,
        // so we de-duplicate using IfGigExist below.
        public List<Gig> GetGigByCategories(string[] categories)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT Gigs.gig_id, Gigs.gig_name, Gigs.gig_description, Gigs.delivery_time_id,
                          Gigs.language_id, Gigs.gig_date, Gigs.gig_photo, Gigs.gig_price, Gigs.seller_id,
                          Gigs.is_publish, Gigs.is_blocked
                          FROM Gigs
                          INNER JOIN [Gigs - Categories] ON Gigs.gig_id = [Gigs - Categories].gig_id");

            if (categories != null && categories.Length > 0)
            {
                sb.Append(" WHERE ");
                int i = 0;
                foreach (string category in categories)
                {
                    sb.Append("[Gigs - Categories].[category_id] = ? ");
                    this.dbHelperOledb.AddParameter("@category_id_" + i, category);

                    if (i != categories.Length - 1)
                    {
                        sb.Append(" Or ");
                    }
                    i++;
                }
            }

            List<Gig> gigs = new List<Gig>();
            using (IDataReader reader = this.dbHelperOledb.Select(sb.ToString()))
            {
                while (reader.Read())
                {
                    Gig gig = this.modelCreators.GigCreator.CreateModel(reader);
                    if (IfGigExist(gig, gigs) == false)
                    {
                        gigs.Add(gig);
                    }
                }
            }
            return gigs;
        }

        // Helper for GetGigByCategories above - returns true if a gig with the same Gig_id is already in the list.
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

        // Returns all categories linked to a single gig (used to show the chips on a gig card).
        public List<Category> GetCategoriesByGigId(string gigId)
        {
            string sql = @"SELECT Categories.category_id, Categories.category_name, Categories.is_blocked
                           FROM Categories INNER JOIN [Gigs - Categories]
                           ON Categories.category_id = [Gigs - Categories].category_id
                           WHERE [Gigs - Categories].gig_id = @gig_id";
            this.dbHelperOledb.AddParameter("@gig_id", gigId);
            List<Category> categories = new List<Category>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    categories.Add(this.modelCreators.CategoryCreator.CreateModel(reader));
                }
            }
            return categories;
        }


        // ============================== Publish / Block / Photo ==============================

        // Set the gig's is_publish flag. Only succeeds if the gig belongs to the given seller.
        public bool SetPublishStatus(string gigId, string sellerId, bool isPublish)
        {
            string sql = @"Update Gigs set
            is_publish = @is_publish
            where gig_id = @gig_id and seller_id = @seller_id";
            this.dbHelperOledb.AddParameter("@is_publish", isPublish);
            this.dbHelperOledb.AddParameter("@gig_id", gigId);
            this.dbHelperOledb.AddParameter("@seller_id", sellerId);
            return this.dbHelperOledb.Update(sql) > 0;
        }

        public bool Block(string id)
        {
            string sql = @"Update Gigs set
            is_blocked = @is_blocked
            where gig_id = @gig_id";
            this.dbHelperOledb.AddParameter("@is_blocked", true);
            this.dbHelperOledb.AddParameter("@gig_id", id);
            return this.dbHelperOledb.Update(sql) > 0;
        }

        public bool Unblock(string id)
        {
            string sql = @"Update Gigs set
            is_blocked = @is_blocked
            where gig_id = @gig_id";
            this.dbHelperOledb.AddParameter("@is_blocked", false);
            this.dbHelperOledb.AddParameter("@gig_id", id);
            return this.dbHelperOledb.Update(sql) > 0;
        }

        public List<Gig> GetBlocked()
        {
            string sql = "Select * from Gigs where is_blocked = True";
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

        public bool UpdateGigPhoto(string FileName, string gig_id)
        {
            string sql = "UPDATE Gigs set gig_photo = @gig_photo where gig_id = @gig_id";
            this.dbHelperOledb.AddParameter("@gig_photo", FileName);
            this.dbHelperOledb.AddParameter("@gig_id", gig_id);
            return this.dbHelperOledb.Update(sql) > 0;
        }


        // ============================== Category linking table ([Gigs - Categories]) ==============================

        public bool AddGigCategory(string gigId, string categoryId)
        {
            string sql = "Insert into [Gigs - Categories] (gig_id, category_id) values (@gig_id, @category_id)";
            this.dbHelperOledb.AddParameter("@gig_id", gigId);
            this.dbHelperOledb.AddParameter("@category_id", categoryId);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool DeleteGigCategories(string gigId)
        {
            string sql = "Delete from [Gigs - Categories] where gig_id = @gig_id";
            this.dbHelperOledb.AddParameter("@gig_id", gigId);
            return this.dbHelperOledb.Delete(sql) >= 0;
        }
    }
}
