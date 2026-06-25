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

            // Ignore empty entries so extra spaces between words (or leading/trailing
            // spaces) don't fail the check - we only validate the actual words.
            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int w = 0; w < words.Length; w++)
            {
                string word = words[w];

                if (word[0] < 'A' || word[0] > 'Z')
                    return false;

                // Only the FIRST letter of each word has to be uppercase. The remaining
                // letters may be upper OR lower case, so acronyms like "SEO", "HTML" or
                // mixed-case words like "PhD" are accepted (previously they were rejected
                // because the loop required every following letter to be lowercase).
                for (int i = 1; i < word.Length; i++)
                {
                    bool isLowerLetter = (word[i] >= 'a' && word[i] <= 'z');
                    bool isUpperLetter = (word[i] >= 'A' && word[i] <= 'Z');
                    if (isLowerLetter == false && isUpperLetter == false)
                        return false;
                }
            }
            return true;
        }
    }
}