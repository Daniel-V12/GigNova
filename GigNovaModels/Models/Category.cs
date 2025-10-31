using GigNovaModels.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Category:Model
    {

        string category_id;
        string category_name;
        string category_photo;
        public string Category_id
        {
            get { return category_id; }
            set { category_id = value; }
        }

        [FirstLetterCaps(ErrorMessage = "Category must start with a capital letter")]
        [Required(ErrorMessage = "category is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Category must be no longer than 20 characters and no less than 2")]
        public string Category_name
        {
            get { return category_name; }
            set { category_name = value; }
        }
        public string Category_photo
        {
            get { return category_photo; }
            set { category_photo = value; }
        }
    }
}
