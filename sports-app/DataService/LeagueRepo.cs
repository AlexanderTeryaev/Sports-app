using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Linq.Expressions;
using AppModel;
using DataService.DTO;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Mapping;
using DataService.Services;

namespace DataService
{
    internal class UnionSeason : IEquatable<UnionSeason>
    {
        public int? UnionId { get; set; }
        public int? SeasonId { get; set; }

        public bool Equals(UnionSeason other)
        {
            return UnionId.HasValue && SeasonId.HasValue
                && other.UnionId.HasValue && other.SeasonId.HasValue
                && other.UnionId == UnionId && other.SeasonId == SeasonId;
        }
    }
    public class AthleticsLeagueStandingModel
    {
        public int? TypeId { get; set; }
        public int GenderId { get; set; }
        public int ClubId { get; set; }
        public int Rank { get; set; }
        public string ClubName { get; set; }
        public decimal FinalScore { get; set; }
    }
    public class LeagueRepo : BaseRepo
    {
        private IEnumerable<League> Leagues { get; set; }

        public LeagueRepo() : base() { }

        public LeagueRepo(DataEntities db) : base(db)
        {
            Leagues = GetQuery(false).ToList();
        }
        public List<AthleticsLeagueStandingModel>  AthleticsLeagueStandings(int id)
        {
            //competiton-rank ex id = 1,2,3//athelete leagueId
            var athleticLeague = db.AthleticLeagues.Find(id);
            var competitionsOfLeague = db.Leagues.Where(x => x.AthleticLeagueId == id).ToList();

            if (athleticLeague == null || !competitionsOfLeague.Any())
            {
                return null;
            }

            var competitionsIds = competitionsOfLeague.Select(x => x.LeagueId).ToArray();

            var results = new List<AthleticsLeagueStandingModel>();

            var data = db.CompetitionClubsCorrections
                .Include(x => x.Club)
                .Where(x => x.TypeId != null &&
                            competitionsIds.Contains(x.LeagueId) &&
                            x.SeasonId == athleticLeague.SeasonId)
                .AsNoTracking()
                .ToList();

            foreach (var competitionData in data)
            {
                var existing = results.FirstOrDefault(x => x.TypeId == competitionData.TypeId &&
                                                           x.GenderId == competitionData.GenderId &&
                                                           x.ClubId == competitionData.ClubId);

                if (existing == null)
                {
                    results.Add(new AthleticsLeagueStandingModel
                    {
                        ClubId = competitionData.ClubId,
                        ClubName = competitionData.Club.Name,
                        TypeId = competitionData.TypeId,
                        GenderId = competitionData.GenderId,
                        FinalScore = competitionData.FinalScore
                    });
                }
                else
                {
                    existing.FinalScore += competitionData.FinalScore;
                }
            }
            var n = 1;
            var viewModel = results
                .OrderBy(x => x.TypeId)
                .ThenByDescending(x => x.GenderId)
                .ThenByDescending(x => x.FinalScore)
                .Select(x => new AthleticsLeagueStandingModel
                {
                    Rank = n++,
                    ClubId = x.ClubId,
                    ClubName = x.ClubName,
                    TypeId = x.TypeId,
                    GenderId = x.GenderId,
                    FinalScore = x.FinalScore
                }).ToList();
            return viewModel;
        }
        public List<CompetitionClubCorrectionDTO> AthleticsLeagueCompetitionRanking(int id, int seasonId, bool isField = false, bool isModal = false)
        {
            //league-rank ex: leagueid = 3056
            List<CompetitionClubCorrectionDTO> grouped = new List<CompetitionClubCorrectionDTO>();
            var _disciplinesRepo = new DisciplinesRepo();
            var competition = this.GetById(id);
            if (!isField)
            {
                var n = 1;
                var data = db.CompetitionClubsCorrections.Where(c => c.LeagueId == id && c.SeasonId == seasonId).ToList();
                grouped = data
                        .OrderBy(x => x.TypeId)
                        .ThenByDescending(x => x.GenderId)
                        .ThenByDescending(x => x.FinalScore)
                        .Select(x=> new CompetitionClubCorrectionDTO
                        {

                            Id = x.Id,
                            Rank = n++,
                            LeagueId = x.LeagueId,
                            SeasonId = x.SeasonId,
                            ClubId = x.ClubId,
                            ClubName = db.Clubs.Where(y=>y.ClubId == x.ClubId).FirstOrDefault()?.Name,
                            GenderId =  x.GenderId,
                            Correction = x.Correction,
                            Points = x.Points,
                            TypeId = x.TypeId,
                            ResultsCounted = x.ResultsCounted
                        })
                        .ToList();
            }
            return grouped;
        }
        public bool CheckIfIsTennisLeague(int id)
        {
            var sectionAlias = GetSectionAlias(id);
            var league = GetById(id);
            return sectionAlias.Equals(SectionAliases.Tennis) && (league?.EilatTournament == null || league?.EilatTournament == false);
        }

        public IEnumerable<LeagueScheduleState> GetLeagueScheduleState(int id, int adminId)
        {
            return db.LeagueScheduleStates.Where(s => s.LeagueId == id && s.UserId == adminId);
        }

        public IEnumerable<LeagueShort> GetLeaguesFilterList(int unionId, int seasonId, int? disciplineId)
        {
            var leagueShortList = new List<LeagueShort>();
            //  Fill in leagues list
            leagueShortList.Add(new LeagueShort
            {
                Name = "All",
                Id = -1,
                Check = false
            });
            leagueShortList.AddRange(
                GetAll(disciplineId)
                .Where(x => x.UnionId == unionId && !x.IsArchive && x.SeasonId == seasonId)
                .Select(x => new LeagueShort
                {
                    Id = x.LeagueId,
                    Name = x.Name,
                    UnionId = x.UnionId,
                    Check = false
                }));
            return leagueShortList;
        }

        public List<PlayersStatisticsDTO> GetLeagueStatistics(int leagueId, int seasonId)
        {
            var league = db.Leagues.FirstOrDefault(l => l.LeagueId == leagueId);
            var leagueStatisticsDto = new List<PlayersStatisticsDTO>();
            if (league != null)
            {
                var alias = GetGameAliasByLeagueId(leagueId);
                if (alias == GamesAlias.BasketBall)
                {
                    var leagueStatistics = db.GameStatistics.Where(g => g.GamesCycle.Stage.LeagueId == leagueId);
                    if (leagueStatistics.Any())
                    {
                        leagueStatisticsDto = GetLeagueStatisticsDto(leagueStatistics, seasonId);
                    }
                }
                else if (alias == GamesAlias.WaterPolo)
                {
                    var leagueStatistics = db.WaterpoloStatistics.Where(g => g.GamesCycle.Stage.LeagueId == leagueId);
                    if (leagueStatistics.Any())
                    {
                        leagueStatisticsDto = GetWLeagueStatisticsDto(leagueStatistics, seasonId);
                    }
                }
            }
            return leagueStatisticsDto;
        }

        private List<PlayersStatisticsDTO> GetWLeagueStatisticsDto(IQueryable<WaterpoloStatistic> leagueStatistics, int seasonId)
        {
            var res = new List<PlayersStatisticsDTO>();
            var playersIds = leagueStatistics.Select(c => c.PlayerId).Distinct();
            foreach (var playerId in playersIds)
            {
                var playersGames = leagueStatistics.Where(s => s.PlayerId == playerId).ToList();
                if (playersGames.Count > 0)
                {
                    res.Add(new PlayersStatisticsDTO
                    {
                        PlayersId = playerId,
                        PlayersName =
                            $"{db.TeamsPlayers.FirstOrDefault(p => p.Id == playerId)?.User?.FullName ?? ""} " +
                            $"({db.TeamsPlayers.FirstOrDefault(p => p.Id == playerId)?.Team?.TeamsDetails.Where(t => t.SeasonId == seasonId)?.OrderByDescending(c => c.Id).FirstOrDefault()?.TeamName ?? db.TeamsPlayers.FirstOrDefault(p => p.Id == playerId)?.Team?.Title})",
                        PlayersImage = db.TeamsPlayers.FirstOrDefault(p => p.Id == playerId)?.User.PlayerFiles.Where(
                                               x => x.SeasonId == seasonId
                                                    && x.FileType == (int)PlayerFileType.PlayerImage)
                                           .Select(x => x.FileName)
                                           .FirstOrDefault() ??
                                       db.TeamsPlayers.FirstOrDefault(p => p.Id == playerId)?.User?.Image,
                        Min = Convert.ToInt32(playersGames.Sum(c => c.MinutesPlayed) ?? 0),
                        Goal = playersGames.Sum(c => c.GOAL) ?? 0,
                        PGoal = playersGames.Sum(c => c.PGOAL) ?? 0,
                        Miss = playersGames.Sum(c => c.Miss) ?? 0,
                        PMiss = playersGames.Sum(c => c.PMISS) ?? 0,
                        AST = playersGames.Sum(c => c.AST) ?? 0,
                        TO = playersGames.Sum(c => c.TO) ?? 0,
                        STL = playersGames.Sum(c => c.STL) ?? 0,
                        BLK = playersGames.Sum(c => c.BLK) ?? 0,
                        Offs = playersGames.Sum(c => c.OFFS) ?? 0,
                        Foul = playersGames.Sum(c => c.FOUL) ?? 0,
                        Exc = playersGames.Sum(c => c.EXC) ?? 0,
                        BFoul = playersGames.Sum(c => c.BFOUL) ?? 0,
                        SSave = playersGames.Sum(c => c.SSAVE) ?? 0,
                        YC = playersGames.Sum(c => c.YC) ?? 0,
                        RD = playersGames.Sum(c => c.RD) ?? 0,
                        EFF = playersGames.Sum(c => c.EFF) ?? 0,
                        GamesCount = playersGames.Count()
                    });
                }
            }

            return res;
        }

        private List<PlayersStatisticsDTO> GetLeagueStatisticsDto(IQueryable<GameStatistic> leagueStatistics, int seasonId)
        {
            var res = new List<PlayersStatisticsDTO>();
            var playersIds = leagueStatistics.Select(c => c.PlayerId).Distinct();
            foreach (var playerId in playersIds)
            {
                var playersGames = leagueStatistics.Where(s => s.PlayerId == playerId).ToList();
                if (playersGames.Count > 0)
                {
                    res.Add(new PlayersStatisticsDTO
                    {
                        PlayersId = playerId,
                        PlayersName =
                            $"{db.TeamsPlayers.FirstOrDefault(p => p.Id == playerId)?.User?.FullName ?? ""} " +
                            $"({db.TeamsPlayers.FirstOrDefault(p => p.Id == playerId)?.Team?.TeamsDetails.Where(t => t.SeasonId == seasonId)?.OrderByDescending(c => c.Id).FirstOrDefault()?.TeamName ?? db.TeamsPlayers.FirstOrDefault(p => p.Id == playerId)?.Team?.Title})",
                        PlayersImage = db.TeamsPlayers.FirstOrDefault(p => p.Id == playerId)?.User.PlayerFiles.Where(
                                               x => x.SeasonId == seasonId
                                                    && x.FileType == (int)PlayerFileType.PlayerImage)
                                           .Select(x => x.FileName)
                                           .FirstOrDefault() ??
                                       db.TeamsPlayers.FirstOrDefault(p => p.Id == playerId)?.User?.Image,
                        Min = Convert.ToInt32(playersGames.Sum(c => c.MinutesPlayed) ?? 0),
                        FG = playersGames.Sum(c => c.FG) ?? 0,
                        FGA = playersGames.Sum(c => c.FGA) ?? 0,
                        ThreePT = playersGames.Sum(c => c.ThreePT) ?? 0,
                        ThreePA = playersGames.Sum(c => c.ThreePA) ?? 0,
                        TwoPT = playersGames.Sum(c => c.TwoPT) ?? 0,
                        TwoPA = playersGames.Sum(c => c.TwoPA) ?? 0,
                        FT = playersGames.Sum(c => c.FT) ?? 0,
                        FTA = playersGames.Sum(c => c.FTA) ?? 0,
                        OREB = playersGames.Sum(c => c.OREB) ?? 0,
                        DREB = playersGames.Sum(c => c.DREB) ?? 0,
                        REB = playersGames.Sum(c => c.REB) ?? 0,
                        AST = playersGames.Sum(c => c.AST) ?? 0,
                        TO = playersGames.Sum(c => c.TO) ?? 0,
                        STL = playersGames.Sum(c => c.STL) ?? 0,
                        BLK = playersGames.Sum(c => c.BLK) ?? 0,
                        PF = playersGames.Sum(c => c.PF) ?? 0,
                        PTS = playersGames.Sum(c => c.PTS) ?? 0,
                        FGM = playersGames.Sum(c => c.FGM) ?? 0,
                        FTM = playersGames.Sum(c => c.FTM) ?? 0,
                        EFF = playersGames.Sum(c => c.EFF) ?? 0,
                        GamesCount = playersGames.Count()
                    });
                }
            }

            return res;
        }

        public void DuplicateLeague(League item)
        {
            var leagueForDuplicate = db.Leagues.FirstOrDefault(l => l.LeagueId == item.DuplicatedLeagueId);

            item.GenderId = leagueForDuplicate.GenderId;
            item.AgeId = leagueForDuplicate.AgeId;
            item.Logo = leagueForDuplicate.Logo;
            item.Image = leagueForDuplicate.Image;
            item.Description = leagueForDuplicate.Description;
            item.Terms = leagueForDuplicate.Terms;
            item.CreateDate = DateTime.Now;
            item.Place = leagueForDuplicate.Place;
            item.EilatTournament = leagueForDuplicate.EilatTournament;
            item.SortOrder = leagueForDuplicate.SortOrder;
            item.ClubId = leagueForDuplicate.ClubId;
            item.MaximumHandicapScoreValue = leagueForDuplicate.MaximumHandicapScoreValue;
            item.AboutLeague = leagueForDuplicate.AboutLeague;
            item.LeagueStructure = leagueForDuplicate.LeagueStructure;
            item.TeamRegistrationPrice = leagueForDuplicate.TeamRegistrationPrice;
            item.PlayerRegistrationPrice = leagueForDuplicate.PlayerRegistrationPrice;
            item.PlayerInsurancePrice = leagueForDuplicate.PlayerInsurancePrice;
            item.MaximumAge = leagueForDuplicate.MaximumAge;
            item.MinimumAge = leagueForDuplicate.MinimumAge;
            item.MinimumPlayersTeam = leagueForDuplicate.MinimumPlayersTeam;
            item.MaximumPlayersTeam = leagueForDuplicate.MaximumPlayersTeam;
            item.LeagueCode = leagueForDuplicate.LeagueCode;
            item.DisciplineId = leagueForDuplicate.DisciplineId;
            item.LeagueStartDate = leagueForDuplicate.LeagueStartDate;
            item.LeagueEndDate = leagueForDuplicate.LeagueEndDate;
            item.StartRegistrationDate = leagueForDuplicate.StartRegistrationDate;
            item.EndRegistrationDate = leagueForDuplicate.EndRegistrationDate;
            item.AllowedCLubsIds = leagueForDuplicate.AllowedCLubsIds;
            item.FiveHandicapReduction = leagueForDuplicate.FiveHandicapReduction;
            item.IsPositionSettingsEnabled = leagueForDuplicate.IsPositionSettingsEnabled;
            item.IsTeam = leagueForDuplicate.IsTeam;
            item.Type = leagueForDuplicate.Type;
            item.MinParticipationReq = leagueForDuplicate.MinParticipationReq;
            item.PlaceOfCompetition = leagueForDuplicate.PlaceOfCompetition;
            item.RegistrationLink = leagueForDuplicate.RegistrationLink;
            item.IsDailyCompetition = leagueForDuplicate.IsDailyCompetition;

            db.Leagues.Add(item);
            db.SaveChanges();

            var copiedLeague = db.Leagues.OrderByDescending(r => r.LeagueId).FirstOrDefault();

            if (leagueForDuplicate.PlayerDiscounts.Count > 0)
            {
                db.PlayerDiscounts.AddRange(
                    leagueForDuplicate.PlayerDiscounts.Select(l => { l.LeagueId = copiedLeague.LeagueId; return l; }));
            }

            if (leagueForDuplicate.LeagueOfficialsSettings.Count > 0)
            {
                db.LeagueOfficialsSettings.AddRange(
                    leagueForDuplicate.LeagueOfficialsSettings.Select(l => { l.LeagueId = copiedLeague.LeagueId; return l; }));
            }

            if (leagueForDuplicate.RefereeRegistrations.Count > 0)
            {
                db.RefereeRegistrations.AddRange(
                    leagueForDuplicate.RefereeRegistrations.Select(r => { r.LeagueId = copiedLeague.LeagueId; return r; }));
            }

            if (leagueForDuplicate.DaysForHostings.Count > 0)
            {
                db.DaysForHostings.AddRange(
                    leagueForDuplicate.DaysForHostings.Select(r => { r.LeagueId = copiedLeague.LeagueId; return r; }));
            }

            if (leagueForDuplicate.LeagueTeams.Count > 0)
            {
                DuplicateLeagueTeams(leagueForDuplicate.LeagueTeams, copiedLeague);
            }

            //if (leagueForDuplicate.ActivityFormsSubmittedDatas.Count > 0)
            //{
            //    db.ActivityFormsSubmittedDatas.AddRange(
            //        leagueForDuplicate.ActivityFormsSubmittedDatas.Select(r =>
            //        {
            //            r.LeagueId = copiedLeague.LeagueId;
            //            return r;
            //        }));
            //}

            //if (leagueForDuplicate.ActivitiesLeagues.Count > 0)
            //{
            //    db.ActivitiesLeagues.AddRange(
            //        leagueForDuplicate.ActivitiesLeagues.Select(r =>
            //        {
            //            r.LeagueId = copiedLeague.LeagueId;
            //            return r;
            //        }));
            //}

            if (leagueForDuplicate.LevelDateSettings.Count > 0)
            {
                db.LevelDateSettings.AddRange(
                    leagueForDuplicate.LevelDateSettings.Select(l =>
                    {
                        l.CompetitionId = copiedLeague.LeagueId;
                        return l;
                    }));
            }

            if (leagueForDuplicate.LeaguesPrices.Count > 0)
            {
                db.LeaguesPrices.AddRange(
                    leagueForDuplicate.LeaguesPrices.Select(l =>
                    {
                        l.LeagueId = copiedLeague.LeagueId;
                        return l;
                    }));
            }

            if (leagueForDuplicate.HandlingFees.Count > 0)
            {
                db.HandlingFees.AddRange(
                    leagueForDuplicate.HandlingFees.Select(l =>
                    {
                        l.LeagueId = copiedLeague.LeagueId;
                        return l;
                    }));
            }

            if (leagueForDuplicate.MemberFees.Count > 0)
            {
                db.MemberFees.AddRange(
                    leagueForDuplicate.MemberFees.Select(l =>
                    {
                        l.LeagueId = copiedLeague.LeagueId;
                        return l;
                    }));
            }
        }

