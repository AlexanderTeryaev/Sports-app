using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CmsApp.Helpers;

namespace CmsApp.Models
{
    public class UserForm
    {
        public int UserId { get; set; }
        public int TypeId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string IdentNum { get; set; }
        [Required]
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }
        public IEnumerable<SelectListItem> RolesList { get; set; }
    }
}