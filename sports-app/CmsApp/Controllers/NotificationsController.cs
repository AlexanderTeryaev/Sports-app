using System.Web.Mvc;
using CmsApp.Models;
using DataService;
using AppModel;
using System.Threading.Tasks;
using System.Linq;
using CmsApp.Services;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Configuration;

namespace CmsApp.Controllers
{
    public class NotificationsController : AdminController
    {
        NotesMessagesRepo notesRep = new NotesMessagesRepo();
        SeasonsRepo seasonsRepo = new SeasonsRepo();

        // GET: Notifications
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add(int entityId, LogicaName logicalName, int? unionId, int? clubId, int? leagueId, int? seasonId)
        {
            int currentUnionIdFromSession = GetCurrentUnionFromSession();
            int? currentSeasonIdFromSession = GetUnionCurrentSeasonFromSession();
            int? currentSeasonId = seasonsRepo.GetLasSeasonByUnionId(currentUnionIdFromSession);
            GetEmailAddress(logicalName, entityId, seasonId ?? currentSeasonId ?? 0, unionId, clubId, leagueId, out string clubEmail, out string unionEmail);
            var vm = new NotificationsForm
            {
                SeasonId = seasonId ?? currentSeasonIdFromSession,
                RelevantEntityLogicalName = logicalName,
                EntityId = entityId,
                NeedHideTextField = currentUnionIdFromSession != -1 ? currentSeasonIdFromSession != currentSeasonId : false,
                ClubManagers = logicalName == LogicaName.Union ? GetListOfClubManagers(entityId, seasonId) : Enumerable.Empty<SelectListItem>(),
                TeamManagers = logicalName == LogicaName.League ? GetListOfTeamManagers(entityId, seasonId ?? currentSeasonIdFromSession) : Enumerable.Empty<SelectListItem>(),
                ClubEmail = clubEmail,
                UnionEmail = unionEmail,
                IsUnionManager = User.IsInAnyRole(AppRole.Admins) || usersRepo.GetTopLevelJob(AdminId)?.Equals(JobRole.UnionManager) == true,
                ClubId = clubId,
                LeagueId = leagueId,
                UnionId = unionId
            };

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);

            return PartialView("_AddNew", vm);
        }

        private void GetEmailAddress(LogicaName logicalName, int entityId, int seasonId, int? unionId, int? clubId, int? leagueId,
            out string clubEmail, out string unionEmail)
        {
            clubEmail = string.Empty;
            unionEmail = string.Empty;

            switch (logicalName)
            {
                case LogicaName.Union:
                    unionEmail = unionsRepo.GetById(entityId)?.Email;
                    break;
                case LogicaName.League:
                    var league = leagueRepo.GetById(entityId);
                    unionEmail = league?.Union?.Email;
                    clubEmail = league?.Club?.Email;
                    break;
                case LogicaName.Team:
                    var team = teamRepo.GetById(entityId, seasonId);
                    if (team != null)
                    {
                        var teamLeague = team.LeagueTeams.LastOrDefault()?.Leagues;
                        var teamClub = team.ClubTeams.LastOrDefault()?.Club;
                        unionEmail = teamLeague?.Union?.Email ?? teamLeague?.Club?.Union?.Email ?? teamClub?.Union?.Email ?? string.Empty;
                        clubEmail = teamClub?.Email ?? teamLeague?.Club?.Email ?? string.Empty;
                    }
                    break;
                case LogicaName.Club:
                    var club = clubsRepo.GetById(entityId);
                    clubEmail = club?.Email;
                    break;
            }
        }

        private IEnumerable<SelectListItem> GetListOfClubManagers(int entityId, int? seasonId)
        {
            var unionClubManagers = jobsRepo.GetClubManagersOfUnion(entityId, seasonId);
            return new SelectList(unionClubManagers, nameof(AppModel.User.UserId), nameof(AppModel.User.FullName));
        }

        private IEnumerable<SelectListItem> GetListOfTeamManagers(int entityId, int? seasonId)
        {
            List<KeyValuePair<int, string>> result = GetListOfTeamManagersKeyValuePair(entityId, seasonId);
                
            return new SelectList(result, nameof(KeyValuePair<int, string>.Key), nameof(KeyValuePair<int, string>.Value));
        }

