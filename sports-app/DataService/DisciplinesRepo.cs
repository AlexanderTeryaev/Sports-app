using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using AppModel;
using AutoMapper;
using DataService.DTO;
using DataService.Services;
using DataService.Utils;

namespace DataService
{
    public class DisciplinesRepo : BaseRepo
    {
        public DisciplinesRepo() : base()
        {

        }

        public DisciplinesRepo(DataEntities db) : base(db) { }

        public List<Discipline> GetBySection(int id, int unionId)
        {
            return db.Disciplines.Where(d => d.Union.SectionId == id
                                            && d.UnionId == unionId
                                            && !d.IsArchive)
                                        .ToList();
        }

        public void Create(Discipline discipline)
        {
            db.Disciplines.Add(discipline);
            db.SaveChanges();
        }

        public Discipline GetById(int id)
        {
            return db.Disciplines.Find(id);
        }

        public IEnumerable<DisciplineDTO> GetAllByUnionId(int unionId)
        {
            var disciplinesDb = db.Disciplines.Where(d => d.UnionId == unionId && d.IsArchive == false).AsEnumerable();
            Mapper.Initialize(cfg => cfg.CreateMap<Discipline, DisciplineDTO>());
            return Mapper.Map<IEnumerable<Discipline>, IEnumerable<DisciplineDTO>>(disciplinesDb);
        }

        public IEnumerable<Discipline> GetAllByUnionIdWithAges(int[] disciplineIds)
        {
            return db.Disciplines.Where(x => disciplineIds.Contains(x.DisciplineId)).Include(x => x.CompetitionAges);
        }

        public List<Discipline> GetAllByUnionIdWithRoad(int unionId)
        {
            return db.Disciplines.Where(d => d.UnionId == unionId && d.RoadHeat == true && d.IsArchive == false).ToList();
        }

        public List<Discipline> GetAllByUnionIdWithMountain(int unionId)
        {
            return db.Disciplines.Where(d => d.UnionId == unionId && d.MountainHeat == true && d.IsArchive == false).ToList();
        }

        public DisciplinesDoc GetTermsDoc(int disciplineId)
        {
            return db.DisciplinesDocs.FirstOrDefault(t => t.DisciplineId == disciplineId);
        }

        public DisciplinesDoc GetDocById(int id)
        {
            return db.DisciplinesDocs.Find(id);
        }

        public void CreateDoc(DisciplinesDoc doc)
        {
            db.DisciplinesDocs.Add(doc);
        }

        public List<Discipline> GetByManagerId(int managerId)
        {
            return db.UsersJobs
                .Where(j => j.UserId == managerId)
                .Select(j => j.Discipline)
                .Where(u => u != null)
                .Distinct()
                .OrderBy(u => u.Name)
                .ToList();
        }

        public Discipline GetByIdWithUnion(int id)
        {
            return db.Disciplines
                .Include(d => d.Union)
                .Single(d => d.DisciplineId == id);
        }

        public bool DeleteRouteById(int id, bool deleteRelation = false)
        {
            var route = db.DisciplineRoutes.Find(id);

            if (deleteRelation)
            {
                foreach (var item in route.UsersRoutes.ToList())
                {
                    db.UsersRanks.RemoveRange(item.UsersRanks.ToList());

                    db.UsersRoutes.Remove(item);
                }

            }

            foreach (var rank in route.RouteRanks.ToList())
            {
                db.CompetitionRoutes.RemoveRange(rank.CompetitionRoutes.ToList());

                db.RouteRanks.Remove(rank);
            }

            db.DisciplineRoutes.Remove(route);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return true;
            }

            return false;
        }

        public bool DeleteTeamRouteById(int id, bool deleteRelation = false)
        {
            var route = db.DisciplineTeamRoutes.Find(id);

            if (deleteRelation)
            {
                foreach (var item in route.TeamsRoutes.ToList())
                {
                    db.TeamsRanks.RemoveRange(item.TeamsRanks.ToList());

                    db.TeamsRoutes.Remove(item);
                }
            }

            foreach (var rank in route.RouteTeamRanks.ToList())
                db.RouteTeamRanks.Remove(rank);
            db.DisciplineTeamRoutes.Remove(route);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return true;
            }

