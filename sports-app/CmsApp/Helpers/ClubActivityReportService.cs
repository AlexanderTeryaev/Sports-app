using AppModel;
using ClosedXML.Excel;
using DataService;
using DataService.DTO;
using DataService.Utils;
using Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace CmsApp.Helpers
{
    public class ClubActivityReportService
    {
        private DataEntities db;
        private readonly PlayersRepo playersRepo;

        public ClubActivityReportService(DataEntities db)
        {
            this.db = db;
            playersRepo = new PlayersRepo();
        }

        public ClubActivityReportService()
        {
        }

        public List<ClubActivityDto> GetActivityReport(int unionId, int seasonId, string sectionAlias)
        {
            switch (sectionAlias)
            {
                case GamesAlias.Gymnastic:
                    return GetActivityReportForGymnastic(unionId, seasonId);
                case GamesAlias.MartialArts:
                    return GetActivityReportForMartialArts(unionId, seasonId);
                case GamesAlias.Athletics:
                    return GetActivityReportForAthletics(unionId, seasonId);
                default:
                    return new List<ClubActivityDto>();
            }
        }

        public void GenerateExcelStructure(IXLWorksheet ws, string sectionAlias, List<ClubActivityDto> clubsInformation, IEnumerable<string> disciplineNames)
        {
            switch (sectionAlias)
            {
                case GamesAlias.Gymnastic:
                    GenerateExcelStructureForGymnastic(ws, clubsInformation, disciplineNames);
                    break;
                case GamesAlias.MartialArts:
                    GenerateExcelStructureForMartialArts(ws, clubsInformation);
                    break;
                case GamesAlias.Athletics:
                    GenerateExcelStructureForMartialArts(ws, clubsInformation);
                    break;
                default:
                    break;
            }
        }

        private List<ClubActivityDto> GetActivityReportForMartialArts(int unionId, int seasonId)
        {
            var union = db.Unions.FirstOrDefault(u => u.UnionId == unionId);
            var clubsInUnion = union.Clubs.Where(c => c.SeasonId == seasonId && !c.IsArchive)?.ToList();
            var sectionAlias = union?.Section?.Alias;
            var clubResult = new List<ClubActivityDto>();

            foreach (var club in clubsInUnion)
            {
                var sporstsmenRegistrations = playersRepo.GetTeamPlayersShortByClubId(club.ClubId, seasonId, sectionAlias);
                var clubsCompetitionRegistrations = db.SportsRegistrations.AsNoTracking()
                    .Where(r => r.SeasonId == seasonId && r.IsApproved && r.ClubId == club.ClubId)
                    .ToList();
                clubResult.Add(new ClubActivityDto
                {
                    Id = club.ClubId,
                    NGONumber = club.NGO_Number,
                    ClubNumber = club.ClubNumber,
                    Name = club.Name,
                    DateOfApproval = club.DateOfClubApproval,
                    ApprovedCount = sporstsmenRegistrations.Count(r => r.IsActive == true && (r.IsPlayerRegistrationApproved || r.IsApproveChecked == true)),
                    FemaleCount = sporstsmenRegistrations.Count(reg => reg.IsFemale),
                    Active = clubsCompetitionRegistrations.GroupBy(r => r.UserId).Count(reg => (reg.Count(_ => _.IsApproved)) >= 4),
                    ThreeCompetitionsNumber = clubsCompetitionRegistrations.GroupBy(r => r.UserId).Count(reg => (reg.Count(_ => _.IsApproved)) == 3),
                    TwoCompetitionsNumber = clubsCompetitionRegistrations.GroupBy(r => r.UserId).Count(reg => (reg.Count(_ => _.IsApproved)) == 2),
                    OneCompetitionNumber = clubsCompetitionRegistrations.GroupBy(r => r.UserId).Count(reg => (reg.Count(_ => _.IsApproved)) == 1)
                });
            }
            return clubResult;
        }


        private List<ClubActivityDto> GetActivityReportForAthletics(int unionId, int seasonId)
        {
            var union = db.Unions.FirstOrDefault(u => u.UnionId == unionId);
            var clubsInUnion = union.Clubs
                .Where(c => c.SeasonId == seasonId && !c.IsArchive)
                .ToList();
            var sectionAlias = union?.Section?.Alias;
            var clubResult = new List<ClubActivityDto>();

            foreach (var club in clubsInUnion)
            {
                var sporstsmenRegistrations = playersRepo.GetTeamPlayersShortByClubId(club.ClubId, seasonId, sectionAlias);
                var clubsCompetitionRegistrations = db.CompetitionDisciplineRegistrations.AsNoTracking()
                    .Where(r =>  r.ClubId == club.ClubId &&
                                !r.CompetitionDiscipline.IsDeleted &&
                                !r.CompetitionDiscipline.League.IsArchive &&
                                r.CompetitionDiscipline.League.SeasonId == seasonId &&
                                !r.IsArchive &&
                                r.CompetitionResult.FirstOrDefault() != null &&
                                r.CompetitionResult.FirstOrDefault().Result != "")
                    .ToList();

                clubResult.Add(new ClubActivityDto
                {
                    Id = club.ClubId,
                    NGONumber = club.NGO_Number,
                    ClubNumber = club.ClubNumber,
                    Name = club.Name,
                    DateOfApproval = club.DateOfClubApproval,
                    ApprovedCount = sporstsmenRegistrations.Count(r => r.IsActive == true && (r.IsPlayerRegistrationApproved || r.IsApproveChecked == true)),
                    FemaleCount = sporstsmenRegistrations.Count(r => r.IsFemale && r.IsActive == true && (r.IsPlayerRegistrationApproved || r.IsApproveChecked == true)),
                    Active = clubsCompetitionRegistrations.GroupBy(r => r.UserId).Count(reg => (reg.GroupBy(rr => rr.CompetitionDiscipline.CompetitionId).Count()) >= 4),
                    ThreeCompetitionsNumber = clubsCompetitionRegistrations.GroupBy(r => r.UserId).Count(reg => (reg.GroupBy(rr => rr.CompetitionDiscipline.CompetitionId).Count()) == 3),
                    TwoCompetitionsNumber = clubsCompetitionRegistrations.GroupBy(r => r.UserId).Count(reg => (reg.GroupBy(rr => rr.CompetitionDiscipline.CompetitionId).Count()) == 2),
                    OneCompetitionNumber = clubsCompetitionRegistrations.GroupBy(r => r.UserId).Count(reg => (reg.GroupBy(rr => rr.CompetitionDiscipline.CompetitionId).Count()) == 1)
                });
            }
            return clubResult.OrderBy(c => c.Name).ToList();
        }


        public List<ClubActivityDto> GetActivityReportForGymnastic(int unionId, int seasonId)
        {
            var clubsInUnion = db.Clubs.Where(c => !c.IsArchive && c.UnionId == unionId && c.SeasonId == seasonId)?.ToList();
            var unionDisciplines = db.Disciplines.Where(d => d.UnionId == unionId)?.ToList();
            var clubResult = new List<ClubActivityDto>();
            var prefferedDisciplinesIds = db.Disciplines.Where(d => d.IsPreffered)?.Select(d => d.DisciplineId)?.AsEnumerable();

            foreach (var club in clubsInUnion)
            {
                var teamPlayers = club.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && !tp.User.IsArchive && tp.IsActive)
                    ?.GroupBy(g => g.UserId)?.Select(c => c.First())?.ToList();

                var approvedGymnastics = teamPlayers.Where(g => !g.User.IsArchive && g.IsActive && g.IsApprovedByManager == true).ToList();
                var waitingGymnastics = teamPlayers.Where(g => !g.User.IsArchive && g.IsActive && g.IsApprovedByManager == false)?.ToList();

                clubResult.Add(new ClubActivityDto
                {
                    Id = club.ClubId,
                    NGONumber = club.NGO_Number,
                    ClubNumber = club.ClubNumber,
                    Name = club.Name,
                    DateOfApproval = club.InitialDateOfClubApproval,
                    ApprovedCount = approvedGymnastics.Count,
                    WaitingCount = approvedGymnastics.Count,
                    FemaleCount = approvedGymnastics.Count(gym => gym.User.GenderId == 0),
                    TotalForVotes = approvedGymnastics.Count(gym =>
                        (gym.User.BirthDay.GetAge() ?? 0) > 10 &&
                         gym.User.CompetitionRegistrations.Count(t => !t.League.IsArchive && t.SeasonId == seasonId) >= 2 &&
                         ApprovedAboveOneSeason(gym.User.InitialApprovalDates)),
                    Active = CountGymnasticsActiveRegs(approvedGymnastics),
                    TotalForPreffered = CountTotalForPreffered(approvedGymnastics, prefferedDisciplinesIds),
                    TotalForNonPreffered = CountTotalForNonPreffered(approvedGymnastics, prefferedDisciplinesIds),
                    DisciplinesInformation = GetResultByDisciplines(unionDisciplines, teamPlayers, club.ClubId, seasonId)
                });
            }
            return clubResult;
        }

        private int CountGymnasticsActiveRegs(List<TeamsPlayer> teamPlayers)
        {
            var result = 0;
            foreach (var tp in teamPlayers)
            {
                var gymnasticCompetitionCount = tp.User.CompetitionRegistrations
                    .Where(c => c.SeasonId == tp.SeasonId &&
                                !c.League.IsArchive &&
                                (c.FinalScore.HasValue || c.Position.HasValue))
                    .GroupBy(r => r.LeagueId)
                    .Select(r => r.First())
                    .Count();

                var otherCompetitionCount = tp.User.SportsRegistrations
                    .Where(c => c.SeasonId == tp.SeasonId &&
                                !c.League.IsArchive &&
                                c.IsApproved &&
                                (c.FinalScore.HasValue || c.Position.HasValue))
                    .GroupBy(r => r.LeagueId)
                    .Select(r => r.First())
                    .Count();

                var competitionCount = gymnasticCompetitionCount + otherCompetitionCount;

                if (competitionCount >= 4) result++;
            }

            return result;
        }

        private List<DisciplineDTO> GetResultByDisciplines(List<Discipline> disciplines, List<TeamsPlayer> teamsPlayers, int clubId, int seasonId)
        {
            var result = new List<DisciplineDTO>();
            foreach (var discipline in disciplines)
            {
                result.Add(new DisciplineDTO
                {
                    DisciplineId = discipline.DisciplineId,
                    Name = discipline.Name,
                    PlayersCount = teamsPlayers.Sum(t => t.User.PlayerDisciplines
                        .Count(d => d.ClubId == clubId && d.SeasonId == seasonId && d.DisciplineId == discipline.DisciplineId))
                });
            }
            return result;
        }

        private int CountTotalForNonPreffered(List<TeamsPlayer> approvedGymnastics, IEnumerable<int> prefferedDisciplinesIds)
        {
            var count = 0;
            foreach (var gym in approvedGymnastics)
            {
                var disciplinesRoutes = gym?.User?.UsersRoutes;
                if (gym?.User?.CompetitionRegistrations?.Any(x => x.SeasonId == gym.SeasonId) == true)
                {
                    foreach (var discipline in disciplinesRoutes)
                    {
                        if (!prefferedDisciplinesIds.Contains(discipline.DisciplineRoute.DisciplineId))
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        private int CountTotalForPreffered(List<TeamsPlayer> approvedGymnastics, IEnumerable<int> prefferedDisciplinesIds)
        {
            var count = 0;
            foreach (var gym in approvedGymnastics)
            {
                var disciplinesRoutes = gym?.User?.UsersRoutes;
                if (gym?.User?.CompetitionRegistrations.Any(x => x.SeasonId == gym.SeasonId) == true)
                {
                    foreach (var discipline in disciplinesRoutes)
                    {
                        if (prefferedDisciplinesIds.Contains(discipline.DisciplineRoute.DisciplineId))
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        private bool ApprovedAboveOneSeason(ICollection<InitialApprovalDate> initialApprovalDates)
        {
            var approvalDate = initialApprovalDates.FirstOrDefault()?.InitialApprovalDate1;
            var seasonsCount = approvalDate.HasValue ? DateTime.Now.Year - approvalDate.Value.Year : 0;
            return seasonsCount > 1;
        }

        private void GenerateExcelStructureForGymnastic(IXLWorksheet ws, List<ClubActivityDto> clubsInformation, IEnumerable<string> disciplineNames)
        {
            var columnCounter = 1;
            var rowCounter = 1;
            var addCell = new Action<object>(value =>
            {
                ws.Cell(rowCounter, columnCounter).Value = value;
                columnCounter++;
            });

            rowCounter++;
            columnCounter = 1;

            addCell(Messages.Name);
            addCell(Messages.NGO_Number);
            addCell(Messages.ClubNumber);
            addCell(Messages.DateOfClubApproval);

            foreach (var disciplineName in disciplineNames)
                addCell(disciplineName);

            addCell(Messages.Approved);
            addCell(Messages.Female);
            addCell(Messages.TotalForVotes);
            addCell(Messages.ActivePlayers);
            addCell(Messages.TotalInPreffered);
            addCell(Messages.TotalInNonPreffered);

            ws.Columns().AdjustToContents();

            rowCounter++;
            columnCounter = 1;

            foreach (var row in clubsInformation)
            {
                addCell(row.Name);
                addCell(row.NGONumber);
                addCell(row.ClubNumber);
                addCell(row.DateOfApproval?.ToShortDateString());

                foreach (var discipline in row.DisciplinesInformation)
                    addCell(discipline.PlayersCount);

                addCell(row.ApprovedCount);
                addCell(row.FemaleCount);
                addCell(row.TotalForVotes);
                addCell(row.Active);
                addCell(row.TotalForPreffered);
                addCell(row.TotalForNonPreffered);

                ws.Columns().AdjustToContents();


                ws.Columns().AdjustToContents();

                rowCounter++;
                columnCounter = 1;
            }
        }

        private void GenerateExcelStructureForMartialArts(IXLWorksheet ws, List<ClubActivityDto> clubsInformation)
        {
            var columnCounter = 1;
            var rowCounter = 1;
            var addCell = new Action<object>(value =>
            {
                ws.Cell(rowCounter, columnCounter).Value = value;
                columnCounter++;
            });

            rowCounter++;
            columnCounter = 1;

            addCell(Messages.Name);
            addCell(Messages.NGO_Number);
            addCell(Messages.ClubNumber);
            addCell(Messages.DateOfClubApproval);

            addCell(Messages.Approved);
            addCell(Messages.Female);
            addCell(Messages.ActivePlayers);
            addCell($"3 {Messages.Competitions.ToLowerInvariant()}");
            addCell($"2 {Messages.Competitions.ToLowerInvariant()}");
            addCell($"1 {Messages.Competition.ToLowerInvariant()}");

            ws.Columns().AdjustToContents();

            rowCounter++;
            columnCounter = 1;

            foreach (var row in clubsInformation)
            {
                addCell(row.Name);
                addCell(row.NGONumber);
                addCell(row.ClubNumber);
                addCell(row.DateOfApproval?.ToShortDateString());

                addCell(row.ApprovedCount);
                addCell(row.FemaleCount);
                addCell(row.Active);
                addCell(row.ThreeCompetitionsNumber);
                addCell(row.TwoCompetitionsNumber);
                addCell(row.OneCompetitionNumber);

                ws.Columns().AdjustToContents();

                rowCounter++;
                columnCounter = 1;
            }
        }
    }
}