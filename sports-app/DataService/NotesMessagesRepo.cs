using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;
using log4net;
using log4net.Config;

namespace DataService
{

    public class NotesMessagesRepo : BaseRepo
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NotesMessagesRepo));

        public void Create(NotesMessage msg)
        {
            db.NotesMessages.Add(msg);
        }

        public IQueryable<NotesRecipient> GetByUser(int userId)
        {
            return (from n in db.NotesMessages
                    from r in n.NotesRecipients
                    where r.UserId == userId && r.IsArchive == false
                    orderby n.MsgId descending
                    select r);
        }

        public void SetRead(int userId, int[] msgsArr)
        {
            var msgList = db.NotesRecipients.Where(t => t.UserId == userId && msgsArr.Contains(t.MsgId));
            foreach (var m in msgList)
            {
                m.IsRead = true;
            }
        }

        public NotesMessage GetMessageById(int msgId)
        {
            return db.NotesMessages.Find(msgId);
        }

        public void DeleteMessage(int messageId)
        {
            var sentMessage = db.SentMessages.FirstOrDefault(x => x.MessageId == messageId);

            if (sentMessage == null) return;

            var message = db.NotesMessages.FirstOrDefault(x => x.MsgId == messageId);

            if (message == null) return;

            db.NotesAttachedFiles.RemoveRange(message.NotesAttachedFiles);
            db.SentMessages.Remove(sentMessage);
            db.NotesMessages.Remove(message);
        }

        public void Delete(int msgId, int userId)
        {
            var msg = db.NotesRecipients.Where(t => t.MsgId == msgId && t.UserId == userId).FirstOrDefault();
            msg.IsArchive = true;
        }

        public void DeleteAll(int userId, int[] msgsArr)
        {
            var msgList = db.NotesRecipients.Where(t => t.UserId == userId && msgsArr.Contains(t.MsgId));
            foreach (var msg in msgList)
            {
                msg.IsArchive = true;
            }
        }

        public int? SendToTeam(int seasonId, int teamId, string message, bool isSchedule = false, bool sendByEmail = false, string subject = null)
        {
            log.InfoFormat("SendToTeam: seasonId={0}, teamId={1}, message={2}", seasonId, teamId, message);

            var playersIds = (from teamPlayer in db.TeamsPlayers
                                    join user in db.Users on teamPlayer.UserId equals user.UserId
                                    where teamPlayer.TeamId == teamId && teamPlayer.IsActive && teamPlayer.SeasonId == seasonId &&
                                          user.IsArchive == false && user.IsActive && user.IsBlocked == false
                                    select user.UserId).ToList();
            var officialsIds = (from jobs in db.UsersJobs
                                      join user in db.Users on jobs.UserId equals user.UserId
                                      where jobs.TeamId == teamId && jobs.SeasonId == seasonId &&
                                      user.IsArchive == false && user.IsActive && user.IsBlocked == false
                                      select user.UserId).ToList();

            var userIds = playersIds.Union(officialsIds).ToList();

            if (isSchedule)
            {
                var fansIds = (from fans in db.TeamsFans
                                     join user in db.Users on fans.UserId equals user.UserId
                                     where fans.TeamId == teamId &&
                                     user.IsArchive == false && user.IsActive && user.IsBlocked == false
                                     select user.UserId).ToList();
                userIds = playersIds.Union(officialsIds).Union(fansIds).ToList();
            }


            var messageId = SaveMessage(userIds, message, sendByEmail, false, subject);
            if (messageId.HasValue)
            {
                db.SentMessages.Add(new SentMessage()
                {
                    MessageId = messageId.Value,
                    TeamId = teamId,
                    SeasonId = seasonId
                });
            }

            Save();

            return messageId;
        }

        public void SetEmailSendStatus(IEnumerable<NotesRecipient> notesRecipients)
        {
            notesRecipients.Select(c => { c.IsRead = true; return c; });
        }

        public IEnumerable<NotesRecipient> GetAllRecipients(int messageId)
        {
            return db.NotesRecipients.Where(c => c.MsgId == messageId && !c.IsEmailSent && c.NotesMessage.SentByEmail);
        }

        public int? SendToPlayer(int seasonId, int userId, string message, bool sendByEmail = false, string subject = null)
        {
            log.InfoFormat($"SendToTeam: seasonId='{seasonId}', userId='{userId}', message='{message}', subject='{subject}'");

            //List<int> playersIds = (from teamPlayer in db.TeamsPlayers
            //                        join user in db.Users on teamPlayer.UserId equals user.UserId
            //                        where teamPlayer.TeamId == teamId && teamPlayer.IsActive && teamPlayer.SeasonId == seasonId &&
            //                              user.IsArchive == false && user.IsActive && user.IsBlocked == false
            //                        select user.UserId).ToList();
            //List<int> officialsIds = (from jobs in db.UsersJobs
            //                          join user in db.Users on jobs.UserId equals user.UserId
            //                          where jobs.TeamId == teamId && jobs.SeasonId == seasonId &&
            //                          user.IsArchive == false && user.IsActive && user.IsBlocked == false
            //                          select user.UserId).ToList();

            var userIds = new List<int> { userId };

            var messageId = SaveMessage(userIds, message, sendByEmail, subject: subject);
            if (messageId.HasValue)
            {
                db.SentMessages.Add(new SentMessage()
                {
                    MessageId = messageId.Value,
                    UserId = userId,
                    SeasonId = seasonId
                });
            }

            Save();

            return messageId;
        }

        public int? SendToLeague(int seasonId, int leagueId, string message, bool sendByEmail = false, IEnumerable<int> teamManagersIds = null, string subject = null)
        {
            log.InfoFormat("SendToLeague: seasonId={0}, leagueId={1}, message={2}", seasonId, leagueId, message);

            var userIds = new List<int>();
            var sentForTeamManagers = false;
            if (teamManagersIds != null && teamManagersIds.Count() > 0)
            {
                userIds = teamManagersIds.ToList();
                sentForTeamManagers = true;
            }
            else
            {
                var playersIds = (from leagueTeam in db.LeagueTeams
                                  join teamPlayer in db.TeamsPlayers on leagueTeam.TeamId equals teamPlayer.TeamId
                                  join user in db.Users on teamPlayer.UserId equals user.UserId
                                  where leagueTeam.LeagueId == leagueId && leagueTeam.SeasonId == seasonId &&
                                        teamPlayer.SeasonId == seasonId && teamPlayer.IsActive &&
                                        user.IsActive && user.IsArchive == false && user.IsBlocked == false
                                  select user.UserId).ToList();

                var officialsIds = (from jobs in db.UsersJobs
                                    join user in db.Users on jobs.UserId equals user.UserId
                                    where jobs.LeagueId == leagueId && jobs.SeasonId == seasonId &&
                                    user.IsArchive == false && user.IsActive && user.IsBlocked == false
                                    select user.UserId).ToList();

                userIds = playersIds.Union(officialsIds).ToList();
            }

            var messageId = SaveMessage(userIds, message, sendByEmail, false, subject, sentForTeamManagers);

            if (messageId.HasValue)
            {
                db.SentMessages.Add(new SentMessage()
                {
                    MessageId = messageId.Value,
                    LeagueId = leagueId,
                    SeasonId = seasonId
                });
            }

            Save();
            return messageId;
        }

        public int? SendToUnion(int seasonId, int unionId, string message, bool sendByEmail = false, IEnumerable<int> clubManagersIds = null, string subject = null)
        {
            log.InfoFormat("SendToUnion: seasonId={0}, unionId={1}, message={2}", seasonId, unionId, message);

            var playersIds = (from season in db.Seasons
                                    join union in db.Unions on season.UnionId equals union.UnionId
                                    join league in db.Leagues on union.UnionId equals league.UnionId
                                    join leagueTeam in db.LeagueTeams on league.LeagueId equals leagueTeam.LeagueId
                                    join teamPlayer in db.TeamsPlayers on leagueTeam.TeamId equals teamPlayer.TeamId
                                    join user in db.Users on teamPlayer.UserId equals user.UserId
                                    where season.Id == seasonId && union.UnionId == unionId && league.UnionId == unionId &&
                                          leagueTeam.SeasonId == seasonId && teamPlayer.SeasonId == seasonId &&
                                          teamPlayer.IsActive && user.IsActive && user.IsArchive == false && user.IsBlocked == false
                                    select user.UserId).Distinct().ToList();

            var officialsIds = (from jobs in db.UsersJobs
                                      join user in db.Users on jobs.UserId equals user.UserId
                                      where jobs.UnionId == unionId && jobs.SeasonId == seasonId &&
                                      user.IsArchive == false && user.IsActive && user.IsBlocked == false
                                      select user.UserId).ToList();

            var userIds = clubManagersIds != null ? clubManagersIds.ToList() : playersIds.Union(officialsIds).ToList();

            var messageId = SaveMessage(userIds, message, sendByEmail, clubManagersIds != null, subject);
            if (messageId.HasValue)
            {
                db.SentMessages.Add(new SentMessage()
                {
                    MessageId = messageId.Value,
                    UnionId = unionId,
                    SeasonId = seasonId
                });
            }

            Save();

            return messageId;
        }

        public int? SendToDiscipline(int seasonId, int disciplineId, string message, bool sendByEmail = false, string subject = null)
        {
            log.InfoFormat("SendToDiscipline: seasonId={0}, disciplineId={1}, message={2}", seasonId, disciplineId, message);

            var playersIds = (from discipline in db.Disciplines
                                    join league in db.Leagues on discipline.DisciplineId equals league.DisciplineId
                                    join leagueTeam in db.LeagueTeams on league.LeagueId equals leagueTeam.LeagueId
                                    join teamPlayer in db.TeamsPlayers on leagueTeam.TeamId equals teamPlayer.TeamId
                                    join user in db.Users on teamPlayer.UserId equals user.UserId
                                    where discipline.DisciplineId == disciplineId && league.DisciplineId == disciplineId &&
                                          leagueTeam.SeasonId == seasonId && teamPlayer.SeasonId == seasonId &&
                                          teamPlayer.IsActive && user.IsActive && user.IsArchive == false && user.IsBlocked == false
                                    select user.UserId).Distinct().ToList();

            var officialsIds = (from jobs in db.UsersJobs
                                      join user in db.Users on jobs.UserId equals user.UserId
                                      where jobs.DisciplineId == disciplineId && jobs.SeasonId == seasonId &&
                                            user.IsArchive == false && user.IsActive && user.IsBlocked == false
                                      select user.UserId).ToList();

            var userIds = playersIds.Union(officialsIds).ToList();

            var messageId = SaveMessage(userIds, message, sendByEmail, false, subject);
            if (messageId.HasValue)
            {
                db.SentMessages.Add(new SentMessage()
                {
                    MessageId = messageId.Value,
                    DisciplineId = disciplineId,
                    SeasonId = seasonId
                });
            }

            Save();
            return messageId;
        }

        public int? SendToUsers(IReadOnlyCollection<int> userIds, string message)
        {
            log.InfoFormat("SendToUsers: userIds={0}, message={1}", userIds, message);
            return SaveMessage(userIds, message, false);
        }

        public int? SendToUsers(IReadOnlyCollection<int> userIds, string message, int messageType)
        {
            log.InfoFormat("SendToUsers: userIds={0}, message={1}, type={2}", userIds, message, messageType);
            return SaveMessage(userIds, message, messageType, false);
        }

        private int? SaveMessage(IReadOnlyCollection<int> playersIds, string message, int messageType, bool sendByEmail, bool sentForClubManagers = false, string subject = null, bool sentForTeamManagers = false)
        {
            if (playersIds.Count > 0)
            {
                var msg = new NotesMessage
                {
                    Subject = subject,
                    Message = message,
                    SendDate = DateTime.Now,
                    TypeId = messageType,
                    SentByEmail = sendByEmail,
                    SentForClubManagers = sentForClubManagers,
                    SentForTeamManagers = sentForTeamManagers
                };

                db.NotesMessages.Add(msg);

                foreach (var userId in playersIds.Distinct())
                {
                    var nr = new NotesRecipient { MsgId = msg.MsgId, UserId = userId };
                    db.NotesRecipients.Add(nr);
                }

                Save();

                return msg.MsgId;
            }

            return null;
        }


        private int? SaveMessage(IReadOnlyCollection<int> playersIds, string message, bool sendByEmail, bool sentForClubManagers = false, string subject = null, bool sentForTeamManagers = false)
        {
            return this.SaveMessage(playersIds, message, MessageTypeEnum.Root, sendByEmail, sentForClubManagers, subject, sentForTeamManagers);
        }

        public int? SendToClub(int seasonId, int clubId, string message, bool sendByEmail = false, string subject = null)
        {
            log.InfoFormat("SendToClub: seasonId={0}, clubId={1}, message={2}", seasonId, clubId, message);
            var clubPlayers = (db.ClubTeams
                    .Join(db.Teams, club => club.TeamId, team => team.TeamId, (club, team) => new {club, team})
                    .Join(db.TeamsPlayers, t => t.team.TeamId, teamplayers => teamplayers.TeamId,
                        (t, teamplayers) => new {t, teamplayers})
                    .Where(t => t.t.club.ClubId == clubId && t.teamplayers.IsActive)
                    .Select(t => t.teamplayers.UserId))
                .ToList();
            var schoolsPlayers = (db.SchoolTeams
                    .Join(db.TeamsPlayers, schoolTeam => schoolTeam.TeamId, teamsPlayer => teamsPlayer.TeamId,
                        (st, teamsPlayer) => new {st.School, teamsPlayer})
                    .Where(x => x.School.ClubId == clubId && x.teamsPlayer.IsActive)
                    .Select(t => t.teamsPlayer.UserId))
                .ToList();

            var officialsIds = (from club in db.ClubTeams
                                      join team in db.Teams on club.TeamId equals team.TeamId
                                      join jobs in db.UsersJobs on team.TeamId equals jobs.TeamId
                                      join user in db.Users on jobs.UserId equals user.UserId
                                      where club.ClubId == clubId && jobs.SeasonId == seasonId &&
                                      user.IsArchive == false && user.IsActive && user.IsBlocked == false
                                      select user.UserId).ToList();

            var userIds = clubPlayers.Union(officialsIds).Union(schoolsPlayers).Distinct().ToList();
            int? messageId = null;
            if (userIds.Any())
            {
                var msg = new NotesMessage
                {
                    Message = message,
                    SendDate = DateTime.Now
                };

                messageId = SaveMessage(userIds, message, sendByEmail, false, subject);
                if (messageId.HasValue)
                {
                    db.SentMessages.Add(new SentMessage()
                    {
                        MessageId = messageId.Value,
                        ClubId = clubId,
                        SeasonId = seasonId
                    });
                }

                Save();
            }
            return messageId;
        }

        public List<NotesMessage> GetLeagueTeamMessages(int seasonId, int teamId)
        {
            Func<SentMessage, bool> predicate = (sentMessage) => sentMessage.SeasonId == seasonId &&
                                                                 sentMessage.TeamId == teamId;
            var teamMessages = GetMessages(predicate);

            return teamMessages;
        }

        public List<NotesMessage> GetLeagueMessages(int seasonId, int leagueId)
        {
            Func<SentMessage, bool> predicate = (sentMessage) => sentMessage.SeasonId == seasonId &&
                                                                 sentMessage.LeagueId == leagueId;
            var leagueMessages = GetMessages(predicate);

            return leagueMessages;
        }

        public List<NotesMessage> GetUnionMessages(int seasonId, int unionId)
        {
            Func<SentMessage, bool> predicate = (sentMessage) => sentMessage.SeasonId == seasonId &&
                                                                 sentMessage.UnionId == unionId;
            var unionMessages = GetMessages(predicate);

            return unionMessages;
        }

        public List<NotesMessage> GetDisciplineMessages(int seasonId, int disciplineId)
        {
            Func<SentMessage, bool> predicate = (sentMessage) => sentMessage.SeasonId == seasonId &&
                                                                 sentMessage.DisciplineId == disciplineId;
            var disciplineMessages = GetMessages(predicate);

            return disciplineMessages;
        }

        public List<NotesMessage> GetMessages(Func<SentMessage, bool> predicate)
        {
            var messages = (from sentMessage in db.SentMessages.Where(predicate)
                            join message in db.NotesMessages on sentMessage.MessageId equals message.MsgId
                            select message).ToList();
            return messages;
        }

        public void AddDeviceToUser(int userId, string deviceToken, int servToken)
        {
            var device = db.UsersDvices.FirstOrDefault(t => t.UserId == userId &&
                                                            t.DeviceToken == deviceToken &&
                                                            t.ServiceToken == servToken);

            if (device == null)
            {
                device = new UsersDvice
                {
                    UserId = userId,
                    DeviceToken = deviceToken,
                    ServiceToken = servToken
                };

                db.UsersDvices.Add(device);
                db.SaveChanges();
            }
        }

        public void DeleteDeviceToken(int userId, string token)
        {
            var device = db.UsersDvices.Where(t => t.UserId == userId && t.DeviceToken == token).FirstOrDefault();
            if (device != null)
            {
                db.UsersDvices.Remove(device);
                db.SaveChanges();
            }
        }

        public int[] GetTeamTokents(int teamId)
        {
            return (from t in db.Teams
                    from u in t.TeamsPlayers
                    from tk in u.User.UsersDvices
                    where t.TeamId == teamId
                    select (int)tk.ServiceToken).ToArray();
        }

        public List<NotesMessage> GetClubMessages(int seasonId, int clubId)
        {
            Func<SentMessage, bool> predicate = (sentMessage) => sentMessage.SeasonId == seasonId &&
                                                                 sentMessage.ClubId == clubId;
            var unionMessages = GetMessages(predicate);

            return unionMessages;
        }


        public void SendChatToUsers(PostChatMessage chatmsg, int senderId)
        {

            var msg = new NotesMessage
            {
                Message = chatmsg.Message,
                SendDate = DateTime.Now,
                TypeId = MessageTypeEnum.ChatMessage,
                Sender = senderId,
                img = chatmsg.img,
                video = chatmsg.video
            };

            if (chatmsg.parent > 0)
            {
                //insert reply, forword
                db.NotesMessages.Where(nn => nn.MsgId == chatmsg.parent).First().Childs.Add(msg);
                Save();
                return;
            }

            if (chatmsg.Users.Count > 0)
            {

                db.NotesMessages.Add(msg);
                Save();

                var bSendMe = false;
                foreach (var userId in chatmsg.Users.Distinct())
                {
                    if (userId == senderId)
                        bSendMe = true;
                    var nr = new NotesRecipient { MsgId = msg.MsgId, UserId = userId };
                    db.NotesRecipients.Add(nr);
                }

                if(!bSendMe)
                {
                    var currentUsernr = new NotesRecipient { MsgId = msg.MsgId, UserId = senderId }; //sned to sender himself
                    db.NotesRecipients.Add(currentUsernr);
                }

                Save();

                var notsServ = new GamesNotificationsService();
                notsServ.SendPushToDevices(false);

                //if (messageId.HasValue)
                //{
                //    db.SentMessages.Add(new SentMessage()
                //    {
                //        MessageId = messageId.Value,
                //    });
                //}

                //Save();
            }
        }

        public void ForwarcChatToUsers(int MsgId, List<int> usres)
        {
            if (usres.Count > 0)
            {

                foreach (var userId in usres.Distinct())
                {
                    var nr = new NotesRecipient { MsgId = MsgId, UserId = userId };
                    var previousCnt = db.NotesRecipients.Where(n => n.MsgId == MsgId && n.UserId == userId).Count();
                    if (previousCnt > 0) continue;
                    db.NotesRecipients.Add(nr);
                }

                Save();

                var notsServ = new GamesNotificationsService();
                notsServ.SendPushToDevices(false);

            }
        }

        public class PostChatMessage
        {
            public bool SendAllFlag { get; set; }
            public List<int> Users { get; set; }
            public string Message { get; set; }
            public int? SenderId { get; set; }
            public int? parent { get; set; }
            public string img { get; set; }
            public string video { get; set; }
        }


    }
}
