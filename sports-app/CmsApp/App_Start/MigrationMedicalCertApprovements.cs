using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using AppModel;
using DataService;

namespace CmsApp.App_Start
{
    public static class MigrationMedicalCertApprovements
    {
        public static void Execute()
        {
            var db = new DataEntities();
            var repo = new BaseRepo(db);

            db.Configuration.AutoDetectChangesEnabled = false;
            db.Configuration.ValidateOnSaveEnabled = false;

            const int pageSize = 2500;
            var offset = 0;

            var newApprovements = new List<MedicalCertApprovement>();

            var usersToMigrate = GetUsers(db, offset, pageSize);

            while (usersToMigrate.Any())
            {
                foreach (var user in usersToMigrate)
                {
                    var seasonIds = user.TeamsPlayers.Select(x => x.SeasonId).Where(x => x != null).Cast<int>().Distinct();

                    foreach (var seasonId in seasonIds)
                    {
                        newApprovements.Add(new MedicalCertApprovement
                        {
                            SeasonId = seasonId,
                            UserId = user.UserId,
                            Approved = user.MedicalCertificate == true
                        });
                    }
                }

                offset += pageSize;
                usersToMigrate = GetUsers(db, offset, pageSize);
            }

            var count = 0;
            foreach (var approvement in newApprovements)
            {
                ++count;

                db = AddToContext(db, approvement, count, 1000, true);
            }

            db.SaveChanges();
        }

        private static IQueryable<User> GetUsers(DataEntities dbContext, int offset, int limit)
        {
            return dbContext.Users.Where(x => !x.MedicalCertApprovements.Any())
                .Include(x => x.TeamsPlayers)
                .Include(x => x.MedicalCertApprovements)
                .OrderBy(x => x.UserId)
                .Skip(offset)
                .Take(limit);
        }

        private static DataEntities AddToContext(DataEntities context, MedicalCertApprovement entity, int count, int commitCount, bool recreateContext)
        {
            context.Set<MedicalCertApprovement>().Add(entity);

            if (count % commitCount == 0)
            {
                context.SaveChanges();
                if (recreateContext)
                {
                    context.Dispose();
                    context = new DataEntities();
                    context.Configuration.AutoDetectChangesEnabled = false;
                }
            }

            return context;
        }
    }
}