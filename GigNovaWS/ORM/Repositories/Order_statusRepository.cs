using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class Order_statusRepository : Repository, IRepository<Order_status>
    {
        public Order_statusRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Order_status model)
        {
            string sql = @$"Insert into Order_Statuses (status_name)
            values ( @status_name)";
            this.dbHelperOledb.AddParameter("@status_name", model.Status_name);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Order_Statuses where order_status_id = @order_status_id";
            this.dbHelperOledb.AddParameter("@order_status_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public List<Order_status> GetAll()
        {
            string sql = "Select * from Order_Statuses";
            List<Order_status> order_files = new List<Order_status>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    order_files.Add(this.modelCreators.OrderStatusCreator.CreateModel(reader));
                }
            }
            return order_files;
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

        public bool Update(Order_status model)
        {
            string sql = @"Update Order_Files set 
            status_name = @status_name";
            this.dbHelperOledb.AddParameter("@status_name", model.Status_name);
            return this.dbHelperOledb.Update(sql) > 0;
        }
    }
}
