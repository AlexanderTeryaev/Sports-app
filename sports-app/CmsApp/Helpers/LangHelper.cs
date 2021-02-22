using DataService.DTO;
using Resources;
using System;

namespace CmsApp.Helpers
{
    public static class LangHelper
    {
        public static string GetRoleName(string name)
        {
            switch (name)
            {
                case AppRole.Admins: return Messages.Admins;
                case AppRole.Editors: return Messages.Editors;
                case AppRole.Workers: return Messages.Workers;
                case AppRole.Players: return Messages.Players;
                case AppRole.Fans: return Messages.Fans;
                case "referee": return Messages.Referee;
                case "desk": return Messages.Desk;
                case "spectator": return Messages.Spectator;
                default: return "";
            }
        }

        public static string GetJobName(string name)
        {
            switch (name)
            {
                case JobRole.UnionManager: return Messages.AssociationAdmin;
                case JobRole.LeagueManager: return Messages.LeagueAdmin;
                case JobRole.TeamManager: return Messages.TeamAdmin;
                case JobRole.Referee: return Messages.Referee;
                case JobRole.ClubManager: return Messages.ClubManager;
                case JobRole.Activitymanager: return Messages.ActivityManager;
                case JobRole.Activityviewer: return Messages.Activity_ActivityViewer;
                case JobRole.ActivityRegistrationActive: return Messages.Activity_ActivityRegistrationActive;
                case JobRole.DisciplineManager: return Messages.DisciplineManager;
                case JobRole.Spectator: return Messages.Spectator;
                case JobRole.DepartmentManager: return Messages.DepartmentMgr;
                case JobRole.Desk: return Messages.Desk;
                case JobRole.Unionviewer: return Messages.Unionviewer;
                case JobRole.UnionCoach: return Messages.UnionCoach;
                case JobRole.CommitteeOfReferees: return Messages.CommitteeOfReferees;
                case JobRole.RefereeAssignment: return Messages.RefereeAssignment;
                case JobRole.RegionalManager: return Messages.RegionalManager;
                case JobRole.CallRoomManager: return Messages.CallRoomManager;
                case JobRole.ClubSecretary: return Messages.ClubSecretary;
                case JobRole.TeamViewer: return Messages.TeamViewer;
                default: return "";
            }
        }

        public static string GetOfficialName(string name)
        {
            if (name == "referee" || name == "שופט")
                return JobRole.Referee;
            else if (name == "spectator" || name == "משקיף")
                return JobRole.Spectator;
            else if (name == "desk" || name == "שׁוּלְחָן כְּתִיבָה") // TODO: enter here true desk role name translation
                return JobRole.Desk;
            else
                return "";
        }

        public static string GetGender(string name)
        {
            switch (name)
            {
                case "Female": return Messages.Female;
                case "Women": return Messages.Women;
                case "Male": return Messages.Male;
                case "Men": return Messages.Men;
                case "All": return Messages.All;
                default: return "";
            }
        }

        public static int? GetGenderId(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "female":
                case "נקבה":
                    return 0;
                case "male":
                case "זכר":
                    return 1;
                default:
                    return null;
            }
        }



        public static string GetCompetitionTypeById(int typeId)
        {
            switch (typeId)
            {
                case 1:
                    return Messages.StadiumCompetition;
                case 2:
                    return Messages.RoadCompetition;
                case 3:
                    return Messages.FieldCompetition;
                default:
                    return string.Empty;
            }
        }



        public static string GetGenderCharById(int genderId)
        {
            switch (genderId)
            {
                case 0:
                    return "נ";
                case 1:
                    return "ז";
                default:
                    return "ז/נ";
            }
        }

        public static string GetDisciplineClassById(int classId)
        {
            switch (classId)
            {
                case 1:
                    return "Class S";
                case 2:
                    return "Class SB";
                case 3:
                    return "Class SM";
                default:
                    return "";
            }
        }

        public static string GetAthleticLeagueTypeById(int? typeId)
        {
            switch (typeId)
            {
                case 1:
                    return Messages.AlLeague;
                case 2:
                    return Messages.PremiereLeague;
                case 3:
                    return Messages.NationalLeague;
                case 4:
                    return Messages.AlefLeague;
                case 11:
                    return Messages.GoldenSpikesU14;
                case 12:
                    return Messages.GoldenSpikesU16;
                default: return "";
            }
        }

        public static string GetGameType(string name)
        {
            switch (name)
            {
                case GameType.Division: return Messages.Division;
                case GameType.Playoff: return Messages.Playoff;
                case GameType.Knockout: return Messages.Knockout;
                case GameType.Knockout34: return Messages.Knockout34;
                case GameType.Knockout34Consolences1Round: return Messages.Knockout34Consolences1Round;
                case GameType.Knockout34ConsolencesQuarterRound: return Messages.Knockout34ConsolencesQuarterRound;
                default: return name;
            }
        }

        public static string GetGameTypeById(int id)
        {
            switch (id)
            {
                case 1: return Messages.Division;
                case 2: return Messages.Playoff;
                case 3: return Messages.Knockout;
                case 4: return Messages.Knockout34Consolences1Round;
                case 5: return Messages.Knockout34ConsolencesQuarterRound;
                case 6: return Messages.Knockout34;
                default: return Messages.Division;
            }
        }

