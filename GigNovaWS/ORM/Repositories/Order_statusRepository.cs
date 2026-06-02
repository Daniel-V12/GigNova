using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class Order_statusRepository : Repository, IRepository<Order_status>
    {
        public Order_statusRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {
        }


        // ============================== Create / Update / Delete ==============================

        public bool Create(Order_status model)
        {
            string sql = @$"Insert into Order_Statuses (status_name)
            values ( @status_name)";
            this.dbHelperOledb.AddParameter("@status_name", model.Status_name);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        // The Order_Statuses table is a fixed lookup (In Progress / Delivered / Completed) - we never
        // edit a row. Update is required by IRepository<T> but unused.
        // (The previous body had both a wrong table name and no WHERE clause - making it dangerous.)
        public bool Update(Order_status model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Order_Statuses where order_status_id = @order_status_id";
            this.dbHelperOledb.AddParameter("@order_status_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }


        // ============================== Read (single + lists) ==============================

        public List<Order_status> GetAll()
        {
            string sql = "Select * from Order_Statuses";
            List<Order_status> order_statuses = new List<Order_status>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    order_statuses.Add(this.modelCreators.OrderStatusCreator.CreateModel(reader));
                }
            }
            return order_statuses;
        }

        public Order_status GetById(string id)
        {
            string sql = "Select * from Order_Statuses where order_status_id = @order_status_id";
            this.dbHelperOledb.AddParameter("@order_status_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.OrderStatusCreator.CreateModel(reader);
            }
        }
    }
}
