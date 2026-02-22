using GigNovaModels;
using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class OrderRepository : Repository, IRepository<Order>
    {
        public OrderRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
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

        public string GetLatestOrderIdByBuyer(string buyerId)
        {
            string sql = "Select Top 1 order_id from Orders where buyer_id = @buyer_id order by order_id desc";
            this.dbHelperOledb.AddParameter("@buyer_id", buyerId);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == true)
                {
                    return Convert.ToString(reader["order_id"]);
                }
            }
            return "";
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Orders where order_id = @order_id";
            this.dbHelperOledb.AddParameter("@order_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

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


        //public List<Order> GetOrdersByPage(int page)
        //{
        //    int ordersperpage = 5;
        //    List<Order> orders = this.GetAll();
        //    return orders.Skip(ordersperpage * (page - 1)).Take(ordersperpage).ToList();
        //}

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
        public bool UpdateOrderStatus(string orderId, int statusId)
        {
            string sql = @"Update Orders set 
            order_status_id = @order_status_id
            where order_id = @order_id";
            this.dbHelperOledb.AddParameter("@order_status_id", statusId);
            this.dbHelperOledb.AddParameter("@order_id", orderId);
            return this.dbHelperOledb.Update(sql) > 0;
        }

        public bool UpdatePaymentStatus(string orderId, bool isPayment)
        {
            string sql = @"Update Orders set 
            is_payment = @is_payment
            where order_id = @order_id";
            this.dbHelperOledb.AddParameter("@is_payment", isPayment);
            this.dbHelperOledb.AddParameter("@order_id", orderId);
            return this.dbHelperOledb.Update(sql) > 0;
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
    }
}
