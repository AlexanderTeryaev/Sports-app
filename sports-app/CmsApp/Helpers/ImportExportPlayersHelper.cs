using AppModel;
using ClosedXML.Excel;
using DataService;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CmsApp.Helpers.ActivityHelpers;
using DataService.DTO;
using Newtonsoft.Json;
using CmsApp.Models;

namespace CmsApp.Helpers
{
    public class ImportExportPlayersHelper
    {
        private Dictionary<string, int> _allowGenderValue = null;

        private TeamsRepo _teamRepo = null;
        private PlayersRepo _playersRepo = null;
        private UsersRepo _usersRepo;
        private ClubsRepo _clubsRepo = null;
        private LeagueRepo _leagueRepo = null;
        private DisciplinesRepo _disciplinesRepo = null;
        private FriendshipTypesRepo _friendshipTypesRepo = null;
        private SeasonsRepo _seasonsRepo = null;

        public DataEntities _db;
        private Dictionary<string, string> _genderMap = new Dictionary<string, string>();

        //for bicycle 
        private List<FriendshipsType> friendships = null;
        private List<Discipline> roadDisciplines = null;
        private List<Discipline> mountainDisciplines = null;
        private Season season = null;

        public ImportExportPlayersHelper(DataEntities db)
        {
            _db = db;
            
            _teamRepo = new TeamsRepo(db);
            _playersRepo = new PlayersRepo(db);
            _usersRepo = new UsersRepo(db);
            _clubsRepo = new ClubsRepo(db);
            _leagueRepo = new LeagueRepo(db);
            _disciplinesRepo = new DisciplinesRepo(db);
            _friendshipTypesRepo = new FriendshipTypesRepo(db);
            _seasonsRepo = new SeasonsRepo(db);

            _allowGenderValue = _usersRepo.GetGenders().ToDictionary(p => p.Title.ToLower(), p => p.GenderId);
            //Hebrew

            //Male = זכר Female = נקבה
            _genderMap.Add("זכר".ToLower(), "Male".ToLower());
            _genderMap.Add("נקבה".ToLower(), "Female".ToLower());
        }

        public ImportExportPlayersHelper()
        {
        }

        public void InitiateFieldsForBicycle(int? unionId, int seasonId)
        {
            season = _seasonsRepo.GetById(seasonId);
            if (!unionId.HasValue) unionId = season.UnionId;
            friendships = _friendshipTypesRepo.GetAllByUnionId(unionId.Value);
            roadDisciplines = _disciplinesRepo.GetAllByUnionIdWithRoad(unionId.Value);
            mountainDisciplines = _disciplinesRepo.GetAllByUnionIdWithMountain(unionId.Value);
            
        }

        public void ExtractData(Stream stream, out List<ImportPlayerModel> correctRows, out List<ImportPlayerModel> validationErrorRows, bool isTennis, bool isSectionClub, string sectionAlias)
        {
            correctRows = new List<ImportPlayerModel>();
            validationErrorRows = new List<ImportPlayerModel>();
            var isFirstRow = true;
            using (var workBook = new XLWorkbook(stream))
            {
                var workSheet = workBook.Worksheet(1);
                foreach (var row in workSheet.Rows())
                {
                    if (!isFirstRow && !row.IsEmpty())
                    {
                        ImportPlayerModel validatedRow;
                        if (ValidateRow(row, out validatedRow, isTennis, isSectionClub, sectionAlias))
                        {
                            correctRows.Add(validatedRow);
                        }
                        else
                        {
                            validationErrorRows.Add(validatedRow);
                        }
                    }
                    else
                    {
                        isFirstRow = false;
                    }
                }
            }
        }

        public Stream BuildErrorFile(CultEnum culture, List<ImportPlayerModel> errorRows, List<ImportPlayerModel> duplicateRows, bool isTennis, bool isSectionClub, string sectionAlias ="")
        {

            var result = new MemoryStream();

            using (var workBook = new XLWorkbook(XLEventTracking.Disabled){ RightToLeft = culture == CultEnum.He_IL })
            {
                #region Error row
                if (errorRows != null && errorRows.Count > 0)
                {
                    var wsError = workBook.AddWorksheet(Messages.ImportPlayers_ErrorWorksheetName);

                    var columnCounter = 1;
                    var rowCounter = 1;
                    var addCell = new Action<string>(value =>
                    {
                        wsError.Cell(rowCounter, columnCounter).Value = value;
                        columnCounter++;
                    });

                    #region Header
                    addCell($"*{Messages.FirstName}");
                    addCell($"*{Messages.LastName}");
                    addCell($"{Messages.MiddleName}");
                    addCell($"*{Messages.Team} {Messages.Id}");
                    addCell($"*{Messages.IdentNum}");
                    addCell($"{Messages.Email}");
                    addCell($"{Messages.Phone} {Messages.Number.ToLower()}");
                    addCell($"*{Messages.BirthDay} (dd/mm/yyyy)");
                    addCell($"*{Messages.Gender} {Messages.Male.ToLower()}/{Messages.Female.ToLower()}");
                    addCell($"{Messages.Height}");
                    addCell($"{Messages.City}");
                    addCell($"{Messages.PlayerCardNumber}");
                    addCell($"{Messages.ShirtNumber}");
                    addCell($"{Messages.MedExamDate}");
                    addCell($"{Messages.DateOfInsuranceValidity}");
                    if (sectionAlias.Equals(GamesAlias.Bicycle, StringComparison.OrdinalIgnoreCase))
                    {
                        addCell(Messages.FriendshipName);
                        addCell(Messages.FrienshipPriceTypeName);
                        addCell(Messages.RoadHeat);
                        addCell(Messages.MountainHeat);
                        addCell(Messages.RoadIronNumber);
                        addCell(Messages.MountainIronNumber);
                        addCell(Messages.VelodromeIronNumber);
                        addCell(Messages.UciId);
                        addCell(Messages.ChipNumber);
                        addCell(Messages.KitStatus + "(" + Messages.Ready + "/" + Messages.Provided + "/" + Messages.Printed + ")");
                        addCell(Messages.ForeignFirstName);
                        addCell(Messages.ForeignLastName);
                    }
                    if (isTennis)
                    {
                        addCell(Messages.TenicardValidity);
                    }
                    if (isSectionClub)
                    {
                        addCell(Messages.Comment);
                    }
                    addCell($"{Messages.Reason}");


                    rowCounter++;
                    columnCounter = 1;

                    #endregion

                    wsError.Columns().AdjustToContents();

                    wsError.LastColumnUsed().Width = 50;

                    for (var rowIndex = 0; rowIndex < errorRows.Count; rowIndex++)
                    {
                        var row = errorRows[rowIndex].OriginalRow;

                        for (var columnIndex = 0; columnIndex < wsError.LastColumnUsed().ColumnNumber(); columnIndex++)
                        {
                            wsError.Cell(rowIndex + 2, columnIndex + 1).DataType = row.Cell(columnIndex + 1).DataType;
                            wsError.Cell(rowIndex + 2, columnIndex + 1).SetValue(row.Cell(columnIndex + 1).Value);
                        }

                        if (errorRows[rowIndex].RowErrors.Count > 0)
                        {
                            //string error = string.Join(@"\n", errorRows[i].RowErrors.Select(p => string.Format("{0} - {1}", p.Key, p.Value)));

                            //wsError.Cell(i + 2, 13).DataType = XLDataType.Text;
                            //wsError.Cell(i + 2, 13).Value = error;

                            foreach (var er in errorRows[rowIndex].RowErrors)
                            {
                                var line = wsError.Cell(rowIndex + 2, wsError.LastColumnUsed().ColumnNumber()).RichText.AddNewLine();
                                line.AddText(string.Format("{0} - {1}", er.Key, er.Value));
                            }

                            //wsError.Cell(i + 2, 13).Style.Alignment.ShrinkToFit = true;
                            wsError.Cell(rowIndex + 2, wsError.LastColumnUsed().ColumnNumber()).Style.Alignment.WrapText = true;
                        }
                    }
                }
                #endregion

                #region Duplicate row
                if (duplicateRows != null && duplicateRows.Count > 0)
                {
                    var wsDuplicate = workBook.AddWorksheet(Messages.ImportPlayers_DuplicateWorksheetName);

                    var columnCounter = 1;
                    var rowCounter = 1;
                    var addCell = new Action<string>(value =>
                    {
                        wsDuplicate.Cell(rowCounter, columnCounter).Value = value;
                        columnCounter++;
                    });

                    #region Header
                    addCell($"*{Messages.FirstName}");
                    addCell($"*{Messages.LastName}");
                    addCell($"{Messages.MiddleName}");
                    addCell($"*{Messages.Team} {Messages.Id}");
                    addCell($"*{Messages.IdentNum}");
                    addCell($"{Messages.Email}");
                    addCell($"{Messages.Phone} {Messages.Number.ToLower()}");
                    addCell($"*{Messages.BirthDay} (dd/mm/yyyy)");
                    addCell($"*{Messages.Gender} {Messages.Male.ToLower()}/{Messages.Female.ToLower()}");
                    addCell($"{Messages.Height}");
                    addCell($"{Messages.City}");
                    addCell($"{Messages.PlayerCardNumber}");
                    addCell($"{Messages.ShirtNumber}");
                    addCell($"{Messages.MedExamDate}");
                    addCell($"{Messages.DateOfInsuranceValidity}");
                    if (sectionAlias.Equals(GamesAlias.Bicycle, StringComparison.OrdinalIgnoreCase))
                    {
                        addCell(Messages.FriendshipName);
                        addCell(Messages.FrienshipPriceTypeName);
                        addCell(Messages.RoadHeat);
                        addCell(Messages.MountainHeat);
                        addCell(Messages.RoadIronNumber);
                        addCell(Messages.MountainIronNumber);
                        addCell(Messages.VelodromeIronNumber);
                        addCell(Messages.UciId);
                        addCell(Messages.ChipNumber);
                        addCell(Messages.KitStatus + "(" + Messages.Ready + "/" + Messages.Provided + "/" + Messages.Printed + ")");
                        addCell(Messages.ForeignFirstName);
                        addCell(Messages.ForeignLastName);
                    }
                    if (isTennis)
                    {
                        addCell(Messages.TenicardValidity);
                    }
                    if (isSectionClub)
                    {
                        addCell(Messages.Comment);
                    }
                    addCell($"{Messages.Reason}");

                    rowCounter++;
                    columnCounter = 1;

                    #endregion

                    wsDuplicate.Columns().AdjustToContents();
                    wsDuplicate.LastColumnUsed().Width = 50;

                    for (var rowIndex = 0; rowIndex < duplicateRows.Count; rowIndex++)
                    {
                        var row = duplicateRows[rowIndex].OriginalRow;

                        for (var columnIndex = 0; columnIndex < wsDuplicate.LastColumnUsed().ColumnNumber(); columnIndex++)
                        {
                            wsDuplicate.Cell(rowIndex + 2, columnIndex + 1).DataType = row.Cell(columnIndex + 1).DataType;
                            wsDuplicate.Cell(rowIndex + 2, columnIndex + 1).SetValue(row.Cell(columnIndex + 1).Value);
                        }

                        if (duplicateRows[rowIndex].RowErrors.Count > 0)
                        {
                            var error = string.Join(@"\n", duplicateRows[rowIndex].RowErrors.Select(p => string.Format("{0}", p.Value)));

                            wsDuplicate.Cell(rowIndex + 2, wsDuplicate.LastColumnUsed().ColumnNumber()).DataType = XLDataType.Text;
                            wsDuplicate.Cell(rowIndex + 2, wsDuplicate.LastColumnUsed().ColumnNumber()).SetValue(error);
                        }
                    }
                }
                #endregion

                workBook.SaveAs(result);
                result.Position = 0;
            }

            return result;
        }

        public Stream BuildErrorFileForGymnastics(List<ImportGymnasticRegistrationModel> errorRows,
            List<ImportGymnasticRegistrationModel> duplicateRows, CultEnum culture = CultEnum.En_US)
        {

            var result = new MemoryStream();

            using (var workBook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = culture == CultEnum.He_IL })
            {
                #region Error row
                if (errorRows != null && errorRows.Count > 0)
                {
                    var wsError = workBook.AddWorksheet(Messages.ImportPlayers_ErrorWorksheetName);

                    #region Header
                    wsError.Cell(1, 1).Value = $"*{Messages.ClubNumber}";
                    wsError.Cell(1, 2).Value = Messages.ClubName;
                    wsError.Cell(1, 3).Value = $"{Messages.FirstName}";
                    wsError.Cell(1, 4).Value = $"{Messages.LastName}";
                    wsError.Cell(1, 5).Value = $"{Messages.FullName}";
                    wsError.Cell(1, 6).Value = $"*{Messages.Id}/{Messages.PassportNum}";
                    wsError.Cell(1, 7).Value = $"*{Messages.BirthDay}";
                    wsError.Cell(1, 8).Value = $"{Messages.Composition} ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsError.Cell(1, 9).Value = $"{Messages.Composition} 2 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsError.Cell(1, 10).Value = $"{Messages.Composition} 3 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsError.Cell(1, 11).Value = $"{Messages.Composition} 4 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsError.Cell(1, 12).Value = $"{Messages.Composition} 5 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsError.Cell(1, 13).Value = $"{Messages.Composition} 6 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsError.Cell(1, 14).Value = $"{Messages.Composition} 7 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsError.Cell(1, 15).Value = $"{Messages.Instrument} #1";
                    wsError.Cell(1, 16).Value = $"{Messages.Order} #1";
                    wsError.Cell(1, 17).Value = $"{Messages.Instrument} #2";
                    wsError.Cell(1, 18).Value = $"{Messages.Order} #2";
                    wsError.Cell(1, 19).Value = $"{Messages.Instrument} #3";
                    wsError.Cell(1, 20).Value = $"{Messages.Order} #3";
                    wsError.Cell(1, 21).Value = $"{Messages.Instrument} #4";
                    wsError.Cell(1, 22).Value = $"{Messages.Order} 4";
                    wsError.Cell(1, 23).Value = $"{Messages.Instrument} #5";
                    wsError.Cell(1, 24).Value = $"{Messages.Order} #5";
                    wsError.Cell(1, 25).Value = Messages.FinalScore;
                    wsError.Cell(1, 26).Value = Messages.Rank;

                    wsError.Cell(1, 27).Value = Messages.Reason;
                    #endregion

                    wsError.Columns(1, 26).AdjustToContents();
                    wsError.Column(27).Width = 60;

                    for (var i = 0; i < errorRows.Count; i++)
                    {
                        var row = errorRows[i].OriginalRow;

                        for (var j = 0; j < 27; j++)
                        {
                            wsError.Cell(i + 2, j + 1).DataType = row.Cell(j + 1).DataType;
                            wsError.Cell(i + 2, j + 1).SetValue(row.Cell(j + 1).Value);
                        }

                        if (errorRows[i].RowErrors.Count > 0)
                        {
                            foreach (var er in errorRows[i].RowErrors)
                            {
                                var line = wsError.Cell(i + 2, 27).RichText.AddNewLine();
                                line.AddText(string.Format("{0} - {1}", er.Key, er.Value));
                            }
                            wsError.Cell(i + 2, 27).Style.Alignment.WrapText = true;
                        }
                    }
                }
                #endregion

                #region Duplicate row

                if (duplicateRows != null && duplicateRows.Count > 0)
                {
                    var wsDuplicate = workBook.AddWorksheet(Messages.ImportPlayers_DuplicateWorksheetName);

                    #region Header
                    wsDuplicate.Cell(1, 1).Value = $"*{Messages.ClubNumber}";
                    wsDuplicate.Cell(1, 2).Value = Messages.ClubName;
                    wsDuplicate.Cell(1, 3).Value = $"*{Messages.FirstName}";
                    wsDuplicate.Cell(1, 4).Value = $"*{Messages.LastName}";
                    wsDuplicate.Cell(1, 5).Value = $"*{Messages.FullName}";
                    wsDuplicate.Cell(1, 6).Value = $"*{Messages.Id}/{Messages.PassportNum}";
                    wsDuplicate.Cell(1, 7).Value = $"*{Messages.BirthDay}";
                    wsDuplicate.Cell(1, 8).Value = $"{Messages.Composition} ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsDuplicate.Cell(1, 9).Value = $"{Messages.Composition} 2 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsDuplicate.Cell(1, 10).Value = $"{Messages.Composition} 3 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsDuplicate.Cell(1, 11).Value = $"{Messages.Composition} 4 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsDuplicate.Cell(1, 12).Value = $"{Messages.Composition} 5 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsDuplicate.Cell(1, 13).Value = $"{Messages.Composition} 6 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsDuplicate.Cell(1, 14).Value = $"{Messages.Composition} 7 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})";
                    wsDuplicate.Cell(1, 15).Value = $"{Messages.Instrument} #1";
                    wsDuplicate.Cell(1, 16).Value = $"{Messages.Order} #1";
                    wsDuplicate.Cell(1, 17).Value = $"{Messages.Instrument} #2";
                    wsDuplicate.Cell(1, 18).Value = $"{Messages.Order} #2";
                    wsDuplicate.Cell(1, 19).Value = $"{Messages.Instrument} #3";
                    wsDuplicate.Cell(1, 20).Value = $"{Messages.Order} #3";
                    wsDuplicate.Cell(1, 21).Value = $"{Messages.Instrument} #4";
                    wsDuplicate.Cell(1, 22).Value = $"{Messages.Order} 4";
                    wsDuplicate.Cell(1, 23).Value = $"{Messages.Instrument} #5";
                    wsDuplicate.Cell(1, 24).Value = $"{Messages.Order} #5";
                    wsDuplicate.Cell(1, 25).Value = Messages.FinalScore;
                    wsDuplicate.Cell(1, 26).Value = Messages.Position;

                    wsDuplicate.Cell(1, 27).Value = Messages.Reason;
                    #endregion

                    wsDuplicate.Columns(1, 26).AdjustToContents();
                    wsDuplicate.Column(27).Width = 60;

                    for (var i = 0; i < duplicateRows.Count; i++)
                    {
                        var row = duplicateRows[i].OriginalRow;

                        for (var j = 0; j < 27; j++)
                        {
                            wsDuplicate.Cell(i + 2, j + 1).DataType = row.Cell(j + 1).DataType;
                            wsDuplicate.Cell(i + 2, j + 1).SetValue(row.Cell(j + 1).Value);
                        }

                        if (duplicateRows[i].RowErrors.Count > 0)
                        {
                            var error = string.Join(@"\n", duplicateRows[i].RowErrors.Select(p => string.Format("{0}", p.Value)));

                            wsDuplicate.Cell(i + 2, 27).DataType = XLDataType.Text;
                            wsDuplicate.Cell(i + 2, 27).SetValue(error);
                        }
                    }
                }
                #endregion

                workBook.SaveAs(result);
                result.Position = 0;
            }

