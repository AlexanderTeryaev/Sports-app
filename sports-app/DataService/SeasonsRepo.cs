using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AppModel;
using System;

namespace DataService
{
    public class SeasonsRepo : BaseRepo
    {
        public SeasonsRepo() : base() { }

        public SeasonsRepo(DataEntities db) : base(db) { }

        public IEnumerable<Season> GetSeasons()
        {
            return db.Seasons.Where(x => x.IsActive);
        }

        public Season GetById(int Id)
        {
            return db.Seasons.FirstOrDefault(s => s.Id == Id && s.IsActive);
        }

        public IEnumerable<Season> GetAllCurrent(DateTime date)
        {
            return db.Seasons.Where(s => s.StartDate <= date && s.EndDate >= date && s.IsActive);
        }

        public int? GetCurrentByUnionId(int unionId)
        {
            var seasonId = db.Seasons.Where(s => s.StartDate <= DateTime.Now && s.EndDate >= DateTime.Now &&
                                                 s.UnionId == unionId &&
                                                 s.IsActive)
                .OrderByDescending(c => c.Id)
                .FirstOrDefault()
                ?.Id;

            if (seasonId == null)
                return db.Seasons.Where(s => s.IsActive &&
                                             s.StartDate.Year <= DateTime.Now.Year &&
                                             s.EndDate.Year >= DateTime.Now.Year && s.UnionId == unionId
                                             && s.StartDate.Year == DateTime.Now.Year - 1 &&
                                             s.EndDate.Year == DateTime.Now.Year)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefault()
                    ?.Id;
            return seasonId;
        }
        public bool isNowAllowSeasonId(int seasonId)
        {
            if(seasonId == 0|| seasonId == null)
            {
                return false;
            }
            var season = db.Seasons.Where(s => s.Id == seasonId);
            var temp = season.First();
            if (season.First().StartDate<=DateTime.Now && season.First().EndDate>= DateTime.Now)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public IEnumerable<Season> GetAllCurrent()
        {
            return GetAllCurrent(DateTime.Now);
        }

        public IEnumerable<Season> GetSeasonsByUnion(int unionId, bool includeInactive)
        {
            var seasons = db.Seasons.Where(x => x.UnionId == unionId);
            if (!includeInactive)
            {
                seasons = seasons.Where(x => x.IsActive);
            }

            return seasons.ToList();
        }

        public IEnumerable<Season> GetSeasonsByClub(int clubId)
        {
            return db.Seasons.Where(x => x.ClubId == clubId && x.IsActive).ToList();
        }

        public void Create(Season model)
        {
            db.Seasons.Add(model);
        }

        public void SetMinimumParticipationRequired(int requirement, int seasonId)
        {
            var season = db.Seasons.FirstOrDefault(p => p.Id == seasonId);
            if (season != null)
            {
                season.MinimumParticipationRequired = requirement;
            }
            Save();
        }

        public void DuplicateClubEntities(int clubId, int oldSeasonId, int newSeasonId)
        {
            var clubTeams = db.ClubTeams
                .AsNoTracking()
                .Include(x => x.Team.TeamsPlayers)
                .Include(x => x.Team.UsersJobs)
                .Include(x => x.Team.TeamsDetails)
                .Where(x => x.ClubId == clubId && x.SeasonId == oldSeasonId)
                .ToList();

            var schools = db.Schools
                .AsNoTracking()
                .Include(x => x.SchoolTeams)
                .Include(x => x.SchoolTeams.Select(st => st.Team.TeamsPlayers))
                .Where(x => x.ClubId == clubId && x.SeasonId == oldSeasonId)
                .ToList();

            var clubOfficials = db.UsersJobs
                .AsNoTracking()
                .Where(x => x.ClubId == clubId && x.SeasonId == oldSeasonId)
                .ToList();

            var clubAuditoriums = db.Auditoriums
                .AsNoTracking()
                .Where(x => x.ClubId == clubId && x.SeasonId == oldSeasonId)
                .ToList();

            var copiedTeams = new List<ClubTeam>();
            var copiedTeamPlayers = new List<TeamsPlayer>();
            var copiedTeamDetails = new List<TeamsDetails>();

            var copiedSchools = new List<School>();
            var copiedSchoolTeamPlayers = new List<TeamsPlayer>();

            var copiedOfficials = new List<UsersJob>();
            var copiedAuditoriums = new List<Auditorium>();

            foreach (var clubTeam in clubTeams)
            {
                var newClubTeam = new ClubTeam
                {
                    ClubId = clubId,
                    TeamId = clubTeam.TeamId,
                    SeasonId = newSeasonId,
                    IsBlocked = clubTeam.IsBlocked,
                    TeamPosition = clubTeam.TeamPosition,
                    DepartmentId = clubTeam.DepartmentId,
                    IsTrainingTeam = clubTeam.IsTrainingTeam
                };

                copiedTeams.Add(newClubTeam);

                foreach (var teamDetail in clubTeam.Team.TeamsDetails.Where(x => x.SeasonId == oldSeasonId))
                {
                    copiedTeamDetails.Add(new TeamsDetails
                    {
                        SeasonId = newSeasonId,
                        RegistrationId = teamDetail.RegistrationId,
                        TeamId = teamDetail.TeamId,
                        TeamName = teamDetail.TeamName
                    });
                }

                foreach (var teamPlayer in clubTeam.Team.TeamsPlayers.Where(x => x.SeasonId == oldSeasonId && x.ClubId == clubId))
                {
                    copiedTeamPlayers.Add(new TeamsPlayer
                    {
                        TeamId = teamPlayer.TeamId,
                        ClubId = clubId,
                        SeasonId = newSeasonId,
                        LeagueId = teamPlayer.LeagueId,
                        ApprovalDate = teamPlayer.ApprovalDate,
                        ClubComment = teamPlayer.ClubComment,
                        //Comment = teamPlayer.Comment,
                        DateOfCreate = teamPlayer.DateOfCreate,
                        HandicapLevel = teamPlayer.HandicapLevel,
                        IsActive = teamPlayer.IsActive,
                        IsApprovedByManager = teamPlayer.IsApprovedByManager,
                        IsEscortPlayer = teamPlayer.IsEscortPlayer,
                        IsExceptionalMoved = teamPlayer.IsExceptionalMoved,
                        IsLocked = teamPlayer.IsLocked,
                        IsTrainerPlayer = teamPlayer.IsTrainerPlayer,
                        //MedExamDate = teamPlayer.MedExamDate,
                        //Paid = teamPlayer.Paid,
                        PosId = teamPlayer.PosId,
                        ShirtNum = teamPlayer.ShirtNum,
                        StartPlaying = teamPlayer.StartPlaying,
                        TennisPositionOrder = teamPlayer.TennisPositionOrder,
                        UnionComment = teamPlayer.UnionComment,
                        UserId = teamPlayer.UserId,
                        WithoutLeagueRegistration = teamPlayer.WithoutLeagueRegistration
                    });
                }

                foreach (var usersJob in clubTeam.Team.UsersJobs.Where(x => x.SeasonId == oldSeasonId))
                {
                    copiedOfficials.Add(new UsersJob
                    {
                        UserId = usersJob.UserId,
                        Active = usersJob.Active,
                        ClubId = usersJob.ClubId,
                        ConnectedClubId = usersJob.ConnectedClubId,
                        ConnectedDisciplineIds = usersJob.ConnectedDisciplineIds,
                        DisciplineId = usersJob.DisciplineId,
                        FormatPermissions = usersJob.FormatPermissions,
                        IsBlocked = usersJob.IsBlocked,
                        IsCompetitionRegistrationBlocked = usersJob.IsCompetitionRegistrationBlocked,
                        JobId = usersJob.JobId,
                        LeagueId = usersJob.LeagueId,
                        RateType = usersJob.RateType,
                        RegionalId = usersJob.RegionalId,
                        SeasonId = newSeasonId,
                        TeamId = usersJob.TeamId,
                        UnionId = usersJob.UnionId,
                        WithhodlingTax = usersJob.WithhodlingTax
                    });
                }
            }

            foreach (var school in schools)
            {
                var newSchool = new School
                {
                    ClubId = clubId,
                    SeasonId = newSeasonId,
                    CreatedBy = school.CreatedBy,
                    DateCreated = DateTime.Now,
                    IsCamp = school.IsCamp,
                    Name = school.Name
                };

                foreach (var schoolTeam in school.SchoolTeams)
                {
                    newSchool.SchoolTeams.Add(new SchoolTeam
                    {
                        TeamId = schoolTeam.TeamId
                    });

                    foreach (var teamPlayer in schoolTeam.Team.TeamsPlayers.Where(x => x.SeasonId == oldSeasonId && x.ClubId == clubId))
                    {
                        copiedSchoolTeamPlayers.Add(new TeamsPlayer
                        {
                            TeamId = teamPlayer.TeamId,
                            ClubId = clubId,
                            SeasonId = newSeasonId,
                            LeagueId = teamPlayer.LeagueId,
                            ApprovalDate = teamPlayer.ApprovalDate,
                            ClubComment = teamPlayer.ClubComment,
                            //Comment = teamPlayer.Comment,
                            DateOfCreate = teamPlayer.DateOfCreate,
                            HandicapLevel = teamPlayer.HandicapLevel,
                            IsActive = teamPlayer.IsActive,
                            IsApprovedByManager = teamPlayer.IsApprovedByManager,
                            IsEscortPlayer = teamPlayer.IsEscortPlayer,
                            IsExceptionalMoved = teamPlayer.IsExceptionalMoved,
                            IsLocked = teamPlayer.IsLocked,
                            IsTrainerPlayer = teamPlayer.IsTrainerPlayer,
                            //MedExamDate = teamPlayer.MedExamDate,
                            //Paid = teamPlayer.Paid,
                            PosId = teamPlayer.PosId,
                            ShirtNum = teamPlayer.ShirtNum,
                            StartPlaying = teamPlayer.StartPlaying,
                            TennisPositionOrder = teamPlayer.TennisPositionOrder,
                            UnionComment = teamPlayer.UnionComment,
                            UserId = teamPlayer.UserId,
                            WithoutLeagueRegistration = teamPlayer.WithoutLeagueRegistration
                        });
                    }
                }

                copiedSchools.Add(newSchool);
            }

            foreach (var clubOfficial in clubOfficials)
            {
                copiedOfficials.Add(new UsersJob
                {
                    UserId = clubOfficial.UserId,
                    Active = clubOfficial.Active,
                    ClubId = clubOfficial.ClubId,
                    ConnectedClubId = clubOfficial.ConnectedClubId,
                    ConnectedDisciplineIds = clubOfficial.ConnectedDisciplineIds,
                    DisciplineId = clubOfficial.DisciplineId,
                    FormatPermissions = clubOfficial.FormatPermissions,
                    IsBlocked = clubOfficial.IsBlocked,
                    IsCompetitionRegistrationBlocked = clubOfficial.IsCompetitionRegistrationBlocked,
                    JobId = clubOfficial.JobId,
                    LeagueId = clubOfficial.LeagueId,
                    RateType = clubOfficial.RateType,
                    RegionalId = clubOfficial.RegionalId,
                    SeasonId = newSeasonId,
                    TeamId = clubOfficial.TeamId,
                    UnionId = clubOfficial.UnionId,
                    WithhodlingTax = clubOfficial.WithhodlingTax
                });
            }

            foreach (var clubAuditorium in clubAuditoriums)
            {
                copiedAuditoriums.Add(new Auditorium
                {
                    ClubId = clubId,
                    UnionId = clubAuditorium.UnionId,
                    SeasonId = newSeasonId,
                    Name = clubAuditorium.Name,
                    Address = clubAuditorium.Address,
                    IsArchive = clubAuditorium.IsArchive,
                    LanesNumber = clubAuditorium.LanesNumber,
                    Length = clubAuditorium.Length
                });
            }

            db.ClubTeams.AddRange(copiedTeams);
            db.TeamsPlayers.AddRange(copiedTeamPlayers);
            db.TeamsDetails.AddRange(copiedTeamDetails);

            db.Schools.AddRange(copiedSchools);
            db.TeamsPlayers.AddRange(copiedSchoolTeamPlayers);

            db.UsersJobs.AddRange(copiedOfficials);

            db.Auditoriums.AddRange(copiedAuditoriums);

            db.SaveChanges();
        }

        public void Duplicate(int unionId, int[] leagueIds, int lastSeasonId, int newSeasonId)
        {
            var dbLazyLoad = db.Configuration.LazyLoadingEnabled;
            var dbDetectChanges = db.Configuration.AutoDetectChangesEnabled;

            db.Configuration.LazyLoadingEnabled = false;
            db.Configuration.AutoDetectChangesEnabled = false;
            List<int> disciplineRouteIds = new List<int>();
            List<int> disciplineTeamRouteIds = new List<int>();
            var union = db.Unions
                .Include(x => x.DistanceTables)
                .Include(x => x.UnionOfficialSettings)
                .Include(x => x.UsersJobs)
                .Include(x => x.CompetitionAges)
                .Include(x => x.CompetitionLevels)
                .Include(x => x.CompetitionRegions)
                .Include(x => x.Section)
                .Include(x => x.UnionForms)
                .FirstOrDefault(x => x.UnionId == unionId);

            var isTennis = string.Equals(union?.Section?.Alias, SectionAliases.Tennis,
                StringComparison.CurrentCultureIgnoreCase);

            db.Clubs.Where(x => x.UnionId == unionId)
                .Include(x => x.UsersJobs)
                .Include(x => x.UsersJobs1)
                .Include(x => x.ClubTeams)
                .Include(x => x.ClubDisciplines)
                .Include(x => x.Auditoriums)
                .Load();

            db.ClubTeams
                .Join(db.Clubs.Where(x => x.UnionId == unionId), x => x.ClubId, x => x.ClubId, (clubTeam, club) => clubTeam)
                .Include(x => x.Team.TeamsPlayers)
                .Include(x => x.Team.TeamDisciplines)
                .Include(x => x.Team.TeamsDetails)
                .Include(x => x.Team.TeamsPlayers.Select(tp => tp.User.PlayerFiles))
                .Include(x => x.Team.TeamsPlayers.Select(tp => tp.User.PlayerDisciplines))
                .Load();

            var leagues = db.Leagues
                .AsNoTracking()
                .Include(t => t.LeagueOfficialsSettings)
                .Include(t => t.UsersJobs)
                .Include(t => t.TeamRegistrations)
                .Include(t => t.LeagueTeams)
                .Include(t => t.LeagueTeams.Select(lt => lt.Teams))
                .Include(t => t.LeagueTeams.Select(lt => lt.Teams.TeamRegistrations))
                .Include(t => t.LeagueTeams.Select(lt => lt.Teams.UsersJobs))
                .Include(t => t.LeagueTeams.Select(lt => lt.Teams.TeamsDetails))
                .Include(t => t.LeagueTeams.Select(lt => lt.Teams.TeamsPlayers))
                .Include(t => t.LeagueTeams.Select(lt => lt.Teams.TeamsPlayers.Select(tp => tp.User.PlayerFiles)))
                .Where(x => leagueIds.Contains(x.LeagueId))
                .ToList();

            var auditoriums = db.Auditoriums
                .AsNoTracking()
                .Where(x => x.SeasonId == lastSeasonId)
                .ToList();

            var athleteNumbers = db.AthleteNumbers.
                AsNoTracking().
                Where(x => x.SeasonId == lastSeasonId)
                .ToList();

            var seasonRecords = db.SeasonRecords
                .AsNoTracking()
                .Where(x => x.SeasonId == lastSeasonId)
                .ToList();


            db.KarateRefereesRanks.Load();


            var copiedLeagues = new List<League>();
            var leagueTeams = new List<LeagueTeams>();
            var copiedAuditoriums = new List<Auditorium>();
            var copiedClubs = new List<Club>();
            var copiedClubTeams = new List<ClubTeam>();
            var copiedUnionOfficialsSettings = new List<UnionOfficialSetting>();
            var copiedLeagueOfficialsSettings = new List<LeagueOfficialsSetting>();
            var copiedDistanceTables = new List<DistanceTable>();
            var copiedJobs = new List<UsersJob>();
            var copiedTeamDisciplines = new List<TeamDiscipline>();
            var copiedClubDisciplines = new List<ClubDiscipline>();
            var copiedPlayerDisciplines = new List<PlayerDiscipline>();
            var copiedPlayerFiles = new List<PlayerFile>();
            var copiedInstruments = new List<Instrument>();
            var copiedCompetitionAges = new List<CompetitionAge>();
            var copiedCompetitionLevels = new List<CompetitionLevel>();
            var copiedCompetitionRegions = new List<CompetitionRegion>();
            var copiedUnionForms = new List<UnionForm>();

            var copiedTeamsRegistrationsTemp = new List<TeamRegistration>();
            var copiedTeamsRegistrations = new List<TeamRegistration>();

            var teamPlayers = new List<TeamsPlayer>();
            var teamDetails = new List<TeamsDetails>();
            var copiedAthleteNumbers = new List<AthleteNumber>();
            var copiedSeasonRecords = new List<SeasonRecord>();
            var copiedKarateRefereesRank = new List<KarateRefereesRank>();

            var addedUserJobsIds = new List<int>();



            foreach (var league in leagues)
            {
                var newLeague = new League
                {
                    ClubId = league.ClubId,
                    SeasonId = newSeasonId,
                    AboutLeague = league.AboutLeague,
                    AgeId = league.AgeId,
                    AllowedCLubsIds = league.AllowedCLubsIds,
                    CreateDate = DateTime.Now,
                    Description = league.Description,
                    DisciplineId = league.DisciplineId,
                    EilatTournament = league.EilatTournament,
                    EndRegistrationDate = league.EndRegistrationDate,
                    FiveHandicapReduction = league.FiveHandicapReduction,
                    GenderId = league.GenderId,
                    Image = league.Image,
                    IsArchive = league.IsArchive,
                    IsPositionSettingsEnabled = league.IsPositionSettingsEnabled,
                    IsTeam = league.IsTeam,
                    LeagueCode = league.LeagueCode,
                    LeagueEndDate = league.LeagueEndDate,
                    LeagueStartDate = league.LeagueStartDate,
                    LeagueStructure = league.LeagueStructure,
                    Logo = league.Logo,
                    MaximumAge = league.MaximumAge,
                    MaximumHandicapScoreValue = league.MaximumHandicapScoreValue,
                    MaximumPlayersTeam = league.MaximumPlayersTeam,
                    MinParticipationReq = league.MinParticipationReq,
                    MinimumAge = league.MinimumAge,
                    MinimumPlayersTeam = league.MinimumPlayersTeam,
                    Name = league.Name,
                    Place = league.Place,
                    PlayerInsurancePrice = league.PlayerInsurancePrice,
                    PlayerRegistrationPrice = league.PlayerRegistrationPrice,
                    SortOrder = league.SortOrder,
                    TeamRegistrationPrice = league.TeamRegistrationPrice,
                    Terms = league.Terms,
                    Type = league.Type,
                    UnionId = league.UnionId
                };

                foreach (var leagueUserJob in league.UsersJobs.Where(x => x.SeasonId == lastSeasonId && x.TeamId == null))
                {
                    var newJob = new UsersJob
                    {
                        ClubId = leagueUserJob.ClubId,
                        TeamId = leagueUserJob.TeamId,
                        SeasonId = newSeasonId,
                        ConnectedClubId = leagueUserJob.ConnectedClubId,
                        ConnectedDisciplineIds = leagueUserJob.ConnectedDisciplineIds,
                        DisciplineId = leagueUserJob.DisciplineId,
                        IsBlocked = leagueUserJob.IsBlocked,
                        JobId = leagueUserJob.JobId,
                        League = newLeague,
                        RateType = leagueUserJob.RateType,
                        UnionId = leagueUserJob.UnionId,
                        UserId = leagueUserJob.UserId,
                        WithhodlingTax = leagueUserJob.WithhodlingTax,
                        Active = leagueUserJob.Active,
                        FormatPermissions = leagueUserJob.FormatPermissions
                    };
                    addedUserJobsIds.Add(leagueUserJob.Id);
                    copiedJobs.Add(newJob);
                    var karateRanks = new List<KarateRefereesRank>();
                    foreach (var karaterank in leagueUserJob.KarateRefereesRanks)
                    {
                        karateRanks.Add(new KarateRefereesRank { Type = karaterank.Type, Date = karaterank.Date, UsersJob = newJob });
                    }
                    copiedKarateRefereesRank.AddRange(karateRanks);
                }

                foreach (var leagueOfficialsSetting in league.LeagueOfficialsSettings)
                {
                    var newLeagueOfficialSetting = new LeagueOfficialsSetting
                    {
                        JobsRolesId = leagueOfficialsSetting.JobsRolesId,
                        PaymentPerGame = leagueOfficialsSetting.PaymentPerGame,
                        PaymentPerGameCurrency = leagueOfficialsSetting.PaymentPerGameCurrency,
                        PaymentTravel = leagueOfficialsSetting.PaymentTravel,
                        RateAForTravel = leagueOfficialsSetting.RateAForTravel,
                        RateAPerGame = leagueOfficialsSetting.RateAPerGame,
                        RateBForTravel = leagueOfficialsSetting.RateBForTravel,
                        RateBPerGame = leagueOfficialsSetting.RateBPerGame,
                        TravelMetricType = leagueOfficialsSetting.TravelMetricType,
                        TravelPaymentCurrencyType = leagueOfficialsSetting.TravelPaymentCurrencyType,
                        League = newLeague
                    };

                    copiedLeagueOfficialsSettings.Add(newLeagueOfficialSetting);
                }

                foreach (var teamRegistration in league.TeamRegistrations.Where(x => x.SeasonId == lastSeasonId))
                {
                    copiedTeamsRegistrationsTemp.Add(new TeamRegistration
                    {
                        TeamId = teamRegistration.TeamId,
                        ClubId = teamRegistration.ClubId, //We are saving old ClubId here to update it later at clubs duplication stage
                        League = newLeague,
                        SeasonId = newSeasonId
                    });
                }

                foreach (var leagueTeam in league.LeagueTeams.Where(x => !x.Teams.IsArchive))
                {
                    var newLeagueTeam = new LeagueTeams
                    {
                        TeamId = leagueTeam.TeamId,
                        SeasonId = newSeasonId,
                        Leagues = newLeague
                    };

                    foreach (var teamOfficial in leagueTeam.Teams.UsersJobs.Where(x => x.SeasonId == lastSeasonId && !addedUserJobsIds.Contains(x.Id)))
                    {
                        var newJob = new UsersJob
                        {
                            League = teamOfficial.LeagueId == league.LeagueId ? newLeague : null,
                            ClubId = teamOfficial.ClubId,
                            TeamId = leagueTeam.TeamId,
                            UnionId = teamOfficial.UnionId,
                            SeasonId = newSeasonId,
                            ConnectedClubId = teamOfficial.ConnectedClubId,
                            ConnectedDisciplineIds = teamOfficial.ConnectedDisciplineIds,
                            DisciplineId = teamOfficial.DisciplineId,
                            IsBlocked = teamOfficial.IsBlocked,
                            JobId = teamOfficial.JobId,
                            RateType = teamOfficial.RateType,
                            UserId = teamOfficial.UserId,
                            WithhodlingTax = teamOfficial.WithhodlingTax,
                            Active = teamOfficial.Active,
                            FormatPermissions = teamOfficial.FormatPermissions
                        };
                        addedUserJobsIds.Add(teamOfficial.Id);
                        copiedJobs.Add(newJob);

                        var karateRanks = new List<KarateRefereesRank>();
                        foreach (var karaterank in teamOfficial.KarateRefereesRanks)
                        {
                            karateRanks.Add(new KarateRefereesRank { Type = karaterank.Type, Date = karaterank.Date, UsersJob = newJob });
                        }
                        copiedKarateRefereesRank.AddRange(karateRanks);
                    }

                    var lastSeasonTeamDetails =
                        leagueTeam.Teams.TeamsDetails?.FirstOrDefault(x => x.SeasonId == lastSeasonId);
                    if (lastSeasonTeamDetails != null)
                    {
                        teamDetails.Add(new TeamsDetails
                        {
                            TeamId = leagueTeam.TeamId,
                            SeasonId = newSeasonId,
                            RegistrationId = lastSeasonTeamDetails.RegistrationId,
                            TeamName = lastSeasonTeamDetails.TeamName
                        });
                    }

                    foreach (var teamPlayer in leagueTeam.Teams.TeamsPlayers.Where(x => x.SeasonId == lastSeasonId && x.LeagueId == league.LeagueId))
                    {
                        if (!isTennis || league.EilatTournament != true) //do not copy players for tennis competitions
                        {
                            teamPlayers.Add(new TeamsPlayer
                            {
                                TeamId = teamPlayer.TeamId,
                                ClubId = teamPlayer.ClubId,
                                SeasonId = newSeasonId,
                                League = newLeague,
                                ApprovalDate = isTennis ? teamPlayer.ApprovalDate : null,
                                //ClubComment = teamPlayer.ClubComment,
                                //Comment = teamPlayer.Comment,
                                DateOfCreate = teamPlayer.DateOfCreate,
                                HandicapLevel = teamPlayer.HandicapLevel,
                                //IsActive = teamPlayer.IsActive,
                                //IsApprovedByManager = teamPlayer.IsApprovedByManager,
                                IsEscortPlayer = teamPlayer.IsEscortPlayer,
                                IsExceptionalMoved = teamPlayer.IsExceptionalMoved,
                                IsLocked = teamPlayer.IsLocked != null ? true : (bool?)null,
                                IsTrainerPlayer = teamPlayer.IsTrainerPlayer,
                                //MedExamDate = teamPlayer.MedExamDate,
                                Paid = teamPlayer.Paid,
                                PosId = teamPlayer.PosId,
                                ShirtNum = teamPlayer.ShirtNum,
                                StartPlaying = teamPlayer.StartPlaying,
                                TennisPositionOrder = teamPlayer.TennisPositionOrder,
                                //UnionComment = teamPlayer.UnionComment,
                                UserId = teamPlayer.UserId,
                                WithoutLeagueRegistration = teamPlayer.WithoutLeagueRegistration
                            });
                        }

                        foreach (var playerFile in teamPlayer.User.PlayerFiles.Where(x =>
                            x.SeasonId == lastSeasonId && x.FileType == (int)PlayerFileType.PlayerImage))
                        {
                            copiedPlayerFiles.Add(new PlayerFile
                            {
                                SeasonId = newSeasonId,
                                DateCreated = playerFile.DateCreated,
                                FileName = playerFile.FileName,
                                FileType = playerFile.FileType,
                                PlayerId = playerFile.PlayerId
                            });
                        }

                        if (isTennis)
                        {
                            foreach (var playerFile in teamPlayer.User.PlayerFiles.Where(x =>
                                x.SeasonId == lastSeasonId && x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive))
                            {
                                copiedPlayerFiles.Add(new PlayerFile
                                {
                                    SeasonId = newSeasonId,
                                    DateCreated = playerFile.DateCreated,
                                    FileName = playerFile.FileName,
                                    FileType = playerFile.FileType,
                                    PlayerId = playerFile.PlayerId
                                });
                            }
                        }
                    }

                    leagueTeams.Add(newLeagueTeam);
                }

                copiedLeagues.Add(newLeague);
            }

            foreach (var auditorium in auditoriums)
            {
                copiedAuditoriums.Add(new Auditorium
                {
                    ClubId = auditorium.ClubId,
                    SeasonId = newSeasonId,
                    Name = auditorium.Name,
                    Address = auditorium.Address,
                    AuditoriumId = auditorium.AuditoriumId,
                    IsArchive = auditorium.IsArchive,
                    UnionId = auditorium.UnionId
                });
            }

            if (union != null)
            {
                foreach (var unionForm in union.UnionForms.Where(x => x.SeasonId == lastSeasonId && !x.IsDeleted))
                {
                    var newForm = new UnionForm
                    {
                        SeasonId = newSeasonId,
                        FilePath = unionForm.FilePath,
                        Title = unionForm.Title,
                        UnionId = unionForm.UnionId
                    };

                    copiedUnionForms.Add(newForm);
                }

                foreach (var unionOfficial in union.UsersJobs.Where(x => x.SeasonId == lastSeasonId && x.LeagueId == null && x.TeamId == null && !addedUserJobsIds.Contains(x.Id)))
                {
                    var newJob = new UsersJob
                    {
                        LeagueId = unionOfficial.LeagueId,
                        ClubId = unionOfficial.ClubId,
                        TeamId = unionOfficial.TeamId,
                        UnionId = unionOfficial.UnionId,
                        SeasonId = newSeasonId,
                        ConnectedClubId = unionOfficial.ConnectedClubId,
                        ConnectedDisciplineIds = unionOfficial.ConnectedDisciplineIds,
                        DisciplineId = unionOfficial.DisciplineId,
                        IsBlocked = unionOfficial.IsBlocked,
                        JobId = unionOfficial.JobId,
                        RateType = unionOfficial.RateType,
                        UserId = unionOfficial.UserId,
                        WithhodlingTax = unionOfficial.WithhodlingTax,
                        Active = unionOfficial.Active,
                        FormatPermissions = unionOfficial.FormatPermissions
                    };
                    addedUserJobsIds.Add(unionOfficial.Id);
                    copiedJobs.Add(newJob);
                    var karateRanks = new List<KarateRefereesRank>();
                    foreach (var karaterank in unionOfficial.KarateRefereesRanks)
                    {
                        karateRanks.Add(new KarateRefereesRank { Type = karaterank.Type, Date = karaterank.Date, UsersJob = newJob });
                    }
                    copiedKarateRefereesRank.AddRange(karateRanks);
                }

                var unionDisciplines = union.Disciplines;
                if (string.Equals(union?.Section?.Alias, SectionAliases.Gymnastic,
                    StringComparison.CurrentCultureIgnoreCase) && (unionDisciplines == null || unionDisciplines.Count == 0))
                {
                    unionDisciplines = db.Disciplines.Where(x => x.UnionId == unionId)
                        .Include(x => x.Instruments)
                        .Include(x => x.DisciplineRoutes.Select(dr => dr.RouteRanks))
                        .Include(x => x.DisciplineTeamRoutes.Select(dtr => dtr.RouteTeamRanks))
                        .ToList();
                }
                foreach (var unionDiscipline in unionDisciplines)
                {
                    var instruments = unionDiscipline.Instruments.Where(x => x.SeasonId == lastSeasonId).ToList();
                    foreach (var instrument in instruments)
                    {
                        var newInstrument = new Instrument
                        {
                            SeasonId = newSeasonId,
                            Name = instrument.Name,
                            DisciplineId = instrument.DisciplineId
                        };

                        copiedInstruments.Add(newInstrument);
                    }

                    var disciplineRoutes = unionDiscipline.DisciplineRoutes.ToList();
                    foreach (var disciplineRoute in disciplineRoutes)
                    {
                        disciplineRouteIds.AddRange(disciplineRoute.RouteRanks.Select(x=>x.Id));
                    }

                    var disciplineTeamRoutes = unionDiscipline.DisciplineTeamRoutes.ToList();
                    foreach (var disciplineTeamRoute in disciplineTeamRoutes)
                    {
                        disciplineTeamRouteIds.AddRange(disciplineTeamRoute.RouteTeamRanks.Select(x => x.Id));
                    }
                }

                foreach (var officialSetting in union.UnionOfficialSettings.Where(x => x.SeasonId == lastSeasonId))
                {
                    var newUnionOfficialSetting = new UnionOfficialSetting
                    {
                        SeasonId = newSeasonId,
                        Union = union,
                        JobsRolesId = officialSetting.JobsRolesId,
                        PaymentPerGame = officialSetting.PaymentPerGame,
                        PaymentPerGameCurrency = officialSetting.PaymentPerGameCurrency,
                        PaymentTravel = officialSetting.PaymentTravel,
                        RateAForTravel = officialSetting.RateAForTravel,
                        RateAPerGame = officialSetting.RateAPerGame,
                        RateBForTravel = officialSetting.RateBForTravel,
                        RateBPerGame = officialSetting.RateBPerGame,
                        RateCForTravel = officialSetting.RateCForTravel,
                        RateCPerGame = officialSetting.RateCPerGame,
                        TravelMetricType = officialSetting.TravelMetricType,
                        TravelPaymentCurrencyType = officialSetting.TravelPaymentCurrencyType
                    };

                    copiedUnionOfficialsSettings.Add(newUnionOfficialSetting);
                }

                foreach (var distanceTable in union.DistanceTables.Where(x => x.SeasonId == lastSeasonId))
                {
                    var newDistanceTable = new DistanceTable
                    {
                        CityFromName = distanceTable.CityFromName,
                        CityToName = distanceTable.CityToName,
                        Distance = distanceTable.Distance,
                        DistanceType = distanceTable.DistanceType,
                        SeasonId = newSeasonId,
                        Union = union
                    };

                    copiedDistanceTables.Add(newDistanceTable);
                }

                foreach (var competitionAge in union.CompetitionAges.Where(x => x.SeasonId == lastSeasonId))
                {
                    var newCompetitionAge = new CompetitionAge
                    {
                        age_name = competitionAge.age_name,
                        from_birth = competitionAge.from_birth?.AddYears(1),
                        to_birth = competitionAge.to_birth?.AddYears(1),
                        UnionId = competitionAge.UnionId,
                        SeasonId = newSeasonId,
                        gender = competitionAge.gender,
                        from_weight = competitionAge.from_weight,
                        to_weight = competitionAge.to_weight
                    };

                    copiedCompetitionAges.Add(newCompetitionAge);
                }

                foreach (var competitionLevel in union.CompetitionLevels.Where(x => x.SeasonId == lastSeasonId))
                {
                    var newCompetitionLevel = new CompetitionLevel
                    {
                        level_name = competitionLevel.level_name,
                        UnionId = competitionLevel.UnionId,
                        SeasonId = newSeasonId
                    };

                    copiedCompetitionLevels.Add(newCompetitionLevel);
                }

                foreach (var competitionRegion in union.CompetitionRegions.Where(x => x.SeasonId == lastSeasonId))
                {
                    var newCompetitionRegion = new CompetitionRegion
                    {
                        region_name = competitionRegion.region_name,
                        UnionId = competitionRegion.UnionId,
                        SeasonId = newSeasonId
                    };

                    copiedCompetitionRegions.Add(newCompetitionRegion);
                }

                var isGymnastics = string.Equals(union?.Section?.Alias, SectionAliases.Gymnastic,
                    StringComparison.CurrentCultureIgnoreCase);
                foreach (var unionClub in union.Clubs.Where(x => x.SeasonId == lastSeasonId).ToList())
                {
                    var newClub = new Club
                    {
                        Address = unionClub.Address,
                        AuthorizedSignatories = !isGymnastics? unionClub.AuthorizedSignatories : string.Empty,
                        CertificateOfIncorporation = unionClub.CertificateOfIncorporation,
                        ClubInsurance = unionClub.ClubInsurance,
                        ClubNumber = unionClub.ClubNumber,
                        ContactPhone = unionClub.ContactPhone,
                        CreateDate = unionClub.CreateDate,
                        DateOfClubApproval = unionClub.DateOfClubApproval,
                        Description = unionClub.Description,
                        DistanceSettings = unionClub.DistanceSettings,
                        Email = unionClub.Email,
                        IndexAbout = unionClub.IndexAbout,
                        IndexImage = unionClub.IndexImage,
                        IsArchive = unionClub.IsArchive,
                        IsAuthorizedSignatoriesApproved = !isGymnastics? unionClub.IsAuthorizedSignatoriesApproved : false,
                        IsCertificateApproved = unionClub.IsCertificateApproved,
                        IsFlowerOfSport = unionClub.IsFlowerOfSport,

                        //ApprovalOfInsuranceCover = unionClub.ApprovalOfInsuranceCover,
                        //IsInsuranceCoverApproved = unionClub.IsInsuranceCoverApproved,

                        IsReportsEnabled = unionClub.IsReportsEnabled,
                        IsSectionClub = unionClub.IsSectionClub,
                        IsTrainingEnabled = unionClub.IsTrainingEnabled,
                        IsUnionClub = unionClub.IsUnionClub,
                        Logo = unionClub.Logo,
                        MedicalSertificateFile = unionClub.MedicalSertificateFile,
                        NGO_Number = unionClub.NGO_Number,
                        Name = unionClub.Name,
                        NumberOfCourts = unionClub.NumberOfCourts,
                        ParentClubId = unionClub.ParentClubId,
                        PrimaryImage = unionClub.PrimaryImage,
                        ReportSettings = unionClub.ReportSettings,
                        SeasonId = newSeasonId,
                        SectionId = unionClub.SectionId,
                        SportCenterId = unionClub.SportCenterId,
                        SportSectionId = unionClub.SportSectionId,
                        SportType = unionClub.SportType,
                        //StatementApproved = unionClub.StatementApproved,
                        TermsCondition = unionClub.TermsCondition,
                        UnionId = unionClub.UnionId
                    };

                    //Update club of teams registrations to a new club
                    foreach (var teamRegistration in copiedTeamsRegistrationsTemp.Where(x => x.ClubId == unionClub.ClubId))
                    {
                        teamRegistration.ClubId = 0;
                        teamRegistration.Club = newClub;

                        copiedTeamsRegistrations.Add(teamRegistration);
                    }

                    foreach (var clubAuditorium in unionClub.Auditoriums)
                    {
                        var newClubAuditorium = new Auditorium
                        {
                            Name = clubAuditorium.Name,
                            IsArchive = clubAuditorium.IsArchive,
                            Address = clubAuditorium.Address,
                            Club = newClub
                        };

                        copiedAuditoriums.Add(newClubAuditorium);
                    }

                    foreach (var clubDiscipline in unionClub.ClubDisciplines.Where(x => x.SeasonId == lastSeasonId))
                    {
                        var newClubDiscipline = new ClubDiscipline
                        {
                            SeasonId = newSeasonId,
                            Club = newClub,
                            DisciplineId = clubDiscipline.DisciplineId
                        };

                        copiedClubDisciplines.Add(newClubDiscipline);
                    }

                    foreach (var userJob in unionClub.UsersJobs.Where(x => x.SeasonId == lastSeasonId && !addedUserJobsIds.Contains(x.Id)))
                    {
                        var newUserJob = new UsersJob
                        {
                            Club = newClub,
                            IsBlocked = userJob.IsBlocked,
                            JobId = userJob.JobId,
                            RateType = userJob.RateType,
                            SeasonId = newSeasonId,
                            UserId = userJob.UserId,
                            WithhodlingTax = userJob.WithhodlingTax,
                            Active = userJob.Active,
                            FormatPermissions = userJob.FormatPermissions
                        };
                        addedUserJobsIds.Add(userJob.Id);
                        copiedJobs.Add(newUserJob);
                        var karateRanks = new List<KarateRefereesRank>();
                        foreach (var karaterank in userJob.KarateRefereesRanks)
                        {
                            karateRanks.Add(new KarateRefereesRank { Type = karaterank.Type, Date = karaterank.Date, UsersJob = newUserJob });
                        }
                        copiedKarateRefereesRank.AddRange(karateRanks);
                    }

                    foreach (var userJob in unionClub.UsersJobs1.Where(x => x.SeasonId == lastSeasonId && !addedUserJobsIds.Contains(x.Id)))
                    {
                        var newUserJob = new UsersJob
                        {
                            UnionId = unionId,
                            ConnectedClub = newClub,
                            IsBlocked = userJob.IsBlocked,
                            JobId = userJob.JobId,
                            RateType = userJob.RateType,
                            SeasonId = newSeasonId,
                            UserId = userJob.UserId,
                            WithhodlingTax = userJob.WithhodlingTax,
                            Active = userJob.Active,
                            FormatPermissions = userJob.FormatPermissions
                        };
                        addedUserJobsIds.Add(userJob.Id);
                        copiedJobs.Add(newUserJob);
                        var karateRanks = new List<KarateRefereesRank>();
                        foreach (var karaterank in userJob.KarateRefereesRanks)
                        {
                            karateRanks.Add(new KarateRefereesRank { Type = karaterank.Type, Date = karaterank.Date, UsersJob = newUserJob });
                        }
                        copiedKarateRefereesRank.AddRange(karateRanks);
                    }

                    foreach (var clubTeam in unionClub.ClubTeams.Where(x => x.SeasonId == lastSeasonId))
                    {
                        var newClubTeam = new ClubTeam
                        {
                            TeamId = clubTeam.TeamId,
                            Club = newClub,
                            DepartmentId = clubTeam.DepartmentId,
                            IsBlocked = clubTeam.IsBlocked,
                            IsTrainingTeam = clubTeam.IsTrainingTeam,
                            SeasonId = newSeasonId,
                            TeamPosition = clubTeam.TeamPosition
                        };

                        var lastSeasonTeamDetails =
                            clubTeam.Team.TeamsDetails?.FirstOrDefault(x => x.SeasonId == lastSeasonId);
                        if (lastSeasonTeamDetails != null)
                        {
                            teamDetails.Add(new TeamsDetails
                            {
                                TeamId = clubTeam.TeamId,
                                SeasonId = newSeasonId,
                                RegistrationId = lastSeasonTeamDetails.RegistrationId,
                                TeamName = lastSeasonTeamDetails.TeamName
                            });
                        }

                        foreach (var teamDiscipline in clubTeam.Team.TeamDisciplines.Where(x => x.SeasonId == lastSeasonId))
                        {
                            var newTeamDiscipline = new TeamDiscipline
                            {
                                TeamId = clubTeam.TeamId,
                                Club = newClub,
                                DisciplineId = teamDiscipline.DisciplineId,
                                SeasonId = newSeasonId
                            };

                            copiedTeamDisciplines.Add(newTeamDiscipline);
                        }

                        foreach (var clubTeamPlayer in clubTeam.Team.TeamsPlayers.Where(x => x.SeasonId == lastSeasonId && x.ClubId == unionClub.ClubId))
                        {
                            if (!isTennis || clubTeam.IsTrainingTeam)
                            {
                                teamPlayers.Add(new TeamsPlayer
                                {
                                    TeamId = clubTeamPlayer.TeamId,
                                    Club = newClub,
                                    SeasonId = newSeasonId,
                                    LeagueId = clubTeamPlayer.LeagueId,
                                    ApprovalDate = isTennis ? clubTeamPlayer.ApprovalDate : null,
                                    //ClubComment = clubTeamPlayer.ClubComment,
                                    //Comment = clubTeamPlayer.Comment,
                                    DateOfCreate = clubTeamPlayer.DateOfCreate,
                                    HandicapLevel = clubTeamPlayer.HandicapLevel,
                                    IsActive = isTennis && clubTeamPlayer.IsActive,
                                    IsApprovedByManager = isTennis ? clubTeamPlayer.IsApprovedByManager : null,
                                    IsEscortPlayer = clubTeamPlayer.IsEscortPlayer,
                                    IsExceptionalMoved = clubTeamPlayer.IsExceptionalMoved,
                                    IsLocked = clubTeamPlayer.IsLocked != null ? true : (bool?)null,
                                    IsTrainerPlayer = clubTeamPlayer.IsTrainerPlayer,
                                    //MedExamDate = clubTeamPlayer.MedExamDate,
                                    Paid = clubTeamPlayer.Paid,
                                    PosId = clubTeamPlayer.PosId,
                                    ShirtNum = clubTeamPlayer.ShirtNum,
                                    StartPlaying = clubTeamPlayer.StartPlaying,
                                    TennisPositionOrder = clubTeamPlayer.TennisPositionOrder,
                                    //UnionComment = clubTeamPlayer.UnionComment,
                                    UserId = clubTeamPlayer.UserId,
                                    WithoutLeagueRegistration = clubTeamPlayer.WithoutLeagueRegistration
                                });
                            }

                            foreach (var playerFile in clubTeamPlayer.User.PlayerFiles.Where(x =>
                                x.SeasonId == lastSeasonId && x.FileType == (int) PlayerFileType.PlayerImage))
                            {
                                copiedPlayerFiles.Add(new PlayerFile
                                {
                                    SeasonId = newSeasonId,
                                    DateCreated = playerFile.DateCreated,
                                    FileName = playerFile.FileName,
                                    FileType = playerFile.FileType,
                                    PlayerId = playerFile.PlayerId
                                });
                            }

                            if (isTennis)
                            {
                                foreach (var playerFile in clubTeamPlayer.User.PlayerFiles.Where(x =>
                                    x.SeasonId == lastSeasonId && x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive))
                                {
                                    copiedPlayerFiles.Add(new PlayerFile
                                    {
                                        SeasonId = newSeasonId,
                                        DateCreated = playerFile.DateCreated,
                                        FileName = playerFile.FileName,
                                        FileType = playerFile.FileType,
                                        PlayerId = playerFile.PlayerId
                                    });
                                }
                            }

                            foreach (var playerDiscipline in clubTeamPlayer.User.PlayerDisciplines.Where(x => x.SeasonId == lastSeasonId))
                            {
                                var newPlayerDiscipline = new PlayerDiscipline
                                {
                                    SeasonId = newSeasonId,
                                    Club = newClub,
                                    DisciplineId = playerDiscipline.DisciplineId,
                                    PlayerId = playerDiscipline.PlayerId
                                };

                                copiedPlayerDisciplines.Add(newPlayerDiscipline);
                            }
                        }

                        copiedClubTeams.Add(newClubTeam);
                    }

                    copiedClubs.Add(newClub);
                }
            }

            foreach (var athleteNumber in athleteNumbers)
            {
                copiedAthleteNumbers.Add(new AthleteNumber
                {
                    UserId = athleteNumber.UserId,
                    SeasonId = newSeasonId
                });
            }

            foreach (var seasonRecord in seasonRecords)
            {
                copiedSeasonRecords.Add(new SeasonRecord
                {
                    DisciplineRecordsId = seasonRecord.DisciplineRecordsId,
                    SeasonId = newSeasonId
                });
            }

            db.Leagues.AddRange(copiedLeagues);
            db.LeagueTeams.AddRange(leagueTeams);

            db.Auditoriums.AddRange(copiedAuditoriums);

            db.Clubs.AddRange(copiedClubs);
            db.ClubTeams.AddRange(copiedClubTeams);

            db.TeamsDetails.AddRange(teamDetails);

            db.TeamsPlayers.AddRange(teamPlayers);

            db.UnionOfficialSettings.AddRange(copiedUnionOfficialsSettings);
            db.LeagueOfficialsSettings.AddRange(copiedLeagueOfficialsSettings);
            db.DistanceTables.AddRange(copiedDistanceTables);
            db.UsersJobs.AddRange(copiedJobs);
            db.ClubDisciplines.AddRange(copiedClubDisciplines);
            db.TeamDisciplines.AddRange(copiedTeamDisciplines);
            db.PlayerDisciplines.AddRange(copiedPlayerDisciplines);
            db.Instruments.AddRange(copiedInstruments);
            db.PlayerFiles.AddRange(copiedPlayerFiles);

            db.CompetitionAges.AddRange(copiedCompetitionAges);
            db.CompetitionLevels.AddRange(copiedCompetitionLevels);
            db.CompetitionRegions.AddRange(copiedCompetitionRegions);

            db.TeamRegistrations.AddRange(copiedTeamsRegistrations);

            db.UnionForms.AddRange(copiedUnionForms);
            db.AthleteNumbers.AddRange(copiedAthleteNumbers);
            db.SeasonRecords.AddRange(copiedSeasonRecords);
            db.KarateRefereesRanks.AddRange(copiedKarateRefereesRank);

            db.SaveChanges();
            if (union!= null)
            {
                if (string.Equals(union?.Section?.Alias, SectionAliases.Athletics,
                        StringComparison.CurrentCultureIgnoreCase) || string.Equals(union?.Section?.Alias, SectionAliases.Gymnastic,
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    ResetMedExamDate(lastSeasonId, union.UnionId);
                }

                if (string.Equals(union?.Section?.Alias, SectionAliases.Gymnastic,
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    IncreaseRouteAndRouteTeamIds(disciplineRouteIds, disciplineTeamRouteIds);
                }
            }
            db.Configuration.LazyLoadingEnabled = dbLazyLoad;
            db.Configuration.AutoDetectChangesEnabled = dbDetectChanges;
        }

        private void ResetMedExamDate(int seasonId, int unionId)
        {
            var clubTeams = db.Clubs.SelectMany(cl => cl.ClubTeams, (club, clubTeam) => new { cl = club, ct = clubTeam })
                .Where(t => t.cl.UnionId == unionId && t.ct.SeasonId == seasonId)
                .Select(t => t.ct)
                .ToList();

            var clubTeamIds = clubTeams.Where(t => t.Club.IsArchive == false && t.Team.IsArchive == false).Select(x => x.TeamId);

            var players = db.TeamsPlayers
                    .Where(tp => clubTeamIds.Contains(tp.TeamId) &&
                                 tp.User.IsArchive == false &&
                                 tp.SeasonId == seasonId &&
                                 (!tp.Team.LeagueTeams.Any() || tp.ClubId == null) && tp.User.MedExamDate != null).Select(x => x.UserId).Distinct().ToList();

            if (players.Count > 0)
            {
                var userIdsCommaSeparated = string.Join(",", players.Select(n => n.ToString()).ToArray());
                db.Database.ExecuteSqlCommand($"Update Users Set MedExamDate = NULL WHERE UserId in ({userIdsCommaSeparated})");
            }
        }

        private void IncreaseRouteAndRouteTeamIds(List<int> disciplineRouteIds, List<int> disciplineTeamRouteIds)
        {
            if (disciplineRouteIds.Count > 0)
            {
                var disciplineRouteIdsCommaSeparated = string.Join(",", disciplineRouteIds.Select(n => n.ToString()).ToArray());
                db.Database.ExecuteSqlCommand($"UPDATE RouteRanks SET FromAge = DATEADD(yyyy,1,FromAge), ToAge = DATEADD(yyyy,1,ToAge) WHERE Id in ({disciplineRouteIdsCommaSeparated})");
            }

            if (disciplineTeamRouteIds.Count > 0)
            {
                var disciplineTeamRouteIdsCommaSeparated = string.Join(",", disciplineTeamRouteIds.Select(n => n.ToString()).ToArray());
                db.Database.ExecuteSqlCommand($"UPDATE RouteTeamRanks SET FromAge = DATEADD(yyyy,1,FromAge), ToAge = DATEADD(yyyy,1,ToAge) WHERE Id in ({disciplineTeamRouteIdsCommaSeparated})");
            }
           
        }

        public Season GetLastSeasonIdByCurrentLeagueId(int leagueId)
        {
            var league = db.Leagues.First(c => c.LeagueId == leagueId);
            if (league?.Club != null) return GetLastSeasonByCurrentClub(league?.Club);
            if (league?.Union != null) return GetById(GetLastSeasonByCurrentUnionId(league.UnionId.Value));
            return league?.Season;
        }


        public Season Get(int seasonId, int? unionId = null)
        {
            return db.Seasons.FirstOrDefault(x => x.Id == seasonId && (!unionId.HasValue || x.UnionId == unionId) && x.IsActive);
        }

        public int GetLastSeasonByCurrentUnionId(int unionId)
        {
            int? seasonId = db.Seasons.Where(x => x.UnionId == unionId && x.IsActive).OrderByDescending(x => x.Id).FirstOrDefault()?.Id;
            return seasonId ?? -1;
        }
        public int GetLastSeasonByCurrentUnionIdByUser(int UserId)
        {
            int? seasonId;
            int? unionId = db.UsersJobs.Where(x => x.UserId == UserId && x.UnionId.HasValue == true).Select(x => x.UnionId).FirstOrDefault();
            var club = db.UsersJobs.Where(x => x.UserId == UserId).FirstOrDefault()?.Team?.ClubTeams?.FirstOrDefault()?.Club ??
                db.UsersJobs.Where(x => x.UserId == UserId).FirstOrDefault()?.League?.Club ?? db.UsersJobs.Where(x => x.UserId == UserId).FirstOrDefault()?.Club ?? null;
            if(club != null)
            {
                seasonId = GetLastSeasonIdByCurrentClubId(club.ClubId);
                if(seasonId != null && seasonId != 0)
                {
                    return seasonId??-1;
                }
            }
            seasonId = db.Seasons.Where(x => x.UnionId == unionId && x.IsActive).OrderByDescending(x => x.Id).FirstOrDefault()?.Id;
            return seasonId ?? -1;
        }

        public int GetLastSeasonIdByCurrentClubId(int clubId)
        {
            //var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            if(club == null)
            {
                return 0;
            }
            return GetLastSeasonByCurrentClub(club).Id;
        }

        public int GetLastSeasonIdByCurrentTeamId(int teamId)
        {
            var clubId = db.ClubTeams.FirstOrDefault(x => x.TeamId == teamId)?.ClubId;
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            if (club == null)
            {
                return 0;
            }
            return GetLastSeasonByCurrentClub(club).Id;
        }

        public Season GetLastSeasonByCurrentClub(Club club)
        {
            if (club.IsSectionClub ?? true)
            {
                if (club.ParentClubId != null)
                    return db.Seasons.Where(x => x.ClubId == club.ParentClubId && x.IsActive).OrderByDescending(x => x.Id).First();

                else
                    return db.Seasons.Where(x => x.ClubId == club.ClubId && x.IsActive).OrderByDescending(x => x.Id).FirstOrDefault();
            }
            else
            {
                return db.Seasons.Where(s => s.UnionId == club.UnionId && s.IsActive).OrderByDescending(s => s.Id).First();
            }
        }

        public List<Season> GetClubsSeasons(int clubId, bool includeInactive)
        {
            var seasons = db.Seasons.Where(x => x.ClubId == clubId);
            if (!includeInactive)
            {
                seasons = seasons.Where(x => x.IsActive);
            }

            return seasons.OrderBy(x => x.Name).ToList();
        }

        public int? GetLastSeasonByLeagueId(int leagueId)
        {
            int? unionId = db.Leagues.Where(x => x.LeagueId == leagueId)
                                   .Select(x => x.UnionId)
                                   .FirstOrDefault();
            int? currentSeasonId = GetLasSeasonByUnionId(unionId??0);
            if(currentSeasonId == null || currentSeasonId == 0)
            {
                currentSeasonId = db.Leagues.Where(x => x.LeagueId == leagueId).OrderBy(g => g.SeasonId).FirstOrDefault()?.SeasonId;
            }
            return currentSeasonId;
        }

        public int? GetLasSeasonByUnionId(int unionId)
        {
            int? seasonId = db.Seasons.Where(x => x.UnionId == unionId && x.IsActive)
                               .Select(x => x.Id)
                               .OrderByDescending(x => x)
                               .FirstOrDefault();
            return seasonId;
        }

        public Season GetLastSeason()
        {
            var lastSeason = db.Seasons.Where(x => !x.ClubId.HasValue && x.IsActive).OrderByDescending(x => x.Id).First();
            return lastSeason;
        }

        public Season GetUnionlessCurrentSeason()
        {
            var lastSeason = db.Seasons.Where(x => !x.ClubId.HasValue && x.IsActive && !x.UnionId.HasValue && x.StartDate <= DateTime.Now && x.EndDate > DateTime.Now).OrderByDescending(x => x.Id).FirstOrDefault();
            return lastSeason;
        }

        public Season GetLastClubSeason()
        {
            var lastSeason = db.Seasons.Where(x => x.ClubId.HasValue && x.IsActive).OrderByDescending(x => x.Id).FirstOrDefault();
            return lastSeason;
        }


        public string GetGameAliasBySeasonId(int seasonId)
        {
            var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId && s.IsActive);
            return GetGameAliasBySeason(season);
        }
        public string GetGameAliasBySeason(Season season)
        {
            return season?.Union?.Section?.Alias;
        }

        public bool IsBasketBallOrWaterPoloSeason(int seasonId)
        {
            var alias = GetGameAliasBySeasonId(seasonId);
            return alias == GamesAlias.BasketBall || alias == GamesAlias.WaterPolo || alias == GamesAlias.Soccer || alias == GamesAlias.Rugby || alias == GamesAlias.Softball;
        }

        #region changes for Regional section

        public Season GetLastSeasonByUnionId(int unionID)
        {
            var lst = db.Seasons.Where(s => s.UnionId == unionID && s.IsActive);
            if (lst != null && lst.Count() > 0)
                return lst.OrderByDescending(s => s.Id).First();
            else
                return null;
        }

        #endregion
    }
}
