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
        public static string GetTeamPosition(int pos)
        {
            return $"{Messages.TeamPosition}{pos}";
        }
        public static string GetAthletePosition(int pos)
        {
            return $"{Messages.Competitor}{pos}";
        }

        public static string GetCrossesTeamPlaceholder(int pos, List<AppModel.Group> groups)
        {
            var groupName = pos % 2 == 1 ? groups?.ElementAtOrDefault(0)?.Name : groups?.ElementAtOrDefault(1)?.Name;

            return string.Format(Messages.CrossesGroupTeam_TeamFromGroup, groupName);
        }

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

        public static string GetBlockadeIconMessage(bool isBlockaded, bool isUnderPenalty)
        {
            var resultString = string.Empty;

            if (isBlockaded && !isUnderPenalty)
                resultString = Messages.Blockaded + " " + Messages.Player.ToLower();
            else if (!isBlockaded && isUnderPenalty)
                resultString = Messages.Exclusion + " " + Messages.Player.ToLower();
            else if (isBlockaded && isUnderPenalty)
                resultString = Messages.Blockaded + " " + Messages.And.ToLower() + " " + Messages.Exclusion + " " + Messages.Player.ToLower();

            return resultString;
        }

        public static string GetAuditoriumCaption(string section, bool isUpper = true)
        {
            var lang = CurrrentUICulture;
            string caption;
            if (string.Equals(section, GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Tennis, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Rugby, StringComparison.OrdinalIgnoreCase))
            {
                caption = Messages.Fields;
            }
            else if (string.Equals(section, GamesAlias.Motorsport, StringComparison.OrdinalIgnoreCase))
            {
                caption = Messages.Tracks;
            }
            else if (string.Equals(section, GamesAlias.Athletics, StringComparison.OrdinalIgnoreCase))
            {
                caption = Messages.Stadiums;
            }
            else if (string.Equals(section, GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(section, GamesAlias.Swimming, StringComparison.OrdinalIgnoreCase))
            {
                caption = Messages.Pools;
            }
            else if (string.Equals(section, GamesAlias.Rowing, StringComparison.OrdinalIgnoreCase))
            {
                caption = Messages.Sites;
            }
            else if (string.Equals(section, GamesAlias.Climbing, StringComparison.OrdinalIgnoreCase))
            {
                caption = Messages.WallClimbing;
            }
            else
            {
                caption = Messages.Auditoriums;
            }

            if (!isUpper && CurrrentUICulture == "en-US")
                return Char.ToLowerInvariant(caption[0]) + caption.Substring(1);

            return caption;
        }

        public static string GetYesNoCaption(bool value)
        {
            return value == true ? Messages.Yes : Messages.No;
        }

        public static string GetPlayerCaption(string section, bool isPlural = true)
        {
            if (section == null) {
                return isPlural ? Messages.Players : Messages.Player;
            }
            string caption;
            if (string.Equals(section, GamesAlias.MartialArts, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Swimming, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Rowing, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.WeightLifting, StringComparison.OrdinalIgnoreCase))
            {
                caption = isPlural ? Messages.Sportsmans : Messages.Sportsman;
            }
            else if (string.Equals(section, GamesAlias.Gymnastic))
            {
                caption = isPlural ? Messages.Gymnastics : Messages.Gymnastic;
            }
            else if (string.Equals(section, GamesAlias.Athletics))
            {
                caption = isPlural ? Messages.Athletes : Messages.Athletes;
            }
            else if (string.Equals(section, GamesAlias.Bicycle) || string.Equals(section, GamesAlias.Motorsport, StringComparison.OrdinalIgnoreCase))
            {
                caption = isPlural ? Messages.Riders : Messages.Rider;
            }
            else if (string.Equals(section, GamesAlias.Climbing))
            {
                caption = isPlural ? Messages.Climbers : Messages.Climber;
            }
            else
            {
                caption = isPlural ? Messages.Players : Messages.Player;
            }

            return caption;
        }

        public static string GetSortCaption(string section)
        {
            string caption;

            if (string.Equals(section, GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Tennis, StringComparison.OrdinalIgnoreCase))
            {
                caption = Messages.SortByField;
            }
            else if (string.Equals(section, GamesAlias.Motorsport, StringComparison.OrdinalIgnoreCase))
            {
                caption = Messages.SortByTracks;
            }
            else if (string.Equals(section, GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(section, GamesAlias.Swimming, StringComparison.OrdinalIgnoreCase))
            {
                caption = Messages.SortByPool;
            }
            else if (string.Equals(section, GamesAlias.Athletics, StringComparison.OrdinalIgnoreCase))
            {
                caption = Messages.SortByStadiums;
            }
            else
            {
                caption = Messages.SortByArena;
            }
            return caption;
        }
        public static void GetButtonCaption(string section, out string downloadForm,
            out string import, out string importPic, out string exportImgs,
            out string move, out string tooltip)
        {
            if (string.Equals(section, GamesAlias.MartialArts, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Motorsport, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Swimming, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Rowing, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.WeightLifting, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Bicycle, StringComparison.OrdinalIgnoreCase))
            {
                downloadForm = Messages.DownloadExcelSportsmen;
                import = Messages.ImportSportsmen;
                importPic = Messages.ImportSportsmenImage_ButtonCaption;
                exportImgs = Messages.ExportSportsmenImg;
                move = Messages.MoveSportsmenToTeam;
                tooltip = Messages.DownloadImportSportsmenFromExcel;
            }
            else if (string.Equals(section, GamesAlias.Gymnastic, StringComparison.OrdinalIgnoreCase))
            {
                downloadForm = Messages.DownloadExcelGymnastics;
                import = Messages.ImportGymnastics;
                importPic = Messages.ImportGymnasticsImage_ButtonCaption;
                exportImgs = Messages.ExportGymnasticsImg;
                move = Messages.MoveGymnasticToTeam;
                tooltip = Messages.DownloadImportGymnasticsFromExcel;
            }
            else if (string.Equals(section, GamesAlias.Athletics, StringComparison.OrdinalIgnoreCase))
            {
                downloadForm = Messages.DownloadExcelSportsmen;
                import = Messages.ImportAthletesAdd;
                importPic = Messages.ImportAthletesImage_ButtonCaptionAdd;
                exportImgs = Messages.ExportAthletesImg;
                move = Messages.MoveAthletesToTeam;
                tooltip = Messages.DownloadImportAthletesFromExcel;
            }
            else if (string.Equals(section, GamesAlias.Tennis, StringComparison.OrdinalIgnoreCase))
            {
                downloadForm = Messages.DownloadExcelPlayers;
                import = Messages.ImportPlayers;
                importPic = Messages.ImportPlayersImage_ButtonCaption;
                exportImgs = Messages.ExportPlayersImg;
                move = Messages.MovePlayerToAnotherCategory;
                tooltip = Messages.DownloadImportPlayersFromExcel;
            }
            else if (string.Equals(section, GamesAlias.WaveSurfing, StringComparison.OrdinalIgnoreCase))
            {
                downloadForm = Messages.DownloadExcelPlayers;
                import = Messages.ImportPlayers;
                importPic = Messages.ImportPlayersImage_ButtonCaption;
                exportImgs = Messages.ExportPlayersImg;
                move = Messages.MoveCopyPlayerToTeam.Replace(Messages.Team.ToLower(), Messages.Category.ToLower());
                tooltip = Messages.DownloadImportPlayersFromExcel;
            }
            else
            {
                downloadForm = Messages.DownloadExcelPlayers;
                import = Messages.ImportPlayers;
                importPic = Messages.ImportPlayersImage_ButtonCaption;
                exportImgs = Messages.ExportPlayersImg;
                move = Messages.MoveCopyPlayerToTeam;
                tooltip = Messages.DownloadImportPlayersFromExcel;
            }
        }

        public static void GetLeagueInfoCaption(string section, out string regPrice, out string insPrice,
            out string minOnTeam, out string maxOnTeam, out string total)
        {
            if (string.Equals(section, GamesAlias.MartialArts, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Motorsport, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Athletics)
                || string.Equals(section, GamesAlias.Swimming, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Rowing, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.WeightLifting)
                || string.Equals(section, GamesAlias.Bicycle)
                || string.Equals(section, GamesAlias.Climbing))
            {
                regPrice = Messages.LeagueDetail_SportsmanRegistrationPrice;
                insPrice = Messages.LeagueDetail_SportsmanInsurancePrice;
                minOnTeam = Messages.LeagueDetail_MinimumSportsmenTeam;
                maxOnTeam = Messages.LeagueDetail_MaximumSportsmenTeam;
                total = Messages.Total + " " + Messages.Sportsmans.ToLower();
            }
            else if (string.Equals(section, GamesAlias.Gymnastic, StringComparison.OrdinalIgnoreCase))
            {
                regPrice = Messages.LeagueDetail_GymnasticRegistrationPrice;
                insPrice = Messages.LeagueDetail_GymnasticInsurancePrice;
                minOnTeam = Messages.LeagueDetail_MinimumGymnastics;
                maxOnTeam = Messages.LeagueDetail_MaximumGymnastics;
                total = Messages.Total + " " + Messages.Gymnastics.ToLower();
            }
            else if (string.Equals(section, GamesAlias.WaveSurfing, StringComparison.OrdinalIgnoreCase))
            {
                regPrice = Messages.LeagueDetail_PlayerRegistrationPrice;
                insPrice = Messages.LeagueDetail_PlayerInsurancePrice;
                minOnTeam = Messages.LeagueDetail_MinimumPlayersTeam.Replace(Messages.Team.ToLower(), Messages.Category.ToLower());
                maxOnTeam = Messages.LeagueDetail_MaximumPlayersTeam.Replace(Messages.Team.ToLower(), Messages.Category.ToLower()); ;
                total = Messages.Total + " " + Messages.Players.ToLower();
            }
            else if (string.Equals(section, GamesAlias.Tennis, StringComparison.OrdinalIgnoreCase))
            {
                regPrice = Messages.LeagueDetail_GymnasticRegistrationPrice;
                insPrice = Messages.LeagueDetail_PlayerInsurancePrice;
                minOnTeam = Messages.LeagueDetail_MinimumPlayersTeam.Replace(Messages.Team.ToLower(), Messages.Category.ToLower());
                maxOnTeam = Messages.LeagueDetail_MaximumPlayersTeam.Replace(Messages.Team.ToLower(), Messages.Category.ToLower()); ;
                total = Messages.Total + " " + Messages.Players.ToLower();
            }
            else
            {
                regPrice = Messages.LeagueDetail_PlayerRegistrationPrice;
                insPrice = Messages.LeagueDetail_PlayerInsurancePrice;
                minOnTeam = Messages.LeagueDetail_MinimumPlayersTeam;
                maxOnTeam = Messages.LeagueDetail_MaximumPlayersTeam;
                total = Messages.Total + " " + Messages.Players.ToLower();
            }
        }

        public static void GetLeagueDetailsCaption(string section, out string leagueName, out string leagueCode, out string aboutLeague,
            out string leagueStructure, out string leagueSettings, bool isTennisCompetition)
        {
            if (string.Equals(section, SectionAliases.Gymnastic, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.WaveSurfing, StringComparison.OrdinalIgnoreCase))
            {
                leagueName = Messages.Name + " " + Messages.Competition.ToLowerInvariant();
                leagueCode = Messages.CompetitionCode;
                aboutLeague = Messages.AboutCompetition;
                leagueStructure = Messages.CompetitionStructure;
                leagueSettings = Messages.Settings + " " + Messages.Competition.ToLowerInvariant();
            }
            else if ((string.Equals(section, SectionAliases.Tennis, StringComparison.OrdinalIgnoreCase) && isTennisCompetition)
                || string.Equals(section, GamesAlias.Athletics, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Swimming, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Rowing, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.WeightLifting, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Bicycle, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, GamesAlias.Climbing, StringComparison.OrdinalIgnoreCase))
            {
                leagueName = Messages.CompetitionName;
                leagueCode = Messages.CompetitionCode;
                aboutLeague = Messages.AboutCompetition;
                leagueStructure = Messages.CompetitionStructure;
                leagueSettings = Messages.CompetitionSettings;
            }
            else
            {
                leagueName = Messages.LeagueName;
                leagueCode = Messages.LeagueCode;
                aboutLeague = Messages.LeagueAbout;
                leagueStructure = Messages.LeagueStructure;
                leagueSettings = Messages.League + " " + Messages.Settings.ToLowerInvariant(); ;
            }
        }

        public static void GetUnionClubInfoCaption(string sectionAlias, out string tournamentValue, out string leagueValue, out string teamsValue,
            out string createValue, out string exportValue, out string addTournament)
        {
            if (string.Equals(sectionAlias, GamesAlias.Gymnastic, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionAlias, GamesAlias.MartialArts, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionAlias, GamesAlias.Motorsport, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionAlias, GamesAlias.Athletics, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionAlias, GamesAlias.Swimming, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionAlias, GamesAlias.Rowing, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionAlias, GamesAlias.WaveSurfing, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionAlias, GamesAlias.WeightLifting, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionAlias, SectionAliases.Surfing, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionAlias, GamesAlias.Bicycle, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sectionAlias, GamesAlias.Climbing, StringComparison.OrdinalIgnoreCase))
            {
                tournamentValue = Messages.Competitions;
                leagueValue = Messages.Competitions;
                createValue = Messages.CreateCompetition;
                exportValue = Messages.ExportCompetition;
                addTournament = Messages.AddCompetition;
                if (string.Equals(sectionAlias, GamesAlias.WaveSurfing, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(sectionAlias, SectionAliases.Surfing, StringComparison.OrdinalIgnoreCase))
                {
                    teamsValue = Messages.Categories;
                }
                else
                {
                    teamsValue = Messages.Teams + " " + Messages.Union.ToLower();
                }
            }
            else
            {
                tournamentValue = Messages.Tournaments;
                leagueValue = Messages.Leagues;
                teamsValue = Messages.LeagueTeams;
                createValue = Messages.CreateTournament;
                exportValue = Messages.ExportLeague;
                addTournament = Messages.AddTournament;
            }
        }

        public static string GetGenderTitles(int? gender)
        {
            switch (gender)
            {
                case 0:
                    return Messages.Female;
                case 1:
                    return Messages.Male;
                case 3:
                    return Messages.All;
                default:
                    return string.Empty;
            }
        }

        public static string GetCurrentResultTitle(string sectionAlias)
        {

            if (string.IsNullOrEmpty(sectionAlias)) return string.Empty;

            if (string.Equals(sectionAlias, GamesAlias.BasketBall, StringComparison.OrdinalIgnoreCase)) return Messages.BasketRatio;
            if (string.Equals(sectionAlias, GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)) return Messages.GoalRatio;

            return Messages.Acts;
        }

        public static string GetCurrentResultRatio(string sectionAlias)
        {
            if (string.IsNullOrEmpty(sectionAlias)) return string.Empty;
            if (string.Equals(sectionAlias, GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)) return Messages.GoalRatio;
            return Messages.ActsRatio;
        }

        public static string GetRankName(string rankName)
        {
            switch (rankName)
            {
                case "KumiteJudgeBCompleteIsrael":
                case "KataJudgeBCompleteIsrael":
                case "KumiteJudgeBCompleteEKF":
                case "KataJudgeBCompleteEKF":
                case "KumiteJudgeBCompleteWKF":
                case "KataJudgeBCompleteWKF":
                    return $"{Messages.Judge} B {Messages.DateOfComplete}";
                case "KumiteJudgeACompleteIsrael":
                case "KataJudgeACompleteIsrael":
                case "KumiteJudgeACompleteEKF":
                case "KataJudgeACompleteEKF":
                case "KumiteJudgeACompleteWKF":
                case "KataJudgeACompleteWKF":
                    return $"{Messages.Judge} A {Messages.DateOfComplete}";
                case "KumiteJudgeBValidityIsrael":
                case "KataJudgeBValidityIsrael":
                case "KumiteJudgeBValidityEKF":
                case "KataJudgeBValidityEKF":
                case "KumiteJudgeBValidityWKF":
                case "KataJudgeBValidityWKF":
                    return $"{Messages.Judge} B {Messages.DateOfLicenseValidity}";
                case "KumiteJudgeAValidityIsrael":
                case "KataJudgeAValidityIsrael":
                case "KumiteJudgeAValidityEKF":
                case "KataJudgeAValidityEKF":
                case "KumiteJudgeAValidityWKF":
                case "KataJudgeAValidityWKF":
                    return $"{Messages.Judge} A {Messages.DateOfLicenseValidity}";
                case "RefereeBCompleteIsrael":
                case "RefereeBCompleteEKF":
                case "RefereeBCompleteWKF":
                    return $" Referee B {Messages.DateOfComplete}";
                case "RefereeACompleteIsrael":
                case "RefereeACompleteEKF":
                case "RefereeACompleteWKF":
                    return $" Referee A {Messages.DateOfComplete}";
                case "RefereeBValidityIsrael":
                case "RefereeBValidityEKF":
                case "RefereeBValidityWKF":
                    return $" Referee B {Messages.DateOfLicenseValidity}";
                case "RefereeAValidityIsrael":
                case "RefereeAValidityEKF":
                case "RefereeAValidityWKF":
                    return $" Referee A {Messages.DateOfLicenseValidity}";

                default: return string.Empty;
            }
        }

        public static void GetTeamDetailsCaptions(string section, out string underAdult, out string reserved, out string registrationData,
            out string aboutTeam, out string settings)
        {
            if (string.Equals(section, GamesAlias.WaveSurfing, StringComparison.OrdinalIgnoreCase)
                || string.Equals(section, SectionAliases.Surfing, StringComparison.OrdinalIgnoreCase))
            {
                underAdult = Messages.UnderAdult.Replace(Messages.Team, Messages.Category);
                reserved = Messages.IsReserved.Replace(Messages.Team, Messages.Category);
                registrationData = Messages.Team_RegistrationData.Replace(Messages.Team, Messages.Category);
                aboutTeam = Messages.AboutTeam.Replace(Messages.Team.ToLower(), Messages.Category.ToLower());
                settings = Messages.TeamDetails_SettingsTitle.Replace(Messages.Team, Messages.Category);
            }
            else
            {
                underAdult = Messages.UnderAdult;
                reserved = Messages.IsReserved;
                registrationData = Messages.Team_RegistrationData;
                aboutTeam = Messages.AboutTeam;
                settings = Messages.TeamDetails_SettingsTitle;
            }
        }

        public static string GetDayCapture(DayOfWeek day)
        {
            string dayOfWeek = string.Empty;
            switch (day)
            {
                case DayOfWeek.Sunday:
                    dayOfWeek = Messages.Sunday;
                    break;
                case DayOfWeek.Monday:
                    dayOfWeek = Messages.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    dayOfWeek = Messages.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    dayOfWeek = Messages.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    dayOfWeek = Messages.Thursday;
                    break;
                case DayOfWeek.Friday:
                    dayOfWeek = Messages.Friday;
                    break;
                case DayOfWeek.Saturday:
                    dayOfWeek = Messages.Saturday;
                    break;
            }
            return dayOfWeek;
        }

        public static List<SelectListItem> GetDaysSelectListItem(DayOfWeek selectedDay = DayOfWeek.Sunday)
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Selected = selectedDay == DayOfWeek.Sunday,
                    Text = Messages.Sunday,
                    Value = $"{(int)DayOfWeek.Sunday}"
                },
                new SelectListItem
                {
                    Selected = selectedDay == DayOfWeek.Monday,
                    Text = Messages.Monday,
                    Value = $"{(int)DayOfWeek.Monday}"
                },
                new SelectListItem
                {
                    Selected = selectedDay == DayOfWeek.Tuesday,
                    Text = Messages.Tuesday,
                    Value = $"{(int)DayOfWeek.Tuesday}"
                },
                new SelectListItem
                {
                    Selected = selectedDay == DayOfWeek.Wednesday,
                    Text = Messages.Wednesday,
                    Value = $"{(int)DayOfWeek.Wednesday}"
                },
                new SelectListItem
                {
                    Selected = selectedDay == DayOfWeek.Thursday,
                    Text = Messages.Thursday,
                    Value = $"{(int)DayOfWeek.Thursday}"
                },
                new SelectListItem
                {
                    Selected = selectedDay == DayOfWeek.Friday,
                    Text = Messages.Friday,
                    Value = $"{(int)DayOfWeek.Friday}"
                },
                new SelectListItem
                {
                    Selected = selectedDay == DayOfWeek.Saturday,
                    Text = Messages.Saturday,
                    Value = $"{(int)DayOfWeek.Saturday}"
                }
            };
        }

        public static string GenerateHostingDaysString(ICollection<TeamHostingDay> days)
        {
            var resultString = string.Empty;
            foreach (var day in days)
            {
                resultString += "<p>";
                resultString += $"{GetDayCapture((DayOfWeek)day.DaysForHosting.Day)}: " +
                    $"{day.DaysForHosting.StartTime} - {day.DaysForHosting.EndTime}";
                resultString += "</p>";
            }
            return resultString;
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


        public static string GetAlternativeResultStringByValue(int value)
        {
            switch (value)
            {
                case 0:
                    return "";
                case 1:
                    return "DNF";
                case 2:
                    return "DQ";
                case 3:
                    return "DNS";
                case 4:
                    return "NM";
            }
            return "";
        }




        public static int GetAlternativeResultIntByString(string str)
        {
            switch (str)
            {
                case "":
                    return 0;
                case "DNF":
                    return 1;
                case "DQ":
                    return 2;
                case "DNS":
                    return 3;
                case "NM":
                    return 4;
            }
            return 0;
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


        internal static bool isResultFormatCorrectForPhotofinish(string result, int format, out double? sortedValue, out string resultStr)
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
                        bool success = TimeSpan.TryParse(str, out res);
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
                        var str = result.Length == 5 ? $"00:0:{result}" : $"00:{result}";
                        Regex checktime = new Regex("^([0-9]:)?[0-9][0-9]\\.[0-9][0-9]$");
                        bool isValid = checktime.IsMatch(result);
                        resultStr = str;
                        TimeSpan res;
                        bool success = TimeSpan.TryParse(str, out res);
                        if (!success || !isValid)
                        {
                            sortedValue = -1;
                            resultStr = "00::.";
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
                            resultStr = "00::.";
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
                        if ((!success && !success2) || (!isValid && !isValid2))
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
                        sortedValue = res * 1000;
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

        internal static List<SelectListItem> PopulateInsuranceTypeList(List<AppModel.InsuranceType> list, int? selectedId)
        {
            var result = new List<SelectListItem>
            {
                new SelectListItem()
                {
                    Selected = selectedId == null,
                    Value = "",
                    Text = Messages.Select
                }
            };

            foreach (var item in list)
            {
                result.Add(new SelectListItem()
                {
                    Selected = selectedId == item.Id,
                    Value = item.Id.ToString(),
                    Text = LangHelper.GetInsuranceTypeById(item.Id)
                });
            }

            return result;

        }

        
    }
}