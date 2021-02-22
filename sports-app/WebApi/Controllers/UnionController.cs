using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AppModel;
using WebApi.Models;
using DataService;
using DataService.LeagueRank;
using System.Configuration;

namespace WebApi.Controllers
{
    [RoutePrefix("api/Union")]
    public class UnionController : BaseLogLigApiController
    {
        /// <summary>
        ///  Returns the union details, requested via HTTP GET Method with the specified id.
        /// </summary>
        /// <param name="id">The id of the union to retrieve it's details.</param>
        /// <returns></returns>
        /// // GET: api/union/{union id}

        private PlayersRepo _playersRepo;
        protected PlayersRepo playersRepo
        {
            get
            {
                if (_playersRepo == null)
                {
                    _playersRepo = new PlayersRepo(db);
                }
                return _playersRepo;
            }
        }

        public HttpResponseMessage Get(int id)
        {
            try
            {
                UnionsRepo unionsRepo = new UnionsRepo();

                Union unionEntity = unionsRepo.GetById(id);

                var doc = unionsRepo.GetTermsDoc(id);

                if (unionEntity != null)
                {
                    var unionViewModel = new UnionViewModel()
                    {
                        Name = unionEntity.Name,
                        Description = unionEntity.Description,
                        IsHandicapped = unionEntity.IsHadicapEnabled,
                        Logo = unionEntity.Logo,
                        PrimaryImage = unionEntity.PrimaryImage,
                        IndexImage = unionEntity.IndexImage,
                        AssociationIndexInfo = unionEntity.IndexAbout,
                        Address = unionEntity.Address,
                        Phone = unionEntity.ContactPhone,
                        Email = unionEntity.Email,
                        TermsFilePath = doc != null ? doc.FileName : null,
                        DocId = doc != null ? doc.DocId : 0,
                        DocFile = doc != null ? doc.DocFile : null
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, unionViewModel);
                }
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Union Not Found");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "Internal Server Error occured while executong request");
            }

        }

        [Route("Activities/{unionId}")]
        public IHttpActionResult GetActivities(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;
            var activities = db.Activities.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsPublished == true).ToList();
            List<ActivitiesViewModel> vm = new List<ActivitiesViewModel>();
            foreach (var activity in activities)
            {
                ActivitiesViewModel item = new ActivitiesViewModel();
                item.ActivityId = activity.ActivityId;
                item.ActivityName = activity.Name;
                item.StartDate = activity.StartDate.HasValue ? activity.StartDate : null;
                item.EndDate = activity.EndDate.HasValue ? activity.EndDate : null;

                vm.Add(item);
            }

