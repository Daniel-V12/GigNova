using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class Order_filesRepository : Repository, IRepository<Order_file>
    {
        public Order_filesRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {
        }


        // ============================== Create / Update / Delete ==============================

        public bool Create(Order_file model)
        {
            string sql = @$"Insert into Order_Files (order_file_path, order_id)
            values ( @order_file_path, @order_id)";
            this.dbHelperOledb.AddParameter("@order_file_path", model.Order_file_path);
            this.dbHelperOledb.AddParameter("@order_id", model.Order_id);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        // Order files aren't edited - they're saved once with their order. Update is required by
        // IRepository<T> but unused. (The previous body had no WHERE clause which would have updated
        // every row.)
        public bool Update(Order_file model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Order_Files where order_file_id = @order_file_id";
            this.dbHelperOledb.AddParameter("@order_file_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }


        // ============================== Read (single + lists) ==============================

        public List<Order_file> GetAll()
        {
            string sql = "Select * from Order_Files";
            List<Order_file> order_files = new List<Order_file>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    order_files.Add(this.modelCreators.OrderFileCreator.CreateModel(reader));
                }
            }
            return order_files;
        }

        public Order_file GetById(string id)
        {
            string sql = "Select * from Order_Files where order_file_id = @order_file_id";
            this.dbHelperOledb.AddParameter("@order_file_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.OrderFileCreator.CreateModel(reader);
            }
        }

        // First file attached to an order (the one shown as the "main" requirement file).
        public Order_file GetByOrderId(string orderId)
        {
            string sql = "Select * from Order_Files where order_id = @order_id";
            this.dbHelperOledb.AddParameter("@order_id", orderId);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == true)
                {
                    return this.modelCreators.OrderFileCreator.CreateModel(reader);
                }
            }
            return null;
        }

        // All files attached to an order (the buyer may have uploaded up to 5).
        public List<Order_file> GetAllByOrderId(string orderId)
        {
            string sql = "Select * from Order_Files where order_id = @order_id";
            this.dbHelperOledb.AddParameter("@order_id", orderId);
            List<Order_file> orderFiles = new List<Order_file>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    orderFiles.Add(this.modelCreators.OrderFileCreator.CreateModel(reader));
                }
            }
            return orderFiles;
        }
    }
}
