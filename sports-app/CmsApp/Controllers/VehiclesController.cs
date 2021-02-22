using ClosedXML.Excel;
using CmsApp.Helpers;
using CmsApp.Models;
using CmsApp.Services;
using Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace CmsApp.Controllers
{
    public class VehiclesController : AdminController
    {
        private VehicleService vehicleService;
        private const string ImportVehiclesErrorResultSessionKey = "ImportPlayers_ErrorResult";
        private const string ImportVehiclesErrorResultFileNameSessionKey = "ImportPlayers_ErrorResultFileName";
        private const string ExportVehiclesResultSessionKey = "ExportPlayers_Result";
        private const string ExportVehiclesResultFileNameSessionKey = "ExportPlayers_ResultFileName";

        public VehiclesController()
        {
            vehicleService = new VehicleService(db);
        }

        public ActionResult List(int id, int seasonId)
        {
            var vm = vehicleService.GetAllVehiclesShort(id, seasonId);
            ViewBag.UnionId = id;
            ViewBag.SeasonId = seasonId;

            return PartialView("_List", vm);
        }

        public ActionResult Create(int unionId, int seasonId, int? id = null)
        {
            var vm = new VehicleModel
            {
                UnionId = unionId,
                SeasonId = seasonId,
                VehicleId = id
            };
            if (id.HasValue)
            {
                vehicleService.FillVehicleInformation(id.Value, vm);
            }

            ViewBag.UnionId = unionId;
            ViewBag.SeasonId = seasonId;
            ViewBag.ListOfSportsmen = vehicleService.GetAllSportsmenList(unionId, seasonId, id);
            ViewBag.ListOfMotorcycleDriverLicensesTypes = vehicleService.GetDriversLicenseTypesList("Motorcycle", vm.TypeOfLicenseId);
            ViewBag.ListOfAtvDriverLicensesTypes = vehicleService.GetDriversLicenseTypesList("ATV", vm.TypeOfLicenseId);
            ViewBag.ListOfMotorcycleProductTypes = vehicleService.GetVehicleProductsTypesList("Motorcycle", vm.ProductId);
            ViewBag.ListOfAtvDriverProductTypes = vehicleService.GetVehicleProductsTypesList("ATV", vm.ProductId);
            ViewBag.DictionaryOfVehicleModelsValues = vehicleService.GetVehicleModelsDictionary();

            return PartialView("_Create", vm);
        }

        [HttpPost]
        public ActionResult Create(VehicleModel form)
        {
            if (form.SportsmanId.HasValue)
            {
                if (!form.OwnershipDate.HasValue)
                {
                    ModelState.AddModelError("OwnershipDate", $"\"{Messages.OwnershipDate}\" {Messages.FieldIsRequired.ToLower()}");
                }
                if (!form.IssueDate.HasValue)
                {
                    ModelState.AddModelError("IssueDate", $"\"{Messages.IssueDate}\" {Messages.FieldIsRequired.ToLower()}");
                }
            }

            if (ModelState.IsValid)
            {
                if (form.VehicleId.HasValue)
                {
                    vehicleService.UpdateVehicle(form, form.VehicleId.Value);
                }
                else
                {
                    vehicleService.CreateVehicle(form);
                }
                return Json(new { Success = true });
            }

            ViewBag.UnionId = form.UnionId;
            ViewBag.SeasonId = form.SeasonId;
            ViewBag.ListOfSportsmen = vehicleService.GetAllSportsmenList(form.UnionId, form.SeasonId, form.VehicleId);
            ViewBag.ListOfMotorcycleDriverLicensesTypes = vehicleService.GetDriversLicenseTypesList("Motorcycle", form.TypeOfLicenseId);
            ViewBag.ListOfAtvDriverLicensesTypes = vehicleService.GetDriversLicenseTypesList("ATV", form.TypeOfLicenseId);
            ViewBag.ListOfMotorcycleProductTypes = vehicleService.GetVehicleProductsTypesList("Motorcycle", form.ProductId);
            ViewBag.ListOfAtvDriverProductTypes = vehicleService.GetVehicleProductsTypesList("ATV", form.ProductId);
            ViewBag.DictionaryOfVehicleModelsValues = vehicleService.GetVehicleModelsDictionary();

            return PartialView("_Create", form);


        }

        [HttpPost]
        public ActionResult FillSportsmanInfo(int unionId, int seasonId, int? teamPlayerId)
        {
            var player = playersRepo.GetTeamsPlayerById(teamPlayerId ?? 0);
            var vm = new VehicleModel();
            if (player != null)
            {
                vm.SportsmanId = player.Id;
                vm.IdentNum = player.User.IdentNum;
                vm.FullName = player.User.FullName;
                vm.Address = player.User.Address ?? player.User.City;
            }

            ViewBag.UnionId = unionId;
            ViewBag.SeasonId = seasonId;
            ViewBag.ListOfSportsmen = vehicleService.GetAllSportsmenList(unionId, seasonId, teamPlayerId);

            return PartialView("_DriverDetails", vm);
        }

        public ActionResult Delete(int id, int unionId, int seasonId)
        {
            vehicleService.DeleteVehicle(id);
            return RedirectToAction(nameof(List), new { id = unionId, seasonId });
        }

        [HttpPost]
        public void ExportVehiclesToExcel(int unionId, int seasonId, bool onlyHeader)
        {
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var vehicleRegistrations = vehicleService.GetAllVehicles(unionId, seasonId);

                var ws = !onlyHeader ? workbook.AddWorksheet($"{Messages.VehicleRegistrationsList}") : workbook.AddWorksheet($"{Messages.VehiclesFormCaption}");

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                #region Excel header

                addCell("#");
                addCell(Messages.IdentNum);
                addCell(Messages.FullName);
                addCell(Messages.Address);
                addCell(Messages.OwnershipDate);
                addCell(Messages.IssueDate);
                addCell(!onlyHeader ? Messages.NumberOfOwners : Messages.NumberOfOwners);
                addCell(!onlyHeader ? Messages.LicenseNumber : Messages.LicenseNumber + "*");
                addCell(!onlyHeader ? Messages.Valid : Messages.Valid + "*");
                addCell(!onlyHeader ? Messages.Type : Messages.Type + "*");
                addCell(!onlyHeader ? Messages.Product : Messages.Product + "*");
                addCell(!onlyHeader ? Messages.Model : Messages.Model + "*");
                addCell(!onlyHeader ? Messages.YearOfProduction : Messages.YearOfProduction + "*");
                addCell(!onlyHeader ? $"{Messages.Weight} ({Messages.Kg})" : $"{Messages.Weight} ({Messages.Kg})*");
                addCell(!onlyHeader ? Messages.ChassisNo : Messages.ChassisNo + "*");
                addCell(!onlyHeader ? Messages.TypeOfDriversLicense : Messages.TypeOfDriversLicense + "*");
                addCell(!onlyHeader ? Messages.EngineNo : Messages.EngineNo + "*");
                addCell(!onlyHeader ? Messages.EngineProduct : Messages.EngineProduct + "*");
                addCell(!onlyHeader ? Messages.EngineVolume : Messages.EngineVolume + "*");
                addCell(!onlyHeader ? Messages.MaxHorsePower : Messages.MaxHorsePower + "*");
                addCell(!onlyHeader ? Messages.TermsAndConditions : Messages.TermsAndConditions + "*");
                addCell(!onlyHeader ? Messages.NumberOfImport : Messages.NumberOfImport + "*");

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                #endregion


                if (!onlyHeader)
                {
                    #region Excel body

                    foreach (var row in vehicleRegistrations)
                    {
                        addCell(row.Id.ToString());
                        addCell(row.IdentNum);
                        addCell(row.FullName);
                        addCell(row.Address);
                        addCell(row.OwnershipDate.ToString());
                        addCell(row.IssueDate.ToString());
                        addCell(row.NumberOfPreviousOwners.ToString());
                        addCell(row.LicenseNumber);
                        addCell(row.Valid.ToString());
                        addCell(row.Type);
                        addCell(row.Product);
                        addCell(row.Model);
                        addCell(row.YearOfProduction.ToString());
                        addCell(row.Weight.ToString());
                        addCell(row.ChassisNo);
                        addCell(row.DriversLicenseType);
                        addCell(row.EngineNo.ToString());
                        addCell(row.EngineProduct);
                        addCell(row.EngineVolume);
                        addCell(row.MaxHorsePower.ToString());
                        addCell(row.TermsAndConditions);
                        addCell(row.NumberOfImportEntry.ToString());

                        rowCounter++;
                        columnCounter = 1;
                    }

                    ws.Columns().AdjustToContents();

                    #endregion
                }



                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                if (onlyHeader)
                {
                    Response.AddHeader("content-disposition", $"attachment;filename={Messages.VehiclesFormCaption.ToLower().Replace(" ", "_")}.xlsx");
                }
                else
                {
                    Response.AddHeader("content-disposition", $"attachment;filename={Messages.Vehicles.ToLower()}_{Messages.Registrations.ToLower()}.xlsx");
                }

                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }


        [HttpGet]
        public ActionResult ImportVehicles(int unionId, int seasonId)
        {
            ImportVehiclesViewModel model = new ImportVehiclesViewModel
            {
                UnionId = unionId,
                SeasonId = seasonId
            };
            //TODO: Create import vehicle 
            return PartialView("_ImportVehicles", model);
        }


        [HttpPost]
        public ActionResult ImportVehicles(ImportVehiclesViewModel model)
        {
            try
            {
                if (model.ImportFile != null)
                {
                    var importHelper = new ImportExportVehiclesHelper(db);
                    List<ImportVehicleModel> correctRows = null;
                    List<ImportVehicleModel> validationErrorRows = null;

                    importHelper.ExtractVehiclesData(model.ImportFile.InputStream, out correctRows, out validationErrorRows, model.UnionId, model.SeasonId);

                    if (correctRows.Count > 0)
                    {
                        List<ImportVehicleModel> importErrorRows = null;
                        List<ImportVehicleModel> duplicatedRows = null;

                        model.SuccessCount = importHelper.ImportVehicles(model.UnionId, model.SeasonId,
                            correctRows, out importErrorRows, out duplicatedRows);

                        validationErrorRows.AddRange(importErrorRows);

                        if (validationErrorRows.Count > 0 || duplicatedRows.Count > 0 && model.SuccessCount > 0)
                        {
                            CreateErrorImportFile(importHelper, model, validationErrorRows, duplicatedRows);
                            model.Result = ImportPlayersResult.PartialyImported;
                            model.ResultMessage = Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, Messages.Gymnastics);
                        }
                        else if (validationErrorRows.Count == 0 || duplicatedRows.Count == 0 && model.SuccessCount > 0)
                        {
                            model.Result = ImportPlayersResult.Success;
                        }
                        else
                        {
                            model.Result = ImportPlayersResult.Error;
                        }

                        model.ErrorCount = validationErrorRows.Count;
                        model.DuplicateCount = duplicatedRows.Count;
                    }
                    else
                    {
                        model.Result = ImportPlayersResult.Error;
                        model.ResultMessage = Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, Messages.Vehicles);
                        model.SuccessCount = 0;
                        model.DuplicateCount = 0;
                        model.ErrorCount = validationErrorRows.Count;

                        CreateErrorImportFile(importHelper, model, validationErrorRows, null);
                    }
                }
                else
                {
                    model.Result = ImportPlayersResult.Error;
                    model.ResultMessage = Messages.ImportPlayers_ChooseFile;
                }

                return PartialView("_ImportVehicles", model);
            }
            catch (Exception ex)
            {
                model.Result = ImportPlayersResult.Error;
                model.ResultMessage = Messages.ImportPlayers_ImportException;
                model.ExceptionMessage = ex.Message;
                model.IsException = true;
                return PartialView("_ImportVehicles", model);
            }
        }

        [NonAction]
        private void CreateErrorImportFile(ImportExportVehiclesHelper importHelper, ImportVehiclesViewModel model,
            List<ImportVehicleModel> validationErrorRows, List<ImportVehicleModel> duplicatedRows)
        {
            var culture = getCulture();
            byte[] errorFileBytes = null;
            using (var errorFile = importHelper.BuildErrorFile(validationErrorRows, duplicatedRows, culture))
            {
                errorFileBytes = new byte[errorFile.Length];
                errorFile.Read(errorFileBytes, 0, errorFileBytes.Length);
            }
            Session.Remove(ImportVehiclesErrorResultSessionKey);
            Session.Remove(ImportVehiclesErrorResultFileNameSessionKey);
            Session.Add(ImportVehiclesErrorResultSessionKey, errorFileBytes);
            Session.Add(ImportVehiclesErrorResultFileNameSessionKey, model.ImportFile.FileName);
        }

        [HttpGet]
        public ActionResult DownloadPartiallyImport()
        {
            var fileByteObj = Session[ImportVehiclesErrorResultSessionKey];
            if (fileByteObj == null)
            {
                throw new FileNotFoundException();
            }

            var fileBytes = (byte[])fileByteObj;
            var fileName = Session[ImportVehiclesErrorResultFileNameSessionKey] as string;

            FileInfo fi = new FileInfo(fileName);

            return File(fileBytes, "application/octet-stream", string.Format("{0}-{2}{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension, Messages.ImportPlayers_OutputFilePrefix));
        }

    }
}
