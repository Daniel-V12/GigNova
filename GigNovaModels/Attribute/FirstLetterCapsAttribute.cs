using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Attribute
{
    public class FirstLetterCapsAttribute : ValidationAttribute
    {

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            string text = value.ToString();
            if (text.Length == 0)
                return true;

            string[] words = text.Split(' ');
            for (int w = 0; w < words.Length; w++)
            {
                string word = words[w];

                if (word.Length == 0)
                    return false;

                if (word[0] < 'A' || word[0] > 'Z')
                    return false;

                for (int i = 1; i < word.Length; i++)
                {
                    if (word[i] < 'a' || word[i] > 'z')
                        return false;
                }
            }
            return true;
        }
    }
}