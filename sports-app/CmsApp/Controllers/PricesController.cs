using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AppModel;
using CmsApp.Helpers.ActivityHelpers;
using CmsApp.Models;
using Resources;

namespace CmsApp.Controllers
{
    public class PricesController : AdminController
    {
        public ActionResult ChipPricesList(int seasonId, int unionId)
        {
            var vm = new PricesTabViewModel();
            var chipPrices = pricesRepo.GetChipPricesBySeasonAndUnion(seasonId, unionId);
            vm.ChipPriceViewModelsList = chipPrices.Select(x => new ChipPriceViewModel
            {
                UnionId = x.UnionId,
                SeasonId = x.SeasonId,
                ChipId = x.ChipId,
                FromAge = x.FromAge,
                Price = x.Price,
                ToAge = x.ToAge
            }).ToList();
            ViewBag.SeasonId = seasonId;
            ViewBag.UnionId = unionId;

            return PartialView("_ChipPricesList", vm);
        }

        public ActionResult UpdateChipPrice(ChipPriceViewModel data)
        {
            pricesRepo.UpdateChipPrice(data.ChipId, data.FromAge, data.ToAge, data.Price);
            return RedirectToAction(nameof(ChipPricesList), new { SeasonId = data.SeasonId, UnionId = data.UnionId });
        }

        public ActionResult DeleteChipPrice(int chipId)
        {
            var item = pricesRepo.GetChipPriceById(chipId);
            pricesRepo.DeleteChipPrice(chipId);
            return RedirectToAction(nameof(ChipPricesList), new { seasonId = item.SeasonId, unionId = item.UnionId });
        }

        [HttpPost]
        public ActionResult SaveChipPrice(int unionId, int seasonId, int? fromAge, int? toAge, int? price)
        {
            var chipPrice = new ChipPrice
            {
                UnionId = unionId,
                SeasonId = seasonId,
                FromAge = fromAge,
                ToAge = toAge,
                Price = price
            };

            pricesRepo.CreateChipPrice(chipPrice);

            return RedirectToAction(nameof(ChipPricesList), new { seasonId, unionId });
        }

        public ActionResult FriendshipPricesList(int seasonId, int unionId)
        {
            var vm = new PricesTabViewModel();
            var friendshipPrices = pricesRepo.GetFriendshipPricesBySeasonAndUnion(seasonId, unionId);
            var friendshipsTypes = friendshipTypesRepo.GetAllByUnionId(unionId);
            vm.FriendshipPriceViewModelsList = friendshipPrices.Select(x => new FriendshipPriceViewModel
            {
                UnionId = x.UnionId,
                SeasonId = x.SeasonId,
                FriendshipPricesId = x.FriendshipPricesId,
                FromAge = x.FromAge,
                Price = x.Price,
                PriceForNew = x.PriceForNew,
                ToAge = x.ToAge,
                GenderId = x.GenderId,
                FriendshipsTypeId = x.FriendshipsTypeId,
                FriendshipPriceType = x.FriendshipPriceType,
                GendersList = GetGendersSelectList(x.GenderId),
                FriendshipTypes = GetFriendshipTypesSelectList(friendshipsTypes, x.FriendshipsTypeId),
                FriendshipPriceTypes = CustomPriceHelper.GetFriendshipPriceTypesSelectList(x.FriendshipPriceType)
            }).ToList();
            vm.GendersList = GetGendersSelectList(null);
            vm.FriendshipTypes = GetFriendshipTypesSelectList(friendshipsTypes, null);
            vm.FriendshipPriceTypes = CustomPriceHelper.GetFriendshipPriceTypesSelectList(null);
            ViewBag.SeasonId = seasonId;
            ViewBag.UnionId = unionId;

            return PartialView("_FriendshipPricesList", vm);
        }

        public ActionResult UpdateFriendshipPrice(FriendshipPriceViewModel data)
        {
            pricesRepo.UpdateFriendshipPrice(data.FriendshipPricesId, data.FromAge, data.ToAge, data.GenderId, data.FriendshipsTypeId, data.Price, data.FriendshipPriceType, data.PriceForNew);
            return RedirectToAction(nameof(FriendshipPricesList), new { SeasonId = data.SeasonId, UnionId = data.UnionId });
        }

