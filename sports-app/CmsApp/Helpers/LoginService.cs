using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace CmsApp.Helpers
{
    public static class LoginService
    {
        public static void UpdateSessions(string newId, string oldId)
        {
            var sessList = HttpRuntime.Cache["sessions"] as List<string>;

            if (sessList == null)
                sessList = new List<string>();
            else
                sessList.Remove(oldId);

            sessList.Add(newId);
            HttpRuntime.Cache["sessions"] = sessList;
        }

        public static bool IsValidSession(string sessId)
        {
            if (string.IsNullOrEmpty(sessId))
                return false;

            var sessList = HttpRuntime.Cache["sessions"] as List<string>;
            if (sessList == null)
                return false;

            return sessList.Contains(sessId);
        }

        public static void RemoveSession(string sessId)
        {
            if (string.IsNullOrEmpty(sessId))
                return;

            var sessList = HttpRuntime.Cache["sessions"] as List<string>;
            if (sessList == null)
                return;

            sessList.Remove(sessId);
            HttpRuntime.Cache["sessions"] = sessList;
        }
    }
}