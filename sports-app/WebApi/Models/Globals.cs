using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace WebApi.Models
{
    public class ExistsResponse
    {
        public bool Exists { get; set; }
    }
}

public class Settings
{
    public static bool IsTest
    {
        get { return bool.Parse(WebConfigurationManager.AppSettings["IsTestEvironment"]); }
    }
}

public class ImageGalleryViewModel
{
    public string url { get; set; }
    public DateTime Created { get; set; }
    public UserModel User { get; set; }
}

public class UserModel
{
    public int Id { get; set; } // User id
    public String Name { get; set; } // User name
    public String Image { get; set; } // User image
    public string UserRole { get; set; }
}
