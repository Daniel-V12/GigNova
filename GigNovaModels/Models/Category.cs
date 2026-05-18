using GigNovaModels.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Category : Model
    {

        string category_id;
        string category_name;
        bool is_blocked;

        public Category()
        {

        }
        public string Category_id
        {
            get { return category_id; }
            set { category_id = value; }
        }

        [Required(ErrorMessage = "Category name is required")]
        [FirstLetterCaps(ErrorMessage = "Each word in category name must start with a capital letter")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 30 characters")]
        public string Category_name
        {
            get { return category_name; }
            set { category_name = value; }
        }

        public bool Is_blocked
        {
            get { return is_blocked; }
            set { is_blocked = value; }
        }
    }
}