        private void DuplicateLeagueTeams(ICollection<LeagueTeams> leagueTeams, League league)
        {
            foreach (var leagueTeam in leagueTeams)
            {
                var teamId = DuplicateTeamInformation(leagueTeam.Teams, leagueTeam.SeasonId ?? 0).TeamId;
                db.LeagueTeams.Add(new LeagueTeams
                {
                    LeagueId = league.LeagueId,
                    SeasonId = leagueTeam.SeasonId,
                    TeamId = teamId
                });
            }
            db.SaveChanges();
        }

        private Team DuplicateTeamInformation(Team team, int seasonId)
        {
            var newTeam = new Team
            {
                Title = team.Title,
                Logo = team.Logo,
                PersonnelPic = team.PersonnelPic,
                Description = team.Description,
                OrgUrl = team.OrgUrl,
                CreateDate = team.CreateDate,
                IsArchive = team.IsArchive,
                GamesUrl = team.GamesUrl,
                IsUnderAdult = team.IsUnderAdult,
                IsReserved = team.IsReserved,
                NeedShirts = team.NeedShirts,
                InsuranceApproval = team.InsuranceApproval,
                HasArena = team.HasArena,
                IsTrainingEnabled = team.IsTrainingEnabled,
                MinimumAge = team.MinimumAge,
                MaximumAge = team.MaximumAge,
                CompetitionAgeId = team.CompetitionAgeId,
                GenderId = team.GenderId,
                LevelId = team.LevelId,
                MinRank = team.MinRank,
                MaxRank = team.MaxRank,
                PlaceForQualification = team.PlaceForQualification,
                PlaceForFinal = team.PlaceForFinal,
                RegionId = team.RegionId,
                IsReligiousTeam = team.IsReligiousTeam,
                IsUnionInsurance = team.IsUnionInsurance,
                IsClubInsurance = team.IsClubInsurance
            };
            db.Teams.Add(newTeam);
            db.SaveChanges();

            var oldTeamDetails = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId);
            if (oldTeamDetails != null)
            {
                db.TeamsDetails.Add(new TeamsDetails
                {
                    SeasonId = seasonId,
                    TeamId = newTeam.TeamId,
                    TeamName = oldTeamDetails.TeamName ?? team.Title,
                    RegistrationId = oldTeamDetails.RegistrationId
                });

                db.SaveChanges();
            }

            return newTeam;
        }

        public League GetLeagueForCategory(int id, int seasonId)
        {
            return db.LeagueTeams.FirstOrDefault(t => t.TeamId == id && t.SeasonId == seasonId)?.Leagues;
        }

        public League GetByStage(int stageId)
        {
            return db.Stages.FirstOrDefault(l => l.StageId == stageId)?.League;
        }

        public IEnumerable<League> GetByTeamAndSeason(int teamId, int seasonId)
        {
            return db.Leagues.Include("LeaguesPrices").Where(l => l.LeagueTeams.Any(lt => lt.TeamId == teamId && lt.SeasonId == seasonId));
        }

        public IList<LeagueShort> GetByTeamAndSeasonShort(int teamId, int seasonId)
        {
            return GetByTeamAndSeason(teamId, seasonId).Select(l => new LeagueShort
            {
                Id = l.LeagueId,
                Name = l.Name,
                UnionId = l.UnionId,
                Check = true,
                IsEilatTournament = l.EilatTournament == true
            }).ToList();
        }

        public IList<LeagueShort> GetByTeamAndSeasonForTennisLeaguesShort(int teamId, int seasonId)
        {
            var leagueRegistrations = db.TeamRegistrations.Where(tr => tr.TeamId == teamId && !tr.Team.IsArchive && tr.SeasonId == seasonId && !tr.IsDeleted);
            IList<LeagueShort> list = new List<LeagueShort>();
            foreach (var leageRegistration in leagueRegistrations)
            {
                list.Add(new LeagueShort
                {
                    Id = leageRegistration.LeagueId,
                    Name = leageRegistration.League.Name,
                    UnionId = leageRegistration.League.UnionId,
                    Check = true,
                    IsEilatTournament = leageRegistration.League.EilatTournament == true
                });
            }
            return list;
        }

        private string CreateTeamTitle(Team team, int? seasonId)
        {
            var leagueTitles = team.LeagueTeams.Where(x => x.SeasonId == seasonId).Select(l => l.Leagues.Name).ToList();

            var teamName = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)?.TeamName ?? team.Title;

