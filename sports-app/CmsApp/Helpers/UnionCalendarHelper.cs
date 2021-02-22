using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataService.DTO;
using CmsApp.Models;
using Resources;
using DataService;
using AppModel;

namespace CmsApp.Helpers
{
    public class UnionCalendarHelper
    {
        private const string regex_match_arabic_hebrew = @"[\u0600-\u06FF,\u0590-\u05FF]+";
        private IEnumerable<GameDto> games;
        private IEnumerable<GameDto> competitions;
        private IEnumerable<EventDTO> events;
        private ClubsRepo clubsRepo;
        private UnionsRepo unionsRepo;
        private LeagueRepo leagueRepo;
        private List<ColorModel> colorList = new List<ColorModel>
            {
                new ColorModel { Id = 1, ColorName = "Navy", HexCode = "#000080" },
                new ColorModel { Id = 2, ColorName = "Crimson", HexCode = "#DC143C" },
                new ColorModel { Id = 3, ColorName = "Teal", HexCode = "#008080" },
                new ColorModel { Id = 4, ColorName = "Maroon", HexCode = "#800000" },
                new ColorModel { Id = 5, ColorName = "Olive", HexCode = "#808000" },
                new ColorModel { Id = 6, ColorName = "Green", HexCode = "#008000" },
                new ColorModel { Id = 7, ColorName = "Red", HexCode = "#008080FF0000" },
                new ColorModel { Id = 8, ColorName = "Blue", HexCode = "#0000FF" },
                new ColorModel { Id = 9, ColorName = "Gray", HexCode = "#808080" },
                new ColorModel { Id = 10, ColorName = "Fuchsia", HexCode = "#FF00FF" },
                new ColorModel { Id = 11, ColorName = "Purple", HexCode = "#800080" },
                new ColorModel { Id = 12, ColorName = "Aquamarine", HexCode = "#7FFFD4" },
                new ColorModel { Id = 13, ColorName = "DarkSlateGrey", HexCode = "#2F4F4F" },
                new ColorModel { Id = 14, ColorName = "DarkGoldenRod", HexCode = "#B8860B" },
                new ColorModel { Id = 15, ColorName = "DarkCyan", HexCode = "#008B8B" },
                new ColorModel { Id = 16, ColorName = "Black", HexCode = "#000000" },
                new ColorModel { Id = 17, ColorName = "Chocolate", HexCode = "#D2691E" },
                new ColorModel { Id = 18, ColorName = "MidnightBlue", HexCode = "#191970" },
                new ColorModel { Id = 19, ColorName = "PaleVioletRed", HexCode = "#DB7093" },
                new ColorModel { Id = 20, ColorName = "RebeccaPurple", HexCode = "#663399" }
            };

        public UnionCalendarHelper()
        {
            this.games = new List<GameDto>();
            this.clubsRepo = new ClubsRepo();
            this.unionsRepo = new UnionsRepo();
            this.leagueRepo = new LeagueRepo();
        }

        public UnionCalendarHelper(IEnumerable<GameDto> games)
        {
            this.games = games;
        }

        public UnionCalendarHelper(IEnumerable<GameDto> games, IEnumerable<GameDto> competitions, IEnumerable<EventDTO> events)
        {
            this.games = games;
            this.competitions = competitions;
            this.events = events;
            this.clubsRepo = new ClubsRepo();
            this.unionsRepo = new UnionsRepo();
            this.leagueRepo = new LeagueRepo();
        }