            return false;
        }

        public List<RouteRank> GetRanksByRouteId(int id)
        {
            return db.RouteRanks.Where(x => x.RouteId == id && x.IsArchived != true).ToList();
        }

        public RouteRank AddRankToRoute(int routeId, string rank,
            DateTime? fromAge = null, DateTime? toAge = null)
        {
            var newRank = new RouteRank
            {
                Rank = rank,
                FromAge = fromAge,
                ToAge = toAge,
                RouteId = routeId,
                IsArchived = false
            };
            db.RouteRanks.Add(newRank);
            return newRank;
        }

        public RouteTeamRank AddTeamRankToRoute(int routeId, string rank,
            DateTime? fromAge = null, DateTime? toAge = null)
        {
            var newRank = new RouteTeamRank
            {
                Rank = rank,
                FromAge = fromAge,
                ToAge = toAge,
                TeamRouteId = routeId
            };
            db.RouteTeamRanks.Add(newRank);
            return newRank;
        }

        public void DeleteRankById(int rankId)
        {
            var rank = db.RouteRanks.Find(rankId);
            db.RouteRanks.Remove(rank);
        }

        public DisciplineRoute GetRouteById(int id)
        {
            return db.DisciplineRoutes.Find(id);
        }

        public DisciplineTeamRoute GetTeamRouteById(int id)
        {
            return db.DisciplineTeamRoutes.Find(id);
        }

        public void DeleteRank(RouteRank rank)
        {
            db.UsersRanks.RemoveRange(rank.UsersRanks);

            //db.RouteRanks.Remove(rank);
            rank.IsArchived = true;
        }

        public void DeleteTeamRank(RouteTeamRank rank)
        {
            db.TeamsRanks.RemoveRange(rank.TeamsRanks);

            db.RouteTeamRanks.Remove(rank);
        }

        public RouteRank GetRankById(int id)
        {
            return db.RouteRanks.Find(id);
        }

        public RouteTeamRank GetTeamRankById(int id)
        {
            return db.RouteTeamRanks.Find(id);
        }

        public IEnumerable<DisciplineRoute> GetDisciplineRoutes(int disciplineId)
        {
            return db.DisciplineRoutes.Where(c => c.DisciplineId == disciplineId);
        }
        public IEnumerable<DisciplineTeamRoute> GetDisciplineTeamRoutes(int disciplineId)
        {
            return db.DisciplineTeamRoutes.Where(c => c.DisciplineId == disciplineId);
        }




        public IDictionary<int, IEnumerable<KeyValuePair<int, string>>> GetRanksByDisciplineId(int disciplineId)
        {
            var disciplineRoutes = db.DisciplineRoutes.Where(c => c.DisciplineId == disciplineId);
            var result = new Dictionary<int, IEnumerable<KeyValuePair<int, string>>>();
            if (disciplineRoutes.Any())
            {
                foreach (var route in disciplineRoutes)
                {
                    if (route.RouteRanks.Any())
                    {
                        result.Add(route.Id, GetRanksObject(route.RouteRanks));
                    }
                }
            }
            return result;
        }

        public IDictionary<int, IEnumerable<KeyValuePair<int, string>>> GetTeamRanksByDisciplineId(int disciplineId)
        {
            var disciplineRoutes = db.DisciplineTeamRoutes.Where(c => c.DisciplineId == disciplineId);
            var result = new Dictionary<int, IEnumerable<KeyValuePair<int, string>>>();
            if (disciplineRoutes.Any())
            {
                foreach (var route in disciplineRoutes)
                {
                    if (route.RouteTeamRanks.Any())
                    {
                        result.Add(route.Id, GetTeamRanksObject(route.RouteTeamRanks));
                    }
                }
            }
            return result;
        }

        private IEnumerable<KeyValuePair<int, string>> GetRanksObject(ICollection<RouteRank> routeRanks)
        {
            foreach (var rank in routeRanks)
            {
                if(rank.IsArchived != true)
                    yield return new KeyValuePair<int, string>(rank.Id, rank.Rank);
            }
        }

        private IEnumerable<KeyValuePair<int, string>> GetTeamRanksObject(ICollection<RouteTeamRank> routeRanks)
        {
            foreach (var rank in routeRanks)
            {
                yield return new KeyValuePair<int, string>(rank.Id, rank.Rank);
            }
        }

        public void CreateCompetitionRoute(CompetitionRouteDto dto)
        {
            try
            {
                var model = new CompetitionRoute
                {
                    DisciplineId = dto.DiciplineId,
                    LeagueId = dto.LeagueId,
                    SeasonId = dto.SeasonId,
                    RouteId = dto.RouteId,
                    RankId = dto.RankId,
                    Composition = dto.Composition,
                    InstrumentIds = dto.IntrumentsIds,
                    SecondComposition = dto.SecondComposition,
                    ThirdComposition = dto.ThirdComposition,
                    FourthComposition = dto.FourthComposition,
                    FifthComposition = dto.FifthComposition,
                    SixthComposition = dto.SixthComposition,
                    SeventhComposition = dto.SeventhComposition,
                    EighthComposition = dto.EighthComposition,
                    NinthComposition = dto.NinthComposition,
                    TenthComposition = dto.TenthComposition,
                    IsCompetitiveEnabled = dto.IsCompetitiveEnabled
                };
                db.CompetitionRoutes.Add(model);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreateCompetitionTeamRoute(CompetitionRouteDto dto)
        {
            try
            {
                var model = new CompetitionTeamRoute
                {
                    DisciplineId = dto.DiciplineId,
                    LeagueId = dto.LeagueId,
                    SeasonId = dto.SeasonId,
                    RouteId = dto.RouteId,
                    RankId = dto.RankId,
                    Composition = dto.Composition,
                    InstrumentIds = dto.IntrumentsIds,
                    SecondComposition = dto.SecondComposition,
                    ThirdComposition = dto.ThirdComposition,
                    FourthComposition = dto.FourthComposition,
                    FifthComposition = dto.FifthComposition,
                    SixthComposition = dto.SixthComposition,
                    SeventhComposition = dto.SeventhComposition,
                    EighthComposition = dto.EighthComposition,
                    NinthComposition = dto.NinthComposition,
                    TenthComposition = dto.TenthComposition,
                    IsCompetitiveEnabled = dto.IsCompetitiveEnabled
                };
                db.CompetitionTeamRoutes.Add(model);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<CompetitionRoute> GetCompetitionRoutes(int disciplineId, int seasonId, int leagueId)
        {
            return db.CompetitionRoutes
                .Include(x => x.DisciplineRoute)
                .Include(x => x.CompetitionRegistrations)
                .Include(x => x.RouteRank)
                .Include(x => x.RouteRank.UsersRanks)
                .Include(x => x.AdditionalGymnastics)
                .Where(x =>
                    x.DisciplineId == disciplineId &&
                    x.SeasonId == seasonId &&
                    x.LeagueId == leagueId)
                .ToList();
        }

        public List<CompetitionTeamRoute> GetCompetitionTeamRoutes(int disciplineId, int seasonId, int leagueId)
        {
            return db.CompetitionTeamRoutes
                .Where(cr =>
                    cr.DisciplineId == disciplineId &&
                    cr.SeasonId == seasonId &&
                    cr.LeagueId == leagueId)
                .ToList();
        }

        public void DeleteCompetitionRoute(int id)
        {
            var competitionRoute = db.CompetitionRoutes.FirstOrDefault(cr => cr.Id == id);
            if (competitionRoute != null)
            {
                if (competitionRoute.CompetitionRegistrations.Any())
                {
                    db.CompetitionRegistrations.RemoveRange(competitionRoute.CompetitionRegistrations);
                }
                db.CompetitionRoutes.Remove(competitionRoute);
                db.SaveChanges();
            }
        }


        public void DeleteCompetitionTeamRoute(int id)
        {
            var competitionRoute = db.CompetitionTeamRoutes.FirstOrDefault(cr => cr.Id == id);
            if (competitionRoute != null)
            {
                if (competitionRoute.CompetitionRegistrations.Any())
                {
                    db.CompetitionRegistrations.RemoveRange(competitionRoute.CompetitionRegistrations);
                }
                db.CompetitionTeamRoutes.Remove(competitionRoute);
                db.SaveChanges();
            }
        }

        public List<CompetitionDto> GetDisciplineCompetitions(List<int> clubDisciplinesIds, int seasonId)
        {
            var leagues = db.Leagues
                .Include(x => x.CompetitionRegistrations)
                .Where(x => x.DisciplineId != null &&
                            x.EndRegistrationDate != null &&
                            clubDisciplinesIds.Contains(x.DisciplineId.Value) &&
                            x.SeasonId == seasonId &&
                            !x.IsArchive)
                .ToList();
            var leaguesIds = leagues.Select(x => x.LeagueId).Distinct().ToArray();

            var disciplinesIds = leagues.Select(x => x.DisciplineId ?? 0).Where(x => x > 0).Distinct().ToArray();
            var leaguesDisciplines = db.Disciplines.Where(x => disciplinesIds.Contains(x.DisciplineId)).ToList();

            var jobsRepo = new JobsRepo(db);
            var disciplinesReferees = jobsRepo.GetDisciplineUsersJobs(disciplinesIds)
                .Where(x => x.RoleName == JobRole.Referee)
                .ToList();

            Expression<Func<CompetitionRoute, bool>> competitionsFilter =
                x =>
                    x.SeasonId == seasonId &&
                    disciplinesIds.Contains(x.DisciplineId) &&
                    leaguesIds.Contains(x.LeagueId);

            var competitionsRoutesQuery = db.CompetitionRoutes
                .Where(competitionsFilter);

            competitionsRoutesQuery.Select(x => x.DisciplineRoute).Load();
            competitionsRoutesQuery.SelectMany(x => x.CompetitionRegistrations).Load();
            competitionsRoutesQuery.Select(x => x.RouteRank).Include(x => x.UsersRanks).Load();
            competitionsRoutesQuery.SelectMany(x => x.AdditionalGymnastics).Load();

            var competitionsRoutes = competitionsRoutesQuery.ToList();

            var competitionTeamRoutes = db.CompetitionTeamRoutes
                .Where(x =>
                    x.SeasonId == seasonId &&
                    disciplinesIds.Contains(x.DisciplineId) &&
                    leaguesIds.Contains(x.LeagueId))
                .ToList();

            var competitions = new List<CompetitionDto>();

            foreach (var league in leagues)
            {
                var discipline = leaguesDisciplines.FirstOrDefault(x => x.DisciplineId == league.DisciplineId);
                
                competitions.Add(new CompetitionDto
                {
                    LeagueId = league.LeagueId,
                    SeasonId = league.SeasonId ?? 0,
                    CompetitionName = league.Name,

                    DisciplineId = discipline?.DisciplineId ?? 0,
                    DisciplineName = discipline?.Name,
                    DisciplineReferees = disciplinesReferees.Where(x => x.RelatedDisciplinesIds.Contains(discipline?.DisciplineId ?? 0))
                        .Select(x => new WorkerMainShortDto
                        {
                            UserJobId = x.Id,
                            UserFullName = x.FullName,
                            UserId = x.UserId
                        })
                        .ToList(),

                    StartDate = league.LeagueStartDate,
                    EndDate = league.LeagueEndDate,
                    StartRegistrationDate = league.StartRegistrationDate,
                    EndRegistrationDate = league.EndRegistrationDate,

                    CompetitionRoutes = competitionsRoutes.Where(x => x.LeagueId == league.LeagueId && x.DisciplineId == league.DisciplineId).ToList(),
                    CompetitionTeamRoutes = competitionTeamRoutes.Where(x => x.DisciplineId == league.DisciplineId && x.LeagueId == league.LeagueId).ToList(),

                    IsMaxed = league.MaxRegistrations.HasValue && league.MaxRegistrations <= league.CompetitionRegistrations.Count(),
                    IsEnded = league.EndRegistrationDate.HasValue && league.EndRegistrationDate.Value <= DateTime.Now,
                    IsStarted = !league.StartRegistrationDate.HasValue || league.StartRegistrationDate.Value <= DateTime.Now,
                    IsClubCompetition = league.IsClubCompetition

                });
            }

            var usersIds = competitions
                .SelectMany(x => x.CompetitionRoutes)
                .SelectMany(cr => cr.RouteRank.UsersRanks.Select(x => x.UserId))
                .Distinct()
                .ToArray();

            db.Users
                .Where(x => usersIds.Contains(x.UserId))
                .Load();

            return competitions;
        }


        public void CreateInsturment(InstrumentDto instrumentDto)
        {
            db.Instruments.Add(new Instrument
            {
                DisciplineId = instrumentDto.DisciplineId,
                Name = instrumentDto.Name,
                SeasonId = instrumentDto.SeasonId
            });
        }

        public IEnumerable<InstrumentDto> GetAllInstruments(int disciplineId, int seasonId)
        {
            return db.Instruments.Where(i => i.DisciplineId == disciplineId && i.SeasonId == seasonId)
                .OrderBy(x=>x.Name)
                .Select(i => new InstrumentDto
                {
                    Id = i.Id,
                    DisciplineId = i.DisciplineId,
                    SeasonId = i.SeasonId,
                    Name = i.Name
                });
        }

        public void DeleteInstrument(int id)
        {
            var instrument = db.Instruments.FirstOrDefault(i => i.Id == id);
            if (instrument != null)
            {
                db.Instruments.Remove(instrument);
            }
        }

        public void SetIsMultiBattle(int id, bool isMultiBattle)
        {
            var league = db.CompetitionDisciplines.FirstOrDefault(l => l.Id == id);
            if (league != null)
            {
                league.IsMultiBattle = isMultiBattle;
                Save();
            }
        }

        public void SetIsForScore(int id, bool isForScore)
        {
            var league = db.CompetitionDisciplines.FirstOrDefault(l => l.Id == id);
            if (league != null)
            {
                league.IsForScore = isForScore;
                Save();
            }
        }


        public void SetCorrectionForClubCompetition(int ClubId, int LeagueId, int SeasonId, decimal Correction, int GenderId, int? typeId)
        {
            var correction = db.CompetitionClubsCorrections.FirstOrDefault(cc => cc.ClubId == ClubId && cc.LeagueId == LeagueId && cc.SeasonId == SeasonId && cc.GenderId == GenderId && (!typeId.HasValue || typeId.Value == cc.TypeId));
            if (correction != null)
            {
                correction.Correction = Correction;
            }
            else
            {
                db.CompetitionClubsCorrections.Add(new CompetitionClubsCorrection {
                    ClubId = ClubId,
                    LeagueId = LeagueId,
                    SeasonId = SeasonId,
                    Correction = Correction,
                    GenderId = GenderId,
                    TypeId = typeId
                });
            }
            Save();
        }

        public void CreateDistance(string name, int seasonId)
        {
            db.RowingDistances.Add(new RowingDistance
            {
                Name = name,
                SeasonId = seasonId
            });
            Save();
        }

        public void EditDistance(int id, string name)
        {
            var distance = db.RowingDistances.FirstOrDefault(t => t.Id == id);
            if (distance != null)
            {
                distance.Name = name;
                Save();
            } 
        }

        public decimal GetCorrectionForClubCompetition(int ClubId, int LeagueId, int SeasonId, int genderId)
        {
            var correction = db.CompetitionClubsCorrections.FirstOrDefault(cc => cc.ClubId == ClubId && cc.LeagueId == LeagueId && cc.SeasonId == SeasonId && cc.GenderId == genderId);
            if (correction != null)
            {
                return correction.Correction;
            }
            return 0;
        }

        public void DeleteDistance(int id)
        {
            var distance = db.RowingDistances.FirstOrDefault(t => t.Id == id);
            if(distance != null)
            {
                db.RowingDistances.Remove(distance);
                Save();
            } 
        }

        
        public List<RowingDistance> GetDistanceTable(int seasonId)
        {
            return db.RowingDistances.Where( d => d.SeasonId == seasonId).ToList();
        }

        



        public void UpdateInstrumentName(int id, string instrumentName)
        {
            var instrument = db.Instruments.FirstOrDefault(i => i.Id == id);
            if (instrument != null)
            {
                instrument.Name = instrumentName;
            }
        }

        public string GetInstrumentsNames(string instrumentIds)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(instrumentIds))
            {
                var intsrumentsIds = instrumentIds.Split(',').Select(int.Parse);
                if (intsrumentsIds.Any())
                {
                    var instruments = db.Instruments.Where(i => intsrumentsIds.Contains(i.Id)).OrderBy(x => x.Name);
                    if (instruments.Any())
                    {
                        var intrumentNames = instruments.Select(i => i.Name);
                        result = string.Join(",", intrumentNames);
                    }
                }
            }
            return result;
        }

        public List<DisciplineStatistics> GetDisciplineStatistics(int id)
        {
            var disciplineResult = new List<DisciplineStatistics>();
            var unionDisciplines = db.Disciplines.Where(d => d.UnionId == id)?.ToList();
            if (unionDisciplines.Any())
            {
                foreach (var discipline in unionDisciplines)
                {
                    var approvedUsers = db.TeamsPlayers.Where(tp => !tp.User.IsArchive && tp.IsActive && tp.IsApprovedByManager == true &&
                        tp.User.PlayerDisciplines.Any(r => r.DisciplineId == discipline.DisciplineId)).AsNoTracking();
                    var groupedUsers = approvedUsers.GroupBy(u => u.UserId)?.Select(g => g.FirstOrDefault())?.AsNoTracking();
                    var activeGymnastics = groupedUsers.Where(g => g.User.CompetitionRegistrations
                         .Where(l => l.League.LeagueEndDate.HasValue && l.League.LeagueEndDate.Value < DateTime.Now && l.Position.HasValue)
                         .Select(c => c.LeagueId).Distinct().Count() >= 4)
                         ?.AsNoTracking().ToList() ?? new List<TeamsPlayer>();
                    disciplineResult.Add(new DisciplineStatistics
                    {
                        Name = discipline.Name,
                        NumberOfClubs = discipline.ClubDisciplines.Count,
                        Approved = groupedUsers.Count(),
                        Active = activeGymnastics.Count
                    });
                }
            }
            return disciplineResult;
        }

        public IEnumerable<CompetitionDiscipline> GetCompetitionDisciplines(int competitionId)
        {
            return db.CompetitionDisciplines.Include("League").Include("CompetitionDisciplineRegistrations").Include("CompetitionAge").Include("CompetitionDisciplineRegistrations.CompetitionResult").AsNoTracking().Where(d => d.CompetitionId == competitionId && d.IsDeleted == false).AsEnumerable();
        }
        public IEnumerable<CompetitionDiscipline> GetCompetitionDisciplinesTrack(int competitionId)
        {
            return db.CompetitionDisciplines.Include("League").Include("CompetitionDisciplineRegistrations").Include("CompetitionAge").Include("CompetitionDisciplineRegistrations.CompetitionResult").Where(d => d.CompetitionId == competitionId && d.IsDeleted == false).AsEnumerable();
        }

        public CompetitionDiscipline GetCompetitionDisciplineNoTracking(int discicplineId)
        {
            return db.CompetitionDisciplines.AsNoTracking().First(d => d.Id == discicplineId);
        }

        public void CreateCompetitionDiscipline(CompetitionDiscipline competitionDiscipline)
        {
            db.CompetitionDisciplines.Add(competitionDiscipline);
            db.SaveChanges();
        }

        public CompetitionAge GetCompetitionCategoryById(int id)
        {
            var age = db.CompetitionAges.FirstOrDefault(t => t.id == id);
            db.Entry(age).Reload();
            return age;
        }

        public List<CompetitionAge> GetCompetitionCategoriesByUnionId(int unionId)
        {
            var ages = db.CompetitionAges.Where(x => x.Discipline.UnionId == unionId);
            return ages.ToList();
        }

        public void DeleteCompetitionDiscipline(int id)
        {
            var compDiscipline = db.CompetitionDisciplines.FirstOrDefault(d => d.Id == id);
            if (compDiscipline != null)
            {
                compDiscipline.IsDeleted = true;
                db.SaveChanges();
            }
        }

        public void UpdateCompetitionDiscipline(CompetitionDiscipline competitionDiscipline)
        {
            var competitionDisciplines = db.CompetitionDisciplines.FirstOrDefault(t => t.Id == competitionDiscipline.Id);
            competitionDisciplines.DisciplineId = competitionDiscipline.DisciplineId;
            competitionDisciplines.CategoryId = competitionDiscipline.CategoryId;
            competitionDisciplines.MinResult = competitionDiscipline.MinResult;
            competitionDisciplines.MaxSportsmen = competitionDiscipline.MaxSportsmen;
            competitionDisciplines.StartTime = competitionDiscipline.StartTime;
            competitionDisciplines.DistanceId = competitionDiscipline.DistanceId;
            competitionDisciplines.IsResultsManualyRanked = competitionDiscipline.IsResultsManualyRanked;
            competitionDisciplines.IncludeRecordInStartList = competitionDiscipline.IncludeRecordInStartList;
            if (string.IsNullOrWhiteSpace(competitionDiscipline.CompetitionRecord))
            {
                competitionDisciplines.CompetitionRecord = null;
                competitionDisciplines.CompetitionRecordSortValue = null;
            }
            else
            {
                var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == competitionDisciplines.DisciplineId);
                if (discipline != null)
                {
                    competitionDisciplines.CompetitionRecord = competitionDiscipline.CompetitionRecord;
                    competitionDisciplines.CompetitionRecordSortValue = Convert.ToInt64(ConvertHelper.BuildResultSortValue(competitionDiscipline.CompetitionRecord, discipline.Format));
                }
                else
                {
                    competitionDisciplines.CompetitionRecord = null;
                    competitionDisciplines.CompetitionRecordSortValue = null;
                }
            }

            db.SaveChanges();
        }

        public CompetitionDiscipline GetCompetitionDisciplineById(int id)
        {
            return db.CompetitionDisciplines.FirstOrDefault(d => d.Id == id);
        }

        public Dictionary<int, int> GetRegistrationIdUserIdDictionary(int competitionDisciplineId)
        {
            return db.CompetitionDisciplineRegistrations
                .Where(x => x.CompetitionDisciplineId == competitionDisciplineId)
                .ToDictionary(x => x.UserId, x => x.Id);
        }

        public void UpdateHeatIsFinal(int heatId, bool isFinal)
        {
            var heat = db.CompetitionDisciplineHeatStartTimes.FirstOrDefault(x => x.Id == heatId);
            if (heat != null)
            {
                heat.IsFinal = isFinal;
                db.SaveChanges();
            }
        }

        public void UpdateHeatStartTime(int heatId, DateTime startTime)
        {
            var heat = db.CompetitionDisciplineHeatStartTimes.FirstOrDefault(x => x.Id == heatId);
            if (heat != null)
            {
                heat.StartTime = startTime;
                db.SaveChanges();
            }
        }

        public void DeleteHeat(int heatId)
        {
            var heat = GetHeatById(heatId);
            if (heat != null)
            {
                var competitionDisciplineId = heat.CompetitionDisciplineId;
                var lanesIds = db.CompetitionDisciplineRegistrations
                    .Where(x => x.CompetitionDisciplineId == competitionDisciplineId).Select(x => x.Id);
                var lanesToDelete = db.CompetitionResults.Where(x=> lanesIds.Contains(x.CompetitionRegistrationId) && x.Heat == heatId.ToString()).ToList();
                db.CompetitionDisciplineHeatStartTimes.Remove(heat);
                db.CompetitionResults.RemoveRange(lanesToDelete);
            }

            db.SaveChanges();
        }

        public CompetitionDisciplineHeatStartTime GetHeatById(int heatId)
        {
            return db.CompetitionDisciplineHeatStartTimes.FirstOrDefault(x => x.Id == heatId);
        }

        public bool SetCompetitionDisciplineHeatsGeneratedFalse(int? competitionDisciplineId)
        {
            var isLast = false;
            if (!db.CompetitionDisciplineHeatStartTimes.Any(x => x.CompetitionDisciplineId == competitionDisciplineId))
            {
                isLast = true;
                var competitionDiscipline = db.CompetitionDisciplines.FirstOrDefault(x => x.Id == competitionDisciplineId);
                if (competitionDiscipline != null)
                {
                    competitionDiscipline.HeatsGenerated = false;
                }

                db.SaveChanges();
            }

            return isLast;
        }

        public void SetCompetitionDisciplineHeatsGeneratedTrue(int? competitionDisciplineId)
        {
            var competitionDiscipline = db.CompetitionDisciplines.FirstOrDefault(x => x.Id == competitionDisciplineId);
            if (competitionDiscipline != null)
            {
                competitionDiscipline.HeatsGenerated = true;
            }
            db.SaveChanges();
        }

        public CompetitionResult GetSwimmerResult(int competitionDisciplineId, int userId)
        {
            return db.CompetitionDisciplineRegistrations
                .FirstOrDefault(x=>x.CompetitionDisciplineId == competitionDisciplineId && x.UserId == userId)?
                .CompetitionResult.FirstOrDefault(x=>x.Lane.HasValue);
        }
        
        public void DeleteSwimmerResult(int competitionDisciplineId, int userId)
        {
            var result = db.CompetitionDisciplineRegistrations
                .FirstOrDefault(x => x.CompetitionDisciplineId == competitionDisciplineId && x.UserId == userId)?
                .CompetitionResult.FirstOrDefault(x => x.Lane.HasValue);
            if (result!=null)
            {
                db.CompetitionResults.Remove(result);
                db.SaveChanges();
            }
        }

        public void UpdateCompetitionResultHeatLane(int competitionResultId, string heat, int? lane)
        {
            var competitionResult = db.CompetitionResults.FirstOrDefault(x => x.Id == competitionResultId);
            if (competitionResult != null)
            {
                competitionResult.Heat = heat;
                competitionResult.Lane = lane;
                db.SaveChanges();
            }
        }

        public CompetitionResult GetCompetitionResult(int competitionResultId)
        {
            return db.CompetitionResults.FirstOrDefault(x => x.Id == competitionResultId);
        }

        public void DeleteCompetitionResult(int competitionResultId)
        {
            var competitionResult = GetCompetitionResult(competitionResultId);
            db.CompetitionResults.Remove(competitionResult);
            db.SaveChanges();
        }

        public void AddSwimmerToHeat(int competitionDisciplineId, int userId, string heatId, int? laneId)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(x =>
                x.CompetitionDisciplineId == competitionDisciplineId && x.UserId == userId);
            if (registration != null)
            {
                db.CompetitionResults.Add(new CompetitionResult
                {
                    CompetitionRegistrationId = registration.Id,
                    Heat = heatId,
                    Lane = laneId
                });
                db.SaveChanges();
            }
        }

        public CompetitionDisciplineHeatStartTime SaveCompetitionDisciplineHeatStartTime(
            CompetitionDisciplineHeatStartTime competitionDisciplineHeatStartTime)
        {
            var saved = db.CompetitionDisciplineHeatStartTimes.Add(competitionDisciplineHeatStartTime);
            db.SaveChanges();
            return saved;
        }

        public void UpdateNumberOfPassesToNextStageForLiveResult(int id, int? numOfNextPass)
        {
            var cd = GetCompetitionDisciplineById(id);
            if(cd != null)
            {
                cd.NumberOfWhoPassesToNextStage = numOfNextPass;
                Save();
            }
        }

        public void UpdateRegisteredAthleteRecordLiveResult(int id, string record)
        {
            var reg = getCompetitionDisciplineRegistrationById(id);
            if (reg != null)
            {
                var result = reg.CompetitionResult.FirstOrDefault();
                if(result == null)
                {
                    result = new CompetitionResult();
                    reg.CompetitionResult.Add(result);
                }
                result.Records = record;
                Save();
            }
        }

        public DateTime? GetCompetitionDisciplineLastResultUpdateById(int id)
        {
            return db.CompetitionDisciplines.AsNoTracking().FirstOrDefault(d => d.Id == id)?.LastResultUpdate;
        }

        public void UpdateCompetitionResultResultTime(int competitionResultId, string resultTime)
        {
            var competitionResult = db.CompetitionResults.FirstOrDefault(x => x.Id == competitionResultId);
            if (competitionResult != null)
            {
                competitionResult.Result = resultTime;
                db.SaveChanges();
            }
        }

        public int GetNumberHeats(int competitionDisciplineId)
        {
            return db.CompetitionDisciplineHeatStartTimes.Count(x => x.CompetitionDisciplineId == competitionDisciplineId);
        }

        public void SaveNewHeat(int competitionDisciplineId, string heatName)
        {
            db.CompetitionDisciplineHeatStartTimes.Add(new CompetitionDisciplineHeatStartTime
            {
                CompetitionDisciplineId = competitionDisciplineId,
                HeatName = heatName
            });
            db.SaveChanges();
        }

        public CompetitionDiscipline GetCompetitionDisciplineByIdNoTrack(int id)
        {
            return db.CompetitionDisciplines.AsNoTracking().FirstOrDefault(d => d.Id == id);
        }

        public CompetitionResult GetCompetitionResultById(int id)
        {
            //return db.CompetitionResults.FirstOrDefault(d => d.CompetitionRegistrationId == id);
            return db.CompetitionResults.FirstOrDefault(d => d.Id == id);
        }


        public IEnumerable<CompetitionDisciplineDto> GetCompetitionDisciplines(
            League league, int clubId, int seasonId, string sectionAlias)
        {
            var disciplines = new List<CompetitionDisciplineDto>();

            foreach (var competitionDisciplines in league.CompetitionDisciplines.ToList())
            {
                //do not load deleted ones
                //UPDT: There was only check for Athletics section (sectionAlias == GamesAlias.Athletics) -> changed for all sections
                if (competitionDisciplines.IsDeleted) continue;
                

                var competitionRegistrations = competitionDisciplines.CompetitionDisciplineRegistrations.ToList();
                if(sectionAlias == GamesAlias.WeightLifting)
                {
                    competitionRegistrations = competitionRegistrations.Where(r => r.ClubId == clubId).ToList();
                }

                if (sectionAlias == GamesAlias.Rowing)
                {
                    if (league.NoTeamRegistration == true)
                    {
                        var team_count = competitionDisciplines.CompetitionDisciplineTeams.Where(x => x.ClubId == clubId).Count();
                        if (team_count == 0 && !competitionDisciplines.IsDeleted)
                        {
                            var competitionDiscipline = GetCompetitionDisciplineById(competitionDisciplines.Id);
                            var newTeam = new CompetitionDisciplineTeam()
                            {
                                ClubId = clubId,
                                CompetitionDisciplineId = competitionDisciplines.Id,
                                TeamNumber = 1
                            };

                            competitionDiscipline.CompetitionDisciplineTeams.Add(newTeam);
                            Save();
                        }
                    }
                }
                bool isAthletics = sectionAlias == SectionAliases.Athletics;
                disciplines.Add(new CompetitionDisciplineDto
                {
                    Id = competitionDisciplines.Id,
                    DisciplineId = competitionDisciplines.DisciplineId,
                    CategoryId = competitionDisciplines.CategoryId,
                    IsMixed = competitionDisciplines.CompetitionAge.IsMix ?? false,
                    CompetitionId = competitionDisciplines.CompetitionId,
                    MaxSportsmen = competitionDisciplines.MaxSportsmen,
                    MinResult = competitionDisciplines.MinResult,
                    StartTime = competitionDisciplines.StartTime,
                    IsDeleted = competitionDisciplines.IsDeleted,
                    DistanceName = competitionDisciplines.RowingDistance?.Name,
                    DisciplinePlayers = GetCompetitionDisciplinePlayers(competitionDisciplines, clubId, seasonId, sectionAlias).ToList(),
                    RegisteredPlayers = competitionRegistrations.Select(cdr => new CompDiscRegDTO
                    {
                        UserId = cdr.UserId,
                        UserName = cdr.User.FullName,
                        TeamTitle = cdr.CompetitionDiscipline.CompetitionAge.age_name,
                        WeightDeclaration = cdr.WeightDeclaration.HasValue ? (int)cdr.WeightDeclaration.Value : 0,
                        TeamId = cdr.CompetitionDisciplineTeamId,
                        IsCoxwain = cdr.IsCoxwain ?? false
                    }).ToList(),
                    TeamRegistration = isAthletics ? 0 : competitionDisciplines.CompetitionDisciplineClubsRegistrations.FirstOrDefault(x => x.ClubId == clubId)?.TeamRegistrations ?? 0,
                    DisciplineTeams = isAthletics ? null : competitionDisciplines.CompetitionDisciplineTeams.Where(x => x.ClubId == clubId).Select(cdt => new CompDiscTeamDTO
                    {
                        TeamId = cdt.Id,
                        ClubId = cdt.ClubId,
                        CompetitionDisciplineId = cdt.CompetitionDisciplineId,
                        TeamNumber = cdt.TeamNumber
                    }),
                    Coxwain = isAthletics ? false : GetById(competitionDisciplines.DisciplineId ?? 0)?.Coxwain ?? false,
                    NumberofSportsmen = db.Disciplines.FirstOrDefault(dp => dp.DisciplineId == competitionDisciplines.DisciplineId)?.NumberOfSportsmen ?? 0,
					Result = "0:00:00",
					Rank = -1
				});
            }

            return disciplines;
        }

        public IEnumerable<CompDiscRegDTO> GetCompetitionAllDisciplinePlayers(
            CompetitionDiscipline discipline, int seasonId, string sectionAlias)
        {
            var fromAge = discipline?.CompetitionAge.from_birth;
            var toAge = discipline?.CompetitionAge.to_birth;

            var genderId = discipline?.CompetitionAge.gender;
            //var minResult = discipline.MinResult;
            //TODO: Add filter for the min result
            var players = db.TeamsPlayers.Where(c => c.SeasonId == seasonId &&
                                                     c.IsApprovedByManager == true &&
                                                     (!c.User.PlayersBlockade.IsActive ||
                                                      c.User.PlayersBlockade == null));

            if(sectionAlias == GamesAlias.Athletics)
            {
                players = players.Where(c => (genderId == 3 || c.User.GenderId == genderId) &&
                                                     fromAge <= c.User.BirthDay &&
                                                     toAge >= c.User.BirthDay);
            }

            if (sectionAlias == GamesAlias.WeightLifting)
            {
                var fromWeight = discipline.CompetitionAge.from_weight;
                var toWeight = discipline.CompetitionAge.to_weight;
                players = players.Where(c => (fromWeight == null || c.User.Weight >= fromWeight) &&
                                             (toWeight == null || c.User.Weight <= toWeight));
            }

            foreach (var player in players)
            {
                var athleteStr =
                    player.User.AthleteNumbers
                        .FirstOrDefault(x => x.SeasonId == seasonId)
                        ?.AthleteNumber1
                        ?.ToString()
                    ?? string.Empty;

                yield return new CompDiscRegDTO
                {
                    UserId = player.User.UserId,
                    UserName = $"{player.User.FullName} {athleteStr}"
                };
            }
        }

        public IEnumerable<CompDiscRegDTO> GetCompetitionDisciplinePlayers(
            CompetitionDiscipline discipline, int clubId, int seasonId, string sectionAlias)
        {
            var fromAge = discipline.CompetitionAge.from_birth;
            var toAge = discipline.CompetitionAge.to_birth;
            var avgAge = false;

            var genderId = discipline.CompetitionAge.gender;
            //var minResult = discipline.MinResult;
            //TODO: Add filter for the min result
            var players = db.TeamsPlayers.Where(c => c.ClubId == clubId && c.SeasonId == seasonId && c.IsApprovedByManager == true && (genderId == 3 || c.User.GenderId == genderId) && (!c.User.PlayersBlockade.IsActive || c.User.PlayersBlockade == null));
            int? seasonForAge = null;
            if (sectionAlias == GamesAlias.Rowing)
            {
                avgAge = discipline.CompetitionAge.AverageAge != null && discipline.CompetitionAge.AverageAge > 0;
                seasonForAge = players.FirstOrDefault()?.Season?.SeasonForAge;
            }
            if(!avgAge)
            {
                players = players.Where(x => fromAge <= x.User.BirthDay && toAge >= x.User.BirthDay);
            }
 

            if (sectionAlias == GamesAlias.Athletics) {
                players = players.Where(c => c.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).AthleteNumber1 != null);
            }
            if (sectionAlias == GamesAlias.Swimming)
            {
                var _class = db.Disciplines.FirstOrDefault(d => d.DisciplineId == discipline.DisciplineId)?.Class;
                players = players.Where(c => (c.User.ClassS.HasValue && _class == 1) || (c.User.ClassSB.HasValue && _class == 2) || (c.User.ClassSM.HasValue && _class == 3));
            }
            if (sectionAlias == GamesAlias.WeightLifting)
            {
                var fromWeight = discipline.CompetitionAge.from_weight;
                var toWeight = discipline.CompetitionAge.to_weight;
                players = players.Where(c => (fromWeight == null || c.User.Weight >= fromWeight)
                                             && (toWeight == null || c.User.Weight <= toWeight));
            }

            var listedUsers = players.Select(p => p.User).ToList();

            if (sectionAlias == GamesAlias.Rowing)
            {
                var playersInDifferentClubs = db.TeamsPlayers.Where(c => c.ClubId != clubId && c.SeasonId == seasonId && c.IsApprovedByManager == true && (genderId == 3 || c.User.GenderId == genderId) && (!c.User.PlayersBlockade.IsActive || c.User.PlayersBlockade == null));
                if (!avgAge)
                {
                    playersInDifferentClubs = playersInDifferentClubs.Where(x => fromAge <= x.User.BirthDay && toAge >= x.User.BirthDay);
                }

                if (!discipline.League.IsCompetitionLeague)
                {
                    listedUsers.AddRange(playersInDifferentClubs.Select(t => t.User));
                }

                //Rowing - For boat of 1 sportsman allowed the average age is
                //the minimum and we can keep the validation of showing only valid sportsmans
                //to register on dropdown
                var discp = GetById(discipline.DisciplineId.Value);
                if (avgAge && discp.NumberOfSportsmen == 1)
                {
                    listedUsers = listedUsers.Where(x => CalculateAge(x.BirthDay) >= discipline.CompetitionAge.AverageAge).ToList();
                }
            }
            else
            {
                var satisfyRequirementPlayerIds = listedUsers.Select(p => p.UserId).ToList();
                var registeredIds = discipline.CompetitionDisciplineRegistrations.Where(r => r.ClubId == clubId).Select(r => r.UserId).ToList();
                var unsatisfiedRequirementsRegistered = registeredIds.Except(satisfyRequirementPlayerIds).ToList();
                var unsatisfyRequirementRegisteredPlayersList = discipline.CompetitionDisciplineRegistrations.Where(p => unsatisfiedRequirementsRegistered.Contains(p.UserId)).Select(r => r.User).ToList();
                listedUsers.AddRange(unsatisfyRequirementRegisteredPlayersList);
            }
            
            foreach (var user in listedUsers)
            {
                var athleteStr = user.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId)?.AthleteNumber1.HasValue == true ? user.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId)?.AthleteNumber1.Value.ToString() : string.Empty;
                var clubName = "";
                int? userAge = null;
                if (sectionAlias == GamesAlias.Rowing)
                {
                    clubName = $" - {user.TeamsPlayers.Where( t => t.SeasonId == seasonId && t.IsApprovedByManager == true).FirstOrDefault()?.Club?.Name}";
                    if (seasonForAge.HasValue && user.BirthDay.HasValue)
                    {
                        userAge = seasonForAge.Value - user.BirthDay.Value.Year;
                    }
                }
                yield return new CompDiscRegDTO
                {
                    UserId = user.UserId,
                    UserName = $"{user.FullName} {athleteStr}",
                    TeamTitle = clubName,
                    IdGender = user.UserId.ToString() + "_" + user.GenderId.ToString(),
                    UserNameAge = userAge != null ? $"{user.FullName} {athleteStr} - {userAge}" : $"{user.FullName} {athleteStr}"
                };
            }
        }

        public List<CompDiscRegDTO> GetRegisteredSwimmersForCompetitionDiscipline (int competitionDisciplineId, int seasonId)
        {
            var discipline = db.CompetitionDisciplines.FirstOrDefault(x => x.Id == competitionDisciplineId);
            var competitionRegistrations = discipline.CompetitionDisciplineRegistrations.ToList();
            return competitionRegistrations.Select(cdr => new CompDiscRegDTO
            {
                UserId = cdr.UserId,
                UserName = cdr.User.FullName
            }).ToList();
        }

        /*
        public ClubDiscipline GetClubDisciplineById(int id) {
            return db.ClubDisciplines.FirstOrDefault(d => d.Id == id);
        }
        */

        public void UpdateCompetitionRouteClubMaximumRegistrations(int CompetitionRouteId, int ClubId , int? maximumRegistrationsAllowed) {

            var competitionRouteClub = db.CompetitionRouteClubs.FirstOrDefault(x => x.ClubId == ClubId && x.CompetitionRouteId == CompetitionRouteId);
            if (competitionRouteClub == null) {
                db.CompetitionRouteClubs.Add(new CompetitionRouteClub {
                    ClubId = ClubId,
                    CompetitionRouteId = CompetitionRouteId,
                    MaximumRegistrationsAllowed = maximumRegistrationsAllowed
                });
            }
            else
                competitionRouteClub.MaximumRegistrationsAllowed = maximumRegistrationsAllowed;
            Save();
        }

        public void UpdateCompetitionTeamRouteClubMaximumRegistrations(int CompetitionRouteId, int ClubId, int? maximumRegistrationsAllowed)
        {

            var competitionRouteClub = db.CompetitionTeamRouteClubs.FirstOrDefault(x => x.ClubId == ClubId && x.CompetitionTeamRouteId == CompetitionRouteId);
            if (competitionRouteClub == null)
            {
                db.CompetitionTeamRouteClubs.Add(new CompetitionTeamRouteClub
                {
                    ClubId = ClubId,
                    CompetitionTeamRouteId = CompetitionRouteId,
                    MaximumRegistrationsAllowed = maximumRegistrationsAllowed
                });
            }
            else
                competitionRouteClub.MaximumRegistrationsAllowed = maximumRegistrationsAllowed;
            Save();
        }

        

        public IEnumerable<ComparableCompDiscRegDTO> GetAllClubsCompetitionDisciplinePlayers(CompetitionDiscipline discipline, int seasonId, string sectionAlias)
        {
            var fromAge = discipline.CompetitionAge.from_birth;
            var toAge = discipline.CompetitionAge.to_birth;

            var genderId = discipline.CompetitionAge.gender;
            //var minResult = discipline.MinResult;
            //TODO: Add filter for the min result
            var players = db.TeamsPlayers.Where(c => c.ClubId != null && c.SeasonId == seasonId && c.IsApprovedByManager == true && (genderId == 3 || c.User.GenderId == genderId) && fromAge <= c.User.BirthDay && toAge >= c.User.BirthDay && (!c.User.PlayersBlockade.IsActive || c.User.PlayersBlockade == null));
            if (sectionAlias == GamesAlias.Athletics)
            {
                players = players.Where(c => c.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).AthleteNumber1 != null);
            }
            if (sectionAlias == GamesAlias.WeightLifting)
            {
                var fromWeight = discipline.CompetitionAge.from_weight;
                var toWeight = discipline.CompetitionAge.to_weight;
                players = players.Where(c => (fromWeight == null || c.User.Weight >= fromWeight)
                                             && (toWeight == null || c.User.Weight <= toWeight));
            }

            foreach (var player in players)
            {
                var athleteStr = player.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).AthleteNumber1.HasValue ? player.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).AthleteNumber1.Value.ToString() : "";
                yield return new ComparableCompDiscRegDTO
                {
                    UserId = player.User.UserId,
                    UserName = $"{player.User.FullName} {athleteStr}",
                    IdentNum = player.User.IdentNum,
                    PassportNum = player.User.PassportNum,
                    ClubId = player.ClubId
                };
            }
        }

        public void RegisterSportsmenUnderCompetitionDiscipline(int competitionDisciplineId, int clubId, IEnumerable<int> sportsmenIds, bool hasHighAuthority, int? maxRegistrations,int currentRegistrationCount, string sectionAlias, int? teamId = null, IEnumerable<int> coxwainIds = null)
        {
            var activeRegistrations =
                db.CompetitionDisciplineRegistrations.Where(x =>
                    !x.IsArchive &&
                    x.CompetitionDisciplineId == competitionDisciplineId &&
                    (x.ClubId == clubId || sectionAlias == GamesAlias.Rowing));
            var disciplineId = db.CompetitionDisciplines.FirstOrDefault(x=>x.Id == competitionDisciplineId)?.DisciplineId;

            if (!hasHighAuthority && maxRegistrations != null)
            {
                var attemptRegisterCount = sportsmenIds.Count();
                if (sectionAlias != GamesAlias.Rowing)
                {
                    int allRegisteredWithoutThisClub = currentRegistrationCount - activeRegistrations.Count();
                    if (maxRegistrations.Value >= allRegisteredWithoutThisClub + attemptRegisterCount)
                    {
                        db.CompetitionDisciplineRegistrations.RemoveRange(activeRegistrations);
                    }
                    else
                    {
                        throw new MaximumRequiredException() { NumOfRegistrationsLeft = maxRegistrations.Value - allRegisteredWithoutThisClub };
                    }
                }
                else
                {
                    var discipline = GetCompetitionDisciplineById(competitionDisciplineId);
                    var female_exist = false;
                    var registered = false;
                    var user_id = 0;
                    var user_name = "";
                    foreach (int id in sportsmenIds)
                    {
                        var reg_cnt = db.TeamsPlayers.Where(x => x.ClubId == clubId && x.UserId == id).Count();
                        user_id = id;
                        user_name = db.Users.FirstOrDefault(x => x.UserId == id).FullName;
                        if (reg_cnt > 0)
                        {
                            registered = true;
                        }

                        if (db.Users.Where(x => x.UserId == id).FirstOrDefault().GenderId == 0)
                        {
                            female_exist = true;
                        }
                    }

                    if (discipline.CompetitionAge.IsMix == true && female_exist == false)
                    {
                        throw new MixedCategoryException() { };
                    }

                    if (registered == false)
                    {
                        throw new UnRegisteredPlayersException() { UserId = user_id, UserName = user_name };
                    }

                    var hasCoxwain = GetById(discipline.DisciplineId.Value)?.Coxwain ?? false;
                    if (hasCoxwain) maxRegistrations += 1;
                    //remove team
                    if(attemptRegisterCount == 0 || (hasCoxwain && coxwainIds.Count() == 0))
                    {
                        var checkExisting = activeRegistrations.Where(x => x.ClubId == clubId && x.CompetitionDisciplineTeamId == teamId);
                        if (hasCoxwain)
                        {
                            if(attemptRegisterCount == 0 && checkExisting.Count() > 0)
                            {
                                db.CompetitionDisciplineRegistrations.RemoveRange(checkExisting);
                                db.SaveChanges();
                                return;
                            }
                        }
                        else
                        {
                            if (attemptRegisterCount == 0 && checkExisting.Count() > 0)
                            {
                                db.CompetitionDisciplineRegistrations.RemoveRange(checkExisting);
                                db.SaveChanges();
                                return;
                            }
                        }                        
                    }

                    if (maxRegistrations.Value == sportsmenIds.Count() + coxwainIds.Count())
                    {
                        var league = discipline.League;
                        var max_available = league.MaxParticipationAllowedForSportsman;
                        var currentRegForUSer = league.CompetitionDisciplines;
                        //check if the player(s) combination is already registered for another Team
                        if (attemptRegisterCount > 0)
                        {
                            foreach (var sportsmanId in sportsmenIds)
                            {
                                var reg_count = 0;
                                var actReg = activeRegistrations.FirstOrDefault(x => x.UserId == sportsmanId);
                                foreach (var compDisc in currentRegForUSer)
                                {
                                    reg_count += compDisc.CompetitionDisciplineRegistrations.Count(x => x.UserId == sportsmanId && x.ClubId == clubId);
                                    if (actReg == null)
                                    {
                                        actReg = compDisc.CompetitionDisciplineRegistrations.Where(x => x.UserId == sportsmanId && x.ClubId == clubId).FirstOrDefault();
                                    }
                                }
                                
                                if (max_available.HasValue && reg_count + 1 > max_available)
                                {
                                    throw new MaxParticipationAllowedForSportsmanReachedException()
                                    {
                                        MaxParticipationAllowed = league.MaxParticipationAllowedForSportsman.Value,
                                        UserName = actReg.User.FullName,
                                        UserId = actReg.UserId
                                    };
                                }
                                else if (!max_available.HasValue && actReg != null)
                                {
                                    throw new PlayerAlreadyRegisteredException() { UserName = actReg.User.FullName, UserId = actReg.UserId };
                                }
                            }
                            if (hasCoxwain)
                            {
                                var actReg = activeRegistrations.FirstOrDefault(x => x.CompetitionDisciplineTeamId != teamId && x.UserId == coxwainIds.FirstOrDefault());
                                if (actReg != null)
                                {
                                    throw new PlayerAlreadyRegisteredException() { UserName = actReg.User.FullName, UserId = actReg.UserId, IsCoxwain = true };
                                }

                                //check if coxwain is not part of players
                                if(sportsmenIds.Where(x => x == coxwainIds.FirstOrDefault()).FirstOrDefault() != 0)
                                {
                                    throw new CoxwainAlsoPartOfTeamException()
                                    {
                                        IsCoxwain = true,
                                        UserId = coxwainIds.FirstOrDefault()
                                    };
                                }

                                //check if coxwain is part of sportsmanIds
                                var countReg = 0;
                                foreach (var compDisc in currentRegForUSer)
                                {
                                    countReg += compDisc.CompetitionDisciplineRegistrations.Count(x => x.UserId == coxwainIds.FirstOrDefault());
                                }
                                var isReg = activeRegistrations.FirstOrDefault(x => x.CompetitionDisciplineTeamId == teamId && x.UserId == coxwainIds.FirstOrDefault());
                                if (isReg != null) countReg = countReg - 1;
                                if (league.MaxParticipationAllowedForSportsman.HasValue && countReg + 1 > league.MaxParticipationAllowedForSportsman)
                                {
                                    throw new MaxParticipationAllowedForSportsmanReachedException()
                                    {
                                        MaxParticipationAllowed = league.MaxParticipationAllowedForSportsman.Value,
                                        UserName = actReg.User.FullName,
                                        UserId = actReg.UserId,
                                        IsCoxwain = true
                                    };
                                }

                            }
                        }
                        
                        var avgAge = discipline.CompetitionAge.AverageAge != null && discipline.CompetitionAge.AverageAge > 0;
                        if(avgAge)
                        {
                            //check average age of the team
                            var ageSum = 0;
                            int avgValue = discipline.CompetitionAge.AverageAge.Value;
                            foreach (var sportsmanId in sportsmenIds)
                            {
                                var user = db.Users.FirstOrDefault(x => x.UserId == sportsmanId);
                                if (user != null)
                                {
                                    ageSum += CalculateAge(user.BirthDay);
                                }
                            }
                            if(hasCoxwain)
                            {
                                var user = db.Users.FirstOrDefault(x => x.UserId == coxwainIds.FirstOrDefault());
                                if (user != null)
                                {
                                    ageSum += CalculateAge(user.BirthDay);
                                }
                            }

                            var average = ageSum / (sportsmenIds.Count() + coxwainIds.Count());
                            if(average < avgValue)
                            {
                                activeRegistrations = activeRegistrations.Where(x => x.ClubId == clubId && x.CompetitionDisciplineTeamId == teamId);
                                if (activeRegistrations.Count() > 0)
                                {
                                    db.CompetitionDisciplineRegistrations.RemoveRange(activeRegistrations);
                                    db.SaveChanges();
                                }
                                throw new AverageAgeException() { AverageAgeValue = avgValue };
                            }
                        }

                        activeRegistrations = activeRegistrations.Where(x => x.ClubId == clubId && x.CompetitionDisciplineTeamId == teamId);
                        db.CompetitionDisciplineRegistrations.RemoveRange(activeRegistrations);

                        if(hasCoxwain)
                        {
                            db.CompetitionDisciplineRegistrations.Add(new CompetitionDisciplineRegistration
                            {
                                UserId = coxwainIds.FirstOrDefault(),
                                CompetitionDisciplineId = competitionDisciplineId,
                                ClubId = clubId,
                                CompetitionDisciplineTeamId = teamId,
                                IsCoxwain = true
                            });
                        }
                    }
                    else
                    {
                        throw new MaximumRequiredException() { NumOfRegistrationsLeft = maxRegistrations.Value };
                    }
                }
            }else
                db.CompetitionDisciplineRegistrations.RemoveRange(activeRegistrations);

            var seasonId = db.Clubs.FirstOrDefault(x => x.ClubId == clubId)?.SeasonId;

            if (sectionAlias == GamesAlias.Swimming)
            {
                var activePlayerDisciplines = db.PlayerDisciplines.Where(x => x.DisciplineId == disciplineId && x.ClubId == clubId && x.SeasonId == seasonId).ToList();
                if (activePlayerDisciplines.Count > 0)
                {
                    db.PlayerDisciplines.RemoveRange(activePlayerDisciplines);
                }
            }

            foreach (var sportsmanId in sportsmenIds)
            {
                db.CompetitionDisciplineRegistrations.Add(new CompetitionDisciplineRegistration
                {
                    UserId = sportsmanId,
                    ClubId = clubId,
                    CompetitionDisciplineTeamId = teamId,
                    CompetitionDisciplineId = competitionDisciplineId,
                });
                if (sectionAlias == GamesAlias.Swimming)
                {
                    db.PlayerDisciplines.Add(new PlayerDiscipline
                    {
                        PlayerId = sportsmanId,
                        DisciplineId = disciplineId.HasValue ? disciplineId.Value : 0,
                        ClubId = clubId,
                        SeasonId = seasonId ?? 0
                    });
                }
            }
            db.SaveChanges();
        }


        public int RegisterAthleteUnderCompetitionDiscipline(int disciplineId, int? clubId, int userId)
        {
            var activeRegistrations = db.CompetitionDisciplineRegistrations
                .FirstOrDefault(x => x.UserId == userId &&
                                     x.CompetitionDisciplineId == disciplineId);

            if (activeRegistrations == null && clubId.HasValue)
            {
                var reg = new CompetitionDisciplineRegistration
                {
                    UserId = userId,
                    CompetitionDisciplineId = disciplineId,
                    ClubId = clubId
                };
                db.CompetitionDisciplineRegistrations.Add(reg);
                db.SaveChanges();
                return reg.Id;
            }

            return 0;
        }

        public int RegisterWeightLifterUnderCompetitionDiscipline(int disciplineId, int? clubId, int userId, decimal? weightDeclaration)
        {
            var activeRegistrations = db.CompetitionDisciplineRegistrations.Where(x => x.UserId == userId && x.CompetitionDisciplineId == disciplineId).FirstOrDefault();
            if (activeRegistrations == null && clubId.HasValue)
            {
                var reg = new CompetitionDisciplineRegistration
                {
                    UserId = userId,
                    CompetitionDisciplineId = disciplineId,
                    ClubId = clubId,
                    WeightDeclaration = weightDeclaration
                };
                db.CompetitionDisciplineRegistrations.Add(reg);
                db.SaveChanges();
                return reg.Id;
            }
            return 0;
        }

        public int RegisterBicycleRiderUnderCompetitionExpertise(int compExpId, int? clubId, int userId)
        {
            var activeRegistrations = db.BicycleDisciplineRegistrations.Where(x => x.UserId == userId && x.CompetitionExpertiesHeatId == compExpId).FirstOrDefault();
            if (activeRegistrations == null && clubId.HasValue)
            {
                var reg = new BicycleDisciplineRegistration
                {
                    UserId = userId,
                    CompetitionExpertiesHeatId = compExpId,
                    ClubId = clubId                    
                };
                db.BicycleDisciplineRegistrations.Add(reg);
                db.SaveChanges();
                return reg.Id;
            }
            return 0;
        }

        public CompetitionDisciplineRegistration getCompetitionDisciplineRegistrationById(int Id) {
            return db.CompetitionDisciplineRegistrations.Where(x => x.Id == Id).FirstOrDefault();
        }


        public void DeleteDisciplineRegistration(int id)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(t => t.Id == id);
            if (registration != null)
            {
                db.CompetitionDisciplineRegistrations.Remove(registration);
                db.SaveChanges();
            }
        }

        public void UpdateDiscipline(int id, string name, int? format, int? _class, string disciplineType, int? NumberOfSportsmen, bool isBicycleSection, bool roadHeat, bool mountainHeat, bool isRowingSection, bool coxwain)
        {
            var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == id);
            if (discipline != null) {
                discipline.Name = name;
                discipline.Format = format;
                discipline.Class = _class;
                discipline.DisciplineType = disciplineType;
                discipline.NumberOfSportsmen = NumberOfSportsmen;
                if (isBicycleSection)
                {
                    discipline.MountainHeat = mountainHeat;
                    discipline.RoadHeat = roadHeat;
                }
                if (isRowingSection)
                {
                    discipline.Coxwain = coxwain;
                }
                db.SaveChanges();
            }
        }

        public List<CompetitionHeatWind> GetCompetitionHeatWindList(int id)
        {
            return db.CompetitionHeatWinds.Where(chw => chw.DisciplineCompetitionId == id).ToList();
        }

        public void CreateHeatWindAssociation(CompetitionHeatWind form)
        {
            db.CompetitionHeatWinds.Add(form);
            Save();
        }

        public int DeleteHeatWindAssociation(int Id)
        {
            var competitionHeatWinds = db.CompetitionHeatWinds.FirstOrDefault(c => c.Id == Id);
            if (competitionHeatWinds != null)
            {
                var compId = competitionHeatWinds.DisciplineCompetitionId;
                db.CompetitionHeatWinds.Remove(competitionHeatWinds);
                Save();
                return compId;
            }
            return 0;
        }

        public WeightLiftingSession GetCompetitionSession(int sessionId)
        {
            return db.WeightLiftingSessions.FirstOrDefault(w => w.Id == sessionId);
        }

        public IEnumerable<WeightLiftingSession> GetCompetitionSessions(int leagueId)
        {
            return db.WeightLiftingSessions.Where(w => w.CompetitionId == leagueId);
        }

        public void CreateWeightliftingSession(int competitionId, DateTime startTime, DateTime weightStartTime, DateTime weightFinishTime)
        {
            var sessions = db.WeightLiftingSessions.Where(w => w.CompetitionId == competitionId).ToList();
            int sessionNum = 1;
            var last = sessions.LastOrDefault();
            if(last != null)
                sessionNum = sessions.LastOrDefault().SessionNum+1;

            db.WeightLiftingSessions.Add(new WeightLiftingSession {
                CompetitionId = competitionId,
                SessionNum = sessionNum,
                StartTime = startTime,
                WeightStartTime = weightStartTime,
                WeightFinishTime = weightFinishTime
            });
            Save();
        }


        public void UpdateWeightliftingSession(int sessionId, int competitionId, int sessionNum, DateTime? startTime, DateTime? weightStartTime, DateTime? weightFinishTime)
        {
            var session = db.WeightLiftingSessions.FirstOrDefault(w => w.CompetitionId == competitionId && w.Id == sessionId);
            if(session != null)
            {
                session.SessionNum = sessionNum;
                if(startTime.HasValue)
                    session.StartTime = startTime.Value;
                if (weightStartTime.HasValue)
                    session.WeightStartTime = weightStartTime.Value;
                if (weightFinishTime.HasValue)
                    session.WeightFinishTime = weightFinishTime.Value;
                Save();
            }
            
        }

        public void UpdateDisciplineRegistrationSession(int id, int? sessionId)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(t => t.Id == id);
            if (registration != null)
            {
                registration.WeightliftingSessionId = sessionId;
                db.SaveChanges();
            }
        }

        public void DeleteWeightliftingSession(int id)
        {
            var session = db.WeightLiftingSessions.FirstOrDefault(w => w.Id == id);
            if(session != null)
            {
                db.WeightLiftingSessions.Remove(session);
                Save();
            }

        }

        public void UpdateRegistrationWeight(int registrationId, decimal? registrationWeight, decimal? weightDeclaration)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(c => c.Id == registrationId);
            if (registration != null) {
                registration.Weight = registrationWeight;
                registration.WeightDeclaration = weightDeclaration;
                Save();
            }
        }

        public void UpdateRegistrationLiftingPush(int registrationId, int? registrationLifting, int? registrationPush)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(c => c.Id == registrationId);
            if (registration != null)
            {
                var result = registration.CompetitionResult.FirstOrDefault();
                if (result == null)
                {
                    registration.CompetitionResult.Add(new CompetitionResult
                    {
                        Lifting1 = registrationLifting,
                        Push1 = registrationPush
                    });
                    Save();
                }
                else
                {
                    result.Lifting1 = registrationLifting;
                    result.Push1 = registrationPush;
                    Save();
                }
            }
        }

        public void ModifyAthleteCompetitionHeat(int regId, string heat)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(r => r.Id == regId);
            if (registration != null) {
                var result = registration.CompetitionResult.FirstOrDefault();
                if (result == null)
                {
                    registration.CompetitionResult.Add(new CompetitionResult {
                        Heat = heat
                    });
                    Save();
                }
                else {
                    result.Heat = heat;
                    Save();
                }
            }
        }
        public void ModifyAthleteCompetitionLane(int regId, int? lane)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(r => r.Id == regId);
            if (registration != null)
            {
                var result = registration.CompetitionResult.FirstOrDefault();
                if (result == null)
                {
                    registration.CompetitionResult.Add(new CompetitionResult
                    {
                        Lane = lane
                    });
                    Save();
                }
                else
                {
                    result.Lane = lane;
                    Save();
                }
            }
        }

        public void ModifyAthleteCompetitionPresence(int regId, bool? presence)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(r => r.Id == regId);
            if (registration != null)
            {
                registration.Presence = presence;
                Save();
            }
        }

        public List<List<CompetitionClubRankedStanding>> CupClubRanks(int id, int seasonId)
        {
            var league = db.Leagues.Include("LeaguesPrices").FirstOrDefault(p => p.LeagueId == id);

            var possitionSettings = db.PositionSettings.Where(p => p.LeagueId == id && p.SeasonId == seasonId).ToList();
            List<CompetitionDisciplineDto> rankedMDisciplines = new List<CompetitionDisciplineDto>();
            List<CompetitionDisciplineDto> rankedFDisciplines = new List<CompetitionDisciplineDto>();
            List<CompetitionClubRankedStanding> menClubRanking = new List<CompetitionClubRankedStanding>();
            List<CompetitionClubRankedStanding> womenClubRanking = new List<CompetitionClubRankedStanding>();

            foreach (var competitionDiscipline in league.CompetitionDisciplines.Where(cd => cd.IsForScore))
            {
                List<decimal> positionValues = new List<decimal>();
                var discipline = GetById(competitionDiscipline.DisciplineId.Value);
                var validForScores = competitionDiscipline.CompetitionDisciplineRegistrations.Where(r => !r.IsArchive && r.CompetitionResult.FirstOrDefault()?.Heat == "1" && r.CompetitionResult.FirstOrDefault()?.AlternativeResult == 0);
                List<CompDiscRegRankDTO> orderedRegistrationsWithResults;
                var genderId = competitionDiscipline.CompetitionAge.gender;

                if (competitionDiscipline.IsResultsManualyRanked)
                {
                    orderedRegistrationsWithResults = validForScores.OrderBy(x => x.CompetitionResult.FirstOrDefault().Rank ?? int.MaxValue).Select(r => new CompDiscRegRankDTO
                    {
                        RegistrationId = r.Id,
                        ClubId = r.ClubId.Value,
                        ClubName = r.Club.Name,
                        isAlternative = r.CompetitionResult.FirstOrDefault()?.AlternativeResult > 0
                    }).ToList();
                }
                else if (discipline.Format != null && ((discipline.Format.Value >= 6 && discipline.Format.Value <= 8) || discipline.Format.Value == 10 || discipline.Format.Value == 11))
                {
                    orderedRegistrationsWithResults = validForScores.OrderByDescending(x => x.CompetitionResult.FirstOrDefault().SortValue ?? long.MinValue).Select(r => new CompDiscRegRankDTO
                    {
                        RegistrationId = r.Id,
                        ClubId = r.ClubId.Value,
                        ClubName = r.Club.Name,
                        SortedValue = r.CompetitionResult.FirstOrDefault()?.SortValue,
                        isAlternative = r.CompetitionResult.FirstOrDefault()?.AlternativeResult > 0
                    }).ToList();
                }
                else
                {
                    orderedRegistrationsWithResults = validForScores.OrderBy(x => x.CompetitionResult.FirstOrDefault().SortValue ?? long.MaxValue).Select(r => new CompDiscRegRankDTO
                    {
                        RegistrationId = r.Id,
                        ClubId = r.ClubId.Value,
                        ClubName = r.Club.Name,
                        SortedValue = r.CompetitionResult.FirstOrDefault()?.SortValue,
                        isAlternative = r.CompetitionResult.FirstOrDefault()?.AlternativeResult > 0
                    }).ToList();
                }

                for (int i = 0; i < orderedRegistrationsWithResults.Count; i++)
                {
                    int upToIndex = i;
                    positionValues.Add(possitionSettings.FirstOrDefault(p => p.Position == i + 1)?.Points ?? 0);

                    if (!competitionDiscipline.IsResultsManualyRanked)
                    {
                        for (int j = i; j >= 0; j--)
                        {
                            if (orderedRegistrationsWithResults.ElementAt(j).SortedValue == orderedRegistrationsWithResults.ElementAt(i).SortedValue)
                            {
                                upToIndex = j;
                            }
                            else
                            {
                                break;
                            }
                        }
                        decimal totalvalue = 0;
                        for (int j = upToIndex; j <= i; j++)
                        {
                            var posVal = possitionSettings.FirstOrDefault(p => p.Position == j+1)?.Points ?? 0;
                            totalvalue = totalvalue + posVal;
                        }
                        decimal media = 0;
                        if (totalvalue != 0)
                        {
                            media = totalvalue / (i - upToIndex + 1);
                            for (int j = upToIndex; j <= i; j++)
                            {
                                positionValues[j] = media;
                            }
                        }
                    }
                }

                for (int i = 0; i < orderedRegistrationsWithResults.Count; i++)
                {
                    var posVal = positionValues[i];
                    if (orderedRegistrationsWithResults.ElementAt(i).SortedValue.HasValue && !orderedRegistrationsWithResults.ElementAt(i).isAlternative)
                    {
                        orderedRegistrationsWithResults.ElementAt(i).Score = posVal;
                        if (genderId == 1) // male
                        {
                                var tempRank = menClubRanking.Find(c => c.ClubName == orderedRegistrationsWithResults.ElementAt(i).ClubName);
                                if (tempRank == null)
                                {
                                menClubRanking.Add(new CompetitionClubRankedStanding
                                    {
                                    ClubName = orderedRegistrationsWithResults.ElementAt(i).ClubName,
                                    Points = orderedRegistrationsWithResults.ElementAt(i).Score.Value,
                                    ClubId = orderedRegistrationsWithResults.ElementAt(i).ClubId.Value,
                                    Correction = GetCorrectionForClubCompetition(orderedRegistrationsWithResults.ElementAt(i).ClubId.Value, id, seasonId, genderId.Value)
                                    });
                                }
                                else
                                {
                                    tempRank.Points += orderedRegistrationsWithResults.ElementAt(i).Score.Value;
                                }
                                validForScores.FirstOrDefault(r => r.Id == orderedRegistrationsWithResults.ElementAt(i).RegistrationId).CompetitionResult.FirstOrDefault().ClubPoints = posVal;
                        }
                        if (genderId == 0) // female
                        {
                                var tempRank = womenClubRanking.Find(c => c.ClubName == orderedRegistrationsWithResults.ElementAt(i).ClubName);
                                if (tempRank == null)
                                {
                                    womenClubRanking.Add(new CompetitionClubRankedStanding
                                    {
                                        ClubName = orderedRegistrationsWithResults.ElementAt(i).ClubName,
                                        ClubId = orderedRegistrationsWithResults.ElementAt(i).ClubId.Value,
                                        Points = orderedRegistrationsWithResults.ElementAt(i).Score.Value,
                                        Correction = GetCorrectionForClubCompetition(orderedRegistrationsWithResults.ElementAt(i).ClubId.Value, id, seasonId, genderId.Value)
                                    });
                                }
                                else
                                {
                                    tempRank.Points += orderedRegistrationsWithResults.ElementAt(i).Score.Value;
                                }
                                validForScores.FirstOrDefault(r => r.Id == orderedRegistrationsWithResults.ElementAt(i).RegistrationId).CompetitionResult.FirstOrDefault().ClubPoints = posVal;
                         }
                    }
                }

                if (orderedRegistrationsWithResults.Count() > 0)
                {
                    var competitionDisciplineDto = new CompetitionDisciplineDto
                    {
                        Id = competitionDiscipline.Id,
                        DisciplineName = discipline.Name,
                        ClubsPointed = orderedRegistrationsWithResults
                    };
                    if (competitionDiscipline.CompetitionAge.gender == 1)
                    {
                        rankedMDisciplines.Add(competitionDisciplineDto);
                    }
                    if (competitionDiscipline.CompetitionAge.gender == 0)
                    {
                        rankedFDisciplines.Add(competitionDisciplineDto);
                    }
                }
            }
            Save();

            List<List<CompetitionClubRankedStanding>> res = new List<List<CompetitionClubRankedStanding>>
            {
                menClubRanking.OrderByDescending(r => r.Points).ToList(),
                womenClubRanking.OrderByDescending(r => r.Points).ToList()
            };

            return res;
        }
        public void SetManualPointCalculation(int competitionDisciplineId, bool isChecked)
        {
            var competitionDiscipline = db.CompetitionDisciplines.FirstOrDefault(c => c.Id == competitionDisciplineId);
            if(competitionDiscipline != null)
            {
                var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == competitionDiscipline.DisciplineId);
                competitionDiscipline.IsManualPointCalculation = isChecked;
                if (isChecked)
                {
                    foreach (var reg in competitionDiscipline.CompetitionDisciplineRegistrations)
                    {
                        var result = reg.CompetitionResult.FirstOrDefault();
                        if(result != null && result.AlternativeResult == 0)
                        {
                            double points;
                            var success = double.TryParse(result.Result, out points);
                            if (success)
                            {
                                var score = IAAFScoringPointsService.getPoints(1000 * points, IAAFScoringPointsService.DefaultEdition, IAAFScoringPointsService.OutdoorsVenue, competitionDiscipline.CompetitionAge.gender.Value, discipline.DisciplineType);
                                result.AlternativeResult = 0;
                                result.CombinedPoint = score;
                                result.ClubPoints = score;
                            }
                            else
                            {
                                result.CombinedPoint = 0;
                                result.ClubPoints = 0;
                            }
                        }
                    }
                }
                Save();
            }
        }
        public void ModifyCompetitionDisciplineHeatStartTime(int competitionDisciplineId, string heat, DateTime? startTime)
        {
            var row = db.CompetitionDisciplineHeatStartTimes.FirstOrDefault(h => h.CompetitionDisciplineId == competitionDisciplineId && h.HeatName == heat);
            if (row != null)
            {
                row.StartTime = startTime;
            }
            else
            {
                db.CompetitionDisciplineHeatStartTimes.Add(new CompetitionDisciplineHeatStartTime {
                    CompetitionDisciplineId = competitionDisciplineId,
                    HeatName = heat,
                    StartTime = startTime
                });
            }
            Save();
        }
        public void UpdateCompetitionDisciplineCustomFields6(int id, List<string> listedFields)
        {
            var competitionDiscipline = GetCompetitionDisciplineById(id);
            if(competitionDiscipline != null)
            {
                competitionDiscipline.SetFormat6CustomFields(listedFields);
                Save();
                var regs = competitionDiscipline.CompetitionDisciplineRegistrations;
                foreach (var reg in regs)
                {
                    UpdateCompetitionRegistrationLiveResult6(reg);
                }
                Save();
            }
        }


        private void UpdateCompetitionRegistrationLiveResult6(CompetitionDisciplineRegistration reg)
        {
            if (reg != null)
            {
                var result = reg.CompetitionResult.FirstOrDefault();
                if (result != null)
                {
                    var possibleResults = reg.CompetitionDiscipline.GetFormat6CustomFields();
                    var successIndex = result.GetSuccessIndex6(possibleResults.Count());
                    if (successIndex > -2)
                    {
                        if (successIndex > -1)
                        {
                            var resultColumn = successIndex / 3;
                            int attemppSuccessWithinColumn = successIndex % 3;
                            var resultStr = possibleResults[resultColumn];
                            result.Result = resultStr;
                            result.SortValue = Convert.ToInt64(Convert.ToDouble(resultStr) * 1000);
                            //column name id not correct but its not used in format 6 so I use it for ordering format 6 results instead of adding more columns.
                            result.LiveSplitOrder = Convert.ToInt32(Convert.ToDouble(resultStr) * 1000) * 100000 - attemppSuccessWithinColumn * 10000 - result.GetNumberFailsForCustomFields6(possibleResults.Count());
                        }
                        else
                        {
                            //need to know what to do when he fails attempts other than not participating at all.
                            result.Result = null;
                            result.SortValue = null;
                            result.LiveSplitOrder = null;
                        }
                    }
                    else
                    {
                        result.Result = null;
                        result.SortValue = null;
                        result.LiveSplitOrder = null;
                    }
                    reg.CompetitionDiscipline.LastResultUpdate = DateTime.Now;
                }
            }
        }

        public void UpdateCompetitionDisciplineTeamRegistration(int id, int clubId, int teamRegistration)
        {
            var competitionDiscipline = GetCompetitionDisciplineById(id);
            if (competitionDiscipline != null)
            {
                var clubReg = competitionDiscipline.CompetitionDisciplineClubsRegistrations.FirstOrDefault(x => x.ClubId == clubId);

                if (clubReg != null)
                {
                    clubReg.TeamRegistrations = teamRegistration;
                    
                }
                else
                {
                    competitionDiscipline.CompetitionDisciplineClubsRegistrations.Add(new CompetitionDisciplineClubsRegistration()
                    {
                        CompetitionDisciplineId = id,
                        ClubId = clubId,
                        TeamRegistrations = teamRegistration
                    });
                }
                Save();
            }
        }

        public CompetitionDisciplineTeam AddTeamForCompetitionDiscipline(int id, int clubId, bool isClubManager = false)
        {
            var competitionDiscipline = GetCompetitionDisciplineById(id);
            if (competitionDiscipline != null)
            {
                var teamsNum = competitionDiscipline.CompetitionDisciplineTeams.Count(x => x.ClubId == clubId); 
                //if(isClubManager)
                //{
                int teamReg = 0;
                var clubReg = competitionDiscipline.CompetitionDisciplineClubsRegistrations.FirstOrDefault(x => x.ClubId == clubId);
                if (clubReg != null) teamReg = clubReg.TeamRegistrations.Value;

                if (teamReg <= teamsNum) {
                    throw new MaximumRequiredException() { NumOfRegistrationsLeft = 0 };
                }
                //}
                var newTeam = new CompetitionDisciplineTeam()
                {
                    ClubId = clubId,
                    CompetitionDisciplineId = id,
                    TeamNumber = ++teamsNum
                };

                competitionDiscipline.CompetitionDisciplineTeams.Add(newTeam);
                Save();

                return newTeam;
            }

            return null;
        }

        private int CalculateAge(DateTime? dateOfBirth)
        {
            if (dateOfBirth == null) return -1;
            int age = 0;
            age = DateTime.Now.Year - dateOfBirth.Value.Year;
            if (DateTime.Now.DayOfYear < dateOfBirth.Value.DayOfYear)
                age = age - 1;

            return age;
            
        }
        public int? GetCompetitonDisciplineRecord(int disciplineId, int categoryId)
        {
            var records = db.DisciplineRecords.ToList();
            foreach (var record in records)
            {
                if(record.isCategorySelected(categoryId) && record.isDisciplineSelected(disciplineId))
                {
                    return record.Id;
                }
            }
            return null;
        }
        public DisciplineRecord GetDisciplineRecordById(int id) {
            return db.DisciplineRecords.FirstOrDefault(r => r.Id == id);
        }

        public void CreateCompetitionExpertise(CompetitionExperty competitionExperty)
        {
            db.CompetitionExperties.Add(competitionExperty);
            db.SaveChanges();
        }

        public CompetitionExperty GetCompetitionExpertyById(int id)
        {
            return db.CompetitionExperties.FirstOrDefault(x => x.Id == id);
        }

        public void DeleteCompetitionExperty(int id)
        {
            var compExperty = db.CompetitionExperties.FirstOrDefault(d => d.Id == id);
            db.CompetitionExperties.Remove(compExperty);
            Save();
        }

        public List<CompetitionExperty> GetCompetitionExperties(int leagueId)
        {
            return db.CompetitionExperties.Where(x => x.CompetitionId == leagueId).Include(x => x.CompetitionExpertiesHeats).ToList();
        }

        public bool CheckCompetitionExpertiesHeatByExpId(int expId, int compHeatId)
        {
            return db.CompetitionExpertiesHeats.FirstOrDefault(x => x.CompetitionExpertiesId == expId && x.BicycleCompetitionHeatId == compHeatId) != null;
        }

        public CompetitionExpertiesHeat GetCompetitionExpertiesHeatByExpId(int? compExpId)
        {
            return db.CompetitionExpertiesHeats.FirstOrDefault(x => x.Id == compExpId);
        }

        public int CreateCompetitionExpertiseHeat(int expId, int compHeatId, int[] unionHeatIds, int? compLevelId, int[] agesIds)
        {

            var ceh = new CompetitionExpertiesHeat
            {
                CompetitionExpertiesId = expId,
                BicycleCompetitionHeatId = compHeatId,
                CompetitionLevelId = compLevelId
            };

            db.CompetitionExpertiesHeats.Add(ceh);
            db.SaveChanges();

            foreach (var heatId in unionHeatIds)
            {
                db.CompetitionExpertiesDisciplineHeats.Add(new CompetitionExpertiesDisciplineHeat
                {
                    CompetitionExpertiesHeatId = ceh.Id,
                    HeatDisciplineId = heatId
                });            
            }
            db.SaveChanges();

            foreach (var ageId in agesIds)
            {
                db.CompetitionExpertiesHeatsAges.Add(new CompetitionExpertiesHeatsAge
                {
                    CompetitionExpertiesHeatId = ceh.Id,
                    CompetitionAgeId = ageId
                });
            }
            db.SaveChanges();

            return ceh.Id;
        }

        public void RemoveCompetitionExpertiseHeat(int expId, int compHeatId, int[] unionHeatIds)
        {
            var list = db.CompetitionExpertiesHeats.Where(x => x.CompetitionExpertiesId == expId && x.BicycleCompetitionHeatId == compHeatId);
            db.CompetitionExpertiesHeats.RemoveRange(list);
            db.SaveChanges();
        }

        public void RemoveCompetitionExpertiseDisciplineHeat(int expId, int compHeatId, int[] unionHeatIds)
        {
            var list = db.CompetitionExpertiesDisciplineHeats.Where(x => x.CompetitionExpertiesHeat.CompetitionExpertiesId == expId && x.CompetitionExpertiesHeat.BicycleCompetitionHeatId == compHeatId);
            db.CompetitionExpertiesDisciplineHeats.RemoveRange(list);
            db.SaveChanges();
        }

        public void AddCompetitionExpertiseDisciplineHeat(int expId, int compHeatId, int[] unionHeatIds)
        {
            var id = db.CompetitionExpertiesHeats.FirstOrDefault(x => x.CompetitionExpertiesId == expId && x.BicycleCompetitionHeatId == compHeatId).Id;

            foreach (var heatId in unionHeatIds)
            {
                db.CompetitionExpertiesDisciplineHeats.Add(new CompetitionExpertiesDisciplineHeat
                {
                    CompetitionExpertiesHeatId = id,
                    HeatDisciplineId = heatId
                });
            }
            db.SaveChanges();
        }

        public void UpdateCompetitionExpertiseHeatLevel(int expId, int compHeatId, int? levelId)
        {
            var compExe = db.CompetitionExpertiesHeats.FirstOrDefault(x => x.CompetitionExpertiesId == expId && x.BicycleCompetitionHeatId == compHeatId);

            compExe.CompetitionLevelId = levelId;

            db.SaveChanges();
        }

        public void RemoveCompetitionExpertiseHeatAges(int expId, int compHeatId, int[] agesIds)
        {
            var list = db.CompetitionExpertiesHeatsAges.Where(x => x.CompetitionExpertiesHeat.CompetitionExpertiesId == expId && x.CompetitionExpertiesHeat.BicycleCompetitionHeatId == compHeatId);
            db.CompetitionExpertiesHeatsAges.RemoveRange(list);
            db.SaveChanges();
        }

        public void AddCompetitionExpertiseHeatAges(int expId, int compHeatId, int[] agesIds)
        {
            var id = db.CompetitionExpertiesHeats.FirstOrDefault(x => x.CompetitionExpertiesId == expId && x.BicycleCompetitionHeatId == compHeatId).Id;

            foreach (var ageId in agesIds)
            {
                db.CompetitionExpertiesHeatsAges.Add(new CompetitionExpertiesHeatsAge
                {
                    CompetitionExpertiesHeatId = id,
                    CompetitionAgeId = ageId
                });
            }
            db.SaveChanges();
        }
    }
}
