using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace WebApi.Models
{
    //[DataContract]

    public class FanFriendViewModel : UserBaseViewModel
    {
        public List<FanTeamsViewModel> Teams { get; set; }
    }

    public class FanFriendViewModel1
    {
        public int Id { get; set; }
        public string UserName { get; set;}
        public string Image { get; set; }
        public bool IsFriend { get; set; }

        public string FriendshipStatus { get; set; }

        public bool CanRcvMsg { get; set; }

        public string FullName { get; set; }

        public string UserRole { get; set; }

        public DateTime Timestamp1 { get; set; }
    }

    public class FanPrfileViewModel : UserBaseViewModel
    {

        public List<TeamInfoViewModel> Teams { get; set; }

        public List<FanFriendViewModel1> Friends { get; set; }

        public int NumberOfFriends { get; set; }

        public int NumberOfCommonFriends { get; set; }
    }

    public class FanOwnPrfileViewModel
    {

        public TeamInfoViewModel TeamInfo { get; set; }

        public NextGameViewModel NextGame { get; set; }

        public GameViewModel LastGame { get; set; }
        public IEnumerable<GameViewModel> NextGames { get; set; }
        public IEnumerable<GameViewModel> LastGames { get; set; }

        public List<FanFriendViewModel> Friends { get; set; }

        public List<UserBaseViewModel> TeamFans { get; set; }
        public List<int> GameCycles { get; set; }
    }

    
    public class TennisLeagueGameModel
    {
        public int Id { get; set; }
        public CompetitionType CompetionType { get; set; }
        public string CompetitionName { get; set; }
        public DateTime? DateOfGame { get; set; }
        public ResultType ResultTypeValue { get; set; }
        public string ResultType { get; set; }
        public string OpponentName { get; set; }
        public string ResultScore { get; set; }
        public string PartnerName { get; set; }
    }

    public class TennisRankModel
    {
        public int Rank { get; set; }
        public int Points { get; set; }
        public string AgeName { get; set; }
    }

    public class TennisAchievement
    {
        public IEnumerable<TennisLeagueGameModel> TennisLeagueGameModel { get; set; }
        public IEnumerable<TennisRankModel> TennisRankModel { get; set; }
    }

    public class TennisAchievement1
    {
        public IEnumerable<TennisLeagueGameModel> tennisLeagueGameModel { get; set; }
        public IEnumerable<TennisRankModel> tennisRankModel { get; set; }
    }
}