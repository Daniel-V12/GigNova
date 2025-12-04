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
            string sql = @$"Insert into Orders (order_requirements, order_creation_date)
            values ( @order_name , @order_creation_date)";
            this.dbHelperOledb.AddParameter("@order_requirements", model.Order_requirements);
            this.dbHelperOledb.AddParameter("@order_creation_date", DateTime.Now.ToShortDateString());
            return this.dbHelperOledb.Insert(sql) > 0;
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
            throw new NotImplementedException();
        }


        //public List<Order> GetOrdersByPage(int page)
        //{
        //    int ordersperpage = 5;
        //    List<Order> orders = this.GetAll();
        //    return orders.Skip(ordersperpage * (page - 1)).Take(ordersperpage).ToList();
        //}

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
