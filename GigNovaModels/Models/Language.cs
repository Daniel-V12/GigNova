using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Language:Model
    {
        string language_id;
        string language_name;

        public Language()
        {

        }
        public string Language_id
        {
            get { return language_id; }
            set { language_id = value; }
        }
        public string Language_name
        {
            get { return language_name; }
            set { language_name = value; }
        }
    }
}