        public IEnumerable<UnionCalendarViewModel> GetCalendarObject(bool? isIndividualOrClub = false, int? unionId = null)
        {
            if (games == null && competitions == null)
                return null;
            var counter = isIndividualOrClub == false? 0 : 1;
            var color = colorList[counter];
            var calendarObject = new List<UnionCalendarViewModel>();
            if (games != null)
            {
                var gamesGroupedByDiscipline = games.GroupBy(g => g.DisciplineId).ToList();
                foreach (var groupedGame in gamesGroupedByDiscipline)
                {
                    if (isIndividualOrClub == false)
                    {
                        counter++;
                    }

                    var gamesValues = groupedGame.Select(c => c).ToList();
                    Dictionary<string, int> leagueNamesColorId = new Dictionary<string, int>();
                    foreach (var game in gamesValues)
                    {
                        var date = game.StartDate.ToString("yyyy-MM-dd");
                        var time = game.StartDate.ToString("HH:mm:ss");
                        var startDateTime = $"{date}T{time}";
                        var auditorium = String.IsNullOrWhiteSpace(game.Auditorium) ? "" : $"{Messages.In} {game.Auditorium}";
                        var description = "";
                        if (isIndividualOrClub == false)
                        {
                            description += $"{game.LeagueName} <br/>";
                        }
                        if (String.IsNullOrWhiteSpace(game.HomeTeamDetail.TeamName))
                            description += $"{Messages.HomeTeam} - {game.GuestTeamDetail.TeamName}<br/> {auditorium}";
                        else if (String.IsNullOrWhiteSpace(game.GuestTeamDetail.TeamName))
                            description += $"{game.HomeTeamDetail.TeamName} - {Messages.GuestTeam}<br/> {auditorium}";
                        else if (String.IsNullOrWhiteSpace(game.GuestTeamDetail.TeamName)
                            && String.IsNullOrWhiteSpace(game.HomeTeamDetail.TeamName))
                            description += $"{Messages.HomeTeam} - {Messages.GuestTeam}<br/> {auditorium}";
                        else
                        {
                            description += $"{game.HomeTeamDetail.TeamName} - {game.GuestTeamDetail.TeamName}<br/> {auditorium}";
                        }

                        string result = string.Empty;
                        string teamsNames = string.Empty;
                        if (isIndividualOrClub == false)
                        {
                            if (string.IsNullOrWhiteSpace(game.HomeTeamDetail.TeamName))
                                teamsNames += $"{Messages.HomeTeam} - {game.GuestTeamDetail.TeamName}";
                            else if (string.IsNullOrWhiteSpace(game.GuestTeamDetail.TeamName))
                                teamsNames += $"{game.HomeTeamDetail.TeamName} - {Messages.GuestTeam}";
                            else if (string.IsNullOrWhiteSpace(game.GuestTeamDetail.TeamName)
                                     && string.IsNullOrWhiteSpace(game.HomeTeamDetail.TeamName))
                                teamsNames += $"{Messages.HomeTeam} - {Messages.GuestTeam}";
                            else
                            {
                                teamsNames += $"{game.HomeTeamDetail.TeamName} - {game.GuestTeamDetail.TeamName}";
                            }
                            if (Regex.IsMatch(description, regex_match_arabic_hebrew))
                            {
                                result = $"{game.GuestTeamScore}:{game.HomeTeamScore}";
                                description += $"<br/> {game.GuestTeamScore}:{game.HomeTeamScore}";
                            }
                            else
                            {
                                result = $"{game.HomeTeamScore}:{game.GuestTeamScore}";
                                description += $"<br/> {game.HomeTeamScore}:{game.GuestTeamScore}";
                            }

                            int colorId;
                            string leagueName = game.LeagueName;
                            if (leagueName != null)
                            {
                                if (leagueNamesColorId.ContainsKey(leagueName))
                                {
                                    colorId = leagueNamesColorId[leagueName];
                                    color = colorList[colorId];
                                }
                                else
                                {
                                    color = counter > 19 ? colorList[1] : colorList[counter];
                                    leagueNamesColorId.Add(leagueName, counter);
                                    if (counter < 19)
                                    {
                                        counter++;
                                    }
                                    else
                                    {
                                        counter = 0;
                                    }
                                }
                            }
                        }

                        var model = new UnionCalendarViewModel
                        {
                            id = game.GameId,
                            allDay = false,
                            color = color.HexCode,
                            start = startDateTime,
                            title = game.DisciplineName,
                            description = description
                        };
                        if (isIndividualOrClub == false)
                        {
                            model.typeOfEvent = CalendarEventTypes.NEWGAME;
                            model.auditorium = game.Auditorium;
                            model.leagueName = game.LeagueName;
                            model.result = result;
                            model.teamsNames = teamsNames;
                        }
                        calendarObject.Add(model);
                    }
                    color = colorList[counter];
                }
            }
            if (competitions != null)
            {
                Dictionary<int, int> competitionTypeIdColorId = new Dictionary<int, int>();
                var sectionAlias = unionsRepo.GetSectionByUnionId(unionId ?? 0)?.Alias;
                var groupLvsC = competitions.GroupBy(x => x.isFromCategoryDates);
                foreach (var g in groupLvsC)
                {
                    var groupComp = g.GroupBy(x => x.DisciplineId);
                    foreach (var group in groupComp)
                    {
                        var compList = group.Select(x => x).ToList();
                        color = colorList[counter];
                        foreach (var comp in compList)
                        {
                            if (string.Equals(sectionAlias, GamesAlias.Athletics, StringComparison.OrdinalIgnoreCase))
                            {
                                var league = this.leagueRepo.GetById(comp.LeagueId);
                                if (competitionTypeIdColorId.ContainsKey(league.CompetitionType))
                                {
                                    color = colorList[competitionTypeIdColorId[league.CompetitionType]];
                                }
                                else
                                {
                                    counter++;
                                    color = colorList[counter];
                                    competitionTypeIdColorId.Add(league.CompetitionType, counter);
                                }
                                
                                
                            }
                            var date = comp.StartDate.ToString("yyyy-MM-dd");
                            var time = comp.StartDate.ToString("HH:mm:ss");
                            var startDateTime = $"{date}T{time}";
                            string endDateTime = null;
                            if (comp.EndDate.HasValue)
                            {
                                comp.EndDate = comp.EndDate.Value.AddDays(1);
                                var endDate = comp.EndDate.Value.ToString("yyyy-MM-dd");
                                var endTime = comp.EndDate.Value.ToString("HH:mm:ss");
                                endDateTime = $"{endDate}T{endTime}";
                            }
                            var auditorium = "";
                            //if (!string.IsNullOrEmpty(comp.Auditorium)) auditorium = comp.Auditorium;
                            if (!string.IsNullOrEmpty(comp.AuditoriumAddress)) auditorium += comp.AuditoriumAddress;
                            calendarObject.Add(new UnionCalendarViewModel
                            {
                                id = comp.LeagueId,
                                allDay = true,
                                color = color.HexCode,
                                start = startDateTime,
                                end = endDateTime,
                                title = comp.LeagueName,
                                description = "",
                                auditorium = auditorium,
                                leagueId = string.Equals(sectionAlias, GamesAlias.Athletics, StringComparison.OrdinalIgnoreCase) ? comp.LeagueId : 0,
                                typeOfEvent = CalendarEventTypes.COMPETITION
                            });
                        }
                        counter++;
                    }
                }
                
            }
            if(events != null)
            {
                color = colorList[counter];
                foreach(var ev in events)
                {
                    calendarObject.Add(GetEventValue(ev, color));
                }
            }
            return calendarObject;
        }

