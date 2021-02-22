using System;
using ClosedXML.Excel;
using Resources;
using DataService.LeagueRank;
using System.Linq;
using CmsApp.Models;

namespace CmsApp.Helpers
{
    public class LeagueRankExportHelper
    {

        public void CreateExcelForWaterpoloAndSoccer(IXLWorksheet ws, RankLeague rLeague, bool isHeb)
        {
            var columnCounter = 1;
            var rowCounter = 1;

            var addCell = new Action<string>(value =>
            {
                ws.Cell(rowCounter, columnCounter).Value = value;
                columnCounter++;
            });

            if (rLeague.Teams.Count > 0)
            {
                addCell("#");
                addCell(Messages.Team);
                addCell(Messages.GamesNum);
                addCell(Messages.WinsNum);
                addCell(Messages.Draw);
                addCell(Messages.LossNum);
                addCell(Messages.GoalRatio);
                addCell(string.Empty);
                addCell(Messages.Points);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                foreach (var team in rLeague.Teams)
                {
                    addCell("-");
                    addCell(team.Title);
                    addCell("0");
                    addCell("0");
                    addCell("0");
                    addCell("0");
                    addCell("0");
                    addCell("0");
                    addCell("0");

                    rowCounter++;
                    columnCounter = 1;
                }
            }
            else if (rLeague.IsEmptyRankTable)
            {
                foreach (var stage in rLeague.Stages.Where(x => x.Groups.Count > 0))
                {

                    var groups = stage.Groups;
                    if (groups.Count() > 0 && groups.All(g => g.IsAdvanced))
                    {
                        if (groups.All(g => !g.Teams.Any()))
                        {
                            continue;
                        }
                        var firstGroup = groups.FirstOrDefault();
                        if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                        {
                            int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                            switch (numOfBrackets)
                            {
                                case 1:
                                    addCell(Messages.Final);
                                    break;
                                case 2:
                                    addCell(Messages.Semifinals);
                                    break;
                                case 4:
                                    addCell(Messages.Quarter_finals);
                                    break;
                                case 8:
                                    addCell(Messages.Final_Eighth);
                                    break;
                                default:
                                    addCell($"{numOfBrackets * 2} { Messages.FinalNumber}");
                                    break;
                            }

                            rowCounter++;
                            columnCounter = 1;
                        }
                    }
                    else
                    {
                        addCell($"{Messages.Stage} {stage.Number}");
                        rowCounter++;
                        columnCounter = 1;
                    }
                    foreach (var group in stage.Groups)
                    {
                        var teams = group.Teams.OrderBy(x => x.TeamPosition).ToList();
                        if (!group.IsAdvanced)
                        {
                            addCell(group.Title);
                            rowCounter++;
                            columnCounter = 1;
                        }
                        addCell("#");
                        addCell(Messages.Team);
                        if (!group.IsAdvanced)
                        {
                            addCell(Messages.GamesNum);
                            addCell(Messages.WinsNum);
                            addCell(Messages.Draw);
                            addCell(Messages.LossNum);
                            addCell(Messages.GoalRatio);
                            addCell(string.Empty);
                            addCell(Messages.Points);
                        }
                        rowCounter++;
                        columnCounter = 1;
                        for (var i = 0; i < teams.Count(); i++)
                        {
                            if (group.IsAdvanced)
                            {
                                int numOfBrackets = (int)group.PlayoffBrackets;
                                if (i % ((numOfBrackets)) == 0)
                                {
                                    addCell($"{i + 1}");
                                }
                                else
                                {
                                    addCell($"-");
                                }
                            }
                            else
                            {
                                if (i != 0 && teams[i].TeamPosition == teams[i - 1].TeamPosition)
                                {
                                    addCell($"-");
                                }
                                else if (teams[i].Games == 0)
                                {
                                    addCell($"-");
                                }
                                else
                                {
                                    addCell($"{teams[i].TeamPosition}");
                                }
                            }

                            addCell($"{group.Teams[i].Title}");
                            if (!group.IsAdvanced)
                            {
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                        rowCounter++;
                        columnCounter = 1;
                    }
                    rowCounter++;
                    columnCounter = 1;
                }
            }
            else
            {
                foreach (var stage in rLeague.Stages)
                {
                    var groups = stage.Groups;
                    if (groups.Count() > 0 && groups.All(g => g.IsAdvanced))
                    {
                        if (groups.All(g => !g.Teams.Any()))
                        {
                            continue;
                        }
                        var firstGroup = groups.FirstOrDefault();
                        if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                        {
                            int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                            switch (numOfBrackets)
                            {
                                case 1:
                                    addCell(Messages.Final);
                                    break;
                                case 2:
                                    addCell(Messages.Semifinals);
                                    break;
                                case 4:
                                    addCell(Messages.Quarter_finals);
                                    break;
                                case 8:
                                    addCell(Messages.Final_Eighth);
                                    break;
                                default:
                                    addCell($"{numOfBrackets * 2} {Messages.FinalNumber}");
                                    break;
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                    }
                    else
                    {
                        addCell($"{Messages.Stage} {stage.Number}");
                        rowCounter++;
                        columnCounter = 1;
                    }
                    foreach (var group in stage.Groups)
                    {
                        var teams = group.Teams.ToList();
                        if (!group.IsAdvanced)
                        {
                            addCell(group.Title);
                            rowCounter++;
                            columnCounter = 1;
                        }
                        addCell("#");
                        addCell(Messages.Team);
                        if (!group.IsAdvanced)
                        {
                            addCell(Messages.GamesNum);
                            addCell(Messages.WinsNum);
                            addCell(Messages.Draw);
                            addCell(Messages.LossNum);
                            addCell(Messages.GoalRatio);
                            addCell(string.Empty);
                            addCell(Messages.Points);
                        }
                        rowCounter++;
                        columnCounter = 1;
                        for (var i = 0; i < teams.Count(); i++)
                        {
                            if (group.IsAdvanced)
                            {
                                int numOfBrackets = (int)group.PlayoffBrackets;
                                if (i % ((numOfBrackets)) == 0)
                                {
                                    addCell($"{i + 1}");
                                }
                                else
                                {
                                    addCell("-");
                                }
                            }
                            else
                            {

                                if (i != 0 && teams[i].Points == teams[i - 1].Points && teams[i].PointsDifference == teams[i - 1].PointsDifference)
                                {
                                    addCell("-");
                                }
                                else
                                {
                                    addCell($"{teams[i].TeamPosition}");
                                }
                            }
                            addCell(teams[i].Title);
                            if (!group.IsAdvanced)
                            {
                                addCell($"{teams[i].Games}");
                                addCell($"{teams[i].Wins}");
                                addCell($"{teams[i].Draw}");
                                addCell($"{teams[i].Loses}");

                                string gameResult = isHeb ? $"{teams[i].GuesTeamFinalScore} - {teams[i].HomeTeamFinalScore}"
                                    : $"{teams[i].HomeTeamFinalScore} - {teams[i].GuesTeamFinalScore}";
                                ws.Cell(rowCounter, columnCounter).SetValue(Convert.ToString(gameResult));
                                columnCounter++;

                                addCell($"{teams[i].PointsDifference}");
                                addCell($"{teams[i].Points}");
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                        rowCounter++;
                        columnCounter = 1;
                    }
                    rowCounter++;
                    columnCounter = 1;
                }
            }
        }

        public void CreateExcelForTennisRank(IXLWorksheet ws, UnionTennisRankForm unionRank, bool isHeb)
        {
            var columnCounter = 1;
            var rowCounter = 1;

            var addCell = new Action<string>(value =>
            {
                ws.Cell(rowCounter, columnCounter).Value = value;
                columnCounter++;
            });

            if (unionRank.RankList.Count > 0)
            {
                addCell("#");
                addCell(Messages.FullName);
                addCell(Messages.BirthDay);
                addCell(Messages.ClubName);
                addCell(Messages.AveragePoints);
                addCell(Messages.PointsToAverage);
                addCell(Messages.Points);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                int count = 1;

                foreach (var rank in unionRank.RankList)
                {
                    addCell(count.ToString());
                    addCell(rank.FullName);
                    addCell(rank.Birthday.Value.ToShortDateString());
                    addCell(rank.TrainingTeam);
                    addCell(rank.AveragePoints.ToString());
                    addCell(rank.PointsToAverage.ToString());
                    addCell(rank.TotalPoints.ToString());

                    rowCounter++;
                    columnCounter = 1;
                    count++;
                }
            }
            else
            {
                addCell("#");
                addCell(Messages.FullName);
                addCell(Messages.BirthDay);
                addCell(Messages.Training);
                addCell(Messages.AveragePoints);
                addCell(Messages.PointsToAverage);
                addCell(Messages.Points);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();
            }
        }

        public void CreateExcelForBasketball(IXLWorksheet ws, RankLeague rLeague, bool isHeb)
        {
            var columnCounter = 1;
            var rowCounter = 1;

            var addCell = new Action<string>(value =>
            {
                ws.Cell(rowCounter, columnCounter).Value = value;
                columnCounter++;
            });

            if (rLeague.Teams.Count > 0)
            {
                addCell("#");
                addCell(Messages.Team);
                addCell(Messages.GamesNum);
                addCell(Messages.WinsNum);
                addCell(Messages.LossNum);
                addCell(Messages.TechLossesNum);
                addCell(Messages.BasketRatio);
                addCell(string.Empty);
                addCell(Messages.Points);

                rowCounter++;
                columnCounter = 1;
                ws.Columns().AdjustToContents();

                foreach (var team in rLeague.Teams)
                {
                    addCell("-");
                    addCell(team.Title);
                    addCell("0");
                    addCell("0");
                    addCell("0");
                    addCell("0");
                    addCell("0");
                    addCell("0");
                    addCell("0");

                    rowCounter++;
                    columnCounter = 1;
                }
            }
            else if (rLeague.IsEmptyRankTable)
            {
                foreach (var stage in rLeague.Stages.Where(x => x.Groups.Count > 0))
                {
                    var groups = stage.Groups;
                    if (groups.Count() > 0 && groups.All(g => g.IsAdvanced))
                    {
                        if (groups.All(g => !g.Teams.Any()))
                        {
                            continue;
                        }
                        var firstGroup = groups.FirstOrDefault();
                        if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                        {
                            int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                            switch (numOfBrackets)
                            {
                                case 1:
                                    addCell(Messages.Final);
                                    break;
                                case 2:
                                    addCell(Messages.Semifinals);
                                    break;
                                case 4:
                                    addCell(Messages.Quarter_finals);
                                    break;
                                case 8:
                                    addCell(Messages.Final_Eighth);
                                    break;
                                default:
                                    addCell($"{numOfBrackets * 2} {Messages.FinalNumber}");
                                    break;
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                    }
                    else
                    {
                        addCell($"{Messages.Stage} {stage.Number}");
                        rowCounter++;
                        columnCounter = 1;
                    }
                    foreach (var group in stage.Groups)
                    {
                        var teams = group.Teams.ToList();
                        if (!group.IsAdvanced)
                        {
                            addCell(group.Title);
                            rowCounter++;
                            columnCounter = 1;
                        }
                        addCell("#");
                        addCell(Messages.Team);
                        if (!group.IsAdvanced)
                        {
                            addCell(Messages.GamesNum);
                            addCell(Messages.WinsNum);
                            addCell(Messages.LossNum);
                            addCell(Messages.TechLossesNum);
                            addCell(Messages.BasketRatio);
                            addCell(string.Empty);
                            addCell(Messages.Points);
                        }
                        rowCounter++;
                        columnCounter = 1;

                        for (var i = 0; i < teams.Count(); i++)
                        {
                            if (group.IsAdvanced)
                            {
                                int numOfBrackets = (int)group.PlayoffBrackets;
                                if (i % ((numOfBrackets)) == 0)
                                {
                                    addCell($"{i + 1}");
                                }
                                else
                                {
                                    addCell("-");
                                }
                            }
                            else
                            {
                                if (i != 0 && teams[i].TeamPosition == teams[i - 1].TeamPosition)
                                {
                                    addCell("-");
                                }
                                else
                                {
                                    addCell($"{teams[i].TeamPosition}");
                                }
                            }
                            addCell($"{group.Teams[i].Title}");
                            if (!group.IsAdvanced)
                            {
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                        rowCounter++;
                        columnCounter = 1;
                    }
                    rowCounter++;
                    columnCounter = 1;
                }
            }
            else
            {
                foreach (var stage in rLeague.Stages)
                {
                    var groups = stage.Groups;
                    if (groups.Count() > 0 && groups.All(g => g.IsAdvanced))
                    {
                        if (groups.All(g => !g.Teams.Any()))
                        {
                            continue;
                        }
                        var firstGroup = groups.FirstOrDefault();
                        if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                        {
                            int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                            switch (numOfBrackets)
                            {
                                case 1:
                                    addCell(Messages.Final);
                                    break;
                                case 2:
                                    addCell(Messages.Semifinals);
                                    break;
                                case 4:
                                    addCell(Messages.Quarter_finals);
                                    break;
                                case 8:
                                    addCell(Messages.Final_Eighth);
                                    break;
                                default:
                                    addCell($"{ numOfBrackets * 2} { Messages.FinalNumber}");
                                    break;
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                    }
                    else
                    {
                        addCell($"{Messages.Stage} {stage.Number}");
                    }
                    foreach (var group in stage.Groups)
                    {
                        var teams = group.Teams;
                        if (!group.IsAdvanced)
                        {
                            addCell(group.Title);
                            rowCounter++;
                            columnCounter = 1;
                        }
                        addCell("#");
                        addCell(Messages.Team);
                        if (!group.IsAdvanced)
                        {
                            addCell(Messages.GamesNum);
                            addCell(Messages.WinsNum);
                            addCell(Messages.LossNum);
                            addCell(Messages.TechLossesNum);
                            addCell(Messages.BasketRatio);
                            addCell(string.Empty);
                            addCell(Messages.Points);
                        }
                        rowCounter++;
                        columnCounter = 1;
                        for (var i = 0; i < teams.Count(); i++)
                        {
                            if (group.IsAdvanced)
                            {
                                int numOfBrackets = (int)group.PlayoffBrackets;
                                if (i % ((numOfBrackets)) == 0)
                                {
                                    addCell($"{i + 1}");
                                }
                                else
                                {
                                    addCell("-");
                                }
                            }
                            else
                            {

                                if (i != 0 && teams[i].Points == teams[i - 1].Points && teams[i].PointsDifference == teams[i - 1].PointsDifference)
                                {
                                    addCell("-");
                                }

                                else
                                {
                                    addCell($"{teams[i].TeamPosition}");
                                }
                            }
                            addCell($"{teams[i].Title}");
                            if (!group.IsAdvanced)
                            {
                                addCell($"{teams[i].Games}");
                                addCell($"{teams[i].Wins}");
                                addCell($"{teams[i].Loses}");
                                addCell($"{teams[i].TechLosses}");

                                string gameResult = isHeb ? $"{teams[i].GuesTeamFinalScore} - {teams[i].HomeTeamFinalScore}"
                                    : $"{teams[i].HomeTeamFinalScore} - {teams[i].GuesTeamFinalScore}";
                                ws.Cell(rowCounter, columnCounter).SetValue(Convert.ToString(gameResult));
                                columnCounter++;

                                addCell($"{teams[i].PointsDifference}");
                                addCell($"{teams[i].Points}");
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                        rowCounter++;
                        columnCounter = 1;
                    }
                    rowCounter++;
                    columnCounter = 1;
                }
            }
        }

        public void CreateExcelForNetballAndVolleyball(IXLWorksheet ws, RankLeague rLeague, bool isHeb)
        {
            var columnCounter = 1;
            var rowCounter = 1;

            var addCell = new Action<string>(value =>
            {
                ws.Cell(rowCounter, columnCounter).Value = value;
                columnCounter++;
            });
            if (rLeague.Teams.Count > 0)
            {
                addCell("#");
                addCell(Messages.Team);
                addCell(Messages.GamesNum);
                addCell(Messages.WinsNum);
                addCell(Messages.LossNum);
                addCell(Messages.Acts);
                addCell(Messages.ActsRatio);
                addCell(Messages.Points);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                foreach (var team in rLeague.Teams)
                {
                    addCell("-");
                    addCell(team.Title);
                    addCell(team.Title);
                    addCell(team.Title);
                    addCell(team.Title);
                    addCell(team.Title);
                    addCell(team.Title);

                    rowCounter++;
                    columnCounter = 1;
                }
            }
            else if (rLeague.IsEmptyRankTable)
            {
                foreach (var stage in rLeague.Stages.Where(x => x.Groups.Count > 0))
                {
                    var groups = stage.Groups;
                    if (groups.Count() > 0 && groups.All(g => g.IsAdvanced))
                    {
                        if (groups.All(g => !g.Teams.Any()))
                        {
                            continue;
                        }
                        var firstGroup = groups.FirstOrDefault();
                        if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                        {
                            int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                            switch (numOfBrackets)
                            {
                                case 1:
                                    addCell(Messages.Final);
                                    break;
                                case 2:
                                    addCell(Messages.Semifinals);
                                    break;
                                case 4:
                                    addCell(Messages.Quarter_finals);
                                    break;
                                case 8:
                                    addCell(Messages.Final_Eighth);
                                    break;
                                default:
                                    addCell($"{numOfBrackets * 2} {Messages.FinalNumber}");
                                    break;
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                    }
                    else
                    {
                        addCell($"{Messages.Stage} {stage.Number}");
                        rowCounter++;
                        columnCounter = 1;
                    }
                    foreach (var group in stage.Groups)
                    {
                        if (!group.IsAdvanced)
                        {
                            addCell(group.Title);
                            rowCounter++;
                            columnCounter = 1;
                        }
                        addCell("#");
                        addCell(Messages.Team);

                        if (!group.IsAdvanced)
                        {
                            addCell(Messages.GamesNum);
                            addCell(Messages.WinsNum);
                            addCell(Messages.LossNum);
                            addCell(Messages.Acts);
                            addCell(Messages.ActsRatio);
                            addCell(Messages.Points);
                        }
                        rowCounter++;
                        columnCounter = 1;
                        for (var i = 0; i < group.Teams.Count(); i++)
                        {
                            if (group.IsAdvanced)
                            {
                                int numOfBrackets = (int)group.PlayoffBrackets;
                                if (i % ((numOfBrackets)) == 0)
                                {
                                    addCell($"{i + 1}");
                                }
                                else
                                {
                                    addCell("-");
                                }
                            }
                            else
                            {
                                if (i != 0 && group.Teams[i].Position == group.Teams[i - 1].Position)
                                {
                                    addCell("-");
                                }
                                else
                                {
                                    addCell($"{i + 1}");
                                }
                            }
                            addCell(group.Teams[i].Title);
                            if (!group.IsAdvanced)
                            {
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                        rowCounter++;
                        columnCounter = 1;
                    }
                    rowCounter++;
                    columnCounter = 1;
                }
            }
            else
            {
                foreach (var stage in rLeague.Stages)
                {
                    var groups = stage.Groups;
                    if (groups.Count() > 0 && groups.All(g => g.IsAdvanced))
                    {
                        if (groups.All(g => !g.Teams.Any()))
                        {
                            continue;
                        }
                        var firstGroup = groups.FirstOrDefault();
                        if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                        {
                            int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                            switch (numOfBrackets)
                            {
                                case 1:
                                    addCell(Messages.Final);
                                    break;
                                case 2:
                                    addCell(Messages.Semifinals);
                                    break;
                                case 4:
                                    addCell(Messages.Quarter_finals);
                                    break;
                                case 8:
                                    addCell(Messages.Final_Eighth);
                                    break;
                                default:
                                    addCell($"{numOfBrackets * 2} {Messages.FinalNumber}");
                                    break;
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                    }
                    else
                    {
                        addCell($"{Messages.Stage} {stage.Number}");
                        rowCounter++;
                        columnCounter = 1;
                    }
                    foreach (var group in stage.Groups)
                    {
                        if (!group.IsAdvanced)
                        {
                            addCell(group.Title);
                            rowCounter++;
                            columnCounter = 1;
                        }
                        addCell("#");
                        addCell(Messages.Team);
                        if (!group.IsAdvanced)
                        {
                            addCell(Messages.GamesNum);
                            addCell(Messages.WinsNum);
                            addCell(Messages.LossNum);
                            addCell(Messages.Acts);
                            addCell(Messages.ActsRatio);
                            addCell(Messages.Points);
                        }
                        rowCounter++;
                        columnCounter = 1;

                        for (var i = 0; i < group.Teams.Count(); i++)
                        {
                            if (group.IsAdvanced)
                            {
                                int numOfBrackets = (int)group.PlayoffBrackets;
                                if (i % ((numOfBrackets)) == 0)
                                {
                                    addCell($"{i + 1}");
                                }
                                else
                                {
                                    addCell("-");
                                }
                            }
                            else
                            {
                                if (i != 0 && group.Teams[i].Position == group.Teams[i - 1].Position)
                                {
                                    addCell("-");
                                }
                                else
                                {
                                    addCell($"{i + 1}");
                                }
                            }
                            addCell(group.Teams[i].Title);
                            if (!group.IsAdvanced)
                            {
                                addCell($"{group.Teams[i].Games}");
                                addCell($"{group.Teams[i].Wins}");
                                addCell($"{group.Teams[i].Loses}");
                                string gameResult = isHeb ? $"{group.Teams[i].SetsLost} - {group.Teams[i].SetsWon}"
                                    : $"{group.Teams[i].SetsWon} - {group.Teams[i].SetsLost}";
                                ws.Cell(rowCounter, columnCounter).SetValue(Convert.ToString(gameResult));
                                columnCounter++;

                                addCell($"{group.Teams[i].SetsRatio}");
                                addCell($"{ group.Teams[i].Points}");
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }

                        rowCounter++;

                        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().Take(group.ExtendedTables.Count).ToArray();
                        int charIndex = 0;

                        addCell(string.Empty);
                        addCell(Messages.Team);

                        for (var i = 0; i < alpha.Length; i++)
                        {
                            addCell($"{alpha[i]}");
                        }

                        rowCounter++;
                        columnCounter = 1;

                        foreach (var team in group.ExtendedTables)
                        {
                            addCell($"{alpha[charIndex++]}");
                            addCell(team.TeamName);

                            for (var i = 0; i < group.ExtendedTables.Count; i++)
                            {
                                if (group.ExtendedTables[i].TeamId == team.TeamId)
                                {
                                    addCell(string.Empty);
                                }
                                else
                                {
                                    var text = string.Empty;
                                    foreach (var score in team.Scores.Where(x => x.OpponentTeamId == group.ExtendedTables[i].TeamId))
                                    {
                                        text += isHeb ? $"{score.OpponentScore} : {score.TeamScore}"
                                            : $"{score.TeamScore} : {score.OpponentScore}";
                                    }
                                    ws.Cell(rowCounter, columnCounter).SetValue(Convert.ToString(text));
                                    columnCounter++;
                                }

                            }

                            rowCounter++;
                            columnCounter = 1;
                        }
                        rowCounter++;
                        columnCounter = 1;
                    }
                    rowCounter++;
                    columnCounter = 1;
                }
            }
        }

        internal void CreateExcelForTennisLeague(IXLWorksheet ws, RankLeague rLeague, bool isHeb)
        {
            var columnCounter = 1;
            var rowCounter = 1;

            var addCell = new Action<string>(value =>
            {
                ws.Cell(rowCounter, columnCounter).Value = value;
                columnCounter++;
            });
            if (rLeague.Teams.Count > 0)
            {
                addCell("#");
                addCell(Messages.TeamName);
                addCell(Messages.Matches);
                addCell(Messages.Points);
                addCell(Messages.WinsNum);
                addCell(Messages.LossNum);
                addCell(Messages.Tie);
                addCell($"{Messages.Sets} +");
                addCell($"{Messages.Sets} -");
                addCell($"{Messages.Gaming} +");
                //addCell(Messages.Penalty);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                foreach (var team in rLeague.Teams)
                {
                    addCell("-");
                    addCell(team.Title);
                    addCell(team.Title);
                    addCell(team.Title);
                    addCell(team.Title);
                    addCell(team.Title);
                    addCell(team.Title);
                    addCell(team.Title);
                    addCell(team.Title);
                    addCell(team.Title);

                    rowCounter++;
                    columnCounter = 1;
                }
            }
            else if (rLeague.IsEmptyRankTable)
            {
                foreach (var stage in rLeague.Stages.Where(x => x.Groups.Count > 0))
                {
                    var groups = stage.Groups;
                    if (groups.Count() > 0 && groups.All(g => g.IsAdvanced))
                    {
                        if (groups.All(g => !g.Teams.Any()))
                        {
                            continue;
                        }
                        var firstGroup = groups.FirstOrDefault();
                        if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                        {
                            int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                            switch (numOfBrackets)
                            {
                                case 1:
                                    addCell(Messages.Final);
                                    break;
                                case 2:
                                    addCell(Messages.Semifinals);
                                    break;
                                case 4:
                                    addCell(Messages.Quarter_finals);
                                    break;
                                case 8:
                                    addCell(Messages.Final_Eighth);
                                    break;
                                default:
                                    addCell($"{numOfBrackets * 2} {Messages.FinalNumber}");
                                    break;
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                    }
                    else
                    {
                        addCell($"{Messages.Stage} {stage.Number}");
                        rowCounter++;
                        columnCounter = 1;
                    }
                    foreach (var group in stage.Groups)
                    {
                        if (!group.IsAdvanced)
                        {
                            addCell(group.Title);
                            rowCounter++;
                            columnCounter = 1;
                        }
                        addCell("#");
                        addCell(Messages.Team);

                        if (!group.IsAdvanced)
                        {
                            addCell(Messages.Matches);
                            addCell(Messages.Points);
                            addCell(Messages.WinsNum);
                            addCell(Messages.LossNum);
                            addCell(Messages.Tie);
                            addCell($"{Messages.Sets} +");
                            addCell($"{Messages.Sets} -");
                            addCell($"{Messages.Gaming} +");
                            addCell($"{Messages.Gaming} -");
                            //addCell(Messages.Penalty);
                        }
                        rowCounter++;
                        columnCounter = 1;
                        for (var i = 0; i < group.Teams.Count(); i++)
                        {
                            if (group.IsAdvanced)
                            {
                                int numOfBrackets = (int)group.PlayoffBrackets;
                                if (i % ((numOfBrackets)) == 0)
                                {
                                    addCell($"{i + 1}");
                                }
                                else
                                {
                                    addCell("-");
                                }
                            }
                            else
                            {
                                if (i != 0 && group.Teams[i].Position == group.Teams[i - 1].Position)
                                {
                                    addCell("-");
                                }
                                else
                                {
                                    addCell($"{i + 1}");
                                }
                            }
                            addCell(group.Teams[i].Title);
                            if (!group.IsAdvanced)
                            {
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                        rowCounter++;
                        columnCounter = 1;
                    }
                    rowCounter++;
                    columnCounter = 1;
                }
            }
            else
            {
                foreach (var stage in rLeague.Stages)
                {
                    var groups = stage.Groups;
                    if (groups.Count() > 0 && groups.All(g => g.IsAdvanced))
                    {
                        if (groups.All(g => !g.Teams.Any()))
                        {
                            continue;
                        }
                        var firstGroup = groups.FirstOrDefault();
                        if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                        {
                            int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                            switch (numOfBrackets)
                            {
                                case 1:
                                    addCell(Messages.Final);
                                    break;
                                case 2:
                                    addCell(Messages.Semifinals);
                                    break;
                                case 4:
                                    addCell(Messages.Quarter_finals);
                                    break;
                                case 8:
                                    addCell(Messages.Final_Eighth);
                                    break;
                                default:
                                    addCell($"{numOfBrackets * 2} {Messages.FinalNumber}");
                                    break;
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                    }
                    else
                    {
                        addCell($"{Messages.Stage} {stage.Number}");
                        rowCounter++;
                        columnCounter = 1;
                    }
                    foreach (var group in stage.Groups)
                    {
                        if (!group.IsAdvanced)
                        {
                            addCell(group.Title);
                            rowCounter++;
                            columnCounter = 1;
                        }
                        addCell("#");
                        addCell(Messages.Team);
                        if (!group.IsAdvanced)
                        {
                            addCell(Messages.Matches);
                            addCell(Messages.Points);
                            addCell(Messages.WinsNum);
                            addCell(Messages.LossNum);
                            addCell(Messages.Tie);
                            addCell($"{Messages.Sets} +");
                            addCell($"{Messages.Sets} -");
                            addCell($"{Messages.Gaming} +");
                            addCell($"{Messages.Gaming} -");
                            //addCell(Messages.Penalty);
                        }
                        rowCounter++;
                        columnCounter = 1;

                        for (var i = 0; i < group.Teams.Count(); i++)
                        {
                            if (group.IsAdvanced)
                            {
                                int numOfBrackets = (int)group.PlayoffBrackets;
                                if (i % ((numOfBrackets)) == 0)
                                {
                                    addCell($"{i + 1}");
                                }
                                else
                                {
                                    addCell("-");
                                }
                            }
                            else
                            {
                                addCell($"{i + 1}");
                            }
                            addCell(group.Teams[i].Title);
                            if (!group.IsAdvanced)
                            {
                                addCell($"{group.Teams[i].TennisInfo.Matches}");
                                addCell($"{group.Teams[i].TennisInfo.Points}");
                                addCell($"{group.Teams[i].TennisInfo.Wins}");
                                addCell($"{group.Teams[i].TennisInfo.Lost}");
                                addCell($"{group.Teams[i].TennisInfo.Ties}");
                                addCell($"{group.Teams[i].TennisInfo.PlayersSetsWon}");
                                addCell($"{group.Teams[i].TennisInfo.PlayersSetsLost}");
                                addCell($"{group.Teams[i].TennisInfo.PlayersGamingWon}");
                                addCell($"{group.Teams[i].TennisInfo.PlayersGamingLost}");
                                //addCell(Messages.Penalty);

                            }
                            rowCounter++;
                            columnCounter = 1;
                        }

                        rowCounter++;

                        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().Take(group.ExtendedTables.Count).ToArray();
                        int charIndex = 0;

                        addCell(string.Empty);
                        addCell(Messages.Team);

                        for (var i = 0; i < alpha.Length; i++)
                        {
                            addCell($"{alpha[i]}");
                        }

                        rowCounter++;
                        columnCounter = 1;

                        foreach (var team in group.ExtendedTables)
                        {
                            addCell($"{alpha[charIndex++]}");
                            addCell(team.TeamName);

                            for (var i = 0; i < group.ExtendedTables.Count; i++)
                            {
                                if (group.ExtendedTables[i].TeamId == team.TeamId)
                                {
                                    addCell(string.Empty);
                                }
                                else
                                {
                                    var text = string.Empty;
                                    foreach (var score in team.Scores.Where(x => x.OpponentTeamId == group.ExtendedTables[i].TeamId))
                                    {
                                        text += isHeb ? $"{score.OpponentScore} : {score.TeamScore}"
                                            : $"{score.TeamScore} : {score.OpponentScore}";
                                    }
                                    ws.Cell(rowCounter, columnCounter).SetValue(Convert.ToString(text));
                                    columnCounter++;
                                }

                            }

                            rowCounter++;
                            columnCounter = 1;
                        }
                        rowCounter++;
                        columnCounter = 1;
                    }
                    rowCounter++;
                    columnCounter = 1;
                }
            }
        }

        public void CreateDefaultExcel(IXLWorksheet ws, RankLeague rLeague, bool isHeb)
        {

            var columnCounter = 1;
            var rowCounter = 1;

            var addCell = new Action<string>(value =>
            {
                ws.Cell(rowCounter, columnCounter).Value = value;
                columnCounter++;
            });

            if (rLeague.Teams.Count > 0)
            {
                addCell("#");
                addCell(Messages.Team);
                addCell(Messages.GamesNum);
                addCell(Messages.WinsNum);
                addCell(Messages.LossNum);
                addCell(Messages.Acts);
                addCell(Messages.ActsRatio);
                addCell(Messages.Points);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                foreach (var row in rLeague.Teams)
                {
                    addCell("-");
                    addCell(row.Title);
                    addCell("0");
                    addCell("0");
                    addCell("0");
                    addCell("0");
                    addCell("0");
                    addCell("0");

                    rowCounter++;
                    columnCounter = 1;
                }
            }
            else if (rLeague.IsEmptyRankTable)
            {
                foreach (var stage in rLeague.Stages.Where(x => x.Groups.Count > 0))
                {
                    var groups = stage.Groups;
                    if (groups.Count() > 0 && groups.All(g => g.IsAdvanced))
                    {
                        if (groups.All(g => !g.Teams.Any()))
                        {
                            continue;
                        }
                        var firstGroup = groups.FirstOrDefault();
                        if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                        {
                            int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                            switch (numOfBrackets)
                            {
                                case 1:
                                    addCell(Messages.Final);
                                    break;
                                case 2:
                                    addCell(Messages.Semifinals);
                                    break;
                                case 4:
                                    addCell(Messages.Quarter_finals);
                                    break;
                                case 8:
                                    addCell(Messages.Final_Eighth);
                                    break;
                                default:
                                    addCell($"{numOfBrackets * 2} {Messages.FinalNumber}");
                                    break;
                            }

                            rowCounter++;
                            columnCounter = 1;
                        }
                    }
                    else
                    {
                        addCell($"{Messages.Stage} {stage.Number}");
                        rowCounter++;
                        columnCounter = 1;
                    }
                    foreach (var group in stage.Groups)
                    {
                        if (!group.IsAdvanced)
                        {
                            addCell(group.Title);
                            rowCounter++;
                            columnCounter = 1;
                        }

                        addCell("#");
                        addCell(Messages.Team);
                        if (!group.IsAdvanced)
                        {
                            addCell(Messages.GamesNum);
                            addCell(Messages.WinsNum);
                            addCell(Messages.LossNum);
                            addCell(Messages.Acts);
                            addCell(Messages.ActsRatio);
                            addCell(Messages.Points);
                            rowCounter++;
                            columnCounter = 1;
                        }
                        rowCounter++;
                        columnCounter = 1;
                        for (var i = 0; i < group.Teams.Count(); i++)
                        {
                            if (group.IsAdvanced)
                            {
                                int numOfBrackets = (int)group.PlayoffBrackets;
                                if (i % ((numOfBrackets)) == 0)
                                {
                                    addCell($"{i + 1}");
                                }
                                else
                                {
                                    addCell("-");
                                }
                            }
                            else
                            {
                                if (i != 0 && group.Teams[i].Position == group.Teams[i - 1].Position)
                                {
                                    addCell("-");
                                }
                                else
                                {
                                    addCell($"{i + 1}");
                                }
                            }
                            addCell($"{group.Teams[i].Title}");
                            if (!group.IsAdvanced)
                            {
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                                addCell("0");
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                        rowCounter++;
                        columnCounter = 1;
                    }
                    rowCounter++;
                    columnCounter = 1;
                }
            }
            else
            {
                foreach (var stage in rLeague.Stages)
                {
                    var groups = stage.Groups;
                    if (groups.Count() > 0 && groups.All(g => g.IsAdvanced))
                    {
                        if (groups.All(g => !g.Teams.Any()))
                        {
                            continue;
                        }
                        var firstGroup = groups.FirstOrDefault();
                        if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                        {
                            int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                            switch (numOfBrackets)
                            {
                                case 1:
                                    addCell(Messages.Final);
                                    break;
                                case 2:
                                    addCell(Messages.Semifinals);
                                    break;
                                case 4:
                                    addCell(Messages.Quarter_finals);
                                    break;
                                case 8:
                                    addCell(Messages.Final_Eighth);
                                    break;
                                default:
                                    addCell($"{numOfBrackets * 2} {Messages.FinalNumber}");
                                    break;
                            }

                            rowCounter++;
                            columnCounter = 1;
                        }
                    }
                    else
                    {
                        addCell($"{Messages.Stage} {stage.Number}");

                        rowCounter++;
                        columnCounter = 1;
                    }
                    foreach (var group in stage.Groups)
                    {
                        if (!group.IsAdvanced)
                        {
                            addCell(group.Title);
                            rowCounter++;
                            columnCounter = 1;
                        }

                        addCell("#");
                        addCell(Messages.Team);
                        if (!group.IsAdvanced)
                        {
                            addCell(Messages.GamesNum);
                            addCell(Messages.WinsNum);
                            addCell(Messages.LossNum);
                            addCell(Messages.Acts);
                            addCell(Messages.ActsRatio);
                            addCell(Messages.Points);
                        }
                        rowCounter++;
                        columnCounter = 1;
                        for (var i = 0; i < group.Teams.Count(); i++)
                        {
                            if (group.IsAdvanced)
                            {
                                int numOfBrackets = (int)group.PlayoffBrackets;
                                if (i % ((numOfBrackets)) == 0)
                                {
                                    addCell($"{i + 1}");
                                }
                                else
                                {
                                    addCell("-");
                                }
                            }
                            else
                            {
                                if (i != 0 && group.Teams[i].Position == group.Teams[i - 1].Position)
                                {
                                    addCell("-");
                                }
                                else
                                {
                                    addCell($"{i + 1}");
                                }
                            }
                            addCell($"{group.Teams[i].Title}");
                            if (!group.IsAdvanced)
                            {
                                addCell($"{group.Teams[i].Games}");
                                addCell($"{group.Teams[i].Wins}");
                                addCell($"{group.Teams[i].Loses}");
                                string gameResult = isHeb ? $"{group.Teams[i].SetsLost} - {group.Teams[i].SetsWon}"
                                    : $"{group.Teams[i].SetsWon} - {group.Teams[i].SetsLost}";
                                ws.Cell(rowCounter, columnCounter).SetValue(Convert.ToString(gameResult));
                                columnCounter++;
                                addCell($"{group.Teams[i].SetsRatio}");
                                addCell($"{group.Teams[i].Points}");
                            }
                            rowCounter++;
                            columnCounter = 1;
                        }
                        rowCounter++;
                        columnCounter = 1;
                    }
                    rowCounter++;
                    columnCounter = 1;
                }
            }
        }
    }
}
