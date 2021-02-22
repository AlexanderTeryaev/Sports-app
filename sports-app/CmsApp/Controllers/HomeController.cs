using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Controllers
{
    public class HomeController : AdminController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            playersRepo.UnblockAllBlockadedPlayers();
            gamesRepo.UpdateGameStatistics();
            playersRepo.CheckForExclusions();

            if (User.IsInRole(AppRole.Workers)) 
            {
                return RedirectToAction("Index", "WorkerHome");
            }

            if (User.IsInRole(AppRole.Players))
            {
                int userId;
                if (int.TryParse(User.Identity.Name, out userId))
                {
                    var user = usersRepo.GetById(userId);

                    var currentSeasons =
                        user.TeamsPlayers.Where(x => x.Season?.StartDate <= DateTime.Now &&
                                                     x.Season?.EndDate > DateTime.Now)
                                                     .ToList();

                    var latestSeason =
                        currentSeasons.FirstOrDefault(x => x.SeasonId == currentSeasons.Max(s => s.SeasonId));

                    if (latestSeason != null)
                    {
                        var isUnion = latestSeason.Season?.Union != null;

                        SetIsSectionClubLevel(!isUnion);
                        if (isUnion)
                        {
                            SetUnionCurrentSeason(latestSeason.SeasonId ?? 0);
                        }
                        else
                        {
                            SetClubCurrentSeason(latestSeason.SeasonId ?? 0);
                        }
                        var data = new { id = User.Identity.Name, seasonId = latestSeason.SeasonId };
                        if (latestSeason.ClubId > 0 && latestSeason.TeamId > 0)
                        {

                            var data2 = new { id = User.Identity.Name, seasonId = latestSeason.SeasonId, clubId = latestSeason.ClubId, teamId = latestSeason.TeamId };
                            return RedirectToAction("Edit", "Players", data2);
                        }
                        return RedirectToAction("Edit", "Players", data);
                    }
                }

                return RedirectToAction("Edit", "Players", new { id = User.Identity.Name });
            }

            return RedirectToAction("Index", "Sections");
        }
    }
}