            return result;
        }


        public int ImportPlayers(List<ImportPlayerAllowedTeamModel> suitableTeams, int? seasonId, int? leagueId, int? clubId,
            List<ImportPlayerModel> playerRows, out List<ImportPlayerModel> importErrorRows,
            out List<ImportPlayerModel> duplicateRows, bool approvePlayers, bool setPlayersAsActive, string sectionAlisas = "")
        {
            var importedRowCount = 0;
            importErrorRows = new List<ImportPlayerModel>();
            duplicateRows = new List<ImportPlayerModel>();
            foreach (var player in playerRows)
            {
                var currentSuitableTeams = suitableTeams.Where(x => x.TeamId == player.TeamId).ToList();

                // user team not exist in the available team list
                if (currentSuitableTeams.Any())
                {
                    var user = _playersRepo.GetUserByIdentNum(player.Id);

                    //if (player.JerseyNo.HasValue && _playersRepo.ShirtNumberExists(player.TeamId, player.JerseyNo.Value, seasonId ?? 0, leagueId, clubId))
                    //{
                    //    // the player with the same Shirt Number exist in the team
                    //    player.RowErrors.Add(new KeyValuePair<string, string>("", Messages.ImportPlayers_PlayerWithJerseyNumberExist));
                    //    importErrorRows.Add(player);
                    //    continue;
                    //}

                    if (player.JerseyNo == null)
                    {
                        // get max shirt number in the team.
                        // calculate new exist number
                        var maxShirtNumber = _teamRepo.GetMaxShirtNumberInTeam(player.TeamId);
                        player.JerseyNo = maxShirtNumber + 1;
                    }

                    foreach (var currentSuitableTeam in currentSuitableTeams)
                    {
                        if (user != null &&
                            _playersRepo.PlayerExistsInTeam(player.TeamId, user.UserId, seasonId, currentSuitableTeam.LeagueId, currentSuitableTeam.ClubId))
                        {
                            var updateTeamPlayer = user.TeamsPlayers.FirstOrDefault(tp => tp.TeamId == player.TeamId &&
                                                                     tp.SeasonId == seasonId &&
                                                                     (currentSuitableTeam.LeagueId != null
                                                                         ? tp.LeagueId == currentSuitableTeam.LeagueId
                                                                         : tp.LeagueId == null) &&
                                                                     (currentSuitableTeam.ClubId != null && currentSuitableTeam.LeagueId == null
                                                                         ? tp.ClubId == currentSuitableTeam.ClubId
                                                                         : tp.ClubId == null));

                            if (updateTeamPlayer != null)
                            {
                                if (approvePlayers)
                                {
                                    if (!updateTeamPlayer.IsActive || updateTeamPlayer.IsApprovedByManager != true)
                                    {
                                        updateTeamPlayer.IsActive = true;
                                        updateTeamPlayer.IsApprovedByManager = true;
                                        updateTeamPlayer.ApprovalDate = DateTime.Now;
                                    }

                                    updateTeamPlayer.User.IsActive = true;
                                }
                                else if (setPlayersAsActive)
                                {

                                    updateTeamPlayer.IsActive = true;
                                    updateTeamPlayer.User.IsActive = true;
                                }

                                updateTeamPlayer.ShirtNum = player.JerseyNo.Value;
                                if(sectionAlisas == SectionAliases.Bicycle)
                                {
                                    if (player.Hierarchy != null && updateTeamPlayer.FriendshipsType.Hierarchy > player.Hierarchy)
                                    {
                                        updateTeamPlayer.FriendshipTypeId = player.FriendshipTypeId;
                                        updateTeamPlayer.FriendshipPriceType = player.FriendshipPriceTypeId;
                                        updateTeamPlayer.RoadDisciplineId = player.RoadHeatId;
                                        updateTeamPlayer.MountaintDisciplineId = player.MountainHeatId;
                                    }
                                    updateTeamPlayer.RoadIronNumber = player.RoadIronNumber;
                                    updateTeamPlayer.MountainIronNumber = player.MountainIronNumber;
                                    updateTeamPlayer.VelodromeIronNumber = player.VelodromeIronNumber;
                                    updateTeamPlayer.KitStatus = player.KitStatus;
                                    
                                }
                                if (player.DateOfMedicalExam.HasValue)
                                    updateTeamPlayer.User.MedExamDate = player.DateOfMedicalExam;
                            }

                            user.FirstName = player.FirstName;
                            user.LastName = player.LastName;
                            user.MiddleName = player.MiddleName;
                            user.Email = player.Email;
                            if (!string.IsNullOrWhiteSpace(player.PhoneNo))
                                user.Telephone = player.PhoneNo;
                            user.BirthDay = player.Birthday;
                            user.GenderId = player.GenderValue;

                            if (!string.IsNullOrWhiteSpace(player.Address)) 
                            {
                                user.Address = player.Address;
                            }
                            if (!string.IsNullOrWhiteSpace(player.PostalCode))
                            {
                                user.PostalCode = player.PostalCode;
                            }
                            if (!string.IsNullOrWhiteSpace(player.PassportNum))
                            {
                                user.PassportNum = player.PassportNum;
                            }

                            if (player.HeightValue.HasValue)
                                user.Height = player.HeightValue;
                            if (!string.IsNullOrWhiteSpace(player.City))
                                user.City = player.City;
                            if (!string.IsNullOrWhiteSpace(player.PlayerCardNo))
                                user.IdentCard = player.PlayerCardNo;
                            if (player.DateOfInsuranceValidity.HasValue)
                            {
                                user.DateOfInsurance = player.DateOfInsuranceValidity;
                            }
                            if (player.TenicardValidity.HasValue)
                            {
                                user.TenicardValidity = player.TenicardValidity;
                            }
                            if (player.DateOfMedicalExam.HasValue)
                                user.MedExamDate = player.DateOfMedicalExam;
                            if (sectionAlisas == SectionAliases.Bicycle)
                            {
                                user.UciId = player.UciId;
                                user.ChipNumber = player.ChipNumber;
                            }
                            if (!string.IsNullOrWhiteSpace(player.ForeignFirstName))
                            {
                                user.ForeignFirstName = player.ForeignFirstName;
                            }
                            if (!string.IsNullOrWhiteSpace(player.ForeignLastName))
                            {
                                user.ForeignLastName = player.ForeignLastName;
                            }

                            _db.SaveChanges();

                            importedRowCount++;

                            continue;
                        }

                        if (user == null)
                        {
                            // create a new user if the user not exist in database
                            user = new User
                            {
                                BirthDay = player.Birthday,
                                City = player.City,
                                Email = player.Email,
                                FirstName = player.FirstName,
                                LastName = player.LastName,
                                MiddleName = player.MiddleName,
                                GenderId = player.GenderValue,
                                Height = player.HeightValue,
                                IdentCard = player.PlayerCardNo,
                                IdentNum = player.Id,
                                Telephone = player.PhoneNo,
                                Password = Protector.Encrypt("123abc12"),
                                TypeId = 4,
                                DateOfInsurance = player.DateOfInsuranceValidity,
                                TenicardValidity = player.TenicardValidity,
                                IsActive = true,
                                MedExamDate = player.DateOfMedicalExam,
                                PostalCode = player.PostalCode,
                                PassportNum = player.PassportNum,
                                Address = player.Address,
                                ChipNumber = player.ChipNumber,
                                UciId = player.UciId,
                                ForeignFirstName = player.ForeignFirstName,
                                ForeignLastName = player.ForeignLastName                                
                            };

                            _usersRepo.Create(user);
                        }

                        var teamPlayer = new TeamsPlayer
                        {
                            ShirtNum = player.JerseyNo.Value,
                            TeamId = player.TeamId,
                            UserId = user.UserId,
                            SeasonId = seasonId,
                            LeagueId = currentSuitableTeam.LeagueId,
                            ClubId = currentSuitableTeam.LeagueId == null ? currentSuitableTeam.ClubId : null,
                            //MedExamDate = player.DateOfMedicalExam,
                            Comment = player.Comment,

                            IsActive = approvePlayers || setPlayersAsActive,
                            IsApprovedByManager = approvePlayers ? true : (bool?) null,
                            ApprovalDate = approvePlayers ? DateTime.Now : (DateTime?) null
                        };

                        if(sectionAlisas == SectionAliases.Bicycle)
                        {
                            teamPlayer.FriendshipTypeId = player.FriendshipTypeId;
                            teamPlayer.FriendshipPriceType = player.FriendshipPriceTypeId;
                            teamPlayer.RoadDisciplineId = player.RoadHeatId;
                            teamPlayer.MountaintDisciplineId = player.MountainHeatId;
                            teamPlayer.RoadIronNumber = player.RoadIronNumber;
                            teamPlayer.MountainIronNumber = player.MountainIronNumber;
                            teamPlayer.VelodromeIronNumber = player.VelodromeIronNumber;
                            teamPlayer.KitStatus = player.KitStatus;
                        }

                        _playersRepo.AddToTeam(teamPlayer);

                        _db.SaveChanges();

                        importedRowCount++;
                    }
                }
                else
                {
                    // player can't be import to available team
                    player.RowErrors.Add(new KeyValuePair<string, string>("", Messages.ImportPlayers_ImportNotAllowedForTeam));
                    importErrorRows.Add(player);
                }
            }

            return importedRowCount;
        }

