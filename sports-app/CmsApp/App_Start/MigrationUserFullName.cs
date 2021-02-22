using System.Linq;
using AppModel;
using DataService.Utils;

namespace CmsApp.App_Start
{
    internal static class MigrationUserFullName
    {
        private const int BatchSize = 2500;

        internal static void Execute()
        {
            using (var db = new DataEntities())
            {
                db.Database.CommandTimeout = 900; //15 minutes

                var users = db.Users
                    .Where(x => x.DeprecatedFullName != null && (x.FirstName == null ||
                                                                  x.FirstName.StartsWith("?") && x.FirstName.EndsWith("?") ||
                                                                 x.LastName == null ||
                                                                  x.LastName.StartsWith("?") && x.LastName.EndsWith("?")))
                    .OrderBy(x => x.UserId)
                    .Skip(0)
                    .Take(BatchSize)
                    .ToList();

                while (users.Any())
                {
                    foreach (var user in users)
                    {
                        user.LastName = GetLastNameByFullName(user.DeprecatedFullName);
                        user.FirstName = GetFirstNameByFullName(user.DeprecatedFullName);

                        user.DeprecatedFullName = null;
                    }

                    db.SaveChanges();

                    users = db.Users
                        .Where(x => x.DeprecatedFullName != null && (x.FirstName == null ||
                                                                     x.FirstName.StartsWith("?") && x.FirstName.EndsWith("?") ||
                                                                     x.LastName == null ||
                                                                     x.LastName.StartsWith("?") && x.LastName.EndsWith("?")))
                        .OrderBy(x => x.UserId)
                        .Skip(0)
                        .Take(BatchSize)
                        .ToList();
                }
            }
        }

        private static string GetLastNameByFullName(string fullName)
        {
            var fullNameArray = fullName?.Split(' ')?.ToList();
            var resultString = string.Empty;
            if (fullNameArray?.Any() == true)
            {
                for (var i = 0; i < fullNameArray.Count; i++)
                {
                    if (fullNameArray.IsLast(fullNameArray[i]))
                        resultString += fullNameArray[i];
                }
            }
            return resultString;
        }

        private static string GetFirstNameByFullName(string fullName)
        {
            var fullNameArray = fullName?.Split(' ')?.ToList();
            var resultString = string.Empty;
            if (fullNameArray?.Any() == true)
            {
                for (var i = 0; i < fullNameArray.Count; i++)
                {
                    if (!fullNameArray.IsLast(fullNameArray[i]))
                    {
                        resultString += fullNameArray[i];
                        resultString += " ";
                    }
                }
                if (resultString.Length - 1 >= 0)
                    resultString = resultString.Remove(resultString.Length - 1);
            }
            return resultString;
        }
    }
}