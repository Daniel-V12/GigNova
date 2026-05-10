using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class CategoryCreator : IModelCreator<Category>
    {
        public Category CreateModel(IDataReader dataReader)
        {
            Category category = new Category();
            category.Category_id = Convert.ToString(dataReader["category_id"]);
            category.Category_name = Convert.ToString(dataReader["category_name"]);
            category.Is_blocked = Convert.ToBoolean(dataReader["is_blocked"]);

            return category;
        }

    }
}