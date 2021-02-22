using System.Web.Hosting;
using DataService.Utils;
using FluentScheduler;

namespace DataService.Jobs
{
    public class GameScrapperJob : IJob, IRegisteredObject
    {
        private readonly object _lock = new object();
        private ClubsRepo _clubsRepo;
        private GamesRepo _gamesRepo;
        private TeamsRepo _teamsRepo;
        private bool _shuttingDown;
        public GameScrapperJob()
        {
            HostingEnvironment.RegisterObject(this);
            _clubsRepo = new ClubsRepo();
            _gamesRepo = new GamesRepo();
            _teamsRepo = new TeamsRepo();
        }
        public void Execute()
        {
            lock (_lock)
            {
                if(_shuttingDown)
                    return;

                //TODO service call
                var teamSchedulesUrl = _clubsRepo.GetClubTeamGamesUrl();
                _gamesRepo.UpdateGamesSchedulersFromScrapper(teamSchedulesUrl);

                var teamStandingsUrl = _teamsRepo.GetTeamStandingsUrl();
                _gamesRepo.UpdateTeamStandingsFromScrapper(teamStandingsUrl);

                
            }
            //close all phantomjs process after the end of the timer
            //TODO
            ProcessHelper.ClosePhantomJSProcess();
        }

        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                _shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }
    }
}
