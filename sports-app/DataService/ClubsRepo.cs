using AppModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DataService.DTO;
using DataService.Utils;
using System.Threading.Tasks;

namespace DataService
{
    public class ClubsRepo : BaseRepo
    {
        private PlayersRepo playersRepo;
        public ClubsRepo() : base()
        {
        }
        public ClubsRepo(DataEntities db) : base(db) { }

        private void GetPlayersRepoInstance()
        {
            playersRepo = new PlayersRepo();
        }

        public List<int> GetByManagerId(int managerId)
        {
            return db.UsersJobs
                //.Where(j => j.UserId == managerId && j.Season.IsActive)
                .Where(j => j.UserId == managerId)
                .Select(j => j.ClubId)
                .Where(u => u != null)
                .Cast<int>()
                .Distinct()
                .ToList();
        }

        public Club GetById(int id)
        {
            return db.Clubs.Find(id);
        }

        public List<Club> GetBySection(int sectionId)
        {
            return db.Clubs
                .Where(c => c.SectionId == sectionId &&
                            !c.IsArchive &&
                            c.ParentClubId == null)
                .Distinct()
                .ToList();
        }

        public List<Club> GetByUnion(int unionId, int? seasonId, bool isFlowersOfSport = false)
        {
            return db.Clubs
                .Where(c =>
                    c.UnionId == unionId &&
                    (!seasonId.HasValue || c.SeasonId == seasonId) &&
                    !c.IsArchive &&
                    c.IsFlowerOfSport == isFlowersOfSport)
                .Distinct()
                .OrderBy(c => c.Name)
                .ToList();
        }

        public List<ActivityFormsSubmittedData> GetUnionClubActivityRegistrations(int unionId, int seasonId, int? clubId = null)
        {
            return GetCollection<ActivityFormsSubmittedData>(x =>
                    (clubId == null || x.ClubId == clubId) &&
                    x.Activity.UnionId == unionId &&
                    x.Activity.SeasonId == seasonId &&
                    x.Activity.Type == ActivityType.Club &&
                    x.Activity.IsAutomatic == true)
                .ToList();
        }

        public void Create(Club club)
        {
            if (!club.UnionId.HasValue)
            {
                var season = new Season
                {
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    ClubId = club.ClubId,
                    IsActive = true
                };
                season.Name = $"{season.StartDate.Year}-{season.EndDate.Year}";
                season.Description = $"Season {season.Name} of club {club.Name}";
                club.Seasons.Add(season);
            }
            club.CreateDate = DateTime.UtcNow;
            db.Clubs.Add(club);
        }

