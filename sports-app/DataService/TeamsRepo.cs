using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using AppModel;
using DataService.DTO;
using DataService.Utils;
using DataService.LeagueRank;

namespace DataService
{
    public class PlayerTeam
    {
        public int TeamId { get; set; }
        public int LeagueId { get; set; }
        public string Title { get; set; }
        public string LeagueName { get; set; }
        public string Logo { get; set; }
        public string Position { get; set; }
        public int ShirtNum { get; set; }
    }

    internal class PlayerTeamComparer : IEqualityComparer<PlayerTeam>
    {
        public bool Equals(PlayerTeam x, PlayerTeam y)
        {
            return x?.TeamId == y?.TeamId;
        }

        public int GetHashCode(PlayerTeam obj)
        {
            return (int)((long)obj.TeamId * 17 + obj.TeamId);
        }
    }

    public class TeamsRepo : BaseRepo
    {
        private Func<DateTime, IEnumerable<Season>> currentSeasons;

        public TeamsRepo() : base()
        {
            currentSeasons = date => db.Seasons.Where(s => s.StartDate <= date && s.EndDate >= date);
        }
        public TeamsRepo(DataEntities db) : base(db)
        {
            currentSeasons = date => db.Seasons.Where(s => s.StartDate <= date && s.EndDate >= date);
        }

        public IEnumerable<Team> GetByUnion(int unionId, int seasonId)
        {
            using (var ctx = new DataEntities())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                ctx.Configuration.AutoDetectChangesEnabled = false;

                db.Leagues
                    .Where(x => x.UnionId == unionId &&
                                x.SeasonId == seasonId &&
                                !x.IsArchive)
                    .Load();

                db.LeagueTeams
                    .Where(x => x.Leagues.UnionId == unionId &&
                                x.Leagues.SeasonId == seasonId &&
                                !x.Leagues.IsArchive &&
                                x.SeasonId == seasonId)
                    .Load();

                db.Clubs
                    .Where(x => x.SeasonId == seasonId &&
                                x.UnionId == unionId &&
                                !x.IsArchive)
                    .Load();

                db.ClubTeams
                    .Where(x => x.SeasonId == seasonId &&
                                x.Club.SeasonId == seasonId &&
                                x.Club.UnionId == unionId &&
                                !x.Club.IsArchive)
                    .Load();

                var teams = db.Teams
                    .Include(x => x.TeamsDetails)
                    .Where(x => !x.IsArchive)
                    .Join(db.LeagueTeams.Where(lt => lt.SeasonId == seasonId &&
                                                     lt.Leagues.UnionId == unionId &&
                                                     lt.Leagues.SeasonId == seasonId &&
                                                     !lt.Leagues.IsArchive),
                        x => x.TeamId,
                        x => x.TeamId,
                        (team, leagueTeam) => team)
                    .AsNoTracking()
                    .ToList();

                teams.AddRange(db.Teams
                    .Include(x => x.TeamsDetails)
                    .Where(x => !x.IsArchive)
                    .Join(db.ClubTeams.Where(ct => ct.SeasonId == seasonId &&
                                                   ct.Club.SeasonId == seasonId &&
                                                   ct.Club.UnionId == unionId &&
                                                   !ct.Club.IsArchive),
                        x => x.TeamId,
                        x => x.TeamId,
                        (team, clubTeam) => team)
                    .AsNoTracking()
                    .ToList());

                return teams;
            }
        }

        public IEnumerable<SelectedRoutesDto> GetSelectedRoutes(int teamId, int userId)
        {
            var team = db.Teams.FirstOrDefault(c => c.TeamId == teamId);
            if (team != null)
            {
                var teamRoutes = team?.TeamsRoutes;
                if (teamRoutes.Any())
                {
                    foreach (var teamRoute in teamRoutes.Where(o=>o.UserId == userId))
                    {
                        yield return new SelectedRoutesDto
                        {
                            DisciplineId = teamRoute?.DisciplineTeamRoute?.DisciplineId ?? 0,
                            UsersRouteId = teamRoute.RouteId,
                            UsersRankId = teamRoute?.TeamsRanks?.FirstOrDefault()?.RankId ?? 0
                        };
                    }
                }
            }
        }

