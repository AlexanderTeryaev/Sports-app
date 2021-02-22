using DataService.DTO;
using Resources;
using System;

namespace CmsApp.Helpers
{
    public static class LangHelper
    {
        

        
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

        public static bool IsOrderByFormatAsc(int? format)
        {
            if (!format.HasValue)
            {
                format = 0;
            }
            if ((format >= 6 && format <= 8) || format == 10 || format == 11)
            {
                return false;
            }
            return true;
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
            }
            return "";
        }
    }
}