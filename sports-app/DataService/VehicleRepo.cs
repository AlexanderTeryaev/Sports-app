using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;

namespace DataService
{
    public class VehicleRepo : BaseRepo
    {
        public VehicleRepo() : base() { }
        public VehicleRepo(DataEntities db) : base(db) { }

        public IEnumerable<DriverLicenseType> GetAllDriverLicenses(string type = null)
        {
            return !string.IsNullOrEmpty(type)
                ? db.DriverLicenseTypes.Where(dlt => type.Equals(dlt.Type, StringComparison.OrdinalIgnoreCase))
                : db.DriverLicenseTypes;
        }

        public IEnumerable<VehicleProduct> GetVehicleProducts(string type = null)
        {
            return !string.IsNullOrEmpty(type)
                ? db.VehicleProducts.Where(vp => type.Equals(vp.Type, StringComparison.OrdinalIgnoreCase))
                : db.VehicleProducts;
        }

        public IEnumerable<Vehicle> GetAllVehicles(int unionId, int seasonId)
        {
            return db.Vehicles.Where(v => v.SeasonId == seasonId && v.UnionId == unionId && !v.IsDeleted);
        }

        public IEnumerable<VehicleModel> GetModelsByProductId(int id)
        {
            return db.VehicleModels.Where(vm => vm.ProductId == id);
        }

        public Vehicle GetById(int id)
        {
            return db.Vehicles.FirstOrDefault(v => v.Id == id);
        }

        public int? CreateDriverDetails(int? sportsmanId, DateTime? ownershipDate, DateTime? issueDate, int numberOfPreviousOwners)
        {
            int? driverDetailsId = null;

            if (sportsmanId.HasValue)
            {
                db.DriverDetails.Add(new DriverDetail
                {
                    SportsmanId = sportsmanId.Value,
                    OwnerShipDate = ownershipDate.Value,
                    IssueDate = issueDate.Value,
                    NumberOfPreviousOwners = numberOfPreviousOwners
                });
                db.SaveChanges();
                driverDetailsId = db.DriverDetails.OrderByDescending(d => d.Id).FirstOrDefault()?.Id;
            }
            return driverDetailsId;
        }

        public int CreateVehicleLicense(string licenseNumber, DateTime? valid)
        {
            db.VehicleLicenses.Add(new VehicleLicens
            {
                LicenseNumber = licenseNumber,
                Valid = valid.Value
            });
            db.SaveChanges();
            return db.VehicleLicenses.OrderByDescending(vl => vl.Id).FirstOrDefault().Id;
        }

        public int CreateVehicleDetails(string type, int productId, int modelId, DateTime? yearOfProduction, double weight, string chassisNo, int typeOfLicenseId)
        {
            db.VehicleDetails.Add(new VehicleDetail
            {
                Type = type,
                ProductId = productId,
                ModelId = modelId,
                YearOfProduction = yearOfProduction.Value,
                Weight = weight,
                ChassisNo = chassisNo,
                DriverLivenceTypeId = typeOfLicenseId
            });
            db.SaveChanges();
            return db.VehicleDetails.OrderByDescending(vd => vd.Id).FirstOrDefault().Id;
        }

        public void CreateVehicle(int? driverDetailsId, int vehicleLicenseId, int vehicleDetailsId, int engineDetailsId, int unionId, int seasonId)
        {
            db.Vehicles.Add(new Vehicle
            {
                DriverDetailsId = driverDetailsId,
                VehicleLicenseId = vehicleLicenseId,
                VehicleDetailsId = vehicleDetailsId,
                EngineDetailsId = engineDetailsId,
                UnionId = unionId,
                SeasonId = seasonId,
            });
            db.SaveChanges();
        }

        public void DeleteVehicle(int id)
        {
            var vehicle = db.Vehicles.Find(id);
            vehicle.IsDeleted = true;
        }

        public int CreateEngineDetail(string engineNo, string engineVolume, string engineProduct, double maxHorsePower, string termsAndConditions, int numberOfImportEntry)
        {
            db.EngineDetails.Add(new EngineDetail
            {
                EngineNo = engineNo,
                EngineVolume = engineVolume,
                EngineProduct = engineProduct,
                MaxPowerHp = maxHorsePower,
                TermsAndConditions = termsAndConditions,
                NumberOfImportEnrty = numberOfImportEntry
            });
            db.SaveChanges();
            return db.EngineDetails.OrderByDescending(ed => ed.Id).FirstOrDefault().Id;

        }

