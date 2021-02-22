<%@ Page Language="C#" AutoEventWireup="true" %>
<% 
    /*GET CLUB BY USER ID - API V2 - LOGLIG*/
    Response.AppendHeader("Access-Control-Allow-Origin","*");
    string json = "[";
    var db = new AppModel.DataEntities();

    int userid = Convert.ToInt32(Request["userid"]);

    AppModel.User user;
    try
    {
        user = db.Users.Where(t => t.UserId == userid).First();
        if(user!=null)
        {
            // list teams in clubs
            AppModel.Club club;
            for(int i = 0; i <user.TeamsFans.ToList().Count;i++ )
            {
                club = user.TeamsFans.ToList()[i].Team.ClubTeams.ToList()[0].Club;
                if(i< user.TeamsFans.ToList().Count  - 1)
                     json += "{\"Clubid\": " + club.ClubId +",\"Name\":\""+club.Name.Replace("\"","") +"\"},";
                else
                     json += "{\"Clubid\": " + club.ClubId +",\"Name\":\""+club.Name.Replace("\"","")+"\"}";
            }
        }
    }
    catch
    {
        json += "";
    }
    json += "]";
%>
<%= json %>