        public ActionResult DeleteFriendshipPrice(int friendshipId)
        {
            var item = pricesRepo.GetFriendshipPriceById(friendshipId);
            pricesRepo.DeleteFriendshipPrice(friendshipId);
            return RedirectToAction(nameof(FriendshipPricesList), new { seasonId = item.SeasonId, unionId = item.UnionId });
        }

        [HttpPost]
        public ActionResult SaveFriendshipPrice(int unionId, int seasonId, int? fromAge, int? toAge, int? genderId, int? friendshipTypeId, int? price, int? friendshipPriceTypeId, int? priceForNew)
        {
            var friendshipPrice = new FriendshipPrice
            {
                UnionId = unionId,
                SeasonId = seasonId,
                FromAge = fromAge,
                ToAge = toAge,
                GenderId = genderId,
                FriendshipPriceType = friendshipPriceTypeId,
                FriendshipsTypeId = friendshipTypeId,
                Price = price,
                PriceForNew = priceForNew
            };

            pricesRepo.CreateFriendshipPrice(friendshipPrice);

            return RedirectToAction(nameof(FriendshipPricesList), new { seasonId, unionId });
        }

        [HttpPost]
        public ActionResult GetFriendshipPricesTypes(int? friendshipTypeId, int userId, int seasonId)
        {
            var user = usersRepo.GetById(userId);
            var season = seasonsRepository.GetById(seasonId);

            var age = Convert.ToInt32(season.Name) - user.BirthDay.Value.Year;

            var ids = new List<int?>();
            if (friendshipTypeId.HasValue && friendshipTypeId != -1)
            {
                var ftype = friendshipTypesRepo.GetById(friendshipTypeId.Value);
                ids = ftype.FriendshipPrices.Where(x => x.FriendshipPriceType != null && x.FromAge <= age && x.ToAge >= age && (x.GenderId == user.GenderId || x.GenderId == 3)).Select(x => x.FriendshipPriceType).ToList();
            }
            return Json(new { Data = CustomPriceHelper.GetFriendshipPriceTypesSelectList(null, ids) });
        }

        [HttpPost]
        public ActionResult GetFriendshipPricesTypesByBirthDate(int? friendshipTypeId, DateTime? birthDay, int seasonId, int? genderId)
        {
            var season = seasonsRepository.GetById(seasonId);           

            var ids = new List<int?>();
            if (friendshipTypeId.HasValue && friendshipTypeId != -1 && birthDay.HasValue && genderId.HasValue)
            {
                genderId = genderId - 10; //Found In TeamPlayers controller this way of handling 
                var age = Convert.ToInt32(season.Name) - birthDay.Value.Year;
                var ftype = friendshipTypesRepo.GetById(friendshipTypeId.Value);
                ids = ftype.FriendshipPrices.Where(x => x.FriendshipPriceType != null && x.FromAge <= age && x.ToAge >= age && (x.GenderId == genderId || x.GenderId == 3)).Select(x => x.FriendshipPriceType).ToList();
            }
            return Json(new { Data = CustomPriceHelper.GetFriendshipPriceTypesSelectList(null, ids) });
        }


        private List<SelectListItem> GetFriendshipTypesSelectList(List<FriendshipsType> friendshipsTypes, int? friendshipTypeId)
        {
            var friendshipTypesSelectList = new List<SelectListItem>();
            friendshipTypesSelectList.Add(new SelectListItem
            {
                Value = "0",
                Text = Messages.Select,
                Selected = !friendshipTypeId.HasValue
            });
            friendshipTypesSelectList.AddRange(friendshipsTypes.Select(x => new SelectListItem
            {
                Value = x.FriendshipsTypesId.ToString(),
                Text = x.Name,
                Selected = x.FriendshipsTypesId == friendshipTypeId
            }));
            return friendshipTypesSelectList;
        }

        private static List<SelectListItem> GetGendersSelectList(int? genderId)
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = null,
                    Text = Messages.Select,
                    Selected = !genderId.HasValue
                },
                new SelectListItem
                {
                    Value = "0",
                    Text = Messages.Female,
                    Selected = genderId == 0
                },
                new SelectListItem
                {
                    Value = "1",
                    Text = Messages.Male,
                    Selected = genderId == 1
                },
                new SelectListItem
                {
                    Value = "3",
                    Text = Messages.All,
                    Selected = genderId == 3
                }
            };
        }
    }
}