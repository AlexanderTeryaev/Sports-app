using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppModel;
using DataService.DTO;
using DataService.Utils;
using System.Data.Entity;
using System.Linq.Expressions;

namespace DataService
{
    public class TeamPlayerItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ShirtNum { get; set; }
        public int? PosId { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? GenderId { get; set; }
        public virtual Gender Gender { get; set; }
        public int? AthletesNumbers { get; set; }
        public string IdentNum { get; set; }
        public string PassportNum { get; set; }
        public bool IsActive { get; set; }
        public int SeasonId { get; set; }
        public int TeamId { get; set; }
        public bool? IsLocked { get; set; }
        public bool IsApproveChecked { get; set; }
        public bool IsNotApproveChecked { get; set; }
        public DateTime? Birthday { get; set; }
        public string WeightUnits { get; set; }
        public int? Weight { get; set; }
        public string Email { get; set; }
        public string ShirtSize { get; set; }
        public string Telephone { get; set; }
        public string City { get; set; }
        public Nullable<bool> MedicalCertificate { get; set; }
        public Nullable<bool> Insurance { get; set; }
        public string MedicalCertificateFile { get; set; }
        public string InsuranceFile { get; set; }
        public string IDFile { get; set; }
        public string UnionComment { get; set; }
        public decimal PlayerRegistrationPrice { get; set; }
        public decimal PlayerInsurancePrice { get; set; }
        public DateTime? TenicardValidity { get; set; }
        public bool IsPlayerRegistered { get; set; }
        public bool IsPlayerRegistrationApproved { get; set; }
        public string PlayerImage { get; set; }
        public ActivityFormsSubmittedData Registration { get; set; }
        public decimal ManagerRegistrationDiscount { get; set; }
        public decimal ManagerParticipationDiscount { get; set; }
        public DateTime? MedExamDate { get; set; }
        public decimal PlayerRegistrationAndEquipmentPrice { get; set; }
        public decimal ParticipationPrice { get; set; }
        public bool NoInsurancePayment { get; set; }

        public string LeagueName { get; set; }
        public string TeamName { get; set; }

        public decimal MembersFee { get; set; }
        public decimal HandlingFee { get; set; }

        public bool IsTrainerPlayer { get; set; }
        public bool IsEscortPlayer { get; set; }
        public bool? IsApprovedByManager { get; set; }
        public int? AthleteNumber { get; set; }

        public bool IsBlockaded { get; set; }

        public bool IsYoungPlayer { get; set; }
        public DateTime? StartPlaying { get; set; }
        public decimal FinalHandicapLevel { get; set; }
        public decimal BaseHandicap { get; set; }
        public string Comment { get; set; }
        public decimal FinalParticipationPrice { get; set; }
        public decimal TeamPlayerPaid { get; set; }
        public bool IsExceptionalMoved { get; set; }
        public bool HasMoreThanOneTeam { get; set; }
        public bool ToWaitingStatus { get; set; }
        public bool IsUnderPenalty { get; set; }
        public int? TennisPositionOrder { get; set; }
        public bool NextTournamentRoster { get; set; }
        public int? SeasonIdOfCreation { get; set; }
        public int? CompetitionCount { get; set; }
        public bool IsNotMeetingRequirements { get; set; }
        public int TennisRank { get; set; }

        public string TrainingClubName { get; set; }
    }

    public class PlayersRepo : BaseRepo
    {
        public PlayersRepo() : base() { }
        public PlayersRepo(DataEntities db) : base(db) { }

        public User GetUserByIdentNum(string identNum)
        {
            return db.Users.FirstOrDefault(t => t.IdentNum == identNum && !t.IsArchive);
        }



        public User GetUserByIdentNumOrPassportNum(string id)
        {
            var user = GetUserByIdentNum(id);
            if (user == null)
            {
                user = GetUserByPassportNumNotArchived(id);
            }
            return user;
        }


        public User GetGymnasticRegByIdentNumOrPassportNum(string id)
        {
            while (id.Length < 9)
            {
                id = "0" + id;
            }
            var user = GetGymnasticRegByIdentNum(id);
            if (user == null)
            {
                user = GetGymnasticRegByPassportNum(id);
            }
            return user;
        }


        public User GetGymnasticRegByIdentNum(string identNum)
        {
            return db.Users.FirstOrDefault(r => r.IdentNum == identNum && !r.IsArchive && r.CompetitionRegistrations.Any(cr => cr.IsActive && cr.UserId == r.UserId))
                ?? db.Users.FirstOrDefault(r => r.IdentNum == identNum && !r.IsArchive);
        }


        public User GetGymnasticRegByPassportNum(string passportNum)
        {
            return db.Users.FirstOrDefault(r => r.PassportNum == passportNum && !r.IsArchive && r.CompetitionRegistrations.Any(cr => cr.IsActive && cr.UserId == r.UserId))
                ?? db.Users.FirstOrDefault(r => r.PassportNum == passportNum && !r.IsArchive);
        }

        public TeamsPlayer GetTeamPlayerByUserId(int userId)
        {
            return db.TeamsPlayers.FirstOrDefault(t => t.User.UserId == userId);
        }

        public int? GetTennisPlayerRank(int userId, int ageId, int seasonId)
        {
            return db.TennisRanks.FirstOrDefault(x => x.UserId == userId && x.AgeId == ageId && x.SeasonId == seasonId)?.Rank;
        }

        public TeamsPlayer GetTeamPlayerByUserIdAndSeasonId(int userId, int seasonId)
        {
            return db.TeamsPlayers.FirstOrDefault(t => t.User.UserId == userId && t.Club != null && t.SeasonId == seasonId);
        }

        public TeamsPlayer GetTeamPlayerByUserIdAndSeasonIdClubCheck(int userId, int seasonId)
        {
            var properClub = db.TeamsPlayers.FirstOrDefault(t => t.User.UserId == userId && t.Club != null && t.SeasonId == seasonId && t.Club.SeasonId == seasonId);
            if (properClub != null)
            {
                return properClub;
            }
            return GetTeamPlayerByUserIdAndSeasonId(userId, seasonId);
        }

        public List<TeamsPlayer> GetTrainingTeamsOfPlayer(int userId, int unionId, int seasonId)
        {
            var unionTrainingTeams = db.ClubTeams
                .Where(x => !x.Club.IsArchive &&
                            !x.Team.IsArchive &&
                            x.Club.UnionId == unionId &&
                            x.Club.SeasonId == seasonId &&
                            x.IsTrainingTeam &&
                            !x.IsBlocked)
                .Select(x => x.TeamId)
                .ToArray();

            return db.TeamsPlayers
                .Where(x => unionTrainingTeams.Contains(x.TeamId) &&
                            !x.Club.IsArchive &&
                            !x.Team.IsArchive &&
                            x.Club.UnionId == unionId &&
                            x.Club.SeasonId == seasonId &&
                            x.SeasonId == seasonId &&
                            x.UserId == userId)
                .ToList();
        }

        public TeamsPlayer GetTeamPlayerByIdentNum(string identNum)
        {
            return db.TeamsPlayers.FirstOrDefault(t => t.User.IdentNum == identNum);
        }

        public TeamsPlayer GetTeamPlayerByIdentNumAndSeasonIdConnectedToClub(string identNum, int seasonId)
        {
            return db.TeamsPlayers.FirstOrDefault(t => t.User.IdentNum == identNum && t.SeasonId == seasonId && t.ClubId.HasValue);
        }

        public TeamsPlayer GetTeamPlayerByIdentNumAndSeasonId(string identNum, int seasonId)
        {
            return db.TeamsPlayers.FirstOrDefault(t => t.User.IdentNum == identNum && t.SeasonId == seasonId);
        }

        public User GetUserByUserId(int userId)
        {
            return db.Users.FirstOrDefault(t => t.UserId == userId);
        }
        public List<TeamsPlayer> GetTeamPlayersByUserIdAndSeasonId(int userId, int seasonId)
        {
            return db.TeamsPlayers.Where(t => t.UserId == userId && t.SeasonId == seasonId).ToList();
        }

        public TeamsPlayer GetTeamPlayerByPassportNum(string passportNum)
        {
            return db.TeamsPlayers.FirstOrDefault(t => t.User.PassportNum == passportNum);
        }

        public List<TeamsPlayer> GetTeamPlayersByIdentNum(string identNum)
        {
            return db.TeamsPlayers.Where(t => t.User.IdentNum == identNum).ToList();
        }

        public TeamsPlayer GetTeamPlayerByIdentNumAndTeamId(string identNum, int teamId)
        {
            return db.TeamsPlayers.FirstOrDefault(t => t.User.IdentNum == identNum && t.TeamId == teamId);
        }


        public void AddToTeam(TeamsPlayer item)
        {
            db.TeamsPlayers.Add(item);
        }

        //public TeamsPlayer GetTeamPlayer(int teamId, int userId, int? posId)
        //{
        //    return db.TeamsPlayers.FirstOrDefault(t => t.TeamId == teamId && t.UserId == userId && t.PosId == posId);
        //}

        public TeamsPlayer GetTeamPlayer(int teamId, int userId, int seasonId, int? posId = null)
        {
            return db.TeamsPlayers.FirstOrDefault(t => t.TeamId == teamId && t.UserId == userId && t.SeasonId == seasonId && (posId == null || t.PosId == posId));
        }
        public TeamsPlayer GetTeamPlayerWithoutTeam( int userId, int seasonId, int? posId = null)
        {
            return db.TeamsPlayers.FirstOrDefault(t => t.UserId == userId && t.SeasonId == seasonId && (posId == null || t.PosId == posId));
        }

        public bool CheckIfBlockadeWasShown(int unionManagerId, int blockadeId)
        {
            var notifications = db.BlockadeNotifications.Where(notification => notification.IsShown == true && notification.ManagerId == unionManagerId).AsEnumerable();
            if (notifications != null && notifications.Any())
            {
                return notifications.Select(n => n.BlockadeId).Contains(blockadeId) ? true : false;
            }
            else
            {
                return false;
            }
        }

        public void AddBlockadeNotification(int unionManagerId, int blockadeId)
        {
            var blockadeNotification = new BlockadeNotification
            {
                BlockadeId = blockadeId,
                ManagerId = unionManagerId,
                IsShown = true
            };
            db.BlockadeNotifications.Add(blockadeNotification);
            db.SaveChanges();
        }


        public void UpdateUserActive(int userId)
        {
            var players = db.TeamsPlayers.Where(p => p.UserId == userId).ToList();
            var user = db.Users.FirstOrDefault(p => p.UserId == userId);

            var isActive = false;
            foreach (var player in players)
            {
                isActive = isActive || player.IsActive;
            }

            if (user != null)
            {
                user.IsActive = isActive;

                db.SaveChanges();
            }
        }

        public TeamsPlayer GetTeamPlayerBySeasonId(int id, int? seasonId)
        {
            return db.TeamsPlayers.FirstOrDefault(x => x.Id == id && x.SeasonId == seasonId);
        }

        public void RemoveFromTeam(TeamsPlayer item)
        {
            db.NationalTeamInvitements.RemoveRange(item.NationalTeamInvitements);
            var payments = db.TeamPlayersPayments.Where(t => t.TeamPlayerId == item.Id);
            db.TeamPlayersPayments.RemoveRange(payments);
            db.TeamsPlayers.Remove(item);
        }

        public IEnumerable<TeamsPlayer> GetAllPlayersForWeightLiftingRegistrationBySession(int? seasonId)
        {
            return db.TeamsPlayers
                .Where(tp => tp.IsActive == true
                                && tp.User.IsArchive == false
                                && tp.SeasonId == seasonId);
        }

        public List<TeamsPlayer> GetTeamPlayers(int teamId, int? seasonId, bool fromLeagueRegistration = false, int? leagueId = null)
        {
            if (seasonId == null) seasonId = db.TeamsPlayers
                    .Where(tp => tp.TeamId == teamId)
                    .Select(tp => tp.SeasonId).Max();
            DateTime? endRegistrationDate = null;

            if (leagueId != null)
            {
                endRegistrationDate = db.Leagues.FirstOrDefault(c => c.LeagueId == leagueId)?.EndRegistrationDate;
            }
            return !fromLeagueRegistration
                ? db.TeamsPlayers
                    .Include(tp => tp.User)
                    .Include(tp => tp.User.TeamsPlayers)
                    .Include(tp => tp.User.Gender)
                    .Where(tp => tp.TeamId == teamId
                                 && tp.LeagueId == leagueId
                                 && tp.IsActive == true
                                 && tp.User.IsArchive == false
                                 && tp.SeasonId == seasonId
                                 && tp.User.PenaltyForExclusions.Where(c => !c.IsCanceled).All(c => c.IsEnded))
                    .ToList()
                : db.TeamsPlayers
                    .Include(tp => tp.User)
                    .Include(tp => tp.User.TeamsPlayers)
                    .Include(tp => tp.User.Gender)
                    .Include(tp => tp.League)
                    .Where(tp => tp.TeamId == teamId
                                 && tp.User.IsArchive == false
                                 && tp.SeasonId == seasonId && !tp.WithoutLeagueRegistration && !tp.DateOfCreate.HasValue || tp.DateOfCreate.HasValue
                                 && tp.User.PenaltyForExclusions.Where(c => !c.IsCanceled).All(c => c.IsEnded) && tp.DateOfCreate <= endRegistrationDate)
                    .ToList();
        }

        public IEnumerable<TeamsPlayer> GetPlayersForTennisRegistrations(int teamId, int? seasonId, DateTime? endRegistrationDate)
        {
            return db.TeamsPlayers.Where(tp => !tp.WithoutLeagueRegistration && !tp.User.IsArchive && tp.TeamId == teamId && tp.SeasonId == seasonId
                   && !tp.User.PenaltyForExclusions.Where(c => !c.IsCanceled).Any(c => !c.IsEnded));
            // && tp.User.MedExamDate.HasValue && tp.User.MedExamDate.Value > DateTime.Now 
        }

        public IEnumerable<TeamPlayerItem> GetTeamPlayers(int teamId, int clubId, int leagueId, int seasonId)
        {
            var league = db.Leagues.Find(leagueId);
            var season = db.Seasons.Find(seasonId);

            var section = season?.Union != null ? season.Union.Section.Alias : null;

            var club = db.Clubs.Find(clubId);
            var IsHandicapEnabled = club?.Union?.IsHadicapEnabled;
            var isClubsBlocked = club?.Union?.IsClubsBlocked ?? false;

            var query = db.Users.SelectMany(user => user.TeamsPlayers, (user, teamsPlayer) => new { user, teamsPlayer })
                .Where(t => !t.user.IsArchive &&
                       t.teamsPlayer.TeamId == teamId &&
                       t.teamsPlayer.SeasonId == seasonId &&
                        (leagueId > 0 ? t.teamsPlayer.LeagueId == leagueId : t.teamsPlayer.LeagueId == null) &&
                        (clubId > 0 ? t.teamsPlayer.ClubId == clubId : t.teamsPlayer.ClubId == null));

            if (section != null && section == GamesAlias.Athletics && (league?.Union?.IsClubsBlocked ?? false))
            {
                query = query.Where(t => t.teamsPlayer.IsActive == true);
            }
            if (section != null && section == GamesAlias.Climbing && isClubsBlocked)
            {
                query = query.Where(t => t.teamsPlayer.IsActive == true);
            }
            var users = query.ToList();

            var usersIds = users.Select(x => x.user.UserId).ToArray();
            var playersActivitiesRegistrations = db.ActivityFormsSubmittedDatas
                .Where(x => usersIds.Contains(x.PlayerId) && x.Activity.SeasonId == seasonId)
                .OrderBy(x => x.ActivityId)
                .ToList();

            var playersFiles = db.PlayerFiles
                .Where(x => usersIds.Contains(x.PlayerId) && x.SeasonId == seasonId)
                .ToList();

            return users.Select(t =>
            {
                ActivityFormsSubmittedData registration = null;
                if ((section == null || section != GamesAlias.Athletics) && clubId > 0 && league == null)
                {
                    var clubRegistrations = playersActivitiesRegistrations
                        .Where(
                            x => x.PlayerId == t.user.UserId &&
                                 x.TeamId == t.teamsPlayer.TeamId &&
                                 x.ClubId == clubId &&
                                 x.Activity.SeasonId == seasonId)
                        .ToList();

                    registration = clubRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic == true && x.Activity.Type == ActivityType.Personal)
                                   ?? clubRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic != true && x.Activity.Type == ActivityType.Personal)
                                   ?? clubRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic == true && x.Activity.Type == ActivityType.UnionPlayerToClub);

                }
                else if (section == null || section != GamesAlias.Athletics)
                {
                    var leagueRegistrations = playersActivitiesRegistrations
                        .Where(
                            x => x.PlayerId == t.user.UserId &&
                                 x.TeamId == t.teamsPlayer.TeamId &&
                                 x.LeagueId == leagueId &&
                                 x.Activity.SeasonId == seasonId)
                        .ToList();

                    registration = leagueRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic == true && x.Activity.Type == ActivityType.Personal)
                                   ?? leagueRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic != true && x.Activity.Type == ActivityType.Personal)
                                   ?? leagueRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic == true && x.Activity.Type == ActivityType.UnionPlayerToClub);
                }

                decimal managerDiscount = 0M;
                if ((section == null || section != GamesAlias.Athletics) && clubId > 0)
                {
                    managerDiscount = t.user.PlayerDiscounts
                                          .FirstOrDefault(
                                              d => d.TeamId == teamId &&
                                                   d.ClubId == clubId &&
                                                   d.SeasonId == seasonId &&
                                                   d.DiscountType == (int)PlayerDiscountTypes.ManagerParticipationDiscount)
                                          ?.Amount ?? 0M;
                }
                else if (section == null || section != GamesAlias.Athletics)
                {
                    managerDiscount = t.user.PlayerDiscounts
                                          .FirstOrDefault(
                                              d => d.TeamId == teamId &&
                                                   d.LeagueId == leagueId &&
                                                   d.SeasonId == seasonId &&
                                                   d.DiscountType == (int)PlayerDiscountTypes.ManagerRegistrationDiscount)
                                          ?.Amount ?? 0M;
                }

                var unionId = league?.UnionId;

                var gymnasticCompetitionCount = (section == GamesAlias.Gymnastic) ? t.user.CompetitionRegistrations
                                    .Where(c => c.SeasonId == seasonId && !c.League.IsArchive && c.IsActive && (c.FinalScore.HasValue || c.Position.HasValue))
                                    .GroupBy(r => r.LeagueId)
                                    .Select(r => r.First())
                                    .Count() : 0;

                int regularCompetitionCount = (section == GamesAlias.WeightLifting) ? t.user.CompetitionDisciplineRegistrations
                    .Where(c => c.CompetitionDiscipline.League.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && c.IsApproved.HasValue && c.IsApproved.Value)
                    .GroupBy(r => r.CompetitionDiscipline.League.LeagueId)
                    .Select(r => r.First())
                    .Count() : 0;

                int athleticRegistrationCount = 0;
                if (section == GamesAlias.Athletics)
                {
                    int AlternativeResultInt = 3; // value for alternative result column if player did not start/show
                    athleticRegistrationCount = t.user.CompetitionDisciplineRegistrations
                    .Where(c => c.CompetitionDiscipline.League.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && !c.CompetitionDiscipline.IsDeleted && c.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(c.CompetitionResult.FirstOrDefault().Result) && c.CompetitionResult.FirstOrDefault().AlternativeResult != AlternativeResultInt)
                    .GroupBy(r => r.CompetitionDiscipline.League.LeagueId)
                    .Select(r => r.First())
                    .Count();
                }

                var otherCompetitionCount = t.user.SportsRegistrations
                    .Where(c => c.SeasonId == seasonId && !c.League.IsArchive && c.IsApproved || (c.FinalScore.HasValue || c.Position.HasValue))
                    .GroupBy(r => r.LeagueId)
                    .Select(r => r.First())
                    .Count();

                var competitionCount = gymnasticCompetitionCount + athleticRegistrationCount + regularCompetitionCount + otherCompetitionCount;
                if (section == GamesAlias.Tennis)
                {
                    competitionCount = t.teamsPlayer.CompetitionParticipationCount ?? 0;
                }

                var teamReg = db.TeamRegistrations.FirstOrDefault(tr => !tr.IsDeleted && tr.ClubId == clubId && tr.TeamId == teamId && !tr.League.IsArchive);
                var isLeagueConditionValid = !t.user.BirthDay.HasValue ? false : CheckLeagueConditionForTheTennis(teamReg, t.user.GenderId.HasValue ? t.user.GenderId.Value : 0, t.user.BirthDay.Value);

                var trainingTeams = GetTrainingTeamsOfPlayer(t.user.UserId, season?.UnionId ?? 0, seasonId);

                decimal _;
                var playerFiles = playersFiles.Where(x => x.PlayerId == t.user.UserId && x.SeasonId == seasonId).ToList();

                var isUnderPenalty = false;
                if (section == null || section != GamesAlias.Athletics)
                {
                    foreach (var penalty in t.user.PenaltyForExclusions.Where(c => !c.IsCanceled && !c.IsEnded))
                    {
                        if (string.IsNullOrWhiteSpace(penalty.LeagueIds))
                        {
                            isUnderPenalty = true;
                            break;
                        }
                        if (!string.IsNullOrWhiteSpace(penalty.LeagueIds) && t.teamsPlayer.LeagueId.HasValue)
                        {
                            var leagueIdsArr = penalty.LeagueIds.Split(',').Where(l => !string.IsNullOrEmpty(l) && int.TryParse(l, out int _)).Select(l => int.Parse(l)).ToList();
                            if (leagueIdsArr.Contains(t.teamsPlayer.LeagueId.Value))
                            {
                                isUnderPenalty = true;
                                break;
                            }
                        }
                    }
                }

                var res = new TeamPlayerItem
                {
                    Id = t.teamsPlayer.Id,
                    Weight = t.user.Weight,
                    IsApproveChecked = t.teamsPlayer.IsApprovedByManager == true,
                    IsNotApproveChecked = t.teamsPlayer.IsApprovedByManager == false,
                    WeightUnits = t.user.WeightUnits,
                    UserId = t.teamsPlayer.UserId,
                    ShirtNum = t.teamsPlayer.ShirtNum,
                    PosId = t.teamsPlayer.PosId,
                    FullName = t.user.FullName,
                    IdentNum = t.user.IdentNum,
                    PassportNum = t.user.PassportNum,
                    FirstName = !string.IsNullOrWhiteSpace(t.user.FirstName) ? t.user.FirstName : GetFirstNameByFullName(t.user.FullName),
                    LastName = !string.IsNullOrWhiteSpace(t.user.LastName) ? t.user.LastName : GetLastNameByFullName(t.user.FullName),
                    GenderId = t.user.GenderId,
                    Gender = t.user.Gender,
                    AthletesNumbers = t.user.AthleteNumbers.FirstOrDefault(x=>x.SeasonId == seasonId)?.AthleteNumber1,
                    IsActive = t.teamsPlayer.IsActive,
                    TeamId = teamId,
                    TeamName = t.teamsPlayer.Team?.TeamsDetails?.FirstOrDefault(c => c.SeasonId == seasonId)?.TeamName
                                ?? t.teamsPlayer.Team?.Title ?? "",
                    SeasonId = seasonId,
                    Birthday = t.user.BirthDay,
                    City = t.user.City,
                    Email = t.user.Email,
                    Insurance = t.user.Insurance,
                    InsuranceFile = playerFiles.Where(x => x.FileType == (int)PlayerFileType.Insurance)
                        .Select(x => x.FileName)
                        .FirstOrDefault(),
                    MedicalCertificate = t.user.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == seasonId)?.Approved == true,
                    MedicalCertificateFile = playerFiles.Where(x => x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive)
                        .Select(x => x.FileName)
                        .FirstOrDefault(),
                    ShirtSize = t.user.ShirtSize,
                    Telephone = t.user.Telephone,
                    AthleteNumber = t.user.AthleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId)?.AthleteNumber1,
                    IsLocked = t.teamsPlayer.IsLocked,

                    IsPlayerRegistered = registration != null,

                    IsPlayerRegistrationApproved = registration?.IsActive == true,

                    PlayerImage = playerFiles.Where(x => x.FileType == (int)PlayerFileType.PlayerImage)
                                      .Select(x => x.FileName)
                                      .FirstOrDefault() ?? t.user.Image,

                    Registration = registration,

                    ManagerParticipationDiscount = clubId > 0 ? managerDiscount : 0,
                    ManagerRegistrationDiscount = clubId <= 0 ? managerDiscount : 0,

                    NoInsurancePayment = t.user.NoInsurancePayment,
                    SeasonIdOfCreation = t.user.SeasonIdOfCreation,
                    IsTrainerPlayer = t.teamsPlayer.IsTrainerPlayer,
                    IsEscortPlayer = t.teamsPlayer.IsEscortPlayer,
                    IsApprovedByManager = t.teamsPlayer.IsApprovedByManager,
                    IsBlockaded = t.user?.BlockadeId != null,
                    StartPlaying = t.teamsPlayer.StartPlaying,
                    FinalHandicapLevel = IsHandicapEnabled.HasValue && IsHandicapEnabled.Value ? GetFinalHandicap(t.teamsPlayer, out _, seasonId, unionId, leagueId) : 0,
                    BaseHandicap = t.teamsPlayer.HandicapLevel,
                    Comment = t.teamsPlayer.Comment,
                    UnionComment = t.teamsPlayer.UnionComment,
                    TeamPlayerPaid = t.teamsPlayer.Paid,
                    IsExceptionalMoved = t.teamsPlayer.IsExceptionalMoved,                                                                                            
                    IsUnderPenalty = isUnderPenalty,
                    TennisPositionOrder = t.teamsPlayer.TennisPositionOrder,
                    NextTournamentRoster = t.teamsPlayer.NextTournamentRoster,
                    TenicardValidity = t.user.TenicardValidity,
                    MedExamDate = t.user.MedExamDate,
                    CompetitionCount = competitionCount,
                    IsNotMeetingRequirements = !isLeagueConditionValid,
                    TrainingClubName = string.Join(", ", trainingTeams.Select(x => x.Club?.Name).Where(x => x != null)),
                    IDFile = t.teamsPlayer.User.IDFile ?? playerFiles.FirstOrDefault(x =>
                        x.FileType == (int)PlayerFileType.IDFile && x.SeasonId == t.teamsPlayer.SeasonId)?.FileName,
                };

                return res;
            }).ToList();

        }

        public void ApproveAndActiveAllTeamplayersOfPlayerThisSeason(int userId, int? seasonId, int approval)
        {
            var teamPlayers = db.TeamsPlayers.Where(t => t.UserId == userId && !t.User.IsArchive && t.SeasonId == seasonId).ToList();

            var approvalDate = DateTime.Now;

            teamPlayers.ForEach(tp => { 
                tp.IsApprovedByManager = approval == 1 ? true : (approval == -1 ? false : (bool?)null);
                if (approval == 1)
                {
                    tp.IsActive = true;

                    if (tp.ApprovalDate == null)
                    {
                        tp.ApprovalDate = approvalDate;
                    }
                }
                else
                {
                    tp.ApprovalDate = null;
                }
            });
            Save();
        }

        public int[] GetTeamPlayersForCompetitionCount(int teamId, Season season, League league, int? clubId = null)
        {
            var section = season?.Union != null ? season.Union.Section.Alias : null;
            var query = db.Users.SelectMany(user => user.TeamsPlayers, (user, teamsPlayer) => new { user, teamsPlayer })
                .Where(t => !t.user.IsArchive && t.teamsPlayer.TeamId == teamId && t.teamsPlayer.SeasonId == season.Id &&
                        (league.LeagueId > 0 ? t.teamsPlayer.LeagueId == league.LeagueId : t.teamsPlayer.LeagueId == null) &&
                        (clubId > 0 ? t.teamsPlayer.ClubId == clubId : t.teamsPlayer.ClubId == null));

            var isClubsBlocked = season?.Union?.IsClubsBlocked ?? false;
            if (section != null && section == GamesAlias.Athletics && (league?.Union?.IsClubsBlocked ?? false))
            {
                query = query.Where(t => t.teamsPlayer.IsActive == true);
            }
            if (section != null && section == GamesAlias.Climbing && isClubsBlocked)
            {
                query = query.Where(t => t.teamsPlayer.IsActive == true);
            }
            var users = query.ToList();
            var usersIds = users.Select(x => x.user.UserId).ToArray();
            return usersIds;
        }
        public IEnumerable<TeamPlayerItem> GetTeamPlayersShort(int teamId, int clubId, League league, Season season)
        {


            var section = season.Union != null ? season.Union.Section.Alias : null;

            var club = db.Clubs.Find(clubId);
            var IsHandicapEnabled = club?.Union?.IsHadicapEnabled;
            var isClubsBlocked = club?.Union?.IsClubsBlocked ?? false;
            var query = db.Users.SelectMany(user => user.TeamsPlayers, (user, teamsPlayer) => new { user, teamsPlayer })
                .Where(t => !t.user.IsArchive &&
                       t.teamsPlayer.TeamId == teamId &&
                       t.teamsPlayer.SeasonId == season.Id &&
                        (league.LeagueId > 0 ? t.teamsPlayer.LeagueId == league.LeagueId : t.teamsPlayer.LeagueId == null) &&
                        (clubId > 0 ? t.teamsPlayer.ClubId == clubId : t.teamsPlayer.ClubId == null));

            if (section != null && (section == GamesAlias.Athletics || section == GamesAlias.Climbing) && isClubsBlocked)
            {
                query = query.Where(t => t.teamsPlayer.IsActive == true);
            }
            var users = query.ToList();

            var usersIds = users.Select(x => x.user.UserId).ToArray();
            var playersActivitiesRegistrations = db.ActivityFormsSubmittedDatas
                .Where(x => usersIds.Contains(x.PlayerId) && x.Activity.SeasonId == season.Id)
                .OrderBy(x => x.ActivityId)
                .ToList();

            var playersFiles = db.PlayerFiles
                .Where(x => usersIds.Contains(x.PlayerId) && x.SeasonId == season.Id)
                .ToList();


            return users.Select(t =>
            {
                var unionId = league?.UnionId;
                var playerFiles = playersFiles.Where(x => x.PlayerId == t.user.UserId && x.SeasonId == season.Id).ToList();
                var res = new TeamPlayerItem
                {
                    Id = t.teamsPlayer.Id,
                    Weight = t.user.Weight,
                    IsApproveChecked = t.teamsPlayer.IsApprovedByManager == true,
                    IsNotApproveChecked = t.teamsPlayer.IsApprovedByManager == false,
                    WeightUnits = t.user.WeightUnits,
                    UserId = t.teamsPlayer.UserId,
                    ShirtNum = t.teamsPlayer.ShirtNum,
                    PosId = t.teamsPlayer.PosId,
                    FullName = t.user.FullName,
                    IdentNum = t.user.IdentNum,
                    PassportNum = t.user.PassportNum,
                    FirstName = !string.IsNullOrWhiteSpace(t.user.FirstName) ? t.user.FirstName : GetFirstNameByFullName(t.user.FullName),
                    LastName = !string.IsNullOrWhiteSpace(t.user.LastName) ? t.user.LastName : GetLastNameByFullName(t.user.FullName),
                    GenderId = t.user.GenderId,
                    Gender = t.user.Gender,
                    AthletesNumbers = t.user.AthleteNumbers.FirstOrDefault(x => x.SeasonId == season.Id)?.AthleteNumber1,
                    IsActive = t.teamsPlayer.IsActive,
                    TeamId = teamId,
                    TeamName = t.teamsPlayer.Team?.TeamsDetails?.FirstOrDefault(c => c.SeasonId == season.Id)?.TeamName
                                ?? t.teamsPlayer.Team?.Title ?? "",
                    SeasonId = season.Id,
                    Birthday = t.user.BirthDay,
                    City = t.user.City,
                    Email = t.user.Email,
                    Insurance = t.user.Insurance,

                    ShirtSize = t.user.ShirtSize,
                    Telephone = t.user.Telephone,
                    AthleteNumber = t.user.AthleteNumbers.FirstOrDefault(x => x.SeasonId == season.Id)?.AthleteNumber1,
                    IsLocked = t.teamsPlayer.IsLocked,

                    NoInsurancePayment = t.user.NoInsurancePayment,
                    SeasonIdOfCreation = t.user.SeasonIdOfCreation,
                    IsTrainerPlayer = t.teamsPlayer.IsTrainerPlayer,
                    IsEscortPlayer = t.teamsPlayer.IsEscortPlayer,
                    IsApprovedByManager = t.teamsPlayer.IsApprovedByManager,
                    IsBlockaded = t.user?.BlockadeId != null,
                    StartPlaying = t.teamsPlayer.StartPlaying,
                    BaseHandicap = t.teamsPlayer.HandicapLevel,
                    Comment = t.teamsPlayer.Comment,
                    UnionComment = t.teamsPlayer.UnionComment,
                    TeamPlayerPaid = t.teamsPlayer.Paid,
                    IsExceptionalMoved = t.teamsPlayer.IsExceptionalMoved,
                    IsUnderPenalty = (section == null || section != GamesAlias.Athletics) && t.user.PenaltyForExclusions.Where(c => !c.IsCanceled).Any(c => !c.IsEnded),
                    TennisPositionOrder = t.teamsPlayer.TennisPositionOrder,
                    NextTournamentRoster = t.teamsPlayer.NextTournamentRoster,
                    TenicardValidity = t.user.TenicardValidity,
                    MedExamDate = t.user.MedExamDate
                };

                return res;
            }).ToList();

        }





        private bool CheckLeagueConditionForTheTennis(TeamRegistration teamReg, int genderId, DateTime birthdate)
        {
            if (teamReg == null)
                return false;
            var isValid = false;
            var leagueGenderId = teamReg.League.GenderId;
            var minAge = (teamReg.League.MinimumAge).GetAge();
            var maxAge = (teamReg.League.MaximumAge).GetAge();
            var currentAge = ((DateTime?)birthdate).GetAge().Value;

            var maxPlayers = teamReg.League.MaximumPlayersTeam;

            isValid = (leagueGenderId == 3 || genderId == leagueGenderId)
                && (!minAge.HasValue && !maxAge.HasValue
                || minAge.HasValue && maxAge.HasValue && minAge.Value <= currentAge && currentAge <= maxAge.Value
                || minAge.HasValue && !maxAge.HasValue && minAge.Value <= currentAge
                || !minAge.HasValue && maxAge.HasValue && currentAge <= maxAge.Value)
                && (maxPlayers == null
                || teamReg.Team.TeamsPlayers.Count(t => !t.User.IsArchive) < maxPlayers);

            return isValid;
        }

        public void RemoveInitialApprovalDate(InitialApprovalDate currentInitialApprovalDate)
        {
            db.InitialApprovalDates.Remove(currentInitialApprovalDate);
        }
        public void RemoveInitialApprovalDate(int id)
        {
            var currentInitialApprovalDate = db.InitialApprovalDates.FirstOrDefault(a => a.Id == id);
            db.InitialApprovalDates.Remove(currentInitialApprovalDate);
        }

        public void CreateInitialApprovalDate(int userId, DateTime approvalDate, int unionId)
        {
            db.InitialApprovalDates.Add(new InitialApprovalDate
            {
                UserId = userId,
                UnionId = unionId,
                InitialApprovalDate1 = approvalDate
            });
        }

        public InitialApprovalDate GetInitialApprovalDate(int userId)
        {
            return db.InitialApprovalDates.FirstOrDefault(ad => ad.UserId == userId);
        }

        public DateTime ChangeTeamsPlayerRegistrationStatus(int teamPlayerId, int managerId)
        {
            var teamsPlayer = db.TeamsPlayers.FirstOrDefault(p => p.Id == teamPlayerId);
            var datetime = DateTime.Now;
            teamsPlayer.IsApprovedByManager = true;
            teamsPlayer.ApprovalDate = datetime;
            teamsPlayer.ActionUserId = managerId;
            db.SaveChanges();
            return datetime;
        }

        public decimal GetFinalParticipationPrice(int userId, decimal participationPrice, decimal managerDiscount, int seasonId, int clubId)
        {
            var finalPartPrice = participationPrice;
            if (clubId > 0)
            {
                var playerDiscount = db.PlayerDiscounts.FirstOrDefault(pd => pd.ClubId == clubId && pd.SeasonId == seasonId && pd.PlayerId == userId);
                if (playerDiscount != null)
                {
                    finalPartPrice = Math.Max(0, participationPrice - managerDiscount);
                    playerDiscount.FinalParticipationPrice = finalPartPrice;
                    db.SaveChanges();
                }
            }
            return finalPartPrice;
        }

        private bool? SetPlayersLockedStatus(TeamsPlayer teamPlayer)
        {
            bool? playersLockedStatus = null;
            if (teamPlayer != null)
            {
                if (teamPlayer.IsLocked == null)
                {
                    var league = teamPlayer.League;
                    if (league != null)
                    {
                        var minAge = league.MinimumAge;
                        var maxAge = league.MaximumAge;
                        if (teamPlayer.User.BirthDay != null)
                        {
                            if (minAge != null && teamPlayer.User.BirthDay > minAge.Value)
                            {
                                playersLockedStatus = true;
                            }

                            if (maxAge != null && teamPlayer.User.BirthDay < maxAge)
                            {
                                playersLockedStatus = true;
                            }
                        }
                    }
                }
            }
            return playersLockedStatus;
        }

        public void ApproveAllPlayers(TeamsPlayer teamPlayer, int adminId)
        {
            var teamsPlayers = teamPlayer.User.TeamsPlayers;
            foreach (var teamPlayerItem in teamsPlayers)
            {
                teamPlayerItem.IsApprovedByManager = true;
                teamPlayerItem.ActionUserId = adminId;
                var dateOfApprove = DateTime.Now;
                if (teamPlayerItem.ApprovalDate == null)
                {
                    teamPlayerItem.ApprovalDate = dateOfApprove;
                }
            }
        }

        public bool DeleteTeamPlayer(int id, int? seasonId)
        {
            try
            {
                var playerToDelete = db.TeamsPlayers.FirstOrDefault(c => c.Id == id && c.SeasonId == seasonId);
                playerToDelete.User.IsArchive = true;
                db.SaveChanges();
                return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public bool RegistratePlayer(int userId, int teamId, int? seasonId)
        {
            try
            {
                var activity = new Activity
                {
                    IsAutomatic = true,
                    Type = ActivityType.Personal,
                    SeasonId = seasonId,
                };
                db.Activities.Add(activity);
                var registrationForm = new ActivityFormsSubmittedData
                {
                    PlayerId = userId,
                    TeamId = teamId,
                    ActivityId = 2,
                    DateSubmitted = DateTime.Now,
                    IsActive = true,
                    Activity = activity
                };
                db.ActivityFormsSubmittedDatas.Add(registrationForm);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public IDictionary<int, IList<AthleteShortDTO>> GetAthletesBySeasonAndLeagues(int seasonId, IEnumerable<int> leagueIds, bool allLeagues = false)
        {
            IEnumerable<int> groupIds = db.GamesCycles
                .Where(gc => allLeagues || leagueIds.Contains(gc.Stage.LeagueId) && gc.Stage.League.SeasonId == seasonId)
                .Where(gc => gc.GroupId.HasValue && !gc.Group.IsArchive)
                .Select(gc => gc.GroupId ?? 0)
                .Distinct();

            var result = new Dictionary<int, IList<AthleteShortDTO>>();
            foreach (var id in groupIds)
                result[id] = new List<AthleteShortDTO>();

            var groupTeams = db.TeamsPlayers
                .Join(db.GroupsTeams, t => t.Id, gt => gt.AthleteId, (t, gt) => gt).Include(gt => gt.TeamsPlayer)
                .Where(gt => !gt.Group.IsArchive).Distinct();
            foreach (var gt in groupTeams)
            {
                if (!result.Keys.Contains(gt.GroupId))
                {
                    result[gt.GroupId] = new List<AthleteShortDTO>();
                }
                var athleteDetails = gt.TeamsPlayer.User;
                var athleteName = $"{athleteDetails?.FullName} ({db.TeamsPlayers.Find(gt.TeamsPlayer.Id).Team.Title})";
                result[gt.GroupId].Add(new AthleteShortDTO
                {
                    Pos = gt.Pos,
                    AthleteId = gt.AthleteId,
                    Title = athleteName
                });
            }
            return result;
        }

        public IEnumerable<KeyValuePair<string, string>> GetAllApprovedTeams(int userId, int teamId, int? leagueId, int? seasonId)
        {
            var teamsPlayersApproved = db.TeamsPlayers.Where(tp => tp.UserId == userId
                    && tp.IsActive
                    && !tp.User.IsArchive
                    && tp.SeasonId == seasonId
                    && tp.LeagueId != leagueId
                    && tp.IsApprovedByManager == true);
            if (teamsPlayersApproved.Any())
            {
                foreach (var teamPlayer in teamsPlayersApproved)
                {
                    var teamName = teamPlayer.Team?.TeamsDetails?.OrderByDescending(c => c.SeasonId == seasonId)?.FirstOrDefault()?.TeamName
                        ?? teamPlayer?.Team?.Title ?? string.Empty;
                    yield return new KeyValuePair<string, string>(teamName, teamPlayer?.League?.Name);
                }
            }
        }

        public IEnumerable<TeamPlayerItem> GetTeamPlayersDTOByIds(int[] playerIds, int? seasonId = null)
        {
            var players = new List<TeamPlayerItem>();
            var playersDb = GetPlayersByIds(playerIds);
            var userIds = playersDb.Select(tp => tp.UserId).ToList();
            var allPlayerImages = db.PlayerFiles.Where(p => userIds.Contains(p.PlayerId) && p.SeasonId == seasonId && p.FileType == (int)PlayerFileType.PlayerImage).ToList();

            foreach (var playerDb in playersDb)
            {
                if (playerDb != null)
                {
                    players.Add(new TeamPlayerItem
                    {
                        Id = playerDb.Id,
                        UserId = playerDb.UserId,
                        ShirtNum = playerDb.ShirtNum,
                        PosId = playerDb.PosId,
                        FullName = playerDb.User.FullName,
                        IdentNum = playerDb.User.IdentNum,
                        IsActive = playerDb.IsActive,
                        TeamId = playerDb.TeamId,
                        Birthday = playerDb.User.BirthDay,
                        Weight = playerDb.User.Weight,
                        WeightUnits = playerDb.User.WeightUnits,
                        City = playerDb.User.City,
                        Email = playerDb.User.Email,
                        PlayerImage = allPlayerImages.Where(f => f.PlayerId == playerDb.UserId)
                                          .Select(x => x.FileName)
                                          .FirstOrDefault() ?? playerDb.User.Image,
                    });
                }
            }
            return players;
        }

        public User GetUserByPassportNum(string passportNum)
        {
            return db.Users.FirstOrDefault(t => t.PassportNum == passportNum);
        }

        public User GetUserByPassportNumNotArchived(string passportNum)
        {
            return db.Users.FirstOrDefault(t => t.PassportNum == passportNum && !t.IsArchive);
        }

        public List<TeamsPlayer> GetGroupAthletes(int leagueId, int stageId)
        {
            var nonIncludedAthletes = new List<TeamsPlayer>();
            var groups = db.Groups.Where(s => s.StageId == stageId && s.IsArchive == false && s.IsIndividual == true).ToList();
            if (groups.Count != 0)
            {
                foreach (var group in groups)
                {
                    nonIncludedAthletes.AddRange(db.GroupsTeams.Where(c => c.GroupId == group.GroupId)
                        .Select(c => c.TeamsPlayer));
                }
            }
            else
                return null;
            return nonIncludedAthletes;
        }

        public List<TeamsPlayer> GetTennisPlayers(int leagueId, int categoryId, int seasonId)
        {
            var nonIncludedAthletes = db.TeamsPlayers
                .Where(x => x.LeagueId == leagueId && x.TeamId == categoryId && x.SeasonId == seasonId)
                .Include(x => x.User.TennisRanks)
                .ToList();

            var rankedPlayers = nonIncludedAthletes
                .Where(x => x.User.TennisRanks.FirstOrDefault()?.Rank > 0)
                .OrderBy(x => x.User.TennisRanks.FirstOrDefault()?.Rank)
                .ToList();

            var unrankedPlayers = nonIncludedAthletes
                .Where(x => x.User.TennisRanks.FirstOrDefault()?.Rank < 1 ||
                            x.User.TennisRanks.FirstOrDefault()?.Rank == null)
                .ToList();

            rankedPlayers.AddRange(unrankedPlayers);

            return rankedPlayers;
        }

        public decimal CreateTeamsPaymentsForTennisLeagues(IEnumerable<TeamRegistration> regisrations, int id)
        {
            decimal totalcost = 0.0m;
            foreach (var regisration in regisrations)
            {
                decimal? priceOfTeamRegistration = regisration.League.LeaguesPrices.FirstOrDefault(p => p.PriceType == 1)?.Price;
                if (priceOfTeamRegistration.HasValue)
                {
                    totalcost += priceOfTeamRegistration.Value;
                }
                db.TeamRegistrationPayments.Add(new TeamRegistrationPayment
                {
                    ClubBalanceId = id,
                    TeamRegistrationId = regisration.Id,
                    Fee = priceOfTeamRegistration
                });
            }
            Save();
            return totalcost;
        }

        public decimal CreatePlayersPaymentsForITenniCard(List<TeamsPlayer> players, int id)
        {
            foreach (var player in players)
            {
                db.TeamPlayersPayments.Add(new TeamPlayersPayment
                {
                    ClubBalanceId = id,
                    TeamPlayerId = player.Id,
                    Fee = 60.0m,
                    Validity = player.User.TenicardValidity
                });
            }
            Save();
            return 60.0m * players.Count();
        }

        public List<TeamPlayersPayment> GetTeamPlayersInPaymentReport(int clubBalanceId)
        {
            return db.TeamPlayersPayments.Where(tpp => tpp.ClubBalanceId == clubBalanceId).ToList();
        }

        public List<TeamRegistrationPayment> GetTeamRegistrationsInPaymentReport(int clubBalanceId)
        {
            return db.TeamRegistrationPayments.Where(tpp => tpp.ClubBalanceId == clubBalanceId).ToList();
        }


        public List<TeamsPlayer> GetAthletesOfTheGroup(int groupId)
        {
            var groupAthletesIds = db.GroupsTeams.Where(t => t.GroupId == groupId && t.AthleteId.HasValue)
                .Select(c => c.AthleteId);
            var listOfPlayers = new List<TeamsPlayer>();
            foreach (var id in groupAthletesIds)
                listOfPlayers.Add(db.TeamsPlayers.Find(id));
            return listOfPlayers;
        }

        public IEnumerable<TennisPlayerWithPair> GetTennisPlayersOfTheGroup(int groupId)
        {
            var groupPlayers = db.TennisGroupTeams.Where(t => t.GroupId == groupId && (t.PlayerId.HasValue || t.PairPlayerId.HasValue));
            foreach (var groupPlayer in groupPlayers)
            {
                yield return new TennisPlayerWithPair
                {
                    Name = groupPlayer?.TeamsPlayer?.User?.FullName + (groupPlayer.PairPlayerId.HasValue
                        ? $" / { groupPlayer.TeamsPlayer1.User.FullName}"
                        : string.Empty),
                    Value = groupPlayer?.PlayerId?.ToString() + (groupPlayer.PairPlayerId.HasValue
                        ? $"/{groupPlayer?.PairPlayerId?.ToString()}"
                        : string.Empty),
                };
            }
        }

        public bool PlayerExistsInTeam(int teamId, int userId, int? seasonId, int? leagueId, int? clubId)
        {
            return PlayerExistsInAnyTeam(new[] { teamId }, userId, seasonId, leagueId, clubId);
        }

        public bool PlayerExistsInAnyTeam(int[] teamsIds, int userId, int? seasonId, int? leagueId, int? clubId)
        {
            return db.TeamsPlayers.Any(tp => teamsIds.Contains(tp.TeamId) &&
                                             tp.UserId == userId &&
                                             tp.SeasonId == seasonId &&
                                             (leagueId != null ? tp.LeagueId == leagueId : tp.LeagueId == null) &&
                                             (clubId != null && leagueId == null ? tp.ClubId == clubId : tp.ClubId == null));
        }

        public bool ShirtNumberExists(int teamId, int shirtNum, int seasonId, int? leagueId, int? clubId)
        {
            return db.TeamsPlayers.Any(tp => tp.TeamId == teamId &&
                                             tp.ShirtNum == shirtNum &&
                                             tp.SeasonId == seasonId &&
                                             (leagueId != null ? tp.LeagueId == leagueId : tp.LeagueId == null) &&
                                             (clubId != null && leagueId == null ? tp.ClubId == clubId : tp.ClubId == null));
        }

        public bool ShirtNumberExists(int teamId, int shirtNum, int updateId, int seasonId, int? leagueId, int? clubId)
        {
            return db.TeamsPlayers.Any(tp => tp.Id != updateId &&
                                             tp.TeamId == teamId &&
                                             tp.SeasonId == seasonId &&
                                             tp.ShirtNum == shirtNum &&
                                             (leagueId != null ? tp.LeagueId == leagueId : tp.LeagueId == null) &&
                                             (clubId != null && leagueId == null ? tp.ClubId == clubId : tp.ClubId == null));
        }

        public object GetTeamPlayerByPassportAndTeamId(string passportNum, int? teamId)
        {
            return db.TeamsPlayers.FirstOrDefault(t => t.User.PassportNum == passportNum && t.TeamId == teamId);
        }

        public List<TeamsPlayer> GetTeamPlayersByPassport(string passportNum)
        {
            return db.TeamsPlayers.Where(t => t.User.PassportNum == passportNum).ToList();
        }

        public List<TeamsPlayer> GetTeamPlayersWithInvalidTeniCards(List<Team> teams, int clubId, int seasonId)
        {
            List<TeamsPlayer> players = new List<TeamsPlayer>();
            foreach (var team in teams)
            {
                players.AddRange(team.TeamsPlayers.Where(t => t.ClubId == clubId && t.SeasonId == seasonId && !t.User.IsArchive && (!t.User.TenicardValidity.HasValue || t.User.TenicardValidity.Value < DateTime.Now)));
            }
            return players;
        }

        public void MovePlayersToTeam(int newTeamId, int newLeagueId, int newClubId, int[] playerIds, int currentTeamId,
            int seasonId, int leagueId, int clubId, int managerId, bool copyPlayers = false)
        {
            var season = db.Seasons.Find(seasonId);
            var section = season?.Union?.Section
                          ?? season?.Club?.Section
                          ?? season?.Club?.Union?.Section;
            var isCompetition = false;
            List<TeamsPlayer> teamPlayers;
            if (leagueId > 0)
            {
                teamPlayers = db.TeamsPlayers.Where(x => x.TeamId == currentTeamId &&
                                                         playerIds.Contains(x.UserId) &&
                                                         x.SeasonId == seasonId &&
                                                         x.Team.LeagueTeams.Where(lt => lt.SeasonId == seasonId)
                                                             .Select(lt => lt.LeagueId).Contains(x.LeagueId ?? 0) &&
                                                         x.ClubId == null)
                    .ToList();
            }
            else
            {
                teamPlayers = db.TeamsPlayers.Where(x => x.TeamId == currentTeamId &&
                                                         playerIds.Contains(x.UserId) &&
                                                         x.SeasonId == seasonId &&
                                                         x.LeagueId == null &&
                                                         (clubId > 0 && leagueId <= 0
                                                             ? x.ClubId == clubId
                                                             : x.ClubId == null))
                    .ToList();
            }

            var movedPlayers = new List<TeamsPlayer>();

            if (newLeagueId > 0)
            {
                var addToLeagues = db.LeagueTeams.Where(x => x.TeamId == newTeamId && x.SeasonId == seasonId)
                    .Select(x => x.Leagues);

                var newLeague = db.Leagues.FirstOrDefault(l => l.LeagueId == newLeagueId);
                if (newLeague != null && newLeague.EilatTournament != null && newLeague.EilatTournament == true)
                {
                    isCompetition = false;
                }
                foreach (var addToLeague in addToLeagues)
                {
                    foreach (var playerId in playerIds)
                    {
                        var existingPlayer =
                            teamPlayers.FirstOrDefault(x => x.LeagueId == leagueId && x.UserId == playerId);
                        var newTeamPlayer = new TeamsPlayer
                        {
                            TeamId = newTeamId,
                            LeagueId = addToLeague.LeagueId,
                            ClubId = null,
                            UserId = playerId,
                            PosId = existingPlayer?.PosId,
                            ShirtNum = existingPlayer?.ShirtNum ?? 0,
                            IsActive = existingPlayer?.IsActive ?? false,
                            SeasonId = seasonId,
                            //MedExamDate = existingPlayer?.MedExamDate,
                            StartPlaying = existingPlayer?.StartPlaying,
                            HandicapLevel = existingPlayer?.HandicapLevel ?? 0,
                            IsApprovedByManager = section?.IsIndividual == true
                                ? existingPlayer?.IsApprovedByManager
                                : null,
                            ApprovalDate = section?.IsIndividual == true
                                ? existingPlayer?.ApprovalDate
                                : null
                        };

                        var nationalTeamInvitements = existingPlayer != null
                            ? db.NationalTeamInvitements.Where(x => x.TeamPlayerId == existingPlayer.Id).AsNoTracking()
                                .ToList()
                            : new List<NationalTeamInvitement>();
                        foreach (var nationalTeamInvitement in nationalTeamInvitements)
                        {
                            nationalTeamInvitement.TeamsPlayer = newTeamPlayer;
                        }

                        newTeamPlayer.NationalTeamInvitements = nationalTeamInvitements;

                        //set new team's disciplines to player
                        if (existingPlayer?.User.PlayerDisciplines.Any() == true)
                        {
                            var playerDisciplines = existingPlayer.User.PlayerDisciplines.ToList();
                            var teamDisciplines =
                                db.TeamDisciplines.Where(x => x.TeamId == newTeamId && x.SeasonId == seasonId)
                                    .AsNoTracking().ToList();

                            var newDisciplines = teamDisciplines.Where(x =>
                                !playerDisciplines.Select(p => p.DisciplineId).Contains(x.DisciplineId));
                            foreach (var newDiscipline in newDisciplines)
                            {
                                db.PlayerDisciplines.Add(new PlayerDiscipline
                                {
                                    DisciplineId = newDiscipline.DisciplineId,
                                    SeasonId = newDiscipline.SeasonId,
                                    ClubId = newDiscipline.ClubId,
                                    PlayerId = existingPlayer.UserId
                                });
                            }
                        }
                        if (section.Alias == GamesAlias.Tennis && !isCompetition)
                        {
                            var players = GetTeamPlayers(newTeamId, newClubId, newLeagueId, seasonId).OrderBy(r => r.TennisPositionOrder ?? int.MaxValue).ThenBy(r => r.FullName);
                            int pos = 1;
                            foreach (var player in players)
                            {
                                var tp = db.TeamsPlayers.FirstOrDefault(t => t.Id == player.Id);
                                tp.TennisPositionOrder = pos;
                                pos++;
                            }
                            newTeamPlayer.TennisPositionOrder = pos;
                        }

                        db.TeamsPlayers.Add(newTeamPlayer);
                        movedPlayers.Add(newTeamPlayer);
                    }
                }

                db.SaveChanges();
            }
            else
            {
                foreach (var teamPlayer in teamPlayers)
                {
                    var newTeamPlayer = new TeamsPlayer
                    {
                        TeamId = newTeamId,
                        LeagueId = null,
                        ClubId = newClubId <= 0 || newLeagueId > 0 ? (int?)null : newClubId,
                        UserId = teamPlayer.UserId,
                        PosId = teamPlayer.PosId,
                        ShirtNum = teamPlayer.ShirtNum,
                        IsActive = teamPlayer.IsActive,
                        SeasonId = teamPlayer.SeasonId ?? seasonId,
                        //MedExamDate = teamPlayer.MedExamDate,
                        StartPlaying = teamPlayer.StartPlaying,
                        HandicapLevel = teamPlayer.HandicapLevel,
                        IsApprovedByManager = section?.IsIndividual == true
                            ? teamPlayer.IsApprovedByManager
                            : null,
                        ApprovalDate = section?.IsIndividual == true
                            ? teamPlayer.ApprovalDate
                            : null
                    };

                    var nationalTeamInvitements = db.NationalTeamInvitements.Where(x => x.TeamPlayerId == teamPlayer.Id)
                        .AsNoTracking().ToList();
                    foreach (var nationalTeamInvitement in nationalTeamInvitements)
                    {
                        nationalTeamInvitement.TeamsPlayer = newTeamPlayer;
                    }
                    newTeamPlayer.NationalTeamInvitements = nationalTeamInvitements;

                    var teamPlayerStatistics =
                        db.Statistics
                            .Where(x => x.PlayerId == teamPlayer.Id)
                            .AsNoTracking()
                            .ToList();
                    foreach (var teamPlayerStatistic in teamPlayerStatistics)
                    {
                        teamPlayerStatistic.TeamsPlayer = newTeamPlayer;
                    }
                    newTeamPlayer.Statistics = teamPlayerStatistics;

                    var teamPlayerGameStatistics =
                        db.GameStatistics
                            .Where(x => x.PlayerId == teamPlayer.Id)
                            .AsNoTracking()
                            .ToList();
                    foreach (var teamPlayerGameStatistic in teamPlayerGameStatistics)
                    {
                        teamPlayerGameStatistic.TeamsPlayer = newTeamPlayer;
                    }
                    newTeamPlayer.GameStatistics = teamPlayerGameStatistics;

                    var teamPlayerWGameStatistics =
                        db.WaterpoloStatistics
                            .Where(x => x.PlayerId == teamPlayer.Id)
                            .AsNoTracking()
                            .ToList();
                    foreach (var teamPlayerWGameStatistic in teamPlayerWGameStatistics)
                    {
                        teamPlayerWGameStatistic.TeamsPlayer = newTeamPlayer;
                    }
                    newTeamPlayer.WaterpoloStatistics = teamPlayerWGameStatistics;

                    if (section.Alias == GamesAlias.Tennis && !isCompetition)
                    {
                        var players = GetTeamPlayers(newTeamId, newClubId, newLeagueId, seasonId).OrderBy(r => r.TennisPositionOrder ?? int.MaxValue).ThenBy(r => r.FullName);
                        int pos = 1;
                        foreach (var player in players)
                        {
                            var tp = db.TeamsPlayers.FirstOrDefault(t => t.Id == player.Id);
                            tp.TennisPositionOrder = pos;
                            pos++;
                        }
                        newTeamPlayer.TennisPositionOrder = pos;
                    }



                    //set new team's disciplines to player
                    //    if (teamPlayer.User.PlayerDisciplines.Any())
                    //{
                    //    var playerDisciplines = teamPlayer.User.PlayerDisciplines.ToList();
                    //    var teamDisciplines =
                    //        db.TeamDisciplines.Where(x => x.TeamId == newTeamId && x.SeasonId == seasonId).AsNoTracking().ToList();

                    //    var newDisciplines = teamDisciplines.Where(x => !playerDisciplines.Select(p => p.DisciplineId).Contains(x.DisciplineId));
                    //    foreach (var newDiscipline in newDisciplines)
                    //    {
                    //        db.PlayerDisciplines.Add(new PlayerDiscipline
                    //        {
                    //            DisciplineId = newDiscipline.DisciplineId,
                    //            SeasonId = newDiscipline.SeasonId,
                    //            ClubId = newDiscipline.ClubId,
                    //            PlayerId = teamPlayer.UserId
                    //        });
                    //    }
                    //}
                    if (teamPlayer.User.PlayerDisciplines.Any())
                    {
                        var oldPlayerDisciplines = teamPlayer.User.PlayerDisciplines.Where(pd => pd.SeasonId == seasonId).ToList();
                        db.PlayerDisciplines.RemoveRange(oldPlayerDisciplines);
                    }

                    SetDisciplinesForTheGymnastic(newTeamId, newClubId, seasonId, teamPlayer.User);

                    db.TeamsPlayers.Add(newTeamPlayer);
                    db.SaveChanges();
                    movedPlayers.Add(newTeamPlayer);
                }
            }

            foreach (var teamsPlayer in teamPlayers)
            {
                if (!copyPlayers)
                {
                    db.NationalTeamInvitements.RemoveRange(teamsPlayer.NationalTeamInvitements);
                }

                if (teamsPlayer.IsApprovedByManager == true && teamsPlayer.ApprovalDate.HasValue)
                {
                    var teamName = teamsPlayer.Team.TeamsDetails?.Where(td => td.SeasonId == seasonId)
                                       ?.OrderByDescending(td => td.Id)?.FirstOrDefault()?.TeamName
                                   ?? teamsPlayer.Team.Title;
                    db.UsersApprovalDatesHistories.Add(new UsersApprovalDatesHistory
                    {
                        TeamName = teamName,
                        UserId = teamsPlayer.UserId,
                        ManagerApprovalDate = teamsPlayer.ApprovalDate.Value,
                        ActionUserId = managerId,
                        SeasonId = seasonId
                    });
                }
            }

            db.SaveChanges();

            if (!copyPlayers)
            {
                foreach (var teamsPlayer in teamPlayers)
                {
                    db.Statistics.RemoveRange(teamsPlayer.Statistics);
                    db.GameStatistics.RemoveRange(teamsPlayer.GameStatistics);
                    db.WaterpoloStatistics.RemoveRange(teamsPlayer.WaterpoloStatistics);
                    db.TeamPlayersPayments.RemoveRange(db.TeamPlayersPayments.Where(t => t.TeamPlayerId == teamsPlayer.Id)
                        .ToList());
                }

                db.SaveChanges();

                db.TeamsPlayers.RemoveRange(teamPlayers);

                db.SaveChanges();
            }

            if (movedPlayers.Count > 0 && !copyPlayers)
            {
                foreach (var movedPlayer in movedPlayers)
                {
                    var history = new PlayerHistory
                    {
                        SeasonId = movedPlayer.SeasonId ?? seasonId,
                        TeamId = movedPlayer.TeamId,
                        UserId = movedPlayer.UserId,
                        TimeStamp = DateTime.Now.Ticks,
                        OldTeamId = currentTeamId,
                        ActionUserId = managerId
                    };

                    db.PlayerHistory.Add(history);
                }

                db.SaveChanges();
            }

            //update registrations
            var registrations = db.ActivityFormsSubmittedDatas.Where(x => x.TeamId == currentTeamId &&
                                                                          playerIds.Contains(x.PlayerId) &&
                                                                          x.Activity.SeasonId == seasonId &&
                                                                          (leagueId > 0
                                                                              ? x.LeagueId == leagueId
                                                                              : x.LeagueId == null) &&
                                                                          (clubId > 0
                                                                              ? x.ClubId == clubId
                                                                              : x.ClubId == null))
                .ToList();
            foreach (var registration in registrations)
            {
                if (copyPlayers)
                {
                    db.Entry(registration).State = EntityState.Added;
                }
                else
                {
                    db.Entry(registration).State = EntityState.Modified;
                }

                registration.TeamId = newTeamId;
                if (newLeagueId > 0)
                {
                    registration.LeagueId = newLeagueId;
                }

                if (newClubId > 0)
                {
                    registration.ClubId = newClubId;
                }
            }

            if (registrations.Any())
            {
                db.SaveChanges();
            }
        }

        public int CheckCompetitionRegistrationsCount(int? clubId, int leagueId, int seasonId, int? competitionRouteId = null, int? teamId = null)
        {
            var playersRegistrationsWithoutInstruments = GetPlayersRegistrationsWithoutInstruments(clubId, leagueId, seasonId, competitionRouteId);

            var playersRegistrationsWithInstruments = GetPlayersRegistrationsWithInstruments(clubId, leagueId, seasonId, competitionRouteId);

            var teamPlayersRegistrationsWithoutInstruments = GetTeamPlayersRegistrationsWithoutInstruments(clubId, leagueId, seasonId, competitionRouteId);
            var teamPlayersRegistrationsWithInstruments = GetTeamPlayersRegistrationsWithInstruments(clubId, leagueId, seasonId, competitionRouteId);


            var playersAll = playersRegistrationsWithoutInstruments.Union(playersRegistrationsWithInstruments)
                    .OrderBy(c => c.Route)
                    .ThenBy(c => c.Rank)
                    .ThenBy(c => c.FullName);

            var teamPlayersAll = teamPlayersRegistrationsWithoutInstruments.Union(teamPlayersRegistrationsWithInstruments)
                    .OrderBy(c => c.Route)
                    .ThenBy(c => c.Rank)
                    .ThenBy(c => c.FullName);
            var allPlayersTogether = playersAll.Concat(teamPlayersAll);
            return allPlayersTogether?.Count() ?? 0;
        }

        public bool IsCorrectAge(Age age, DateTime? birthDay)
        {
            var isCorrect = false;
            if (birthDay.HasValue && age != null)
            {
                var today = DateTime.Today;
                var currentAge = today.Year - birthDay.Value.Year;
                if (birthDay > today.AddYears(-currentAge)) currentAge--;
                if (age.FromAge <= currentAge && currentAge <= age.ToAge) isCorrect = true;
            }
            return isCorrect;
        }

        public void SetDisciplinesForTheGymnastic(int teamId, int? clubId, int seasonId, User user)
        {
            var teamDisciplines = db.TeamDisciplines.Where(t => t.TeamId == teamId && t.ClubId == clubId && t.SeasonId == seasonId);
            foreach (var teamDiscipline in teamDisciplines)
            {
                user.PlayerDisciplines.Add(new PlayerDiscipline
                {
                    PlayerId = user.UserId,
                    ClubId = clubId.Value,
                    DisciplineId = teamDiscipline.DisciplineId,
                    SeasonId = seasonId
                });
            }
        }

        public Club GetClubOfPlayer(int userId)
        {
            return db.TeamsPlayers.Include(x => x.Club).FirstOrDefault(x => x.ClubId != null && x.UserId == userId)?.Club;
        }

        public List<TeamsPlayer> GetTeamPlayersByUnionIdShort(int unionId, int seasonId)
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

        public List<PlayerViewModel> GetTeamPlayersByUnionIdShortView(int unionId, int seasonId, string seasonAlias)
        {
            return GetPlayersViewModel(db.TeamsPlayers
                .Include(x => x.Team)
                .Include(x => x.Team.LeagueTeams)
                .Where(tp => !tp.User.IsArchive &&
                             !tp.Team.IsArchive &&
                             tp.SeasonId == seasonId &&
                             tp.League.UnionId == unionId)
                .ToList(), seasonId, seasonAlias);
        }

        public List<PlayerViewModel> GetTeamPlayersByTeamIdsShort(int teamId, int leagueId, int seasonId, string sectionAlias)
        {
            var playersBase = db.TeamsPlayers
                .Where(tp => tp.TeamId == teamId && tp.LeagueId == leagueId &&
                             tp.User.IsArchive == false &&
                             tp.User.IsActive == true &&
                             tp.SeasonId == seasonId)?.ToList();

            return GetPlayersViewModelShort(playersBase, seasonId, sectionAlias);
        }

        public TeamsPlayer GetTeamsPlayerById(int teamPlayerId)
        {
            return db.TeamsPlayers.FirstOrDefault(tp => tp.Id == teamPlayerId);
        }

        public List<PlayerViewModel> GetSchoolTeamPlayersByClubId(int id, int seasonId, string sectionAlias)
        {
            var shoolTeams = db.SchoolTeams
                .Where(st => st.School.ClubId == id && st.School.SeasonId == seasonId)
                .AsNoTracking()
                .ToList();

            var result = GetSchoolTeamPlayersByClubTeamIds(shoolTeams, seasonId, sectionAlias);
            return result;
        }

        private List<PlayerViewModel> GetSchoolTeamPlayersByClubTeamIds(List<SchoolTeam> shoolTeams, int seasonId, string sectionAlias)
        {
            using (var ctx = new DataEntities())
            {
                ctx.Configuration.LazyLoadingEnabled = false;

                var clubTeamIds = shoolTeams
                    .Select(c => c.TeamId)
                    .Distinct()
                    .ToList();

                Expression<Func<TeamsPlayer, bool>> tpExpression =
                    tp => clubTeamIds.Contains(tp.TeamId) &&
                          tp.User.IsArchive == false &&
                          tp.SeasonId == seasonId &&
                          (!tp.Team.LeagueTeams.Any() || tp.ClubId == null);

                ctx.Genders.Load();

                ctx.Users
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.UserId, x => x.UserId, (user, tp) => user)
                    .Load();

                ctx.ActivityFormsSubmittedDatas
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.PlayerId, x => x.UserId, (reg, tp) => reg)
                    //.Where(x => x.Activity.SeasonId == seasonId)
                    .Load();

                ctx.Leagues
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.LeagueId, x => x.LeagueId, (league, tp) => league)
                    .Load();

                ctx.Teams
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.TeamId, x => x.TeamId, (team, tp) => team)
                    .Load();

                ctx.ClubTeams
                    .Where(x => x.SeasonId == seasonId)
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.TeamId, x => x.TeamId, (club, tp) => club)
                    .Load();

                ctx.LeagueTeams
                    .Where(x => x.SeasonId == seasonId)
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.TeamId, x => x.TeamId, (lt, tp) => lt)
                    .Load();

                ctx.TeamsDetails
                    .Where(x => x.SeasonId == seasonId)
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.TeamId, x => x.TeamId, (details, tp) => details)
                    .Load();

                ctx.UsersRoutes
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.UserId, x => x.UserId, (route, tp) => route)
                    .Load();

                ctx.UsersRanks
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.UserId, x => x.UserId, (rank, tp) => rank)
                    .Load();

                ctx.PlayerAchievements
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.PlayerId, x => x.UserId, (achievement, tp) => achievement)
                    .Load();

                ctx.PlayerFiles
                    .Where(x => x.SeasonId == seasonId)
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.PlayerId, x => x.UserId, (file, tp) => file)
                    .Load();

                ctx.PlayerDisciplines
                    .Where(x => x.SeasonId == seasonId)
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.PlayerId, x => x.UserId, (discipline, tp) => discipline)
                    .Load();

                ctx.PlayersBlockades
                    .Where(x => x.SeasonId == seasonId)
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.UserId, x => x.UserId, (block, tp) => block)
                    .Load();

                ctx.TeamsPlayers
                    .Where(x => x.SeasonId == seasonId)
                    .Join(ctx.TeamsPlayers.Where(tpExpression), x => x.UserId, x => x.UserId, (block, tp) => block)
                    .Load();

                ctx.Positions.Load();

                var playersBase = ctx.TeamsPlayers
                    //.Include(tp => tp.User)
                    .Include(tp => tp.User.ActivityFormsSubmittedDatas)
                    .Include(tp => tp.Season)
                    //.Include(tp => tp.Team)
                    //.Include(tp => tp.Team.ClubTeams)
                    //.Include(tp => tp.Team.LeagueTeams)
                    //.Include(tp => tp.Team.TeamsDetails)
                    //.Include(tp => tp.User.Gender)
                    //.Include(tp => tp.User.UsersRoutes)
                    //.Include(tp => tp.User.UsersRanks)
                    //.Include(tp => tp.User.PlayerAchievements)
                    //.Include(tp => tp.User.PlayerFiles)
                    //.Include(tp => tp.User.PlayerDisciplines)
                    //.Include(tp => tp.User.PlayersBlockade)
                    //.Include(tp => tp.User.TeamsPlayers)
                    //.Include(tp => tp.Position)
                    .Where(tpExpression)
                    .ToList()
                    .OrderBy(x => x.User.FullName)
                    .ToList();

                return GetPlayersViewModel(playersBase, seasonId, sectionAlias);
            }
        }

        public void ChangeTennisCompetitionPlayerActiveStatus(int regId, bool isActive)
        {
            var registration = db.CompetitionRegistrations.Find(regId);
            if (registration != null)
            {
                registration.IsActive = isActive;
                db.SaveChanges();
            }
        }

        public IEnumerable<PlayerRegistrationDto> LoadExcelTennisCompetitionRegistrations(int leagueId)
        {
            var regs = db.CompetitionRegistrations.Where(r => r.LeagueId == leagueId && r.IsRegisteredByExcel);
            foreach (var registration in regs)
            {
                yield return new PlayerRegistrationDto
                {
                    RegistrationId = registration.Id,
                    FullName = registration.User.FullName,
                    ClubName = registration.Club.Name,
                    ClubNumber = registration.Club.ClubNumber,
                    Birthday = registration.User.BirthDay,
                    IsActive = registration.IsActive,
                    UserId = registration.User.UserId,
                    IdentNum = registration.User.IdentNum,
                    Gender = registration.User.Gender.Title
                };
            }
        }

        public TeamsPlayer GetTeamPlayerWithStartPlaying(string identNum, int seasonId)
        {
            return db.TeamsPlayers.FirstOrDefault(tp => tp.User.IdentNum == identNum && tp.SeasonId == seasonId && tp.StartPlaying.HasValue);
        }

        public void SetActive(IList<TeamDto> frmPlayerTeams, int userId, int seasonId, string sectionAlias)
        {
            if (frmPlayerTeams != null)
            {
                foreach (var team in frmPlayerTeams)
                {
                    var teamPlayer = db.TeamsPlayers.FirstOrDefault(x => x.UserId == userId &&
                                                                           x.TeamId == team.TeamId &&
                                                                           (team.LeagueId > 0 ? x.LeagueId == team.LeagueId : x.LeagueId == null) &&
                                                                           (team.ClubId > 0 ? x.ClubId == team.ClubId : x.ClubId == null) &&
                                                                           x.SeasonId == seasonId);
                    if (teamPlayer != null)
                    {
                        if(sectionAlias == SectionAliases.Bicycle)
                        {
                            //save for approaval mark player in player/edit POST as active
                            teamPlayer.IsActive = teamPlayer.IsActive == true ? true : team.IsActive;
                        }
                        else
                        {
                            teamPlayer.IsActive = team.IsActive;
                        }                        

                        teamPlayer.IsTrainerPlayer = team.IsTrainerPlayer;
                    }
                }
                db.SaveChanges();
            }
        }

        public Section GetSectionByUserId(int id, int seasonId)
        {
            var player = db.TeamsPlayers.FirstOrDefault(c => c.UserId == id && c.SeasonId == seasonId);
            if (player.ClubId.HasValue)
                return player?.Club?.Section ?? player?.Club?.Union?.Section;
            if (player.LeagueId.HasValue)
                return player?.League?.Club?.Section ?? player?.League?.Club?.Union?.Section ?? player?.League?.Union?.Section;
            return null;
        }

        public IEnumerable<MartialArtsCompetitionDto> GetMartialArtsPlayerStats(int id)
        {
            var userRegistrations = db.SportsRegistrations.Where(t => t.UserId == id && t.IsApproved);
            foreach (var registration in userRegistrations)
            {
                yield return new MartialArtsCompetitionDto
                {
                    RegistrationId = registration.Id,
                    StartDate = registration.League?.LeagueStartDate,
                    EndDate = registration.League?.LeagueEndDate,
                    ClubName = registration.Club?.Name,
                    CompetitionName = registration.League?.Name,
                    Points = registration.FinalScore,
                    Rank = registration.Position,
                    SeasonId = registration.SeasonId
                };
            }
        }





        public List<TeamsPlayer> GetPlayersByIds(int[] playersIds)
        {
            return db.TeamsPlayers.Include(tp => tp.User).Where(tp => playersIds.Contains(tp.Id)).ToList();
        }

        public List<TeamsPlayer> GetPlayersByIds(int?[] playersIds)
        {
            return db.TeamsPlayers.Where(tp => playersIds.Contains(tp.Id)).ToList();
            /*
            var players = new List<TeamsPlayer>();
            foreach (var playerId in playersIds)
                players.Add(playerId.HasValue ? db.TeamsPlayers.Find(playerId) : null);
            return players;
            */
        }

        public IEnumerable<NationalTeamInvitement> GetAllNatTeamInvitements(int id, int? seasonId)
        {
            if (seasonId.HasValue)
            {
                return db.NationalTeamInvitements.Where(t => t.TeamPlayerId == id && t.SeasonId.HasValue && t.SeasonId == seasonId.Value)
                    .ToList();
            }
            else
            {
                return null;
            }
        }

        public void SaveInvitementValue(List<NationalTeamInvitementDTO> nationalTeamInvitement, int teamPlayerId, int? seasonId)
        {
            try
            {
                foreach (var invitement in nationalTeamInvitement)
                {
                    db.NationalTeamInvitements.Add(new NationalTeamInvitement
                    {
                        TeamPlayerId = teamPlayerId,
                        SeasonId = seasonId,
                        StartDate = invitement.StartDate,
                        EndDate = invitement.EndDate
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateInvitementValue(List<NationalTeamInvitementDTO> newInvitementsValues, int teamPlayerId, int? seasonId)
        {
            try
            {
                var valuesToDelete = db.NationalTeamInvitements.Where(c => c.TeamPlayerId == teamPlayerId && c.SeasonId.HasValue && c.SeasonId == seasonId).ToList();
                if (valuesToDelete != null && valuesToDelete.Count > 0)
                {
                    db.NationalTeamInvitements.RemoveRange(valuesToDelete);
                    foreach (var invitement in newInvitementsValues)
                    {
                        db.NationalTeamInvitements.Add(new NationalTeamInvitement
                        {
                            TeamPlayerId = teamPlayerId,
                            SeasonId = seasonId,
                            StartDate = invitement.StartDate,
                            EndDate = invitement.EndDate
                        });
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<PlayerStatusViewModel> GetPlayersStatusesByClubId(int id, int? seasonId, bool fullCheck = false)
        {
            string sectionAlias = "";
            var club1 = db.Clubs.Where(c => c.ClubId == id)
                                .Include(c => c.Union)
                                .Include(c => c.Union.Section)
                                .Include(c => c.Section)
                                .FirstOrDefault();
            if (club1 != null)
            {
                if (club1.Section != null)
                {
                    sectionAlias = club1.Section.Alias;
                }
                else if (club1.Union != null)
                {
                    sectionAlias = club1.Union.Section.Alias;
                }
            }

            var clubTeams = db.ClubTeams.Where(ct => ct.ClubId == id).ToList();
            var result = GetPlayersStatusesByClubTeamIds(clubTeams, seasonId, sectionAlias, fullCheck);
            return result;
        }

        public IEnumerable<PlayerStatusViewModel> GetPlayersStatusesForCountByClubId(int id, int? seasonId)
        {
            string sectionAlias = "";
            var club1 = db.Clubs.Where(c => c.ClubId == id)
                                .Include(c => c.Union)
                                .Include(c => c.Union.Section)
                                .Include(c => c.Section)
                                .FirstOrDefault();
            if (club1 != null)
            {
                if (club1.Section != null)
                {
                    sectionAlias = club1.Section.Alias;
                }
                else if (club1.Union != null)
                {
                    sectionAlias = club1.Union.Section.Alias;
                }
            }

            var clubTeams = db.ClubTeams.Where(ct => ct.ClubId == id).ToList();
            var result = GetPlayersStatusesForCountByClubTeamIds(clubTeams, seasonId, sectionAlias);
            return result;
        }

        public List<PlayerStatusViewModel> GetPlayersStatusesByUnionId(int id, int? seasonId, string sectionAlias, bool fullCheck = false)
        {
            if (string.IsNullOrEmpty(sectionAlias))
            {
                var union = db.Unions.Where(c => c.UnionId == id)
                    .Include(c => c.Section)
                    .FirstOrDefault();
                if (union != null)
                {
                    sectionAlias = union.Section.Alias;
                }
            }

            var clubTeams = db.ClubTeams
                .Include(x => x.Club)
                .Include(x => x.Team)
                .Where(x => x.SeasonId == seasonId && x.Club.UnionId == id && !x.Club.IsUnionArchive)
                .ToList();

            var result = GetPlayersStatusesByClubTeamIds(clubTeams, seasonId, sectionAlias, fullCheck);
            return result;
        }

        public List<PlayerStatusViewModel> GetPlayersStatusesForCountByUnionId(int id, int? seasonId)
        {
            string sectionAlias = "";
            var union = db.Unions.Where(c => c.UnionId == id)
                                .Include(c => c.Section)
                                .FirstOrDefault();
            if (union != null)
            {
                sectionAlias = union.Section.Alias;
            }
            var clubTeams = db.Clubs.SelectMany(cl => cl.ClubTeams, (club, clubTeam) => new { cl = club, ct = clubTeam })
                .Where(t => t.cl.UnionId == id && t.ct.SeasonId == seasonId && !t.cl.IsUnionArchive)
                .Select(t => t.ct)
                .ToList();
            var result = GetPlayersStatusesForCountByClubTeamIds(clubTeams, seasonId, sectionAlias);
            return result;
        }

        private List<PlayerStatusViewModel> GetPlayersStatusesByClubTeamIds(List<ClubTeam> clubTeams, int? seasonId, string sectionAlias, bool fullCheck = false)
        {
            var clubTeamIds = clubTeams.Where(t => t.Club.IsArchive == false && t.Team.IsArchive == false).Select(x => x.TeamId).ToList();

            var data = db.TeamsPlayers
                .Include(x => x.User)
                .Include(x => x.Team)
                .Where(tp => clubTeamIds.Contains(tp.TeamId) &&
                             tp.User.IsArchive == false &&
                             tp.SeasonId == seasonId);

            if (sectionAlias == GamesAlias.Athletics)
            {
                data = data.Include("User.AthleteNumbers");
            }

            if (sectionAlias != GamesAlias.Athletics)
            {
                data = data
                    .Include(x => x.Team.LeagueTeams)
                    .Where(tp=>!tp.Team.LeagueTeams.Any() || tp.ClubId == null);
            }

            if (fullCheck)
            {
                data = data
                    .Include(x => x.User.CompetitionDisciplineRegistrations)
                    .Include(x => x.User.SportsRegistrations)
                    .Include(x => x.User.CompetitionRegistrations)
                    .Include(x => x.Team.TeamsDetails);
            }

            var players = data.AsNoTracking().ToList();

            var result = GetPlayersStatusViewModel(players, seasonId, sectionAlias, fullCheck);
            return result;
        }

        private List<PlayerStatusViewModel> GetPlayersStatusesForCountByClubTeamIds(List<ClubTeam> clubTeams, int? seasonId, string sectionAlias)
        {
            var clubTeamIds = clubTeams.Where(t => t.Club.IsArchive == false && t.Team.IsArchive == false).Select(x => x.TeamId);

            var data = db.TeamsPlayers
                .Where(tp => clubTeamIds.Contains(tp.TeamId) &&
                             tp.User.IsArchive == false &&
                             tp.SeasonId == seasonId);

            if(sectionAlias != GamesAlias.MartialArts)
            {
                data = data
                .Where(tp => (!tp.Team.LeagueTeams.Any() || tp.ClubId == null));
            }

            var players = data.AsNoTracking().ToList();

            var result = GetPlayersStatusViewModelForCount(players, seasonId, sectionAlias);
            return result;
        }

        private List<PlayerStatusViewModel> GetPlayersStatusViewModel(List<TeamsPlayer> players, int? seasonId, string sectionAlias, bool fullCheck = false)
        {
            var result = new List<PlayerStatusViewModel>();
            var playerIds = players.Select(x => x.UserId).Distinct().ToList();
            var teamIds = players.Select(x => x.TeamId).Distinct().ToList();

            List<ActivityFormsSubmittedData> allPlayersRegistrations = new List<ActivityFormsSubmittedData>();
            if (sectionAlias != GamesAlias.Athletics)
            {
                allPlayersRegistrations = db.ActivityFormsSubmittedDatas.Where(
                        x => playerIds.Contains(x.PlayerId) && x.Activity.IsAutomatic == true &&
                             (x.Activity.Type == ActivityType.Personal || x.Activity.Type == ActivityType.UnionPlayerToClub) &&
                             x.Activity.SeasonId == seasonId)
                    .AsNoTracking()
                    .ToList();
            }

            var allPlayersDisciplines = db.PlayerDisciplines.Where(x => playerIds.Contains(x.PlayerId)).AsNoTracking().ToList();
            var allPlayersClubs = db.ClubTeams.Where(x => teamIds.Contains(x.TeamId) && x.SeasonId == seasonId)
                .AsNoTracking().ToList();

            var allPlayersFiles = db.PlayerFiles
                .Where(x => playerIds.Contains(x.PlayerId) && x.SeasonId == seasonId)
                .AsNoTracking()
                .ToList();

            var teamName = "";

            var sportsRegistrations = db.SportsRegistrations
                .Where(x => playerIds.Contains(x.UserId) &&
                            (x.SeasonId == seasonId &&
                             !x.League.IsArchive &&
                             x.IsApproved ||
                             (x.FinalScore.HasValue || x.Position.HasValue)))
                .Include(x => x.League)
                .ToList();

            int AlternativeResultInt = 3;
            var competitionDisciplineRegistrations = sectionAlias == GamesAlias.Athletics ? db.CompetitionDisciplineRegistrations.Where(c => playerIds.Contains(c.UserId) && c.CompetitionDiscipline.League.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && !c.CompetitionDiscipline.IsDeleted && c.CompetitionResult.FirstOrDefault() != null && !(c.CompetitionResult.FirstOrDefault().Result == null || c.CompetitionResult.FirstOrDefault().Result.Trim() == string.Empty) && c.CompetitionResult.FirstOrDefault().AlternativeResult != AlternativeResultInt).ToList() : new List<CompetitionDisciplineRegistration>();

            foreach (var tp in players)
            {
                var registration = allPlayersRegistrations.FirstOrDefault(x => x.PlayerId == tp.UserId);
                var playerDisciplines = allPlayersDisciplines
                    .Where(x => x.ClubId == tp.ClubId && x.SeasonId == tp.SeasonId).Select(x => x.DisciplineId);
                var club = allPlayersClubs.FirstOrDefault(x => x.TeamId == tp.TeamId && x.SeasonId == seasonId)?.ClubId;

                var playerFiles = allPlayersFiles.Where(x => x.PlayerId == tp.UserId).ToList();

                var competitionCount = 0;

                if (fullCheck)
                {
                    var gymnasticCompetitionCount = (sectionAlias == GamesAlias.Gymnastic) ? tp.User.CompetitionRegistrations
                    .Where(c => c.SeasonId == seasonId && !c.League.IsArchive && c.IsActive && (c.FinalScore.HasValue || c.Position.HasValue))
                    .GroupBy(r => r.LeagueId)
                    .Select(r => r.First())
                    .Count() : 0;

                    int regularCompetitionCount = (sectionAlias == GamesAlias.WeightLifting) ? tp.User.CompetitionDisciplineRegistrations
                        .Where(c => c.CompetitionDiscipline.League.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && c.IsApproved.HasValue && c.IsApproved.Value)
                        .GroupBy(r => r.CompetitionDiscipline.League.LeagueId)
                        .Select(r => r.First())
                        .Count() : 0;

                    int athleticRegistrationCount = 0;
                    if (sectionAlias == GamesAlias.Athletics)
                    {          
                        athleticRegistrationCount = competitionDisciplineRegistrations
                            .Where(c => c.UserId == tp.UserId)
                            .GroupBy(r => r.CompetitionDiscipline.League.LeagueId)
                            .Select(r => r.First())
                            .Count();
                    }

                    var otherCompetitionCount = sportsRegistrations
                        .Where(c => c.UserId == tp.UserId && c.IsApproved)
                        .GroupBy(r => r.LeagueId)
                        .Select(r => r.First())
                        .Count();
                    competitionCount = gymnasticCompetitionCount + athleticRegistrationCount + regularCompetitionCount + otherCompetitionCount;
                    if (sectionAlias == GamesAlias.Tennis)
                    {
                        competitionCount = tp.CompetitionParticipationCount ?? 0;
                    }

                    teamName = tp.Team.TeamsDetails.FirstOrDefault()?.TeamName ?? tp.Team.Title;
                }

                var playerStatusViewModel = new PlayerStatusViewModel
                {
                    Id = tp.Id,
                    UserId = tp.UserId,
                    IsActive = tp.IsActive,
                    IsApproveChecked = tp.IsApprovedByManager == true,
                    IsNotApproveChecked = tp.IsApprovedByManager == false,
                    IsPlayerRegistered = registration != null,
                    IsPlayerRegistrationApproved = registration?.IsActive ?? false,
                    ClubId = club ?? 0,
                    DisciplinesIds = playerDisciplines,
                    HasMedicalCert = playerFiles.Any(x => x.FileType == (int) PlayerFileType.MedicalCertificate && !x.IsArchive),
                    FirstName = !string.IsNullOrWhiteSpace(tp.User.FirstName)
                        ? tp.User.FirstName
                        : GetFirstNameByFullName(tp.User.FullName),
                    LastName = !string.IsNullOrWhiteSpace(tp.User.LastName)
                        ? tp.User.LastName
                        : GetLastNameByFullName(tp.User.FullName),
                    FullName = tp.User.FullName,
                    TeamName = teamName,
                    GenderId = tp.User.GenderId.HasValue ? tp.User.GenderId.Value : 3,
                    Birthday = tp.User.BirthDay,
                    IdentNum = tp.User.IdentNum,
                    MedExamDate = tp.User.MedExamDate,
                    RawTenicardValidity = tp.User.TenicardValidity,
                    PassportNum = tp.User.PassportNum,
                    AthletesNumbers = sectionAlias == GamesAlias.Athletics ? tp.User.AthleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId)?.AthleteNumber1 : null,
                    IsAthleteNumberProduced = sectionAlias == GamesAlias.Athletics ? (tp.User.AthleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId)?.IsAthleteNumberProduced ?? false) : false,
                    CompetitionCount = competitionCount
                };
                if (sectionAlias == GamesAlias.Swimming)
                {
                    playerStatusViewModel.ClassS = tp.User.ClassS;
                    playerStatusViewModel.ClassSB = tp.User.ClassSB;
                    playerStatusViewModel.ClassSM = tp.User.ClassSM;
                }
                result.Add(playerStatusViewModel);
            }

            return result;
        }



        private List<PlayerStatusViewModel> GetPlayersStatusViewModelForCount(List<TeamsPlayer> players, int? seasonId, string sectionAlias)
        {
            var playerIds = players.Select(x => x.UserId).Distinct().ToList();
            var medfiles = db.PlayerFiles.Where(pf => playerIds.Contains(pf.PlayerId) && pf.FileType == (int)PlayerFileType.MedicalCertificate && pf.SeasonId == seasonId && !pf.IsArchive).ToList();
            var isTennis = sectionAlias == GamesAlias.Tennis;
            var users = isTennis ? db.Users.Where(u => playerIds.Contains(u.UserId)).ToList() : new List<User>();

            var result = new List<PlayerStatusViewModel>();
            foreach (var tp in players)
            {
                var statusViewData = new PlayerStatusViewModel
                {
                    Id = tp.Id,
                    UserId = tp.UserId,
                    IsActive = tp.IsActive,
                    IsApproveChecked = tp.IsApprovedByManager == true,
                    IsNotApproveChecked = tp.IsApprovedByManager == false,
                    HasMedicalCert = medfiles.Any(x => x.PlayerId == tp.UserId),
                };
                if (isTennis)
                {
                    var user = users.FirstOrDefault(u => u.UserId == tp.UserId);
                    statusViewData.MedExamDate = user.MedExamDate;
                    statusViewData.RawTenicardValidity = user.TenicardValidity;
                }
                result.Add(statusViewData);
            }
            return result;
        }


        //todo - copy from union
        public List<PlayerViewModel> GetTeamPlayersByClubIdFiltered(int id, int? seasonId, string searchText, List<int> filterClubs,
           List<int> filterDisciplines, List<int> filterStatus, string sortBy, string sortDirection, int? skip,
           int? take, string sectionAlias, string filterByClubs, string filterByDisciplines, string filterByPlayersStatus, out int recordsFiltered)
        {
            var clubTeams = db.ClubTeams.Include(x => x.Club).Include(x => x.Team).Include(x => x.Season)
               .Where(x => x.Club.UnionId == id).ToList();
            var leagueTeams = db.LeagueTeams.Include(x => x.Leagues).Include(x => x.Teams).Include(x => x.Season)
                .Where(x => x.Leagues.UnionId == id);
            if (filterClubs?.Any() == true)
            {
                clubTeams = clubTeams.Where(x => filterClubs.Contains(x.ClubId)).ToList();
            }
            var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
            var isClubsBlocked = season?.Union?.IsClubsBlocked ?? false;


            var clubTeamsIds = clubTeams?.Select(c => c.TeamId).ToList();

            var playersBase = db.TeamsPlayers
                .Include(tp => tp.User)
                .Include(tp => tp.User.ActivityFormsSubmittedDatas)
                .Include(tp => tp.League)
                .Include(tp => tp.Team)
                //.Include(tp => tp.Team.ClubTeams)
                //.Include(tp => tp.Team.LeagueTeams)
                //.Include(tp => tp.Team.TeamsDetails)
                .Include(tp => tp.User.Gender)
                .Include(tp => tp.User.UsersRoutes)
                .Include(tp => tp.User.UsersRanks)
                .Include(tp => tp.User.PlayerAchievements)
                .Include(tp => tp.User.PlayerFiles)
                .Include(tp => tp.User.PlayerDisciplines)
                .Include(tp => tp.User.PlayersBlockade)
                .Include(tp => tp.User.TeamsPlayers)
                .Include(tp => tp.Position)
                .Where(tp => clubTeamsIds.Contains(tp.TeamId) &&
                             tp.User.IsArchive == false &&
                             tp.SeasonId == seasonId &&
                             (!tp.Team.LeagueTeams.Any() || tp.ClubId == null));

            if (sectionAlias == GamesAlias.Athletics && isClubsBlocked)
            {
                playersBase = playersBase.Where(tp => tp.IsActive == true || tp.IsApprovedByManager == true);
            }
            if(sectionAlias == GamesAlias.Climbing && isClubsBlocked)
            {
                playersBase = playersBase.Where(tp => tp.IsActive == true);
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                playersBase = playersBase.Where(tp => tp.User.FullName.Contains(searchText)
                    || tp.User.IdentNum.Contains(searchText)
                    || tp.User.Email.Contains(searchText));
            }

            if (filterDisciplines?.Any() == true)
            {
                playersBase = playersBase.Where(x =>
                    x.User.PlayerDisciplines.Where(d => d.ClubId == x.ClubId && d.SeasonId == x.SeasonId).Select(d => d.DisciplineId).Intersect(filterDisciplines).Any());
            }

            if (filterStatus?.Any() == true)
            {
                Expression<Func<TeamsPlayer, bool>> expression = tp =>
                    (filterStatus.Contains(1) && (tp.IsActive && tp.IsApprovedByManager != true && tp.IsApprovedByManager != false)) ||
                    (filterStatus.Contains(2) && tp.IsApprovedByManager == true) ||
                    (filterStatus.Contains(3) && tp.IsApprovedByManager == false) ||
                    (filterStatus.Contains(4) && !tp.IsActive ||
                     (filterStatus.Contains(5) && tp.User.BlockadeId.HasValue));

                playersBase = playersBase.Where(expression);
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy + "." + sortDirection)
                {
                    case "FullName.asc":
                        playersBase = playersBase.OrderBy(p => p.User.FullName);
                        break;
                    case "FullName.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.FullName);
                        break;
                    case "BirthdayString.asc":
                        playersBase = playersBase.OrderBy(p => p.User.BirthDay);
                        break;
                    case "BirthdayString.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.BirthDay);
                        break;
                    case "ShirtNum.asc":
                        playersBase = playersBase.OrderBy(p => p.ShirtNum);
                        break;
                    case "ShirtNum.desc":
                        playersBase = playersBase.OrderByDescending(p => p.ShirtNum);
                        break;
                    case "PosId.asc":
                        playersBase = playersBase.OrderBy(p => p.Position.Title);
                        break;
                    case "PosId.desc":
                        playersBase = playersBase.OrderByDescending(p => p.Position.Title);
                        break;
                    case "IdentNum.asc":
                        playersBase = playersBase.OrderBy(p => p.User.IdentNum);
                        break;
                    case "IdentNum.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.IdentNum);
                        break;
                    case "Email.asc":
                        playersBase = playersBase.OrderBy(p => p.User.Email);
                        break;
                    case "Email.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.Email);
                        break;
                    case "Phone.asc":
                        playersBase = playersBase.OrderBy(p => p.User.Telephone);
                        break;
                    case "Phone.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.Telephone);
                        break;
                    case "City.asc":
                        playersBase = playersBase.OrderBy(p => p.User.City);
                        break;
                    case "City.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.City);
                        break;
                    case "Height.asc":
                        playersBase = playersBase.OrderBy(p => p.User.Height);
                        break;
                    case "Height.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.Height);
                        break;
                    case "Weight.asc":
                        playersBase = playersBase.OrderBy(p => p.User.Weight);
                        break;
                    case "Weight.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.Weight);
                        break;
                    case "Gender.asc":
                        playersBase = playersBase.OrderBy(p => p.User.GenderId);
                        break;
                    case "Gender.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.GenderId);
                        break;
                    case "ParentName.asc":
                        playersBase = playersBase.OrderBy(p => p.User.ParentName);
                        break;
                    case "ParentName.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.ParentName);
                        break;
                    case "ClubName.asc":
                        playersBase = playersBase.OrderBy(p => p.Team.ClubTeams.FirstOrDefault(t => t.TeamId == p.TeamId).Club.Name ?? "");
                        break;
                    case "ClubName.desc":
                        playersBase = playersBase.OrderByDescending(p => p.Team.ClubTeams.FirstOrDefault(t => t.TeamId == p.TeamId).Club.Name ?? "");
                        break;
                    case "StartPlaying.asc":
                        playersBase = playersBase.OrderBy(p => p.StartPlaying);
                        break;
                    case "StartPlaying.desc":
                        playersBase = playersBase.OrderByDescending(p => p.StartPlaying);
                        break;
                    case "EndBlockadeDateString.asc":
                        playersBase = playersBase.OrderBy(p => p.User.PlayersBlockade.EndDate);
                        break;
                    case "EndBlockadeDateString.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.PlayersBlockade.EndDate);
                        break;
                    case "ShirtSize.asc":
                        playersBase = playersBase.OrderBy(p => p.User.ShirtSize == "3XL")
                            .ThenBy(p => p.User.ShirtSize == "2XL")
                            .ThenBy(p => p.User.ShirtSize == "XL")
                            .ThenBy(p => p.User.ShirtSize == "L")
                            .ThenBy(p => p.User.ShirtSize == "M")
                            .ThenBy(p => p.User.ShirtSize == "S")
                            .ThenBy(p => p.User.ShirtSize == null);
                        break;
                    case "ShirtSize.desc":
                        playersBase = playersBase.OrderBy(p => p.User.ShirtSize == null)
                          .ThenBy(p => p.User.ShirtSize == "S")
                          .ThenBy(p => p.User.ShirtSize == "M")
                          .ThenBy(p => p.User.ShirtSize == "L")
                          .ThenBy(p => p.User.ShirtSize == "XL")
                          .ThenBy(p => p.User.ShirtSize == "2XL")
                          .ThenBy(p => p.User.ShirtSize == "3XL");
                        break;
                    case "BaseHandicap.asc":
                        playersBase = playersBase.OrderBy(p => p.HandicapLevel);
                        break;
                    case "BaseHandicap.desc":
                        playersBase = playersBase.OrderByDescending(p => p.HandicapLevel);
                        break;
                    case "UnionComment.asc":
                        playersBase = playersBase.OrderBy(p => p.UnionComment);
                        break;
                    case "UnionComment.desc":
                        playersBase = playersBase.OrderByDescending(p => p.UnionComment);
                        break;
                    case "ClubComment.asc":
                        playersBase = playersBase.OrderBy(p => p.ClubComment);
                        break;
                    case "ClubComment.desc":
                        playersBase = playersBase.OrderByDescending(p => p.ClubComment);
                        break;
                    case "TeamName.asc":
                        playersBase = playersBase.OrderBy(p => p.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId).TeamName ?? p.Team.Title);
                        break;
                    case "TeamName.desc":
                        playersBase = playersBase.OrderByDescending(p => p.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId).TeamName ?? p.Team.Title);
                        break;
                    case "LeagueName.asc":
                        playersBase = playersBase.OrderBy(p => p.League.Name);
                        break;
                    case "LeagueName.desc":
                        playersBase = playersBase.OrderByDescending(p => p.League.Name);
                        break;
                    case "CompetitionCount.asc":
                        playersBase = playersBase.OrderBy(p => p.User.SportsRegistrations.Count(r => !r.League.IsArchive && r.IsApproved || (r.FinalScore.HasValue || r.Position.HasValue))
                        + p.User.CompetitionRegistrations.Count(c => !c.League.IsArchive && (c.IsActive && c.IsRegisteredByExcel) || c.FinalScore.HasValue || c.Position.HasValue));
                        break;
                    case "CompetitionCount.desc":
                        playersBase = playersBase.OrderBy(p => p.User.SportsRegistrations.Count(r => !p.League.IsArchive && r.IsApproved || (r.FinalScore.HasValue || r.Position.HasValue))
                        + p.User.CompetitionRegistrations.Count(c => !c.League.IsArchive && (c.IsActive && c.IsRegisteredByExcel) && c.FinalScore.HasValue || c.Position.HasValue));
                        break;
                    case "TenicardValidity.asc":
                        playersBase = playersBase.OrderBy(p => p.User.TenicardValidity);
                        break;
                    case "TenicardValidity.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.TenicardValidity);
                        break;
                    default:
                        playersBase = playersBase.OrderBy(p => p.User.FullName);
                        break;
                }
            }
            else
            {
                playersBase = playersBase.OrderBy(p => p.User.FullName);
            }
            recordsFiltered = playersBase.GroupBy(x => x.UserId).Count();

            var playersBaseOrdered = playersBase.GroupBy(x => x.UserId);
            var playersToResult = new List<TeamsPlayer>();



            if (skip.HasValue)
            {
                playersBaseOrdered = playersBaseOrdered.OrderBy(c => c.Key).Skip(skip.Value);

                if (take != null)
                {
                    playersBaseOrdered = playersBaseOrdered.Take((int)take);
                }

            }
            var playersKeys = playersBaseOrdered.ToList();
            foreach (var key in playersKeys)
            {
                playersToResult.Add(playersBase.First(c => c.UserId == key.Key));
            }

            var result = GetPlayersViewModel(playersToResult.ToList(), seasonId, sectionAlias);
            return result;
        }

        public bool IsUserApprovedInTeamPlayer(int userId, int seasonId)
        {
            var teamplayer = db.TeamsPlayers.FirstOrDefault(t => t.UserId == userId && t.SeasonId == seasonId && t.IsApprovedByManager.HasValue && t.IsApprovedByManager.Value);
            if (teamplayer != null)
            {
                return true;
            }
            return false;
        }

        public List<PlayerViewModel> GetTeamPlayersByClubId(int id, int? seasonId, string searchText,
        List<int> filterDisciplines, List<int> filterStatus, List<string> searchColumnIds, string sortBy, string sortDirection, int? skip,
        int? take, string sectionAlias, out int playersCount)
        {
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == id);
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
                clubTeams = db.ClubTeams.Where(ct => ct.ClubId == id && !ct.Team.IsArchive).ToList();
            }
            var clubTeamsIds = clubTeams?.Select(c => c.Team.TeamId)?.ToList();
            var isClubsBlocked = club?.Union?.IsClubsBlocked ?? false;
            if (isClubsBlocked && club?.IsUnionArchive == true)
            {
                isClubsBlocked = false;
            }
            var result = GetTeamPlayersByClubTeamIds(clubTeamsIds, seasonId, searchText, filterDisciplines, filterStatus, searchColumnIds, sortBy, sortDirection, skip, take, sectionAlias, club.UnionId, out playersCount, isClubsBlocked);
            return result;
        }

        public IDictionary<Season, List<StatisticsDTO>> GetPlayersStatistics(int id, int teamId, int currentSeasonId, int? leagueId)
        {
            var secRepo = new SectionsRepo();
            var section = secRepo.GetSectionByTeamId(teamId);
            var isBasketBall = section?.Alias == GamesAlias.BasketBall;
            var isWaterPolo = section?.Alias == GamesAlias.WaterPolo;

            TeamsPlayer teamPlayer = null;
            if (isBasketBall)
            {
                teamPlayer = db.TeamsPlayers.FirstOrDefault(c => c.UserId == id && c.TeamId == teamId && c.SeasonId == currentSeasonId && !c.User.IsArchive && c.GameStatistics.Any());
            }
            else if (isWaterPolo)
            {
                teamPlayer = db.TeamsPlayers.FirstOrDefault(c => c.UserId == id && c.TeamId == teamId && c.SeasonId == currentSeasonId && !c.User.IsArchive && c.WaterpoloStatistics.Any());
            }

            if (teamPlayer != null)
            {
                if (isBasketBall && teamPlayer.GameStatistics.Any())
                {
                    var gamesStatistics = GetAllStatistics(teamPlayer.GameStatistics, teamId);
                    var seasonsIds = gamesStatistics.Where(c => c.Season != null).Select(c => c.Season).Select(c => c.Id).Distinct();
                    var gamesDictionary = new Dictionary<Season, List<StatisticsDTO>>();
                    foreach (var seasonId in seasonsIds)
                    {
                        var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
                        var seasonGames = gamesStatistics.Where(c => c.Season?.Id == seasonId)?.ToList();
                        gamesDictionary.Add(season, seasonGames);
                    }
                    return gamesDictionary;
                }
                else if (isWaterPolo && teamPlayer.WaterpoloStatistics.Any())
                {
                    var gamesStatistics = GetAllWStatistics(teamPlayer.WaterpoloStatistics, teamId);
                    var seasonsIds = gamesStatistics.Where(c => c.Season != null).Select(c => c.Season).Select(c => c.Id).Distinct();
                    var gamesDictionary = new Dictionary<Season, List<StatisticsDTO>>();
                    foreach (var seasonId in seasonsIds)
                    {
                        var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
                        var seasonGames = gamesStatistics.Where(c => c.Season?.Id == seasonId)?.ToList();
                        gamesDictionary.Add(season, seasonGames);
                    }
                    return gamesDictionary;
                }
                else
                    return new Dictionary<Season, List<StatisticsDTO>>();
            }
            else
                return new Dictionary<Season, List<StatisticsDTO>>();
        }

        private IEnumerable<StatisticsDTO> GetAllWStatistics(ICollection<WaterpoloStatistic> statistics, int teamId)
        {
            var gamesIds = statistics.Select(c => c.GameId).Distinct();
            foreach (var gameId in gamesIds)
            {
                var gamesStatistics = statistics.FirstOrDefault(s => s.GameId == gameId);

                var game = db.GamesCycles.Find(gameId);
                var oppoTeamInfo = db.WaterpoloStatistics.Where(x => x.TeamId == game.GuestTeamId && x.GameId == game.CycleId);
                var oppoGoal = oppoTeamInfo.Sum(x => x.GOAL) ?? 0;
                var oppoMiss = oppoTeamInfo.Sum(x => x.Miss) ?? 0;

                if (gamesStatistics != null)
                {
                    var plStat = new StatisticsDTO();

                    plStat.Season = gamesStatistics.GamesCycle?.Stage?.League?.Season;
                    plStat.Overall = gamesStatistics.GamesCycle?.HomeTeamId != teamId
                        ? gamesStatistics.GamesCycle?.HomeTeam?.Title
                        : gamesStatistics.GamesCycle?.GuestTeam?.Title;
                    plStat.GP = CheckGameWinner(gameId, teamId);
                    plStat.Min = gamesStatistics.MinutesPlayed.ToMinutesFromMiliseconds();
                    plStat.Goal = gamesStatistics.GOAL ?? 0;
                    plStat.PGoal = gamesStatistics.PGOAL ?? 0;
                    plStat.Miss = gamesStatistics.Miss ?? 0;
                    plStat.PMiss = gamesStatistics.PMISS ?? 0;
                    plStat.AST = gamesStatistics.AST ?? 0;
                    plStat.TO = gamesStatistics.TO ?? 0;
                    plStat.STL = gamesStatistics.STL ?? 0;
                    plStat.BLK = gamesStatistics.BLK ?? 0;
                    plStat.Offs = gamesStatistics.OFFS ?? 0;
                    plStat.Foul = gamesStatistics.FOUL ?? 0;
                    plStat.Exc = gamesStatistics.EXC ?? 0;
                    plStat.BFoul = gamesStatistics.BFOUL ?? 0;
                    plStat.SSave = gamesStatistics.SSAVE ?? 0;
                    plStat.YC = gamesStatistics.YC ?? 0;
                    plStat.RD = gamesStatistics.RD ?? 0;
                    plStat.EFF = (plStat.Goal + plStat.PGoal + plStat.AST + plStat.BLK + plStat.STL - (plStat.Miss + plStat.PMiss + plStat.TO));
                    plStat.PlusMinus = gamesStatistics.DIFF ?? 0D;
                    plStat.FGP = (plStat.Goal + plStat.Miss) == 0 ? 0D : Math.Round((double)plStat.Goal * 100 / (double)(plStat.Goal + plStat.Miss), 1);
                    plStat.GSPP = oppoGoal == 0 ? 0D : Math.Round((double)plStat.SSave * 100 / (double)oppoGoal, 1);
                    plStat.SAR = plStat.SSave == 0 ? 0D : Math.Round((double)(oppoGoal + plStat.SSave) / (double)plStat.SSave, 1);
                    plStat.SCR = (plStat.SSave == 0 || oppoGoal == 0) ? 0D : Math.Round((double)(oppoGoal + oppoMiss) / (double)oppoGoal, 1);

                    yield return plStat;

                };
            }
        }

        private IEnumerable<StatisticsDTO> GetAllStatistics(ICollection<GameStatistic> statistics, int teamId)
        {
            var gamesIds = statistics.Select(c => c.GameId).Distinct();
            foreach (var gameId in gamesIds)
            {
                var gamesStatistics = statistics.FirstOrDefault(s => s.GameId == gameId);

                if (gamesStatistics != null)
                {
                    var plStat = new StatisticsDTO();

                    plStat.Season = gamesStatistics.GamesCycle?.Stage?.League?.Season;
                    plStat.Overall = gamesStatistics.GamesCycle?.HomeTeamId != teamId
                        ? gamesStatistics.GamesCycle?.HomeTeam?.Title
                        : gamesStatistics.GamesCycle?.GuestTeam?.Title;
                    plStat.GP = CheckGameWinner(gameId, teamId);
                    plStat.Min = gamesStatistics.MinutesPlayed.ToMinutesFromMiliseconds();
                    plStat.FG = gamesStatistics.FG ?? 0;
                    plStat.FGA = gamesStatistics.FGA ?? 0;
                    plStat.ThreePT = gamesStatistics.ThreePT ?? 0;
                    plStat.ThreePA = gamesStatistics.ThreePA ?? 0;
                    plStat.TwoPT = gamesStatistics.TwoPT ?? 0;
                    plStat.TwoPA = gamesStatistics.TwoPA ?? 0;
                    plStat.FT = gamesStatistics.FT ?? 0;
                    plStat.FTA = gamesStatistics.FTA ?? 0;
                    plStat.OREB = gamesStatistics.OREB ?? 0;
                    plStat.DREB = gamesStatistics.DREB ?? 0;
                    plStat.REB = plStat.OREB + plStat.DREB;
                    plStat.AST = gamesStatistics.AST ?? 0;
                    plStat.TO = gamesStatistics.TO ?? 0;
                    plStat.STL = gamesStatistics.STL ?? 0;
                    plStat.BLK = gamesStatistics.BLK ?? 0;
                    plStat.PF = gamesStatistics.PF ?? 0;
                    plStat.PTS = plStat.FT + plStat.TwoPT * 2 + plStat.ThreePT * 3;
                    plStat.FGM = plStat.TwoPT + plStat.ThreePT;
                    plStat.FTM = plStat.TwoPA + plStat.ThreePA;
                    plStat.EFF = (plStat.PTS + plStat.REB + plStat.AST + plStat.STL + plStat.BLK - ((plStat.TwoPA - plStat.TwoPT) + (plStat.FTA - plStat.FT) + plStat.TO));
                    plStat.PlusMinus = gamesStatistics.DIFF ?? 0D;
                    yield return plStat;

                };
            }
        }

        private string CheckGameWinner(int gameId, int teamId)
        {
            var game = db.GamesCycles.FirstOrDefault(c => c.CycleId == gameId);
            if (game != null)
            {
                var isTechnicalWinner = game.TechnicalWinnnerId == teamId;

                if (isTechnicalWinner) return GPType.Winner;
                else
                {
                    var isHomeTeam = game.HomeTeamId == teamId;
                    var homeTeamPoints = game.GameSets.Sum(gs => gs.HomeTeamScore);
                    var guestTeamPoints = game.GameSets.Sum(gs => gs.GuestTeamScore);
                    return GetGameResult(homeTeamPoints, guestTeamPoints, isHomeTeam);
                }
            }
            else
                return string.Empty;
        }

        private string GetGameResult(int homeTeamPoints, int guestTeamPoints, bool isHomeTeam)
        {
            if (isHomeTeam)
            {
                if (homeTeamPoints > guestTeamPoints) return GPType.Winner;
                else return GPType.Loser;
            }
            else
            {
                if (homeTeamPoints < guestTeamPoints) return GPType.Winner;
                else return GPType.Winner;
            }
        }

        public List<PlayerViewModel> GetTeamPlayersByUnionId(int id, int? seasonId, string searchText,
            List<int> filterClubs, List<int> filterDisciplines, List<int> filterStatus, List<string> searchColumnIds, string sortBy,
            string sortDirection, int? skip, int? take, string sectionAlias, out int playersCount)
        {
            var clubTeams = db.ClubTeams.Include("Team")
                .Where(x => x.Club.UnionId == id && !x.Team.IsArchive && !x.Club.IsUnionArchive)
                .ToList();

            if (filterClubs?.Any() == true)
            {
                clubTeams = clubTeams.Where(x => filterClubs.Contains(x.ClubId)).ToList();
            }

            var clubTeamsIds = clubTeams?.Select(c => c.Team.TeamId);

            if (filterClubs?.Any() != true)
            {
                var leagueTeams = db.LeagueTeams.Include("Teams")
                    .Where(x => x.Leagues.UnionId == id && !x.Teams.IsArchive)
                    .ToList();
                clubTeamsIds = clubTeamsIds.Union(leagueTeams?.Select(t => t.Teams.TeamId))?.Distinct();
            }

            var clubTeamsIdsList = clubTeamsIds.ToList();
            var result = GetTeamPlayersByClubTeamIds(clubTeamsIdsList, seasonId, searchText, filterDisciplines, filterStatus, searchColumnIds, sortBy, sortDirection, skip, take, sectionAlias, id, out playersCount, false, false);
            return result;
        }

        public IEnumerable<PlayerViewModel> GetTeamPlayersShortByUnionId(int id, int? seasonId)
        {
            var clubTeamIds = db.ClubTeams.Where(x => x.Club.UnionId == id && !x.Club.IsUnionArchive)
                .Select(x => x.TeamId);

            var players = db.TeamsPlayers
                 .Where(tp => clubTeamIds.Contains(tp.TeamId) &&
                              tp.User.IsArchive == false &&
                              tp.SeasonId == seasonId &&
                              (!tp.Team.LeagueTeams.Any() || tp.ClubId == null));
            if (players.Any())
            {
                foreach (var player in players)
                {
                    yield return new PlayerViewModel
                    {
                        Id = player.Id,
                        FullName = player.User.FullName
                    };
                }
            }
        }

        public List<PlayerViewModel> GetTeamPlayersShortByTeamId(int teamId, int? seasonId, string sectionAlias)
        {
            var players = db.TeamsPlayers
                .Where(tp => tp.SeasonId == seasonId && 
                             tp.TeamId == teamId)
                .Include(x => x.User);

            if (sectionAlias == GamesAlias.Gymnastic)
            {
                players.Include(x => x.User.CompetitionRegistrations);
            }
            var playerList = players.ToList();
            return GetPlayersViewModelShort(playerList, seasonId, sectionAlias);
        }

        public List<PlayerViewModel> GetTeamPlayersShortByClubId(int clubId, int? seasonId, string sectionAlias)
        {
            var clubTeamIds = db.ClubTeams.Where(ct => ct.ClubId == clubId && ct.SeasonId == seasonId && !ct.IsTrainingTeam)
                .Select(x => x.TeamId);

            var players = db.TeamsPlayers
                .Where(tp => tp.SeasonId == seasonId &&
                             clubTeamIds.Contains(tp.TeamId) &&
                             tp.User.IsArchive == false &&
                             (!tp.Team.LeagueTeams.Any() || tp.ClubId == null))
                .Include(x => x.User);

            if (sectionAlias == GamesAlias.Swimming || sectionAlias == GamesAlias.Rowing || sectionAlias == GamesAlias.WeightLifting)
            {
                players.Include(x => x.User.CompetitionDisciplineRegistrations);
            }
            else
            if (sectionAlias == GamesAlias.Gymnastic)
            {
                players.Include(x => x.User.CompetitionRegistrations);
            }
            else
            if (sectionAlias != GamesAlias.Tennis)
            {
                players.Include(x => x.User.SportsRegistrations);
            }
            var playersList = players.ToList();
            return GetPlayersViewModelShort(playersList, seasonId, sectionAlias);
        }
        public bool IsAthleteNumberUsed(int? athleteNumber, int userId, int? seasonId)
        {
            var player = db.Users.Where(u => u.AthleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId) != null && u.AthleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId).AthleteNumber1 == athleteNumber).FirstOrDefault();
            if (player == null || player.UserId == userId)
            {
                return false;
            }
            return true;
        }

        private List<PlayerViewModel> GetPlayersViewModelShort(List<TeamsPlayer> players, int? seasonId, string sectionAlias)
        {
            var result = new List<PlayerViewModel>();

            var usersIds = players.Select(p => p.UserId);

            var registrations = db.ActivityFormsSubmittedDatas
                                .Where(x => usersIds.Contains(x.PlayerId) &&
                                    x.Activity.Type == ActivityType.Personal &&
                                    x.Activity.SeasonId == seasonId)?.ToList();
            int AlternativeResultInt = 3;
            var competitionDisciplineRegistrations = sectionAlias == GamesAlias.Athletics ? db.CompetitionDisciplineRegistrations.Where(c => usersIds.Contains(c.UserId) && c.CompetitionDiscipline.League.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && !c.CompetitionDiscipline.IsDeleted && c.CompetitionResult.FirstOrDefault() != null && !(c.CompetitionResult.FirstOrDefault().Result == null || c.CompetitionResult.FirstOrDefault().Result.Trim() == string.Empty) && c.CompetitionResult.FirstOrDefault().AlternativeResult != AlternativeResultInt).ToList() : new List<CompetitionDisciplineRegistration>();

            foreach (var tp in players)
            {
                var competitionCount = 0;
                if (sectionAlias.Equals(GamesAlias.Athletics) == true)
                {
                     // value for alternative result column if player did not start/show               
                    var athleticRegistrationCount = competitionDisciplineRegistrations
                        .Where(c => c.UserId == tp.UserId)
                        .GroupBy(r => r.CompetitionDiscipline.League.LeagueId)
                        .Select(r => r.First())
                        .Count();
                    
                    competitionCount = athleticRegistrationCount;
                }
                else if (sectionAlias.Equals(GamesAlias.Gymnastic) == true)
                {
                    var gymnasticCompetitionCount = tp.User.CompetitionRegistrations
                                                    .Where(x => x.SeasonId == seasonId && (x.FinalScore.HasValue || x.Position.HasValue))
                                                    .GroupBy(x => x.LeagueId)
                                                    .Select(x => x.First())
                                                    .Count();
                    competitionCount = gymnasticCompetitionCount;
                }
                else if (sectionAlias.Equals(GamesAlias.Tennis) == true)
                {
                    competitionCount = tp.CompetitionParticipationCount ?? 0;
                }
                else
                {
                    var otherCompetitionCount = tp.User.SportsRegistrations
                        .Where(x => x.SeasonId == seasonId && (x.IsApproved || (x.FinalScore.HasValue || x.Position.HasValue)))
                        .GroupBy(x => x.LeagueId)
                        .Select(x => x.First())
                        .Count();
                    competitionCount = otherCompetitionCount;
                }

                var registration = registrations.FirstOrDefault(x => x.PlayerId == tp.UserId && x.TeamId == tp.TeamId);
                var groupedUsers = (new List<TeamsPlayer> { tp }).GroupBy(t => t.UserId);
                foreach (var groupedUser in groupedUsers)
                {
                    result.Add(new PlayerViewModel
                    {
                        Id = tp.Id,
                        UserId = tp.UserId,
                        IsActive = tp.IsActive,
                        IsPlayerRegistered = registration != null,
                        IsPlayerRegistrationApproved = registration?.IsActive == true,
                        IsApproveChecked = tp.IsApprovedByManager == true,
                        IsNotApproveChecked = tp.IsApprovedByManager == false,
                        IsActivePlayer = competitionCount >= 4,
                        IsNotActive = !tp.IsActive,
                        IsFemale = tp.User.GenderId == 0,
                        TeamId = tp.TeamId,
                        TenicardValidityDate = tp.User?.TenicardValidity
                        //IsLocked = tp.IsLocked ?? SetPlayersLockedStatus(tp.Id),
                    });
                }
            }

            return result;
        }

        private bool shouldCheckColumn(List<string> list, string column)
        {
            if (list == null || list.Count() == 0 || list.ElementAt(0) == "null" || list.Contains(column))
            {
                return true;
            }
            return false;
        }



        public void UpdateMedicalCertificatesToUnion(int unionId)
        {
            var seasonsRep = new SeasonsRepo(db);
            var lastSeasonForUnion = seasonsRep.GetCurrentByUnionId(unionId);
            var union = db.Unions.FirstOrDefault(u => u.UnionId == unionId);
            var clubTeams = db.ClubTeams
                        .Include(x => x.Club)
                        .Include(x => x.Team)
                        .Include(x => x.Season)
                        .Where(x => x.Club.UnionId == unionId && !x.Team.IsArchive);
            var leagueTeams = db.LeagueTeams
                .Include(x => x.Leagues)
                .Include(x => x.Teams)
                .Include(x => x.Season)
                .Where(x => x.Leagues.UnionId == unionId && !x.Teams.IsArchive);

            var clubTeamsIds = clubTeams?.Select(c => c.Team.TeamId);
            clubTeamsIds = clubTeamsIds.Union(leagueTeams?.Select(t => t.Teams.TeamId))?.Distinct();

            var groupPlayers = union.Section.Alias == SectionAliases.Gymnastic ||
               union.Section.Alias == SectionAliases.Tennis ||
               union.Section.Alias == SectionAliases.Bicycle;


            var playersBase = db.TeamsPlayers.Where(tp =>
                clubTeamsIds.Contains(tp.TeamId) &&
                tp.User.IsArchive == false &&
                tp.SeasonId == lastSeasonForUnion &&
                (!tp.Team.LeagueTeams.Any() || tp.ClubId == null) &&
                (tp.LeagueId == null ||
                 tp.Team.LeagueTeams.Any(lt => lt.SeasonId == lastSeasonForUnion && lt.LeagueId == tp.LeagueId)))
                    .Include(tp => tp.User)
                    .Include(tp => tp.User.PlayersBlockade)
                    .Include(tp => tp.User.MedicalCertApprovements);



            if (union.Section.Alias == GamesAlias.Tennis)
            {
                //Tennis specific: filter teams by team registrations checking also that the league is not deleted
                playersBase = playersBase.Where(tp =>
                    !tp.Team.TeamRegistrations.Any() ||
                    tp.Team.TeamRegistrations.Any(tr => tr.ClubId == tp.ClubId &&
                                                       tr.SeasonId == lastSeasonForUnion &&
                                                       !tr.IsDeleted &&
                                                       !tr.League.IsArchive));
            }

            if (groupPlayers)
            {
                var groupedPlayers = playersBase
                    .GroupBy(x => x.UserId)
                    .Where(x => x.Count() > 1)
                    .SelectMany(x => x)
                    .Select(x => new { teamId = x.TeamId, userId = x.UserId, clubId = x.ClubId })
                    .ToList();

                var teamsIds = groupedPlayers.Select(x => x.teamId).Distinct().ToArray();
                var clubsIds = groupedPlayers.Select(x => x.clubId).Distinct().ToArray();

                var teams = db.Teams
                    .Include(x => x.TeamsDetails)
                    .Include(x => x.ClubTeams)
                    .Include(x => x.LeagueTeams)
                    .Where(x => teamsIds.Contains(x.TeamId))
                    .AsNoTracking()
                    .ToList();

                var clubs = db.Clubs
                    .Where(x => clubsIds.Contains(x.ClubId))
                    .AsNoTracking()
                    .ToList();
                playersBase = playersBase.GroupBy(x => x.UserId).Select(x => x.FirstOrDefault());
            }

            var playersList = playersBase.ToList();

            var foundExpiredValidity = false;

            foreach (var tp in playersList)
            {
                switch (union.Section.Alias)
                {
                    case GamesAlias.Tennis:
                        if (tp.IsApprovedByManager.HasValue && tp.IsApprovedByManager.Value)
                        {
                            if (((!tp.User.MedExamDate.HasValue || tp.User.MedExamDate.Value < DateTime.Now) || (!tp.User.TenicardValidity.HasValue || tp.User.TenicardValidity.Value < DateTime.Now)))
                            {
                                foundExpiredValidity = true;
                                tp.IsApprovedByManager = null;
                                tp.User.MedExamDate = null;
                                var medcert = tp.User.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == lastSeasonForUnion);
                                if (medcert != null)
                                    medcert.Approved = false;

                                var medfiles = tp.User.PlayerFiles.Where(x => x.SeasonId == lastSeasonForUnion && x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive).ToList();
                                foreach (var medfile in medfiles)
                                {
                                    medfile.IsArchive = true;
                                }
                            }
                            if (tp.User.MedExamDate.HasValue && tp.User.MedExamDate.Value.AddMonths(-1) < DateTime.Now)
                            {
                                var medsAlerts = tp.User.PlayersBlockades.Where(b => b.BType == BlockadeType.MedicalExpiration && b.EndDate > DateTime.Now);
                                if (medsAlerts.Count() == 0)
                                {
                                    db.PlayersBlockades.Add(new PlayersBlockade
                                    {
                                        UserId = tp.UserId,
                                        StartDate = DateTime.Now,
                                        EndDate = tp.User.MedExamDate.Value,
                                        IsActive = false,
                                        BType = BlockadeType.MedicalExpiration,
                                        SeasonId = lastSeasonForUnion
                                    });
                                    foundExpiredValidity = true;
                                }

                            }
                        }
                        break;
                    case GamesAlias.Gymnastic:
                        if (tp.IsApprovedByManager.HasValue && tp.IsApprovedByManager.Value)
                        {
                            if (tp.User.MedExamDate.HasValue && tp.User.MedExamDate.Value.AddMonths(12) < DateTime.Now)
                            {
                                foundExpiredValidity = true;
                                tp.IsApprovedByManager = null;
                                var medcert = tp.User.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == lastSeasonForUnion);
                                if (medcert != null) medcert.Approved = false;
                            }
                            if (tp.User.MedExamDate.HasValue && tp.User.MedExamDate.Value.AddMonths(11) < DateTime.Now && tp.User.MedExamDate.Value.AddMonths(12) > DateTime.Now)
                            {
                                var medsAlerts = tp.User.PlayersBlockades.Where(b => b.BType == BlockadeType.MedicalExpiration && b.EndDate > DateTime.Now);
                                if (medsAlerts.Count() == 0)
                                {
                                    db.PlayersBlockades.Add(new PlayersBlockade
                                    {
                                        UserId = tp.UserId,
                                        StartDate = DateTime.Now,
                                        EndDate = tp.User.MedExamDate.Value.AddMonths(12),
                                        IsActive = false,
                                        BType = BlockadeType.MedicalExpiration,
                                        SeasonId = lastSeasonForUnion
                                    });
                                    foundExpiredValidity = true;
                                }

                            }

                        }

                        break;
                    case GamesAlias.Bicycle:
                        if (tp.IsApprovedByManager.HasValue && tp.IsApprovedByManager.Value)
                        {
                            if (tp.User.MedExamDate.HasValue && tp.User.MedExamDate.Value < DateTime.Now)
                            {
                                foundExpiredValidity = true;
                                tp.IsApprovedByManager = null;
                                var medcert = tp.User.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == lastSeasonForUnion);
                                if(medcert != null) medcert.Approved = false;
                            }
                            if (tp.User.MedExamDate.HasValue && tp.User.MedExamDate.Value.AddMonths(-1) < DateTime.Now)
                            {
                                var medsAlerts = tp.User.PlayersBlockades.Where(b => b.BType == BlockadeType.MedicalExpiration && b.EndDate > DateTime.Now);
                                if (medsAlerts.Count() == 0)
                                {
                                    db.PlayersBlockades.Add(new PlayersBlockade
                                    {
                                        UserId = tp.UserId,
                                        StartDate = DateTime.Now,
                                        EndDate = tp.User.MedExamDate.Value,
                                        IsActive = false,
                                        BType = BlockadeType.MedicalExpiration,
                                        SeasonId = lastSeasonForUnion
                                    });
                                    foundExpiredValidity = true;
                                }

                            }

                        }

                        break;
                    case GamesAlias.BasketBall:
                        if (tp.IsApprovedByManager.HasValue && tp.IsApprovedByManager.Value)
                        {
                            if (tp.User.MedExamDate.HasValue && tp.User.MedExamDate.Value.AddMonths(12) < DateTime.Now)
                            {
                                foundExpiredValidity = true;
                                tp.IsApprovedByManager = null;
                                var medcert = tp.User.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == lastSeasonForUnion);
                                if (medcert != null)
                                {
                                    medcert.Approved = false;
                                }
                            }
                            if (tp.User.MedExamDate.HasValue && tp.User.MedExamDate.Value.AddMonths(12).AddDays(-7) < DateTime.Now && tp.User.MedExamDate.Value.AddMonths(12) > DateTime.Now)
                            {
                                var medsAlerts = tp.User.PlayersBlockades.Where(b => b.BType == BlockadeType.MedicalExpiration && b.EndDate > DateTime.Now);
                                if (medsAlerts.Count() == 0)
                                {
                                    db.PlayersBlockades.Add(new PlayersBlockade
                                    {
                                        UserId = tp.UserId,
                                        StartDate = DateTime.Now,
                                        EndDate = tp.User.MedExamDate.Value.AddMonths(12),
                                        IsActive = false,
                                        BType = BlockadeType.MedicalExpiration,
                                        SeasonId = lastSeasonForUnion
                                    });
                                    foundExpiredValidity = true;
                                }
                            }
                            //check insurance for player 
                            if (tp.User.DateOfInsurance.HasValue && tp.User.DateOfInsurance.Value.AddMonths(12) < DateTime.Now)
                            {
                                foundExpiredValidity = true;
                                tp.IsApprovedByManager = null;
                            }
                            if (tp.User.DateOfInsurance.HasValue && tp.User.DateOfInsurance.Value.AddMonths(12).AddDays(-7) < DateTime.Now && tp.User.DateOfInsurance.Value.AddMonths(12) > DateTime.Now)
                            {
                                var insuranceAlerts = tp.User.PlayersBlockades.Where(b => b.BType == BlockadeType.InsuranceExpiration && b.EndDate > DateTime.Now);
                                if (insuranceAlerts.Count() == 0)
                                {
                                    db.PlayersBlockades.Add(new PlayersBlockade
                                    {
                                        UserId = tp.UserId,
                                        StartDate = DateTime.Now,
                                        EndDate = tp.User.DateOfInsurance.Value.AddMonths(12),
                                        IsActive = false,
                                        BType = BlockadeType.InsuranceExpiration,
                                        SeasonId = lastSeasonForUnion
                                    });
                                    foundExpiredValidity = true;
                                }
                            }
                        }
                        break;
                    case GamesAlias.Rowing:

                        if (tp.IsApprovedByManager.HasValue && tp.IsApprovedByManager.Value)
                        {
                            /*
                            if (tp.User.MedExamDate.HasValue && tp.User.MedExamDate.Value < DateTime.Now)
                            {
                                foundExpiredValidity = true;
                                tp.IsApprovedByManager = null;
                                var medcert = tp.User.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == lastSeasonForUnion);
                                medcert.Approved = false;
                            }
                            */
                            if (tp.User.MedExamDate.HasValue && tp.User.MedExamDate.Value.AddMonths(-1) < DateTime.Now && tp.User.MedExamDate.Value > DateTime.Now)
                            {
                                var medsAlerts = tp.User.PlayersBlockades.Where(b => b.BType == BlockadeType.MedicalExpiration && b.EndDate > DateTime.Now);
                                if (medsAlerts.Count() == 0)
                                {
                                    db.PlayersBlockades.Add(new PlayersBlockade
                                    {
                                        UserId = tp.UserId,
                                        StartDate = DateTime.Now,
                                        EndDate = tp.User.MedExamDate.Value,
                                        IsActive = false,
                                        BType = BlockadeType.MedicalExpiration,
                                        SeasonId = lastSeasonForUnion
                                    });
                                    foundExpiredValidity = true;
                                }

                            }

                        }

                        break;
                }

            }
            if (foundExpiredValidity)
            {
                Save();
            }

        }





        public void UpdateMedicalCertificatesToSection(int sectionId)
        {
            var unions = db.Unions.Where(u => u.SectionId == sectionId && !u.IsArchive).ToList();
            foreach (var union in unions)
            {
                UpdateMedicalCertificatesToUnion(union.UnionId);
            }
        }

        public int GetITennisCount(List<int> clubTeamIds, int? seasonId, int? unionId, bool isClubsBlocked = true, bool onlyActive = true)
        {

            var playersBase = db.TeamsPlayers.Where(tp =>
                    clubTeamIds.Contains(tp.TeamId) &&
                    tp.User.IsArchive == false &&
                    tp.SeasonId == seasonId)
                .Include(tp => tp.User)
                .Include(tp => tp.Team);
            playersBase = playersBase
                    .Where(tp => (!tp.Team.LeagueTeams.Any() || tp.ClubId == null) &&
                                 (tp.LeagueId == null ||
                                  tp.Team.LeagueTeams.Any(lt =>
                                      lt.SeasonId == seasonId && lt.LeagueId == tp.LeagueId)));
            
          playersBase = playersBase.Where(tp =>
                    !tp.Team.TeamRegistrations.Any() ||
                    tp.Team.TeamRegistrations.Any(tr => tr.ClubId == tp.ClubId &&
                                                        tr.SeasonId == seasonId &&
                                                        !tr.IsDeleted &&
                                                        !tr.League.IsArchive));
           var teamsOfGroupedPlayers = new List<Tuple<int, Team, Club>>();
            var groupedPlayers = playersBase
               .GroupBy(x => x.UserId)
               .Where(x => x.Count() > 1)
               .SelectMany(x => x)
               .Select(x => new { teamId = x.TeamId, userId = x.UserId, clubId = x.ClubId })
               .ToList();

           var teamsIds = groupedPlayers.Select(x => x.teamId).Distinct().ToArray();
           var clubsIds = groupedPlayers.Select(x => x.clubId).Distinct().ToArray();

           var teams = db.Teams
               .Where(x => teamsIds.Contains(x.TeamId))
               .AsNoTracking()
               .ToList();

           var clubs = db.Clubs
               .Where(x => clubsIds.Contains(x.ClubId))
               .AsNoTracking()
               .ToList();

           foreach (var groupedPlayer in groupedPlayers)
           {
               var playerTeam = teams.FirstOrDefault(x => x.TeamId == groupedPlayer.teamId);
               var playerClub = clubs.FirstOrDefault(x => x.ClubId == groupedPlayer.clubId);

               if (playerTeam != null)
               {
                   teamsOfGroupedPlayers.Add(new Tuple<int, Team, Club>(groupedPlayer.userId, playerTeam, playerClub));
               }
           }

           playersBase = playersBase.GroupBy(x => x.UserId).Select(x => x.FirstOrDefault());
           return playersBase.Count(x => x.User!=null && x.User.TenicardValidity != null && x.User.TenicardValidity > DateTime.Now);
        }

        public bool CheckIfFileExits(PlayerFileType insurance, int userId)
        {
            return db.PlayerFiles.Where(x => x.PlayerId == userId && x.FileType == (int)insurance).FirstOrDefault() != null;
        }

        public List<PlayerViewModel> GetTeamPlayersByClubTeamIds(List<int> clubTeamIds, int? seasonId,
            string searchText, List<int> filterDisciplines, List<int> filterStatus, List<string> searchColumnIds, string sortBy, string sortDirection,
            int? skip, int? take, string sectionAlias, int? unionId, out int playersCount,  bool isClubsBlocked = true, bool onlyActive = true)
        {

            var playersBase = db.TeamsPlayers.Where(tp =>
                    clubTeamIds.Contains(tp.TeamId) &&
                    tp.User.IsArchive == false &&
                    tp.SeasonId == seasonId)
                .Include(tp => tp.User)
                .Include(tp => tp.Team);
            //.Include(tp => tp.User.PlayerAchievements); // DONE
            //.Include(tp => tp.User.TeamsPlayers) // i dont think we need this
            //.Include(tp => tp.User.PlayersBlockade) DONE
            if (sectionAlias != GamesAlias.MartialArts)
            {
                playersBase = playersBase
                .Where(tp => (!tp.Team.LeagueTeams.Any() || tp.ClubId == null) &&
                    (tp.LeagueId == null ||
                     tp.Team.LeagueTeams.Any(lt => lt.SeasonId == seasonId && lt.LeagueId == tp.LeagueId)));
            }

            if (sortBy == "CompetitionCount") {
                playersBase = playersBase.Include(tp => tp.User.CompetitionRegistrations);
            }

            if (sortBy == "EndBlockadeDateString")
            {
                playersBase = playersBase.Include(tp => tp.User.PlayersBlockade);
            }

            

            if (sectionAlias != GamesAlias.Athletics)
            {
                playersBase = playersBase
                    .Include(tp => tp.User.PlayersBlockade);
            }
            
            if (sectionAlias == GamesAlias.Athletics && (sortBy == "AthletesNumbers" || sortBy == "IsAthleteNumberProduced"))
            {
                playersBase = playersBase.Include("User.AthleteNumbers");
            }
            
            var groupPlayers = sectionAlias == SectionAliases.Gymnastic ||
                               sectionAlias == SectionAliases.Tennis;


            var isKaratee = sectionAlias == SectionAliases.MartialArts && unionId == 37; //karatee union need started with league relations so we eed to make it behave as individual in stats and players.

            if (sectionAlias == GamesAlias.Athletics || sectionAlias == GamesAlias.WeightLifting)
            {
                //playersBase = playersBase.Include(tp => tp.User.CompetitionDisciplineRegistrations);
            }
            /*
            if (sectionAlias == GamesAlias.Athletics)
            {
                playersBase = playersBase.Include("User.CompetitionDisciplineRegistrations.CompetitionDiscipline")
                    .Include("User.CompetitionDisciplineRegistrations.CompetitionDiscipline.League")
                    .Include("User.CompetitionDisciplineRegistrations.CompetitionResult");
            }
            */
            if (sectionAlias == GamesAlias.MartialArts)
            {
                playersBase = playersBase.Include(tp => tp.User.SportsRegistrations);
            }
            if (sectionAlias == GamesAlias.Climbing && onlyActive && isClubsBlocked)
            {
                playersBase = playersBase.Where(tp => tp.IsActive == true);
            }
            if (sectionAlias == GamesAlias.Athletics && onlyActive && isClubsBlocked)
            {
                playersBase = playersBase.Where(tp => tp.IsActive == true || tp.IsApprovedByManager == true);
            }
            
            if (sectionAlias == GamesAlias.Gymnastic)
            {
                playersBase = playersBase
                    .Include(tp => tp.User.UsersRoutes)
                    .Include(tp => tp.User.TeamsRoutes)
                    .Include(tp => tp.User.UsersRanks)
                    .Include(tp => tp.User.TeamsRanks)
                    .Include(tp => tp.User.PlayerDisciplines);
            }
            else if (sectionAlias != GamesAlias.Athletics)
            {
                playersBase = playersBase
                    .Include(tp => tp.League)
                    .Include(tp => tp.Position)
                    .Include(tp => tp.User.ActivityFormsSubmittedDatas);
            }

            if (string.Equals(sectionAlias, SectionAliases.Tennis, StringComparison.CurrentCultureIgnoreCase))
            {
                //Tennis specific: filter teams by team registrations checking also that the league is not deleted
                playersBase = playersBase.Where(tp =>
                    !tp.Team.TeamRegistrations.Any() ||
                    tp.Team.TeamRegistrations.Any(tr => tr.ClubId == tp.ClubId &&
                                                       tr.SeasonId == seasonId &&
                                                       !tr.IsDeleted &&
                                                       !tr.League.IsArchive));
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                IQueryable<TeamsPlayer> filter = null;
                List<IQueryable<TeamsPlayer>> filterList = new List<IQueryable<TeamsPlayer>>();
                var words = searchText.Split(' ').ToList();
                var word1 = words.ElementAtOrDefault(0) ?? "";
                var word2 = words.ElementAtOrDefault(1) ?? "";
                var word3 = words.ElementAtOrDefault(2) ?? "";
                var word4 = words.ElementAtOrDefault(3) ?? "";
                if (shouldCheckColumn(searchColumnIds, "FullName"))
                {
                    filterList.Add(playersBase.Where(tp =>
                        (string.Concat(tp.User.FirstName, " ", tp.User.MiddleName, " ", tp.User.LastName)
                            .Contains(word1)) &&
                        (string.Concat(tp.User.FirstName, " ", tp.User.MiddleName, " ", tp.User.LastName)
                            .Contains(word2)) &&
                        (string.Concat(tp.User.FirstName, " ", tp.User.MiddleName, " ", tp.User.LastName)
                            .Contains(word3)) &&
                        (string.Concat(tp.User.FirstName, " ", tp.User.MiddleName, " ", tp.User.LastName)
                            .Contains(word4))
                    ));
                }
                if (shouldCheckColumn(searchColumnIds, "IdentNum"))
                {
                    filterList.Add(playersBase.Where(tp => tp.User.IdentNum.Contains(searchText)));
                }
                if (shouldCheckColumn(searchColumnIds, "Email"))
                {
                    filterList.Add(playersBase.Where(tp => tp.User.Email.Contains(searchText)));
                }

                if (shouldCheckColumn(searchColumnIds, "AthletesNumbers"))
                {
                    int searchNumber;
                    if (int.TryParse(searchText, out searchNumber))
                    {
                        filterList.Add(playersBase.Where(tp =>
                            tp.User.AthleteNumbers.Any(x => x.SeasonId == seasonId && x.AthleteNumber1 == searchNumber)));
                    }
                }

                foreach (var item in filterList)
                {
                    if (filter == null)
                    {
                        filter = item;
                    }
                    else
                    {
                        filter = filter.Union(item);
                    }

                }
                playersBase = filter;

            }


            if (filterDisciplines?.Any() == true)
            {
                playersBase = playersBase.Include("User.PlayerDisciplines");
                playersBase = playersBase.Where(x =>
                    x.User.PlayerDisciplines.Where(d => d.ClubId == x.ClubId && d.SeasonId == x.SeasonId).Select(d => d.DisciplineId).Intersect(filterDisciplines).Any());
            }

            if (filterStatus?.Any() == true)
            {
                Expression<Func<TeamsPlayer, bool>> exp = null;

                if (filterStatus.Contains(1))
                {
                    exp = exp.Or(
                        tp => tp.IsActive &&
                              tp.IsApprovedByManager != true &&
                              tp.IsApprovedByManager != false);
                }

                if (filterStatus.Contains(2))
                {
                    exp = exp.Or(tp => tp.IsApprovedByManager == true);
                }

                if (filterStatus.Contains(3))
                {
                    exp = exp.Or(tp => tp.IsApprovedByManager == false);
                }

                if (filterStatus.Contains(4))
                {
                    exp = exp.Or(tp => !tp.IsActive);
                }

                if (filterStatus.Contains(5))
                {
                    exp = exp.Or(tp => tp.User.BlockadeId.HasValue);
                }

                if (filterStatus.Contains(6))
                {
                    exp = exp.Or(tp => tp.IsActive &&
                                        tp.IsApprovedByManager != true &&
                                        tp.IsApprovedByManager != false &&
                                        (sectionAlias != SectionAliases.Tennis || (tp.User.TenicardValidity != null && tp.User.TenicardValidity > DateTime.Now && (tp.User.MedExamDate == null || tp.User.MedExamDate.Value > DateTime.Now))) &&
                                        tp.User.PlayerFiles.Any(x =>
                                            x.FileType ==
                                            (int)PlayerFileType.MedicalCertificate &&
                                            x.SeasonId == tp.SeasonId && !x.IsArchive));
                }

                if (exp != null)
                {
                    playersBase = playersBase.Where(exp);
                }
            }

            var teamsOfGroupedPlayers = new List<Tuple<int, Team, Club>>();
            if (groupPlayers || isKaratee)
            {
                var groupedPlayers = playersBase
                    .GroupBy(x => x.UserId)
                    .Where(x => x.Count() > 1)
                    .SelectMany(x => x)
                    .Select(x => new { teamId = x.TeamId, userId = x.UserId, clubId = x.ClubId })
                    .ToList();

                var teamsIds = groupedPlayers.Select(x => x.teamId).Distinct().ToArray();
                var clubsIds = groupedPlayers.Select(x => x.clubId).Distinct().ToArray();

                var teams = db.Teams
                    .Include(x => x.TeamsDetails)
                    .Include(x => x.ClubTeams)
                    .Include(x => x.LeagueTeams)
                    .Where(x => teamsIds.Contains(x.TeamId))
                    .AsNoTracking()
                    .ToList();

                var clubs = db.Clubs
                    .Where(x => clubsIds.Contains(x.ClubId))
                    .AsNoTracking()
                    .ToList();

                foreach (var groupedPlayer in groupedPlayers)
                {
                    var playerTeam = teams.FirstOrDefault(x => x.TeamId == groupedPlayer.teamId);
                    var playerClub = clubs.FirstOrDefault(x => x.ClubId == groupedPlayer.clubId);

                    if (playerTeam != null)
                    {
                        teamsOfGroupedPlayers.Add(new Tuple<int, Team, Club>(groupedPlayer.userId, playerTeam, playerClub));
                    }
                }

                playersBase = playersBase.GroupBy(x => x.UserId).Select(x => x.FirstOrDefault());
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy + "." + sortDirection)
                {
                    case "FullName.asc":
                        playersBase = playersBase.OrderBy(p => string.Concat(p.User.FirstName, " ", p.User.MiddleName, " ", p.User.LastName));
                        break;
                    case "FullName.desc":
                        playersBase = playersBase.OrderByDescending(p => string.Concat(p.User.FirstName, " ", p.User.MiddleName, " ", p.User.LastName));
                        break;
                    case "BirthdayString.asc":
                        playersBase = playersBase.OrderBy(p => p.User.BirthDay);
                        break;
                    case "BirthdayString.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.BirthDay);
                        break;
                    case "ShirtNum.asc":
                        playersBase = playersBase.OrderBy(p => p.ShirtNum);
                        break;
                    case "ShirtNum.desc":
                        playersBase = playersBase.OrderByDescending(p => p.ShirtNum);
                        break;
                    case "PosId.asc":
                        playersBase = playersBase.OrderBy(p => p.Position.Title);
                        break;
                    case "PosId.desc":
                        playersBase = playersBase.OrderByDescending(p => p.Position.Title);
                        break;
                    case "IdentNum.asc":
                        playersBase = playersBase.OrderBy(p => p.User.IdentNum);
                        break;
                    case "IdentNum.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.IdentNum);
                        break;
                    case "Email.asc":
                        playersBase = playersBase.OrderBy(p => p.User.Email);
                        break;
                    case "Email.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.Email);
                        break;
                    case "Phone.asc":
                        playersBase = playersBase.OrderBy(p => p.User.Telephone);
                        break;
                    case "Phone.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.Telephone);
                        break;
                    case "City.asc":
                        playersBase = playersBase.OrderBy(p => p.User.City);
                        break;
                    case "City.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.City);
                        break;
                    case "Height.asc":
                        playersBase = playersBase.OrderBy(p => p.User.Height);
                        break;
                    case "Height.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.Height);
                        break;
                    case "Weight.asc":
                        playersBase = playersBase.OrderBy(p => p.User.Weight);
                        break;
                    case "Weight.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.Weight);
                        break;
                    case "Gender.asc":
                        playersBase = playersBase.OrderBy(p => p.User.GenderId);
                        break;
                    case "Gender.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.GenderId);
                        break;
                    case "ParentName.asc":
                        playersBase = playersBase.OrderBy(p => p.User.ParentName);
                        break;
                    case "ParentName.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.ParentName);
                        break;
                    case "ClubName.asc":
                        playersBase = playersBase.OrderBy(p => p.Team.ClubTeams.FirstOrDefault(t => t.TeamId == p.TeamId).Club.Name ?? "");
                        break;
                    case "ClubName.desc":
                        playersBase = playersBase.OrderByDescending(p => p.Team.ClubTeams.FirstOrDefault(t => t.TeamId == p.TeamId).Club.Name ?? "");
                        break;
                    case "StartPlaying.asc":
                        playersBase = playersBase.OrderBy(p => p.StartPlaying);
                        break;
                    case "StartPlaying.desc":
                        playersBase = playersBase.OrderByDescending(p => p.StartPlaying);
                        break;
                    case "EndBlockadeDateString.asc":
                        playersBase = playersBase.OrderBy(p => p.User.PlayersBlockade.EndDate);
                        break;
                    case "EndBlockadeDateString.desc":
                        playersBase = playersBase.OrderByDescending(p => p.User.PlayersBlockade.EndDate);
                        break;
                    case "ShirtSize.asc":
                        playersBase = playersBase.OrderBy(p => p.User.ShirtSize == "3XL")
                            .ThenBy(p => p.User.ShirtSize == "2XL")
                            .ThenBy(p => p.User.ShirtSize == "XL")
                            .ThenBy(p => p.User.ShirtSize == "L")
                            .ThenBy(p => p.User.ShirtSize == "M")
                            .ThenBy(p => p.User.ShirtSize == "S")
                            .ThenBy(p => p.User.ShirtSize == null);
                        break;
                    case "ShirtSize.desc":
                        playersBase = playersBase.OrderBy(p => p.User.ShirtSize == null)
                          .ThenBy(p => p.User.ShirtSize == "S")
                          .ThenBy(p => p.User.ShirtSize == "M")
                          .ThenBy(p => p.User.ShirtSize == "L")
                          .ThenBy(p => p.User.ShirtSize == "XL")
                          .ThenBy(p => p.User.ShirtSize == "2XL")
                          .ThenBy(p => p.User.ShirtSize == "3XL");
                        break;
                    case "BaseHandicap.asc":
                        playersBase = playersBase.OrderBy(p => p.HandicapLevel);
                        break;
                    case "BaseHandicap.desc":
                        playersBase = playersBase.OrderByDescending(p => p.HandicapLevel);
                        break;
                    case "UnionComment.asc":
                        playersBase = playersBase.OrderBy(p => p.UnionComment);
                        break;
                    case "UnionComment.desc":
                        playersBase = playersBase.OrderByDescending(p => p.UnionComment);
                        break;
                    case "ClubComment.asc":
                        playersBase = playersBase.OrderBy(p => p.ClubComment);
                        break;
                    case "ClubComment.desc":
                        playersBase = playersBase.OrderByDescending(p => p.ClubComment);
                        break;
                    case "TeamName.asc":
                    case "TrainingTeamName.asc":
                    case "LeagueTeamName.asc":
                        playersBase = playersBase.OrderBy(p => p.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId).TeamName ?? p.Team.Title);
                        break;
                    case "TeamName.desc":
                    case "TrainingTeamName.desc":
                    case "LeagueTeamName.desc":
                        playersBase = playersBase.OrderByDescending(p => p.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId).TeamName ?? p.Team.Title);
                        break;
                    case "LeagueName.asc":
                        playersBase = playersBase.OrderBy(p => p.League.Name);
                        break;
                    case "LeagueName.desc":
                        playersBase = playersBase.OrderByDescending(p => p.League.Name);
                        break;
                    case "CompetitionCount.asc":
                        if (sectionAlias == GamesAlias.Athletics)
                        {
                            int AlternativeResultInt = 3; // value for alternative result column if player did not start/show
                            playersBase = playersBase.OrderBy(p => p.User.SportsRegistrations.Count(r => !r.League.IsArchive && r.IsApproved || (r.FinalScore.HasValue || r.Position.HasValue))
                            + p.User.CompetitionDisciplineRegistrations.Where(c => c.CompetitionDiscipline.League.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && !c.CompetitionDiscipline.IsDeleted && c.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(c.CompetitionResult.FirstOrDefault().Result) && c.CompetitionResult.FirstOrDefault().AlternativeResult != AlternativeResultInt).Select(c => c.CompetitionDiscipline.CompetitionId).Distinct().Count()
                            );
                        }
                        else if (sectionAlias == GamesAlias.Tennis)
                        {
                            playersBase = playersBase.OrderBy(p => p.CompetitionParticipationCount);
                        }
                        else
                        {
                            playersBase = playersBase.OrderBy(p => p.User.SportsRegistrations.Count(r => !r.League.IsArchive && r.IsApproved || (r.FinalScore.HasValue || r.Position.HasValue))
                            + p.User.CompetitionRegistrations.Count(c => !c.League.IsArchive && c.League.SeasonId == seasonId && c.League.Season.IsActive && (c.IsActive && c.IsRegisteredByExcel) || c.FinalScore.HasValue || c.Position.HasValue)
                            );
                        }
                        break;
                    case "CompetitionCount.desc":
                        if (sectionAlias == GamesAlias.Athletics)
                        {
                            int AlternativeResultInt = 3; // value for alternative result column if player did not start/show
                            playersBase = playersBase.OrderByDescending(p => p.User.SportsRegistrations.Count(r => !r.League.IsArchive && r.IsApproved || (r.FinalScore.HasValue || r.Position.HasValue))
                            + p.User.CompetitionDisciplineRegistrations.Where(c => c.CompetitionDiscipline.League.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && !c.CompetitionDiscipline.IsDeleted && c.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(c.CompetitionResult.FirstOrDefault().Result) && c.CompetitionResult.FirstOrDefault().AlternativeResult != AlternativeResultInt).Select(c => c.CompetitionDiscipline.CompetitionId).Distinct().Count()
                            );
                        }
                        else if (sectionAlias == GamesAlias.Tennis)
                        {
                            playersBase = playersBase.OrderByDescending(p => p.CompetitionParticipationCount);
                        }
                        else
                        {
                            playersBase = playersBase.OrderByDescending(p => p.User.SportsRegistrations.Count(r => !p.League.IsArchive && r.IsApproved || (r.FinalScore.HasValue || r.Position.HasValue))
                            + p.User.CompetitionRegistrations.Count(c => !c.League.IsArchive && c.League.SeasonId == seasonId && (c.IsActive && c.IsRegisteredByExcel) && c.FinalScore.HasValue || c.Position.HasValue)
                            );
                        }
                        break;
                    case "MedicalCertificateFile.asc":
                        playersBase = playersBase.OrderBy(x => x.User.PlayerFiles.FirstOrDefault(f =>
                                f.FileType == (int)PlayerFileType.MedicalCertificate && f.SeasonId == x.SeasonId && !f.IsArchive) == null);
                        break;
                    case "MedicalCertificateFile.desc":
                        playersBase = playersBase.OrderBy(x => x.User.PlayerFiles.FirstOrDefault(f =>
                                                                   f.FileType == (int)PlayerFileType.MedicalCertificate && f.SeasonId == x.SeasonId && !f.IsArchive) != null);
                        break;
                    case "MedExamDateString.asc":
                        playersBase = playersBase.OrderBy(x => x.User.MedExamDate);
                        break;
                    case "MedExamDateString.desc":
                        playersBase = playersBase.OrderByDescending(x => x.User.MedExamDate);
                        break;
                    case "TenicardValidity.asc":
                        playersBase = playersBase.OrderBy(x => x.User.TenicardValidity);
                        break;
                    case "TenicardValidity.desc":
                        playersBase = playersBase.OrderByDescending(x => x.User.TenicardValidity);
                        break;   
                    case "IsAthleteNumberProduced.asc":
                        playersBase = playersBase.OrderBy(x => x.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).IsAthleteNumberProduced);
                        break;
                    case "IsAthleteNumberProduced.desc":
                        playersBase = playersBase.OrderByDescending(x => x.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).IsAthleteNumberProduced);
                        break;
                    case "AthletesNumbers.asc":
                        playersBase = playersBase.OrderBy(x => x.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).AthleteNumber1);
                        break;
                    case "AthletesNumbers.desc":
                        playersBase = playersBase.OrderByDescending(x => x.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).AthleteNumber1);
                        break;
                    default:
                        playersBase = playersBase.OrderBy(x => x.User.FirstName).ThenBy(x => x.User.LastName);
                        break;
                }
            }
            else
            {
                playersBase = playersBase.OrderBy(x => x.User.FirstName).ThenBy(x => x.User.LastName);
            }
            //var playersCount = playersBase.Count(); maybe add it later if with further testing it doesnt affect performance
            //EDIT BTO 21/11/2019 - as request was pagination to be fixed:  Enabled count 
            playersCount = playersBase.Count();

            if (skip.HasValue)
            {
                playersBase = playersBase.Skip(skip.Value);
            }
            if (take.HasValue)
            {
                playersBase = playersBase.Take(take.Value);
            }

            var players = playersBase.ToList();

            var result = new List<PlayerViewModel>();
            var page = 0;
            var pageSize = 5000;

            var partOfPlayers = players.Skip(page * pageSize).Take(pageSize).ToList();

            while (partOfPlayers.Any())
            {
                result.AddRange(GetPlayersViewModel(partOfPlayers, seasonId, sectionAlias));

                page++;
                partOfPlayers = players.Skip(page * pageSize).Take(pageSize).ToList();
            }
            //var result = GetPlayersViewModel(players, seasonId, sectionAlias);

            if (groupPlayers)
            {
                foreach (var resultPlayer in result)
                {
                    var playerTeams = teamsOfGroupedPlayers
                        .Where(x => x.Item1 == resultPlayer.UserId)
                        .ToList();

                    if (playerTeams.Count > 1)
                    {
                        var teamsNames = new List<Tuple<string, bool, bool>>();
                        foreach (var team in playerTeams.Select(x => x.Item2))
                        {
                            var teamName = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)?.TeamName ??
                                           team.Title;
                            var isTrainingTeam =
                                team.ClubTeams.FirstOrDefault(x => x.SeasonId == seasonId)?.IsTrainingTeam == true;
                            var isCompetitionTeam =
                                team.LeagueTeams
                                    .FirstOrDefault(x => x.SeasonId == seasonId)
                                    ?.Leagues
                                    ?.EilatTournament == true;

                            teamsNames.Add(new Tuple<string, bool, bool>(teamName, isTrainingTeam, isCompetitionTeam));
                        }

                        if (string.Equals(sectionAlias, SectionAliases.Tennis, StringComparison.CurrentCultureIgnoreCase))
                        {
                            resultPlayer.TeamName = null;

                            resultPlayer.TrainingTeamName = string.Join(", ", teamsNames.Where(x => x.Item2 && !x.Item3).Select(x => x.Item1));
                            resultPlayer.LeagueTeamName = string.Join(", ", teamsNames.Where(x => !x.Item2 && !x.Item3).Select(x => x.Item1));
                        }
                        else
                        {
                            resultPlayer.TeamName = string.Join(", ", teamsNames.Select(x => x.Item1));
                        }

                        resultPlayer.ClubName = string.Join(", ", playerTeams.Where(x => x.Item3 != null).Select(x => x.Item3).Select(x => x.Name).Distinct());
                    }
                }
            }

            return result;
        }



        private void UpdateTenniTeamPlayerParticipationCount(TeamsPlayer sportsman, List<GamesCycle> regularGames, List<TennisGameCycle> tennisGames, Season season)
        {
            var result = 0;
            foreach (var game in regularGames)
            {
                if (game.GameStatus == GameStatus.Ended && game.TennisLeagueGames.Where(tg => tg.HomePlayerId == sportsman.UserId || tg.HomePairPlayerId == sportsman.UserId || tg.GuestPlayerId == sportsman.UserId || tg.GuestPairPlayerId == sportsman.UserId).Count() > 0 &&
                            (
                                (game.Stage.League.SeasonId == season.Id && game.StartDate < new DateTime(game.StartDate.Year, 9, 1))
                                || (season.PreviousSeasonId.HasValue && game.Stage.League.SeasonId == season.PreviousSeasonId && game.StartDate >= new DateTime(game.StartDate.Year - 1, 9, 1))
                            )
                           )
                {
                    result++;
                }
            }
            var players = sportsman.User.TeamsPlayers.Where(tp => tp.IsActive && (tp.SeasonId == season.Id || tp.SeasonId == season.PreviousSeasonId));
            foreach (var oneOfPlayers in players)
            {
                foreach (var game in tennisGames)
                {
                    if ((game.FirstPlayerId == oneOfPlayers.Id ||
                            game.SecondPlayerId == oneOfPlayers.Id ||
                            game.FirstPlayerPairId == oneOfPlayers.Id ||
                            game.SecondPlayerPairId == oneOfPlayers.Id)
                            && (
                                (game.TennisStage.SeasonId == season.Id && game.StartDate < new DateTime(game.StartDate.Year, 9, 1))
                                || (season.PreviousSeasonId.HasValue && game.TennisStage.SeasonId == season.PreviousSeasonId && game.StartDate >= new DateTime(game.StartDate.Year - 1, 9, 1))
                            )
                       )
                    {
                        result++;
                    }
                }
            }

            sportsman.CompetitionParticipationCount = result;
            foreach (var player in players)
            {
                player.CompetitionParticipationCount = result;
            }
        }


        public void UpdateTennisCompetitionParticipations(List<TotoCompetition> competitions, int seasonId)
        {
            var competitionIds = competitions.Where(l => l.Id.HasValue).Select(l => l.Id.Value).ToArray();
            var teamLeagues = new Dictionary<int, int>();
            var teamIds = new List<int>();
            foreach (var competition in competitions)
            {
                if (competition.IsCompetitionNotLeague)
                {
                    if (!competition.IsDailyCompetition)
                    {
                        teamIds.AddRange(competition.CategoryIds);
                    }
                    foreach (var categoryId in competition.CategoryIds)
                    {
                        teamLeagues[categoryId] = competition.Id.Value;
                    }
                }
            }

            List<GymnasticTotoValue> res = new List<GymnasticTotoValue>();
            var tennisSectionUnions = db.Unions
                            .Where(u => u.Section.Alias == GamesAlias.Tennis)
                            .Include(x => x.Leagues)
                            .Include(x => x.Leagues.Select(l => l.LeagueTeams))
                            .ToList();
            var tennisUnionsLeagues = tennisSectionUnions
                .SelectMany(x => x.Leagues.Where(l => !l.IsArchive))
                .ToList();
            var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
            var tennisGames = db.TennisGameCycles
                .Include(x => x.TennisStage)
                .Include("TeamsPlayer")
                .Include("TeamsPlayer1")
                .Include("TeamsPlayer11")
                .Include("TeamsPlayer3")
                .Where(t => t.GameStatus == GameStatus.Ended && teamIds.Contains(t.TennisStage.CategoryId))
                .ToList();
            var tennisLeaguesIds = tennisUnionsLeagues.Select(x => x.LeagueId).ToArray();
            var regularGames = db.GamesCycles
                .Include(x => x.TennisLeagueGames)
                .Include(x => x.Stage)
                .Where(g => tennisLeaguesIds.Contains(g.Stage.LeagueId) && g.GameStatus == GameStatus.Ended && competitionIds.Contains(g.Stage.LeagueId))
                .ToList();

            var dailyCompetitions = competitions.Where(c => c.IsDailyCompetition).ToList();

            var playersCompetitionCount = new Dictionary<int, int>();
            var playersCompetitionParticipation = new Dictionary<int, Dictionary<int, int>>();

            foreach (var game in regularGames)
            {
                var playersParticipatedInThisGame = new List<int>();
                foreach (var gameInCycle in game.TennisLeagueGames)
                {
                    if (gameInCycle.HomePlayerId.HasValue && !playersParticipatedInThisGame.Contains(gameInCycle.HomePlayerId.Value))
                        playersParticipatedInThisGame.Add(gameInCycle.HomePlayerId.Value);
                    if (gameInCycle.HomePairPlayerId.HasValue && !playersParticipatedInThisGame.Contains(gameInCycle.HomePairPlayerId.Value))
                        playersParticipatedInThisGame.Add(gameInCycle.HomePairPlayerId.Value);
                    if (gameInCycle.GuestPlayerId.HasValue && !playersParticipatedInThisGame.Contains(gameInCycle.GuestPlayerId.Value))
                        playersParticipatedInThisGame.Add(gameInCycle.GuestPlayerId.Value);
                    if (gameInCycle.GuestPairPlayerId.HasValue && !playersParticipatedInThisGame.Contains(gameInCycle.GuestPairPlayerId.Value))
                        playersParticipatedInThisGame.Add(gameInCycle.GuestPairPlayerId.Value);
                }
                foreach (var userId in playersParticipatedInThisGame)
                {
                    Dictionary<int, int> value2;
                    var isHasValue2 = playersCompetitionParticipation.TryGetValue(userId, out value2);
                    if (isHasValue2)
                    {
                        int value3;
                        var isHasValue3 = playersCompetitionParticipation[userId].TryGetValue(game.Stage.LeagueId, out value3);
                        if (isHasValue3)
                        {
                            playersCompetitionParticipation[userId][game.Stage.LeagueId]++;
                        }
                        else
                        {
                            playersCompetitionParticipation[userId][game.Stage.LeagueId] = 1;
                        }
                    }
                    else
                    {
                        playersCompetitionParticipation[userId] = new Dictionary<int, int>();
                        playersCompetitionParticipation[userId][game.Stage.LeagueId] = 1;
                    }
                }
            }

            foreach (var game in tennisGames)
            {
                if (game.GameStatus == GameStatus.Ended && ((game.TennisStage.SeasonId == season.Id && game.StartDate < new DateTime(game.StartDate.Year, 9, 1)) || (season.PreviousSeasonId.HasValue && game.TennisStage.SeasonId == season.PreviousSeasonId && game.StartDate >= new DateTime(game.StartDate.Year - 1, 9, 1))))
                {
                    var playersParticipatedInThisGame = new List<int>();
                    if (game.FirstPlayer?.UserId != null && !playersParticipatedInThisGame.Contains(game.FirstPlayer.UserId))
                        playersParticipatedInThisGame.Add(game.FirstPlayer.UserId);
                    if (game.SecondPlayer?.UserId != null && !playersParticipatedInThisGame.Contains(game.SecondPlayer.UserId))
                        playersParticipatedInThisGame.Add(game.SecondPlayer.UserId);
                    if (game.FirstPairPlayer?.UserId != null && !playersParticipatedInThisGame.Contains(game.FirstPairPlayer.UserId))
                        playersParticipatedInThisGame.Add(game.FirstPairPlayer.UserId);
                    if (game.SecondPairPlayer?.UserId != null && !playersParticipatedInThisGame.Contains(game.SecondPairPlayer.UserId))
                        playersParticipatedInThisGame.Add(game.SecondPairPlayer.UserId);
                    foreach (var userId in playersParticipatedInThisGame)
                    {
                        Dictionary<int, int> value;
                        var isHasValue = playersCompetitionParticipation.TryGetValue(userId, out value);
                        if (isHasValue)
                        {
                            playersCompetitionParticipation[userId][teamLeagues[game.TennisStage.CategoryId]] = 1;
                        }
                        else
                        {
                            playersCompetitionParticipation[userId] = new Dictionary<int, int>();
                            playersCompetitionParticipation[userId][teamLeagues[game.TennisStage.CategoryId]] = 1;
                        }
                    }
                }
            }

            var leagueIds = dailyCompetitions.Select(c => c.Id.Value).ToArray();
            var leagues = db.Leagues.Where(l => leagueIds.Contains(l.LeagueId)).ToList();
            foreach (var competition in dailyCompetitions)
            {
                foreach (var categoryId in competition.CategoryIds)
                {
                    var league = leagues.FirstOrDefault(l => l.LeagueId == competition.Id.Value);
                    var userIdsInTeam = GetTeamPlayersForCompetitionCount(categoryId, season, league);
                    foreach (var userId in userIdsInTeam)
                    {
                        Dictionary<int, int> value;
                        var isHasValue = playersCompetitionParticipation.TryGetValue(userId, out value);
                        if (isHasValue)
                        {
                            playersCompetitionParticipation[userId][competition.Id.Value] = 1;
                        }
                        else
                        {
                            playersCompetitionParticipation[userId] = new Dictionary<int, int>();
                            playersCompetitionParticipation[userId][competition.Id.Value] = 1;
                        }
                    }
                }
            }

            foreach (var userId in playersCompetitionParticipation.Keys)
            {
                var dict = playersCompetitionParticipation[userId];
                var competitionsRegisteredTo = dict.Values.Sum();
                int value;
                var isHasValue = playersCompetitionCount.TryGetValue(userId, out value);
                if (isHasValue)
                {
                    playersCompetitionCount[userId] = competitionsRegisteredTo;
                }
                else
                {
                    playersCompetitionCount[userId] = competitionsRegisteredTo;
                }
            }

            var userIdsInvolved = playersCompetitionCount.Select(x => x.Key).ToArray();
            var teamPlayersGroupedByUser = db.TeamsPlayers.Include("User").Include("Team").Include("Team.ClubTeams").Where(tp => userIdsInvolved.Contains(tp.UserId) && !tp.User.IsArchive &&
                    tp.IsActive &&
                    tp.SeasonId == seasonId).GroupBy(tp => tp.UserId).ToList();
            foreach (var groupOfPlayers in teamPlayersGroupedByUser)
            {
                var userId = groupOfPlayers.Key;
                foreach (var player in groupOfPlayers)
                {
                    player.CompetitionParticipationCount = playersCompetitionCount[player.UserId];
                }
            }

            Save();
        }

        public bool? CheckApproveStatus(TeamsPlayer teamsPlayerValue)
        {
            if (teamsPlayerValue != null)
            {
                var registration = teamsPlayerValue.User.ActivityFormsSubmittedDatas
                    .FirstOrDefault(x => x.Activity.IsAutomatic == true &&
                                         (x.Activity.Type == ActivityType.Personal ||
                                          x.Activity.Type == ActivityType.UnionPlayerToClub)
                                         && x.Activity.SeasonId == teamsPlayerValue.SeasonId &&
                                         x.PlayerId == teamsPlayerValue.UserId);

                bool? isApprovedByManager = teamsPlayerValue.IsApprovedByManager == true;
                var isApproved = registration?.IsActive ?? isApprovedByManager ?? false;
                return isApproved && teamsPlayerValue.IsActive;
            }
            return null;
        }

        public bool CheckApproveStatus(int userId)
        {
            var teamPlayers = db.TeamsPlayers.Where(t => t.UserId == userId && !t.User.IsArchive);
            if (teamPlayers.Any())
            {
                foreach (var teamPlayer in teamPlayers)
                {
                    var registration = teamPlayer.User.ActivityFormsSubmittedDatas.Where(x => x.Activity.IsAutomatic == true &&
                     (x.Activity.Type == ActivityType.Personal || x.Activity.Type == ActivityType.UnionPlayerToClub))
                        ?.FirstOrDefault(x => x.PlayerId == teamPlayer.UserId);

                    bool? isApprovedByManager = teamPlayer?.IsApprovedByManager == true;
                    var isApproved = registration?.IsActive ?? isApprovedByManager ?? false;
                    if (isApproved && teamPlayer.IsActive)
                        return true;
                }
            }
            return false;
        }


        public bool CheckIfAllTeamPlayersApproved(int userId, int seasonId)
        {
            var teamPlayers = db.TeamsPlayers.Where(t => t.UserId == userId && !t.User.IsArchive && t.SeasonId == seasonId);
            if (teamPlayers.All(tp => tp.IsApprovedByManager == true))
            {
                return true;
            }
            return false;
        }


        public IEnumerable<UsersApprovalDatesHistory> GetAllApprovalHistory(int userId, int seasonId)
        {
            var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
            var alias = season?.Union?.Section?.Alias;
            var seasonLess = db.UsersApprovalDatesHistories.Where(d => d.UserId == userId && d.SeasonId == 0);
            var seasonible = db.UsersApprovalDatesHistories.Where(d => d.UserId == userId && d.Season.UnionId.HasValue && d.Season.UnionId.Value > 0 && d.Season.Union.SectionId > 0 && d.Season.Union.Section.Alias == alias);
            return seasonLess.Union(seasonible);
        }

        public IEnumerable<PenaltyInformationDto> GetPenaltyHistory(int userId)
        {
            var penalties = db.PenaltyForExclusions.Where(c => c.UserId == userId && !c.IsCanceled);

            var teamsNames = GetTeamNamesOfUser(userId);

            if (penalties.Any())
            {
                foreach (var penalty in penalties)
                {
                    yield return new PenaltyInformationDto
                    {
                        Id = penalty.Id,
                        DateOfExclusion = penalty.DateOfExclusion,
                        ExclusionNumber = penalty.ExclusionNumber,
                        IsEnded = penalty.IsEnded,
                        TeamName = string.IsNullOrWhiteSpace(penalty.LeagueIds) ? teamsNames : GetTeamNamesOfUserWithLeague(userId, penalty.LeagueIds),
                        UserActionName = penalty.User1?.FullName
                    };
                }
            }
        }

        private string GetTeamNamesOfUser(int userId)
        {
            var resultString = new StringBuilder();
            var teamPlayers = db.TeamsPlayers.Where(c => c.UserId == userId).ToList();
            foreach (var player in teamPlayers)
            {
                var seasonalTeamName =
                    player?.Team?.TeamsDetails?.Where(c => c.SeasonId == player.SeasonId)?.OrderByDescending(c => c.Id)
                        ?.FirstOrDefault()?.TeamName ?? player?.Team?.Title;

                var teamName =
                    $"{player?.Season.Name} - {seasonalTeamName}";

                if (!teamPlayers.IsLast(player))
                {
                    resultString.Append(teamName);
                    resultString.Append(", ");
                }
                else
                {
                    resultString.Append(teamName);
                }
            }
            return resultString.ToString();
        }

        private string GetTeamNamesOfUserWithLeague(int userId, string leagueIds)
        {
            var resultString = new StringBuilder();
            var leagueIdsArr = leagueIds.Split(',').Where(l => !string.IsNullOrEmpty(l) && int.TryParse(l, out int _)).Select(l => int.Parse(l)).ToList();


            var teamPlayers = db.TeamsPlayers.Where(c => c.UserId == userId && leagueIdsArr.Contains(c.LeagueId ?? 0)).ToList();
            foreach (var player in teamPlayers)
            {
                var seasonalTeamName =
                    player?.Team?.TeamsDetails?.Where(c => c.SeasonId == player.SeasonId)?.OrderByDescending(c => c.Id)
                        ?.FirstOrDefault()?.TeamName ?? player?.Team?.Title;

                var teamName =
                    $"{player?.Season.Name} - {seasonalTeamName}";

                if (!teamPlayers.IsLast(player))
                {
                    resultString.Append(teamName);
                    resultString.Append(", ");
                }
                else
                {
                    resultString.Append(teamName);
                }
            }
            return resultString.ToString();
        }


        private string GetLeagueNamesByArrayString(string leagueIds)
        {
            var resultString = new StringBuilder();
            var leagueIdsArr = leagueIds.Split(',').Where(l => !string.IsNullOrEmpty(l) && int.TryParse(l, out int _)).Select(l => int.Parse(l)).ToList();
            var leagues = db.Leagues.Where(c => leagueIdsArr.Contains(c.LeagueId)).ToList();
            foreach (var league in leagues)
            {
                resultString.Append(league.Name);
                if (!leagues.IsLast(league))
                {
                    resultString.Append(", ");
                }
            }
            return resultString.ToString();
        }




        public string GetLastNameByFullName(string fullName)
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

        public string GetFirstNameByFullName(string fullName)
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

        public int? GetTeamPlayerId(int userId, int seasonId, int leagueId, int clubId, int currentTeamId, bool onlyActive = true)
        {
            TeamsPlayer teamPlayer = null;
            if (leagueId > 0 && currentTeamId > 0)
            {
                teamPlayer = db.TeamsPlayers
                    .FirstOrDefault(t =>
                        t.UserId == userId &&
                        t.SeasonId == seasonId &&
                        t.LeagueId == leagueId &&
                        t.TeamId == currentTeamId &&
                        (!onlyActive || t.IsActive) &&
                        !t.User.IsArchive);
            }
            else if (leagueId > 0 && currentTeamId == 0)
            {
                teamPlayer = db.TeamsPlayers
                    .FirstOrDefault(t =>
                        t.UserId == userId &&
                        t.SeasonId == seasonId &&
                        t.LeagueId == leagueId &&
                        (!onlyActive || t.IsActive) &&
                        !t.User.IsArchive);
            }
            else if (clubId > 0 && currentTeamId > 0)
            {
                teamPlayer = db.TeamsPlayers
                    .FirstOrDefault(t =>
                        t.UserId == userId &&
                        t.SeasonId == seasonId &&
                        t.ClubId == clubId &&
                        t.TeamId == currentTeamId &&
                        (!onlyActive || t.IsActive) &&
                        !t.User.IsArchive);
            }
            else if (clubId > 0 && currentTeamId == 0)
            {
                teamPlayer = db.TeamsPlayers
                    .FirstOrDefault(t =>
                        t.UserId == userId &&
                        t.SeasonId == seasonId &&
                        t.ClubId == clubId &&
                        (!onlyActive || t.IsActive) &&
                        !t.User.IsArchive);
            }
            else
            {
                teamPlayer = db.TeamsPlayers
                    .Where(c => c.UserId == userId &&
                                c.SeasonId == seasonId &&
                                (!onlyActive || c.IsActive) &&
                                !c.User.IsArchive)
                    ?.OrderByDescending(c => c.Id)
                    ?.FirstOrDefault();
            }

            return teamPlayer?.Id;
        }

        public List<PlayerViewModel> FilterPlayers(List<PlayerViewModel> players, string filterByClubs, string filterByDisciplines, string filterByPlayersStatus, IEnumerable<PlayerStatusViewModel> playersLite, ref int recordsFiltered)
        {
            var clubsIds = string.IsNullOrEmpty(filterByClubs) ? new List<int>() : filterByClubs.Split(',').Select(int.Parse).ToList();
            var clubNames = new List<string>();
            var _clubRepo = new ClubsRepo();
            foreach (var id in clubsIds)
            {
                clubNames.Add(_clubRepo.GetById(id).Name);
            }

            var disciplinesIds = string.IsNullOrEmpty(filterByDisciplines) ? new List<int>() : filterByDisciplines.Split(',').Select(int.Parse).ToList();
            var playersStatusIds = string.IsNullOrEmpty(filterByPlayersStatus) ? new List<int>() : filterByPlayersStatus.Split(',').Select(int.Parse).ToList();

            if (clubsIds.Any())
            {
                players = players.Where(c => c.ClubId.HasValue && (clubsIds.Contains(c.ClubId.Value) || clubNames.Contains(c.ClubName))).ToList();
                playersLite = playersLite.Where(c => c.ClubId.HasValue && clubsIds.Contains(c.ClubId.Value));
            }

            if (disciplinesIds.Any())
            {
                players = players.Where(c => c.DisciplinesIds.Intersect(disciplinesIds).Any()).ToList();
                playersLite = playersLite.Where(c => c.DisciplinesIds.Intersect(disciplinesIds).Any());
            }

            if (playersStatusIds.Any())
            {
                Predicate<PlayerStatusViewModel> pr = p =>
                    (playersStatusIds.Contains(1) && (p.IsActive == true && !p.IsApproveChecked && !p.IsNotApproveChecked)) ||
                    (playersStatusIds.Contains(2) && p.IsApproveChecked) ||
                    (playersStatusIds.Contains(3) && p.IsNotApproveChecked) ||
                    (playersStatusIds.Contains(4) && p.IsActive == false ||
                     (playersStatusIds.Contains(5) && p.IsBlockaded)) ||
                    (playersStatusIds.Contains(6) && (p.IsActive == true && !p.IsApproveChecked && !p.IsNotApproveChecked && p.HasMedicalCert));

                players = players.Where(p => pr(p)).ToList();
                playersLite = playersLite.Where(p => pr(p));
            }

            recordsFiltered = playersLite.Count();
            return players;
        }

        public PenaltyDto UpdatePenalty(int id, int exclusionNumber)
        {
            var penalty = db.PenaltyForExclusions.FirstOrDefault(c => c.Id == id);
            var dateOfUpdate = DateTime.Now;
            penalty.DateOfExclusion = dateOfUpdate;
            penalty.ExclusionNumber = exclusionNumber;
            if (!string.IsNullOrWhiteSpace(penalty.LeagueIds))
            {
                var leagueForExclusionIds = penalty.LeagueIds.Split(',').Where(l => !string.IsNullOrEmpty(l) && int.TryParse(l, out int _)).Select(l => int.Parse(l)).ToList();
                var prevPenalisedGames = db.GamesCycles.Where(g =>
                        (g.GuestTeam.TeamsPlayers.Select(c => c.UserId).Contains(penalty.UserId) ||
                         g.HomeTeam.TeamsPlayers.Select(c => c.UserId).Contains(penalty.UserId)) &&
                         g.AppliedExclusionId.HasValue && g.AppliedExclusionId.Value == penalty.Id).ToList();

                foreach (var game in prevPenalisedGames)
                {
                    game.AppliedExclusionId = null;
                }
                db.SaveChanges();
                var games = db.GamesCycles.Where(g =>
                    (g.GuestTeam.TeamsPlayers.Select(c => c.UserId).Contains(penalty.UserId) ||
                     g.HomeTeam.TeamsPlayers.Select(c => c.UserId).Contains(penalty.UserId)) &&
                    g.GameStatus != GameStatus.Ended && g.StartDate > DateTime.Now && !g.AppliedExclusionId.HasValue && leagueForExclusionIds.Contains(g.Stage.LeagueId)).OrderBy(g => g.StartDate).Take(exclusionNumber).ToList();
                foreach (var game in games)
                {
                    game.PenaltyForExclusion = penalty;
                }
            }
            db.SaveChanges();
            return new PenaltyDto
            {
                Id = penalty.Id,
                ExclusionNumber = exclusionNumber,
                DateOfExclusion = dateOfUpdate,
                LeagueName = string.IsNullOrWhiteSpace(penalty.LeagueIds) ? string.Empty : GetLeagueNamesByArrayString(penalty.LeagueIds)
            };
        }

        public void CreatePenalty(int userId, int exclusionNumber, int[] leagueForExclusionIds, int managerId)
        {
            var leagueForExclusionIdsStr = string.Join(",", leagueForExclusionIds);
            var penalty = new PenaltyForExclusion
            {
                UserId = userId,
                ExclusionNumber = exclusionNumber,
                DateOfExclusion = DateTime.Now,
                ActionUserId = managerId,
                LeagueIds = leagueForExclusionIdsStr
            };
            db.PenaltyForExclusions.Add(penalty);
                if (leagueForExclusionIds.Count() > 0)
                {
                var games = db.GamesCycles.Where(g =>
                        (g.GuestTeam.TeamsPlayers.Select(c => c.UserId).Contains(userId) ||
                         g.HomeTeam.TeamsPlayers.Select(c => c.UserId).Contains(userId)) &&
                        g.GameStatus != GameStatus.Ended && g.StartDate > DateTime.Now && !g.AppliedExclusionId.HasValue && leagueForExclusionIds.Contains(g.Stage.LeagueId)).OrderBy(g => g.StartDate).Take(exclusionNumber).ToList();
                    foreach (var game in games)
                    {
                        game.PenaltyForExclusion = penalty;
                    }
                }
            db.SaveChanges();
        }

        public void SetAthleteNumberProduced(int userId, bool isProduced, int? seasonId)
        {
            var athleteNumbers = db.Users.Where(u => u.UserId == userId).FirstOrDefault()?.AthleteNumbers;
            var athleteNumber = athleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId);
            if (athleteNumber == null)
            {
                athleteNumber = new AthleteNumber
                {
                    SeasonId = seasonId ?? 0,
                    UserId = userId,
                    IsAthleteNumberProduced = isProduced
                };
                db.AthleteNumbers.Add(athleteNumber);
            }
            else
            {
                athleteNumber.IsAthleteNumberProduced = isProduced;
            }
            db.SaveChanges();
        }

        public void SetAthleteNumberProduced(List<PlayerStatusViewModel> players, int? seasonId)
        {
            foreach (var player in players)
            {
                var athleteNumbers = db.Users.Where(u => u.UserId == player.UserId).FirstOrDefault()?.AthleteNumbers;
                var athleteNumber = athleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId);
                if (athleteNumber == null)
                {
                    athleteNumber = new AthleteNumber
                    {
                        SeasonId = seasonId ?? 0,
                        UserId = player.UserId,
                        IsAthleteNumberProduced = true
                    };
                    db.AthleteNumbers.Add(athleteNumber);
                }
                else
                {
                    athleteNumber.IsAthleteNumberProduced = true;
                }
            }
            db.SaveChanges();
        }

        public void DeletePenalty(int id)
        {
            var penalty = db.PenaltyForExclusions.FirstOrDefault(c => c.Id == id);
            penalty.IsCanceled = true;
            db.SaveChanges();
        }

        public IEnumerable<PenaltyInformationDto> GetPenaltiesForPlayer(int id, int seasonId)
        {
            var penalties = db.PenaltyForExclusions.Where(c => c.UserId == id && !c.IsCanceled);
            if (penalties.Any())
            {
                foreach (var penalty in penalties)
                {
                    var teamsNames = GetTeamNamesOfUser(penalty.UserId);
                    yield return new PenaltyInformationDto
                    {
                        Id = penalty.Id,
                        DateOfExclusion = penalty.DateOfExclusion,
                        ExclusionNumber = penalty.ExclusionNumber,
                        UserActionName = penalty.User1?.FullName,
                        IsEnded = penalty.IsEnded,
                        TeamName = string.IsNullOrWhiteSpace(penalty.LeagueIds) ? teamsNames : GetTeamNamesOfUserWithLeague(penalty.UserId, penalty.LeagueIds),
                        LeagueName = string.IsNullOrWhiteSpace(penalty.LeagueIds) ? string.Empty : GetLeagueNamesByArrayString(penalty.LeagueIds)
                    };
                }
            }
        }

        private List<PlayerViewModel> GetPlayersViewModel(List<TeamsPlayer> players, int? seasonId, string sectionAlias)
        {
            List<League> leagues = new List<League>();
            var leaguesIds = players.Where(x => x.LeagueId != null).Select(x => x.LeagueId).Distinct().ToList();
            if (leaguesIds!=null && leaguesIds.Any())
            {
                db.Leagues
                    .Where(x => leaguesIds.Contains(x.LeagueId))
                    .AsNoTracking()
                    .ToList();
            }
            List<PlayerDiscipline> playersDisciplines = new List<PlayerDiscipline>();
            if (sectionAlias != GamesAlias.Athletics)
            {
                playersDisciplines = db.PlayerDisciplines
                    .Where(x => x.SeasonId == seasonId)
                    .AsNoTracking()
                    .ToList();
            }
             
            var playersDisciplinesIds = playersDisciplines.Select(pd => pd.DisciplineId);
            List<Discipline> disciplines = new List<Discipline>();
            if (playersDisciplinesIds != null && playersDisciplinesIds.Any())
            {
                disciplines = db.Disciplines
                    .Where(x => playersDisciplinesIds.Contains(x.DisciplineId))
                    .AsNoTracking()
                    .ToList();
            }

            var usersIds = players.Select(x => x.UserId).Distinct().ToList();
            var registrations =
                db.ActivityFormsSubmittedDatas
                    .Include(x => x.Activity)
                    .Where(x => usersIds.Contains(x.PlayerId))
                    .AsNoTracking()
                    .ToList();

            var usersRoutes = db.UsersRoutes
                .Include(x => x.DisciplineRoute)
                .Where(x => usersIds.Contains(x.UserId))
                .AsNoTracking()
                .ToList();

            var teamsRoutes = db.TeamsRoutes
                .Include(x => x.DisciplineTeamRoute)
                .Where(x => usersIds.Contains(x.UserId))
                .AsNoTracking()
                .ToList();

            var usersRanks = db.UsersRanks
                .Include(x => x.RouteRank)
                .Include(x => x.UsersRoute)
                .Include(x => x.UsersRoute.DisciplineRoute)
                .Where(x => usersIds.Contains(x.UserId))
                .AsNoTracking()
                .ToList();

            var teamsRanks = db.TeamsRanks
                .Include(x => x.RouteTeamRank)
                .Include(x => x.TeamsRoute)
                .Include(x => x.TeamsRoute.DisciplineRoute)
                .Where(x => usersIds.Contains(x.UserId))
                .AsNoTracking()
                .ToList();


            var seasons = db.Seasons.Where(x => x.IsActive)
                .AsNoTracking()
                .ToList();
            var unionId = db.Seasons.FirstOrDefault(x => x.Id == seasonId)?.UnionId;
            var seasonsIds = db.Seasons.Where(x => unionId.HasValue && x.UnionId == unionId.Value)
                .AsNoTracking().Select(x => x.Id).ToList();


            var sportsRegistrations = db.SportsRegistrations
                .Where(x => usersIds.Contains(x.UserId) &&
                            (x.SeasonId == seasonId &&
                             !x.League.IsArchive &&
                             x.IsApproved ||
                             (x.FinalScore.HasValue || x.Position.HasValue)))
                .Include(x => x.League)
                .ToList();

            var result = new List<PlayerViewModel>();
            bool foundExpiredValidity = false;

            var isTennis = string.Equals(sectionAlias, SectionAliases.Tennis,
                StringComparison.CurrentCultureIgnoreCase);

            var teamsIds = players.Select(x => x.TeamId).Distinct().ToList();
            var teamPlayersAllSeasons = db.TeamsPlayers.Where(x => usersIds.Contains(x.UserId)).ToList();

            var clubTeams = db.ClubTeams
                .Include(x => x.Club)
                //.Include(x => x.Club.Section)
                //.Include(x => x.Club.Union)
                //.Include(x => x.Club.Union.Section)
                .Where(x => teamsIds.Contains(x.TeamId) && x.SeasonId == seasonId)
                .AsNoTracking()
                .ToList();

            var seasonForAge = players.FirstOrDefault()?.Season.SeasonForAge;

            var athleteNumbers = sectionAlias == GamesAlias.Athletics ? db.AthleteNumbers.Where(a => usersIds.Contains(a.UserId) && a.SeasonId == seasonId).ToList() : new List<AthleteNumber>();
            var playerAchievements = players.Count <= 100 ? db.PlayerAchievements.Where(a => usersIds.Contains(a.PlayerId)).ToList() : db.PlayerAchievements.ToList();
            int AlternativeResultInt = 3;
            var competitionDisciplineRegistrations = sectionAlias == GamesAlias.Athletics ? db.CompetitionDisciplineRegistrations.Where(c => usersIds.Contains(c.UserId) && c.CompetitionDiscipline.League.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && !c.CompetitionDiscipline.IsDeleted && c.CompetitionResult.FirstOrDefault() != null && !(c.CompetitionResult.FirstOrDefault().Result == null || c.CompetitionResult.FirstOrDefault().Result.Trim() == string.Empty) && c.CompetitionResult.FirstOrDefault().AlternativeResult != AlternativeResultInt).ToList() : new List<CompetitionDisciplineRegistration>();

            var bicycleFriendshipPriceHelper = new BicycleFriendshipPriceHelper();

            var bicycleFriendshipPayments = db.BicycleFriendshipPayments
                .Where(x => usersIds.Contains(x.UserId) && x.IsPaid && !x.Discarded)
                .OrderByDescending(x => x.DateCreated)
                .ToList();

            foreach (var tp in players)
            {
                var clubTeam = clubTeams.FirstOrDefault(t => t.TeamId == tp.TeamId && t.SeasonId == seasonId);
                var club = clubTeam?.Club;
                var league = leagues?.FirstOrDefault();

                var isSubmittedActive = registrations
                    .FirstOrDefault(x => x.PlayerId == tp.UserId && x.TeamId == tp.TeamId && x.LeagueId == tp.LeagueId)
                    ?.IsActive;

                var athleteNumber = sectionAlias == GamesAlias.Athletics
                    ? athleteNumbers.FirstOrDefault(a => a.UserId == tp.UserId)
                    : null;

                bool isTrainingTeam = false;
                if (sectionAlias == GamesAlias.Tennis)
                {
                    isTrainingTeam = clubTeam?.IsTrainingTeam == true;
                }

                var isEilatTournamentLeague =
                    leagues?.FirstOrDefault(x => x.LeagueId == tp.LeagueId)?.EilatTournament == true;
                var teamName = tp.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == tp.SeasonId)?.TeamName ??
                               tp.Team.Title;

                var unionSeasons = seasons.Where(x =>
                        x.UnionId == (club?.UnionId ?? league?.UnionId))
                    .ToList();
                var previousSeason = unionSeasons.Count > 1 ? unionSeasons.OrderBy(x => x.Id).ElementAtOrDefault(unionSeasons.Count - 2) : null;
                if (previousSeason?.Id == seasonId)
                {
                    previousSeason = null;
                }

                var previousSeasonTeamPlayers = teamPlayersAllSeasons.Where(x =>
                    x.UserId == tp.UserId &&
                    x.SeasonId == previousSeason?.Id &&
                    x.TeamId == tp.TeamId).ToList();
                var isAnyTeamPlayerPreviousSeasonWasActive = previousSeasonTeamPlayers.Any(x => x.IsActive);
                var isAnyTeamPlayerPreviousSeasonWasApproved = previousSeasonTeamPlayers.Any(x => x.IsApprovedByManager == true);
                var isPlayerParticipatedPreviousSeason = isAnyTeamPlayerPreviousSeasonWasActive && isAnyTeamPlayerPreviousSeasonWasApproved;

                var registrationPreviousSeason = registrations.FirstOrDefault(x =>
                                                     x.PlayerId == tp.UserId &&
                                                     x.Activity.IsAutomatic == true &&
                                                     x.Activity.Type == ActivityType.Personal &&
                                                     x.Activity.SeasonId == previousSeason?.Id)
                                                 ?? registrations.FirstOrDefault(x =>
                                                     x.PlayerId == tp.UserId &&
                                                     x.Activity.IsAutomatic == true &&
                                                     x.Activity.Type == ActivityType.UnionPlayerToClub &&
                                                     x.Activity.SeasonId == previousSeason?.Id);

                var registration = registrations.FirstOrDefault(x =>
                                       x.PlayerId == tp.UserId &&
                                       x.Activity.IsAutomatic == true &&
                                       x.Activity.Type == ActivityType.Personal &&
                                       x.Activity.SeasonId == seasonId)
                                   ?? registrations.FirstOrDefault(x =>
                                       x.PlayerId == tp.UserId &&
                                       x.Activity.IsAutomatic == true &&
                                       x.Activity.Type == ActivityType.UnionPlayerToClub &&
                                       x.Activity.SeasonId == seasonId);

                var disciplinesIds = playersDisciplines
                    .Where(x => x.PlayerId == tp.UserId && x.ClubId == club?.ClubId &&
                                x.SeasonId == seasonId)
                    .Select(x => x.DisciplineId)
                    .ToList();

                var disciplinesNames = disciplinesIds.Any()
                    ? string.Join(",",
                        disciplines.Where(d => disciplinesIds.Contains(d.DisciplineId)).Select(d => d.Name))
                    : "";

                var gymnasticCompetitionCount = (sectionAlias == GamesAlias.Gymnastic)
                    ? tp.User.CompetitionRegistrations
                        .Where(c => c.SeasonId == seasonId && !c.League.IsArchive && c.IsActive &&
                                    (c.FinalScore.HasValue || c.Position.HasValue))
                        .GroupBy(r => r.LeagueId)
                        .Select(r => r.First())
                        .Count()
                    : 0;

                int regularCompetitionCount = (sectionAlias == GamesAlias.WeightLifting)
                    ? tp.User.CompetitionDisciplineRegistrations
                        .Where(c => c.CompetitionDiscipline.League.SeasonId == seasonId &&
                                    !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive &&
                                    c.IsApproved.HasValue && c.IsApproved.Value)
                        .GroupBy(r => r.CompetitionDiscipline.League.LeagueId)
                        .Select(r => r.First())
                        .Count()
                    : 0;
                int athleticRegistrationCount = 0;

                if (sectionAlias == GamesAlias.Athletics)
                {
                    athleticRegistrationCount = competitionDisciplineRegistrations
                        .Where(c => c.UserId == tp.UserId)
                        .GroupBy(r => r.CompetitionDiscipline.League.LeagueId)
                        .Select(r => r.First())
                        .Count();
                }

                var otherCompetitionCount = sportsRegistrations
                    .Where(c => c.UserId == tp.UserId && c.IsApproved)
                    .GroupBy(r => r.LeagueId)
                    .Select(r => r.First())
                    .Count();

                var competitionCount = gymnasticCompetitionCount + athleticRegistrationCount + regularCompetitionCount +
                                       otherCompetitionCount;
                if (sectionAlias.Equals(GamesAlias.Tennis) == true)
                {
                    competitionCount = tp.CompetitionParticipationCount ?? 0;
                }

                var individualRoutesNames = usersRoutes
                    .Where(x => x.UserId == tp.UserId && disciplinesIds.Contains(x.DisciplineRoute.DisciplineId))
                    .Select(x => x.DisciplineRoute.Route);
                var individualTeamRoutesNames = teamsRoutes
                    .Where(x => x.UserId == tp.UserId && disciplinesIds.Contains(x.DisciplineTeamRoute.DisciplineId))
                    .Select(x => x.DisciplineTeamRoute.Route);
                var routesTogether = individualRoutesNames.Concat(individualTeamRoutesNames);

                var individualRanksNames = usersRanks
                    .Where(x => x.UserId == tp.UserId &&
                                disciplinesIds.Contains(x.UsersRoute.DisciplineRoute.DisciplineId))
                    .Select(x => x.RouteRank.Rank);
                var teamsRanksNames = teamsRanks
                    .Where(x => x.UserId == tp.UserId &&
                                disciplinesIds.Contains(x.TeamsRoute.DisciplineTeamRoute.DisciplineId))
                    .Select(x => x.RouteTeamRank.Rank);
                var ranksTogether = individualRanksNames.Concat(teamsRanksNames);

                var bicycleFriendshipPayment = bicycleFriendshipPayments
                    .FirstOrDefault(x => x.UserId == tp.UserId &&
                                         x.ClubId == tp.ClubId &&
                                         x.TeamId == tp.TeamId &&
                                         x.SeasonId == tp.SeasonId);
                var isUnderPenalty = false;
                if (sectionAlias == null || sectionAlias != GamesAlias.Athletics)
                {
                    foreach (var penalty in tp.User.PenaltyForExclusions.Where(c => !c.IsCanceled && !c.IsEnded))
                    {
                        if (string.IsNullOrWhiteSpace(penalty.LeagueIds))
                        {
                            isUnderPenalty = true;
                            break;
                        }
                        if (!string.IsNullOrWhiteSpace(penalty.LeagueIds) && tp.LeagueId.HasValue)
                        {
                            var leagueIdsArr = penalty.LeagueIds.Split(',').Where(l => !string.IsNullOrEmpty(l) && int.TryParse(l, out int _)).Select(l => int.Parse(l)).ToList();
                            if (leagueIdsArr.Contains(tp.LeagueId.Value))
                            {
                                isUnderPenalty = true;
                                break;
                            }
                        }
                    }
                }
                var playerViewModel = new PlayerViewModel
                {
                    Id = tp.Id,
                    FullName = tp.User.FullName,
                    ShirtNum = tp.ShirtNum,
                    Weight = tp.User.Weight,
                    WeightUnits = tp.User.WeightUnits,
                    UserId = tp.UserId,
                    PosId = tp.PosId,
                    IdentNum = string.IsNullOrEmpty(tp.User.PassportNum)
                        ? tp.User.IdentNum
                        : !string.IsNullOrWhiteSpace(tp.User.IdentNum)
                            ? tp.User.IdentNum
                            : tp.User.PassportNum,
                    IsActive = tp.IsActive,

                    TeamId = tp.TeamId,
                    TeamName = teamName,
                    TrainingTeamName = isTennis && isTrainingTeam ? teamName : null, //tennis specific
                    LeagueTeamName =
                        isTennis && (isTrainingTeam || isEilatTournamentLeague) ? null : teamName, //tennis specific
                    IsTrainingTeam = isTrainingTeam, //tennis specific
                    SeasonId = tp.SeasonId,
                    Birthday = tp.User.BirthDay,
                    BirthdayString = tp.User?.BirthDay?.ToString("dd/MM/yyyy") ?? "",
                    City = tp.User.City,
                    Email = tp.User.Email,
                    Phone = tp.User.Telephone,
                    Height = tp.User.Height,
                    Gender = tp.User.Gender?.Title,
                    Position = tp.Position?.Title,
                    InsuranceFile =
                        tp.User.PlayerFiles.FirstOrDefault(x =>
                            x.FileType == (int)PlayerFileType.Insurance && x.SeasonId == tp.SeasonId)?.FileName ??
                        tp.User.InsuranceFile,
                    PlayerImage =
                        tp.User.PlayerFiles.FirstOrDefault(x =>
                            x.FileType == (int)PlayerFileType.PlayerImage && x.SeasonId == tp.SeasonId)?.FileName ??
                        tp.User.Image,
                    ParentName = tp.User.ParentName,
                    IDFile = tp.User.IDFile ?? tp.User.PlayerFiles.FirstOrDefault(x =>
                                 x.FileType == (int)PlayerFileType.IDFile && x.SeasonId == tp.SeasonId)?.FileName,
                    PassportFile = tp.User.PassportFile ?? tp.User.PlayerFiles.FirstOrDefault(x =>
                                           x.FileType == (int)PlayerFileType.PassportFile && x.SeasonId == tp.SeasonId)
                                       ?.FileName,
                    DisciplinesNames = disciplinesNames,
                    ClubName = club?.Name ?? "",
                    ClubId = club?.ClubId ?? 0,
                    ClubComment = tp.ClubComment,
                    UnionComment = tp.UnionComment,
                    DisciplinesIds =
                        tp.User?.PlayerDisciplines.Where(x => x.ClubId == club?.ClubId && x.SeasonId == seasonId)
                            .Select(x => x.DisciplineId) ?? new List<int>(),
                    MedicalCertificateFile = tp.User.PlayerFiles.FirstOrDefault(x =>
                        x.FileType == (int)PlayerFileType.MedicalCertificate && x.SeasonId == tp.SeasonId &&
                        !x.IsArchive)?.FileName, // ?? tp.User.MedicalCertificateFile,
                    HasMedicalCert = tp.User.PlayerFiles.Any(x =>
                        x.FileType == (int)PlayerFileType.MedicalCertificate && x.SeasonId == tp.SeasonId &&
                        !x.IsArchive),
                    MedicalCertApproved =
                        tp.User.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == seasonId)?.Approved == true,
                    IsPlayerRegistered = registration != null,
                    IsPlayerRegistrationApproved = registration?.IsActive ?? false,
                    ShirtSize = tp.User?.ShirtSize,
                    BaseHandicap = tp.HandicapLevel,
                    HandicapReduction =
                        0m, // isGymnastic ? 0m : GetHandicapReduction(tp, seasonId, unionId, tp.LeagueId),
                    FinalHandicap =
                        0m, // isGymnastic ? 0m : GetFinalHandicap(tp, out _, seasonId, unionId, tp.LeagueId),
                    StartPlayingString = tp.StartPlaying?.ToShortDateString() ?? "",
                    StartPlaying = tp.StartPlaying,
                    MedExamDateString = tp.User.MedExamDate?.ToShortDateString() ?? "",
                    DateOfInsuranceString = tp.User.DateOfInsurance?.ToShortDateString() ?? "",
                    DateOfInsurance = tp.User.DateOfInsurance,
                    IsApprovedByClubManager = tp.IsApprovedByClubManager == true,
                    IsApproveChecked = tp.IsApprovedByManager == true,
                    IsNotApproveChecked = tp.IsApprovedByManager == false,
                    IsApprovedInSubmitted = isSubmittedActive ?? false,
                    BlockadeId = tp.User?.BlockadeId,
                    IsBlockaded = tp.User.BlockadeId.HasValue,
                    EndBlockadeDate = tp.User?.PlayersBlockade?.EndDate,
                    EndBlockadeDateString = tp.User?.PlayersBlockade?.EndDate.ToString("dd/MM/yyyy HH:mm:ss") ?? "",
                    LeagueId = tp.LeagueId,
                    LeagueName = tp.League?.Name ?? string.Empty,
                    SeasonForAge = seasonForAge,
                    Route = $"{string.Join(", ", routesTogether)}",
                    Rank = $"{string.Join(", ", ranksTogether)}",
                    Achievements = string.Join(",",
                        playerAchievements.Where(a => a.PlayerId == tp.UserId && a.DateCompleted.HasValue)
                            .Select(x => x.SportRank.RankName)),
                    AchievementsHeb = string.Join(",",
                        playerAchievements.Where(a => a.PlayerId == tp.UserId && a.DateCompleted.HasValue)
                            .Select(x => x.SportRank.RankNameHeb)),
                    IsLocked = tp.IsLocked ?? SetPlayersLockedStatus(tp),
                    IsYoung = CheckIfPlayerIsYoung(tp),
                    IsExceptional = tp.IsExceptionalMoved,
                    CompetitiveLicenseNumber = tp.User.CompetitiveLicenseNumber,
                    LicenseValidity = tp.User.LicenseValidity?.ToShortDateString() ?? string.Empty,
                    LicenseLevel = tp.User.LicenseLevel,
                    DriverLicenseFile = tp.User.PlayerFiles.FirstOrDefault(x =>
                        x.FileType == (int) PlayerFileType.DriverLicense && x.SeasonId == tp.SeasonId)?.FileName,
                    CompetitionCount = competitionCount,
                    ApprovalDate = tp.ApprovalDate?.ToString("dd/MM/yyyy HH:mm:ss") ?? "",
                    IsActivePlayer = competitionCount >= 4,
                    IsUnderPenalty = isUnderPenalty,
                    InitialApprovalDate = tp.User.InitialApprovalDates?.FirstOrDefault()?.InitialApprovalDate1.ToShortDateString() ?? string.Empty,
                    AthletesNumbers = sectionAlias == GamesAlias.Athletics ? athleteNumber?.AthleteNumber1 : null,
                    IsAthleteNumberProduced = sectionAlias == GamesAlias.Athletics
                        ? (athleteNumber?.IsAthleteNumberProduced ?? false)
                        : false,
                    SeasonIdOfCreation = tp.User.SeasonIdOfCreation,
                    ApprovedInPreviousSeason = registrationPreviousSeason?.IsActive == true || isPlayerParticipatedPreviousSeason == true,
                    ParentStatementFile = tp.User.PlayerFiles.FirstOrDefault(x =>
                            x.FileType == (int) PlayerFileType.ParentStatement && x.SeasonId == tp.SeasonId && x.IsArchive != true)
                        ?.FileName,
                    TenicardValidity = tp.User?.TenicardValidity?.ToString("dd/MM/yyyy") ?? "",
                    TenicardValidityDate = tp.User?.TenicardValidity,
                };

                if (sectionAlias == GamesAlias.Swimming)
                {
                    playerViewModel.ClassS = tp.User.ClassS;
                    playerViewModel.ClassSB = tp.User.ClassSB;
                    playerViewModel.ClassSM = tp.User.ClassSM;
                }

                if (sectionAlias == GamesAlias.Bicycle)
                {
                    playerViewModel.GenderId = tp.User.GenderId ?? -1;
                    playerViewModel.FriendshipTypeId = tp.FriendshipTypeId;
                    playerViewModel.FriendshipTypeName = tp.FriendshipsType?.Name.ToString();

                    playerViewModel.FriendshipPriceTypeId = tp.FriendshipPriceType;
                    playerViewModel.FriendshipPriceTypeName = "";

                    playerViewModel.RoadHeat = tp.Discipline?.Name;
                    playerViewModel.MountainHeat = tp.Discipline1?.Name;
                    playerViewModel.ChipNumber = tp.User.ChipNumber;
                    playerViewModel.UciId = tp.User.UciId?.ToString();

                    playerViewModel.KitStatusId = tp.KitStatus;
                    playerViewModel.KitStatusName = "";

                    playerViewModel.MountainIronNumber = tp.MountainIronNumber?.ToString();
                    playerViewModel.RoadIronNumber = tp.RoadIronNumber?.ToString();
                    playerViewModel.PaymentForChipNumber = tp.User.PaymentForChipNumber == true;
                    playerViewModel.PaymentForUciId = tp.User.PaymentForUciId == true;

                    if (bicycleFriendshipPayment?.IsPaid == true)
                    {
                        //payment already initiated and paid, show price that was stored with payment
                        var friendshipPrice = new BicycleFriendshipPriceHelper.BicycleFriendshipPrice(
                            bicycleFriendshipPayment.FriendshipPrice,
                            bicycleFriendshipPayment.ChipPrice,
                            bicycleFriendshipPayment.UciPrice);

                        playerViewModel.FriendshipTotalPrice = friendshipPrice.Total;
                        playerViewModel.FriendshipPaid = true;
                    }
                    else
                    {
                        var friendshipPrice = bicycleFriendshipPriceHelper.GetFriendshipPrice(tp); //Might be a performance issue in future
                        playerViewModel.FriendshipTotalPrice = friendshipPrice.Total;
                        playerViewModel.FriendshipPaid = bicycleFriendshipPayment?.IsPaid == true;
                    }
                }

                result.Add(playerViewModel);
            }

            if (foundExpiredValidity)
            {
                Save();
            }
            return result;
        }

        public TeamsPlayer GetTeamPlayerBySeasonId(int teamId, int userId, int leagueId, int? clubId, int seasonId)
        {
            return leagueId != 0
               ? db.TeamsPlayers.FirstOrDefault(tp => tp.UserId == userId && tp.TeamId == teamId && tp.SeasonId == seasonId && tp.LeagueId == leagueId)
               : db.TeamsPlayers.FirstOrDefault(tp => tp.UserId == userId && tp.TeamId == teamId && tp.SeasonId == seasonId && tp.ClubId == clubId);
        }

        private bool CheckIfPlayerIsYoung(TeamsPlayer tp)
        {
            if (tp != null)
            {
                var birthday = tp.User.BirthDay;
                var isYoung = false;
                if (birthday.HasValue)
                {
                    var age = DateTime.Now.Year - birthday.Value.Year;
                    if (age > 19 && age < 24)
                    {
                        isYoung = true;
                    }
                }
                return isYoung;
            }
            else
                return false;
        }

        public string GetHiddenColumns(int userId)
        {
            var columns = db.ColumnVisibilities
                .Where(c => c.UserId == userId && !c.Visible)
                .Select(c => c.ColumnIndex);
            var result = string.Join(",", columns);
            return result == string.Empty ? null : result;
        }

        public void SetVisibility(int item, bool value, int userId)
        {
            var column = db.ColumnVisibilities.SingleOrDefault(c => c.UserId == userId && c.ColumnIndex == item);
            if (column == null)
            {
                column = new ColumnVisibility
                {
                    UserId = userId,
                    ColumnIndex = item
                };
                db.ColumnVisibilities.Add(column);
            }

            column.Visible = value;
            db.SaveChanges();
        }

        public decimal GetFinalHandicap(TeamsPlayer player, out decimal reduction, int? seasonId, int? unionId, int? leagueId)
        {
            var handicap = player.HandicapLevel;
            var league = db.Leagues.FirstOrDefault(l => l.LeagueId == leagueId);
            reduction = handicap <= 5 ? GetHandicapReduction(player, seasonId, unionId, leagueId) : 0;


            handicap += reduction;

            if (handicap < 0) handicap = 0;
            return handicap;
        }

        private decimal GetHandicapReduction(TeamsPlayer player, int? seasonId, int? unionId, int? leagueId)
        {
            var league = db.Leagues.FirstOrDefault(l => l.LeagueId == leagueId);
            decimal result = 0;
            decimal? leagueReduction = null;
            if (league != null && unionId == 31 && player.HandicapLevel == 5)
            {
                if (league.FiveHandicapReduction.HasValue)
                {
                    leagueReduction = Convert.ToDecimal(league.FiveHandicapReduction.Value);
                }
            }
            if (leagueReduction != null)
            {
                return leagueReduction.Value * -1;
            }
            else
            {
                var birthday = player.User?.BirthDay;
                var tps = player.User?.TeamsPlayers;
                var teamsCount = tps?.Count(tp => tp.SeasonId == seasonId) ?? 0;

                var age = birthday.HasValue ? DateTime.Today.Year - birthday.Value.Year : 0;

                if (player.User?.Gender?.GenderId == 0)
                    result -= 1.5m;
                else if (player.IsPlayereInTeamLessThan3year && teamsCount == 1)
                    result--;
                else if (birthday != null)
                {
                    if (birthday.Value > DateTime.Today.AddYears(-age)) age--;

                    if (age < 19 || age <= 24 && player.User?.TeamsPlayers?.Count == 1)
                    {
                        result--;
                    }
                }

                return result;
            }

        }

        public void BlockadePlayer(IEnumerable<int> teamPlayersIds, DateTime? endDate, int? seasonId, int managerId)
        {
            try
            {
                foreach (var playerId in teamPlayersIds)
                {
                    db.PlayersBlockades.Add(new PlayersBlockade
                    {
                        UserId = playerId,
                        StartDate = DateTime.Now,
                        EndDate = endDate.Value,
                        IsActive = true,
                        SeasonId = seasonId,
                        ActionUserId = managerId
                    });
                    db.SaveChanges();

                    var player = db.Users.FirstOrDefault(u => u.UserId == playerId);
                    var playersBlockade = db.PlayersBlockades.OrderByDescending(c => c.Id).FirstOrDefault();
                    if (playersBlockade != null)
                    {
                        player.BlockadeId = playersBlockade.Id;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<BlockadeHistoryDTO> GetAllBlockadesForPlayer(int userId, int? seasonId = null, int type = BlockadeType.Blockade)
        {
            var playersBlockades = db.PlayersBlockades.Where(b => b.UserId == userId && b.BType == type);

            if (seasonId != null)
            {
                playersBlockades = playersBlockades.Where(x => x.SeasonId == seasonId);
            }

            playersBlockades = playersBlockades.OrderBy(x => x.SeasonId).ThenBy(x => x.EndDate);

            if (playersBlockades.Any())
            {
                foreach (var blockade in playersBlockades)
                {
                    yield return new BlockadeHistoryDTO
                    {
                        SeasonId = blockade.SeasonId ?? 0,
                        SeasonName = blockade.Season?.Name,
                        StartDate = blockade.StartDate,
                        EndDate = blockade.EndDate,
                        UserActionName = blockade.User1?.FullName
                    };
                }
            }
        }

        public List<PlayersBlockadeShortDTO> GetAllBlockadedPlayersForUnion(int unionId, int? seasonId, int? clubId, int? teamId, int BType = BlockadeType.All)
        {
            var playersToReturn = new List<PlayersBlockadeShortDTO>();
            var blockadeValues = db.PlayersBlockades.Where(blockade => blockade.IsActive == false && blockade.SeasonId == seasonId
            && (!clubId.HasValue || blockade.User.TeamsPlayers.FirstOrDefault(t => t.SeasonId == seasonId && t.ClubId == clubId.Value) != null)
            && (!teamId.HasValue || blockade.User.TeamsPlayers.FirstOrDefault(t => t.SeasonId == seasonId && t.TeamId == teamId.Value) != null)
            && (BType == BlockadeType.All || blockade.BType == BType));
            foreach (var blockade in blockadeValues)
            {
                if (blockade != null)
                {
                    playersToReturn.Add(new PlayersBlockadeShortDTO
                    {
                        UserId = blockade.UserId,
                        UserName = blockade.User.FullName,
                        TeamTitle = blockade.User.TeamsPlayers.FirstOrDefault(t => t.SeasonId == seasonId)?.Team?.Title ?? string.Empty,
                        BlockadeId = blockade.Id,
                        BType = blockade.BType,
                        EndBlockadeDate = blockade.EndDate,
                        StartDate = blockade.StartDate
                    });
                }
            }
            return playersToReturn;
        }


        public List<PlayersBlockadeShortDTO> GetAllBlockadedPlayersForClub(int clubId, int? teamId, int BType = BlockadeType.All)
        {
            var playersToReturn = new List<PlayersBlockadeShortDTO>();
            var blockadeValues = db.PlayersBlockades.Where(blockade => blockade.IsActive == false
            && (blockade.User.TeamsPlayers.FirstOrDefault(t => t.ClubId == clubId) != null)
            && (!teamId.HasValue || blockade.User.TeamsPlayers.FirstOrDefault(t => t.TeamId == teamId.Value) != null)
            && (BType == BlockadeType.All || blockade.BType == BType));
            foreach (var blockade in blockadeValues)
            {
                if (blockade != null)
                {
                    playersToReturn.Add(new PlayersBlockadeShortDTO
                    {
                        UserId = blockade.UserId,
                        UserName = blockade.User.FullName,
                        TeamTitle = blockade.User.TeamsPlayers.FirstOrDefault(t => t.ClubId == clubId)?.Team?.Title ?? string.Empty,
                        BlockadeId = blockade.Id,
                        BType = blockade.BType,
                        EndBlockadeDate = blockade.EndDate,
                        StartDate = blockade.StartDate
                    });
                }
            }

            return playersToReturn;
        }

        public void UnblockPlayer(int userId)
        {
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);

            if (user.BlockadeId != null)
            {
                user.BlockadeId = null;
                user.PlayersBlockade.IsActive = false;
            }

            db.SaveChanges();
        }

        public void UnblockAllBlockadedPlayers()
        {
            var blockadedPlayers = db.PlayersBlockades.Where(blockade => blockade.IsActive == true && blockade.EndDate <= DateTime.Now).AsEnumerable();
            foreach (var blockadedPlayer in blockadedPlayers)
            {
                blockadedPlayer.IsActive = false;
                blockadedPlayer.User.BlockadeId = null;
            }
            db.SaveChanges();
        }

        public Dictionary<Discipline, IEnumerable<DisciplineRoute>> GetRoutesByDisciplinesIds(IEnumerable<int> disciplineIds)
        {
            var disciplines = db.Disciplines.Where(d => disciplineIds.Contains(d.DisciplineId));
            var dictionary = new Dictionary<Discipline, IEnumerable<DisciplineRoute>>();
            foreach (var discipline in disciplines)
            {
                dictionary.Add(discipline, discipline.DisciplineRoutes);
            }
            return dictionary;
        }

        public Dictionary<Discipline, IEnumerable<DisciplineTeamRoute>> GetTeamRoutesByDisciplinesIds(IEnumerable<int> disciplineIds)
        {
            var disciplines = db.Disciplines.Where(d => disciplineIds.Contains(d.DisciplineId));
            var dictionary = new Dictionary<Discipline, IEnumerable<DisciplineTeamRoute>>();
            foreach (var discipline in disciplines)
            {
                dictionary.Add(discipline, discipline.DisciplineTeamRoutes);
            }
            return dictionary;
        }

        private int CreateNewRoute(int userId, int routeId)
        {
            try
            {
                var disciplineId = db.DisciplineRoutes.FirstOrDefault(d => d.Id == routeId)?.DisciplineId;
                var usersRoutes = db.UsersRoutes.Where(c => c.UserId == userId);
                if (usersRoutes.Any())
                {
                    foreach (var usersRoute in usersRoutes)
                    {
                        if (usersRoute.DisciplineRoute?.DisciplineId == disciplineId)
                        {
                            var usersRank = usersRoute?.UsersRanks?.FirstOrDefault();
                            if (usersRank != null)
                                db.UsersRanks.Remove(usersRank);
                            db.UsersRoutes.Remove(usersRoute);
                        }
                    }
                }

                db.UsersRoutes.Add(new UsersRoute { RouteId = routeId, UserId = userId });
                db.SaveChanges();
                return db.UsersRoutes.OrderByDescending(c => c.Id).FirstOrDefault().Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DeleteRoute(int userId)
        {
            try
            {
                var routeDb = db.UsersRoutes.FirstOrDefault(ur => ur.UserId == userId);
                if (routeDb != null)
                {
                    var usersRank = routeDb?.UsersRanks?.FirstOrDefault();
                    if (usersRank != null)
                    {
                        db.UsersRanks.Remove(usersRank);
                    }
                    db.UsersRoutes.Remove(routeDb);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<AthleticRegDto> GetAthleticsRegistrations(int? clubId, int? competitionId, int? disciplineId, int seasonId)
        {
			// risky yet when dealing hundreds of rows it's not that bad. Need to cache later on of course
			var disciplines = db.Disciplines.ToDictionary(p => p.DisciplineId, p => p.Name);

			var query = db.CompetitionDisciplineRegistrations
				.Include(p => p.CompetitionDiscipline)
				.Include(p => p.CompetitionDiscipline.RowingDistance)
				.Include(p => p.CompetitionDisciplineTeam)
				.Include(p => p.CompetitionResult)
				.Include(p => p.Club)
				.Include(p => p.User)
				.Include(p => p.User.AthleteNumbers)
				.Include(p => p.User.TeamsPlayers)
				.Include(p => p.User.PlayerDisciplines);
			if (clubId.HasValue)
				query = query.Where(p => p.ClubId == clubId);
			if (competitionId.HasValue)
				query = query.Where(p => 
					p.CompetitionDiscipline.CompetitionId == competitionId
					&& !p.CompetitionDiscipline.IsDeleted);
			if (disciplineId.HasValue)
				query = query.Where(p =>
					p.CompetitionDiscipline.Id == disciplineId
					&& !p.CompetitionDiscipline.IsDeleted);

			var league = db.Leagues.FirstOrDefault(p => p.LeagueId == competitionId);

			// Vitaly: the whole point of doing all these inlined if statements is to pass that on to SQL server. The more narrow
			// result set is the faster the whole thingy roundtrips
			var regQuery = query.Select(registration => new AthleticRegDto
			{
				UserId = registration.UserId,
				BirthDay = registration.User.BirthDay,
				RowingDistance = registration.CompetitionDiscipline.RowingDistance != null 
					? registration.CompetitionDiscipline.RowingDistance.Name
					: null,
				// Vitaly: there shouldn't be things like that and it's easy to avoid by returning an anonymous type instead of
				// AthleticRegDto. Still more code to type so let's live with that for a while. DTOs are quite diluted here in this
				// project and doesn't distinguish themselves well enough from DB entities, that's a more impacting design flaw to
				// addresss, anonymous types might not even be needed after that.
				FullName = registration.User.FirstName + "|QQ|" + registration.User.LastName + "|QQ|" + registration.User.MiddleName,
				FirstName = registration.User.FirstName,
				LastName = registration.User.LastName,
				IdentNum = registration.User.IdentNum,
				PassportNum = registration.User.PassportNum,
				CategoryName = registration.CompetitionDiscipline != null && registration.CompetitionDiscipline.CompetitionAge != null
					? registration.CompetitionDiscipline.CompetitionAge.age_name 
					: string.Empty,
				RegistrationId = registration.Id,
				ClubName = registration.Club.Name,
				ClubId = registration.ClubId ?? null,
				SeassonId = registration.Club.SeasonId,
				DisciplineId = registration.CompetitionDiscipline.DisciplineId,
				RelatedClubName = registration.User.TeamsPlayers.Any(x => x.IsActive && x.SeasonId == seasonId)
					? registration.User.TeamsPlayers.FirstOrDefault(x => x.IsActive && x.SeasonId == seasonId).Club.Name
					: null,
				WeightDeclaration = (int)(registration.WeightDeclaration ?? 0),
				AthleteNumber = registration.User.AthleteNumbers.Any(y => y.SeasonId == seasonId)
					? registration.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).AthleteNumber1
					: null,
				IsApproved = registration.IsApproved ?? false,
				IsCharged = registration.IsCharged ?? false,
				Heat = registration.CompetitionResult.Any() ? registration.CompetitionResult.FirstOrDefault().Heat : string.Empty,
				Lane = registration.CompetitionResult.Any() ? registration.CompetitionResult.FirstOrDefault().Lane : null,
				TeamNumber = registration.CompetitionDisciplineTeam != null ? registration.CompetitionDisciplineTeam.TeamNumber : null,
				CompetitionDisciplineTeamId = registration.CompetitionDisciplineTeamId,
				IsCoxwain = registration.IsCoxwain,
				//CoxwainFullName = registrations.FirstOrDefault(x => x.CompetitionDisciplineTeamId == registration.CompetitionDisciplineTeamId && x.IsCoxwain == true)?.User.FullName ?? "",
				EntryTime = registration.User.PlayerDisciplines.Any(x => x.DisciplineId == registration.CompetitionDisciplineId)
					? registration.User.PlayerDisciplines.FirstOrDefault(x => x.DisciplineId == registration.CompetitionDisciplineId).EntryTime 
					: "00:00.00",
				Result = registration.Result,
				Rank = registration.Rank
			});

			// now here's where the unavoidable delay happens. Whatever was before might be sliced down to zero using precompiled
			// expressions. And here we might be thinking of adding an index or two... or more :)
			var registrations = regQuery.ToList();

			foreach (var reg in registrations)
			{
				//Vitaly: I know it doesn't make sense. Yet that was the original routine: composed fullname was used by Get___NameByFullName methods
				var fullname = string.IsNullOrEmpty(reg.FullName) ? null : User.GetFullName(
					System.Threading.Thread.CurrentThread.CurrentUICulture,
					reg.FullName.Split(new string[] { "|QQ|" }, StringSplitOptions.None)[0],
					reg.FullName.Split(new string[] { "|QQ|" }, StringSplitOptions.None)[1],
					reg.FullName.Split(new string[] { "|QQ|" }, StringSplitOptions.None)[2]);
				reg.FirstName = string.IsNullOrWhiteSpace(reg.FirstName) ? GetFirstNameByFullName(fullname) : reg.FirstName;
				reg.LastName = string.IsNullOrWhiteSpace(reg.LastName) ? GetLastNameByFullName(fullname) : reg.LastName;
				reg.FullName = fullname;
				reg.DisciplineName = disciplines.ContainsKey(reg.DisciplineId ?? -1)
					? disciplines[reg.DisciplineId.Value]
					: null;
				reg.CoxwainFullName = registrations.FirstOrDefault(p =>
					p.CompetitionDisciplineTeamId == reg.CompetitionDisciplineTeamId &&
					p.IsCoxwain == true)?.FullName;
				reg.CompetitionStartDate = league.LeagueStartDate;
			}

			// Vitaly: removed yield return. You do that as a last step, never as a first one when optimizing db-heavy works.
			return registrations;
        }

        public IEnumerable<AthleticRegDto> GetBicycleRegistrations(int? clubId, int? competitionId, int? compExpId, int seasonId)
        {
            var results = db.BicycleDisciplineRegistrations.Where(x => x.CompetitionExpertiesHeat.CompetitionExperty.CompetitionId == competitionId);

            if(compExpId.HasValue)
            {
                results = results.Where(x => x.CompetitionExpertiesHeatId == compExpId);
            }

            if(clubId.HasValue)
            {
                results = results.Where(x => x.ClubId == clubId);
            }

            foreach(var result in results)
            {
                yield return new AthleticRegDto()
                {
                    UserId = result.UserId.Value,
                    FullName = result.User.FullName,
                    FirstName = result.User.FirstName,
                    LastName = result.User.LastName,
                    IdentNum = result.User.IdentNum,
                    BirthDay = result.User.BirthDay,
                    ClubName = result.Club?.Name,
                    CategoryName = result.CompetitionExpertiesHeat.CompetitionExperty.DisciplineExpertise.Name,
                    Heat = result.CompetitionExpertiesHeat.BicycleCompetitionHeat.Name,
                    RegistrationId = result.Id,
                };
            }
        }

        public CompDiscRegDTO GetPlayersDisciplineRegistration(int competitionId, int userId)
        {
            return db.CompetitionDisciplineRegistrations
                .Include(cd => cd.CompetitionDiscipline.CompetitionAge)
                .Include(cd => cd.User)
                .Where(r => r.CompetitionDiscipline.CompetitionId == competitionId && r.UserId == userId)
                .Select(r => new CompDiscRegDTO
                {
                    RegistrationId = r.Id,
                    UserId = r.UserId,
                    GenterId = r.User.GenderId ?? 0,
                    UserName = r.User.FullName + " - " + r.CompetitionDiscipline.CompetitionAge.age_name + " - " + (r.WeightDeclaration ?? 0).ToString() + " - " + ((r.User.GenderId ?? 0) == 1 ? "M" : "F").ToString(),
                    Name = r.User.FullName,
                    WeightDeclaration = (int)(r.WeightDeclaration ?? 0),
                    TeamTitle = r.CompetitionDiscipline.CompetitionAge.age_name,
                    SessionId = r.WeightliftingSessionId
                }).FirstOrDefault();
        }

        public IEnumerable<CompDiscRegDTO> GetPlayersDisciplineRegistrations(int competitionId)
        {
            return db.CompetitionDisciplineRegistrations
                .Include(cd => cd.CompetitionDiscipline.CompetitionAge)
                .Include(cd => cd.User)
                .Where(r => r.CompetitionDiscipline.CompetitionId == competitionId)
                .ToList()
                .Select(r => new CompDiscRegDTO
                {
                    RegistrationId = r.Id,
                    UserId = r.UserId,
                    GenterId = r.User.GenderId ?? 0,
                    UserName = r.User.FullName + " - " + r.CompetitionDiscipline.CompetitionAge.age_name + " - " + (r.WeightDeclaration ?? 0).ToString() + " - " + ((r.User.GenderId ?? 0) == 1 ? "M" : "F").ToString() + " - " + r.Club.Name,
                    Name = r.User.FullName,
                    WeightDeclaration = (int)(r.WeightDeclaration ?? 0),
                    TeamTitle = r.CompetitionDiscipline.CompetitionAge.age_name,
                    SessionId = r.WeightliftingSessionId
                });
        }


        public IEnumerable<CompDiscRegDTO> GetPlayersDisciplineRegistrationsBySession(int competitionId, int sessionId)
        {
            return db.CompetitionDisciplineRegistrations
                .Include(cd => cd.CompetitionDiscipline.CompetitionAge)
                .Include(cd => cd.User)
                .Where(r => r.CompetitionDiscipline.CompetitionId == competitionId && ((sessionId == 0 && r.WeightliftingSessionId > 0) || r.WeightliftingSessionId == sessionId))
                .Select(r => new CompDiscRegDTO
                {
                    RegistrationId = r.Id,
                    UserId = r.UserId,
                    GenterId = r.User.GenderId ?? 0,
                    UserName = string.Concat(r.User.FirstName, " ", r.User.LastName) + " - " + r.CompetitionDiscipline.CompetitionAge.age_name + " - " + (r.WeightDeclaration ?? 0).ToString() + " - " + ((r.User.GenderId ?? 0) == 1 ? "M" : "F").ToString(),
                    WeightDeclaration = (int)(r.WeightDeclaration ?? 0),
                    TeamTitle = r.CompetitionDiscipline.CompetitionAge.age_name,
                    SessionId = r.WeightliftingSessionId,
                    FullName = string.Concat(r.User.FirstName, " ", r.User.LastName),
                    BirthDate = r.User.BirthDay,
                    ClubName = r.Club.Name,
                    Weight = r.Weight,
                    IsWeightOk = (!r.Weight.HasValue || (r.CompetitionDiscipline.CompetitionAge.from_weight <= r.Weight && r.CompetitionDiscipline.CompetitionAge.to_weight > r.Weight)) ? true : false,
                    Lifting = r.CompetitionResult.FirstOrDefault().Lifting1,
                    Push = r.CompetitionResult.FirstOrDefault().Push1
                });
        }

        private void CreateNewRank(int userId, int usersRouteId, int rankId)
        {
            try
            {
                var usersRank = db.UsersRanks.FirstOrDefault(c => c.UsersRouteId == usersRouteId);
                if (usersRank == null)
                {
                    db.UsersRanks.Add(new UsersRank { RankId = rankId, UsersRouteId = usersRouteId, UserId = userId });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DeleteRank(int userId)
        {
            try
            {
                var rank = db.UsersRanks.FirstOrDefault(c => c.UserId == userId);
                if (rank != null)
                {
                    db.UsersRanks.Remove(rank);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateUsersRoute(int userId, int? routeId, int? rankId, out string errorMessage)
        {
            try
            {
                var routeDb = db.UsersRoutes.FirstOrDefault(c => c.UserId == userId && c.RouteId == routeId);
                if (routeId != null)
                {
                    if (routeDb == null)
                    {
                        var newUsersRouteId = CreateNewRoute(userId, routeId.Value);

                        if (rankId != null)
                            CreateNewRank(userId, newUsersRouteId, rankId.Value);
                    }
                    else
                    {
                        DeleteRoute(userId);
                        var newUsersRouteId = CreateNewRoute(userId, routeId.Value);
                        if (rankId != null)
                        {
                            CreateNewRank(userId, newUsersRouteId, rankId.Value);
                        }
                    }
                }
                else
                {
                    DeleteRank(userId);
                    DeleteRoute(userId);
                }
                errorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        public IEnumerable<SelectedRoutesDto> GetSelectedRoutes(int userId)
        {
            var user = db.Users.FirstOrDefault(c => c.UserId == userId);
            if (user != null)
            {
                var userRoutes = user?.UsersRoutes;
                if (userRoutes.Any())
                {
                    foreach (var userRoute in userRoutes)
                    {
                        yield return new SelectedRoutesDto
                        {
                            DisciplineId = userRoute?.DisciplineRoute?.DisciplineId ?? 0,
                            UsersRouteId = userRoute.RouteId,
                            UsersRankId = userRoute?.UsersRanks?.FirstOrDefault()?.RankId ?? 0
                        };
                    }
                }
            }
        }

        public void UpdatePlayersFromExcelImport(List<ExcelPlayerDto> playersToUpdate, int updateUserId)
        {
            foreach (var playerDto in playersToUpdate)
            {
                var teamPlayer = db.TeamsPlayers.FirstOrDefault(tp => tp.UserId == playerDto.UserId
                                                                && tp.SeasonId == playerDto.SeasonId
                                                                && tp.TeamId == playerDto.TeamId);
                if (teamPlayer != null)
                {
                    try
                    {
                        UpdateObjects(teamPlayer, playerDto, updateUserId);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            db.SaveChanges();
        }

        private void UpdateObjects(TeamsPlayer teamPlayer, ExcelPlayerDto playerDto, int updateUserId)
        {
            try
            {
                var discount = teamPlayer.User.PlayerDiscounts.FirstOrDefault(c => c.PlayerId == teamPlayer.UserId
                                                                  && c.SeasonId == teamPlayer.SeasonId
                                                                  && c.ClubId == teamPlayer.ClubId);
                if (discount != null)
                    discount.Amount = playerDto.ParticipationDiscount;
                else
                {
                    teamPlayer.User.PlayerDiscounts.Add(new PlayerDiscount
                    {
                        PlayerId = teamPlayer.UserId,
                        TeamId = teamPlayer.TeamId,
                        LeagueId = teamPlayer.LeagueId,
                        ClubId = teamPlayer.ClubId,
                        SeasonId = playerDto.SeasonId,
                        DiscountType = (int)PlayerDiscountTypes.ManagerParticipationDiscount,
                        Amount = playerDto.ParticipationDiscount,
                        UpdateUserId = updateUserId,
                        DateUpdated = DateTime.Now,
                    });
                }

                teamPlayer.Paid = playerDto.Paid;
                teamPlayer.Comment = playerDto.Comments;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<int> GetActiveClubPlayersIds(int clubId, int seasonId)
        {
            return db.TeamsPlayers
                .Include(x => x.User)
                .Include(x => x.User.PlayersBlockade)
                .Where(c =>
                    c.ClubId == clubId &&
                    c.SeasonId == seasonId &&
                    c.IsApprovedByManager == true &&
                    (!c.User.PlayersBlockade.IsActive || c.User.PlayersBlockade == null))
                .Select(c => c.UserId)
                .Distinct()
                .ToList();
        }

        public List<int> GetActiveTeamPlayersIds(int teamId, int seasonId)
        {
            return db.TeamsPlayers
                .Include(x => x.User)
                .Include(x => x.User.PlayersBlockade)
                .Where(c =>
                    c.TeamId == teamId &&
                    c.SeasonId == seasonId &&
                    c.IsApprovedByManager == true &&
                    (!c.User.PlayersBlockade.IsActive || c.User.PlayersBlockade == null))
                .Select(c => c.UserId)
                .Distinct()
                .ToList();
        }

        public void RegisterPlayersInCompetition(IEnumerable<CompetitionRegistrationForm> playersRegistrations, int clubId, int competitionId, int seasonId, bool isHighAuthority, int? maxAllowed, string created, string creator)
        {


            var allRegistrationsCount = CheckCompetitionRegistrationsCount(null, competitionId, seasonId);
            var clubRegistrationsCount = CheckCompetitionRegistrationsCount(clubId, competitionId, seasonId);
            List<int> gymnastsToRegister = new List<int>();

            var groupedPlayersRegIndividual = playersRegistrations.Where(r => !r.IsTeam).GroupBy(r => r.CompetitionRouteId);
            var groupedPlayersRegTeam = playersRegistrations.Where(r => r.IsTeam).GroupBy(r => r.CompetitionRouteId);

            var totalPlayersToRegister = 0;
            foreach (var groupComp in groupedPlayersRegIndividual)
            {
                List<int> regsInRoute = new List<int>();
                foreach (var reg in groupComp)
                {
                    regsInRoute = regsInRoute.Union(reg.PlayersIds.ToList()).Distinct().ToList();
                }
                totalPlayersToRegister += regsInRoute.Count();
            }

            foreach (var groupComp in groupedPlayersRegTeam)
            {
                List<int> regsInRoute = new List<int>();
                foreach (var reg in groupComp)
                {
                    regsInRoute = regsInRoute.Union(reg.PlayersIds.ToList()).Distinct().ToList();
                }
                totalPlayersToRegister += regsInRoute.Count();
            }

            Club club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            MaximumRequiredPerClubException exception = new MaximumRequiredPerClubException();
            if (playersRegistrations == null)
                playersRegistrations = new List<CompetitionRegistrationForm>();

            foreach (var playerRegistration in playersRegistrations)
            {
                int? MaxPossibleRegistrations = null;
                string routeName = null;
                if (!playerRegistration.IsTeam)
                {
                    CompetitionRoute compRoute = db.CompetitionRoutes.FirstOrDefault(p => p.Id == playerRegistration.CompetitionRouteId);
                    routeName = $"{compRoute.Discipline.Name} - {compRoute.DisciplineRoute.Route} - {compRoute.RouteRank.Rank}";
                    var competitionRouteClubs = club.CompetitionRouteClubs.FirstOrDefault(c => c.CompetitionRouteId == playerRegistration.CompetitionRouteId);
                    MaxPossibleRegistrations = competitionRouteClubs != null ? competitionRouteClubs.MaximumRegistrationsAllowed : null;
                }
                else
                {
                    CompetitionTeamRoute compRoute = db.CompetitionTeamRoutes.FirstOrDefault(p => p.Id == playerRegistration.CompetitionRouteId);
                    routeName = $"[T] {compRoute.Discipline.Name} - {compRoute.DisciplineTeamRoute.Route} - {compRoute.RouteTeamRank.Rank}";
                    var competitionRouteClubs = club.CompetitionTeamRouteClubs.FirstOrDefault(c => c.CompetitionTeamRouteId == playerRegistration.CompetitionRouteId);
                    MaxPossibleRegistrations = competitionRouteClubs != null ? competitionRouteClubs.MaximumRegistrationsAllowed : null;
                }
                if (playerRegistration.PlayersIds != null && playerRegistration.PlayersIds.Any())
                {
                    if (MaxPossibleRegistrations.HasValue && MaxPossibleRegistrations.Value < playerRegistration.PlayersIds.Count())
                    {
                        exception.AddError(routeName, MaxPossibleRegistrations.Value);
                    }
                }
            }

            if (exception.HasErrors())
            {
                throw exception;
            }

            try
            {
                var competitionRegistrations = db.CompetitionRegistrations
                    .Where(cr => cr.LeagueId == competitionId && cr.SeasonId == seasonId && cr.ClubId == clubId);
                if (playersRegistrations != null && playersRegistrations.Any())
                {
                    if (!isHighAuthority && maxAllowed != null)
                    {
                        if (maxAllowed.Value >= allRegistrationsCount - clubRegistrationsCount + totalPlayersToRegister)
                        {
                            if (competitionRegistrations != null)
                                db.CompetitionRegistrations.RemoveRange(competitionRegistrations);
                        }
                        else
                        {
                            throw new MaximumRequiredException() { NumOfRegistrationsLeft = maxAllowed.Value - (allRegistrationsCount - clubRegistrationsCount + totalPlayersToRegister) };
                        }
                    }
                    else
                    {
                        if (competitionRegistrations != null)
                            db.CompetitionRegistrations.RemoveRange(competitionRegistrations);
                    }
                }

                foreach (var playerRegistration in playersRegistrations)
                {
                    if (playerRegistration.PlayersIds != null && playerRegistration.PlayersIds.Any())
                    {
                        foreach (var userId in playerRegistration.PlayersIds)
                        {
                            db.CompetitionRegistrations.Add(new CompetitionRegistration
                            {
                                UserId = userId,
                                ClubId = clubId,
                                LeagueId = competitionId,
                                CompetitionRouteId = playerRegistration.CompetitionRouteId,
                                SeasonId = seasonId,
                                CompositionNumber = playerRegistration.CompositionNumber,
                                InstrumentId = playerRegistration.InstrumentId,
                                IsActive = true,
                                IsTeam = playerRegistration.IsTeam,
                                Created = created,
                                Creator = creator
                            });
                        }
                    }
                    //if (playerRegistration.Composition.HasValue)
                    //{
                    if (playerRegistration.IsTeam)
                    {
                        UpdateAdditinalTeamGymnastic(playerRegistration.CompetitionRouteId, playerRegistration.AdditionalGymnasticId, clubId, competitionId, seasonId, playerRegistration.CompositionNumber);
                    }
                    else
                        UpdateAdditinalGymnastic(playerRegistration.CompetitionRouteId, playerRegistration.AdditionalGymnasticId, clubId, competitionId, seasonId, playerRegistration.CompositionNumber);
                    //}

                    /*
                    if (playerRegistration.Composition.HasValue && playerRegistration.CompositionNumber == 0)
                    {
                        if (playerRegistration.IsTeam)
                        {
                            UpdateAdditinalTeamGymnastic(playerRegistration.CompetitionRouteId, playerRegistration.AdditionalGymnasticId, clubId, competitionId, seasonId, playerRegistration.CompositionNumber);
                        }
                        else
                            UpdateAdditinalGymnastic(playerRegistration.CompetitionRouteId, playerRegistration.AdditionalGymnasticId, clubId, competitionId, seasonId, playerRegistration.CompositionNumber);
                    }
                    if (playerRegistration.Composition.HasValue && playerRegistration.CompositionNumber >= 1)
                    {
                        if (playerRegistration.IsTeam)
                        {
                            UpdateAdditinalTeamGymnastic(playerRegistration.CompetitionRouteId, playerRegistration.AdditionalGymnasticId, clubId, competitionId, seasonId, playerRegistration.CompositionNumber);
                        }
                        else
                            UpdateAdditinalGymnastic(playerRegistration.CompetitionRouteId, playerRegistration.AdditionalGymnasticId, clubId, competitionId, seasonId, playerRegistration.CompositionNumber);
                    }
                    */
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IDictionary<Season, AveragePlayersStatistics> GetGeneralStatistics(IDictionary<Season, IEnumerable<AveragePlayersStatistics>> guestStatistics,
            IDictionary<Season, IEnumerable<AveragePlayersStatistics>> homeStatistics, int teamId)
        {
            var generalDictionary = new Dictionary<Season, AveragePlayersStatistics>();
            if (guestStatistics != null && homeStatistics != null)
            {
                if (homeStatistics.Any() || guestStatistics.Any())
                {
                    var seasonsIds = homeStatistics.Union(guestStatistics).Select(c => c.Key.Id).Distinct();
                    foreach (var seasonId in seasonsIds)
                    {
                        var gamesCount = db.GameStatistics.Where(s => s.GamesCycle.Stage.League.SeasonId == seasonId && s.TeamId == teamId).AsNoTracking()
                            ?.Select(c => c.GameId)?.Distinct()?.Count() ?? 0;
                        var seasonGames = homeStatistics.Where(c => c.Key.Id == seasonId)
                            .Union(guestStatistics.Where(c => c.Key.Id == seasonId))
                            .Select(c => c.Value);
                        var statistics = new List<AveragePlayersStatistics>();
                        if (seasonGames.Any())
                        {
                            foreach (var games in seasonGames)
                            {
                                foreach (var game in games)
                                {
                                    statistics.Add(game);
                                }
                            }
                        }
                        var plStat = new AveragePlayersStatistics();

                        plStat.Min = statistics.Sum(c => c.Min);

                        plStat.GP = gamesCount.ToString();

                        plStat.GamesCount = gamesCount;

                        plStat.FG = statistics.Sum(c => c.FG);

                        plStat.FGA = statistics.Sum(c => c.FGA);

                        plStat.ThreePT = statistics.Sum(c => c.ThreePT);

                        plStat.ThreePA = statistics.Sum(c => c.ThreePA);

                        plStat.TwoPT = statistics.Sum(c => c.TwoPT);

                        plStat.TwoPA = statistics.Sum(c => c.TwoPA);

                        plStat.FT = statistics.Sum(c => c.FT);

                        plStat.FTA = statistics.Sum(c => c.FTA);

                        plStat.OREB = statistics.Sum(c => c.OREB);

                        plStat.DREB = statistics.Sum(c => c.DREB);

                        plStat.REB = statistics.Sum(c => c.REB);

                        plStat.AST = statistics.Sum(c => c.AST);

                        plStat.TO = statistics.Sum(c => c.TO);

                        plStat.STL = statistics.Sum(c => c.STL);

                        plStat.BLK = statistics.Sum(c => c.BLK);

                        plStat.PF = statistics.Sum(c => c.PF);

                        plStat.PTS = statistics.Sum(c => c.PTS);

                        plStat.FGM = statistics.Sum(c => c.FGM);

                        plStat.FTM = statistics.Sum(c => c.FTM);

                        plStat.EFF = statistics.Sum(c => c.EFF);

                        plStat.PlusMinus = statistics.Sum(c => c.PlusMinus);

                        generalDictionary.Add(db.Seasons.FirstOrDefault(s => s.Id == seasonId), plStat);
                    }
                }
            }
            return generalDictionary;
        }


        private void UpdateAdditinalGymnastic(int competitionRouteId, int? userId, int clubId, int competitionId, int seasonId, int compositionNumber = 0)
        {
            var additinalGymnastic = db.AdditionalGymnastics.FirstOrDefault(ag => ag.CompetitionRouteId == competitionRouteId
                                        && ag.ClubId == clubId && ag.SeasonId == seasonId
                                        && ag.LeagueId == competitionId
                                        && ag.CompositionNumber == compositionNumber);
            if (additinalGymnastic != null && userId.HasValue)
            {
                additinalGymnastic.UserId = userId.Value;
            }
            else if (additinalGymnastic != null && !userId.HasValue)
            {
                db.AdditionalGymnastics.Remove(additinalGymnastic);
            }
            else if (additinalGymnastic == null && userId.HasValue)
            {
                db.AdditionalGymnastics.Add(new AdditionalGymnastic
                {
                    UserId = userId.Value,
                    ClubId = clubId,
                    CompetitionRouteId = competitionRouteId,
                    LeagueId = competitionId,
                    SeasonId = seasonId,
                    CompositionNumber = compositionNumber
                });
            }
            db.SaveChanges();
        }

        private void UpdateAdditinalTeamGymnastic(int competitionRouteId, int? userId, int clubId, int competitionId, int seasonId, int compositionNumber = 0)
        {
            var additinalGymnastic = db.AdditionalTeamGymnastics.FirstOrDefault(ag => ag.CompetitionRouteId == competitionRouteId
                                        && ag.ClubId == clubId && ag.SeasonId == seasonId
                                        && ag.LeagueId == competitionId
                                        && ag.CompositionNumber == compositionNumber);
            if (additinalGymnastic != null && userId.HasValue)
            {
                additinalGymnastic.UserId = userId.Value;
            }
            else if (additinalGymnastic != null && !userId.HasValue)
            {
                db.AdditionalTeamGymnastics.Remove(additinalGymnastic);
            }
            else if (additinalGymnastic == null && userId.HasValue)
            {
                db.AdditionalTeamGymnastics.Add(new AdditionalTeamGymnastic
                {
                    UserId = userId.Value,
                    ClubId = clubId,
                    CompetitionRouteId = competitionRouteId,
                    LeagueId = competitionId,
                    SeasonId = seasonId,
                    CompositionNumber = compositionNumber
                });
            }
            db.SaveChanges();
        }

        public IEnumerable<GymnasticDto> GetSportsmenRegs(int? clubId, int leagueId, int seasonId)
        {
            var sportsmenRegs = db.SportsRegistrations.Where(r => r.ClubId == clubId && r.LeagueId == leagueId && r.SeasonId == seasonId);
            if (sportsmenRegs.Any())
            {
                foreach (var sportsman in sportsmenRegs)
                {
                    yield return new GymnasticDto
                    {
                        RegistrationId = sportsman.Id,
                        ClubName = sportsman.Club?.Name,
                        FullName = sportsman?.User?.FullName,
                        BirthDate = sportsman?.User?.BirthDay,
                        IdentNum = sportsman?.User?.IdentNum,
                        ClubNumber = sportsman?.Club?.ClubNumber,
                        FinalScore = sportsman?.FinalScore,
                        Position = sportsman?.Position,
                        PassportNum = sportsman?.User?.PassportNum,
                        IsReligious = sportsman?.User?.IsReligious
                    };
                }
            }
        }

        public void UpdateTennisPlayersOrder(List<PlayerOrder> playersList)
        {
            foreach (var playerOrder in playersList)
            {
                var teamPlayer = db.TeamsPlayers.Find(playerOrder.RegistrationId);
                if (teamPlayer != null)
                {
                    teamPlayer.TennisPositionOrder = playerOrder.PositionOrder;
                }
            }
        }

        public void DeleteSportsmanRegistration(int id)
        {
            var registration = db.SportsRegistrations.FirstOrDefault(c => c.Id == id);
            if (registration != null)
            {
                db.SportsRegistrations.Remove(registration);
            }
            db.SaveChanges();
        }

        public void RegisterSportsmenInCompetition(IEnumerable<int> sportsmenIds, int clubId, int leagueId, int seasonId)
        {
            if (sportsmenIds != null && sportsmenIds.Any())
            {
                var currentRegistrations = db.SportsRegistrations.Where(c => c.ClubId == clubId && c.LeagueId == leagueId
                    && c.SeasonId == seasonId);
                if (currentRegistrations.Any())
                {
                    db.SportsRegistrations.RemoveRange(currentRegistrations);
                }
                foreach (var userId in sportsmenIds)
                {
                    db.SportsRegistrations.Add(new SportsRegistration
                    {
                        UserId = userId,
                        ClubId = clubId,
                        LeagueId = leagueId,
                        SeasonId = seasonId
                    });
                }
            }
            db.SaveChanges();
        }

        public bool RegisterSportsmenInCompetitionDiscipline(IEnumerable<int> sportsmenIds, int clubId,
            int competitionDisciplineId, bool isAllowed, int? maxRegistrations, int currentRegistrationCount)
        {
            var thereAreChanges = false;
            if (sportsmenIds == null)
            {
                sportsmenIds = new int[0];
            }
            var currentRegistered = db.CompetitionDisciplineRegistrations
            .Where(r => r.ClubId == clubId
                        && r.CompetitionDisciplineId == competitionDisciplineId)
            .ToList();
            var current = db.CompetitionDisciplineRegistrations
                .Where(r => r.ClubId == clubId
                            && r.CompetitionDisciplineId == competitionDisciplineId
                            && !sportsmenIds.Contains(r.UserId))
                .ToList();
            var newIds = sportsmenIds.Where(si =>
                !db.CompetitionDisciplineRegistrations
                    .Where(r => r.ClubId == clubId
                            && r.CompetitionDisciplineId == competitionDisciplineId)
                    .Select(cdr => cdr.UserId)
                    .Contains(si))
                .ToList();



            if (!isAllowed && maxRegistrations != null)
            {
                var attemptRegisterCount = sportsmenIds.Count();
                int allRegisteredWithoutThisClub = currentRegistrationCount - currentRegistered.Count();
                if (maxRegistrations.Value < allRegisteredWithoutThisClub + attemptRegisterCount)
                {
                    throw new MaximumRequiredException() { NumOfRegistrationsLeft = maxRegistrations.Value - allRegisteredWithoutThisClub };
                }
            }


            if (current.Any())
            {
                db.CompetitionDisciplineRegistrations.RemoveRange(current);
                thereAreChanges = true;
            }


            if (newIds.Any())
            {
                foreach (var sportsmenId in newIds)
                {
                    db.CompetitionDisciplineRegistrations.Add(new CompetitionDisciplineRegistration
                    {
                        UserId = sportsmenId,
                        ClubId = clubId,
                        CompetitionDisciplineId = competitionDisciplineId
                    });
                }

                thereAreChanges = true;
            }

            if (thereAreChanges)
            {
                try
                {
                    var competitionDisciplines = db.CompetitionDisciplines.FirstOrDefault(c => c.Id == competitionDisciplineId);
                    var leagueId = competitionDisciplines.League.LeagueId;
                    db.SaveChanges();
                    UpdateClubsBalance(clubId, leagueId);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            return true;
        }

        public void DeleteDisciplineRegistration(int id)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(t => t.Id == id);
            if (registration != null)
            {

                var clubId = registration.ClubId.Value;
                var leagueId = registration.CompetitionDiscipline.League.LeagueId;
                db.CompetitionDisciplineRegistrations.Remove(registration);
                db.SaveChanges();
                UpdateClubsBalance(clubId, leagueId);
                db.SaveChanges();
            }
        }

        public void DeleteBicycleRegistration(int id)
        {
            var registration = db.BicycleDisciplineRegistrations.FirstOrDefault(t => t.Id == id);
            if (registration != null)
            {
                db.BicycleDisciplineRegistrations.Remove(registration);
                db.SaveChanges();
            }
        }

        public void ResetDisciplineRegistration(int id)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(t => t.Id == id);
            if (registration != null)
            {
                var results = registration.CompetitionResult.ToList();
                foreach (var result in results)
                {
                    result.CombinedPoint = null;
                    result.AlternativeResult = 0;
                    result.CombinedPoint = null;
                    result.SortValue = null;
                    result.Result = string.Empty;
                    result.Rank = null;
                }
                db.SaveChanges();
            }
        }





        public void UpdateSportsmanRegistrationForDiscipline(int clubId, int competitionDisciplineId, int sportsmanId,
            int weightDeclaration)
        {
            var disciplineRegistration = db.CompetitionDisciplineRegistrations
                .Where(cdr =>
                    cdr.ClubId == clubId && cdr.CompetitionDisciplineId == competitionDisciplineId &&
                    cdr.UserId == sportsmanId).First();
            disciplineRegistration.WeightDeclaration = weightDeclaration;
            db.SaveChanges();
        }

        public IEnumerable<CompDiscRegDTO> GetPlayersDisciplineRegistrations(int clubId, int competitionDisciplineId)
        {
            return db.CompetitionDisciplineRegistrations
                .Include(cd => cd.CompetitionDiscipline.CompetitionAge)
                .Include(cd => cd.User)
                .Where(r => r.ClubId == clubId && r.CompetitionDisciplineId == competitionDisciplineId)
                .Select(r => new CompDiscRegDTO
                {
                    UserId = r.UserId,
                    UserName = string.Concat(r.User.FirstName, " ", r.User.LastName),
                    WeightDeclaration = (int)(r.WeightDeclaration ?? 0),
                    TeamTitle = r.CompetitionDiscipline.CompetitionAge.age_name
                });
        }

        public IEnumerable<GymnasticDto> GetPlayersRegistrationsWithoutInstruments(int? clubId, int leagueId, int seasonId, int? competitionRouteId = null, int? teamId = null)
        {
            List<CompetitionRegistration> playersRegistrations;
            List<AdditionalGymnastic> additionPlayersRegistrations;

            if (clubId.HasValue)
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                r.ClubId == clubId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                !r.InstrumentId.HasValue &&
                                !r.IsRegisteredByExcel &&
                                !r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();

                additionPlayersRegistrations = db.AdditionalGymnastics
                    .Where(r => r.ClubId == clubId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();
            }
            else if (competitionRouteId.HasValue)
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                r.CompetitionRouteId == competitionRouteId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                !r.InstrumentId.HasValue &&
                                !r.IsRegisteredByExcel &&
                                !r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();

                additionPlayersRegistrations = db.AdditionalGymnastics
                    .Where(r => r.CompetitionRouteId == competitionRouteId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();
            }
            else
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                !r.InstrumentId.HasValue &&
                                !r.IsRegisteredByExcel &&
                                !r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();

                additionPlayersRegistrations = db.AdditionalGymnastics
                    .Where(r => r.LeagueId == leagueId &&
                                r.SeasonId == seasonId)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();
            }

            if (playersRegistrations.Any())
            {
                foreach (var gymnastic in playersRegistrations)
                {
                    yield return new GymnasticDto
                    {
                        RegistrationId = gymnastic.Id,
                        ClubName = gymnastic.Club?.Name,
                        FullName = gymnastic?.User?.FullName,
                        BirthDate = gymnastic?.User?.BirthDay,
                        IdentNum = gymnastic?.User?.IdentNum,
                        PassportNum = gymnastic?.User?.PassportNum,
                        Route = gymnastic?.CompetitionRoute?.DisciplineRoute?.Route,
                        Rank = gymnastic?.CompetitionRoute?.RouteRank?.Rank,
                        Composition = gymnastic?.CompetitionRoute?.Composition,
                        PositionOrder = gymnastic.CompetitionRoute?.Composition != null ? null : gymnastic.OrderNumber,
                        ClubNumber = gymnastic?.Club?.ClubNumber,
                        FinalScore = gymnastic.FinalScore,
                        Position = gymnastic.Position,
                        IsTeam = false,
                        SecondComposition = gymnastic.CompetitionRoute?.SecondComposition,
                        ThirdComposition = gymnastic.CompetitionRoute?.ThirdComposition,
                        FourthComposition = gymnastic.CompetitionRoute?.FourthComposition,
                        FifthComposition = gymnastic.CompetitionRoute?.FifthComposition,
                        SixthComposition = gymnastic.CompetitionRoute?.SixthComposition,
                        SeventhComposition = gymnastic.CompetitionRoute?.SeventhComposition,
                        EighthComposition = gymnastic.CompetitionRoute?.EighthComposition,
                        NinthComposition = gymnastic.CompetitionRoute?.NinthComposition,
                        TenthComposition = gymnastic.CompetitionRoute?.TenthComposition,
                        CompetitionRouteId = gymnastic?.CompetitionRouteId,
                        CompositionNumber = gymnastic.CompositionNumber ?? 0,
                        Created = gymnastic.Created,
                        Creator = gymnastic.Creator,
                        IsReligious = gymnastic?.User?.IsReligious
                    };
                }
            }
            if (additionPlayersRegistrations.Any())
            {
                foreach (var gymnastic in additionPlayersRegistrations)
                {
                    yield return new GymnasticDto
                    {
                        RegistrationId = gymnastic.Id,
                        ClubName = gymnastic.Club?.Name,
                        FullName = gymnastic?.User?.FullName,
                        BirthDate = gymnastic?.User?.BirthDay,
                        IdentNum = gymnastic?.User?.IdentNum,
                        PassportNum = gymnastic?.User?.PassportNum,
                        ClubNumber = gymnastic.Club?.ClubNumber,
                        Route = gymnastic?.CompetitionRoute?.DisciplineRoute?.Route,
                        Rank = gymnastic?.CompetitionRoute?.RouteRank?.Rank,
                        Composition = gymnastic?.CompetitionRoute?.Composition,
                        SecondComposition = gymnastic.CompetitionRoute?.SecondComposition,
                        ThirdComposition = gymnastic.CompetitionRoute?.ThirdComposition,
                        FourthComposition = gymnastic.CompetitionRoute?.FourthComposition,
                        FifthComposition = gymnastic.CompetitionRoute?.FifthComposition,
                        SixthComposition = gymnastic.CompetitionRoute?.SixthComposition,
                        SeventhComposition = gymnastic.CompetitionRoute?.SeventhComposition,
                        EighthComposition = gymnastic.CompetitionRoute?.EighthComposition,
                        NinthComposition = gymnastic.CompetitionRoute?.NinthComposition,
                        TenthComposition = gymnastic.CompetitionRoute?.TenthComposition,
                        CompetitionRouteId = gymnastic?.CompetitionRouteId,
                        CompositionNumber = gymnastic.CompositionNumber,
                        IsAdditional = true,
                        IsTeam = false,
                        IsReligious = gymnastic?.User?.IsReligious
                    };
                }
            }
        }

        public IEnumerable<GymnasticDto> GetTeamPlayersRegistrationsWithoutInstruments(int? clubId, int leagueId, int seasonId, int? competitionRouteId = null, int? teamId = null)
        {
            var playersRegistrations = new List<CompetitionRegistration>();
            var additionPlayersRegistrations = new List<AdditionalTeamGymnastic>();
            if (clubId.HasValue)
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                r.ClubId == clubId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                !r.InstrumentId.HasValue &&
                                !r.IsRegisteredByExcel &&
                                r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();

                additionPlayersRegistrations = db.AdditionalTeamGymnastics
                    .Where(r => r.ClubId == clubId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();
            }
            else if (competitionRouteId.HasValue)
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                r.CompetitionRouteId == competitionRouteId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                !r.InstrumentId.HasValue &&
                                !r.IsRegisteredByExcel &&
                                r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();

                additionPlayersRegistrations = db.AdditionalTeamGymnastics
                    .Where(r => r.CompetitionRouteId == competitionRouteId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();
            }
            else
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                !r.InstrumentId.HasValue &&
                                !r.IsRegisteredByExcel &&
                                r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();

                additionPlayersRegistrations = db.AdditionalTeamGymnastics
                    .Where(r => r.LeagueId == leagueId &&
                                r.SeasonId == seasonId)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();
            }

            if (playersRegistrations.Any())
            {
                foreach (var gymnastic in playersRegistrations)
                {
                    yield return new GymnasticDto
                    {
                        RegistrationId = gymnastic.Id,
                        ClubName = gymnastic.Club?.Name,
                        FullName = gymnastic?.User?.FullName,
                        BirthDate = gymnastic?.User?.BirthDay,
                        IdentNum = gymnastic?.User?.IdentNum,
                        PassportNum = gymnastic?.User?.PassportNum,
                        IsTeam = true,
                        Route = gymnastic?.CompetitionTeamRoute?.DisciplineTeamRoute?.Route,
                        Rank = gymnastic?.CompetitionTeamRoute?.RouteTeamRank?.Rank,
                        Composition = gymnastic?.CompetitionTeamRoute?.Composition,
                        PositionOrder = gymnastic.CompetitionTeamRoute?.Composition != null ? null : gymnastic.OrderNumber,
                        ClubNumber = gymnastic?.Club?.ClubNumber,
                        FinalScore = gymnastic.FinalScore,
                        Position = gymnastic.Position,
                        SecondComposition = gymnastic.CompetitionTeamRoute?.SecondComposition,
                        ThirdComposition = gymnastic.CompetitionTeamRoute?.ThirdComposition,
                        FourthComposition = gymnastic.CompetitionTeamRoute?.FourthComposition,
                        FifthComposition = gymnastic.CompetitionTeamRoute?.FifthComposition,
                        SixthComposition = gymnastic.CompetitionTeamRoute?.SixthComposition,
                        SeventhComposition = gymnastic.CompetitionTeamRoute?.SeventhComposition,
                        EighthComposition = gymnastic.CompetitionTeamRoute?.EighthComposition,
                        NinthComposition = gymnastic.CompetitionTeamRoute?.NinthComposition,
                        TenthComposition = gymnastic.CompetitionTeamRoute?.TenthComposition,
                        CompetitionRouteId = gymnastic?.CompetitionRouteId,
                        CompositionNumber = gymnastic.CompositionNumber ?? 0,
                        Created = gymnastic.Created,
                        Creator = gymnastic.Creator,
                        IsReligious = gymnastic?.User?.IsReligious
                    };
                }
            }
            if (additionPlayersRegistrations.Any())
            {
                foreach (var gymnastic in additionPlayersRegistrations)
                {
                    yield return new GymnasticDto
                    {
                        RegistrationId = gymnastic.Id,
                        ClubName = gymnastic.Club?.Name,
                        FullName = gymnastic?.User?.FullName,
                        ClubNumber = gymnastic.Club?.ClubNumber,
                        BirthDate = gymnastic?.User?.BirthDay,
                        IdentNum = gymnastic?.User?.IdentNum,
                        PassportNum = gymnastic?.User?.PassportNum,
                        IsTeam = true,
                        Route = gymnastic?.CompetitionTeamRoute?.DisciplineTeamRoute?.Route,
                        Rank = gymnastic?.CompetitionTeamRoute?.RouteTeamRank?.Rank,
                        Composition = gymnastic?.CompetitionTeamRoute?.Composition,
                        SecondComposition = gymnastic.CompetitionTeamRoute?.SecondComposition,
                        ThirdComposition = gymnastic.CompetitionTeamRoute?.ThirdComposition,
                        FourthComposition = gymnastic.CompetitionTeamRoute?.FourthComposition,
                        FifthComposition = gymnastic.CompetitionTeamRoute?.FifthComposition,
                        SixthComposition = gymnastic.CompetitionTeamRoute?.SixthComposition,
                        SeventhComposition = gymnastic.CompetitionTeamRoute?.SeventhComposition,
                        EighthComposition = gymnastic.CompetitionTeamRoute?.EighthComposition,
                        NinthComposition = gymnastic.CompetitionTeamRoute?.NinthComposition,
                        TenthComposition = gymnastic.CompetitionTeamRoute?.TenthComposition,
                        CompetitionRouteId = gymnastic?.CompetitionRouteId,
                        CompositionNumber = gymnastic.CompositionNumber,
                        IsAdditional = true,
                        IsReligious = gymnastic?.User?.IsReligious
                    };
                }
            }
        }

        public IEnumerable<GymnasticExportDto> GetDisciplinePlayers(int disciplineId, int seasonId)
        {
            List<PlayerDiscipline> players = db.PlayerDisciplines
                                                .Where(pd => pd.DisciplineId == disciplineId && pd.SeasonId == seasonId)
                                                .ToList();

            foreach (var p in players)
            {
                var player = db.TeamsPlayers.Where(tp => tp.UserId == p.User.UserId && tp.SeasonId == seasonId).FirstOrDefault();
                if (player != null && player.IsApprovedByManager != null && player.IsActive == true && player.IsApprovedByManager == true)
                {
                    var routeId = db.UsersRoutes.FirstOrDefault(ur => ur.UserId == p.User.UserId)?.RouteId;
                    var route = db.DisciplineRoutes.FirstOrDefault(d => d.Id == routeId && d.DisciplineId == disciplineId)?.Route;
                    var rank = db.UsersRanks.FirstOrDefault(r => r.UserId == p.User.UserId)?.RouteRank.Rank;
                    var identNum = p.User.IdentNum;
                    yield return new GymnasticExportDto
                    {
                        FirstName = p.User.FirstName,
                        LastName = p.User.LastName,
                        ClubName = p.Club.Name,
                        Route = route,
                        Rank = rank,
                        IdentNum = identNum
                    };
                }
            }
        }

        public IEnumerable<GymnasticDto> GetPlayersRegistrationsWithInstruments(int? clubId, int leagueId, int seasonId, int? competitionRouteId = null, int? teamId = null)
        {
            List<CompetitionRegistration> playersRegistrations;
            if (clubId.HasValue)
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                !r.IsRegisteredByExcel &&
                                r.ClubId == clubId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                r.InstrumentId.HasValue &&
                                !r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();
            }
            else if (competitionRouteId.HasValue)
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                !r.IsRegisteredByExcel &&
                                r.CompetitionRouteId == competitionRouteId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                r.InstrumentId.HasValue &&
                                !r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();
            }
            else
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                !r.IsRegisteredByExcel &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                r.InstrumentId.HasValue &&
                                !r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();

            }

            if (playersRegistrations.Any())
            {
                var playersIds = playersRegistrations.Select(r => r.UserId).Distinct();
                foreach (var playerId in playersIds)
                {
                    var playerRoutesIds = playersRegistrations.Select(c => c.CompetitionRouteId).Distinct();
                    foreach (var playerRouteId in playerRoutesIds)
                    {
                        var gymnastic = playersRegistrations.FirstOrDefault(c => c.UserId == playerId && c.CompetitionRouteId == playerRouteId);
                        var playersRegistraions = playersRegistrations.Where(c => c.UserId == playerId && c.CompetitionRouteId == playerRouteId);
                        var instrumentOrder = GenerateInstrumentOrder(playersRegistraions);

                        if (gymnastic == null)
                        {
                            continue;
                        }

                        yield return new GymnasticDto
                        {
                            RegistrationId = gymnastic.Id,
                            ClubName = gymnastic.Club?.Name,
                            FullName = gymnastic.User?.FullName,
                            BirthDate = gymnastic.User?.BirthDay,
                            IdentNum = gymnastic.User?.IdentNum,
                            PassportNum = gymnastic?.User?.PassportNum,
                            Route = gymnastic.CompetitionRoute?.DisciplineRoute?.Route,
                            Rank = gymnastic.CompetitionRoute?.RouteRank?.Rank,
                            Composition = gymnastic.CompetitionRoute?.Composition,
                            PositionOrder = gymnastic.CompetitionRoute?.Composition != null ? null : gymnastic.OrderNumber,
                            ClubNumber = gymnastic.Club?.ClubNumber,
                            FinalScore = playersRegistraions?.FirstOrDefault(r => r.FinalScore.HasValue)?.FinalScore,
                            Position = playersRegistraions?.FirstOrDefault(r => r.Position.HasValue)?.Position,
                            SecondComposition = gymnastic.CompetitionRoute?.SecondComposition,
                            ThirdComposition = gymnastic.CompetitionRoute?.ThirdComposition,
                            FourthComposition = gymnastic.CompetitionRoute?.FourthComposition,
                            FifthComposition = gymnastic.CompetitionRoute?.FifthComposition,
                            SixthComposition = gymnastic.CompetitionRoute?.SixthComposition,
                            SeventhComposition = gymnastic.CompetitionRoute?.SeventhComposition,
                            EighthComposition = gymnastic.CompetitionRoute?.EighthComposition,
                            NinthComposition = gymnastic.CompetitionRoute?.NinthComposition,
                            TenthComposition = gymnastic.CompetitionRoute?.TenthComposition,
                            CompetitionRouteId = gymnastic.CompetitionRouteId,
                            CompositionNumber = gymnastic.CompositionNumber ?? 0,
                            RegistrationInstruments = instrumentOrder,
                            IsTeam = false,
                            Created = gymnastic.Created,
                            Creator = gymnastic.Creator,
                            IsReligious = gymnastic?.User?.IsReligious
                        };
                    }
                }
            }
        }

        public IEnumerable<GymnasticDto> GetTeamPlayersRegistrationsWithInstruments(int? clubId, int leagueId, int seasonId, int? competitionRouteId = null, int? teamId = null)
        {
            List<CompetitionRegistration> playersRegistrations;
            if (clubId.HasValue)
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                !r.IsRegisteredByExcel &&
                                r.ClubId == clubId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                r.InstrumentId.HasValue &&
                                r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();
            }
            else if (competitionRouteId.HasValue)
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                !r.IsRegisteredByExcel &&
                                r.CompetitionRouteId == competitionRouteId &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                r.InstrumentId.HasValue &&
                                r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();
            }
            else
            {
                playersRegistrations = db.CompetitionRegistrations
                    .Where(r => r.IsActive &&
                                !r.IsRegisteredByExcel &&
                                r.LeagueId == leagueId &&
                                r.SeasonId == seasonId &&
                                r.InstrumentId.HasValue &&
                                r.IsTeam)
                    .ToList()
                    .OrderBy(r => r.User.FullName)
                    .ToList();
            }

            if (playersRegistrations.Any())
            {
                var playersIds = playersRegistrations.Select(r => r.UserId).Distinct();
                foreach (var playerId in playersIds)
                {
                    var playerRoutesIds = playersRegistrations.Select(c => c.CompetitionRouteId).Distinct();
                    foreach (var playerRouteId in playerRoutesIds)
                    {
                        var gymnastic = playersRegistrations.FirstOrDefault(c => c.UserId == playerId && c.CompetitionRouteId == playerRouteId);
                        var playersRegistraions = playersRegistrations.Where(c => c.UserId == playerId && c.CompetitionRouteId == playerRouteId);
                        var instrumentOrder = GenerateInstrumentOrder(playersRegistraions);

                        if (gymnastic == null)
                        {
                            continue;
                        }

                        yield return new GymnasticDto
                        {
                            RegistrationId = gymnastic.Id,
                            ClubName = gymnastic.Club?.Name,
                            FullName = gymnastic.User?.FullName,
                            BirthDate = gymnastic.User?.BirthDay,
                            IdentNum = gymnastic.User?.IdentNum,
                            PassportNum = gymnastic?.User?.PassportNum,
                            Route = gymnastic.CompetitionTeamRoute?.DisciplineTeamRoute?.Route,
                            Rank = gymnastic.CompetitionTeamRoute?.RouteTeamRank?.Rank,
                            Composition = gymnastic.CompetitionTeamRoute?.Composition,
                            PositionOrder = gymnastic.CompetitionTeamRoute?.Composition != null ? null : gymnastic.OrderNumber,
                            ClubNumber = gymnastic.Club?.ClubNumber,
                            FinalScore = playersRegistraions?.FirstOrDefault(r => r.FinalScore.HasValue)?.FinalScore,
                            Position = playersRegistraions?.FirstOrDefault(r => r.Position.HasValue)?.Position,
                            SecondComposition = gymnastic.CompetitionTeamRoute?.SecondComposition,
                            ThirdComposition = gymnastic.CompetitionTeamRoute?.ThirdComposition,
                            FourthComposition = gymnastic.CompetitionTeamRoute?.FourthComposition,
                            FifthComposition = gymnastic.CompetitionTeamRoute?.FifthComposition,
                            SixthComposition = gymnastic.CompetitionTeamRoute?.SixthComposition,
                            SeventhComposition = gymnastic.CompetitionTeamRoute?.SeventhComposition,
                            EighthComposition = gymnastic.CompetitionTeamRoute?.EighthComposition,
                            NinthComposition = gymnastic.CompetitionTeamRoute?.NinthComposition,
                            TenthComposition = gymnastic.CompetitionTeamRoute?.TenthComposition,
                            CompetitionRouteId = gymnastic.CompetitionRouteId,
                            CompositionNumber = gymnastic.CompositionNumber ?? 0,
                            RegistrationInstruments = instrumentOrder,
                            IsTeam = true,
                            Created = gymnastic.Created,
                            Creator = gymnastic.Creator,
                            IsReligious = gymnastic?.User?.IsReligious
                        };
                    }
                }
            }
        }

        private List<RegistrationInstrument> GenerateInstrumentOrder(IEnumerable<CompetitionRegistration> playersRegistraions)
        {
            var result = new List<RegistrationInstrument>();
            if (playersRegistraions.Any())
            {
                var regs = playersRegistraions.OrderBy(c => c.InstrumentId);
                foreach (var reg in regs)
                {
                    result.Add(new RegistrationInstrument
                    {
                        InstrumentId = reg.InstrumentId.Value,
                        Name = reg.Instrument.Name,
                        OrderNumber = reg.OrderNumber
                    });
                }
            }
            return result;
        }

        public string GetInstrumentsNames(string instrumentIds)
        {
            var result = string.Empty;
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

        public IEnumerable<GymnasticDto> GetPlayersRegistrationsByRoute(int clubId, int leagueId, int seasonId, int competitionRouteId, int? instrumentId)
        {
            var playersRegistrations = db.CompetitionRegistrations.Where(r => r.IsActive && !r.IsRegisteredByExcel && r.CompetitionRouteId == competitionRouteId && r.LeagueId == leagueId && r.SeasonId == seasonId && r.ClubId == clubId && r.InstrumentId == instrumentId)?
                    .OrderBy(r => r.OrderNumber).ThenBy(r => r.User.FullName).AsEnumerable();
            var additionPlayersRegistrations = db.AdditionalGymnastics.Where(r => r.CompetitionRouteId == competitionRouteId && r.LeagueId == leagueId && r.SeasonId == seasonId && r.ClubId == clubId)?
                    .OrderBy(r => r.User.FullName).AsEnumerable();

            if (playersRegistrations.Any())
            {
                foreach (var gymnastic in playersRegistrations)
                {
                    yield return new GymnasticDto
                    {
                        RegistrationId = gymnastic.Id,
                        ClubName = gymnastic.Club?.Name,
                        FullName = gymnastic?.User?.FullName,
                        BirthDate = gymnastic?.User?.BirthDay,
                        IdentNum = gymnastic?.User?.IdentNum,
                        PassportNum = gymnastic?.User?.PassportNum,
                        Route = gymnastic?.CompetitionRoute?.DisciplineRoute?.Route,
                        Rank = gymnastic?.CompetitionRoute?.RouteRank?.Rank,
                        Composition = gymnastic?.CompetitionRoute?.Composition,
                        InstrumentName = GetInstrumentsNames(gymnastic?.CompetitionRoute?.InstrumentIds) ?? string.Empty,
                        PositionOrder = gymnastic.OrderNumber,
                        ClubNumber = gymnastic?.Club?.ClubNumber,
                        FinalScore = gymnastic.FinalScore,
                        Position = gymnastic.Position,
                        IsTeam = gymnastic.IsTeam,
                        CompositionNumber = gymnastic.CompositionNumber ?? 0,
                        IsReligious = gymnastic?.User?.IsReligious
                    };
                }
            }
            if (additionPlayersRegistrations.Any())
            {
                foreach (var gymnastic in additionPlayersRegistrations)
                {
                    yield return new GymnasticDto
                    {
                        RegistrationId = gymnastic.Id,
                        ClubName = gymnastic.Club?.Name,
                        FullName = gymnastic?.User?.FullName,
                        BirthDate = gymnastic?.User?.BirthDay,
                        IdentNum = gymnastic?.User?.IdentNum,
                        PassportNum = gymnastic?.User?.PassportNum,
                        Route = gymnastic?.CompetitionRoute?.DisciplineRoute?.Route,
                        Rank = gymnastic?.CompetitionRoute?.RouteRank?.Rank,
                        Composition = gymnastic?.CompetitionRoute?.Composition,
                        InstrumentName = GetInstrumentsNames(gymnastic?.CompetitionRoute?.InstrumentIds) ?? string.Empty,
                        ClubNumber = gymnastic?.Club?.ClubNumber,
                        CompositionNumber = gymnastic.CompositionNumber,
                        IsAdditional = true,
                        IsTeam = false,
                        IsReligious = gymnastic?.User?.IsReligious
                    };
                }
            }
        }

        public void ChangeRegistrationStatus(int id, bool isApproved)
        {
            var registration = db.SportsRegistrations.FirstOrDefault(r => r.Id == id);
            if (registration != null)
            {
                registration.IsApproved = isApproved;
                db.SaveChanges();
            }
        }

        public void ChangeRegistrationStatusForAllPlayers(int leagueId, int seasonId, bool isApproved)
        {
            var registrations = db.SportsRegistrations.Where(c => c.LeagueId == leagueId && c.SeasonId == seasonId);
            foreach (var registration in registrations)
            {
                registration.IsApproved = isApproved;
            }
            db.SaveChanges();
        }


        public void ChangeToChargeForAllPlayers(int leagueId, int seasonId, int? disciplineId, bool isCharged)
        {
            var registrations = db.CompetitionDisciplineRegistrations.Where(c => c.CompetitionDiscipline.CompetitionId == leagueId && c.CompetitionDiscipline.League.SeasonId == seasonId && (!disciplineId.HasValue || c.CompetitionDisciplineId == disciplineId));
            foreach (var registration in registrations)
            {
                registration.IsCharged = isCharged;
            }
            db.SaveChanges();
        }

        public void ChangeToApproveForAllPlayers(int leagueId, int seasonId, int? disciplineId, bool isApproved)
        {
            var registrations = db.CompetitionDisciplineRegistrations.Where(c => c.CompetitionDiscipline.CompetitionId == leagueId && c.CompetitionDiscipline.League.SeasonId == seasonId && (!disciplineId.HasValue || c.CompetitionDisciplineId == disciplineId));
            foreach (var registration in registrations)
            {
                registration.IsApproved = isApproved;
            }
            db.SaveChanges();
        }

        private decimal GetClubCompetitionUpdatedBalance(League league, int ClubId)
        {
            var price = league.LeaguesPrices.FirstOrDefault(lp => lp.PriceType == 2);
            if (price != null)
            {
                int count = db.CompetitionDisciplineRegistrations.Where(cdp => cdp.IsCharged != null && cdp.IsCharged == true && cdp.ClubId == ClubId && cdp.CompetitionDiscipline.League.LeagueId == league.LeagueId).Count();
                return count * price.Price.Value;
            }
            return 0;
        }



        public void UpdateAllClubsBalance(int LeagueId, int AdminId = 1)
        {
            var clubBalances = db.ClubBalances.Where(cb => cb.LeagueId == LeagueId).ToList();
            clubBalances.ForEach(cb => UpdateClubsBalance(cb.ClubId, LeagueId, AdminId));
            db.SaveChanges();
        }


        public void UpdateClubsBalance(int ClubId, int LeagueId, int userId = 1)
        {
            var clubBalance = db.ClubBalances.FirstOrDefault(cb => cb.ClubId == ClubId && cb.LeagueId != null && cb.LeagueId == LeagueId);
            var league = db.Leagues.FirstOrDefault(l => l.LeagueId == LeagueId);
            var expenses = GetClubCompetitionUpdatedBalance(league, ClubId);
            if (clubBalance == null)
            {
                db.ClubBalances.Add(new ClubBalance()
                {
                    ClubId = ClubId,
                    LeagueId = LeagueId,
                    Income = 0,
                    Expense = expenses,
                    ActionUserId = userId,
                    SeasonId = league.SeasonId,
                    Comment = league.Name
                });
            }
            else
            {
                clubBalance.Expense = expenses;
            }
        }

        public void ChargeRegistrationStatus(int id, int leagueId, int userId, bool isCharged)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(r => r.Id == id);
            if (registration != null)
            {
                registration.IsCharged = isCharged;
                db.SaveChanges();
                UpdateClubsBalance(registration.ClubId.Value, leagueId, userId);
                db.SaveChanges();
            }
        }

        public void ApproveRegistrationStatus(int id, int leagueId, int userId, bool isApproved)
        {
            var registration = db.CompetitionDisciplineRegistrations.FirstOrDefault(r => r.Id == id);
            if (registration != null)
            {
                registration.IsApproved = isApproved;
                if (isApproved)
                {
                    registration.IsCharged = isApproved;
                    db.SaveChanges();
                    UpdateClubsBalance(registration.ClubId.Value, leagueId, userId);
                }
                db.SaveChanges();
            }
        }

        public void DeleteSportsmenRegistration(int id)
        {
            var registration = db.SportsRegistrations.FirstOrDefault(r => r.Id == id);
            if (registration != null)
            {
                db.SportsRegistrations.Remove(registration);
                db.SaveChanges();
            }
        }

        public IEnumerable<PlayerRegistrationDto> GetAllRegisteredPlayers(int leagueId, int seasonId)
        {
            var registrations = db.SportsRegistrations.Where(c => c.LeagueId == leagueId && c.SeasonId == seasonId);
            if (registrations.Any())
            {
                foreach (var registration in registrations)
                {
                    yield return new PlayerRegistrationDto
                    {
                        RegistrationId = registration.Id,
                        UserId = registration.UserId,
                        ClubNumber = registration.Club.ClubNumber,
                        ClubName = registration.Club.Name,
                        FullName = registration.User.FullName,
                        FinalScore = registration.FinalScore,
                        IdentNum = registration.User.IdentNum,
                        Rank = registration.Position,
                        Birthday = registration.User.BirthDay,
                        IsApproved = registration.IsApproved || (registration.Position.HasValue || registration.FinalScore.HasValue)
                    };
                }
            }
        }

        public void DeleteRegistration(int id)
        {

            var registration = db.CompetitionRegistrations.FirstOrDefault(c => c.Id == id);
            if (registration != null)
            {
                db.CompetitionRegistrations.Remove(registration);
                db.SaveChanges();
            }
        }
        public void DeleteAdditionalRegistration(int id)
        {

            var registration = db.AdditionalGymnastics.FirstOrDefault(c => c.Id == id);
            if (registration != null)
            {
                db.AdditionalGymnastics.Remove(registration);
                db.SaveChanges();
            }
        }
        public void DeleteAdditionalTeamRegistration(int id)
        {

            var registration = db.AdditionalTeamGymnastics.FirstOrDefault(c => c.Id == id);
            if (registration != null)
            {
                db.AdditionalTeamGymnastics.Remove(registration);
                db.SaveChanges();
            }
        }
        public List<GymnasticTotoValue> GetPlayersForTotoReport(int unionId, int seasonId, int? clubId)
        {
            var sectionAlias = db.Unions.Find(unionId)?.Section?.Alias;

            var allGymnastics = db.TeamsPlayers
                .Where(x =>
                    !x.User.IsArchive &&
                    x.IsActive &&
                    x.IsApprovedByManager == true &&
                    x.SeasonId == seasonId &&
                    (!clubId.HasValue || clubId == x.ClubId) &&
                    (x.Club.UnionId == unionId || x.League.Club.UnionId == unionId || x.League.UnionId == unionId))
                ?.ToList();

            var gymnastics =
                string.Equals(sectionAlias, GamesAlias.Gymnastic, StringComparison.CurrentCultureIgnoreCase)
                    ? allGymnastics.GroupBy(x => x.UserId)
                    : allGymnastics.GroupBy(x => x.Id);

            var clubs = db.ClubTeams
                .Include(x => x.Club)
                .Include(x => x.Club.Section)
                .Include(x => x.Club.Union)
                .Include(x => x.Club.Union.Section)
                .Where(x => x.SeasonId == seasonId && !x.Club.IsUnionArchive)
                .AsNoTracking()
                .ToList();

            return GetGymnasticTotoValue(gymnastics, seasonId, clubs, sectionAlias);
        }


        private void AddLeagueCycleNum(Dictionary<int, List<TotoCompetitionUsers>> list, int leagueId, int groupId, int cycleNum) {
            var isHasValue2 = list.TryGetValue(leagueId, out List<TotoCompetitionUsers> leagueCycleNums);
            if (isHasValue2)
            {
                var isContained = leagueCycleNums.Any(r => r.GroupId == groupId && r.CycleNum == cycleNum);
                if (!isContained)
                {
                    list[leagueId].Add(new TotoCompetitionUsers { CycleNum = cycleNum, GroupId = groupId, Id = leagueId});
                }
            }
            else
            {
                var newList = new List<TotoCompetitionUsers> { new TotoCompetitionUsers { CycleNum = cycleNum, GroupId = groupId, Id = leagueId } };
                list.Add(leagueId, newList);
            }
        }

        public List<GymnasticTotoValue> GetPlayersForTennisTotoReport(List<TotoCompetition> competitions, int seasonId, int? clubId, int? totoReportMaxBirthYear, out Dictionary<int, List<TotoCompetitionUsers>> leagueCycleNums)
        {
            var competitionIds = competitions.Where(l => l.Id.HasValue).Select(l => l.Id.Value).ToArray();
            var teamLeagues = new Dictionary<int, int>();
            leagueCycleNums = new Dictionary<int, List<TotoCompetitionUsers>>();
            var teamIds = new List<int>();
            foreach (var competition in competitions)
            {
                if (competition.IsCompetitionNotLeague)
                {
                    if (!competition.IsDailyCompetition)
                    {
                        teamIds.AddRange(competition.CategoryIds);
                    }
                    foreach (var categoryId in competition.CategoryIds)
                    {
                        teamLeagues[categoryId] = competition.Id.Value;
                    }
                }
            }

            List<GymnasticTotoValue> res = new List<GymnasticTotoValue>();
            var tennisSectionUnions = db.Unions
                            .Where(u => u.Section.Alias == GamesAlias.Tennis)
                            .Include(x => x.Leagues)
                            .Include(x => x.Leagues.Select(l => l.LeagueTeams))
                            .ToList();
            var tennisUnionsLeagues = tennisSectionUnions
                .SelectMany(x => x.Leagues.Where(l => !l.IsArchive))
                .ToList();
            var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
            var tennisGames = db.TennisGameCycles
                .Include(x => x.TennisStage)
                .Include("TeamsPlayer")
                .Include("TeamsPlayer1")
                .Include("TeamsPlayer11")
                .Include("TeamsPlayer3")
                .Where(t => t.GameStatus == GameStatus.Ended && teamIds.Contains(t.TennisStage.CategoryId))
                .ToList();
            var tennisLeaguesIds = tennisUnionsLeagues.Select(x => x.LeagueId).ToArray();
            var regularGames = db.GamesCycles
                .Include(x => x.TennisLeagueGames)
                .Include(x => x.Stage)
                .Where(g => tennisLeaguesIds.Contains(g.Stage.LeagueId) && g.GameStatus == GameStatus.Ended && competitionIds.Contains(g.Stage.LeagueId))
                .ToList();

            var dailyCompetitions = competitions.Where(c => c.IsDailyCompetition).ToList();

            var playersCompetitionCount = new Dictionary<int, int>();
            var playersCompetitionParticipation = new Dictionary<int, Dictionary<int, MultiKeyDictionary<int,int, int>>>();

            foreach (var game in regularGames)
            {
                var playersParticipatedInThisGame = new List<int>();
                foreach (var gameInCycle in game.TennisLeagueGames)
                {
                    if (gameInCycle.HomePlayerId.HasValue && !playersParticipatedInThisGame.Contains(gameInCycle.HomePlayerId.Value))
                        playersParticipatedInThisGame.Add(gameInCycle.HomePlayerId.Value);
                    if (gameInCycle.HomePairPlayerId.HasValue && !playersParticipatedInThisGame.Contains(gameInCycle.HomePairPlayerId.Value))
                        playersParticipatedInThisGame.Add(gameInCycle.HomePairPlayerId.Value);
                    if (gameInCycle.GuestPlayerId.HasValue && !playersParticipatedInThisGame.Contains(gameInCycle.GuestPlayerId.Value))
                        playersParticipatedInThisGame.Add(gameInCycle.GuestPlayerId.Value);
                    if (gameInCycle.GuestPairPlayerId.HasValue && !playersParticipatedInThisGame.Contains(gameInCycle.GuestPairPlayerId.Value))
                        playersParticipatedInThisGame.Add(gameInCycle.GuestPairPlayerId.Value);
                }
                foreach (var userId in playersParticipatedInThisGame)
                {
                    Dictionary<int, MultiKeyDictionary<int, int, int>> value;
                    var isHasValue = playersCompetitionParticipation.TryGetValue(userId, out value);
                    var cycleNum = game.CycleNum;
                    var groupId = game.StageId;
                    AddLeagueCycleNum(leagueCycleNums, game.Stage.LeagueId, groupId, cycleNum);
                    if (isHasValue)
                    {
                        var isHasValue2 = playersCompetitionParticipation[userId].TryGetValue(game.Stage.LeagueId, out MultiKeyDictionary<int, int, int> value2);
                        if (isHasValue2)
                        {
                            var isHasValue3 = value2.ContainsKey(groupId, cycleNum);
                            if (isHasValue3)
                            {
                                playersCompetitionParticipation[userId][game.Stage.LeagueId][groupId][cycleNum]++;
                            }
                            else
                            {
                                playersCompetitionParticipation[userId][game.Stage.LeagueId].Add(groupId,cycleNum, 1);
                            }
                        }
                        else
                        {
                            var newDic = new MultiKeyDictionary<int, int, int>();
                            newDic.Add(groupId, cycleNum, 1);
                            playersCompetitionParticipation[userId].Add(game.Stage.LeagueId, newDic);
                        }
                    }
                    else
                    {
                        var newDic = new MultiKeyDictionary<int, int, int>();
                        newDic.Add(groupId, cycleNum, 1);
                        playersCompetitionParticipation[userId] = new Dictionary<int, MultiKeyDictionary<int,int, int>>
                        {
                            [game.Stage.LeagueId] = newDic
                        };
                    }
                }
            }

            
            foreach (var game in tennisGames)
            {
                if (game.GameStatus == GameStatus.Ended && ((game.TennisStage.SeasonId == season.Id && game.StartDate < new DateTime(game.StartDate.Year, 9, 1)) || (season.PreviousSeasonId.HasValue && game.TennisStage.SeasonId == season.PreviousSeasonId && game.StartDate >= new DateTime(game.StartDate.Year - 1, 9, 1))))
                {
                    var playersParticipatedInThisGame = new List<int>();
                    if (game.FirstPlayer?.UserId != null && !playersParticipatedInThisGame.Contains(game.FirstPlayer.UserId))
                        playersParticipatedInThisGame.Add(game.FirstPlayer.UserId);
                    if (game.SecondPlayer?.UserId != null && !playersParticipatedInThisGame.Contains(game.SecondPlayer.UserId))
                        playersParticipatedInThisGame.Add(game.SecondPlayer.UserId);
                    if (game.FirstPairPlayer?.UserId != null && !playersParticipatedInThisGame.Contains(game.FirstPairPlayer.UserId))
                        playersParticipatedInThisGame.Add(game.FirstPairPlayer.UserId);
                    if (game.SecondPairPlayer?.UserId != null && !playersParticipatedInThisGame.Contains(game.SecondPairPlayer.UserId))
                        playersParticipatedInThisGame.Add(game.SecondPairPlayer.UserId);
                    foreach (var userId in playersParticipatedInThisGame)
                    {
                        Dictionary<int, MultiKeyDictionary<int, int, int>> value;
                        var cycleNum = 0;
                        var groupId = 0;
                        AddLeagueCycleNum(leagueCycleNums, teamLeagues[game.TennisStage.CategoryId], groupId, cycleNum);
                        var isHasValue = playersCompetitionParticipation.TryGetValue(userId, out value);
                        if (isHasValue)
                        {
                            var isHasValue2 = playersCompetitionParticipation[userId].TryGetValue(teamLeagues[game.TennisStage.CategoryId], out MultiKeyDictionary<int, int, int> value2);
                            if (isHasValue2)
                            {
                                playersCompetitionParticipation[userId][teamLeagues[game.TennisStage.CategoryId]].Add(groupId, cycleNum, 1);
                            }
                            else
                            {
                                var newDic = new MultiKeyDictionary<int, int, int>();
                                newDic.Add(groupId, cycleNum, 1);
                                playersCompetitionParticipation[userId].Add(teamLeagues[game.TennisStage.CategoryId], newDic);
                            }
                        }
                        else
                        {
                            var newDic = new MultiKeyDictionary<int, int, int>();
                            newDic.Add(groupId, cycleNum, 1);
                            playersCompetitionParticipation[userId] = new Dictionary<int, MultiKeyDictionary<int, int, int>>
                            {
                                [teamLeagues[game.TennisStage.CategoryId]] = newDic
                            };
                        }
                    }
                }
            }

            var leagueIds = dailyCompetitions.Select(c => c.Id.Value).ToArray();
            var leagues = db.Leagues.Where(l => leagueIds.Contains(l.LeagueId)).ToList();
            foreach (var competition in dailyCompetitions)
            {
                foreach (var categoryId in competition.CategoryIds)
                {
                    var cycleNum = -1;
                    var groupId = 0;
                    var league = leagues.FirstOrDefault(l => l.LeagueId == competition.Id.Value);
                    var userIdsInTeam = GetTeamPlayersForCompetitionCount(categoryId, season, league, clubId ?? 0);
                    AddLeagueCycleNum(leagueCycleNums, competition.Id.Value, groupId, cycleNum);
                    foreach (var userId in userIdsInTeam)
                    {
                        Dictionary<int, MultiKeyDictionary<int, int, int>> value;
                        var isHasValue = playersCompetitionParticipation.TryGetValue(userId, out value);
                        if (isHasValue)
                        {
                            var isHasValue2 = playersCompetitionParticipation[userId].TryGetValue(competition.Id.Value, out MultiKeyDictionary<int, int, int> value2);
                            if (isHasValue2)
                            {
                                playersCompetitionParticipation[userId][competition.Id.Value].Add(groupId, cycleNum, 1);
                            }
                            else
                            {
                                var newDic = new MultiKeyDictionary<int, int, int>();
                                newDic.Add(groupId, cycleNum, 1);
                                playersCompetitionParticipation[userId].Add(competition.Id.Value, newDic);
                            }
                        }
                        else
                        {
                            var newDic = new MultiKeyDictionary<int, int, int>();
                            newDic.Add(groupId, cycleNum, 1);
                            playersCompetitionParticipation[userId] = new Dictionary<int, MultiKeyDictionary<int, int, int>>
                            {
                                [competition.Id.Value] = newDic
                            };
                        }
                    }
                }
            }
            

            foreach (var userId in playersCompetitionParticipation.Keys)
            {
                var dict = playersCompetitionParticipation[userId];
                var competitionsRegisteredTo = 0;
                foreach (var league in dict.Values)
                {
                    competitionsRegisteredTo += league.Values.Sum();
                }
 
                var isHasValue = playersCompetitionCount.TryGetValue(userId, out int value);
                if (isHasValue)
                {
                    playersCompetitionCount[userId] += competitionsRegisteredTo;
                }
                else
                {
                    playersCompetitionCount[userId] = competitionsRegisteredTo;
                }
            }

            var userIdsInvolved = playersCompetitionCount.Select(x => x.Key).ToArray();
            var teamPlayersGroupedByUser = db.TeamsPlayers.Include("User").Include("User.MedicalCertApprovements").Include("Team").Include("Team.ClubTeams").Where(tp => userIdsInvolved.Contains(tp.UserId) && (!totoReportMaxBirthYear.HasValue || (tp.User.BirthDay.HasValue && tp.User.BirthDay.Value.Year <= totoReportMaxBirthYear.Value)) && !tp.User.IsArchive &&
                    tp.IsActive &&
                    tp.SeasonId == seasonId &&
                    (!clubId.HasValue || clubId == tp.ClubId)).GroupBy(tp => tp.UserId).ToList();

            var clubs = db.ClubTeams
                .Include(x => x.Club)
                .Include(x => x.Club.Section)
                .Include(x => x.Club.Union)
                .Include(x => x.Club.Union.Section)
                .Where(x => x.SeasonId == seasonId)
                .AsNoTracking()
                .ToList();

            var listOfUsers = new List<GymnasticTotoValue>();

            foreach (var groupedSportsman in teamPlayersGroupedByUser)
            {

                TeamsPlayer sportsman = groupedSportsman.FirstOrDefault(tp => tp.Team.ClubTeams.FirstOrDefault(ct => ct.SeasonId == seasonId && ct.IsTrainingTeam) != null);
                string teamName = string.Empty;
                if (sportsman == null)
                {
                    sportsman = groupedSportsman.FirstOrDefault();
                }
                else
                {
                    teamName = sportsman.Team.TeamsDetails.FirstOrDefault(td => td.SeasonId == seasonId)?.TeamName ?? sportsman.Team.Title;
                }
                var club = clubs.FirstOrDefault(t => t.TeamId == sportsman?.TeamId)?.Club;
                var comeptitions = new List<TotoCompetitionUsers>();
                foreach (var key in playersCompetitionParticipation[sportsman.UserId].Keys)
                {
                    foreach (var item in playersCompetitionParticipation[sportsman.UserId][key])
                    {
                        foreach (var group in item.Value)
                        {
                            var participationCount = group.Value;
                            comeptitions.Add(new TotoCompetitionUsers { Id = key, CycleNum = group.Key, GroupId = item.Key, count = participationCount });
                        }
                    }
                }

                var teamNameForNonTrainingId = groupedSportsman.FirstOrDefault(tp => tp.Team.ClubTeams.FirstOrDefault(ct => ct.SeasonId == seasonId && !ct.IsTrainingTeam) != null && tp.SeasonId == seasonId && tp.ClubId == clubId)?.TeamId;
                var teamNameForNonTraining = string.Empty;

                if (teamNameForNonTrainingId > 0)
                {
                    teamNameForNonTraining = sportsman.League.LeagueTeams.FirstOrDefault(x => x.TeamId == teamNameForNonTrainingId)?.Teams?.Title;
                }

                listOfUsers.Add(new GymnasticTotoValue
                {
                    UsersInformation = new UsersInformation
                    {
                        FirstName = sportsman?.User?.FirstName ?? GetFirstNameByFullName(sportsman?.User?.FullName),
                        LastName = sportsman?.User?.LastName ?? GetLastNameByFullName(sportsman?.User?.FullName),
                        FullName = sportsman?.User?.FullName,
                        BirthDate = sportsman?.User?.BirthDay,
                        IdentNum = sportsman?.User?.IdentNum,
                        PassportNum = sportsman?.User?.PassportNum,
                        Insurance = !string.IsNullOrEmpty(sportsman?.User?.MedicalCertificateFile),
                        MedicalCertificate = sportsman?.User.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == seasonId)?.Approved == true,
                        Category = GetUsersCategory(sportsman?.User?.BirthDay),
                        GenderId = sportsman?.User?.GenderId
                    },
                    MainInformation = new MainInformation
                    {
                        ClubName = (!clubs.Any(x => x.IsTrainingTeam)) ? teamNameForNonTraining : (club?.Name ?? sportsman?.League?.Club?.Name ?? string.Empty),
                        ClubNumber = club?.NGO_Number ?? sportsman?.League?.Club?.ClubNumber.ToString(),
                        SportCenterNameHeb = club?.SportCenter?.Heb ?? sportsman?.League?.Club?.SportCenter?.Heb,
                        SportCenterNameEng = club?.SportCenter?.Eng ?? sportsman?.League?.Club?.SportCenter?.Eng,
                        ClubDisciplineName = string.Empty,
                        TeamName = (!clubs.Any(x => x.IsTrainingTeam)) ? teamNameForNonTraining : (club?.Name ?? sportsman?.League?.Club?.Name ?? string.Empty)
                    },
                    Competitions = comeptitions,
                    CompetitionParticipationCount = playersCompetitionCount[sportsman.UserId]
                });
            }
            return listOfUsers;
        }

        private List<GymnasticTotoValue> GetGymnasticTotoValue(IEnumerable<IGrouping<int, TeamsPlayer>> unionUsers, int seasonId,
            List<ClubTeam> clubs, string sectionAlias)
        {
            var listOfUsers = new List<GymnasticTotoValue>();
            int AlternativeResultInt = 3; // value for alternative result column if player did not start/show
            foreach (var groupedSportsman in unionUsers)
            {
                var sportsman = groupedSportsman.FirstOrDefault();
                var club = clubs.FirstOrDefault(t => t.TeamId == sportsman?.TeamId)?.Club;
                listOfUsers.Add(new GymnasticTotoValue
                {
                    UsersInformation = new UsersInformation
                    {
                        FirstName = sportsman?.User?.FirstName ?? GetFirstNameByFullName(sportsman?.User?.FullName),
                        LastName = sportsman?.User?.LastName ?? GetLastNameByFullName(sportsman?.User?.FullName),
                        FullName = sportsman?.User?.FullName,
                        BirthDate = sportsman?.User?.BirthDay,
                        IdentNum = sportsman?.User?.IdentNum,
                        PassportNum = sportsman?.User?.PassportNum,
                        Insurance = !string.IsNullOrEmpty(sportsman?.User?.MedicalCertificateFile), /*!string.IsNullOrEmpty(gymnastic?.User?.InsuranceFile),*/
                        MedicalCertificate = !string.IsNullOrEmpty(sportsman?.User?.MedicalCertificateFile),
                        Category = GetUsersCategory(sportsman?.User?.BirthDay),
                        GenderId = sportsman?.User?.GenderId,
                        UserId = sportsman?.UserId ?? 0
                    },
                    MainInformation = new MainInformation
                    {
                        ClubName = club?.Name ?? sportsman?.League?.Club?.Name ?? string.Empty,
                        ClubNumber = club?.NGO_Number ?? sportsman?.League?.Club?.ClubNumber.ToString(),
                        SportCenterNameHeb = club?.SportCenter?.Heb ?? sportsman?.League?.Club?.SportCenter?.Heb,
                        SportCenterNameEng = club?.SportCenter?.Eng ?? sportsman?.League?.Club?.SportCenter?.Eng,
                        ClubDisciplineName = sectionAlias.Equals(GamesAlias.Gymnastic)
                            ? string.Join(",",
                                club?.ClubDisciplines?.Where(c => c.SeasonId == seasonId)
                                    ?.Select(d => d.Discipline.Name) ?? new List<string>())
                            : string.Empty,
                        TeamName = string.Join(",",
                            groupedSportsman.Select(x =>
                                x.Team.TeamsDetails.FirstOrDefault(td => td.SeasonId == seasonId)?.TeamName ??
                                x.Team.Title))
                    },
                    Competitions = GetSportsmen(groupedSportsman, sectionAlias),
                    WeightLiftingCompetitionsIds = (sectionAlias == GamesAlias.WeightLifting) ? sportsman?.User?.CompetitionDisciplineRegistrations
                        .Where(c => c.CompetitionDiscipline.League.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && c.IsApproved.HasValue && c.IsApproved.Value)
                        .GroupBy(r => r.CompetitionDiscipline.League.LeagueId)
                        .Select(r => r.First().CompetitionDiscipline.CompetitionId).ToList() : new List<int>(),
                    AthleticsCompetitionsIds = (sectionAlias == GamesAlias.Athletics) ? sportsman?.User?.CompetitionDisciplineRegistrations
                        .Where(c => c.CompetitionDiscipline.League.SeasonId == seasonId && c.Club.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && !c.CompetitionDiscipline.IsDeleted && c.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(c.CompetitionResult.FirstOrDefault().Result) && c.CompetitionResult.FirstOrDefault().AlternativeResult != AlternativeResultInt)
                        .GroupBy(r => r.CompetitionDiscipline.League.LeagueId)
                        .Select(r => r.First().CompetitionDiscipline.CompetitionId).ToList() : new List<int>()
                });
            }
            return listOfUsers;
        }

        private List<TotoCompetitionUsers> GetSportsmen(IGrouping<int, TeamsPlayer> groupedSportsman, string sectionAlias)
        {
            var users = new List<TotoCompetitionUsers>();

            if (sectionAlias.Equals(GamesAlias.Gymnastic))
                users = GetGymnasticsCompetitionsList(groupedSportsman);
            else if (sectionAlias.Equals(GamesAlias.Tennis))
                users = GetTennisCompetitionsList(groupedSportsman);
            else
                users = GetSportsmanCompetitionList(groupedSportsman);

            return users;
        }

        private List<TotoCompetitionUsers> GetTennisCompetitionsList(IGrouping<int, TeamsPlayer> groupedSportsman)
        {
            var competitionList = new List<TotoCompetitionUsers>();

            foreach (var sportsman in groupedSportsman)
            {
                var leagues = GetTennisLeaguesForPlayer(sportsman);
                foreach (var league in leagues)
                {
                    competitionList.Add(new TotoCompetitionUsers
                    {
                        Id = league.LeagueId,
                        //Position = GetTotoPositionValue(registration)
                    });
                }
                var excelRegs = sportsman.User.CompetitionRegistrations.Where(t => t.IsActive && t.IsRegisteredByExcel);
                foreach (var registration in excelRegs)
                {
                    competitionList.Add(new TotoCompetitionUsers
                    {
                        Id = registration.LeagueId
                    });
                }
            }

            return competitionList;
        }


        private List<League> GetTennisLeaguesForPlayer(TeamsPlayer sportsman, List<League> unionLeagues,
            List<GamesCycle> regularGames, List<TennisGameCycle> tennisGames)
        {
            var result = new List<League>();

            foreach (var league in unionLeagues)
            {
                var isLeague = league.EilatTournament == null || league.EilatTournament == false;
                if (isLeague)
                {
                    foreach (var game in regularGames)
                    {
                        if (game.HomeTeamId == sportsman.TeamId || game.GuestTeamId == sportsman.TeamId)
                        {
                            var tennisLeagueGames = game.TennisLeagueGames.Where(r => r.IsEnded);
                            foreach (var tennisGame in tennisLeagueGames)
                            {
                                if (tennisGame.HomePlayerId == sportsman.UserId ||
                                    tennisGame.GuestPlayerId == sportsman.UserId ||
                                    tennisGame.HomePairPlayerId == sportsman.UserId ||
                                    tennisGame.GuestPairPlayerId == sportsman.UserId)
                                {
                                    result.Add(league);

                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var game in tennisGames)
                    {
                        if (game.FirstPlayerId == sportsman.Id ||
                            game.SecondPlayerId == sportsman.Id ||
                            game.FirstPlayerPairId == sportsman.Id ||
                            game.SecondPlayerPairId == sportsman.Id)
                        {
                            if (league.LeagueTeams.Any(t => t.TeamId == sportsman.TeamId))
                                result.Add(league);
                        }
                    }
                }
            }

            return result;
        }


        private IEnumerable<League> GetTennisLeaguesForPlayer(TeamsPlayer sportsman)
        {
            var sectionUnions = db.Unions
                .Where(u => u.Section.Alias == GamesAlias.Tennis)
                .Include(x => x.Leagues)
                .Include(x => x.Leagues.Select(l => l.LeagueTeams))
                .ToList();

            var unionLeagues = sectionUnions
                .SelectMany(x => x.Leagues.Where(l => !l.IsArchive))
                .ToList();

            var leaguesIds = unionLeagues.Select(x => x.LeagueId).ToArray();

            var tennisGames = db.TennisGameCycles
                .Where(t => t.TennisStage.CategoryId == sportsman.TeamId &&
                            t.GameStatus == GameStatus.Ended)
                .ToList();

            var regularGames = db.GamesCycles
                .Include(x => x.TennisLeagueGames)
                .Where(g => leaguesIds.Contains(g.Stage.LeagueId))
                .ToList();

            return GetTennisLeaguesForPlayer(sportsman, unionLeagues, regularGames, tennisGames);
        }

        private List<League> GetTennisLeaguesForPlayer_Bulk(
            TeamsPlayer sportsman,
            List<League> tennisUnionLeagues,
            List<TennisGameCycle> allTennisEndedGames,
            List<GamesCycle> allRegularGames)
        {
            var tennisGames = allTennisEndedGames
                .Where(t => t.TennisStage.CategoryId == sportsman.TeamId)
                .ToList();

            return GetTennisLeaguesForPlayer(sportsman, tennisUnionLeagues, allRegularGames, tennisGames);
        }

        private List<TotoCompetitionUsers> GetSportsmanCompetitionList(IGrouping<int, TeamsPlayer> groupedSportsman)
        {
            var competitionList = new List<TotoCompetitionUsers>();

            foreach (var sporstman in groupedSportsman)
            {
                var competitionRegistrations = sporstman.User.SportsRegistrations.Where(r => r.IsApproved);
                if (competitionRegistrations.Any())
                {
                    var leagues = competitionRegistrations.Select(c => c.League);
                    foreach (var league in leagues)
                    {
                        var registration = competitionRegistrations.Where(c => c.LeagueId == league.LeagueId)
                            ?.FirstOrDefault(c => c.Position.HasValue || c.FinalScore.HasValue);

                        competitionList.Add(new TotoCompetitionUsers
                        {
                            Id = league.LeagueId,
                            Position = GetTotoPositionValue(registration)
                        });
                    }
                }
            }

            return competitionList;
        }

        private List<TotoCompetitionUsers> GetGymnasticsCompetitionsList(IGrouping<int, TeamsPlayer> gymnastics)
        {
            var competitionList = new List<TotoCompetitionUsers>();

            foreach (var gymnastic in gymnastics)
            {
                var competitionRegistrations = gymnastic.User.CompetitionRegistrations.Where(t => !t.League.IsArchive && t.IsActive);
                if (competitionRegistrations.Any())
                {
                    var leagues = competitionRegistrations.Select(c => c.League);
                    foreach (var league in leagues)
                    {
                        var registration = competitionRegistrations.Where(c => c.LeagueId == league.LeagueId)
                                                   ?.FirstOrDefault(c => c.Position.HasValue || c.FinalScore.HasValue);

                        competitionList.Add(new TotoCompetitionUsers
                        {
                            Id = league.LeagueId,
                            Position = GetTotoPositionValue(registration)
                        });
                    }
                }
            }

            return competitionList;
        }

        private int? GetTotoPositionValue(dynamic registration)
        {
            int? result = null;

            if (registration != null)
            {
                if (registration.Position != null)
                    result = registration.Position > 7 ? 7 : registration.Position;

                else if (registration.FinalScore != null && registration.Position == null)
                    result = 7;
            }

            return result;
        }

        public string GetUsersCategory(DateTime? birthDay)
        {
            return GetUsersCategory(birthDay.GetAge());
        }

        public string GetUsersCategory(int? age)
        {
            var category = string.Empty;
            if (age.HasValue)
            {
                var categories = db.Ages.Where(ages => (ages.FromAge.HasValue && ages.ToAge.HasValue)
                    && age.Value >= ages.FromAge.Value && age.Value <= ages.ToAge.Value)?.Select(ages => ages.Title)?.AsEnumerable();
                if (categories.Any())
                {
                    category = string.Join(",", categories);
                }
            }
            return category;
        }

        public void DeleteTennisRegistration(int teamPlayerId)
        {
            var teamPlayer = db.TeamsPlayers.FirstOrDefault(c => c.Id == teamPlayerId);
            if (teamPlayer != null)
            {
                teamPlayer.WithoutLeagueRegistration = true;
                db.SaveChanges();
            }
        }

        public void CheckLockStatus(IEnumerable<TeamsPlayer> teamPlayers, int? leagueId)
        {
            DateTime? minOfMinDate;
            DateTime? maxOfMaxDate;
            SetLeaguesAgeValues(leagueId, out minOfMinDate, out maxOfMaxDate);

            foreach (var player in teamPlayers)
            {
                if (player.User.BirthDay.HasValue)
                {
                    if (player.IsLocked == null)
                    {
                        if (player.User.BirthDay != null)
                        {
                            if (minOfMinDate != null && player.User.BirthDay > minOfMinDate.Value)
                            {
                                player.IsLocked = true;
                            }

                            if (maxOfMaxDate != null && player.User.BirthDay < maxOfMaxDate)
                            {
                                player.IsLocked = true;
                            }
                        }
                    }
                }
            }
        }

        private void SetLeaguesAgeValues(int? leagueId, out DateTime? minOfMinDate, out DateTime? maxOfMaxDate)
        {
            minOfMinDate = null;
            maxOfMaxDate = null;
            var league = db.Leagues.FirstOrDefault(c => c.LeagueId == leagueId);

            if (league == null) return;

            if (minOfMinDate == null && league.MinimumAge != null)
            {
                minOfMinDate = league.MinimumAge;
            }
            else if (league.MinimumAge != null)
            {
                if (league.MinimumAge < minOfMinDate)
                {
                    minOfMinDate = league.MinimumAge;
                }
            }
            if (maxOfMaxDate == null && league.MaximumAge != null)
            {
                maxOfMaxDate = league.MaximumAge;
            }
            else if (league.MaximumAge != null)
            {
                if (league.MaximumAge > maxOfMaxDate)
                {
                    maxOfMaxDate = league.MaximumAge;
                }
            }
        }


        public void UpdatePlayersOrder(int clubId, int leagueId, int seasonId, int competitionRouteId, List<GymnasticShortDto> playersList)
        {
            var positionsNow = db.CompetitionRegistrations.Where(r => r.IsActive && !r.IsRegisteredByExcel && r.ClubId == clubId && r.LeagueId == leagueId && r.SeasonId == seasonId
                && r.CompetitionRouteId == competitionRouteId);

            foreach (var regPos in positionsNow)
            {
                foreach (var position in playersList)
                {
                    if (regPos.Id == position.RegistrationId)
                    {
                        regPos.OrderNumber = position.PositionOrder;
                    }
                }
            }
            db.SaveChanges();
        }

        public void CheckForExclusions()
        {
            var isChanged = false;
            var exclusions = db.PenaltyForExclusions
                .Where(p => !p.IsCanceled && !p.IsEnded && !(p.LeagueIds == null || p.LeagueIds.Trim() == string.Empty))
                .ToList();
            if (exclusions.Any())
            {
                foreach (var penalty in exclusions)
                {
                    var gamesCount = db.GamesCycles.Count(g =>
                        (g.GuestTeam.TeamsPlayers.Select(c => c.UserId).Contains(penalty.UserId) ||
                         g.HomeTeam.TeamsPlayers.Select(c => c.UserId).Contains(penalty.UserId)) &&
                        g.GameStatus == GameStatus.Ended && !g.AppliedExclusionId.HasValue &&
                        g.StartDate >= penalty.DateOfExclusion &&
                        g.StartDate <= DateTime.Now);

                    if (gamesCount >= penalty.ExclusionNumber)
                    {
                        penalty.IsEnded = true;
                        isChanged = true;
                    }
                }
            }

            if (isChanged)
            {
                db.SaveChanges();
            }
        }

        public void UpdateClubIsPaymentDone(List<Team> teams, int clubId, int seasonId, bool isPayed)
        {
            var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId && c.SeasonId == seasonId);
            if (club != null)
            {
                club.IsClubFeesPaid = isPayed;
                if (isPayed)
                {
                    var players = GetTeamPlayersWithInvalidTeniCards(teams, clubId, seasonId).ToList();
                    var year = season.EndDate.Year;
                    var tenicardValidityUntil = new DateTime(year, 12, 31);
                    // update tenicard to all those players.
                    foreach (var player in players)
                    {
                        player.User.TenicardValidity = tenicardValidityUntil;
                    }
                }
                Save();
            }
        }

        public void AthleteNumber(AthleteNumber athleteNumber)
        {
            db.AthleteNumbers.Add(athleteNumber);
        }

        public void SetPlayerHandicapLevel(int userId, int seasonId, decimal handicapLevel)
        {
            var teamsPlayers = db.TeamsPlayers
                .Where(x => x.UserId == userId &&
                            x.SeasonId == seasonId)
                .ToList();

            foreach (var teamPlayer in teamsPlayers)
            {
                teamPlayer.HandicapLevel = handicapLevel;
            }
        }
    }
}