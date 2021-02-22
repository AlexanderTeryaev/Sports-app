<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.AppendHeader("Access-Control-Allow-Origin", "*");
        try
        {
            int userid = Convert.ToInt32(Request["id"]);
            var db = new AppModel.DataEntities();
            AppModel.User user = db.Users.Where(u => u.UserId == userid && u.IsActive == true).FirstOrDefault();
            Noti noti = new Noti();
            noti.IsStartAlert = Convert.ToBoolean(user.IsStartAlert);
            noti.IsTimeChange = Convert.ToBoolean(user.IsTimeChange);
            noti.IsGameScores = Convert.ToBoolean(user.IsGameScores);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(noti);
            Response.Write(json);
        }
        catch
        {
            Response.Write("Error");
        }
    }

    public class Noti
    {
        public bool IsStartAlert { get; set; }
        public bool IsTimeChange { get; set; }
        public bool IsGameScores { get; set; }
    }

</script>