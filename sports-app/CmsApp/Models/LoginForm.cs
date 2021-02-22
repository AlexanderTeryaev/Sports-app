using System;
using System.ComponentModel.DataAnnotations;
using Resources;

namespace CmsApp.Models
{
    public class LoginForm
    {
        public LoginAction LoginAction { get; set; }
        public CultEnum Culture { get; set; } //TEMP

        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

        public string Captcha { get; set; }
        public bool IsRemember { get; set; }
        public bool IsSecure { get; set; }
    }

    public class LoginForgotPasswordModel : LoginForm
    {
        [Required]
        public string RestoreEmail { get; set; }

        [Required]
        public string RestoreIdentNum { get; set; }

        public bool RestoreEmailSent { get; set; }
    }

    public class LoginResetPasswordModel : LoginForm
    {
        public Guid ResetId { get; set; }

        [Required]
        public string ResetPassword { get; set; }

        [Required]
        [Compare(nameof(ResetPassword), ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.Login_ResetPassword_ConfirmPasswordDoesNotMatch))]
        public string ConfirmResetPassword { get; set; }

        public bool ResetCompleted { get; set; }
    }

    public class LoggedView
    {
        public string UserName { get; set; }
        public string RoleType { get; set; }
    }

    public enum LoginAction
    {
        Login,
        ForgotPassword,
        ResetPassword
    }
}