using System;
using CmsApp.Helpers;
using DataService;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppModel;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using Resources;

namespace CmsApp.Controllers
{
    [AppAuthorize]
    public class AdminController : Controller
    {
        private const string UNION_CURRENT_SEASON_ID = "UnionCurrentSeasonId";
        private const string CURRENT_UNION_ID = "CurrentUnionId";
        private const string CLUB_CURRENT_SEASON_ID = "ClubCurrentSeasonId";
        private const string CURRENT_CLUB_ID = "CurrentClubId";
        private const string CURRENT_SECTION = "CurrentSection";
        private const string IS_SECTION_CLUB_LEVEL = "IsSectionClubLevel";

        protected AuthorizationEntitiesService AuthSvc;

        protected DataEntities db;

        public AdminController()
        {
            db = new DataEntities();
            AuthSvc = new AuthorizationEntitiesService();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }

        #region Repositories
        private ActivityRepo _activityRepo;
        private UsersRepo _usersRepo;
        private SeasonsRepo _seasonsRepository;
        private GamesRepo _gamesRepo;
        private LeagueRepo _leagueRepo;
        private ClubsRepo _clubsRepo;
        private EventsRepo _eventsRepo;
        private BenefitsRepo _benefitsRepo;
        private TeamsRepo _teamRepo;
        private PlayersRepo _playersRepo;
        private PositionsRepo _posRepo;
        private UnionsRepo _unionsRepo;
        private AuditoriumsRepo _auditoriumsRepo;
        private SectionsRepo _secRepo;
        private JobsRepo _jobsRepo;
        private GroupsRepo _groupRepo;
        private StagesRepo _stagesRepo;
        private AgesRepo _agesRepo;
        private SportsRepo _sportsRepo;
        private SportRanksRepo _sportRanksRepo;
        private PlayerAchievementsRepo _playerAchievementsRepo;
        private ActivityFormsRepo _activityFormsRepo;
        private DisciplinesRepo _disciplinesRepo;
        private SchoolRepo _schoolRepo;
        private RegionalsRepo _regionalsRepo;
        private FriendshipTypesRepo _friendshipTypesRepo;
        private PricesRepo _pricesRepo;

        protected SportsRepo SportsRepo => _sportsRepo ?? (_sportsRepo = new SportsRepo(db));
        protected SportRanksRepo SportRanksRepo => _sportRanksRepo ?? (_sportRanksRepo = new SportRanksRepo(db));
        protected PlayerAchievementsRepo PlayerAchievementsRepo => _playerAchievementsRepo ?? (_playerAchievementsRepo = new PlayerAchievementsRepo(db));
        protected ActivityFormsRepo ActivityFormsRepo => _activityFormsRepo ?? (_activityFormsRepo = new ActivityFormsRepo(db));
        protected SchoolRepo SchoolRepo => _schoolRepo ?? (_schoolRepo = new SchoolRepo(db));
        protected ActivityRepo activityRepo => _activityRepo ?? (_activityRepo = new ActivityRepo(db));
        protected DisciplinesRepo disciplinesRepo => _disciplinesRepo ?? (_disciplinesRepo = new DisciplinesRepo(db));
        protected UsersRepo usersRepo => _usersRepo ?? (_usersRepo = new UsersRepo(db));
        protected GamesRepo gamesRepo => _gamesRepo ?? (_gamesRepo = new GamesRepo(db));
        protected LeagueRepo leagueRepo => _leagueRepo ?? (_leagueRepo = new LeagueRepo(db));
        protected ClubsRepo clubsRepo => _clubsRepo ?? (_clubsRepo = new ClubsRepo(db));
        protected EventsRepo eventsRepo => _eventsRepo ?? (_eventsRepo = new EventsRepo(db));
        protected BenefitsRepo benefitsRepo => _benefitsRepo ?? (_benefitsRepo = new BenefitsRepo(db));
        protected TeamsRepo teamRepo => _teamRepo ?? (_teamRepo = new TeamsRepo(db));
        protected PlayersRepo playersRepo => _playersRepo ?? (_playersRepo = new PlayersRepo(db));
        protected PositionsRepo posRepo => _posRepo ?? (_posRepo = new PositionsRepo(db));
        protected UnionsRepo unionsRepo => _unionsRepo ?? (_unionsRepo = new UnionsRepo(db));
        protected AuditoriumsRepo auditoriumsRepo => _auditoriumsRepo ?? (_auditoriumsRepo = new AuditoriumsRepo(db));
        protected SeasonsRepo seasonsRepository => _seasonsRepository ?? (_seasonsRepository = new SeasonsRepo(db));
        protected SectionsRepo secRepo => _secRepo ?? (_secRepo = new SectionsRepo());
        public JobsRepo jobsRepo => _jobsRepo ?? (_jobsRepo = new JobsRepo(db));
        public GroupsRepo groupRepo => _groupRepo ?? (_groupRepo = new GroupsRepo(db));
        public StagesRepo stagesRepo => _stagesRepo ?? (_stagesRepo = new StagesRepo(db));
        protected AgesRepo agesRepo => _agesRepo ?? (_agesRepo = new AgesRepo());
        protected FriendshipTypesRepo friendshipTypesRepo => _friendshipTypesRepo ?? (_friendshipTypesRepo = new FriendshipTypesRepo(db));
        protected PricesRepo pricesRepo => _pricesRepo ?? (_pricesRepo = new PricesRepo(db));