        private bool ValidateRow(IXLRow row, out ImportPlayerModel model, bool isTennis, bool isSectionClub, string sectionAlias)
        {
            model = new ImportPlayerModel(row);
            var mainCnt = 16;

            #region 1 – *First name
            var firstName = row.Cell(1).Value.ToString();
            if (string.IsNullOrWhiteSpace(firstName))
            {
                //return false;
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_FirstName, Messages.ImportPlayers_Required));
            }
            #endregion

            #region 2 – *Last name
            var lastName = row.Cell(2).Value.ToString();
            if (string.IsNullOrWhiteSpace(lastName))
            {
                //return false;
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_LastName, Messages.ImportPlayers_Required));
            }
            #endregion

            #region 3 – Middle name
            var middleName = row.Cell(3)?.Value?.ToString();
            //if (string.IsNullOrWhiteSpace(middleName))
            //{
            //    //return false;
            //    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_MiddleName, Messages.ImportPlayers_Required));
            //}
            #endregion

            #region 4 - *TeamID.
            var teamIdStr = row.Cell(4).Value.ToString();
            var teamId = 0;
            if (string.IsNullOrWhiteSpace(teamIdStr))
            {
                //return false;
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_TeamID, Messages.ImportPlayers_Required));
            }
            else
            {
                if (!int.TryParse(teamIdStr, out teamId))
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_TeamID, Messages.ImportPlayers_ShouldBeNumber));
                }
            }
            #endregion

            #region 5 - *ID number.
            var idStr = row.Cell(5).Value.ToString();
            var idSb = new System.Text.StringBuilder();
            var id = string.Empty;
            if (string.IsNullOrWhiteSpace(idStr))
            {
                //return false;
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_Required));
            }
            else
            {
                idStr = idStr.Trim();
                foreach (var ch in idStr)
                {
                    var tmp = 0;
                    if (!int.TryParse(ch.ToString(), out tmp))
                    {
                        //return false;
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_ShouldBeNumber));
                    }
                    idSb.Append(ch);
                }
                var idLength = idSb.Length;
                if (idLength > 9)
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_IDNumberMaxLength));
                }

                while (idSb.Length < 9)
                {
                    idSb.Insert(0, "0");
                }
                id = idSb.ToString();
            }
            #endregion

            #region 6 - Email.
            var email = row.Cell(6)?.Value?.ToString()?.Trim();
            //if (!string.IsNullOrWhiteSpace(email))
            //{
            //    //if (!Regex.IsMatch(email, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.CultureInvariant))
            //    //{
            //    //    return false;
            //    //}
            //}
            #endregion

            #region 7 - Phone No'.
            var phoneNo = row.Cell(7).Value.ToString();
            if (!string.IsNullOrWhiteSpace(phoneNo))
            {
                phoneNo = phoneNo.Trim();
            }
            #endregion

            #region 8 - *Birth date.
            var birthDate = DateTime.MinValue;
            if (row.Cell(8).DataType == XLDataType.DateTime)
            {
                birthDate = row.Cell(8).GetDateTime();
            }
            else
            {
                var birthDateStr = row.Cell(8).Value.ToString();

                if (string.IsNullOrWhiteSpace(birthDateStr))
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Birthday, Messages.ImportPlayers_Required));
                }
                else
                {
                    if (!DateTime.TryParseExact(birthDateStr, "dd/MM/yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out birthDate))
                    {
                        //return false;
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Birthday, Messages.ImportPlayers_BirthdayIncorrectFormat));
                    }
                }
            }
            #endregion

            #region 9 - *Gender.
            var gender = row.Cell(9).Value.ToString();
            var genderId = 0;
            if (string.IsNullOrWhiteSpace(gender))
            {
                //return false;
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Gender, Messages.ImportPlayers_Required));
            }
            else
            {
                if (_allowGenderValue.ContainsKey(gender.Trim().ToLower()))
                {
                    genderId = _allowGenderValue[gender.Trim().ToLower()];
                }
                else if (_genderMap.ContainsKey(gender.Trim().ToLower()))
                {
                    genderId = _allowGenderValue[_genderMap[gender.Trim().ToLower()]];
                }
                else
                {
                    return false;
                }
            }
            #endregion

            #region 10 - Height.
            var heightIdStr = row.Cell(10).Value.ToString();
            int? height = null;
            if (!string.IsNullOrWhiteSpace(heightIdStr))
            {
                int tmpHeight;
                if (!int.TryParse(heightIdStr, out tmpHeight))
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Height, Messages.ImportPlayers_ShouldBeNumber));
                }
                height = tmpHeight;
            }
            #endregion

            //11 - City.
            var city = row.Cell(11).Value.ToString();
            //12 - Player card NO'.
            var playerCardNo = row.Cell(12).Value.ToString();

            #region 13 - Jersey NO' (depend on which team)
            var jerseyNOStr = row.Cell(13).Value.ToString();
            int? jerseyNO = null;
            if (!string.IsNullOrWhiteSpace(jerseyNOStr))
            {
                var tmp = 0;
                if (!int.TryParse(jerseyNOStr, out tmp))
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_JerseyNO, Messages.ImportPlayers_ShouldBeNumber));
                }
                else
                {
                    jerseyNO = tmp;
                }
            }
            #endregion

            #region 14 - Date of medical exam.
            DateTime? medExamDateValue = null;
            var medExamDate = DateTime.MinValue;
            if (row.Cell(14).DataType == XLDataType.DateTime)
            {
                medExamDateValue = row.Cell(14).GetDateTime();
            }
            else
            {
                var medExamDateStr = row.Cell(14).Value.ToString();
                if (!DateTime.TryParseExact(medExamDateStr, "dd-MM-yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out medExamDate)
                    && !string.IsNullOrEmpty(medExamDateStr))
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Medical, Messages.ImportPlayers_BirthdayIncorrectFormat));
                }
                else if (string.IsNullOrEmpty(medExamDateStr))
                {
                    medExamDateValue = null;
                }
                else
                {
                    medExamDateValue = medExamDate;
                }
            }
            #endregion

            #region 15 - Date of insurance.
            DateTime? dateOfInsuranceValue = DateTime.MinValue;
            var dateOfInsurance = DateTime.MinValue;

            if (row.Cell(15).DataType == XLDataType.DateTime)
            {
                dateOfInsuranceValue = row.Cell(15).GetDateTime();
            }
            else
            {
                var dateOfInsuranceStr = row.Cell(15).Value.ToString();
                if (!DateTime.TryParseExact(dateOfInsuranceStr, "dd-MM-yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out dateOfInsurance)
                    && !string.IsNullOrEmpty(dateOfInsuranceStr))
                {
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Insurance, Messages.ImportPlayers_BirthdayIncorrectFormat));
                }
                else if (string.IsNullOrEmpty(dateOfInsuranceStr))
                {
                    dateOfInsuranceValue = null;
                }
                else
                {
                    dateOfInsuranceValue = dateOfInsurance;
                }
            }
            #endregion

            DateTime? tenicardValue = null;


            #region 16 - Tenicard validity.
            if (isTennis)
            {
                var team = _teamRepo.GetById(teamId);
                var section = string.Empty;
                if (team != null)
                {
                    var teamClub = team.ClubTeams.FirstOrDefault()?.Club;
                    if (teamClub != null)
                    {
                        section = teamClub.Section?.Alias ?? teamClub.Union?.Section?.Alias;
                    }
                    else
                    {
                        var teamLeague = team.LeagueTeams.FirstOrDefault()?.Leagues;
                        if (teamLeague != null)
                        {
                            section = teamLeague?.Union?.Section?.Alias ?? teamLeague?.Club?.Section?.Alias ?? teamLeague?.Club?.Union?.Section?.Alias;
                        }
                    }
                }

                var tenicard = DateTime.MinValue;
                if (string.Equals(section, SectionAliases.Tennis, StringComparison.OrdinalIgnoreCase))
                {
                    if (row.Cell(mainCnt).DataType == XLDataType.DateTime)
                    {
                        tenicardValue = row.Cell(mainCnt).GetDateTime();
                    }
                    else
                    {
                        var tenicardStr = row.Cell(mainCnt++).Value.ToString();

                        if (!DateTime.TryParseExact(tenicardStr, "dd-MM-yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out tenicard)
                            && !string.IsNullOrEmpty(tenicardStr))
                        {
                            //return false;
                            model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Tenicard, Messages.ImportPlayers_BirthdayIncorrectFormat));
                        }
                        else if (string.IsNullOrEmpty(tenicardStr))
                        {
                            tenicardValue = null;
                        }
                        else
                        {
                            tenicardValue = tenicard;
                        }
                    }
                }
            }

            #endregion

            #region 17(16) - Comment

            var comment = string.Empty;
            if (isSectionClub)
            {
                comment = row.Cell(mainCnt++).Value.ToString();
            }
            #endregion

            if(sectionAlias == SectionAliases.Bicycle)
            {
                var cnt = 16;
                int playerAge = Convert.ToInt32(season.Name) - birthDate.Year;                

                var friendshipValid = true;
                //if the birthday is not set or gender - there cannot be validation for friendships
                if (!default(KeyValuePair<string, string>).Equals(model.RowErrors.Find(x => x.Key == Messages.BirthDay))
                    || !default(KeyValuePair<string, string>).Equals(model.RowErrors.Find(x => x.Key == Messages.Gender))) friendshipValid = false;


                #region 18(16) – FriendshipName
                var frienshipname = row.Cell(cnt++).Value.ToString();
                var frienshipId = -1;
                int? hierarchy = null;
                if (!friendshipValid && !string.IsNullOrWhiteSpace(frienshipname))
                {
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.FriendshipName, Messages.ImportPlayers_AgeAndGenderRequiredForValidation));
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(frienshipname))
                    {
                        if (!int.TryParse(frienshipname, out frienshipId))
                        {
                            model.RowErrors.Add(new KeyValuePair<string, string>(Messages.FriendshipName, Messages.ImportPlayers_ShouldBeNumber));
                            friendshipValid = false;
                        }
                        else
                        {
                            //check if id exist in db 
                            var friendshipList = friendships.Where(x => x.FriendshipsTypesId == frienshipId && x.CompetitionAges.Any(c => c.from_age <= playerAge && c.to_age >= playerAge && (c.gender == genderId || c.gender == 3)));
                            var friendshipCount = friendshipList.Count();
                            if (friendshipCount == 0)
                            {
                                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.FriendshipName, Messages.ImportPlayers_FriendshipNameIdNotValid));
                                friendshipValid = false;
                            }
                            else
                            {
                                hierarchy = friendshipList.FirstOrDefault()?.Hierarchy;
                            }

                        }

                    }
                }
                #endregion

                #region 19(17) – FriendshipName
                var frienshiptype = row.Cell(cnt++).Value.ToString();
                var frienshiptypeId = -1;
                if(!friendshipValid && !string.IsNullOrWhiteSpace(frienshiptype))
                {
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.FrienshipPriceTypeName, Messages.ImportPlayers_FriendshipNameIdNotValid));
                }
                if (!string.IsNullOrWhiteSpace(frienshiptype))
                {
                    if (!int.TryParse(frienshiptype, out frienshiptypeId))
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.FrienshipPriceTypeName, Messages.ImportPlayers_ShouldBeNumber));
                    }
                    else
                    {
                        //check if the frienship type exists 
                        var ftype = friendships.Where(x => x.FriendshipsTypesId == frienshipId && x.FriendshipPrices.Any(c => c.FriendshipPriceType == frienshiptypeId && c.FromAge <= playerAge && c.ToAge >= playerAge && (c.GenderId == genderId || c.GenderId == 3))).Count();
                        if(ftype == 0)
                        {
                            model.RowErrors.Add(new KeyValuePair<string, string>(Messages.FrienshipPriceTypeName, Messages.ImportPlayers_FriendshipTypeIdNotValid));
                        }
                    }
                }
                #endregion

                #region 20(18) – RoadHeat
                var roadheatName = row.Cell(cnt++).Value.ToString();
                var roadheatId = -1;
                if (!friendshipValid && !string.IsNullOrWhiteSpace(roadheatName))
                {
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.RoadHeat, Messages.ImportPlayers_FriendshipNameIdNotValid));
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(roadheatName))
                    {
                        if (!int.TryParse(roadheatName, out roadheatId))
                        {
                            model.RowErrors.Add(new KeyValuePair<string, string>(Messages.RoadHeat, Messages.ImportPlayers_ShouldBeNumber));
                        }
                        else
                        {
                            //check in db
                            var rHeat = roadDisciplines.Where(x => x.DisciplineId == roadheatId && x.CompetitionAges.Any(c => c.from_age <= playerAge && c.to_age >= playerAge && (c.gender == genderId || c.gender == 3) && c.FriendshipTypeId == frienshipId)).Count();
                            if(rHeat == 0)
                            {
                                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.RoadHeat, Messages.ImportPlayers_RoadHeatNotValid));
                            }
                        }
                    }
                }
                #endregion

                #region 21(19) – MountainHeat
                var mountainHeatName = row.Cell(cnt++).Value.ToString();
                var mountainHeatId = -1;
                if (!friendshipValid && !string.IsNullOrWhiteSpace(mountainHeatName))
                {
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.MountainHeat, Messages.ImportPlayers_FriendshipNameIdNotValid));
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(mountainHeatName))
                    {
                        if (!int.TryParse(mountainHeatName, out mountainHeatId)) { 
                            model.RowErrors.Add(new KeyValuePair<string, string>(Messages.MountainHeat, Messages.ImportPlayers_ShouldBeNumber));
                        }
                        else
                        {
                            //check in db
                            var rHeat = mountainDisciplines.Where(x => x.DisciplineId == mountainHeatId && x.CompetitionAges.Any(c => c.from_age <= playerAge && c.to_age >= playerAge && (c.gender == genderId || c.gender == 3) && c.FriendshipTypeId == frienshipId)).Count();
                            if (rHeat == 0)
                            {
                                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.RoadHeat, Messages.ImportPlayers_MountainHeatNotValid));
                            }
                        }
                    }
                }
                #endregion

                #region 22(20) – RoadIronNumber
                var roadIron = row.Cell(cnt++).Value.ToString();
                var roadIronNumber = -1;
                if (!string.IsNullOrWhiteSpace(roadIron))
                {
                    if (!int.TryParse(roadIron, out roadIronNumber))
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.RoadIronNumber, Messages.ImportPlayers_ShouldBeNumber));
                }
                #endregion

                #region 23(21) – MountainIronNumber
                var mountainIron = row.Cell(cnt++).Value.ToString();
                var mountainIronNumber = -1;
                if (!string.IsNullOrWhiteSpace(mountainIron))
                {
                    if (!int.TryParse(mountainIron, out mountainIronNumber))
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.MountainIronNumber, Messages.ImportPlayers_ShouldBeNumber));
                }
                #endregion

                #region 24(22) – velodromeIronNumber
                var velodromeIron = row.Cell(cnt++).Value.ToString();
                var velodromeIronNumber = -1;
                if (!string.IsNullOrWhiteSpace(velodromeIron))
                {
                    if (!int.TryParse(velodromeIron, out velodromeIronNumber))
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.VelodromeIronNumber, Messages.ImportPlayers_ShouldBeNumber));
                }
                #endregion

                #region 25(23) – UciID
                var uciTxt = row.Cell(cnt++).Value.ToString();
                long uciId = -1L;
                if (!string.IsNullOrWhiteSpace(uciTxt))
                {
                    if (!long.TryParse(uciTxt, out uciId))
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.UciId, Messages.ImportPlayers_ShouldBeNumber));
                }
                #endregion
                #region 26(24) – ChipNumber
                var chipNumber = row.Cell(cnt++).Value.ToString();
                #endregion
                #region 27(25) – KitStatus
                var kitStatusVal = row.Cell(cnt++).Value.ToString();
                int? kitStatus = null;
                if (!string.IsNullOrWhiteSpace(kitStatusVal))
                {
                    var k = PlayerKitHelper.GetKitValue(kitStatusVal);
                    if(k != null)
                    {
                        kitStatus = Convert.ToInt32(k);
                    }
                }
                #endregion


                model.FriendshipTypeId = frienshipId != -1 ? frienshipId : (int?)null;
                model.FriendshipPriceTypeId = frienshiptypeId != -1 ? frienshiptypeId : (int?)null;
                model.RoadHeatId = roadheatId != -1 ? roadheatId : (int?)null;
                model.MountainHeatId = mountainHeatId != -1 ? mountainHeatId : (int?)null;
                model.RoadIronNumber = roadIronNumber != -1 ? roadIronNumber : (int?)null;
                model.MountainIronNumber = mountainIronNumber != -1 ? mountainIronNumber : (int?)null ;
                model.VelodromeIronNumber = velodromeIronNumber != -1 ? velodromeIronNumber : (int?)null;
                model.UciId = uciId != -1L ? uciId : (long?)null;
                model.ChipNumber = chipNumber;
                model.KitStatus = kitStatus;
                model.Hierarchy = hierarchy;
                mainCnt = cnt;
            }

            #region 28(26) – ForeignFirstName
            var ffName = row.Cell(mainCnt++).Value.ToString();
            #endregion

            #region 29(27) – ForeignLastName
            var flName = row.Cell(mainCnt++).Value.ToString();
            #endregion

            #region 18(17) - postalcode

            var postalcode = string.Empty;
            //if (isSectionClub)
            //{
                postalcode = row.Cell(mainCnt++).Value.ToString();
            //}
            #endregion

            #region 19(18) - PassportNum

            var passport = string.Empty;
            //if (isSectionClub)
            //{
                passport = row.Cell(mainCnt++).Value.ToString();
            //}
            #endregion

            #region 20(19) - Address

            var address = string.Empty;
            //if (isSectionClub)
            //{
                address = row.Cell(mainCnt++).Value.ToString();
            //}
            #endregion

            model.FirstName = firstName.Trim();
            model.LastName = lastName.Trim();
            model.MiddleName = middleName?.Trim();
            model.TeamId = teamId;
            model.Id = id;
            model.Email = email;
            model.PhoneNo = phoneNo;
            model.Birthday = birthDate;
            model.Gender = gender.Trim();
            model.GenderValue = genderId;
            model.Height = heightIdStr;
            model.HeightValue = height;
            model.City = city;
            model.PlayerCardNo = playerCardNo;
            model.JerseyNo = jerseyNO;
            model.TenicardValidity = tenicardValue;
            model.DateOfInsuranceValidity = dateOfInsuranceValue;
            model.DateOfMedicalExam = medExamDateValue;
            model.Comment = comment;
            model.ForeignFirstName = ffName?.ToUpper();
            model.ForeignLastName = flName?.ToUpper();
            model.PostalCode = postalcode;
            model.PassportNum = passport;
            model.Address = address;

            return model.RowErrors.Count == 0;
        }

        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public Stream ExportAthletesNumbers(IEnumerable<PlayerViewModel> rows)
        {
            var result = new MemoryStream();
            using (var workBook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = workBook.AddWorksheet(Messages.ImportPlayers);
                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                #region Header

                addCell(Messages.FullName);
                addCell(Messages.IdentNum);
                addCell(Messages.AthleteNumber);

                #endregion

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                #region Body

                foreach (var row in rows)
                {
                    GetNameSurname(row.FullName, out var firstName, out var lastName);

                    addCell(firstName + " " + lastName);
                    addCell(row.IdentNum ?? row.PassportNum ?? string.Empty);
                    addCell(row.AthletesNumbers?.ToString());

                    rowCounter++;
                    columnCounter = 1;
                }

                ws.Columns().AdjustToContents();
                workBook.SaveAs(result);
                result.Position = 0;

                #endregion

            }
            return result;
        }

        public string GetLastNameByFullName(string fullName)
        {
            var fullNameArray = fullName?.Split(' ')?.ToList();
            return fullNameArray[fullNameArray.Count-1];
        }

        public Stream ExportAllPlayers(IEnumerable<PlayerViewModel> rows, string section, CultEnum culture, bool isIndividual = false)
        {
            var isGymnastics = string.Equals(GamesAlias.Gymnastic, section, StringComparison.OrdinalIgnoreCase);
            var isAthletic = string.Equals(GamesAlias.Athletics, section, StringComparison.OrdinalIgnoreCase);
            var isWeightLifting = string.Equals(GamesAlias.WeightLifting, section, StringComparison.OrdinalIgnoreCase);
            var isRowing = string.Equals(GamesAlias.Rowing, section, StringComparison.OrdinalIgnoreCase);
            var isTennis = string.Equals(GamesAlias.Tennis, section, StringComparison.OrdinalIgnoreCase);
            var isBicycle = string.Equals(GamesAlias.Bicycle, section, StringComparison.OrdinalIgnoreCase);
            var isBasketball = string.Equals(GamesAlias.BasketBall, section, StringComparison.OrdinalIgnoreCase);

            var result = new MemoryStream();
            using (var workBook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = culture == CultEnum.He_IL })
            {
                var ws = workBook.AddWorksheet(Messages.ImportPlayers);
                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                #region Header

                addCell(Messages.FirstName);
                addCell(Messages.LastName);
                if (!isRowing)
                {
                    if (isGymnastics)
                    {
                        addCell(Messages.Disciplines);
                    }
                    else
                    {
                        addCell(Messages.Team);
                    }
                }

                if (!isIndividual)
                {
                    addCell(Messages.League);
                }

                if (!isGymnastics)
                {
                    addCell(Messages.Image);
                }

                addCell(Messages.BirthDay);

                if (!isIndividual)
                {                   
                    addCell(Messages.ImportPlayers_Columns_JerseyNO);
                    addCell(Messages.Position);
                    addCell(Messages.ShirtSize);
                }
                if (isAthletic)
                {
                    addCell(Messages.AthleteNumber);
                }

                addCell(Messages.IdentNum);
                addCell(Messages.Email);
                addCell(Messages.Phone);
                addCell(Messages.City);

                if (!isGymnastics && !isRowing)
                {
                    if (!isAthletic)
                    {
                        addCell(Messages.Insurance);
                    }
                    addCell(Messages.Height);
                    addCell(Messages.Weight);
                }

                addCell(Messages.Gender);
                addCell(Messages.ParentName);

                if (!isGymnastics)
                {
                    addCell(Messages.IDFile);
                }

                addCell(Messages.ClubName);

                if (!isAthletic && !isWeightLifting && !isRowing)
                {
                    addCell(Messages.Disciplines);
                }
                
                addCell(Messages.Activity_BuildForm_UnionComment);
                addCell(Messages.Activity_BuildForm_ClubComment);
                if (!isGymnastics)
                {
                    addCell(Messages.MedicalCertificate);
                    addCell(Messages.Activity_BuildForm_ApproveMedicalCert);
                }
                if (isTennis)
                {
                    addCell(Messages.TenicardValidity);
                }
                addCell(Messages.Active);
                addCell($"{Messages.ApproveDate}");
                addCell(Messages.Blockaded);
                addCell(Messages.BlockadeEndDate);
                if (isIndividual)
                {
                    addCell($"{Messages.Competitions}");
                    addCell($"{Messages.InitialApprovalDate}");
                }
                if (isBicycle)
                {
                    addCell(Messages.FriendshipName);
                    addCell(Messages.FrienshipPriceTypeName);
                    addCell(Messages.RoadHeat);
                    addCell(Messages.MountainHeat);
                    addCell(Messages.RoadIronNumber);
                    addCell(Messages.MountainIronNumber);
                    addCell(Messages.UciId);
                    addCell(Messages.ChipNumber);
                    addCell(Messages.KitStatus);
                    addCell(Messages.RegistrationFormSigned);
                    addCell(Messages.TotalPrice);
                    addCell(Messages.IsPaid);
                }
                if (isBasketball)
                {
                    addCell(Messages.HandicapLevel);
                    addCell(Messages.StartPlaying);
                }

                #endregion

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                #region Body
                if (isAthletic)
                {
                    foreach (var row in rows)
                    {
                        if (string.IsNullOrWhiteSpace(row.LastName))
                        {
                            row.LastName = GetLastNameByFullName(row.FullName);
                        }
                    }
                    rows = rows.OrderBy(r => r.LastName);
                }
                
                foreach (var row in rows)
                {
                    GetNameSurname(row.FullName, out var firstName, out var lastName);

                    addCell(firstName);
                    addCell(lastName);
                    if (!isRowing)
                    {
                        addCell(row.TeamName);
                    }

                    if (!isIndividual)
                    {
                        addCell(row.LeagueName);
                    }

                    if (!isGymnastics)
                    {
                        addCell(!String.IsNullOrEmpty(row.PlayerImage) ? Messages.Yes : Messages.No);
                    }

                    addCell(row.Birthday != null ? row.Birthday.Value.ToString("dd/MM/yyyy") : string.Empty);

                    if (!isIndividual)
                    {
                        addCell(row.ShirtNum.ToString() ?? string.Empty);
                        addCell(row.Position ?? string.Empty);
                        addCell(row.ShirtSize ?? string.Empty);
                    }
                    if (isAthletic)
                    {
                        addCell(row.AthletesNumbers?.ToString() ?? string.Empty);
                    }

                    addCell(row.IdentNum ?? row.PassportNum ?? string.Empty);
                    addCell(row.Email ?? string.Empty);
                    addCell(row.Phone ?? string.Empty);
                    addCell(row.City ?? string.Empty);

                    if (!isGymnastics && !isRowing)
                    {
                        if (!isAthletic)
                        {
                            addCell(row.Insurance == true ? Messages.Yes : Messages.No);
                        }
                        addCell(row.Height?.ToString() ?? string.Empty);
                        addCell(row.Weight?.ToString() ?? string.Empty);
                    }

                    addCell(row.Gender);
                    addCell(row.ParentName ?? string.Empty);

                    if (!isGymnastics)
                    {
                        addCell(!String.IsNullOrEmpty(row.IDFile) ? Messages.Yes : Messages.No);
                    }

                    addCell(row.ClubName);

                    if (!isAthletic && !isWeightLifting && !isRowing)
                    {
                        addCell(row.DisciplinesNames ?? string.Empty);
                    }

                    addCell(!String.IsNullOrEmpty(row.UnionComment) ? row.UnionComment : string.Empty);
                    addCell(!String.IsNullOrEmpty(row.ClubComment) ? row.ClubComment : string.Empty);

                    if (!isGymnastics)
                    {
                        addCell(!String.IsNullOrEmpty(row.MedicalCertificateFile) ? Messages.Yes : Messages.No);
                        addCell(row.MedicalCertApproved == true ? Messages.Yes : Messages.No);
                    }
                    if (isTennis)
                    {
                        addCell(row.TenicardValidity);
                    }
                    addCell(GetPlayerStatus(row.IsActive == true, row.IsApproveChecked, row.IsNotApproveChecked));
                    addCell(row.ApprovalDate);
                    addCell(row.IsBlockaded == true ? Messages.Yes : Messages.No);
                    addCell(row.EndBlockadeDate.HasValue ? row.EndBlockadeDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty);

                    if (isIndividual)
                    {
                        addCell(row.CompetitionCount?.ToString() ?? string.Empty);
                        addCell(row.InitialApprovalDate);
                    }

                    if (isBicycle)
                    {
                        addCell(row.FriendshipTypeName);
                        addCell(CustomPriceHelper.GetFriendshipPriceTypeName(row.FriendshipPriceTypeId));
                        addCell(row.RoadHeat);
                        addCell(row.MountainHeat);
                        addCell(row.RoadIronNumber);
                        addCell(row.MountainIronNumber);
                        addCell(row.UciId);
                        addCell(row.ChipNumber);
                        addCell(PlayerKitHelper.GetKitName(row.KitStatusId));
                        addCell(string.IsNullOrWhiteSpace(row.ParentStatementFile) ? Messages.No : Messages.Yes);
                        addCell(row.FriendshipTotalPrice?.ToString() ?? string.Empty);
                        addCell(row.FriendshipPaid ? Messages.Yes : Messages.No);
                    }
                    if (isBasketball)
                    {
                        addCell(row.BaseHandicap.ToString());
                        addCell(row.StartPlaying.ToString());
                    }

                    rowCounter++;
                    columnCounter = 1;
                }

                ws.Columns().AdjustToContents();
                workBook.SaveAs(result);
                result.Position = 0;

                #endregion

            }
            return result;
        }

        public int ImportTennisCompetitionRegistrations(int leagueId, int seasonId, List<ImportTennisCompetitionPlayerModel> correctRows,
            out List<ImportTennisCompetitionPlayerModel> importErrorRows)
        {
            var importedRowCount = 0;
            importErrorRows = new List<ImportTennisCompetitionPlayerModel>();
            foreach (var player in correctRows)
            {
                var hasValidationErrors = false;
                var sportsmanInSystem = _playersRepo.GetUserByIdentNum(player.IdentNumber);
                if (sportsmanInSystem == null)
                {
                    player.RowErrors.Add(new KeyValuePair<string, string>(string.Empty, Messages.PlayerNotExists));
                    hasValidationErrors = true;
                }

                var league = _leagueRepo.GetById(leagueId);
                var unionId = league?.UnionId ?? league?.Club?.UnionId;

                var club = _clubsRepo.GetByNumber(player.ClubNumber, seasonId, unionId);

                if (club == null)
                {
                    player.RowErrors.Add(new KeyValuePair<string, string>(string.Empty, $"{Messages.ImportGymnastics_ClubDoesntExist} {player.ClubNumber}"));
                    hasValidationErrors = true;
                }

                if (hasValidationErrors)
                {
                    importErrorRows.Add(player);
                    continue;
                }

                _db.CompetitionRegistrations.Add(new CompetitionRegistration
                {
                    UserId = sportsmanInSystem.UserId,
                    ClubId = club.ClubId,
                    LeagueId = leagueId,
                    SeasonId = seasonId,
                    IsRegisteredByExcel = true,
                    IsActive = true,
                });
                importedRowCount++;
            }
            _db.SaveChanges();

            return importedRowCount;
        }

        public Stream BuildErrorFileForTennisCompetition(List<ImportTennisCompetitionPlayerModel> errorRows, CultEnum culture = CultEnum.En_US)
        {
            var result = new MemoryStream();
            using (var workBook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = culture == CultEnum.He_IL })
            {
                #region Error row
                if (errorRows != null && errorRows.Count > 0)
                {
                    var wsError = workBook.AddWorksheet(Messages.ImportPlayers_ErrorWorksheetName);
                    wsError.Cell(1, 1).Value = $"{Messages.FullName} *";
                    wsError.Cell(1, 1).RichText.SetFontColor(XLColor.Red);
                    wsError.Cell(1, 1).RichText.SetBold();
                    wsError.Cell(1, 2).Value = $"{Messages.BirthDay} (dd/mm/yyyy) *";
                    wsError.Cell(1, 2).RichText.SetFontColor(XLColor.Red);
                    wsError.Cell(1, 2).RichText.SetBold();
                    wsError.Cell(1, 3).Value = $"{Messages.Gender} *"; ;
                    wsError.Cell(1, 3).RichText.SetFontColor(XLColor.Red);
                    wsError.Cell(1, 3).RichText.SetBold();
                    wsError.Cell(1, 4).Value = $"{Messages.IdentNum} ({Messages.NineDigits})";
                    wsError.Cell(1, 4).RichText.SetFontColor(XLColor.Red);
                    wsError.Cell(1, 4).RichText.SetBold();
                    wsError.Cell(1, 5).Value = $"{Messages.MedicalExam} ({Messages.Yes}/{Messages.No}";
                    wsError.Cell(1, 6).Value = $"{Messages.TypeOfInsurance} ({Messages.Private.ToLower()}/{Messages.School.ToLower()}/{Messages.Club.ToLower()}";
                    wsError.Cell(1, 7).Value = Messages.InsuranceStartDate;
                    wsError.Cell(1, 8).Value = $"{Messages.ClubNumber} *";
                    wsError.Cell(1, 8).RichText.SetFontColor(XLColor.Red);
                    wsError.Cell(1, 8).RichText.SetBold();

                    wsError.Cell(1, 10).Value = Messages.Reason;

                    wsError.Columns(1, 10).AdjustToContents();
                    wsError.Column(10).Width = 80;

                    for (var i = 0; i < errorRows.Count; i++)
                    {
                        var row = errorRows[i].OriginalRow;

                        for (var j = 0; j < 8; j++)
                        {
                            wsError.Cell(i + 2, j + 1).DataType = row.Cell(j + 1).DataType;
                            wsError.Cell(i + 2, j + 1).SetValue(row.Cell(j + 1).Value);
                        }

                        if (errorRows[i].RowErrors.Count > 0)
                        {
                            foreach (var er in errorRows[i].RowErrors)
                            {
                                var line = wsError.Cell(i + 2, 10).RichText.AddNewLine();
                                line.AddText(string.Format("{0} - {1}", er.Key, er.Value));
                            }
                            wsError.Cell(i + 2, 10).Style.Alignment.WrapText = true;
                        }
                    }
                }
                #endregion

                workBook.SaveAs(result);
                result.Position = 0;
            }

            return result;
        }

        public void ExtractTennisCompetitionPlayersData(Stream stream, out List<ImportTennisCompetitionPlayerModel> correctRows, out List<ImportTennisCompetitionPlayerModel> validationErrorRows)
        {
            correctRows = new List<ImportTennisCompetitionPlayerModel>();
            validationErrorRows = new List<ImportTennisCompetitionPlayerModel>();
            var rowsCount = 1;
            using (var workBook = new XLWorkbook(stream))
            {
                var workSheet = workBook.Worksheet(1);
                foreach (var row in workSheet.Rows())
                {
                    if (rowsCount >= 4 && row.CellsUsed().Any())
                    {
                        if (ValidateTennisCompetitionRow(row, out var validatedRow))
                        {
                            correctRows.Add(validatedRow);
                        }
                        else
                        {
                            validationErrorRows.Add(validatedRow);
                        }
                    }
                    rowsCount++;
                }
            }
        }

        private bool ValidateTennisCompetitionRow(IXLRow row, out ImportTennisCompetitionPlayerModel model)
        {
            model = new ImportTennisCompetitionPlayerModel(row);

            #region 1 - *Full name

            var fullNameString = row.Cell(1).Value.ToString();

            if (string.IsNullOrWhiteSpace(fullNameString))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Fullname, Messages.ImportGymnastics_FullNameRequired));
            }
            else
            {
                model.FullName = fullNameString;
            }

            #endregion

            #region 2 - *Birth date.

            var birthDate = DateTime.MinValue;
            if (row.Cell(2).DataType == XLDataType.DateTime)
            {
                birthDate = row.Cell(2).GetDateTime();
            }
            else
            {
                var birthDateStr = row.Cell(2).Value.ToString();

                if (!string.IsNullOrEmpty(birthDateStr))
                {
                    if (!DateTime.TryParseExact(birthDateStr, "dd/MM/yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out birthDate))
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Birthday, Messages.ImportPlayers_BirthdayIncorrectFormat));
                    }
                }
            }

            model.Birthday = birthDate;
            #endregion

            #region 3 - *Gender

            var genderString = row.Cell(3).Value.ToString();

            if (string.IsNullOrWhiteSpace(fullNameString))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Fullname, Messages.ImportPlayers_Columns_Fullname));
            }
            else
            {
                var genderId = LangHelper.GetGenderId(genderString);
                if (!genderId.HasValue)
                {
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Gender, Messages.NoGenderInSystem));
                }
                else
                {
                    model.GenderId = genderId.Value;
                }
            }


            #endregion

            #region 4 - *ID number.
            var idStr = row.Cell(4).Value.ToString();
            var idSb = new System.Text.StringBuilder();
            var id = string.Empty;
            if (string.IsNullOrWhiteSpace(idStr))
            {
                //return false;
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.IdentNum, Messages.ImportGymnastics_IdentNumRequired));
            }
            else
            {
                idStr = idStr.Trim();
                foreach (var ch in idStr)
                {
                    var tmp = 0;
                    if (!int.TryParse(ch.ToString(), out tmp))
                    {
                        //return false;
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_ShouldBeNumber));
                    }
                    idSb.Append(ch);
                }
                var idLength = idSb.Length;
                if (idLength > 9)
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_IDNumberMaxLength));
                }

                while (idSb.Length < 9)
                {
                    idSb.Insert(0, "0");
                }
                id = idSb.ToString();
            }
            model.IdentNumber = id;
            #endregion

            #region 5 - Medical exam


            var medicalExam = DateTime.MinValue;
            if (row.Cell(5).DataType == XLDataType.DateTime)
            {
                medicalExam = row.Cell(5).GetDateTime();
                model.MedicalExam = medicalExam;
            }
            else
            {
                var medicalExamString = row.Cell(5).Value.ToString();

                if (!string.IsNullOrEmpty(medicalExamString))
                {
                    if (!DateTime.TryParseExact(medicalExamString, "dd/MM/yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out medicalExam))
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Birthday, Messages.ImportPlayers_BirthdayIncorrectFormat));
                    }
                    model.MedicalExam = medicalExam;
                }
                else
                {
                    model.MedicalExam = null;
                }
            }

            #endregion

            #region 6 - Type of insurance

            var typeOfInsuranceString = row.Cell(6).Value.ToString();

            if (!string.IsNullOrWhiteSpace(typeOfInsuranceString))
            {
                if (typeOfInsuranceString.Equals(Messages.Private, StringComparison.OrdinalIgnoreCase))
                {
                    model.TypeOfInsurance = InsuranceType.Private;
                }
                else if (typeOfInsuranceString.Equals(Messages.School, StringComparison.OrdinalIgnoreCase))
                {
                    model.TypeOfInsurance = InsuranceType.School;
                }
                else if (typeOfInsuranceString.Equals(Messages.Club, StringComparison.OrdinalIgnoreCase))
                {
                    model.TypeOfInsurance = InsuranceType.Club;
                }
                else
                {
                    model.TypeOfInsurance = InsuranceType.None;
                }
            }

            #endregion

            #region 7 - StartDateOfInsurance

            var insuranceStart = DateTime.MinValue;
            if (row.Cell(7).DataType == XLDataType.DateTime)
            {
                insuranceStart = row.Cell(7).GetDateTime();
                model.InsuranceStartDate = insuranceStart;
            }
            else
            {
                var insuranceStartStr = row.Cell(7).Value.ToString();

                if (!string.IsNullOrEmpty(insuranceStartStr))
                {
                    if (!DateTime.TryParseExact(insuranceStartStr, "dd/MM/yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out insuranceStart))
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.InsuranceStartDate, Messages.InsuranceStartDateInccorrect));
                    }
                }
                model.InsuranceStartDate = insuranceStart;
            }
            #endregion

            #region 8 – * Club number
            var clubNumberStr = row.Cell(8).Value.ToString();
            var clubNumber = 0;
            if (string.IsNullOrWhiteSpace(clubNumberStr))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ClubNumber, Messages.ImportGymnastics_ClubNumberRequired));
            }
            else
            {
                if (!int.TryParse(row.Cell(8).Value.ToString(), out clubNumber))
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ClubNumber, Messages.ImportGymnastics_ClubNumberShouldBeNumber));
                }
            }
            model.ClubNumber = clubNumber;

            #endregion

            return model.RowErrors.Count == 0;
        }

        private string GetPlayerStatus(bool isActive, bool isApproveChecked, bool isNotApproveChecked)
        {
            var playerStatus = string.Empty;

            if (isActive && isApproveChecked)
            {
                playerStatus = Messages.ApprovedColumn;
            }
            else if (isActive && isNotApproveChecked)
            {
                playerStatus = Messages.NotApproved;
            }
            else if (isActive && !isApproveChecked && !isNotApproveChecked)
            {
                playerStatus = Messages.Waiting;
            }
            else if (!isActive)
            {
                playerStatus = Messages.NotActiveColumn;
            }

            return playerStatus;
        }

        private void GetNameSurname(string fullName, out string name, out string surname)
        {
            name = string.Empty;
            surname = string.Empty;
            var fullNameStrings = new List<string>();

            if (!string.IsNullOrEmpty(fullName))
            {
                fullNameStrings = fullName.Split(' ').ToList();
            }

            if (!string.IsNullOrEmpty(fullName) && fullNameStrings.Count() > 0)
            {
                for (var i = 0; i < fullNameStrings.Count(); i++)
                {
                    if (i == 0)
                    {
                        name = fullNameStrings[i];
                    }
                    else
                    {
                        surname += fullNameStrings[i] + " ";
                    }
                }
            }
        }

        public Stream ExportPlayers(IEnumerable<TeamPlayerItem> rows, IEnumerable<Position> positions, Club club, League league, bool check = false, bool IsHandicapEnabled = false)
        {
            var result = new MemoryStream();
            var IsAthletics = club?.Union?.Section?.Alias == GamesAlias.Athletics;
            var IsCatchball = league?.Union?.Section?.Alias == GamesAlias.NetBall;
            var IsBasketball = league?.Union?.Section?.Alias == GamesAlias.BasketBall;


            using (var workBook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheetName = Messages.CommonPlayersList;
                if (IsAthletics) {
                    worksheetName = worksheetName.Replace(Messages.Players, Messages.Athletes);
                }
                var ws = workBook.AddWorksheet(worksheetName);
                var isSectionClub = false;
                if (club != null)
                {
                    isSectionClub = !club.UnionId.HasValue;
                }
                else
                {
                    if (league != null && league.Club != null)
                    {
                        isSectionClub = !league.Club.UnionId.HasValue;
                    }
                }
                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                #region Header
                addCell("#");
                addCell(Messages.FirstName);
                addCell(Messages.LastName);
                if (IsAthletics)
                {
                    addCell(Messages.AthleteNumber);
                }
                if (!IsAthletics)
                {
                    addCell(Messages.LeagueName);
                }
                if (IsAthletics)
                {
                    addCell($"{Messages.SubClub}");
                }
                else
                {
                    addCell($"{Messages.Team} {Messages.Name}");
                }
                if (!IsAthletics)
                    addCell(Messages.Shirt);

                addCell(Messages.ShirtSize);
                if (!IsAthletics)
                    addCell(Messages.Position);
                
                addCell(Messages.IdentNum);
                addCell(Messages.Email);
                addCell(Messages.Phone);
                addCell(Messages.City);
                if (IsBasketball)
                {
                    addCell(Messages.HandicapLevel);
                    addCell(Messages.StartPlaying);
                }
                addCell(Messages.BirthDay);
                if (!IsCatchball)
                {
                addCell(Messages.Gender);
                }
                addCell(Messages.MedicalCertificate);
                addCell(Messages.MedicalCertificateApproval);
                if (!IsAthletics)
                {
                    addCell(Messages.Insurance);
                    addCell(Messages.LeagueDetail_PlayerRegistrationPrice);
                    addCell(Messages.LeagueDetail_PlayerInsurancePrice);
                }
                if (rows.Any(x => x.MembersFee > 0))
                {
                    addCell(Messages.LeagueDetail_MemberFees);
                }
                if (rows.Any(x => x.HandlingFee > 0))
                {
                    addCell(Messages.LeagueDetail_HandlingFee);
                }
                if (!IsAthletics)
                {
                    addCell(Messages.TeamPlayers_ManagerRegistrationDiscount);
                    addCell(Messages.TeamPlayers_NoInsurance);
                }
                addCell(Messages.Active);
                addCell(Messages.Approved);
                addCell($"{Messages.Image}");
                if (isSectionClub)
                {
                    addCell(Messages.TeamDetails_ParticipationPrice);
                    addCell(Messages.TeamPlayers_ManagerParticipationDiscount);
                    addCell(Messages.FinalParticipationPrice);
                }
                if (!IsAthletics)
                    addCell(Messages.Paid);
                if (rows.Any(x => !string.IsNullOrWhiteSpace(x.Registration?.CustomPrices)))
                {
                    addCell(Messages.Activity_CustomPrices_Paid);
                }
                if (isSectionClub)
                {
                    addCell(Messages.Activity_BuildForm_Comments);
                }
                addCell(string.Empty);

                rowCounter++;
                columnCounter = 1;
                #endregion

                ws.Columns().AdjustToContents();

                foreach (var row in rows)
                {
                    var completed = true;

                    if (check)
                    {
                        if (string.IsNullOrWhiteSpace(row.FullName))
                        {
                            completed = false;
                            var line = ws.Cell(rowCounter, ws.ColumnsUsed().Count()).RichText.AddNewLine();
                            line.AddText(string.Format("{0} - {1}", Messages.Name, Messages.PropertyValueRequired));
                        }

                            //if (string.IsNullOrWhiteSpace(row.ShirtSize))
                            //{
                            //    completed = false;
                            //    var line = ws.Cell(rowNum, 17).RichText.AddNewLine();
                            //    line.AddText(string.Format("{0} - {1}", Messages.ShirtSize, Messages.PropertyValueRequired));
                            //}
                            //if (row.PosId == null)
                            //{
                            //    completed = false;
                            //    var line = ws.Cell(rowNum, 17).RichText.AddNewLine();
                            //    line.AddText(string.Format("{0} - {1}", Messages.Position, Messages.PropertyValueRequired));
                            //}
                        if (string.IsNullOrWhiteSpace(row.IdentNum))
                        {
                            completed = false;
                            var line = ws.Cell(rowCounter, ws.ColumnsUsed().Count()).RichText.AddNewLine();
                            line.AddText(string.Format("{0} - {1}", Messages.IdentNum, Messages.PropertyValueRequired));
                        }
                        //if (string.IsNullOrWhiteSpace(row.Email))
                        //{
                        //    completed = false;
                        //    var line = ws.Cell(rowNum, 17).RichText.AddNewLine();
                        //    line.AddText(string.Format("{0} - {1}", Messages.Email, Messages.PropertyValueRequired));
                        //}
                        //if (string.IsNullOrWhiteSpace(row.Telephone))
                        //{
                        //    completed = false;
                        //    var line = ws.Cell(rowNum, 17).RichText.AddNewLine();
                        //    line.AddText(string.Format("{0} - {1}", Messages.Phone, Messages.PropertyValueRequired));
                        //}
                        //if (string.IsNullOrWhiteSpace(row.City))
                        //{
                        //    completed = false;
                        //    var line = ws.Cell(rowNum, 17).RichText.AddNewLine();
                        //    line.AddText(string.Format("{0} - {1}", Messages.City, Messages.PropertyValueRequired));
                        //}
                        if (row.Birthday == null || row.Birthday == DateTime.MinValue)
                        {
                            completed = false;
                            var line = ws.Cell(rowCounter, ws.ColumnsUsed().Count()).RichText.AddNewLine();
                            line.AddText(string.Format("{0} - {1}", Messages.BirthDay, Messages.PropertyValueRequired));
                        }
                        if (string.IsNullOrWhiteSpace(row.MedicalCertificateFile))
                        {
                            completed = false;
                            var line = ws.Cell(rowCounter, ws.ColumnsUsed().Count()).RichText.AddNewLine();
                            line.AddText(string.Format("{0} - {1}", Messages.MedicalCertificate, Messages.PropertyValueRequired));
                        }

                        if (string.IsNullOrWhiteSpace(row.InsuranceFile))
                        {
                            completed = false;
                            var line = ws.Cell(rowCounter, ws.ColumnsUsed().Count()).RichText.AddNewLine();
                            line.AddText(string.Format("{0} - {1}", Messages.Insurance, Messages.PropertyValueRequired));
                        }

                        ws.Cell(rowCounter, ws.ColumnsUsed().Count()).Style.Alignment.WrapText = true;
                    }
                    else
                    {
                        completed = false;
                    }

                    if (!completed)
                    {
                        addCell(row.UserId.ToString());
                        GetNameSurname(row.FullName, out var firstName, out var lastName);
                        addCell(row.IsTrainerPlayer ? $"{Messages.Player_TrainerIndicator} {row.FirstName}" : row.FirstName);
                        addCell(lastName);
                        if (!IsAthletics)
                            addCell(row.LeagueName);
                        if (IsAthletics)
                            addCell(row.AthleteNumber.HasValue? row.AthleteNumber.Value.ToString() : "");
                        addCell(row.TeamName);
                        if (!IsAthletics)
                            addCell(row.ShirtNum.ToString());
                        addCell(row.ShirtSize);
                        if (!IsAthletics)
                            addCell(positions.FirstOrDefault(p => p.PosId == row.PosId)?.Title);
                        addCell(row.IdentNum);
                        addCell(row.Email);
                        AddTextCell(rowCounter, ref columnCounter, row.Telephone, ws);
                        addCell(row.City);
                        if (IsBasketball)
                        {
                            addCell(row.BaseHandicap.ToString());
                            addCell(row.StartPlaying.ToString());
                        }
                        addCell(row.Birthday != null ? row.Birthday.Value.ToString("dd/MM/yyyy") : "");
                        if (!IsCatchball)
                        addCell(row.Gender?.Title ?? "Female");
                        addCell((!string.IsNullOrWhiteSpace(row.MedicalCertificateFile) || (row.IsApprovedByManager.HasValue && row.IsApprovedByManager.Value)) ? Messages.Yes : Messages.No);
                        addCell(row.MedicalCertificate.HasValue && row.MedicalCertificate.Value ? @Messages.Approved : @Messages.NotApproved);
                        if (!IsAthletics)
                        {
                            addCell(string.IsNullOrWhiteSpace(row.InsuranceFile) == false ? Messages.Yes : Messages.No);

                            if (row.Registration != null)
                            {
                                addCell(row.Registration.RegistrationPrice.ToString(CultureInfo.CurrentCulture));
                            }
                            else if (club != null && club.Union == null)
                            {
                                addCell(row.PlayerRegistrationAndEquipmentPrice.ToString(CultureInfo.CurrentCulture));
                            }
                            else
                            {
                                addCell(row.PlayerRegistrationPrice.ToString(CultureInfo.CurrentCulture));
                            }

                            addCell(row.Registration?.InsurancePrice.ToString(CultureInfo.CurrentCulture) ?? row.PlayerInsurancePrice.ToString(CultureInfo.CurrentCulture));

                            if (rows.Any(x => x.MembersFee > 0))
                            {
                                addCell(row.Registration?.MembersFee.ToString(CultureInfo.CurrentCulture) ??
                                        row.MembersFee.ToString(CultureInfo.CurrentCulture));
                            }
                            if (rows.Any(x => x.HandlingFee > 0))
                            {
                                addCell(row.Registration?.HandlingFee.ToString(CultureInfo.CurrentCulture) ??
                                        row.HandlingFee.ToString(CultureInfo.CurrentCulture));
                            }

                            addCell(row.ManagerRegistrationDiscount.ToString(CultureInfo.CurrentCulture));
                            addCell(row.NoInsurancePayment ? Messages.Yes : Messages.No);
                        }
                        addCell(row.IsActive ? Messages.Yes : Messages.No);
                        addCell(row.IsPlayerRegistrationApproved || row.IsApprovedByManager == true ? Messages.Yes : Messages.No);
                        addCell(!string.IsNullOrEmpty(row.PlayerImage) ? Messages.Yes : Messages.No);
                        if (!IsAthletics)
                        {
                            if (isSectionClub)
                            {
                                addCell(row.ParticipationPrice.ToString());
                                addCell(row.ManagerParticipationDiscount.ToString());
                                addCell(row.FinalParticipationPrice.ToString());
                            }

                            if (isSectionClub)
                            {
                                var paid = row.Registration?.Paid == 0
                                      ? (club != null
                                          ? row.Registration?.RegistrationPaid + row.Registration?.InsurancePaid + row.Registration?.ParticipationPaid
                                          : row.Registration?.RegistrationPaid + row.Registration?.InsurancePaid)
                                      : row.Registration?.Paid ?? row.TeamPlayerPaid;
                                addCell(paid.ToString());
                            }
                            else
                            {
                                addCell((row.Registration?.MembersFeePaid +
                                 row.Registration?.HandlingFeePaid +
                                 row.Registration?.InsurancePaid +
                                 row.Registration?.ParticipationPaid +
                                 row.Registration?.RegistrationPaid).ToString());
                            }

                            if (rows.Any(x => !string.IsNullOrWhiteSpace(x.Registration?.CustomPrices)))
                            {
                                if (row.Registration?.CustomPrices != null)
                                {
                                    var customPrices = JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(row.Registration?.CustomPrices);
                                    var sum = customPrices.Sum(x => x.Paid);
                                    addCell(sum.ToString());
                                }
                                else
                                {
                                    addCell(string.Empty);
                                }
                            }
                        }
                        if (isSectionClub)
                        {
                            addCell(row.Comment);
                        }
                        rowCounter++;
                        columnCounter = 1;
                    }
                }
                ws.Columns().AdjustToContents();
                workBook.Worksheets.FirstOrDefault().RightToLeft = true;
                workBook.SaveAs(result);
                result.Position = 0;
            }

            return result;
        }

        private void AddTextCell(int rowCounter, ref int columnCounter, string value, IXLWorksheet ws)
        {
            ws.Cell(rowCounter, columnCounter).Value = value;
            ws.Cell(rowCounter, columnCounter).DataType = XLDataType.Text;
            columnCounter++;
        }


        public Stream ExportAllWorkers(
            IEnumerable<UserJobDto> vmUsers,
            bool isMultiDiscipline,
            string sportName,
            CultEnum culture)
        {
            var isKarate = string.Equals(sportName, "Karate", StringComparison.CurrentCultureIgnoreCase);

            var result = new MemoryStream();
            var rows = vmUsers.GroupBy(p => p.UserId).Select(p => new
            {
                UserId = p.First().UserId,
                Id = p.First().Id,
                FullName = p.First().FullName,
                JobName = string.Join(", ", p.Select(j => j.JobName).ToArray()),
                Email = p.First().Email,
                Phone = p.First().Phone,
                BirthDay = p.First().BirthDate,
                Address = p.First().Address,
                City = p.First().City,
                IdentNum = p.First().IdentNum,
                ClubRelated = p.First().ConnectedClubName,
                TeamName = p.First().TeamName,
                ClubName = p.First().ClubName,
                DisciplinesRelated = p.First().DisciplinesRelatedNames,
                PaymentRateType = p.First().PaymentRateType,
                IsraelRefereeRanks = p.First().KarateRefereeRanks.Where(c => c.Type.Contains("Israel")),
                WkfRefereeRanks = p.First().KarateRefereeRanks.Where(c => c.Type.Contains("WKF")),
                EkfRefereeRanks = p.First().KarateRefereeRanks.Where(c => c.Type.Contains("EKF")),
            });

            using (var workBook = new XLWorkbook(XLEventTracking.Disabled){RightToLeft = culture == CultEnum.He_IL})
            {
                var ws = workBook.AddWorksheet(Messages.ExportOfficials);

                var rowNum = 1;
                var colNum = 1;
                Action<object> addCell = value =>
                {
                    ws.Cell(rowNum, colNum).SetValue(value?.ToString() ?? string.Empty);
                    colNum++;
                };

                #region Header

                addCell("#");
                addCell(Messages.IdentNum);
                addCell(Messages.Name);
                addCell(Messages.Role);
                addCell(Messages.Email);
                addCell(Messages.Phone);
                addCell(Messages.BirthDay);
                addCell(Messages.Address);
                addCell(Messages.City);

                if (!isKarate)
                {
                    addCell(Messages.TeamName);
                }

                addCell(Messages.Club);
                
                if (isMultiDiscipline)
                {
                    addCell(Messages.ConnectedClub);
                    addCell(Messages.ConnectedDisciplines);
                }

                if (isKarate)
                {
                    addCell(Messages.ConnectedClub);
                    addCell(Messages.PaymentRateType);
                    addCell($"{Messages.IsraelLicense} - {Messages.Kumite}");
                    addCell($"{Messages.IsraelLicense} - {Messages.Kata}");
                    addCell($"{Messages.EKFLicense} - {Messages.Kumite}");
                    addCell($"{Messages.EKFLicense} - {Messages.Kata}");
                    addCell($"{Messages.WKFLiscense} - {Messages.Kumite}");
                    addCell($"{Messages.WKFLiscense} - {Messages.Kata}");
                }

                #endregion

                ws.Columns(1, 12).AdjustToContents();

                #region Body

                foreach (var row in rows)
                {
                    rowNum++;
                    colNum = 1;

                    addCell(row.Id);
                    addCell(row.IdentNum);
                    addCell(row.FullName);
                    addCell(row.JobName);
                    addCell(row.Email);
                    addCell(row.Phone);
                    addCell(row.BirthDay);
                    addCell(row.Address);
                    addCell(row.City);

                    if (!isKarate)
                    {
                        addCell(row.TeamName);
                    }

                    addCell(row.ClubName);

                    if (isMultiDiscipline)
                    {
                        addCell(row.ClubRelated);
                        addCell(row.DisciplinesRelated);
                    }

                    if (isKarate)
                    {
                        addCell(row.ClubRelated);
                        switch (row.PaymentRateType?.ToLower())
                        {
                            case "ratea":
                                addCell(Messages.RateA);
                                break;
                            case "rateb":
                                addCell(Messages.RateB);
                                break;
                            default:
                                addCell(Messages.DefaultRate);
                                break;
                        }

                        var sb = new StringBuilder();

                        foreach (var rank in row.IsraelRefereeRanks.Where(x => x.Type.Contains("Kumite") || x.Type.Contains("Referee")))
                        {
                            sb.AppendLine($"{UIHelpers.GetRankName(rank.Type)} - {rank.Date?.ToShortDateString() ?? string.Empty}");
                        }
                        addCell(sb);
                        sb.Clear();

                        foreach (var rank in row.IsraelRefereeRanks.Where(x => x.Type.Contains("Kata")))
                        {
                            sb.AppendLine($"{UIHelpers.GetRankName(rank.Type)} - {rank.Date?.ToShortDateString() ?? string.Empty}");
                        }
                        addCell(sb);
                        sb.Clear();

                        foreach (var rank in row.EkfRefereeRanks.Where(x => x.Type.Contains("Kumite") || x.Type.Contains("Referee")))
                        {
                            sb.AppendLine($"{UIHelpers.GetRankName(rank.Type)} - {rank.Date?.ToShortDateString() ?? string.Empty}");
                        }
                        addCell(sb);
                        sb.Clear();

                        foreach (var rank in row.EkfRefereeRanks.Where(x => x.Type.Contains("Kata")))
                        {
                            sb.AppendLine($"{UIHelpers.GetRankName(rank.Type)} - {rank.Date?.ToShortDateString() ?? string.Empty}");
                        }
                        addCell(sb);
                        sb.Clear();

                        foreach (var rank in row.WkfRefereeRanks.Where(x => x.Type.Contains("Kumite") || x.Type.Contains("Referee")))
                        {
                            sb.AppendLine($"{UIHelpers.GetRankName(rank.Type)} - {rank.Date?.ToShortDateString() ?? string.Empty}");
                        }
                        addCell(sb);
                        sb.Clear();

                        foreach (var rank in row.WkfRefereeRanks.Where(x => x.Type.Contains("Kata")))
                        {
                            sb.AppendLine($"{UIHelpers.GetRankName(rank.Type)} - {rank.Date?.ToShortDateString() ?? string.Empty}");
                        }
                        addCell(sb);
                        sb.Clear();
                    }
                }

                #endregion

                ws.Columns().AdjustToContents();
                ws.Rows().AdjustToContents();
                workBook.SaveAs(result);
                result.Position = 0;
            }
            return result;
        }

        public void ExtractGymnasticsData(Stream inputStream, out List<ImportGymnasticRegistrationModel> correctRows, out List<ImportGymnasticRegistrationModel> validationErrorRows)
        {
            correctRows = new List<ImportGymnasticRegistrationModel>();
            validationErrorRows = new List<ImportGymnasticRegistrationModel>();
            var isFirstRow = true;
            using (var workBook = new XLWorkbook(inputStream))
            {
                var workSheet = workBook.Worksheet(1);
                foreach (var row in workSheet.Rows())
                {
                    if (!isFirstRow && !row.IsEmpty())
                    {
                        ImportGymnasticRegistrationModel validatedRow;
                        if (ValidateGymnasticsRow(row, out validatedRow))
                        {
                            correctRows.Add(validatedRow);
                        }
                        else
                        {
                            validationErrorRows.Add(validatedRow);
                        }
                    }
                    else
                    {
                        isFirstRow = false;
                    }
                }
            }
        }

        private bool ValidateGymnasticsRow(IXLRow row, out ImportGymnasticRegistrationModel model)
        {
            model = new ImportGymnasticRegistrationModel(row);

            #region 1 – * Club number
            var clubNumberStr = row.Cell(1).Value.ToString();
            var clubNumber = 0;
            if (string.IsNullOrWhiteSpace(clubNumberStr))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ClubNumber, Messages.ImportGymnastics_ClubNumberRequired));
            }
            else
            {
                if (!int.TryParse(row.Cell(1).Value.ToString(), out clubNumber))
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ClubNumber, Messages.ImportGymnastics_ClubNumberShouldBeNumber));
                }
            }

            #endregion

            #region 2 - Club name

            var clubNameString = row.Cell(2).Value.ToString();

            #endregion

            var firstNameString = row.Cell(3).Value.ToString();
            model.FirstName = firstNameString;
            var lastNameString = row.Cell(4).Value.ToString();
            model.LastName = lastNameString;
            var fullNameString = row.Cell(5).Value.ToString();
            model.FullName = fullNameString;

            #region 6 - *ID number.
            var idStr = row.Cell(6).Value.ToString();
            var idSb = new System.Text.StringBuilder();
            var id = string.Empty;
            if (string.IsNullOrWhiteSpace(idStr))
            {
                //return false;
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.IdentNum, Messages.ImportGymnastics_IdentNumRequired));
            }
            else
            {
                idStr = idStr.Trim();
                /*
                foreach (var ch in idStr)
                {
                    var tmp = 0;
                    if (!int.TryParse(ch.ToString(), out tmp))
                    {
                        //return false;
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_ShouldBeNumber));
                    }
                    idSb.Append(ch);
                }
                var idLength = idSb.Length;
                if (idLength > 9)
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_IDNumberMaxLength));
                }

                while (idSb.Length < 9)
                {
                    idSb.Insert(0, "0");
                }
                */
                id = idStr.ToString();
            }
            #endregion

            #region 7 - Birth date.

            var birthDate = DateTime.MinValue;
            if (row.Cell(7).DataType == XLDataType.DateTime)
            {
                birthDate = row.Cell(7).GetDateTime();
            }
            else
            {
                var birthDateStr = row.Cell(7).Value.ToString();

                if (!string.IsNullOrEmpty(birthDateStr))
                {
                    if (!DateTime.TryParseExact(birthDateStr, "dd-MM-yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out birthDate))
                    {
                        //return false;
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Birthday, Messages.ImportPlayers_BirthdayIncorrectFormat));
                    }
                    model.Birthday = birthDate;
                }
                else
                {
                    model.Birthday = null;
                }
            }
            #endregion
            #region 8 - Composition
            var compositionStr = row.Cell(8).Value.ToString();
            var composition = -1;
            if (!string.IsNullOrEmpty(compositionStr))
            {
                if (!string.IsNullOrEmpty(compositionStr))
                {
                    if (string.Equals(compositionStr, Messages.Yes, StringComparison.OrdinalIgnoreCase))
                    {
                        composition = 0;
                    }
                    else
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Composition, Messages.ValidValueForComposition));
                    }
                }
            }

            #endregion
            #region 9 - Composition2
            var composition2Str = row.Cell(9).Value.ToString();
            if (!string.IsNullOrEmpty(composition2Str))
            {
                if (!string.IsNullOrEmpty(composition2Str))
                {
                    if (string.Equals(composition2Str, Messages.Yes, StringComparison.OrdinalIgnoreCase))
                    {
                        composition = 1;
                    }
                    else
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Composition, Messages.ValidValueForComposition));
                    }
                }
            }

            #endregion
            #region 10 - Composition3
            var composition3Str = row.Cell(10).Value.ToString();
            if (!string.IsNullOrEmpty(composition3Str))
            {
                if (!string.IsNullOrEmpty(composition3Str))
                {
                    if (string.Equals(composition3Str, Messages.Yes, StringComparison.OrdinalIgnoreCase))
                    {
                        composition = 2;
                    }
                    else
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Composition, Messages.ValidValueForComposition));
                    }
                }
            }

            #endregion
            #region 11 - Composition4
            var composition4Str = row.Cell(11).Value.ToString();
            if (!string.IsNullOrEmpty(composition4Str))
            {
                if (!string.IsNullOrEmpty(composition4Str))
                {
                    if (string.Equals(composition3Str, Messages.Yes, StringComparison.OrdinalIgnoreCase))
                    {
                        composition = 3;
                    }
                    else
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Composition, Messages.ValidValueForComposition));
                    }
                }
            }

            #endregion
            #region 12 - Composition5
            var composition5Str = row.Cell(12).Value.ToString();
            if (!string.IsNullOrEmpty(composition5Str))
            {
                if (!string.IsNullOrEmpty(composition5Str))
                {
                    if (string.Equals(composition5Str, Messages.Yes, StringComparison.OrdinalIgnoreCase))
                    {
                        composition = 4;
                    }
                    else
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Composition, Messages.ValidValueForComposition));
                    }
                }
            }

            #endregion
            #region 13 - Composition6
            var composition6Str = row.Cell(13).Value.ToString();
            if (!string.IsNullOrEmpty(composition6Str))
            {
                if (!string.IsNullOrEmpty(composition6Str))
                {
                    if (string.Equals(composition6Str, Messages.Yes, StringComparison.OrdinalIgnoreCase))
                    {
                        composition = 5;
                    }
                    else
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Composition, Messages.ValidValueForComposition));
                    }
                }
            }

            #endregion
            #region 14 - Composition7
            var composition7Str = row.Cell(14).Value.ToString();
            if (!string.IsNullOrEmpty(composition7Str))
            {
                if (!string.IsNullOrEmpty(composition7Str))
                {
                    if (string.Equals(composition7Str, Messages.Yes, StringComparison.OrdinalIgnoreCase))
                    {
                        composition = 6;
                    }
                    else
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Composition, Messages.ValidValueForComposition));
                    }
                }
            }

            #endregion
            #region 15 - Composition8
            var composition8Str = row.Cell(15).Value.ToString();
            if (!string.IsNullOrEmpty(composition8Str))
            {
                if (!string.IsNullOrEmpty(composition8Str))
                {
                    if (string.Equals(composition8Str, Messages.Yes, StringComparison.OrdinalIgnoreCase))
                    {
                        composition = 7;
                    }
                    else
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Composition, Messages.ValidValueForComposition));
                    }
                }
            }
            #endregion
            #region 16 - Composition9
            var composition9Str = row.Cell(16).Value.ToString();
            if (!string.IsNullOrEmpty(composition9Str))
            {
                if (!string.IsNullOrEmpty(composition9Str))
                {
                    if (string.Equals(composition9Str, Messages.Yes, StringComparison.OrdinalIgnoreCase))
                    {
                        composition = 8;
                    }
                    else
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Composition, Messages.ValidValueForComposition));
                    }
                }
            }

            #endregion

            #region 17 - Composition10
            var composition10Str = row.Cell(17).Value.ToString();
            if (!string.IsNullOrEmpty(composition10Str))
            {
                if (!string.IsNullOrEmpty(composition10Str))
                {
                    if (string.Equals(composition10Str, Messages.Yes, StringComparison.OrdinalIgnoreCase))
                    {
                        composition = 9;
                    }
                    else
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Composition, Messages.ValidValueForComposition));
                    }
                }
            }

            #endregion


            #region 15 - 25 - Instruments/Orders

            for (var i = 0; i < 5; i++)
            {
                var instrument = row.Cell(15 + i * 2).Value.ToString();
                var orderStr = row.Cell(16 + i * 2).Value.ToString();
                var order = 0;
                if (!string.IsNullOrWhiteSpace(orderStr))
                {
                    if (!int.TryParse(orderStr, out order))
                    {
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Order, Messages.OrderShouldBeNumeric));
                    }
                }
                if (!string.IsNullOrEmpty(instrument))
                {
                    model.Instruments.Add(new GymnasticInstrumentImport
                    {
                        InstrumentName = instrument,
                        OrderNumber = order != 0 ? (int?)order : null
                    });
                }
            }

            #endregion

            #region 25 - Final score

            var scoreStr = row.Cell(25).Value.ToString();
            var score = double.TryParse(scoreStr, out var scoreResult) ? (double?)scoreResult : null;

            #endregion

            #region 26 - Position

            var positionStr = row.Cell(26).Value.ToString();
            var position = int.TryParse(positionStr, out var positionResult) ? (int?)positionResult : null;

            #endregion

            if(score.HasValue && score.Value == 0 && position.HasValue && position.Value == 0)
            {
                position = null;
                score = null;
            }

            model.ClubNumber = clubNumber;
            model.ClubName = clubNameString;
            model.IdentNum = id;
            model.FinalScore = score;
            model.Position = position;
            model.CompositionNumber = composition;

            return model.RowErrors.Count == 0;
        }

        public int ImportGymnasticsRegistrations(int seasonId, int leagueId, List<ImportGymnasticRegistrationModel> correctRows, out List<ImportGymnasticRegistrationModel> importErrorRows, out List<ImportGymnasticRegistrationModel> duplicatedRows)
        {
            var importedRowCount = 0;
            try
            {
                importErrorRows = new List<ImportGymnasticRegistrationModel>();
                duplicatedRows = new List<ImportGymnasticRegistrationModel>();
                foreach (var registration in correctRows)
                {
                    var hasError = false;
                    var league = _leagueRepo.GetById(leagueId);
                    var leagueDisciplineId = league?.DisciplineId;
                    var unionId = league?.UnionId ?? league?.Club?.UnionId;
                    var competitionDate = league.LeagueStartDate ?? DateTime.MaxValue;
                    var club = _clubsRepo.GetByNumber(registration.ClubNumber, seasonId, unionId);
                    var user = _playersRepo.GetGymnasticRegByIdentNumOrPassportNum(registration.IdentNum)
                        ?? _playersRepo.GetUserByIdentNumOrPassportNum(registration.IdentNum);
                    
                    var route = user?.UsersRoutes?.FirstOrDefault(r => r.DisciplineRoute.DisciplineId == leagueDisciplineId)?.DisciplineRoute
                        ?? league?.Discipline?.DisciplineRoutes?.FirstOrDefault()
                        ?? user?.PlayerDisciplines?.FirstOrDefault(r => r.DisciplineId == leagueDisciplineId)?.Discipline?.DisciplineRoutes?.FirstOrDefault()
                        ?? user?.PlayerDisciplines?.FirstOrDefault()?.Discipline?.DisciplineRoutes?.FirstOrDefault();

                    #region Errors

                    if (club == null)
                    {
                        registration.RowErrors.Add(new KeyValuePair<string, string>("", $"{Messages.ImportGymnastics_ClubDoesntExist} {registration.ClubNumber}"));
                        importErrorRows.Add(registration);
                        hasError = true;
                    }
                    if (user == null)
                    {
                        registration.RowErrors.Add(new KeyValuePair<string, string>("", Messages.PlayerNotExists.Replace(Messages.Player,Messages.Gymnast)));
                        importErrorRows.Add(registration);
                        hasError = true;
                    }
                    else
                    {
                        var isApproved = _playersRepo.IsUserApprovedInTeamPlayer(user.UserId, seasonId);
                        var isRegistered = league.CompetitionRegistrations.FirstOrDefault(r => r.UserId == user.UserId) != null;
                        if (!isApproved)
                        {
                            registration.RowErrors.Add(new KeyValuePair<string, string>("", Messages.GymnastIsNotApproved));
                            importErrorRows.Add(registration);
                            hasError = true;
                        }
                        else if (!isRegistered)
                        {
                            registration.RowErrors.Add(new KeyValuePair<string, string>("", LangHelper.ReplacePlayerAcordingToSection(Messages.PlayerIsNotRegisteredRaw, GamesAlias.Gymnastic, false, true)));
                            importErrorRows.Add(registration);
                            hasError = true;
                        }
                        else
                        {
                            var teamPlayer = _playersRepo.GetTeamPlayerByUserIdAndSeasonId(user.UserId, seasonId);
                            if(teamPlayer != null && teamPlayer.IsApprovedByManager == true && teamPlayer.ApprovalDate != null && teamPlayer.ApprovalDate > competitionDate)
                            {
                                registration.RowErrors.Add(new KeyValuePair<string, string>("", Messages.GymnastIsApprovalIsAfterCompetitionDate));
                                importErrorRows.Add(registration);
                                hasError = true;
                            }
                        }
                    }

                    if (route == null)
                    {
                        registration.RowErrors.Add(new KeyValuePair<string, string>("", Messages.NoRoutes_Error));
                        importErrorRows.Add(registration);
                        hasError = true;
                    }

                    if (hasError) continue;

                    #endregion

                    var rank = user.UsersRanks.FirstOrDefault(r => r.UsersRoute.RouteId == route.Id)?.RouteRank
                        ?? route?.RouteRanks?.FirstOrDefault();

                    var routeName = route?.Route;
                    var rankName = rank?.Rank;

                    if (registration.Instruments != null && registration.Instruments.Any())
                    {
                        foreach (var instrument in registration.Instruments)
                        {
                            var competitionRoute = _leagueRepo.GetCompetitionRoute(leagueId, route.Id, rank.Id, registration.Composition, registration.SecondComposition, instrument)
                                ?? _leagueRepo.CreateCompetitionRoute(route, rank, seasonId, leagueId);

                            if (hasError) continue;

                            _leagueRepo.CreateOrUpdateGymnasticRegistration(user.UserId, club.ClubId, leagueId, seasonId, competitionRoute?.Id,
                                registration.CompositionNumber, registration.FinalScore, registration.Position, instrument);

                            importedRowCount++;
                        }
                    }
                    else
                    {
                        var competitionRoute = _leagueRepo.GetCompetitionRoute(leagueId, route.Id, rank.Id, registration.Composition, registration.SecondComposition)
                            ?? _leagueRepo.CreateCompetitionRoute(route, rank, seasonId, leagueId);

                        if (hasError) continue;

                        _leagueRepo.CreateOrUpdateGymnasticRegistration(user.UserId, club.ClubId, leagueId, seasonId, competitionRoute?.Id,
                            registration.CompositionNumber, registration.FinalScore, registration.Position);

                        importedRowCount++;
                    }
                }
                if (importedRowCount > 0) _leagueRepo.Save();
                return importedRowCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ExtractSportsmanData(Stream inputStream, out List<ImportSportsmanRegistrationModel> correctRows, out List<ImportSportsmanRegistrationModel> validationErrorRows)
        {
            correctRows = new List<ImportSportsmanRegistrationModel>();
            validationErrorRows = new List<ImportSportsmanRegistrationModel>();
            var isFirstRow = true;
            using (var workBook = new XLWorkbook(inputStream))
            {
                var workSheet = workBook.Worksheet(1);
                foreach (var row in workSheet.Rows())
                {
                    if (!isFirstRow && !row.IsEmpty())
                    {
                        ImportSportsmanRegistrationModel validatedRow;
                        if (ValidateSportsmanRow(row, out validatedRow))
                        {
                            correctRows.Add(validatedRow);
                        }
                        else
                        {
                            validationErrorRows.Add(validatedRow);
                        }
                    }
                    else
                    {
                        isFirstRow = false;
                    }
                }
            }
        }

        public bool ValidateSportsmanRow(IXLRow row, out ImportSportsmanRegistrationModel model)
        {
            model = new ImportSportsmanRegistrationModel(row);

            #region 1 – * Club number
            var clubNumberStr = row.Cell(1).Value.ToString();
            var clubNumber = 0;
            if (string.IsNullOrWhiteSpace(clubNumberStr))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ClubNumber, Messages.FieldIsRequired));
            }
            else
            {
                if (!int.TryParse(row.Cell(1).Value.ToString(), out clubNumber))
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ClubNumber, Messages.ImportGymnastics_ClubNumberShouldBeNumber));
                }
            }

            #endregion

            #region 2 - Club name

            var clubNameString = row.Cell(2).Value.ToString();

            #endregion

            #region 3 - *Full name

            var fullNameString = row.Cell(3).Value.ToString();

            if (string.IsNullOrWhiteSpace(fullNameString))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Fullname, Messages.ImportGymnastics_FullNameRequired));
            }

            #endregion

            #region 4 - *ID number.
            var idStr = row.Cell(4).Value.ToString();
            var idSb = new System.Text.StringBuilder();
            var id = string.Empty;
            if (string.IsNullOrWhiteSpace(idStr))
            {
                //return false;
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.IdentNum, Messages.ImportGymnastics_IdentNumRequired));
            }
            else
            {
                idStr = idStr.Trim();
                foreach (var ch in idStr)
                {
                    var tmp = 0;
                    if (!int.TryParse(ch.ToString(), out tmp))
                    {
                        //return false;
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_ShouldBeNumber));
                    }
                    idSb.Append(ch);
                }
                var idLength = idSb.Length;
                if (idLength > 9)
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_IDNumberMaxLength));
                }

                while (idSb.Length < 9)
                {
                    idSb.Insert(0, "0");
                }
                id = idSb.ToString();
            }
            #endregion

            #region 5 - *Birth date.
            var birthDate = DateTime.MinValue;
            if (row.Cell(5).DataType == XLDataType.DateTime)
            {
                birthDate = row.Cell(5).GetDateTime();
            }
            else
            {
                var birthDateStr = row.Cell(5).Value.ToString();

                if (string.IsNullOrWhiteSpace(birthDateStr))
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Birthday, Messages.ImportPlayers_Required));
                }
                else
                {
                    if (!DateTime.TryParseExact(birthDateStr, "dd-MM-yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out birthDate))
                    {
                        //return false;
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_Birthday, Messages.ImportPlayers_BirthdayIncorrectFormat));
                    }
                }
            }
            #endregion

            #region 6 - Final score

            var scoreStr = row.Cell(6).Value.ToString();
            var score = double.TryParse(scoreStr, out var scoreResult) ? (double?)scoreResult : null;

            #endregion

            #region 7 - Position

            var positionStr = row.Cell(7).Value.ToString();
            var position = int.TryParse(positionStr, out var positionResult) ? (int?)positionResult : null;

            #endregion

            model.ClubNumber = clubNumber;
            model.ClubName = clubNameString;
            model.FullName = fullNameString;
            model.IdentNum = id;
            model.Birthday = birthDate;
            model.FinalScore = score;
            model.Rank = position;

            return model.RowErrors.Count == 0;
        }

        public int ImportSportsmen(int seasonId, int leagueId, List<ImportSportsmanRegistrationModel> sportsmanRows, out List<ImportSportsmanRegistrationModel> importErrorRows, out List<ImportSportsmanRegistrationModel> duplicateRows)
        {
            var importedRowCount = 0;
            importErrorRows = new List<ImportSportsmanRegistrationModel>();
            duplicateRows = new List<ImportSportsmanRegistrationModel>();
            foreach (var sportsman in sportsmanRows)
            {
                var hasValidationErrors = false;
                var sportsmanInSystem = _playersRepo.GetUserByIdentNum(sportsman.IdentNum);
                if (sportsmanInSystem == null)
                {
                    // the sportsman doesn't exist in the system
                    sportsman.RowErrors.Add(new KeyValuePair<string, string>(string.Empty, Messages.PlayerNotExists.Replace(Messages.Player, Messages.Sportsman)));
                    importErrorRows.Add(sportsman);
                    hasValidationErrors = true;
                }

                var league = _leagueRepo.GetById(leagueId);
                var unionId = league?.UnionId ?? league?.Club?.UnionId;

                var club = _clubsRepo.GetByNumber(sportsman.ClubNumber, seasonId, unionId);

                if (club == null)
                {
                    // the club doesn't exist in the system
                    sportsman.RowErrors.Add(new KeyValuePair<string, string>(string.Empty, $"{Messages.ImportGymnastics_ClubDoesntExist} {sportsman.ClubNumber}"));
                    importErrorRows.Add(sportsman);
                    hasValidationErrors = true;
                }

                // has errors - no import 
                if (hasValidationErrors) continue;

                var registration = _leagueRepo.GetSportsmanRegistration(sportsmanInSystem.UserId, leagueId, club.ClubId, seasonId);

                if (registration != null)
                    _leagueRepo.UpdateSportsmanRegistration(registration, sportsman.Rank, sportsman.FinalScore);
                else
                    _leagueRepo.CreateSportsmanRegistration(sportsmanInSystem.UserId, leagueId, club.ClubId, seasonId, sportsman.Rank, sportsman.FinalScore);

                _leagueRepo.Save();
                importedRowCount++;
            }

            return importedRowCount;
        }

        public Stream BuildErrorFileForSportsmen(List<ImportSportsmanRegistrationModel> errorRows, List<ImportSportsmanRegistrationModel> duplicatedRows, CultEnum culture)
        {
            var result = new MemoryStream();

            using (var workBook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = culture == CultEnum.He_IL })
            {
                #region Error row
                if (errorRows != null && errorRows.Count > 0)
                {
                    var wsError = workBook.AddWorksheet(Messages.ImportPlayers_ErrorWorksheetName);

                    #region Header
                    wsError.Cell(1, 1).Value = $"*{Messages.ClubNumber}";
                    wsError.Cell(1, 2).Value = Messages.ClubName;
                    wsError.Cell(1, 3).Value = $"*{Messages.FullName}";
                    wsError.Cell(1, 4).Value = $"*{Messages.Id}";
                    wsError.Cell(1, 5).Value = $"*{Messages.BirthDay}";
                    wsError.Cell(1, 6).Value = Messages.FinalScore;
                    wsError.Cell(1, 7).Value = Messages.PlayerInfoRank;

                    wsError.Cell(1, 9).Value = Messages.Reason;
                    #endregion

                    wsError.Columns(1, 9).AdjustToContents();
                    wsError.Column(9).Width = 60;

                    for (var i = 0; i < errorRows.Count; i++)
                    {
                        var row = errorRows[i].OriginalRow;

                        for (var j = 0; j < 7; j++)
                        {
                            wsError.Cell(i + 2, j + 1).DataType = row.Cell(j + 1).DataType;
                            wsError.Cell(i + 2, j + 1).SetValue(row.Cell(j + 1).Value);
                        }

                        if (errorRows[i].RowErrors.Count > 0)
                        {
                            foreach (var er in errorRows[i].RowErrors)
                            {
                                var line = wsError.Cell(i + 2, 9).RichText.AddNewLine();
                                line.AddText(string.Format("{0} - {1}", er.Key, er.Value));
                            }
                            wsError.Cell(i + 2, 9).Style.Alignment.WrapText = true;
                        }
                    }
                }
                #endregion

                #region Duplicate row

                if (duplicatedRows != null && duplicatedRows.Count > 0)
                {
                    var wsDuplicate = workBook.AddWorksheet(Messages.ImportPlayers_DuplicateWorksheetName);

                    #region Header
                    wsDuplicate.Cell(1, 1).Value = $"*{Messages.ClubNumber}";
                    wsDuplicate.Cell(1, 2).Value = Messages.ClubName;
                    wsDuplicate.Cell(1, 3).Value = $"*{Messages.FullName}";
                    wsDuplicate.Cell(1, 4).Value = $"*{Messages.Id}";
                    wsDuplicate.Cell(1, 5).Value = $"*{Messages.BirthDay}";
                    wsDuplicate.Cell(1, 6).Value = Messages.FinalScore;
                    wsDuplicate.Cell(1, 7).Value = Messages.PlayerInfoRank;

                    wsDuplicate.Cell(1, 9).Value = Messages.Reason;
                    #endregion

                    wsDuplicate.Columns(1, 9).AdjustToContents();
                    wsDuplicate.Column(9).Width = 60;

                    for (var i = 0; i < duplicatedRows.Count; i++)
                    {
                        var row = duplicatedRows[i].OriginalRow;

                        for (var j = 0; j < 7; j++)
                        {
                            wsDuplicate.Cell(i + 2, j + 1).DataType = row.Cell(j + 1).DataType;
                            wsDuplicate.Cell(i + 2, j + 1).SetValue(row.Cell(j + 1).Value);
                        }

                        if (duplicatedRows[i].RowErrors.Count > 0)
                        {
                            var error = string.Join(@"\n", duplicatedRows[i].RowErrors.Select(p => string.Format("{0}", p.Value)));

                            wsDuplicate.Cell(i + 2, 9).DataType = XLDataType.Text;
                            wsDuplicate.Cell(i + 2, 9).SetValue(error);
                        }
                    }
                }
                #endregion

                workBook.SaveAs(result);
                result.Position = 0;
            }

            return result;
        }

        public void ExtractAthletesNumbersData(Stream inputStream, out List<ImportAthletesNumbersModel> correctRows, out List<ImportAthletesNumbersModel> validationErrorRows)
        {
            correctRows = new List<ImportAthletesNumbersModel>();
            validationErrorRows = new List<ImportAthletesNumbersModel>();
            var isFirstRow = true;
            using (var workBook = new XLWorkbook(inputStream))
            {
                var workSheet = workBook.Worksheet(1);
                foreach (var row in workSheet.Rows())
                {
                    if (!isFirstRow && !row.IsEmpty())
                    {
                        ImportAthletesNumbersModel validatedRow;
                        if (ValidateAthletesNumberRow(row, out validatedRow))
                        {
                            correctRows.Add(validatedRow);
                        }
                        else
                        {
                            validationErrorRows.Add(validatedRow);
                        }
                    }
                    else
                    {
                        isFirstRow = false;
                    }
                }
            }
        }

        public void ExtractCompetitionResultsData(Stream inputStream, out List<ImportCompetitionResultsModel> correctRows, out List<ImportCompetitionResultsModel> validationErrorRows)
        {
            correctRows = new List<ImportCompetitionResultsModel>();
            validationErrorRows = new List<ImportCompetitionResultsModel>();
            var isFirstRow = true;
            using (var workBook = new XLWorkbook(inputStream))
            {
                var workSheet = workBook.Worksheet(1);
                foreach (var row in workSheet.Rows())
                {
                    if (!isFirstRow && !row.IsEmpty())
                    {
                        ImportCompetitionResultsModel validatedRow;
                        if (ValidateCompetitionResultsRow(row, out validatedRow))
                        {
                            correctRows.Add(validatedRow);
                        }
                        else
                        {
                            validationErrorRows.Add(validatedRow);
                        }
                    }
                    else
                    {
                        isFirstRow = false;
                    }
                }
            }
        }



        private bool ValidateAthletesNumberRow(IXLRow row, out ImportAthletesNumbersModel model)
        {
            model = new ImportAthletesNumbersModel(row);

            #region 1 - Full name

            var fullNameString = row.Cell(1).Value.ToString();
            model.FullName = fullNameString;

            #endregion


            #region 2 - Club name

            var clubNameString = row.Cell(2).Value.ToString();
            model.ClubName = clubNameString;

            #endregion



            #region 3 - *ID number.
            var idStr = row.Cell(3).Value.ToString();
            var idSb = new System.Text.StringBuilder();
            var id = string.Empty;
            if (string.IsNullOrWhiteSpace(idStr))
            {
                //return false;
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.IdentNum, Messages.ImportGymnastics_IdentNumRequired));
            }
            else
            {
                /*
                idStr = idStr.Trim();
                foreach (var ch in idStr)
                {
                    int tmp = 0;
                    if (!int.TryParse(ch.ToString(), out tmp))
                    {
                        //return false;
                        model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_ShouldBeNumber));
                    }
                    idSb.Append(ch);
                }
                int idLength = idSb.Length;
                if (idLength > 9)
                {
                    //return false;
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_IDNumberMaxLength));
                }

                while (idSb.Length < 9)
                {
                    idSb.Insert(0, "0");
                }
                id = idSb.ToString();
                */

            }

            model.IdentNum = idStr;

            #endregion

            #region 4 - Birthday

            //var borthdayString = row.Cell(4).Value.ToString();
            //model.BirthDay = DateTime.Parse(borthdayString);

            #endregion


            #region 5 - *AthleteNumber
            var athleteNumberStr = row.Cell(5).Value.ToString();
            if (string.IsNullOrEmpty(athleteNumberStr))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.AthleteNumber, Messages.FieldIsRequired));
            }
            else
            {
                if (!(int.TryParse(athleteNumberStr, out var athleteNumber)))
                {
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.AthleteNumber, Messages.ImportPlayers_ShouldBeNumber));
                }
                else
                {
                    model.AthleteNumber = athleteNumber;
                }
            }

            #endregion


            return model.RowErrors.Count == 0;
        }




        private bool ValidateCompetitionResultsRow(IXLRow row, out ImportCompetitionResultsModel model)
        {
            model = new ImportCompetitionResultsModel(row);

            #region 1 - *AthleteNumber
            var athleteNumberStr = row.Cell(1).Value.ToString();
            if (!string.IsNullOrEmpty(athleteNumberStr))
            {
                if (!(int.TryParse(athleteNumberStr, out var athleteNumber)))
                {
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.AthleteNumber, Messages.ImportPlayers_ShouldBeNumber));
                }
                else
                {
                    model.AthleteNumber = athleteNumber;
                }
            }

            #endregion

            #region 2 - *ID number.
            var idStr = row.Cell(2).Value.ToString();
            var idSb = new System.Text.StringBuilder();
            var id = string.Empty;
            if (string.IsNullOrWhiteSpace(idStr))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.IdentNum, Messages.ImportGymnastics_IdentNumRequired));
            }
            model.IdentNum = idStr;

            #endregion

            #region 3 - First name

            var firstNameString = row.Cell(3).Value.ToString();
            if (string.IsNullOrEmpty(firstNameString))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.FirstName, Messages.FieldIsRequired));
            }
            else
            {
                model.FirstName = firstNameString;
            }

            #endregion

            #region 4 - Last name

            var lastNameString = row.Cell(4).Value.ToString();
            if (string.IsNullOrEmpty(lastNameString))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.LastName, Messages.FieldIsRequired));
            }
            else
            {
                model.LastName = lastNameString;
            }

            #endregion

            #region 5 - Club name
            var clubNameString = row.Cell(5).Value.ToString();
            if (!string.IsNullOrEmpty(clubNameString))
            {
                model.ClubName = clubNameString;
            }
            #endregion

            #region 6 - Birthday
            var birthdayString = row.Cell(6).Value.ToString();
            if (!(DateTime.TryParse(birthdayString, out var birthDay)))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.BirthDay, Messages.MustBeDateTime));
            }
            else
            {
                model.BirthDay = birthDay;
            }
            #endregion

            #region 7 - Heat
            var heatString = row.Cell(7).Value.ToString();
            model.Heat = heatString;
            #endregion

            #region 8 - Lane
            var laneString = row.Cell(8).Value.ToString();
            if (!string.IsNullOrEmpty(laneString))
            {
                if (!(int.TryParse(laneString, out var laneNumber)))
                {
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Lane, Messages.ImportPlayers_ShouldBeNumber));
                }
                else
                {
                    model.Lane = laneNumber;
                }
            }


            #endregion

            #region 9 - Result
            var resultString = row.Cell(9).SetDataType(XLDataType.Text).GetString();
            if (string.IsNullOrEmpty(resultString))
            {
                model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Result, Messages.FieldIsRequired));
            }
            else
            {
                model.Result = resultString;
            }
            #endregion

            #region 10 - Wind
            var windString = row.Cell(10).Value.ToString();
            if (!string.IsNullOrEmpty(windString))
            {
                if (!(double.TryParse(windString, out var windNumber)))
                {
                    model.RowErrors.Add(new KeyValuePair<string, string>(Messages.Lane, Messages.ImportPlayers_ShouldBeNumber));
                }
                else
                {
                    model.Wind = windNumber;
                }
            }

            #endregion

            return model.RowErrors.Count == 0;
        }




        public int ImportAthletesNumbers(List<ImportAthletesNumbersModel> correctRows, out List<ImportAthletesNumbersModel> importErrorRows , IEnumerable<PlayerStatusViewModel> players, int seasonId)
        {
            var athleteNumsAdded = new List<int>();
            var importedRowCount = 0;
            importErrorRows = new List<ImportAthletesNumbersModel>();
            foreach (var playerNumber in correctRows)
            {
                var user = _playersRepo.GetTeamPlayerByIdentNum(playerNumber.IdentNum);
                if (user == null) {
                    user = _playersRepo.GetTeamPlayersByPassport(playerNumber.IdentNum).FirstOrDefault();
                }

                var hasError = false;
                if (user == null)
                {
                    playerNumber.RowErrors.Add(new KeyValuePair<string, string>("", Messages.AthleteNotExist));
                    importErrorRows.Add(playerNumber);
                    hasError = true;
                }
                else {
                    var playerFoundWithSameAthleticNumber = players.FirstOrDefault(o => o.AthletesNumbers == playerNumber.AthleteNumber);
                    if (playerNumber.AthleteNumber.HasValue && playerNumber.AthleteNumber != user.User.AthleteNumbers.FirstOrDefault(x=>x.SeasonId == seasonId)?.AthleteNumber1 && (athleteNumsAdded.Contains((int)playerNumber.AthleteNumber) || (playerFoundWithSameAthleticNumber != null && playerFoundWithSameAthleticNumber.UserId > 0 && playerFoundWithSameAthleticNumber.UserId != user.User.UserId))) {
                        playerNumber.RowErrors.Add(new KeyValuePair<string, string>("", Messages.AthleteNumberAlreadyExists));
                        importErrorRows.Add(playerNumber);
                        hasError = true;
                    }
                }


                if (hasError) continue;
                athleteNumsAdded.Add((int)playerNumber.AthleteNumber);
                var athleteNumber = user.User.AthleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId);
                if (athleteNumber != null)
                {
                    athleteNumber.AthleteNumber1 = playerNumber.AthleteNumber;
                }
                else
                {
                    athleteNumber = new AthleteNumber
                    {
                        AthleteNumber1 = playerNumber.AthleteNumber,
                        SeasonId = seasonId,
                        UserId = user.UserId
                    };
                    _playersRepo.AthleteNumber(athleteNumber);
                }

                var playerFind = players.FirstOrDefault(u => u.UserId == user.UserId);
                if (playerFind != null) { playerFind.AthletesNumbers = playerNumber.AthleteNumber; }
                importedRowCount++;
            }
            _playersRepo.Save();
            return importedRowCount;
        }



        public int ImportCompetitionResults(List<ImportCompetitionResultsModel> correctRows, out List<ImportCompetitionResultsModel> importErrorRows, IEnumerable<ComparableCompDiscRegDTO> players, IEnumerable<CompetitionDisciplineRegistration> registrations, int competitionDisciplineId, int format)
        {
            var importedRowCount = 0;
            importErrorRows = new List<ImportCompetitionResultsModel>();
            foreach (var correctRow in correctRows)
            {
                var hasError = false;
                double? sortedValue = null;
                string resultStr = null;
                var foundSuitablePlayer = players.Where(r => r.IdentNum == correctRow.IdentNum || r.PassportNum == correctRow.IdentNum).FirstOrDefault();
                if (foundSuitablePlayer == null)
                {
                    string repairedIdentNum = null;
                    if (correctRow.IdentNum.Length < 9)
                    {
                        repairedIdentNum = GetRepairedIdentNum(correctRow.IdentNum);
                    }
                    if (repairedIdentNum != null)
                    {
                        foundSuitablePlayer = players.Where(r => r.IdentNum == repairedIdentNum).FirstOrDefault();
                        if (foundSuitablePlayer != null)
                        {
                            correctRow.IdentNum = repairedIdentNum;
                        }
                    }
                }
                if (foundSuitablePlayer == null) {

                    var playerById = _playersRepo.GetTeamPlayerByIdentNum(correctRow.IdentNum);
                    var playerByPassport = _playersRepo.GetTeamPlayerByPassportNum(correctRow.IdentNum);
                    if (playerById == null && playerByPassport == null)
                    {
                        correctRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.IdentNum, Messages.AthleteNotExist));
                    }
                    else
                    {
                        correctRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.IdentNum, Messages.NotMeetRequirements));
                    }
                    hasError = true;
                    importErrorRows.Add(correctRow);
                }
                else if (!UIHelpers.isResultFormatCorrect(correctRow.Result, format, out sortedValue, out resultStr))
                {
                    hasError = true;
                    correctRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.Result, Messages.ResultFormatIsIncorrect));
                    importErrorRows.Add(correctRow);
                }
                if (hasError) continue;
                var foundRegistration = registrations.Where(r => r.User.IdentNum == correctRow.IdentNum || r.User.PassportNum == correctRow.IdentNum).FirstOrDefault();
                if (foundRegistration != null)
                {
                    var result = foundRegistration.CompetitionResult.FirstOrDefault();
                    if (result == null)
                    {
                        var compRes = new CompetitionResult
                        {
                            CompetitionRegistrationId = foundRegistration.Id,
                            Heat = correctRow.Heat,
                            Lane = correctRow.Lane,
                            Result = resultStr,
                            Wind = correctRow.Wind,
                            SortValue = Convert.ToInt64(sortedValue)
                        };
                        foundRegistration.CompetitionResult.Add(compRes);
                    }
                    else
                    {
                        result.Heat = correctRow.Heat;
                        result.Lane = correctRow.Lane;
                        result.Result = resultStr;
                        result.Wind = correctRow.Wind;
                        result.SortValue = Convert.ToInt64(sortedValue);
                    }
                }
                else {
                    var player = _playersRepo.GetTeamPlayerByUserId(foundSuitablePlayer.UserId);
                    var isAdded = _disciplinesRepo.RegisterAthleteUnderCompetitionDiscipline(competitionDisciplineId, foundSuitablePlayer.ClubId, foundSuitablePlayer.UserId);
                    if (isAdded > 0)
                    {
                        // player registered.
                        var registeredPlayer = _disciplinesRepo.getCompetitionDisciplineRegistrationById(isAdded);
                        var compRes = new CompetitionResult
                        {
                            CompetitionRegistrationId = isAdded,
                            Heat = correctRow.Heat,
                            Lane = correctRow.Lane,
                            Result = resultStr,
                            Wind = correctRow.Wind,
                            SortValue = Convert.ToInt64(sortedValue)
                        };
                        registeredPlayer.CompetitionResult.Add(compRes);
                    }
                    else {
                        correctRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.IdentNum, Messages.NotApproved));
                        importErrorRows.Add(correctRow);
                        continue;
                    }
                }
                importedRowCount++;
            }
            _disciplinesRepo.Save();
            return importedRowCount;
        }

        private string GetRepairedIdentNum(string identNum)
        {
            string tempIdent = identNum;
            while (tempIdent.Length < 9) {
                tempIdent = "0" + tempIdent;
            }
            return tempIdent;
        }

        public Stream BuildErrorFileForAthletesNumber(List<ImportAthletesNumbersModel> errorRows, CultEnum culture)
        {
            var result = new MemoryStream();

            using (var workBook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = culture == CultEnum.He_IL })
            {
                #region Error row
                if (errorRows != null && errorRows.Count > 0)
                {
                    var wsError = workBook.AddWorksheet(Messages.ImportPlayers_ErrorWorksheetName);

                    #region Header
                    wsError.Cell(1, 1).Value = Messages.FullName;
                    wsError.Cell(1, 2).Value = $" {Messages.ClubName}";
                    wsError.Cell(1, 3).Value = $"* {Messages.IdentNum}";
                    wsError.Cell(1, 4).Value = $" {Messages.BirthDay}";
                    wsError.Cell(1, 5).Value = $"* {Messages.AthleteNumber}";

                    wsError.Cell(1, 6).Value = Messages.Reason;
                    #endregion

                    wsError.Column(3).Style.NumberFormat.SetFormat("@");

                    wsError.Column(6).Width = 60;

                    for (var i = 0; i < errorRows.Count; i++)
                    {
                        var row = errorRows[i].OriginalRow;

                        for (var j = 0; j < 5; j++)
                        {
                            wsError.Cell(i + 2, j + 1).DataType = row.Cell(j + 1).DataType;
                            wsError.Cell(i + 2, j + 1).SetValue(row.Cell(j + 1).Value);
                        }

                        if (errorRows[i].RowErrors.Count > 0)
                        {
                            foreach (var er in errorRows[i].RowErrors)
                            {
                                var line = wsError.Cell(i + 2, 6).RichText.AddNewLine();
                                line.AddText(string.Format("{0} - {1}", er.Key, er.Value));
                            }
                            wsError.Cell(i + 2, 6).Style.Alignment.WrapText = true;
                        }
                    }
                    wsError.Columns(1, 6).AdjustToContents();
                }
                #endregion

                workBook.SaveAs(result);
                result.Position = 0;
            }

            return result;
        }



        public Stream BuildErrorFileForImportingCompetitionResults(List<ImportCompetitionResultsModel> errorRows, CultEnum culture)
        {
            var result = new MemoryStream();

            using (var workBook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = culture == CultEnum.He_IL })
            {

                if (errorRows != null && errorRows.Count > 0)
                {
                    var wsError = workBook.AddWorksheet(Messages.ImportPlayers_ErrorWorksheetName);

                    var columnCounter = 1;
                    var rowCounter = 1;

                    var addCell = new Action<string>(value =>
                    {
                        wsError.Cell(rowCounter, columnCounter).Value = value;
                        columnCounter++;
                    });


                    var addRow = new Action<ImportCompetitionResultsModel>(value =>
                    {
                        addCell($"{value.AthleteNumber}");
                        addCell($"{value.IdentNum}");
                        addCell($"{value.FirstName}");
                        addCell($"{value.LastName}");
                        addCell($"{value.ClubName}");
                        addCell($"{value.BirthDay}");
                        addCell($"{value.Heat}");
                        addCell($"{value.Lane}");
                        addCell($"{value.Result}");
                        addCell($"{value.Wind}");
                        var errCell = value.RowErrors.FirstOrDefault();
                        addCell($"{string.Format("{0} - {1}", errCell.Key, errCell.Value)}");
                        columnCounter = 1;
                        rowCounter++;
                    });

                    #region Excel header

                    addCell($"{Messages.AthleteNumber}");
                    addCell($"* {Messages.IdentNum}/{Messages.PassportNum}");
                    addCell($"* {Messages.FirstName}");
                    addCell($"* {Messages.LastName}");
                    addCell($"* {Messages.ClubName}");
                    addCell($"* {Messages.BirthDay}");
                    addCell($"{Messages.Heat}");
                    addCell($"{Messages.Lane}");
                    addCell($"* {Messages.Result}");
                    addCell($"{Messages.Wind}");
                    addCell($"{Messages.Reason}");
                    columnCounter = 1;
                    rowCounter++;

                    #endregion

                    wsError.Column(9).Style.NumberFormat.SetFormat("@");
                    wsError.Columns().AdjustToContents();

                    foreach (var errorRow in errorRows)
                    {
                        addRow(errorRow);
                    }

                    wsError.Column(9).Style.NumberFormat.SetFormat("@");
                    wsError.Columns().AdjustToContents();
                    wsError.Columns().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    workBook.SaveAs(result);
                    result.Position = 0;
                }

                return result;
            }
        }
    }

    public class ImportPlayerAllowedTeamModel
    {
        public int TeamId { get; set; }
        public int? ClubId { get; set; }
        public int? LeagueId { get; set; }
    }

    public class ImportPlayerModel
    {
        public ImportPlayerModel(IXLRow row)
        {
            OriginalRow = row;
            RowErrors = new List<KeyValuePair<string, string>>();
        }

        //1 –*First name
        public string FirstName { get; set; }

        //2 –*Last name
        public string LastName { get; set; }

        //3 –*Middle name
        public string MiddleName { get; set; }

        //4- *TeamID.
        public int TeamId { get; set; }
        //5- *ID number.
        public string Id { get; set; }
        //6- Email.
        public string Email { get; set; }
        //7- Phone No'.
        public string PhoneNo { get; set; }
        //8- *Birth date.
        public DateTime Birthday { get; set; }
        //9- *Gender.
        public string Gender { get; set; }
        //10- Height.
        public string Height { get; set; }
        //11- City.
        public string City { get; set; }
        //12- Player card NO'.
        public string PlayerCardNo { get; set; }
        //13- Jersey NO' (depend on which team)
        public int? JerseyNo { get; set; }

        public IXLRow OriginalRow { get; set; }
        public int GenderValue { get; set; }
        public int? HeightValue { get; set; }
        public List<KeyValuePair<string, string>> RowErrors { get; set; }
        public DateTime? TenicardValidity { get; set; }
        public DateTime? DateOfInsuranceValidity { get; set; }
        public DateTime? DateOfMedicalExam { get; set; }
        public string Comment { get; internal set; }
        public string PostalCode { get; set; }
        public string PassportNum { get; set; }
        public string Address { get; set; }

        public int? FriendshipTypeId { get; set; }
        public int? FriendshipPriceTypeId { get; set; }
        public int? RoadHeatId { get; set; }
        public int? MountainHeatId { get; set; }
        public int? MountainIronNumber { get; set; }
        public int? RoadIronNumber { get; set; }
        public int? VelodromeIronNumber { get; set; }
        public long? UciId { get; set; }
        public string ChipNumber { get; set; }
        public int? KitStatus { get; set; }
        public string ForeignFirstName { get; set; }
        public string ForeignLastName { get; set; }
        //helper property
        public int? Hierarchy { get; set; }

    }

    public class ImportGymnasticRegistrationModel
    {
        public ImportGymnasticRegistrationModel(IXLRow row)
        {
            OriginalRow = row;
            RowErrors = new List<KeyValuePair<string, string>>();
            Instruments = new List<GymnasticInstrumentImport>();
        }

        //1 –*Club number
        public int ClubNumber { get; set; }
        //2- Club name.
        public string ClubName { get; set; }
        //3- *First name
        public string FirstName { get; set; }
        //4- *Last name
        public string LastName { get; set; }
        //5- *Full name
        public string FullName { get; set; }
        //6- Ident number.
        public string IdentNum { get; set; }
        //7- *Birth date.
        public DateTime? Birthday { get; set; }
        //8 - Composition.
        public int? Composition { get; set; }
        //9 - Composition2.
        public int? SecondComposition { get; set; }
        //10 - 19
        public List<GymnasticInstrumentImport> Instruments { get; set; }
        //18 - Final score
        public double? FinalScore { get; set; }
        //21 - Position
        public int? Position { get; set; }

        public IXLRow OriginalRow { get; set; }
        public List<KeyValuePair<string, string>> RowErrors { get; set; }
        public DateTime? TenicardValidity { get; set; }
        public int CompositionNumber { get; set; }
    }

    public class ImportSportsmanRegistrationModel
    {
        public ImportSportsmanRegistrationModel(IXLRow row)
        {
            OriginalRow = row;
            RowErrors = new List<KeyValuePair<string, string>>();
        }

        //1 –*Club number
        public int ClubNumber { get; set; }
        //2- Club name.
        public string ClubName { get; set; }
        //3- *Full name
        public string FullName { get; set; }
        //4- Ident number.
        public string IdentNum { get; set; }
        //5- *Birth date.
        public DateTime Birthday { get; set; }
        //6 - Final score
        public double? FinalScore { get; set; }
        //7 - Rank/Position
        public int? Rank { get; set; }

        public IXLRow OriginalRow { get; set; }
        public List<KeyValuePair<string, string>> RowErrors { get; set; }
    }

    public class ImportTennisCompetitionPlayerModel
    {
        public ImportTennisCompetitionPlayerModel(IXLRow row)
        {
            OriginalRow = row;
            RowErrors = new List<KeyValuePair<string, string>>();
        }
        public string FullName { get; set; }
        public DateTime Birthday { get; set; }
        public int? GenderId { get; set; }
        public string IdentNumber { get; set; }
        public DateTime? MedicalExam { get; set; }
        public InsuranceType TypeOfInsurance { get; set; }
        public DateTime? InsuranceStartDate { get; set; }
        public int ClubNumber { get; set; }

        public IXLRow OriginalRow { get; set; }
        public List<KeyValuePair<string, string>> RowErrors { get; set; }
    }

    public class ImportAthletesNumbersModel
    {
        public ImportAthletesNumbersModel(IXLRow row)
        {
            OriginalRow = row;
            RowErrors = new List<KeyValuePair<string, string>>();
        }

        public string FullName { get; set; }
        public string ClubName { get; set; }
        public string IdentNum { get; set; }
        public DateTime? BirthDay { get; set; }
        public int? AthleteNumber { get; set; }

        public IXLRow OriginalRow { get; set; }
        public List<KeyValuePair<string, string>> RowErrors { get; set; }
    }

    public class ImportCompetitionResultsModel
    {
        public ImportCompetitionResultsModel(IXLRow row)
        {
            OriginalRow = row;
            RowErrors = new List<KeyValuePair<string, string>>();
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ClubName { get; set; }
        public string IdentNum { get; set; }
        public DateTime? BirthDay { get; set; }
        public int? AthleteNumber { get; set; }
        public string Result { get; set; }
        public string Heat { get; set; }
        public int? Lane { get; set; }
        public double? Wind { get; set; }

        public IXLRow OriginalRow { get; set; }
        public List<KeyValuePair<string, string>> RowErrors { get; set; }
    }

    public enum InsuranceType
    {
        None,
        Private,
        School,
        Club
    }
}

