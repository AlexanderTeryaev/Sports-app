using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using AppModel;

namespace CmsApp.App_Start
{
    public static class MigrationPlayersIdFile
    {
        public static void Execute()
        {
            using (var db = new DataEntities())
            {
                var usersWithoutIdFile = db.Users
                    .Where(x =>
                        x.IDFile == null &&
                        x.PlayerFiles.Any(pf => pf.FileType == (int) PlayerFileType.IDFile))
                    .ToList();

                if (usersWithoutIdFile.Any())
                {
                    var usersIds = usersWithoutIdFile.Select(x => x.UserId).Distinct().ToArray();
                    var playersFiles = db.PlayerFiles
                        .Where(x => usersIds.Contains(x.PlayerId) &&
                                    x.FileType == (int) PlayerFileType.IDFile)
                        .AsNoTracking()
                        .ToList();

                    foreach (var user in usersWithoutIdFile)
                    {
                        var playerFiles = playersFiles
                            .Where(x => x.PlayerId == user.UserId)
                            .OrderBy(x => x.SeasonId)
                            .ToList();

                        if (!playerFiles.Any())
                        {
                            continue;
                        }

                        var latestIdFile = playerFiles.LastOrDefault();
                        if (latestIdFile == null)
                        {
                            continue;
                        }

                        user.IDFile = latestIdFile.FileName;
                    }

                    db.SaveChanges();
                }
            }
        }
    }
}