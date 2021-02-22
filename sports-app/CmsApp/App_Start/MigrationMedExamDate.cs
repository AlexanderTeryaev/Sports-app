using System.Data.Entity;
using System.Linq;
using AppModel;

namespace CmsApp.App_Start
{
    internal static class MigrationMedExamDate
    {
        internal static void Execute()
        {
            using (var db = new DataEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;

                db.Users.Join(db.TeamsPlayers.Where(x => x.MedExamDate != null),
                        x => x.UserId,
                        x => x.UserId,
                        (user, tp) => user)
                    .Load();

                var teamPlayers = db.TeamsPlayers.Where(x => x.MedExamDate != null).ToList();

                foreach (var teamPlayer in teamPlayers)
                {
                    if (teamPlayer.User.MedExamDate == null || teamPlayer.MedExamDate > teamPlayer.User.MedExamDate)
                    {
                        teamPlayer.User.MedExamDate = teamPlayer.MedExamDate;
                    }

                    teamPlayer.MedExamDate = null;
                }

                db.SaveChanges();
            }
        }
    }
}