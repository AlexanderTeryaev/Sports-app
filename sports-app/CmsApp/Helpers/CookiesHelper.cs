using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Helpers
{
    public static class CookiesHelper
    {
        public const string CookieName = "cmsdata";

        public static bool IsCookieExist()
        {
            var cookie = HttpContext.Current.Request.Cookies.Get(CookieName);
            return cookie != null;
        }

        public static string GetCookieValue(string valName)
        {
            var cookie = HttpContext.Current.Request.Cookies.Get(CookieName);
            if (cookie != null)
                return HttpUtility.UrlDecode(cookie.Values[valName]);
            else
                return "";
        }

        public static string GetSessionWorkerValueOrTopLevelLeagueJob(this IPrincipal user, int leagueId)
        {
            var value = (string) HttpContext.Current.Session["utype"];
            if (!string.IsNullOrWhiteSpace(value) && user.HasLeagueLevelJob(leagueId, value))
                return value;
            else
                return user.CurrentTopLeagueLevelJob(leagueId);
        }

        public static string GetSessionWorkerValueOrTopLevelClubJob(this IPrincipal user, int clubId)
        {
            var value = (string) HttpContext.Current.Session["utype"];
            if (!string.IsNullOrWhiteSpace(value) && user.HasClubLevelJob(clubId, value))
                return value;
            else
                return user.CurrentTopClubLevelJob(clubId);
        }

        public static string GetSessionWorkerValueOrTopLevelSeasonJob(this IPrincipal user, int seasonId)
        {
            var value = (string) HttpContext.Current.Session["utype"];
            if (!string.IsNullOrWhiteSpace(value) && user.HasSeasonLevelJob(seasonId, value))
                return value;
            else
                return user.CurrentTopLevelJob(seasonId);
        }

        public static System.Collections.Generic.List<int> GetClubsUserManagingBySeason(this IPrincipal user, int seasonId)
        {
            return user.GetClubsUserManaging(seasonId);
        }

        public static void SetWorkerSession(this Controller controller, string role)
        {
            controller.Session.Add("utype", role);
        }

        public static void SetCookieValue(string valName, string val)
        {
            var cookie = HttpContext.Current.Request.Cookies.Get(CookieName);
            if (cookie == null)
            {
                cookie = new HttpCookie(CookieName);
            }

            cookie.Values[valName] = HttpUtility.UrlEncode(val);
            cookie.Expires = DateTime.Now.AddDays(1d);

            HttpContext.Current.Response.Cookies.Set(cookie);
        }

        public static void RemoveCookie(string cookieName)
        {
            var cookie = HttpContext.Current.Request.Cookies.Get(cookieName);
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1d);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        public static void SetCookie(string cName, object cValue, DateTime expires)
        {
            var cookie = HttpContext.Current.Request.Cookies[cName];
            if (cookie == null)
            {
                cookie = new HttpCookie(cName);
            }

            cookie.Value = cValue.ToString();
            cookie.Expires = expires;

            HttpContext.Current.Response.Cookies.Set(cookie);
        }

        public static string GetCookie(string cName)
        {
            var cookie = HttpContext.Current.Request.Cookies[cName];
            if (cookie != null)
                return HttpUtility.UrlDecode(cookie.Value);
            else
                return "";
        }
    }
}