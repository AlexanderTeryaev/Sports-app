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

            user.IsStartAlert = Convert.ToBoolean(Request["IsStartAlert"]);
            user.IsTimeChange = Convert.ToBoolean(Request["IsTimeChange"]);
            user.IsGameScores = Convert.ToBoolean(Request["IsGameScores"]);
            db.SaveChanges();
        } catch
        {
            Response.Write("Error");
        }

    }

</script>