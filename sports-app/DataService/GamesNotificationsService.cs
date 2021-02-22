using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppModel;
using PushServiceLib;
using log4net;
using log4net.Config;

namespace DataService
{
    public class GamesNotificationsService : BaseRepo
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GamesNotificationsService));

        class CloseGamesItem
        {
            public string Message { get; set; }
            public int[] Recipients { get; set; }
        }

        class RecipiendDto
        {
            public int UserId { get; set; }
            public int TeamId { get; set; }
            public int LeagueId { get; set; }
        }

        public void SaveNotifications()
        {
            var messages = GetGamesAndUsers();

            foreach (var msg in messages)
            {
                var nm = new NotesMessage();
                nm.Message = msg.Message;
                nm.SendDate = DateTime.Now;
                db.NotesMessages.Add(nm);

                foreach (int userId in msg.Recipients)
                {
                    var recip = new NotesRecipient { MsgId = nm.MsgId, UserId = userId };
                    db.NotesRecipients.Add(recip);
                }

                base.Save();
            }
        }

        public async Task SaveUserDeviceToken(int userId, string deviceToken, int servToken, bool isIOS, string section)
        {
            log.DebugFormat("SaveUserDeviceToken: userId={0}, deviceToken={1}, serveToken={2}, section={3}", userId, deviceToken, servToken, section);

            var sectionObj = db.Sections.Where(s => s.Alias == section)
                .FirstOrDefault();

            var sectionId = (sectionObj != null ? sectionObj.SectionId : 0);

            var device = db.UsersDvices.FirstOrDefault(t => t.UserId == userId &&
                                                            t.DeviceToken == deviceToken &&
                                                            t.ServiceToken == servToken &&
                                                            t.SectionId == sectionId);

            if (device == null)
            {
                device = new UsersDvice
                {
                    UserId = userId,
                    DeviceToken = deviceToken,
                    ServiceToken = servToken,
                    DeviceType = isIOS ? UserDeviceType.IOS : UserDeviceType.Android,
                    //ServiceType = isIOS ? NotificationServiceType.APNS : NotificationServiceType.GCM,
                    ServiceType = NotificationServiceType.GCM,
                    SectionId = (sectionObj != null ? sectionObj.SectionId : 0)
                };

                db.UsersDvices.Add(device);
                log.Info("SaveUserDeviceToken: Device Token added");
            }
            else
            {
                log.Info("SaveUserDeviceToken: Device Token already exists");
            }

            if(deviceToken!=null && device.EndpointArn == null)
            {
                try
                {
                    AmznSnsWrapper snsWrapper = new AmznSnsWrapper();
                    //AmznSnsWrapper.Platform mobAppPlatform = isIOS ? AmznSnsWrapper.Platform.APNS : AmznSnsWrapper.Platform.GCM;
                    AmznSnsWrapper.Platform mobAppPlatform =  AmznSnsWrapper.Platform.GCM;
                    AmznSnsWrapper.MobileAppForSports mobAppForSports = 0;

                    if (sectionObj != null)
                    {
                        if ("basketball".Equals(sectionObj.Alias))
                            mobAppForSports = AmznSnsWrapper.MobileAppForSports.Basketball;
                        else if ("volleyball".Equals(sectionObj.Alias))
                            mobAppForSports = AmznSnsWrapper.MobileAppForSports.Volleyball;
                        else if ("netball".Equals(sectionObj.Alias))
                            mobAppForSports = AmznSnsWrapper.MobileAppForSports.Netball;
                        else if ("waterpolo".Equals(sectionObj.Alias))
                            mobAppForSports = AmznSnsWrapper.MobileAppForSports.Waterpolo;
                    }

                    device.EndpointArn = await snsWrapper.CreateEndpoint(deviceToken, mobAppPlatform, mobAppForSports);
                } catch (Exception e)
                {
                    log.ErrorFormat("SaveUserDeviceToken: Error while registering ARN from Amazon for User Device Token: {0}", e.ToString());
                }
            }

            db.SaveChanges();
        }

        public async Task UnregisterDeviceToken(int userId, string deviceToken)
        {
            log.InfoFormat("UnregisterDeviceToken: userId={0}, deviceToken={1}", userId, deviceToken);
            AmznSnsWrapper snsWrapper = new AmznSnsWrapper();
            var device = db.UsersDvices.Where(t => t.UserId == userId && t.DeviceToken == deviceToken).FirstOrDefault();
            if (device != null)
            {
                db.UsersDvices.Remove(device);
                if(device.ServiceToken!=null)
                    await snsWrapper.RemoveEndpoint(device.EndpointArn);
                db.SaveChanges();
            }
            else
            {
                log.Info("UnregisterDeviceToken: Device Token doesn't exists");
            }
            
        }

        public void SendPushToDevices(bool isTest)
        {
            log.Info("SendPushToDevices");
            var recList = (from m in db.NotesMessages
                           from r in m.NotesRecipients
                           from u in r.User.UsersDvices
                           where ((m.TypeId & MessageTypeEnum.NoPushNotify) == 0)
                            && r.IsPushSent == false && u.ServiceType != null && u.EndpointArn != null && m.Sender != r.UserId
                           select new
                           {
                               m.MsgId,
                               m.Message,
                               r.UserId,
                               u.ServiceToken,
                               u.ServiceType,
                               u.EndpointArn
                           }).ToList();

            if(recList.Count > 0)
            {
                var snsWrapper = new AmznSnsWrapper();

                foreach (var msg in recList.GroupBy(g => new { g.MsgId, g.Message, g.ServiceType }))
                {
                    var endpointsArnArr = msg.Select(t => new
                    {
                        t.EndpointArn,
                        t.UserId
                    }).ToArray();
                    string rawMsg = null;

                    try
                    {

                        log.InfoFormat("SendPushToDevices: Sending. MsgId={0}", msg.Key.MsgId);

                        switch (msg.Key.ServiceType)
                        {
                            case NotificationServiceType.APNS:
                                rawMsg = snsWrapper.BuildPlatformMessage(AmznSnsWrapper.Platform.APNS, msg.Key.Message).ToString();
                                break;
                            case NotificationServiceType.GCM:
                                rawMsg = snsWrapper.BuildPlatformMessage(AmznSnsWrapper.Platform.GCM, msg.Key.Message).ToString();
                                break;
                        }
                        foreach (var ea in endpointsArnArr)
                        {
                            switch (snsWrapper.SendMessage(ea.EndpointArn.Trim(), rawMsg))
                            {
                                case 0:
                                    log.InfoFormat("SendPushToDevices: Success. UserId={0}, EndpointArn={1}", ea.UserId, ea.EndpointArn);
                                    // success:
                                    break;
                                case -1:
                                    // EndpointArn Disabled
                                    UsersDvice ud = db.UsersDvices.First(d => d.EndpointArn == ea.EndpointArn);
                                    if (ud != null)
                                    {
                                        db.UsersDvices.Remove(ud);
                                        db.SaveChanges();
                                    }

                                    log.InfoFormat("SendPushToDevices: EndpointArd Disabled. Removed Device. UserId={0}, EndpointArn={1}", ea.UserId, ea.EndpointArn);
                                    break;
                                case -9999:
                                    log.InfoFormat("SendPushToDevices: Unknown Error. UserId={0}, EndpointArn={1}", ea.UserId, ea.EndpointArn);
                                    // other exception occured
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Debug("Error while trying to send message to device", e);
                    }
                }

                foreach (var item in db.NotesRecipients.Where(t => t.IsPushSent == false))
                {
                    item.IsPushSent = true;
                }

                db.SaveChanges();
            }
            else
            {
                log.Info("SendPushToDevices: Nothing to send");
            }
        }

        private string GetTimeMessage(DateTime gameDate)
        {
            var diff = gameDate.Subtract(DateTime.Now);
            string res = "";

            if (diff.TotalHours > 0)
            {
                res += diff.TotalHours.ToString("00") + " שעות ";
            }

            if (diff.Minutes > 0)
            {
                res += diff.Minutes.ToString("00") + " דקות ";
            }

            if (diff.Seconds > 0)
            {
                res += diff.Seconds.ToString("00") + " שניות ";
            }

            return res;
        }

        private List<RecipiendDto> GetFansTeams()
        {
            return (from u in db.Users
                    from n in u.Notifications.Where(t => t.Type == "StartAlert").DefaultIfEmpty()
                    from tf in u.TeamsFans
                    where u.IsArchive == false && u.UsersType.TypeRole == AppRole.Fans && n == null
                    select new RecipiendDto { UserId = u.UserId, TeamId = tf.TeamId, LeagueId = tf.LeageId }).ToList();
        }

        private List<RecipiendDto> GetPlayersTeams()
        {
            return (from l in db.Leagues
                    from t in l.LeagueTeams
                    from tp in t.Teams.TeamsPlayers
                    from n in tp.User.Notifications.Where(nt => nt.Type == "StartAlert").DefaultIfEmpty()
                    where t.Teams.IsArchive == false && n == null
                    select new RecipiendDto { UserId = tp.UserId, TeamId = t.TeamId, LeagueId = l.LeagueId }).ToList();
        }

        private IEnumerable<CloseGamesItem> GetGamesAndUsers()
        {
            var resList = new List<CloseGamesItem>();
            var todayDate = DateTime.Today;
            var fromDate = todayDate.AddDays(1);
            var toDate = todayDate.AddDays(2);

            var nextGames = db.GamesCycles.Where(t => t.StartDate >= fromDate && t.StartDate < toDate && !t.NotesGames.Any())
                .Select(t => new { t.CycleId, t.StartDate, t.Stage.LeagueId, GuestTeam = t.GuestTeam.Title, HomeTeam = t.HomeTeam.Title, t.HomeTeamId, t.GuestTeamId })
                .ToList();

            if (nextGames.Count == 0)
            {
                return resList;
            }

            foreach (var game in nextGames)
            {
                var noteGame = new NotesGame { CycleId = game.CycleId, SendDate = DateTime.Now };
                db.NotesGames.Add(noteGame);
            }

            base.Save();

            var usersList = GetFansTeams();

            var playersList = GetPlayersTeams();

            usersList.AddRange(playersList);

            if (usersList.Count == 0)
            {
                return resList;
            }

            foreach (var game in nextGames)
            {
                var recipients = usersList.Where(t => t.LeagueId == game.LeagueId
                    && (t.TeamId == game.HomeTeamId || t.TeamId == game.GuestTeamId))
                    .Select(t => t.UserId)
                    .Distinct()
                    .ToArray();

                if (recipients.Length > 0)
                {
                    string timeMessage = GetTimeMessage(game.StartDate);

                    var item = new CloseGamesItem();
                    item.Message = "המשחק " + game.HomeTeam + " - " + game.GuestTeam + ", מתחיל בעוד " + timeMessage;
                    item.Recipients = recipients;
                    resList.Add(item);
                }
            }

            return resList;
        }
    }
}
