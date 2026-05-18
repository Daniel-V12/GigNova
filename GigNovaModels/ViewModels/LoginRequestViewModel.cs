using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels.ViewModels
{
    public class LoginRequestViewModel
    {
        [Required(ErrorMessage = "Username or email is required")]
        public string identifier { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string password { get; set; }
    }
}