            return $"{teamName} ({string.Join(", ", leagueTitles)})";
        }

        public IEnumerable<ListItemDto> FindTeamsByNameAndSection(string name, int? sectionId, int num, int? leagueId, int? seasonId)
        {
            var filteredTeams = db.Teams.Include(t => t.TeamsDetails)
                   .Where(t => t.IsArchive == false
                               && (t.Title.Contains(name)
                                   || t.TeamsDetails.Any(td => td.TeamName.Contains(name) && td.SeasonId == seasonId))
                               && (!sectionId.HasValue
                                   || t.LeagueTeams.Any(lt => lt.SeasonId == seasonId && (lt.Leagues.Union.SectionId == sectionId
                                                              || t.ClubTeams.Any(ct => ct.Club.Union.SectionId == sectionId && ct.SeasonId == seasonId)
                                                              || t.ClubTeams.Any(ct => ct.Club.SectionId == sectionId && ct.SeasonId == seasonId))))
                   );

            if (leagueId.HasValue)
            {
                var leagueTeams = db.LeagueTeams.Where(t => t.LeagueId == leagueId && t.Teams.IsArchive == false && t.SeasonId == seasonId)
                    .Select(t => t.TeamId)
                    .ToList();

                filteredTeams = filteredTeams.Where(t => leagueTeams.Contains(t.TeamId));
            }

            var teams = filteredTeams
                .OrderBy(t => t.Title)
                .Take(num)
                .ToList();

            var dtos = new List<ListItemDto>();
            if (teams.Count > 0)
            {
                foreach (var team in teams)
                {
                    var teamName = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)?.TeamName ??
                                   team.Title;

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

            return dtos;
        }

        public List<League> LeaguesForDuplicate(int unionId, int seasonId, bool isTennisCompetition)
        {
            var list = new List<League>();
            if (unionId > 0 && isTennisCompetition)
            {
                list = db.Leagues.AsNoTracking()
                    .Where(x => x.UnionId == unionId &&
                                x.SeasonId == seasonId &&
                                x.EilatTournament == true &&
                                !x.DuplicatedLeagueId.HasValue &&
                                !x.IsArchive)
                    .ToList();
            }
            return list;
        }

        public IQueryable<League> GetQuery(bool isArchive)
        {
            return db.Leagues.Where(t => t.IsArchive == isArchive);
        }

        public IQueryable<League> GetLastSeasonLeaguesBySection(int sectionId)
        {
            var allLeagues = db.Unions.Where(u => u.SectionId == sectionId && !u.IsArchive)
                    .Join(db.Seasons, u => u.UnionId, s => s.UnionId,
                        (u, s) => new { unionId = u.UnionId, seasonId = s.Id })
                    //  Get last season in each union 
                    .GroupBy(us => us.unionId, us => us.seasonId,
                        (key, g) => new UnionSeason { UnionId = key, SeasonId = g.Max() })
                    .Join(db.Leagues,
                        us => new UnionSeason { UnionId = us.UnionId, SeasonId = us.SeasonId },
                        l => new UnionSeason { UnionId = l.UnionId, SeasonId = l.SeasonId }, (us, l) => l);
            return allLeagues;
        }

        public IEnumerable<CompetitionLevel> GetCompetitionLevels(int? unionId)
        {
            if (unionId.HasValue)
            {
                var unionLevels = db.CompetitionLevels.Where(l => l.UnionId == unionId.Value);
                foreach (var level in unionLevels) yield return level;
            }
        }

        public List<League> GetByUnion(int unionId, int seasonId)
        {
            return db.Leagues
                .Include(t => t.Gender)
                .Include(t => t.Age)
                .Include(t => t.Discipline)
                .Include(t => t.Auditorium)
                .Include(t => t.CompetitionRegistrations)
                .Include(t => t.CompetitionDisciplines)
                .Where(t => t.IsArchive == false && t.UnionId == unionId && t.SeasonId == seasonId)
                .ToList();
        }


        private static string GetGenderCharById(int genderId)
        {
            switch (genderId)
            {
                case 0:
                    return "F";
                case 1:
                    return "M";
                default:
                    return "M/F";
            }
        }

        public List<IGrouping<string, CompetitionDisciplineRegistration>> GetAthleticsLeagueClubResults(int clubId, int leagueId, int seasonId, int? genderId, int? leagueType)
        {
            var league = db.Leagues.Include("CompetitionDisciplines").Include("CompetitionDisciplines.CompetitionDisciplineRegistrations").FirstOrDefault(l => l.LeagueId == leagueId);
            var registrations = new List<CompetitionDisciplineRegistration>();
            foreach (var competitionDiscipline in league.CompetitionDisciplines.Where(cd => (!genderId.HasValue || cd.CompetitionAge.gender == genderId)))
            {

                var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == competitionDiscipline.DisciplineId);
                var allRegsForCD = competitionDiscipline.CompetitionDisciplineRegistrations.ToList();
                if (competitionDiscipline.IsResultsManualyRanked)
                {
                    var resulted = allRegsForCD.Where(x => x.CompetitionResult.Count() > 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().Rank ?? int.MaxValue).ToList();
                    allRegsForCD = resulted;
                }
                else if (discipline.Format != null && ((discipline.Format.Value >= 6 && discipline.Format.Value <= 8) || discipline.Format.Value == 10 || discipline.Format.Value == 11))
                {
                    var resulted = allRegsForCD.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().SortValue);
                    if (discipline.Format.Value == 7 || discipline.Format.Value == 10 || discipline.Format.Value == 11)
                    {
                        resulted = resulted.ThenByDescending(r => r.GetThrowingsOrderPower());
                    }
                    if (discipline.Format.Value == 6)
                    {
                        resulted = resulted.ThenByDescending(r => r.CompetitionResult.FirstOrDefault()?.LiveSplitOrder ?? int.MinValue);
                    }
                    var res = resulted.Union(allRegsForCD).ToList();
                    allRegsForCD = res;
                }
                else
                {
                    var resulted = allRegsForCD.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().SortValue);
                    var res = resulted.Union(allRegsForCD).ToList();
                    allRegsForCD = res;
                }
                allRegsForCD = allRegsForCD.Where(x => x.CompetitionResult.Count() > 0 && !string.IsNullOrWhiteSpace(x.CompetitionResult.FirstOrDefault().Result)).ToList();
                for (int i = 0; i < allRegsForCD.Count; i++)
                {
                    var reg = allRegsForCD.ElementAt(i);
                    reg.TempRank = i + 1;
                }
                
                var regsOfClub = allRegsForCD.Where(r => r.ClubId == clubId && !r.IsArchive).ToList();

                var genderChar = GetGenderCharById(competitionDiscipline.CompetitionAge.gender ?? 2);
                var groupName = $"{discipline.Name} - {genderChar}";
                if (genderId.HasValue)
                {
                    groupName = discipline.Name;
                }
                var isCombinedDiscipline = !string.IsNullOrWhiteSpace(discipline.DisciplineType) && (discipline.DisciplineType == "decathlon" || discipline.DisciplineType == "heptathlon");
                regsOfClub.ForEach(r => { r.GroupName = groupName; r.isCombinedDiscipline = isCombinedDiscipline; });
                registrations.AddRange(regsOfClub);
            }

            var grouped = registrations.GroupBy(r => r.GroupName).ToList();
            return grouped;
        }



        private void UpdateSpecificCompetitionInAthleticLeagues(List<Club> allClubsInvolved, int genderId, int typeId,
            string clubsStr, int competitionId, int seasonId, List<CompetitionDisciplineRegistration> validRegistrations,
            bool isFieldCompetition = false, bool isAdultLeague = false, int leagueType = -1)
        {
            var clubIds = clubsStr?.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(int.Parse).ToList()
                          ?? new List<int>();

            var clubsInvolved = allClubsInvolved.Where(c => clubIds.Contains(c.ClubId)).ToList();

            foreach (var club in clubsInvolved)
            {
                var chosenRegs = new List<CompetitionDisciplineRegistration>();
                //var discChosen = new Dictionary<string, int>();

                var clubRegistrations = validRegistrations
                    .Where(r => r.ClubId == club.ClubId &&
                                r.User.GenderId == genderId)
                    .ToList();
                clubRegistrations.Where(r => r.CompetitionResult.Count >0 ).ToList().ForEach(r => r.CompetitionResult.FirstOrDefault().IsPointsAddedToClub = false);
                if (competitionId == 3029 || competitionId == 3030)
                {
                    var combinationsList = new List<string>();
                    //TEMPORARY SOLUTION. It is a shame to write it, but it is necessary
                    if (genderId == 0) //WOMEN
                    {
                        /* WOMENS COMBINATIONS:
                         *
                         * 100mh + 400m
                         * hammer_throw + javelin_throw
                         * 1500m
                         * high_jump
                         */
                        combinationsList.AddRange(new List<string>
                        {
                            "100mh+400m",
                            "hammer_throw+javelin_throw",
                            "1500m",
                            "high_jump"
                        });
                    }
                    else //MEN
                    {
                        /*
                         * MENS COMBINATIONS:
                         *
                         * 110mh + 400m
                         * hammer_throw + javelin_throw
                         * 1500m
                         * long_jump
                         */
                        combinationsList.AddRange(new List<string>
                        {
                            "110mh+400m",
                            "hammer_throw+javelin_throw",
                            "1500m",
                            "long_jump"
                        });
                    }

                    if (!combinationsList.Any())
                    {
                        return;
                    }

                    foreach (var combination in combinationsList)
                    {
                        var combinationResults = new List<CompetitionDisciplineRegistration>();
                        var combinationTypes = combination.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var type in combinationTypes)
                        {
                            combinationResults.AddRange(clubRegistrations.Where(x =>
                                x.CompetitionResult.FirstOrDefault()?.DisciplineType == type));
                        }

                        var combinationTopRegistration = combinationResults
                            .OrderByDescending(x => x.CompetitionResult.FirstOrDefault()?.ClubPoints)
                            .FirstOrDefault();

                        var combinationTopResult = combinationTopRegistration?.CompetitionResult
                            ?.FirstOrDefault();

                        if (combinationTopRegistration == null || combinationTopResult == null)
                        {
                            continue;
                        }
                        combinationTopResult.PointsAfterPenalty = combinationTopResult.ClubPoints ?? 0;
                        chosenRegs.Add(combinationTopRegistration);
                    }
                }
                else if (competitionId == 3035 || competitionId == 3036)
                {
                    var combinationsList = new List<string>();
                    combinationsList.AddRange(new List<string>
                    {
                            "400mh+200m",
                            "800m",
                            "shot_put+discus_throw",
                            "triple_jump+pole_vault",
                            "4x100m"
                    });

                    if (!combinationsList.Any())
                    {
                        return;
                    }

                    foreach (var combination in combinationsList)
                    {
                        var combinationResults = new List<CompetitionDisciplineRegistration>();
                        var combinationTypes = combination.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var type in combinationTypes)
                        {
                            combinationResults.AddRange(clubRegistrations.Where(x =>
                                x.CompetitionResult.FirstOrDefault()?.DisciplineType == type));
                        }

                        var combinationTopRegistration = combinationResults
                            .OrderByDescending(x => x.CompetitionResult.FirstOrDefault()?.ClubPoints)
                            .FirstOrDefault();

                        var combinationTopResult = combinationTopRegistration?.CompetitionResult
                            ?.FirstOrDefault();

                        if (combinationTopRegistration == null || combinationTopResult == null)
                        {
                            continue;
                        }
                        combinationTopResult.PointsAfterPenalty = combinationTopResult.ClubPoints ?? 0;
                        chosenRegs.Add(combinationTopRegistration);
                    }
                }
                else
                {
                    if (isAdultLeague)
                    {
                        var athletesRegisteredWithResult = clubRegistrations
                                                    .OrderByDescending(r => r.CompetitionResult.FirstOrDefault()?.ClubPoints)
                                                    .Where(r => r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault()?.DisciplineType) && (
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "100mh" || 
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "110mh" ||
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "400mh" ||
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "3000mSt" ||
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "4x100m" ||
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "4x400m" ||
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "javelin_throw" ||
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "hammer_throw" ||
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "shot_put" ||
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "discus_throw" ||
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "heptathlon" ||
                                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "decathlon"
                                                    ))
                                                    .ToList();

                        var resultsToTake = 2;
                        athletesRegisteredWithResult = athletesRegisteredWithResult.Take(resultsToTake).ToList();

                        foreach (var reg in athletesRegisteredWithResult)
                        {
                            var competitionResult = reg.CompetitionResult.FirstOrDefault();
                            competitionResult.PointsAfterPenalty = competitionResult.ClubPoints ?? 0;
                            chosenRegs.Add(reg);
                        }
                    }
                    else
                    {
                        if (!isFieldCompetition)
                        {
                            var athletesRegisteredWithResult = clubRegistrations
                            .OrderByDescending(r => r.CompetitionResult.FirstOrDefault()?.ClubPoints)
                            .GroupBy(r => r.CompetitionResult.FirstOrDefault()?.DisciplineType)
                            .ToList();

                            var resultsToTake =
                                athletesRegisteredWithResult.Any(x => x.Key == "4x100m" ||
                                                                      x.Key == "4x200m" ||
                                                                      x.Key == "4x400m")
                                    ? 5
                                    : 4;





                            athletesRegisteredWithResult = athletesRegisteredWithResult.Take(resultsToTake).ToList();

                            foreach (var group in athletesRegisteredWithResult)
                            {
                                var competitionResult = group.First().CompetitionResult.FirstOrDefault();
                                if (competitionResult == null)
                                {
                                    continue;
                                }

                                competitionResult.PointsAfterPenalty = competitionResult.ClubPoints ?? 0;
                                chosenRegs.Add(group.First());
                                //if (!discChosen.ContainsKey(competitionResult.DisciplineType))
                                //{
                                //    discChosen[competitionResult.DisciplineType] = 0;
                                //}

                                //discChosen[competitionResult.DisciplineType] += 1;
                            }
                        }
                    }
                }

                var athletesRegisteredWithResultList = validRegistrations
                    .Where(r => r.ClubId == club.ClubId &&
                                r.User.GenderId == genderId)
                    .OrderByDescending(r => r.CompetitionResult.FirstOrDefault()?.ClubPoints)
                    .ToList();

                var maxExtra = isFieldCompetition ? 5 : 7;
                var indexAdded = 0;

                if (isAdultLeague)
                {
                    maxExtra = 10;
                    var normalMinimum = 600;
                    var requiredMinimum = 450;
                    if (leagueType > 2)
                    {
                        maxExtra = 8;
                    }
                    if (leagueType == 2)
                    {
                        normalMinimum = 550;
                        requiredMinimum = 400;
                    }
                    if (leagueType == 3)
                    {
                        normalMinimum = 500;
                        requiredMinimum = 350;
                    }
                    if (leagueType == 4)
                    {
                        normalMinimum = 450;
                        requiredMinimum = 300;
                    }
                    if(genderId == 0)
                    {
                        maxExtra -= 2;
                    }
                    var requiredDisciplinesRegs = athletesRegisteredWithResultList.Where(r => r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault()?.DisciplineType) && (
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "100mh" ||
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "110mh" ||
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "400mh" ||
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "3000mSt" ||
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "4x100m" ||
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "4x400m" ||
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "javelin_throw" ||
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "hammer_throw" ||
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "shot_put" ||
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "discus_throw" ||
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "heptathlon" ||
                            r.CompetitionResult.FirstOrDefault()?.DisciplineType == "decathlon"
                    )).ToList();

                    var normalDisciplinesRegs = athletesRegisteredWithResultList.Except(requiredDisciplinesRegs).ToList();
                    var requiredUnPicked = requiredDisciplinesRegs.Except(chosenRegs).ToList();

                    var regsLeft = new List<CompetitionDisciplineRegistration>();
                    
                    foreach (var reg in normalDisciplinesRegs)
                    {
                        var competitionResult = reg.CompetitionResult.FirstOrDefault();
                        if (competitionResult == null)
                        {
                            continue;
                        }
                        regsLeft.Add(reg);
                        if (competitionResult.ClubPoints < normalMinimum)
                        {
                            competitionResult.PointsAfterPenalty = 0;
                        }
                        else
                        {
                            competitionResult.PointsAfterPenalty = competitionResult.ClubPoints ?? 0;
                        }
                    }

                    var counterDic = new Dictionary<int, int>();
                    foreach (var reg in regsLeft.OrderByDescending(r => r.CompetitionResult.FirstOrDefault().ClubPoints))
                    {
                        int count;
                        bool found = counterDic.TryGetValue(reg.CompetitionDisciplineId, out count);
                        if (!found)
                        {
                            count = 1;
                        }
                        else
                        {
                            counterDic.Remove(reg.CompetitionDisciplineId);
                            count++;
                        }
                        counterDic.Add(reg.CompetitionDisciplineId, count);
                        var competitionResult = reg.CompetitionResult.FirstOrDefault();
                        if (count > 7)
                        {
                            competitionResult.PointsAfterPenalty = 0;
                        }
                    }
                    foreach (var reg in requiredUnPicked)
                    {
                        var competitionResult = reg.CompetitionResult.FirstOrDefault();
                        if (competitionResult == null)
                        {
                            continue;
                        }
                        regsLeft.Add(reg);
                        if (competitionResult.ClubPoints < requiredMinimum)
                        {
                            competitionResult.PointsAfterPenalty = 0;
                        }
                        else
                        {
                            competitionResult.PointsAfterPenalty = competitionResult.ClubPoints ?? 0;
                        }
                    }
                    foreach (var reg in regsLeft.OrderByDescending(x => x.CompetitionResult.FirstOrDefault().PointsAfterPenalty))
                    {
                        chosenRegs.Add(reg);
                        indexAdded += 1;
                        if (indexAdded == maxExtra)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var reg in athletesRegisteredWithResultList)
                    {
                        var competitionResult = reg.CompetitionResult.FirstOrDefault();
                        if (competitionResult == null)
                        {
                            continue;
                        }

                        if (chosenRegs.Any(r => r.Id == reg.Id)) // || !discChosen.ContainsKey(competitionResult.DisciplineType) || discChosen[competitionResult.DisciplineType] >= 7)
                        {
                            //skip already counted registrations
                            continue;
                        }

                        chosenRegs.Add(reg);
                        if (competitionResult.ClubPoints < 350 && !isFieldCompetition)
                        {
                            competitionResult.PointsAfterPenalty = 0;
                        }
                        else
                        {
                            competitionResult.PointsAfterPenalty = competitionResult.ClubPoints ?? 0;
                        }
                        indexAdded += 1;
                        if (indexAdded == maxExtra)
                        {
                            break;
                        }
                    }

                    var groupedByUserRegistrations = chosenRegs.GroupBy(r => r.UserId);
                    foreach (var group in groupedByUserRegistrations)
                    {
                        if (group.Count() > 2)
                        {
                            var participationNoRelays = group
                                .Where(r => r.CompetitionResult.FirstOrDefault()?.DisciplineType != "4x100m" &&
                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType != "4x200m" &&
                                            r.CompetitionResult.FirstOrDefault()?.DisciplineType != "4x400m")
                                .ToList();
                            var countParticipation = participationNoRelays.Count();
                            if (countParticipation > 2)
                            {
                                var zeroingCount = countParticipation - 2;
                                for (var i = 0; i < zeroingCount; i++)
                                {
                                    var competitionResult = participationNoRelays[i].CompetitionResult.FirstOrDefault();
                                    if (competitionResult == null)
                                    {
                                        continue;
                                    }

                                    competitionResult.PointsAfterPenalty = 0;
                                }
                            }
                        }
                    }
                }
                var clubPoints = chosenRegs.Sum(x => x.CompetitionResult.FirstOrDefault()?.PointsAfterPenalty ?? 0);
                var countedRegs = chosenRegs.Where(x => x.CompetitionResult.FirstOrDefault()?.PointsAfterPenalty > 0).ToList();
                countedRegs.ForEach(r => r.CompetitionResult.FirstOrDefault().IsPointsAddedToClub = true); 
                var resultsCounted = countedRegs.Count();

                var correction = db.CompetitionClubsCorrections
                    .FirstOrDefault(c => c.LeagueId == competitionId &&
                                         c.SeasonId == seasonId &&
                                         c.ClubId == club.ClubId &&
                                         c.GenderId == genderId &&
                                         c.TypeId == typeId);
                if (correction == null)
                {
                    db.CompetitionClubsCorrections.Add(new CompetitionClubsCorrection
                    {
                        LeagueId = competitionId,
                        SeasonId = seasonId,
                        Points = clubPoints,
                        ClubId = club.ClubId,
                        Correction = 0,
                        GenderId = genderId,
                        TypeId = typeId,
                        ResultsCounted = resultsCounted
                    });
                }
                else
                {
                    correction.Points = clubPoints;
                    correction.ResultsCounted = resultsCounted;
                }
            }
        }





        private void UpdateSpecificCompetitionGoldenSpikeClubRanks(List<Club> allClubsInvolved, int typeId, int genderId, int competitionId, int seasonId, List<CompetitionDisciplineRegistration> validRegistrations)
        {
            var clubsInvolved = allClubsInvolved.ToList();
            var spikeType = typeId == 1 ? "U14" : "U16";
            foreach (var club in clubsInvolved)
            {
                var chosenRegs = new List<CompetitionDisciplineRegistration>();

                var clubRegistrations = validRegistrations
                    .Where(r => r.ClubId == club.ClubId &&
                                r.User.GenderId == genderId)
                    .ToList();

                var runningDisciplines = new List<string> { "50m", "60m", "100m", "80m", "300m", "600m", "1000m", "2000m", "60mh", "50mh", "80mh", "100mh", "250mh" };
                var jumpingDisciplines = new List<string> { "high_jump", "pole_vault", "long_jump", "triple_jump"};
                var throwingDisciplines = new List<string> { "shot_put", "discus_throw", "hockey_ball", "hammer_throw", "javelin_throw" };


                var athletesRegisteredWithResultForRunningList = validRegistrations
                    .Where(r => r.ClubId == club.ClubId && runningDisciplines.Contains(r.CompetitionResult.FirstOrDefault()?.DisciplineType)  && 
                                r.User.GenderId == genderId)
                    .OrderByDescending(r => r.CompetitionResult.FirstOrDefault()?.ClubPoints)
                    .ToList();
                var athletesRegisteredWithResultForJumpingList = validRegistrations
                    .Where(r => r.ClubId == club.ClubId && jumpingDisciplines.Contains(r.CompetitionResult.FirstOrDefault()?.DisciplineType) &&
                                r.User.GenderId == genderId)
                    .OrderByDescending(r => r.CompetitionResult.FirstOrDefault()?.ClubPoints)
                    .ToList();
                var athletesRegisteredWithResultForThrowingList = validRegistrations
                    .Where(r => r.ClubId == club.ClubId && throwingDisciplines.Contains(r.CompetitionResult.FirstOrDefault()?.DisciplineType) &&
                                r.User.GenderId == genderId)
                    .OrderByDescending(r => r.CompetitionResult.FirstOrDefault()?.ClubPoints)
                    .ToList();

                chosenRegs.Add(athletesRegisteredWithResultForRunningList.FirstOrDefault());
                chosenRegs.Add(athletesRegisteredWithResultForJumpingList.FirstOrDefault());
                chosenRegs.Add(athletesRegisteredWithResultForThrowingList.FirstOrDefault());
                if (spikeType == "U16")
                {
                    clubRegistrations = clubRegistrations.Except(chosenRegs).OrderByDescending(r => r.CompetitionResult.FirstOrDefault()?.ClubPoints).ToList();
                    chosenRegs.Add(clubRegistrations.FirstOrDefault());
                }

                var clubPoints = chosenRegs.Where(x => x != null).Sum(x => x.CompetitionResult.FirstOrDefault()?.ClubPoints ?? 0);

                var correction = db.CompetitionClubsCorrections
                    .FirstOrDefault(c => c.LeagueId == competitionId &&
                                         c.SeasonId == seasonId &&
                                         c.ClubId == club.ClubId &&
                                         c.GenderId == genderId &&
                                         c.TypeId == typeId+10);
                if (correction == null)
                {
                    db.CompetitionClubsCorrections.Add(new CompetitionClubsCorrection
                    {
                        LeagueId = competitionId,
                        SeasonId = seasonId,
                        Points = clubPoints,
                        ClubId = club.ClubId,
                        Correction = 0,
                        GenderId = genderId,
                        TypeId = typeId + 10
                    });
                }
                else
                {
                    correction.Points = clubPoints;
                }
            }
        }







        public List<Tuple<string, List<Tuple<string, int, int>>>> GetFieldRaceTables(League competition)
        {
            var competitionDisiplines = competition.CompetitionDisciplines.Where(c => !c.IsDeleted).ToList();
            var fieldRaceTables = new List<Tuple<string, List<Tuple<string, int, int>>>>();
            foreach (var competitionDisipline in competitionDisiplines)
            {

                var disciplinesToCalculateAsResultsOnly = new List<string> { "גברים", "נשים", "נערים", "נערות" };

                List<CompetitionDisciplineRegistration> orderedRegistrations;
                List<IGrouping<string, CompetitionDisciplineRegistration>> registrationsGroupedByClubEn;
                if (disciplinesToCalculateAsResultsOnly.Contains(competitionDisipline.CompetitionAge.age_name))
                {
                    List<CompetitionDisciplineRegistration> res = new List<CompetitionDisciplineRegistration>();
                    var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == competitionDisipline.DisciplineId.Value);
                    if (competitionDisipline.IsResultsManualyRanked)
                    {
                        res = competitionDisipline.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().Rank ?? int.MaxValue).ToList();
                    }
                    else if (discipline.Format != null && ((discipline.Format.Value >= 6 && discipline.Format.Value <= 8) || discipline.Format.Value == 10 || discipline.Format.Value == 11))
                    {
                        var resulted = competitionDisipline.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().SortValue);
                        if (discipline.Format.Value == 7 || discipline.Format.Value == 10 || discipline.Format.Value == 11)
                        {
                            resulted = resulted.ThenByDescending(r => r.GetThrowingsOrderPower());
                        }
                        if (discipline.Format.Value == 6)
                        {
                            resulted = resulted.ThenByDescending(r => r.CompetitionResult.FirstOrDefault()?.LiveSplitOrder ?? int.MinValue);
                        }
                        res = resulted.Union(competitionDisipline.CompetitionDisciplineRegistrations).ToList();
                    }
                    else
                    {
                        var resulted = competitionDisipline.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().SortValue).ThenByDescending(r => r.CompetitionResult.FirstOrDefault()?.LiveSplitOrder ?? int.MinValue);
                        res = resulted.Union(competitionDisipline.CompetitionDisciplineRegistrations).ToList();
                    }
                    orderedRegistrations = res;
                    registrationsGroupedByClubEn = orderedRegistrations.Where(r => r.CompetitionResult.FirstOrDefault() != null && r.CompetitionResult.FirstOrDefault().AlternativeResult == 0 && !string.IsNullOrEmpty(r.CompetitionResult.FirstOrDefault().Result)).GroupBy(r => r.Club.Name).ToList();
                }
                else
                {
                    orderedRegistrations = competitionDisipline.CompetitionDisciplineRegistrations.Where(r => !r.IsArchive && r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Result) && r.CompetitionResult.FirstOrDefault().AlternativeResult == 0 && r.CompetitionResult.FirstOrDefault().ClubPoints != null).OrderBy(r => r.CompetitionResult.FirstOrDefault().SortValue).OrderByDescending(r => r.CompetitionResult.FirstOrDefault().ClubPoints).ToList();
                    registrationsGroupedByClubEn = competitionDisipline.CompetitionDisciplineRegistrations.Where(r => !r.IsArchive && r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Result) && r.CompetitionResult.FirstOrDefault().AlternativeResult == 0 && r.CompetitionResult.FirstOrDefault().ClubPoints != null).OrderBy(r => r.CompetitionResult.FirstOrDefault().SortValue).GroupBy(r => r.Club.Name).ToList();
                }

                List<IGrouping<string, CompetitionDisciplineRegistration>> registrationsGroupedByClub = new List<IGrouping<string, CompetitionDisciplineRegistration>>();
                foreach (var regByClub in registrationsGroupedByClubEn)
                {
                    if (regByClub.Count() >= 3)
                    {
                        registrationsGroupedByClub.Add(regByClub);
                    }
                }

                var clubsForDisciplineCompetition = new List<Tuple<string, int, int>>();
                foreach (var regByClub in registrationsGroupedByClub)
                {
                    var sum = 0;
                    var orderOfThirdPlace = -1;
                    for (int i = 0; i < regByClub.Count() && i < 3; i++)
                    {
                        var order = orderedRegistrations.FindIndex(r => r.Id == regByClub.ElementAt(i).Id);
                        order++;
                        sum += order;
                        if (i == 2)
                        {
                            orderOfThirdPlace = order;
                        }
                    }
                    clubsForDisciplineCompetition.Add(new Tuple<string, int, int>(regByClub.Key, sum, orderOfThirdPlace));
                    clubsForDisciplineCompetition = clubsForDisciplineCompetition.OrderBy(c => c.Item2).ThenBy(c => c.Item3).ToList();
                }

                if (registrationsGroupedByClub.Count() > 0)
                {
                    fieldRaceTables.Add(new Tuple<string, List<Tuple<string, int, int>>>(competitionDisipline.CompetitionAge.age_name, clubsForDisciplineCompetition));
                }

            }
            return fieldRaceTables;
        }




        







        public void UpdateAthleticLeagueCompetitionRanks(int competitionId, int seasonId, bool isFullRanks = true)
        {
            var vm = new List<List<CompetitionClubRankedStanding>>();
            var clubsInvolved = new List<Club>();
            var validRegistrations = new List<CompetitionDisciplineRegistration>();
            var validCombinedRegistrations = new List<CompetitionDisciplineRegistration>();
            var competition = db.Leagues.Include(l => l.CompetitionDisciplines).FirstOrDefault(l => l.LeagueId == competitionId);
            var combinedDisciplinesList = new List<CompetitionDiscipline>();
            var competitionsNotDeleted = competition.CompetitionDisciplines.Where(c => !c.IsDeleted).ToList();
            var goldeSpikeDisciplineType = 0;
            foreach (var competitionDiscipline in competitionsNotDeleted)
            {
                var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == competitionDiscipline.DisciplineId);
                if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && discipline.DisciplineType == "GoldenSpikesU14" )
                {
                    goldeSpikeDisciplineType = 1;
                    break;
                }
                if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && discipline.DisciplineType == "GoldenSpikesU16")
                {
                    goldeSpikeDisciplineType = 2;
                    break;
                }
            }
            if (goldeSpikeDisciplineType == 0)
            {
                foreach (var competitionDiscipline in competitionsNotDeleted)
                {
                    var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == competitionDiscipline.DisciplineId);
                    var registrations = competitionDiscipline.CompetitionDisciplineRegistrations.Where(p => !p.IsArchive && p.CompetitionResult.Count() > 0);
                    var discValidRegistrations = registrations.Where(r => r.CompetitionResult.FirstOrDefault().AlternativeResult == 0).ToList();

                    if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && (discipline.DisciplineType == "decathlon" || discipline.DisciplineType == "heptathlon"))
                    {
                        foreach (var registration in discValidRegistrations)
                        {
                            var result = registration.CompetitionResult.FirstOrDefault();
                            result.DisciplineType = discipline.DisciplineType;
                            if (competitionDiscipline.IsForScore)
                            {
                                clubsInvolved.Add(registration.Club);
                                validRegistrations.Add(registration);
                            }
                        }
                        combinedDisciplinesList.Add(competitionDiscipline);
                        continue;
                    }

                    foreach (var registration in discValidRegistrations)
                    {
                        clubsInvolved.Add(registration.Club);
                        var result = registration.CompetitionResult.FirstOrDefault();
                        var resultDouble = IAAFScoringPointsService.GetResultForCalculation(result.SortValue, discipline.Format);
                        if (string.IsNullOrWhiteSpace(discipline.DisciplineType))
                        {
                            //for manually pointed disciplines to add for combined discipline calculation.
                            if (competitionDiscipline.IsMultiBattle && registration.CompetitionResult.FirstOrDefault().CombinedPoint.HasValue && registration.CompetitionResult.FirstOrDefault().CombinedPoint.Value >= 0)
                            {
                                validCombinedRegistrations.Add(registration);
                            }
                            resultDouble = -1;
                        }
                        if (resultDouble > -1)
                        {
                            var venue = IAAFScoringPointsService.OutdoorsVenue;
                            string[] fieldDisciplineTypes = new[] { "1900m_13_field", "1100m_field", "2500m_field", "1900m_15_field", "4500m_field", "3100m_field" };
                            if (fieldDisciplineTypes.Contains(discipline.DisciplineType))
                            {
                                venue = IAAFScoringPointsService.FieldsVenue;
                            }
                            var points = IAAFScoringPointsService.getPoints(resultDouble, IAAFScoringPointsService.DefaultEdition, venue, competitionDiscipline.CompetitionAge.gender.Value, discipline.DisciplineType);
                            int? combinePoint = 0;
                            if (competitionDiscipline.IsMultiBattle)
                            {
                                validCombinedRegistrations.Add(registration);
                                combinePoint = IAAFScoringPointsService.getPointsCombined(resultDouble, IAAFScoringPointsService.DefaultEdition, IAAFScoringPointsService.OutdoorsVenue, competitionDiscipline.CompetitionAge.gender.Value, discipline.DisciplineType);
                                if (combinePoint >= 0)
                                    result.CombinedPoint = combinePoint;
                            }
                            if (points >= 0)
                            {
                                result.ClubPoints = points;
                                result.DisciplineType = discipline.DisciplineType;
                                if (competitionDiscipline.IsForScore)
                                {
                                    validRegistrations.Add(registration);
                                }
                            }
                        }

                    }
                }
            }
            else
            {
                var goldenSpikeType = goldeSpikeDisciplineType == 1 ? "U14" : "U16";
                GoldenSpikeData goldenSpikeData = GoldenSpikeData.ParseGoldenSpikesExcelFile();
                var goldenSpikeForPointsDisciplinesList = new List<CompetitionDiscipline>();
                foreach (var competitionDiscipline in competitionsNotDeleted)
                {
                    var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == competitionDiscipline.DisciplineId);
                    if (string.IsNullOrWhiteSpace(discipline.DisciplineType) || (discipline.DisciplineType != "GoldenSpikesU14" && discipline.DisciplineType != "GoldenSpikesU16"))
                    {
                        goldenSpikeForPointsDisciplinesList.Add(competitionDiscipline);
                    }
                }

                foreach (var competitionDiscipline in goldenSpikeForPointsDisciplinesList)
                {
                    var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == competitionDiscipline.DisciplineId);
                    var registrations = competitionDiscipline.CompetitionDisciplineRegistrations.Where(p => !p.IsArchive && p.CompetitionResult.Count() > 0);
                    var discValidRegistrations = registrations.ToList();

                    foreach (var registration in discValidRegistrations)
                    {
                        clubsInvolved.Add(registration.Club);
                        var result = registration.CompetitionResult.FirstOrDefault();
                        var resultDouble = IAAFScoringPointsService.GetResultForCalculation(result.SortValue, discipline.Format);
                        if (string.IsNullOrWhiteSpace(discipline.DisciplineType))
                        {
                            if (competitionDiscipline.IsMultiBattle)
                            {
                                validRegistrations.Add(registration);
                            }
                            resultDouble = -1;
                        }
                        if (resultDouble > -1)
                        {
                            var points = goldenSpikeData.GetPoint(result.SortValue, discipline.DisciplineType, competitionDiscipline.CompetitionAge.gender.Value, goldenSpikeType);
                            if (points >= 0)
                            {
                                if (result.AlternativeResult > 0)
                                {
                                    result.ClubPoints = 0;
                                    result.CombinedPoint = 0;
                                }
                                else
                                {
                                    result.ClubPoints = points;
                                    result.CombinedPoint = points;
                                }
                                result.DisciplineType = discipline.DisciplineType;
                                if (competitionDiscipline.IsMultiBattle)
                                {
                                    validRegistrations.Add(registration);
                                }
                            }
                        }
                    }
                }

            }
            db.SaveChanges();


            foreach (var combinedDiscipline in combinedDisciplinesList)
            {
                if (combinedDiscipline.IsManualPointCalculation != true)
                {
                    UpdateCombinedDisciplinesResults(combinedDiscipline, validCombinedRegistrations);
                }
            }


            if (!isFullRanks)
                return;

            clubsInvolved = clubsInvolved.GroupBy(x => x.ClubId).Select(x => x.First()).ToList();


            var competitionOldPoints = db.CompetitionClubsCorrections.Where(c => c.LeagueId == competitionId && c.SeasonId == seasonId && !c.TypeId.HasValue);
            db.CompetitionClubsCorrections.RemoveRange(competitionOldPoints);
            db.SaveChanges();

            if (goldeSpikeDisciplineType > 0)
            {
                // club ranking not needed for goldspikes.
                //UpdateSpecificCompetitionGoldenSpikeClubRanks(clubsInvolved, goldeSpikeDisciplineType, 1, competitionId, seasonId, validRegistrations);
                //UpdateSpecificCompetitionGoldenSpikeClubRanks(clubsInvolved, goldeSpikeDisciplineType, 0, competitionId, seasonId, validRegistrations);
                //db.SaveChanges();
                return;
            }

            for (var i = 1; i < 5; i++)
            {
                switch (i)
                {
                    case 1:
                        {
                            UpdateSpecificCompetitionInAthleticLeagues(clubsInvolved, 1, i, competition.AthleticLeague.AlLeague, competitionId, seasonId, validRegistrations, competition.IsFieldCompetition, competition.AthleticLeague.IsAdultLeague, 1);
                            UpdateSpecificCompetitionInAthleticLeagues(clubsInvolved, 0, i, competition.AthleticLeague.AlLeagueF, competitionId, seasonId, validRegistrations, competition.IsFieldCompetition, competition.AthleticLeague.IsAdultLeague, 1);
                            break;
                        }
                    case 2:
                        {
                            UpdateSpecificCompetitionInAthleticLeagues(clubsInvolved, 1, i, competition.AthleticLeague.PremiereLeague, competitionId, seasonId, validRegistrations, competition.IsFieldCompetition, competition.AthleticLeague.IsAdultLeague, 2);
                            UpdateSpecificCompetitionInAthleticLeagues(clubsInvolved, 0, i, competition.AthleticLeague.PremiereLeagueF, competitionId, seasonId, validRegistrations, competition.IsFieldCompetition, competition.AthleticLeague.IsAdultLeague, 2);
                            break;
                        }
                    case 3:
                        {
                            UpdateSpecificCompetitionInAthleticLeagues(clubsInvolved, 1, i, competition.AthleticLeague.NationalLeague, competitionId, seasonId, validRegistrations, competition.IsFieldCompetition, competition.AthleticLeague.IsAdultLeague, 3);
                            UpdateSpecificCompetitionInAthleticLeagues(clubsInvolved, 0, i, competition.AthleticLeague.NationalLeagueF, competitionId, seasonId, validRegistrations, competition.IsFieldCompetition, competition.AthleticLeague.IsAdultLeague, 3);
                            break;
                        }
                    case 4:
                        {
                            UpdateSpecificCompetitionInAthleticLeagues(clubsInvolved, 1, i, competition.AthleticLeague.AlefLeague, competitionId, seasonId, validRegistrations, competition.IsFieldCompetition, competition.AthleticLeague.IsAdultLeague, 4);
                            UpdateSpecificCompetitionInAthleticLeagues(clubsInvolved, 0, i, competition.AthleticLeague.AlefLeagueF, competitionId, seasonId, validRegistrations, competition.IsFieldCompetition, competition.AthleticLeague.IsAdultLeague, 4);
                            break;
                        }
                }
            }

            db.SaveChanges();
        }

        public void SetIsAdultLeague(int id, bool isAdult)
        {
            var league = db.AthleticLeagues.FirstOrDefault(l => l.Id == id);
            if (league != null)
            {
                league.IsAdultLeague = isAdult;
                Save();
            }
        }
        private void UpdateCombinedDisciplinesResults(CompetitionDiscipline combinedDiscipline, List<CompetitionDisciplineRegistration> validCombinedRegistrations)
        {
            var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == combinedDiscipline.DisciplineId);
            var validRegistrations = combinedDiscipline.CompetitionDisciplineRegistrations.Where(r => !r.IsArchive).ToList();
            foreach (var reg in validRegistrations)
            {
                var totalCombinedPoints = validCombinedRegistrations.Where(r => r.UserId == reg.UserId && r.CompetitionResult.FirstOrDefault() != null && r.CompetitionResult.FirstOrDefault().CombinedPoint != null).Select(r => r.CompetitionResult.FirstOrDefault().CombinedPoint).Sum();
                var score = IAAFScoringPointsService.getPoints(totalCombinedPoints, IAAFScoringPointsService.DefaultEdition, IAAFScoringPointsService.OutdoorsVenue, combinedDiscipline.CompetitionAge.gender.Value, discipline.DisciplineType);
                string resultStr = BuildResultStringFormat8(totalCombinedPoints);
                var result = reg.CompetitionResult.FirstOrDefault();
                if (result == null)
                {
                    reg.CompetitionResult.Add(new CompetitionResult
                    {
                        SortValue = totalCombinedPoints,
                        Result = resultStr,
                        ClubPoints = score,
                        AlternativeResult = 0
                    });
                }
                else
                {
                    result.SortValue = totalCombinedPoints;
                    result.ClubPoints = score;
                    result.Result = resultStr;
                }
            }
            db.SaveChanges();
        }

        private string BuildResultStringFormat8(int? score)
        {
            var scoreStr = score.ToString();
            while (scoreStr.Length < 4)
            {
                scoreStr = "0" + scoreStr;
            }
            if (scoreStr.Length == 4)
            {
                scoreStr = scoreStr.Insert(1, ".");
            }
            return scoreStr;
        }

        public IEnumerable<League> GetByDiscipline(int disciplineId, int seasonId)
        {
            return db.Leagues
                .Include(t => t.Gender)
                .Include(t => t.Age)
                .Where(t => t.IsArchive == false && t.DisciplineId == disciplineId && t.SeasonId == seasonId)
                .ToList();
        }

        public class CombinedDisciplineComparer : IComparer<Discipline>
        {
            private int BattleType;
            private List<string> arr10 = new List<string> { "100m", "long_jump", "shot_put", "high_jump", "400m", "110mh", "discus_throw", "pole_vault", "javelin_throw", "1500m" };
            private List<string> arr7 = new List<string> { "100mh", "high_jump", "shot_put", "200m", "long_jump", "javelin_throw", "800m" };
            private List<string> arrGoldenSpikes = new List<string> { "50m", "60m", "100m", "80m", "300m", "600m", "1000m", "2000m", "60mh", "50mh", "80mh", "100mh", "250mh",
                                                                      "high_jump", "pole_vault", "long_jump", "triple_jump",
                                                                      "shot_put", "discus_throw", "hockey_ball", "hammer_throw", "javelin_throw"
                                                                    };

            public CombinedDisciplineComparer(int battleType)
            {
                this.BattleType = battleType;
            }

            public int Compare(Discipline x, Discipline y)
            {
                List<string> arr;
                if (BattleType == -1)
                {
                    arr = arrGoldenSpikes;
                }
                else if (BattleType == 10)
                {
                    arr = arr10;
                }
                else
                {
                    arr = arr7;
                }

                var index1 = arr.IndexOf(x.DisciplineType);
                var index2 = arr.IndexOf(y.DisciplineType);

                if (index1 > index2)
                    return 1;

                if (index1 < index2)
                    return -1;

                else
                    return 0;
            }
        }



        public class WeightLiftingCategoryComparer : IComparer<string>
        {
            private string ExtractWeightFromString(string str) {

                if (string.IsNullOrWhiteSpace(str))
                {
                    return null;
                }
                string[] separators = { " " };
                var splitedStr = str.Split(separators, StringSplitOptions.None).ToList();
                foreach (var part in splitedStr)
                {
                    if (part.Contains("KG"))
                    {
                        return part.Replace("KG", "");
                    }
                }
                return null;
            }

            private string ExtractTypeFromString(string str)
            {
                string[] separators = { " " };
                var splitedStr = str.Split(separators, StringSplitOptions.None).ToList();
                int i = 0;
                foreach (var part in splitedStr)
                {
                    if (part.Contains("KG"))
                    {
                        return string.Join(" ", splitedStr.Take(i));
                    }
                    i++;
                }
                return str;
            }

            public int Compare(string x, string y)
            {
                if (string.IsNullOrWhiteSpace(x) || string.IsNullOrWhiteSpace(y))
                {
                    string.Compare(x, y);
                }

                var weightByNameX = ExtractWeightFromString(x);
                var weightByNameY = ExtractWeightFromString(y);
                var categoryTypeX = ExtractTypeFromString(x);
                var categoryTypeY = ExtractTypeFromString(y);

                if (categoryTypeX != categoryTypeY)
                {
                    //return string.Compare(categoryTypeX, categoryTypeY);
                }


                if (!string.IsNullOrWhiteSpace(weightByNameX) && !string.IsNullOrWhiteSpace(weightByNameY))
                {
                    var resultC =  int.Parse(weightByNameY) > int.Parse(weightByNameX) ? -1 : (int.Parse(weightByNameY) < int.Parse(weightByNameX) ? 1 : (weightByNameY.Contains("+") && !weightByNameX.Contains("+") ? -1 : (weightByNameX.Contains("+") && !weightByNameY.Contains("+") ? 1 : 0)));
                    return resultC;
                }
                
                if (!string.IsNullOrWhiteSpace(weightByNameY))
                {
                    return -1;
                }
                if (!string.IsNullOrWhiteSpace(weightByNameX))
                {
                    return 1;
                }
                
                return string.Compare(x,y);
            }
        }



        public List<CombinedPlayerRankDto> GetAthleticCombinedRanking(League competition, int goldenSpikeType = 0)
        {
            var competitionsDisciplines = competition.CompetitionDisciplines.Where(cd => cd.IsMultiBattle && !cd.IsDeleted);
            var multiBattleCompetitionsDisciplines = competitionsDisciplines.OrderBy(a => db.Disciplines.FirstOrDefault(t => t.DisciplineId == a.DisciplineId), new CombinedDisciplineComparer(goldenSpikeType == 0 ? competitionsDisciplines.Count() : -1));
            var allMultibattleRegistrations = new List<CompetitionDisciplineRegistration>();
            foreach (var competitionDiscipline in multiBattleCompetitionsDisciplines)
            {
                var regsWithPoints = competitionDiscipline.CompetitionDisciplineRegistrations.Where(r => !r.IsArchive && r.CompetitionResult.FirstOrDefault() != null);
                allMultibattleRegistrations.AddRange(regsWithPoints);
            }
            var regsByUsers = allMultibattleRegistrations.GroupBy(r => r.UserId).ToList();
            var multiBattleDisciplinesIdListForMen = multiBattleCompetitionsDisciplines.Where(r => r.CompetitionAge.gender == 1).Select(x => x.Id);
            var multiBattleDisciplinesIdListForWomen = multiBattleCompetitionsDisciplines.Where(r => r.CompetitionAge.gender == 0).Select(x => x.Id);

            var playesAllowed = new List<int>();
            var competitionDisciplines = competition.CompetitionDisciplines;

            if (goldenSpikeType > 0) {
                foreach (var competitionDiscipline in competitionDisciplines)
                {
                    if (competitionDiscipline.DisciplineId.HasValue)
                    {
                        var discipline = db.Disciplines.Find(competitionDiscipline.DisciplineId.Value); ;
                        if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && discipline.DisciplineType == "GoldenSpikesU14")
                        {
                            playesAllowed.AddRange(competitionDiscipline.CompetitionDisciplineRegistrations.Select(r => r.UserId));
                        }
                        if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && discipline.DisciplineType == "GoldenSpikesU16")
                        {
                            playesAllowed.AddRange(competitionDiscipline.CompetitionDisciplineRegistrations.Select(r => r.UserId));
                        }
                    }
                }
            }

            var combinedPlayerRanks = new List<CombinedPlayerRankDto>();
            foreach (var regsByUser in regsByUsers)
            {
                var combinedPlayerRankDto = new CombinedPlayerRankDto();
                combinedPlayerRankDto.User = regsByUser.First().User;
                combinedPlayerRankDto.ClubName = regsByUser.First().Club?.Name ?? "";
                combinedPlayerRankDto.Points = new List<int>();
                combinedPlayerRankDto.Formats = new List<int?>();
                combinedPlayerRankDto.Results = new List<string>();
                combinedPlayerRankDto.Winds = new List<double?> ();
                combinedPlayerRankDto.GenderId = regsByUser.First().CompetitionDiscipline.CompetitionAge.gender ?? 0;
                var multiBattleDisciplinesIdList = combinedPlayerRankDto.GenderId == 1 ? multiBattleDisciplinesIdListForMen : multiBattleDisciplinesIdListForWomen;
                var playerRegistrations = new List<CompetitionDisciplineRegistration>();
                foreach (var competitionDisciplineId in multiBattleDisciplinesIdList)
                {
                    var points = 0;
                    double? wind = null;
                    string resultStr = "";
                    var reg = regsByUser.FirstOrDefault(r => r.CompetitionDisciplineId == competitionDisciplineId);
                    var format = 0;
                    if (reg != null)
                    {
                        playerRegistrations.Add(reg);
                        var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == reg.CompetitionDiscipline.DisciplineId);
                        var discipolineType = discipline.DisciplineType;
                        format = discipline.Format ?? 0;

                        var result = reg.CompetitionResult.FirstOrDefault();
                        if (result != null)
                        {
                            result.DisciplineType = discipolineType;
                            points = result.CombinedPoint ?? 0;
                            if (points < 0)
                                points = 0;
                            if (result.AlternativeResult > 0)
                            {
                                switch (result.AlternativeResult)
                                {
                                    case 1:
                                        resultStr = "DNF";
                                        break;
                                    case 2:
                                        resultStr = "DQ";
                                        break;
                                    case 3:
                                        resultStr = "DNS";
                                        break;
                                    case 4:
                                        resultStr = "NM";
                                        break;
                                }
                                points = 0;
                            }
                            else
                            {
                                resultStr = result.Result;
                                wind = result.Wind;
                            }
                        }
                    }
                    combinedPlayerRankDto.Points.Add(points);
                    combinedPlayerRankDto.Results.Add(resultStr);
                    combinedPlayerRankDto.Formats.Add(format);
                    combinedPlayerRankDto.Winds.Add(wind);

                }
                if (goldenSpikeType > 0)
                {
                    var chosenRegs = new List<CompetitionDisciplineRegistration>();
                    var beginRegistrations = playerRegistrations.Select(x => x).ToList();
                    var runningDisciplines = new List<string> { "50m", "60m", "100m", "80m", "300m", "600m", "1000m", "2000m", "60mh", "50mh", "80mh", "100mh", "250mh" };
                    var jumpingDisciplines = new List<string> { "high_jump", "pole_vault", "long_jump", "triple_jump" };
                    var throwingDisciplines = new List<string> { "shot_put", "discus_throw", "hockey_ball", "hammer_throw", "javelin_throw" };
                    var athletesRegisteredWithResultForRunningList = playerRegistrations.Where(r => runningDisciplines.Contains(r.CompetitionResult.FirstOrDefault()?.DisciplineType)).OrderByDescending(r => r.CompetitionResult.FirstOrDefault()?.CombinedPoint).ToList();
                    var athletesRegisteredWithResultForJumpingList = playerRegistrations.Where(r => jumpingDisciplines.Contains(r.CompetitionResult.FirstOrDefault()?.DisciplineType)).OrderByDescending(r => r.CompetitionResult.FirstOrDefault()?.CombinedPoint).ToList();
                    var athletesRegisteredWithResultForThrowingList = playerRegistrations.Where(r => throwingDisciplines.Contains(r.CompetitionResult.FirstOrDefault()?.DisciplineType)).OrderByDescending(r => r.CompetitionResult.FirstOrDefault()?.CombinedPoint).ToList();
                    chosenRegs.Add(athletesRegisteredWithResultForRunningList.FirstOrDefault());
                    chosenRegs.Add(athletesRegisteredWithResultForJumpingList.FirstOrDefault());
                    chosenRegs.Add(athletesRegisteredWithResultForThrowingList.FirstOrDefault());
                    if (goldenSpikeType == 2)
                    {
                        beginRegistrations = beginRegistrations.Except(chosenRegs).OrderByDescending(r => r.CompetitionResult.FirstOrDefault()?.CombinedPoint).ToList();
                        chosenRegs.Add(beginRegistrations.FirstOrDefault());
                    }
                    combinedPlayerRankDto.SumPoints = chosenRegs.Where(x => x != null).Sum(x => x.CompetitionResult.FirstOrDefault()?.CombinedPoint ?? 0);
                    if (playesAllowed.Contains(combinedPlayerRankDto.User.UserId))
                    {
                        combinedPlayerRanks.Add(combinedPlayerRankDto);
                    }

                }
                else
                {
                    combinedPlayerRankDto.SumPoints = combinedPlayerRankDto.Points.Sum();
                    combinedPlayerRanks.Add(combinedPlayerRankDto);
                }
            }
            combinedPlayerRanks = combinedPlayerRanks.OrderByDescending(x => x.SumPoints).ThenByDescending(x => x.Points.OrderByDescending(p => p).First()).ThenByDescending(x => x.Points.Where(pp => pp == x.Points.OrderByDescending(p => p).First()).Count()).ToList();

            var disciplinesId = multiBattleCompetitionsDisciplines.Select(x => x.DisciplineId).ToList();
            var disciplinesNameList = new List<string>();
            foreach (var disciplineId in disciplinesId)
            {
                var name = db.Disciplines.FirstOrDefault(d => d.DisciplineId == disciplineId).Name;
                disciplinesNameList.Add(name);
            }
            return combinedPlayerRanks;
        }


        public ICollection<League> GetByClub(int clubId, int seasonId)
        {
            return db.Leagues
                .Include(t => t.Gender)
                .Include(t => t.Age)
                .Where(t => t.IsArchive == false && t.ClubId == clubId && t.SeasonId == seasonId)
                .ToList();
        }

        public League GetById(int id)
        {
            return db.Leagues.Include("LeaguesPrices").FirstOrDefault(p => p.LeagueId == id);
        }

        public League GetByIdExtended(int id)
        {
            return db.Leagues.Include("Union").Include("Club").Include("Union.Section").Include("TeamRegistrations").Include("TeamRegistrations.Team").Include("TeamRegistrations.Team.TeamsPlayers").Include("TeamRegistrations.Team.TeamsPlayers.User").Include("LeaguesPrices").Include("LeagueTeams").Include("LeagueOfficialsSettings").Include("HandlingFees").FirstOrDefault(p => p.LeagueId == id);
        }

        public bool ChangePositionSettingsStatus(int leagueId, bool status)
        {
            try
            {
                var league = db.Leagues.FirstOrDefault(c => c.LeagueId == leagueId);
                league.IsPositionSettingsEnabled = status;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public IQueryable<League> GetBySeason(int seasonId)
        {
            return db.Leagues.Where(t => t.IsArchive == false && t.SeasonId == seasonId);
        }


        public bool ChangeTeamSettingsStatus(int leagueId, bool status)
        {
            try
            {
                var league = db.Leagues.FirstOrDefault(c => c.LeagueId == leagueId);
                league.IsTeam = status;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public IEnumerable<PositionSettingsDto> GetAllPositionSettings(int leagueId, int seasonId)
        {
            var positionSettings = db.PositionSettings.Where(ps => ps.LeagueId == leagueId && ps.SeasonId == seasonId);
            if (positionSettings.Any())
            {
                foreach (var positionSetting in positionSettings)
                {
                    yield return new PositionSettingsDto
                    {
                        Id = positionSetting.Id,
                        Position = positionSetting.Position,
                        Points = positionSetting.Points ?? 0,
                    };
                }
            }
        }

        public void CreatePositionSetting(PositionSettingFormDto dto)
        {
            db.PositionSettings.Add(new PositionSetting
            {
                LeagueId = dto.LeagueId,
                SeasonId = dto.SeasonId,
                Position = dto.Position,
                Points = dto.Points
            });
            db.SaveChanges();
        }

        public void DeletePositionSetting(int id)
        {
            var positionSetting = db.PositionSettings.FirstOrDefault(ps => ps.Id == id);
            if (positionSetting != null)
            {
                db.PositionSettings.Remove(positionSetting);
                db.SaveChanges();
            }
        }

        public void UpdatePositionSettings(PositionSettingsDto dto)
        {
            var setting = db.PositionSettings.FirstOrDefault(ps => ps.Id == dto.Id);
            if (setting != null)
            {
                setting.Points = dto.Points;
                setting.Position = dto.Position;
            }
            db.SaveChanges();
        }

        public string GetGameAliasByLeague(League league)
        {
            return league?.Union?.Section?.Alias ?? league?.Club?.Union?.Section?.Alias ?? league?.Club?.Section?.Alias ?? string.Empty;
        }
        public string GetGameAliasByLeagueId(int leagueId)
        {
            var league = GetById(leagueId);
            return GetGameAliasByLeague(league);
        }
        public bool IsBasketBallOrWaterPoloLeague(int leagueId)
        {
            var alias = GetGameAliasByLeagueId(leagueId);
            return alias == GamesAlias.BasketBall || alias == GamesAlias.WaterPolo || alias == GamesAlias.Soccer || alias == GamesAlias.Rugby || alias == GamesAlias.Softball;
        }
        public League GetByIdForRanks(int id)
        {
            return
                db.Leagues.Include("Games")
                    .Include("Stages")
                    .Include("Stages.Groups")
                    .FirstOrDefault(x => x.LeagueId == id);
        }

        public LeaguesDoc GetTermsDoc(int leagueId)
        {
            return db.LeaguesDocs.FirstOrDefault(t => t.LeagueId == leagueId);
        }

        public LeaguesDoc GetDocById(int id)
        {
            return db.LeaguesDocs.Find(id);
        }
        public int GetDocId(int id)
        {
            return db.LeaguesDocs.Find(id).DocId;
        }

        public void CreateDoc(LeaguesDoc doc)
        {
            db.LeaguesDocs.Add(doc);
        }

        public void Create(League item)
        {
            item.CreateDate = DateTime.Now;

            db.Leagues.Add(item);
            db.SaveChanges();

            var game = new Game
            {
                LeagueId = item.LeagueId,
                GameDays = "5,6",
                StartDate = DateTime.Now,
                GamesInterval = "01:00",
                PointsWin = 2,
                PointsDraw = 0,
                PointsLoss = 1,
                PointsTechWin = 2,
                PointsTechLoss = 0,
                SortDescriptors = "0,1,2",
                ActiveWeeksNumber = 1,
                BreakWeeksNumber = 0
            };

            db.Games.Add(game);
            db.SaveChanges();
        }

        public void CreateTennis(League item)
        {
            item.CreateDate = DateTime.Now;

            db.Leagues.Add(item);
            db.SaveChanges();
        }

        public void SetRegistrationsOrders(int? currClubId = null)
        {
            var allRegistrations = db.CompetitionRegistrations.Where(c => !c.OrderNumber.HasValue && !c.IsRegisteredByExcel);
            if (allRegistrations.Any())
            {
                var clubsIds = allRegistrations.Select(c => c.ClubId).Distinct();
                var competitonRoutesIds = allRegistrations.Select(c => c.CompetitionRouteId).Distinct();
                if (currClubId.HasValue)
                {
                    foreach (var routeId in competitonRoutesIds)
                    {
                        GenerateOrder(currClubId.Value, routeId, allRegistrations);
                    }
                }
                else
                {
                    foreach (var clubId in clubsIds)
                    {
                        foreach (var routeId in competitonRoutesIds)
                        {
                            GenerateOrder(clubId, routeId, allRegistrations);
                        }
                    }
                }

                db.SaveChanges();
            }
        }

        private void GenerateOrder(int clubId, int? routeId, IQueryable<CompetitionRegistration> allRegistrations)
        {
            var registrations = allRegistrations.Where(c => c.CompetitionRouteId == routeId && c.ClubId == clubId).ToList();
            var instruments = registrations.Where(c => c.InstrumentId.HasValue).Select(c => c.InstrumentId.Value).Distinct().ToList();

            if (instruments.Any())
            {
                for (var i = 0; i < instruments.Count; i++)
                {
                    var regsWithInstruments = registrations.Where(c => c.InstrumentId == instruments[i]).ToList();
                    for (var j = 0; j < regsWithInstruments.Count; j++)
                    {
                        regsWithInstruments[j].OrderNumber = j + 1;
                    }
                }
            }
            else
            {
                for (var i = 0; i < registrations.Count(); i++)
                {
                    registrations[i].OrderNumber = i + 1;
                }
            }
        }

        public IEnumerable<ListItemDto> FindByName(string name, int num)
        {
            return GetQuery(false)
                .ToList()
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains(name))
                .Select(x => new ListItemDto
                {
                    Id = x.LeagueId,
                    Name = $"{x.Name} - {x.Union.Name} - {x.Season.Name}",
                    SeasonId = x.SeasonId
                })
                .OrderBy(t => t.Name)
                .Take(num)
                .ToList();
        }

        public IEnumerable<ListItemDto> FindByName(string name, int num, int? unionId, int? seasonId = null)
        {
            return GetQuery(false).ToList().Where(t => t.Name.Contains(name) && t.UnionId == unionId && t.SeasonId == seasonId)
                  .Select(t => new ListItemDto { Id = t.LeagueId, Name = t.Name, SeasonId = t.SeasonId })
                  .OrderBy(t => t.Name)
                  .Take(num)
                  .ToList();
        }

        public IEnumerable<Job> GetLeagueJobs(int leagueId)
        {
            return (from l in db.Leagues
                    from j in l.Union.Section.Jobs
                    where l.LeagueId == leagueId && j.IsArchive == false
                    select j).ToList();
        }

        public List<League> GetByManagerId(int managerId, int? seasonId)
        {
            if (seasonId != null)
            {
                return db.UsersJobs
                    .Where(j => j.UserId == managerId)
                    .Select(j => j.League)
                    .Where(l => l != null && l.SeasonId == seasonId && l.Season.IsActive)
                    .Distinct()
                    .OrderBy(u => u.Name)
                    .ToList();
            }
            else
            {
                return db.UsersJobs
                    .Where(j => j.UserId == managerId)
                    .Select(j => j.League)
                    .Where(l => l != null)
                    .Distinct()
                    .OrderBy(u => u.Name)
                    .ToList();
            }
        }

        public bool CheckAllTennisGamesIsEnded(int stageId, int categoryId, bool isKnockoutOrPlayoff)
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

        public List<League> GetAll(int? disciplineId = null)
        {
            if (disciplineId.HasValue)
                return db.Leagues.Where(l => l.DisciplineId == disciplineId).ToList();

            return db.Leagues.ToList();
        }

        public List<League> GetAllWithPricesAsNoTracking(Expression<Func<League, bool>> expression)
        {
            return db.Leagues
                .Where(expression)
                .Include(x => x.LeaguesPrices)
                .AsNoTracking()
                .ToList();
        }

        public List<League> GetLeaguesForMoveByUnionSeasonId(int unionId, int seasonId, int leagueId)
        {
            return db.Leagues.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.LeagueId != leagueId).AsEnumerable()
                 .Select(x => new League
                 {
                     LeagueId = x.LeagueId,
                     Name = x.Name,
                     SortOrder = x.SortOrder
                 }).ToList();
        }

        public List<League> GetLeaguesBySesonUnion(int unionId, int seasonId)
        {
            return db.Leagues.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false).AsEnumerable()
                .Select(x => new League
                {
                    LeagueId = x.LeagueId,
                    Name = x.Name,
                    SeasonId = x.SeasonId
                }).ToList();
        }

        public League GetByLeagueSeasonId(int leagueId, int seasonId)
        {
            return db.Leagues.FirstOrDefault(x => x.LeagueId == leagueId && x.SeasonId == seasonId);
        }

        /// <summary>
        /// Get all teams in league except current team
        /// </summary>
        /// <param name="managerId"></param>
        /// <param name="currentTeamId"></param>
        /// <param name="seasonId"></param>
        /// <returns></returns>
        public List<TeamDto> GetTeamsByManager(int managerId, int currentTeamId, int seasonId, int? unionId)
        {
            var teams = db.UsersJobs
                .Where(j => j.UserId == managerId)
                .Where(j => j.League.SeasonId == seasonId)
                .SelectMany(j => j.League.LeagueTeams)
                .Where(l => l != null && l.Leagues.IsArchive == false && l.TeamId != currentTeamId).AsQueryable();
            if (unionId.HasValue)
            {
                teams = teams.Where(x => x.Leagues.UnionId == unionId);
            }
            var teamDto =
                teams.OrderBy(l => l.TeamId)
                    .Distinct()
                    .ToList()
                    .Select(x => new TeamDto
                    {
                        TeamId = x.TeamId,
                        Title = x.Teams.Title,
                        LeagueId = x.Leagues.LeagueId,
                        LeagueName = x.Leagues.Name,
                        ClubId = x.Teams.ClubTeams.FirstOrDefault(ct => ct.SeasonId == seasonId)?.ClubId ?? 0,
                        SchoolName = db.SchoolTeams.FirstOrDefault(st => st.TeamId == x.TeamId)?.School?.Name
                    })
                    .ToList();

            return teamDto;
        }

        public IEnumerable<LeagueTeams> GetLeaguesTeamsByIds(IEnumerable<int> leaguesIds, int seasonId)
        {
            var leaguesTeams = new List<LeagueTeams>();

            foreach (var leagueId in leaguesIds)
            {
                leaguesTeams.AddRange(db.LeagueTeams
                    .Where(lt => lt.LeagueId == leagueId && lt.SeasonId == seasonId));
            }
            return leaguesTeams;
        }

        public IEnumerable<LeagueTeams> GetLeagueTeam(int leagueId, int seasonId)
        {
            return db.LeagueTeams.Where(t => t.SeasonId == seasonId && t.LeagueId == leagueId);
        }

        public IEnumerable<League> GetAllUnionLeagues(int unionId, int seasonId, int clubId)
        {
            var leaguesList = new List<League>();
            var leagues = db.Leagues.Where(l => l.UnionId.HasValue && l.UnionId == unionId && l.SeasonId == seasonId);
            if (leagues != null && leagues.Any())
            {
                foreach (var league in leagues)
                {
                    var leagueClubsAllowed = league?.AllowedCLubsIds?.Split(',').Select(int.Parse).AsEnumerable();
                    if (leagueClubsAllowed == null || !leagueClubsAllowed.Any()) continue;

                    if (leagueClubsAllowed.Contains(clubId)) leaguesList.Add(league);
                }
            }
            return leaguesList;
        }

        public List<TennisPlayerRegistrationDto> GetAllTennisRegistrations(int id, int seasonId, bool isMustBeApproved = true)
        {
            var registeredPlayers = new List<TennisPlayerRegistrationDto>();
            var leagueRegistrations = db.TeamRegistrations.Where(tr => tr.LeagueId == id && tr.SeasonId == seasonId && !tr.IsDeleted && !tr.Team.IsArchive);
            if (leagueRegistrations != null && leagueRegistrations.Any())
            {
                registeredPlayers = GetAllRegisteredPlayersByRegistrations(leagueRegistrations, seasonId, id, isMustBeApproved);
            }

            return registeredPlayers;
        }

        public List<TennisPlayerRegistrationDto> GetAllTennisRegistrationsForClub(int leagueId, int clubId, int seasonId, bool isMustBeApproved = true)
        {
            var registeredPlayers = new List<TennisPlayerRegistrationDto>();
            var leagueRegistrations = db.TeamRegistrations.Where(tr => tr.LeagueId == leagueId && tr.ClubId == clubId && tr.SeasonId == seasonId && !tr.IsDeleted);
            if (leagueRegistrations != null && leagueRegistrations.Any())
            {
                registeredPlayers = GetAllRegisteredPlayersByRegistrations(leagueRegistrations, seasonId, leagueId, isMustBeApproved);
            }

            return registeredPlayers;
        }

        private List<TennisPlayerRegistrationDto> GetAllRegisteredPlayersByRegistrations(IEnumerable<TeamRegistration> leagueRegistrations,
            int seasonId, int leagueId, bool isMustBeApproved = true)
        {
            var result = new List<TennisPlayerRegistrationDto>();
            foreach (var reg in leagueRegistrations)
            {
                var league = db.Leagues.FirstOrDefault(c => c.LeagueId == leagueId);
                var teamPlayers = reg.Team
                    .TeamsPlayers
                    .Where(x => x.SeasonId == seasonId &&
                                x.User.IsArchive == false &&
                                !x.WithoutLeagueRegistration &&
                                x.IsActive &&
                                (!isMustBeApproved ||
                                (x.IsApprovedByManager == true &&
                                x.ApprovalDate <= league?.EndRegistrationDate)))
                    .ToList();

                if (teamPlayers.Any())
                {
                    foreach (var teamPlayer in teamPlayers)
                    {
                        result.Add(new TennisPlayerRegistrationDto
                        {
                            TeamPlayerId = teamPlayer.Id,
                            ClubName = reg.Club.Name,
                            TeamName = reg.TeamName ?? reg.Team.TeamsDetails.Where(c => c.SeasonId == seasonId)
                                           .OrderByDescending(c => c.Id).FirstOrDefault()?.TeamName
                                       ?? reg.Team.Title,
                            BirthDay = teamPlayer.User.BirthDay,
                            FullName = teamPlayer.User.FullName,
                            IdentNum = teamPlayer.User.IdentNum,
                            Phone = teamPlayer.User.Telephone,
                            TennicardValidity = teamPlayer.User.TenicardValidity,
                            TeamId = reg.TeamId,
                            InsuranceValidity = teamPlayer.User.DateOfInsurance,
                            MedicalValidity = teamPlayer.User.MedExamDate,
                            TeamPositionOrder = teamPlayer.TennisPositionOrder
                        });
                    }
                }
            }
            return result;
        }

        public bool RegisterReferees(int clubId, int seasonId, int leagueId, IEnumerable<int> refereeIds)
        {
            try
            {
                var hasAnyRegistrations = refereeIds != null && refereeIds.Where(c => c != 0).Any();
                var refereeRegs = GetAllRefereeRegistrations(clubId, seasonId, leagueId);
                if (hasAnyRegistrations && !refereeRegs.Any())
                {
                    foreach (var refereeId in refereeIds.Where(c => c != 0))
                    {
                        db.RefereeRegistrations.Add(new RefereeRegistration
                        {
                            ClubId = clubId,
                            SeasonId = seasonId,
                            LeagueId = leagueId,
                            RefereeId = refereeId
                        });
                    }
                }
                else if (hasAnyRegistrations && refereeRegs.Any())
                {
                    foreach (var reg in refereeRegs)
                    {
                        reg.IsArchive = true;
                    }

                    foreach (var refereeId in refereeIds.Where(c => c != 0))
                    {
                        db.RefereeRegistrations.Add(new RefereeRegistration
                        {
                            ClubId = clubId,
                            SeasonId = seasonId,
                            LeagueId = leagueId,
                            RefereeId = refereeId
                        });
                    }
                }
                else
                {
                    if (refereeRegs.Any())
                    {
                        foreach (var reg in refereeRegs)
                        {
                            reg.IsArchive = true;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public IEnumerable<RefereeRegistration> GetAllRefereeRegistrations(int clubId, int seasonId, int leagueId)
        {
            return db.RefereeRegistrations.Where(r => r.ClubId == clubId && r.SeasonId == seasonId && r.LeagueId == leagueId && !r.IsArchive);
        }

        public Dictionary<int, List<int>> GetAllRegisteredRefereesIds(int clubId, int seasonId, int unionId)
        {
            var unionLeaguesWithRegs =
                db.Leagues
                .Include(x => x.RefereeRegistrations)
                .Include(x => x.RefereeRegistrations.Select(r => r.UsersJob))
                .Where(c =>
                    c.RefereeRegistrations.Any(r =>
                        !r.IsArchive &&
                        r.ClubId == clubId &&
                        r.SeasonId == seasonId))
                .AsNoTracking()
                .ToList();

            var result = new Dictionary<int, List<int>>();
            if (unionLeaguesWithRegs.Any())
            {
                foreach (var league in unionLeaguesWithRegs)
                {
                    result.Add(
                        league.LeagueId,
                        league.RefereeRegistrations
                            .Where(r =>
                                !r.IsArchive &&
                                r.ClubId == clubId &&
                                r.SeasonId == seasonId)
                            .Select(c => c.UsersJob.Id)
                            .ToList());
                }
            }
            return result;
        }

        public IEnumerable<DaysForHostingDto> GetAllDaysForHosting(int leagueId, int seasonId)
        {
            var daysForHosting = db.DaysForHostings.Where(dh => dh.LeagueId == leagueId && dh.SeasonId == seasonId);
            if (daysForHosting.Any())
            {
                foreach (var day in daysForHosting)
                {
                    yield return new DaysForHostingDto
                    {
                        Id = day.Id,
                        Day = (DayOfWeek)day.Day,
                        StartTime = day.StartTime,
                        EndTime = day.EndTime
                    };
                }
            }
        }

        public Dictionary<int, bool> GetHostingDaysDictionary(IEnumerable<DaysForHostingDto> daysForHostingDb, int teamId)
        {
            var result = new Dictionary<int, bool>();
            var hostingDaysIds = daysForHostingDb.Where(d => d.Id.HasValue)?.Select(d => d.Id.Value)?.Distinct();
            var teamHostingDays = db.TeamHostingDays.Where(t => t.TeamId == teamId)?.AsNoTracking()?.ToList();
            if (hostingDaysIds.Any())
            {
                foreach (var hostingDayId in hostingDaysIds)
                {
                    result.Add(hostingDayId, teamHostingDays.Any(r => r.HostingDayId == hostingDayId));
                }
            }
            return result;
        }

        public void ProcessTeamHostingDay(int hostingDayId, bool isChecked, int teamId)
        {
            var hostingDay = db.TeamHostingDays.FirstOrDefault(t => t.TeamId == teamId && t.HostingDayId == hostingDayId);
            if (hostingDay != null && !isChecked)
            {
                db.TeamHostingDays.Remove(hostingDay);
            }
            else if (hostingDay == null && isChecked)
            {
                db.TeamHostingDays.Add(new TeamHostingDay
                {
                    TeamId = teamId,
                    HostingDayId = hostingDayId
                });
            }
        }

        public void DeleteDayForHosting(int id)
        {
            var day = db.DaysForHostings.Find(id);
            if (day != null)
            {
                if (day.TeamHostingDays.Count > 0)
                    db.TeamHostingDays.RemoveRange(day.TeamHostingDays);

                db.DaysForHostings.Remove(day);
            }
        }

        public void CreateDayForHosting(DaysForHostingDto model, int leagueId, int seasonId)
        {
            AutoMapper.Mapper.Initialize(cfg => cfg.CreateMap<DaysForHostingDto, DaysForHosting>());
            var modelDb = AutoMapper.Mapper.Map<DaysForHostingDto, DaysForHosting>(model);

            modelDb.LeagueId = leagueId;
            modelDb.SeasonId = seasonId;

            db.DaysForHostings.Add(modelDb);

        }

        public List<WorkerMainShortDto> GetAllClubRelatedReferees(
            int clubId,
            bool includeBlockedFromCompetitions = true)
        {
            return db.UsersJobs
                .Include(x => x.User)
                .AsNoTracking()
                .Where(c => c.ConnectedClubId == clubId &&
                            (includeBlockedFromCompetitions || !c.IsCompetitionRegistrationBlocked))
                .ToList()
                .Select(c => new WorkerMainShortDto
                {
                    UserJobId = c.Id,
                    UserFullName = c.User.FullName,
                    UserId = c.UserId
                })
                .ToList();
        }

        public void UpdateRefereeRegistration(int userId, int sessionId, int leagueId, int seasonId, int unionId, int isAdd)
        {
            var reg = db.RefereeCompetitionRegistrations.Where(r => r.UserId == userId && r.LeagueId == leagueId).FirstOrDefault();
            if (isAdd == 1)
            {
                if (reg == null)
                {
                    db.RefereeCompetitionRegistrations.Add(new RefereeCompetitionRegistration
                    {
                        UserId = userId,
                        SeasonId = seasonId,
                        UnionId = unionId,
                        LeagueId = leagueId,
                        SessionIds = sessionId.ToString()
                    });
                }
                else
                {
                    if (String.IsNullOrEmpty(reg.SessionIds))
                    {
                        reg.SessionIds = sessionId.ToString();
                    }
                    else
                    {
                        if (sessionId > 0 && !reg.SessionIds.Split(',').Contains(sessionId.ToString()))
                            reg.SessionIds += "," + sessionId.ToString();
                    }
                    reg.LeagueId = leagueId;
                }
            }
            else
            {
                if (reg == null)
                    return;
                var new_arr = reg.SessionIds.Split(',').Where(s => s != sessionId.ToString()).ToArray();
                reg.SessionIds = String.Join(",", new_arr);
                if (reg.SessionIds.Length == 0)
                {
                    db.RefereeCompetitionRegistrations.Remove(reg);
                }
            }

            db.SaveChanges();
        }

        private void removeClubFromAllSpeficifLeagues(AthleticLeague league, int clubId)
        {
            var alLeague = league.AlLeague ?? "";
            var premiereLeague = league.PremiereLeague ?? "";
            var nationalLeague = league.NationalLeague ?? "";
            var alefLeague = league.AlefLeague ?? "";

            var alLeagueList = alLeague.Split(',').ToList();
            var premiereLeagueList = premiereLeague.Split(',').ToList();
            var nationalLeagueList = nationalLeague.Split(',').ToList();
            var alefLeagueList = alefLeague.Split(',').ToList();

            if (alLeagueList.Contains(clubId.ToString()))
            {
                alLeagueList.Remove(clubId.ToString());
            }
            if (premiereLeagueList.Contains(clubId.ToString()))
            {
                premiereLeagueList.Remove(clubId.ToString());
            }
            if (nationalLeagueList.Contains(clubId.ToString()))
            {
                nationalLeagueList.Remove(clubId.ToString());
            }
            if (alefLeagueList.Contains(clubId.ToString()))
            {
                alefLeagueList.Remove(clubId.ToString());
            }

            var corrections = db.CompetitionClubsCorrections.Where(cc => cc.ClubId == clubId && cc.LeagueId > 0 && cc.League.AthleticLeagueId == league.Id && cc.SeasonId == league.SeasonId && cc.GenderId == 1 && cc.TypeId.HasValue);
            db.CompetitionClubsCorrections.RemoveRange(corrections);

            league.AlLeague = String.Join(",", alLeagueList);
            league.PremiereLeague = String.Join(",", premiereLeagueList);
            league.NationalLeague = String.Join(",", nationalLeagueList);
            league.AlefLeague = String.Join(",", alefLeagueList);
        }

        private void removeClubFromAllSpeficifLeaguesF(AthleticLeague league, int clubId)
        {
            var alLeague = league.AlLeagueF ?? "";
            var premiereLeague = league.PremiereLeagueF ?? "";
            var nationalLeague = league.NationalLeagueF ?? "";
            var alefLeague = league.AlefLeagueF ?? "";

            var alLeagueList = alLeague.Split(',').ToList();
            var premiereLeagueList = premiereLeague.Split(',').ToList();
            var nationalLeagueList = nationalLeague.Split(',').ToList();
            var alefLeagueList = alefLeague.Split(',').ToList();

            if (alLeagueList.Contains(clubId.ToString()))
            {
                alLeagueList.Remove(clubId.ToString());
            }
            if (premiereLeagueList.Contains(clubId.ToString()))
            {
                premiereLeagueList.Remove(clubId.ToString());
            }
            if (nationalLeagueList.Contains(clubId.ToString()))
            {
                nationalLeagueList.Remove(clubId.ToString());
            }
            if (alefLeagueList.Contains(clubId.ToString()))
            {
                alefLeagueList.Remove(clubId.ToString());
            }

            var corrections = db.CompetitionClubsCorrections.Where(cc => cc.ClubId == clubId && cc.LeagueId > 0 && cc.League.AthleticLeagueId == league.Id && cc.SeasonId == league.SeasonId && cc.GenderId == 0 && cc.TypeId.HasValue);
            db.CompetitionClubsCorrections.RemoveRange(corrections);

            league.AlLeagueF = String.Join(",", alLeagueList);
            league.PremiereLeagueF = String.Join(",", premiereLeagueList);
            league.NationalLeagueF = String.Join(",", nationalLeagueList);
            league.AlefLeagueF = String.Join(",", alefLeagueList);
        }

        public void RegisterClubToAthleticLeague(int leagueId, int typeId, int clubId, bool isChecked)
        {
            var league = db.AthleticLeagues.FirstOrDefault(l => l.Id == leagueId);
            if (league != null)
            {
                if (isChecked)
                {
                    removeClubFromAllSpeficifLeagues(league, clubId);
                    switch (typeId)
                    {
                        case 1:
                            {
                                var clubIds = league.AlLeague ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Add(clubId.ToString());
                                }
                                league.AlLeague = String.Join(",", clubIdsList);
                                break;
                            }
                        case 2:
                            {
                                var clubIds = league.PremiereLeague ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Add(clubId.ToString());
                                }
                                league.PremiereLeague = String.Join(",", clubIdsList);
                                break;
                            }
                        case 3:
                            {
                                var clubIds = league.NationalLeague ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Add(clubId.ToString());
                                }
                                league.NationalLeague = String.Join(",", clubIdsList);
                                break;
                            }
                        case 4:
                            {
                                var clubIds = league.AlefLeague ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Add(clubId.ToString());
                                }
                                league.AlefLeague = String.Join(",", clubIdsList);
                                break;
                            }
                    }
                }
                else
                {
                    switch (typeId)
                    {
                        case 1:
                            {
                                var clubIds = league.AlLeague ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Remove(clubId.ToString());
                                    var corrections = db.CompetitionClubsCorrections.Where(cc => cc.ClubId == clubId && cc.LeagueId > 0 && cc.League.AthleticLeagueId == league.Id && cc.SeasonId == league.SeasonId && cc.GenderId == 1 && cc.TypeId.HasValue && cc.TypeId.Value == typeId);
                                    db.CompetitionClubsCorrections.RemoveRange(corrections);
                                }
                                league.AlLeague = String.Join(",", clubIdsList);
                                break;
                            }
                        case 2:
                            {
                                var clubIds = league.PremiereLeague ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Remove(clubId.ToString());
                                    var corrections = db.CompetitionClubsCorrections.Where(cc => cc.ClubId == clubId && cc.LeagueId > 0 && cc.League.AthleticLeagueId == league.Id && cc.SeasonId == league.SeasonId && cc.GenderId == 1 && cc.TypeId.HasValue && cc.TypeId.Value == typeId);
                                    db.CompetitionClubsCorrections.RemoveRange(corrections);
                                }
                                league.PremiereLeague = String.Join(",", clubIdsList);
                                break;
                            }
                        case 3:
                            {
                                var clubIds = league.NationalLeague ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Remove(clubId.ToString());
                                    var corrections = db.CompetitionClubsCorrections.Where(cc => cc.ClubId == clubId && cc.LeagueId > 0 && cc.League.AthleticLeagueId == league.Id && cc.SeasonId == league.SeasonId && cc.GenderId == 1 && cc.TypeId.HasValue && cc.TypeId.Value == typeId);
                                    db.CompetitionClubsCorrections.RemoveRange(corrections);
                                }
                                league.NationalLeague = String.Join(",", clubIdsList);
                                break;
                            }
                        case 4:
                            {
                                var clubIds = league.AlefLeague ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Remove(clubId.ToString());
                                    var corrections = db.CompetitionClubsCorrections.Where(cc => cc.ClubId == clubId && cc.LeagueId > 0 && cc.League.AthleticLeagueId == league.Id && cc.SeasonId == league.SeasonId && cc.GenderId == 1 && cc.TypeId.HasValue && cc.TypeId.Value == typeId);
                                    db.CompetitionClubsCorrections.RemoveRange(corrections);
                                }
                                league.AlefLeague = String.Join(",", clubIdsList);
                                break;
                            }
                    }
                }
                Save();
            }
        }

        public void RegisterClubToAthleticLeagueF(int leagueId, int typeId, int clubId, bool isChecked)
        {
            var league = db.AthleticLeagues.FirstOrDefault(l => l.Id == leagueId);
            if (league != null)
            {
                if (isChecked)
                {
                    removeClubFromAllSpeficifLeaguesF(league, clubId);
                    switch (typeId)
                    {
                        case 1:
                            {
                                var clubIds = league.AlLeagueF ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Add(clubId.ToString());
                                }
                                league.AlLeagueF = String.Join(",", clubIdsList);
                                break;
                            }
                        case 2:
                            {
                                var clubIds = league.PremiereLeagueF ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Add(clubId.ToString());
                                }
                                league.PremiereLeagueF = String.Join(",", clubIdsList);
                                break;
                            }
                        case 3:
                            {
                                var clubIds = league.NationalLeagueF ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Add(clubId.ToString());
                                }
                                league.NationalLeagueF = String.Join(",", clubIdsList);
                                break;
                            }
                        case 4:
                            {
                                var clubIds = league.AlefLeagueF ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Add(clubId.ToString());
                                }
                                league.AlefLeagueF = String.Join(",", clubIdsList);
                                break;
                            }
                    }
                }
                else
                {
                    switch (typeId)
                    {
                        case 1:
                            {
                                var clubIds = league.AlLeagueF ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Remove(clubId.ToString());
                                    var corrections = db.CompetitionClubsCorrections.Where(cc => cc.ClubId == clubId && cc.LeagueId > 0 && cc.League.AthleticLeagueId == league.Id && cc.SeasonId == league.SeasonId && cc.GenderId == 0 && cc.TypeId.HasValue && cc.TypeId.Value == typeId);
                                    db.CompetitionClubsCorrections.RemoveRange(corrections);
                                }
                                league.AlLeagueF = String.Join(",", clubIdsList);
                                break;
                            }
                        case 2:
                            {
                                var clubIds = league.PremiereLeagueF ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Remove(clubId.ToString());
                                    var corrections = db.CompetitionClubsCorrections.Where(cc => cc.ClubId == clubId && cc.LeagueId > 0 && cc.League.AthleticLeagueId == league.Id && cc.SeasonId == league.SeasonId && cc.GenderId == 0 && cc.TypeId.HasValue && cc.TypeId.Value == typeId);
                                    db.CompetitionClubsCorrections.RemoveRange(corrections);
                                }
                                league.PremiereLeagueF = String.Join(",", clubIdsList);
                                break;
                            }
                        case 3:
                            {
                                var clubIds = league.NationalLeagueF ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Remove(clubId.ToString());
                                    var corrections = db.CompetitionClubsCorrections.Where(cc => cc.ClubId == clubId && cc.LeagueId > 0 && cc.League.AthleticLeagueId == league.Id && cc.SeasonId == league.SeasonId && cc.GenderId == 0 && cc.TypeId.HasValue && cc.TypeId.Value == typeId);
                                    db.CompetitionClubsCorrections.RemoveRange(corrections);
                                }
                                league.NationalLeagueF = String.Join(",", clubIdsList);
                                break;
                            }
                        case 4:
                            {
                                var clubIds = league.AlefLeagueF ?? "";
                                var clubIdsList = clubIds.Split(',').ToList();
                                if (!clubIdsList.Contains(clubId.ToString()))
                                {
                                    clubIdsList.Remove(clubId.ToString());
                                    var corrections = db.CompetitionClubsCorrections.Where(cc => cc.ClubId == clubId && cc.LeagueId > 0 && cc.League.AthleticLeagueId == league.Id && cc.SeasonId == league.SeasonId && cc.GenderId == 0 && cc.TypeId.HasValue && cc.TypeId.Value == typeId);
                                    db.CompetitionClubsCorrections.RemoveRange(corrections);
                                }
                                league.AlefLeagueF = String.Join(",", clubIdsList);
                                break;
                            }
                    }
                }
                Save();
            }
        }

        public CompetitionDisciplineRegistration GetCompetitionDisciplineRegistration(int regId)
        {
            return db.CompetitionDisciplineRegistrations.FirstOrDefault(r => r.Id == regId);
        }

        public Dictionary<ClubShort, IEnumerable<WorkerMainShortDto>> GetLeagueRelatedRefereesByClubs(int leagueId, int seasonId)
        {
            var result = new Dictionary<ClubShort, IEnumerable<WorkerMainShortDto>>();
            var refereeRegistrations = db.RefereeRegistrations.Where(x => x.LeagueId == leagueId && x.SeasonId == seasonId && !x.IsArchive);

            if (refereeRegistrations.Any())
            {
                var clubs = refereeRegistrations.Select(x => x.Club).Distinct();
                if (clubs != null && clubs.Any())
                {
                    foreach (var club in clubs)
                    {
                        var registrations = GetClubsRefereeDtos(club, leagueId, seasonId);
                        if (registrations.Any())
                            result.Add(new ClubShort { Id = club.ClubId, Name = club.Name }, registrations);
                    }
                }
            }
            return result;
        }

        public List<CompetitionAge> GetCompetitionCategories(int competitionId)
        {
            var competition = db.Leagues.Find(competitionId);
            return db.CompetitionAges
                .AsNoTracking()
                .Where(x => x.UnionId == competition.UnionId &&
                            x.SeasonId == competition.SeasonId)
                .ToList();
        }

        public IEnumerable<RowingDistance> GetCompetitionRowingDistances(int competitionId)
        {
            var competition = db.Leagues.Find(competitionId);
            return db.RowingDistances.AsNoTracking().Where(c => c.SeasonId == competition.SeasonId).AsEnumerable();
        }
        


        public IEnumerable<RefereeShortDto> GetRefereesForCompetitionRegistration(int leagueId, int seasonId)
        {
            var referees = db.UsersJobs.Where(r => r.SeasonId == seasonId)
                .Join(db.Jobs.Where(j => j.RoleId == 4), r => r.JobId, j => j.JobId, (r, j) => r)
                .OrderBy(r => r.UserId);
            if (referees.Any())
            {
                foreach (var reg in referees)
                {
                    yield return new RefereeShortDto
                    {
                        UserId = reg.UserId,
                        LeagueId = reg.LeagueId ?? 0,
                        UnionId = reg.UnionId ?? 0,
                        SeasonId = reg.SeasonId ?? 0,
                        UserFullName = reg.User.FullName
                    };
                }
            }
        }

        public IEnumerable<WorkerMainShortDto> GetClubsRefereeDtos(Club club, int leagueId, int seasonId)
        {
            var clubsRefereeRegs = club.RefereeRegistrations.Where(x => x.LeagueId == leagueId && x.SeasonId == seasonId && !x.IsArchive);
            if (clubsRefereeRegs.Any())
            {
                foreach (var reg in clubsRefereeRegs)
                {
                    yield return new WorkerMainShortDto
                    {
                        RegistrationId = reg.Id,
                        UserJobId = reg.UsersJob.Id,
                        UserFullName = reg.UsersJob.User.FullName,
                        IsApproved = reg.IsApproved
                    };
                }
            }
        }


        public int CountRegisteredReferees(int leagueId, int seasonId)
        {
            return db.RefereeRegistrations.Where(x => x.LeagueId == leagueId && x.SeasonId == seasonId && !x.IsArchive)
                .Select(x => x.RefereeId).Distinct().Count();
        }

        public IEnumerable<CompetitionRoute> GetCompetitionRoute(string routeName, int leagueId, int seasonId)
        {
            var routes = db.CompetitionRoutes.Where(cr =>
                        cr.LeagueId == leagueId && cr.SeasonId == seasonId
                        && string.Equals(cr.DisciplineRoute.Route.Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty),
                            routeName.Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty))).AsEnumerable();
            return routes;
        }


        public CompetitionRoute GetCompetitionRouteWithRank(IEnumerable<CompetitionRoute> competitionRoutes, string rank, string instrument, bool isFirstComposition, bool isSecondComposition)
        {
            var routesWithRanks = competitionRoutes.Where(cr => string.Equals(cr.RouteRank.Rank, rank, StringComparison.OrdinalIgnoreCase));
            CompetitionRoute resultRoute = null;
            if (routesWithRanks != null && routesWithRanks.Any())
            {
                var routesWithComposition = routesWithRanks.Where(c => c.Composition.HasValue || c.SecondComposition.HasValue);
                if (routesWithComposition != null && routesWithComposition.Any() && (isFirstComposition || isSecondComposition))
                {
                    if (isFirstComposition && string.IsNullOrEmpty(instrument))
                        resultRoute = routesWithComposition.FirstOrDefault(c => c.Composition.HasValue && string.IsNullOrEmpty(GetInstrumentsNames(c.InstrumentIds)));

                    else if (isSecondComposition && string.IsNullOrEmpty(instrument))
                        resultRoute = routesWithComposition.FirstOrDefault(c => c.SecondComposition.HasValue && string.IsNullOrEmpty(GetInstrumentsNames(c.InstrumentIds)));

                    else if (isFirstComposition && !string.IsNullOrEmpty(instrument))
                        resultRoute = routesWithComposition.FirstOrDefault(c => c.Composition.HasValue && !string.IsNullOrEmpty(GetInstrumentsNames(c.InstrumentIds))
                        && GetInstrumentsNames(c.InstrumentIds).Contains(instrument));

                    else if (isSecondComposition && !string.IsNullOrEmpty(instrument))
                        resultRoute = routesWithComposition.FirstOrDefault(c => c.SecondComposition.HasValue && !string.IsNullOrEmpty(GetInstrumentsNames(c.InstrumentIds))
                         && GetInstrumentsNames(c.InstrumentIds).Contains(instrument));
                }
                else
                {
                    resultRoute = routesWithRanks.FirstOrDefault();
                }
            }

            return resultRoute;
        }

        public void SetIsCompetitionLeague(int leagueId, bool isLeague)
        {
            var league = db.Leagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league != null)
            {
                league.IsCompetitionLeague = isLeague;
                Save();
            }
        }

        public void SetIsTeam(int leagueId, bool isTeam)
        {
            var league = db.Leagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league != null)
            {
                league.IsTeam = isTeam;
                Save();
            }
        }

        


        public void SetIsFieldCompetition(int leagueId, bool isLeague)
        {
            var league = db.Leagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league != null)
            {
                league.IsFieldCompetition = isLeague;
                Save();
            }
        }


        public string GetInstrumentsNames(string instrumentIds)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(instrumentIds))
            {
                var intsrumentsIds = instrumentIds.Split(',').Select(int.Parse);
                if (intsrumentsIds.Any())
                {
                    var instruments = db.Instruments.Where(i => intsrumentsIds.Contains(i.Id));
                    if (instruments.Any())
                    {
                        var intrumentNames = instruments.Select(i => i.Name);
                        result = string.Join(",", intrumentNames);
                    }
                }
            }
            return result;
        }

        public void CheckIfAlreadyRegistered(CompetitionRegistration registrationModel, ref int importedRowCount)
        {
            try
            {
                var competitionReg = db.CompetitionRegistrations.FirstOrDefault(c => c.IsActive && c.LeagueId == registrationModel.LeagueId && c.CompetitionRouteId == registrationModel.CompetitionRouteId
                    && c.UserId == registrationModel.UserId);
                if (competitionReg != null)
                {
                    competitionReg.FinalScore = registrationModel.FinalScore;
                    competitionReg.OrderNumber = registrationModel.OrderNumber;
                    competitionReg.Position = registrationModel.Position;
                }
                else
                {
                    db.CompetitionRegistrations.Add(registrationModel);
                }
                db.SaveChanges();
                importedRowCount++;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SportsRegistration GetSportsmanRegistration(int userId, int leagueId, int clubId, int seasonId)
        {
            return db.SportsRegistrations.FirstOrDefault(sr => sr.UserId == userId && sr.LeagueId == leagueId && sr.ClubId == clubId && sr.SeasonId == seasonId);
        }

        public IEnumerable<CompetitionAchievement> GetCompetionsDtos(User user, string sectionAlias)
        {
            var registrationList = user.CompetitionRegistrations.Where(sr => sr.FinalScore.HasValue || sr.Position.HasValue);
            var otherRegList = user.SportsRegistrations.Where(sr => sr.FinalScore.HasValue || sr.Position.HasValue);
            foreach (var registration in registrationList)
            {
                var instruments = !string.IsNullOrEmpty(registration?.CompetitionRoute?.InstrumentIds)
                    ? GetInstrumentsNames(registration.CompetitionRoute.InstrumentIds)
                    : string.Empty;
                if (registration?.League?.IsArchive == false && (registration?.League?.Club?.Section?.Alias == sectionAlias
                    || registration?.League?.Club?.Union?.Section?.Alias == sectionAlias
                    || registration?.League?.Union?.Section?.Alias == sectionAlias))
                {
                    yield return new CompetitionAchievement
                    {
                        LeagueName = registration.League.Name,
                        Discipline = registration.CompetitionRoute?.Discipline?.Name,
                        ClubName = registration.Club?.Name,
                        StartDate = registration.League?.LeagueStartDate?.ToString(),
                        EndDate = registration.League?.LeagueEndDate?.ToString(),
                        EndDateDateTime = registration.League?.LeagueEndDate,
                        Route = registration.CompetitionRoute?.DisciplineRoute?.Route,
                        Rank = registration.CompetitionRoute?.RouteRank?.Rank,
                        Composition = registration?.CompetitionRoute?.Composition?.ToString() ?? registration?.CompetitionRoute?.SecondComposition?.ToString() ?? string.Empty,
                        Reserved = db.AdditionalGymnastics.FirstOrDefault(ag => ag.CompetitionRouteId == registration.CompetitionRouteId && ag.UserId == registration.UserId) != null
                            ? "+" : string.Empty,
                        Instruments = instruments,
                        FinalScore = registration.FinalScore?.ToString(),
                        Position = registration.Position?.ToString(),
                        SeasonId = registration.SeasonId
                    };
                }
            }

            foreach (var registration in otherRegList)
            {
                if (registration?.League?.IsArchive == false && (registration?.League?.Club?.Section?.Alias == sectionAlias
                    || registration?.League?.Club?.Union?.Section?.Alias == sectionAlias
                    || registration?.League?.Union?.Section?.Alias == sectionAlias))
                {
                    yield return new CompetitionAchievement
                    {
                        LeagueName = registration.League.Name,
                        StartDate = registration.League?.LeagueStartDate?.ToString(),
                        EndDate = registration.League?.LeagueEndDate?.ToString(),
                        EndDateDateTime = registration.League?.LeagueEndDate,
                        FinalScore = registration.FinalScore?.ToString(),
                        Position = registration.Position?.ToString(),
                        SeasonId = registration.SeasonId
                    };
                }
            }
        }

        public void RegisterCompetitionToAthleticLeague(int leagueId, int competitionId, bool isChecked)
        {
            var competition = db.Leagues.FirstOrDefault(l => l.LeagueId == competitionId);
            if (competition != null)
            {
                if (isChecked)
                {
                    competition.AthleticLeagueId = leagueId;
                }
                else if (leagueId == competition.AthleticLeagueId)
                {
                    competition.AthleticLeagueId = null;
                }
                Save();
            }
        }

        public void CreateLevelSettings(LevelDateSettingDto settings, int competitionId)
        {
            db.LevelDateSettings.Add(new LevelDateSetting
            {
                CompetitionId = competitionId,
                CompetitionLevelId = settings.CompetitionLevelId,
                FinalStartDate = settings.FinalStartDate,
                FinalEndDate = settings.FinalEndDate,
                QualificationEndDate = settings.QualificationEndDate,
                QualificationStartDate = settings.QualificationStartDate
            });
            db.SaveChanges();
        }

        public void RemoveCompetitionRoutes(string ids)
        {
            int[] id_list = Array.ConvertAll(ids.Split(','), s => int.Parse(s));
            IEnumerable<CompetitionRoute> range = Enumerable.Empty<CompetitionRoute>();
            for (int i = 0;i < id_list.Length;i++)
            {
                int id = id_list[i];
                var entity = db.CompetitionRoutes.Find(id);
                range = range.Concat(new[] { entity });
            }

            db.CompetitionRoutes.RemoveRange(range);
            db.SaveChanges();
        }

        public void RemoveCompetitionTeamsRoutes(string ids)
        {
            int[] id_list = Array.ConvertAll(ids.Split(','), s => int.Parse(s));
            IEnumerable<CompetitionTeamRoute> range = Enumerable.Empty<CompetitionTeamRoute>();
            for (int i = 0; i < id_list.Length; i++)
            {
                int id = id_list[i];
                var entity = db.CompetitionTeamRoutes.Find(id);
                range = range.Concat(new[] { entity });
            }

            db.CompetitionTeamRoutes.RemoveRange(range);
            db.SaveChanges();
        }

        public void ChangeCompetitionRouteStatus(int competitionRouteId, bool isEnabled)
        {
            var comRoute = db.CompetitionRoutes.Find(competitionRouteId);
            comRoute.IsCompetitiveEnabled = isEnabled;
            db.SaveChanges();
        }

        public void ChangeCompetitionTeamRouteStatus(int competitionRouteId, bool isEnabled)
        {
            var comRoute = db.CompetitionTeamRoutes.Find(competitionRouteId);
            comRoute.IsCompetitiveEnabled = isEnabled;
            db.SaveChanges();
        }


        public void DeleteLevelSettings(int id)
        {
            var levelSetting = db.LevelDateSettings.Find(id);
            if (levelSetting != null)
            {
                db.LevelDateSettings.Remove(levelSetting);
                db.SaveChanges();
            }
        }

        public void UpdateLevelSettings(LevelDateSettingDto settings)
        {
            var level = db.LevelDateSettings.Find(settings.Id);
            if (level != null)
            {
                level.CompetitionLevelId = settings.CompetitionLevelId;
                level.QualificationStartDate = settings.QualificationStartDate;
                level.QualificationEndDate = settings.QualificationEndDate;
                level.FinalStartDate = settings.FinalStartDate;
                level.FinalEndDate = settings.FinalEndDate;
                db.SaveChanges();
            }
        }

        public void DeleteAllRegistrations(int? leagueId, int seasonId, int? competitionRouteId, int? deleteType = null)
        {
            IEnumerable<CompetitionRegistration> registrations = null;
            if (deleteType == null || deleteType == 3)
            {
                registrations = competitionRouteId.HasValue
                    ? db.CompetitionRegistrations.Where(t => t.LeagueId == leagueId && t.SeasonId == seasonId && t.CompetitionRouteId == competitionRouteId.Value)
                    : db.CompetitionRegistrations.Where(t => t.LeagueId == leagueId && t.SeasonId == seasonId);
            }
            else if (deleteType == 1)
            {
                registrations = competitionRouteId.HasValue
                    ? db.CompetitionRegistrations.Where(t => t.LeagueId == leagueId && t.SeasonId == seasonId && t.CompetitionRouteId == competitionRouteId.Value && t.CompetitionRoute != null)
                    : db.CompetitionRegistrations.Where(t => t.LeagueId == leagueId && t.SeasonId == seasonId && t.CompetitionRoute != null);
            }
            else if (deleteType == 2)
            {
                registrations = competitionRouteId.HasValue
                    ? db.CompetitionRegistrations.Where(t => t.LeagueId == leagueId && t.SeasonId == seasonId && t.CompetitionRouteId == competitionRouteId.Value && t.CompetitionTeamRoute != null)
                    : db.CompetitionRegistrations.Where(t => t.LeagueId == leagueId && t.SeasonId == seasonId && t.CompetitionTeamRoute != null);
            }

            try
            {
                db.CompetitionRegistrations.RemoveRange(registrations);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<InstrumentDto> GetAllInstrumentsDto()
        {
            return db.Instruments
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToList()
                .Select(x => new InstrumentDto
                {
                    InstrumentId = x.Id,
                    Name = x.Name
                })
                .ToList();
        }

        public List<CompetitionDto> GetAllMartialArtsCompetitions(int clubId, int unionId, int seasonId)
        {
            var unionLeagues = db.Leagues
                .Where(l =>
                    (l.UnionId == unionId || l.Club.UnionId == unionId) &&
                    l.SeasonId == seasonId &&
                    !string.IsNullOrEmpty(l.AllowedCLubsIds))
                .ToList();
            var listOfLeagues = new List<League>();
            foreach (var unionLeague in unionLeagues)
            {
                var clubsAllowed = unionLeague.AllowedCLubsIds
                    ?.Split(',')
                    .Select(int.Parse)
                    .ToList();
                if (clubsAllowed?.Any() == true && clubsAllowed.Contains(clubId))
                {
                    listOfLeagues.Add(unionLeague);
                }
            }

            return listOfLeagues
                .Select(x => new CompetitionDto
                {
                    LeagueId = x.LeagueId,
                    SeasonId = x.SeasonId ?? 0,
                    CompetitionName = x.Name,
                    StartDate = x.LeagueStartDate,
                    EndDate = x.LeagueEndDate,
                    StartRegistrationDate = x.StartRegistrationDate,
                    EndRegistrationDate = x.EndRegistrationDate,
                    IsEnded = x.EndRegistrationDate.HasValue &&
                              x.EndRegistrationDate.Value <= DateTime.Now,
                    TypeId = x.Type
                })
                .ToList();
        }

        public Dictionary<int, IEnumerable<int>> GetAllRegisteredSportsmenIds(int clubId, int seasonId, int unionId)
        {
            var unionLeaguesWithRegs = db.Leagues.Where(c => c.SportsRegistrations.Any(r => r.ClubId == clubId && r.SeasonId == seasonId));
            var result = new Dictionary<int, IEnumerable<int>>();
            if (unionLeaguesWithRegs != null && unionLeaguesWithRegs.Any())
            {
                foreach (var league in unionLeaguesWithRegs)
                {
                    result.Add(league.LeagueId, league.SportsRegistrations
                        .Where(r => r.ClubId == clubId && r.SeasonId == seasonId)
                        .Select(c => c.User.UserId));
                }
            }
            return result;
        }

        public void UpdateSportsmanRegistration(SportsRegistration registration, int? rank, double? finalScore)
        {
            registration.Position = rank;
            registration.FinalScore = finalScore;
            if (rank.HasValue || finalScore.HasValue)
            {
                registration.IsApproved = true;
            }
        }

        public void CreateSportsmanRegistration(int userId, int leagueId, int clubId, int seasonId, int? rank, double? finalScore)
        {
            db.SportsRegistrations.Add(new SportsRegistration
            {
                UserId = userId,
                ClubId = clubId,
                SeasonId = seasonId,
                LeagueId = leagueId,
                Position = rank,
                FinalScore = finalScore,
                IsApproved = rank.HasValue || finalScore.HasValue
            });
        }

        public Instrument GetInstrumentByName(string instrumentName, int leagueId)
        {
            return !string.IsNullOrEmpty(instrumentName)
                ? db.Instruments.FirstOrDefault(c => c.Name.Equals(instrumentName, StringComparison.OrdinalIgnoreCase) && c.Discipline.Leagues.Any(l => l.LeagueId == leagueId))
                : null;
        }


        public List<RegistrationInstrument> GetInstrumentsByIds(IEnumerable<int> ids)
        {
            var listOfInstruments = new List<RegistrationInstrument>();

            foreach (var id in ids)
            {
                listOfInstruments.Add(new RegistrationInstrument
                {
                    InstrumentId = id,
                    Name = db.Instruments.FirstOrDefault(c => c.Id == id)?.Name
                });
            }

            return listOfInstruments;
        }

        public void CreateAthleticLeague(string name, int seasonId)
        {
            db.AthleticLeagues.Add(new AthleticLeague
            {
                Name = name,
                SeasonId = seasonId
            });
            Save();
        }

        public string GetSectionAlias(int leagueId)
        {
            var league = db.Leagues.FirstOrDefault(c => c.LeagueId == leagueId);
            return league?.Union?.Section?.Alias ?? league?.Club?.Section?.Alias
                ?? league?.Club?.Union?.Section?.Alias ?? string.Empty;
        }

        public CompetitionRoute GetCompetitionRoute(int leagueId, int routeId, int rankId, int? composition, int? secondComposition, GymnasticInstrumentImport instruments = null)
        {
            var competitionRoutes = db.CompetitionRoutes.Where(x => x.DisciplineRoute.Id == routeId
                && x.RouteRank.Id == rankId
                && x.Composition == composition
                && x.SecondComposition == secondComposition);

            var instrumentId = instruments != null ? GetInstrumentByName(instruments?.InstrumentName, leagueId)?.Id.ToString() : string.Empty;

            return instruments != null
                ? competitionRoutes?.ToList()?.FirstOrDefault(x => !string.IsNullOrEmpty(x.InstrumentIds) && x.InstrumentIds.Split(',').Contains(instrumentId))
                : competitionRoutes?.FirstOrDefault();
        }

        public void CreateOrUpdateGymnasticRegistration(int userId, int clubId, int leagueId, int seasonId, int? competitionRouteId, int compositionNumber,
            double? finalScore, int? position, GymnasticInstrumentImport instrument = null)
        {
            var registrations = db.CompetitionRegistrations.Where(r => r.UserId == userId && r.ClubId == clubId && r.LeagueId == leagueId
                     && r.SeasonId == seasonId && r.CompetitionRouteId == competitionRouteId);
            var instrumentId = GetInstrumentByName(instrument?.InstrumentName, leagueId)?.Id;

            var registration = instrumentId.HasValue
                    ? registrations.FirstOrDefault(r => r.InstrumentId == instrumentId)
                    : registrations.FirstOrDefault();

            if (registration == null)
            {
                registration = db.CompetitionRegistrations.FirstOrDefault(r => r.UserId == userId && r.ClubId == clubId && r.LeagueId == leagueId
                     && r.SeasonId == seasonId);
            }

            if (registration != null)
            {
                registration.FinalScore = finalScore;
                registration.Position = position;
            }
            else
            {
                db.CompetitionRegistrations.Add(new CompetitionRegistration
                {
                    UserId = userId,
                    ClubId = clubId,
                    LeagueId = leagueId,
                    SeasonId = seasonId,
                    CompetitionRouteId = competitionRouteId.Value,
                    CompositionNumber = compositionNumber,
                    FinalScore = finalScore,
                    Position = position,
                    InstrumentId = instrumentId,
                    OrderNumber = instrument?.OrderNumber,
                    IsActive = true
                });
            }
        }

        public IEnumerable<League> GetTennisTeamLeagues(int teamId, int seasonId)
        {
            return db.TeamRegistrations.Where(c => c.TeamId == teamId && !c.IsDeleted && c.SeasonId == seasonId && !c.League.IsArchive)?.Select(c => c.League);
        }

        public List<TotoCompetition> GetFinishedCompeitions(int unionId, int seasonId, int excelPosition = 0)
        {
            var sectionAlias = db.Unions.Find(unionId)?.Section?.Alias;
            var competitionList =
                string.Equals(sectionAlias, GamesAlias.Gymnastic, StringComparison.CurrentCultureIgnoreCase)
                    ? db.Leagues
                        .Where(x => x.SeasonId == seasonId &&
                                    !x.IsArchive &&
                                    (x.UnionId == unionId || x.Club.UnionId == unionId) &&
                                    x.LeagueEndDate.HasValue &&
                                    x.LeagueEndDate.Value < DateTime.Now)
                        .OrderBy(x => x.LeagueStartDate)
                        .ToList()
                    : db.Leagues
                        .Where(x => x.SeasonId == seasonId && !x.IsArchive &&
                                    (x.UnionId == unionId || x.Club.UnionId == unionId))
                        .ToList();
            return GetTotoCompetitionsValues(competitionList, seasonId, excelPosition);
        }

        public List<TotoCompetition> GetFinishedTennisCompeitions(int unionId, int seasonId, int excelPosition = 0)
        {
            var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
            var prevSeason = db.Seasons.FirstOrDefault(s => s.Id == season.PreviousSeasonId);
            var hasPrevSeason = true;
            if(prevSeason == null)
            {
                hasPrevSeason = false;
            }
            var currentSeasonYear = season.EndDate.Year;
            var currentSeasonMaxDate = new DateTime(currentSeasonYear, 9, 1);
            var previousSeasonMinDate = new DateTime(currentSeasonYear-1, 9, 1);
            var sectionAlias = db.Unions.Find(unionId)?.Section?.Alias;
            var competitionList = db.Leagues.Where(x => !x.IsArchive && ((x.SeasonId == seasonId && ((x.LeagueStartDate.HasValue && x.LeagueStartDate < currentSeasonMaxDate && x.LeagueStartDate >= previousSeasonMinDate) || (!x.LeagueStartDate.HasValue && x.EndRegistrationDate < currentSeasonMaxDate && x.EndRegistrationDate >= previousSeasonMinDate))) || (hasPrevSeason && prevSeason.Id == x.SeasonId && ((x.LeagueStartDate.HasValue && x.LeagueStartDate < currentSeasonMaxDate && x.LeagueStartDate >= previousSeasonMinDate) || (!x.LeagueStartDate.HasValue && x.EndRegistrationDate >= previousSeasonMinDate && x.EndRegistrationDate < currentSeasonMaxDate)))) &&  (x.UnionId == unionId || x.Club.UnionId == unionId)).ToList();
            return GetTotoCompetitionsValues(competitionList, seasonId, excelPosition, true);
        }



        private List<TotoCompetition> GetTotoCompetitionsValues(List<League> competitionList, int seasonId , int excelPosition = 0, bool isEndRegistrationAllowed = false)
        {
            var listOfCompetitions = new List<TotoCompetition>();
            
            if (competitionList.Any())
            {
                foreach (var competition in competitionList)
                {
                    listOfCompetitions.Add(new TotoCompetition
                    {
                        Id = competition.LeagueId,
                        Name = competition.Name,
                        StartDate = isEndRegistrationAllowed ? (competition.LeagueStartDate.HasValue ? competition.LeagueStartDate : competition.EndRegistrationDate) : competition.LeagueStartDate,
                        IsCompetitionNotLeague = competition.EilatTournament ?? false,
                        IsDailyCompetition = competition.IsDailyCompetition,
                        CategoryIds = competition.LeagueTeams.Where(t => t.SeasonId == seasonId).Select(t => t.TeamId).ToList()
                    }); 
                }
            }
            listOfCompetitions = listOfCompetitions.OrderBy(x => x.StartDate ?? DateTime.MaxValue).ToList();
            var count = 0;
            foreach (var competition in listOfCompetitions)
            {
                competition.ExcelPosition = count + excelPosition;
                count++;
            }
            return listOfCompetitions;
        }

        public CompetitionRoute CreateCompetitionRoute(DisciplineRoute route, RouteRank rank, int seasonId, int leagueId)
        {
            db.CompetitionRoutes.Add(new CompetitionRoute
            {
                DisciplineId = route.DisciplineId,
                RouteId = route.Id,
                RankId = rank.Id,
                LeagueId = leagueId,
                SeasonId = seasonId
            });

            db.SaveChanges();

            return db.CompetitionRoutes.OrderByDescending(cr => cr.Id)?.FirstOrDefault();
        }

        public void ResetAllRegistrationsPositionsAndScoresToGymnasticCompetition(int competitionId, int seasonId)
        {
            var competitionDate = db.Leagues.FirstOrDefault(l => l.LeagueId == competitionId).LeagueStartDate ?? DateTime.MaxValue;
            var regs = db.CompetitionRegistrations.Where(r => r.LeagueId == competitionId && r.SeasonId == seasonId).ToList();
            var userIds = regs.Select(r => r.UserId).ToList();
            var teamPlayers = db.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && userIds.Contains(tp.UserId)).ToList();
            foreach (var reg in regs)
            {
                var tp = teamPlayers.FirstOrDefault(p => p.UserId == reg.UserId);
                var ignoreReset = false;
                if (tp.ApprovalDate != null && tp.ApprovalDate > competitionDate)
                {
                    ignoreReset = true;
                }
                if (!ignoreReset)
                {
                    reg.Position = null;
                    reg.FinalScore = null;
                }
            }
            Save();
        }



        public void UpdateScheduleState(string elementDivId, int leagueId, bool isHidden, int adminId)
        {
            var element = db.LeagueScheduleStates.FirstOrDefault(l => l.LeagueId == leagueId && l.ElementDivId == elementDivId && l.UserId == adminId);

            if (element == null)
            {
                db.LeagueScheduleStates.Add(new LeagueScheduleState
                {
                    LeagueId = leagueId,
                    UserId = adminId,
                    ElementDivId = elementDivId,
                    IsHidden = isHidden
                });
            }
            else
            {
                element.IsHidden = isHidden;
            }

            db.SaveChanges();
        }
    }
}