            return Ok(vm);
        }
        [Route("TotalFans/{unionId}")]
        public IHttpActionResult GetTotalFans(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;
            var totalfans = (from clubs in db.Clubs
                             from clubteams in db.ClubTeams
                             from teamsfans in db.TeamsFans
                             where clubs.UnionId == unionId && clubs.ClubId == clubteams.ClubId && teamsfans.TeamId == clubteams.TeamId
                             select new
                             {
                                 fans = teamsfans.UserId,
                                 names = teamsfans.User.UserName??""
                             }
                             );
            var totalcount = totalfans.GroupBy(x => x.fans).Count();
            return Ok(totalcount);
        }
        [Route("Clubs/{unionId}")]
        public IHttpActionResult GetClubs(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) : (int?)null;
            var sectionAlias = seasonRepo.GetById(seasonId ?? 0)?.Union?.Section.Alias;
            var clubs = db.Clubs.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false && (sectionAlias != GamesAlias.Tennis || x.IsClubApproved == true)).ToList();

            List<ClubListViewModel> vm = new List<ClubListViewModel>();
            foreach (var club in clubs)
            {
                ClubListViewModel item = new ClubListViewModel();
                item.Id = club.ClubId;
                item.Logo = club.Logo;
                item.Name = club.Name;
                int fans = 0;
                // Cheng Li. Get total Players : 20180808:6. club list: please change count of teams to count of sportsmans. show first sportsmans and then fans
                int players = club.TeamsPlayers.Where(x=>x.SeasonId==seasonId&&x.IsApprovedByManager==true).Select(x=>x.UserId).Distinct().Count();
                if(unionId != 38)
                {
                    players = 0;
                }
                foreach (var clubTeam in club.ClubTeams)
                {
                    fans += clubTeam.Team.TeamsFans.Count;
                    if(unionId != 38)
                    {
                        //if tennis
                        players += clubTeam.Team.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && tp.IsApprovedByManager == true).Select(tp => tp.UserId).Distinct().Count();
                    }
                }
                item.TotalFans = fans;
                item.TotalTeams = club.ClubTeams.Count;
                item.TotalPlayers = players;

                vm.Add(item);
            }
            var result = vm.OrderBy(v => v.Name);
            return Ok(result);
        }
        [HttpGet]
        [Route("Events/{unionId}")]
        public IHttpActionResult GetEvents(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            UsersRepo usersRepo = new UsersRepo();
            LeagueRepo leagueRepo = new LeagueRepo();
            UnionsRepo unionsRepo = new UnionsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;
            //var clubs = db.Clubs.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false && x.IsClubApproved == true).ToList();
             var EventList = unionsRepo.GetById(unionId).Events.Where(t=>t.IsPublished == true).ToList();
            
            var IsUnionViewer = usersRepo.GetTopLevelJob(CurrUserId) == JobRole.Unionviewer;
            List< EventListViewModel > vm = new List<EventListViewModel>();
            foreach ( var eventitem in EventList)
            {
                EventListViewModel item = new EventListViewModel();
                item.Date = eventitem.EventTime.ToString("dd/MM/yyyy HH:mm");
                item.Place = eventitem.Place;
                item.Logo = eventitem.EventImage;
                item.Name = eventitem.Title;
                item.Description = eventitem.EventDescription;
                item.Id = eventitem.EventId;
                vm.Add(item);
            }
            var result = vm.OrderBy(v => v.Name);
            return Ok(result);
        }

        [HttpGet]
        [Route("Event/{eventId}")]
        public IHttpActionResult GetEvent(int eventId,int unionId = 38)
        {
            UnionsRepo unionsRepo = new UnionsRepo();
            var eventItem = unionsRepo.GetById(unionId).Events.Where(x=>x.EventId == eventId).ToList();
            EventListViewModel item = new EventListViewModel();
            if(eventItem == null)
            {
                return Ok(item);
            }
            item.Date = eventItem.FirstOrDefault().EventTime.ToString("dd/MM/yyyy HH:mm");
            item.Place = eventItem.FirstOrDefault().Place;
            item.Logo = eventItem.FirstOrDefault().EventImage;
            item.Name = eventItem.FirstOrDefault().Title;
            item.Description = eventItem.FirstOrDefault().EventDescription;
            return Ok(item);
        }


        [HttpGet]
        [Route("Benefits/{unionId}")]
        public IHttpActionResult GetBenefits(int unionId, int? seasonId = null)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            UsersRepo usersRepo = new UsersRepo();
            if (!seasonId.HasValue) {
                seasonId = seasonRepo.GetLastSeasonByCurrentUnionId(unionId);
            }
            BenefitsRepo benefitsRepo = new BenefitsRepo();
            benefitsRepo.GetBenefits(unionId, seasonId.Value);
            //var clubs = db.Clubs.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false && x.IsClubApproved == true).ToList();
            var benefitList = benefitsRepo.GetPublishedBenefits(unionId, seasonId.Value);

            var IsUnionViewer = usersRepo.GetTopLevelJob(CurrUserId) == JobRole.Unionviewer;
            List<BenefitListViewModel> vm = new List<BenefitListViewModel>();
            foreach (var benefitItem in benefitList)
            {
                BenefitListViewModel item = new BenefitListViewModel();
                item.Company = benefitItem.Company;
                item.Logo = benefitItem.Image;
                item.Name = benefitItem.Title;
                item.Description = benefitItem.Description;
                item.Id = benefitItem.BenefitId;
                item.Code = benefitItem.Code;
                vm.Add(item);
            }
            var result = vm.OrderBy(v => v.Name);
            return Ok(result);
        }


        [HttpGet]
        [Route("Benefit/{benefitId}")]
        public IHttpActionResult GetBenefit(int benefitId)
        {
            BenefitsRepo benefitsRepo = new BenefitsRepo();

            var benefitItem = benefitsRepo.GetByIdIfPublished(benefitId);
            BenefitListViewModel item = new BenefitListViewModel();
            if (benefitItem == null)
            {
                return Ok(item);
            }
            item.Company = benefitItem.Company;
            item.Logo = benefitItem.Image;
            item.Code = benefitItem.Code;
            item.Name = benefitItem.Title;
            item.Description = benefitItem.Description;
            item.SeasonId = benefitItem.SeasonId;
            return Ok(item);
        }





        [HttpGet]
        [Route("ClubsArea1/{unionId}")]
        public IHttpActionResult GetClubsArea1(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :(int?)null;
            var south_clubs = db.Clubs.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false && x.IsClubApproved == true).ToList();
            var clubs = south_clubs.Where(t => t.ClubId == 3074 || t.ClubId == 3223 || t.ClubId == 3209 || t.ClubId == 3115 || t.ClubId == 3091 || t.ClubId == 3087 || t.ClubId == 3089 ||
            t.ClubId == 3047 || t.ClubId == 3053 || t.ClubId == 3202 || t.ClubId == 3060).ToList();
            List<ClubListViewModel> vm = new List<ClubListViewModel>();
            foreach (var club in clubs)
            {
                ClubListViewModel item = new ClubListViewModel();
                item.Id = club.ClubId;
                item.Logo = club.Logo;
                item.Name = club.Name;
                int fans = 0;
                // Cheng Li. Get total Players : 20180808:6. club list: please change count of teams to count of sportsmans. show first sportsmans and then fans
                int players = club.TeamsPlayers.Where(x => x.SeasonId == seasonId && x.IsApprovedByManager == true).Select(x => x.UserId).Distinct().Count();
                if (unionId != 38)
                {
                    players = 0;
                }
                foreach (var clubTeam in club.ClubTeams)
                {
                    fans += clubTeam.Team.TeamsFans.Count;
                    if (unionId != 38)
                    {
                        //if tennis
                        players += clubTeam.Team.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && tp.IsApprovedByManager == true).Select(tp => tp.UserId).Distinct().Count();
                    }
                }
                item.TotalFans = fans;
                item.TotalTeams = club.ClubTeams.Count;
                item.TotalPlayers = players;

                vm.Add(item);
            }
            var result = vm.OrderBy(v => v.Name);
            return Ok(result);
        }
        [HttpGet]
        [Route("ClubsArea2/{unionId}")]
        public IHttpActionResult GetClubsArea2(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;
            var north_clubs = db.Clubs.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false && x.IsClubApproved == true).ToList();
            var clubs = north_clubs.Where(t => t.ClubId == 3050 || t.ClubId == 3082 || t.ClubId == 3063 || t.ClubId == 3123 || t.ClubId == 3059 || t.ClubId == 3157 || t.ClubId == 3069 ||
            t.ClubId == 3101 || t.ClubId == 3133 || t.ClubId == 3103 || t.ClubId == 3152 || t.ClubId == 3036 || t.ClubId == 3076 || t.ClubId == 3255 || t.ClubId == 3163
            || t.ClubId == 3095 || t.ClubId == 3084).ToList();

            List<ClubListViewModel> vm = new List<ClubListViewModel>();
            foreach (var club in clubs)
            {
                ClubListViewModel item = new ClubListViewModel();
                item.Id = club.ClubId;
                item.Logo = club.Logo;
                item.Name = club.Name;
                int fans = 0;
                // Cheng Li. Get total Players : 20180808:6. club list: please change count of teams to count of sportsmans. show first sportsmans and then fans
                int players = club.TeamsPlayers.Where(x => x.SeasonId == seasonId && x.IsApprovedByManager == true).Select(x => x.UserId).Distinct().Count();
                if (unionId != 38)
                {
                    players = 0;
                }
                foreach (var clubTeam in club.ClubTeams)
                {
                    fans += clubTeam.Team.TeamsFans.Count;
                    if (unionId != 38)
                    {
                        //if tennis
                        players += clubTeam.Team.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && tp.IsApprovedByManager == true).Select(tp => tp.UserId).Distinct().Count();
                    }
                }
                item.TotalFans = fans;
                item.TotalTeams = club.ClubTeams.Count;
                item.TotalPlayers = players;

                vm.Add(item);
            }
            var result = vm.OrderBy(v => v.Name);
            return Ok(result);
        }
        [HttpGet]
        [Route("ClubsArea3/{unionId}")]
        public IHttpActionResult GetClubsArea3(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;
            var Center_clubs = db.Clubs.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false && x.IsClubApproved == true).ToList();
            var clubs = Center_clubs.Where(t => t.ClubId == 3052 || t.ClubId == 3180 || t.ClubId == 3056 || t.ClubId == 3072 || t.ClubId == 3128 || t.ClubId == 3040 || t.ClubId == 3071 ||
            t.ClubId == 3067 || t.ClubId == 3077 || t.ClubId == 3054 || t.ClubId == 3333 || t.ClubId == 3623 || t.ClubId == 3090 || t.ClubId == 3189 || t.ClubId == 3149
            || t.ClubId == 3097 || t.ClubId == 3075).ToList();

            List<ClubListViewModel> vm = new List<ClubListViewModel>();
            foreach (var club in clubs)
            {
                ClubListViewModel item = new ClubListViewModel();
                item.Id = club.ClubId;
                item.Logo = club.Logo;
                item.Name = club.Name;
                int fans = 0;
                // Cheng Li. Get total Players : 20180808:6. club list: please change count of teams to count of sportsmans. show first sportsmans and then fans
                int players = club.TeamsPlayers.Where(x => x.SeasonId == seasonId && x.IsApprovedByManager == true).Select(x => x.UserId).Distinct().Count();
                if (unionId != 38)
                {
                    players = 0;
                }
                foreach (var clubTeam in club.ClubTeams)
                {
                    fans += clubTeam.Team.TeamsFans.Count;
                    if (unionId != 38)
                    {
                        //if tennis
                        players += clubTeam.Team.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && tp.IsApprovedByManager == true).Select(tp => tp.UserId).Distinct().Count();
                    }
                }
                item.TotalFans = fans;
                item.TotalTeams = club.ClubTeams.Count;
                item.TotalPlayers = players;

                vm.Add(item);
            }
            var result = vm.OrderBy(v => v.Name);
            return Ok(result);
        }
        [Route("ClubsArea4/{unionId}")]
        [HttpGet]
        public IHttpActionResult GetClubsArea4(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;
            var Jerusalem_clubs = db.Clubs.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false && x.IsClubApproved == true).ToList();
            var clubs = Jerusalem_clubs.Where(t => t.ClubId == 3210 || t.ClubId == 3058 || t.ClubId == 3043 || t.ClubId == 3111).ToList();

            List<ClubListViewModel> vm = new List<ClubListViewModel>();
            foreach (var club in clubs)
            {
                ClubListViewModel item = new ClubListViewModel();
                item.Id = club.ClubId;
                item.Logo = club.Logo;
                item.Name = club.Name;
                int fans = 0;
                // Cheng Li. Get total Players : 20180808:6. club list: please change count of teams to count of sportsmans. show first sportsmans and then fans
                int players = club.TeamsPlayers.Where(x => x.SeasonId == seasonId && x.IsApprovedByManager == true).Select(x => x.UserId).Distinct().Count();
                if (unionId != 38)
                {
                    players = 0;
                }
                foreach (var clubTeam in club.ClubTeams)
                {
                    fans += clubTeam.Team.TeamsFans.Count;
                    if (unionId != 38)
                    {
                        //if tennis
                        players += clubTeam.Team.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && tp.IsApprovedByManager == true).Select(tp => tp.UserId).Distinct().Count();
                    }
                }
                item.TotalFans = fans;
                item.TotalTeams = club.ClubTeams.Count;
                item.TotalPlayers = players;

                vm.Add(item);
            }
            var result = vm.OrderBy(v => v.Name);
            return Ok(result);
        }
        [HttpGet]
        [Route("ClubsArea5/{unionId}")]
        public IHttpActionResult GetClubsArea5(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;
            var Sharon_clubs = db.Clubs.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false && x.IsClubApproved == true).ToList();
            var clubs = Sharon_clubs.Where(t => t.ClubId == 3083 || t.ClubId == 3172 || t.ClubId == 3177 || t.ClubId == 3035 || t.ClubId == 3085 || t.ClubId == 3042 || t.ClubId == 3126
            || t.ClubId == 3185 || t.ClubId == 3037 || t.ClubId == 3061 || t.ClubId == 3045 || t.ClubId == 3068 || t.ClubId == 3178 || t.ClubId == 3212 || t.ClubId == 3057 || t.ClubId == 3039).ToList();

            List<ClubListViewModel> vm = new List<ClubListViewModel>();
            foreach (var club in clubs)
            {
                ClubListViewModel item = new ClubListViewModel();
                item.Id = club.ClubId;
                item.Logo = club.Logo;
                item.Name = club.Name;
                int fans = 0;
                // Cheng Li. Get total Players : 20180808:6. club list: please change count of teams to count of sportsmans. show first sportsmans and then fans
                int players = club.TeamsPlayers.Where(x => x.SeasonId == seasonId && x.IsApprovedByManager == true).Select(x => x.UserId).Distinct().Count();
                if (unionId != 38)
                {
                    players = 0;
                }
                foreach (var clubTeam in club.ClubTeams)
                {
                    fans += clubTeam.Team.TeamsFans.Count;
                    if (unionId != 38)
                    {
                        //if tennis
                        players += clubTeam.Team.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && tp.IsApprovedByManager == true).Select(tp => tp.UserId).Distinct().Count();
                    }
                }
                item.TotalFans = fans;
                item.TotalTeams = club.ClubTeams.Count;
                item.TotalPlayers = players;

                vm.Add(item);
            }
            var result = vm.OrderBy(v => v.Name);
            return Ok(result);
        }

        [Route("LeaguesList/{unionId}")]
        public IHttpActionResult GetUnionLeagues(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;

            int tmp = seasonRepo.GetLastSeasonByCurrentUnionId(unionId);

            var allLeagues = new List<League>();

            // Check Job Type?
            if (unionId == 37)
            {
                allLeagues = db.Leagues.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false && x.Type == 0).Where(x => x.EilatTournament != null || x.EilatTournament == true).ToList();
            }
            else if (unionId == 38)
            {
                //tennis
                allLeagues = db.Leagues.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false).Where(x => x.EilatTournament == null || x.EilatTournament == false).ToList();
                var resultsub = allLeagues.OrderByDescending(l => l.LeagueStartDate)
                .Select(l => new LeaguesListItemViewModel
                {
                    Id = l.LeagueId,
                    Title = l.Name,
                    TotalTeams = unionId == 38 ? l.TeamRegistrations.Count(c => !c.IsDeleted) : l.LeagueTeams.Count(),
                    TotalFans = l.LeagueTeams.Join(db.TeamsFans, tf => tf.TeamId, lt => lt.TeamId, (lt, tf) => tf.UserId).Distinct().Count(),
                    TotalPlayers = GetLeaguePlayersCount(l, unionId, seasonId, true),
                    Logo = l.Logo
                }).OrderBy(x => x.Id).ToList();

                return Ok(resultsub);
            }
            else
            {
                allLeagues = db.Leagues.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false).Where(x => x.EilatTournament == null || x.EilatTournament == false).ToList();
            }

            var result = allLeagues.OrderByDescending(l => l.LeagueStartDate)
                .Select(l => new LeaguesListItemViewModel
                {
                    Id = l.LeagueId,
                    Title = l.Name,
                    TotalTeams = unionId == 38 ? l.TeamRegistrations.Count(c => !c.IsDeleted) : l.LeagueTeams.Count(),
                    TotalFans = l.LeagueTeams.Join(db.TeamsFans, tf => tf.TeamId, lt => lt.TeamId, (lt, tf) => tf.UserId).Distinct().Count(),
                    TotalPlayers = GetLeaguePlayersCount(l, unionId, seasonId, true),
                    Logo = l.Logo
                }).OrderBy(x=>x.Id).ToList();

            return Ok(result);
        }

        [Route("Disciplines/{unionId}")]
        public IHttpActionResult GetUnionDisiplines(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;

            var allDisciplines = new List<Discipline>();
            allDisciplines = db.Disciplines.Where(x => x.UnionId == unionId && x.IsArchive == false).ToList();

            var result = allDisciplines
                .Select(l => new DisciplinesListItemViewModel
                {
                    Id = l.DisciplineId,
                    Title = l.Name
                }).ToList();

            return Ok(result);
        }

        [Route("Competitions/{unionId}/Disciplines/{disciplineId}")]
        public IHttpActionResult GetDisciplineCompetitions(int unionId, int disciplineId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;

            var allCompetitions = new List<League>();

            if (disciplineId != 0)
            {
                allCompetitions = db.Leagues.Where(x => x.DisciplineId == disciplineId && x.UnionId == unionId && x.IsArchive == false && x.SeasonId == seasonId).OrderByDescending(x => x.LeagueStartDate).ToList();
            } else
            {
                allCompetitions = db.Leagues.Where(x => x.UnionId == unionId && x.IsArchive == false && x.DisciplineId != null && x.SeasonId == seasonId).OrderByDescending(x => x.LeagueStartDate).ToList();
            }

            LeagueRepo leagueRepo = new LeagueRepo();

            var result = allCompetitions
                .Select(l => new LeaguesListItemViewModel
                {
                    Id = l.LeagueId,
                    Title = l.Name,
                    TotalFans = l.LeagueTeams.Join(db.TeamsFans, tf => tf.TeamId, lt => lt.TeamId, (lt, tf) => tf.UserId).Distinct().Count(),
                    TotalPlayers = l.CompetitionRegistrations.Count(),
                    Logo = l.Logo,
                    DisciplineName = l.Discipline.Name
                }).ToList();

            return Ok(result);
        }

        [Route("CompetitionAges/{unionId}")]
        public IHttpActionResult GetUnionCompetitionAges(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;
            List<CompetitionAgeModel> listAges = db.CompetitionAges.Where(x => x.UnionId == unionId && x.SeasonId == seasonId).Select(ca => new CompetitionAgeModel
            {
                id = ca.id,
                age_name = ca.age_name
            }).ToList();
            return Ok(listAges);
        }

        [Route("UnionRankings/{unionId}/ageId/{ageId}")]
        public IHttpActionResult GetUnionRankings(int unionId, int ageId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;
            CategoryRankService svc = new CategoryRankService(0);
            TeamsRepo teamsRepo = new TeamsRepo();

            var playersInfo = teamsRepo.GetTennisUnionRanks(unionId, ageId, null, seasonId.Value);

            return Ok(playersInfo);
        }

        [Route("Competitions/{unionId}")]
        public IHttpActionResult GetUnionCompetitions(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            int? seasonId = unionId != null ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;

            List<League> allCompetitions = new List<League>();

            // Check Job Type?
            if (unionId == 37)
            {
                allCompetitions = db.Leagues.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false && x.Type == 1).Where(x => x.EilatTournament != null && x.EilatTournament == true).OrderBy(x => x.SortOrder).ToList();
            }
            else
            {
                allCompetitions = db.Leagues.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false).Where(x => x.EilatTournament != null && x.EilatTournament == true).OrderBy(x => x.SortOrder).ToList();
            }

            var result = new List<LeaguesListItemViewModel>();
            if (unionId == 38) { // for tennis competitions
                LeagueRepo leagueRepo = new LeagueRepo();
                foreach ( League competition in allCompetitions) {
                    IEnumerable<LeagueTeams> leagueTeams = leagueRepo.GetLeagueTeam(competition.LeagueId, competition.SeasonId != null ? (int)competition.SeasonId : -1);
                    foreach(LeagueTeams leagueTeam in leagueTeams)
                    {
                        LeaguesListItemViewModel vm = new LeaguesListItemViewModel();
                        vm.Id = competition.LeagueId;
                        vm.TeamId = leagueTeam.TeamId;
                        int? competitionAgeId = db.Teams.FirstOrDefault(t => t.TeamId == leagueTeam.TeamId).CompetitionAgeId;
                        vm.Title = competition.Name;
                        if(competitionAgeId != null)
                        {
                            //String age_name = db.CompetitionAges.Where(x => x.id == competitionAgeId).FirstOrDefault().age_name;
                            //vm.Title += (" - " + age_name);
                            var teamDetails = leagueTeam.Teams.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId.Value);
                            if (teamDetails != null)
                            {
                                vm.Title += (" - " + teamDetails.TeamName);
                            }
                        }
                        vm.TotalFans = db.TeamsFans.Where(tf => tf.TeamId == leagueTeam.TeamId).Count();
                        vm.TotalPlayers = db.TeamsPlayers.Where(tp => tp.TeamId == leagueTeam.TeamId && tp.SeasonId == seasonId).Count();
                        vm.Logo = competition.Logo;
                        result.Add(vm);
                    }
                }
            }
            else {
                result = allCompetitions
                    .Select(l => new LeaguesListItemViewModel
                    {
                        Id = l.LeagueId,
                        Title = l.Name,
                        TotalFans = l.LeagueTeams.Join(db.TeamsFans, tf => tf.TeamId, lt => lt.TeamId, (lt, tf) => tf.UserId).Distinct().Count(),
                        TotalPlayers = GetLeaguePlayersCount(l, unionId, seasonId, false),
                        Logo = l.Logo
                    }).ToList();
            }
            return Ok(result);
        }
        private Dictionary<int, int> GetLeaguePlayersCountDictionary(List<League> resList, string sectionAlias, int unionId, int seasonId)
        {
            var result = new Dictionary<int, int>();
            if (resList != null && resList.Any())
            {
                if (sectionAlias.Equals(GamesAlias.MartialArts))
                {
                    foreach (var league in resList)
                    {
                        var leaguePlayersCount = league.SportsRegistrations.Count;
                        result.Add(league.LeagueId, leaguePlayersCount);
                    }
                }
                else
                {
                    var players = playersRepo.GetTeamPlayersByUnionIdShort(unionId, seasonId);
                    foreach (var league in resList)
                    {
                        var leaguePlayersCount = players.Count(c =>
                            c.Team.LeagueTeams.Any(lt => lt.LeagueId == league.LeagueId && lt.SeasonId == seasonId) &&
                            c.LeagueId == league.LeagueId &&
                            c.SeasonId == seasonId);
                        result.Add(league.LeagueId, leaguePlayersCount);
                    }
                }
            }
            return result;
        }
        [Route("CompetitionsYouth/{unionId}")]
        public IHttpActionResult GetUnionCompetitionsYouth(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            UsersRepo usersRepo = new UsersRepo();
            LeagueRepo leagueRepo = new LeagueRepo();
            TeamsRepo teamRepo = new TeamsRepo();
            int? seasonId = unionId != 0 ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;

            List<League> allCompetitions = new List<League>();

            //allCompetitions = db.Leagues.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false && x.IsDailyCompetition == false).Where(x => x.EilatTournament != null && x.EilatTournament == true).OrderBy(x => x.SortOrder).ToList();
            if (unionId == 38)
            {
                var isPlayer = false;
                isPlayer = db.TeamsPlayers.Where(t => t.UserId == CurrentUser.UserId && t.SeasonId == seasonId).ToList().Count > 0;
                if (isPlayer)
                {
                    allCompetitions = leagueRepo.GetByUnion(unionId, seasonId ?? 0)
                        .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                        .ToList();
                }
                else if (CurrentUser.UsersType.TypeRole == (AppRole.Workers))
                {
                    switch (usersRepo.GetTopLevelJob(CurrentUser.UserId))
                    {
                        case JobRole.UnionManager:
                        case JobRole.Unionviewer:
                        case JobRole.RefereeAssignment:
                            allCompetitions = leagueRepo.GetByUnion(unionId, seasonId ?? 0)
                                .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                                .ToList();
                            break;
                        case JobRole.LeagueManager:
                            allCompetitions = leagueRepo.GetByManagerId(CurrentUser.UserId, seasonId)
                                .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                                .ToList();
                            break;
                        case JobRole.TeamManager:
                            break;
                    }
                }
                else
                {
                    allCompetitions = leagueRepo.GetByUnion(unionId, seasonId ?? 0)
                        .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                        .ToList();
                }
            }
            if (allCompetitions != null)
            {
                allCompetitions = allCompetitions.Where(t => t.IsDailyCompetition == false && (t.IsSeniorCompetition ?? false) == false).OrderBy(c => c.SortOrder).ToList();
            }
            var result = new List<CompetitionItemViewModel>();
            var LeaguePlayersCount = GetLeaguePlayersCountDictionary(allCompetitions, "Tennis", unionId, seasonId ?? 0);
            var leaguesStats = LeaguePlayersCount != null ? LeaguePlayersCount as Dictionary<int, int> : new Dictionary<int, int>();
            if (unionId == 38)
            { // for tennis competitions

                var teams = db.Teams.ToList();
                var teamPlayers = db.TeamsPlayers.Where(tp => tp.SeasonId == seasonId).ToList();
                var teamFans = db.TeamsFans.ToList();
                var competitionIds = allCompetitions.Select(l => l.LeagueId).ToArray();
                var allTeamsList = teamRepo.GetAllTeams(seasonId ?? 0, competitionIds);
                foreach (League competition in allCompetitions)
                {
                    CompetitionItemViewModel item = new CompetitionItemViewModel();
                    item.CompetitionTitle = competition.Name;
                    item.Logo = competition.Logo;
                    item.categoriPlayersNum = (leaguesStats.ContainsKey(competition.LeagueId) ? leaguesStats[competition.LeagueId] : 0);
                    item.categoriNum = competition.LeagueTeams.Count;
                    var TeamsList = allTeamsList.Where(t => t.RetrievedLeagueId == competition.LeagueId).ToList();
                    TeamsList = TeamsList.OrderBy(x => x.Title).ToList();
                    item.categoriLIstItem = new List<LeaguesListItemViewModel>();
                    foreach (var leagueTeam in TeamsList)
                    {
                        LeaguesListItemViewModel vm = new LeaguesListItemViewModel();
                        vm.Id = competition.LeagueId;
                        vm.TeamId = leagueTeam.TeamId;
                        int? competitionAgeId = teams.FirstOrDefault(t => t.TeamId == leagueTeam.TeamId).CompetitionAgeId;
                        vm.Title = competition.Name;
                        if (competitionAgeId != null)
                        {
                            vm.Title += (" - " + leagueTeam.Title);
                        }
                        vm.TotalFans = teamFans.Where(tf => tf.TeamId == leagueTeam.TeamId).Count();
                        vm.TotalPlayers = teamPlayers.Where(tp => tp.TeamId == leagueTeam.TeamId).Count();
                        vm.Logo = competition.Logo;
                        item.categoriLIstItem.Add(vm);
                    }
                    result.Add(item);
                }
            }
            return Ok(result);
        }

        [Route("CompetitionsDaily/{unionId}")]
        public IHttpActionResult GetUnionCompetitionsDaily(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            UsersRepo usersRepo = new UsersRepo();
            LeagueRepo leagueRepo = new LeagueRepo();
            TeamsRepo teamRepo = new TeamsRepo();
            int? seasonId = unionId != 0 ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;
            List<League> allCompetitions = new List<League>();
            if (unionId == 38)
            {
                var isPlayer = false;
                isPlayer = db.TeamsPlayers.Where(t => t.UserId == CurrentUser.UserId && t.SeasonId == seasonId).ToList().Count > 0;
                if (isPlayer)
                {
                    allCompetitions = leagueRepo.GetByUnion(unionId, seasonId ?? 0)
                        .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                        .ToList();
                }
                if (CurrentUser.UsersType.TypeRole == (AppRole.Workers))
                {
                    switch (usersRepo.GetTopLevelJob(CurrentUser.UserId))
                    {
                        case JobRole.UnionManager:
                        case JobRole.Unionviewer:
                        case JobRole.RefereeAssignment:
                            allCompetitions = leagueRepo.GetByUnion(unionId, seasonId ?? 0)
                                .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                                .ToList();
                            break;
                        case JobRole.LeagueManager:
                            allCompetitions = leagueRepo.GetByManagerId(CurrentUser.UserId, seasonId)
                                .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                                .ToList();
                            break;
                        case JobRole.TeamManager:
                            break;
                    }
                }
                else
                {
                    allCompetitions = leagueRepo.GetByUnion(unionId, seasonId ?? 0)
                        .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                        .ToList();
                }
            }
            if (allCompetitions != null)
            {
                allCompetitions = allCompetitions.Where(t=>t.IsDailyCompetition==true&&(t.IsSeniorCompetition??false)==false).OrderBy(c => c.SortOrder).ToList();
            }
            var result = new List<CompetitionItemViewModel>();
            var LeaguePlayersCount = GetLeaguePlayersCountDictionary(allCompetitions, "Tennis", unionId, seasonId ?? 0);
            var leaguesStats = LeaguePlayersCount != null ? LeaguePlayersCount as Dictionary<int, int> : new Dictionary<int, int>();
            if (unionId == 38)
            { // for tennis competitions
                var teams = db.Teams.ToList();
                var teamPlayers = db.TeamsPlayers.Where(tp => tp.SeasonId == seasonId).ToList();
                var teamFans = db.TeamsFans.ToList();
                var competitionIds = allCompetitions.Select(l => l.LeagueId).ToArray();
                var allTeamsList = teamRepo.GetAllTeams(seasonId ?? 0, competitionIds);
                foreach (League competition in allCompetitions)
                {
                    CompetitionItemViewModel item = new CompetitionItemViewModel();
                    item.CompetitionTitle = competition.Name;
                    item.Logo = competition.Logo;
                    item.categoriPlayersNum = (leaguesStats.ContainsKey(competition.LeagueId) ? leaguesStats[competition.LeagueId] : 0);
                    item.categoriNum = competition.LeagueTeams.Count;
                    var TeamsList = allTeamsList.Where(t => t.RetrievedLeagueId == competition.LeagueId).ToList();
                    TeamsList = TeamsList.OrderBy(x => x.Title).ToList();
                    item.categoriLIstItem = new List<LeaguesListItemViewModel>();
                    foreach (var leagueTeam in TeamsList)
                    {
                        LeaguesListItemViewModel vm = new LeaguesListItemViewModel();
                        vm.Id = competition.LeagueId;
                        vm.TeamId = leagueTeam.TeamId;
                        int? competitionAgeId = teams.FirstOrDefault(t => t.TeamId == leagueTeam.TeamId).CompetitionAgeId;
                        vm.Title = competition.Name;
                        if (competitionAgeId != null)
                        {
                            vm.Title += (" - " + leagueTeam.Title);
                        }
                        vm.TotalFans = teamFans.Where(tf => tf.TeamId == leagueTeam.TeamId).Count();
                        vm.TotalPlayers = teamPlayers.Where(tp => tp.TeamId == leagueTeam.TeamId && tp.SeasonId == seasonId).Count();
                        vm.Logo = competition.Logo;
                        item.categoriLIstItem.Add(vm);
                    }
                    result.Add(item);
                }
            }
            return Ok(result);
        }

        [Route("CompetitionsSenior/{unionId}")]
        public IHttpActionResult GetUnionCompetitionsSenior(int unionId)
        {
            SeasonsRepo seasonRepo = new SeasonsRepo();
            UsersRepo usersRepo = new UsersRepo();
            LeagueRepo leagueRepo = new LeagueRepo();
            TeamsRepo teamRepo = new TeamsRepo();
            int? seasonId = unionId != 0 ? seasonRepo.GetLastSeasonByCurrentUnionId(unionId) :
                                              (int?)null;

            List<League> allCompetitions = new List<League>();
            if (unionId == 38)
            {
                var isPlayer = false;
                isPlayer = db.TeamsPlayers.Where(t => t.UserId == CurrentUser.UserId && t.SeasonId == seasonId).ToList().Count > 0;
                if (isPlayer)
                {
                    allCompetitions = leagueRepo.GetByUnion(unionId, seasonId ?? 0)
                        .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                        .ToList();
                }
                if (CurrentUser.UsersType.TypeRole == (AppRole.Workers))
                {
                    switch (usersRepo.GetTopLevelJob(CurrentUser.UserId))
                    {
                        case JobRole.UnionManager:
                        case JobRole.Unionviewer:
                        case JobRole.RefereeAssignment:
                            allCompetitions = leagueRepo.GetByUnion(unionId, seasonId ?? 0)
                                .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                                .ToList();
                            break;
                        case JobRole.LeagueManager:
                            allCompetitions = leagueRepo.GetByManagerId(CurrentUser.UserId, seasonId)
                                .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                                .ToList();
                            break;
                        case JobRole.TeamManager:
                            break;
                    }
                }
                else
                {
                    allCompetitions = leagueRepo.GetByUnion(unionId, seasonId ?? 0)
                        .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                        .ToList();
                }
            }
            if (allCompetitions != null)
            {
                allCompetitions = allCompetitions.Where(t=>t.IsSeniorCompetition??false==true).OrderBy(c => c.SortOrder).ToList();
            }
            var result = new List<CompetitionItemViewModel>();
            var LeaguePlayersCount = GetLeaguePlayersCountDictionary(allCompetitions, "Tennis", unionId, seasonId ?? 0);
            var leaguesStats = LeaguePlayersCount != null ? LeaguePlayersCount as Dictionary<int, int> : new Dictionary<int, int>();
            if (unionId == 38)
            { // for tennis competitions
                var teams = db.Teams.ToList();
                var teamPlayers = db.TeamsPlayers.Where(tp => tp.SeasonId == seasonId).ToList();
                var teamFans = db.TeamsFans.ToList();
                var competitionIds = allCompetitions.Select(l => l.LeagueId).ToArray();
                var allTeamsList = teamRepo.GetAllTeams(seasonId ?? 0, competitionIds);
                foreach (League competition in allCompetitions)
                {
                    CompetitionItemViewModel item = new CompetitionItemViewModel();
                    item.CompetitionTitle = competition.Name;
                    item.Logo = competition.Logo;
                    item.categoriPlayersNum = (leaguesStats.ContainsKey(competition.LeagueId) ? leaguesStats[competition.LeagueId] : 0);
                    item.categoriNum = competition.LeagueTeams.Count;
                    //var TeamsList = teamRepo.GetRegisteredTeamsByLeagueId(competition.LeagueId, competition.SeasonId??0);
                    var TeamsList = allTeamsList.Where(t => t.RetrievedLeagueId == competition.LeagueId).ToList();
                    TeamsList = TeamsList.OrderBy(x => x.Title).ToList();
                    item.categoriLIstItem = new List<LeaguesListItemViewModel>();
                    //IEnumerable<LeagueTeams> leagueTeams = leagueRepo.GetLeagueTeam(competition.LeagueId, competition.SeasonId != null ? (int)competition.SeasonId : -1);
                    foreach (var leagueTeam in TeamsList)
                    {
                        LeaguesListItemViewModel vm = new LeaguesListItemViewModel();
                        vm.Id = competition.LeagueId;
                        vm.TeamId = leagueTeam.TeamId;
                        int? competitionAgeId = teams.FirstOrDefault(t => t.TeamId == leagueTeam.TeamId).CompetitionAgeId;
                        vm.Title = competition.Name;
                        if (competitionAgeId != null)
                        {
                            vm.Title += (" - " + leagueTeam.Title);
                        }
                        vm.TotalFans = teamFans.Where(tf => tf.TeamId == leagueTeam.TeamId).Count();
                        vm.TotalPlayers = teamPlayers.Where(tp => tp.TeamId == leagueTeam.TeamId && tp.SeasonId == seasonId).Count();
                        vm.Logo = competition.Logo;
                        item.categoriLIstItem.Add(vm);
                    }
                    result.Add(item);
                }
            }
            return Ok(result);
        }


        /// <summary>
        ///  Returns the list of Eilat Tournament PDFs details, requested via HTTP GET Method
        /// </summary>
        /// <param name="id">The id of the union to retrieve it's details.</param>
        /// <returns>["file url 1 or null", "file url 2 or null", "file url 3 or null", "file url 4 or null"]</returns>
        /// // GET: api/union/leagues/{union id}
        [Route("Leagues/{id}")]
        public HttpResponseMessage GetLeagues(int id)
        {
            var routeToPDF = ConfigurationManager.AppSettings["PdfRoute"];
            var urlToPDF = ConfigurationManager.AppSettings["PdfUrl"];

            string[] pdfArr = new string[] { $"{routeToPDF}PDF1.pdf", $"{routeToPDF}PDF2.pdf", $"{routeToPDF}PDF3.pdf", $"{routeToPDF}PDF4.pdf" };
            for (int i = 0; i < pdfArr.Length; i++)
            {
                if (System.IO.File.Exists(pdfArr[i]))
                {
                    pdfArr[i] = $"{urlToPDF}PDF{i + 1}.pdf";
                }
                else
                {
                    pdfArr[i] = null;
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, pdfArr);
        }

        private int GetLeaguePlayersCount(League league, int unionId, int? seasonId, bool bLeague)
        {
            if (seasonId == null) return 0;

            if ( unionId == 37) // Karate
                return league.SportsRegistrations.Count();
            if( !bLeague )
            {
                if (unionId == 36) // Gymnastics
                    return league.CompetitionRegistrations.Count();

                var players = playersRepo.GetTeamPlayersByUnionIdShort(unionId, (int)seasonId);

                var leaguePlayersCount = players.Count(c =>
                            c.Team.LeagueTeams.Any(lt => lt.LeagueId == league.LeagueId && lt.SeasonId == seasonId) &&
                            c.LeagueId == league.LeagueId &&
                            c.SeasonId == seasonId);
                return leaguePlayersCount;
            }
            else
            {
                if (unionId == 38)
                    return league.TeamRegistrations.Where(r => !r.IsDeleted).Sum(r => r.Team.TeamsPlayers.Count(tp => tp.IsActive && !tp.User.IsArchive));

                return league.LeagueTeams.Sum(x => x.Teams.TeamsPlayers.Count(tp => tp.LeagueId == league.LeagueId));
            }
        }
    }
}
