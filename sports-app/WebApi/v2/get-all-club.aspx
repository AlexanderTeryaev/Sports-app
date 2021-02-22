<%@ Page Language="C#" AutoEventWireup="true" %>
<% 
    Response.AppendHeader("Access-Control-Allow-Origin","*");
    var data = new AppModel.DataEntities();
    List<AppModel.Club> list = data.Clubs.Where(t=>t.ClubTeams.Count>0 ).ToList();
    string json = "[";
    for(int i = 0; i < list.Count;i++)
    {
        if (i < list.Count - 1)
        {
            json += "{\"ClubId\":" + list[i].ClubId + ",\"Title\":\"" + list[i].Name + "\",\"TotalTeams\":" + list[i].ClubTeams.Count + ",\"LeagueId\":" + list[i].ClubId + ",\"Name\":\"" + list[i].Name + "\",\"Teams\":[";
            for (int j = 0; j < list[i].ClubTeams.Count; j++)
            {
                if (j < list[i].ClubTeams.Count - 1)
                    json += "{\"TeamId\":" + list[i].ClubTeams.ToList()[j].TeamId + ",\"Title\":\"" + list[i].ClubTeams.ToList()[j].Team.Title.Replace("\"","") + "\",\"LeagueId\":" + list[i].ClubId + "},";
                else
                    json += "{\"TeamId\":" + list[i].ClubTeams.ToList()[j].TeamId + ",\"Title\":\"" + list[i].ClubTeams.ToList()[j].Team.Title.Replace("\"","") + "\",\"LeagueId\":" + list[i].ClubId + "}";

            }
            json += "]},";
        }
        else
        {
            json += "{\"ClubId\":" + list[i].ClubId + ",\"Title\":\"" + list[i].Name + "\",\"TotalTeams\":" + list[i].ClubTeams.Count + ",\"LeagueId\":" + list[i].ClubId + ",\"Name\":\"" + list[i].Name + "\",\"Teams\":[";
            for (int j = 0; j < list[i].ClubTeams.Count; j++)
            {
                if (j < list[i].ClubTeams.Count - 1)
                    json += "{\"TeamId\":" + list[i].ClubTeams.ToList()[j].TeamId + ",\"Title\":\"" + list[i].ClubTeams.ToList()[j].Team.Title.Replace("\"","") + "\",\"LeagueId\":" + list[i].ClubId + "},";
                else
                    json += "{\"TeamId\":" + list[i].ClubTeams.ToList()[j].TeamId + ",\"Title\":\"" + list[i].ClubTeams.ToList()[j].Team.Title.Replace("\"","") + "\",\"LeagueId\":" + list[i].ClubId + "}";

            }
            json += "]}";

        }
    }
    json += "]";
%>
<%= json %>

