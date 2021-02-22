using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebApi.Models
{
    public class FanRegisterBindingModel
    {
        [Required]
        public string UserName { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public List<TeamRegisterBindingModel> Teams { get; set; }

        public string Language { get; set; }

        public int UnionId { get; set; }
    }

    public class FBFanRegisterBindingModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string FullName { get; set; }
        //[Required]
        //public string FirstName { get; set; }
        //[Required]
        //public string LastName { get; set; }
        //public string MiddleName { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "FBID")]
        public string FbId { get; set; }

        public string Language { get; set; }

        public List<TeamRegisterBindingModel> Teams { get; set; }
    }

    public class WorkerRegisterBindingModel
    {
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }


        [Required]
        [Display(Name = "PersonalId")]
        public string PersonalId { get; set; }
    }

    // Models used as parameters to AccountController actions.
    public class AddExternalLoginBindingModel
    {
        [Required]
        [Display(Name = "External access token")]
        public string ExternalAccessToken { get; set; }
    }

    public class ChangePasswordBindingModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
   
    public class RegisterExternalBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class RemoveLoginBindingModel
    {
        [Required]
        [Display(Name = "Login provider")]
        public string LoginProvider { get; set; }

        [Required]
        [Display(Name = "Provider key")]
        public string ProviderKey { get; set; }
    }

    public class SetPasswordBindingModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
