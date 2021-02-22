<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">



            protected void Page_Load(object sender, EventArgs e)
        {
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            string API_ACCESS_KEY = "AAAAUhktnWs:APA91bEDF18oeJfVymY_XqRazecx-JkQiTACBZkWzntbNaL9I9QU_454_pIUElNStzc_AzPKQxyn1uY3M4q-mRa2JeWY6fKQ8gy3qZX4g1qY6uW8JG1h2x0eKK05xvdZlKnOw8Fy5lMH";

            GamesCycleManager gm = new GamesCycleManager();
            TeamsFanManager tm = new TeamsFanManager();
            TeamsPlayerManager tp = new TeamsPlayerManager();
            UserDeviceManager um = new UserDeviceManager();
            TeamsManager t = new TeamsManager();

            var db = new AppModel.DataEntities();

            List<AppModel.GamesCycle> listgame = gm.getListEnded();
            List<AppModel.TeamsFan> listfan;
            List<AppModel.TeamsPlayer> listplayer;
            List<AppModel.UsersDvice> listdevice;
            AppModel.Team guest, home;
            string title;
            // Guest Team
            for (int i = 0;i < listgame.Count; i++)
            {
                DateTime datenow = DateTime.Now;
                TimeSpan dis = datenow -  listgame[i].StartDate;
                int time = dis.Days * 24 * 60 * 60 + dis.Hours * 60 * 60 + dis.Minutes * 60 + dis.Seconds;
                // check Game has just ended: Use time in Israel - End Time of Game < 1 minute
                if (time <= 60)
                {
                    listfan = tm.getListByTeamId(Convert.ToInt32(listgame[i].GuestTeamId));
                    guest = t.getTeamById(Convert.ToInt32(listgame[i].GuestTeamId));
                    home = t.getTeamById(Convert.ToInt32(listgame[i].HomeTeamId));
                    title = home.Title + " " + listgame[i].HomeTeamScore + " - " + listgame[i].GuestTeamScore + guest.Title;


                    for (int j = 0; j < listfan.Count; j++)
                    {
                        listdevice = um.getListByUserId(listfan[j].UserId);
                        for (int d = 0; d < listdevice.Count; d++)
                        {
                            PostForm(API_ACCESS_KEY, listdevice[d].DeviceToken, title, title);
                        }

                    }
                    listplayer = tp.getListByTeamId(Convert.ToInt32(listgame[i].GuestTeamId));
                    for (int j = 0; j < listplayer.Count; j++)
                    {
                        listdevice = um.getListByUserId(listplayer[j].UserId);
                        for (int d = 0; d < listdevice.Count; d++)
                        {
                            PostForm(API_ACCESS_KEY, listdevice[d].DeviceToken, title, title);
                        }

                    }
                }

            }
            // Home Team
            for (int i = 0; i < listgame.Count; i++)
            {
                DateTime datenow = DateTime.Now;
                TimeSpan dis = datenow - listgame[i].StartDate;
                int time = dis.Days * 24 * 60 * 60 + dis.Hours * 60 * 60 + dis.Minutes * 60 + dis.Seconds;
                // check Game has just ended: Use time in Israel - End Time of Game < 1 minute
                if (time <= 60)
                {
                    listfan = tm.getListByTeamId(Convert.ToInt32(listgame[i].HomeTeamId));
                    guest = t.getTeamById(Convert.ToInt32(listgame[i].GuestTeamId));
                    home = t.getTeamById(Convert.ToInt32(listgame[i].HomeTeamId));
                    title = home.Title + " " + listgame[i].HomeTeamScore + " - " + listgame[i].GuestTeamScore + guest.Title;
                    for (int j = 0; j < listfan.Count; j++)
                    {
                        listdevice = um.getListByUserId(listfan[j].UserId);
                        for (int d = 0; d < listdevice.Count; d++)
                        {
                            PostForm(API_ACCESS_KEY, listdevice[d].DeviceToken, title, title);
                        }

                    }
                    listplayer = tp.getListByTeamId(Convert.ToInt32(listgame[i].HomeTeamId));
                    for (int j = 0; j < listplayer.Count; j++)
                    {
                        listdevice = um.getListByUserId(listplayer[j].UserId);
                        for (int d = 0; d < listdevice.Count; d++)
                        {
                            PostForm(API_ACCESS_KEY, listdevice[d].DeviceToken, title, title);
                        }

                    }
                }

            }
        }

        private string PostForm(string API_ACCESS_KEY, string to, string title, string message)
        {
            var registrationIds = new string[] { to };
            var msg = new
            {
                message = message,
                title = title,
                vibrate = 1,
                soundname = "default",
                // you can also add images, additionalData
            };
            var fields = new
            {
                registration_ids = registrationIds,
                data = msg
            };

            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            request.Method = "POST";
            request.Headers.Add("Authorization", "key=" + API_ACCESS_KEY);

            request.ContentType = "application/json";

            string postData = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(fields);

            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = bytes.Length;

            System.IO.Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);

            System.Net.WebResponse response = request.GetResponse();
            System.IO.Stream stream = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream);

            var result = reader.ReadToEnd();
            stream.Dispose();
            reader.Dispose();

            return result;
        }


        public class GamesCycleManager
        {
            AppModel.DataEntities db = new AppModel.DataEntities();

            public List<AppModel.GamesCycle> getListEnded()
            {
                try
                {
                    return db.GamesCycles.Where(u => u.GameStatus == "ended").ToList();
                }
                catch
                {
                    return null;
                }
            }

            public List<AppModel.GamesCycle> getListStart()
            {
                try
                {
                    return db.GamesCycles.Where(u => u.GameStatus == null).ToList();
                }
                catch
                {
                    return null;
                }
            }
        }
        
            public class TeamsPlayerManager
    {
       AppModel.DataEntities db = new AppModel.DataEntities();

        public List<AppModel.TeamsPlayer> getListByTeamId(int id)
        {
            try
            {
                return db.TeamsPlayers.Where(u => u.TeamId == id).ToList();
            }
            catch
            {
                return null;
            }
        }
    }
        



    public class TeamsFanManager
    {
       AppModel.DataEntities db = new AppModel.DataEntities();

        public List<AppModel.TeamsFan> getListByTeamId(int id)
        {
            try
            {
                return db.TeamsFans.Where(u => u.TeamId == id).ToList();
            }
            catch
            {
                return null;
            }
        }
    }

     public class UserDeviceManager
    {
       AppModel.DataEntities db = new AppModel.DataEntities();

        public List<AppModel.UsersDvice> getListByUserId(int id)
        {
            try
            {
                return db.UsersDvices.Where(u => u.UserId == id).ToList();
            }
            catch
            {
                return null;
            }
        }
    }

     public class TeamsManager
    {
        AppModel.DataEntities db = new AppModel.DataEntities();

        public AppModel.Team getTeamById(int id)
        {
            try
            {
                return db.Teams.Where(u => u.TeamId == id).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }

</script>