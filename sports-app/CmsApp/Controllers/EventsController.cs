using AppModel;
using CmsApp.Models;
using CmsApp.Models.Mappers;
using System;
using System.Net;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using Resources;
using System.IO;

namespace CmsApp.Controllers
{
    public class EventsController : AdminController
    {
        public ActionResult Index(int? leagueId, int? clubId, int? unionId)
        {
            return PartialView("List", new EventModel { LeagueId = leagueId, ClubId = clubId, UnionId = unionId });
        }

        // GET: Events
        public ActionResult List(int? leagueId, int? clubId, int? unionId)
        {
            EventModel ev = new EventModel()
            {
                ClubId = clubId,
                LeagueId = leagueId,
                UnionId = unionId
            };
            if (leagueId.HasValue)
            {
                ev.isCollapsable = true;
                ev.EventList = leagueRepo.GetById(leagueId.Value).Events.ToList();
            }
            else 
            if(clubId.HasValue)
            {
                ev.isCollapsable = false;
                ev.EventList = leagueRepo.GetClubById(clubId.Value).Events.ToList();
            }
            else
            {
                ev.isCollapsable = false;
                ev.EventList = unionsRepo.GetById(unionId.Value).Events.ToList();
            }
            ViewBag.IsUnionViewer = usersRepo.GetTopLevelJob(AdminId) == JobRole.Unionviewer;
            return PartialView("_Events", ev);
        }

        [HttpGet]
        public ActionResult AddEvent(int? leagueId, int? clubId, int? unionId)
        {
            var ef = new EventForm
            {
                LeagueId = leagueId,
                LeagueName = leagueId.HasValue ? leagueRepo.GetById(leagueId.Value).Name : null,
                ClubId = clubId,
                ClubName = clubId.HasValue ? leagueRepo.GetClubById(clubId.Value).Name : null,
                UnionId = unionId,
                UnionName = unionId.HasValue ? unionsRepo.GetById(unionId.Value).Name : null
            };

            return PartialView("_AddEvent", ef);
        }

        [HttpPost]
        public ActionResult AddEvent(EventForm ef)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddEvent", ef);
            }
            var e = ef.ToEvent();
            if(ef.UnionId.HasValue)
            {
                //upload image and add to event
                var maxFileSize = GlobVars.MaxFileSize * 1000;
                HttpPostedFileBase postedFile = GetPostedFile("ImageFile");

                if(postedFile != null && postedFile.ContentLength > 0)
                {
                    if(postedFile.ContentLength > maxFileSize)
                    {
                        ModelState.AddModelError("ImageFile", Messages.FileSizeError);
                    }
                    else
                    {
                        var newName = SaveFile(postedFile, "eventImg");
                        if (newName == null)
                        {
                            ModelState.AddModelError("ImageFile", Messages.FileError);
                        }
                        else
                        {
                            e.EventImage = newName;
                        }
                    }
                }

            }
            eventsRepo.Create(e);
            return RedirectToAction("List", new { leagueId = ef.LeagueId, clubId = ef.ClubId, unionId = ef.UnionId });
        }

        [HttpPost]
        public ActionResult UpdateEvent(EventUpdateForm ev)
        {
            var eventObj = eventsRepo.GetById(ev.EventId);
            var imgChanged = false;
            var imgPath = "";
            if(ev.UnionId.HasValue)
            {
                //upload image and add to event
                var maxFileSize = GlobVars.MaxFileSize * 1000;
                HttpPostedFileBase postedFile = GetPostedFile("ImageFile");

                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    if (postedFile.ContentLength > maxFileSize)
                    {
                        return Json(new { stat = "failed", id = ev.EventId , Message = Messages.FileSizeError});
                        //ModelState.AddModelError("ImageFile", Messages.FileSizeError);
                    }
                    else
                    {
                        var newName = SaveFile(postedFile, "eventImg");
                        if (newName == null)
                        {
                            return Json(new { stat = "failed", id = ev.EventId, Message = Messages.FileError });
                        }
                        else
                        {
                            eventObj.EventImage = newName;
                            imgChanged = true;
                            imgPath = GlobVars.ContentPath + "/Events/" + newName;
                        }
                    }
                }
                else
                {
                    if (ev.RemoveEventImageFile) eventObj.EventImage = null;

                }

            }
            eventObj.Title = ev.Title;
            eventObj.EventTime = ev.EventTime;
            eventObj.Place = ev.Place;
            eventObj.EventDescription = ev.EventDescription;

            eventsRepo.Update(eventObj);
            return Json(new { stat = "ok", id = ev.EventId, imgChanged, imgPath });
        }

        public ActionResult DeleteEvent(int eventId, int? leagueId, int? clubId, int? unionId)
        {
            eventsRepo.Delete(eventId);
            return RedirectToAction("List", new { leagueId = leagueId, clubId = clubId, unionId = unionId });
        }

        public ActionResult PublishEvent(int eventId, bool isPublished)
        {
            try
            {
                var ev = eventsRepo.GetById(eventId);
                ev.IsPublished = isPublished;
                eventsRepo.Update(ev);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.ToString());
            }
        }

        [NonAction]
        private HttpPostedFileBase GetPostedFile(string name)
        {
            if (Request.Files[name] == null)
                return null;

            if (Request.Files[name].ContentLength == 0)
                return null;

            return Request.Files[name];
        }

        [NonAction]
        private string SaveFile(HttpPostedFileBase file, string name)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!GlobVars.ValidImages.Contains(ext))
                return null;

            var newName = name + "_" + AppFunc.GetUniqName() + ext;

            var savePath = Server.MapPath(GlobVars.EventContentPath);

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
    }
}