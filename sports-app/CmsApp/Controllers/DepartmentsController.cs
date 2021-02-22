using CmsApp.Models;
using DataService;
using DataService.DTO;
using System.Linq;
using System.Web.Mvc;

namespace CmsApp.Controllers
{
    public class DepartmentsController : AdminController
    {
        private DepartmentRepo depRepo;

        public DepartmentsController()
        {
            depRepo = new DepartmentRepo();
        }

        public ActionResult Edit(int id, int? seasonId, int? sportId)
        {
            return RedirectToAction("Edit", "Clubs", new { id = id, isDepartment = true, seasonId = seasonId, sportId = sportId });
        }

        public ActionResult Create(DeparmentModel deparmentFrm, int parentClubId, int? seasonId)
        {
            var departmentModel = new DepartmentDTO
            {
                ParentClubId = parentClubId,
                SeasonId = seasonId,
                Title = deparmentFrm.DepartmentTitle,
                SportId = deparmentFrm.SelectedSportId
            };

            var department = depRepo.Create(departmentModel);

            activityRepo.CreateActivityForClubSeason(department.ClubId, seasonId ?? 0);

            return RedirectToAction("Edit", "Clubs", new { id = parentClubId, sectionId = department.SectionId });
        }

        public ActionResult List(int id, int? seasonId)
        {
            var departments = depRepo.GetByParentClubId(id, seasonId)
                    .Select(c => new DeparmentModel
                    {
                        DepartmentId = c.Id,
                        DepartmentTitle = c.Title,
                        Sports = new SelectList(secRepo.GetSections(x => x.Alias != SectionAliases.MultiSport), "SectionId", "Name", c.SportId.ToString()),
                        ParentSectionId = c.SportId
                    })
                    .ToList();
            var vm = new DepartmentsForm
            {
                ParentClubId = id,
                SeasonId = seasonId,
                Deparments = departments,
                Sports = new SelectList(secRepo.GetSections(x => x.Alias != SectionAliases.MultiSport), "SectionId", "Name"),
            };

            return PartialView("_List", vm);
        }

        [HttpPost]
        public ActionResult Update(DeparmentModel deparmentFrm, int parentClubId, int? seasonId)
        {
            var depDto = new DepartmentDTO { Id = deparmentFrm.DepartmentId ?? 0, Title = deparmentFrm.DepartmentTitle, SportId = deparmentFrm.SelectedSportId };

            var sectionId = depRepo.UpdateDepartment(depDto);

            return RedirectToAction("Edit", "Clubs", new { id = parentClubId, sectionId = sectionId });
        }

        public ActionResult Delete(int departmentId, int parentClubId, int? seasonId)
        {
            var sectionId = depRepo.DeleteDepartment(departmentId);
            return RedirectToAction("Edit", "Clubs", new { id = parentClubId, sectionId = sectionId });
        }
    }
}