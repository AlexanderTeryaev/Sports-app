using System.Web.Mvc;
using CmsApp.Models;
using AppModel;

namespace CmsApp.Controllers
{
    public class AuditoriumsController : AdminController
    {
        private int? currClubId = null;
        private bool isUnionClub = false;
        
        // GET: Auditoriums
        public ActionResult List(int? unionId, int? clubId, int? seasonId)
        {
            var sectionAlias = unionId.HasValue ? unionsRepo.GetById(unionId.Value).Section.Alias : seasonId.HasValue ? seasonsRepository.GetById(seasonId.Value).Union?.Section?.Alias : clubsRepo.GetById(clubId.Value).Section?.Alias ;
            
            var vm = new AuditoriumForm
            {
                UnionId = unionId,
                ClubId = clubId,
                Auditoriums = clubId.HasValue ? auditoriumsRepo.GetByClubAndSeason(clubId.Value, seasonId) :
                                                auditoriumsRepo.GetByUnionAndSeason(unionId, seasonId.Value),
                SeasonId = seasonId,
                SectionAlias = sectionAlias
            };       
            if(sectionAlias == SectionAliases.Climbing)
            {
                var uId = unionId ?? clubsRepo.GetById(clubId.Value).UnionId;
                vm.Disciplines = disciplinesRepo.GetAllByUnionId(uId.Value);
            }

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.UnionId = unionId;
            ViewBag.SeasonId = seasonId;
            ViewBag.ClubId = clubId;

            return PartialView("_List", vm);
        }

        [HttpPost]
        public ActionResult Create(AuditoriumForm frm)
        {
            Auditorium aud;
            if (frm.AuditoriumId > 0)
            {
                aud = auditoriumsRepo.GetById(frm.AuditoriumId);
                aud.Name = frm.Name;
                aud.Address = frm.Address;
                aud.LanesNumber = frm.LanesNumber;
                aud.Length = frm.Length;
                aud.DisciplineId = frm.DisciplineId;
                aud.IsIndoor = frm.IndoorOutdoor == IndoorOutdoor.Indoor;
                aud.IsHeated = frm.IsHeated == IsHeated.Yes;
            }
            else
            {
                aud = new Auditorium
                {
                    ClubId = frm.ClubId,
                    UnionId = isClubUnderUnion(frm.ClubId) ? null : frm.UnionId,
                    SeasonId = isClubUnderUnion(frm.ClubId) ? null : frm.SeasonId,
                    Name = frm.Name,
                    Address = frm.Address,
                    Length = frm.Length,
                    LanesNumber = frm.LanesNumber,
                    DisciplineId = frm.DisciplineId,
                    IsIndoor = frm.IndoorOutdoor == IndoorOutdoor.Indoor,
                    IsHeated = frm.IsHeated == IsHeated.Yes
                };
                auditoriumsRepo.Create(aud);
            }

            auditoriumsRepo.Save();

            TempData["SavedId"] = aud.AuditoriumId;

            return RedirectToAction("List", new { unionId = frm.UnionId, clubId = frm.ClubId, seasonId = frm.SeasonId });
        }

        private bool isClubUnderUnion(int? clubId)
        {
            if (!clubId.HasValue)
                return false;
            if (!currClubId.HasValue || currClubId.Value != clubId.Value)
            {
                currClubId = clubId;
                isUnionClub = clubsRepo.GetById(clubId.Value).IsUnionClub ?? true;
            }
            return isUnionClub;
        }

        public ActionResult Delete(int id,int? unionId, int? seasonId, int? clubId)
        {
            var aud = auditoriumsRepo.GetById(id);
            aud.IsArchive = true;
            auditoriumsRepo.Save();
            if (unionId.HasValue && seasonId.HasValue)
            {
                return RedirectToAction("List", new { unionId, clubId, seasonId });
            }
            return RedirectToAction("List", new { unionId = aud.UnionId, clubId = aud.ClubId, seasonId = aud.SeasonId });
        }

    }
}