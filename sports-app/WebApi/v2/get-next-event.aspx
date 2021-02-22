<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">

     protected void Page_Load(object sender, EventArgs e)
        {
             /*GET NEXT EVENT- API V2 - LOGLIG*/
            Response.AppendHeader("Access-Control-Allow-Origin","*");
            

            int leagueid = Convert.ToInt32(Request["leagueid"]);

             var db = new AppModel.DataEntities();
             List<AppModel.Event> list = db.Events.Where(t => t.LeagueId == leagueid && t.IsPublished == true).ToList();
               


            string json;

            DateTime datenow = new DateTime();
            datenow = DateTime.Now;
            DateTime eventdate;
            if (list != null)
            {
                List<eventList> listEvent = new List<eventList>();

                for (int i = 0; i < list.Count; i++)
                {
                    eventdate = list[i].EventTime;
                    if (eventdate >= datenow )
                    {
                        listEvent.Add(new eventList
                        {
                            id = list[i].EventId,
                            title = list[i].Title,
                            date = list[i].EventTime,
                            place = list[i].Place,
                        });
                    }

                }

                json = Newtonsoft.Json.JsonConvert.SerializeObject(listEvent);

                Response.Write(json);
            }
            else
            {
                Response.Write(null);
            }
        }

    public class eventList
        {
            public int id { get; set; }
            public string title { get; set; }
            public DateTime date { get; set; }
            public string place { get; set; }
        }

</script>