        #region ClubBalance
        public void CreateClubBalance(ClubBalance model)
        {
            db.ClubBalances.Add(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateClubBalance(ClubBalanceDto dto)
        {
            var entity = db.ClubBalances.Find(dto.Id.Value);
            entity.Comment = dto.Comment;
            entity.Expense = dto.Expense;
            entity.Income = dto.Income;
            entity.Reference = dto.Reference;
        }
        /// <summary>
        /// Update the balance of club from excel
        /// </summary>
        /// <param name="balances"></param>
        public void UpdateClubBalanceFromExcel(List<ClubBalanceDto> balances)
        {
            ClubBalance balance = new ClubBalance();
            foreach (var item in balances)
            {
                balance.ActionUserId = item.ActionUser.UserId;
                balance.ClubId = item.ClubId;
                balance.SeasonId = item.SeasonId;
                balance.Reference = item.Reference;
                balance.Comment = item.Comment;
                balance.Expense = item.Expense;
                balance.Income = item.Income;
                balance.TimeOfAction = item.TimeOfAction;

                using (var ctx = new DataEntities())
                {

                    //Reference number is unique. Need to check when import and update by the reference number id exist already or create a new row
                    var e_check = ctx.ClubBalances.FirstOrDefault(x => x.Reference == item.Reference && x.ClubId == item.ClubId && x.SeasonId == item.SeasonId);
                    if (e_check != null)
                    {
                        //if row already exist then we need to update the Expense or the Income (there is a second row of income or the same row with new data)
                        if (balance.Expense != 0 && e_check.Expense != balance.Expense)
                        {
                            e_check.Expense = balance.Expense;
                        }
                        if (balance.Income != 0 && e_check.Income != balance.Income)
                        {
                            e_check.Income = balance.Income;
                        }
                        if (e_check.ActionUserId != balance.ActionUserId)
                        {
                            e_check.ActionUserId = balance.ActionUserId;
                        }
                        if (e_check.Comment != balance.Comment && !e_check.Comment.Contains('/'))
                        {
                            e_check.Comment = balance.Comment + " / " + e_check.Comment;
                        }

                        ctx.ClubBalances.Attach(e_check);
                        ctx.Configuration.ValidateOnSaveEnabled = true;
                        var entry = ctx.Entry(e_check);
                        entry.State = EntityState.Modified;
                    }
                    else
                    {
                        var entry = ctx.Entry(balance);
                        ctx.ClubBalances.Attach(balance);
                        ctx.Entry(balance).State = EntityState.Added;
                    }

                    ctx.SaveChanges();
                }
            }
        }
        #endregion

        public void CreateTeamClub(ClubTeam clubTeam)
        {
            db.ClubTeams.Add(clubTeam);
        }

        public ClubTeam GetTeamClub(int clubId, int teamId, int seasonId)
        {
            return db.ClubTeams.FirstOrDefault(x => x.ClubId == clubId &&
                                                    x.TeamId == teamId &&
                                                    x.SeasonId == seasonId);
        }

        public void DeleteClubBalance(int clubBalanceId)
        {
            var item = db.ClubBalances.Find(clubBalanceId);
            if (item != null)
            {
                db.TeamPlayersPayments.RemoveRange(item.TeamPlayersPayments);
                db.TeamRegistrationPayments.RemoveRange(item.TeamRegistrationPayments);
                db.ClubBalances.Remove(item);
            }
        }

        public int GetNumberOfClubTeams(int clubId, int teamId)
        {
            return db.ClubTeams.Count(x => x.ClubId == clubId && x.TeamId == teamId);
        }

        public bool IsExistClubTeamForCurrentSeason(int clubId, int teamId, int? seasonId)
        {
            return db.ClubTeams.Any(t => t.ClubId == clubId && t.TeamId == teamId && t.SeasonId == seasonId);
        }

        public void RemoveTemClub(ClubTeam item)
        {
            db.ClubTeams.Remove(item);
        }

        public List<string> GetClubTeamGamesUrl()
        {
            var gamesUrl = db.Teams.Where(x => !string.IsNullOrEmpty(x.GamesUrl) && x.ClubTeams.Any()).Select(x => x.GamesUrl).ToList();
            return gamesUrl;
        }

        public IEnumerable<Club> GetByTeamAndSeason(int teamId, int seasonId)
        {
            var clubsByTeam = db.Clubs.Where(c => !c.IsArchive && c.ClubTeams.Any(ct => ct.TeamId == teamId && ct.SeasonId == seasonId)).ToList();

            var clubsBySchool = db.SchoolTeams.Where(x => x.TeamId == teamId && x.School.SeasonId == seasonId && !x.School.Club.IsArchive).Select(x => x.School.Club).ToList();

            clubsByTeam.AddRange(clubsBySchool);

            return clubsByTeam;
        }

        public IList<ClubShort> GetByTeamAndSeasonShort(int teamId, int seasonId)
        {
            return GetByTeamAndSeason(teamId, seasonId).Select(c => new ClubShort
            {
                Id = c.ClubId,
                Name = c.Name,
                SeasonId = seasonId
            }).ToList();
        }

        public IEnumerable<ClubTeam> GetTeamClubs(int clubId, int seasonId)
        {
            return db.ClubTeams.Where(c => c.ClubId == clubId && c.SeasonId == seasonId && !c.IsBlocked);
        }

        public IEnumerable<TrainingDTO> GetAllClubTrainings(int clubId)
        {
            var clubSection = db.Clubs.Find(clubId)?.SportSection;
            var clubTeamsIds = db.ClubTeams.Where(t => t.ClubId == clubId).Select(c => c.TeamId);
            var clubTrainings = db.TeamTrainings.Where(x => clubTeamsIds.Contains(x.TeamId)).ToList();

            var leaguesIds = db.Leagues.Where(c => c.ClubId == clubId).Select(c => c.LeagueId).ToList();

            if (leaguesIds != null && leaguesIds.Count > 0)
            {
                var leagueTeams = db.LeagueTeams.Where(lt => leaguesIds.Contains(lt.LeagueId)).Select(t => t.TeamId).ToList();
                var leaguesTeamsIds = leagueTeams.Except(clubTeamsIds);
                foreach (var leagueTeamId in leaguesTeamsIds)
                {
                    clubTrainings.AddRange(db.TeamTrainings.Where(t => t.TeamId == leagueTeamId));
                }
            }

            if (clubTrainings != null && clubTrainings.Count > 0)
            {
                return clubTrainings.Select(c => new TrainingDTO
                {
                    ClubId = clubId,
                    ClubName = db.Clubs.FirstOrDefault(club => club.ClubId == clubId)?.Name,
                    TeamId = c?.TeamId,
                    AuditoriumId = c?.AuditoriumId,
                    AuditoriumName = c?.Auditorium?.Name ?? "",
                    StartDate = c.TrainingDate,
                    SportId = clubSection.SectionId,
                    SportName = clubSection.Name,
                    TeamName = c.Team?.Title
                });
            }
            else
            {
                return new List<TrainingDTO>();
            }
        }
        public Dictionary<int, string> GetAllClubs() =>
            db.Clubs.ToDictionary(p => p.ClubId, p => p.Name);

        public IEnumerable<GameDTO> GetAllClubGames(int clubId)
        {
            var clubSection = db.Clubs.Find(clubId)?.SportSection;

            var clubTeamsIds = db.ClubTeams.Where(t => t.ClubId == clubId).Select(c => c.TeamId);

            var clubGamesDb = new List<GamesCycle>();

            foreach (var teamId in clubTeamsIds)
            {
                clubGamesDb.AddRange(db.GamesCycles.Where(c => c.HomeTeamId == teamId));
                clubGamesDb.AddRange(db.GamesCycles.Where(c => c.GuestTeamId == teamId));
            }

            var leaguesIds = db.Leagues.Where(c => c.ClubId == clubId).Select(c => c.LeagueId).ToList();

            if (leaguesIds != null && leaguesIds.Count > 0)
            {
                var leagueTeams = db.LeagueTeams.Where(lt => leaguesIds.Contains(lt.LeagueId)).Select(t => t.TeamId).ToList();
                var leagueTeamsIds = leagueTeams.Except(clubTeamsIds);
                foreach (var leagueTeamId in leagueTeamsIds)
                {
                    clubGamesDb.AddRange(db.GamesCycles.Where(c => c.HomeTeamId == leagueTeamId));
                    clubGamesDb.AddRange(db.GamesCycles.Where(c => c.GuestTeamId == leagueTeamId));
                }
            }

            if (clubGamesDb != null && clubGamesDb.Count > 0)
            {
                var clubGames = clubGamesDb.GroupBy(c => c.CycleId).Select(g => g.FirstOrDefault()).ToList();
                return clubGames.Select(g => new GameDTO
                {
                    ClubId = clubId,
                    ClubName = db.Clubs.FirstOrDefault(club => club.ClubId == clubId)?.Name,
                    HomeTeamId = g.HomeTeamId,
                    GuestTeamId = g.GuestTeamId,
                    AuditoriumId = g.AuditoriumId,
                    AuditoriumName = g.Auditorium?.Name,
                    GroupName = g.Group?.Name,
                    StartDate = g.StartDate,
                    SportId = clubSection.SectionId,
                    SportName = clubSection.Name,
                    HomeTeamName = g.HomeTeam?.Title ?? "",
                    GuestTeamName = g.GuestTeam?.Title ?? ""
                });
            }
            else
            {
                return new List<GameDTO>();
            }

        }

        public Dictionary<int, int[]> GetPlayersInformation(List<Club> unionClubs, int seasonId)
        {
            GetPlayersRepoInstance();
            var playersInfo = new Dictionary<int, int[]>();
            if (unionClubs.Any())
            {
                foreach (var club in unionClubs)
                {
                    string sectionAlias = club?.Section?.Alias ?? club?.Union?.Section?.Alias ?? string.Empty;
                    List<PlayerViewModel> players = playersRepo.GetTeamPlayersShortByClubId(club.ClubId, seasonId, sectionAlias);
                    if (sectionAlias.Equals(GamesAlias.Gymnastic))
                    {
                        var result = new List<PlayerViewModel>();
                        var groupedPlayers = players.GroupBy(x => x.UserId);
                        foreach (var groupedPlayer in groupedPlayers)
                        {
                            var resultPlayer = players.First(x => x.UserId == groupedPlayer.Key);
                            resultPlayer.TeamName = string.Join(", ", groupedPlayer.Select(x => x.TeamName));
                            result.Add(resultPlayer);
                        }

                        players = result;
                    }
                    var section = club?.Union?.Section?.Alias ?? club?.Section?.Alias;
                    CountPlayersRegistrations(players, section.Equals(GamesAlias.Gymnastic), out int approvedPlayers, out int completedPlayers,
                             out int notApprovedPlayers, out int playersCount, out int waitingForApproval, out int active, out int notActive,
                             out int registered);
                    int iTennis = GetItennisNumber(club.ClubId, seasonId);
                    playersInfo.Add(club.ClubId, new int[] { waitingForApproval, approvedPlayers, active, iTennis });
                }
            }

            return playersInfo;
        }

        public void CountPlayersRegistrations(List<PlayerViewModel> players, bool isGymnastic, out int approvedPlayers, out int completedPlayers,
            out int notApprovedPlayers, out int playersCount, out int waitingForApproval, out int activePlayers, out int notActive, out int registered)
        {
            approvedPlayers = 0;
            completedPlayers = 0;
            notApprovedPlayers = 0;
            waitingForApproval = 0;
            activePlayers = 0;
            playersCount = 0;
            notActive = 0;
            registered = 0;
            var playersList = isGymnastic
                ? players.GroupBy(p => p.UserId)?.Select(t => t.First())?.ToList()
                : players.ToList();
            foreach (var player in playersList)
            {
                playersCount++;

                if (player.IsActive == true)
                    completedPlayers += 1;

                if (player.IsNotApproveChecked)
                    notApprovedPlayers += 1;

                if (player.IsActivePlayer)
                    activePlayers += 1;

                if (player.IsNotActive)
                    notActive += 1;

                if (player.IsActive == true && (player.IsPlayerRegistrationApproved || player.IsApproveChecked == true))
                {
                    approvedPlayers += 1;
                    continue;
                }

                if (player.IsActive == true && !player.IsApproveChecked && !player.IsNotApproveChecked)
                {
                    waitingForApproval += 1;
                    continue;
                }

                if (player.IsActive == true && player.IsPlayerRegistered && !player.IsPlayerRegistrationApproved)
                {
                    registered += 1;
                    continue;
                }
            }
        }

        public int GetItennisNumber(int clubId, int seasonId)
        {
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            var clubTeams = new List<ClubTeam>();

            if (club?.RelatedClubs != null && club.RelatedClubs.Count > 0)
            {
                foreach (var departament in club.RelatedClubs)
                {
                    clubTeams.AddRange(departament.ClubTeams.Where(c => c.SeasonId == seasonId && !c.Team.IsArchive));
                }
            }
            else
            {
                clubTeams = db.ClubTeams.Where(ct => ct.ClubId == clubId && !ct.Team.IsArchive).ToList();
            }

            var clubTeamsIds = clubTeams?.Select(c => c.Team.TeamId)?.ToList();
            var isClubsBlocked = club?.Union?.IsClubsBlocked ?? false;
            if (isClubsBlocked && club?.IsUnionArchive == true)
            {
                isClubsBlocked = false;
            }

            GetPlayersRepoInstance();
            var itennis = playersRepo.GetITennisCount(clubTeamsIds, seasonId, club.UnionId, isClubsBlocked);
            return itennis;
        }

        public void CountSchoolPlayersRegistrations(IEnumerable<PlayerViewModel> players, bool isGymnastic, out int approvedPlayers, out int completedPlayers,
            out int notApprovedPlayers, out int playersCount, out int waitingForApproval, out int activePlayers)
        {
            approvedPlayers = 0;
            completedPlayers = 0;
            notApprovedPlayers = 0;
            waitingForApproval = 0;
            activePlayers = 0;
            playersCount = 0;

            var playersList = isGymnastic
                ? players.GroupBy(p => p.UserId)?.Select(t => t.First())?.ToList()
                : players.ToList();
            foreach (var player in playersList)
            {
                playersCount++;

                if (player.IsActive == true)
                    completedPlayers += 1;

                if (player.IsNotApproveChecked)
                    notApprovedPlayers += 1;

                if (player.IsActivePlayer)
                    activePlayers += 1;

                if ((player.IsActive == true && (player.IsPlayerRegistrationApproved || player.IsApproveChecked == true))
                    || player.IsApprovedInSubmitted)
                {
                    approvedPlayers += 1;
                    continue;
                }

                if (player.IsActive == true && !player.IsApproveChecked && !player.IsNotApproveChecked)
                {
                    waitingForApproval += 1;
                    continue;
                }
            }
        }

        public IEnumerable<EventDTO> GetAllClubEvents(int clubId)
        {
            var clubSection = db.Clubs.Find(clubId)?.SportSection ?? db.Clubs.Find(clubId)?.Section;

            var clubEvents = db.Clubs.Find(clubId)?.Events;

            if (clubEvents != null && clubEvents.Count > 0)
            {
                return clubEvents.Select(e => new EventDTO
                {
                    ClubId = clubId,
                    ClubName = db.Clubs.FirstOrDefault(club => club.ClubId == clubId)?.Name,
                    EventTime = e.EventTime,
                    Place = e.Place,
                    Title = e.Title,
                    SportId = clubSection?.SectionId,
                    SportName = clubSection?.Name
                });
            }
            return new List<EventDTO>();
        }

        public void ChangeDistanceSettings(int id, string type)
        {
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == id);
            if (club != null)
            {
                club.DistanceSettings = type;
                db.SaveChanges();
            }
        }

        public void ChangeReportSettings(int id, string type, bool removeTravel)
        {
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == id);
            if (club != null)
            {
                club.ReportSettings = type;
                club.ReportRemoveTravelDistance = removeTravel;
                db.SaveChanges();
            }
        }

