using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class PasswordRecoveryViewModel
    {

        public PasswordRecoveryViewModel()
        {
            UserName = "";
            Password = "";
        }

        [Required]
        public string UserName { get; set; }

        public string Password { get; set; }

    }
}