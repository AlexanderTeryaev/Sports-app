using AppModel;
using ClosedXML.Excel;
using CmsApp.Services;
using DataService;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CmsApp.Helpers
{
    public class ImportExportVehiclesHelper
    {
        public DataEntities db;
        private VehicleService vehicleService;
        private VehicleRepo vehicleRepo;

        public ImportExportVehiclesHelper(DataEntities db)
        {
            this.db = db;
            vehicleService = new VehicleService(db);
            vehicleRepo = new VehicleRepo(db);
        }

        public ImportExportVehiclesHelper()
        {

        }

        public void ExtractVehiclesData(Stream inputStream, out List<ImportVehicleModel> correctRows, out List<ImportVehicleModel> validationErrorRows,
            int unionId, int seasonId)
        {
            correctRows = new List<ImportVehicleModel>();
            validationErrorRows = new List<ImportVehicleModel>();
            var isFirstRow = true;
            using (XLWorkbook workBook = new XLWorkbook(inputStream))
            {
                IXLWorksheet workSheet = workBook.Worksheet(1);
                foreach (IXLRow row in workSheet.Rows())
                {
                    if (!isFirstRow && !row.IsEmpty())
                    {
                        ImportVehicleModel validatedRow;
                        if (ValidateVehicleRow(row, out validatedRow))
                        {
                            validatedRow.UnionId = unionId;
                            validatedRow.SeasonId = seasonId;
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

        private bool ValidateVehicleRow(IXLRow row, out ImportVehicleModel validatedRow)
        {
            validatedRow = new ImportVehicleModel(row);

            #region Driver details

            #region 1 - VehicleId
            if (!string.IsNullOrEmpty(row.Cell(1).Value.ToString()))
            {
                if (!int.TryParse(row.Cell(1).Value.ToString(), out int vehicleId))
                {
                    validatedRow.RowErrors.Add(new KeyValuePair<string, string>("#", Messages.MustBeNumber.ToLower()));
                }

                if (vehicleId != 0)
                {
                    validatedRow.Id = vehicleId;
                }
            }


            #endregion

            #region 2 - Driver IdentNum

            var idStr = row.Cell(2).Value.ToString();
            System.Text.StringBuilder idSb = new System.Text.StringBuilder();
            if (!string.IsNullOrWhiteSpace(idStr))
            {
                idStr = idStr.Trim();
                foreach (var ch in idStr)
                {
                    if (!int.TryParse(ch.ToString(), out int tmp))
                    {
                        //return false;
                        validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_ShouldBeNumber));
                    }
                    idSb.Append(ch);
                }
                int idLength = idSb.Length;
                if (idLength > 9)
                {
                    //return false;
                    validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.ImportPlayers_Columns_IDNumber, Messages.ImportPlayers_IDNumberMaxLength));
                }

                while (idSb.Length < 9)
                {
                    idSb.Insert(0, "0");
                }
                validatedRow.IdentNum = idSb.ToString();

            }

            #endregion

            #region 3 - FullName

            validatedRow.FullName = row.Cell(3).Value.ToString();

            #endregion

            #region 4 - Address

            validatedRow.Address = row.Cell(4).Value.ToString();


            #endregion

            #region 5 - Ownership date

            if (!string.IsNullOrEmpty(validatedRow.IdentNum))
            {
                DateTime ownershipDate = DateTime.MinValue;
                if (row.Cell(5).DataType == XLDataType.DateTime)
                {
                    ownershipDate = row.Cell(5).GetDateTime();
                }
                else
                {
                    var ownershipDateStr = row.Cell(5).Value.ToString();

                    if (string.IsNullOrWhiteSpace(ownershipDateStr))
                    {
                        //return false;
                        validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.OwnershipDate, Messages.RequiredForDriver.ToLower()));
                    }
                    else
                    {
                        if (!DateTime.TryParseExact(ownershipDateStr, "dd-MM-yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out ownershipDate))
                        {
                            //return false;
                            validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.OwnershipDate, Messages.ImportPlayers_BirthdayIncorrectFormat));
                        }
                    }
                }
                if (ownershipDate != DateTime.MinValue)
                {
                    validatedRow.OwnershipDate = ownershipDate;
                }
            }

            #endregion

            #region 6 - IssueDate

            if (!string.IsNullOrEmpty(validatedRow.IdentNum))
            {
                DateTime issuDate = DateTime.MinValue;
                if (row.Cell(6).DataType == XLDataType.DateTime)
                {
                    issuDate = row.Cell(6).GetDateTime();
                }
                else
                {
                    var issuDateStr = row.Cell(6).Value.ToString();

                    if (string.IsNullOrWhiteSpace(issuDateStr))
                    {
                        validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.IssueDate, Messages.RequiredForDriver.ToLower()));
                    }
                    else
                    {
                        if (!DateTime.TryParseExact(issuDateStr, "dd-MM-yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out issuDate))
                        {
                            validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.IssueDate, Messages.ImportPlayers_BirthdayIncorrectFormat));
                        }
                    }
                }

                if (issuDate != DateTime.MinValue)
                {
                    validatedRow.IssueDate = issuDate;
                }
            }

            #endregion

            #region 7 - Number of previous owners

            if (!string.IsNullOrEmpty(validatedRow.IdentNum))
            {
                if (string.IsNullOrEmpty(row.Cell(7).Value.ToString()))
                {
                    validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.NumberOfOwners, Messages.RequiredForDriver.ToLower()));

                }
                if (!int.TryParse(row.Cell(7).Value.ToString(), out int numberOfPrevious))
                {
                    validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.NumberOfOwners, Messages.MustBeNumber.ToLower()));
                }

                if (numberOfPrevious != 0)
                {
                    validatedRow.NumberOfPreviousOwners = numberOfPrevious;
                }
            }

            #endregion

            #endregion

            #region Vehicle license

            #region 8 - License number*

            var licenseNumberStr = row.Cell(8).Value.ToString();

            if (string.IsNullOrEmpty(licenseNumberStr))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.LicenseNumber, Messages.FieldIsRequired));
            }
            else
            {
                validatedRow.LicenseNumber = licenseNumberStr;
            }

            #endregion

            #region 9 - Valid*

            DateTime validDate = DateTime.MinValue;
            if (row.Cell(9).DataType == XLDataType.DateTime)
            {
                validDate = row.Cell(9).GetDateTime();
            }
            else
            {
                var validDateStr = row.Cell(9).Value.ToString();

                if (string.IsNullOrWhiteSpace(validDateStr))
                {
                    validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.Valid, Messages.FieldIsRequired));
                }
                else
                {
                    if (!DateTime.TryParseExact(validDateStr, "dd-MM-yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out validDate))
                    {
                        validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.Valid, Messages.ImportPlayers_BirthdayIncorrectFormat));
                    }
                }
            }

            if (validDate != DateTime.MinValue)
            {
                validatedRow.Valid = validDate;
            }
            #endregion


            #endregion

            #region Vehicle details

            #region 10 - Type*

            var typeStr = row.Cell(10).Value.ToString();

            if (string.IsNullOrEmpty(typeStr))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.Type, Messages.FieldIsRequired));
            }
            else
            {
                validatedRow.Type = typeStr;
            }

            #endregion

            #region 11 - Product*

            var productStr = row.Cell(11).Value.ToString();

            if (string.IsNullOrEmpty(productStr))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.Product, Messages.FieldIsRequired));
            }
            else
            {
                validatedRow.Product = productStr;
            }

            #endregion

            #region 12 - Model*

            var modelStr = row.Cell(12).Value.ToString();

            if (string.IsNullOrEmpty(modelStr))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.Model, Messages.FieldIsRequired));
            }
            else
            {
                validatedRow.Model = modelStr;
            }

            #endregion

            #region 13 - Year of production*

            DateTime productionDate = DateTime.MinValue;
            if (row.Cell(13).DataType == XLDataType.DateTime)
            {
                productionDate = row.Cell(13).GetDateTime();
            }
            else
            {
                var validDateStr = row.Cell(13).Value.ToString();

                if (string.IsNullOrWhiteSpace(validDateStr))
                {
                    validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.YearOfProduction, Messages.FieldIsRequired));
                }
                else
                {
                    if (!DateTime.TryParseExact(validDateStr, "dd-MM-yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out productionDate))
                    {
                        validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.YearOfProduction, Messages.ImportPlayers_BirthdayIncorrectFormat));
                    }
                }
            }

            if (productionDate != DateTime.MinValue)
            {
                validatedRow.YearOfProduction = productionDate;
            }
            #endregion

            #region 14 - Weight*

            var weightStr = row.Cell(14).Value.ToString();

            if (string.IsNullOrEmpty(weightStr))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.Weight, Messages.FieldIsRequired));
            }
            else
            {
                if (!double.TryParse(row.Cell(14).Value.ToString(), out double weight))
                {
                    //return false;
                    validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.Weight, Messages.MustBeNumber));
                }
                if (weight != 0.0D)
                {
                    validatedRow.Weight = weight;
                }
            }


            #endregion

            #region 15 - Chassis number*

            var chassisStr = row.Cell(15).Value.ToString();

            if (string.IsNullOrEmpty(chassisStr))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.ChassisNo, Messages.FieldIsRequired));
            }
            else
            {
                validatedRow.ChassisNo = chassisStr;
            }

            #endregion

            #region 16 - Type of driver's license*

            var driverLicenseStr = row.Cell(16).Value.ToString();

            if (string.IsNullOrEmpty(driverLicenseStr))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.TypeOfDriversLicense, Messages.FieldIsRequired));
            }
            else
            {
                validatedRow.DriversLicenseType = driverLicenseStr;
            }

            #endregion

            #endregion

            #region Engine details

            #region 17 - Engine number*

            var enigneNumberStr = row.Cell(17).Value.ToString();

            if (string.IsNullOrEmpty(enigneNumberStr))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.EngineNo, Messages.FieldIsRequired));
            }
            else
            {
                validatedRow.EngineNo = enigneNumberStr;
            }

            #endregion

            #region 18 - Engine product*

            var engineProduct = row.Cell(18).Value.ToString();
            if (string.IsNullOrEmpty(engineProduct))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.EngineProduct, Messages.FieldIsRequired));
            }
            else
            {
                validatedRow.EngineProduct = engineProduct;
            }


            #endregion

            #region 19 - Engine volume*

            var engineVolume = row.Cell(19).Value.ToString();
            if (string.IsNullOrEmpty(engineVolume))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.EngineVolume, Messages.FieldIsRequired));
            }
            else
            {
                validatedRow.EngineVolume = engineVolume;
            }


            #endregion


            #region 20 - Max horse power *

            var maxHorsePowerString = row.Cell(20).Value.ToString();

            if (string.IsNullOrEmpty(maxHorsePowerString))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.MaxHorsePower, Messages.FieldIsRequired));
            }
            else
            {
                if (!int.TryParse(row.Cell(20).Value.ToString(), out int maxHorsePower))
                {
                    //return false;
                    validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.MaxHorsePower, Messages.MustBeNumber));
                }
                if (maxHorsePower != 0)
                {
                    validatedRow.MaxHorsePower = maxHorsePower;
                }
            }

            #endregion

            #region 21 - Terms and conditions *

            var termsAndConditions = row.Cell(21).Value.ToString();
            if (string.IsNullOrEmpty(engineProduct))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.TermsAndConditions, Messages.FieldIsRequired));
            }
            else
            {
                validatedRow.TermsAndConditions = termsAndConditions;
            }

            #endregion

            #region 22 - Number of import *

            var numberOfImportStr = row.Cell(22).Value.ToString();

            if (string.IsNullOrEmpty(numberOfImportStr))
            {
                validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.NumberOfImport, Messages.FieldIsRequired));
            }
            else
            {
                if (!int.TryParse(row.Cell(22).Value.ToString(), out int numberOfImport))
                {
                    //return false;
                    validatedRow.RowErrors.Add(new KeyValuePair<string, string>(Messages.NumberOfImport, Messages.MustBeNumber));
                }
                if (numberOfImport != 0)
                {
                    validatedRow.NumberOfImportEntry = numberOfImport;
                }
            }

            #endregion

            #endregion




            return validatedRow.RowErrors.Count == 0;
        }

        public Stream BuildErrorFile(List<ImportVehicleModel> errorRows, List<ImportVehicleModel> duplicateRows, CultEnum culture)
        {
            MemoryStream result = new MemoryStream();

            using (XLWorkbook workBook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = culture == CultEnum.He_IL })
            {
                #region Error row
                if (errorRows != null && errorRows.Count > 0)
                {
                    var wsError = workBook.AddWorksheet(Messages.ImportPlayers_ErrorWorksheetName);

                    #region Header

                    wsError.Cell(1, 1).Value = "#";
                    wsError.Cell(1, 2).Value = Messages.IdentNum;
                    wsError.Cell(1, 3).Value = Messages.FullName;
                    wsError.Cell(1, 4).Value = Messages.Address;
                    wsError.Cell(1, 5).Value = Messages.OwnershipDate;
                    wsError.Cell(1, 6).Value = Messages.IssueDate;
                    wsError.Cell(1, 7).Value = Messages.NumberOfOwners;
                    wsError.Cell(1, 8).Value = Messages.LicenseNumber + "*";
                    wsError.Cell(1, 9).Value = Messages.Valid + "*";
                    wsError.Cell(1, 10).Value = Messages.Type + "*";
                    wsError.Cell(1, 11).Value = Messages.Product + "*";
                    wsError.Cell(1, 12).Value = Messages.Model + "*";
                    wsError.Cell(1, 13).Value = Messages.YearOfProduction + "*";
                    wsError.Cell(1, 14).Value = $"{Messages.Weight} ({Messages.Kg})*";
                    wsError.Cell(1, 15).Value = Messages.ChassisNo + "*";
                    wsError.Cell(1, 16).Value = Messages.TypeOfDriversLicense + "*";
                    wsError.Cell(1, 17).Value = Messages.EngineNo + "*";
                    wsError.Cell(1, 18).Value = Messages.EngineProduct + "*";
                    wsError.Cell(1, 19).Value = Messages.EngineVolume + "*";
                    wsError.Cell(1, 20).Value = Messages.MaxHorsePower + "*";
                    wsError.Cell(1, 21).Value = Messages.TermsAndConditions + "*";
                    wsError.Cell(1, 22).Value = Messages.NumberOfImport + "*";

                    wsError.Cell(1, 24).Value = Messages.Reason;
                    #endregion

                    wsError.Columns(1, 22).AdjustToContents();
                    wsError.Column(24).Width = 60;

                    for (int i = 0; i < errorRows.Count; i++)
                    {
                        var row = errorRows[i].OriginalRow;

                        for (int j = 0; j < 22; j++)
                        {
                            wsError.Cell(i + 2, j + 1).DataType = row.Cell(j + 1).DataType;
                            wsError.Cell(i + 2, j + 1).SetValue(row.Cell(j + 1).Value);
                        }

                        if (errorRows[i].RowErrors.Count > 0)
                        {
                            foreach (var er in errorRows[i].RowErrors)
                            {
                                var line = wsError.Cell(i + 2, 24).RichText.AddNewLine();
                                line.AddText(string.Format("{0} - {1}", er.Key, er.Value));
                            }
                            wsError.Cell(i + 2, 24).Style.Alignment.WrapText = true;
                        }
                    }
                }
                #endregion

                #region Duplicate row

                if (duplicateRows != null && duplicateRows.Count > 0)
                {
                    var wsDuplicate = workBook.AddWorksheet(Messages.ImportPlayers_DuplicateWorksheetName);

                    #region Header
                    wsDuplicate.Cell(1, 1).Value = "#";
                    wsDuplicate.Cell(1, 2).Value = Messages.IdentNum;
                    wsDuplicate.Cell(1, 3).Value = Messages.FullName;
                    wsDuplicate.Cell(1, 4).Value = Messages.Address;
                    wsDuplicate.Cell(1, 5).Value = Messages.OwnershipDate;
                    wsDuplicate.Cell(1, 6).Value = Messages.IssueDate;
                    wsDuplicate.Cell(1, 7).Value = Messages.NumberOfOwners;
                    wsDuplicate.Cell(1, 8).Value = Messages.LicenseNumber + "*";
                    wsDuplicate.Cell(1, 9).Value = Messages.Valid + "*";
                    wsDuplicate.Cell(1, 10).Value = Messages.Type + "*";
                    wsDuplicate.Cell(1, 11).Value = Messages.Product + "*";
                    wsDuplicate.Cell(1, 12).Value = Messages.Model + "*";
                    wsDuplicate.Cell(1, 13).Value = Messages.YearOfProduction + "*";
                    wsDuplicate.Cell(1, 14).Value = $"{Messages.Weight} ({Messages.Kg})*";
                    wsDuplicate.Cell(1, 15).Value = Messages.ChassisNo + "*";
                    wsDuplicate.Cell(1, 16).Value = Messages.TypeOfDriversLicense + "*";
                    wsDuplicate.Cell(1, 17).Value = Messages.EngineNo + "*";
                    wsDuplicate.Cell(1, 18).Value = Messages.EngineProduct + "*";
                    wsDuplicate.Cell(1, 19).Value = Messages.EngineVolume + "*";
                    wsDuplicate.Cell(1, 20).Value = Messages.MaxHorsePower + "*";
                    wsDuplicate.Cell(1, 21).Value = Messages.TermsAndConditions + "*";
                    wsDuplicate.Cell(1, 22).Value = Messages.NumberOfImport + "*";

                    wsDuplicate.Cell(1, 24).Value = Messages.Reason;
                    #endregion

                    wsDuplicate.Columns(1, 22).AdjustToContents();
                    wsDuplicate.Column(22).Width = 60;

                    for (int i = 0; i < duplicateRows.Count; i++)
                    {
                        var row = duplicateRows[i].OriginalRow;

                        for (int j = 0; j < 22; j++)
                        {
                            wsDuplicate.Cell(i + 2, j + 1).DataType = row.Cell(j + 1).DataType;
                            wsDuplicate.Cell(i + 2, j + 1).SetValue(row.Cell(j + 1).Value);
                        }

                        if (duplicateRows[i].RowErrors.Count > 0)
                        {
                            string error = string.Join(@"\n", duplicateRows[i].RowErrors.Select(p => string.Format("{0}", p.Value)));

                            wsDuplicate.Cell(i + 2, 24).DataType = XLDataType.Text;
                            wsDuplicate.Cell(i + 2, 24).SetValue(error);
                        }
                    }
                }
                #endregion

                workBook.SaveAs(result);
                result.Position = 0;
            }

            return result;
        }

        public int ImportVehicles(int? unionId, int seasonId, List<ImportVehicleModel> correctRows,
            out List<ImportVehicleModel> importErrorRows, out List<ImportVehicleModel> duplicatedRows)
        {
            int importedRowCount = 0;
            importErrorRows = new List<ImportVehicleModel>();
            duplicatedRows = new List<ImportVehicleModel>();
            foreach (var vehicle in correctRows)
            {
                bool hasErrors = false;
                vehicleService.CheckForImportExcelValues(vehicle.IdentNum, vehicle.Product, vehicle.Model, vehicle.DriversLicenseType,
                    out int? driverId, out int? productId, out int? modelId, out int? driverLicenseTypeId);
                if (!string.IsNullOrEmpty(vehicle.IdentNum) && !driverId.HasValue)
                {
                    vehicle.RowErrors.Add(new KeyValuePair<string, string>(Messages.IdentNum, Messages.NoDriversFound.Replace("{0}",vehicle.IdentNum)));
                    hasErrors = true;
                }
                if (!productId.HasValue)
                {
                    vehicle.RowErrors.Add(new KeyValuePair<string, string>(Messages.Product, Messages.NoProductFound.Replace("{0}", vehicle.Product)));
                    hasErrors = true;
                }
                if (!modelId.HasValue)
                {
                    vehicle.RowErrors.Add(new KeyValuePair<string, string>(Messages.Model, Messages.NoProductFound.Replace("{0}", vehicle.Model)));
                    hasErrors = true;
                }
                if (!driverLicenseTypeId.HasValue)
                {
                    vehicle.RowErrors.Add(new KeyValuePair<string, string>(Messages.TypeOfDriversLicense, Messages.NoDriverLicense.Replace("{0}", vehicle.DriversLicenseType)));
                    hasErrors = true;
                }

                if (!hasErrors)
                {
                    if (vehicle.Id != 0)
                    {
                        vehicleRepo.UpdateVehicle(vehicle.Id, driverId, vehicle.OwnershipDate, vehicle.IssueDate, vehicle.NumberOfPreviousOwners ?? 0, vehicle.LicenseNumber, vehicle.Valid,
                            vehicle.Type, productId.Value, modelId.Value, vehicle.YearOfProduction, vehicle.Weight, vehicle.ChassisNo, driverLicenseTypeId.Value,
                            vehicle.EngineNo, vehicle.EngineVolume, vehicle.EngineProduct, vehicle.MaxHorsePower, vehicle.TermsAndConditions, vehicle.NumberOfImportEntry);
                        vehicleRepo.Save();
                    }
                    else
                    {
                        int? driverDetailsId = vehicleRepo.CreateDriverDetails(driverId, vehicle.OwnershipDate, vehicle.IssueDate, vehicle.NumberOfPreviousOwners ?? 0);
                        int vehicleLicenseId = vehicleRepo.CreateVehicleLicense(vehicle.LicenseNumber, vehicle.Valid);
                        int vehicleDetailsId = vehicleRepo.CreateVehicleDetails(vehicle.Type, productId.Value, modelId.Value, vehicle.YearOfProduction, vehicle.Weight, vehicle.ChassisNo, driverLicenseTypeId.Value);
                        int engineDetailsId = vehicleRepo.CreateEngineDetail(vehicle.EngineNo, vehicle.EngineVolume, vehicle.EngineProduct, vehicle.MaxHorsePower, vehicle.TermsAndConditions, vehicle.NumberOfImportEntry);
                        vehicleRepo.CreateVehicle(driverDetailsId, vehicleLicenseId, vehicleDetailsId, engineDetailsId, vehicle.UnionId, vehicle.SeasonId);

                    }
                    importedRowCount++;
                }
                else
                {
                    importErrorRows.Add(vehicle);
                }

            }

            return importedRowCount;
        }
    }
}

public class ImportVehicleModel : VehicleRegistration
{
    public ImportVehicleModel(IXLRow row)
    {
        OriginalRow = row;
        RowErrors = new List<KeyValuePair<string, string>>();
    }

    public IXLRow OriginalRow { get; set; }
    public List<KeyValuePair<string, string>> RowErrors { get; set; }
}