        public void UpdateTeamsRoute(int teamId, int userId, int? routeId, int? rankId, out string errorMessage)
        {
            try
            {
                var routeDb = db.TeamsRoutes.FirstOrDefault(c => c.TeamId == teamId && c.RouteId == routeId);
                if (routeId != null)
                {
                    if (routeDb == null)
                    {
                        var newTeamsRouteId = CreateNewRoute(teamId, userId, routeId.Value);

                        if (rankId != null)
                            CreateNewRank(teamId, userId, newTeamsRouteId, rankId.Value);
                    }
                    else
                    {
                        DeleteRoute(teamId, userId);
                        var newTeamsRouteId = CreateNewRoute(teamId, userId, routeId.Value);
                        if (rankId != null)
                        {
                            CreateNewRank(teamId, userId, newTeamsRouteId, rankId.Value);
                        }
                    }
                }
                else
                {
                    DeleteRank(teamId, userId);
                    DeleteRoute(teamId, userId);
                }
                errorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        private List<League> GetLeagueByUnion(int unionId, int seasonId)
        {
            return db.Leagues
                .Include(t => t.Gender)
                .Include(t => t.Age)
                .Where(t => t.IsArchive == false && t.UnionId == unionId && t.SeasonId == seasonId)
                .ToList();
        }

        private ICollection<League> GetLeagueByClub(int clubId, int seasonId)
        {
            return db.Leagues
                .Include(t => t.Gender)
                .Include(t => t.Age)
                .Where(t => t.IsArchive == false && t.ClubId == clubId && t.SeasonId == seasonId)
                .ToList();
        }


        public List<TennisRank> GetTennisRanks(int? ageId, int? seasonId)
        {
            return db.TennisRanks.Where(r => r.AgeId == ageId && seasonId == r.SeasonId).ToList();
        }


        public List<Club> GetClubsByCorrection(int leagueId, int? seasonId)
        {
            return db.CompetitionClubsCorrections
            .Include(x => x.Club)
            .Where(x => x.TypeId != null && x.ClubId > 0 && x.SeasonId == seasonId && x.LeagueId == leagueId)
            .Select(x => x.Club).Distinct()
            .ToList();
        }


        private List<TeamsPlayer> GetTeamPlayersByUnionIdShort(int unionId, int seasonId)
        {
            return db.TeamsPlayers
                .Include(x => x.User)
                .Include(x => x.Team)
                .Include(x => x.Team.LeagueTeams)
                .Where(tp => !tp.User.IsArchive &&
                             !tp.Team.IsArchive &&
                             tp.SeasonId == seasonId &&
                             tp.League.UnionId == unionId)
                .ToList();
        }

        private Club GetClubOfPlayer(int userId)
        {
            return db.TeamsPlayers.Include(x => x.Club).FirstOrDefault(x => x.ClubId != null && x.UserId == userId)?.Club;
        }

        private Club GetClubOfPlayerBySeasonId(int userId, int seasonId)
        {
            var teamPlayers = db.TeamsPlayers.Include(x => x.Club).Include(x => x.Club.ClubTeams).Where(x => x.ClubId != null && x.UserId == userId && x.Club.SeasonId == seasonId);
            var suitable = teamPlayers.FirstOrDefault(t => t.Club.ClubTeams.Where(c => c.IsTrainingTeam && c.TeamId == t.TeamId && c.SeasonId == seasonId && !c.IsBlocked).Count() > 0);
            if(suitable != null)
            {
                return suitable.Club;
            }else
                return teamPlayers.FirstOrDefault()?.Club;
        }


        private IEnumerable<Club> GetByTeamAndSeason(int teamId, int seasonId)
        {
            var clubsByTeam = db.Clubs.Where(c => !c.IsArchive && c.ClubTeams.Any(ct => ct.TeamId == teamId && ct.SeasonId == seasonId)).ToList();

            var clubsBySchool = db.SchoolTeams.Where(x => x.TeamId == teamId && x.School.SeasonId == seasonId && !x.School.Club.IsArchive).Select(x => x.School.Club).ToList();

            clubsByTeam.AddRange(clubsBySchool);

            return clubsByTeam;
        }

        private Section GetSectionByUnionId(int unionId)
        {
            var section = db.Unions.Include(x => x.Section).FirstOrDefault(x => x.UnionId == unionId);
            if (section != null)
                return section.Section;
            return new Section();
        }

        private bool CheckAllTennisGamesIsEnded(int stageId, int categoryId, bool isKnockoutOrPlayoff)
        {
            var stage = isKnockoutOrPlayoff
                ? db.TennisStages.AsEnumerable().LastOrDefault(t => !t.IsArchive && t.CategoryId == categoryId)
                : db.TennisStages.Include(x => x.TennisGameCycles).FirstOrDefault(x => x.StageId == stageId);

            if (stage != null)
            {
                return stage.TennisGameCycles.All(y => y.GameStatus == GameStatus.Ended);
            }
            return false;
        }

        private RankCategory UpdateRankCategory(int categoryId, int seasonId, bool isCategoryStading = false)
        {
            CategoryRankService svc = new CategoryRankService(categoryId);
            RankCategory rCategory = svc.CreateCategoryRankTable(seasonId, isCategoryStading);

            if (rCategory == null)
            {
                rCategory = new RankCategory();
            }
            else if (rCategory.Stages.Count == 0)
            {
                rCategory = svc.CreateEmptyRankTable(seasonId);
                rCategory.IsEmptyRankTable = true;

                if (rCategory.Stages.Count == 0)
                {
                    rCategory.Players = db.TeamsPlayers.Where(x => x.TeamId == categoryId && x.SeasonId == seasonId).ToList();
                }
            }


            return rCategory;
        }

        public List<UnionTennisRankDto> GetTennisUnionRanks(int? unionId, int? ageId, int? clubId, int seasonId, bool isUpdate = false)
        {
            var fromBirth = db.CompetitionAges.Where(x => x.id == ageId).FirstOrDefault().from_birth;
            var toBirth = db.CompetitionAges.Where(x => x.id == ageId).FirstOrDefault().to_birth;
            var genderId = db.CompetitionAges.Where(x => x.id == ageId).FirstOrDefault().gender;
            var season = db.Seasons.FirstOrDefault(x => x.Id == seasonId);

            var minimumParticipationRequired = 0;
            if (season != null)
            {
                if(season.MinimumParticipationRequired.HasValue && season.MinimumParticipationRequired.Value > 0)
                {
                    minimumParticipationRequired = season.MinimumParticipationRequired.Value;
                }
            }
            

            List<League> competitionList = new List<League>();
            List<UnionTennisRankDto> rankList = new List<UnionTennisRankDto>();

            if (clubId.HasValue)
            {
                competitionList = GetLeagueByClub(clubId.Value, seasonId)
                    .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                    .ToList();
            }
            else if (unionId.HasValue)
            {
                    competitionList = GetLeagueByUnion(unionId.Value, seasonId)
                        .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                        .ToList();
                
            }
            var previousRanks = GetTennisRanks(ageId, season.PreviousSeasonId).ToList();
            var newRanks = db.TennisCategoryPlayoffRanks.Where(r => r.SeasonId == seasonId && r.Team.CompetitionAgeId == ageId && (r.PlayerId.HasValue || r.PairPlayerId.HasValue)).ToList();

            foreach (var rank in previousRanks)
            {
                var playerAdded = rankList.FirstOrDefault(p => p.UserId == rank.UserId);
                if (playerAdded == null)
                {
                    UnionTennisRankDto item = new UnionTennisRankDto();
                    item.UserId = rank.UserId.Value;
                    item.Birthday = rank.User.BirthDay;
                    item.FullName = rank.User.FullName;
                    item.CategoryId = ageId.Value;
                    item.TotalPoints = 0;
                    item.AveragePoints = 0;
                    item.PointsToAverage = 0;
                    var club = GetClubOfPlayerBySeasonId(item.UserId, seasonId);
                    item.TrainingTeam = club == null ? string.Empty : club.Name;
                    item.CompetitionPoints = new List<int>();
                    item.CompetitionPoints.Add(rank.Points.Value);
                    rankList.Add(item);
                }
                else
                {
                    playerAdded.CompetitionPoints.Add(rank.Points.Value);
                }

            }

            foreach (var rank in newRanks)
            {
                var playerAdded = rankList.FirstOrDefault(p => p.UserId == rank.TeamsPlayer?.UserId || p.UserId == rank.TeamsPlayer1?.UserId);
                if (playerAdded == null)
                {
                    UnionTennisRankDto item = new UnionTennisRankDto();
                    item.UserId = rank.TeamsPlayer != null ? rank.TeamsPlayer.UserId : rank.TeamsPlayer1.UserId;
                    item.Birthday = rank.TeamsPlayer != null ? rank.TeamsPlayer?.User.BirthDay : rank.TeamsPlayer1?.User.BirthDay;
                    item.FullName = rank.TeamsPlayer != null ? rank.TeamsPlayer?.User.FullName : rank.TeamsPlayer1?.User.FullName;
                    item.TotalPoints = 0;
                    item.AveragePoints = 0;
                    item.CategoryId = ageId.Value;
                    item.PointsToAverage = 0;
                    var club = GetClubOfPlayerBySeasonId(item.UserId, seasonId);
                    item.TrainingTeam = club == null ? string.Empty : club.Name;
                    item.CompetitionPoints = new List<int>();
                    item.CompetitionPoints.Add(rank.Points.Value+rank.Correction.Value);
                    rankList.Add(item);
                }
                else
                {
                    playerAdded.CompetitionPoints.Add(rank.Points.Value + rank.Correction.Value);
                }
            }

            foreach (var rank in rankList)
            {
                rank.CompetitionPoints = rank.CompetitionPoints.OrderByDescending(x => x).ToList();
                rank.TotalPoints = rank.CompetitionPoints.Sum();
                rank.PointsToAverage = minimumParticipationRequired == 0 ? rank.CompetitionPoints.Sum() : rank.CompetitionPoints.GetRange(0, Math.Min(rank.CompetitionPoints.Count(), minimumParticipationRequired)).ToList().Sum();
                rank.AveragePoints = minimumParticipationRequired == 0 ? rank.CompetitionPoints.Sum() : (int)Math.Ceiling((double)rank.PointsToAverage / minimumParticipationRequired);
            }

            rankList = rankList.OrderByDescending(x => x.PointsToAverage).ThenByDescending(x=> x.TotalPoints).ToList();
            if (isUpdate)
            {
                for (int i = 0; i < rankList.Count(); i++)
                {
                    UpdateTennisPlayerRank(rankList.ElementAt(i), seasonId, i+1);
                }
                Save();
            }
            return rankList;
        }

        private void UpdateTennisPlayerRank(UnionTennisRankDto rank, int seasonId, int rankOrder)
        {
            var tennisRank = db.TennisRanks.FirstOrDefault(tr => tr.SeasonId == seasonId && tr.UserId == rank.UserId && tr.AgeId == rank.CategoryId);
            if(tennisRank == null)
            {
                db.TennisRanks.Add(new TennisRank {
                    SeasonId = seasonId,
                    UserId = rank.UserId,
                    Rank = rankOrder,
                    Points = rank.TotalPoints,
                    AgeId = rank.CategoryId,
                    AveragePoints = rank.AveragePoints,
                    PointsToAverage = rank.PointsToAverage
                });
            }
            else
            {
                tennisRank.Rank = rankOrder;
                tennisRank.Points = rank.TotalPoints;
                tennisRank.AveragePoints = rank.AveragePoints;
                tennisRank.PointsToAverage = rank.PointsToAverage;
            }
        }

        private int CreateNewRoute(int teamId, int userId, int routeId)
        {
            try
            {
                var disciplineId = db.DisciplineTeamRoutes.FirstOrDefault(d => d.Id == routeId)?.DisciplineId;
                var teamsRoutes = db.TeamsRoutes.Where(c => c.TeamId == teamId && c.UserId == userId);
                if (teamsRoutes.Any())
                {
                    foreach (var teamsRoute in teamsRoutes)
                    {
                        if (teamsRoute.DisciplineTeamRoute?.DisciplineId == disciplineId)
                        {
                            var teamsRank = teamsRoute?.TeamsRanks?.FirstOrDefault();
                            if (teamsRank != null)
                                db.TeamsRanks.Remove(teamsRank);
                            db.TeamsRoutes.Remove(teamsRoute);
                        }
                    }
                }

                db.TeamsRoutes.Add(new TeamsRoute { RouteId = routeId, TeamId = teamId, UserId = userId });
                db.SaveChanges();
                return db.TeamsRoutes.OrderByDescending(c => c.Id).FirstOrDefault().Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DeleteRoute(int teamId, int userId)
        {
            try
            {
                var routeDb = db.TeamsRoutes.FirstOrDefault(ur => ur.TeamId == teamId && ur.UserId == userId);
                if (routeDb != null)
                {
                    var teamsRank = routeDb?.TeamsRanks?.FirstOrDefault();
                    if (teamsRank != null)
                    {
                        db.TeamsRanks.Remove(teamsRank);
                    }
                    db.TeamsRoutes.Remove(routeDb);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void CreateNewRank(int teamId, int userId, int teamsRouteId, int rankId)
        {
            try
            {
                var teamsRank = db.TeamsRanks.FirstOrDefault(c => c.TeamsRouteId == teamsRouteId && c.UserId == userId);
                if (teamsRank == null)
                {
                    db.TeamsRanks.Add(new TeamsRank { RankId = rankId, TeamsRouteId = teamsRouteId, TeamId = teamId, UserId = userId });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DeleteRank(int teamId, int userId)
        {
            try
            {
                var rank = db.TeamsRanks.FirstOrDefault(c => c.TeamId == teamId && c.UserId == userId);
                if (rank != null)
                {
                    db.TeamsRanks.Remove(rank);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void AddPenalty(TeamPenalty penalty)
        {
            db.TeamPenalties.Add(penalty);

            db.SaveChanges();
        }
        
        public string GetCurrentTeamName(int teamId, int seasonId)
        {
            var team = GetById(teamId, seasonId);
            return team.TeamsDetails?.Where(td => td.SeasonId == seasonId)?.OrderByDescending(r => r.Id)?.FirstOrDefault()?.TeamName
                    ?? team.Title;
        }

        public void RemovePenalty(int penaltyId)
        {
            var penalty = db.TeamPenalties.Find(penaltyId);
            if (penalty != null)
            {
                db.Entry(penalty).State = EntityState.Deleted;

                db.SaveChanges();
            }
        }

        public int? GetSeasonIdByTeamId(int id, DateTime? date = null)
        {
            DateTime rDate = date ?? DateTime.Now;
            var league = GetLeagueByTeamId(id, rDate);
            return league?.SeasonId != null ? league.SeasonId : GetClubByTeamId(id, rDate)?.SeasonId;
        }

        public League GetLeagueByTeamId(int id, DateTime? date = null)
        {
            DateTime rDate = date ?? DateTime.Now;
            var currSeasons = currentSeasons(rDate).Select(s => s.Id).ToList();
            int? leagueId = db.LeagueTeams
                .Where(lt => lt.TeamId == id && currSeasons.Contains(lt.SeasonId ?? 0))
                .FirstOrDefault()?.LeagueId;
            if (leagueId.HasValue)
            {
                return db.Leagues.Where(l => l.LeagueId == leagueId).First();
            }
            else
            {
                leagueId = db.LeagueTeams
                .Where(lt => lt.TeamId == id)
                .FirstOrDefault()?.LeagueId;
                if (leagueId.HasValue)
                {
                    return db.Leagues.Where(l => l.LeagueId == leagueId).First();
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<Team> GetByCategoryAndSeason(int categoryId, int seasonId)
        {
            return db.Teams.Where(l => l.LeagueTeams.Any(lt => lt.TeamId == categoryId && lt.SeasonId == seasonId));
        }

        public IEnumerable<CompetitionAge> GetCompetitionAges(int unionId, int seasonId)
        {
            return db.CompetitionAges.OrderBy(t => t.id).Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList();
        }

        public IEnumerable<CompetitionRegion> GetCompetitionRegions(int unionId, int seasonId)
        {
            return db.CompetitionRegions.OrderBy(t => t.id).Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList();
        }

        public IEnumerable<CompetitionLevel> GetCompetitionLevels(int unionId, int seasonId)
        {
            return db.CompetitionLevels.OrderBy(t => t.id).Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList();
        }

        public int GetLeagueIdByTeamId(int id)
        {
            League lg = this.GetLeagueByTeamId(id);

            if (lg != null)
                return lg.LeagueId;
            lg = db.Leagues.First(x => x.IsArchive == false);
            return lg.LeagueId;
        }

        public ClubTeam GetClubByTeamId(int id, DateTime? date = null)
        {
            DateTime rDate = date ?? DateTime.Now;
            var currSeasons = currentSeasons(rDate).Select(s => s.Id).ToList();
            var currentClubTeam = db.ClubTeams
                .FirstOrDefault(ct => ct.TeamId == id && currSeasons.Contains(ct.SeasonId));
            if (currentClubTeam == null) {
                currentClubTeam = db.ClubTeams
                .FirstOrDefault(ct => ct.TeamId == id);
            }
            return currentClubTeam;
        }

        public IEnumerable<Team> GetTeams(int seasonId, int leagueId)
        {
            var teams =
                db.LeagueTeams
                    .Include(x => x.Teams)
                    .Include(x => x.Teams.TeamsDetails)
                    .Include(x => x.Teams.ActivityFormsSubmittedDatas)
                    .Where(t => t.SeasonId == seasonId && t.LeagueId == leagueId)
                    .Select(x => x.Teams)
                    .ToList();

            foreach (var team in teams)
            {
                var teamDetails = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
                if (teamDetails != null)
                {
                    team.Title = teamDetails.TeamName;
                }
            }
            return teams;
        }

        public IEnumerable<Team> GetAllTeams(int seasonId, int[] leagueIds)
        {
            var leagueTeams =
                db.LeagueTeams
                    .Include(x => x.Teams)
                    .Include(x => x.Teams.TeamsDetails)
                    .Include(x => x.Teams.ActivityFormsSubmittedDatas)
                    .Where(t => t.SeasonId == seasonId && leagueIds.Contains(t.LeagueId))
                    .ToList();
            leagueTeams.ForEach(lt => lt.Teams.RetrievedLeagueId = lt.LeagueId);
            var teams = leagueTeams.Select(x => x.Teams).ToList();

            foreach (var team in teams)
            {
                var teamDetails = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
                if (teamDetails != null)
                {
                    team.Title = teamDetails.TeamName;
                }
            }
            return teams;
        }

		public Dictionary<int, string> GetAllTeams() =>
			db.Teams.ToDictionary(p => p.TeamId, p => p.Title);


		public void ChangeTeamNamesForTheTennis(SchedulesDto resList)
        {
            if (resList != null)
            {
                foreach (var group in resList.BracketData)
                {
                    foreach (var stage in group.Stages)
                    {
                        foreach (var stageItem in stage.Items)
                        {
                            stageItem.HomeTeam = GetTeamNameWithoutLeagueName(stageItem.HomeTeam, stageItem.LeagueName);
                            stageItem.GuestTeam = GetTeamNameWithoutLeagueName(stageItem.GuestTeam, stageItem.LeagueName);
                        }
                    }
                }
            }
        }

        public void ChangeTeamNamesForTheTennis(IEnumerable<GamesCycleDto> resList)
        {
            if (resList != null)
            {
                foreach (var game in resList)
                {
                    game.HomeTeam = GetTeamNameWithoutLeagueName(game.HomeTeam, game.LeagueName);
                    game.GuestTeam = GetTeamNameWithoutLeagueName(game.GuestTeam, game.LeagueName);
                }
            }
        }



        public List<Team> GetTeamsByLeague(int leagueId)
        {
            return (from l in db.Leagues
                    from t in l.LeagueTeams
                    where t.Teams.IsArchive == false && l.LeagueId == leagueId
                    orderby t.Teams.Title
                    select t.Teams).Include(t => t.TeamsDetails).ToList();

        }

        public List<Team> GetTennisLeagueTeams(int leagueId, int seasonId)
        {
            return db.TeamRegistrations.Where(r => r.LeagueId == leagueId && !r.IsDeleted && r.SeasonId == seasonId && !r.Team.IsArchive)
                .Select(t => t.Team)?.ToList();
        }

        /// <summary>
        /// Select GroupTeams/Teams belonging to certain set of Leagues during certain season
        /// </summary>
        /// <param name="seasonId">int id of Season</param>
        /// <param name="leagueIds">int[] set of LeagueId</param>
        /// <returns>IDictionary<int, IList<TeamShortDTO>> dictionary where list of teams is returned by </returns>
        public IDictionary<int, IList<TeamShortDTO>> GetGroupTeamsBySeasonAndLeagues(int seasonId, IEnumerable<int> leagueIds, bool allLeagues = false)
        {
            IEnumerable<int> groupsIds = db.GamesCycles
                .Where(gc => allLeagues || leagueIds.Contains(gc.Stage.LeagueId) && !gc.Stage.League.IsArchive && gc.Stage.League.SeasonId == seasonId)
                .Where(gc => gc.GroupId != null && !gc.Group.IsArchive)
                .Select(gc => gc.GroupId ?? 0).Distinct();
            Dictionary<int, IList<TeamShortDTO>> result = new Dictionary<int, IList<TeamShortDTO>>();
            foreach (var id in groupsIds)
            {
                result[id] = new List<TeamShortDTO>();
            }
            var groupTeams = db.Teams
                .Join(db.LeagueTeams.Where(lt => lt.SeasonId == seasonId && leagueIds.Contains(lt.LeagueId)),
                        t => t.TeamId, lt => lt.TeamId, (t, lt) => t)
                .Join(db.GroupsTeams, t => t.TeamId, gt => gt.TeamId, (t, gt) => gt).Include(gt => gt.Team)
                .Where(gt => !gt.Group.IsArchive).Distinct();
            foreach (var gt in groupTeams)
            {
                if (!result.Keys.Contains(gt.GroupId))
                {
                    result[gt.GroupId] = new List<TeamShortDTO>();
                }
                var teamDetails = gt.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId);
                var teamName = teamDetails == null ? gt.Team.Title : teamDetails.TeamName;
                result[gt.GroupId].Add(new TeamShortDTO
                {
                    Pos = gt.Pos,
                    TeamId = gt.TeamId,
                    Title = teamName                    
                });
            }
            return result;
        }

        public IDictionary<int, IList<TeamShortDTO>> GetGroupTeamsBySeasonAndLeaguesForTennis(int seasonId, IEnumerable<int> leagueIds, bool allLeagues = false)
        {
            IEnumerable<int> groupsIds = db.GamesCycles
                .Where(gc => allLeagues || leagueIds.Contains(gc.Stage.LeagueId) && !gc.Stage.League.IsArchive && gc.Stage.League.SeasonId == seasonId)
                .Where(gc => gc.GroupId != null && !gc.Group.IsArchive)
                .Select(gc => gc.GroupId ?? 0).Distinct();
            Dictionary<int, IList<TeamShortDTO>> result = new Dictionary<int, IList<TeamShortDTO>>();
            foreach (var id in groupsIds)
            {
                result[id] = new List<TeamShortDTO>();
                var groupTeams = db.GroupsTeams.Where(t => !t.Group.IsArchive && t.GroupId == id);
                foreach (var gt in groupTeams)
                {
                    if (gt.TeamId.HasValue)
                    {
                        var teamDetails = gt.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId);
                        var teamName = teamDetails == null ? gt.Team.Title : teamDetails.TeamName;
                        result[gt.GroupId].Add(new TeamShortDTO
                        {
                            Pos = gt.Pos,
                            TeamId = gt.TeamId,
                            Title = teamName
                        });
                    }
                }
            }
            return result;
        }

        public GroupsTeam GetGroupTeam(int idGroup, int idTeam)
        {
            return db.GroupsTeams.FirstOrDefault(x => x.TeamId == idTeam && x.GroupId == idGroup);
        }

        public void Create(Team item)
        {
            item.CreateDate = DateTime.Now;
            db.Teams.Add(item);
            db.SaveChanges();
        }

        public void CreateBenefactor(TeamBenefactor item)
        {
            item.CreatedDate = DateTime.Now;
            db.TeamBenefactors.Add(item);
            db.SaveChanges();
        }

        public void AddTeamDetailToSeason(Team team, int season)
        {
            var teamDetail = new TeamsDetails
            {
                SeasonId = season,
                TeamId = team.TeamId,
                TeamName = team.Title
            };

            db.TeamsDetails.Add(teamDetail);
            db.SaveChanges();
        }

        public Team GetByName(string name)
        {
            return db.Teams.FirstOrDefault(t => t.Title == name);
        }

        public IEnumerable<Team> GetByPlayer(int playerId)
        {
            return db.TeamsPlayers.Where(t => t.UserId == playerId).Select(t => t.Team).ToList();
        }

        public League GetLeague(int id)
        {
            return db.Leagues.Find(id);
        }

        public Team GetById(int? id, int? seasonId = null)
        {
            if (id.HasValue)
            {
                var team = db.Teams
                    .Include(t => t.TeamsDetails)
                    .Include(x => x.LeagueTeams)
                    .Include(p => p.TeamBenefactors)
                    .FirstOrDefault(f => f.TeamId == id);
                if (team != null)
                {
                    var teamDetail = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
                    if (teamDetail != null)
                    {
                        team.Title = teamDetail.TeamName;
                    }
                    return team;
                }
            }
            return null;
        }

        public Team GetByIdForRanks(int categoryId)
        {
            return
                db.Teams
                    .Include(x => x.TennisGames)
                    .Include(x => x.TennisStages)
                    .Include(x => x.TennisStages.Select(t => t.TennisGroups))
                    .Include(x => x.TennisStages.Select(t => t.TennisGroups.Select(tg => tg.TennisGameCycles)))
                    .Include(x => x.TennisStages.Select(t => t.TennisGroups.Select(tg => tg.TennisGameCycles.Select(tgc => tgc.TennisGameSets))))
                    .FirstOrDefault(x => x.TeamId == categoryId);
        }

        public TeamBenefactor GetBenefactorByTeamId(int id)
        {
            return db.TeamBenefactors
                .Include(x => x.PlayersBenefactorPrices)
                .FirstOrDefault(p => p.TeamId == id);
        }

        public TeamScheduleScrapperGame GetScheduleScrapperById(int teamId, int clubId, int seasonId)
        {
            return db.TeamScheduleScrapperGames.FirstOrDefault(x => x.TeamId == teamId && x.ClubId == clubId && x.SeasonId == seasonId);

        }

        public List<TeamScheduleScrapper> GetTeamGamesFromScrapper(int clubId, int teamId)
        {
            return
                db.TeamScheduleScrapperGames.Where(x => x.ClubId == clubId && x.TeamId == teamId)
                    .SelectMany(x => x.TeamScheduleScrappers)
                    .ToList();
        }

        public Team GetTeamByTeamSeasonId(int teamId, int season)
        {
            var team = db.Teams.Find(teamId);
            if (team != null)
            {
                var teamDetails = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == season);
                if (teamDetails != null)
                {
                    team.Title = teamDetails.TeamName;
                }
                return team;
            }
            return null;
        }

        public List<TeamsPlayer> GetCategoryPlayers(int categoryId, int seasonId)
        {
            return db.TeamsPlayers.Where(t => t.TeamId == categoryId && t.SeasonId == seasonId)?.ToList();
        }

        public void RemoveTeamDetails(Team team, int seasonId)
        {
            var details = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
            if (details != null)
            {
                team.TeamsDetails.Remove(details);
            }
        }

        public LeagueTeams GetLeagueTeam(int teamId, int? leagueId, int seasonId)
        {
            var leagueTeam =
                db.LeagueTeams.Include(x => x.Teams)
                    .Include(t => t.Teams.TeamsDetails)
                    .FirstOrDefault(x => x.TeamId == teamId && x.LeagueId == leagueId && seasonId == x.SeasonId);
            if (leagueTeam != null)
            {
                var teamDetails = leagueTeam.Teams.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
                if (teamDetails != null)
                {
                    leagueTeam.Teams.Title = teamDetails.TeamName;
                    return leagueTeam;
                }
                return leagueTeam;
            }
            return null;
        }

        public IEnumerable<ListItemDto> FindByNameAndSection(string name, int? sectionId, int num, int? seasonId, int? clubId = null, bool isDepartment = false)
        {
            var filteredTeams = !isDepartment
                ? db.Teams.Include(t => t.TeamsDetails)
                    .Where(t => t.IsArchive == false && (t.Title.Contains(name) || t.TeamsDetails.Any(td =>
                                                             td.TeamName.Contains(name)
                                                             && td.SeasonId == seasonId)))

                : db.Teams.Include(t => t.TeamsDetails).Where(t => t.IsArchive == false
                                                                   && (t.Title.Contains(name) ||
                                                                       t.TeamsDetails.Any(td =>
                                                                           td.TeamName.Contains(name) && td.SeasonId == seasonId)));

            if (sectionId.HasValue)
            {
                var unionSectionTeams = filteredTeams.Where(t => t.ClubTeams.Any(ct => ct.Club.UnionId != null && ct.Club.Union.SectionId == sectionId));
                var sectionTeams = filteredTeams.Where(t => t.ClubTeams.Any(ct => ct.Club.UnionId == null && ct.Club.SectionId == sectionId));
                var schoolTeams = filteredTeams.Where(t => t.SchoolTeams.Any(st => st.School.Club.UnionId == null && st.School.Club.SectionId == sectionId));
                var leagueTeams = filteredTeams.Where(t => t.LeagueTeams.Any(lt => lt.Leagues.Union.SectionId == sectionId && (seasonId == null || lt.SeasonId == seasonId)));

                filteredTeams = unionSectionTeams.Union(sectionTeams).Union(schoolTeams).Union(leagueTeams);
            }
            if (clubId.HasValue)
            {
                var clubTeams = db.ClubTeams.Where(c => c.ClubId == clubId && c.SeasonId == seasonId).Select(t => t.TeamId);
                var schoolTeams = db.SchoolTeams.Where(c => c.School.ClubId == clubId).Select(t => t.TeamId);
                var filteredClubTeams = filteredTeams.Where(t => clubTeams.Contains(t.TeamId));
                var filteredSchoolTeams = filteredTeams.Where(t => schoolTeams.Contains(t.TeamId));
                filteredTeams = filteredClubTeams.Union(filteredSchoolTeams);
            }

            var teams = filteredTeams
                .OrderBy(t => t.Title)
                .Take(num)
                .ToList();

            var dtos = new List<ListItemDto>();
            if (teams.Count > 0)
            {
                if (!isDepartment)
                {
                    foreach (var team in teams)
                    {
                        var teamName = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)?.TeamName ??
                                       team.Title;

                        foreach (var clubTeam in team.ClubTeams.Where(x => seasonId == null || x.SeasonId == seasonId))
                        {
                            dtos.Add(new ListItemDto
                            {
                                Id = team.TeamId,
                                SeasonId = clubTeam.SeasonId,
                                ClubId = clubTeam.ClubId,
                                Name = $"{teamName} - {clubTeam.Club.Name} - {clubTeam.Season.Name}"
                            });
                        }
                        foreach (var schoolTeam in team.SchoolTeams.Where(x => seasonId == null || x.School.SeasonId == seasonId))
                        {
                            dtos.Add(new ListItemDto
                            {
                                Id = team.TeamId,
                                SeasonId = schoolTeam.School.SeasonId,
                                ClubId = schoolTeam.School.ClubId,
                                Name = $"{teamName} - {schoolTeam.School.Club.Name} - {schoolTeam.School.Season.Name}"
                            });
                        }
                        foreach (var leagueTeam in team.LeagueTeams.Where(x => seasonId == null || x.SeasonId == seasonId))
                        {
                            dtos.Add(new ListItemDto
                            {
                                Id = team.TeamId,
                                SeasonId = leagueTeam.SeasonId,
                                LeagueId = leagueTeam.LeagueId,
                                Name = $"{teamName} - {leagueTeam.Leagues.Name} - {leagueTeam.Season.Name}"
                            });
                        }
                    }
                }
                else
                {
                    dtos.AddRange(teams.Select(team => new ListItemDto
                    {
                        Id = team.TeamId,
                        Name = CreateTeamTitle(team, team.ClubTeams.OrderByDescending(c => c.SeasonId).FirstOrDefault()?.SeasonId),
                        TeamSeasonId = team.ClubTeams.OrderByDescending(c => c.SeasonId).FirstOrDefault()?.SeasonId
                    }));
                }

            }
            return dtos;
        }

        public IEnumerable<ListItemDto> FindByDepartmentId(string name, int num, int? departmentId, int? seasonId)
        {
            var filteredTeams = db.Teams.Include(t => t.TeamsDetails)
                .Where(t => t.IsArchive == false
                            && (t.Title.Contains(name)
                                || t.TeamsDetails.Any(td => td.TeamName.Contains(name) && td.SeasonId == seasonId))
                );
            if (departmentId.HasValue)
            {
                var clubTeams = db.ClubTeams.Where(c => c.ClubId == departmentId && c.SeasonId == seasonId)
                    .Select(t => t.TeamId);
                filteredTeams = filteredTeams.Where(t => clubTeams.Contains(t.TeamId));
            }

            var teams = filteredTeams
                .OrderBy(t => t.Title)
                .Take(num).ToList();

            var dtos = new List<ListItemDto>();
            if (teams.Count > 0)
            {
                dtos.AddRange(teams.Select(team => new ListItemDto
                {
                    Id = team.TeamId,
                    Name = CreateTeamTitle(team, seasonId)
                }));
            }

            return dtos;
        }

        private string CreateTeamTitle(Team team, int? seasonId)
        {
            var leagueTitles = team.LeagueTeams.Where(x => seasonId == null || x.SeasonId == seasonId)
                .Select(lt => $"{lt.Leagues.Name} - {lt.Season.Name}")
                .ToList();

            var clubTitles = team.ClubTeams.Where(x => seasonId == null || x.SeasonId == seasonId)
                .Select(ct => $"{ct.Club.Name} - {ct.Season.Name}")
                .ToArray();

            var groupTitles = team.GroupsTeams.Where(x => seasonId == null || x.SeasonId == seasonId)
                .Select(gt => $"{gt.Group.Name} - {gt.Group.Stage.League.Name} - {gt.Group.Stage.League.Season.Name}")
                .ToArray();

            var schoolTitles = team.SchoolTeams.Where(x => seasonId == null || x.School.SeasonId == seasonId)
                .Select(st => $"{st.School.Name} - {st.School.Season.Name}")
                .ToArray();

            leagueTitles.AddRange(clubTitles);
            leagueTitles.AddRange(groupTitles);
            leagueTitles.AddRange(schoolTitles);

            var teamName =
                team.TeamsDetails.OrderBy(x => x.SeasonId)
                    .LastOrDefault(x => seasonId == null || x.SeasonId == seasonId)?.TeamName ?? team.Title;

            return $"{teamName} ({string.Join(", ", leagueTitles)})";
        }

        public IEnumerable<ListItemDto> FindInUnionByLeague(int leagueId, string name, int num)
        {
            int? unionId = db.Leagues
                .Where(t => t.LeagueId == leagueId && t.IsArchive == false)
                .Select(t => t.UnionId).First();

            return (from u in db.Unions
                    from le in u.Leagues
                    from t in le.LeagueTeams
                    where le.IsArchive == false && t.Teams.IsArchive == false
                          && u.UnionId == unionId
                          && t.Teams.Title.Contains(name)
                    orderby t.Teams.Title
                    select new ListItemDto { Id = t.TeamId, Name = t.Teams.Title }).Distinct().ToList();
        }

        public List<Team> GetByManagerId(int managerId, int? seasonId = null)
        {
            var teams = db.UsersJobs
                .Include(t => t.Team.TeamsDetails)
                .Include(t => t.Team.ActivityFormsSubmittedDatas)
                .Where(j => j.UserId == managerId)
                .Select(j => j.Team)
                .Where(l => l != null && l.IsArchive == false)
                .Distinct()
                .OrderBy(u => u.Title)
                .ToList();

            foreach (var team in teams)
            {
                var teamDetail = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
                if (teamDetail != null)
                {
                    team.Title = teamDetail.TeamName;
                }
            }

            return teams;
        }

        public List<TeamManagerTeamInformationDto> GetByTeamManagerId(int managerId)
        {
            var teamManagerTeamsInformation = (from userJob in db.UsersJobs.Where(x => x.Season.IsActive)
                                               join team in db.Teams on userJob.TeamId equals team.TeamId
                                               where userJob.UserId == managerId && team.IsArchive == false

                                               let teamsDetails =
                                                   team.TeamsDetails.FirstOrDefault(t => t.SeasonId == userJob.SeasonId.Value && t.Season.IsActive)
                                               let league = team.LeagueTeams.FirstOrDefault(x =>
                                                   x.TeamId == team.TeamId && x.SeasonId == userJob.SeasonId.Value && x.Season.IsActive)
                                               let club = team.ClubTeams.FirstOrDefault(x =>
                                                   x.TeamId == team.TeamId && x.SeasonId == userJob.SeasonId.Value && x.Season.IsActive)
                                               let schoolClub = team.SchoolTeams.FirstOrDefault(x =>
                                                       x.TeamId == team.TeamId && x.School.SeasonId == userJob.SeasonId.Value &&
                                                       x.School.Season.IsActive)
                                                   .School.Club
                                               select new TeamManagerTeamInformationDto
                                               {
                                                   LeagueId = league.LeagueId,
                                                   LeagueName = league != null ? league.Leagues.Name : string.Empty,
                                                   ClubId = userJob.ClubId ?? (club != null ? club.ClubId : schoolClub.ClubId),
                                                   SeasonId = userJob.SeasonId,
                                                   TeamId = userJob.TeamId,
                                                   Title = teamsDetails != null ? teamsDetails.TeamName : team.Title,
                                                   UnionId = userJob.UnionId
                                               }).ToList();
            if (!teamManagerTeamsInformation.Any())
            {
                teamManagerTeamsInformation = (from userJob in db.UsersJobs
                                               join seasons in db.Seasons on userJob.SeasonId equals seasons.Id
                                               join clubteams in db.ClubTeams on userJob.TeamId equals clubteams.TeamId
                                               //join team in db.Teams on clubteams.TeamId equals team.TeamId
                                               where userJob.UserId == managerId && clubteams.Team.IsArchive == false && seasons.IsActive

                                               let teamsDetails =
                                                   clubteams.Team.TeamsDetails.FirstOrDefault(t => t.SeasonId == userJob.SeasonId.Value && t.Season.IsActive)
                                               let league = clubteams.Team.LeagueTeams.FirstOrDefault(x =>
                                                   x.TeamId == clubteams.Team.TeamId && x.SeasonId == userJob.SeasonId.Value && x.Season.IsActive)
                                               let club = clubteams.Team.ClubTeams.FirstOrDefault(x =>
                                                   x.TeamId == clubteams.Team.TeamId && x.SeasonId == userJob.SeasonId.Value && x.Season.IsActive)
                                               let schoolClub = clubteams.Team.SchoolTeams.FirstOrDefault(x =>
                                                       x.TeamId == clubteams.Team.TeamId && x.School.SeasonId == userJob.SeasonId.Value &&
                                                       x.School.Season.IsActive)
                                                   .School.Club
                                               select new TeamManagerTeamInformationDto
                                               {
                                                   LeagueId = league.LeagueId,
                                                   LeagueName = league != null ? league.Leagues.Name : string.Empty,
                                                   ClubId = clubteams != null?clubteams.ClubId:(club != null ? club.ClubId : schoolClub.ClubId),
                                                   SeasonId = userJob.SeasonId,
                                                   TeamId = userJob.TeamId,
                                                   Title = teamsDetails != null ? teamsDetails.TeamName : clubteams.Team.Title,
                                                   UnionId = userJob.UnionId
                                               }).ToList();
            }
            if (!teamManagerTeamsInformation.Any())
            {
                teamManagerTeamsInformation = (from userJob in db.UsersJobs
                                               join seasons in db.Seasons on userJob.SeasonId equals seasons.Id
                                               join clubteams in db.ClubTeams on userJob.ClubId equals clubteams.ClubId
                                               //join team in db.Teams on clubteams.TeamId equals team.TeamId
                                               where userJob.UserId == managerId && clubteams.Team.IsArchive == false && seasons.IsActive&&clubteams.SeasonId == userJob.SeasonId

                                               let teamsDetails =
                                                   clubteams.Team.TeamsDetails.FirstOrDefault(t => t.SeasonId == userJob.SeasonId.Value && t.Season.IsActive)
                                               let league = clubteams.Team.LeagueTeams.FirstOrDefault(x =>
                                                   x.TeamId == clubteams.Team.TeamId && x.SeasonId == userJob.SeasonId.Value && x.Season.IsActive)
                                               let club = clubteams.Team.ClubTeams.FirstOrDefault(x =>
                                                   x.TeamId == clubteams.Team.TeamId && x.SeasonId == userJob.SeasonId.Value && x.Season.IsActive)
                                               let schoolClub = clubteams.Team.SchoolTeams.FirstOrDefault(x =>
                                                       x.TeamId == clubteams.Team.TeamId && x.School.SeasonId == userJob.SeasonId.Value &&
                                                       x.School.Season.IsActive)
                                                   .School.Club
                                               select new TeamManagerTeamInformationDto
                                               {
                                                   LeagueId = league.LeagueId,
                                                   LeagueName = league != null ? league.Leagues.Name : string.Empty,
                                                   ClubId = clubteams != null ? clubteams.ClubId : (club != null ? club.ClubId : schoolClub.ClubId),
                                                   SeasonId = userJob.SeasonId,
                                                   TeamId = userJob.TeamId,
                                                   Title = teamsDetails != null ? teamsDetails.TeamName : clubteams.Team.Title,
                                                   UnionId = userJob.UnionId
                                               }).ToList();
            }
            if (!teamManagerTeamsInformation.Any())
            {
                teamManagerTeamsInformation = (from userJob in db.UsersJobs
                                               join seasons in db.Seasons on userJob.SeasonId equals seasons.Id
                                               join leagueteams in db.LeagueTeams on userJob.TeamId equals leagueteams.TeamId
                                               join team in db.Teams on leagueteams.TeamId equals team.TeamId
                                               where userJob.UserId == managerId && team.IsArchive == false && seasons.IsActive

                                               let teamsDetails =
                                                   team.TeamsDetails.FirstOrDefault(t => t.SeasonId == userJob.SeasonId.Value && t.Season.IsActive)
                                               let league = team.LeagueTeams.FirstOrDefault(x =>
                                                   x.TeamId == team.TeamId && x.SeasonId == userJob.SeasonId.Value && x.Season.IsActive)
                                               let club = team.ClubTeams.FirstOrDefault(x =>
                                                   x.TeamId == team.TeamId && x.SeasonId == userJob.SeasonId.Value && x.Season.IsActive)
                                               let schoolClub = team.SchoolTeams.FirstOrDefault(x =>
                                                       x.TeamId == team.TeamId && x.School.SeasonId == userJob.SeasonId.Value &&
                                                       x.School.Season.IsActive)
                                                   .School.Club
                                               select new TeamManagerTeamInformationDto
                                               {
                                                   LeagueId = league.LeagueId,
                                                   LeagueName = league != null ? league.Leagues.Name : string.Empty,
                                                   ClubId = userJob.ClubId ?? (club != null ? club.ClubId : schoolClub.ClubId),
                                                   SeasonId = userJob.SeasonId,
                                                   TeamId = userJob.TeamId,
                                                   Title = teamsDetails != null ? teamsDetails.TeamName : team.Title,
                                                   UnionId = userJob.UnionId
                                               }).ToList();
            }
            if (!teamManagerTeamsInformation.Any())
            {
                teamManagerTeamsInformation = (from userJob in db.UsersJobs
                                               join seasons in db.Seasons on userJob.SeasonId equals seasons.Id
                                               join leagueteams in db.LeagueTeams on userJob.LeagueId equals leagueteams.LeagueId
                                               join team in db.Teams on leagueteams.TeamId equals team.TeamId
                                               where userJob.UserId == managerId && team.IsArchive == false && seasons.IsActive

                                               let teamsDetails =
                                                   team.TeamsDetails.FirstOrDefault(t => t.SeasonId == userJob.SeasonId.Value && t.Season.IsActive)
                                               let league = team.LeagueTeams.FirstOrDefault(x =>
                                                   x.TeamId == team.TeamId && x.SeasonId == userJob.SeasonId.Value && x.Season.IsActive)
                                               let club = team.ClubTeams.FirstOrDefault(x =>
                                                   x.TeamId == team.TeamId && x.SeasonId == userJob.SeasonId.Value && x.Season.IsActive)
                                               let schoolClub = team.SchoolTeams.FirstOrDefault(x =>
                                                       x.TeamId == team.TeamId && x.School.SeasonId == userJob.SeasonId.Value &&
                                                       x.School.Season.IsActive)
                                                   .School.Club
                                               select new TeamManagerTeamInformationDto
                                               {
                                                   LeagueId = league.LeagueId,
                                                   LeagueName = league != null ? league.Leagues.Name : string.Empty,
                                                   ClubId = userJob.ClubId ?? (club != null ? club.ClubId : schoolClub.ClubId),
                                                   SeasonId = userJob.SeasonId,
                                                   TeamId = userJob.TeamId,
                                                   Title = teamsDetails != null ? teamsDetails.TeamName : team.Title,
                                                   UnionId = userJob.UnionId
                                               }).ToList();
            }
            return teamManagerTeamsInformation;
        }

        public int? GetMainOrFirstAuditoriumForTeam(int? teamId)
        {
            if (teamId.HasValue)
            {
                var team = GetById(teamId);
                var auditorium = team.TeamsAuditoriums.FirstOrDefault(x => x.IsMain) ?? team.TeamsAuditoriums.FirstOrDefault();

                if (auditorium != null)
                {
                    return auditorium.AuditoriumId;
                }
            }
            return null;
        }

        public TennisStage GetTennisStage(int? stageId)
        {
            var tennisStage = db.TennisStages.Where(x => x.StageId == stageId).FirstOrDefault();
            return tennisStage;
        }

        public int GetTeamsUnion(int teamId)
        {
            return (from u in db.Unions
                    from le in u.Leagues
                    from t in le.LeagueTeams
                    where t.TeamId == teamId
                    select u.UnionId).FirstOrDefault();
        }

        public List<Team> GetTeamsByIds(int?[] ids)
        {
            List<Team> list = new List<Team>();
            foreach (int? id in ids)
            {
                list.Add(id.HasValue ? db.Teams.Find(id) : null);
            }
            return list;
        }

        public List<Team> GetTeamsByIds(int[] ids)
        {
            var newIds = new int?[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                newIds[i] = ids[i];
            }
            return GetTeamsByIds(newIds);
        }

        public IQueryable<IGrouping<League, IGrouping<Stage, GamesCycle>>> GetTeamGames(int id, int seasonId, List<LeagueShort> teamLeagues, out HashSet<int> referees, bool isTennis = false)
        {
            var leagueIds = teamLeagues.Select(x => x.Id).ToList();

            var gamesCycles = db.GamesCycles
                .Where(game => (game.HomeTeamId == id || game.GuestTeamId == id) &&
                               game.IsPublished &&
                               ( isTennis || (
                               game.HomeTeam.LeagueTeams.Any(
                                   t => t.SeasonId == seasonId && leagueIds.Contains(t.LeagueId)) &&
                               game.GuestTeam.LeagueTeams.Any(
                                   t => t.SeasonId == seasonId && leagueIds.Contains(t.LeagueId))
                               )) &&
                               leagueIds.Contains(game.Stage.LeagueId)
                );

            referees = new HashSet<int>();
            foreach (var gamesCycle in gamesCycles)
            {
                var gameReferees = gamesCycle.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (gameReferees == null) continue;
                foreach (var referee in gameReferees)
                {
                    referees.Add(int.Parse(referee));
                }
            }

            IQueryable<IGrouping<League, IGrouping<Stage, GamesCycle>>> leagues =
                gamesCycles.GroupBy(game => game.Stage)
                .Where(g => g.Key.IsArchive == false)
                .GroupBy(s => s.Key.League)
                .Where(l => l.Key.IsArchive == false);
            return leagues;
        }

        public void UpdateCategoryPlaceDates(CategoriesPlaceDate categoriesPlace, DateTime? qualificationStartDate, DateTime? qualificationEndDate,
            DateTime? finalStartDate, DateTime? finalEndDate)
        {
            categoriesPlace.QualificationStartDate = qualificationStartDate;
            categoriesPlace.QualificationEndDate = qualificationEndDate;
            categoriesPlace.FinalStartDate = finalStartDate;
            categoriesPlace.FinalEndDate = finalEndDate;
        }

        public CategoriesPlaceDate CreateCategoryPlaceDate(int teamId)
        {
            db.CategoriesPlaceDates.Add(new CategoriesPlaceDate { TeamId = teamId });
            db.SaveChanges();
            return db.CategoriesPlaceDates.OrderByDescending(p => p.Id)?.FirstOrDefault();
        }

        public IEnumerable<GameDto> GetTeamGames(int teamId)
        {
            var sectionAlias = GetSectionByTeamId(teamId)?.Alias;
            bool isPenaltySection = string.Equals(sectionAlias, GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(sectionAlias, GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(sectionAlias, GamesAlias.Rugby, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(sectionAlias, GamesAlias.Handball, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(sectionAlias, GamesAlias.BasketBall, StringComparison.OrdinalIgnoreCase);
            var gamesDb = db.GamesCycles.Where(g => g.HomeTeamId == teamId).ToList();
            if (gamesDb == null)
                return null;
            var gamesDtos = new List<GameDto>();
            foreach (var game in gamesDb)
            {
                var penalty = game.GameSets.FirstOrDefault(c => c.IsPenalties);
                var homeTeamTitle = game.HomeTeam == null ? "" : game.HomeTeam.Title;
                var guestTeamTitle = game.GuestTeam == null ? "" : game.GuestTeam.Title;
                gamesDtos.Add(new GameDto
                {
                    StartDate = game.StartDate,
                    Auditorium = game.Auditorium?.Name,
                    AuditoriumAddress = game.Auditorium?.Address,
                    HomeTeamDetail = new TeamDetailsDto
                    {
                        TeamId = game.HomeTeamId,
                        TeamName = homeTeamTitle,
                        TeamScore = (penalty != null || !isPenaltySection) ? game.HomeTeamScore : game.GameSets.Where(c => !c.IsPenalties).Sum(x => x.HomeTeamScore)
                    },
                    GuestTeamDetail = new TeamDetailsDto
                    {
                        TeamId = game.GuestTeamId,
                        TeamName = guestTeamTitle,
                        TeamScore = (penalty != null || !isPenaltySection) ? game.GuestTeamScore : game.GameSets.Where(c => !c.IsPenalties).Sum(x => x.GuestTeamScore)
                    },
                    GameCycleStatus = game.GameStatus ?? "",
                    StageId = game.StageId
                });
            }
            return gamesDtos;
        }


        public Tuple<int, int> GetByMostFans(int leagueId)
        {

            var bestTeam = db.LeagueTeams.Where(lt => lt.LeagueId == leagueId)
                .Join(db.TeamsFans, lt => lt.TeamId, tf => tf.TeamId, (lt, tf) => tf)
                .Select(tf => new { tf.TeamId, tf.UserId }).Distinct()
                .GroupBy(tf => tf.TeamId).Select(g => new { TeamId = g.Key, FansCount = g.Count() })
                .OrderByDescending(g => g.FansCount).FirstOrDefault();

            return bestTeam == null ? null : Tuple.Create(bestTeam.TeamId, bestTeam.FansCount);
        }

        public IList<TeamDto> GetFanTeamsByClub(int fanId, int? seasonId)
        {
            var now = DateTime.Now;
            //SeasonsRepo _seasonRepo = new SeasonsRepo();
            List<TeamDto> fanTeams = new List<TeamDto>();
            var fanTeamsInformation = (from l in db.Clubs
                                       from t in db.ClubTeams
                                       from tp in db.TeamsFans
                                       where (l.IsArchive == false && t.IsBlocked == false &&
                                           tp.UserId == fanId && t.TeamId == tp.TeamId && l.ClubId == t.ClubId)
                                       let teamDetails = (from detail in db.TeamsDetails
                                                          from teams in db.Teams
                                                          from season in db.Seasons
                                                          where (detail.SeasonId == t.SeasonId  && tp.TeamId == teams.TeamId 
                                                          && detail.TeamId == tp.TeamId)
                                                          select new TeamDetailsDto
                                                          {
                                                              TeamId = detail.TeamId,
                                                              SeasonId = detail.SeasonId,
                                                              TeamName = detail.TeamName
                                                          }).FirstOrDefault()
                                       select new TeamInformationDto()
                                       {
                                           Team = new TeamDto()
                                           {
                                               LeagueId = 0,
                                               ClubId = l.ClubId,
                                               TeamId = t.TeamId,
                                               Logo = (from teamclub in db.Teams//t.Logo,
                                                       where (teamclub.TeamId == t.TeamId)
                                                       select new
                                                       {
                                                           img = teamclub.Logo
                                                       }).FirstOrDefault().img,
                                               Title = (from teamclub in db.Teams//t.Logo,
                                                        where (teamclub.TeamId == t.TeamId)
                                                        select new
                                                        {
                                                            title = teamclub.Title
                                                        }).FirstOrDefault().title,
                                               SeasonId = t.SeasonId
                                           },
                                           TeamInformation = teamDetails != null ?
                                                         new TeamDetailsDto()
                                                         {
                                                             TeamId = teamDetails.TeamId,
                                                             SeasonId = teamDetails.SeasonId,
                                                             TeamName = teamDetails.TeamName
                                                         } : null
                                       }).ToList().Concat(from l in db.Leagues
                                                          from t in db.Teams
                                                          from tp in db.TeamsFans
                                                          where l.IsArchive == false && t.IsArchive == false &&
                                                                tp.UserId == fanId && tp.TeamId == t.TeamId && tp.LeageId == l.LeagueId

                                                          let teamDetails = t.TeamsDetails.FirstOrDefault(x=>x.SeasonId==l.SeasonId)
                                                          select new TeamInformationDto()
                                                          {
                                                              Team = new TeamDto()
                                                              {
                                                                  LeagueId = l.LeagueId,
                                                                  TeamId = t.TeamId,
                                                                  ClubId = 0,
                                                                  Logo = t.Logo,
                                                                  Title = t.Title,
                                                                  SeasonId = l.SeasonId
                                                              },
                                                              TeamInformation = teamDetails != null ?
                                                                            new TeamDetailsDto()
                                                                            {
                                                                                TeamId = teamDetails.TeamId,
                                                                                SeasonId = teamDetails.SeasonId,
                                                                                TeamName = teamDetails.TeamName
                                                                            } : null
                                                          })
                            .ToList();

            if (seasonId.HasValue)
            {//Where(x => x.Team.SeasonId != null && _seasonRepo.isNowAllowSeasonId(x.Team.SeasonId.Value) == true)
                fanTeams = (from info in fanTeamsInformation
                           from season in db.Seasons
                           where (info.Team.SeasonId == info.Team.SeasonId)
                           select new TeamDto()
                            {
                                TeamId = info.Team.TeamId,
                                Title = info.TeamInformation != null && info.TeamInformation.SeasonId == info.Team.SeasonId ?
                                         info.TeamInformation.TeamName : info.Team.Title,
                                LeagueId = info.Team.LeagueId,
                                ClubId = info.Team.ClubId,
                                Logo = info.Team.Logo
                            })
                            .ToList();

                fanTeams = fanTeams.GroupBy(x => x.TeamId).Select(g => g.First()).ToList();
            }
            else
            {
                fanTeams = fanTeamsInformation
                    .Select(t => new TeamDto()
                    {
                        TeamId = t.Team.TeamId,
                        Title = t.TeamInformation != null  ?
                                t.TeamInformation.TeamName : t.Team.Title,
                        LeagueId = t.Team.LeagueId,
                        ClubId = t.Team.ClubId,
                        Logo = t.Team.Logo
                    })
                    .GroupBy(x => x.TeamId).Select(g => g.First()).ToList();
            }

            for (int i = 0; i < fanTeams.Count; i++)
            {
                int teamId = fanTeams[i].TeamId;
                var schoolTeam = db.SchoolTeams.Where(st => st.TeamId == teamId).FirstOrDefault();
                if (schoolTeam != null)
                {
                    fanTeams[i].IsSchoolTeam = true;
                }
                else
                {
                    fanTeams[i].IsSchoolTeam = false;
                }
            }

            return fanTeams;
        }
        public IList<TeamDto> GetFanTeams(int fanId, int? seasonId)
        {
            List<TeamDto> fanTeams = new List<TeamDto>();
            var fanTeamsInformation = (from l in db.Leagues
                                       from t in l.LeagueTeams
                                       from tp in t.Teams.TeamsFans
                                       where l.IsArchive == false && t.Teams.IsArchive == false &&
                                             tp.UserId == fanId

                                       let teamDetails = t.Teams.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)
                                       select new TeamInformationDto()
                                       {
                                           Team = new TeamDto()
                                           {
                                               LeagueId = l.LeagueId,
                                               TeamId = t.TeamId,
                                               Logo = t.Teams.Logo,
                                               Title = t.Teams.Title,
                                               SeasonId = t.SeasonId
                                           },
                                           TeamInformation = teamDetails != null ?
                                                         new TeamDetailsDto()
                                                         {
                                                             TeamId = teamDetails.TeamId,
                                                             SeasonId = teamDetails.SeasonId,
                                                             TeamName = teamDetails.TeamName
                                                         } : null
                                       })
                            .ToList();

            if (seasonId.HasValue)
            {
                fanTeams = fanTeamsInformation.Where(x => x.Team.SeasonId != null && x.Team.SeasonId.Value == seasonId.Value)
                    .Select(t => new TeamDto()
                    {
                        TeamId = t.Team.TeamId,
                        Title = t.TeamInformation != null && t.TeamInformation.SeasonId == seasonId ?
                                t.TeamInformation.TeamName : t.Team.Title,
                        LeagueId = t.Team.LeagueId,
                        Logo = t.Team.Logo
                    })
                    .ToList();

                fanTeams = fanTeams.GroupBy(x => x.TeamId).Select(g => g.First()).ToList();
            }
            else
            {
                fanTeams = fanTeamsInformation
                    .Select(t => new TeamDto()
                    {
                        TeamId = t.Team.TeamId,
                        Title = t.TeamInformation != null && t.TeamInformation.SeasonId == seasonId ?
                                t.TeamInformation.TeamName : t.Team.Title,
                        LeagueId = t.Team.LeagueId,
                        Logo = t.Team.Logo
                    })
                    .GroupBy(x => x.TeamId).Select(g => g.First()).ToList();
            }

            for (int i = 0; i < fanTeams.Count; i++)
            {
                int teamId = fanTeams[i].TeamId;
                var schoolTeam = db.SchoolTeams.Where(st => st.TeamId == teamId).FirstOrDefault();
                if (schoolTeam != null)
                {
                    fanTeams[i].IsSchoolTeam = true;
                }
                else
                {
                    fanTeams[i].IsSchoolTeam = false;
                }
            }

            return fanTeams;
        }
        public IList<TeamDto> GetPlayerTeams(int playerId, int? seasonId = null)
        {
            List<TeamDto> playerTeams;
            var playerTeamsInformation = (from l in db.Leagues
                                          from t in l.LeagueTeams
                                          from tp in t.Teams.TeamsPlayers
                                          where l.IsArchive == false && t.Teams.IsArchive == false &&
                                                tp.UserId == playerId

                                          let teamDetails = t.Teams.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId.Value)
                                          select new TeamInformationDto()
                                          {
                                              Team = new TeamDto()
                                              {
                                                  LeagueId = l.LeagueId,
                                                  TeamId = t.TeamId,
                                                  Logo = t.Teams.Logo,
                                                  Title = t.Teams.Title,
                                                  SeasonId = t.SeasonId
                                              },
                                              TeamInformation = teamDetails != null ?
                                                                new TeamDetailsDto()
                                                                {
                                                                    TeamId = teamDetails.TeamId,
                                                                    SeasonId = teamDetails.SeasonId,
                                                                    TeamName = teamDetails.TeamName
                                                                } : null
                                          }).ToList().Concat((from l in db.Clubs
                                                              from t in l.ClubTeams
                                                              from tp in t.Team.TeamsPlayers
                                                              where l.IsArchive == false && t.Team.IsArchive == false &&
                                                                    tp.UserId == playerId

                                                              let teamDetails = t.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId.Value)
                                                              select new TeamInformationDto()
                                                              {
                                                                  Team = new TeamDto()
                                                                  {
                                                                      LeagueId = 0,
                                                                      ClubId = l.ClubId,
                                                                      TeamId = t.TeamId,
                                                                      Logo = t.Team.Logo,
                                                                      Title = t.Team.Title,
                                                                      SeasonId = t.SeasonId
                                                                  },
                                                                  TeamInformation = teamDetails != null ?
                                                                                    new TeamDetailsDto()
                                                                                    {
                                                                                        TeamId = teamDetails.TeamId,
                                                                                        SeasonId = teamDetails.SeasonId,
                                                                                        TeamName = teamDetails.TeamName
                                                                                    } : null
                                                              }).ToList());

            if (seasonId.HasValue)
            {
                playerTeams = playerTeamsInformation.Where(x => x.Team.SeasonId != null && x.Team.SeasonId.Value == seasonId.Value)
                    .Select(t => new TeamDto()
                    {
                        TeamId = t.Team.TeamId,
                        Title = t.TeamInformation != null && t.TeamInformation.SeasonId == seasonId ?
                                t.TeamInformation.TeamName : t.Team.Title,
                        LeagueId = t.Team.LeagueId,
                        Logo = t.Team.Logo
                    })
                    .ToList();
            }
            else
            {
                playerTeams = playerTeamsInformation.Select(x => x.Team).GroupBy(x => x.TeamId).Select(g => g.First()).ToList();
            }

            playerTeams = playerTeams.GroupBy(x => x.TeamId).Select(g => g.First()).ToList();

            return playerTeams;
        }
        public IList<TeamDto> GetPlayerTeamsTennis(int playerId, int? seasonId = null)
        {

            List<TeamDto> playerTeams;
            var playerTeamsInformation = ((from l in db.Clubs
                                                              from t in l.ClubTeams
                                                              from tp in t.Team.TeamsPlayers
                                                              where l.IsArchive == false && t.Team.IsArchive == false &&
                                                                    tp.UserId == playerId

                                                              let teamDetails = t.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId.Value)
                                                              select new TeamInformationDto()
                                                              {
                                                                  Team = new TeamDto()
                                                                  {
                                                                      LeagueId = 0,
                                                                      ClubId = l.ClubId,
                                                                      TeamId = t.TeamId,
                                                                      Logo = t.Team.Logo,
                                                                      Title = t.Team.Title,
                                                                      SeasonId = t.SeasonId
                                                                  },
                                                                  TeamInformation = teamDetails != null ?
                                                                                    new TeamDetailsDto()
                                                                                    {
                                                                                        TeamId = teamDetails.TeamId,
                                                                                        SeasonId = teamDetails.SeasonId,
                                                                                        TeamName = teamDetails.TeamName
                                                                                    } : null
                                                              }).ToList());

            if (seasonId.HasValue)
            {
                playerTeams = playerTeamsInformation.Where(x => x.Team.SeasonId != null && x.Team.SeasonId.Value == seasonId.Value)
                    .Select(t => new TeamDto()
                    {
                        TeamId = t.Team.TeamId,
                        Title = t.TeamInformation != null && t.TeamInformation.SeasonId == seasonId ?
                                t.TeamInformation.TeamName : t.Team.Title,
                        LeagueId = t.Team.LeagueId,
                        Logo = t.Team.Logo
                    })
                    .ToList();
            }
            else
            {
                playerTeams = playerTeamsInformation.Select(x => x.Team).GroupBy(x => x.TeamId).Select(g => g.First()).ToList();
            }

            playerTeams = playerTeams.GroupBy(x => x.TeamId).Select(g => g.First()).ToList();

            return playerTeams;
        }


        public IList<TeamDto> GetPlayerTeamsInActiveSeasons(int playerId, int? seasonId = null)
        {
            var seasonIds = new List<int>();
            if (seasonId.HasValue)
            {
                seasonIds.Add(seasonId.Value);
            }
            else
            {
                seasonIds.AddRange(currentSeasons(DateTime.Now).Select(s => s.Id));
            }

            var test = db.TeamsPlayers.Where(tp => tp.UserId == playerId && !tp.Team.IsArchive && seasonIds.Contains(tp.SeasonId ?? 0)).ToList();

            List<TeamDto> playerTeams = db.TeamsPlayers.Where(tp => tp.UserId == playerId && !tp.Team.IsArchive && seasonIds.Contains(tp.SeasonId ?? 0)).Select(t => new TeamDto()
            {
                TeamId = t.Team.TeamId,
                Title = t.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == t.SeasonId) != null ? t.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == t.SeasonId).TeamName : t.Team.Title,
                LeagueId = t.LeagueId.HasValue ? t.LeagueId.Value : (t.Team.LeagueTeams.FirstOrDefault(tl => tl.SeasonId == t.SeasonId) != null ? (t.Team.LeagueTeams.FirstOrDefault(tl => tl.SeasonId == t.SeasonId).LeagueId) : 0),
                Logo = t.Team.Logo,
                ClubId = t.ClubId ?? 0,
                SeasonId = t.SeasonId
            }).ToList();
            return playerTeams;
        }

        public IList<TeamDto> GetClubPlayerTeams(int playerId, int? seasonId = null)
        {
            List<TeamDto> playerTeams;
            var playerTeamsInformation = (from ct in db.ClubTeams
                                          from tp in ct.Team.TeamsPlayers
                                          where /* tp.IsActive == true && */ct.TeamId == tp.TeamId &&
                                                tp.UserId == playerId && tp.SeasonId == seasonId.Value && ct.SeasonId == seasonId.Value

                                          let teamDetails = ct.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId.Value)
                                          select new TeamInformationDto()
                                          {
                                              Team = new TeamDto()
                                              {
                                                  ClubId = ct.ClubId,
                                                  TeamId = tp.TeamId,
                                                  Logo = ct.Team.Logo,
                                                  Title = ct.Team.Title,
                                                  SeasonId = tp.SeasonId
                                              },
                                              TeamInformation = teamDetails != null ?
                                                                new TeamDetailsDto()
                                                                {
                                                                    TeamId = teamDetails.TeamId,
                                                                    SeasonId = teamDetails.SeasonId,
                                                                    TeamName = teamDetails.TeamName
                                                                } : null
                                          }).ToList();

            var schoolplayerTeamsInformation = (from club in db.Clubs
                                                from school in club.Schools
                                                from st in school.SchoolTeams
                                                from tp in st.Team.TeamsPlayers
                                                where tp.IsActive == true && st.TeamId == tp.TeamId &&
                                                      tp.UserId == playerId

                                                let teamDetails = st.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId.Value)
                                                select new TeamInformationDto()
                                                {
                                                    Team = new TeamDto()
                                                    {
                                                        ClubId = club.ClubId,
                                                        TeamId = tp.TeamId,
                                                        Logo = st.Team.Logo,
                                                        Title = st.Team.Title,
                                                        SeasonId = tp.SeasonId
                                                    },
                                                    TeamInformation = teamDetails != null ?
                                                                      new TeamDetailsDto()
                                                                      {
                                                                          TeamId = teamDetails.TeamId,
                                                                          SeasonId = teamDetails.SeasonId,
                                                                          TeamName = teamDetails.TeamName
                                                                      } : null
                                                }).ToList();

            playerTeamsInformation = playerTeamsInformation.Union(schoolplayerTeamsInformation).ToList();

            /** add cheng : append league team */
            var leagueTeamsInformation = (from ct in db.LeagueTeams
                                          from tp in ct.Teams.TeamsPlayers
                                          where ct.TeamId == tp.TeamId &&
                                                tp.UserId == playerId && tp.SeasonId == seasonId.Value && ct.SeasonId == seasonId.Value

                                          let teamDetails = ct.Teams.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId.Value)
                                          select new TeamInformationDto()
                                          {
                                              Team = new TeamDto()
                                              {
                                                  ClubId = 0,
                                                  LeagueId = ct.LeagueId,
                                                  TeamId = tp.TeamId,
                                                  Logo = ct.Teams.Logo,
                                                  Title = ct.Teams.Title,
                                                  SeasonId = tp.SeasonId
                                              },
                                              TeamInformation = teamDetails != null ?
                                                                new TeamDetailsDto()
                                                                {
                                                                    TeamId = teamDetails.TeamId,
                                                                    SeasonId = teamDetails.SeasonId,
                                                                    TeamName = teamDetails.TeamName
                                                                } : null
                                          }).ToList();
            playerTeamsInformation = playerTeamsInformation.Union(leagueTeamsInformation).ToList();

            if (seasonId.HasValue)
            {
                playerTeams = playerTeamsInformation.Where(x => x.Team.SeasonId != null && x.Team.SeasonId.Value == seasonId.Value)
                    .Select(t => new TeamDto()
                    {
                        TeamId = t.Team.TeamId,
                        Title = t.TeamInformation != null && t.TeamInformation.SeasonId == seasonId ?
                                t.TeamInformation.TeamName : t.Team.Title,
                        LeagueId = t.Team.LeagueId,
                        Logo = t.Team.Logo,
                        ClubId = t.Team.ClubId
                    })
                    .ToList();
            }
            else
            {
                playerTeams = playerTeamsInformation.Select(x => x.Team).GroupBy(x => x.TeamId).Select(g => g.First()).ToList();
            }
            if(playerTeams != null)
                playerTeams = playerTeams.GroupBy(x => x.TeamId).Select(g => g.First()).ToList();   
            return playerTeams;
        }

        public IList<TeamStanding> GetTeamStandings(int clubId, int teamId)
        {
            var result = (from tsg in db.TeamStandingGames
                          join ts in db.TeamStandings on tsg.Id equals ts.TeamStandingGamesId
                          where teamId == tsg.TeamId && clubId == tsg.ClubId
                          orderby ts.Rank
                          select ts).ToList();

            return result;
        }

        public TeamStandingGame GetTeamStandingGame(int teamId, int clubId, int seasonId)
        {
            var result = db.TeamStandingGames.FirstOrDefault(x => x.TeamId == teamId && x.ClubId == clubId && x.SeasonId == seasonId);
            return result;
        }

        public IList<PlayerTeam> GetPlayerPositions(int playerId, int? seasonId, int? leagueId)
        {
            var playerTeamsDtos = (from l in db.Leagues
                                   from t in l.LeagueTeams
                                   from tp in t.Teams.TeamsPlayers
                                   let teamDetails = t.Teams.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)
                                   where t.Teams.IsArchive == false
                                         && tp.UserId == playerId
                                         && (seasonId == null || l.SeasonId == seasonId)
                                         && (leagueId == null || l.LeagueId == leagueId)
                                   select new PlayerTeamDto()
                                   {
                                       LeagueId = l.LeagueId,
                                       LeagueName = l.Name,
                                       TeamDetails = new TeamInformationDto()
                                       {
                                           Team = new TeamDto()
                                           {
                                               TeamId = t.TeamId,
                                               Logo = t.Teams.Logo,
                                               Title = t.Teams.Title,
                                           },
                                           TeamInformation = teamDetails != null ? new TeamDetailsDto()
                                           {
                                               TeamId = teamDetails.TeamId,
                                               TeamName = teamDetails.TeamName,
                                               SeasonId = teamDetails.SeasonId
                                           } : null
                                       },
                                       ShirtNum = tp.ShirtNum,
                                       Position = tp.Position.Title
                                   }).ToList();

            List<PlayerTeam> playerTeams = (from playerTeamsDto in playerTeamsDtos
                                            let teamInformation = playerTeamsDto.TeamDetails.TeamInformation
                                            select new PlayerTeam()
                                            {
                                                LeagueId = playerTeamsDto.LeagueId,
                                                LeagueName = playerTeamsDto.LeagueName,
                                                TeamId = playerTeamsDto.TeamDetails.Team.TeamId,
                                                Logo = playerTeamsDto.TeamDetails.Team.Logo,
                                                Title = teamInformation != null ? teamInformation.TeamName : playerTeamsDto.TeamDetails.Team.Title,
                                                ShirtNum = playerTeamsDto.ShirtNum,
                                                Position = playerTeamsDto.Position
                                            }).Distinct(new PlayerTeamComparer()).ToList();

            return playerTeams;
        }


        public IList<PlayerTeam> GetClubPlayerTeams(int playerId, int? seasonId, int? clubId)
        {
            var playerTeamsDtos = (from c in db.Clubs
                                   from t in c.ClubTeams
                                   from tp in t.Team.TeamsPlayers
                                   let teamDetails = t.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)
                                   where t.Team.IsArchive == false
                                         && tp.UserId == playerId
                                         && (clubId == null || c.ClubId == clubId)
                                   select new PlayerTeamDto()
                                   {
                                       LeagueId = c.ClubId,
                                       LeagueName = c.Name,
                                       TeamDetails = new TeamInformationDto()
                                       {
                                           Team = new TeamDto()
                                           {
                                               TeamId = t.TeamId,
                                               Logo = t.Team.Logo,
                                               Title = t.Team.Title,
                                           },
                                           TeamInformation = teamDetails != null ? new TeamDetailsDto()
                                           {
                                               TeamId = teamDetails.TeamId,
                                               TeamName = teamDetails.TeamName,
                                               SeasonId = teamDetails.SeasonId
                                           } : null
                                       },
                                       ShirtNum = tp.ShirtNum,
                                       Position = tp.Position.Title
                                   }).ToList();

            List<PlayerTeam> playerTeams = (from playerTeamsDto in playerTeamsDtos
                                            let teamInformation = playerTeamsDto.TeamDetails.TeamInformation
                                            select new PlayerTeam()
                                            {
                                                LeagueId = playerTeamsDto.LeagueId,
                                                LeagueName = playerTeamsDto.LeagueName,
                                                TeamId = playerTeamsDto.TeamDetails.Team.TeamId,
                                                Logo = playerTeamsDto.TeamDetails.Team.Logo,
                                                Title = teamInformation != null ? teamInformation.TeamName : playerTeamsDto.TeamDetails.Team.Title,
                                                ShirtNum = playerTeamsDto.ShirtNum,
                                                Position = playerTeamsDto.Position
                                            }).Distinct(new PlayerTeamComparer()).ToList();

            return playerTeams;
        }

        public void SetIsRankCalculated(int teamId, bool isChecked)
        {
            var team = db.Teams.FirstOrDefault(t => t.TeamId == teamId);
            if(team != null)
            {
                team.IsRankCalculated = isChecked;
                Save();
            }
        }

        public Team UpdateTeamNameInSeason(Team team, int seasonId, string teamName)
        {
            var existedDetails = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
            if (existedDetails != null)
            {
                existedDetails.TeamName = teamName;
            }
            else
            {
                db.TeamsDetails.Add(new TeamsDetails
                {
                    TeamId = team.TeamId,
                    SeasonId = seasonId,
                    TeamName = teamName
                });
            }
            return team;
        }

        public Team UpdateForeignTeamNameInSeason(Team team, int seasonId, string foreignTeamName)
        {
            var existedDetails = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
            if (existedDetails != null)
            {
                existedDetails.TeamForeignName = foreignTeamName;
            }
            else
            {
                db.TeamsDetails.Add(new TeamsDetails
                {
                    TeamId = team.TeamId,
                    SeasonId = seasonId,
                    TeamForeignName = foreignTeamName
                });
            }
            return team;
        }

        public void MoveTeams(int leagueId, int[] teams, int currentLeagueId, int seasonId)
        {
            //find teams to move
            var teamsToMove = db.Teams.Include(x => x.LeagueTeams).Where(x => teams.Contains(x.TeamId)).ToList();
            //find teams in league that will that will be moved
            var previousLeagueTeams =
                db.LeagueTeams.Where(
                    x => x.LeagueId == currentLeagueId && seasonId == x.SeasonId && teams.Contains(x.TeamId));
            foreach (var team in teamsToMove)
            {
                team.LeagueTeams.Add(new LeagueTeams
                {
                    LeagueId = leagueId,
                    TeamId = team.TeamId,
                    SeasonId = seasonId
                });

                //update activity registrations
                var registrations = team.ActivityFormsSubmittedDatas.Where(x => x.LeagueId == currentLeagueId);
                foreach (var registration in registrations)
                {
                    registration.LeagueId = leagueId;
                }

                //update teamplayers league id
                var currentLeaguePlayers =
                    team.TeamsPlayers.Where(x => x.LeagueId == currentLeagueId && x.SeasonId == seasonId);
                foreach (var teamPlayer in currentLeaguePlayers)
                {
                    teamPlayer.LeagueId = leagueId;
                }
            }
            db.LeagueTeams.RemoveRange(previousLeagueTeams);
            db.SaveChanges();
        }

        /// <summary>
        /// Get all teams except current team
        /// </summary>
        /// <returns></returns>
        public List<TeamDto> GetAllExceptCurrent(int currentTeamId, int seasonId, int? unionId, bool allowArchievedUnions = true)
        {
            var teams =
                db.Teams.Include(t => t.TeamsDetails)
                    .Include(t => t.LeagueTeams)
                    .Where(t => t.IsArchive == false && t.TeamId != currentTeamId)
                    .AsQueryable();
            teams = unionId.HasValue
                ? teams.Where(t => t.LeagueTeams.Any(f => f.SeasonId == seasonId && f.Leagues.UnionId == unionId))
                : teams.Where(t => t.LeagueTeams.Any(f => f.SeasonId == seasonId));

            var teamsDto = new List<TeamDto>();
            var teamsList = teams.ToList();
            foreach (var t in teamsList)
            {
                var teamDto = new TeamDto();
                var leagues = db.Leagues.Where(l => l.LeagueTeams.Any(lt => lt.TeamId == t.TeamId && lt.SeasonId == seasonId));
                var league = leagues.OrderByDescending(c => c.LeagueId).FirstOrDefault();
                teamDto.LeagueId = league?.LeagueId ?? 0;
                teamDto.LeagueName = $"({string.Join(", ", leagues.Select(x => x.Name))})";
                teamDto.TeamId = t.TeamId;
                teamDto.Title = t.TeamsDetails.FirstOrDefault(c => c.TeamId == t.TeamId && c.SeasonId == seasonId)?.TeamName ?? t.Title;
                teamDto.SchoolName = db.SchoolTeams.FirstOrDefault(st => st.TeamId == t.TeamId)?.School?.Name;
                var club = t.ClubTeams.FirstOrDefault(ct => ct.SeasonId == seasonId)?.Club;
                teamDto.ClubId = club?.ClubId ?? 0;
                if(club == null || (allowArchievedUnions) || !club.IsUnionArchive)
                {
                    teamsDto.Add(teamDto);
                }    
            }
            return teamsDto;

        }

        public List<Team> GetSchoolTeamsByClubAndSeason(int clubId, int seasonId)
        {
            var teams = db.SchoolTeams.Where(x => x.School.ClubId == clubId && x.School.SeasonId == seasonId)
                .Select(t => t.Team)
                .OrderBy(x => x.Title)
                .AsQueryable();
            var teamList = teams.ToList();
            foreach (var team in teamList)
            {
                var teamDetail = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
                if (teamDetail != null)
                {
                    team.Title = teamDetail.TeamName;
                }
            }
            return teamList;

        }

        public List<Team> GetTennisLeagueTeamsBySeasonAndUnion(int seasonId, int? unionId = null)
        {
            var teams = db.ClubTeams.Where(x => x.SeasonId == seasonId && !x.IsTrainingTeam)
                .Include(x => x.Team)
                .Include(x => x.Team.TeamsDetails)
                .Include(x => x.Team.LeagueTeams)
                .Select(t => t.Team)
                .OrderBy(x => x.Title)
                .AsQueryable();
            if (unionId.HasValue)
            {
                teams = teams.Where(x => x.LeagueTeams.Any(f => f.Leagues.UnionId == unionId));
            }
            var teamList = teams.ToList();
            foreach (var team in teamList)
            {
                var teamDetail = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
                if (teamDetail != null)
                {
                    team.Title = teamDetail.TeamName;
                }
            }
            return teamList;
        }


        public List<Team> GetClubTeamsByClubAndSeasonId(int clubId, int seasonId, int? unionId = null, bool shouldLoadSchoolTeams = false)
        {
            var teams = db.ClubTeams.Where(x => x.ClubId == clubId && x.SeasonId == seasonId && !x.IsTrainingTeam)
                .Include(x => x.Team)
                .Include(x => x.Team.TeamsDetails)
                .Include(x => x.Team.ActivityFormsSubmittedDatas)
                .Select(t => t.Team)
                .OrderBy(x => x.Title)
                .AsQueryable();
            if (unionId.HasValue)
            {
                teams = teams.Where(x => x.LeagueTeams.Any(f => f.Leagues.UnionId == unionId));
            }
            var teamList = teams.ToList();
            if (shouldLoadSchoolTeams)
            {
                teamList.AddRange(db.SchoolTeams.Where(x => x.School.ClubId == clubId).Select(x => x.Team));
            }
            foreach (var team in teamList)
            {
                var teamDetail = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
                if (teamDetail != null)
                {
                    team.Title = teamDetail.TeamName;
                }
            }
            return teamList;
        }

        public List<Team> GetTeamsForAddTraining(int clubId, int seasonId)
        {
            var clubTeamsIds = db.ClubTeams.Where(c => c.ClubId == clubId && c.SeasonId == seasonId).AsNoTracking().Select(c => c.TeamId).ToList();
            List<int> schoolTeamIds = new List<int>();
            var club = db.Clubs.Find(clubId);
            var schools = club.Schools.Where(x => x.SeasonId == seasonId);
            foreach (var school in schools)
            {
                schoolTeamIds.AddRange(school.SchoolTeams.Select(y => y.TeamId).ToList());
            }
            clubTeamsIds.AddRange(schoolTeamIds);

            var teams = db.Teams.Where(x => clubTeamsIds.Contains(x.TeamId))
                .OrderBy(x => x.Title)
                .ToList();
            var teamList = teams.ToList();
            return teamList;
        }

        public bool CheckIfTeamIsLeagueRegistrationTeam(int id, int? seasonId, int? clubId)
        {
            return db.TeamRegistrations.AsNoTracking()
                .Any(tr => !tr.IsDeleted && tr.TeamId == id && tr.ClubId == clubId && tr.SeasonId == seasonId);
        }

        public List<Team> GetTeamsByClubAndSeasonId(int clubId, int seasonId, int? unionId = null)
        {
            var teams = db.Teams
                .Include(x => x.LeagueTeams)
                .Include(x => x.TeamsDetails)
                .Include(x => x.ClubTeams)
                .Where(x => !x.IsArchive &&
                            !x.SchoolTeams.Any(st => st.TeamId == x.TeamId &&
                                                     st.School.SeasonId == seasonId));

            var club = db.Clubs.Find(clubId);

            if (club != null)
            {
                if (club.UnionId.HasValue)
                {
                    var isTennis = club.Union.Section.Alias == GamesAlias.Tennis;
                    teams = teams.Where(f => !f.IsArchive &&
                        (!isTennis && f.LeagueTeams.Any(l => l.Leagues.UnionId == unionId && l.SeasonId == seasonId)) ||
                        f.ClubTeams.Any(x => x.Club.UnionId == club.UnionId && x.SeasonId == seasonId && (!isTennis || !x.IsTrainingTeam)));
                }
                else
                {
                    var section = club.Section;
                    var sectionClubs = section?.Clubs;
                    var finalTeams = new List<Team>();
                    if (sectionClubs != null)
                    {
                        foreach (var sectionclub in sectionClubs)
                        {
                            finalTeams.AddRange(teams.Where(f =>
                                !f.IsArchive &&
                                f.ClubTeams.Any(c => c.ClubId == sectionclub.ClubId)
                            ));
                        }
                    }

                    teams = finalTeams.AsQueryable();
                }
            }
            else if (unionId.HasValue)
            {
                teams = teams.Where(x => x.LeagueTeams.Any(f => f.Leagues.UnionId == unionId && f.SeasonId == seasonId));
            }

            var teamList = teams.ToList();
            foreach (var team in teamList)
            {
                var teamDetail = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
                if (teamDetail != null)
                {
                    team.Title = teamDetail.TeamName;
                }
            }
            return teamList;
        }

        public List<TeamDto> GetTeamsByClubSeasonIdExceptCurrent(int currentTeamId, int clubId, int seasonId, int? unionId = null)
        {
            var club = db.Clubs.First(c => c.ClubId == clubId);
            var isTennis = club.Union?.Section.Alias == GamesAlias.Tennis;

            var teams = db.ClubTeams
                .Where(x => x.ClubId == clubId && x.SeasonId == seasonId && x.TeamId != currentTeamId && !x.Team.IsArchive && (!isTennis ||!x.IsTrainingTeam))
                .Include(x => x.Team)
                .Include(x => x.Team.TeamsDetails)
                .Include(x => x.Team.ActivityFormsSubmittedDatas)
                .Select(t => t.Team)
                .ToList();

            teams.AddRange(db.SchoolTeams.Where(x => x.School.ClubId == clubId && x.TeamId != currentTeamId && x.School.SeasonId == seasonId).Select(x => x.Team));

            teams = teams.OrderBy(x => x.Title).ToList();

            foreach (var team in teams)
            {
                var teamDetail = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
                if (teamDetail != null)
                {
                    team.Title = teamDetail.TeamName;
                }
            }

            var listDto = new List<TeamDto>();

            foreach (var team in teams)
            {
                var teamDto = new TeamDto
                {
                    LeagueId = team.LeagueTeams.FirstOrDefault(x =>
                                   x.SeasonId == seasonId && x.Leagues.UnionId == x.Teams.ClubTeams
                                       .FirstOrDefault(c => c.SeasonId == seasonId)?.Club?.UnionId)?.LeagueId ?? 0,

                    LeagueName = team.LeagueTeams.FirstOrDefault(x =>
                        x.SeasonId == seasonId && x.Leagues.UnionId ==
                        x.Teams.ClubTeams.FirstOrDefault(c => c.SeasonId == seasonId)?.Club?.UnionId)?.Leagues?.Name,

                    ClubId = team.ClubTeams.FirstOrDefault(x => x.SeasonId == seasonId)?.ClubId ??
                             team.SchoolTeams.FirstOrDefault(x => x.School.SeasonId == seasonId)?.School?.ClubId ?? 0,

                    TeamId = team.TeamId,
                    Title = team.Title,
                    SchoolName = team.SchoolTeams.FirstOrDefault(st => st.TeamId == team.TeamId)?.School?.Name,
                    ClubName = team?.Title ?? string.Empty
                };

                listDto.Add(teamDto);
            }

            return listDto.OrderBy(x => x.Title).ToList();
        }


        public List<TeamDto>  GetTeamsByExceptClubBySeasonId(int excludedclubId, int seasonId, List<int> managingClubIds, int? unionId = null)
        {
            var excludedclub = db.Clubs.First(c => c.ClubId == excludedclubId);
            var isTennis = excludedclub.Union?.Section.Alias == GamesAlias.Tennis;

            var teams = db.ClubTeams
                .Where(x => x.ClubId != excludedclubId && x.SeasonId == seasonId && managingClubIds.Contains(x.ClubId) && !x.Team.IsArchive && (!isTennis || !x.IsTrainingTeam))
                .Include(x => x.Team)
                .Include(x => x.Team.TeamsDetails)
                .Include(x => x.Team.ActivityFormsSubmittedDatas)
                .Select(t => t.Team)
                .ToList();

            teams.AddRange(db.SchoolTeams.Where(x => x.School.ClubId != excludedclubId && managingClubIds.Contains(x.School.ClubId) && x.School.SeasonId == seasonId).Select(x => x.Team));

            teams = teams.OrderBy(x => x.Title).ToList();

            foreach (var team in teams)
            {
                var teamDetail = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId);
                if (teamDetail != null)
                {
                    team.Title = teamDetail.TeamName;
                }
            }

            var listDto = new List<TeamDto>();

            foreach (var team in teams)
            {
                var teamDto = new TeamDto
                {
                    LeagueId = team.LeagueTeams.FirstOrDefault(x =>
                                   x.SeasonId == seasonId && x.Leagues.UnionId == x.Teams.ClubTeams
                                       .FirstOrDefault(c => c.SeasonId == seasonId)?.Club?.UnionId)?.LeagueId ?? 0,

                    LeagueName = team.LeagueTeams.FirstOrDefault(x =>
                        x.SeasonId == seasonId && x.Leagues.UnionId ==
                        x.Teams.ClubTeams.FirstOrDefault(c => c.SeasonId == seasonId)?.Club?.UnionId)?.Leagues?.Name,

                    ClubId = team.ClubTeams.FirstOrDefault(x => x.SeasonId == seasonId)?.ClubId ??
                             team.SchoolTeams.FirstOrDefault(x => x.School.SeasonId == seasonId)?.School?.ClubId ?? 0,

                    TeamId = team.TeamId,
                    Title = team.Title,
                    SchoolName = team.SchoolTeams.FirstOrDefault(st => st.TeamId == team.TeamId)?.School?.Name,
                    ClubName = team?.Title ?? string.Empty
                };

                listDto.Add(teamDto);
            }

            return listDto.OrderBy(x => x.Title).ToList();
        }



        public int GetNumberOfLeaguesAndClubs(int teamId)
        {
            return db.LeagueTeams.Count(lt => lt.TeamId == teamId) + db.ClubTeams.Count(ct => ct.TeamId == teamId);
        }

        public string GetGamesUrl(int teamId)
        {
            var team = db.Teams.FirstOrDefault(x => x.TeamId == teamId);
            if (team != null)
            {
                return team.GamesUrl;
            }

            return string.Empty;
        }

        public int SaveTeamStandingUrl(int teamId, int clubId, string url, string externalTeamName, int seasonId)
        {
            try
            {
                var teamStanding = db.TeamStandingGames.FirstOrDefault(x => x.TeamId == teamId && x.ClubId == clubId);
                if (teamStanding != null)
                {
                    teamStanding.GamesUrl = url;
                    teamStanding.ExternalTeamName = externalTeamName;
                    teamStanding.SeasonId = seasonId;

                    db.SaveChanges();
                    return teamStanding.Id;
                }

                var newTeamStanding = new TeamStandingGame();
                newTeamStanding.GamesUrl = url;
                newTeamStanding.TeamId = teamId;
                newTeamStanding.ClubId = clubId;
                newTeamStanding.SeasonId = seasonId;
                newTeamStanding.ExternalTeamName = externalTeamName;
                db.TeamStandingGames.Add(newTeamStanding);
                db.SaveChanges();
                return newTeamStanding.Id;



            }
            catch (Exception)
            {

                return -1;
            }

        }

        public IList<string> GetTeamStandingsUrl()
        {
            return db.TeamStandingGames.Where(x => !string.IsNullOrEmpty(x.GamesUrl)).Select(x => x.GamesUrl).ToList();
        }

        public int GetNumberOfLeagueTeams(int leagueId, int teamId)
        {
            return db.LeagueTeams.Count(x => x.LeagueId == leagueId && x.TeamId == teamId);
        }

        public IEnumerable<LeagueTeams> GetAllLeagueTeams(int leagueId, int? seasonId = null)
        {
            return db.LeagueTeams.Where(t => t.LeagueId == leagueId && t.SeasonId == seasonId && !t.Teams.IsArchive);
        }

        public int GetMaxShirtNumberInTeam(int teamId)
        {
            int max = db.TeamsPlayers
                .Where(p => p.TeamId == teamId)
                .Select(p => p.ShirtNum)
                .DefaultIfEmpty(0)
                .Max();
            return max;
        }

        public string GetTeamNameById(int teamId)
        {
            var team = db.Teams.FirstOrDefault(t => t.TeamId == teamId);
            var teamDetails = team.TeamsDetails.OrderByDescending(t => t.Id)?.FirstOrDefault();
            return teamDetails?.TeamName ?? team?.Title ?? string.Empty;
        }

        public IDictionary<Season, IEnumerable<AveragePlayersStatistics>> GetHomeWGamesStatistics(int id)
        {
            var team = db.Teams.FirstOrDefault(t => t.TeamId == id);
            if (team != null)
            {
                var homeStatistic = team.WaterpoloStatistics.Where(s => s.GamesCycle.HomeTeamId == id);
                var homeSeasons = homeStatistic.Select(s => s.GamesCycle.Stage.League.SeasonId).Where(s => s.HasValue).Select(s => s.Value).Distinct();
                var statDict = new Dictionary<Season, IEnumerable<AveragePlayersStatistics>>();
                foreach (var seasonId in homeSeasons)
                {
                    var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
                    var seasonGames = GetWStatisticsByPlayers(season, homeStatistic);
                    statDict.Add(season, seasonGames);
                }
                return statDict;
            }
            else
            {
                return new Dictionary<Season, IEnumerable<AveragePlayersStatistics>>();
            }
        }

        public IDictionary<Season, IEnumerable<AveragePlayersStatistics>> GetGuestWGamesStatistics(int id)
        {
            var team = db.Teams.FirstOrDefault(t => t.TeamId == id);
            if (team != null)
            {
                var guestStatistics = team.WaterpoloStatistics.Where(s => s.GamesCycle.GuestTeamId == id);
                var guestSeasons = guestStatistics.Select(s => s.GamesCycle.Stage.League.SeasonId).Where(s => s.HasValue).Select(s => s.Value).Distinct();
                var statDict = new Dictionary<Season, IEnumerable<AveragePlayersStatistics>>();
                foreach (var seasonId in guestSeasons)
                {
                    var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
                    var seasonGames = GetWStatisticsByPlayers(season, guestStatistics);
                    statDict.Add(season, seasonGames);
                }
                return statDict;
            }
            else
            {
                return new Dictionary<Season, IEnumerable<AveragePlayersStatistics>>();
            }
        }

        private IEnumerable<AveragePlayersStatistics> GetWStatisticsByPlayers(Season season, IEnumerable<WaterpoloStatistic> statistic)
        {
            if (season != null)
            {
                var seasonGames = statistic.Where(s => s.GamesCycle?.Stage?.League?.SeasonId == season.Id);
                var playersIds = seasonGames.Select(c => c.PlayerId).Distinct();
                foreach (var playerId in playersIds)
                {
                    var teamPlayersStatistics = seasonGames.Where(c => c.PlayerId == playerId)?.ToList();

                    if (teamPlayersStatistics != null)
                    {
                        yield return new AveragePlayersStatistics
                        {
                            PlayersName = db.TeamsPlayers.FirstOrDefault(pl => pl.Id == playerId)?.User?.FullName,
                            PlayersId = playerId,
                            GamesCount = teamPlayersStatistics.Count,
                            GP = teamPlayersStatistics.Count.ToString(),
                            Season = season,
                            Min = teamPlayersStatistics.Sum(c => c.MinutesPlayed.ToMinutesFromMiliseconds()),
                            Goal = teamPlayersStatistics.Sum(u => u.GOAL ?? 0),
                            PGoal = teamPlayersStatistics.Sum(u => u.PGOAL ?? 0),
                            Miss = teamPlayersStatistics.Sum(u => u.Miss ?? 0),
                            PMiss = teamPlayersStatistics.Sum(u => u.PMISS ?? 0),
                            Offs = teamPlayersStatistics.Sum(u => u.OFFS ?? 0),
                            Foul = teamPlayersStatistics.Sum(u => u.FOUL ?? 0),
                            Exc = teamPlayersStatistics.Sum(u => u.EXC ?? 0),
                            BFoul = teamPlayersStatistics.Sum(u => u.BFOUL ?? 0),
                            SSave = teamPlayersStatistics.Sum(u => u.SSAVE ?? 0),
                            YC = teamPlayersStatistics.Sum(u => u.YC ?? 0),
                            RD = teamPlayersStatistics.Sum(u => u.RD ?? 0),
                            AST = teamPlayersStatistics.Sum(u => u.AST ?? 0),
                            TO = teamPlayersStatistics.Sum(u => u.TO ?? 0),
                            STL = teamPlayersStatistics.Sum(u => u.STL ?? 0),
                            BLK = teamPlayersStatistics.Sum(u => u.BLK ?? 0),
                            EFF = teamPlayersStatistics.Sum(u => u.EFF ?? 0),
                            PlusMinus = teamPlayersStatistics.Sum(u => u.DIFF ?? 0D)
                        };
                    }
                }
            }
        }

        public IDictionary<Season, IEnumerable<AveragePlayersStatistics>> GetHomeGamesStatistics(int id)
        {
            var team = db.Teams.FirstOrDefault(t => t.TeamId == id);
            if (team != null)
            {
                var homeStatistic = team.GameStatistics.Where(s => s.GamesCycle.HomeTeamId == id);
                var homeSeasons = homeStatistic.Select(s => s.GamesCycle.Stage.League.SeasonId).Where(s => s.HasValue).Select(s => s.Value).Distinct();
                var statDict = new Dictionary<Season, IEnumerable<AveragePlayersStatistics>>();
                foreach (var seasonId in homeSeasons)
                {
                    var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
                    var seasonGames = GetStatisticsByPlayers(season, homeStatistic);
                    statDict.Add(season, seasonGames);
                }
                return statDict;
            }
            else
            {
                return new Dictionary<Season, IEnumerable<AveragePlayersStatistics>>();
            }
        }

        public IDictionary<Season, IEnumerable<AveragePlayersStatistics>> GetGuestGamesStatistics(int id)
        {
            var team = db.Teams.FirstOrDefault(t => t.TeamId == id);
            if (team != null)
            {
                var guestStatistics = team.GameStatistics.Where(s => s.GamesCycle.GuestTeamId == id);
                var guestSeasons = guestStatistics.Select(s => s.GamesCycle.Stage.League.SeasonId).Where(s => s.HasValue).Select(s => s.Value).Distinct();
                var statDict = new Dictionary<Season, IEnumerable<AveragePlayersStatistics>>();
                foreach (var seasonId in guestSeasons)
                {
                    var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
                    var seasonGames = GetStatisticsByPlayers(season, guestStatistics);
                    statDict.Add(season, seasonGames);
                }
                return statDict;
            }
            else
            {
                return new Dictionary<Season, IEnumerable<AveragePlayersStatistics>>();
            }
        }

        private IEnumerable<AveragePlayersStatistics> GetStatisticsByPlayers(Season season, IEnumerable<GameStatistic> statistic)
        {
            if (season != null)
            {
                var seasonGames = statistic.Where(s => s.GamesCycle?.Stage?.League?.SeasonId == season.Id);
                var playersIds = seasonGames.Select(c => c.PlayerId).Distinct();
                foreach (var playerId in playersIds)
                {
                    var teamPlayersStatistics = seasonGames.Where(c => c.PlayerId == playerId)?.ToList();

                    if (teamPlayersStatistics != null)
                    {
                        yield return new AveragePlayersStatistics
                        {
                            PlayersName = db.TeamsPlayers.FirstOrDefault(pl => pl.Id == playerId)?.User?.FullName,
                            PlayersId = playerId,
                            GamesCount = teamPlayersStatistics.Count,
                            GP = teamPlayersStatistics.Count.ToString(),
                            Season = season,
                            Min = teamPlayersStatistics.Sum(c => c.MinutesPlayed.ToMinutesFromMiliseconds()),
                            FG = teamPlayersStatistics.Sum(u => u.FG ?? 0),
                            FGA = teamPlayersStatistics.Sum(u => u.FGA ?? 0),
                            ThreePT = teamPlayersStatistics.Sum(u => u.ThreePT ?? 0),
                            ThreePA = teamPlayersStatistics.Sum(u => u.ThreePA ?? 0),
                            TwoPT = teamPlayersStatistics.Sum(u => u.TwoPT ?? 0),
                            TwoPA = teamPlayersStatistics.Sum(u => u.TwoPA ?? 0),
                            FT = teamPlayersStatistics.Sum(u => u.FT ?? 0),
                            FTA = teamPlayersStatistics.Sum(u => u.FTA ?? 0),
                            OREB = teamPlayersStatistics.Sum(u => u.OREB ?? 0),
                            DREB = teamPlayersStatistics.Sum(u => u.DREB ?? 0),
                            REB = teamPlayersStatistics.Sum(u => u.REB ?? 0),
                            AST = teamPlayersStatistics.Sum(u => u.AST ?? 0),
                            TO = teamPlayersStatistics.Sum(u => u.TO ?? 0),
                            STL = teamPlayersStatistics.Sum(u => u.STL ?? 0),
                            BLK = teamPlayersStatistics.Sum(u => u.BLK ?? 0),
                            PF = teamPlayersStatistics.Sum(u => u.PF ?? 0),
                            PTS = teamPlayersStatistics.Sum(u => u.PTS ?? 0),
                            FGM = teamPlayersStatistics.Sum(u => u.FGM ?? 0),
                            FTM = teamPlayersStatistics.Sum(u => u.FTM ?? 0),
                            EFF = teamPlayersStatistics.Sum(u => u.EFF ?? 0),
                            PlusMinus = teamPlayersStatistics.Sum(u => u.DIFF ?? 0D)
                        };
                    }
                }
            }
        }

        public bool IsUnionTeam(int teamId, int? unionId, int? seasonId)
        {
            var teams = db.Teams.Include(t => t.TeamsDetails)
                .Include(t => t.LeagueTeams)
                .Where(t => t.IsArchive == false)
                .AsQueryable();

            teams = unionId.HasValue
                ? teams.Where(t => t.LeagueTeams.Any(f => f.SeasonId == seasonId && f.Leagues.UnionId == unionId))
                : teams.Where(t => t.LeagueTeams.Any(f => f.SeasonId == seasonId));

            if (teams.Any())
            {
                var teamsIds = teams.Select(c => c.TeamId);
                return teamsIds.Contains(teamId);
            }

            return false;
        }

        public IEnumerable<Team> GetRegisteredTeamsByLeagueId(int id, int seasonId)
        {
            var registrationList = db.TeamRegistrations.Where(tr => tr.LeagueId == id && tr.SeasonId == seasonId && !tr.IsDeleted && !tr.Team.IsArchive);

            if (registrationList == null || !registrationList.Any()) yield break;

            foreach (var registration in registrationList)
            {
                yield return registration.Team;
            }
        }

        public void DeleteTennisTeam(int teamId, int leagueId, int seasonId)
        {
            var tennisReg = db.TeamRegistrations.FirstOrDefault(c =>
                c.TeamId == teamId && c.LeagueId == leagueId && c.SeasonId == seasonId);
            if (tennisReg != null)
            {
                tennisReg.IsDeleted = true;
                db.SaveChanges();
            }
        }

        public void ChangeInsuranceStatus(int teamId, string type, bool isChecked)
        {
            var team = db.Teams.Find(teamId);
            if (isChecked)
            {
                switch (type)
                {
                    case "union":
                        team.IsClubInsurance = false;
                        team.IsUnionInsurance = true;
                        break;
                    case "club":
                        team.IsClubInsurance = true;
                        team.IsUnionInsurance = false;
                        break;
                }
            }
            else
            {
                team.IsClubInsurance = null;
                team.IsUnionInsurance = null;
            }
        }

        public string GetTeamNameWithoutLeagueName(string teamName, string leagueName)
        {
            return teamName.Replace($" - {leagueName}", string.Empty);
        }

        public string GetTeamNameWithoutLeagueName(int? teamId, int leagueId)
        {
            var team = db.Teams.FirstOrDefault(t => t.TeamId == teamId);
            var teamName = team?.TeamsDetails?.OrderByDescending(t => t.Id)?.FirstOrDefault()?.TeamName
                ?? team?.Title ?? string.Empty;
            var leagueName = db.Leagues.FirstOrDefault(l => l.LeagueId == leagueId)?.Name ?? string.Empty;
            return GetTeamNameWithoutLeagueName(teamName, leagueName);
        }

        public bool IsInTrainingTeam(int userId, int seasonId)
        {
            return db.TeamsPlayers.Any(x => x.UserId == userId && x.SeasonId == seasonId && x.Team.ClubTeams.Any(ct => ct.IsTrainingTeam && ct.SeasonId == seasonId));
        }

        public ClubTeam GetTrainingTeam(int? clubId, int seasonId)
        {
            return db.ClubTeams.FirstOrDefault(x => x.ClubId == clubId && x.SeasonId == seasonId && x.IsTrainingTeam);
        }
    }
}
