using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Attribute
{
    public class NoSpacesAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            string str = value.ToString();
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ' ')
                    return false;
            }
            return true;
        }
    }
}