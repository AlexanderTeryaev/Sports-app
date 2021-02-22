﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AppModel
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class DataEntities : DbContext
    {
        public DataEntities()
            : base("name=DataEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Comments> Comments { get; set; }
        public virtual DbSet<Contacts> Contacts { get; set; }
        public virtual DbSet<ContentStates> ContentStates { get; set; }
        public virtual DbSet<ContentTypes> ContentTypes { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<Contents> Contents { get; set; }
        public virtual DbSet<EventsLog> EventsLog { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Gender> Genders { get; set; }
        public virtual DbSet<JobsRole> JobsRoles { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<Section> Sections { get; set; }
        public virtual DbSet<Union> Unions { get; set; }
        public virtual DbSet<UsersType> UsersTypes { get; set; }
        public virtual DbSet<Age> Ages { get; set; }
        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<TeamsAuditorium> TeamsAuditoriums { get; set; }
        public virtual DbSet<GamesType> GamesTypes { get; set; }
        public virtual DbSet<Game> Games { get; set; }
        public virtual DbSet<GameSet> GameSets { get; set; }
        public virtual DbSet<CompetitionClubsCorrection> CompetitionClubsCorrections { get; set; }
        public virtual DbSet<TeamsFan> TeamsFans { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<NotesRecipient> NotesRecipients { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<NotesGame> NotesGames { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<WallThread> WallThreads { get; set; }
        public virtual DbSet<UsersDvice> UsersDvices { get; set; }
        public virtual DbSet<UnionsDoc> UnionsDocs { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<LeaguesDoc> LeaguesDocs { get; set; }
        public virtual DbSet<LeagueTeams> LeagueTeams { get; set; }
        public virtual DbSet<PlayerHistory> PlayerHistory { get; set; }
        public virtual DbSet<TeamsDetails> TeamsDetails { get; set; }
        public virtual DbSet<UsersJob> UsersJobs { get; set; }
        public virtual DbSet<Season> Seasons { get; set; }
        public virtual DbSet<TeamStanding> TeamStandings { get; set; }
        public virtual DbSet<TeamStandingGame> TeamStandingGames { get; set; }
        public virtual DbSet<TeamScheduleScrapper> TeamScheduleScrappers { get; set; }
        public virtual DbSet<TeamScheduleScrapperGame> TeamScheduleScrapperGames { get; set; }
        public virtual DbSet<NotesMessage> NotesMessages { get; set; }
        public virtual DbSet<SportCenter> SportCenters { get; set; }
        public virtual DbSet<Club> Clubs { get; set; }
        public virtual DbSet<Auditorium> Auditoriums { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<UsersFriend> UsersFriends { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<GroupsTeam> GroupsTeams { get; set; }
        public virtual DbSet<LeaguesPrice> LeaguesPrices { get; set; }
        public virtual DbSet<Sport> Sports { get; set; }
        public virtual DbSet<PlayerAchievement> PlayerAchievements { get; set; }
        public virtual DbSet<SportRank> SportRanks { get; set; }
        public virtual DbSet<PlayerFile> PlayerFiles { get; set; }
        public virtual DbSet<RetirementRequest> RetirementRequests { get; set; }
        public virtual DbSet<GamesCycle> GamesCycles { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<PlayoffBracket> PlayoffBrackets { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<TeamsPlayer> TeamsPlayers { get; set; }
        public virtual DbSet<League> Leagues { get; set; }
        public virtual DbSet<SentMessage> SentMessages { get; set; }
        public virtual DbSet<TeamBenefactor> TeamBenefactors { get; set; }
        public virtual DbSet<ClubTrainingDay> ClubTrainingDays { get; set; }
        public virtual DbSet<TeamTraining> TeamTrainings { get; set; }
        public virtual DbSet<TrainingAttendance> TrainingAttendances { get; set; }
        public virtual DbSet<TrainingDaysSetting> TrainingDaysSettings { get; set; }
        public virtual DbSet<TrainingSetting> TrainingSettings { get; set; }
        public virtual DbSet<Discipline> Disciplines { get; set; }
        public virtual DbSet<DisciplinesDoc> DisciplinesDocs { get; set; }
        public virtual DbSet<PlayersBenefactorPrice> PlayersBenefactorPrices { get; set; }
        public virtual DbSet<School> Schools { get; set; }
        public virtual DbSet<SchoolTeam> SchoolTeams { get; set; }
        public virtual DbSet<ClubTeamPrice> ClubTeamPrices { get; set; }
        public virtual DbSet<PlayerDiscount> PlayerDiscounts { get; set; }
        public virtual DbSet<NationalTeamInvitement> NationalTeamInvitements { get; set; }
        public virtual DbSet<UnionForm> UnionForms { get; set; }
        public virtual DbSet<MemberFee> MemberFees { get; set; }
        public virtual DbSet<HandlingFee> HandlingFees { get; set; }
        public virtual DbSet<PlayersBlockade> PlayersBlockades { get; set; }
        public virtual DbSet<BlockadeNotification> BlockadeNotifications { get; set; }
        public virtual DbSet<LeagueOfficialsSetting> LeagueOfficialsSettings { get; set; }
        public virtual DbSet<DistanceTable> DistanceTables { get; set; }
        public virtual DbSet<DisciplineRoute> DisciplineRoutes { get; set; }
        public virtual DbSet<UsersRank> UsersRanks { get; set; }
        public virtual DbSet<UsersRoute> UsersRoutes { get; set; }
        public virtual DbSet<PlayerDiscipline> PlayerDisciplines { get; set; }
        public virtual DbSet<ClubDiscipline> ClubDisciplines { get; set; }
        public virtual DbSet<TeamDiscipline> TeamDisciplines { get; set; }
        public virtual DbSet<Activity> Activities { get; set; }
        public virtual DbSet<ActivitiesLeague> ActivitiesLeagues { get; set; }
        public virtual DbSet<ActivitiesPrice> ActivitiesPrices { get; set; }
        public virtual DbSet<ActivitiesUser> ActivitiesUsers { get; set; }
        public virtual DbSet<TeamRegistrationPayment> TeamRegistrationPayments { get; set; }
        public virtual DbSet<TeamPlayersPayment> TeamPlayersPayments { get; set; }
        public virtual DbSet<ActivityBranch> ActivityBranches { get; set; }
        public virtual DbSet<ActivityCustomPrice> ActivityCustomPrices { get; set; }
        public virtual DbSet<ActivityForm> ActivityForms { get; set; }
        public virtual DbSet<ActivityFormsDetail> ActivityFormsDetails { get; set; }
        public virtual DbSet<ActivityFormsFile> ActivityFormsFiles { get; set; }
        public virtual DbSet<ActivityFormsSubmittedData> ActivityFormsSubmittedDatas { get; set; }
        public virtual DbSet<ActivityStatusColumnsVisibility> ActivityStatusColumnsVisibilities { get; set; }
        public virtual DbSet<ColumnVisibility> ColumnVisibilities { get; set; }
        public virtual DbSet<WeightLiftingSession> WeightLiftingSessions { get; set; }
        public virtual DbSet<CompetitionRouteClub> CompetitionRouteClubs { get; set; }
        public virtual DbSet<CompetitionTeamRouteClub> CompetitionTeamRouteClubs { get; set; }
        public virtual DbSet<CompetitionRoute> CompetitionRoutes { get; set; }
        public virtual DbSet<CompetitionRegistration> CompetitionRegistrations { get; set; }
        public virtual DbSet<CompetitionResult> CompetitionResults { get; set; }
        public virtual DbSet<AdditionalGymnastic> AdditionalGymnastics { get; set; }
        public virtual DbSet<AdditionalTeamGymnastic> AdditionalTeamGymnastics { get; set; }
        public virtual DbSet<UsersEducation> UsersEducations { get; set; }
        public virtual DbSet<KarateRefereesRank> KarateRefereesRanks { get; set; }
        public virtual DbSet<KarateUnionPayment> KarateUnionPayments { get; set; }
        public virtual DbSet<DisplayedPaymentMessage> DisplayedPaymentMessages { get; set; }
        public virtual DbSet<TeamRegistration> TeamRegistrations { get; set; }
        public virtual DbSet<UnionOfficialSetting> UnionOfficialSettings { get; set; }
        public virtual DbSet<PositionSetting> PositionSettings { get; set; }
        public virtual DbSet<RefereeRegistration> RefereeRegistrations { get; set; }
        public virtual DbSet<Instrument> Instruments { get; set; }
        public virtual DbSet<TeamPenalty> TeamPenalties { get; set; }
        public virtual DbSet<Statistic> Statistics { get; set; }
        public virtual DbSet<GameStatistic> GameStatistics { get; set; }
        public virtual DbSet<UnionPrice> UnionPrices { get; set; }
        public virtual DbSet<DriverDetail> DriverDetails { get; set; }
        public virtual DbSet<DriverLicenseType> DriverLicenseTypes { get; set; }
        public virtual DbSet<EngineDetail> EngineDetails { get; set; }
        public virtual DbSet<VehicleDetail> VehicleDetails { get; set; }
        public virtual DbSet<VehicleLicens> VehicleLicenses { get; set; }
        public virtual DbSet<VehicleModel> VehicleModels { get; set; }
        public virtual DbSet<VehicleProduct> VehicleProducts { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }
        public virtual DbSet<SportsRegistration> SportsRegistrations { get; set; }
        public virtual DbSet<CompetitionLevel> CompetitionLevels { get; set; }
        public virtual DbSet<TennisRank> TennisRanks { get; set; }
        public virtual DbSet<PenaltyForExclusion> PenaltyForExclusions { get; set; }
        public virtual DbSet<AdvBanner> AdvBanners { get; set; }
        public virtual DbSet<InitialApprovalDate> InitialApprovalDates { get; set; }
        public virtual DbSet<UsersApprovalDatesHistory> UsersApprovalDatesHistories { get; set; }
        public virtual DbSet<TennisGameCycle> TennisGameCycles { get; set; }
        public virtual DbSet<TennisGame> TennisGames { get; set; }
        public virtual DbSet<TennisGameSet> TennisGameSets { get; set; }
        public virtual DbSet<TennisGroup> TennisGroups { get; set; }
        public virtual DbSet<TennisStage> TennisStages { get; set; }
        public virtual DbSet<TennisPlayoffBracket> TennisPlayoffBrackets { get; set; }
        public virtual DbSet<TennisGroupTeam> TennisGroupTeams { get; set; }
        public virtual DbSet<DaysForHosting> DaysForHostings { get; set; }
        public virtual DbSet<TeamHostingDay> TeamHostingDays { get; set; }
        public virtual DbSet<ActivityStatusColumnName> ActivityStatusColumnNames { get; set; }
        public virtual DbSet<ActivityStatusColumnsOrder> ActivityStatusColumnsOrders { get; set; }
        public virtual DbSet<ActivityStatusColumnsSorting> ActivityStatusColumnsSortings { get; set; }
        public virtual DbSet<CompetitionRegion> CompetitionRegions { get; set; }
        public virtual DbSet<TennisLeagueGame> TennisLeagueGames { get; set; }
        public virtual DbSet<TennisLeagueGameScore> TennisLeagueGameScores { get; set; }
        public virtual DbSet<ActivityBranchesState> ActivityBranchesStates { get; set; }
        public virtual DbSet<ClubTeam> ClubTeams { get; set; }
        public virtual DbSet<CategoriesPlaceDate> CategoriesPlaceDates { get; set; }
        public virtual DbSet<LeagueScheduleState> LeagueScheduleStates { get; set; }
        public virtual DbSet<MedicalCertApprovement> MedicalCertApprovements { get; set; }
        public virtual DbSet<LevelDateSetting> LevelDateSettings { get; set; }
        public virtual DbSet<LeaguesFan> LeaguesFans { get; set; }
        public virtual DbSet<ClubPayment> ClubPayments { get; set; }
        public virtual DbSet<NotesAttachedFile> NotesAttachedFiles { get; set; }
        public virtual DbSet<ClubBalance> ClubBalances { get; set; }
        public virtual DbSet<TeamsRank> TeamsRanks { get; set; }
        public virtual DbSet<TeamsRoute> TeamsRoutes { get; set; }
        public virtual DbSet<DisciplineTeamRoute> DisciplineTeamRoutes { get; set; }
        public virtual DbSet<CompetitionAge> CompetitionAges { get; set; }
        public virtual DbSet<CompetitionHeatWind> CompetitionHeatWinds { get; set; }
        public virtual DbSet<CompetitionDiscipline> CompetitionDisciplines { get; set; }
        public virtual DbSet<CompetitionDisciplineRegistration> CompetitionDisciplineRegistrations { get; set; }
        public virtual DbSet<CompetitionTeamRoute> CompetitionTeamRoutes { get; set; }
        public virtual DbSet<RouteRank> RouteRanks { get; set; }
        public virtual DbSet<RouteTeamRank> RouteTeamRanks { get; set; }
        public virtual DbSet<OfficialGameReportDetail> OfficialGameReportDetails { get; set; }
        public virtual DbSet<RankedStandingCorrection> RankedStandingCorrections { get; set; }
        public virtual DbSet<Stage> Stages { get; set; }
        public virtual DbSet<RefereeCompetitionRegistration> RefereeCompetitionRegistrations { get; set; }
        public virtual DbSet<Regional> Regionals { get; set; }
        public virtual DbSet<LiqPayPaymentsNotification> LiqPayPaymentsNotifications { get; set; }
        public virtual DbSet<AthleticLeague> AthleticLeagues { get; set; }
        public virtual DbSet<TennisCategoryPlayoffRank> TennisCategoryPlayoffRanks { get; set; }
        public virtual DbSet<RefereeSalaryReport> RefereeSalaryReports { get; set; }
        public virtual DbSet<ResetPasswordRequest> ResetPasswordRequests { get; set; }
        public virtual DbSet<RowingDistance> RowingDistances { get; set; }
        public virtual DbSet<TravelInformation> TravelInformations { get; set; }
        public virtual DbSet<CardComIndicator> CardComIndicators { get; set; }
        public virtual DbSet<FriendshipsType> FriendshipsTypes { get; set; }
        public virtual DbSet<ChipPrice> ChipPrices { get; set; }
        public virtual DbSet<FriendshipPrice> FriendshipPrices { get; set; }
        public virtual DbSet<CompetitionDisciplineHeatStartTime> CompetitionDisciplineHeatStartTimes { get; set; }
        public virtual DbSet<CompetitionDisciplineTeam> CompetitionDisciplineTeams { get; set; }
        public virtual DbSet<DisciplineRecord> DisciplineRecords { get; set; }
        public virtual DbSet<CompetitionDisciplineClubsRegistration> CompetitionDisciplineClubsRegistrations { get; set; }
        public virtual DbSet<AthleteNumber> AthleteNumbers { get; set; }
        public virtual DbSet<SeasonRecord> SeasonRecords { get; set; }
        public virtual DbSet<LevelPointsSetting> LevelPointsSettings { get; set; }
        public virtual DbSet<BicycleCompetitionDiscipline> BicycleCompetitionDisciplines { get; set; }
        public virtual DbSet<BicycleCompetitionHeat> BicycleCompetitionHeats { get; set; }
        public virtual DbSet<DisciplineExpertise> DisciplineExpertises { get; set; }
        public virtual DbSet<CompetitionExperty> CompetitionExperties { get; set; }
        public virtual DbSet<CompetitionExpertiesHeat> CompetitionExpertiesHeats { get; set; }
        public virtual DbSet<CompetitionExpertiesDisciplineHeat> CompetitionExpertiesDisciplineHeats { get; set; }
        public virtual DbSet<BicycleDisciplineRegistration> BicycleDisciplineRegistrations { get; set; }
        public virtual DbSet<Benefit> Benefits { get; set; }
        public virtual DbSet<BicycleFriendshipPayment> BicycleFriendshipPayments { get; set; }
        public virtual DbSet<MedicalInstitute> MedicalInstitutes { get; set; }
        public virtual DbSet<CompetitionExpertiesHeatsAge> CompetitionExpertiesHeatsAges { get; set; }
        public virtual DbSet<InsuranceType> InsuranceTypes { get; set; }
        public virtual DbSet<WaterpoloStatistic> WaterpoloStatistics { get; set; }
    
        public virtual int sp_alterdiagram(string diagramname, Nullable<int> owner_id, Nullable<int> version, byte[] definition)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var versionParameter = version.HasValue ?
                new ObjectParameter("version", version) :
                new ObjectParameter("version", typeof(int));
    
            var definitionParameter = definition != null ?
                new ObjectParameter("definition", definition) :
                new ObjectParameter("definition", typeof(byte[]));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_alterdiagram", diagramnameParameter, owner_idParameter, versionParameter, definitionParameter);
        }
    
        public virtual int sp_creatediagram(string diagramname, Nullable<int> owner_id, Nullable<int> version, byte[] definition)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var versionParameter = version.HasValue ?
                new ObjectParameter("version", version) :
                new ObjectParameter("version", typeof(int));
    
            var definitionParameter = definition != null ?
                new ObjectParameter("definition", definition) :
                new ObjectParameter("definition", typeof(byte[]));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_creatediagram", diagramnameParameter, owner_idParameter, versionParameter, definitionParameter);
        }
    
        public virtual int sp_dropdiagram(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_dropdiagram", diagramnameParameter, owner_idParameter);
        }
    
        public virtual ObjectResult<sp_helpdiagramdefinition_Result> sp_helpdiagramdefinition(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_helpdiagramdefinition_Result>("sp_helpdiagramdefinition", diagramnameParameter, owner_idParameter);
        }
    
        public virtual ObjectResult<sp_helpdiagrams_Result> sp_helpdiagrams(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_helpdiagrams_Result>("sp_helpdiagrams", diagramnameParameter, owner_idParameter);
        }
    
        public virtual int sp_renamediagram(string diagramname, Nullable<int> owner_id, string new_diagramname)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var new_diagramnameParameter = new_diagramname != null ?
                new ObjectParameter("new_diagramname", new_diagramname) :
                new ObjectParameter("new_diagramname", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_renamediagram", diagramnameParameter, owner_idParameter, new_diagramnameParameter);
        }
    
        public virtual int sp_upgraddiagrams()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_upgraddiagrams");
        }
    }
}