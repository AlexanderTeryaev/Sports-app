
namespace DataService.Jobs.Registry
{
    public class GamesRegistry : FluentScheduler.Registry
    {
        public GamesRegistry()
        {
            //TODO Test for 5 minutes

            Schedule<GameScrapperJob>().ToRunNow().AndEvery(10).Minutes();
            // Schedule<GameScrapperJob>().ToRunNow().AndEvery(4).Hours();
        }
    }
}
