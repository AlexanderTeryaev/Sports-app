
using System;

public enum CultEnum
{
    He_IL,
    En_US,
    Uk_UA,
}

public static class Languages
{
    public const string En = "en";
    public const string He = "he";
    public const string Uk = "uk";
}

public static class Locales
{
    public const string En_US = "en-US";
    public const string He_IL = "he-IL";
    public const string Uk_UA = "uk-UA";
}

public static class AppRole
{
    public const string Admins = "admins";
    public const string Editors = "editors";
    public const string Workers = "workers";
    public const string Players = "players";
    public const string Fans = "fans";
}

public static class JobRole
{
    public const string UnionManager = "unionmgr";
    public const string LeagueManager = "leaguemgr";
    public const string TeamManager = "teammgr";
    public const string Referee = "referee";
    public const string ClubManager = "clubmgr";
    public const string Activitymanager = "activitymanager";
    public const string Activityviewer = "activityviewer";
    public const string ActivityRegistrationActive = "registrationactive";
    public const string DisciplineManager = "disciplinemgr";
    public const string Spectator = "spectator";
    public const string DepartmentManager = "departmentmgr";
    public const string Desk = "desk";
    public const string Unionviewer = "unionviewer";
    public const string Multiple = "multiple";
    public const string CommitteeOfReferees = "committeeofreferees";
    public const string UnionCoach = "unioncoach";
    public const string RefereeAssignment = "referee_assignment";
    public const string RegionalManager = "Regionalmgr";
    public const string CallRoomManager = "callroommgr";
    public const string ClubSecretary = "clubsecretary";
    public const string TeamViewer = "teamviewer";
}

public static class GameStatus
{
    public const string Started = "started";
    public const string Ended = "ended";
    public const string Next = "next";
    public const string Closetodate = "closetodate";
}

public static class GamesAlias
{
    public const string BasketBall = "basketball";
    public const string NetBall = "netball";
    public const string WaterPolo = "waterpolo";
    public const string VolleyBall = "volleyball";
    public const string Soccer = "Soccer";
    public const string Gymnastic = "Gymnastic";
    public const string MartialArts = "Martial-arts";
    public const string Motorsport = "Motorsport";
    public const string Tennis = "Tennis";
    public const string Swimming = "Swimming";
    public const string Athletics = "Athletics";
    public const string Handball = "Handball";
    public const string WaveSurfing = "Wave surfing";
    public const string WeightLifting = "weight lifting";
    public const string Rugby = "Rugby";
    public const string Softball = "Softball";
    public const string Rowing = "Rowing";
    public const string Bicycle = "Bicycle";
    public const string Climbing = "Climbing";
}


public static class GPType
{
    public const string Winner = "W";
    public const string Loser = "L";
}

public static class GameType
{
    public const string Division = "Division";
    public const string Playoff = "Playoff";
    public const string Knockout = "Knockout";
    public const string Knockout34 = "Knockout_3_4";
    public const string Knockout34Consolences1Round = "knockout_3_4_condolences_1r";
    public const string Knockout34ConsolencesQuarterRound = "knockout_3_4_condolences_2q";
    
}

public static class GameTypeId
{
    public const int Division = 1;
    public const int Playoff = 2;
    public const int Knockout = 3;
    public const int Knockout34Consolences1Round = 4;
    public const int Knockout34ConsolencesQuarterRound = 5;
    public const int Knockout34 = 6;

}

public static class RoundStartCycle
{
    public const int ContinueSequentially = 1;
    public const int StartEachRoundFromCycleOne = 2;

}

public static class BlockadeType
{
    public const int All = -1;
    public const int Blockade = 0;
    public const int MedicalExpiration = 1;
    public const int InsuranceExpiration = 2; 
}

public static class PointEditType
{
    public const int WithTheirRecords = 0;
    public const int ResetScores = 1;
    public const int SetTheScores = 2;
    public const int SameRecords = 3;
}

