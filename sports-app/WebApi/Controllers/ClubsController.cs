using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using DataService;
using WebApi.Models;
using WebApi.Services;
using System.Configuration;
using System.IO;
using System.Linq;
using AppModel;

namespace WebApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("api/Clubs")]
    public class ClubsController : BaseLogLigApiController
    {
        /// <summary>
        /// Get info about sepcific club by his id
        /// </summary>
        /// <param name="id">id of club</param>
        /// <param name="unionId"></param>
        /// <returns></returns>
        [Route("{id}")]
        [ResponseType(typeof(ClubInfoViewModel))]
        public IHttpActionResult Get(int id, int? unionId = null)
        {
            SectionsRepo sRepo = new SectionsRepo();
            var sectionName = sRepo.GetSectionByClubId(id)?.Alias;
            bool isTennis = sectionName != null ? sectionName.Equals(GamesAlias.Tennis) : false;

            SeasonsRepo seasonsRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                              seasonsRepo.GetLastSeasonIdByCurrentClubId(id);

            var clubInfo = ClubService.GetClub(id, seasonId, isTennis);
            return Ok(clubInfo);
        }

        [Route("departments/{id}")]
        [ResponseType(typeof(ClubInfoViewModel))]
        public IHttpActionResult GetDepartment(int id)
        {
            SectionsRepo sRepo = new SectionsRepo();
            var sectionName = sRepo.GetSectionByClubId(id)?.Alias;
            bool isTennis = sectionName != null ? sectionName.Equals(GamesAlias.Tennis) : false;

            var clubInfo = ClubService.GetClub(id, null, isTennis);
            return Ok(clubInfo);
        }

        [Route("players/{clubId}")]
        [ResponseType(typeof(ClubInfoViewModel))]
        public IHttpActionResult GetPlayers(int clubId, int? unionId = null)
        {
            SeasonsRepo seasonsRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                              seasonsRepo.GetLastSeasonIdByCurrentClubId(clubId);
            int userId = (CurrentUser == null || CurrentUser.UserId == null) ? -1 : CurrentUser.UserId;
            var info = ClubService.GetPlayersForClub(clubId, userId, seasonId);
            return Ok(info);
        }

        [Route("fans/{clubId}")]
        [ResponseType(typeof(ClubInfoViewModel))]
        public IHttpActionResult GetFans(int clubId, int? unionId = null)
        {
            SeasonsRepo seasonsRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                              seasonsRepo.GetLastSeasonIdByCurrentClubId(clubId);

            var info = ClubService.GetFansForClub(clubId, seasonId);
            return Ok(info);
        }
        /*[Route("fans/{UnionId}")]
        [ResponseType(typeof(ClubInfoViewModel))]
        public IHttpActionResult GetClubsOfCurUser(int? unionId = null)
        {
            SeasonsRepo seasonsRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                              0;

            var info = ClubService.GetClubsOfCurUser(CurrentUser,seasonId);
            return Ok(info);
        }*/

        [ResponseType(typeof(List<ImageGalleryViewModel>))]
        [Route("ImageGallery/{clubId}")]
        public IHttpActionResult GetImageGallery(int clubId)
        {
            List<ImageGalleryViewModel> result = new List<ImageGalleryViewModel>();
            string dirPath = ConfigurationManager.AppSettings["ClubUrl"] + "\\" + clubId;
            if (!Directory.Exists(dirPath))
            {
                return Ok(result);
            }

            UsersRepo usersRepo = new UsersRepo();
            var allfiles = System.IO.Directory.EnumerateFiles(dirPath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".jpeg") || s.EndsWith(".jpg") || s.EndsWith(".png"));

            foreach (var file in allfiles)
            {
                try
                {

                    FileInfo info = new FileInfo(file);
                    var uid = int.Parse(info.Name.Substring(0, info.Name.IndexOf("__")));
                    User user = usersRepo.GetById(uid);
                    if (user != null)
                    {
                        var elem = new ImageGalleryViewModel();
                        elem.Created = info.CreationTime;
                        elem.url = clubId + "/" + info.Name;
                        elem.User = new UserModel
                        {
                            Id = user.UserId,
                            Name = user.FullName ?? user.UserName,
                            Image = user.Image == null && user.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(user, null).Image : user.Image,
                            UserRole = user.UsersType.TypeRole
                        };
                        result.Add(elem);
                    }

                }
                catch (Exception e)
                {
                    continue;
                }

                // Do something with the Folder or just add them to a list via nameoflist.add();
            }

            result = result.OrderByDescending(x => x.Created).ToList();

            return Ok(result);
        }

        [Route("DeleteGallery/{clubId}/{galleryName}")]
        [HttpGet]
        public IHttpActionResult DeleteImageGallery(int clubId, string galleryName)
        {
            string filePath = ConfigurationManager.AppSettings["ClubUrl"] + "\\" + clubId + "\\" + galleryName;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Ok();
            }
            else
            {
                return null;
            }
        }

        [Route("isMyClub/{clubId}")]
        [HttpGet]
        public IHttpActionResult isMyClub(int clubId, int? unionId = null)
        {
            SeasonsRepo seasonsRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                              seasonsRepo.GetLastSeasonIdByCurrentClubId(clubId);

            string job = null;
            var bRet = false;
            UsersRepo usersRepo = new UsersRepo();
            if (User.IsInRole(AppRole.Fans))
            {
                job = AppRole.Fans;
            }
            else
            {
                job = usersRepo.GetTopLevelJob(CurrentUser.UserId);
                if (job == null)
                {
                    job = AppRole.Players;
                }
            }

            if(job != null)
            {
                if(job == AppRole.Fans)
                { 
                    bRet = ClubService.GetFansForClub(clubId, seasonId).Where(c=>c.Id == CurrentUser.UserId).Count() > 0 ? true : false;
                }
                else
                {
                    var clubs = db.Clubs.Where(c => c.ClubId == clubId && c.SeasonId == seasonId).ToList();
                    foreach (var club in clubs)
                    {
                        if (job == AppRole.Players)
                        { 
                            bRet = club.TeamsPlayers.Where(tp => tp.UserId == CurrentUser.UserId).Count() > 0 ? true : false;
                        }
                        else if( job == JobRole.ClubManager)
                        {
                            bRet = club.UsersJobs.Where(tp => tp.UserId == CurrentUser.UserId).Count() > 0 ? true : false;
                        }
                        if (bRet) break;
                    }
                }
            }
            return Ok(bRet);
        }

    }
}
