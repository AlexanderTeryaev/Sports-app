using AppModel;
using CmsApp.Models;
using DataService;
using DataService.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Services
{
    public class VehicleService
    {
        private DataEntities db;
        private PlayersRepo playersRepo;
        private VehicleRepo vehicleRepo;

        public VehicleService(DataEntities db)
        {
            this.db = db;
            playersRepo = new PlayersRepo(db);
            vehicleRepo = new VehicleRepo(db);
        }

        public IEnumerable<VehicleRegistrationShort> GetAllVehiclesShort(int id, int seasonId)
        {
            var vehicles = vehicleRepo.GetAllVehicles(id, seasonId);
            return ConvertToVehicleRegistrationForm(vehicles);
        }

        public IEnumerable<VehicleRegistration> GetAllVehicles(int id, int seasonId)
        {
            var vehicles = vehicleRepo.GetAllVehicles(id, seasonId);
            return ConvertToVehicleRegistrationFullForm(vehicles);
        }

        public void FillVehicleInformation(int id, Models.VehicleModel vm)
        {
            var vehicleModel = vehicleRepo.GetById(id);
            if (vehicleModel != null)
            {
                vm.SportsmanId = vehicleModel.DriverDetail?.SportsmanId;
                vm.IdentNum = vehicleModel.DriverDetail?.Sportsman?.User?.IdentNum;
                vm.FullName = vehicleModel.DriverDetail?.Sportsman?.User?.FullName;
                vm.Address = vehicleModel.DriverDetail?.Sportsman?.User?.Address ?? vehicleModel?.DriverDetail?.Sportsman?.User?.City;
                vm.OwnershipDate = vehicleModel.DriverDetail?.OwnerShipDate;
                vm.IssueDate = vehicleModel.DriverDetail?.IssueDate;
                vm.NumberOfPreviousOwners = vehicleModel.DriverDetail?.NumberOfPreviousOwners ?? 0;

                vm.LicenseNumber = vehicleModel.VehicleLicens.LicenseNumber;
                vm.Valid = vehicleModel.VehicleLicens.Valid;

                vm.Type = vehicleModel.VehicleDetail.Type;
                vm.ProductId = vehicleModel.VehicleDetail.ProductId;
                vm.ModelId = vehicleModel.VehicleDetail.ModelId;
                vm.YearOfProduction = vehicleModel.VehicleDetail.YearOfProduction;
                vm.Weight = vehicleModel.VehicleDetail.Weight;
                vm.ChassisNo = vehicleModel.VehicleDetail.ChassisNo;
                vm.TypeOfLicenseId = vehicleModel.VehicleDetail.DriverLivenceTypeId;

                vm.EngineNo = vehicleModel.EngineDetail.EngineNo;
                vm.EngineVolume = vehicleModel.EngineDetail.EngineVolume;
                vm.EngineProduct = vehicleModel.EngineDetail.EngineProduct;
                vm.MaxHorsePower = vehicleModel.EngineDetail.MaxPowerHp;
                vm.TermsAndConditions = vehicleModel.EngineDetail.TermsAndConditions;
                vm.NumberOfImportEntry = vehicleModel.EngineDetail.NumberOfImportEnrty;

            }
        }

        public IEnumerable<SelectListItem> GetAllSportsmenList(int unionId, int seasonId, int? selectedUserId)
        {
            var unionSportsmen = playersRepo.GetTeamPlayersShortByUnionId(unionId, seasonId);
            return new SelectList(unionSportsmen, nameof(PlayerViewModel.Id), nameof(PlayerViewModel.FullName), selectedUserId);
        }

        public IEnumerable<SelectListItem> GetDriversLicenseTypesList(string type = null, int? selectedValue = null)
        {
            if (string.Equals("Motorcycle", type, StringComparison.OrdinalIgnoreCase))
            {
                var motoDriversLicenseTypeList = vehicleRepo.GetAllDriverLicenses("Motorcycle");
                return new SelectList(motoDriversLicenseTypeList, nameof(DriverLicenseType.Id), nameof(DriverLicenseType.Name), selectedValue);
            }
            else if (string.Equals("ATV", type, StringComparison.OrdinalIgnoreCase))
            {
                var atvDriversLicenseTypeList = vehicleRepo.GetAllDriverLicenses("ATV");
                return new SelectList(atvDriversLicenseTypeList, nameof(DriverLicenseType.Id), nameof(DriverLicenseType.Name), selectedValue);
            }
            else
            {
                var driversLicenseTypeList = vehicleRepo.GetAllDriverLicenses();
                return new SelectList(driversLicenseTypeList, nameof(DriverLicenseType.Id), nameof(DriverLicenseType.Name), selectedValue);
            }
        }

        public void DeleteVehicle(int id)
        {
            vehicleRepo.DeleteVehicle(id);
            vehicleRepo.Save();
        }

        public void CreateVehicle(Models.VehicleModel form)
        {
            int? driverDetailsId = vehicleRepo.CreateDriverDetails(form.SportsmanId, form.OwnershipDate, form.IssueDate, form.NumberOfPreviousOwners);
            int vehicleLicenseId = vehicleRepo.CreateVehicleLicense(form.LicenseNumber, form.Valid);
            int vehicleDetailsId = vehicleRepo.CreateVehicleDetails(form.Type, form.ProductId, form.ModelId, form.YearOfProduction, form.Weight, form.ChassisNo, form.TypeOfLicenseId);
            int engineDetailsId = vehicleRepo.CreateEngineDetail(form.EngineNo, form.EngineVolume, form.EngineProduct, form.MaxHorsePower, form.TermsAndConditions, form.NumberOfImportEntry);
            vehicleRepo.CreateVehicle(driverDetailsId, vehicleLicenseId, vehicleDetailsId, engineDetailsId, form.UnionId, form.SeasonId);
        }

        public void UpdateVehicle(Models.VehicleModel form, int vehicleId)
        {
            vehicleRepo.UpdateVehicle(vehicleId, form.SportsmanId, form.OwnershipDate, form.IssueDate, form.NumberOfPreviousOwners, form.LicenseNumber, form.Valid,
                form.Type, form.ProductId, form.ModelId, form.YearOfProduction, form.Weight, form.ChassisNo, form.TypeOfLicenseId,
                form.EngineNo, form.EngineVolume, form.EngineProduct, form.MaxHorsePower, form.TermsAndConditions, form.NumberOfImportEntry);
            vehicleRepo.Save();
        }

        public IEnumerable<SelectListItem> GetVehicleProductsTypesList(string type = null, int? selectedValue = null)
        {
            if (string.Equals("Motorcycle", type, StringComparison.OrdinalIgnoreCase))
            {
                var motoDriversProductTypeList = vehicleRepo.GetVehicleProducts("Motorcycle");
                return new SelectList(motoDriversProductTypeList, nameof(VehicleProduct.Id), nameof(DriverLicenseType.Name), selectedValue);
            }
            else if (string.Equals("ATV", type, StringComparison.OrdinalIgnoreCase))
            {
                var atvProductTypeList = vehicleRepo.GetVehicleProducts("ATV");
                return new SelectList(atvProductTypeList, nameof(VehicleProduct.Id), nameof(DriverLicenseType.Name), selectedValue);
            }
            else
            {
                var prouctLicenseTypeList = vehicleRepo.GetAllDriverLicenses();
                return new SelectList(prouctLicenseTypeList, nameof(VehicleProduct.Id), nameof(DriverLicenseType.Name), selectedValue);
            }
        }

        public Dictionary<int, IEnumerable<KeyValuePair<int, string>>> GetVehicleModelsDictionary()
        {
            var productList = vehicleRepo.GetVehicleProducts();
            var result = new Dictionary<int, IEnumerable<KeyValuePair<int, string>>>();
            if (productList.Any())
            {
                foreach (var product in productList)
                {
                    result.Add(product.Id, GetModelsObject(product.VehicleModels));
                }
            }
            return result;
        }

        private IEnumerable<KeyValuePair<int, string>> GetModelsObject(ICollection<AppModel.VehicleModel> vehicleModels)
        {
            if (vehicleModels.Any())
            {
                foreach (var model in vehicleModels)
                {
                    yield return new KeyValuePair<int, string>(model.Id, model.Name);
                }
            }
        }

        private IEnumerable<VehicleRegistrationShort> ConvertToVehicleRegistrationForm(IEnumerable<Vehicle> vehicles)
        {
            if (vehicles.Any())
            {
                foreach (var vehicle in vehicles)
                {
                    yield return new VehicleRegistrationShort
                    {
                        Id = vehicle.Id,
                        Type = vehicle?.VehicleDetail.Type,
                        Product = vehicle.VehicleDetail.VehicleProduct.Name,
                        Model = vehicle.VehicleDetail.VehicleModel.Name,
                        YearOfProduction = vehicle.VehicleDetail.YearOfProduction,
                        FullName = vehicle.DriverDetail?.Sportsman?.User?.FullName,
                        IdentNum = vehicle.DriverDetail?.Sportsman?.User?.IdentNum,
                        OwnershipDate = vehicle.DriverDetail?.OwnerShipDate,
                        IssueDate = vehicle.DriverDetail?.IssueDate,
                    };
                }
            }
        }

        private IEnumerable<VehicleRegistration> ConvertToVehicleRegistrationFullForm(IEnumerable<Vehicle> vehicles)
        {
            if (vehicles.Any())
            {
                foreach (var vehicle in vehicles)
                {
                    yield return new VehicleRegistration
                    {
                        Id = vehicle.Id,
                        Type = vehicle?.VehicleDetail.Type,
                        Product = vehicle.VehicleDetail.VehicleProduct.Name,
                        Model = vehicle.VehicleDetail.VehicleModel.Name,
                        YearOfProduction = vehicle.VehicleDetail.YearOfProduction,
                        FullName = vehicle.DriverDetail?.Sportsman?.User?.FullName,
                        IdentNum = vehicle.DriverDetail?.Sportsman?.User?.IdentNum,
                        OwnershipDate = vehicle.DriverDetail?.OwnerShipDate,
                        IssueDate = vehicle.DriverDetail?.IssueDate,
                        SportsmanId = vehicle.DriverDetail?.SportsmanId,
                        Address = vehicle.DriverDetail?.Sportsman?.User?.Address ?? vehicle.DriverDetail?.Sportsman?.User?.City,
                        NumberOfPreviousOwners = vehicle.DriverDetail?.NumberOfPreviousOwners,
                        LicenseNumber = vehicle.VehicleLicens.LicenseNumber,
                        Valid = vehicle.VehicleLicens.Valid,
                        Weight = vehicle.VehicleDetail.Weight,
                        ChassisNo = vehicle.VehicleDetail.ChassisNo,
                        DriversLicenseType = vehicle.VehicleDetail.DriverLicenseType.Name,
                        EngineNo = vehicle.EngineDetail.EngineNo,
                        EngineVolume = vehicle.EngineDetail.EngineVolume,
                        EngineProduct = vehicle.EngineDetail.EngineProduct,
                        MaxHorsePower = vehicle.EngineDetail.MaxPowerHp,
                        TermsAndConditions = vehicle.EngineDetail.TermsAndConditions,
                        NumberOfImportEntry = vehicle.EngineDetail.NumberOfImportEnrty
                    };
                }
            }
        }

        internal void CheckForImportExcelValues(string identNum, string product, string model, string driversLicenseType, 
            out int? driverId, out int? productId, out int? modelId, out int? driverLicenseTypeId)
        {
            driverId = vehicleRepo.GetDriversId(identNum);
            productId = vehicleRepo.GetProductIdByName(product);
            modelId = vehicleRepo.GetModelIdByName(model);
            driverLicenseTypeId = vehicleRepo.GetDriverLicenseTypeIdByName(driversLicenseType);
        }
    }

    public class VehicleRegistrationShort
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Product { get; set; }
        public string Model { get; set; }
        public DateTime YearOfProduction { get; set; }
        public string FullName { get; set; }
        public string IdentNum { get; set; }
        public DateTime? OwnershipDate { get; set; }
        public DateTime? IssueDate { get; set; }

        public int UnionId { get; set; }
        public int SeasonId { get; set; }
    }

    public class VehicleRegistration : VehicleRegistrationShort
    {
        public int? SportsmanId { get; set; }
        public string Address { get; set; }
        public int? NumberOfPreviousOwners { get; set; }

        public string LicenseNumber { get; set; }
        public DateTime? Valid { get; set; }

        public int ProductId { get; set; }
        public int ModelId { get; set; }
        public double Weight { get; set; }
        public string ChassisNo { get; set; }
        public string DriversLicenseType { get; set; }

        public string EngineNo { get; set; }
        public string EngineVolume { get; set; }
        public string EngineProduct { get; set; }
        public double MaxHorsePower { get; set; }
        public string TermsAndConditions { get; set; }
        public int NumberOfImportEntry { get; set; }

    }
}