        public IEnumerable<UnionCalendarViewModel> GetClubsCalendarObject()
        {
            var counter = 0;
            var color = colorList[counter];
            var calendarObject = new List<UnionCalendarViewModel>();
            if (games != null)
            {
                var gamesGroupedByDiscipline = games.GroupBy(g => g.DisciplineId).ToList();
                foreach (var groupedGame in gamesGroupedByDiscipline)
                {
                    counter++;

                    var gamesValues = groupedGame.Select(c => c).ToList();
                    Dictionary<int, int> clubIdColorId = new Dictionary<int, int>();
                    foreach (var game in gamesValues)
                    {
                        var date = game.StartDate.ToString("yyyy-MM-dd");
                        var time = game.StartDate.ToString("HH:mm:ss");
                        var startDateTime = $"{date}T{time}";
                        var auditorium = String.IsNullOrWhiteSpace(game.Auditorium)
                            ? ""
                            : $"{Messages.In} {game.Auditorium}";
                        var description = "";
                        description += $"{game.LeagueName} <br/>";

                        if (String.IsNullOrWhiteSpace(game.HomeTeamDetail.TeamName))
                            description += $"{Messages.HomeTeam} - {game.GuestTeamDetail.TeamName}<br/> {auditorium}";
                        else if (String.IsNullOrWhiteSpace(game.GuestTeamDetail.TeamName))
                            description += $"{game.HomeTeamDetail.TeamName} - {Messages.GuestTeam}<br/> {auditorium}";
                        else if (String.IsNullOrWhiteSpace(game.GuestTeamDetail.TeamName)
                                 && String.IsNullOrWhiteSpace(game.HomeTeamDetail.TeamName))
                            description += $"{Messages.HomeTeam} - {Messages.GuestTeam}<br/> {auditorium}";
                        else
                        {
                            description +=
                                $"{game.HomeTeamDetail.TeamName} - {game.GuestTeamDetail.TeamName}<br/> {auditorium}";
                        }

                        string result = string.Empty;
                        string teamsNames = string.Empty;

                        if (string.IsNullOrWhiteSpace(game.HomeTeamDetail.TeamName))
                            teamsNames += $"{Messages.HomeTeam} - {game.GuestTeamDetail.TeamName}";
                        else if (string.IsNullOrWhiteSpace(game.GuestTeamDetail.TeamName))
                            teamsNames += $"{game.HomeTeamDetail.TeamName} - {Messages.GuestTeam}";
                        else if (string.IsNullOrWhiteSpace(game.GuestTeamDetail.TeamName)
                                 && string.IsNullOrWhiteSpace(game.HomeTeamDetail.TeamName))
                            teamsNames += $"{Messages.HomeTeam} - {Messages.GuestTeam}";
                        else
                        {
                            teamsNames += $"{game.HomeTeamDetail.TeamName} - {game.GuestTeamDetail.TeamName}";
                        }

                        if (Regex.IsMatch(description, regex_match_arabic_hebrew))
                        {
                            result = $"{game.GuestTeamDetail.TeamScore}:{game.HomeTeamDetail.TeamScore}";
                            description += $"<br/> {game.GuestTeamDetail.TeamScore}:{game.HomeTeamDetail.TeamScore}";
                        }
                        else
                        {
                            result = $"{game.HomeTeamDetail.TeamScore}:{game.GuestTeamDetail.TeamScore}";
                            description += $"<br/> {game.HomeTeamDetail.TeamScore}:{game.GuestTeamDetail.TeamScore}";
                        }

                        int colorId;
                        int clubTeamId = game.ClubTeamId ?? 0;
                        if (clubTeamId != null)
                        {
                            if (clubIdColorId.ContainsKey(clubTeamId))
                            {
                                colorId = clubIdColorId[clubTeamId];
                                color = colorList[colorId];
                            }
                            else
                            {
                                color = counter > 19 ? colorList[1] : colorList[counter];
                                clubIdColorId.Add(clubTeamId, counter);
                                if (counter < 19)
                                {
                                    counter++;
                                }
                                else
                                {
                                    counter = 0;
                                }
                            }


                            var model = new UnionCalendarViewModel
                            {
                                id = game.GameId,
                                allDay = false,
                                color = color.HexCode,
                                start = startDateTime,
                                description = description
                            };

                            model.typeOfEvent = CalendarEventTypes.NEWGAME;
                            model.auditorium = game.Auditorium;
                            model.leagueName = game.LeagueName;
                            model.result = result;
                            model.teamsNames = teamsNames;

                            calendarObject.Add(model);
                        }

                        color = colorList[counter];
                    }
                }

                return calendarObject;
            }
            return null;
        }

