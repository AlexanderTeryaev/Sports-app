using System;
using System.Linq.Expressions;
using System.Threading;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Resources;
using System.Collections.Generic;
using System.Linq;
using AppModel;
using DataService;
using System.Text.RegularExpressions;

namespace CmsApp.Helpers
{
    public static class UIHelpers
    {
        private readonly static DisciplinesRepo disciplineRepo = new DisciplinesRepo();

        public static string CurrrentUICulture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        /// <summary>
        /// Removes value substring from the end of source string if source string ends
        /// with value substring.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TrimEnd(this string source, string value)
        {
            return source.EndsWith(value) ? source.Remove(source.LastIndexOf(value, StringComparison.Ordinal)) : source;
        }

        /// <summary>
        /// If controllerName string ends with "Controller", this method cuts it off.
        /// It is useful to prepare second parameter of Url.Action method.
        /// Use it like nameof(SomeController).TrimControllerName().
        /// </summary>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public static string TrimControllerName(this string controllerName)
        {
            return controllerName.TrimEnd("Controller");
        }

        /// <summary>
        /// Returns html to render Ckeditor with the specified form name
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="name"></param>
        /// <param name="config">A Ckeditor config object which can contain any setting mentioned here: http://docs.ckeditor.com/#!/api/CKEDITOR.config e.g. new { height = 500 }</param>
        /// <returns></returns>
        public static MvcHtmlString RichEditor(this HtmlHelper htmlHelper, string name, object config = null)
        {
            return htmlHelper.Editor(name, "RichEditor", new { Config = config });
        }

        /// <summary>
        /// Returns html to render Ckeditor for the specified model property
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="config">A Ckeditor config object which can contain any setting mentioned here: http://docs.ckeditor.com/#!/api/CKEDITOR.config e.g. new { height = 500 }</param>
        /// <returns></returns>
        public static MvcHtmlString RichEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object config = null)
        {
            return htmlHelper.EditorFor(expression, "RichEditor", new { Config = config });
        }

        /// <summary>
        /// Returns name of stage
        /// </summary>
        /// <param name="gameTypeId">GameType: Division (1), Playoff (2) or Knockout (3)</param>
        /// <param name="braketsCount">Number of brackets in the current game</param>
        /// <param name="stageIndex">1-based number of stage</param>
        /// <param name="stageCustomName"></param>
        /// <param name="rounds"></param>
        /// <returns></returns>
        public static string GetStageName(int gameTypeId, int braketsCount, int stageIndex, string stageCustomName,
            int rounds)
        {
            if (gameTypeId == GameTypeId.Division)
            {
                return stageCustomName ?? $"{Messages.Stage} {stageIndex}";
            }
            else
            {
                var bracketsNo = braketsCount / rounds >> (stageIndex - 1);
                switch (bracketsNo)
                {
                    case 1:
                        return Messages.Final;
                    case 2:
                        return Messages.Semifinals;
                    case 3:
                    case 4:
                        return Messages.Quarter_finals;
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        return Messages.Final_Eighth;
                    default:
                        {
                            var stDiv = 1;
                            while (stDiv < bracketsNo) { stDiv <<= 1; }
                            return $"1/{stDiv}";
                        }
                }
            }

        }

        /// <summary>
        /// Returns string for team's position
        /// </summary>
        /// <param name="pos">Integer value of team position</param>
        /// <returns>String, representing team's position</returns>

