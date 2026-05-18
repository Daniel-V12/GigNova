using GigNovaModels.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Person : Model
    {
        string person_id = "";
        string person_username;
        string person_password;
        string person_birthdate;
        string person_join_date = "";
        string person_email;


        public Person()
        {

        }

        public string Person_salt { get; set; } = "";

        public string Person_id
        {
            get { return person_id; }
            set
            {
                if (value == null)
                    person_id = "";
                else
                    person_id = value;
            }
        }
        [Required(ErrorMessage = "Username is required")]
        [NoSpaces(ErrorMessage = "Username cannot contain spaces.")]
        [LettersAndDigits(ErrorMessage = "Username can only contain letters and digits")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters")]
        public string Person_username
        {
            get { return person_username; }
            set { person_username = value; }
        }
        [Required(ErrorMessage = "Password is required")]
        [NoSpaces(ErrorMessage = "Password cannot contain spaces.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters")]
        public string Person_password
        {
            get { return person_password; }
            set { person_password = value; }
        }
        [Required(ErrorMessage = "Birthdate is required")]
        public string Person_birthdate
        {
            get { return person_birthdate; }
            set
            {
                if (value == null)
                    person_birthdate = "";
                else
                    person_birthdate = value;
            }
        }
        public string Person_join_date
        {
            get { return person_join_date; }
            set { person_join_date = value; }
        }

        [Required(ErrorMessage = "Email is required")]
        [NoSpaces(ErrorMessage = "Email cannot contain spaces.")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Person_email
        {
            get { return person_email; }
            set { person_email = value; }
        }
    }
}