using AppModel;
using AutoMapper;
using ClosedXML.Excel;
using CmsApp.Helpers;
using CmsApp.Models;
using DataService;
using DataService.DTO;
using DataService.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Omu.ValueInjecter;
using Resources;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace CmsApp.Controllers
{
    public class LeaguesController : AdminController
    {
        private const string ImportPlayersErrorResultSessionKey = "ImportCompetitionResults_ErrorResult";
        private const string ImportPlayersErrorResultFileNameSessionKey = "ImportCompetitionResults_ErrorResultFileName";

        public ActionResult Edit(int id, int? seasonId, int? departmentId = null, string roleType = null, int isTennisCompetition = 0)
        {

            if (!string.IsNullOrWhiteSpace(roleType))
            {
                this.SetWorkerSession(roleType);
            }

            gamesRepo.UpdateGameStatistics();
            var item = leagueRepo.GetById(id);

            ViewBag.IsItMultiBattleType = false;

            var competitionDisciplines = disciplinesRepo.GetCompetitionDisciplines(id).Select(c => new CompetitionDisciplineDto
            {
                SectionAlias = c.League.Union.Section.Alias,
                DisciplineId = c.DisciplineId

            }).ToList();

            var isGoldenSpike = false;
            if (competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Athletics)
            {
                foreach (var competitionDiscipline in competitionDisciplines)
                {
                    if (competitionDiscipline.DisciplineId.HasValue)
                    {
                        var discipline = disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                        if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && (discipline.DisciplineType == "GoldenSpikesU14" || discipline.DisciplineType == "GoldenSpikesU16"))
                        {
                            isGoldenSpike = true;
                            break;
                        }
                    }
                }
            }

            SetIsSectionClubLevel(false);
            
            if (seasonId != null)
            {
                SetUnionCurrentSeason(seasonId.Value);
            }
            
            var roleName = usersRepo.GetTopLevelJob(AdminId);
            var isAdminsRole = User.IsInAnyRole(AppRole.Admins, AppRole.Editors);
            var isUnionViewer = AuthSvc.AuthorizeUnionViewerByManagerId(item?.UnionId ?? item?.Club?.UnionId ?? 0, AdminId);
            bool isCallRoomManager = User.GetSessionWorkerValueOrTopLevelLeagueJob(id) == JobRole.CallRoomManager;
            var isNotValidWorker = User.IsInRole(AppRole.Workers) && !AuthSvc.AuthorizeLeagueByIdAndManagerId(id, AdminId) && roleName != JobRole.ClubManager && roleName != JobRole.ClubSecretary
                && roleName != JobRole.DepartmentManager && !isUnionViewer && roleName != JobRole.Referee && roleName != JobRole.RefereeAssignment && !isCallRoomManager;



            if (isNotValidWorker)
            {
                return RedirectToAction("Index", "NotAuthorized");
            }

            if (item == null || item.IsArchive)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var section = departmentId == null ? secRepo.GetByLeagueId(item.LeagueId) : clubsRepo.GetClubById(departmentId.Value).SportSection;
            var isDepartmentLeague = false;
            if (item.ClubId.HasValue)
            {
                isDepartmentLeague = clubsRepo.GetById(item.ClubId.Value).ParentClub != null ? true : false;
            }
            var vm = new LeagueNavView
            {
                LeagueId = item.LeagueId,
                LeagueName = item.Name,
                UnionId = item?.UnionId,
                ClubId = item.ClubId,
                IsDepartmentLeague = isDepartmentLeague,
                SectionId = section?.SectionId,
                SectionAlias = section?.Alias,
                UnionName = item.Union == null ? string.Empty : item.Union.Name,
                ClubName = item.Club == null ? string.Empty : item.Club.Name,
                DisciplineId = item.DisciplineId,
                DisciplineAlias = item.Discipline?.Name ?? "",
                IsUnionValid = isAdminsRole,
                SeasonId = seasonId ?? item.SeasonId ?? getCurrentSeason().Id,
                IsIndividual = item.Union?.Section?.IsIndividual ?? item?.Club?.Section?.IsIndividual ?? item?.Club?.Union?.Section?.IsIndividual ?? false,
                IsPositionSettingsEnabled = item.IsPositionSettingsEnabled,
                IsTeam = item.IsTeam,
                IsTennisCompetition = isTennisCompetition,
                AthleticLeagueId = item.AthleticLeagueId
            };
            ViewBag.SectionAlias = section?.Alias;
            Session["desOrder"] = false;
            if (!isAdminsRole)
            {
                if (vm.UnionId != null)
                    vm.IsUnionValid = AuthSvc.AuthorizeUnionByIdAndManagerId(vm.UnionId.Value, AdminId);
            }

            ViewBag.IsActivityManager = jobsRepo.IsActivityManager(AdminId);
            ViewBag.IsActivityViewer = jobsRepo.IsActivityViewer(AdminId);
            ViewBag.IsActivityRegistrationActive = jobsRepo.IsActivityRegistrationActive(AdminId);
            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.IsUnionViewer = isUnionViewer;
            ViewBag.IsTennisLeagueReferee = usersRepo.GetTopLevelJob(AdminId)?.Equals(JobRole.Referee) == true
                && jobsRepo.GetAllTennisLeagues(AdminId).Count > 0;
            ViewBag.UserId = AdminId;
            ViewBag.IsTennisLeague = section?.Alias.Equals(GamesAlias.Tennis) == true && (item.EilatTournament == false || item.EilatTournament == null);
            ViewBag.UserId = AdminId;
            ViewBag.IsGoldenSpike = isGoldenSpike;
            return View(vm);
        }

        public ActionResult ChangeClubCompetitionStatus(int leagueId, int value)
        {
            League l = leagueRepo.GetById(leagueId);
            l.IsClubCompetition = value;
            leagueRepo.Save();

            return null;
        }

        [HttpPost]
        public ActionResult Create(LeagueCreateForm frm)
        {
            var item = new League();
            var union = unionsRepo.GetById(frm.UnionId ?? 0);
            var section = union?.Section?.Alias ?? string.Empty;
            bool isAthleteCompetition = section.Equals(GamesAlias.Athletics) && frm.Et != TournamentsPDF.EditType.LgUnion;
            if (frm.Et != TournamentsPDF.EditType.LgUnion)
                item.EilatTournament = true;
            UpdateModel(item);
            item.DuplicatedLeagueId = frm.DuplicateLeagueId;
            if (frm.IsTennisCompetition == 1 && !isAthleteCompetition)
            {
                item.AgeId = 1;
                item.GenderId = 0;
            }

            if (frm.IsTennisCompetition == 0 || isAthleteCompetition)
            {
                leagueRepo.Create(item);
                leagueRepo.Save();
            }
            else
            {
                if (item.DuplicatedLeagueId.HasValue)
                {
                    leagueRepo.DuplicateLeague(item);
                }
                else
                {
                    leagueRepo.CreateTennis(item);
                }
                leagueRepo.Save();
            }

            bool isDepartmentLeague = false;

            if (frm.ClubId != null)
            {
                var club = clubsRepo.GetById(frm.ClubId.Value);
                isDepartmentLeague = club?.ParentClub != null ? true : false;
            }

            activityRepo.AttachLeagueToActivity(item.LeagueId, item.SeasonId.Value);
            int isTennisCompetition = frm.IsTennisCompetition;
            if (isDepartmentLeague)
            {
                if (isTennisCompetition == 0)
                {
                    return RedirectToAction("Edit", new { id = item.LeagueId, seasonId = frm.SeasonId, departmentId = frm.ClubId });
                }
                else
                {
                    return RedirectToAction("Edit", new { id = item.LeagueId, seasonId = frm.SeasonId, departmentId = frm.ClubId, isTennisCompetition = isTennisCompetition });
                }
            }
            else
            {
                if (isTennisCompetition == 0 || isAthleteCompetition)
                {
                    return RedirectToAction("Edit", new { id = item.LeagueId, seasonId = frm.SeasonId });
                }
                else
                {
                    return RedirectToAction("Edit", new { id = item.LeagueId, seasonId = frm.SeasonId, isTennisCompetition = isTennisCompetition });
                }
            }
        }

        public ActionResult CreatePDF(int id, TournamentsPDF.EditType et)
        {
            var vm = new LeagueCreateForm { UnionId = id, Et = et };
            vm.Ages = new SelectList(leagueRepo.GetAges(), "AgeId", "Title", vm.AgeId);
            vm.Genders = new SelectList(leagueRepo.GetGenders(), "GenderId", "TitleMany", vm.GenderId);

            return PartialView("_CreateForm", vm);
        }


        public ActionResult Create(int? unionId, int? disciplineId, int? clubId, TournamentsPDF.EditType et, int seasonId, int isTennisCompetition = 0)
        {
            var isHandicapEnabled = unionId.HasValue && unionsRepo.GetHandicapValueByUnionId(unionId.Value);
            var vm = new LeagueCreateForm
            {
                UnionId = unionId,
                ClubId = clubId,
                Et = et,
                SeasonId = seasonId,
                IsHandicapEnabled = isHandicapEnabled,
                IsTennisCompetition = isTennisCompetition,
                LeaguesForDuplicate = leagueRepo.LeaguesForDuplicate(unionId ?? 0, seasonId, isTennisCompetition == 1)
            };

            vm.Ages = new SelectList(leagueRepo.GetAges(), "AgeId", "Title", vm.AgeId);
            vm.Genders = new SelectList(leagueRepo.GetGenders(), "GenderId", "TitleMany", vm.GenderId);
            if (unionId.HasValue)
            {
                vm.Section = unionsRepo.GetById(unionId.Value)?.Section?.Alias;
            }
            else if (clubId.HasValue)
            {
                var club = clubsRepo.GetById(clubId.Value);
                vm.Section = club?.Section?.Alias ?? club.Union?.Section?.Alias;
            }
            else if (disciplineId.HasValue)
            {
                vm.Section = disciplinesRepo.GetById(disciplineId.Value)?.Union?.Section?.Alias;
            }
            if(vm.Section == SectionAliases.Bicycle)
            {
                vm.BicycleDisciplines = new SelectList(db.BicycleCompetitionDisciplines.Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList(), "Id", "Name");
                vm.Levels = new SelectList(db.CompetitionLevels.Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList(), "id", "level_name");
            }
            if(vm.Section == SectionAliases.Climbing)
            {
                //add disciplines
                vm.Disciplines = new SelectList(disciplinesRepo.GetAllByUnionId(unionId.Value), "DisciplineId", "Name");
            }

            return PartialView("_CreateForm", vm);
        }

        public ActionResult Details(int id, int isTennisCompetition, string sectionAliasRead)
        {
            League league;
            if (sectionAliasRead == GamesAlias.Tennis)
            {
                league = leagueRepo.GetByIdExtended(id);
            }
            else
            {
                league = leagueRepo.GetById(id);
            }
            var playersCount = league.LeagueTeams.Sum(x => x.Teams.TeamsPlayers.Count(tp => tp.LeagueId == league.LeagueId));
            var sectionAlias = leagueRepo.GetSectionAlias(id);
            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            var union = league.Union;
            var vm = new LeagueDetailsForm();

            vm.InjectFrom(league);

            vm.GenderId = league.GenderId ?? 0;
            vm.AgeId = league.AgeId ?? 0;
            vm.IsIndividual = union?.Section?.IsIndividual == true;
            vm.isTennisCompetition = isTennisCompetition == 1;
            vm.IsTennisLeague = sectionAlias?.Equals(SectionAliases.Tennis) == true
                && (league.EilatTournament == null || league.EilatTournament == false);
            vm.IsDailyCompetition = league.IsDailyCompetition;
            vm.IsSeniorCompetition = league.IsSeniorCompetition ?? false;
            if (isTennisCompetition == 1)
            {
                vm.LevelDatesSettings = new LevelDatesSettingsForm
                {
                    CompetitionId = league.LeagueId,
                    LevelDatesSettings = league.LevelDateSettings?.Select(lds => new LevelDateSettingDto
                    {
                        Id = lds.Id,
                        CompetitionLevelId = lds.CompetitionLevelId,
                        QualificationStartDate = lds.QualificationStartDate,
                        QualificationEndDate = lds.QualificationEndDate,
                        FinalStartDate = lds.FinalStartDate,
                        FinalEndDate = lds.FinalEndDate
                    }),
                    LevelList = leagueRepo.GetCompetitionLevels(league?.UnionId)
                };
            }
            vm.IsAthleticsCompetition = sectionAlias?.Equals(GamesAlias.Athletics) == true
                && league.EilatTournament == true;

            vm.LeaguesPlayerInsurancePrice = league.LeaguesPrices
                .Where(p => p.PriceType == (int)LeaguePriceType.PlayerInsurancePrice)
                .Select(p => new LeaguesPricesForm
                {
                    EndDate = p.EndDate,
                    Price = p.Price,
                    StartDate = p.StartDate,
                    CardComProductId = p.CardComProductId
                }).ToList();

            vm.LeaguesPlayerRegistrationPrice = league.LeaguesPrices
                .Where(p => p.PriceType == (int)LeaguePriceType.PlayerRegistrationPrice)
                .Select(p => new LeaguesPricesForm
                {
                    EndDate = p.EndDate,
                    Price = p.Price,
                    StartDate = p.StartDate,
                    CardComProductId = p.CardComProductId
                }).ToList();

            vm.LeaguesTeamRegistrationPrice = league.LeaguesPrices
                .Where(p => p.PriceType == (int)LeaguePriceType.TeamRegistrationPrice)
                .Select(p => new LeaguesPricesForm
                {
                    EndDate = p.EndDate,
                    Price = p.Price,
                    StartDate = p.StartDate,
                    CardComProductId = p.CardComProductId
                }).ToList();

            vm.MemberFees = league.MemberFees.Select(x => new MemberFeeModel
            {
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Price = x.Amount,
                CardComProductId = x.CardComProductId
            }).ToList();

            vm.HandlingFees = league.HandlingFees.Select(x => new HandlingFeeModel
            {
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Price = x.Amount,
                CardComProductId = x.CardComProductId
            }).ToList();

            vm.IsHadicapEnabled = league?.Union?.IsHadicapEnabled ?? false;
            vm.Ages = new SelectList(leagueRepo.GetAges(), "AgeId", "Title", vm.AgeId);
            vm.Genders = new SelectList(leagueRepo.GetGenders(), "GenderId", "TitleMany", vm.GenderId);
            vm.Section = sectionAlias;

            var doc = leagueRepo.GetTermsDoc(id);
            if (doc != null)
            {
                vm.DocId = doc.DocId;
            }

            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }

            vm.PlayersCount = sectionAlias.Equals(SectionAliases.Tennis)
                ? GetPlayersCountForTennis(league)
                : playersCount;

            vm.OfficialsCount = jobsRepo.CountOfficialsInLeague(id);
            vm.IsTeamCheck = league.IsTeam;
            vm.NoTeamRegistration = league.NoTeamRegistration ?? false;

            vm.TeamsCount = sectionAlias.Equals(SectionAliases.Tennis)
                ? league.TeamRegistrations.Count(r => !r.IsDeleted)
                : league.LeagueTeams.Count;
            vm.IsOfficialsFeatureEnabled = league?.ClubId != null ? league?.Club?.IsReportsEnabled ?? false : league?.Union?.IsReportsEnabled ?? false;
            if (vm.IsOfficialsFeatureEnabled && sectionAlias == GamesAlias.Athletics && union?.IsOfficialSettingsChecked == true) {
                vm.IsOfficialsFeatureEnabled = false;
            }

            #region Set official settings
            var refereeSettings = league.LeagueOfficialsSettings
                .FirstOrDefault(x => x.JobsRolesId == jobsRepo.GetJobRoleByRoleName(JobRole.Referee).RoleId);
            var spectatorSettings = league.LeagueOfficialsSettings
                .FirstOrDefault(x => x.JobsRolesId == jobsRepo.GetJobRoleByRoleName(JobRole.Spectator).RoleId);
            var deskSettings = league.LeagueOfficialsSettings
                .FirstOrDefault(x => x.JobsRolesId == jobsRepo.GetJobRoleByRoleName(JobRole.Desk).RoleId);

            if (refereeSettings != null)
            {
                vm.RefereeFeePerGame = refereeSettings.PaymentPerGame;
                vm.RefereePaymentCurrencyUnits = (CurrencyUnits)refereeSettings.PaymentPerGameCurrency;
                vm.RefereePaymentForTravel = refereeSettings.PaymentTravel;
                vm.RefereeTravelCurrencyUnits = (CurrencyUnits)refereeSettings.TravelPaymentCurrencyType;
                vm.RefereeTravelMetricUnits = (MetricUnits)refereeSettings.TravelMetricType;
                vm.RateAForTravel = refereeSettings.RateAForTravel;
                vm.RateAPerGame = refereeSettings.RateAPerGame;
                vm.RateBForTravel = refereeSettings.RateBForTravel;
                vm.RateBPerGame = refereeSettings.RateBPerGame;
                vm.RateCForTravel = refereeSettings.RateCForTravel;
                vm.RateCPerGame = refereeSettings.RateCPerGame;
            }

            if (deskSettings != null)
            {
                vm.DeskFeePerGame = deskSettings.PaymentPerGame;
                vm.DeskPaymentCurrencyUnits = (CurrencyUnits)deskSettings.PaymentPerGameCurrency;
                vm.DeskPaymentForTravel = deskSettings.PaymentTravel;
                vm.DeskTravelCurrencyUnits = (CurrencyUnits)deskSettings.TravelPaymentCurrencyType;
                vm.DeskTravelMetricUnits = (MetricUnits)deskSettings.TravelMetricType;
            }

            if (spectatorSettings != null)
            {
                vm.SpectatorFeePerGame = spectatorSettings.PaymentPerGame;
                vm.SpectatorPaymentCurrencyUnits = (CurrencyUnits)spectatorSettings.PaymentPerGameCurrency;
                vm.SpectatorPaymentForTravel = spectatorSettings.PaymentTravel;
                vm.SpectatorTravelCurrencyUnits = (CurrencyUnits)spectatorSettings.TravelPaymentCurrencyType;
                vm.SpectatorTravelMetricUnits = (MetricUnits)spectatorSettings.TravelMetricType;
            }
            #endregion

            var unionId = league.Club?.UnionId ?? league.UnionId;
            ViewBag.SectionName = league?.Club?.Section?.Alias ?? league?.Club?.Union?.Section?.Alias ?? league?.Union?.Section?.Alias ?? string.Empty;

            if(ViewBag.SectionName == GamesAlias.Swimming)
            {
                vm.AuditoriumList = auditoriumsRepo.GetByUnionAndSeason(unionId, league.SeasonId.Value).Select(a => new SelectListItem { Text = $"{a.Name} - {a.Length ?? 0}m - {a.LanesNumber ?? 0} {Messages.Lane}", Value = a.AuditoriumId.ToString(), Selected = a.AuditoriumId == league.CompetitionAuditoriumId}).ToList();
                vm.AuditoriumList.Insert(0, new SelectListItem { Text = Messages.Select });
            }
            if (vm.Section == SectionAliases.Bicycle)
            {
                vm.BicycleDisciplines = new SelectList(db.BicycleCompetitionDisciplines.Where(x => x.UnionId == unionId && x.SeasonId == vm.SeasonId).ToList(), "Id", "Name", vm.BicycleCompetitionDisciplineId);
                vm.Levels = new SelectList(db.CompetitionLevels.Where(x => x.UnionId == unionId && x.SeasonId == vm.SeasonId).ToList(), "id", "level_name", vm.LevelId);
            }

            bool isMartialArts = ((string)ViewBag.SectionName).Equals(SectionAliases.MartialArts, StringComparison.OrdinalIgnoreCase);
            if (unionId.HasValue && unionId == 38 || isMartialArts)
            {
                vm.SelectedClubsIds = !string.IsNullOrEmpty(league.AllowedCLubsIds)
                    ? league.AllowedCLubsIds.Split(',').Select(int.Parse).AsEnumerable()
                    : Enumerable.Empty<int>();
                vm.AllowedClubs = clubsRepo.GetByUnion(unionId.Value, league.SeasonId) ?? Enumerable.Empty<Club>();
            }

            vm.IsMastersCompetition = league.IsMastersCompetition;

            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }

            return PartialView("_Details", vm);

        }

        private int GetPlayersCountForTennis(League league)
        {
            return league.TeamRegistrations.Where(r => !r.IsDeleted)
                .Sum(r => r.Team.TeamsPlayers.Count(tp => tp.IsActive && !tp.User.IsArchive));
        }

        /// <summary>
        /// Edit League details and League official settings
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Details(LeagueDetailsForm frm)
        {
            int maxFileSize = GlobVars.MaxFileSize * 1000;
            var savePath = Server.MapPath(GlobVars.ContentPath + "/league/");

            var league = leagueRepo.GetById(frm.LeagueId);
            UpdateModel(league);

            #region Price

            db.LeaguesPrices.RemoveRange(league.LeaguesPrices);
            db.MemberFees.RemoveRange(league.MemberFees);
            db.HandlingFees.RemoveRange(league.HandlingFees);

            league.LeaguesPrices.Clear();
            league.MemberFees.Clear();
            league.HandlingFees.Clear();

            for (int i = 0; i < frm.LeaguesTeamRegistrationPrice.Count; i++)
            {
                var priceItem = frm.LeaguesTeamRegistrationPrice[i];

                league.LeaguesPrices.Add(new LeaguesPrice
                {
                    EndDate = priceItem.EndDate,
                    League = league,
                    Price = priceItem.Price,
                    PriceType = (int)LeaguePriceType.TeamRegistrationPrice,
                    StartDate = priceItem.StartDate,
                    CardComProductId = priceItem.CardComProductId
                });
            }

            foreach (var priceItem in frm.LeaguesPlayerInsurancePrice)
            {
                league.LeaguesPrices.Add(new LeaguesPrice
                {
                    EndDate = priceItem.EndDate,
                    League = league,
                    Price = priceItem.Price,
                    PriceType = (int)LeaguePriceType.PlayerInsurancePrice,
                    StartDate = priceItem.StartDate,
                    CardComProductId = priceItem.CardComProductId
                });
            }

            foreach (var priceItem in frm.LeaguesPlayerRegistrationPrice)
            {
                league.LeaguesPrices.Add(new LeaguesPrice
                {
                    EndDate = priceItem.EndDate,
                    League = league,
                    Price = priceItem.Price,
                    PriceType = (int)LeaguePriceType.PlayerRegistrationPrice,
                    StartDate = priceItem.StartDate,
                    CardComProductId = priceItem.CardComProductId
                });
            }
            
            foreach (var fee in frm.MemberFees)
            {
                league.MemberFees.Add(new MemberFee
                {
                    StartDate = fee.StartDate,
                    EndDate = fee.EndDate,
                    LeagueId = league.LeagueId,
                    Amount = fee.Price,
                    CardComProductId = fee.CardComProductId
                });
            }

            foreach (var fee in frm.HandlingFees)
            {
                league.HandlingFees.Add(new HandlingFee
                {
                    StartDate = fee.StartDate,
                    EndDate = fee.EndDate,
                    LeagueId = league.LeagueId,
                    Amount = fee.Price,
                    CardComProductId = fee.CardComProductId
                });
            }

            #endregion

            #region Officials settings

            // remove old settings
            db.LeagueOfficialsSettings.RemoveRange(league.LeagueOfficialsSettings);

            league.LeagueOfficialsSettings.Add(new LeagueOfficialsSetting
            {
                JobsRolesId = jobsRepo.GetJobRoleByRoleName(JobRole.Referee).RoleId,
                PaymentPerGame = frm.RefereeFeePerGame,
                PaymentPerGameCurrency = (int)frm.RefereePaymentCurrencyUnits,
                PaymentTravel = frm.RefereePaymentForTravel,
                TravelPaymentCurrencyType = (int)frm.RefereeTravelCurrencyUnits,
                TravelMetricType = (int)frm.RefereeTravelMetricUnits,
                RateAForTravel = frm.RateAForTravel,
                RateBForTravel = frm.RateBForTravel,
                RateCForTravel = frm.RateCForTravel,
                RateAPerGame = frm.RateAPerGame,
                RateBPerGame = frm.RateBPerGame,
                RateCPerGame = frm.RateCPerGame
            });

            league.LeagueOfficialsSettings.Add(new LeagueOfficialsSetting
            {
                JobsRolesId = jobsRepo.GetJobRoleByRoleName(JobRole.Spectator).RoleId,
                PaymentPerGame = frm.SpectatorFeePerGame,
                PaymentPerGameCurrency = (int)frm.SpectatorPaymentCurrencyUnits,
                PaymentTravel = frm.SpectatorPaymentForTravel,
                TravelPaymentCurrencyType = (int)frm.SpectatorTravelCurrencyUnits,
                TravelMetricType = (int)frm.SpectatorTravelMetricUnits
            });

            league.LeagueOfficialsSettings.Add(new LeagueOfficialsSetting
            {
                JobsRolesId = jobsRepo.GetJobRoleByRoleName(JobRole.Desk).RoleId,
                PaymentPerGame = frm.DeskFeePerGame,
                PaymentPerGameCurrency = (int)frm.DeskPaymentCurrencyUnits,
                PaymentTravel = frm.DeskPaymentForTravel,
                TravelPaymentCurrencyType = (int)frm.DeskTravelCurrencyUnits,
                TravelMetricType = (int)frm.DeskTravelMetricUnits
            });

            #endregion

            league.IsDailyCompetition = frm.IsDailyCompetition;
            league.IsSeniorCompetition = frm.IsSeniorCompetition;
            league.IsTeam = frm.IsTeamCheck;
            league.IsCompetitionLeague = frm.IsCompetitionLeague;
            if (frm.AllowedClubsIds != null && frm.AllowedClubsIds.Any())
            {
                league.AllowedCLubsIds = string.Join(",", frm.AllowedClubsIds.Distinct());
            }
            else
            {
                league.AllowedCLubsIds = null;
            }

            if (frm.MinParticipationReq != null && frm.MinParticipationReq.HasValue)
            {
                league.MinParticipationReq = frm.MinParticipationReq;
            }
            var imageFile = GetPostedFile("ImageFile");
            if (imageFile != null)
            {
                if (imageFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("ImageFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(imageFile, "img");
                    if (newName == null)
                    {
                        ModelState.AddModelError("ImageFile", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(league.Image))
                            FileUtil.DeleteFile(savePath + league.Image);

                        league.Image = newName;
                    }
                }
            }

            league.CompetitionAuditoriumId = frm.AuditoriumId;

            var logoFile = GetPostedFile("LogoFile");
            if (logoFile != null)
            {
                if (logoFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("LogoFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(logoFile, "logo");
                    if (newName == null)
                    {
                        ModelState.AddModelError("LogoFile", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(league.Logo))
                            FileUtil.DeleteFile(savePath + league.Logo);

                        league.Logo = newName;
                    }
                }
            }

            var docFile = GetPostedFile("DocFile");
            if (docFile != null)
            {
                if (docFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("DocFile", Messages.FileSizeError);
                }
                bool isValid = SaveDocument(docFile, frm.LeagueId);
                if (!isValid)
                {
                    ModelState.AddModelError("DocFile", Messages.FileError);
                }
            }

            league.IsMastersCompetition = frm.IsMastersCompetition;
            if (ModelState.IsValid)
            {
                leagueRepo.Save();
                TempData["Saved"] = true;
                if(!league.ClubId.HasValue)
                    playersRepo.UpdateAllClubsBalance(league.LeagueId, AdminId);
            }
            else
            {
                TempData["ViewData"] = ViewData;
            }
            var section = leagueRepo.GetSectionAlias(league.LeagueId);
            return RedirectToAction("Details", new
            {
                id = league.LeagueId,
                isTennisCompetition = section?.Equals(GamesAlias.Tennis) == true && league.EilatTournament == true ? 1 : 0
            });
        }

        [HttpPost]
        public ActionResult DeleteGallery(int leagueId, String filename)
        {
            string dirPath = ConfigurationManager.AppSettings["LeagueUrl"] + "\\" + leagueId + "\\" + filename.Split('/')[1];

            if (System.IO.File.Exists(dirPath))
            {
                System.IO.File.Delete(dirPath);
            }

            return RedirectToAction("LeagueGalleries", new { leagueId });
        }



        public void DownloadCompetitionResultsForm()
        {
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet(Messages.CompetitionResults);

                var columnCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(1, columnCounter).Value = value;
                    columnCounter++;
                });

                #region Excel header

                addCell($"{Messages.AthleteNumber}");
                addCell($"* {Messages.IdentNum}/{Messages.PassportNum}");
                addCell($"* {Messages.FirstName}");
                addCell($"* {Messages.LastName}");
                addCell($"{Messages.ClubName}");
                addCell($"* {Messages.BirthDay}");
                addCell($"{Messages.Heat}");
                addCell($"{Messages.Lane}");
                addCell($"* {Messages.Result}");
                addCell($"{Messages.Wind}");

                #endregion

                ws.Column(9).Style.NumberFormat.SetFormat("@");
                ws.Column(2).Style.NumberFormat.SetFormat("@");
                ws.Columns().AdjustToContents();

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment;filename={Messages.Template}_{Messages.ImportCompetitionResults}.xlsx");

                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }



        [HttpGet]
        public ActionResult ImportCompetitionResults(int DisciplineId)
        {
            ViewBag.DisciplineId = DisciplineId;
            return PartialView("Disciplines/_ImportCompetitionResults", new ImportViewModelBasic());
        }



        [HttpPost]
        public ActionResult ImportCompetitionResults(ImportViewModelBasic model, int DisciplineId)
        {
            CompetitionDiscipline competitionDiscipline = disciplinesRepo.GetCompetitionDisciplineById(DisciplineId);
            var discipline = disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);

            if (competitionDiscipline == null || discipline == null) {
                model.Result = ImportPlayersResult.Error;
                model.ResultMessage = Messages.ImportPlayers_ImportException;
                model.ExceptionMessage = "competitionId is erroneous somehow.";
                return PartialView("Disciplines/_ImportCompetitionResults", model);
            }
            try
            {
                if (model.ImportFile != null)
                {
                    var importHelper = new ImportExportPlayersHelper(db);
                    List<ImportCompetitionResultsModel> correctRows = null;
                    List<ImportCompetitionResultsModel> validationErrorRows = null;
                    
                    importHelper.ExtractCompetitionResultsData(model.ImportFile.InputStream, out correctRows, out validationErrorRows);

                    if (correctRows.Count > 0)
                    {
                        List<ImportCompetitionResultsModel> importErrorRows = null;

                        IEnumerable<CompetitionDisciplineRegistration> registrations = competitionDiscipline.CompetitionDisciplineRegistrations.AsEnumerable();
                        var players = disciplinesRepo.GetAllClubsCompetitionDisciplinePlayers(competitionDiscipline, competitionDiscipline.League.SeasonId.Value, SectionAliases.Athletics);

                        model.SuccessCount = importHelper.ImportCompetitionResults(correctRows, out importErrorRows, players, registrations, competitionDiscipline.Id, discipline.Format.Value);

                        validationErrorRows.AddRange(importErrorRows);

                        if (validationErrorRows.Count > 0 && model.SuccessCount > 0)
                        {
                            CreateErrorImportFile(importHelper, model, validationErrorRows);
                            model.Result = ImportPlayersResult.PartialyImported;
                            model.ErrorCount = validationErrorRows.Count;
                            model.ResultMessage = Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, $"{Messages.Athletes} {Messages.Numbers.ToLower()}");
                        }
                        else if (validationErrorRows.Count == 0 && model.SuccessCount > 0)
                        {
                            model.Result = ImportPlayersResult.Success;
                        }
                        else
                        {
                            CreateErrorImportFile(importHelper, model, validationErrorRows);
                            model.Result = ImportPlayersResult.Error;
                            model.ResultMessage = Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, Messages.Results);
                        }

                        model.ErrorCount = validationErrorRows.Count;
                    }
                    else
                    {
                        model.Result = ImportPlayersResult.Error;
                        model.ResultMessage = Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, Messages.Results);
                        model.SuccessCount = 0;
                        model.DuplicateCount = 0;
                        model.ErrorCount = correctRows.Count - model.SuccessCount;

                        CreateErrorImportFile(importHelper, model, validationErrorRows);
                    }
                }
                else
                {
                    model.Result = ImportPlayersResult.Error;
                    model.ResultMessage = Messages.ImportPlayers_ChooseFile;
                }

                return PartialView("Disciplines/_ImportCompetitionResults", model);
            }
            catch (Exception ex)
            {
                model.Result = ImportPlayersResult.Error;
                model.ResultMessage = Messages.ImportPlayers_ImportException;
                model.ExceptionMessage = ex.Message;

                return PartialView("Disciplines/_ImportCompetitionResults", model);
            }
        }

        private void CreateErrorImportFile(ImportExportPlayersHelper importHelper, ImportViewModelBasic model, List<ImportCompetitionResultsModel> validationErrorRows)
        {
            var culture = getCulture();
            byte[] errorFileBytes = null;
            using (var errorFile = importHelper.BuildErrorFileForImportingCompetitionResults(validationErrorRows, culture))
            {
                errorFileBytes = new byte[errorFile.Length];
                errorFile.Read(errorFileBytes, 0, errorFileBytes.Length);
            }

        Session.Remove(ImportPlayersErrorResultSessionKey);
        Session.Remove(ImportPlayersErrorResultFileNameSessionKey);
        Session.Add(ImportPlayersErrorResultSessionKey, errorFileBytes);
        Session.Add(ImportPlayersErrorResultFileNameSessionKey, model.ImportFile.FileName);
    }

        [HttpGet]
        public ActionResult DownloadPartiallyImportResults()
        {
            var fileByteObj = Session[ImportPlayersErrorResultSessionKey];
            if (fileByteObj == null)
            {
                throw new FileNotFoundException();
            }

            var fileBytes = (byte[])fileByteObj;
            var fileName = Session[ImportPlayersErrorResultFileNameSessionKey] as string;

            FileInfo fi = new FileInfo(fileName);

            return File(fileBytes, "application/octet-stream", string.Format("{0}-{2}{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension, Messages.ImportPlayers_OutputFilePrefix));
        }
        public ActionResult LeagueGalleries(int leagueId)
        {
            TeamGalleryModel result = new TeamGalleryModel();
            string dirPath = ConfigurationManager.AppSettings["LeagueUrl"] + "\\" + leagueId;
            if (!Directory.Exists(dirPath))
            {
                return null;
            }

            result.TeamGalleries = new List<TeamGalleriesForm>();

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
                        var galleryForm = new TeamGalleriesForm();
                        var elem = new TeamGalleryForm();
                        elem.Created = info.CreationTime;
                        elem.url = leagueId + "/" + info.Name;
                        elem.User = new GalleryUserModel
                        {
                            Id = user.UserId,
                            Name = user.FullName != null ? user.FullName : user.UserName,
                            Image = user.Image
                        };
                        galleryForm.TeamGallery = elem;
                        galleryForm.TeamId = leagueId;
                        result.TeamGalleries.Add(galleryForm);
                    }

                }
                catch (Exception e)
                {
                    continue;
                }

                // Do something with the Folder or just add them to a list via nameoflist.add();
            }
            return PartialView("LeagueGallery", result);
        }

        public ActionResult DeleteImage(int leagueId, string image)
        {
            DataEntities db = new DataEntities();
            var item = db.Leagues.FirstOrDefault(x => x.LeagueId == leagueId);
            if (item == null || string.IsNullOrEmpty(image))
                return RedirectToAction("Edit", new { id = leagueId });
            if (image == "Image")
            {
                item.Image = null;
            }
            if (image == "Logo")
            {
                item.Logo = null;
            }
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = leagueId, seasonId = item.SeasonId });
        }

        public ActionResult ShowDoc(int id)
        {
            var doc = leagueRepo.GetDocById(id);

            Response.AddHeader("content-disposition", "inline;filename=" + doc.FileName + ".pdf");

            return this.File(doc.DocFile, "application/pdf");
        }

        [NonAction]
        private bool SaveDocument(HttpPostedFileBase file, int leagueId)
        {
            string ext = Path.GetExtension(file.FileName).ToLower();

            if (ext != ".pdf")
            {
                return false;
            }

            var doc = leagueRepo.GetTermsDoc(leagueId);
            if (doc == null)
            {
                doc = new LeaguesDoc { LeagueId = leagueId };
                leagueRepo.CreateDoc(doc);
            }

            doc.FileName = file.FileName;

            byte[] docData;
            using (var reader = new BinaryReader(file.InputStream))
            {
                docData = reader.ReadBytes(file.ContentLength);
            }

            doc.DocFile = docData;
            leagueRepo.Save();
            return true;
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
            string ext = Path.GetExtension(file.FileName).ToLower();

            if (!GlobVars.ValidImages.Contains(ext))
                return null;

            string newName = name + "_" + AppFunc.GetUniqName() + ext;

            var savePath = Server.MapPath(GlobVars.ContentPath + "/league/");

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

        public ActionResult LeagueStatistics(int id, int seasonId)
        {
            var statisticsService = new GamesStaticticsService();
            var vm = statisticsService.GetLeagueStatistics(id, seasonId);
            return PartialView("_LeagueStatistics", vm);
        }

        public ActionResult DaysForHosting(int leagueId, int seasonId,
            int? teamId = null)
        {
            var daysForHostingDb = leagueRepo.GetAllDaysForHosting(leagueId, seasonId);
            Mapper.Initialize(cfg => cfg.CreateMap<DaysForHostingDto, DaysForHostingViewModel>());
            var daysForHostingViewModel = Mapper.Map<IEnumerable<DaysForHostingDto>, IEnumerable<DaysForHostingViewModel>>(daysForHostingDb);

            if (teamId.HasValue)
            {
                ViewBag.TeamsHostingDays = leagueRepo.GetHostingDaysDictionary(daysForHostingDb, teamId.Value);
            }

            ViewBag.LeagueId = leagueId;
            ViewBag.SeasonId = seasonId;
            ViewBag.TeamId = teamId;

            ViewBag.DaysSelectListItem = UIHelpers.GetDaysSelectListItem();

            return PartialView("_DaysForHosting", daysForHostingViewModel);
        }



        [HttpPost]
        public ActionResult SetIsAdultLeague(int Id, bool IsAdult)
        {
            leagueRepo.SetIsAdultLeague(Id, IsAdult);
            return Json(new { Success = true });
        }



        [HttpPost]
        public ActionResult CreateDayForHosting(DaysForHostingViewModel vm, int leagueId, int seasonId)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<DaysForHostingViewModel, DaysForHostingDto>());
            var model = Mapper.Map<DaysForHostingViewModel, DaysForHostingDto>(vm);

            leagueRepo.CreateDayForHosting(model, leagueId, seasonId);
            leagueRepo.Save();

            return RedirectToAction(nameof(DaysForHosting), new { leagueId, seasonId });
        }

        [HttpGet]
        public ActionResult DeleteDayForHosting(int id, int leagueId, int seasonId)
        {
            leagueRepo.DeleteDayForHosting(id);
            leagueRepo.Save();

            return RedirectToAction(nameof(DaysForHosting), new { leagueId, seasonId });
        }

        [HttpPost]
        public void ProcessTeamHostingDay(int id, bool isChecked, int teamId)
        {
            leagueRepo.ProcessTeamHostingDay(id, isChecked, teamId);
            leagueRepo.Save();
        }

        public ActionResult Disciplines(int leagueId)
        {
            var league = leagueRepo.GetById(leagueId);
            string sectionName = secRepo.GetByLeagueId(leagueId).Alias;
            ViewBag.CompetitionId = leagueId;
            ViewBag.LeagueId = leagueId;

            int seasonId = (int)league.SeasonId;
            var userJob = jobsRepo.GetLeagueUsersJobs(leagueId, seasonId).Where(x => x.UserId == AdminId).FirstOrDefault();
            string[] permissionIds = null;
            bool doNotFilter = false;
            var refRole = jobsRepo.GetJobRoleByRoleName(JobRole.Referee);
            if (userJob != null && userJob.RoleId == refRole.RoleId)
                permissionIds = string.IsNullOrEmpty(userJob.FormatPermission) ? null : userJob.FormatPermission.Split(',');
            else
                doNotFilter = true;


            var vm = new CompetitionDisciplineListModel
            {
                SectionName = sectionName,
                CompetitionId = leagueId,
                ListLeagues = leagueRepo.GetByUnion(league.UnionId ?? 0, seasonId).Where(x => league.LeagueId != x.LeagueId && x.EilatTournament != null && ((bool)x.EilatTournament) == true).ToList()
            };

            if (sectionName != SectionAliases.Bicycle)
            {

                var competitionDisciplines = disciplinesRepo.GetCompetitionDisciplines(leagueId)
                    .Select(c => new CompetitionDisciplineDto
                    {
                        Id = c.Id,
                        DisciplineId = c.DisciplineId,
                        CompetitionId = c.CompetitionId,
                        CategoryId = c.CategoryId,
                        MaxSportsmen = c.MaxSportsmen,
                        MinResult = c.MinResult,
                        StartTime = c.StartTime,
                        IsMultiBattle = c.IsMultiBattle,
                        IsForScore = c.IsForScore,
                        SectionAlias = c.League.Union.Section.Alias,
                        RegistrationsCount = c.CompetitionDisciplineRegistrations.Count,
                        IsResultsManualyRanked = c.IsResultsManualyRanked,
                        CategoryName = c.CompetitionAge?.age_name,
                        DistanceName = c.RowingDistance?.Name,
                        Heats = GetHeatDtos(c.Id, seasonId),
                        HeatsGenerated = c.HeatsGenerated ?? false
                    }).ToList();
                ViewBag.IsItMultiBattleType = false;
                var isGoldenSpike = false;
                var resultCompetitionDispciplines = new List<CompetitionDisciplineDto>();
                if (competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Athletics || competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Swimming || competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Rowing)
                {
                    foreach (var competitionDiscipline in competitionDisciplines)
                    {
                        if (competitionDiscipline.DisciplineId.HasValue)
                        {
                            var discipline = disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                            //user must have permissions set
                            if (!doNotFilter)
                            {
                                if (permissionIds != null)
                                {
                                    var format = discipline.Format != null ? discipline.Format.ToString() : "-1";
                                    if (permissionIds.FirstOrDefault(x => x == format) == null) continue;
                                }
                                else continue;
                            }
                            competitionDiscipline.DisciplineName = $"{discipline.Name} {LangHelper.GetDisciplineClassById(discipline.Class ?? 0)}";
                            competitionDiscipline.Format = discipline.Format;
                            if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && (discipline.DisciplineType == "decathlon" || discipline.DisciplineType == "heptathlon"))
                            {
                                ViewBag.IsItMultiBattleType = true;
                            }
                            if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && (discipline.DisciplineType == "GoldenSpikesU14" || discipline.DisciplineType == "GoldenSpikesU16"))
                            {
                                isGoldenSpike = true;
                            }

                            resultCompetitionDispciplines.Add(competitionDiscipline);
                        }
                    }
                    resultCompetitionDispciplines = resultCompetitionDispciplines.OrderBy(d => d.DisciplineName).ToList();
                }
                else
                {
                    resultCompetitionDispciplines = competitionDisciplines.OrderBy(d => d.DisciplineName).ToList();
                }
                var competitionSessions = disciplinesRepo.GetCompetitionSessions(leagueId);


                var sportsmen = playersRepo.GetPlayersDisciplineRegistrations(leagueId)
                    .OrderBy(s => s.TeamTitle)
                    .ThenBy(s => s.WeightDeclaration)
                    .ThenBy(s => s.GenterId)
                    .ToList();

                ViewBag.Sportsmen = sportsmen;

                List<RefereeShortDto> referees = leagueRepo.GetRefereesForCompetitionRegistration(leagueId, seasonId).ToList();
                var prevId = 0;
                List<RefereeShortDto> newList = new List<RefereeShortDto>();
                foreach (var refer in referees)
                {
                    RefereeCompetitionRegistration refComp = db.RefereeCompetitionRegistrations.Where(r => r.UserId == refer.UserId && r.LeagueId == leagueId).FirstOrDefault();
                    if (refComp != null)
                    {
                        refer.RegId = refComp.Id;
                        refer.SessionIds = refComp.SessionIds;
                    }

                    if (prevId != refer.UserId)
                    {
                        newList.Add(refer);
                    }

                    prevId = refer.UserId;
                }
                ViewBag.Referees = newList;
                ViewBag.IsItMultiBattleType = ViewBag.IsItMultiBattleType || isGoldenSpike;

                vm.Disciplines = resultCompetitionDispciplines;
                vm.Sessions = competitionSessions.ToList();
            }
            else
            {
                vm.CompetitionExperties = disciplinesRepo.GetCompetitionExperties(leagueId);
                ViewBag.CompetitionHeats = new SelectList(db.BicycleCompetitionHeats.Where(x => x.UnionId == league.UnionId && x.SeasonId == seasonId).ToList(), "Id", "Name");
                ViewBag.UnionHeats = new SelectList(disciplinesRepo.GetAllByUnionId(league.UnionId.Value), "DisciplineId", "Name");
                ViewBag.CompetitionLevels = leagueRepo.GetCompetitionLevels(league.UnionId);
            }
            
            ViewBag.SeasonId = league?.SeasonId;
            ViewBag.ClubId = league?.ClubId;
            bool isCompetitionInAthleticLeague = league != null && league.AthleticLeagueId.HasValue && league.AthleticLeagueId.Value > 0;
            ViewBag.IsCompetitionWithScore = (league?.IsCompetitionLeague ?? false) || (isCompetitionInAthleticLeague);
           
            ViewBag.CompetitionStartDate = league.LeagueStartDate.HasValue ? league.LeagueStartDate.Value.ToShortDateString()  : "";

            return PartialView("_Disciplines", vm);
        }

        public ActionResult SearchForCompRegistration(string qry, int leagueId)
        {
            var league = leagueRepo.GetById(leagueId);
            List<TeamsPlayer> menList = playersRepo.GetAllPlayersForWeightLiftingRegistrationBySession(league?.SeasonId).ToList();
            // List<CompDiscRegDTO> sportsmen = playersRepo.GetPlayersDisciplineRegistrations(leagueId).OrderBy(s => s.UserId).ThenBy(s => s.WeightDeclaration).ThenBy(s => s.GenterId).ToList();
            List<PlayerShortDTO> res = new List<PlayerShortDTO>();
            foreach (TeamsPlayer man in menList)
            {
                if (!man.User.FullName.Contains(qry) && !man.User.IdentNum.Contains(qry))
                    continue;

                var player = playersRepo.GetPlayersDisciplineRegistration(leagueId, man.UserId);
                if (player != null)
                {
                    continue;
                }

                PlayerShortDTO newMan = new PlayerShortDTO();
                newMan.UserId = man.UserId;
                newMan.UserName = man.User.FullName;
                newMan.IdentNum = man.User.IdentNum;
                res.Add(newMan);
            }
            /*foreach (CompDiscRegDTO man in sportsmen)
            {
                if (man.SessionId != null)
                    continue;
                if (!man.Name.Contains(qry) && !Convert.ToString(man.RegistrationId).Contains(qry))
                    continue;

                res.Add(man);
            }*/

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RegisterSportsmanToSession(int sessionId, int leagueId, int userId)
        {
            return Json(new { Success = true });
        }

        public ActionResult DisplayWeightliftSessionRegistrations(int SessionId, int LeagueId)
        {
            var playersInSession = playersRepo.GetPlayersDisciplineRegistrationsBySession(LeagueId, SessionId).ToList();
            ViewBag.SessionId = SessionId;
            ViewBag.CompetitionId = LeagueId;
            ViewBag.CompetitionName = leagueRepo.GetById(LeagueId).Name;
            return PartialView("Disciplines/_SessionsRegistrationList", playersInSession);
        }

        [HttpPost]
        public ActionResult EditSession(int SessionId, int LeagueId, int SessionNum, DateTime? StartTime, DateTime? WeightStartTime, DateTime? WeightFinishTime)//, int SessionNum, DateTime? StartTime, DateTime? WeightStartTime, DateTime? WeightFinishTime
        {
            disciplinesRepo.UpdateWeightliftingSession(SessionId, LeagueId, SessionNum, StartTime, WeightStartTime, WeightFinishTime);
            return Json(new { });
        }
        


        public ActionResult UpdateWeightInWeightliftSessionRegistration(int RegistrationId, int SessionId, int LeagueId, decimal? RegistrationWeightDeclaration, decimal? RegistrationWeight, int? RegistrationLifting, int? RegistrationPush) {
            disciplinesRepo.UpdateRegistrationWeight(RegistrationId, RegistrationWeight, RegistrationWeightDeclaration);
            disciplinesRepo.UpdateRegistrationLiftingPush(RegistrationId, RegistrationLifting, RegistrationPush);
            return RedirectToAction("DisplayWeightliftSessionRegistrations", new { SessionId = SessionId, LeagueId = LeagueId });
        }

        public ActionResult CreateNewSession(int competitionId, WeightLiftingSessionForm form)
        {
            League league = leagueRepo.GetById(competitionId);
            if (ModelState.IsValid)
            {
                disciplinesRepo.CreateWeightliftingSession(competitionId, form.StartTime, form.WeightStartTime, form.WeightFinishTime);
            }
            else
            {
                ViewBag.SessionRequiresAllDates = true;
            }
            ViewBag.Sportsmen = playersRepo.GetPlayersDisciplineRegistrations(competitionId).OrderBy(s => s.TeamTitle).ThenBy(s => s.WeightDeclaration).ThenBy(s => s.GenterId);
            if (league.ClubId.HasValue)
            {
                int clubId = (int)league.ClubId;
                int seasonId = (int)league.SeasonId;
                Club club = clubsRepo.GetById(clubId);
                ViewBag.Referees = leagueRepo.GetClubsRefereeDtos(club, league.LeagueId, seasonId).ToList();
            }
            List<RefereeShortDto> referees = leagueRepo.GetRefereesForCompetitionRegistration(competitionId, (int)league.SeasonId).ToList();
            var prevId = 0;
            List<RefereeShortDto> newList = new List<RefereeShortDto>();
            foreach (var refer in referees)
            {
                RefereeCompetitionRegistration refComp = db.RefereeCompetitionRegistrations.Where(r => r.UserId == refer.UserId && r.LeagueId == competitionId).FirstOrDefault();
                if (refComp != null)
                {
                    refer.RegId = refComp.Id;
                    refer.SessionIds = refComp.SessionIds;
                }

                if (prevId != refer.UserId)
                {
                    newList.Add(refer);
                }

                prevId = refer.UserId;
            }

            ViewBag.Referees = newList;
            ViewBag.CompetitionId = competitionId;
            var competitionSessions = disciplinesRepo.GetCompetitionSessions(competitionId).ToList();
            var data = RenderPartialViewToString("Disciplines/_WeightLiftingSessionsTable", competitionSessions);
            return Json(new { data = data });
        }


        public ActionResult DeleteWeightLiftingSession(int Id, int competitionId, int seasonId)
        {
            var playersOnSession = playersRepo.GetPlayersDisciplineRegistrationsBySession(competitionId, Id).Count();
            List<RefereeShortDto> referees = leagueRepo.GetRefereesForCompetitionRegistration(competitionId, seasonId).ToList();
            var prevId = 0;
            List<RefereeShortDto> newList = new List<RefereeShortDto>();
            foreach (var refer in referees)
            {
                RefereeCompetitionRegistration refComp = db.RefereeCompetitionRegistrations.Where(r => r.UserId == refer.UserId && r.LeagueId == competitionId).FirstOrDefault();
                if (refComp != null)
                {
                    refer.RegId = refComp.Id;
                    refer.SessionIds = refComp.SessionIds;
                }

                if (prevId != refer.UserId)
                {
                    newList.Add(refer);
                }

                prevId = refer.UserId;
            }

            ViewBag.Referees = newList;

            if (playersOnSession > 0)
            {
                ViewBag.Sportsmen = playersRepo.GetPlayersDisciplineRegistrations(competitionId).OrderBy(s => s.TeamTitle).ThenBy(s => s.WeightDeclaration).ThenBy(s => s.GenterId);
                ViewBag.SessionHasRegistersCantDelete = true;
                ViewBag.CompetitionId = competitionId;
                var competitionSessions = disciplinesRepo.GetCompetitionSessions(competitionId).ToList();
                return PartialView("Disciplines/_WeightLiftingSessionsTable", competitionSessions);
            }
            else
            {
                disciplinesRepo.DeleteWeightliftingSession(Id);
                ViewBag.CompetitionId = competitionId;
                ViewBag.Sportsmen = playersRepo.GetPlayersDisciplineRegistrations(competitionId).OrderBy(s => s.TeamTitle).ThenBy(s => s.WeightDeclaration).ThenBy(s => s.GenterId);
                var competitionSessions = disciplinesRepo.GetCompetitionSessions(competitionId).ToList();
                return PartialView("Disciplines/_WeightLiftingSessionsTable", competitionSessions);
            }
        }

        public ActionResult UpdateRegistrationToItsSession2(int competitionId, CompDiscRegDTO form)
        {
            bool res = true;
            var user = usersRepo.GetByIdentityNumber(form.IdentNum);
            var player = playersRepo.GetPlayersDisciplineRegistration(competitionId, user.UserId);
            var league = leagueRepo.GetById(competitionId);
            if (player == null)
            {
                CompetitionDiscipline cd = disciplinesRepo.GetCompetitionDisciplines(competitionId).FirstOrDefault();
                var isAllowed = User.IsInAnyRole(AppRole.Admins) || JobRole.UnionManager.Equals(usersRepo.GetTopLevelJob(AdminId));
                var currentRegistrationCount = 0;
                var maxRegistrations = cd.League.MaxRegistrations;
                List<int> ids = new List<int>();
                ids.Add(user.UserId);

                Club club = clubsRepo.GetByUnion(league.UnionId ?? 0, league.SeasonId).FirstOrDefault();

                res = playersRepo.RegisterSportsmenInCompetitionDiscipline(ids, club.ClubId, cd.Id, isAllowed, maxRegistrations, currentRegistrationCount);
                if (!res)
                {
                    return Json(new { Success = false });
                }
                player = playersRepo.GetPlayersDisciplineRegistration(competitionId, user.UserId);
            }
            int sessionId = form.SessionId ?? 0;
            disciplinesRepo.UpdateDisciplineRegistrationSession(player.RegistrationId, form.SessionId);

            string clubname = "";
            if (league.Club != null)
            {
                clubname = league?.Club?.Name;
            }
            string birthStr = user.BirthDay.HasValue ? user.BirthDay.Value.ToShortDateString() : "";

            return Json(new {
                Success = res,
                regId = player.RegistrationId,
                sessionId = form.SessionId,
                leagueId = competitionId,
                fullname = user.FullName,
                name = player.Name,
                birthdate = birthStr,
                clubname = clubname,
                gender = LangHelper.GetGenderCharById(player.GenterId ?? 0),
                teamtitle = player.TeamTitle,
                declare = player.WeightDeclaration ?? 0,
                danger = player.IsWeightOk ? "" : "alert-danger",
                weight = player.Weight.HasValue ? player.Weight.ToString() : ""
            });
        }
        public ActionResult UpdateRegistrationToItsSession(int competitionId, CompDiscRegDTO form)
        {
            disciplinesRepo.UpdateDisciplineRegistrationSession(form.RegistrationId, form.SessionId);

            return Json(new { Success = true });
        }

        public ActionResult UpdateRegistrationRefereeToItsSession(int competitionId, RefereeShortDto form)
        {
            var league = leagueRepo.GetById(competitionId);
            leagueRepo.UpdateRefereeRegistration(form.UserId, form.SessionId, competitionId, league.SeasonId ?? 0, league.UnionId ?? 0, form.isAdd);

            return Json(new { Success = true });
        }

        public ActionResult CreateDiscipline(int competitionId)
        {
            ViewBag.CompetitionId = competitionId;
            
            int? unionId = db.Leagues.Where(c => c.LeagueId == competitionId).Select(c => c.UnionId).FirstOrDefault();
            int? sectionId = db.Unions.Where(c => c.UnionId == unionId).Select(c => c.SectionId).FirstOrDefault();
            var sectionAlias = secRepo.GetById(sectionId.Value).Alias;

            if (sectionAlias != GamesAlias.Bicycle)
            {
                var competitionCategories = leagueRepo.GetCompetitionCategories(competitionId);
                if (sectionAlias == GamesAlias.Athletics)
                {
                    competitionCategories = competitionCategories.Where(x => x.IsHidden != true).ToList();
                }
                ViewBag.CategoryList = new SelectList(competitionCategories.OrderBy(c => c.age_name), nameof(CompetitionAge.id), nameof(CompetitionAge.age_name));
                ViewBag.DisciplineList = new SelectList(disciplinesRepo.GetBySection((int)sectionId, (int)unionId).OrderBy(c => c.Name), nameof(Discipline.DisciplineId), nameof(Discipline.Name));
                ViewBag.DistanceList = new SelectList(leagueRepo.GetCompetitionRowingDistances(competitionId).OrderBy(c => c.Name), nameof(RowingDistance.Id), nameof(RowingDistance.Name));
            }
            else
            {
                ViewBag.DisciplineExpertiseList = new SelectList(db.DisciplineExpertises.Where(x => x.BicycleCompetitionDiscipline.UnionId == unionId.Value).OrderBy(c => c.Name), nameof(DisciplineExpertise.Id), nameof(DisciplineExpertise.Name));
            }
            return PartialView("Disciplines/_Create", new CompetitionDisciplineViewModel
            {
                CompetitionId = competitionId,
                UseAllProps = (!sectionId.HasValue || sectionAlias == GamesAlias.Athletics) && sectionAlias != GamesAlias.Bicycle,
                IsSwimming = sectionAlias == GamesAlias.Swimming,
                IsRowing = sectionAlias == GamesAlias.Rowing,
                IsBicycle = sectionAlias == GamesAlias.Bicycle,
                IsClimbing = sectionAlias == GamesAlias.Climbing
            });
        }

        [HttpPost]
        public ActionResult SetIsCompetitionLeague(int LeagueId, bool IsLeague)
        {
            leagueRepo.SetIsCompetitionLeague(LeagueId, IsLeague);
            return Json(new { Success=true});
        }

        [HttpPost]
        public ActionResult SetIsTeam(int LeagueId, bool IsTeam)
        {
            leagueRepo.SetIsTeam(LeagueId, IsTeam);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult SetIsFieldCompetition(int LeagueId, bool IsLeague)
        {
            leagueRepo.SetIsFieldCompetition(LeagueId, IsLeague);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult RegisterCompetitionToAthleticLeague(int LeagueId, int competitionId, bool isChecked)
        {
            leagueRepo.RegisterCompetitionToAthleticLeague(LeagueId, competitionId, isChecked);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult RegisterClubToAthleticLeague(int leagueId, int typeId, int clubId, bool isChecked, int genderId)
        {
            if (genderId == 0)
            {
                leagueRepo.RegisterClubToAthleticLeagueF(leagueId, typeId, clubId, isChecked);
            }
            else
            {
                leagueRepo.RegisterClubToAthleticLeague(leagueId, typeId, clubId, isChecked);
            }
            return Json(new { Success = true });
        }


        [HttpPost]
        public ActionResult CreateDiscipline(CompetitionDisciplineViewModel vm)
        {
            int? unionId = db.Leagues.Where(c => c.LeagueId == vm.CompetitionId).Select(c => c.UnionId).FirstOrDefault();
            int? sectionId = db.Unions.Where(c => c.UnionId == unionId).Select(c => c.SectionId).FirstOrDefault();
            if(!vm.IsBicycle)
            {
                if (string.IsNullOrEmpty(vm.CategoryId)) ModelState.AddModelError("CategoryId", Messages.FieldIsRequired);                
            }
            else
            {
                if (string.IsNullOrEmpty(vm.DisciplineExpertiseId)) ModelState.AddModelError("DisciplineExpertiseId", Messages.FieldIsRequired);
                ModelState.Remove("DisciplineId");
            }
            if (!ModelState.IsValid)
            {
                if(vm.IsBicycle)
                {
                    ViewBag.DisciplineExpertiseList = new SelectList(db.DisciplineExpertises.Where(x => x.BicycleCompetitionDiscipline.UnionId == unionId.Value).OrderBy(c => c.Name), nameof(DisciplineExpertise.Id), nameof(DisciplineExpertise.Name));
                }
                else
                {
                    ViewBag.CategoryList = new SelectList(leagueRepo.GetCompetitionCategories(vm.CompetitionId), nameof(CompetitionAge.id), nameof(CompetitionAge.age_name));
                    ViewBag.DistanceList = new SelectList(leagueRepo.GetCompetitionRowingDistances(vm.CompetitionId).OrderBy(c => c.Name), nameof(RowingDistance.Id), nameof(RowingDistance.Name));
                    ViewBag.DisciplineList = new SelectList(disciplinesRepo.GetBySection((int)sectionId, (int)unionId), nameof(Discipline.DisciplineId), nameof(Discipline.Name));

                }
                ViewBag.CompetitionId = vm.CompetitionId;
                return PartialView("Disciplines/_Create", vm);
            }

            if (!vm.IsBicycle)
            {
                var catList = vm.CategoryId.Split(',').Select(int.Parse).ToList();

                List<int> distList = new List<int>();
                if (!string.IsNullOrWhiteSpace(vm.DistanceId))
                {
                    distList = vm.DistanceId.Split(',').Select(int.Parse).ToList();
                }
                List<int> boatTypesList = new List<int>();
                if (!string.IsNullOrWhiteSpace(vm.BoatTypesId))
                {
                    boatTypesList = vm.BoatTypesId.Split(',').Select(int.Parse).ToList();
                }
                foreach (var cat in catList)
                {
                    if (distList.Count() == 0)
                    {
                        if (boatTypesList.Count == 0)
                            disciplinesRepo.CreateCompetitionDiscipline(new CompetitionDiscipline
                            {
                                DisciplineId = vm.DisciplineId,
                                CategoryId = cat,
                                CompetitionId = vm.CompetitionId,
                                MinResult = vm.MinResult,
                                IncludeRecordInStartList = secRepo.GetById(sectionId.Value).Alias == GamesAlias.Athletics ? true : false,
                                MaxSportsmen = vm.MaxSportsmen,
                                StartTime = vm.StartTime,
                                IsResultsManualyRanked = vm.IsResultsManualyRanked
                            });
                        else
                        {
                            foreach (var boatType in boatTypesList)
                            {
                                disciplinesRepo.CreateCompetitionDiscipline(new CompetitionDiscipline
                                {
                                    DisciplineId = boatType,
                                    CategoryId = cat,
                                    CompetitionId = vm.CompetitionId,
                                    MinResult = vm.MinResult,
                                    MaxSportsmen = vm.MaxSportsmen,
                                    StartTime = vm.StartTime,
                                    IsResultsManualyRanked = vm.IsResultsManualyRanked,
                                    TeamRegistration = 1
                                });
                            }
                        }
                    }
                    else
                    {
                        foreach (var dist in distList)
                        {
                            if (boatTypesList.Count == 0)
                                disciplinesRepo.CreateCompetitionDiscipline(new CompetitionDiscipline
                                {
                                    DisciplineId = vm.DisciplineId,
                                    CategoryId = cat,
                                    CompetitionId = vm.CompetitionId,
                                    MinResult = vm.MinResult,
                                    MaxSportsmen = vm.MaxSportsmen,
                                    StartTime = vm.StartTime,
                                    IsResultsManualyRanked = vm.IsResultsManualyRanked,
                                    DistanceId = dist
                                });
                            else
                            {
                                foreach (var boatType in boatTypesList)
                                {
                                    disciplinesRepo.CreateCompetitionDiscipline(new CompetitionDiscipline
                                    {
                                        DisciplineId = boatType,
                                        CategoryId = cat,
                                        CompetitionId = vm.CompetitionId,
                                        MinResult = vm.MinResult,
                                        MaxSportsmen = vm.MaxSportsmen,
                                        StartTime = vm.StartTime,
                                        IsResultsManualyRanked = vm.IsResultsManualyRanked,
                                        DistanceId = dist,
                                        TeamRegistration = 1
                                    });
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var expList = vm.DisciplineExpertiseId.Split(',').Select(int.Parse).ToList();

                foreach(var expId in expList)
                {
                    disciplinesRepo.CreateCompetitionExpertise(new CompetitionExperty
                    {
                        CompetitionId = vm.CompetitionId,
                        DisciplineExpertiseId = expId
                    });
                }
            }

            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult DeleteDiscipline(int id, bool isBicycle)
        {
            if (isBicycle)
            {
                disciplinesRepo.DeleteCompetitionExperty(id);
            }
            else
            {
                disciplinesRepo.DeleteCompetitionDiscipline(id);
            }

            return Json(new { Success = true });
        }



        [HttpPost]
        public ActionResult SetManualPointCalculation(int CompetitionDiscipline, bool isChecked)
        {
            disciplinesRepo.SetManualPointCalculation(CompetitionDiscipline, isChecked);
            return Json(new { Success = true });
        }


        public ActionResult DisciplineLiveResults(int id, int compIdLast = 0)
        {
            var disciplineCompetition = disciplinesRepo.GetCompetitionDisciplineByIdNoTrack(id);
            var discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

            ViewBag.CompetitionId = disciplineCompetition.CompetitionId;
            ViewBag.LeagueId = disciplineCompetition.League.LeagueId;
            int? unionId = disciplineCompetition.League.UnionId;
            int? seasonId = disciplineCompetition.League.SeasonId;
            ViewBag.IsResultsManualyRanked = disciplineCompetition.IsResultsManualyRanked;
            ViewBag.Format = discipline.Format;
            ViewBag.DisciplineName = discipline.Name;
            ViewBag.DisciplineType = discipline.DisciplineType;
            var venue = IAAFScoringPointsService.OutdoorsVenue;
            string[] fieldDisciplineTypes = new[] { "1900m_13_field", "1100m_field", "2500m_field", "1900m_15_field", "4500m_field", "3100m_field" };
            if (fieldDisciplineTypes.Contains(discipline.DisciplineType))
            {
                venue = IAAFScoringPointsService.FieldsVenue;
            }



            var competitionDisciplines = disciplinesRepo.GetCompetitionDisciplines(disciplineCompetition.CompetitionId).Select(c => new CompetitionDisciplineDto
            {
                SectionAlias = c.League.Union.Section.Alias,
                DisciplineId = c.DisciplineId
            }).ToList();
            var isGoldenSpike = 0;
            if (competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Athletics)
            {
                foreach (var competitionDiscipline in competitionDisciplines)
                {
                    if (competitionDiscipline.DisciplineId.HasValue)
                    {
                        var disciplinel = disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                        if (!string.IsNullOrWhiteSpace(disciplinel.DisciplineType) && disciplinel.DisciplineType == "GoldenSpikesU14")
                        {
                            isGoldenSpike = 1;
                            break;
                        }
                        if (!string.IsNullOrWhiteSpace(disciplinel.DisciplineType) && disciplinel.DisciplineType == "GoldenSpikesU16")
                        {
                            isGoldenSpike = 2;
                            break;
                        }
                    }
                }
            }

            ViewBag.IsDisciplineHasRecognizedPointSystem = string.IsNullOrWhiteSpace(discipline.DisciplineType) ? false : (IAAFScoringPointsService.IsCompetitionDisciplineValid(discipline.DisciplineType, venue, disciplineCompetition.CompetitionAge.gender.Value, disciplineCompetition.IsMultiBattle) || isGoldenSpike > 0);
            ViewBag.CompetitionDisciplineId = id;
            ViewBag.CompetitionHeatWindTable = disciplinesRepo.GetCompetitionHeatWindList(id);

            disciplineCompetition.CompetitionDisciplineRegistrations = disciplineCompetition.CompetitionDisciplineRegistrations.OrderBy(r => r.CompetitionResult.FirstOrDefault()?.Lane ?? int.MaxValue).ToList();

            ViewBag.NumberPasses = disciplineCompetition.NumberOfWhoPassesToNextStage;
            if (disciplineCompetition.NumberOfWhoPassesToNextStage.HasValue)
            {
                disciplineCompetition.CompetitionDisciplineRegistrations = disciplineCompetition.CompetitionDisciplineRegistrations.OrderBy(r => r.CompetitionResult.FirstOrDefault()?.LiveSplitOrder ?? int.MaxValue).ToList();
            }

            ViewBag.SeasonId = seasonId;
            if (discipline.Format == 6) {
                return PartialView("Disciplines/_DisciplineLiveResults_6", disciplineCompetition);
            }
            return PartialView("Disciplines/_DisciplineLiveResults", disciplineCompetition);
        }

        

        public ActionResult UpdateCompetitionDisciplineCustomFields6(int id, string fields)
        {
            var listedFields = fields.Split(',').ToList();
            disciplinesRepo.UpdateCompetitionDisciplineCustomFields6(id, listedFields);
            return RedirectToAction("DisciplineLiveResults", new { @id = id });
        }


        public ActionResult AddNewFormat6CustomFieldColumn(int id)
        {
            var disciplineCompetition = disciplinesRepo.GetCompetitionDisciplineById(id);
            disciplineCompetition.AddFormat6CustomFieldsNewColumn();
            disciplinesRepo.Save();
            return RedirectToAction("DisciplineLiveResults", new { @id = id });
        }

        public ActionResult RemoveNewFormat6CustomFieldColumn(int id)
        {
            var disciplineCompetition = disciplinesRepo.GetCompetitionDisciplineById(id);
            disciplineCompetition.RemoveFormat6CustomFieldsNewColumn();
            disciplinesRepo.Save();
            return RedirectToAction("DisciplineLiveResults", new { @id = id });
        }

        public ActionResult DisciplineResults(int id, int compIdLast = 0)
        {
            var disciplineCompetition = disciplinesRepo.GetCompetitionDisciplineByIdNoTrack(id);
            var discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

            ViewBag.CompetitionId = disciplineCompetition.CompetitionId;
            ViewBag.LeagueId = disciplineCompetition.League.LeagueId;
            int? unionId = disciplineCompetition.League.UnionId;
            int? seasonId = disciplineCompetition.League.SeasonId;
            ViewBag.IsResultsManualyRanked = disciplineCompetition.IsResultsManualyRanked;
            ViewBag.Format = discipline.Format;
            ViewBag.DisciplineName = discipline.Name;
            ViewBag.DisciplineType = discipline.DisciplineType;
            var venue = IAAFScoringPointsService.OutdoorsVenue;
            string[] fieldDisciplineTypes = new[] { "1900m_13_field", "1100m_field", "2500m_field", "1900m_15_field", "4500m_field", "3100m_field" };
            if (fieldDisciplineTypes.Contains(discipline.DisciplineType))
            {
                venue = IAAFScoringPointsService.FieldsVenue;
            }



            var competitionDisciplines = disciplinesRepo.GetCompetitionDisciplines(disciplineCompetition.CompetitionId).Select(c => new CompetitionDisciplineDto
            {
                SectionAlias = c.League.Union.Section.Alias,
                DisciplineId = c.DisciplineId
            }).ToList();
            var isGoldenSpike = 0;
            if (competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Athletics)
            {
                foreach (var competitionDiscipline in competitionDisciplines)
                {
                    if (competitionDiscipline.DisciplineId.HasValue)
                    {
                        var disciplinel = disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                        if (!string.IsNullOrWhiteSpace(disciplinel.DisciplineType) && disciplinel.DisciplineType == "GoldenSpikesU14")
                        {
                            isGoldenSpike = 1;
                            break;
                        }
                        if (!string.IsNullOrWhiteSpace(disciplinel.DisciplineType) && disciplinel.DisciplineType == "GoldenSpikesU16")
                        {
                            isGoldenSpike = 2;
                            break;
                        }
                    }
                }
            }

            ViewBag.IsDisciplineHasRecognizedPointSystem = string.IsNullOrWhiteSpace(discipline.DisciplineType) ? false : (IAAFScoringPointsService.IsCompetitionDisciplineValid(discipline.DisciplineType, venue, disciplineCompetition.CompetitionAge.gender.Value, disciplineCompetition.IsMultiBattle) || isGoldenSpike > 0);
            ViewBag.CompetitionDisciplineId = id;
            ViewBag.CompetitionHeatWindTable = disciplinesRepo.GetCompetitionHeatWindList(id);
            disciplineCompetition.CompetitionDisciplineRegistrations = disciplineCompetition.CompetitionDisciplineRegistrations.OrderBy(r => r.User.FullName).ToList();
            if (compIdLast > 0) {
                var reg = disciplineCompetition.CompetitionDisciplineRegistrations.FirstOrDefault(c => c.Id == compIdLast);
                if (reg != null) {
                    disciplineCompetition.CompetitionDisciplineRegistrations.Remove(reg);
                    disciplineCompetition.CompetitionDisciplineRegistrations.Add(reg);
                }
            }

            ViewBag.SeasonId = seasonId;
            return PartialView("Disciplines/_DisciplinesResults", disciplineCompetition);
        }

        public ActionResult StartList(int id, int resultId = 0, string resultHeatValue = null)
        {
            if (resultId != 0)
            {
                db.CompetitionResults.FirstOrDefault(r => r.Id == resultId).Heat = resultHeatValue;
                db.SaveChanges();
            }

            var disciplineCompetition = disciplinesRepo.GetCompetitionDisciplineById(id);
            Discipline discipline = null;
            if (disciplineCompetition.DisciplineId.HasValue)
            {
                discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);
            }
            ViewBag.SectionAlias = secRepo.GetByLeagueId(disciplineCompetition.League.LeagueId)?.Alias;
            ViewBag.LeagueId = disciplineCompetition.League.LeagueId;
            ViewBag.CompetitionId = disciplineCompetition.CompetitionId;
            ViewBag.SeasonId = disciplineCompetition.League.SeasonId;
            ViewBag.DisciplineName = discipline?.Name;
            ViewBag.Format = discipline?.Format;
            ViewBag.CompetitionDisciplineId = id;
            ViewBag.CompetitionName = disciplineCompetition.League.Name;
            ViewBag.CompetitionDate = disciplineCompetition.League.LeagueStartDate.HasValue ? disciplineCompetition.League.LeagueStartDate.Value.ToShortDateString() : "";
            ViewBag.HeatStartTimes = disciplineCompetition.CompetitionDisciplineHeatStartTimes.ToList();

            var disciplineRecordId = disciplinesRepo.GetCompetitonDisciplineRecord(disciplineCompetition.DisciplineId ?? 0, disciplineCompetition.CategoryId);


            if (disciplineCompetition.IncludeRecordInStartList)
            {
                if (!disciplineRecordId.HasValue)
                {
                    ViewBag.DisciplineRecord = new DisciplineRecord { CompetitionRecord = disciplineCompetition.CompetitionRecord };
                }
                else
                {
                    var disciplineRecordR = disciplinesRepo.GetDisciplineRecordById(disciplineRecordId.Value);
                    disciplineRecordR.CompetitionRecord = disciplineCompetition.CompetitionRecord;
                    ViewBag.DisciplineRecord = disciplineRecordR;
                    if (disciplineRecordR.SeasonRecords.FirstOrDefault(x =>
                            x.SeasonId == disciplineCompetition.League.SeasonId) != null)
                    {
                        ViewBag.SeasonRecord =
                            disciplineRecordR.SeasonRecords.FirstOrDefault(x =>
                                x.SeasonId == disciplineCompetition.League.SeasonId).SeasonRecord1;
                    }
                }
            }

            var registrationsWithHeat = disciplineCompetition.CompetitionDisciplineRegistrations;
            ViewBag.PlayersCount = registrationsWithHeat.Count();
            var grouped = registrationsWithHeat.OrderBy(r => r.CompetitionResult.FirstOrDefault()?.Lane ?? int.MaxValue).GroupBy(r => string.IsNullOrEmpty(r.CompetitionResult.FirstOrDefault()?.Heat) == false ? r.CompetitionResult.FirstOrDefault()?.Heat : Messages.None).OrderBy(group => group.Key).ToList();
            var groupedByHeat = grouped.Where(g => g.Key != Messages.None).ToList();
            var nonGroup = grouped.FirstOrDefault(g => g.Key == Messages.None);
            if (nonGroup != null)
            {
                groupedByHeat.Add(nonGroup);
            }
            var numericValues = groupedByHeat.Where(r => int.TryParse(r.Key, out _)).ToList();
            numericValues = numericValues.OrderBy(group => int.Parse(group.Key)).ToList();
            var nonNumericValues = groupedByHeat.Where(r => !int.TryParse(r.Key, out _)).ToList();
            groupedByHeat = numericValues.Concat(nonNumericValues).ToList();
            ViewBag.SeasonId = disciplineCompetition.League?.SeasonId;
            return PartialView("Disciplines/_Startlist", groupedByHeat);
        }

        public ActionResult GenerateHeats(int competitionId, int competitionDisciplineId, int seasonId)
        {
            var numberOfLanes = leagueRepo.GetById(competitionId)?.Auditorium?.LanesNumber ?? 0;
            var disciplineId = disciplinesRepo.GetCompetitionDisciplineById(competitionDisciplineId).DisciplineId;
            Dictionary<int, int> userIdRegistrationIdDictionary = disciplinesRepo.GetRegistrationIdUserIdDictionary(competitionDisciplineId);
            var swimmersIds = userIdRegistrationIdDictionary.Select(x => x.Key).ToArray();

            var heats = GenerateHeatsFromSwimmers(swimmersIds, disciplineId, numberOfLanes);
            SaveGeneratedHeats(competitionDisciplineId, heats, userIdRegistrationIdDictionary);
            ViewBag.SeasonId = seasonId;
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult SetHeatIsFinal(int id, bool isFinal)
        {
            disciplinesRepo.UpdateHeatIsFinal(id, isFinal);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult SetHeatStartTime(int heatId, DateTime heatStartTime)
        {
            disciplinesRepo.UpdateHeatStartTime(heatId, heatStartTime);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult DeleteHeat(int heatId)
        {
            var heat = disciplinesRepo.GetHeatById(heatId);
            var competitionDisciplineId = heat?.CompetitionDisciplineId;
            disciplinesRepo.DeleteHeat(heatId);
            var isLast = disciplinesRepo.SetCompetitionDisciplineHeatsGeneratedFalse(competitionDisciplineId);
            return Json(new { Success = true, isLast = isLast });
        }

        public ActionResult AddNewHeat(int competitionId, int competitionDisciplineId)
        {
            var currentNumberOfHeats = disciplinesRepo.GetNumberHeats(competitionDisciplineId);
            var heatName = $"{currentNumberOfHeats + 1}";
            disciplinesRepo.SaveNewHeat(competitionDisciplineId, heatName);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult RemoveSwimmer(int competitionResultId, int competitionDisciplineId, int seasonId)
        {
            var competitionResult = disciplinesRepo.GetCompetitionResult(competitionResultId);
            var heatId = competitionResult.Heat;
            var laneId = competitionResult.Lane;
            disciplinesRepo.DeleteCompetitionResult(competitionResultId);
            ViewBag.SeasonId = seasonId;
            return Json(new { competitionDisciplineId = competitionDisciplineId, seasonId = seasonId, laneId = laneId, heatId = heatId });
        }

        [HttpPost]
        public ActionResult GetHeatForCompetitionDiscipline(int competitionDisciplineId, int seasonId, int heatId)
        {
            List<HeatDto> heats = GetHeatDtos(competitionDisciplineId, seasonId);
            var heat = heats.FirstOrDefault(x => x.Id == heatId);
            ViewBag.SeasonId = seasonId;
            return PartialView("Disciplines/HeatLanesList", heat);
        }

        [HttpPost]
        public ActionResult AddSwimmer(int competitionDisciplineId, int userId, int heatId, int laneId, int seasonId)
        {
            disciplinesRepo.AddSwimmerToHeat(competitionDisciplineId, userId, heatId.ToString(), laneId);
            ViewBag.SeasonId = seasonId;
            return Json(new { competitionDisciplineId = competitionDisciplineId, seasonId = seasonId, laneId = laneId, heatId = heatId });
        }

        private void SaveGeneratedHeats(int competitionDisciplineId, List<HeatDto> heats, Dictionary<int, int> swimmerIdRegistrationIdDictionary)
        {
            foreach (var heat in heats)
            {
                var competitionDisciplineHeatStartTime = new CompetitionDisciplineHeatStartTime
                {
                    CompetitionDisciplineId = competitionDisciplineId,
                    HeatName = heat.Name,
                    StartTime = heat.StartTime,
                    IsFinal = heat.IsFinal
                };
                var saved = disciplinesRepo.SaveCompetitionDisciplineHeatStartTime(competitionDisciplineHeatStartTime);

                var numberOfLane = 1;
                foreach (var lane in heat.Lanes)
                {
                    if (lane.ClubId.HasValue)
                    {
                        var competitionResult = new CompetitionResult
                        {
                            Heat = saved.Id.ToString(),
                            Lane = numberOfLane,
                            Result = lane.Result,
                            CompetitionRegistrationId = swimmerIdRegistrationIdDictionary[lane.UserId]
                        };
                        db.CompetitionResults.Add(competitionResult);
                    }

                    numberOfLane++;
                }
            }

            disciplinesRepo.SetCompetitionDisciplineHeatsGeneratedTrue(competitionDisciplineId);
        }

        private List<HeatDto> GenerateHeatsFromSwimmers(int[] swimmersIds, int? disciplineId, int numberOfLanes)
        {
            var swimmers = usersRepo.GetByIds(swimmersIds).Select(x => new
            {
                EntryTime = x.PlayerDisciplines.FirstOrDefault(y => y.DisciplineId == disciplineId)?.EntryTime ?? "NT",
                UserId = x.UserId,
                User = x
            }).ToList();
            var numberOfHeats = (swimmers.Count / numberOfLanes) + 1;
            List<HeatDto> heats = new List<HeatDto>();
            swimmers = swimmers.OrderByDescending(x => x.EntryTime).ToList();
            for (int currentHeatNumber = 0; currentHeatNumber < numberOfHeats; currentHeatNumber++)
            {
                HeatDto heatDto = new HeatDto
                {
                    Name = $"{currentHeatNumber + 1}"
                };
                var swimmersForCurrentHeat = swimmers.Skip(currentHeatNumber * numberOfLanes).Take(numberOfLanes);
                var swimmersForCurrentHeatSorted = swimmersForCurrentHeat.OrderBy(x => x.EntryTime).ToList();
                bool addLast = true;
                var numberOfLane = 1;
                foreach (var heatSwimmer in swimmersForCurrentHeatSorted)
                {
                    var club = clubsRepo.GetByUserId(heatSwimmer.UserId);
                    if (addLast)
                    {
                        heatDto.Lanes.AddLast(new LaneDto
                        {
                            UserId = heatSwimmer.UserId,
                            SwimmerName = heatSwimmer.User.FullName,
                            BirthDate = heatSwimmer.User.BirthDay ?? new DateTime(),
                            ClubName = club?.Name,
                            ClubId = club?.ClubId,
                            EntryTime = heatSwimmer.EntryTime,
                        });
                    }
                    else
                    {
                        heatDto.Lanes.AddFirst(new LaneDto
                        {
                            UserId = heatSwimmer.UserId,
                            SwimmerName = heatSwimmer.User.FullName,
                            BirthDate = heatSwimmer.User.BirthDay ?? new DateTime(),
                            ClubName = club?.Name,
                            ClubId = club?.ClubId,
                            EntryTime = heatSwimmer.EntryTime
                        });
                    }

                    addLast = !addLast;
                }

                var currentNumberOfLanes = heatDto.Lanes.Count;

                if (currentNumberOfLanes < numberOfLanes)
                {
                    for (int i = 0; i < numberOfLanes - currentNumberOfLanes; i++)
                    {
                        if (addLast)
                        {
                            heatDto.Lanes.AddLast(new LaneDto
                            {
                                ClubId = null
                            });
                        }
                        else
                        {
                            heatDto.Lanes.AddFirst(new LaneDto
                            {
                                ClubId = null
                            });
                        }

                        addLast = !addLast;
                    }
                }
                heats.Add(heatDto);
            }
            return heats;
        }

        private List<HeatDto> GetHeatDtos(int competitionDisciplineId, int seasonId)
        {
            var heats = new List<HeatDto>();
            var heatsFromDb = db.CompetitionDisciplineHeatStartTimes
                .Where(x => x.CompetitionDisciplineId == competitionDisciplineId).ToList();
            var disciplineId = db.CompetitionDisciplines.FirstOrDefault(x => x.Id == competitionDisciplineId)?.DisciplineId;
            foreach (var heatFromDb in heatsFromDb)
            {
                Dictionary<int, LaneDto> laneDtosDictionary = new Dictionary<int, LaneDto>();
                var lanesFromDb = db.CompetitionDisciplineRegistrations
                    .Where(x => x.CompetitionDisciplineId == competitionDisciplineId && x.CompetitionResult.Any(y=>y.Heat == heatFromDb.Id.ToString())).ToList();
                var alreadyAssignedUserIdsOnHeat = db.CompetitionDisciplineRegistrations
                    .Where(x => x.CompetitionDisciplineId == competitionDisciplineId && x.CompetitionResult.Any(y => y.Heat.Length > 0 && y.Lane.HasValue)).Select(x => x.UserId).ToList();
                var players = disciplinesRepo.GetRegisteredSwimmersForCompetitionDiscipline(competitionDisciplineId, seasonId);
                players = players.Where(x => !alreadyAssignedUserIdsOnHeat.Contains(x.UserId)).OrderBy(r => r.UserName).ToList();
                foreach (var laneFromDb in lanesFromDb)
                {
                    var competitionResult = laneFromDb.CompetitionResult
                        .FirstOrDefault(x => x.CompetitionRegistrationId == laneFromDb.Id);
                    var id = competitionResult?.Lane ?? 0;
                    var entryTime = laneFromDb.User.PlayerDisciplines
                                        .FirstOrDefault(y => y.DisciplineId == disciplineId)
                                        ?.EntryTime ?? "NT";
                    entryTime = entryTime.Substring(entryTime.IndexOf(":", StringComparison.Ordinal) + 1);
                    laneDtosDictionary.Add(id, new LaneDto
                    {
                        Number = id,
                        Id = competitionResult.Id,
                        BirthDate = laneFromDb.User.BirthDay,
                        UserId = laneFromDb.UserId,
                        ClubId = laneFromDb.ClubId,
                        ClubName = laneFromDb.Club?.Name,
                        EntryTime = entryTime,
                        SwimmerName = laneFromDb.User.FullName,
                        Result = competitionResult?.Result
                    });
                }
                var competitionId = db.CompetitionDisciplines.FirstOrDefault(x => x.Id == competitionDisciplineId)?.CompetitionId;
                var numberOfLanes = leagueRepo.GetById(competitionId ?? 0)?.Auditorium?.LanesNumber;
                LinkedList<LaneDto> lanes = new LinkedList<LaneDto>();

                for (int i = 1; i < numberOfLanes + 1; i++)
                {
                    if (laneDtosDictionary.ContainsKey(i))
                    {
                        lanes.AddLast(laneDtosDictionary[i]);
                    }
                    else
                    {
                        lanes.AddLast(new LaneDto
                        {
                            Number = i,
                            PlayersList = players
                        });
                    }
                }
                heats.Add(new HeatDto
                {
                    Id = heatFromDb.Id,
                    Name = $"{Messages.Heat} {heatFromDb.HeatName}",
                    IsFinal = heatFromDb.IsFinal ?? false,
                    StartTime = heatFromDb.StartTime,
                    CompetitionDisciplineId = competitionDisciplineId,
                    Lanes = lanes
                });
            }
            ViewBag.SeasonId = seasonId;
            return heats;
        }

        public ActionResult UpdateSwimmingResult(int competitionResultId, string result1, string result2, string result3, string result4)
        {
            result1 = string.IsNullOrEmpty(result1) ? "00" : result1;
            result2 = string.IsNullOrEmpty(result2) ? "00" : result2;
            result3 = string.IsNullOrEmpty(result3) ? "00" : result3;
            result4 = string.IsNullOrEmpty(result4) ? "00" : result4;

            string resultTime = BuildResultString(result1, result2, result3, result4, 3);
            disciplinesRepo.UpdateCompetitionResultResultTime(competitionResultId, resultTime);
            return Json(new { Success = true });
        }

        public ActionResult GetChangeSwimmerModel(int competitionDisciplineId, int seasonId, int swimmerId)
        {
            ChangeSwimmerDto model = new ChangeSwimmerDto();
            var players = disciplinesRepo.GetRegisteredSwimmersForCompetitionDiscipline(competitionDisciplineId, seasonId);
            model.Swimmers = players.Where(x=>x.UserId != swimmerId).OrderBy(r => r.UserName).ToList();
            model.CompetitionDisciplineId = competitionDisciplineId;
            model.OriginalSwimmerId = swimmerId;
            ViewBag.SeasonId = seasonId;
            return PartialView("Disciplines/ChangeSwimmerModel", model);
        }

        public ActionResult ChangeSwimmers(int competitionDisciplineId, int originalSwimmerId, int changingSwimmerId, int seasonId)
        {
            var originalSwimmerResult = disciplinesRepo.GetSwimmerResult(competitionDisciplineId, originalSwimmerId);
            var changingSwimmerResult = disciplinesRepo.GetSwimmerResult(competitionDisciplineId, changingSwimmerId);
            var originalHeat = originalSwimmerResult.Heat;
            var originalLane = originalSwimmerResult.Lane;
            var changingSwimmerHeat = "0";
            if (changingSwimmerResult != null)
            {
                changingSwimmerHeat = changingSwimmerResult.Heat;
                disciplinesRepo.UpdateCompetitionResultHeatLane(originalSwimmerResult.Id, changingSwimmerResult.Heat,
                    changingSwimmerResult.Lane);
                disciplinesRepo.UpdateCompetitionResultHeatLane(changingSwimmerResult.Id, originalHeat, originalLane);
            }
            else
            {
                disciplinesRepo.DeleteSwimmerResult(competitionDisciplineId, originalSwimmerId);
                disciplinesRepo.AddSwimmerToHeat(competitionDisciplineId, changingSwimmerId, originalHeat, originalLane);
            }
            ViewBag.SeasonId = seasonId;
            return Json(new { competitionDisciplineId = competitionDisciplineId, seasonId = seasonId, originalHeat = originalHeat, changingHeat = changingSwimmerHeat });

        }

        public void CSVAutomation()
        {
            if (Request.Form["CSVType"] == "csv")
            {
                GenerateMultiStartListCSV();
            }
            else
            {
                ParsePhotoFinishData();
            }
        }

        public void ParsePhotoFinishData()
        {

            var ip = "192.117.158.164";
            var username = "ftp_user";
            var password = "123456";
            var port = "21";

            var leaguesDataForm = Request.Form["leagues"];
            var ids = leaguesDataForm.Split(new char[] { ',', '[', ']' }).Where(x => int.TryParse(x, out _)).Select(x => int.Parse(x)).ToArray();
            List<CompetitionDisciplineCSVDto> cdCSV = new List<CompetitionDisciplineCSVDto>();
            int? seasonid = null;
            Dictionary<string, List<CompetitionDisciplineRegistration>> dict = new Dictionary<string, List<CompetitionDisciplineRegistration>>();
            List<PhotoFinishReportItem> errors = new List<PhotoFinishReportItem>();
            foreach (var competitionId in ids)
            {
                var competitionDisiplines = disciplinesRepo.GetCompetitionDisciplinesTrack(competitionId).OrderBy(d => d.StartTime ?? DateTime.MaxValue).ToList();
                seasonid = competitionDisiplines.FirstOrDefault()?.League?.SeasonId;
                foreach (var disciplineCompetition in competitionDisiplines)
                {
                    var discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);
                    var registrationsWithHeat = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r => r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Heat));
                    var groupedByHeat = registrationsWithHeat.OrderBy(r => r.CompetitionResult.FirstOrDefault().Lane ?? int.MaxValue).GroupBy(r => r.CompetitionResult.FirstOrDefault().Heat).OrderBy(group => group.Key).ToList();
                    if (discipline.Format == 1 || discipline.Format == 2 || discipline.Format == 3)
                    {
                        foreach (var heatGroup in groupedByHeat)
                        {
                            var fileName = $"{disciplineCompetition.Id.ToString()}{heatGroup.Key}.CL";
                            var regs = heatGroup.Select(t => t).ToList();
                            foreach (var reg in regs)
                            {
                                reg.Format = discipline.Format;
                            }
                            dict[fileName] = regs;
                        }
                    }
                }
            }
            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(username, password);
                client.BaseAddress = $"ftp://{ip}:{port}/";
                foreach (var key in dict.Keys)
                {
                    var regs = dict[key];                  
                    try
                    {
                        
                        var response = client.DownloadData($"/{key}");
                        errors.Add(new PhotoFinishReportItem { Message = $"Discipline file: {key} parsing Started.", Color = "green" });
                        string result = System.Text.Encoding.GetEncoding(1255).GetString(response);
                        var windIndex = result.IndexOf("Wind speed : ");
                        
                        var resFromWind = result.Substring(windIndex + 13);
                        var windStr = resFromWind.Substring(0, resFromWind.IndexOf("\r\n"));
                        double? wind = null;
                        if(windStr != "No measurement")
                        {
                            var windVal = windStr.Substring(0, resFromWind.IndexOf(" "));
                            double windDVal;
                            var isNum = double.TryParse(windVal, out windDVal);
                            if (isNum)
                            {
                                wind = windDVal;
                            }
                        }

                        var resultsIndex = resFromWind.IndexOf("------------------------------------------------------------------------------------------\r\n");
                        var resultsStr = resFromWind.Substring(resultsIndex+93);

                        var lineSplited = resultsStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        foreach (var line in lineSplited.Where(l => l.Length > 12))
                        {
                            var extraRank = 0;
                            
                            //0. index
                            //1. lane
                            //2. athlete number
                            var data = line.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                            if(data[0].Last() == '.')
                            {
                                extraRank = 1;
                            }

                            var athleteNum = data[1+ extraRank];
                            var lane = data[0 + extraRank];

                            int? regLane = null;

                            int laneDVal;
                            var isNum = int.TryParse(lane, out laneDVal);
                            if (isNum)
                            {
                                regLane = laneDVal;
                            }

                            int? regAthleteNum = null;

                            int athgleteNumDVal;
                            isNum = int.TryParse(athleteNum, out athgleteNumDVal);
                            if (isNum)
                            {
                                regAthleteNum = athgleteNumDVal;
                            }

                            var regResultStr1 = string.Empty;
                            var maxIndex = Math.Min(data.Count() - 1, 8);

                            var alternative = 0;


                            for (int i = 3; i <= maxIndex; i++)
                            {
                                alternative = UIHelpers.GetAlternativeResultIntByString(data[i]);
                                if (alternative == 0)
                                {
                                    var firstChar = data[i].First();
                                    var isDigit = char.IsDigit(firstChar);
                                    if (isDigit)
                                    {
                                        regResultStr1 = data[i];
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }



                            // by chars distance from start line is not viable it changes in different files.
                            /*
                            var restStartStr = line.Substring(54);
                            var regResultStr2 = restStartStr.Substring(0, restStartStr.IndexOf(" ")); // seems different in different files
                            */


                            var currentReg = regs.FirstOrDefault(r => r.User.AthleteNumbers.FirstOrDefault(at => at.SeasonId == r.CompetitionDiscipline.League.SeasonId) != null && r.User.AthleteNumbers.FirstOrDefault(at => at.SeasonId == r.CompetitionDiscipline.League.SeasonId).AthleteNumber1 == regAthleteNum);
                            if(currentReg == null)
                            {
                                errors.Add(new PhotoFinishReportItem {Message =  $"Discipline file: \"{key}\", athlete with athletenumber : \"{ athleteNum }\" has result but is not registered to the discipline.", Color = "red"});
                                continue;
                            }


                            string properResult = null;
                            double? sortedValue;
                            if(!UIHelpers.isResultFormatCorrectForPhotofinish(regResultStr1, currentReg.Format ?? 0, out sortedValue, out properResult))
                            {
                                var wrongResultFormat = true;
                            }
                            if(sortedValue == -1 && alternative == 0)
                            {
                                errors.Add(new PhotoFinishReportItem { Message = $"Discipline file: {key}, registration athlete: \"{ athleteNum }\" failed to parse result, reading result is: \"{regResultStr1}\"", Color = "red" });
                            }

                            long? sort = 0;
                            if(sortedValue != -1)
                            {
                                sort = Convert.ToInt64(sortedValue);
                            }
                            if (currentReg.CompetitionResult.Count() == 0)
                            {
                                
                                var newResult = new CompetitionResult
                                {
                                    Lane = regLane,
                                    Wind = wind,
                                    Result = sortedValue != -1 ? properResult : string.Empty,
                                    SortValue = sort,
                                    AlternativeResult = alternative

                                };
                                currentReg.CompetitionResult.Add(newResult);                              
                            }
                            else
                            {
                                var oldResult = currentReg.CompetitionResult.First();
                                oldResult.Wind = wind;
                                oldResult.Lane = regLane;
                                oldResult.Result = sortedValue != -1 || alternative != 0 ? properResult : string.Empty;
                                oldResult.SortValue = sort;
                                oldResult.AlternativeResult = alternative;
                            }
                        }

                        errors.Add(new PhotoFinishReportItem { Message = $"Discipline file: \"{key}\" parsing ended.", Color = "green" });
                    }
                    catch (System.Net.WebException ex)
                    {
                        errors.Add(new PhotoFinishReportItem { Message = $"Discipline file: \"{key}\" missing from ftp.", Color = "black" });
                    }
                }


            }
            disciplinesRepo.Save();

            PhotofinishErrorPdf photofinishErrorPdf = new PhotofinishErrorPdf(errors, IsHebrew);
            var exportResult = photofinishErrorPdf.GetDocumentStream();

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename=photofinishErrorPdf.pdf");

            Response.OutputStream.Write(exportResult.GetBuffer(), 0, exportResult.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }


        public void GenerateMultiStartListCSV()
        {
            var leaguesDataForm = Request.Form["leagues"];
            var ids = leaguesDataForm.Split(new char[] { ',', '[', ']' }).Where(x => int.TryParse(x, out _)).Select(x => int.Parse(x)).ToArray();
            List<CompetitionDisciplineCSVDto> cdCSV = new List<CompetitionDisciplineCSVDto>();
            int? seasonid = null;
            foreach (var competitionId in ids)
            {
                var competitionDisiplines = disciplinesRepo.GetCompetitionDisciplines(competitionId).OrderBy(d => d.StartTime ?? DateTime.MaxValue).ToList();
                seasonid = competitionDisiplines.FirstOrDefault()?.League?.SeasonId;
                foreach (var disciplineCompetition in competitionDisiplines)
                {
                    var discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);
                    var registrationsWithHeat = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r => r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Heat));
                    var groupedByHeat = registrationsWithHeat.OrderBy(r => r.CompetitionResult.FirstOrDefault().Lane ?? int.MaxValue).GroupBy(r => r.CompetitionResult.FirstOrDefault().Heat).OrderBy(group => group.Key).ToList();

                    var noCommaName = discipline.Name.Replace(",", string.Empty);
                    var length = Regex.Match(noCommaName, @"\d+").Value; // getting the number from discipline name by parsing it(maybe should improve on it to add value attribute to disciplines just like type.).
                    if (discipline.Name.IndexOf("400x4", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        length = "1600";
                    }
                    if (discipline.Name.IndexOf("4x400", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        length = "1600";
                    }
                    if (discipline.Name.IndexOf("100x4", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        length = "400";
                    }
                    if (discipline.Name.IndexOf("4x100", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        length = "400";
                    }
                    var heatStartTimes = disciplineCompetition.CompetitionDisciplineHeatStartTimes.ToList();
                    if (discipline.Format == 1 || discipline.Format == 2 || discipline.Format == 3)
                    {
                        foreach (var heatGroup in groupedByHeat)
                        {
                            CompetitionDisciplineCSVDto item = new CompetitionDisciplineCSVDto
                            {
                                CompetitionDesciplineId = disciplineCompetition.Id.ToString(),
                                DisciplineName = discipline.Name,
                                CategoryName = disciplineCompetition.CompetitionAge.age_name,
                                DisciplineDate = heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key) != null && heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key).StartTime.HasValue ? heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key).StartTime.Value.ToShortDateString() : (disciplineCompetition.StartTime.HasValue ? disciplineCompetition.StartTime.Value.ToShortDateString() : ""),
                                DisciplineTime = heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key) != null && heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key).StartTime.HasValue ? heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key).StartTime.Value.ToShortTimeString() : (disciplineCompetition.StartTime.HasValue ? disciplineCompetition.StartTime.Value.ToShortTimeString() : ""),
                                StartDateTicks = heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key) != null && heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key).StartTime.HasValue ? heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key).StartTime.Value.Ticks : (disciplineCompetition.StartTime.HasValue ? disciplineCompetition.StartTime.Value.Ticks : DateTime.MaxValue.Ticks),
                                DisciplineLength = length,
                                HeatName = heatGroup.Key,
                                Registrations = heatGroup.Select(t => t).ToList()
                            };
                            cdCSV.Add(item);
                        }
                    }
                }
            }

            var numericValues = cdCSV.Where(r => int.TryParse(r.HeatName, out _)).ToList();
            numericValues = numericValues.OrderBy(group => int.Parse(group.HeatName)).ToList();
            var nonNumericValues = cdCSV.Where(r => !int.TryParse(r.HeatName, out _)).ToList();
            cdCSV = numericValues.Concat(nonNumericValues).ToList();

            cdCSV = cdCSV.OrderBy(h => h.StartDateTicks).ToList();
            var contentPath = Server.MapPath(GlobVars.ContentPath);

            StartListCSVHelper startListPdfHelper = new StartListCSVHelper(cdCSV, IsHebrew, seasonid);
            var exportResult = startListPdfHelper.GetDocumentBytes();
            var encod = new UnicodeEncoding(false, false);


            var ip = "192.117.158.164";
            var username = "ftp_user";
            var password = "123456";
            var port = "21";
            var fileAdressRemotely = $"ftp://{ip}/startlist_test.csv";
            
            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(username, password);
                client.BaseAddress = $"ftp://{ip}:{port}/";
                try
                {
                    var response = client.UploadData("startlist.csv", WebRequestMethods.Ftp.UploadFile, exportResult);
                }
                catch (System.Net.WebException ex)
                {
                    //Response.StatusCode = 503;
                }
            }

            Response.Clear();
            Response.ContentType = "application/csv";
            Response.AddHeader("Content-Type", "text/csv");
            Response.AddHeader("content-disposition", $"attachment;filename={Messages.StartList}_{DateTime.Now.ToShortDateString()}.csv");
            Response.BinaryWrite(exportResult);
            Response.Charset = "Windows-1255";
            Response.ContentEncoding = encod; // Encoding.GetEncoding(1255);
            Response.Flush();
            Response.End();

        }

        public ActionResult UpdateCompetitionResultsFromExternalFtp(int competitionId) {


            return Json(new { });
        }



        public void GenerateStartListCSV(int competitionId)
        {
            var competitionDisiplines = disciplinesRepo.GetCompetitionDisciplines(competitionId).OrderBy(d => d.StartTime ?? DateTime.MaxValue).ToList();
            List<CompetitionDisciplineCSVDto> cdCSV = new List<CompetitionDisciplineCSVDto>();
            int? seasonid = null;
            foreach (var disciplineCompetition in competitionDisiplines)
            {
                var discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);
                var registrationsWithHeat = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r => r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Heat));
                var groupedByHeat = registrationsWithHeat.OrderBy(r => r.CompetitionResult.FirstOrDefault().Lane ?? int.MaxValue).GroupBy(r => r.CompetitionResult.FirstOrDefault().Heat).OrderBy(group => group.Key).ToList();
                var length = Regex.Match(discipline.Name, @"\d+").Value; // getting the number from discipline name by parsing it(maybe should improve on it to add value attribute to disciplines just like type.).
                var heatStartTimes = disciplineCompetition.CompetitionDisciplineHeatStartTimes.ToList();
                seasonid = disciplineCompetition?.League?.SeasonId;
                if (discipline.Format == 1 || discipline.Format == 2 || discipline.Format == 3)
                {
                    foreach (var heatGroup in groupedByHeat)
                    {
                        CompetitionDisciplineCSVDto item = new CompetitionDisciplineCSVDto
                        {
                            CompetitionDesciplineId = disciplineCompetition.Id.ToString(),
                            DisciplineName = discipline.Name,
                            CategoryName = disciplineCompetition.CompetitionAge.age_name,
                            DisciplineDate = heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key) != null ? heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key).StartTime.Value.ToShortDateString() : (disciplineCompetition.StartTime.HasValue ? disciplineCompetition.StartTime.Value.ToShortDateString() : ""),
                            DisciplineTime = heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key) != null ? heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key).StartTime.Value.ToShortTimeString() : (disciplineCompetition.StartTime.HasValue ? disciplineCompetition.StartTime.Value.ToShortTimeString() : ""),
                            StartDateTicks = heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key) != null ? heatStartTimes.FirstOrDefault(h => h.HeatName == heatGroup.Key).StartTime.Value.Ticks : (disciplineCompetition.StartTime.HasValue ? disciplineCompetition.StartTime.Value.Ticks : DateTime.MaxValue.Ticks),
                            DisciplineLength = length,
                            HeatName = heatGroup.Key,
                            Registrations = heatGroup.Select(t => t).ToList()
                        };
                        cdCSV.Add(item);
                    }
                }
            }
            cdCSV = cdCSV.OrderBy(h => h.StartDateTicks).ToList();
            var contentPath = Server.MapPath(GlobVars.ContentPath);

            StartListCSVHelper startListPdfHelper = new StartListCSVHelper(cdCSV, IsHebrew, seasonid);
            var exportResult = startListPdfHelper.GetDocumentBytes();
            var encod = new UnicodeEncoding(false, false);

            Response.Clear();
            Response.ContentType = "application/csv";
            Response.AddHeader("Content-Type", "text/csv");
            Response.AddHeader("content-disposition", $"attachment;filename={Messages.StartList}_{DateTime.Now.ToShortDateString()}.csv");
            Response.BinaryWrite(exportResult);
            Response.Charset = "Windows-1255";
            Response.ContentEncoding = encod; // Encoding.GetEncoding(1255);
            Response.Flush();
            Response.End();
        }


        public void GenerateAllRefereeForms(int competitionId, bool onlyPresent = false)
        {
            var league = leagueRepo.GetById(competitionId);
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 30, 30, 90.0f + 45, 25);
            MemoryStream stream = new MemoryStream();
            PdfCopy pdf = new PdfCopy(pdfDoc, stream);
            pdfDoc.Open();
            pdf.Open();
            foreach (var disciplineCompetition in league.CompetitionDisciplines)
            {
                var discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

                if (discipline.Format.HasValue && !(discipline.Format.Value > 7 && discipline.Format.Value != 10 && discipline.Format.Value != 11))
                {
                    var disciplineName = discipline.Name;
                    var registrationsWithHeat = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r => !onlyPresent || r.Presence.HasValue && r.Presence.Value);

                    var grouped = registrationsWithHeat.OrderBy(r => r.CompetitionResult.FirstOrDefault()?.Lane ?? int.MaxValue).GroupBy(r => string.IsNullOrEmpty(r.CompetitionResult.FirstOrDefault()?.Heat) == false ? r.CompetitionResult.FirstOrDefault()?.Heat : Messages.None).OrderBy(group => group.Key).ToList();
                    var groupedByHeat = grouped.Where(g => g.Key != Messages.None).ToList();
                    var nonGroup = grouped.FirstOrDefault(g => g.Key == Messages.None);
                    if (nonGroup != null)
                    {
                        groupedByHeat.Add(nonGroup);
                    }
                    var numericValues = groupedByHeat.Where(r => int.TryParse(r.Key, out _)).ToList();
                    numericValues = numericValues.OrderBy(group => int.Parse(group.Key)).ToList();
                    var nonNumericValues = groupedByHeat.Where(r => !int.TryParse(r.Key, out _)).ToList();
                    groupedByHeat = numericValues.Concat(nonNumericValues).ToList();
                    if (groupedByHeat.Count > 0)
                    {
                        RefereeFormPdfHelper refereeFormPdfHelper = new RefereeFormPdfHelper(groupedByHeat, league, disciplineName, discipline.Format.Value, LangHelper.GetGenderCharById(disciplineCompetition.CompetitionAge.gender.Value), $"{disciplineCompetition.CompetitionAge.age_name}", IsHebrew, contentPath);
                        var exportResult = refereeFormPdfHelper.GetDocumentStream();
                        pdf.AddDocument(new PdfReader(exportResult.GetBuffer()));
                    }
                }
            }
            pdfDoc.Close();
            pdf.Close();
            var leagueName = league?.Name.Replace(".", "").Replace(",", "");
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={leagueName}_{Messages.RefereeForm}_{DateTime.Now.ToShortDateString()}.pdf");
            Response.OutputStream.Write(stream.GetBuffer(), 0, stream.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }


        public void GenerateDisciplineRefereeForm(int id, bool onlyPresent = false)
        {
            var disciplineCompetition = disciplinesRepo.GetCompetitionDisciplineById(id);
            var discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

            var disciplineName = discipline.Name;
            var registrationsWithHeat = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r => !onlyPresent || r.Presence.HasValue && r.Presence.Value );

            var grouped = registrationsWithHeat.OrderBy(r => r.CompetitionResult.FirstOrDefault()?.Lane ?? int.MaxValue).GroupBy(r => string.IsNullOrEmpty(r.CompetitionResult.FirstOrDefault()?.Heat) == false ? r.CompetitionResult.FirstOrDefault()?.Heat : Messages.None).OrderBy(group => group.Key).ToList();
            var groupedByHeat = grouped.Where(g => g.Key != Messages.None).ToList();
            var nonGroup = grouped.FirstOrDefault(g => g.Key == Messages.None);
            if (nonGroup != null)
            {
                groupedByHeat.Add(nonGroup);
            }
            var numericValues = groupedByHeat.Where(r => int.TryParse(r.Key, out _)).ToList();
            numericValues = numericValues.OrderBy(group => int.Parse(group.Key)).ToList();
            var nonNumericValues = groupedByHeat.Where(r => !int.TryParse(r.Key, out _)).ToList();
            groupedByHeat = numericValues.Concat(nonNumericValues).ToList();
            var league = disciplineCompetition.League;
            var leagueName = league?.Name;
            var firstFilenamePart = disciplineName.Replace(".","").Replace(",", "");
            var contentPath = Server.MapPath(GlobVars.ContentPath);

            RefereeFormPdfHelper refereeFormPdfHelper = new RefereeFormPdfHelper(groupedByHeat, league, disciplineName, discipline.Format.Value, LangHelper.GetGenderCharById(disciplineCompetition.CompetitionAge.gender.Value),$"{disciplineCompetition.CompetitionAge.age_name}", IsHebrew, contentPath);
            var exportResult = refereeFormPdfHelper.GetDocumentStream();

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={firstFilenamePart}_{Messages.RefereeForm}_{DateTime.Now.ToShortDateString()}.pdf");

            Response.OutputStream.Write(exportResult.GetBuffer(), 0, exportResult.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }


        public void GenerateAllStartList(int competitionId, bool onlyPresent = false)
        {
            var league = leagueRepo.GetById(competitionId);
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 30, 30, 90.0f + 45, 25);
            MemoryStream stream = new MemoryStream();
            PdfCopy pdf = new PdfCopy(pdfDoc, stream);
            pdfDoc.Open();
            pdf.Open();
            foreach (var disciplineCompetition in league.CompetitionDisciplines)
            {
                var discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId ?? 0);
                var disciplineName = discipline?.Name;
                var registrationsWithHeat = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r => r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Heat));
                var groupedByHeat = registrationsWithHeat.OrderBy(r => r.CompetitionResult.FirstOrDefault().Lane ?? int.MaxValue).GroupBy(r => r.CompetitionResult.FirstOrDefault().Heat).OrderBy(group => group.Key).ToList();
                var registrationsWithoutHeat = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r => r.CompetitionResult.FirstOrDefault() == null || string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Heat)).GroupBy(r => Messages.None);
                foreach (var item in registrationsWithoutHeat)
                {
                    groupedByHeat.Add(item);
                    break;
                }
                var numericValues = groupedByHeat.Where(r => int.TryParse(r.Key, out _)).ToList();
                numericValues = numericValues.OrderBy(group => int.Parse(group.Key)).ToList();
                var nonNumericValues = groupedByHeat.Where(r => !int.TryParse(r.Key, out _)).ToList();
                groupedByHeat = numericValues.Concat(nonNumericValues).ToList();

                var userIds = disciplineCompetition.CompetitionDisciplineRegistrations.Select(r => r.UserId).ToList();

                var usersRegsGroupedByUserId = gamesRepo.GetBulkAthleteCompetitionsAchievements(userIds, disciplineCompetition.League.SeasonId.Value);
                var isAsc = Utils.IsOrderByFormatAsc(discipline?.Format);

                var disciplineRecordId = disciplinesRepo.GetCompetitonDisciplineRecord(disciplineCompetition.DisciplineId ?? 0, disciplineCompetition.CategoryId);


                foreach (var heatGroup in groupedByHeat)
                {
                    foreach (var reg in heatGroup)
                    {
                        var userId = reg.UserId;
                        if (disciplineRecordId.HasValue)
                        {
                            var userRegs = usersRegsGroupedByUserId.FirstOrDefault(g => g.Key == userId);
                            var userTopResultByRecordIds = string.Empty;
                            if (isAsc)
                            {
                                userTopResultByRecordIds = userRegs?.Where(r => disciplineRecordId.HasValue && r.RecordId.Contains(disciplineRecordId.Value) && r.AlternativeResult == 0).OrderBy(r => r.SortValue).FirstOrDefault()?.Result ?? string.Empty;
                            }
                            else
                            {
                                userTopResultByRecordIds = userRegs?.Where(r => disciplineRecordId.HasValue && r.RecordId.Contains(disciplineRecordId.Value) && r.AlternativeResult == 0).OrderByDescending(r => r.SortValue).FirstOrDefault()?.Result ?? string.Empty;
                            }
                            reg.SeasonalBest = userTopResultByRecordIds;
                        }
                        else
                        {
                            reg.SeasonalBest = string.Empty;
                        }
                    }
                }

                var sectionAlias = secRepo.GetByLeagueId(competitionId)?.Alias;
                var categoryName = disciplineCompetition?.CompetitionAge?.age_name;
                //var firstFilenamePart = disciplineName.Replace(".", "").Replace(",", "");
                var discCatName = sectionAlias == SectionAliases.Climbing ? categoryName : $"{disciplineName} - {categoryName}";
                StartListPdfHelper startListPdfHelper;
                if (disciplineCompetition.IncludeRecordInStartList)
                {
                    DisciplineRecord disciplineRecord;
                    if (!disciplineRecordId.HasValue)
                    {
                        disciplineRecord = new DisciplineRecord { CompetitionRecord = disciplineCompetition.CompetitionRecord };
                    }
                    else
                    {
                        var disciplineRecordR = disciplinesRepo.GetDisciplineRecordById(disciplineRecordId.Value);
                        disciplineRecordR.CompetitionRecord = disciplineCompetition.CompetitionRecord;
                        disciplineRecord = disciplineRecordR;
                    }
                    startListPdfHelper = new StartListPdfHelper(groupedByHeat, league, discCatName, IsHebrew, contentPath, disciplineCompetition.StartTime, disciplineRecord, sectionAlias);
                }
                else
                {
                    startListPdfHelper = new StartListPdfHelper(groupedByHeat, league, discCatName, IsHebrew, contentPath, disciplineCompetition.StartTime, null, sectionAlias);
                }
                if (groupedByHeat.Count > 0)
                {
                        var exportResult = startListPdfHelper.GetDocumentStream();
                        pdf.AddDocument(new PdfReader(exportResult.GetBuffer()));
                }
                
            }
            pdfDoc.Close();
            pdf.Close();
            var leagueName = league?.Name.Replace(".", "").Replace(",", "");
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={leagueName}_{Messages.StartList}_{DateTime.Now.ToShortDateString()}.pdf");
            Response.OutputStream.Write(stream.GetBuffer(), 0, stream.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }

        public void GenerateStartList(int id)
        {
            var disciplineCompetition = disciplinesRepo.GetCompetitionDisciplineById(id);
            var discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId ?? 0);
            var disciplineName = discipline?.Name;
            var registrationsWithHeat = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r => r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Heat));
            var groupedByHeat = registrationsWithHeat.OrderBy(r => r.CompetitionResult.FirstOrDefault().Lane ?? int.MaxValue).GroupBy(r => r.CompetitionResult.FirstOrDefault().Heat).OrderBy(group => group.Key).ToList();
            var registrationsWithoutHeat = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r => r.CompetitionResult.FirstOrDefault() == null || string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Heat)).GroupBy(r => Messages.None);
            foreach (var item in registrationsWithoutHeat)
            {
                groupedByHeat.Add(item);
                break;
            }
            var numericValues = groupedByHeat.Where(r => int.TryParse(r.Key, out _)).ToList();
            numericValues = numericValues.OrderBy(group => int.Parse(group.Key)).ToList();
            var nonNumericValues = groupedByHeat.Where(r => !int.TryParse(r.Key, out _)).ToList();
            groupedByHeat = numericValues.Concat(nonNumericValues).ToList();

            var userIds = disciplineCompetition.CompetitionDisciplineRegistrations.Select(r => r.UserId).ToList();

            var usersRegsGroupedByUserId = gamesRepo.GetBulkAthleteCompetitionsAchievements(userIds, disciplineCompetition.League.SeasonId.Value);
            var isAsc = Utils.IsOrderByFormatAsc(discipline?.Format);

            var disciplineRecordId = disciplinesRepo.GetCompetitonDisciplineRecord(disciplineCompetition.DisciplineId ?? 0, disciplineCompetition.CategoryId);


            foreach (var heatGroup in groupedByHeat)
            {
                foreach (var reg in heatGroup)
                {
                    var userId = reg.UserId;
                    if (disciplineRecordId.HasValue)
                    {
                        var userRegs = usersRegsGroupedByUserId.FirstOrDefault(g => g.Key == userId);
                        var userTopResultByRecordIds = string.Empty;
                        if (isAsc)
                        {
                            userTopResultByRecordIds = userRegs?.Where(r => disciplineRecordId.HasValue && r.RecordId.Contains(disciplineRecordId.Value) && r.AlternativeResult == 0).OrderBy(r => r.SortValue).FirstOrDefault()?.Result ?? string.Empty;
                        }
                        else
                        {
                            userTopResultByRecordIds = userRegs?.Where(r => disciplineRecordId.HasValue && r.RecordId.Contains(disciplineRecordId.Value) && r.AlternativeResult == 0).OrderByDescending(r => r.SortValue).FirstOrDefault()?.Result ?? string.Empty;
                        }
                        reg.SeasonalBest = userTopResultByRecordIds;
                    }
                    else
                    {
                        reg.SeasonalBest = string.Empty;
                    }
                }
            }

            var sectionName = secRepo.GetByLeagueId(disciplineCompetition.League.LeagueId)?.Alias;
            var league = disciplineCompetition.League;
            var leagueName = league?.Name;
            var categoryName = disciplineCompetition?.CompetitionAge?.age_name;
            var firstFilenamePart = disciplineName?.Replace(".", "").Replace(",", "");
            if (sectionName == SectionAliases.Climbing) firstFilenamePart = categoryName;
            var contentPath = Server.MapPath(GlobVars.ContentPath);

            StartListPdfHelper startListPdfHelper;
            var discCatName = sectionName == SectionAliases.Climbing ? categoryName : $"{disciplineName} - {categoryName}";
            if (disciplineCompetition.IncludeRecordInStartList)
            {
                DisciplineRecord disciplineRecord;
                if (!disciplineRecordId.HasValue)
                {
                    disciplineRecord = new DisciplineRecord { CompetitionRecord = disciplineCompetition.CompetitionRecord };
                }
                else
                {
                    var disciplineRecordR = disciplinesRepo.GetDisciplineRecordById(disciplineRecordId.Value);
                    disciplineRecordR.CompetitionRecord = disciplineCompetition.CompetitionRecord;
                    disciplineRecord = disciplineRecordR;
                }
                startListPdfHelper = new StartListPdfHelper(groupedByHeat, league, discCatName, IsHebrew, contentPath, disciplineCompetition.StartTime, disciplineRecord, sectionName);
            }
            else
            {
                startListPdfHelper = new StartListPdfHelper(groupedByHeat, league, discCatName, IsHebrew, contentPath, disciplineCompetition.StartTime, null, sectionName);
            }

            var exportResult = startListPdfHelper.GetDocumentStream();

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={firstFilenamePart}_{Messages.StartList}_{DateTime.Now.ToShortDateString()}.pdf");

            Response.OutputStream.Write(exportResult.GetBuffer(), 0, exportResult.GetBuffer().Length);
            Response.Flush();
            Response.End();

        }
        


        public ActionResult UpdateWeightInWeightliftRegistrationResult(int RegistrationId, int CompetitionDisciplineId, int LeagueId, WeightLiftingResultsForm form) {
            if (!ModelState.IsValid) {
                ViewBag.ErrorMessage = Messages.FieldMustBeNumeric;
                return RedirectToAction("WeighLiftingResults", new { id = CompetitionDisciplineId, compIdLast = RegistrationId });
            }

            if (form.WeightDeclaration.HasValue && ((form.Lifting1.HasValue && form.Push1.HasValue && form.Lifting1.Value + form.Push1.Value < form.WeightDeclaration.Value - 20)))
            {
                ViewBag.ErrorMessage = "One of the Lifts+Pushes is less than 20 from Declaration.";
                return RedirectToAction("WeighLiftingResults", new { id = CompetitionDisciplineId, compIdLast = RegistrationId });
            }

            int? bestLift = null;
            if (form.Lifting1.HasValue && form.Lift1Success.HasValue && form.Lift1Success.Value == 1 && (!bestLift.HasValue || form.Lifting1.Value > bestLift.Value))
            {
                bestLift = form.Lifting1;
            }
            if (form.Lifting2.HasValue && form.Lift2Success.HasValue && form.Lift2Success.Value == 1 && (!bestLift.HasValue || form.Lifting2.Value > bestLift.Value))
            {
                bestLift = form.Lifting2;
            }
            if (form.Lifting3.HasValue && form.Lift3Success.HasValue && form.Lift3Success.Value == 1 && (!bestLift.HasValue || form.Lifting3.Value > bestLift.Value))
            {
                bestLift = form.Lifting3;
            }
            int? bestPush = null;
            if (form.Push1.HasValue && form.Push1Success.HasValue && form.Push1Success.Value == 1 && (!bestPush.HasValue || form.Lifting1.Value > bestPush.Value))
            {
                bestPush = form.Push1;
            }
            if (form.Push2.HasValue && form.Push2Success.HasValue && form.Push2Success.Value == 1 && (!bestPush.HasValue || form.Push2.Value > bestPush.Value))
            {
                bestPush = form.Push2;
            }
            if (form.Push3.HasValue && form.Push3Success.HasValue && form.Push3Success.Value == 1 && (!bestPush.HasValue || form.Push3.Value > bestPush.Value))
            {
                bestPush = form.Push3;
            }

            int? finalResult = null;
            if (bestPush.HasValue && bestLift.HasValue)
            {
                finalResult = bestPush + bestLift;
            }

            var foundRegistration = disciplinesRepo.getCompetitionDisciplineRegistrationById(RegistrationId);
            if (foundRegistration != null)
            {
                var result = foundRegistration.CompetitionResult.FirstOrDefault();
                if (result == null)
                {
                    var compRes = new CompetitionResult
                    {
                        CompetitionRegistrationId = foundRegistration.Id,
                        Lifting1 = form.Lifting1,
                        Lifting2 = form.Lifting2,
                        Lifting3 = form.Lifting3,
                        Push1 = form.Push1,
                        Push2 = form.Push2,
                        Push3 = form.Push3,
                        Lift1Success = form.Lift1Success,
                        Lift2Success = form.Lift2Success,
                        Lift3Success = form.Lift3Success,
                        Push1Success = form.Push1Success,
                        Push2Success = form.Push2Success,
                        Push3Success = form.Push3Success,
                        LiftingResult = bestLift,
                        PushResult = bestPush,
                        FinalResult = finalResult

                    };
                    foundRegistration.CompetitionResult.Add(compRes);
                }
                else
                {
                    result.Lifting1 = form.Lifting1;
                    result.Lifting2 = form.Lifting2;
                    result.Lifting3 = form.Lifting3;
                    result.Push1 = form.Push1;
                    result.Push2 = form.Push2;
                    result.Push3 = form.Push3;
                    result.Lift1Success = form.Lift1Success;
                    result.Lift2Success = form.Lift2Success;
                    result.Lift3Success = form.Lift3Success;
                    result.Push1Success = form.Push1Success;
                    result.Push2Success = form.Push2Success;
                    result.Push3Success = form.Push3Success;
                    result.LiftingResult = bestLift;
                    result.PushResult = bestPush;
                    result.FinalResult = finalResult;
                }
            }
            else
            {
                return Json(new { Success = false });
            }
            disciplinesRepo.Save();
            return RedirectToAction("WeighLiftingResults", new { id = CompetitionDisciplineId, compIdLast = RegistrationId });
        }

        public ActionResult WeighLiftingResults(int id, int compIdLast = 0)
        {
            var disciplineCompetition = disciplinesRepo.GetCompetitionDisciplineById(id);
            ViewBag.CompetitionId = disciplineCompetition.CompetitionId;
            int? unionId = disciplineCompetition.League.UnionId;
            int? sectionId = disciplineCompetition.League.SeasonId;
            ViewBag.CategoryName = disciplineCompetition.CompetitionAge.age_name;
            ViewBag.CompetitionName = disciplineCompetition.League.Name;
            ViewBag.CompetitionDisciplineId = id;
            disciplineCompetition.CompetitionDisciplineRegistrations = disciplineCompetition.CompetitionDisciplineRegistrations.OrderBy(r => r.User.FullName).ToList();
            ViewBag.LastModifiedReg = compIdLast;
            return PartialView("Disciplines/_WeighLiftingResults", disciplineCompetition);
        }

        public ActionResult UpdateWeightInWeightliftRegistrationResultBySession(int RegistrationId, int CompetitionDisciplineId, int LeagueId, int SessionId, int ClubId, WeightLiftingResultsForm form)
        {
            int? bestLift = null;

            bool isLiftingChanged = false;
            bool isPushingChanged = false;
            int liftTest = 0;
            int pushTest = 0;

            if (form.Lifting1.HasValue && form.Lift1Success.HasValue)
            {
                if (form.Lift1Success.Value == 1 && (!bestLift.HasValue || form.Lifting1.Value > bestLift.Value))
                {
                    bestLift = form.Lifting1;
                }
                liftTest = form.Lifting1.Value;
            }
            if (form.Lifting2.HasValue && form.Lift2Success.HasValue)
            {
                if (form.Lift2Success.Value == 1 && (!bestLift.HasValue || form.Lifting2.Value > bestLift.Value))
                {
                    bestLift = form.Lifting2;
                }
                liftTest = form.Lifting2.Value;
            }
            if (form.Lifting3.HasValue && form.Lift3Success.HasValue)
            {
                if (form.Lift3Success.Value == 1 && (!bestLift.HasValue || form.Lifting3.Value > bestLift.Value))
                {
                    bestLift = form.Lifting3;
                }
                liftTest = form.Lifting3.Value;
            }
            int? bestPush = null;
            if (form.Push1.HasValue && form.Push1Success.HasValue)
            {              
                if (form.Push1Success.Value == 1 && (!bestPush.HasValue || form.Push1.Value > bestPush.Value))
                {
                    bestPush = form.Push1;
                }
                pushTest = form.Push1.Value;
            }
            if (form.Push2.HasValue && form.Push2Success.HasValue)
            {
                if (form.Push2Success.Value == 1 && (!bestPush.HasValue || form.Push2.Value > bestPush.Value))
                {
                    bestPush = form.Push2;
                }
                pushTest = form.Push2.Value;
            }
            if (form.Push3.HasValue && form.Push3Success.HasValue)
            {
                if (form.Push3Success.Value == 1 && (!bestPush.HasValue || form.Push3.Value > bestPush.Value))
                {
                    bestPush = form.Push3;
                }
                pushTest = form.Push3.Value;
            }

            int? finalResult = null;
            if (bestPush.HasValue && bestLift.HasValue)
            {
                if (bestPush == null || bestLift == null)
                    finalResult = null;
                else
                    finalResult = bestPush + bestLift;
            }

            var foundRegistration = disciplinesRepo.getCompetitionDisciplineRegistrationById(RegistrationId);
            if (foundRegistration != null)
            {
                var result = foundRegistration.CompetitionResult.FirstOrDefault();
                if (result == null)
                {
                    if (liftTest > 0)
                    {
                        isLiftingChanged = true;
                    }
                    else if (pushTest > 0)
                    {
                        isPushingChanged = true;
                    }
                    var compRes = new CompetitionResult
                    {
                        CompetitionRegistrationId = foundRegistration.Id,
                        Lifting1 = form.Lifting1,
                        Lifting2 = form.Lifting2,
                        Lifting3 = form.Lifting3,
                        Push1 = form.Push1,
                        Push2 = form.Push2,
                        Push3 = form.Push3,
                        Lift1Success = form.Lift1Success,
                        Lift2Success = form.Lift2Success,
                        Lift3Success = form.Lift3Success,
                        Push1Success = form.Push1Success,
                        Push2Success = form.Push2Success,
                        Push3Success = form.Push3Success,
                        LiftingResult = bestLift,
                        PushResult = bestPush,
                        FinalResult = finalResult

                    };
                    foundRegistration.CompetitionResult.Add(compRes);
                }
                else
                {
                    if (result.Lift1Success != form.Lift1Success || result.Lift2Success != form.Lift2Success || result.Lift3Success != form.Lift3Success || result.Lifting1 != form.Lifting1 || result.Lifting2 != form.Lifting2 || result.Lifting3 != form.Lifting3)
                    {
                        isLiftingChanged = true;
                    }
                    else if (result.Push1Success != form.Push1Success || result.Push2Success != form.Push2Success || result.Push3Success != form.Push3Success || result.Push1 != form.Push1 || result.Push2 != form.Push2 || result.Push3 != form.Push3)
                    {
                        isPushingChanged = true;
                    }
                    result.Lifting1 = form.Lifting1;
                    result.Lifting2 = form.Lifting2;
                    result.Lifting3 = form.Lifting3;
                    result.Push1 = form.Push1;
                    result.Push2 = form.Push2;
                    result.Push3 = form.Push3;
                    result.Lift1Success = form.Lift1Success;
                    result.Lift2Success = form.Lift2Success;
                    result.Lift3Success = form.Lift3Success;
                    result.Push1Success = form.Push1Success;
                    result.Push2Success = form.Push2Success;
                    result.Push3Success = form.Push3Success;
                    result.LiftingResult = bestLift;
                    result.PushResult = bestPush;
                    result.FinalResult = finalResult;
                }
                foundRegistration.CompetitionDiscipline.LastResultUpdate = DateTime.Now;
            }

            List<CompetitionDiscipline> competitionDisciplines = disciplinesRepo.GetCompetitionDisciplinesTrack(LeagueId).ToList();
            competitionDisciplines = competitionDisciplines.OrderBy(cd => cd.CompetitionAge.age_name, new LeagueRepo.WeightLiftingCategoryComparer()).ToList();

            var registrationsBySession = new List<CompetitionDisciplineRegistration>();
            foreach (var competitionDiscipline in competitionDisciplines)
            {
                registrationsBySession.AddRange(competitionDiscipline.CompetitionDisciplineRegistrations.Where(r => r.WeightliftingSessionId == SessionId).ToList());
            }
            int chosenRegId = 0;
            List<CompetitionDisciplineRegistration> suitables = new List<CompetitionDisciplineRegistration>();
            if (isLiftingChanged)
            {
                    suitables = registrationsBySession.Where(r => {
                        var result = r.CompetitionResult.FirstOrDefault();
                        var suitableDeclaration = -1;
                        var suitableDeclarationNumber = -1;
                        var previouslyAttemptedWeight = -1;
                        if (result != null)
                        {
                            var test1 = !result.Lift1Success.HasValue && result.Lifting1.HasValue; // && result.Lifting1 >= liftTest;
                            if (test1)
                            {
                                suitableDeclaration = result.Lifting1.Value;
                                suitableDeclarationNumber = 1;
                                previouslyAttemptedWeight = -1;
                            }
                            else
                            {
                                var test2 = !result.Lift2Success.HasValue && result.Lifting2.HasValue; // && result.Lifting2 >= liftTest;
                                if (test2)
                                {
                                    suitableDeclaration = result.Lifting2.Value;
                                    suitableDeclarationNumber = 2;
                                    previouslyAttemptedWeight = result.Lifting1 ?? -1;
                                }
                                else
                                {
                                    var test3 = !result.Lift3Success.HasValue && result.Lifting3.HasValue; // && result.Lifting3 >= liftTest;
                                    if (test3)
                                    {
                                        suitableDeclaration = result.Lifting3.Value;
                                        suitableDeclarationNumber = 3;
                                        previouslyAttemptedWeight = result.Lifting2 ?? -1;
                                    }
                                }
                            }
                            if (suitableDeclarationNumber > 0)
                            {
                                r.SuitableNextWeight = suitableDeclaration;
                                r.SuitableNextWeightNumber = suitableDeclarationNumber;
                                r.PreviouslyAttemptedWeight = previouslyAttemptedWeight;
                            }
                        }
                        return suitableDeclarationNumber > 0;
                    }).ToList();
            }
            else if (isPushingChanged)
            {
                 suitables = registrationsBySession.Where(r => {
                    var result = r.CompetitionResult.FirstOrDefault();
                    var suitableDeclaration = -1;
                    var suitableDeclarationNumber = -1;
                    var previouslyAttemptedWeight = -1;
                    if (result != null)
                    {
                         var test1 = !result.Push1Success.HasValue && result.Push1.HasValue; // && result.Push1 >= pushTest;
                         if (test1)
                        {
                            suitableDeclaration = result.Push1.Value;
                            suitableDeclarationNumber = 1;
                            previouslyAttemptedWeight = -1;
                         }
                        else
                        {
                             var test2 = !result.Push2Success.HasValue && result.Push2.HasValue; // && result.Push2 >= pushTest;
                             if (test2)
                            {
                                suitableDeclaration = result.Push2.Value;
                                suitableDeclarationNumber = 2;
                                previouslyAttemptedWeight = result.Push1 ?? -1;
                             }
                            else
                            {
                                 var test3 = !result.Push3Success.HasValue && result.Push3.HasValue; // && result.Push3 >= pushTest;
                                 if (test3)
                                {
                                    suitableDeclaration = result.Push3.Value;
                                    suitableDeclarationNumber = 3;
                                    previouslyAttemptedWeight = result.Push2 ?? -1;
                                 }
                            }
                        }
                        if (suitableDeclarationNumber > 0)
                        {
                            r.SuitableNextWeight = suitableDeclaration;
                            r.SuitableNextWeightNumber = suitableDeclarationNumber;
                            r.PreviouslyAttemptedWeight = previouslyAttemptedWeight;
                        }
                    }
                    return suitableDeclarationNumber > 0;
                }).ToList();
            }
            else
            {
                chosenRegId = form.ChosenNextRegId;
            }

            suitables = suitables.OrderBy(r => r.SuitableNextWeight ?? int.MaxValue).ThenBy(r => r.SuitableNextWeightNumber ?? int.MaxValue).ThenBy(r => r.PreviouslyAttemptedWeight ?? int.MaxValue).ToList();
            if (suitables.FirstOrDefault() != null)
            {
                chosenRegId = suitables.FirstOrDefault()?.Id ?? 0;
            }

            foreach (var competitionDiscipline in competitionDisciplines)
            {
                competitionDiscipline.LastResultChangeId = chosenRegId;
            }

            disciplinesRepo.Save();
            int? clubIdNext = null;
            if (ClubId > 0)
            {
                clubIdNext = ClubId;
            }
            return RedirectToAction("WeightLiftingResultsBySession", new { sessionId = SessionId, leagueId = LeagueId, chosenNextReg = chosenRegId, ClubId = clubIdNext });
            // return PartialView("Disciplines/_WeighLiftingResultsBySession");
        }
        public ActionResult WeightLiftingResultsBySession(int sessionId, int leagueId, int? clubId, int chosenNextReg = 0)
        {
            List<CompetitionDiscipline> competitionDisciplines = disciplinesRepo.GetCompetitionDisciplines(leagueId).ToList();

            // order by categories 
            competitionDisciplines = competitionDisciplines.OrderBy(cd => cd.CompetitionAge.age_name, new LeagueRepo.WeightLiftingCategoryComparer()).ToList();

            int comp_count = competitionDisciplines.ToArray().Count();
            CompetitionDiscipline[] model = new CompetitionDiscipline[comp_count];

            League league = leagueRepo.GetById(leagueId);

            ViewBag.isTeam = "false";
            ViewBag.isCup = "false";
            if (league.IsTeam)
                ViewBag.isTeam = "true";
            if (league.IsCompetitionLeague)
                ViewBag.isCup = "true";

            int i = 0;
            int reg_count = 0;
            ViewBag.IsCompetitionEnded = league.LeagueEndDate.HasValue ? DateTime.Now.Date.Ticks - league.LeagueEndDate.Value.Date.Ticks > 0 : false;
            ViewBag.CompetitionId = new int[comp_count];
            ViewBag.CategoryName = new string[comp_count];
            ViewBag.CompetitionName = new string[comp_count];
            ViewBag.CompetitionDisciplineId = new int[comp_count];
            ViewBag.SessionId = sessionId;
            foreach (CompetitionDiscipline cd in competitionDisciplines)
            {
                model[i] = cd;
                model[i].CompetitionDisciplineRegistrations = cd.CompetitionDisciplineRegistrations.Where(r => r.WeightliftingSessionId == sessionId).ToList();
                ViewBag.CompetitionId[i] = cd.CompetitionId;
                int? unionId = cd.League.UnionId;
                int? sectionId = cd.League.SeasonId;
                ViewBag.CategoryName[i] = cd.CompetitionAge.age_name;
                ViewBag.CompetitionName[i] = cd.League.Name;
                ViewBag.CompetitionDisciplineId[i] = cd.Id;

                reg_count += model[i].CompetitionDisciplineRegistrations.Count();
                i++;
            }

            ViewBag.compCount = comp_count;
            ViewBag.reg_count = reg_count;
            ViewBag.LeagueId = leagueId;
            ViewBag.ChosenNextReg = chosenNextReg;
            ViewBag.ClubId = clubId;
            return PartialView("Disciplines/_WeighLiftingResultsBySession", model);
        }

        [HttpPost]
        public ActionResult AddHeatWindAssociation(CompetitionHeatWind form)
        {
            disciplinesRepo.CreateHeatWindAssociation(form);
            return RedirectToAction("HeatWindTableLoop", new { Id = form.DisciplineCompetitionId });
        }


        public ActionResult ModifyAthleteCompetitionHeat(int regId, string heat)
        {
            disciplinesRepo.ModifyAthleteCompetitionHeat(regId, heat);
            return Json(new { Success = true});
        }


        public ActionResult ModifyCompetitionDisciplineHeatStartTime(int competitionDisciplineId, string heat, DateTime? startTime)
        {
            disciplinesRepo.ModifyCompetitionDisciplineHeatStartTime(competitionDisciplineId, heat, startTime);
            return Json(new { Success = true });
        }

        public ActionResult ModifyAthleteCompetitionLane(int regId, int? lane)
        {
            disciplinesRepo.ModifyAthleteCompetitionLane(regId, lane);
            return Json(new { Success = true });
        }


        public ActionResult ModifyAthleteCompetitionPresence(int regId, bool? presence)
        {
            disciplinesRepo.ModifyAthleteCompetitionPresence(regId, presence);
            return Json(new { Success = true });
        }


        [HttpPost]
        public ActionResult CopyAutomaticRankingToManual(int Id)
        {
            var disciplineCompetition = disciplinesRepo.GetCompetitionDisciplineById(Id);
            var discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

            List<CompetitionDisciplineRegistration> automaticalyRankedRegistrations = null;
            if (discipline.Format != null && discipline.Format.Value >= 6 && discipline.Format.Value <= 8)
            {
                var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().SortValue);
                var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations).ToList();
                automaticalyRankedRegistrations = res;
            }
            else
            {
                var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().SortValue);
                var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations).ToList();
                automaticalyRankedRegistrations = res;
            }

            int index = 1;
            foreach (var registration in automaticalyRankedRegistrations)
            {
                var results = registration.CompetitionResult.FirstOrDefault();
                if (results != null)
                {
                    results.Rank = index;
                }
                index += 1;
            }
            leagueRepo.Save();
            return Json(new { Success = true});
        }
        


        public ActionResult DeleteHeatWindAssociation(int Id)
        {
            var id = disciplinesRepo.DeleteHeatWindAssociation(Id);
            if(id != 0)
                return RedirectToAction("HeatWindTableLoop", new { Id = id });
            return Json(new { Success = false});
        }

        public ActionResult HeatWindTableLoop(int Id)
        {
            var vm = disciplinesRepo.GetCompetitionHeatWindList(Id);
            return PartialView("Disciplines/HeatWindLoop", vm);
        }

        public ActionResult UpdateRegistrationSwimming(int disciplineId, int clubId, int seasonId, int userId, string result1, string result2, string result3, string result4)
        {
            result1 = string.IsNullOrEmpty(result1) ? "00" : result1;
            result2 = string.IsNullOrEmpty(result2) ? "00" : result2;
            result3 = string.IsNullOrEmpty(result3) ? "00" : result3;
            result4 = string.IsNullOrEmpty(result4) ? "00" : result4;

            string entryTime = BuildResultString(result1, result2, result3, result4, 3);
            var playerDiscipline = db.PlayerDisciplines.FirstOrDefault(x => x.DisciplineId == disciplineId && x.SeasonId == seasonId && x.PlayerId == userId);
            if (playerDiscipline == null)
            {
                playerDiscipline = new PlayerDiscipline
                {
                    DisciplineId = disciplineId,
                    PlayerId = userId,
                    SeasonId = seasonId,
                    ClubId = clubId,
                    EntryTime = entryTime
                };
                db.PlayerDisciplines.Add(playerDiscipline);
            }
            else
            {
                playerDiscipline.EntryTime = entryTime;

            }
            db.SaveChanges();
            return Json(new {Success = true});
        }


        public ActionResult IsCompetitionResultValid(CompetitionResultForm form) {
            bool isValid = isResultFormatValid(form);
            return Json(new { Success = isValid });
        }

        private bool isResultFormatValid(CompetitionResultForm form) {
            bool isNotValid = false;
            int num;
            if (form.Result1 != null)
            {
                var isNum = int.TryParse(form.Result1, out num);
                if (!isNum)
                {
                    isNotValid = isNotValid || true;
                }
            }
            if (form.Result2 != null)
            {
                var isNum = int.TryParse(form.Result2, out num);
                if (!isNum)
                {
                    isNotValid = isNotValid || true;
                }
            }
            if (form.Result3 != null)
            {
                var isNum = int.TryParse(form.Result3, out num);
                if (!isNum)
                {
                    isNotValid = isNotValid || true;
                }
            }
            if (form.Result4 != null)
            {
                var isNum = int.TryParse(form.Result4, out num);
                if (!isNum)
                {
                    isNotValid = isNotValid || true;
                }
            }
            return !isNotValid;
        }

        public ActionResult AddCompetitionResultToAthlete(CompetitionResultForm form) {
            CompetitionDiscipline competitionDiscipline = disciplinesRepo.GetCompetitionDisciplineById(form.CompetitionDisciplineId);
            IEnumerable<CompetitionDisciplineRegistration> registrations = competitionDiscipline.CompetitionDisciplineRegistrations.AsEnumerable();
            bool isResultValidated = isResultFormatValid(form);
            var Result = BuildResultString(form.Result1, form.Result2, form.Result3, form.Result4, form.Format);
            double ResultForSort = BuildResultSortValue(form.Result1, form.Result2, form.Result3, form.Result4, form.Format);

            var foundRegistration = registrations.Where(r => r.User.UserId == form.UserId).FirstOrDefault();
            if (foundRegistration != null && isResultValidated)
            {
                var result = foundRegistration.CompetitionResult.FirstOrDefault();
                if (result == null)
                {
                    var compRes = new CompetitionResult
                    {
                        CompetitionRegistrationId = foundRegistration.Id,
                        Heat = form.Heat,
                        Lane = form.Lane,
                        Result = Result,
                        AlternativeResult = form.AlternativeResult,
                        Wind = form.Wind,
                        Rank = form.Rank,
                        ClubPoints = form.Points,
                        CombinedPoint = form.Points,
                        SortValue = Convert.ToInt64(ResultForSort)
                    };
                    foundRegistration.CompetitionResult.Add(compRes);
                }
                else
                {
                    result.Heat = form.Heat;
                    result.Lane = form.Lane;
                    result.Result = Result;
                    result.AlternativeResult = form.AlternativeResult;
                    result.Wind = form.Wind;
                    result.Rank = form.Rank;
                    if (form.Points.HasValue)
                    {
                        result.ClubPoints = form.Points;
                        result.CombinedPoint = form.Points;
                    }
                    result.SortValue = Convert.ToInt64(ResultForSort);
                }
            }
            else
            {
                return Json(new { Success = false });
            }
            disciplinesRepo.Save();
            return RedirectToAction("DisciplineResults", new { id = form.CompetitionDisciplineId, compIdLast = foundRegistration.Id});
        }




        [HttpPost]
        public ActionResult UpdateRegisteredAthleteRecordLiveResult(int id, string record)
        {
            disciplinesRepo.UpdateRegisteredAthleteRecordLiveResult(id, record);
            return Json(new { });
        }



        public ActionResult UpdateCompetitionRegistrationCustomFields6(int regId, string value, int column, int attempt)
        {
            var reg = leagueRepo.GetCompetitionDisciplineRegistration(regId);
            if (reg != null)
            {
                var result = reg.CompetitionResult.FirstOrDefault();
                if (result == null)
                {
                    result = new CompetitionResult();
                    reg.CompetitionResult.Add(result);
                }

                result.SetFormat6CustomFields(value, column, attempt);
                var possibleResults = reg.CompetitionDiscipline.GetFormat6CustomFields();
                var successIndex = result.GetSuccessIndex6(possibleResults.Count());
                if (successIndex > -2)
                {
                    if (successIndex > -1)
                    {
                        var resultColumn = successIndex / 3;
                        int attemppSuccessWithinColumn = successIndex % 3;
                        var resultStr = possibleResults[resultColumn];
                        result.Result = resultStr;
                        result.SortValue = Convert.ToInt64(Convert.ToDouble(resultStr) * 1000);
                        //column name id not correct but its not used in format 6 so I use it for ordering format 6 results instead of adding more columns.
                        result.LiveSplitOrder = Convert.ToInt32(Convert.ToDouble(resultStr) * 1000) * 100000 - attemppSuccessWithinColumn * 10000 - result.GetNumberFailsForCustomFields6(possibleResults.Count());
                    }
                    else {
                        //need to know what to do when he fails attempts other than not participating at all.
                        result.Result = null;
                        result.SortValue = null;
                        result.LiveSplitOrder = null;
                    }
                }
                else
                {
                    result.Result = null;
                    result.SortValue = null;
                    result.LiveSplitOrder = null;
                }
                reg.CompetitionDiscipline.LastResultUpdate = DateTime.Now;
                leagueRepo.Save();
                return Json(new { @regId = regId, finalResult = result.Result == null ? string.Empty : result.Result, @column = column });
            }
            return Json(new { @regId = regId, finalResult = string.Empty, @column = column });

        }


        public ActionResult UpdateRegisteredAthleteCompetitionLiveResultWind(int RegId, float? ResultWind, int Number, int CompetitionDisciplineId)
        {
            var reg = leagueRepo.GetCompetitionDisciplineRegistration(RegId);
            if (reg != null)
            {
                var result = reg.CompetitionResult.FirstOrDefault();
                if (result == null)
                {
                    result = new CompetitionResult();
                    reg.CompetitionResult.Add(result);
                }
                switch (Number)
                {
                    case 1:
                        if (result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult
                            {
                                CompetitionRegistrationId = RegId,
                                Attempt1Wind = ResultWind
                            });
                        }
                        else
                        {
                            result.Attempt1Wind = ResultWind;
                        }
                        break;
                    case 2:
                        if (result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult
                            {
                                CompetitionRegistrationId = RegId,
                                Attempt2Wind = ResultWind
                            });
                        }
                        else
                        {
                            result.Attempt2Wind = ResultWind;
                        }
                        break;
                    case 3:
                        if (result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult
                            {
                                CompetitionRegistrationId = RegId,
                                Attempt3Wind = ResultWind
                            });
                        }
                        else
                        {
                            result.Attempt3Wind = ResultWind;
                        }
                        break;
                    case 4:
                        if (result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult
                            {
                                CompetitionRegistrationId = RegId,
                                Attempt4Wind = ResultWind
                            });
                        }
                        else
                        {
                            result.Attempt4Wind = ResultWind;
                        }
                        break;
                    case 5:
                        if (result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult
                            {
                                CompetitionRegistrationId = RegId,
                                Attempt5Wind = ResultWind
                            });
                        }
                        else
                        {
                            result.Attempt5Wind = ResultWind;
                        }
                        break;
                    case 6:
                        if (result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult
                            {
                                CompetitionRegistrationId = RegId,
                                Attempt6Wind = ResultWind
                            });
                        }
                        else
                        {
                            result.Attempt6Wind = ResultWind;
                        }
                        break;
                }
                UpdateFormat7_10_11LiveResults(result, reg);
                return Json(new { regId = RegId, finalResult = result.Result == null ? string.Empty : (result.AlternativeResult == 0 ? result.Result : UIHelpers.GetAlternativeResultStringByValue(result.AlternativeResult)), wind = (result == null || result.AlternativeResult != 0) ? string.Empty : (result.Wind.HasValue ? Math.Round(result.Wind.Value, 2).ToString() : string.Empty) });
            }
            return Json(new { regId = RegId, finalResult = string.Empty, wind = string.Empty });
        }


        public ActionResult UpdateRegisteredAthleteCompetitionLiveAlternativeResult(int RegId, int AlternativeResult, int CompetitionDisciplineId)
        {
            var reg = leagueRepo.GetCompetitionDisciplineRegistration(RegId);
            if (reg != null)
            {
                var result = reg.CompetitionResult.FirstOrDefault();
                if (result == null)
                {
                    result = new CompetitionResult();
                    reg.CompetitionResult.Add(result);
                }

                result.AlternativeResult = AlternativeResult;
                reg.CompetitionDiscipline.LastResultUpdate = DateTime.Now;
                leagueRepo.Save();
                return Json(new { regId = RegId, finalResult = result.Result == null ? string.Empty : (result.AlternativeResult == 0 ? result.Result : UIHelpers.GetAlternativeResultStringByValue(result.AlternativeResult)), wind = result == null || result.AlternativeResult != 0 ? string.Empty : Math.Round(result.Wind.Value, 2).ToString() });
            }
            return Json(new { regId = RegId, finalResult = string.Empty, wind = string.Empty });
        }

        private void UpdateFormat7_10_11LiveResults(CompetitionResult result, CompetitionDisciplineRegistration reg) {
            var sortValueList = new List<double>();
            sortValueList.Add(result.Alternative1 == 0 ? Convert.ToDouble(result.Attempt1) * 1000 : -1);
            sortValueList.Add(result.Alternative2 == 0 ? Convert.ToDouble(result.Attempt2) * 1000 : -1);
            sortValueList.Add(result.Alternative3 == 0 ? Convert.ToDouble(result.Attempt3) * 1000 : -1);
            sortValueList.Add(result.Alternative4 == 0 ? Convert.ToDouble(result.Attempt4) * 1000 : -1);
            sortValueList.Add(result.Alternative5 == 0 ? Convert.ToDouble(result.Attempt5) * 1000 : -1);
            sortValueList.Add(result.Alternative6 == 0 ? Convert.ToDouble(result.Attempt6) * 1000 : -1);

            var max = sortValueList.Max();
            int maxIndex = sortValueList.IndexOf(max);

            if (max > -1)
            {
                switch (maxIndex)
                {
                    case 0:
                        result.Result = result.Attempt1;
                        result.Wind = result.Attempt1Wind;
                        result.SortValue = Convert.ToInt64(Convert.ToDouble(result.Attempt1) * 1000);
                        break;
                    case 1:
                        result.Result = result.Attempt2;
                        result.Wind = result.Attempt2Wind;
                        result.SortValue = Convert.ToInt64(Convert.ToDouble(result.Attempt2) * 1000);
                        break;
                    case 2:
                        result.Result = result.Attempt3;
                        result.Wind = result.Attempt3Wind;
                        result.SortValue = Convert.ToInt64(Convert.ToDouble(result.Attempt3) * 1000);
                        break;
                    case 3:
                        result.Result = result.Attempt4;
                        result.Wind = result.Attempt4Wind;
                        result.SortValue = Convert.ToInt64(Convert.ToDouble(result.Attempt4) * 1000);
                        break;
                    case 4:
                        result.Result = result.Attempt5;
                        result.Wind = result.Attempt5Wind;
                        result.SortValue = Convert.ToInt64(Convert.ToDouble(result.Attempt5) * 1000);
                        break;
                    case 5:
                        result.Result = result.Attempt6;
                        result.Wind = result.Attempt6Wind;
                        result.SortValue = Convert.ToInt64(Convert.ToDouble(result.Attempt6) * 1000);
                        break;
                }
            }
            else
            {
                result.Result = null;
                result.SortValue = null;
            }
            
            reg.CompetitionDiscipline.LastResultUpdate = DateTime.Now;
            leagueRepo.Save();
        }

        public ActionResult UpdateRegisteredAthleteCompetitionLiveResult(int RegId, string Result, int Alternative, int Number, int CompetitionDisciplineId)
        {
            if (string.IsNullOrWhiteSpace(Result))
            {
                Result = null;
            }
            var reg = leagueRepo.GetCompetitionDisciplineRegistration(RegId);
            if(reg != null)
            {
                var result = reg.CompetitionResult.FirstOrDefault();
                if(result == null)
                {
                    result = new CompetitionResult();
                    reg.CompetitionResult.Add(result);
                }

                switch (Number)
                {
                    case 1:
                        if(result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult {
                                CompetitionRegistrationId = RegId,
                                Attempt1 = Result,
                                Alternative1 = Alternative
                            });
                        }
                        else
                        {
                            result.Attempt1 = Result;
                            result.Alternative1 = Alternative;
                        }
                        break;
                    case 2:
                        if (result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult
                            {
                                CompetitionRegistrationId = RegId,
                                Attempt2 = Result,
                                Alternative2 = Alternative
                            });
                        }
                        else
                        {
                            result.Attempt2 = Result;
                            result.Alternative2 = Alternative;
                        }
                        break;
                    case 3:
                        if (result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult
                            {
                                CompetitionRegistrationId = RegId,
                                Attempt3 = Result,
                                Alternative3 = Alternative
                            });
                        }
                        else
                        {
                            result.Attempt3 = Result;
                            result.Alternative3 = Alternative;
                        }
                        break;
                    case 4:
                        if (result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult
                            {
                                CompetitionRegistrationId = RegId,
                                Attempt4 = Result,
                                Alternative4 = Alternative
                            });
                        }
                        else
                        {
                            result.Attempt4 = Result;
                            result.Alternative4 = Alternative;
                        }
                        break;
                    case 5:
                        if (result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult
                            {
                                CompetitionRegistrationId = RegId,
                                Attempt5 = Result,
                                Alternative5 = Alternative
                            });
                        }
                        else
                        {
                            result.Attempt5 = Result;
                            result.Alternative5 = Alternative;
                        }
                        break;
                    case 6:
                        if (result == null)
                        {
                            reg.CompetitionResult.Add(new CompetitionResult
                            {
                                CompetitionRegistrationId = RegId,
                                Attempt6 = Result,
                                Alternative6 = Alternative
                            });
                        }
                        else
                        {
                            result.Attempt6 = Result;
                            result.Alternative6 = Alternative;
                        }
                        break;
                }
                UpdateFormat7_10_11LiveResults(result, reg);
                return Json(new { regId = RegId, finalResult = result.Result == null ? string.Empty : (result.AlternativeResult == 0 ? result.Result : UIHelpers.GetAlternativeResultStringByValue(result.AlternativeResult)), wind = (result == null || result.AlternativeResult != 0) ? string.Empty : (result.Wind.HasValue ? Math.Round(result.Wind.Value, 2).ToString() : string.Empty) });
            }
            return Json(new { regId = RegId, finalResult = string.Empty, wind = string.Empty });
        }


        [HttpPost]
        public ActionResult UpdateNumberOfPassesToNextStageForLiveResult(int CompetitionDisciplineId, int? NumPassNextStage)
        {
            disciplinesRepo.UpdateNumberOfPassesToNextStageForLiveResult(CompetitionDisciplineId, NumPassNextStage);
            var competitionDiscipline = disciplinesRepo.GetCompetitionDisciplineById(CompetitionDisciplineId);
            var discipline = disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);

            var regs = new List<CompetitionDisciplineRegistration>();
            if (competitionDiscipline.IsResultsManualyRanked)
            {
                var resulted = competitionDiscipline.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().Rank ?? int.MaxValue).ToList();
                regs = resulted;
            }
            else if (discipline.Format != null && ((discipline.Format.Value >= 6 && discipline.Format.Value <= 8) || discipline.Format.Value == 10 || discipline.Format.Value == 11))
            {
                var resulted = competitionDiscipline.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().SortValue);
                var res = resulted.Union(competitionDiscipline.CompetitionDisciplineRegistrations).ToList();
                regs = res;
            }
            else
            {
                var resulted = competitionDiscipline.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().SortValue);
                var res = resulted.Union(competitionDiscipline.CompetitionDisciplineRegistrations).ToList();
                regs = res;
            }
            if (NumPassNextStage.HasValue)
            {
                for (int i = 0; i < regs.Count; i++)
                {
                    var reg = regs[i];
                    var result = reg.CompetitionResult.FirstOrDefault();

                    var place = i;
                    if(NumPassNextStage.Value > i)
                    {
                        place = NumPassNextStage.Value-1-i;
                    }


                    if (result == null)
                    {
                        result = new CompetitionResult
                        {
                            LiveSplitOrder = place
                        };
                        reg.CompetitionResult.Add(result);
                    }
                    else
                    {
                        result.LiveSplitOrder = place;
                    }
                }
            }
            else
            {
                for (int i = 0; i < regs.Count; i++)
                {
                    var reg = regs[i];
                    var result = reg.CompetitionResult.FirstOrDefault();
                    if (result != null)
                    {
                        result.LiveSplitOrder = null;
                    }
                }
            }
            competitionDiscipline.LastResultUpdate = DateTime.Now;
            disciplinesRepo.Save();
            return RedirectToAction("DisciplineLiveResults", new { id = CompetitionDisciplineId });
        }

        public ActionResult UpdateRegisteredAthleteCompetitionResult(UpdateDisciplineRegistrationResultForm data)
        {
            if (data.Id == 0) {
                ModelState.AddModelError(nameof(data.Id), Messages.FieldIsRequired);
            }

            if (data.CompetitionResultId == 0)
            {
                ModelState.AddModelError(nameof(data.CompetitionResultId), Messages.FieldIsRequired);
            }


            var disciplineCompetition = disciplinesRepo.GetCompetitionDisciplineById(data.Id);
            var discipline = disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

            var disciplineResults = disciplinesRepo.GetCompetitionResultById(data.CompetitionResultId);
            if (discipline.Format != null)
            {
                data.Format = discipline.Format.Value;
            }

            ViewBag.CompetitionId = disciplineCompetition.CompetitionId;
            ViewBag.Format = discipline.Format;
            ViewBag.DisciplineName = discipline.Name;

            disciplineCompetition.CompetitionDisciplineRegistrations = disciplineCompetition.CompetitionDisciplineRegistrations.OrderBy(r => r.User.FullName).ToList();
            //TODO validate format
            //TODO ERROR MESSAGE
            if (data.Result1 != null)
            {
                int num;
                var isNum = int.TryParse(data.Result1, out num);
                if (!isNum) {
                    ModelState.AddModelError(nameof(data.Result1), Messages.MustBeNumber);
                }
            }
            if (data.Result2 != null)
            {
                int num;
                var isNum = int.TryParse(data.Result2, out num);
                if (!isNum)
                {
                    ModelState.AddModelError(nameof(data.Result2), Messages.MustBeNumber);
                }
            }
            if (data.Result3 != null)
            {
                int num;
                var isNum = int.TryParse(data.Result3, out num);
                if (!isNum)
                {
                    ModelState.AddModelError(nameof(data.Result3), Messages.MustBeNumber);
                }
            }
            if (data.Result4 != null)
            {
                int num;
                var isNum = int.TryParse(data.Result4, out num);
                if (!isNum)
                {
                    ModelState.AddModelError(nameof(data.Result4), Messages.MustBeNumber);
                }
            }
            if (!ModelState.IsValid)
            {
                var showErrorMessageString = new
                {
                    isError = true,
                    param2 = "Exception occured while converting user date"
                };
                return Json(showErrorMessageString, JsonRequestBehavior.AllowGet);
            }

            var Result = BuildResultString(data.Result1, data.Result2, data.Result3, data.Result4, discipline.Format);
            double ResultForSort = BuildResultSortValue(data.Result1, data.Result2, data.Result3, data.Result4, discipline.Format);
            if (disciplineResults == null)
            {
                //New CompetitionResult
                disciplineResults = new CompetitionResult();
                disciplineResults.Heat = data.Heat;
                disciplineResults.Lane = data.Lane;
                disciplineResults.Wind = data.Wind;
                disciplineResults.Result = Result;
                disciplineResults.SortValue = (long)ResultForSort;
                var registeredCompetition = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r=> r.Id == data.CompetitionResultId).FirstOrDefault();
                registeredCompetition.CompetitionResult.Add(disciplineResults);
                disciplinesRepo.Save();
            }
            else
            {
                disciplineResults.Heat = data.Heat;
                disciplineResults.Lane = data.Lane;
                disciplineResults.Wind = data.Wind;
                disciplineResults.Result = Result;
                disciplineResults.SortValue = (long)ResultForSort;
                disciplinesRepo.Save();
            }
            var showMessageString = new
            {
                isError = false,
                compIdUpdated = data.CompetitionResultId
            };
            return Json(showMessageString, JsonRequestBehavior.AllowGet);
            //return PartialView("Disciplines/_DisciplinesResults", disciplineCompetition);
        }

        private double BuildResultSortValue(string result1, string result2, string result3, string result4, int? format)
        {
            if (format == null)
            {
                format = 0;
            }
            if (result1 != null) {
                result1 = result1.Trim();
            }
            if (result2 != null)
            {
                result2 = result2.Trim();
            }
            if (result3 != null)
            {
                result3 = result3.Trim();
            }
            if (result4 != null)
            {
                result4 = result4.Trim();
            }
            if (string.IsNullOrWhiteSpace(result1)) result1 = "0";
            if (string.IsNullOrWhiteSpace(result2)) result2 = "0";
            if (string.IsNullOrWhiteSpace(result3)) result3 = "0";
            if (string.IsNullOrWhiteSpace(result4)) result4 = "0";

            switch (format)
            {
                case 1:
                    {
                        var str = $"00:00:{result1}.{result2}";
                        TimeSpan _time = TimeSpan.Parse(str);
                        return _time.TotalMilliseconds;
                    }
                case 2:
                    {
                        var str = $"00:{result1}:{result2}.{result3}";
                        TimeSpan _time = TimeSpan.Parse(str);
                        return _time.TotalMilliseconds;
                    }
                case 3:
                    {
                        var str = $"00:{result1}:{result2}.{result3}";
                        TimeSpan _time = TimeSpan.Parse(str);
                        return _time.TotalMilliseconds;
                    }
                case 4:
                    {
                        int hours;
                        bool isNum = int.TryParse(result1, out hours);
                        var str = $"{result1}:{result2}:{result3}.{result4}";
                        if (isNum && hours >= 24)
                        {
                            int days = hours / 24;
                            int rHours = hours - (days * 24);
                            var toParse = $"{string.Format("{0:00}", days)}:{string.Format("{0:00}", rHours)}:{result2}:{result3}.00";
                            TimeSpan _time = TimeSpan.Parse(toParse);
                            return _time.TotalMilliseconds;
                        }
                        else
                        {
                            TimeSpan _time = TimeSpan.Parse(str);
                            return _time.TotalMilliseconds;
                        }
                    }
                case 5:
                    {
                        int hours;
                        bool isNum = int.TryParse(result1, out hours);
                        var str = $"{result1}:{result2}:{result3}.00";
                        if (isNum && hours >= 24) {
                            int days = hours / 24;
                            int rHours = hours -(days * 24);
                            var toParse = $"{string.Format("{0:00}", days)}:{string.Format("{0:00}", rHours)}:{result2}:{result3}.00";
                            TimeSpan _time = TimeSpan.Parse(toParse);
                            return _time.TotalMilliseconds;
                        }
                        else {
                            TimeSpan _time = TimeSpan.Parse(str);
                            return _time.TotalMilliseconds;
                        }
                    }
                case 9:
                    {
                        var str = $"00:{result1}:{result2}.00";
                        TimeSpan _time = TimeSpan.Parse(str);
                        return _time.TotalMilliseconds;
                    }
                case 10:
                case 6:
                    {
                        var str = $"{result1}.{result2}";
                        return (Convert.ToDouble(str)*1000);
                    }
                case 11:
                case 7:
                    {
                        var str = $"{result1}.{result2}";
                        return (Convert.ToDouble(str) * 1000);
                    }
                case 8:
                    {
                        var str = $"{result1}.{result2}";
                        return (Convert.ToDouble(str) * 1000);
                    }
                default:
                    {
                        double num;
                        bool res = double.TryParse(result1, out num);
                        if (res == false)
                        {
                            return 0;
                        }
                        return num;
                    }
            }
        }

        private string BuildResultString(string result1, string result2, string result3, string result4, int? format)
        {
            if (format == null) {
                format = 0;
            }
            if (result1 != null)
            {
                result1 = result1.Trim();
            }
            if (result2 != null)
            {
                result2 = result2.Trim();
            }
            if (result3 != null)
            {
                result3 = result3.Trim();
            }
            if (result4 != null)
            {
                result4 = result4.Trim();
            }
            switch (format) {
                case 1:
                    {
                        return $"00:00:{result1}.{result2}";        
                    }
                case 2:
                    {
                        return $"00:{result1}:{result2}.{result3}";
                    }
                case 3:
                    {
                        return $"00:{result1}:{result2}.{result3}";
                    }
                case 4:
                    {
                        return $"{result1}:{result2}:{result3}.{result4}";
                    }
                case 5:
                    {
                        return $"{result1}:{result2}:{result3}.00";
                    }
                case 9:
                    {
                        return $"00:{result1}:{result2}.00";
                    }
                case 10:
                case 6:
                    {
                        return $"{result1}.{result2}";
                    }
                case 11:
                case 7:
                    {
                        return $"{result1}.{result2}";
                    }
                case 8:
                    {
                        return $"{result1}.{result2}";
                    }
                default:
                    {
                        return result1;
                    }
            }
        }

        public ActionResult UpdateDiscipline(int id, bool isBicycle = false)
        {
            CompetitionDiscipline disciplineCompetition = null;
            CompetitionExperty competitionExperty = null;    
            if(isBicycle)
            {
                competitionExperty = disciplinesRepo.GetCompetitionExpertyById(id);
            }
            else
            {
                disciplineCompetition = disciplinesRepo.GetCompetitionDisciplineById(id);
            }
            
            var compId = isBicycle ? competitionExperty.CompetitionId : disciplineCompetition.CompetitionId;
            ViewBag.CompetitionId = compId;
            ViewBag.IsUpdate = true;

            int? unionId = db.Leagues.Where(c => c.LeagueId == compId).Select(c => c.UnionId).FirstOrDefault();
            int? sectionId = db.Unions.Where(c => c.UnionId == unionId).Select(c => c.SectionId).FirstOrDefault();
            var sectionAlias = secRepo.GetById(sectionId.Value).Alias;
            if (isBicycle)
            {
                ViewBag.DisciplineExpertiseList = new SelectList(db.DisciplineExpertises.Where(x => x.BicycleCompetitionDiscipline.UnionId == unionId.Value).OrderBy(c => c.Name), nameof(DisciplineExpertise.Id), nameof(DisciplineExpertise.Name));
            }
            else
            {
                ViewBag.CategoryList = new SelectList(leagueRepo.GetCompetitionCategories(disciplineCompetition.CompetitionId), nameof(CompetitionAge.id), nameof(CompetitionAge.age_name));
                ViewBag.DistanceList = new SelectList(leagueRepo.GetCompetitionRowingDistances(disciplineCompetition.CompetitionId).OrderBy(c => c.Name), nameof(RowingDistance.Id), nameof(RowingDistance.Name));
                ViewBag.DisciplineList = new SelectList(disciplinesRepo.GetBySection((int)sectionId, (int)unionId), nameof(Discipline.DisciplineId), nameof(Discipline.Name));

            }

            if (!isBicycle)
            {
                var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == disciplineCompetition.DisciplineId);
                int? format = null;
                if (discipline != null)
                {
                    format = discipline.Format;
                }


                return PartialView("Disciplines/_Create", new CompetitionDisciplineViewModel
                {
                    Id = id,
                    DisciplineId = disciplineCompetition.DisciplineId,
                    CompetitionId = disciplineCompetition.CompetitionId,
                    CategoryId = disciplineCompetition.CategoryId.ToString(),
                    DistanceId = disciplineCompetition.DistanceId.ToString(),
                    MaxSportsmen = disciplineCompetition.MaxSportsmen,
                    MinResult = disciplineCompetition.MinResult,
                    StartTime = disciplineCompetition.StartTime,
                    CompetitionRecord = disciplineCompetition.CompetitionRecord,
                    IsResultsManualyRanked = disciplineCompetition.IsResultsManualyRanked,
                    UseAllProps = secRepo.GetById(sectionId.Value).Name == GamesAlias.Athletics,
                    IsSwimming = secRepo.GetById(sectionId.Value).Name == GamesAlias.Swimming,
                    IsRowing = secRepo.GetById(sectionId.Value).Alias == GamesAlias.Rowing,
                    IsBicycle = sectionAlias == SectionAliases.Bicycle,
                    IsClimbing = sectionAlias == SectionAliases.Climbing,
                    Format = format,
                    IncludeRecordInStartList = disciplineCompetition.IncludeRecordInStartList
                });
            }
            else
            {
                return PartialView("Disciplines/_Create", new CompetitionDisciplineViewModel
                {
                    Id = id,
                    CompetitionId = competitionExperty.CompetitionId.Value,
                    DisciplineExpertiseId = competitionExperty.DisciplineExpertiseId.Value.ToString(),
                    UseAllProps = false,
                    IsBicycle = true
                });
            }
        }


        [HttpPost]
        public ActionResult UpdateDiscipline(CompetitionDisciplineViewModel vm)
        {
            if (!vm.IsBicycle)
            {
                if (string.IsNullOrEmpty(vm.CategoryId)) ModelState.AddModelError("CategoryId", Messages.FieldIsRequired);
            }
            else
            {
                if (string.IsNullOrEmpty(vm.DisciplineExpertiseId)) ModelState.AddModelError("DisciplineExpertiseId", Messages.FieldIsRequired);
                ModelState.Remove("DisciplineId");
            }
            if (!ModelState.IsValid)
            {
                int? unionId = db.Leagues.Where(c => c.LeagueId == vm.CompetitionId).Select(c => c.UnionId).FirstOrDefault();
                int? sectionId = db.Unions.Where(c => c.UnionId == unionId).Select(c => c.SectionId).FirstOrDefault();
                ViewBag.IsUpdate = true;                
                ViewBag.CompetitionId = vm.CompetitionId;
                if (!vm.IsBicycle)
                {
                    ViewBag.CategoryList = new SelectList(leagueRepo.GetCompetitionCategories(vm.CompetitionId), nameof(CompetitionAge.id), nameof(CompetitionAge.age_name));
                    ViewBag.DistanceList = new SelectList(leagueRepo.GetCompetitionRowingDistances(vm.CompetitionId).OrderBy(c => c.Name), nameof(RowingDistance.Id), nameof(RowingDistance.Name));
                    ViewBag.DisciplineList = new SelectList(disciplinesRepo.GetBySection((int)sectionId, (int)unionId), nameof(Discipline.DisciplineId), nameof(Discipline.Name));
                }
                else
                {
                    ViewBag.DisciplineExpertiseList = new SelectList(db.DisciplineExpertises.Where(x => x.BicycleCompetitionDiscipline.UnionId == unionId.Value).OrderBy(c => c.Name), nameof(DisciplineExpertise.Id), nameof(DisciplineExpertise.Name));
                }

                
                return PartialView("Disciplines/_Create", vm);
            }
            if (!vm.IsBicycle)
            {
                var catList = vm.CategoryId.Split(',').Select(int.Parse).ToList();
                List<int> distList = new List<int>();
                if (!string.IsNullOrWhiteSpace(vm.DistanceId))
                {
                    distList = vm.DistanceId.Split(',').Select(int.Parse).ToList();
                }
                if(vm.IsRowing)
                {
                    var disc = vm.BoatTypesId.Split(',').Select(int.Parse).ToList();
                    vm.DisciplineId = disc.FirstOrDefault();
                }
                foreach (var cat in catList)
                {
                    if (distList.Count() == 0)
                    {
                        disciplinesRepo.UpdateCompetitionDiscipline(new CompetitionDiscipline
                        {
                            Id = vm.Id,
                            DisciplineId = vm.DisciplineId,
                            CategoryId = cat,
                            CompetitionId = vm.CompetitionId,
                            MinResult = vm.MinResult,
                            MaxSportsmen = vm.MaxSportsmen,
                            CompetitionRecord = vm.CompetitionRecord,
                            StartTime = vm.StartTime,
                            IsResultsManualyRanked = vm.IsResultsManualyRanked,
                            IncludeRecordInStartList = vm.IncludeRecordInStartList
                        });
                    }
                    else
                    {
                        foreach (var dist in distList)
                        {
                            disciplinesRepo.UpdateCompetitionDiscipline(new CompetitionDiscipline
                            {
                                Id = vm.Id,
                                DisciplineId = vm.DisciplineId,
                                CategoryId = cat,
                                DistanceId = dist,
                                CompetitionId = vm.CompetitionId,
                                MinResult = vm.MinResult,
                                MaxSportsmen = vm.MaxSportsmen,
                                StartTime = vm.StartTime,
                                IsResultsManualyRanked = vm.IsResultsManualyRanked
                            });
                        }
                    }
                }
            }
            else
            {
                var expList = vm.DisciplineExpertiseId.Split(',').Select(int.Parse).ToList();
                var expId = expList.LastOrDefault();

                var competitionExpertise = disciplinesRepo.GetCompetitionExpertyById(vm.Id);

                competitionExpertise.DisciplineExpertiseId = expId;
            }

            disciplinesRepo.Save();
            return Json(new { Success = true });
        }

        public ActionResult DownloadTennisCompetitionImportForm()
        {
            bool isHebrew = getCulture() == CultEnum.He_IL;

            string file = HostingEnvironment.MapPath(isHebrew
                ? "~/Content/excelfiles/hebrew_tennis_category_excel.xlsx"
                : "~/Content/excelfiles/english_tennis_category_excel.xlsx");

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(file, contentType, Path.GetFileName(file));
        }

        [HttpGet]
        public ActionResult ImportTennisCompetitionPlayers(int competitionId, int seasonId)
        {
            ImportPlayersViewModel model = new ImportPlayersViewModel
            {
                ImportFile = null,
                LeagueId = competitionId
            };

            return PartialView("_ImportTennisCompetitionPlayers", model);
        }

        [HttpPost]
        public ActionResult ImportTennisCompetitionPlayers(ImportPlayersViewModel model)
        {
            try
            {
                if (model.ImportFile != null)
                {
                    var importHelper = new ImportExportPlayersHelper(db);

                    importHelper.ExtractTennisCompetitionPlayersData(model.ImportFile.InputStream,
                        out List<ImportTennisCompetitionPlayerModel> correctRows,
                        out List<ImportTennisCompetitionPlayerModel> validationErrorRows);

                    if (correctRows.Count > 0)
                    {
                        model.SuccessCount = importHelper.ImportTennisCompetitionRegistrations(model.LeagueId.Value, model.SeasonId.Value, correctRows,
                            out List<ImportTennisCompetitionPlayerModel> importErrorRows);

                        validationErrorRows.AddRange(importErrorRows);

                        if (validationErrorRows.Count > 0 && model.SuccessCount > 0)
                        {
                            CreateErrorImportFileForTennisCompetition(importHelper, model, validationErrorRows);
                            model.Result = ImportPlayersResult.PartialyImported;
                            model.ResultMessage = Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, Messages.Gymnastics);
                            model.ErrorCount = validationErrorRows.Count;
                        }
                        else if (validationErrorRows.Count == 0 && model.SuccessCount > 0)
                        {
                            model.Result = ImportPlayersResult.Success;
                        }
                        else
                        {
                            CreateErrorImportFileForTennisCompetition(importHelper, model, validationErrorRows);
                            model.Result = ImportPlayersResult.Error;
                            model.ResultMessage = Messages.ImportPlayers_NoRowLoaded;
                            model.ErrorCount = validationErrorRows.Count;
                        }
                    }
                    else
                    {
                        model.Result = ImportPlayersResult.Error;
                        model.ResultMessage = Messages.ImportPlayers_NoRowLoaded;
                        model.SuccessCount = 0;
                        model.DuplicateCount = 0;
                        model.ErrorCount = correctRows.Count - model.SuccessCount;
                        CreateErrorImportFileForTennisCompetition(importHelper, model, validationErrorRows);
                    }
                }
                else
                {
                    model.Result = ImportPlayersResult.Error;
                    model.ResultMessage = Messages.ImportPlayers_ChooseFile;
                }
                return PartialView("_ImportTennisCompetitionPlayers", model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


 

        private void CreateErrorImportFileForTennisCompetition(ImportExportPlayersHelper importHelper, ImportPlayersViewModel model, List<ImportTennisCompetitionPlayerModel> validationErrorRows)
        {
            var culture = getCulture();
            byte[] errorFileBytes = null;
            using (var errorFile = importHelper.BuildErrorFileForTennisCompetition(validationErrorRows, culture))
            {
                errorFileBytes = new byte[errorFile.Length];
                errorFile.Read(errorFileBytes, 0, errorFileBytes.Length);
            }
            Session.Remove(ImportPlayersErrorResultSessionKey);
            Session.Remove(ImportPlayersErrorResultFileNameSessionKey);
            Session.Add(ImportPlayersErrorResultSessionKey, errorFileBytes);
            Session.Add(ImportPlayersErrorResultFileNameSessionKey, model.ImportFile.FileName);
        }

        [HttpGet]
        public ActionResult DownloadPartiallyImport()
        {
            var fileByteObj = Session[ImportPlayersErrorResultSessionKey];
            if (fileByteObj == null)
            {
                throw new FileNotFoundException();
            }

            var fileBytes = (byte[])fileByteObj;
            var fileName = Session[ImportPlayersErrorResultFileNameSessionKey] as string;

            FileInfo fi = new FileInfo(fileName);

            return File(fileBytes, "application/octet-stream", string.Format("{0}-{2}{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension, Messages.ImportPlayers_OutputFilePrefix));
        }

        public ActionResult LoadTennisCompetitionPlayers(int leagueId)
        {
            var tennisCompetitionPlayers = playersRepo.LoadExcelTennisCompetitionRegistrations(leagueId);

            ViewBag.IsValidToUncheck = User.IsInAnyRole(AppRole.Admins)
                || usersRepo.GetTopLevelJob(AdminId) == JobRole.LeagueManager
                || usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager;

            return PartialView("_TennisCompetitionPlayersList", tennisCompetitionPlayers);
        }

        public void ChangeTennisCompetitionPlayerActiveStatus(int regId, bool isActive)
        {
            playersRepo.ChangeTennisCompetitionPlayerActiveStatus(regId, isActive);
        }

        public ActionResult CreateLevelSettings(int competitionId, LevelDateSettingDto settings)
        {
            leagueRepo.CreateLevelSettings(settings, competitionId);
            var league = leagueRepo.GetById(competitionId);
            var vm = new LevelDatesSettingsForm
            {
                CompetitionId = league.LeagueId,
                LevelDatesSettings = league.LevelDateSettings?.Select(lds => new LevelDateSettingDto
                {
                    Id = lds.Id,
                    CompetitionLevelId = lds.CompetitionLevelId,
                    QualificationStartDate = lds.QualificationStartDate,
                    QualificationEndDate = lds.QualificationEndDate,
                    FinalStartDate = lds.FinalStartDate,
                    FinalEndDate = lds.FinalEndDate
                }),
                LevelList = leagueRepo.GetCompetitionLevels(league?.UnionId)
            };
            return PartialView("_LevelDatesSettings", vm);
        }

        public ActionResult DeleteLevelSettings(int id)
        {
            leagueRepo.DeleteLevelSettings(id);
            return Json(new { Success = true });
        }

        public ActionResult UpdateLevelSettings(LevelDateSettingDto settings)
        {
            leagueRepo.UpdateLevelSettings(settings);
            return Json(new { Success = true });
        }

        [HttpPost]
        public JsonResult DeleteAllRegistrations(int? leagueId, int seasonId, int? competitionRouteId, int? deleteType)
        {
            try
            {
                leagueRepo.DeleteAllRegistrations(leagueId, seasonId, competitionRouteId, deleteType);
                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }
    }
}