        protected RegionalsRepo regionalsRepo => _regionalsRepo ?? (_regionalsRepo = new RegionalsRepo(db));

        #endregion

        internal CultEnum getCulture()
        {
            return LocaleHelper.GetCultureByLocale(Request.Cookies[LocaleHelper.LocaleCookieName]?.Value);
        }

        internal List<KeyValuePair<string, string>> getCountries()
        {
            List<KeyValuePair<string, string>> CountryList = new List<KeyValuePair<string, string>>();
            CountryList.Add(new KeyValuePair<string, string>("", Messages.Select));
            CultureInfo[] CInfoList = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo CInfo in CInfoList)
            {
                RegionInfo R = new RegionInfo(CInfo.LCID);
                if (!(CountryList.Any(x => x.Key == R.EnglishName)))
                {
                    CountryList.Add(new KeyValuePair<string, string>(R.EnglishName, R.DisplayName));
                }
            }

            CountryList = CountryList.OrderBy(x => x.Key).ToList();

            return CountryList;
        }

        internal string getCountryCode(string country)
        {
            var code = "";
            CultureInfo[] CInfoList = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo CInfo in CInfoList)
            {
                RegionInfo R = new RegionInfo(CInfo.LCID);
                if (R.EnglishName == country)
                {
                    return R.ThreeLetterISORegionName;
                }
            }
            return code;
        }

        protected bool IsHebrew => Request.IsHebrew();

        protected int AdminId
        {
            get { return int.Parse(User.Identity.Name); }
        }

        [HttpPost]
        public ActionResult SetUnionCurrentSeason(int seasonId)
        {
            Session[UNION_CURRENT_SEASON_ID] = seasonId;

            return Json(seasonId, JsonRequestBehavior.AllowGet);
        }

        public ContentResult AppleSiteAssociation()
        {
            var data = "{  \"applinks\": {  \"apps\": [], \"details\": [  {   \"appID\": \"95572EV626.com.loglig.tennis\",     \"paths\": [       \"NOT \\/api\\/*\",         \"NOT \\/\",         \"*\",        \"event/event/*\"       ]    }    ]  }}";
            return new ContentResult { Content = data, ContentType = "application/json" };
        }

