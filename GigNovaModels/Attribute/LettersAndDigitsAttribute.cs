using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Attribute
{
    public class LettersAndDigitsAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            string str = value.ToString();
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                bool isLowerLetter = (c >= 'a' && c <= 'z');
                bool isUpperLetter = (c >= 'A' && c <= 'Z');
                bool isDigit = (c >= '0' && c <= '9');

                if (!isLowerLetter && !isUpperLetter && !isDigit)
                    return false;
            }
            return true;
        }
    }
}