        public void UpdateVehicle(int vehicleId, int? sportsmanId, DateTime? ownershipDate, DateTime? issueDate, int numberOfPreviousOwners,
            string licenseNumber, DateTime? valid, string type, int productId, int modelId, DateTime? yearOfProduction, double weight, string chassisNo,
            int typeOfLicenseId, string engineNo, string engineVolume, string engineProduct, double maxHorsePower, string termsAndConditions, int numberOfImportEntry)
        {
            var vehicle = db.Vehicles.FirstOrDefault(v => v.Id == vehicleId);

            if (vehicle != null)
            {
                #region Driver details update

                if (vehicle.DriverDetailsId.HasValue && sportsmanId.HasValue)
                {
                    vehicle.DriverDetail.SportsmanId = sportsmanId.Value;
                    if (ownershipDate.HasValue)
                        vehicle.DriverDetail.OwnerShipDate = ownershipDate.Value;
                    if (issueDate.HasValue)
                        vehicle.DriverDetail.IssueDate = issueDate.Value;
                }
                else if (vehicle.DriverDetailsId.HasValue && !sportsmanId.HasValue)
                {
                    vehicle.DriverDetailsId = null;
                }
                else if (!vehicle.DriverDetailsId.HasValue && sportsmanId.HasValue)
                {
                    db.DriverDetails.Add(new DriverDetail
                    {
                        SportsmanId = sportsmanId.Value,
                        OwnerShipDate = ownershipDate.Value,
                        IssueDate = issueDate.Value,
                        NumberOfPreviousOwners = numberOfPreviousOwners
                    });
                    db.SaveChanges();
                    var newDriverDetailsId = db.DriverDetails.OrderByDescending(d => d.Id).FirstOrDefault().Id;
                    vehicle.DriverDetailsId = newDriverDetailsId;
                }

                #endregion

                #region Vehicle license update

                vehicle.VehicleLicens.LicenseNumber = licenseNumber;
                vehicle.VehicleLicens.Valid = valid.Value;

                #endregion

                #region Vehicle details update

                vehicle.VehicleDetail.Type = type;
                vehicle.VehicleDetail.ProductId = productId;
                vehicle.VehicleDetail.ModelId = modelId;
                vehicle.VehicleDetail.YearOfProduction = yearOfProduction.Value;
                vehicle.VehicleDetail.Weight = weight;
                vehicle.VehicleDetail.ChassisNo = chassisNo;
                vehicle.VehicleDetail.DriverLivenceTypeId = typeOfLicenseId;

                #endregion

                #region Engine details update

                vehicle.EngineDetail.EngineNo = engineNo;
                vehicle.EngineDetail.EngineVolume = engineVolume;
                vehicle.EngineDetail.EngineProduct = engineProduct;
                vehicle.EngineDetail.MaxPowerHp = maxHorsePower;
                vehicle.EngineDetail.TermsAndConditions = termsAndConditions;
                vehicle.EngineDetail.NumberOfImportEnrty = numberOfImportEntry;

                #endregion

            }
        }

        public int? CreateDriverDetailsForImport(string identNum, DateTime? ownershipDate, DateTime? issueDate, int? numberOfPreviousOwners)
        {
            int? driverDetailsId = null;

            var sportsmanId = db.TeamsPlayers.FirstOrDefault(tp => tp.User.IdentNum == identNum)?.Id;

            if (sportsmanId.HasValue)
            {
                db.DriverDetails.Add(new DriverDetail
                {
                    SportsmanId = sportsmanId.Value,
                    OwnerShipDate = ownershipDate.Value,
                    IssueDate = issueDate.Value,
                    NumberOfPreviousOwners = numberOfPreviousOwners ?? 0
                });
                db.SaveChanges();
                driverDetailsId = db.DriverDetails.OrderByDescending(d => d.Id).FirstOrDefault()?.Id;
            }
            return driverDetailsId;
        }

        public int? GetDriversId(string identNum)
        {
            return !string.IsNullOrEmpty(identNum) ? db.TeamsPlayers.FirstOrDefault(tp => tp.User.IdentNum.Equals(identNum))?.Id : null;
        }

        public int? GetProductIdByName(string product)
        {
            return db.VehicleProducts.FirstOrDefault(vp => vp.Name.Equals(product, StringComparison.OrdinalIgnoreCase))?.Id;
        }

        public int? GetModelIdByName(string model)
        {
            return db.VehicleModels.FirstOrDefault(vm => vm.Name.Equals(model, StringComparison.OrdinalIgnoreCase))?.Id;

        }

        public int? GetDriverLicenseTypeIdByName(string driverLicense)
        {
            return db.DriverLicenseTypes.FirstOrDefault(vm => vm.Name.Equals(driverLicense, StringComparison.OrdinalIgnoreCase))?.Id;
        }
    }
}
