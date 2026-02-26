using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class DeliveryRepository : Repository, IRepository<Delivery>
    {
        public DeliveryRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }

        public bool Create(Delivery model)
        {
            string sql = @"Insert into Deliveries (delivery_text, delivery_file, order_id) values (@delivery_text, @delivery_file, @order_id)";
            this.dbHelperOledb.AddParameter("@delivery_text", model.Delivery_text);
            this.dbHelperOledb.AddParameter("@delivery_file", model.Delivery_file);
            this.dbHelperOledb.AddParameter("@order_id", model.Order_id);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public string GetLastInsertedDeliveryId()
        {
            string sql = "Select @@IDENTITY as new_id";
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == true)
                {
                    return Convert.ToString(reader["new_id"]);
                }
            }
            return "";
        }

        public bool UpdateFileById(string deliveryId, string deliveryFile)
        {
            string sql = @"Update Deliveries set delivery_file = @delivery_file where delivery_id = @delivery_id";
            this.dbHelperOledb.AddParameter("@delivery_file", deliveryFile);
            this.dbHelperOledb.AddParameter("@delivery_id", deliveryId);
            return this.dbHelperOledb.Update(sql) > 0;
        }

        public List<Delivery> GetAllByOrderId(string orderId)
        {
            string sql = "Select * from Deliveries where order_id = @order_id order by delivery_id desc";
            this.dbHelperOledb.AddParameter("@order_id", orderId);
            List<Delivery> deliveries = new List<Delivery>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    deliveries.Add(this.modelCreators.DeliveryCreator.CreateModel(reader));
                }
            }
            return deliveries;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Deliveries where delivery_id = @delivery_id";
            this.dbHelperOledb.AddParameter("@delivery_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public List<Delivery> GetAll()
        {
            string sql = "Select * from Deliveries";
            List<Delivery> deliveries = new List<Delivery>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    deliveries.Add(this.modelCreators.DeliveryCreator.CreateModel(reader));
                }
            }
            return deliveries;
        }

        public Delivery GetById(string id)
        {
            string sql = "Select * from Deliveries where delivery_id = @delivery_id";
            this.dbHelperOledb.AddParameter("@delivery_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == false)
                {
                    return null;
                }
                return this.modelCreators.DeliveryCreator.CreateModel(reader);
            }
        }

        public bool Update(Delivery model)
        {
            string sql = @"Update Deliveries set delivery_text = @delivery_text, delivery_file = @delivery_file, order_id = @order_id where delivery_id = @delivery_id";
            this.dbHelperOledb.AddParameter("@delivery_text", model.Delivery_text);
            this.dbHelperOledb.AddParameter("@delivery_file", model.Delivery_file);
            this.dbHelperOledb.AddParameter("@order_id", model.Order_id);
            this.dbHelperOledb.AddParameter("@delivery_id", model.Delivery_id);
            return this.dbHelperOledb.Update(sql) > 0;
        }
    }
}