public enum LogicaName
{
    Union,
    League,
    Team,
    Club,
    Player,
    Discipline,
    RegionalFederation,
    Unspecified
}

public enum Sections
{
    None,
    Tennis,
    MartialArts,
    Gymnastic,
    WeightLifting
}

public enum CompetitionType
{
    League,
    Competition,
    DailyCompetition
}

public enum ResultType
{
    None,
    Win,
    Lose,
    Draw
}

public class ActivityFormPaymentType
{
    public const string Fixed = "fixed";
    public const string Periods = "periods";
}

public class ActivityType
{
    public const string Group = "group";
    public const string Personal = "personal";
    public const string Club = "club";
    public const string UnionPlayerToClub = "unionplayertoclub";
}

public static class SectionAliases
{
    public const string MartialArts = "martial-arts";
    public const string Netball = "netball";
    public const string Basketball = "basketball";
    public const string MultiSport = "multi-sport";
    public const string Gymnastic = "Gymnastic";
    public const string Tennis = "Tennis";
    public const string Waterpolo = "waterpolo";
    public const string Surfing = "Surfing";
    public const string Athletics = "Athletics";
    public const string WeightLifting = "weight lifting";
    public const string Rugby = "Rugby";
    public const string Swimming = "Swimming";
    public const string Softball = "Softball";
    public const string Rowing = "Rowing";
    public const string Bicycle = "Bicycle";
    public const string Climbing = "Climbing";
}

public static class StatisticButtonsTypes
{
    public const string MadeFreeThrow = "+1";
    public const string MissFreeThrow = "Miss1";
    public const string Made2Points = "+2";
    public const string Miss2Points = "Miss2";
    public const string Made3Points = "+3";
    public const string Miss3Points = "Miss3";
    public const string Assist = "Ast";
    public const string Steal = "Stl";
    public const string Block = "Blk";
    public const string TurnOver = "TO";
    public const string TechnicalFoul = "Tecf";
    public const string OffensiveFoul = "OFoul";
    public const string PersonalFoul = "Foul";
    public const string OffensiveRebound = "OReb";
    public const string DefensiveRebound = "DReb";
    public const string OnField = "onField";
    public const string OffField = "offField";
    public const string OFoul = "OFoul";
    public const string Tecf = "Tecf";
    public const string SSave = "SSave";
    public const string Goal5m = "Goal5m";
    public const string GoalCA = "GoalCA";
    public const string GoalCF = "GoalCF";
    public const string NGoal = "NGoal";
    public const string GoalD = "GoalD";
    public const string Exc = "Exc";
    public const string Miss = "Miss";
    public const string PMiss = "MissP";
    public const string PGoal = "PGoal";
    public const string Offs = "Offs";
    public const string Foul = "Foul";
    public const string BFoul = "BFoul";
    public const string YC = "YC";
    public const string RD = "RC";
}

public enum PlayerFileType
{
    Unknown,
    Insurance,
    MedicalCertificate,
    PlayerImage,
    TeamRetirement,
    IDFile,
    EducationCert,
    DriverLicense,
    PassportFile,
    ParentStatement,
    ParentApproval,
    SpecialClassificationFile
}

public static class UnionConstants
{
    public const int IsraelCatchballAssociation = 15;
}

//public static class RegionalConstants
//{
//    public const string RegionalId = "RegionalId";

//    public const string RegionalList = "Reionals List";
//    public const string Name = "Regional Name";
//    public const string RegionalManager = "Regional Manager";
//    public const string UnionName = "Union Name";
//    public const string IsArchived = "IsArchived";
//    public const string Description = "Description";
//    public const string AdoutUs = "Adout Regional";

//    public const string RegionalApproveDate = "Regional Approve date";
//    public const string ChooseClub = "Choose Club";
//    public const string EnableRegionallevel = "Enable Regional level";

//}