        public IEnumerable<UnionCalendarViewModel> GetDepartmentCalendarObject(int clubId)
        {
            var club = clubsRepo.GetById(clubId);
            var relatedClubs = club.RelatedClubs;

            var clubGames = new List<GameDTO>();
            var clubTrainings = new List<TrainingDTO>();
            var clubEvents = new List<EventDTO>();

            if (relatedClubs.Count > 0)
            {
                var relatedClubGames = new List<GameDTO>();
                var relatedClubTrainings = new List<TrainingDTO>();
                var relatedClubEvents = new List<EventDTO>();

                GetAllClubsEvents(clubId, ref clubGames, ref clubTrainings, ref clubEvents);

                clubGames.AddRange(relatedClubGames);
                clubTrainings.AddRange(relatedClubTrainings);
                clubEvents.AddRange(relatedClubEvents);
            }
            else
            {
                GetAllClubsEvents(clubId, ref clubGames, ref clubTrainings, ref clubEvents);
            }

            var model = GetUnionCalendarValues(clubGames, clubTrainings, clubEvents);


            return model;
        }

        private IEnumerable<UnionCalendarViewModel> GetUnionCalendarValues(List<GameDTO> clubGames, List<TrainingDTO> clubTrainings, List<EventDTO> clubEvents)
        {
            var calendarObject = new List<UnionCalendarViewModel>();

            var groupedGames = clubGames.OrderBy(c => c.ClubId).GroupBy(c => c.ClubId);
            var groupedTrainings = clubTrainings.OrderBy(c => c.ClubId).GroupBy(c => c.ClubId);
            var groupedEvents = clubEvents.OrderBy(c => c.ClubId).GroupBy(c => c.ClubId);

            var gamesColorCounter = 0;
            var trainingsColorCounter = 0;
            var eventsColorCounter = 0;

            foreach (var groupedGame in groupedGames)
            {
                var color = colorList[gamesColorCounter];
                var gameList = groupedGame.Select(c => c).ToList();

                foreach (var game in gameList)
                {
                    calendarObject.Add(GetGameValue(game, color));
                }

                gamesColorCounter++;
            }

            foreach (var groupedTraining in groupedTrainings)
            {
                var color = colorList[trainingsColorCounter];
                var trainingList = groupedTraining.Select(c => c).ToList();

                foreach (var training in trainingList)
                {
                    calendarObject.Add(GetTrainingValue(training, color));
                }

                trainingsColorCounter++;
            }

            foreach (var groupedEvent in groupedEvents)
            {
                var color = colorList[eventsColorCounter];
                var eventList = groupedEvent.Select(c => c).ToList();

                foreach (var clubEvent in eventList)
                {
                    calendarObject.Add(GetEventValue(clubEvent, color));
                }

                eventsColorCounter++;
            }

            return calendarObject;
        }

        

