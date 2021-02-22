using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using AppModel;
using DataService.DTO;
using DataService.Services;
using DataService.Utils;

namespace DataService
{
    public class UnionsRepo : BaseRepo
    {
        public UnionsRepo() : base() { }
        public UnionsRepo(DataEntities db) : base(db) { }

        public IEnumerable<Union> GetBySection(int sectionId)
        {
            return db.Unions.Where(t => t.SectionId == sectionId && t.IsArchive == false)
                .OrderBy(t => t.Name)
                .ToList();
        }

        public Union GetById(int id)
        {
            return db.Unions.Find(id);
        }

        public bool GetHandicapValueByUnionId(int id)
        {
            return db.Unions.Where(u => u.UnionId == id).Select(u => u.IsHadicapEnabled).SingleOrDefault();
        }

        public void Create(Union item)
        {
            db.Unions.Add(item);
        }

        public Section GetSectionByUnionId(int unionId)
        {
            var section = db.Unions.Include(x => x.Section).FirstOrDefault(x => x.UnionId == unionId);
            if (section != null)
                return section.Section;
            return new Section();
        }

        public UnionsDoc GetTermsDoc(int unionId)
        {
            return db.UnionsDocs.FirstOrDefault(x => x.UnionId == unionId && !x.IsArchive);
        }

        public UnionsDoc GetDocById(int docId)
        {
            return db.UnionsDocs.FirstOrDefault(x => x.DocId == docId && !x.IsArchive);
        }

        public void CreateDoc(UnionsDoc doc)
        {
            db.UnionsDocs.Add(doc);
        }

        public void RemoveDoc(int docId)
        {
            var doc = db.UnionsDocs.FirstOrDefault(x => x.DocId == docId && !x.IsArchive);

            if (doc != null)
            {
                doc.IsArchive = true;

                db.SaveChanges();
            }
        }

        public List<Union> GetByManagerId(int managerId)
        {
            return db.UsersJobs
                .Where(j => j.UserId == managerId)
                .Select(j => j.Union)
                .Where(u => u != null)
                .Distinct()
                .OrderBy(u => u.Name)
                .ToList();
        }

        private string CreateTeamTitle(Team team, int? seasonId)
        {
            var leagueTitles = team.LeagueTeams.Where(x => x.SeasonId == seasonId).Select(l => l.Leagues.Name).ToList();

            var teamName = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)?.TeamName ?? team.Title;

            return $"{teamName} ({string.Join(", ", leagueTitles)})";
        }

        public IEnumerable<ListItemDto> FindTeamsByNameAndSection(string name, int? sectionId, int num, int? unionId, int? seasonId)
        {
            var filteredTeams = db.Teams.Include(t => t.TeamsDetails)
               .Where(t => t.IsArchive == false
                           && (t.Title.Contains(name)
                               || t.TeamsDetails.Any(td => td.TeamName.Contains(name) && td.SeasonId == seasonId))
                           && (!sectionId.HasValue
                               || t.LeagueTeams.Any(lt => lt.SeasonId == seasonId && (lt.Leagues.Union.SectionId == sectionId
                                                          || t.ClubTeams.Any(ct => ct.Club.Union.SectionId == sectionId && ct.SeasonId == seasonId)
                                                          || t.ClubTeams.Any(ct => ct.Club.SectionId == sectionId && ct.SeasonId == seasonId))))
               );
            if (unionId.HasValue)
            {
                var unionLeagues = db.Leagues.Where(c => c.UnionId == unionId && (seasonId == null || c.SeasonId == seasonId)).ToList();
                var unionClubs = db.Clubs.Where(c => c.UnionId == unionId && (seasonId == null || c.SeasonId == seasonId));
                var unionTeamsIds = new List<int>();

                foreach (var league in unionLeagues)
                {
                    unionTeamsIds.AddRange(league.LeagueTeams.Select(c => c.TeamId));
                }

                foreach (var club in unionClubs)
                {
                    unionTeamsIds.AddRange(club.ClubTeams.Select(c => c.TeamId));
                }

                filteredTeams = filteredTeams.Where(t => unionTeamsIds.Contains(t.TeamId));
            }


            var teams = filteredTeams
                .OrderBy(t => t.Title)
                .Take(num).ToList();

            var dtos = new List<ListItemDto>();
            if (teams.Count > 0)
            {
                foreach (var team in teams)
                {
                    var teamName = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)?.TeamName ??
                                   team.Title;

                    foreach (var leagueTeam in team.LeagueTeams.Where(x => seasonId == null || x.SeasonId == seasonId))
                    {
                        dtos.Add(new ListItemDto
                        {
                            Id = team.TeamId,
                            SeasonId = leagueTeam.SeasonId,
                            LeagueId = leagueTeam.LeagueId,
                            Name = $"{teamName} - {leagueTeam.Leagues.Name} - {leagueTeam.Season.Name}"
                        });
                    }
                }
            }

