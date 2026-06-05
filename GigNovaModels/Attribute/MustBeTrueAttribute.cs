using System.ComponentModel.DataAnnotations;

namespace GigNovaModels.Attribute
{
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }

            return (bool)value;
        }
    }
}
