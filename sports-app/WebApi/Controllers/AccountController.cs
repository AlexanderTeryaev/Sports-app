using AppModel;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApi.Models;
using WebApi.Photo;
using Resources;
using WebApi.Services.Email;
using WebApi.Services;
using System.Web.Http.Description;
using DataService;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using DataService.DTO;
using System.Collections.Generic;
namespace WebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : BaseLogLigApiController
    {
        /// <summary>
        /// הרשמת אוהד עם שם משתמש וסיסמה
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        // POST api/Account/Register
        [AllowAnonymous]
        [Route("RegisterFan")]
        public async Task<IHttpActionResult> PostRegisterFan(FanRegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (db.Users.Where(u => u.UserName == model.UserName).Count() > 0)
            {
                return BadRequest(Messages.UsernameExists);
            }


            if (db.Users.Where(u => u.Email == model.Email).Count() > 0)
            {
                return BadRequest(Messages.EmailExists);
            }
            var lang = db.Languages.FirstOrDefault(x => x.Code == model.Language);

            var user = new User()
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = Protector.Encrypt(model.Password),
                UsersType = db.UsersTypes.FirstOrDefault(t => t.TypeRole == AppRole.Fans),
                IsActive = true,
                LangId = lang != null ? lang.LangId : 1
            };

            foreach (var item in model.Teams)
            {
                if (item.LeagueId == 0)
                {
                    TeamsRepo repo = new TeamsRepo();
                    item.LeagueId = repo.GetLeagueIdByTeamId(item.TeamId);
                }

                user.TeamsFans.Add(new TeamsFan
                {
                    TeamId = item.TeamId,
                    UserId = user.UserId,
                    LeageId = item.LeagueId
                });
            }


            var newUser = db.Users.Add(user);
            await db.SaveChangesAsync();

            if (newUser != null)
            {
                return Ok();
            }
            else
            {
                return InternalServerError();
            }
        }

        public static Image Crop(Image image, Rectangle selection)
        {
            Bitmap bmp = image as Bitmap;

            // Check if it is a bitmap:
            if (bmp == null)
                throw new ArgumentException("No valid bitmap");

            // Crop the image:
            Bitmap cropBmp = bmp.Clone(selection, bmp.PixelFormat);

            // Release the resources:
            image.Dispose();

            return cropBmp;
        }

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }


        /// <summary>
        /// העלאת תמונת פרופיל 
        /// </summary>
        /// <returns></returns>
        [Route("UploadProfilePicture")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PostUploadProfilePicture()
        {

            if (Request.Content.IsMimeMultipartContent("form-data"))
            {
                try
                {
                    User user = base.CurrentUser;
                    if (user != null)
                    {
                        string appPath = ConfigurationManager.AppSettings["ImageUrl"];
                        string fileName = DateTime.Now.Ticks.ToString() + ".jpeg";
                        string absoluteFolderPath = appPath + user.UserId;
                        string pathToFile = absoluteFolderPath + "\\" + fileName;

                        if (!Directory.Exists(absoluteFolderPath))
                        {
                            Directory.CreateDirectory(absoluteFolderPath);
                        }

                        var provider = new PhotoMultipartFormDataStreamProvider(absoluteFolderPath, fileName);
                        await Request.Content.ReadAsMultipartAsync(provider);

                        using (var image = Image.FromFile(pathToFile))
                        {
                            var size = image.Height >= image.Width ? image.Width : image.Height;
                            var paddingW = (image.Width - size) / 2;
                            var padding = (image.Height - size) / 2;
                            using (var newImage = Crop(image, new Rectangle(paddingW, padding, image.Width - paddingW*2, image.Height - padding*2)))
                            {
                                newImage.Save(absoluteFolderPath + "\\thumb_" + fileName, ImageFormat.Jpeg);
                            }
                        }

                        user.Image = user.UserId + "/thumb_" + fileName;
                        db.Entry(user).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        return Ok();
                    }
                    else
                    {
                        return BadRequest(Messages.UserNotFound);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(Messages.ErrorUploadingFile);
                }
            }
            else
            {
                return BadRequest(Messages.NoFileContent);
            }
        }
        [HttpPost]
        [Route("UploadMCFile")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PostUploadMCFile()
        {

            if (Request.Content.IsMimeMultipartContent("form-data"))
            {
                try
                {
                    User user = base.CurrentUser;
                    if (user != null)
                    {
                        HttpPostedFile file = HttpContext.Current.Request.Files["MCFile"];
                        var coll = HttpContext.Current.Request.Headers;
                        String fileExt = HttpContext.Current.Request.Params.GetValues("fileExt").FirstOrDefault()??"";
                        String s_id = HttpContext.Current.Request.Params.GetValues("seasonId").FirstOrDefault() ?? "";
                        int seasonId = int.Parse(s_id);
                        var Ext = "."+fileExt;
                        var newName = PlayerFileType.MedicalCertificate+ "_"+user.UserId.ToString()+"_"+DateTime.Now.ToString("ddMMyyyyHHmmssfff")+Ext;
                        if(newName == null)
                        {
                            return BadRequest(Messages.ErrorUploadingFile);
                        }
                        //var savePath = HttpContext.Current.Server.MapPath("players");
                        //var finalPath = savePath.Substring(0, savePath.Length - 27) + "\\CmsApp\\assets\\players\\";
                        //var finalPath = String.Concat(ConfigurationManager.AppSettings["SiteUrl"], "/assets/players/");
                        string finalPath = ConfigurationManager.AppSettings["ImageUrl"];
                        var di = new DirectoryInfo(finalPath);
                        if (!di.Exists)
                            di.Create();
                        var provider = new PhotoMultipartFormDataStreamProvider(finalPath, newName);
                        await Request.Content.ReadAsMultipartAsync(provider);
                        /*
                        using (var image = Image.FromFile(finalPath+ newName))
                        {
                            var size = image.Height >= image.Width ? image.Width : image.Height;
                            var paddingW = (image.Width - size) / 2;
                            var padding = (image.Height - size) / 2;
                            using (var newImage = Crop(image, new Rectangle(paddingW, padding, image.Width - paddingW * 2, image.Height - padding * 2)))
                            {
                                newImage.Save(finalPath + newName, ImageFormat.Jpeg);
                            }
                        }
                        */

                        /*
                        no need to delete old file because we archive it, also the files were moved to new table. 
                        
                        if (!string.IsNullOrEmpty(user.MedicalCertificateFile))
                        {
                            if (File.Exists(finalPath + user.MedicalCertificateFile))
                            {
                                File.Delete(finalPath + user.MedicalCertificateFile);
                            }
                        }
                        */
                        user.MedicalCertificateFile = newName;
                        user.MedicalCertificate = true;
                        db.Entry(user).State = EntityState.Modified;
                        

                        var activeCertFiles = db.PlayerFiles.Where(pf => pf.PlayerId == user.UserId && !pf.IsArchive && pf.SeasonId == seasonId);
                        await activeCertFiles.ForEachAsync(pf => { pf.IsArchive = true; });
                        var newCert = new PlayerFile { PlayerId = user.UserId, SeasonId = seasonId, FileType = (int)PlayerFileType.MedicalCertificate, DateCreated= DateTime.Now, FileName = newName };
                        db.PlayerFiles.Add(newCert);
                        await db.SaveChangesAsync();
                        return Ok(finalPath);
                    }
                    else
                    {
                        return BadRequest(Messages.UserNotFound);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(Messages.ErrorUploadingFile);
                }
            }
            else
            {
                return BadRequest(Messages.NoFileContent);
            }
        }

        /// <summary>
        /// הרשמת משתמש דרך פייסבוק
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        // POST api/Account/Register
        [AllowAnonymous]
        [Route("RegisterFanFB")]
        public IHttpActionResult RegisterFanFB(FBFanRegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (db.Users.Any(u => u.FbId == model.FbId))
            {
                return BadRequest(Messages.UsernameExists);
            }

            if (db.Users.Any(u => u.Email == model.Email))
            {
                return BadRequest(Messages.EmailExists);
            }

            var lang = db.Languages.FirstOrDefault(x => x.Code == model.Language);

            //TODO: we should get rid of "name"(FullName) from request and use First, Last and Middle names ASAP
            var playersRepo = new PlayersRepo(db);
            var firstName = playersRepo.GetFirstNameByFullName(model.FullName);
            var lastName = playersRepo.GetLastNameByFullName(model.FullName);

            var user = new User
            {
                UserName = model.UserName,
                FirstName = firstName,
                LastName = lastName,
                //FirstName = model.FirstName,
                //LastName = model.LastName,
                //MiddleName = model.MiddleName,
                Email = model.Email,
                Password = Protector.Encrypt("12345"),
                FbId = model.FbId,
                UsersType = db.UsersTypes.FirstOrDefault(t => t.TypeRole == AppRole.Fans),
                IsActive = true,
                Image = CreateFacebookProfilePictureUrl(model.FbId),
                LangId = lang?.LangId ?? 1,
            };

            foreach (var item in model.Teams)
            {
                if (item.LeagueId == 0)
                {
                    TeamsRepo repo = new TeamsRepo();
                    item.LeagueId = repo.GetLeagueIdByTeamId(item.TeamId);
                }
                user.TeamsFans.Add(new TeamsFan
                {
                    TeamId = item.TeamId,
                    UserId = user.UserId,
                    LeageId = item.LeagueId
                });
            }

            var newUser = db.Users.Add(user);
            db.SaveChanges();
            if (newUser != null)
            {
                return Ok();
            }
            else
            {
                return InternalServerError();
            }
        }

        /// <summary>
        /// הרשמת בעל תפקיד
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        // POST api/Account/Register
        [AllowAnonymous]
        [Route("RegisterWorker")]
        public async Task<IHttpActionResult> RegisterWorker(WorkerRegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usr = db.Users.FirstOrDefault(u => u.IdentNum == model.PersonalId);

            if (usr == null)
            {
                return BadRequest(Messages.UserNotExists);
            }

            usr.IsActive = true;

            db.Entry(usr).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// החזרת מידע על המשתמש הנוכחי
        /// </summary>
        /// <param name="unionId"></param>
        /// <returns></returns>
        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        [ResponseType(typeof(UserInfoViewModel))]
        public IHttpActionResult GetUserInfo(int? unionId = null)
        {
            var usr = CurrentUser;
            if (usr == null)
            {
                return NotFound();
            }

            SeasonsRepo seasonsRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                              (int?) null;

            var vm = new UserInfoViewModel
            {
                Id = usr.UserId,
                UserName = usr.UserName,
                Email = usr.Email,
                FullName = usr.FullName,
                Role = usr.UsersType.TypeRole,
                Image = usr.Image,
            };

            if (usr.UsersJobs != null && usr.UsersJobs.Count > 0)
            {
                //vm.UserJobs = usr.UsersJobs.Select(x => x.JobId).ToList();
                vm.UserJobs = usr.UsersJobs.Where(u => u.SeasonId == seasonId).OrderByDescending(u => u.ClubId).Select(x => new UserJobDetail()
                {
                    UserId = x.Id,
                    JobId = x.JobId,
                    JobName = (x.Job == null ? null : x.Job.JobName),
                    JobRoleId = (x.Job == null || x.Job.RoleId == null ? 0 : x.Job.RoleId??0),
                    JobRoleName = (x.Job == null || x.Job.JobsRole == null ? null : x.Job.JobsRole.RoleName),
                    JobRolePriority = (x.Job == null || x.Job.JobsRole == null ? 0 : x.Job.JobsRole.Priority),
                    LeagueId = x.LeagueId,
                    LeagueName = (x.League == null ? null : x.League.Name),
                    TeamId = x.TeamId,
                    ClubId = x.ClubId,
                    TeamName = (x.Team == null ? null : ( x.Team.TeamsDetails == null || x.Team.TeamsDetails.Count == 0 ? x.Team.Title : x.Team.TeamsDetails.FirstOrDefault().TeamName))
                }).ToList();
            }

            var teamsRepo = new TeamsRepo();
            bool bWorker = false;
            if (User.IsInRole(AppRole.Fans))
            {
                vm.Teams = teamsRepo.GetFanTeams(usr.UserId, seasonId);
                if (vm.Teams.Count == 0)
                {
                    List<TeamDto> fanTeams = new List<TeamDto>();
                    foreach (TeamsFan teamsfan in usr.TeamsFans)
                    {
                        TeamsService ts = new TeamsService();
                        TeamsRepo tr = new TeamsRepo();

                        Team tt = ts.GetTeamById(teamsfan.TeamId);
                        TeamDto Team = new TeamDto()
                        {
                            LeagueId = teamsfan.LeageId,
                            TeamId = tt.TeamId,
                            Logo = tt.Logo,
                            Title = (tt.TeamsDetails == null && tt.TeamsDetails.Count == 0)||tt.TeamsDetails.Count()==0 ? tt.Title : tt.TeamsDetails.FirstOrDefault()?.TeamName,
                            SeasonId = tr.GetSeasonIdByTeamId(tt.TeamId)
                        };
                        fanTeams.Add(Team);
                    }
                    vm.Teams = fanTeams;
                }
            }
            else if (User.IsInRole(AppRole.Players))
            {
                vm.Teams = teamsRepo.GetPlayerTeams(usr.UserId, seasonId);
                // If player, get image of profile from dbo.playerfiles
                PlayerProfileViewModel ppv = PlayerService.GetPlayerProfile(usr, seasonId);
                if (vm.Image == null)
                {
                    vm.Image = ppv.Image;
                }
                if(vm.Teams.Count() == 0 && usr.UsersJobs.Count() > 0)
                {
                    bWorker = true;
                }
            }

            if (User.IsInRole(AppRole.Workers) || bWorker == true)
            {
                vm.Teams = usr.UsersJobs.Select(x => new DataService.DTO.TeamDto()
                {
                    TeamId = (x.Team == null ? 0 : x.Team.TeamId),
                    Title = (x.Team == null ? null : (x.Team.TeamsDetails == null || x.Team.TeamsDetails.Count == 0 ? x.Team.Title : x.Team.TeamsDetails.FirstOrDefault().TeamName)),
                    LeagueId = (x.LeagueId == null ? 0 : x.LeagueId??0),
                    Logo = (x.Team == null ? null : x.Team.Logo),
                    SeasonId = (x.SeasonId == null ? 0 : x.SeasonId)
                }).ToList();
            }

            return Ok(vm);
        }

        /// <summary>
        /// החזרת מידע על המשתמש הנוכחי
        /// </summary>
        /// <param name="unionId"></param>
        /// <returns></returns>
        // GET api/Account/ClubUserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("ClubUserInfo")]
        [ResponseType(typeof(UserInfoViewModel))]
        public IHttpActionResult ClubUserInfo(int? unionId = null)
        {
            var usr = CurrentUser;
            if (usr == null)
            {
                return NotFound();
            }

            SeasonsRepo seasonsRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                              (int?)null;
            //get medical certfile
            var MCertificationFile = "";
            int seasonId_tennis = 0;
            if (unionId == 38)
            {
                //if tennis
                var srepo = new SeasonsRepo();
                seasonId_tennis = srepo.GetCurrentByUnionId(38)??0;
                var usersRepo = new UsersRepo();
                var player = usersRepo.GetById(CurrentUser.UserId);
                var PlayerFiles = new List<String>();
                foreach (var playerFile in player.PlayerFiles.Where(x => x.SeasonId == seasonId_tennis))
                {
                    if (playerFile.FileType == (int)PlayerFileType.MedicalCertificate)
                    {
                        PlayerFiles.Add(playerFile.FileName);
                    }
                }
                MCertificationFile = player.MedicalCertificateFile == null || player.MedicalCertificateFile.CompareTo("System.Web.HttpPostedFileWrapper")==0 ? PlayerFiles.FirstOrDefault() ?? ""
                    :player.MedicalCertificateFile;
            }
            //end medical certfile
            var vm = new UserInfoViewModel
            {
                Id = usr.UserId,
                seasonId = unionId == 38? seasonId_tennis:seasonId,
                UserName = usr.UserName,
                Email = usr.Email,
                FullName = usr.FullName,
                Role = usr.UsersType.TypeRole,
                Image = usr.Image,
                MCertification = usr.MedicalCertificate ?? false,
                MCertificationFile = MCertificationFile,
                IsApproved = db.TeamsPlayers.Where(x=>x.UserId == usr.UserId && x.SeasonId == seasonId)?.FirstOrDefault()?.IsApprovedByManager??false,
                TenicardValidity = db.Users.Where(x=>x.UserId == CurrUserId).FirstOrDefault()?.TenicardValidity == null?false: 
                (db.Users.Where(x => x.UserId == CurrUserId).FirstOrDefault()?.TenicardValidity >=DateTime.Now)?true:false
            };

            if(vm.UserName == null)
            {
                vm.UserName = vm.FullName;
            }

            if (usr.UsersType.TypeRole == "players")
            {
                PlayerProfileViewModel ppv = PlayerService.GetPlayerProfile(usr, seasonId);
                if(vm.Image == null)
                {
                    vm.Image = ppv.Image;
                }
            }

            if (usr.UsersJobs != null && usr.UsersJobs.Count > 0)
            {
                vm.UserJobs = usr.UsersJobs.Select(x => new UserJobDetail()
                //vm.UserJobs = usr.UsersJobs.Where(x=>x.SeasonId == seasonId).Select(x => new UserJobDetail()
                {
                    UserId = x.Id,
                    JobId = x.JobId,
                    JobName = (x.Job == null ? null : x.Job.JobName),
                    JobRoleId = (x.Job == null || x.Job.RoleId == null ? 0 : x.Job.RoleId??0),
                    JobRoleName = (x.Job == null || x.Job.JobsRole == null ? null : x.Job.JobsRole.RoleName),
                    JobRolePriority = (x.Job == null || x.Job.JobsRole == null ? 0 : x.Job.JobsRole.Priority),
                    LeagueId = x.LeagueId,
                    LeagueName = (x.League == null ? null : x.League.Name),
                    TeamId = x.TeamId,
                    ClubId = x.ClubId,
                    TeamName = (x.Team == null ? null : (x.Team.TeamsDetails == null || x.Team.TeamsDetails.Count == 0 ? x.Team.Title : x.Team.TeamsDetails.FirstOrDefault().TeamName))
                }).ToList();
            }

            var teamsRepo = new TeamsRepo();
            if (User.IsInRole(AppRole.Fans))
            {
                /*
                if(unionId == 15)
                {
                    vm.Teams = teamsRepo.GetFanTeamsByClub(usr.UserId, seasonId);
                }
                else
                {
                    vm.Teams = teamsRepo.GetFanTeams(usr.UserId, seasonId);
                }
                */
                vm.Teams = teamsRepo.GetFanTeams(usr.UserId, seasonId);
                if (vm.Teams.Count == 0)
                {
                    List<TeamDto> fanTeams = new List<TeamDto>();
                    foreach (TeamsFan teamsfan in usr.TeamsFans)
                    {
                        TeamsService ts = new TeamsService();
                        TeamsRepo tr = new TeamsRepo();

                        Team tt = ts.GetTeamById(teamsfan.TeamId);

                        var schoolTeam = db.SchoolTeams.Where(st => st.TeamId == teamsfan.TeamId).FirstOrDefault();
                        var isSchoolTeam = false;
                        if (schoolTeam != null)
                        {
                            isSchoolTeam = true;
                        }

                        var clubs = db.ClubTeams.Where(x => x.TeamId == teamsfan.TeamId && x.SeasonId == seasonId).ToList();
                        foreach(var club in clubs)
                        {
                            TeamDto Team = new TeamDto()
                            {
                                LeagueId = teamsfan.LeageId,
                                TeamId = tt.TeamId,
                                Logo = tt.Logo,
                                Title = tt.Title,
                                SeasonId = tr.GetSeasonIdByTeamId(tt.TeamId),
                                IsSchoolTeam = isSchoolTeam,
                                ClubId = club.ClubId
                            };
                            fanTeams.Add(Team);
                        }
                    }
                    vm.Teams = (fanTeams);
                }
            }
            else if (User.IsInRole(AppRole.Players))
            {
                if(unionId == 38)
                {
                    //if tennis
                    vm.Teams = teamsRepo.GetPlayerTeamsTennis(usr.UserId);
                }
                else
                {
                    vm.Teams = teamsRepo.GetPlayerTeamsInActiveSeasons(usr.UserId);
                }
                // vm.Teams = teamsRepo.GetClubPlayerTeams(usr.UserId, seasonId);
                for (int i = 0; i < vm.Teams.Count; i++)
                {
                    var teamId = vm.Teams[i].TeamId;

                    var clubInfo = db.ClubTeams.Where(ct => ct.TeamId == teamId && ct.SeasonId == seasonId).FirstOrDefault();
                    if (clubInfo != null)
                    {
                        vm.Teams[i].IsTrainingTeam = clubInfo.IsTrainingTeam;
                    }
                    else
                    {
                        vm.Teams[i].IsTrainingTeam = false;
                    }
                    if (vm.Teams[i].LeagueId == 0)
                    {
                        var gamesDb = db.GamesCycles.Where(g => g.HomeTeamId == teamId && g.Stage != null && g.Stage.League != null && g.Stage.League.SeasonId == seasonId).FirstOrDefault();
                        if (gamesDb != null && gamesDb.Stage != null)
                        {
                            vm.Teams[i].LeagueId = gamesDb.Stage.LeagueId;
                        }
                        else
                        {
                            var LeagueId = db.TeamsPlayers.Where(tp => tp.UserId == usr.UserId).Select(tp => tp.LeagueId).FirstOrDefault();
                            if (LeagueId > 0) vm.Teams[i].LeagueId = LeagueId.Value;
                        }
                    }

                    var schoolTeam = db.SchoolTeams.Where(st => st.TeamId == teamId).FirstOrDefault();
                    if (schoolTeam != null)
                    {
                        vm.Teams[i].IsSchoolTeam = true;
                    }
                    else
                    {
                        vm.Teams[i].IsSchoolTeam = false;
                    }
                }
            }
            else if (User.IsInRole(AppRole.Workers))
            {
                for (int j = 0; j < usr.UsersJobs.Count(); j++)
                {
                    if (usr.UsersJobs.ElementAt(j).Job.JobsRole.RoleName == JobRole.TeamManager || usr.UsersJobs.ElementAt(j).Job.JobsRole.RoleName == JobRole.ClubManager
                        || usr.UsersJobs.ElementAt(j).Job.JobsRole.RoleName == JobRole.DepartmentManager || usr.UsersJobs.ElementAt(j).Job.JobsRole.RoleName == JobRole.LeagueManager)
                    {
                        vm.Teams = teamsRepo.GetByTeamManagerId(usr.UserId).Select(x => new DataService.DTO.TeamDto()
                        {
                            TeamId = (x.TeamId == null ? 0 : x.TeamId.Value),
                            Title = x.Title,
                            LeagueId = (x.LeagueId == null ? 0 : x.LeagueId.Value),
                            Logo = teamsRepo.GetById(x.TeamId)?.Logo,
                            SeasonId = (x.SeasonId == null ? 0 : x.SeasonId),
                            ClubId = (x.ClubId == null ? 0 : x.ClubId.Value)
                        }).ToList();

                        // vm.Teams = teamsRepo.GetClubPlayerTeams(usr.UserId, seasonId);
                        var jobTeams = usr.UsersJobs.Where(x => x.SeasonId == seasonId).Select(x => new DataService.DTO.TeamDto()
                        {
                            TeamId = (x.Team == null ? 0 : x.Team.TeamId),
                            Title = (x.Team == null ? null : (x.Team.TeamsDetails == null || x.Team.TeamsDetails.Count == 0 ? x.Team.Title : x.Team.TeamsDetails.FirstOrDefault().TeamName)),
                            LeagueId = (x.LeagueId == null ? 0 : x.LeagueId??0),
                            Logo = (x.Team == null ? null : x.Team?.Logo),
                            SeasonId = (x.SeasonId == null ? 0 : x.SeasonId),
                            ClubId = (x.Club == null && x.Team != null ? x.Team.ClubTeams.Where(t => t.TeamId == x.Team.TeamId).Select(ct => ct.ClubId).FirstOrDefault() : x.ClubId ?? 0)
                        }).ToList();

                        if (seasonId == null)
                        {
                            jobTeams = usr.UsersJobs.Select(x => new DataService.DTO.TeamDto()
                            {
                                TeamId = (x.Team == null ? 0 : x.Team.TeamId),
                                Title = (x.Team == null ? null : (x.Team.TeamsDetails == null || x.Team.TeamsDetails.Count == 0 ? x.Team.Title : x.Team.TeamsDetails.FirstOrDefault().TeamName)),
                                LeagueId = (x.LeagueId == null ? 0 : x.LeagueId??0),
                                Logo = (x.Team == null ? null : x.Team.Logo),
                                SeasonId = (x.SeasonId == null ? 0 : x.SeasonId),
                                ClubId = (x.Club == null && x.Team != null ? x.Team.ClubTeams.Where(t => t.TeamId == x.Team.TeamId).Select(ct => ct.ClubId).FirstOrDefault() : x.ClubId ?? 0)
                            }).ToList();
                        }

                        for (int i = 0; i < jobTeams.Count; i++)
                        {
                            if (jobTeams[i].TeamId == 0)
                            {
                                jobTeams.RemoveAt(i);
                                i--;
                            }
                        }

                        for (int i = 0; i < jobTeams.Count; i++)
                        {
                            //if (vm.Teams.Where(x => x.TeamId == jobTeams[i].TeamId) == null)
                            /** 
                             *   if vm.Teams.Count is 0, vm.Teams.Where == null not work.
                             */

                            /** Cheng Li.
                             * If length is 0 or null append team.
                            */
                            var ts = vm.Teams.Where(x => x.TeamId == jobTeams[i].TeamId).ToList();
                            if (vm.Teams.Count == 0 || ts == null || ts.Count == 0)
                            //if (vm.Teams.Count == 0 || vm.Teams.Where(x => x.TeamId == jobTeams[i].TeamId) == null)
                            {
                                vm.Teams.Add(jobTeams[i]);
                            }
                        }

                        for (int i = 0; i < vm.Teams.Count; i++)
                        {
                            var teamId = vm.Teams[i].TeamId;

                            var clubInfo = db.ClubTeams.Where(ct => ct.TeamId == teamId).FirstOrDefault();
                            if (clubInfo != null)
                            {
                                vm.Teams[i].IsTrainingTeam = clubInfo.IsTrainingTeam;
                            }
                            else
                            {
                                vm.Teams[i].IsTrainingTeam = false;
                            }
                            var gamesDb = db.GamesCycles.Where(g => g.HomeTeamId == teamId && g.Stage != null && g.Stage.League != null && g.Stage.League.SeasonId == seasonId).FirstOrDefault();
                            if (vm.Teams[i].LeagueId == 0)
                            {
                                if (gamesDb != null && gamesDb.Stage != null)
                                {
                                    vm.Teams[i].LeagueId = gamesDb.Stage.LeagueId;
                                }
                                else
                                {
                                    var LeagueId = db.TeamsPlayers.Where(tp => tp.UserId == usr.UserId && tp.SeasonId == seasonId).Select(tp => tp.LeagueId).FirstOrDefault();
                                    if (LeagueId > 0) vm.Teams[i].LeagueId = LeagueId.Value;
                                }
                            }
                            var schoolTeam = db.SchoolTeams.Where(st => st.TeamId == teamId).FirstOrDefault();

                            if (schoolTeam != null)
                            {
                                vm.Teams[i].IsSchoolTeam = true;
                            }
                            else
                            {
                                vm.Teams[i].IsSchoolTeam = false;
                            }
                        }

                        break;
                    }
                }
                /*if (usr.UsersJobs.First().Job.JobsRole.RoleName == JobRole.TeamManager || usr.UsersJobs.First().Job.JobsRole.RoleName == JobRole.ClubManager)
                {
                    
                }*/
            }

            if(vm.Teams != null && vm.Teams.Count > 0)
                vm.Teams = vm.Teams.OrderByDescending(l => l.LeagueId).ToList();

            return Ok(vm);
        }
        // GET api/Account/ClubUserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("ClubUserInfoOfClub")]
        [ResponseType(typeof(UserInfoViewModel))]
        public IHttpActionResult ClubUserInfoOfClub(int? unionId = null)
        {
            var usr = CurrentUser;
            if (usr == null)
            {
                return NotFound();
            }

            SeasonsRepo seasonsRepo = new SeasonsRepo();
            //var seasonIdo = (from tps in db.TeamsPlayers
            //                 from clubs in db.Clubs
            //                 from ss in db.Seasons
            //                 where (tps.UserId == CurrUserId && tps.ClubId == clubs.ClubId && ss.ClubId == clubs.ClubId &&
            //                 ss.StartDate < DateTime.Now && ss.EndDate > DateTime.Now)
            //                 select new
            //                 {
            //                     id = tps.SeasonId
            //                 }).FirstOrDefault();
            //int? seasonId = CurrentUser.TeamsPlayers.Where(x=>x.Seas)
            //if (seasonId == null)
            //{
            //    seasonId = db.TeamsFans.Where(x =>x.League!=null&& x.League.Season!=null&& x.League.Season.StartDate < DateTime.Now && x.League.Season.EndDate > DateTime.Now)
            //    .Select(x => x.League.SeasonId).FirstOrDefault().Value;
            //    if(seasonId == null)
            //    {
            //        seasonId = db.UsersJobs.Where(x => x.UserId == CurrUserId &&x.Season!=null && x.Season.StartDate < DateTime.Now && x.Season.EndDate > DateTime.Now)
            //            .FirstOrDefault().SeasonId;
            //    }
            //}
            int? seasonId = CurrentUser.TeamsPlayers?.OrderByDescending(x => x.SeasonId)?.FirstOrDefault()?.SeasonId;
            if(seasonId == null)
            {
                seasonId = CurrentUser.TeamsFans?.OrderByDescending(x => x.League?.Club?.SeasonId)?.FirstOrDefault()?.League?.Club?.SeasonId;
            }
            var vm = new UserInfoViewModel
            {
                Id = usr.UserId,
                UserName = usr.UserName,
                Email = usr.Email,
                FullName = usr.FullName,
                Role = usr.UsersType.TypeRole,
                Image = usr.Image,
            };

            if (vm.UserName == null)
            {
                vm.UserName = vm.FullName;
            }

            if (usr.UsersType.TypeRole == "players")
            {
                PlayerProfileViewModel ppv = PlayerService.GetPlayerProfile(usr, seasonId);
                if (vm.Image == null)
                {
                    vm.Image = ppv.Image;
                }
            }

            if (usr.UsersJobs != null && usr.UsersJobs.Count > 0)
            {
                vm.UserJobs = usr.UsersJobs.Select(x => new UserJobDetail()
                //vm.UserJobs = usr.UsersJobs.Where(x=>x.SeasonId == seasonId).Select(x => new UserJobDetail()
                {
                    UserId = x.Id,
                    JobId = x.JobId,
                    JobName = (x.Job == null ? null : x.Job.JobName),
                    JobRoleId = (x.Job == null || x.Job.RoleId == null ? 0 : x.Job.RoleId??0),
                    JobRoleName = (x.Job == null || x.Job.JobsRole == null ? null : x.Job.JobsRole.RoleName),
                    JobRolePriority = (x.Job == null || x.Job.JobsRole == null ? 0 : x.Job.JobsRole.Priority),
                    LeagueId = x.LeagueId,
                    LeagueName = (x.League == null ? null : x.League.Name),
                    TeamId = x.TeamId,
                    ClubId = x.ClubId,
                    TeamName = (x.Team == null ? null : (x.Team.TeamsDetails == null || x.Team.TeamsDetails.Count == 0 ? x.Team.Title : x.Team.TeamsDetails.FirstOrDefault().TeamName))
                }).ToList();
            }

            var teamsRepo = new TeamsRepo();
            if (User.IsInRole(AppRole.Fans))
            {
                vm.Teams = teamsRepo.GetFanTeamsByClub(usr.UserId, seasonId);
                
                if (vm.Teams.Count == 0)
                {
                    List<TeamDto> fanTeams = new List<TeamDto>();
                    foreach (TeamsFan teamsfan in usr.TeamsFans)
                    {
                        TeamsService ts = new TeamsService();
                        TeamsRepo tr = new TeamsRepo();

                        Team tt = ts.GetTeamById(teamsfan.TeamId);

                        var schoolTeam = db.SchoolTeams.Where(st => st.TeamId == teamsfan.TeamId).FirstOrDefault();
                        var isSchoolTeam = false;
                        if (schoolTeam != null)
                        {
                            isSchoolTeam = true;
                        }

                        var clubs = db.ClubTeams.Where(x => x.TeamId == teamsfan.TeamId && x.SeasonId == seasonId).ToList();
                        foreach (var club in clubs)
                        {
                            TeamDto Team = new TeamDto()
                            {
                                LeagueId = teamsfan.LeageId,
                                TeamId = tt.TeamId,
                                Logo = tt.Logo,
                                Title = tt.Title,
                                SeasonId = tr.GetSeasonIdByTeamId(tt.TeamId),
                                IsSchoolTeam = isSchoolTeam,
                                ClubId = club.ClubId
                            };
                            fanTeams.Add(Team);
                        }
                    }
                    vm.Teams = (fanTeams);
                }
            }
            else if (User.IsInRole(AppRole.Players))
            {
                if (unionId == 38)
                {
                    //if tennis
                    vm.Teams = teamsRepo.GetPlayerTeamsTennis(usr.UserId);
                }
                else
                {
                    vm.Teams = teamsRepo.GetPlayerTeams(usr.UserId);
                }
                // vm.Teams = teamsRepo.GetClubPlayerTeams(usr.UserId, seasonId);
                for (int i = 0; i < vm.Teams.Count; i++)
                {
                    var teamId = vm.Teams[i].TeamId;

                    var clubInfo = db.ClubTeams.Where(ct => ct.TeamId == teamId && ct.SeasonId == seasonId).FirstOrDefault();
                    if (clubInfo != null)
                    {
                        vm.Teams[i].IsTrainingTeam = clubInfo.IsTrainingTeam;
                    }
                    else
                    {
                        vm.Teams[i].IsTrainingTeam = false;
                    }
                    var gamesDb = db.GamesCycles.Where(g => g.HomeTeamId == teamId && g.Stage != null && g.Stage.League != null && g.Stage.League.SeasonId == seasonId).FirstOrDefault();
                    if (gamesDb != null && gamesDb.Stage != null)
                    {
                        vm.Teams[i].LeagueId = gamesDb.Stage.LeagueId;
                    }
                    else
                    {
                        var LeagueId = db.TeamsPlayers.Where(tp => tp.UserId == usr.UserId).Select(tp => tp.LeagueId).FirstOrDefault();
                        if (LeagueId > 0) vm.Teams[i].LeagueId = LeagueId.Value;
                    }

                    var schoolTeam = db.SchoolTeams.Where(st => st.TeamId == teamId).FirstOrDefault();
                    if (schoolTeam != null)
                    {
                        vm.Teams[i].IsSchoolTeam = true;
                    }
                    else
                    {
                        vm.Teams[i].IsSchoolTeam = false;
                    }
                }
            }
            else if (User.IsInRole(AppRole.Workers))
            {
                for (int j = 0; j < usr.UsersJobs.Count(); j++)
                {
                    if (usr.UsersJobs.ElementAt(j).Job.JobsRole.RoleName == JobRole.TeamManager || usr.UsersJobs.ElementAt(j).Job.JobsRole.RoleName == JobRole.ClubManager
                        || usr.UsersJobs.ElementAt(j).Job.JobsRole.RoleName == JobRole.DepartmentManager || usr.UsersJobs.ElementAt(j).Job.JobsRole.RoleName == JobRole.LeagueManager)
                    {
                        seasonId = usr.UsersJobs.ElementAt(j).SeasonId;
                        vm.Teams = teamsRepo.GetByTeamManagerId(usr.UserId).Select(x => new DataService.DTO.TeamDto()
                        {
                            TeamId = (x.TeamId == null ? 0 : x.TeamId??0),
                            Title = x.Title,
                            LeagueId = (x.LeagueId == null ? 0 : x.LeagueId??0),
                            Logo = teamsRepo.GetById(x.TeamId??0)?.Logo,
                            SeasonId = (x.SeasonId == null ? 0 : x.SeasonId??0),
                            ClubId = (x.ClubId == null ? 0 : x.ClubId??0)
                        }).ToList();

                        // vm.Teams = teamsRepo.GetClubPlayerTeams(usr.UserId, seasonId);
                        var jobTeams = usr.UsersJobs.Where(x => x.SeasonId == seasonId).Select(x => new DataService.DTO.TeamDto()
                        {
                            TeamId = (x.Team == null ? 0 : x.Team.TeamId),
                            Title = (x.Team == null ? null : (x.Team.TeamsDetails == null || x.Team.TeamsDetails.Count == 0 ? x.Team.Title : x.Team.TeamsDetails.FirstOrDefault().TeamName)),
                            LeagueId = (x.LeagueId == null ? 0 : x.LeagueId ?? 0),
                            Logo = (x.Team == null ? null : x.Team.Logo),
                            SeasonId = x.SeasonId ?? 0,
                            ClubId = x.ClubId == null? x.Team?.ClubTeams.Where(t => t.TeamId == x.Team.TeamId).FirstOrDefault()?.ClubId??0 : x.ClubId ?? 0
                        }).ToList();

                        if (seasonId == null)
                        {
                            jobTeams = usr.UsersJobs.Select(x => new DataService.DTO.TeamDto()
                            {
                                TeamId = (x.Team == null ? 0 : x.Team.TeamId),
                                Title = (x.Team == null ? null : (x.Team.TeamsDetails == null || x.Team.TeamsDetails.Count == 0 ? x.Team.Title : x.Team.TeamsDetails.FirstOrDefault().TeamName)),
                                LeagueId = (x.LeagueId == null ? 0 : x.LeagueId??0),
                                Logo = (x.Team == null ? null : x.Team.Logo),
                                SeasonId = (x.SeasonId == null ? 0 : x.SeasonId),
                                ClubId = x.Club == null ? x.Team?.ClubTeams.Where(t => t.TeamId == x.Team.TeamId).FirstOrDefault()?.ClubId??0 : x.ClubId??0
                            }).ToList();
                        }

                        for (int i = 0; i < jobTeams.Count; i++)
                        {
                            if (jobTeams[i].TeamId == 0)
                            {
                                jobTeams.RemoveAt(i);
                                i--;
                            }
                        }

                        for (int i = 0; i < jobTeams.Count; i++)
                        {
                            //if (vm.Teams.Where(x => x.TeamId == jobTeams[i].TeamId) == null)
                            /** 
                             *   if vm.Teams.Count is 0, vm.Teams.Where == null not work.
                             */

                            /** Cheng Li.
                             * If length is 0 or null append team.
                            */
                            var ts = vm.Teams.Where(x => x.TeamId == jobTeams[i].TeamId).ToList();
                            if (vm.Teams.Count == 0 || ts == null || ts.Count == 0)
                            //if (vm.Teams.Count == 0 || vm.Teams.Where(x => x.TeamId == jobTeams[i].TeamId) == null)
                            {
                                vm.Teams.Add(jobTeams[i]);
                            }
                        }

                        for (int i = 0; i < vm.Teams.Count; i++)
                        {
                            var teamId = vm.Teams[i].TeamId;

                            var clubInfo = db.ClubTeams.Where(ct => ct.TeamId == teamId).FirstOrDefault();
                            if (clubInfo != null)
                            {
                                vm.Teams[i].IsTrainingTeam = clubInfo.IsTrainingTeam;
                            }
                            else
                            {
                                vm.Teams[i].IsTrainingTeam = false;
                            }
                            var gamesDb = db.GamesCycles.Where(g => g.HomeTeamId == teamId && g.Stage != null && g.Stage.League != null && g.Stage.League.SeasonId == seasonId).FirstOrDefault();
                            if (gamesDb != null && gamesDb.Stage != null)
                            {
                                vm.Teams[i].LeagueId = gamesDb.Stage.LeagueId;
                            }
                            else
                            {
                                var LeagueId = db.TeamsPlayers.Where(tp => tp.UserId == usr.UserId && tp.SeasonId == seasonId).Select(tp => tp.LeagueId).FirstOrDefault();
                                if (LeagueId > 0) vm.Teams[i].LeagueId = LeagueId.Value;
                            }

                            var schoolTeam = db.SchoolTeams.Where(st => st.TeamId == teamId).FirstOrDefault();

                            if (schoolTeam != null)
                            {
                                vm.Teams[i].IsSchoolTeam = true;
                            }
                            else
                            {
                                vm.Teams[i].IsSchoolTeam = false;
                            }
                        }

                        break;
                    }
                }
                /*if (usr.UsersJobs.First().Job.JobsRole.RoleName == JobRole.TeamManager || usr.UsersJobs.First().Job.JobsRole.RoleName == JobRole.ClubManager)
                {
                    
                }*/
            }

            if (vm.Teams != null && vm.Teams.Count > 0)
                vm.Teams = vm.Teams.OrderByDescending(l => l.LeagueId).ToList();

            return Ok(vm);
        }

        /// <summary>
        /// מחזיר אם חשבון פייסבוק קיים במערכת
        /// </summary>
        /// <param name="facebookId"></param>
        /// <returns></returns>
        // GET api/Account/FBAccounExists/{facebookId}
        [Route("FBAccountExists/{facebookId}")]
        [AllowAnonymous]
        public ExistsResponse GetFBAccountExists(string facebookId)
        {
            return new ExistsResponse { Exists = db.Users.Any(u => u.FbId.Trim().ToLower() == facebookId.Trim().ToLower()) };
        }

        public class UserMail
        {
            public string Mail { get; set; }
            public string UserId { get; set; }
        }
        public class ChatMail
        {
            public string Sender { get; set; } //imagegallery=>src
            public DateTime Date { get; set; }
            public string Message { get; set; } //imagegallery=>subscription
            public int SenderId { get; set; }
            public string ImgName { get; set; }
            public string ParentName { get; set; }
            public string Type { get; set; }
            public int ParentId { get; set; }
            // Cheng Li. : Add url of image, video
            public string ImgUrl { get; set; }
            public string VideoUrl { get; set; }

        }

        /// <summary>
        /// שולח את הסיסימה למשתמש דרך אימייל 
        /// </summary>
        /// <returns></returns>
        [Route("ForgotPassword")]
        [AllowAnonymous]
        [HttpPost]
        public IHttpActionResult GetForgotPassword(UserMail userMail)
        {
            if (userMail == null || string.IsNullOrEmpty(userMail.Mail) || string.IsNullOrWhiteSpace(userMail.Mail) || string.IsNullOrEmpty(userMail.UserId) || string.IsNullOrWhiteSpace(userMail.UserId))
            {
                return NotFound();
            }

            var user = db.Users.FirstOrDefault(u => u.Email.Trim().ToLower() == userMail.Mail.Trim().ToLower() && u.IdentNum == userMail.UserId);
            if (user == null)
            {
                return NotFound();
            }
            EmailService emailService = new EmailService();
            ForgotPasswordEmailModel model = new ForgotPasswordEmailModel
            {
                Name = user.UserName,
                Password = Protector.Decrypt(user.Password)
            };

            var templateService = new RazorEngine.Templating.TemplateService();
            var templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "Email", "ForgotPasswordEmailTemplate.cshtml");
            var emailHtmlBody = templateService.Parse(File.ReadAllText(templateFilePath), model, null, null);

            try
            {
                IdentityMessage msg = new IdentityMessage();
                msg.Subject = "Loglig";
                msg.Body = emailHtmlBody;
                msg.Destination = user.Email;
                emailService.SendAsync(msg);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
            return Ok();
        }

        [Route("ReportChatMessage")]
        [AllowAnonymous]
        [HttpPost]
        public IHttpActionResult ReportChatMessage(ChatMail chatMail)
        {
            EmailService emailService = new EmailService();
            string appPath = ConfigurationManager.AppSettings["ChatUrl"];
            ChatEmailModel model = new ChatEmailModel
            {
                Title = "Report",
                Name = CurrentUser.UserName + CurrentUser.FullName,
                Sender = chatMail.Sender,
                Date = chatMail.Date,
                Message = chatMail.Message,
                ImgUrl = chatMail.ImgUrl == null || chatMail.ImgUrl.Length == 0 ? "None": appPath + "\\" + chatMail.ImgUrl,
                VideoUrl = chatMail.VideoUrl == null || chatMail.VideoUrl.Length == 0 ? "None" : appPath + "\\" + chatMail.VideoUrl
            };

            var templateService = new RazorEngine.Templating.TemplateService();
            var templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "Email", "ChatEmailTemplate.cshtml");
            var emailHtmlBody = templateService.Parse(File.ReadAllText(templateFilePath), model, null, null);

            try
            {
                IdentityMessage msg = new IdentityMessage();
                msg.Subject = "Loglig Chat Message";
                msg.Body = emailHtmlBody;
                msg.Destination = "info@loglig.com";
                emailService.SendAsync(msg);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
            return Ok();
        }

        [Route("ReportImageGallery")]
        [AllowAnonymous]
        [HttpPost]
        public IHttpActionResult ReportImageGallery(ChatMail chatMail)
        {
            EmailService emailService = new EmailService();
            ChatEmailModel model = new ChatEmailModel
            {
                Name = CurrentUser.UserName + CurrentUser.FullName,
                Sender = chatMail.Sender,
                Date = chatMail.Date,
                SenderId = chatMail.SenderId,
                ImgName = chatMail.ImgName,
                ParentId = chatMail.ParentId,
                ParentName = chatMail.ParentName,
                Type = chatMail.Type,
                Message = chatMail.Message
            };

            var templateService = new RazorEngine.Templating.TemplateService();
            var templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "Email", "ImageGalleryEmailTemplate.cshtml");
            var emailHtmlBody = templateService.Parse(File.ReadAllText(templateFilePath), model, null, null);

            try
            {
                IdentityMessage msg = new IdentityMessage();
                msg.Subject = "Loglig Chat Message";
                msg.Body = emailHtmlBody;
                msg.Destination = "info@loglig.com";
                emailService.SendAsync(msg);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
            return Ok();
        }

        /// <summary>
        /// עדכון פרטי משתמש 
        /// StartAlert תזכורת יום לפני המשחק
        /// TimeChange שינוי מועד משחק
        /// GameScores תוצאות משחק עם סיומו
        /// GameRecords שיאים לאחר משחק
        /// </summary>
        /// <returns></returns>
        [Route("Update")]
        public IHttpActionResult PostUpdateUser(UserDetails model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("נתון לא תקין");
            }

            var user = base.CurrentUser;

            if (!string.IsNullOrEmpty(model.OldPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                string pass = Protector.Encrypt(model.OldPassword);

                if (user.Password != pass)
                {
                    return BadRequest("סיסמה שגוייה");
                }

                user.Password = Protector.Encrypt(model.NewPassword);
            }
            var lang = db.Languages.FirstOrDefault(x => x.Code == model.Language);
            if (!string.IsNullOrEmpty(model.Email))
            {
                user.Email = model.Email;
            }


            //TODO: we should get rid of "FullName" from request and use First, Last and Middle names ASAP
            var playersRepo = new PlayersRepo(db);
            var firstName = model.FullName == null
                ? null
                : playersRepo.GetFirstNameByFullName(model.FullName);
            var lastName = model.FullName == null
                ? null
                : playersRepo.GetLastNameByFullName(model.FullName);

            user.FirstName = firstName ?? user.FirstName;
            user.LastName = lastName ?? user.LastName;
            //user.MiddleName = model.MiddleName ?? user.MiddleName; //TODO

            user.UserName = model.UserName ?? user.UserName;
            user.LangId = lang?.LangId ?? user.LangId;

            if (model.Teams != null && model.Teams.Any())
            {
                db.TeamsFans.RemoveRange(user.TeamsFans);
                user.TeamsFans.Clear();

                foreach (var t in model.Teams)
                {
                    var team = db.Teams.FirstOrDefault(x => x.TeamId == t.TeamId);
                    var league = db.Leagues.FirstOrDefault(x => x.LeagueId == t.LeagueId);
                    if(league == null)
                    {
                        league = db.LeagueTeams.FirstOrDefault(x => x.TeamId == team.TeamId)?.Leagues;
                    }
                    if (team != null && league != null)
                        user.TeamsFans.Add(new TeamsFan
                        {
                            TeamId = t.TeamId,
                            UserId = user.UserId,
                            LeageId = t.LeagueId,
                            Team = team,
                            League = league,
                            User = user
                        });
                    else
                        user.TeamsFans.Add(new TeamsFan
                        {
                            TeamId = t.TeamId,
                            UserId = user.UserId,
                            LeageId = db.Leagues.First(x=>x.IsArchive == false).LeagueId,
                            Team = team,
                            User = user
                        });
                }
            }
            user.Notifications.ToList().ForEach(t => user.Notifications.Remove(t));

            db.SaveChanges();

            var notesList = db.Notifications.ToList();

            if (!model.IsStartAlert)
            {
                var nItem = notesList.FirstOrDefault(t => t.Type == "StartAlert");
                user.Notifications.Add(nItem);
            }

            if (!model.IsTimeChange)
            {
                var nItem = notesList.FirstOrDefault(t => t.Type == "TimeChange");
                user.Notifications.Add(nItem);
            }

            if (!model.IsGameRecords)
            {
                var nItem = notesList.FirstOrDefault(t => t.Type == "GameRecords");
                user.Notifications.Add(nItem);
            }
            if (!model.IsGameScores)
            {
                var nItem = notesList.FirstOrDefault(t => t.Type == "GameScores");
                user.Notifications.Add(nItem);
            }

            db.SaveChanges();

            return Ok("saved");
        }

        /// <summary>
        /// קבלת פרטי משתמש 
        /// </summary>
        /// <returns></returns>
        [Route("Details")]
        public IHttpActionResult GetEditUser()
        {
            var user = base.CurrentUser;

            var model = new UserDetails
            {
                Email = user.Email,
                UserName = user.UserName,
                //FirstName = user.FirstName,
                //LastName = user.LastName,
                //MiddleName = user.MiddleName,
                FullName = user.FullName
            };

            var lang = user.LangId != null ? db.Languages.FirstOrDefault(x => x.LangId == user.LangId) : null;

            model.Language = lang != null ? lang.Code : "he";

            var userNotes = user.Notifications.ToList();

            model.IsStartAlert = userNotes.All(t => t.Type != "StartAlert");
            model.IsTimeChange = userNotes.All(t => t.Type != "TimeChange");
            model.IsGameRecords = false;//!userNotes.Any(t => t.Type == "GameRecords");
            model.IsGameScores = userNotes.All(t => t.Type != "GameScores");

            return Ok(model);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("Activity")]
        public IHttpActionResult getCurrentActivityID()
        {
            var seasonRepo = new SeasonsRepo();
            int seasonId = seasonRepo.GetCurrentByUnionId(38)??0;
            int? activityId = 0;
            activityId = db.Activities.Where(x => x.Type.CompareTo("unionplayertoclub") == 0 && x.SeasonId == (seasonId) && x.UnionId == 38 && x.IsAutomatic == true).FirstOrDefault()?.ActivityId;
            return Ok(activityId);
        }
        [NonAction]
        private string CreateFacebookProfilePictureUrl(string fbid)
        {
            return string.Format("https://graph.facebook.com/{0}/picture?type=large", fbid);
        }

    }
}