        private void GetAllClubsEvents(int clubId, ref List<GameDTO> clubGames, ref List<TrainingDTO> clubTrainings, ref List<EventDTO> clubEvents)
        {
            var club = clubsRepo.GetById(clubId);
            if (club.ParentClub != null)
            {
                clubGames = clubsRepo.GetAllClubGames(clubId).ToList();
                clubTrainings = clubsRepo.GetAllClubTrainings(clubId).ToList();
                clubEvents = clubsRepo.GetAllClubEvents(clubId).ToList();
            }
            else
            {
                var clubDepartments = clubsRepo.GetById(clubId)?.RelatedClubs;
                if (clubDepartments != null && clubDepartments.Count > 0)
                {
                    foreach (var department in clubDepartments)
                    {
                        clubGames.AddRange(clubsRepo.GetAllClubGames(department.ClubId));
                        clubTrainings.AddRange(clubsRepo.GetAllClubTrainings(department.ClubId));
                        clubEvents.AddRange(clubsRepo.GetAllClubEvents(department.ClubId));
                    }
                }
                clubEvents.AddRange(clubsRepo.GetAllClubEvents(club.ClubId));
            }
        }

        private UnionCalendarViewModel GetGameValue(GameDTO game, ColorModel color)
        {
            var date = game.StartDate.ToString("yyyy-MM-dd");
            var time = game.StartDate.ToString("HH:mm:ss");
            var startDateTime = $"{date}T{time}";

            var description = "";
            var auditorium = game.AuditoriumId.HasValue ? $"<br/> {Messages.In} {game.AuditoriumName}" : "";

            if (game.HomeTeamId.HasValue && !game.GuestTeamId.HasValue) // Just home team
            {
                description += $"{game.HomeTeamName} - {Messages.GuestTeam} {auditorium}";
            }
            else if (!game.HomeTeamId.HasValue && game.GuestTeamId.HasValue) // Hust gues team
            {
                description += $"{Messages.HomeTeam} - {game.GuestTeamName} {auditorium}";
            }
            else // two teams
            {
                description += $"{game.HomeTeamName} - {game.GuestTeamName} {auditorium}";
            }

            return new UnionCalendarViewModel
            {
                //id = game.GameId,
                allDay = false,
                color = color.HexCode,
                start = startDateTime,
                title = $"{game.ClubName}: {Messages.Games}",
                description = description
            };
        }

        private UnionCalendarViewModel GetTrainingValue(TrainingDTO training, ColorModel color)
        {
            var date = training.StartDate.ToString("yyyy-MM-dd");
            var time = training.StartDate.ToString("HH:mm:ss");
            var startDateTime = $"{date}T{time}";

            var auditorium = training.AuditoriumId.HasValue ? $"<br/> {Messages.In} {training.AuditoriumName}" : "";
            var description = $"{Messages.Training} {Messages.OfTeam} {training.TeamName} {auditorium}";

            return new UnionCalendarViewModel
            {
                //id = game.GameId,
                allDay = false,
                color = color.HexCode,
                start = startDateTime,
                title = $"{training.ClubName}: {Messages.Training}",
                description = description
            };
        }

        private UnionCalendarViewModel GetEventValue(EventDTO clubEvent, ColorModel color)
        {
            var date = clubEvent.EventTime.ToString("yyyy-MM-dd");
            var time = clubEvent.EventTime.ToString("HH:mm:ss");
            var startDateTime = $"{date}T{time}";

            var description = $"{Messages.Event}: {clubEvent.Title} <br/> {Messages.At} {clubEvent.Place}";

            return new UnionCalendarViewModel
            {
                //id = game.GameId,
                allDay = false,
                color = color.HexCode,
                start = startDateTime,
                title = clubEvent.UnionId.HasValue ? clubEvent.Title : $"{clubEvent.Title}: {Messages.Event}",
                description = clubEvent.UnionId.HasValue ? clubEvent.EventDescription : description,
                typeOfEvent = CalendarEventTypes.EVENT,
                eventImage = string.IsNullOrEmpty(clubEvent.EventImage) ? null : GlobVars.ContentPath + "/Events/" + clubEvent.EventImage,
                auditorium = clubEvent.Place
            };
        }
    }

    public class ColorModel
    {
        public int Id { get; set; }
        public string ColorName { get; set; }
        public string HexCode { get; set; }
    }
}