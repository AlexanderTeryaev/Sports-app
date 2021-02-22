using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for PatchManager
/// </summary>
public class PatchManager
{
    public PatchManager()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    DataClassesDataContext db = new DataClassesDataContext();

    /// <summary>
    /// get events list by leagueid
    /// </summary>
    /// <param name="leagueid"></param>
    /// <returns></returns>
    public List<Event> GetLastEventList(int leagueid)
    {
        try
        {
            var list = db.Events.Where(t => t.LeagueId == leagueid && t.IsPublished == true).ToList();
            return list;
        }
        catch
        {
            return null;
        }
    } 

    public List<Event> GetNextEventList(int leagueid)
    {
        try
        {
            var list = db.Events.Where(t => t.LeagueId == leagueid && t.IsPublished == false).ToList();
            return list;
        }
        catch
        {
            return null;
        }
    }
}