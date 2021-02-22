using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

public partial class v2_get_event : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.AppendHeader("Access-Control-Allow-Origin", "*");

    
        string leagueid = Request["leagueid"];

        PatchManager PM = new PatchManager();


        string json;

        List<Event> list = PM.GetLastEventList(Convert.ToInt32(leagueid));
        
        if (list != null)
        {
            List<eventList> listEvent = new List<eventList>();

            for (int i = 0; i < list.Count; i++)
            {
                listEvent.Add(new eventList
                {
                    id = list[i].EventId,
                    title = list[i].Title,
                    date =  list[i].EventTime,
                });
            }

            json = JsonConvert.SerializeObject(listEvent);

            Response.Write(json);
        } else
        {
            Response.Write(null);
        }

    }

    public class eventList
    {
        public int id { get; set; }
        public string title { get; set; }
        public DateTime date { get; set; }
    }
}