        private List<KeyValuePair<int, string>> GetListOfTeamManagersKeyValuePair(int entityId, int? seasonId, List<int> notesRec = null)
        {
            List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();
            var unionClubManagers = jobsRepo.GetTeamManagersOfLeague(entityId, seasonId);
            var filter = notesRec != null && notesRec.Count > 0;

            foreach (var cm in unionClubManagers)
            {
                if (filter && !notesRec.Contains(cm.Key.UserId)) continue;
                result.Add(new KeyValuePair<int, string>(cm.Key.UserId, string.Join(" - ", cm.Key.FullName, cm.FirstOrDefault()?.Team.Title)));
            }

            return result.OrderBy(x => x.Value).ToList();
        }

        [HttpPost]
        public async Task<ActionResult> Add(NotificationsForm frm)
        {
            int? messageId = null;

            if (!ModelState.IsValid)
            {
                return Content("Error");
            }

            if (frm.SeasonId != null)
            {
                switch (frm.RelevantEntityLogicalName)
                {
                    case LogicaName.Team:
                        messageId = notesRep.SendToTeam(frm.SeasonId.Value, frm.EntityId, frm.Message, false, frm.SendByEmail, frm.Subject);
                        break;
                    case LogicaName.League:
                        messageId = notesRep.SendToLeague(frm.SeasonId.Value, frm.EntityId, frm.Message, frm.SendByEmail, frm.TeamManagerIds, frm.Subject);
                        break;
                    case LogicaName.Union:
                        messageId = notesRep.SendToUnion(frm.SeasonId.Value, frm.EntityId, frm.Message, frm.SendByEmail, frm.ClubManagerIds, frm.Subject);
                        break;
                    case LogicaName.Discipline:
                        messageId = notesRep.SendToDiscipline(frm.SeasonId.Value, frm.EntityId, frm.Message, frm.SendByEmail, frm.Subject);
                        break;
                    case LogicaName.Club:
                        messageId = notesRep.SendToClub(frm.SeasonId.Value, frm.EntityId, frm.Message, frm.SendByEmail, frm.Subject);
                        break;
                    case LogicaName.Player:
                        messageId = notesRep.SendToPlayer(frm.SeasonId.Value, frm.EntityId, frm.Message, frm.SendByEmail, frm.Subject);
                        break;
                }
            }

            var notsServ = new GamesNotificationsService();
            notsServ.SendPushToDevices(GlobVars.IsTest);

            if (frm.SendByEmail)
            {
                if (messageId.HasValue)
                {
                    await SendEmailsToUsersAsync(messageId.Value, frm.IsClubEmailChecked, frm.EntityId, frm.RelevantEntityLogicalName,
                        frm.SeasonId ?? 0, frm.UnionId, frm.ClubId, frm.LeagueId);
                }
            }

            return Redirect(Request.UrlReferrer.PathAndQuery);
        }

        private async Task SendEmailsToUsersAsync(int messageId, bool isClubEmailChecked, int id, LogicaName logicalName, int seasonId, int? unionId = null, int? clubId = null, int? leagueId = null)
        {
            var isUnionManager = User.IsInAnyRole(AppRole.Admins) || usersRepo.GetTopLevelJob(AdminId)?.Equals(JobRole.UnionManager) == true;
            var notesRecipients = notesRep.GetAllRecipients(messageId);
            var message = notesRep.GetMessageById(messageId);
            var messageText = message?.Message;
            var emailService = new EmailService();
            var emailFile = Request.Files["EmailFile"];
            GetEmailAddress(logicalName, id, seasonId, unionId, clubId, leagueId, out string clubEmail, out string unionEmail);
            var sender = GetSenderMail(clubEmail, unionEmail, isUnionManager, isClubEmailChecked);
            if (messageText != null)
            {
                var userEmails = string.Join(",", notesRecipients.Select(c => usersRepo.GetById(c.UserId)?.Email).Where(c => !string.IsNullOrEmpty(c)));
                if (!string.IsNullOrEmpty(userEmails))
                {
                    if (emailFile != null && emailFile.ContentLength > 0)
                    {
                        ProcessNotificationFile(message?.MsgId, emailFile);
                        await emailService.SendWithFileAsync(userEmails, messageText, message?.Subject, emailFile, sender);
                    }
                    else
                        await emailService.SendAsync(userEmails, messageText, message?.Subject, sender);
                }

                notesRep.SetEmailSendStatus(notesRecipients);
                notesRep.Save();
            }
        }

