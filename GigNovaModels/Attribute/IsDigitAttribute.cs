using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Attribute
{
    public class IsDigitsAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            for (int i = 0; i < value.ToString().Length; i++)
            {
                if (!(value.ToString()[i] >= '0' && value.ToString()[i] <= '9'))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
