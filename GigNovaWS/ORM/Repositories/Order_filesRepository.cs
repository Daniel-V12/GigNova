using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class Order_filesRepository : Repository, IRepository<Order_file>
    {
        public Order_filesRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Order_file model)
        {
            string sql = @$"Insert into Order_Files (order_file_name)
            values ( @order_file_name)";
            this.dbHelperOledb.AddParameter("@order_file_name", model.Order_file_name);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Order_Files where order_file_id = @order_file_id";
            this.dbHelperOledb.AddParameter("@order_file_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

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

        public bool Update(Order_file model)
        {
            string sql = @"Update Order_Files set 
            order_file_name = @order_file_name";
            this.dbHelperOledb.AddParameter("@order_file_name", model.Order_file_name);
            return this.dbHelperOledb.Update(sql) > 0;
        }
    }
}