        public static string GetDayOfWeek(DateTime? date)
        {
            var dayString = string.Empty;
            if (date.HasValue)
            {
                switch (date.Value.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        dayString = Messages.Monday;
                        break;
                    case DayOfWeek.Tuesday:
                        dayString = Messages.Tuesday;
                        break;
                    case DayOfWeek.Wednesday:
                        dayString = Messages.Wednesday;
                        break;
                    case DayOfWeek.Thursday:
                        dayString = Messages.Thursday;
                        break;
                    case DayOfWeek.Friday:
                        dayString = Messages.Friday;
                        break;
                    case DayOfWeek.Saturday:
                        dayString = Messages.Saturday;
                        break;
                    case DayOfWeek.Sunday:
                        dayString = Messages.Sunday;
                        break;
                }
            }
            return dayString;
        }

        public static string GetLeagueTypeValue(int? typeId)
        {
            var typeName = string.Empty;

            if (typeId != null)
            {
                switch (typeId)
                {
                    case (int)CompetitionType.League:
                        typeName = Messages.League;
                        break;
                    case (int)CompetitionType.Competition:
                        typeName = Messages.Competition;
                        break;
                }
            }

            return typeName;
        }

        public static string GetPlayerCaption(string section, bool isPlural = false)
        {
            var result = string.Empty;

            if (section.Equals(SectionAliases.MartialArts, StringComparison.OrdinalIgnoreCase)) result = isPlural ? Messages.Sportsmans : Messages.Sportsman;
            else if (section.Equals(SectionAliases.Gymnastic, StringComparison.OrdinalIgnoreCase)) result = isPlural ? Messages.Gymnastics : Messages.Gymnastic;
            else if (section.Equals(SectionAliases.Athletics, StringComparison.OrdinalIgnoreCase)) result = isPlural ? Messages.Athletes : Messages.Athlete;
            else result = isPlural ? Messages.Players : Messages.Player;

            return result;
        }

        public static string ReplacePlayerAcordingToSection(string message, string section, bool shouldBePlural = false, bool isBothGenders = false)
        {
            var correctReplacement = string.Empty;
            var femaleGendersuffix = $"{Messages.Player}/ית";
            switch (section)
            {       
                case SectionAliases.MartialArts:
                    {
                        correctReplacement = shouldBePlural ? Messages.Sportsmans : Messages.Sportsman;
                        femaleGendersuffix = $"{Messages.Player}/ת";
                        break;
                    }
                case SectionAliases.Gymnastic:
                    {
                        correctReplacement = shouldBePlural ? Messages.Gymnastics : Messages.Gymnast;
                        femaleGendersuffix = $"{Messages.Player}/ת";
                        break;
                    }
                case SectionAliases.Athletics:
                    {
                        correctReplacement = shouldBePlural ? Messages.Athletes : Messages.Athlete;
                        femaleGendersuffix = $"{Messages.Player}/ת";
                        break;
                    }
                default:
                    {
                        correctReplacement = shouldBePlural ? Messages.Players : Messages.Player;
                        break;
                    }
            }
            if (!shouldBePlural)
            {
                message = message.Replace(Messages.Player, femaleGendersuffix);
            }
            return message.Replace(Messages.Player, correctReplacement);
        }

        public static string isCompositionHasValue(GymnasticDto gymnasticDto) {
            int compositionNumUi = gymnasticDto.CompositionNumber + 1;
            switch (gymnasticDto.CompositionNumber) {
                case 0:
                {
                        if(gymnasticDto.Composition != null)
                            return compositionNumUi.ToString();
                        break;
                }
                case 1:
                    {
                        if (gymnasticDto.SecondComposition != null)
                            return compositionNumUi.ToString();
                        break;
                    }
                case 2:
                    {
                        if (gymnasticDto.ThirdComposition != null)
                            return compositionNumUi.ToString();
                        break;
                    }
                case 3:
                    {
                        if (gymnasticDto.FourthComposition != null)
                            return compositionNumUi.ToString();
                        break;
                    }
                case 4:
                    {
                        if (gymnasticDto.FifthComposition != null)
                            return compositionNumUi.ToString();
                        break;
                    }
                case 5:
                    {
                        if (gymnasticDto.SixthComposition != null)
                            return compositionNumUi.ToString();
                        break;
                    }
                case 6:
                    {
                        if (gymnasticDto.SeventhComposition != null)
                            return compositionNumUi.ToString();
                        break;
                    }
                case 7:
                    {
                        if (gymnasticDto.EighthComposition != null)
                            return compositionNumUi.ToString();
                        break;
                    }
                case 8:
                    {
                        if (gymnasticDto.NinthComposition != null)
                            return compositionNumUi.ToString();
                        break;
                    }
                case 9:
                    {
                        if (gymnasticDto.TenthComposition != null)
                            return compositionNumUi.ToString();
                        break;
                    }
            }
            return "";
        }

        public static string GetInsuranceTypeById(int id)
        {
            switch (id)
            {
                case 1: return Messages.InsType_MontlyPaidMinWage;
                case 2: return Messages.InsType_ActivitySupportByWork;
                case 3: return Messages.InsType_SchoolStudent;
                case 4: return Messages.InsType_ActivityNoPayments;
                case 5: return Messages.InsType_None;
                default: return "";
            }
        }
    }
}