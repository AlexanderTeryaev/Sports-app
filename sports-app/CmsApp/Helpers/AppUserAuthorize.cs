using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

public static class AppUserAuthorize
{
    public static string GetJobRole()
    {
        var authCookie = GetAuthCookie();
        if (authCookie == null)
        {
            return null;
        }

        var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
        string[] userData = authTicket.UserData.Split(new Char[] { '^' });
        if (userData == null || userData.Length < 3)
        {
            return null;
        }

        return userData[2];
    }

    private static HttpCookie GetAuthCookie()
    {
        string cookieName = FormsAuthentication.FormsCookieName;
        return HttpContext.Current.Request.Cookies[cookieName];
    }
}