[HttpPost]
        public ActionResult SetClubCurrentSeason(int seasonId)
        {
            Session[CLUB_CURRENT_SEASON_ID] = seasonId;

            return Json(seasonId, JsonRequestBehavior.AllowGet);
        }

        protected void SetIsSectionClubLevel(bool value)
        {
            Session[IS_SECTION_CLUB_LEVEL] = value;
        }
        protected void SetCurrentClubId(int clubId)
        {
            Session[CURRENT_CLUB_ID] = clubId;
        }

        protected Season getCurrentSeason()
        {
            var isClubLevel = Session[IS_SECTION_CLUB_LEVEL];
            int seasonId;
            if (isClubLevel != null && (bool)isClubLevel)
            {
                seasonId = GetClubCurrentSeasonFromSession();
            }
            else
            {
                seasonId = GetUnionCurrentSeasonFromSession();
            }

            return seasonsRepository.GetSeasons().FirstOrDefault(s => s.Id == seasonId);
        }

        protected Season getUnionlessCurrentSeason()
        {
            return seasonsRepository.GetUnionlessCurrentSeason();
        }

        [HttpGet]
        public ActionResult GetCurrentSeason()
        {
            var season = getCurrentSeason();
            if (season != null)
            {
                string seasonName = season.Name;

                return Json(seasonName, JsonRequestBehavior.AllowGet);
            }

            return Json(string.Empty, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetSeasonFromUrl(int seasonId)
        {
            var season = seasonsRepository.GetById(seasonId);
            if (season != null)
            {
                string seasonName = season.Name;

                return Json(seasonName, JsonRequestBehavior.AllowGet);
            }

            return Json(string.Empty, JsonRequestBehavior.AllowGet);
        }

        protected int GetUnionCurrentSeasonFromSession()
        {
            var seasonId = Session[UNION_CURRENT_SEASON_ID];
            var currentUnionId = Session[CURRENT_UNION_ID];

            if (currentUnionId == null)
            {
                if (seasonId == null || (int)seasonId <= 0)
                {
                    var season = seasonsRepository.GetLastSeason();
                    Session[UNION_CURRENT_SEASON_ID] = season.Id;
                    seasonId = season.Id;
                }
            }
            else
            {
                if (seasonId == null || (int)seasonId <= 0)
                {
                    seasonId = seasonsRepository.GetLastSeason().Id;
                }
                var name = seasonsRepository.GetSeasons().SingleOrDefault(s => s.Id == (int)seasonId)?.Name;
                var seasons = seasonsRepository.GetSeasonsByUnion((int)currentUnionId, false).ToList();
                if (!seasons.Select(s => s.Id).Contains((int)seasonId))
                {
                    seasonId = seasons.Select(s => s.Name).Contains(name) ?
                        seasons.FirstOrDefault(s => s.Name.Equals(name)).Id :
                        seasonsRepository.GetLastSeasonByCurrentUnionId((int)currentUnionId);
                    Session[UNION_CURRENT_SEASON_ID] = seasonId;
                }
            }

            return (int)seasonId;
        }
        /// <summary>
        /// Renders the specified partial view to a string.
        /// </summary>
        /// <param name="controller">The current controller instance.</param>
        /// <param name="viewName">The name of the partial view.</param>
        /// <param name="model">The model.</param>
        /// <returns>The partial view as a string.</returns>
        public string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.RouteData.GetRequiredString("action");
            }

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                // Find the partial view by its name and the current controller context.
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);

                // Create a view context.
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);

                // Render the view using the StringWriter object.
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
        protected int GetClubCurrentSeasonFromSession()
        {
            var seasonId = Session[CLUB_CURRENT_SEASON_ID];
            var currentClubId = Session[CURRENT_CLUB_ID];

            if (currentClubId == null)
            {
                if (seasonId == null)
                {
                    var season = seasonsRepository.GetLastClubSeason();
                    Session[CLUB_CURRENT_SEASON_ID] = season.Id;
                    seasonId = season.Id;
                }
            }
            else
            {
                if (seasonId == null)
                {
                    seasonId = seasonsRepository.GetLastSeasonIdByCurrentClubId(Convert.ToInt32(currentClubId));
                }

                var season = seasonsRepository.GetSeasons().FirstOrDefault(s => s.Id == (int)seasonId);
                if (season != null)
                {
                    var name = season.Name;
                    var seasons = seasonsRepository.GetSeasonsByClub((int)currentClubId).ToList();

                    if (seasons.Select(s => s.Id).Contains((int)seasonId))
                    {
                        seasonId = seasons.Select(s => s.Name).Contains(name) ?
                            seasons.FirstOrDefault(s => s.Name.Equals(name)).Id :
                            seasonsRepository.GetLastSeasonIdByCurrentClubId((int)currentClubId);
                        Session[CLUB_CURRENT_SEASON_ID] = seasonId;
                    }
                }
            }

            return (int)seasonId;
        }

        protected int GetCurrentUnionFromSession()
        {
            int unionId;
            if (Session[CURRENT_UNION_ID] != null && int.TryParse(Session[CURRENT_UNION_ID].ToString(), out unionId))
            {
                return unionId;
            }
            return -1;
        }

        protected void SaveCurrentSectionAliasIntoSession(Section model)
        {
            Session[CURRENT_SECTION] = model.Alias;
        }
        protected void SaveSectionIntoCookie(Section model)
        {
            var sectionAlias = new HttpCookie(CURRENT_SECTION);
            sectionAlias.Values["alias"] = model.Alias;
            System.Web.HttpContext.Current.Response.Cookies.Set(sectionAlias);
        }

        protected string GetSectionAliasFromCookie()
        {
            var cookie = System.Web.HttpContext.Current.Request.Cookies[CURRENT_SECTION];
            if (cookie != null)
            {
                return cookie.Value;
            }
            return "";
        }

        protected string GetAliasSectionFromSession()
        {
            var section = Session[CURRENT_SECTION];
            if (section != null)
            {
                return (string)Session[CURRENT_SECTION];
            }
            return string.Empty;
        }

        public bool IsRegionalFederationEnabled(int regionalId, int unionId = 0, int seasonId = 0)
        {
            if (unionId > 0)
            {
                return unionsRepo.GetById(unionId)?.IsRegionallevelEnabled == true;
            }

            if (seasonId > 0)
            {
                return seasonsRepository.GetById(seasonId)?.Union?.IsRegionallevelEnabled == true;
            }

            var regional = regionalsRepo.GetById(regionalId);

            return regional?.Union?.IsRegionallevelEnabled == true;
        }
    }
}