        public Union GetUnionByClub(int clubId, int seasonId)
        {
            return db.Clubs
                .Include(t => t.Union)
                .Where(t => t.IsArchive == false && t.ClubId == clubId && t.SeasonId == seasonId)
                .Select(t => t.Union)
                .FirstOrDefault();
        }

        public void RegisterTennisTeam(IEnumerable<int> teamIds, int clubId, int leagueId, int? seasonId)
        {
            var registrations = db.TeamRegistrations.Where(tr => tr.SeasonId == seasonId && tr.ClubId == clubId && tr.LeagueId == leagueId)
                .Include(c => c.TeamsDetails);
            if (teamIds != null && teamIds.Any())
            {
                /*
                if (registrations.Any())
                {
                    db.TeamRegistrations.RemoveRange(registrations);
                }
                */
                var league = db.Leagues.FirstOrDefault(t => t.LeagueId == leagueId);
                foreach (var teamId in teamIds)
                {
                    if (teamId != 0)
                    {
                        var registeredButDeleted = registrations.Where(t => t.TeamId == teamId).FirstOrDefault();
                        if (registeredButDeleted != null)
                        {
                            registeredButDeleted.IsDeleted = false;
                        }
                        else
                        {
                            db.TeamRegistrations.Add(new TeamRegistration
                            {
                                TeamId = teamId,
                                ClubId = clubId,
                                LeagueId = leagueId,
                                SeasonId = seasonId,
                            });
                        }
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                if (registrations.Any())
                {
                    db.TeamRegistrations.RemoveRange(registrations);
                    db.SaveChanges();
                }
            }
        }



        public void MoveTennisTeams(int[] teamIds, int ToleagueId, int fromLeagueId, int? seasonId)
        {
            var registrations = db.TeamRegistrations.Where(tr => tr.SeasonId == seasonId && tr.LeagueId == fromLeagueId).Include(c => c.TeamsDetails);
            if (teamIds != null && teamIds.Any())
            {
                foreach (var teamId in teamIds)
                {
                    if (teamId != 0)
                    {
                        var registration = registrations.FirstOrDefault(t => t.TeamId == teamId);
                        if (registration != null)
                        {
                            registration.LeagueId = ToleagueId;
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        public void CopyTennisTeams(int[] teamIds, int ToleagueId, int fromLeagueId, int? seasonId)
        {
            var newLeague = db.Leagues.FirstOrDefault(l => l.LeagueId == ToleagueId);
            var registrations = db.TeamRegistrations.Where(tr => tr.SeasonId == seasonId && tr.LeagueId == fromLeagueId).Include(c => c.TeamsDetails);
            if (teamIds != null && teamIds.Any())
            {
                foreach (var teamId in teamIds)
                {
                    if (teamId != 0)
                    {
                        var registration = registrations.FirstOrDefault(t => t.TeamId == teamId);
                        if (registration != null)
                        {
                            newLeague.TeamRegistrations.Add(new TeamRegistration
                            {

                                TeamId = teamId,
                                ClubId = registration.ClubId,
                                LeagueId = ToleagueId,
                                SeasonId = seasonId
                            });
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        public IEnumerable<TeamRegistration> GetAllTennisRegistrations(int clubId, int seasonId)
        {
            return db.TeamRegistrations.Where(tr => tr.ClubId == clubId && tr.SeasonId == seasonId && !tr.League.IsArchive && !tr.IsDeleted);
        }

        public ClubTeam CheckForTrainingTeam(int clubId, int seasonId)
        {
            var trainingTeam = db.ClubTeams.FirstOrDefault(c => c.ClubId == clubId && c.SeasonId == seasonId && c.IsTrainingTeam);
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            if (trainingTeam == null)
            {
                db.Teams.Add(new Team { Title = club?.Name, CreateDate = DateTime.Now });
                db.SaveChanges();
                var teamId = db.Teams.OrderByDescending(c => c.TeamId).FirstOrDefault()?.TeamId;
                if (teamId != null)
                {
                    db.ClubTeams.Add(new ClubTeam
                    {
                        ClubId = clubId,
                        SeasonId = seasonId,
                        TeamId = teamId.Value,
                        IsTrainingTeam = true
                    });
                    db.SaveChanges();
                    trainingTeam = db.ClubTeams.FirstOrDefault(c => c.ClubId == clubId && c.SeasonId == seasonId && c.IsTrainingTeam);
                }
            }

            return trainingTeam;
        }

        public Club GetByNumber(int clubNumber, int seasonId, int? unionId)
        {
            return db.Clubs.FirstOrDefault(c => c.UnionId == unionId && c.ClubNumber == clubNumber && c.SeasonId == seasonId);
        }

        public string GetSectionByClubId(int clubId)
        {
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            return club?.Section?.Alias ?? club?.Union?.Section?.Alias;
        }

        public Club GetByUserId(int id)
        {
            Club club = null;
            var user = db.Users.FirstOrDefault(u => u.UserId == id);
            var userTeam = user?.TeamsPlayers?.OrderByDescending(t => t.Id)?.FirstOrDefault()?.Team;
            if (userTeam != null)
            {
                club = userTeam.ClubTeams.FirstOrDefault()?.Club;
            }
            return club;
        }

        public int RemoveLeagueNamesFromTennis()
        {
            var count = 0;
            var leagueRegistrationTeams = db.TeamsDetails.Where(t => t.RegistrationId.HasValue);
            db.TeamsDetails.RemoveRange(leagueRegistrationTeams);
            count = leagueRegistrationTeams.Count();
            var tennisRegistrations = db.TeamRegistrations;
            foreach (var registration in tennisRegistrations)
            {
                var registrationTeam = registration.Team;
                var teamDetails = registrationTeam.TeamsDetails.FirstOrDefault(t => t.TeamName.Contains(registration.League.Name));
                if (teamDetails != null)
                {
                    db.TeamsDetails.Remove(teamDetails);
                    count++;
                }
                if (registration.Team.Title.Contains(registration.League.Name))
                {
                    registrationTeam.Title = registrationTeam.Title.Replace($" - {registration.League.Name}", string.Empty);
                    count++;
                }
            }
            db.SaveChanges();
            return count;
        }

        public void UpdateClubShowPaymentToClubManager(int clubId, int seasonId, bool isShow)
        {
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId && c.SeasonId == seasonId);
            if (club != null)
            {
                club.IsClubManagerCanSeePayReport = isShow;
                Save();
            }
        }

        public Club AccountingKeyNumber(int accountingKeyNumber, int SeasonId)
        {
            return db.Clubs.FirstOrDefault(x => x.AccountingKeyNumber == accountingKeyNumber && x.SeasonId == SeasonId);
        }
    }
}
