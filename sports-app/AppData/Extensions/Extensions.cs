using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace AppModel
{
    public partial class TeamsPlayer
    {
        public bool IsPlayereInTeamLessThan3year
        {
            get
            {
                if (StartPlaying == null) return false;
                return StartPlaying > DateTime.Now.AddYears(-3);
            }
        }
        public decimal? ClubPaymentFee { get; set; }
    }




    public partial class DisciplineRecord
    {
        public bool isCategorySelected(int id)
        {
            if (Categories == null)
            {
                return false;
            }
            var categories = Categories.Split(',').ToList();
            return categories.Contains(id.ToString());
        }

        public void AddCategoryId(int categoryId)
        {
            if (Categories == null)
            {
                Categories = categoryId.ToString();
            }
            else
            {
                var categoryList = Categories.Split(',').ToList();
                if (!categoryList.Contains(categoryId.ToString()))
                {
                    categoryList.Add(categoryId.ToString());
                }
                Categories = string.Join(",", categoryList);
            }
        }

        public void RemoveCategoryId(int categoryId)
        {
            if (Categories != null)
            {
                var categoryList = Categories.Split(',').ToList();
                if (categoryList.Contains(categoryId.ToString()))
                {
                    categoryList.Remove(categoryId.ToString());
                }
                Categories = string.Join(",", categoryList);
            }
        }

        public bool isDisciplineSelected(int id)
        {
            if (Disciplines == null)
            {
                return false;
            }
            var disciplines = Disciplines.Split(',').ToList();
            return disciplines.Contains(id.ToString());
        }

        public void AddDisciplineId(int disciplineId)
        {
            if (Disciplines == null)
            {
                Disciplines = disciplineId.ToString();
            }
            else
            {
                var disciplineList = Disciplines.Split(',').ToList();
                if (!disciplineList.Contains(disciplineId.ToString()))
                {
                    disciplineList.Add(disciplineId.ToString());
                }
                Disciplines = string.Join(",", disciplineList);
            }
        }

        public void RemoveDisciplineId(int disciplineId)
        {
            if (Disciplines != null)
            {
                var disciplineList = Disciplines.Split(',').ToList();
                if (disciplineList.Contains(disciplineId.ToString()))
                {
                    disciplineList.Remove(disciplineId.ToString());
                }
                Categories = string.Join(",", disciplineList);
            }
        }




        public string CompetitionRecord { get; set; }

    }







    public partial class PlayoffBracket
    {
        public Team FirstTeam
        {
            get
            {
                return Team1;
            }
            set
            {
                Team1 = value;
            }
        }

        public Team SecondTeam
        {
            get
            {
                return Team2;
            }
            set
            {
                Team2 = value;
            }
        }

        public Team WinnerTeam
        {
            get
            {
                return Team3;
            }
            set
            {
                Team3 = value;
            }
        }

        public Team LoserTeam
        {
            get
            {
                return Team;
            }
            set
            {
                Team = value;
            }
        }

        public PlayoffBracket Parent1
        {
            get
            {
                return PlayoffBracket1;
            }
            set
            {
                PlayoffBracket1 = value;
            }
        }

        public PlayoffBracket Parent2
        {
            get
            {
                return PlayoffBracket2;
            }
            set
            {
                PlayoffBracket2 = value;
            }
        }

        public ICollection<PlayoffBracket> ChildrenSide1
        {
            get
            {
                return PlayoffBrackets1;
            }
            set
            {
                PlayoffBrackets1 = value;
            }
        }

        public ICollection<PlayoffBracket> ChildrenSide2
        {
            get
            {
                return PlayoffBrackets11;
            }
            set
            {
                PlayoffBrackets11 = value;
            }
        }


        public IEnumerable<PlayoffBracket> AllChildren
        {
            get
            {
                return PlayoffBrackets1.Concat(PlayoffBrackets11);
            }
        }



        //public override bool Equals(object obj)
        //{
        //    var other = obj as PlayoffBracket;
        //    if (this.Id == other.Id)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        if (this.Team1Id != null && this.Team2Id != null && other.Team1Id != null && other.Team2Id != null)
        //        {
        //            return ((this.Team1Id == other.Team1Id && this.Team2Id == other.Team2Id) ||
        //            (this.Team1Id == other.Team2Id && this.Team2Id == other.Team1Id));
        //        }
        //    }
        //    return false;
        //}
    }

    public partial class Club
    {
        public Club ParentClub { get { return Club1; } set { Club1 = value; } }
        public ICollection<Club> RelatedClubs { get { return Clubs1; } set { Clubs1 = value; } }
        public Section SportSection { get { return Section1; } set { Section1 = value; } }
        public ICollection<ClubTeam> Departments { get { return ClubTeams1; } set { ClubTeams1 = value; } }
        public virtual ICollection<UsersJob> ConnectedReferees { get { return UsersJobs1; } set { UsersJobs1 = value; } }
    }

    public partial class ClubTeam
    {
        public Club Department { get { return Club1; } set { Club1 = value; } }
    }

    public partial class Section
    {
        public ICollection<Club> Departments { get { return Clubs1; } set { Clubs1 = value; } }
    }

    public partial class BlockadeNotification
    {
        public User UnionManager { get { return User; } set { User = value; } }
    }

    public partial class UsersJob
    {
        public Club ConnectedClub { get { return Club1; } set { Club1 = value; } }
    }

    public partial class DriverDetail
    {
        public TeamsPlayer Sportsman { get { return TeamsPlayer; } set { TeamsPlayer = value; } }
    }

    public partial class TennisLeagueGame
    {
        public User HomePlayer { get { return User1; } set { User1 = value; } }
        public User GuestPlayer { get { return User; } set { User = value; } }
        public User TechnicalWinner { get { return User2; } set { User2 = value; } }
        public User HomePairPlayer { get { return User21; } set { User21 = value; } }
        public User GuestPairPlayer { get { return User3; } set { User3 = value; } }
    }

    public partial class User
    {
        public ICollection<TennisLeagueGame> TennisHomePlayers { get { return TennisLeagueGames; } set { TennisLeagueGames = value; } }
        public ICollection<TennisLeagueGame> TennisGuestPlayers { get { return TennisLeagueGames1; } set { TennisLeagueGames1 = value; } }
        public ICollection<TennisLeagueGame> TennisTechnicalWinners { get { return TennisLeagueGames2; } set { TennisLeagueGames2 = value; } }
        public ICollection<TennisLeagueGame> TennisHomePairPlayers { get { return TennisLeagueGames3; } set { TennisLeagueGames3 = value; } }
        public ICollection<TennisLeagueGame> TennisGuestPairPlayers { get { return TennisLeagueGames21; } set { TennisLeagueGames21 = value; } }

        public bool? TempIsApprovedByManager { get; set; }
    }

    public partial class TennisPlayoffBracket
    {
        public TennisPlayoffBracket Parent1
        {
            get
            {
                return TennisPlayoffBracket1;
            }
            set
            {
                TennisPlayoffBracket1 = value;
            }
        }

        public TennisPlayoffBracket Parent2
        {
            get
            {
                return TennisPlayoffBracket2;
            }
            set
            {
                TennisPlayoffBracket2 = value;
            }
        }

        public bool IsHasPlayers
        {
            get
            {
                return FirstPlayerId.HasValue || FirstPlayerPairId.HasValue || SecondPlayerId.HasValue || SecondPlayerPairId.HasValue;
            }
        }
        public bool IsMissingPlayers
        {
            get
            {
                return (!FirstPlayerId.HasValue && !FirstPlayerPairId.HasValue) || (!SecondPlayerId.HasValue && !SecondPlayerPairId.HasValue);
            }
        }
    }

    public partial class TennisGameCycle
    {
        public TeamsPlayer FirstPlayer
        {
            get
            {
                return TeamsPlayer;
            }
            set
            {
                TeamsPlayer = value;
            }
        }

        public TeamsPlayer SecondPlayer
        {
            get
            {
                return TeamsPlayer1;
            }
            set
            {
                TeamsPlayer1 = value;
            }
        }

        public TeamsPlayer FirstPairPlayer
        {
            get
            {
                return TeamsPlayer11;
            }
            set
            {
                TeamsPlayer11 = value;
            }
        }

        public TeamsPlayer SecondPairPlayer
        {
            get
            {
                return TeamsPlayer3;
            }
            set
            {
                TeamsPlayer3 = value;
            }
        }


        public int GameTypeId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Rank { get; set; }

    }



    public partial class GamesCycle
    {
        public int[] PossibleDaysForGame { get; set; }
    }


    public partial class League
    {
        public ICollection<League> DuplicatedLeagues { get { return Leagues1; } set { Leagues1 = value; } }
        public League DuplicatedLeague { get { return League1; } set { League1 = value; } }
        public string NameAndStartDate
        {
            get
            {
                if (LeagueStartDate.HasValue)
                {
                    return $"{Name} - {LeagueStartDate.Value.ToShortDateString()}";
                }
                else
                {
                    return $"{Name}";
                }

            }
        }
    }

    public enum PlayoffBracketType
    {
        Root = 0,
        Loseer = 1,
        Winner = 2,
        CondolenceWinnerLooser = 3,
        CondolenceLooserWinner = 4,
        CondolenceWinner = 5,
        Condolence3rdPlaceBracket = 6,
    }





    // Indicates NotesMessage.TypeId 's value
    public class MessageTypeEnum
    {
        public const int Root = 0x0;
        public const int Reply = 0x1;
        public const int PushNotifyOnly = 0x10;
        public const int NoInAppMessage = 0x10;
        public const int MessagingOnly = 0x20;
        public const int NoPushNotify = 0x20;
        public const int ChatMessage = 2;

        public static bool IsPushNotification(int TypeId)
        {
            return ((TypeId & NoPushNotify) == 0);
        }

        public static bool IsInAppMessage(int TypeId)
        {
            return ((TypeId & NoInAppMessage) == 0);
        }

        public static bool isReply(int TypeId)
        {
            return ((TypeId & Reply) != 0);
        }
    }

    public partial class CompetitionResult
    {
        public int LiftingRank { get; set; }
        public int PushRank { get; set; }
        public string DisciplineType { get; set; }
        public decimal PointsAfterPenalty { get; set; }
        public List<string> GetFormat6CustomFields()
        {
            if (string.IsNullOrWhiteSpace(CustomFields))
            {
                return new List<string> { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
            }
            var fields = CustomFields?.Split(',').ToList();
            while (fields.Count() < 30)
            {
                fields.Add("");
            }
            return fields;
        }

        public string GetFormat6CustomField(int column, int attempt)
        {
            if (string.IsNullOrWhiteSpace(CustomFields))
            {
                return "";
            }
            var fields = CustomFields?.Split(',').ToList();
            if (fields.Count() - 1 < column * 3 + attempt)
            {
                return "";
            }
            return fields[column * 3 + attempt];
        }

        public void SetFormat6CustomFields(string value, int column, int attempt)
        {
            var fields = GetFormat6CustomFields();
            while (fields.Count() <= column * 3 + attempt)
            {
                fields.Add("");
                fields.Add("");
                fields.Add("");
            }
            if (fields.Count() > column * 3 + attempt)
            {
                fields[column * 3 + attempt] = value;
                SetFormat6CustomFields(fields);
            }
        }

        public int GetSuccessIndex6(int columns)
        {
            if (string.IsNullOrWhiteSpace(CustomFields))
            {
                return -2;
            }
            var fields = GetFormat6CustomFields();
            var startAt = Math.Min(columns * 3, fields.Count());
            for (int i = startAt - 1; i >= 0; i--)
            {
                if (fields[i] == "O")
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetNumberFailsForCustomFields6(int columns)
        {
            if (string.IsNullOrWhiteSpace(CustomFields))
            {
                return 0;
            }
            var failCount = 0;
            var fields = GetFormat6CustomFields();
            var startAt = Math.Min(columns * 3, fields.Count());
            for (int i = startAt - 1; i >= 0; i--)
            {
                if (fields[i] == "X")
                {
                    failCount++;
                }
            }
            return failCount;
        }

        public void SetFormat6CustomFields(List<string> fields)
        {
            CustomFields = string.Join(",", fields);
        }

    }

    public partial class CompetitionClubsCorrection
    {
        public decimal FinalScore
        {
            get
            {
                var points = Points ?? 0;
                return points + Correction;
            }
        }
    }


    public partial class CompetitionDiscipline
    {
        public List<IGrouping<string, CompetitionDisciplineRegistration>> CompetitionDisciplineRegistrationsByHeat { get; set; }
        public List<CompetitionDisciplineRegistration> ResultedCompetitionDisciplineRegistrations { get; set; }

        public List<string> GetFormat6CustomFields()
        {
            if (string.IsNullOrWhiteSpace(CustomFields))
            {
                return new List<string> { "" };
            }
            return CustomFields?.Split(',').ToList();
        }

        public void SetFormat6CustomFields(List<string> fields)
        {
            CustomFields = string.Join(",", fields);
        }

        public void AddFormat6CustomFieldsNewColumn()
        {
            var fields = GetFormat6CustomFields();
            fields.Add("");
            SetFormat6CustomFields(fields);
        }
        public void RemoveFormat6CustomFieldsNewColumn()
        {
            var fields = GetFormat6CustomFields();
            if (fields.Count > 0)
            {
                fields.RemoveAt(fields.Count() - 1);
                SetFormat6CustomFields(fields);
            }
        }

        public string CategoryName { get; set; }
        public string DisciplineName { get; set; }



    }



    public partial class TennisCategoryPlayoffRank
    {
        public string CategoryName { get; set; }
        public int FinalScore
        {
            get
            {
                var points = Points ?? 0;
                var cor = Correction ?? 0;
                return points + cor;
            }
        }
    }

    public partial class Team
    {
        public int? RetrievedLeagueId { get; set; }
    }
    public partial class CompetitionDisciplineRegistration
    {
        public int? SuitableNextWeight { get; set; }
        public int? SuitableNextWeightNumber { get; set; }
        public int? PreviouslyAttemptedWeight { get; set; }

        public int? Format { get; set; }
        public int TempRank { get; set; }
        public string GroupName { get; set; }
        public bool isCombinedDiscipline { get; set; }

        public string SeasonalBest { get; set; }

        public double GetThrowingsOrderPower()
        {
            double powerValue = 0;
            var result = CompetitionResult.FirstOrDefault();
            var sortValueList = new List<double>();
            if (result != null)
            {

                sortValueList.Add(result.Alternative1 == 0 ? Convert.ToDouble(result.Attempt1) * 1000 : -1);
                sortValueList.Add(result.Alternative2 == 0 ? Convert.ToDouble(result.Attempt2) * 1000 : -1);
                sortValueList.Add(result.Alternative3 == 0 ? Convert.ToDouble(result.Attempt3) * 1000 : -1);
                sortValueList.Add(result.Alternative4 == 0 ? Convert.ToDouble(result.Attempt4) * 1000 : -1);
                sortValueList.Add(result.Alternative5 == 0 ? Convert.ToDouble(result.Attempt5) * 1000 : -1);
                sortValueList.Add(result.Alternative6 == 0 ? Convert.ToDouble(result.Attempt6) * 1000 : -1);
            }
            sortValueList = sortValueList.OrderByDescending(r => r).ToList();
            for (int i = 0; i < sortValueList.Count(); i++)
            {
                powerValue += sortValueList.ElementAt(i) * Math.Pow(10000, 6 - i);
            }

            return powerValue;
        }

    }



}
