<%@ Page Language="C#" AutoEventWireup="true" %>
<% 
    /*REGISTER FAN - API V2 - LOGLIG*/
    Response.AppendHeader("Access-Control-Allow-Origin","*");
    var db = new AppModel.DataEntities();

    System.IO.StreamReader reader = new System.IO.StreamReader(HttpContext.Current.Request.InputStream);
    reader.BaseStream.Position = 0;
    string requestFromPost = reader.ReadToEnd();

  

    string name = requestFromPost.Split(',')[0].Split(':')[1].Replace("\"","");

    string password = requestFromPost.Split(',')[1].Split(':')[1].Replace("\"","");

    string email = requestFromPost.Split(',')[2].Split(':')[1].Replace("\"","");

    string teams = requestFromPost.Split('[')[1].Split(']')[0].Replace("{","").Replace("}","");


    // validate existed emails
     if (db.Users.Where(u => u.UserName == name).Count() > 0)
            {
                Response.Write(Resources.Messages.UsernameExists);
                return;
               
            }


            if (db.Users.Where(u => u.Email == email).Count() > 0)
            {
                Response.Write(Resources.Messages.EmailExists);
                return;
             
            }


            // create a new user
    var user = new AppModel.User()
    {
        UserName = name,
        Email = email,
        Password = Protector.Encrypt(password),
        UsersType = db.UsersTypes.FirstOrDefault(t => t.TypeRole == AppRole.Fans),
        IsActive = true,

    };



    int team1 = 0;




    try
    {
        team1 =   Convert.ToInt32(teams.Split(',')[0].Split(':')[1]);
        user.TeamsFans.Add(new AppModel.TeamsFan
        {
            TeamId = team1,
            UserId = user.UserId,
            LeageId = 56 // defeaul leagueId 56 is club team
        });
    }
    catch
    {

    }



    int team2 = 0;
    try
    {
        team2 =  Convert.ToInt32(teams.Split(',')[4].Split(':')[1]);
        user.TeamsFans.Add(new AppModel.TeamsFan
        {
            TeamId = team2,
            UserId = user.UserId,
            LeageId = 56 // defeaul leagueId 56 is club team
        });
    }
    catch
    {

    }



    int team3 = 0;
    try
    {
        team3 =  Convert.ToInt32(teams.Split(',')[8].Split(':')[1]);
        user.TeamsFans.Add(new AppModel.TeamsFan
        {
            TeamId = team3,
            UserId = user.UserId,
            LeageId = 56 // defeaul leagueId 56 is club team
        });
    }
    catch
    {

    }






    var newUser = db.Users.Add(user);
    db.SaveChanges();
    if (newUser != null)
    {
        Response.Write("Registered!");
    }


%>