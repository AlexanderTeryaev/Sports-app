using AppModel;
using DataService.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService
{
    public class ActivityRepo : BaseRepo
    {
        public ActivityRepo() : base()
        {

        }

        public ActivityRepo(DataEntities db) : base(db)
        {
        }


        public void CreateActivityForSectionClubSeason(int clubId, int seasonId)
        {
            var season = db.Seasons.FirstOrDefault(p => p.Id == seasonId);

            if (season != null)
            {
                var bName = $"Club registrations - {season.Name}";
                var branch =
                    db.ActivityBranches.FirstOrDefault(
                        x => x.BranchName.ToLower().Trim().Equals(bName, StringComparison.InvariantCultureIgnoreCase) &&
                             x.ClubId == clubId &&
                             x.SeasonId == seasonId);

                if (branch == null)
                {
                    branch = new ActivityBranch
                    {
                        BranchName = bName,
                        Season = season,
                        ClubId = clubId
                    };
                    db.ActivityBranches.Add(branch);
                }

                var playerActivity = new Activity
                {
                    ActivityBranch = branch,
                    ClubId = clubId,
                    SeasonId = seasonId,
                    Name = "Player registration",
                    Type = ActivityType.Personal,
                    FormPayment = ActivityFormPaymentType.Periods,
                    Date = DateTime.Now,
                    StartDate = season.StartDate,
                    EndDate = season.EndDate,
                    IsAutomatic = true,
                    DefaultLanguage = "he"
                };

                db.Activities.Add(playerActivity);

                db.SaveChanges();
            }
        }

        public void CreateActivityForClubSeason(int clubId, int seasonId)
        {
            var season = db.Seasons.FirstOrDefault(p => p.Id == seasonId);

            if (season != null)
            {
                var bName = $"Clubs registration - {season.Name}";
                var branch =
                    db.ActivityBranches.FirstOrDefault(
                        x => x.BranchName.ToLower().Trim().Equals(bName, StringComparison.InvariantCultureIgnoreCase) &&
                             x.ClubId == clubId &&
                             x.SeasonId == seasonId);

                if (branch == null)
                {
                    branch = new ActivityBranch
                    {
                        BranchName = bName,
                        Season = season,
                        ClubId = clubId
                    };
                    db.ActivityBranches.Add(branch);
                }

                var playerActivity = new Activity
                {
                    ActivityBranch = branch,
                    ClubId = clubId,
                    SeasonId = seasonId,
                    Name = "Player registration",
                    Type = ActivityType.Personal,
                    FormPayment = ActivityFormPaymentType.Periods,
                    Date = DateTime.Now,
                    StartDate = season.StartDate,
                    EndDate = season.EndDate,
                    IsAutomatic = true,
                    DefaultLanguage = "he"
                };

                db.Activities.Add(playerActivity);

                db.SaveChanges();
            }
        }

        public void CreateActivityForUnionSeason(int unionId, int seasonId, bool isDuplicate, int[] leagueIds)
        {
            var season = db.Seasons.FirstOrDefault(p => p.Id == seasonId);

            if (season != null)
            {
                var branchName = $"Leagues registration - {season.Name}";
                var branch =
                    db.ActivityBranches.FirstOrDefault(
                        p => p.BranchName.Trim().Equals(branchName, StringComparison.InvariantCultureIgnoreCase) &&
                             p.UnionId == unionId && p.SeasonId == seasonId);

                if (branch == null)
                {
                    branch = new ActivityBranch
                    {
                        BranchName = branchName,
                        Season = season,
                        UnionId = unionId
                    };
                    db.ActivityBranches.Add(branch);
                }

                var teamActivity = new Activity
                {
                    ActivityBranch = branch,
                    UnionId = unionId,
                    SeasonId = seasonId,
                    Name = "Teams registration",
                    Type = ActivityType.Group,
                    FormPayment = ActivityFormPaymentType.Periods,
                    Date = DateTime.Now,
                    StartDate = season.StartDate,
                    EndDate = season.EndDate,
                    IsAutomatic = true,
                    DefaultLanguage = "he"
                };
                db.Activities.Add(teamActivity);

                var playerActivity = new Activity
                {
                    ActivityBranch = branch,
                    UnionId = unionId,
                    SeasonId = seasonId,
                    Name = "Player registration",
                    Type = ActivityType.Personal,
                    FormPayment = ActivityFormPaymentType.Periods,
                    Date = DateTime.Now,
                    StartDate = season.StartDate,
                    EndDate = season.EndDate,
                    IsAutomatic = true,
                    DefaultLanguage = "he"
                };
                db.Activities.Add(playerActivity);

                var clubActivity = new Activity
                {
                    ActivityBranch = branch,
                    UnionId = unionId,
                    SeasonId = seasonId,
                    Name = "Club registration",
                    Type = ActivityType.Club,
                    FormPayment = ActivityFormPaymentType.Periods,
                    Date = DateTime.Now,
                    StartDate = season.StartDate,
                    EndDate = season.EndDate,
                    IsAutomatic = true,
                    DefaultLanguage = "he"
                };
                db.Activities.Add(clubActivity);

                var playerToClubActivity = new Activity
                {
                    ActivityBranch = branch,
                    UnionId = unionId,
                    SeasonId = seasonId,
                    Name = "Player to club registration",
                    Type = ActivityType.UnionPlayerToClub,
                    FormPayment = ActivityFormPaymentType.Periods,
                    Date = DateTime.Now,
                    StartDate = season.StartDate,
                    EndDate = season.EndDate,
                    IsAutomatic = true,
                    DefaultLanguage = "he"
                };
                db.Activities.Add(playerToClubActivity);

                db.SaveChanges();

                if (isDuplicate && leagueIds != null)
                {
                    foreach (var lId in leagueIds)
                    {
                        var tal = new ActivitiesLeague
                        {
                            Activity = teamActivity,
                            LeagueId = lId
                        };

                        db.ActivitiesLeagues.Add(tal);

                        var pal = new ActivitiesLeague
                        {
                            Activity = playerActivity,
                            LeagueId = lId
                        };

                        db.ActivitiesLeagues.Add(pal);
                    }

                    db.SaveChanges();
                }
            }
        }

        public void AttachLeagueToActivity(int leagueId, int seasonId)
        {
            var activities = db.Activities.Where(p => p.SeasonId == seasonId && p.IsAutomatic == true).ToList();

            if (activities.Count > 0)
            {
                var team = activities.FirstOrDefault(p => p.Type == ActivityType.Group);
                if (team != null)
                {
                    var tal = new ActivitiesLeague
                    {
                        Activity = team,
                        LeagueId = leagueId
                    };

                    db.ActivitiesLeagues.Add(tal);
                }

                var player = activities.FirstOrDefault(p=>p.Type == ActivityType.Personal);
                if (player != null)
                {
                    var pal = new ActivitiesLeague
                    {
                        Activity = player,
                        LeagueId = leagueId
                    };

                    db.ActivitiesLeagues.Add(pal);

                }

                db.SaveChanges();
            }
        }

        public Activity GetByIdAsNoTracking(int id)
        {
            return db.Activities
                .Include("ActivitiesPrices")
                .Include("ActivitiesUsers")
                .Include("ActivityBranch")
                .AsNoTracking()
                .FirstOrDefault(p => p.ActivityId == id);
        }

        public Activity GetById(int id)
        {
            return db.Activities.FirstOrDefault(p => p.ActivityId == id);
        }


        public void Add(Activity activity)
        {
            db.Activities.Add(activity);
        }

        public ActivityBranch GetBranchById(int id)
        {
            return db.ActivityBranches
                .Include("Activities")
                .AsNoTracking()
                .FirstOrDefault(p => p.AtivityBranchId == id);
        }

        public List<PlayerToClubInfo> GetUnionPlayerToClubByPrevClub(string sectionAlias, int? unionId, int clubId, int? seasonId)
        {
            return GetUnionPlayerToClubByPrevClubQuery(sectionAlias, unionId, clubId, seasonId).Select(x => new PlayerToClubInfo()
                {
                    PlayerName = x.User.FirstName + " " + x.User.LastName,
                    NewClubName = x.Club.Name
                }).ToList();
        }

        public void MarkPlayerToClubActivityAsSeen(string sectionAlias, int? unionId, int clubId, int? seasonId)
        {
            var results = GetUnionPlayerToClubByPrevClubQuery(sectionAlias, unionId, clubId, seasonId);

            foreach(var r in results)
            {
                r.IsSeenByClubManager = true;
            }

            db.SaveChanges();
        }

        private IQueryable<ActivityFormsSubmittedData> GetUnionPlayerToClubByPrevClubQuery(string sectionAlias, int? unionId, int clubId, int? seasonId)
        {
            return db.ActivityFormsSubmittedDatas.Include("Activity")
                .Where(x => x.Activity.Type == ActivityType.UnionPlayerToClub
                && x.Activity.Union.Section.Alias == sectionAlias
                && x.PreviousClubId == clubId && x.IsSeenByClubManager != true);
        }

        //public Activity Create(Activity activity)
        //{
        //    db.Activities.Add(activity);
        //    db.SaveChanges();
        //
        //    return activity;
        //}
        //
        //public void Update(Activity activity)
        //{
        //    db.Entry(activity).State = System.Data.Entity.EntityState.Modified;
        //
        //    db.SaveChanges();
        //}
    }
}
