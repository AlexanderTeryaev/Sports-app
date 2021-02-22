using System.Web.Mvc;
using Resources;
using CmsApp.Models;
using AppModel;

namespace CmsApp.Controllers
{
    public class TeamsAuditoriumsController : AdminController
    {
        // GET: TeamsAuditoriums
        public ActionResult List(int id, int seasonId)
        {
            var team = teamRepo.GetById(id);

            var vm = new TeamsAuditoriumForm
            {
                TeamId = id,
                HasArena = team.HasArena,
                TeamAuditoriums = auditoriumsRepo.GetByTeam(id),
                SeasonId = seasonId,
                Auditoriums = new SelectList(auditoriumsRepo.GetByTeamAndSeason(id, seasonId),
                    nameof(Auditorium.AuditoriumId), nameof(Auditorium.Name))
            };

            ViewBag.Section = secRepo.GetSectionByTeamId(id).Alias;

            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }

            return PartialView("_List", vm);
        }

        [HttpPost]
        public ActionResult Add(TeamsAuditoriumForm frm, int seasonId)
        {
            bool isExists = auditoriumsRepo.IsExistsInTeam(frm.AuditoriumId, frm.TeamId);

            if (!isExists)
            {
                var item = new TeamsAuditorium();

                UpdateModel(item);

                auditoriumsRepo.AddToTeam(item);
                auditoriumsRepo.Save();
            }
            else
            {
                ModelState.AddModelError("AuditoriumId", Messages.AuditoriumAlreadyExsits);
                TempData["ViewData"] = ViewData;
            }

            return RedirectToAction("List", new { id = frm.TeamId, seasonId});
        }

        public ActionResult Delete(int id, int seasonId)
        {
            var item = auditoriumsRepo.GetAuditoriumById(id);
            auditoriumsRepo.RemoveFromTeam(item);
            auditoriumsRepo.Save();

            return RedirectToAction("List", new { id = item.TeamId, seasonId = seasonId });
        }

        [HttpPost]
        public ActionResult HasArena(int id, int seasonId, bool? HasArena)
        {
            var team = teamRepo.GetById(id);

            team.HasArena = HasArena;
            teamRepo.Save();

            return RedirectToAction("List", new { id = id, seasonId });
        }
    }
}