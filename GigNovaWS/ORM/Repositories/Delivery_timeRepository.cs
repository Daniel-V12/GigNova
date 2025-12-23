using GigNovaModels.Models;
using System.Data;
using System.Reflection.PortableExecutable;

namespace GigNovaWS
{
    public class Delivery_timeRepository : Repository, IRepository<Delivery_time>
    {
        public Delivery_timeRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }

        public bool Create(Delivery_time model)
        {
            string sql = @$"Insert into Delivery_Times (delivery_time_name)
            values ( @delivery_time_name)";
            this.dbHelperOledb.AddParameter("@delivery_time_name", model.Delivery_time_name);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Delivery_Times where delivery_time_id = @delivery_time_id";
            this.dbHelperOledb.AddParameter("@delivery_time_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public List<Delivery_time> GetAll()
        {
            string sql = "Select * from Delivery_Times";
            List<Delivery_time> delivery_times = new List<Delivery_time>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    delivery_times.Add(this.modelCreators.DeliveryTimeCreator.CreateModel(reader));
                }
            }
            return delivery_times;
        }

        public Delivery_time GetById(string id)
        {
            string sql = "Select * from Delivery_Times where delivery_time_id = @delivery_time_id";
            this.dbHelperOledb.AddParameter("@delivery_time_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.DeliveryTimeCreator.CreateModel(reader);
            }
        }

        public bool Update(Delivery_time model)
        {
            string sql = @"Update Delivery_Times set 
            delivery_time_name = @delivery_time_name";
            this.dbHelperOledb.AddParameter("@delivery_time_name", model.Delivery_time_name);
            return this.dbHelperOledb.Update(sql) > 0;
        }
    }
}
