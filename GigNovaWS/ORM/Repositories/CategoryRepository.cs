using GigNovaModels;
using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class CategoryRepository : Repository, IRepository<Category>
    {
        public bool Create(Category model)
        {
            string sql = "Insert into Categories (category_name, category_photo) values ( @category_name ,  @category_photo)";
            this.dbHelperOledb.AddParameter("@category_name", model.Category_name);
            this.dbHelperOledb.AddParameter("@category_photo", model.Category_photo);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Categories where category_id = @category_id";
            this.dbHelperOledb.AddParameter("category_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public List<Category> GetAll()
        {
            string sql = "Select * from Categories";
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

        public Category GetById(string id)
        {
            string sql = "Select * from Categories where category_id = @category_id";
            this.dbHelperOledb.AddParameter("@category_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.CategoryCreator.CreateModel(reader);
            }
        }

        public bool Update(Category model)
        {
            string sql = @"Update Categories set 
            category_name = @category_name ,
            category_photo = @category_photo";
            this.dbHelperOledb.AddParameter("@category_name", model.Category_name);
            this.dbHelperOledb.AddParameter("@category_photo", model.Category_photo);
            return this.dbHelperOledb.Update(sql) > 0;
        }
    }
}
