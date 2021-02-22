using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using CmsApp.Helpers;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading;
using CmsApp.Controllers;
using System.Timers;
using AppModel;
using CmsApp.App_Start;
using DataService;


namespace CmsApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        System.Timers.Timer aTimer;
        private System.Timers.Timer recurringTasks;
        private readonly int recurringTasksInterval = 12 * 60 * 60 * 1000; // 2 * 60 * 1000 = 2 minutes, dont test less than that because the procedure iterates throough over 30k users and takes long.
        protected void Application_Start()
        {
            // if you want to modify and test minified scripts
            // System.Web.Optimization.BundleTable.EnableOptimizations = true;

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ModelBinders.Binders.Add(typeof(DateTime?), new DateModelBinder());

            ClientDataTypeModelValidatorProvider.ResourceClassKey = "Messages";
            DefaultModelBinder.ResourceClassKey = "Messages";

            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                    typeof(RequiredAttribute),
                    typeof(CustRequiredAttributeAdapter)
                );

            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                    typeof(RangeAttribute),
                    typeof(CustRangeAttributeAdapter)
                );

            GlobalConfiguration.Configuration.EnsureInitialized();

            //JobManager.Initialize(new GamesRegistry());

            //MigrationTeamPlayersAddLeagueIdAndClubId.Execute();
            //MigrationMedExamDate.Execute();
            //MigrationMedicalCertApprovements.Execute();
            //MigrationPlayersIdFile.Execute();
            //MigrationUserFullName.Execute();

            string scrapperValue = Environment.GetEnvironmentVariable("LOGLIG_DEVELOPMENT_NO_SCRAPPER", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrWhiteSpace(scrapperValue))
            {
                aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.Interval = 60 * 1000 * 60 * 6;
                aTimer.Enabled = true;
                aTimer.Start();
            }

            recurringTasks = new System.Timers.Timer();
            recurringTasks.Elapsed += new ElapsedEventHandler(OnRecurringEvent);
            recurringTasks.Interval = recurringTasksInterval;
            recurringTasks.Enabled = true;
            recurringTasks.Start();

        }

        private static void OnRecurringEvent(object source, ElapsedEventArgs e)
        {
            runMedicalCertificateChecks();
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            runScrapper();
        }

        private static void runMedicalCertificateChecks()
        {
            Startup.RunMedicalCertificateChecks();
        }


        private static void runScrapper()
        {
            DataEntities db = new DataEntities();
            var teamGames = db.TeamScheduleScrapperGames.ToList();
            TeamsController controller = new TeamsController();
            var existedSchedulerGame = db.TeamScheduleScrappers.ToList();

            if (existedSchedulerGame.Count > 0)
            {
                db.TeamScheduleScrappers.RemoveRange(existedSchedulerGame);
                db.SaveChanges();
            }
            for (int i = 0; i < teamGames.Count; i++)
            {
                var gameDetail = teamGames[i];

                controller.SaveGamesUrl(gameDetail.TeamId.Value, gameDetail.GameUrl, gameDetail.ExternalTeamName, gameDetail.ClubId.Value, gameDetail.SeasonId ?? 0, true);

                Thread.Sleep(30000);
            }

            var teamStandings = db.TeamStandingGames.ToList();
            for (int j = 0; j < teamStandings.Count; j++)
            {
                var standingDetail = teamStandings[j];
                controller.SaveTeamStandingGameUrl(standingDetail.TeamId, standingDetail.ClubId.Value, standingDetail.GamesUrl, standingDetail.ExternalTeamName, standingDetail.SeasonId ?? 0);

                Thread.Sleep(30000);
            }
        }

        protected void Application_BeginRequest()
        {
            HttpCookie cookie = Request.Cookies[LocaleHelper.LocaleCookieName];
            if (cookie != null)
            {
                var ci = CultureInfo.GetCultureInfo(cookie.Value);

                Thread.CurrentThread.CurrentCulture = new CultureInfo("he-IL");
                Thread.CurrentThread.CurrentUICulture = ci;
            }
        }

    }
}