            return dtos;
        }

        public IEnumerable<KarateUnionPayment> GetKaratePaymentsSettings(int id, int? seasonId)
        {
            return db.KarateUnionPayments.Where(k => k.UnionId == id && k.SeasonId == seasonId);
        }

        public IEnumerable<UnionForm> GetUnionForms(int unionId, int? seasonId)
        {
            return db.UnionForms.Where(u => u.UnionId == unionId && u.SeasonId == seasonId && u.IsDeleted == false);
        }

        public void CreateUnionForm(UnionForm form)
        {
            db.UnionForms.Add(form);
            db.SaveChanges();
        }

        public void DeleteUnionForm(int formId)
        {
            db.UnionForms.Find(formId).IsDeleted = true;
            db.SaveChanges();
        }

        public void ChangeDistanceSettings(int id, string type)
        {
            var union = db.Unions.FirstOrDefault(u => u.UnionId == id);
            if (union != null)
            {
                union.DistanceSettings = type;
                db.SaveChanges();
            }
        }

        public void ChangeReportSettings(int id, string type, bool removeTravel)
        {
            var union = db.Unions.FirstOrDefault(u => u.UnionId == id);
            if (union != null)
            {
                union.ReportSettings = type;
                union.ReportRemoveTravelDistance = removeTravel;
                db.SaveChanges();
            }
        }

        public void ChangeTariffSettings(int id, bool enableTariff, DateTime? fromTime, DateTime? toTime)
        {
            var union = db.Unions.FirstOrDefault(u => u.UnionId == id);
            if (union != null)
            {
                union.SaturdaysTariff = enableTariff;
                union.SaturdaysTariffFromTime = fromTime;
                union.SaturdaysTariffToTime = toTime;
                db.SaveChanges();
            }
        }

        public void UpdateKarateUnionSettings(Union item, IEnumerable<KarateUnionPaymentForm> settings, int? seasonId)
        {
            var karateUnionSettings = item?.KarateUnionPayments;
            if (karateUnionSettings != null && karateUnionSettings.Any())
            {
                db.KarateUnionPayments.RemoveRange(karateUnionSettings);
            }
            if (settings != null && settings.Any())
            {
                foreach (var setting in settings)
                {
                    if (seasonId.HasValue)
                    {
                        db.KarateUnionPayments.Add(new KarateUnionPayment
                        {
                            UnionId = item.UnionId,
                            SeasonId = seasonId.Value,
                            FromNumber = setting.FromSportNumber,
                            ToNumber = setting.ToSportNumber,
                            Price = setting.PricePerSportsman
                        });
                    }
                }
            }

        }

        public void SetIsShownPaymentStatus(int id, int userId)
        {
            db.DisplayedPaymentMessages.Add(new DisplayedPaymentMessage
            {
                PaymentId = id,
                UserId = userId,
            });
        }

        public List<ClubTeam> GetAllTrainingTeams(int unionId, int seasonId)
        {
            return db.ClubTeams
                .Where(x => !x.Club.IsArchive &&
                            !x.Team.IsArchive &&
                            x.Club.UnionId.HasValue &&
                            x.Club.UnionId == unionId &&
                            x.Club.SeasonId == seasonId &&
                            x.IsTrainingTeam &&
                            !x.IsBlocked)
                .ToList();
        }

        public List<ClubTeam> GetAllTeamsByUnionId(int unionId, int seasonId)
        {
            return db.ClubTeams
                .Where(x => !x.Club.IsArchive &&
                            !x.Team.IsArchive &&
                            x.Club.UnionId.HasValue &&
                            x.Club.UnionId == unionId &&
                            x.Club.SeasonId == seasonId &&
                            !x.IsBlocked)
                .ToList();
        }

        public void CreateNewRecord(string name, int format, int unionId)
        {
            db.DisciplineRecords.Add(new DisciplineRecord { Name = name, Format = format, UnionId = unionId});
            Save();
        }

        public void ChangeDisciplineRelatedToRecord(int disciplineId, int recordId, bool isChecked)
        {
            var record = db.DisciplineRecords.FirstOrDefault(r => r.Id == recordId);
            if (record != null)
            {
                if (isChecked)
                {
                    record.AddDisciplineId(disciplineId);
                }
                else 
                {
                    record.RemoveDisciplineId(disciplineId);
                }
                Save();
            }
        }

        public void ChangeCategoryRelatedToRecord(int categoryId, int recordId, bool isChecked)
        {
            var record = db.DisciplineRecords.FirstOrDefault(d => d.Id == recordId);
            if (record != null)
            {
                if (isChecked)
                {
                    record.AddCategoryId(categoryId);
                }
                else 
                {
                    record.RemoveCategoryId(categoryId);
                }
                Save();
            }
        }
        public void RemoveRecord(int id)
        {
            var record = db.DisciplineRecords.FirstOrDefault(d => d.Id == id);
            if (record != null)
            {
                db.DisciplineRecords.Remove(record);
                Save();
            }
        }

        public void EditRecordBests(int recordId, string intentionalIsraeliRecord, string israeliRecord, string seasonRecord, int seasonId)
        {
            var record = db.DisciplineRecords.FirstOrDefault(d => d.Id == recordId);
            if (record != null)
            {
                record.IntentionalIsraeliRecord = intentionalIsraeliRecord;
                record.IsraeliRecord = israeliRecord;
                var seasonRecordFromDb = record.SeasonRecords.FirstOrDefault(x => x.SeasonId == seasonId);
                if (seasonRecordFromDb != null)
                {
                    seasonRecordFromDb.SeasonRecord1 = seasonRecord;
                    seasonRecordFromDb.SeasonRecordSortValue = Convert.ToInt64(ConvertHelper.BuildResultSortValue(seasonRecord, record.Format));
                }
                else
                {
                    db.SeasonRecords.Add(new SeasonRecord
                    {
                        DisciplineRecordsId = recordId,
                        SeasonId = seasonId,
                        SeasonRecord1 = seasonRecord,
                        SeasonRecordSortValue = Convert.ToInt64(ConvertHelper.BuildResultSortValue(seasonRecord, record.Format))
                });
                }
                record.IntentionalIsraeliRecordSortValue = Convert.ToInt64(ConvertHelper.BuildResultSortValue(intentionalIsraeliRecord, record.Format));
                record.IsraeliRecordSortValue = Convert.ToInt64(ConvertHelper.BuildResultSortValue(israeliRecord, record.Format));

                Save();
            }
        }

        public IEnumerable<EventDTO> GetAllUnionEvents(int unionId, bool onlyPublished = false)
        {
            var unionEvents = db.Unions.Find(unionId)?.Events;


            if (unionEvents != null && unionEvents.Count > 0)
            {
                if (onlyPublished) unionEvents = unionEvents.Where(x => x.IsPublished == true).ToList();
                return unionEvents.Select(e => new EventDTO
                {                    
                    EventTime = e.EventTime,
                    Place = e.Place,
                    Title = e.Title,
                    UnionId = unionId,
                    EventDescription = e.EventDescription,
                    EventImage = e.EventImage
                });
            }
            return new List<EventDTO>();
        }

        public List<MedicalInstitute> GetMedicalInstitutes(int? unionId, int? seasonId)
        {
            return db.MedicalInstitutes.Where(x=> x.SeasonId == seasonId && x.UnionId == unionId).ToList();
        }

        public void CreateMedicalInstitute(MedicalInstitute item)
        {
            db.MedicalInstitutes.Add(item);
        }

        public MedicalInstitute GetMedicalInstituteById(int id)
        {
            return db.MedicalInstitutes.Find(id);
        }

        public void DeleteMedicalInstituteById(int id)
        {
            var record = db.MedicalInstitutes.FirstOrDefault(d => d.MedicalInstitutesId == id);
            if (record != null)
            {
                db.MedicalInstitutes.Remove(record);
                Save();
            }
        }
    }
}
