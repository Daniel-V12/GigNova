using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.Models
{
    public class Person
    {
        string person_id;
        string person_username;
        string person_password;
        string person_birthdate;
        string person_join_date;
        string person_email;
        public string Person_id
        {
            get { return person_id; }
            set { person_id = value; }
        }
        [NoSpaces(ErrorMessage = "Username cannot contain spaces.")]
        [Required(ErrorMessage = "Username is required")]
        public string Person_username
        {
            get { return person_username; }
            set { person_username = value; }
        }
        [Required(ErrorMessage = "Password is required")]
        [NoSpaces(ErrorMessage = "Password cannot contain spaces.")]
        public string Person_password
        {
            get { return person_password; }
            set { person_password = value; }
        }
        public string Person_birthdate
        {
            get { return person_birthdate; }
            set { person_birthdate = value; }
        }
        public string Person_join_date
        {
            get { return person_join_date; }
            set { person_join_date = value; }
        }
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Required(ErrorMessage = "Email is required")]
        [NoSpaces(ErrorMessage = "Email cannot contain spaces.")]
        public string Person_email
        {
            get { return person_email; }
            set { person_email = value; }
        }
    }
}
