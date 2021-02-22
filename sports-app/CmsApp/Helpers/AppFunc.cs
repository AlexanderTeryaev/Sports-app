using DataService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web.Mvc;

public static class AppFunc
{
    public static SelectList GetGridItemsCombo(object selected)
    {
        var resList = new List<SelectListItem>();

        for (int i = GlobVars.GridItems; i <= 35; i += 10)
        {
            string val = i.ToString();
            resList.Add(new SelectListItem { Value = val, Text = val });
        }

        return new SelectList(resList, "Value", "Text", selected);
    }

    public static string GetUniqName()
    {
        return DateTime.Now.ToString("ddMMyyyyHHmmssfff");
    }

    public static bool HasTopLevelJob(this IPrincipal principal, string roleName)
    {
        using (var rep = new UsersRepo())
        {
            var identityName = principal.Identity.Name;
            var userId = int.Parse(string.IsNullOrEmpty(identityName) ? "0" : identityName);
            return rep.GetTopLevelJob(userId) == roleName;
        }
    }

    public static bool IsInAnyRole(this IPrincipal principal, params string[] roles)
    {
        return roles.Any(principal.IsInRole);
    }

    public static bool IsInAnyCurrentRole(this IPrincipal principal, params string[] roles)
    {
        return roles.Any(principal.IsInRole);
    }

    public static string CurrentTopLevelJob(this IPrincipal principal, int seasonId)
    {
        using (var rep = new UsersRepo())
        {
            var identityName = principal.Identity.Name;
            var userId = int.Parse(string.IsNullOrEmpty(identityName) ? "0" : identityName);
            return rep.GetCurrentJob(userId, seasonId);
        }
    }

    public static List<int> GetClubsUserManaging(this IPrincipal principal, int seasonId)
    {
        using (var rep = new UsersRepo())
        {
            var identityName = principal.Identity.Name;
            var userId = int.Parse(string.IsNullOrEmpty(identityName) ? "0" : identityName);
            return rep.GetClubsUserManaging(userId, seasonId);
        }
    }


    public static bool HasSeasonLevelJob(this IPrincipal principal, int seasonId, string role)
    {
        using (var rep = new UsersRepo())
        {
            var identityName = principal.Identity.Name;
            var userId = int.Parse(string.IsNullOrEmpty(identityName) ? "0" : identityName);
            return rep.HasCurrentJob(userId, seasonId, role);
        }
    }



    public static string CurrentTopClubLevelJob(this IPrincipal principal, int clubId)
    {
        using (var rep = new UsersRepo())
        {
            var identityName = principal.Identity.Name;
            var userId = int.Parse(string.IsNullOrEmpty(identityName) ? "0" : identityName);
            return rep.GetCurrentClubJob(userId, clubId);
        }
    }


    public static bool HasClubLevelJob(this IPrincipal principal, int clubId, string role)
    {
        using (var rep = new UsersRepo())
        {
            var identityName = principal.Identity.Name;
            var userId = int.Parse(string.IsNullOrEmpty(identityName) ? "0" : identityName);
            return rep.HasClubJob(userId, clubId, role);
        }
    }


    public static string CurrentTopLeagueLevelJob(this IPrincipal principal, int leagueId)
    {
        using (var rep = new UsersRepo())
        {
            var identityName = principal.Identity.Name;
            var userId = int.Parse(string.IsNullOrEmpty(identityName) ? "0" : identityName);
            return rep.GetCurrentLeagueJob(userId, leagueId);
        }
    }

    public static bool HasLeagueLevelJob(this IPrincipal principal, int leagueId, string role)
    {
        using (var rep = new UsersRepo())
        {
            var identityName = principal.Identity.Name;
            var userId = int.Parse(string.IsNullOrEmpty(identityName) ? "0" : identityName);
            return rep.HasLeagueJob(userId, leagueId, role);
        }
    }

    public static IEnumerable<SelectListItem> GetHebWeekDays()
    {
        var resList = new List<SelectListItem>();
        string days = "אבגדהוש";
        int i = 0;

        foreach (char ch in days)
        {
            var item = new SelectListItem();
            item.Text = ch.ToString();
            item.Value = (i++).ToString();
            resList.Add(item);
        }

        return resList;
    }

    public static List<T> ConvertDataTableToList<T>(DataTable dt, Boolean doesSchameMatch = true)
    {
        List<T> data = new List<T>();
        foreach (DataRow row in dt.Rows)
        {
            if (doesSchameMatch)
            {
                T item = GetRow<T>(row);
                data.Add(item);
            }
            else
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
        }

        return data;
    }

    private static T GetItem<T>(DataRow dr)
    {
        Type temp = typeof(T);
        T item = Activator.CreateInstance<T>();

        try
        {
            PropertyInfo[] arrProp = temp.GetProperties();//.ToList().OrderBy(a => a.Name).ToArray();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in arrProp)
                {
                    if (pro.Name == column.ColumnName)
                    {
                        if (dr[column.ColumnName] == DBNull.Value)
                            pro.SetValue(item, null, null);
                        else
                            pro.SetValue(item, dr[column.ColumnName], null);
                        break;
                    }
                    else
                        continue;
                }
            }
        }
        catch (Exception ex)
        {

        }
        return item;
    }

    private static T GetRow<T>(DataRow dr)
    {
        Type temp = typeof(T);
        T obj = Activator.CreateInstance<T>();
        try
        {
            PropertyInfo[] pros = temp.GetProperties();
            DataColumnCollection columns = dr.Table.Columns;
            for (int i =0; i< dr.Table.Columns.Count; i++)
            {
                if (dr[columns[i]] == DBNull.Value)
                    pros[i].SetValue(obj, null, null);
                else
                    pros[i].SetValue(obj, dr[columns[i]], null);

            }
        }
        catch (Exception ex)
        {

        }
        return obj;
    }

}