        private string GetSenderMail(string clubEmail, string unionEmail, bool isUnionManager, bool isClubEmailChecked)
        {
            var email = ConfigurationManager.AppSettings["MailServerSenderAdress"];
            if (isClubEmailChecked)
            {
                if (!string.IsNullOrEmpty(unionEmail) && isUnionManager)
                {
                    email = unionEmail;
                }
                else if (!string.IsNullOrEmpty(clubEmail))
                {
                    email = clubEmail;
                }
            }
            return email;
        }

        private void ProcessNotificationFile(int? messageId, HttpPostedFileBase postedFile)
        {
            if (messageId.HasValue)
            {
                var newName = SaveFile(postedFile, messageId.Value);
                db.NotesAttachedFiles.Add(new NotesAttachedFile
                {
                    NoteMessageId = messageId.Value,
                    FilePath = newName
                });
                db.SaveChanges();
            }
        }

        private string SaveFile(HttpPostedFileBase file, int msgId)
        {
            string ext = Path.GetExtension(file.FileName).ToLower();

            var newName = $"{AppFunc.GetUniqName()}{ext}";

            var savePath = Server.MapPath(GlobVars.ContentPath + "/notifications/");

            var di = new DirectoryInfo(savePath);
            if (!di.Exists)
                di.Create();

            byte[] imgData;
            using (var reader = new BinaryReader(file.InputStream))
            {
                imgData = reader.ReadBytes(file.ContentLength);
            }
            System.IO.File.WriteAllBytes(savePath + newName, imgData);
            return newName;
        }

        [HttpPost]
        public ActionResult Delete(int id, int entityId, LogicaName logicalName, int? seasonId)
        {
            notesRep.DeleteMessage(id);
            notesRep.Save();

            return RedirectToAction("List", new { entityId, logicalName, seasonId });
        }

        public ActionResult List(int entityId, LogicaName logicalName, int? seasonId)
        {
            int? currentSeasonId = GetUnionCurrentSeasonFromSession();
            var nvm = new NotificationsViewModel
            {
                EntityId = entityId,
                RelevantEntityLogicalName = logicalName,
                SeasonId = seasonId
            };

            if (seasonId.HasValue)
            {
                switch (logicalName)
                {
                    case LogicaName.Team:
                        nvm.Notifications = notesRep.GetLeagueTeamMessages(seasonId.Value, entityId);
                        break;
                    case LogicaName.League:
                        nvm.Notifications = notesRep.GetLeagueMessages(seasonId.Value, entityId);
                        var notesRec = new List<int>();
                        foreach(var n in nvm.Notifications)
                        {
                            notesRec.AddRange(n.NotesRecipients.Select(x => x.User.UserId));
                        }
                        notesRec = notesRec.Distinct().ToList();
                        nvm.UserTeamNames = GetListOfTeamManagersKeyValuePair(entityId, seasonId, notesRec).ToDictionary(x => x.Key, x => x.Value);
                        break;
                    case LogicaName.Union:
                        nvm.Notifications = notesRep.GetUnionMessages(seasonId.Value, entityId);
                        break;
                    case LogicaName.Discipline:
                        nvm.Notifications = notesRep.GetDisciplineMessages(seasonId.Value, entityId);
                        break;
                    case LogicaName.Club:
                        nvm.Notifications = notesRep.GetClubMessages(seasonId ?? currentSeasonId.Value, entityId);
                        break;
                }
            }
            if (nvm.Notifications != null)
            {
                nvm.Notifications = nvm.Notifications.FindAll(n => ((n.TypeId & MessageTypeEnum.NoPushNotify) == 0));
            }

            return PartialView("_List", nvm);
        }
    }
}