        public static string GetFormatRawHtml(string formId, int? FormatId, string defaultValue, bool isDisabled = false, int regId = -1)
        {
            string disabledAtt = "";
            if (isDisabled) {
                disabledAtt = " disabled ";
            }

            if (!FormatId.HasValue) {
                FormatId = 0;
            }

            var funcToCall = "";
      
            if (formId != "")
                funcToCall = "onchange = 'updateExistingResult(" + regId + ")'";

            switch (FormatId)
            {
                case 1:
                    {
                        var strs = new string[0];
                        if (defaultValue != null) {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 4) {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return $"<input type='text' {disabledAtt} class='resultField form-control' name='Result1' value='{strs[2]}' form='{formId}' maxlength='2' {funcToCall}> . <input type='text' {disabledAtt} class='resultField form-control' name='Result2' value='{strs[3]}' form='{formId}' maxlength='2'  {funcToCall}>";
                    }
                case 2:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };

                        if (strs.Length < 4)
                        {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return $"<input type='text' {disabledAtt} class='resultField form-control' name='Result1' value='{strs[1]}' form='{formId}' maxlength='1'  {funcToCall}> : <input type='text' {disabledAtt} class='resultField form-control' name='Result2' value='{strs[2]}' form='{formId}' maxlength='2' {funcToCall}> . <input type='text' {disabledAtt} class='resultField form-control' name='Result3' value='{strs[3]}' form='{formId}' maxlength='2'  {funcToCall}>";
                    }
                case 3:
                    {
                        
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 4)
                        {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return $"<input type='text' {disabledAtt} class='resultField form-control' name='Result1' value='{strs[1]}' form='{formId}' maxlength='2'  {funcToCall}> : <input type='text' {disabledAtt} class='resultField form-control' name='Result2' value='{strs[2]}' form='{formId}' maxlength='2' {funcToCall}> . <input {disabledAtt} class='resultField form-control' type='text' name='Result3' value='{strs[3]}' form='{formId}' maxlength='2'  {funcToCall}>";
                    }
                case 4:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 4)
                        {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return $"<input type='text' {disabledAtt} class='resultField form-control' name='Result1' value='{strs[0]}' form='{formId}' maxlength='2' {funcToCall} > : <input type='text' {disabledAtt} class='resultField form-control' name='Result2' value='{strs[1]}' form='{formId}' maxlength='2' {funcToCall}> : <input type='text' name='Result3' {disabledAtt} class='resultField form-control' value='{strs[2]}' form='{formId}' maxlength='2' {funcToCall}> . <input type='text' name='Result4' {disabledAtt} class='resultField form-control' value='{strs[3]}' form='{formId}' maxlength='2'  {funcToCall}>";
                    }
                case 5:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 4)
                        {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return $"<input type='text' {disabledAtt} class='resultField form-control' name='Result1' value='{strs[0]}' form='{formId}' maxlength='2' {funcToCall}> : <input type='text' {disabledAtt} class='resultField form-control' name='Result2' value='{strs[1]}' form='{formId}' maxlength='2' {funcToCall}> : <input type='text' name='Result3' {disabledAtt} class='resultField form-control' value='{strs[2]}' form='{formId}' maxlength='2'  {funcToCall}>";
                    }
                case 9:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 4)
                        {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return $"<input type='text' {disabledAtt} class='resultField form-control' name='Result1' value='{strs[1]}' form='{formId}' maxlength='2' {funcToCall}> : <input type='text' {disabledAtt} class='resultField form-control' name='Result2' value='{strs[2]}' form='{formId}' maxlength='2' {funcToCall}>";
                    }
                case 10:
                case 6:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 2)
                        {
                            strs = new string[2];
                            strs[0] = "";
                            strs[1] = "";
                        }
                        return $"<input type='text' {disabledAtt} class='resultField form-control' name='Result1' value='{strs[0]}' form='{formId}' maxlength='1'  {funcToCall}> . <input type='text' {disabledAtt} class='resultField form-control' name='Result2' value='{strs[1]}' form='{formId}' maxlength='2' {funcToCall}>";
                    }
                case 11:
                case 7:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 2)
                        {
                            strs = new string[2];
                            strs[0] = "";
                            strs[1] = "";
                        }
                        return $"<input type='text' {disabledAtt} class='resultField form-control' name='Result1' value='{strs[0]}' form='{formId}' maxlength='2'  {funcToCall}> . <input type='text' {disabledAtt} class='resultField form-control' name='Result2' value='{strs[1]}' form='{formId}' maxlength='2' {funcToCall}>";
                    }
                case 8:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 2)
                        {
                            strs = new string[2];
                            strs[0] = "";
                            strs[1] = "";
                        }
                        return $"<input type='text' {disabledAtt} class='resultField form-control' name='Result1' value='{strs[0]}' form='{formId}' maxlength='1'  {funcToCall}> , <input type='text' {disabledAtt} class='resultField form-control' name='Result2' value='{strs[1]}' form='{formId}' maxlength='3' {funcToCall}>";
                    }
                default: return $"<input type='text' {disabledAtt} class='resultField form-control' name='Result1' value='{defaultValue}' style='width:80px;' form='{formId}' maxlength='30' {funcToCall} >";
            }
        }

        public static string[] SplitResultByFormat(int? FormatId, string defaultValue)
        {
            if (!FormatId.HasValue)
            {
                FormatId = 0;
            }
            switch (FormatId)
            {
                case 1:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 4)
                        {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return new string[]{ strs[2], strs[3] };
                    }
                case 2:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };

                        if (strs.Length < 4)
                        {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return new string[] { strs[1], strs[2], strs[3] };
                    }
                case 3:
                    {

                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 4)
                        {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return new string[] { strs[1], strs[2], strs[3] };
                    }
                case 4:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 4)
                        {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return new string[] { strs[0], strs[1], strs[2], strs[3] };
                    }
                case 5:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 4)
                        {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return new string[] { strs[0], strs[1], strs[2] };
                    }
                case 9:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 4)
                        {
                            strs = new string[4];
                            strs[0] = "";
                            strs[1] = "";
                            strs[2] = "";
                            strs[3] = "";
                        }
                        return new string[] { strs[1], strs[2] };
                    }
                case 6:
                case 10:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 2)
                        {
                            strs = new string[2];
                            strs[0] = "";
                            strs[1] = "";
                        }
                        return new string[] { strs[0], strs[1] };
                    }
                case 11:
                case 7:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 2)
                        {
                            strs = new string[2];
                            strs[0] = "";
                            strs[1] = "";
                        }
                        return new string[] { strs[0], strs[1] };
                    }
                case 8:
                    {
                        var strs = new string[0];
                        if (defaultValue != null)
                        {
                            strs = defaultValue.Split(new[] { ":", "." }, StringSplitOptions.None);
                        };
                        if (strs.Length < 2)
                        {
                            strs = new string[2];
                            strs[0] = "";
                            strs[1] = "";
                        }
                        return new string[] { strs[0], strs[1] };
                    }
                default: return new string[] { defaultValue };
            }
        }

        
        
        public static string GetTeamNameWithoutLeague(string teamName, string leagueName)
        {
            return teamName.Replace($" - {leagueName}", string.Empty);
        }

        public static string GetResutTitle(ResultType resultType)
        {
            switch (resultType)
            {
                case ResultType.Win:
                    return "W";
                case ResultType.Lose:
                    return "L";
                case ResultType.Draw:
                    return "D";
                default:
                    return string.Empty;
            }
        }
        public static string GetCompetitionCategoryName(int id)
        {
            return disciplineRepo.GetCompetitionCategoryById(id)?.age_name;
        }

        public static string GetMinimumBetweenMaxSportsmenAndBoatNumber(int? id, int? maxSportsmen)
        {
            int? maxRegistrations = null;
            var discipline = disciplineRepo.GetById(id.Value);
            if (maxSportsmen.HasValue || discipline.NumberOfSportsmen.HasValue)
            {
                if (!maxSportsmen.HasValue)
                {
                    maxRegistrations = discipline.NumberOfSportsmen.Value;
                }
                else if (!discipline.NumberOfSportsmen.HasValue)
                {
                    maxRegistrations = maxSportsmen.Value;
                }
                else
                {
                    maxRegistrations = Math.Min(maxSportsmen.Value, discipline.NumberOfSportsmen.Value);
                }
            }
            if (!maxRegistrations.HasValue)
            {
                return "";
            }
            return maxRegistrations.ToString();
        }
        



        public static string GetCompetitionDisciplineName(int? id)
        {
            return disciplineRepo.GetById(id.Value)?.Name;
        }

        private static string AddMissingZeroIfMissing(string str, char ch = ':') {
            var splits = str.Split(ch);
            if (splits.Length > 0 && splits[0].Length == 1) {
                return "0" + str;
            }
            return str;
        }

        private static string removeStringLeft2Zeros(string str)
        {
            str = removeStringLeftZero(str);
            str = removeStringLeftZero(str);
            return str;
        }

        private static string removeStringLeftZero(string str)
        {
            if (str.Length > 0 && str.Substring(0, 1) == "0")
                str = str.Substring(1, str.Length - 1);
            return str;
        }

        private static string removeStringLeftSemiColumn(string str)
        {
            if (str.Length > 0 && str.Substring(0, 1) == ":")
                str = str.Substring(1, str.Length - 1);
            return str;
        }

        private static string GetPostRenderResult(string str)
        {
            return removeStringLeftSemiColumn(str);
        }

        public static string GetCompetitionDisciplineResultString(string result, int? format)
        {
            string[] alternativeResultArray = new string[] { "DNF", "DQ", "DNS", "NM" };
            if (alternativeResultArray.Contains(result))
            {
                return result;
            }

            if (format == null)
            {
                format = 0;
            }
            var strs = new string[0];
            var strs2 = new string[4];
            if (result != null)
            {
                strs = result.Split(new[] { ":", "." }, StringSplitOptions.None);
            };
            strs2[0] = "";
            strs2[1] = "";
            strs2[2] = "";
            strs2[3] = "";
            for (int i = 0; i < strs.Length; i++)
            {
                strs2[i] = strs[i];
            }
            switch (format)
            {
                case 1:
                    {
                        strs2[2] = removeStringLeft2Zeros(strs2[2]);
                        return GetPostRenderResult($"{strs2[2]}.{strs2[3]}");
                    }
                case 2:
                    {
                        strs2[1] = removeStringLeft2Zeros(strs2[1]);
                        return GetPostRenderResult($"{strs2[1]}:{strs2[2]}.{strs2[3]}");
                    }
                case 3:
                    {
                        strs2[1] = removeStringLeft2Zeros(strs2[1]);
                        return GetPostRenderResult($"{strs2[1]}:{strs2[2]}.{strs2[3]}");
                    }
                case 4:
                    {
                        strs2[0] = removeStringLeft2Zeros(strs2[0]);
                        return GetPostRenderResult($"{strs2[0]}:{strs2[1]}:{strs2[2]}.{strs2[3]}");
                    }
                case 5:
                    {
                        if (strs2[0] == "00")
                        {
                            return $"{strs2[1]}:{strs2[2]}";
                        }
                        else
                        {
                            strs2[0] = removeStringLeftZero(strs2[0]);
                            strs2[0] = removeStringLeftSemiColumn(strs2[0]);
                            return GetPostRenderResult($"{strs2[0]}:{strs2[1]}:{strs2[2]}");
                        }
                    }
                case 9:
                    {
                        strs2[1] = removeStringLeft2Zeros(strs2[1]);
                        return GetPostRenderResult($"{strs2[1]}:{strs2[2]}");
                    }
                case 10:
                case 6:
                    {
                        strs2[0] = removeStringLeft2Zeros(strs2[0]);
                        return GetPostRenderResult($"{strs2[0]}.{strs2[1]}");
                    }
                case 11:
                case 7:
                    {
                        strs2[0] = removeStringLeft2Zeros(strs2[0]);
                        return GetPostRenderResult($"{strs2[0]}.{strs2[1]}");
                    }
                case 8:
                    {
                        strs2[0] = removeStringLeft2Zeros(strs2[0]);
                        return GetPostRenderResult($"{strs2[0]},{strs2[1]}");
                    }
                default:
                    {
                        return GetPostRenderResult(result);
                    }
            }
        }


        internal static bool isResultFormatCorrect(string result, int format, out double? sortedValue, out string resultStr)
        {
            switch (format)
            {
                case 1:
                    {
                        result = AddMissingZeroIfMissing(result, '.');
                        var str = $"00:00:{result}";
                        Regex checktime = new Regex("^[0-9][0-9]\\.[0-9][0-9]$");
                        bool isValid = checktime.IsMatch(result);
                        resultStr = str;
                        TimeSpan res;
                        bool success = TimeSpan.TryParse(str,out res);
                        if (!success || !isValid)
                        {
                            sortedValue = -1;
                            return false;
                        }
                        sortedValue = res.TotalMilliseconds;
                        return true;
                    }
                case 2:
                    {
                        var str = $"00:{result}";
                        Regex checktime = new Regex("^[0-9]:[0-9][0-9]\\.[0-9][0-9]$");
                        bool isValid = checktime.IsMatch(result);
                        resultStr = str;
                        TimeSpan res;
                        bool success = TimeSpan.TryParse(str, out res);
                        if (!success || !isValid)
                        {
                            sortedValue = -1;
                            return false;
                        }
                        sortedValue = res.TotalMilliseconds;
                        return true;
                    }
                case 3:
                    {
                        result = AddMissingZeroIfMissing(result);
                        var str = $"00:{result}";
                        Regex checktime = new Regex("^[0-9][0-9]:[0-9][0-9]\\.[0-9][0-9]$");
                        bool isValid = checktime.IsMatch(result);
                        resultStr = str;
                        TimeSpan res;
                        bool success = TimeSpan.TryParse(str, out res);
                        if (!success || !isValid)
                        {
                            sortedValue = -1;
                            return false;
                        }
                        sortedValue = res.TotalMilliseconds;
                        return true;
                    }
                case 4:
                    {
                        result = AddMissingZeroIfMissing(result);
                        var str = $"{result}";
                        Regex checktime = new Regex("^[0-9][0-9]:[0-9][0-9]:[0-9][0-9]\\.[0-9][0-9]$");
                        bool isValid = checktime.IsMatch(result);
                        resultStr = str;
                        TimeSpan res;
                        bool success = TimeSpan.TryParse(str, out res);
                        if (!success || !isValid)
                        {
                            sortedValue = -1;
                            return false;
                        }
                        sortedValue = res.TotalMilliseconds;
                        return true;
                    }
                case 5:
                    {
                        result = AddMissingZeroIfMissing(result);
                        var str = $"{result}.00";
                        var str2 = $"00:{result}.00";
                        Regex checktime = new Regex("^[0-9][0-9]:[0-9][0-9]:[0-9][0-9]$");
                        bool isValid = checktime.IsMatch(result);
                        Regex checktime2 = new Regex("^[0-9][0-9]:[0-9][0-9]$");
                        bool isValid2 = checktime2.IsMatch(result);
                        resultStr = str;
                        TimeSpan res;
                        TimeSpan res2;
                        bool success = TimeSpan.TryParse(str, out res);
                        bool success2 = TimeSpan.TryParse(str2, out res2);
                        if ((!success && !success2) || (!isValid && !isValid2) )
                        {
                            sortedValue = -1;
                            return false;
                        }
                        if (success && isValid)
                        {
                            sortedValue = res.TotalMilliseconds;
                            return true;
                        }
                        if (success2 && isValid2)
                        {
                            resultStr = str2;
                            sortedValue = res2.TotalMilliseconds;
                            return true;
                        }
                        sortedValue = res.TotalMilliseconds;
                        return true;
                    }
                case 9:
                    {
                        result = AddMissingZeroIfMissing(result);
                        var str = $"00:{result}.00";
                        Regex checktime = new Regex("^[0-9][0-9]:[0-9][0-9]$");
                        bool isValid = checktime.IsMatch(result);
                        resultStr = str;
                        TimeSpan res;
                        bool success = TimeSpan.TryParse(str, out res);
                        if (!success || !isValid)
                        {
                            sortedValue = -1;
                            return false;
                        }
                        sortedValue = res.TotalMilliseconds;
                        return true;
                    }
                case 10:
                case 6:
                    {
                        var str = $"{result}";
                        Regex checktime = new Regex("^[0-9]\\.[0-9][0-9]$");
                        bool isValid = checktime.IsMatch(result);
                        resultStr = str;
                        double res;
                        bool success = double.TryParse(str, out res);
                        if (!success || !isValid)
                        {
                            sortedValue = -1;
                            return false;
                        }
                        sortedValue = res*1000;
                        return true;
                    }
                case 11:
                case 7:
                    {
                        result = AddMissingZeroIfMissing(result, '.');
                        var str = $"{result}";
                        Regex checktime = new Regex("^[0-9][0-9]\\.[0-9][0-9]$");
                        bool isValid = checktime.IsMatch(result);
                        resultStr = str;
                        double res;
                        bool success = double.TryParse(str, out res);
                        if (!success || !isValid)
                        {
                            sortedValue = -1;
                            return false;
                        }
                        sortedValue = res * 1000;
                        return true;
                    }
                case 8:
                    {
                        var str = $"{result}";
                        Regex checktime = new Regex("^[0-9],[0-9][0-9][0-9]$");
                        bool isValid = checktime.IsMatch(result);
                        resultStr = str;
                        double res;
                        bool success = double.TryParse(str, out res);
                        if (!success || !isValid)
                        {
                            sortedValue = -1;
                            return false;
                        }
                        sortedValue = res * 1000;
                        return true;
                    }
                default:
                    {
                        double num;
                        bool res = double.TryParse(result, out num);
                        resultStr = result;
                        if (res == false)
                        {
                            sortedValue = 0;
                        }
                        sortedValue = num;
                        return true;
                    }
            }
        }
    }
}