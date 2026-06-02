using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class OrderRepository : Repository, IRepository<Order>
    {
        public OrderRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {
        }


        // ============================== Create / Update / Delete ==============================

        public bool Create(Order model)
        {
            string sql = @$"Insert into Orders (order_status_id, order_requirements, order_creation_date, gig_id, buyer_id, seller_id, is_payment)
            values ( @order_status_id , @order_requirements , @order_creation_date, @gig_id, @buyer_id, @seller_id, @is_payment)";
            this.dbHelperOledb.AddParameter("@order_status_id", model.Order_status_id);
            this.dbHelperOledb.AddParameter("@order_requirements", model.Order_requirements);
            this.dbHelperOledb.AddParameter("@order_creation_date", DateTime.Now.ToShortDateString());
            this.dbHelperOledb.AddParameter("@gig_id", model.Gig_id);
            this.dbHelperOledb.AddParameter("@buyer_id", model.Buyer_id);
            this.dbHelperOledb.AddParameter("@seller_id", model.Seller_id);
            this.dbHelperOledb.AddParameter("@is_payment", model.Is_payment);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Update(Order model)
        {
            string sql = @"Update Orders set
            order_status_id = @order_status_id,
            order_requirements = @order_requirements,
            is_payment = @is_payment
            where order_id = @order_id";
            this.dbHelperOledb.AddParameter("@order_status_id", model.Order_status_id);
            this.dbHelperOledb.AddParameter("@order_requirements", model.Order_requirements);
            this.dbHelperOledb.AddParameter("@is_payment", model.Is_payment);
            this.dbHelperOledb.AddParameter("@order_id", model.Order_id);
            return this.dbHelperOledb.Update(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Orders where order_id = @order_id";
            this.dbHelperOledb.AddParameter("@order_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }


        // ============================== Read (single + lists) ==============================

        public List<Order> GetAll()
        {
            string sql = "Select * from Orders";
            List<Order> orders = new List<Order>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    orders.Add(this.modelCreators.OrderCreator.CreateModel(reader));
                }
            }
            return orders;
        }

        public Order GetById(string id)
        {
            string sql = "Select * from Orders where order_id = @order_id";
            this.dbHelperOledb.AddParameter("@order_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.OrderCreator.CreateModel(reader);
            }
        }

        public List<Order> GetOrderByBuyerId(string buyerId)
        {
            string sql = "Select * from Orders where buyer_id = @buyer_id";
            this.dbHelperOledb.AddParameter("@buyer_id", buyerId);
            List<Order> orders = new List<Order>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    orders.Add(this.modelCreators.OrderCreator.CreateModel(reader));
                }
            }
            return orders;
        }

        public List<Order> GetOrderBySellerId(string sellerId)
        {
            string sql = "Select * from Orders where seller_id = @seller_id";
            this.dbHelperOledb.AddParameter("@seller_id", sellerId);
            List<Order> orders = new List<Order>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    orders.Add(this.modelCreators.OrderCreator.CreateModel(reader));
                }
            }
            return orders;
        }


        // ============================== Status updates ==============================

        public bool UpdateOrderStatus(string orderId, int statusId)
        {
            string sql = @"Update Orders set
            order_status_id = @order_status_id
            where order_id = @order_id";
            this.dbHelperOledb.AddParameter("@order_status_id", statusId);
            this.dbHelperOledb.AddParameter("@order_id", orderId);
            return this.dbHelperOledb.Update(sql) > 0;
        }


        // ============================== Helpers used by other flows ==============================

        // After Create, the WS needs the new order's id (so it can save the order files with it).
        // @@IDENTITY in Access returns the last auto-increment value inserted on this connection.
        public string GetLastInsertedOrderId()
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

        // Used by the admin/seller "Delete Gig" flow to refuse deletion when the gig already has orders.
        public bool HasOrdersForGig(string gigId)
        {
            string sql = "Select Count(*) as order_count from Orders where gig_id = @gig_id";
            this.dbHelperOledb.AddParameter("@gig_id", gigId);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == true)
                {
                    int count = Convert.ToInt32(reader["order_count"]);
                    return count > 0;
                }
            }
            return false;
        }
    }
}
