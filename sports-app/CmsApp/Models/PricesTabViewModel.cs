using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class PricesTabViewModel
    {
        public List<ChipPriceViewModel> ChipPriceViewModelsList { get; set; }
        public List<FriendshipPriceViewModel> FriendshipPriceViewModelsList { get; set; }
        public int? GenderId { get; set; }
        public List<SelectListItem> GendersList { get; set; }
        public int? FriendshipTypeId { get; set; }
        public List<SelectListItem> FriendshipTypes { get; set; }
        public int? FriendshipPriceTypeId { get; set; }
        public List<SelectListItem> FriendshipPriceTypes { get; set; }
    }

    public class ChipPriceViewModel
    {
        public int ChipId { get; set; }
        public int UnionId { get; set; }
        public int? FromAge { get; set; }
        public int? ToAge { get; set; }
        public int? Price { get; set; }
        public int SeasonId { get; set; }
    }

    public class FriendshipPriceViewModel
    {
        public int FriendshipPricesId { get; set; }
        public int UnionId { get; set; }
        public int? FriendshipsTypeId { get; set; }
        public int? FromAge { get; set; }
        public int? ToAge { get; set; }
        public int? GenderId { get; set; }
        public int? FriendshipPriceType { get; set; }
        public int? Price { get; set; }
        public int? PriceForNew { get; set; }
        public int SeasonId { get; set; }
        public List<SelectListItem> GendersList { get; set; }
        public List<SelectListItem> FriendshipTypes { get; set; }
        public List<SelectListItem> FriendshipPriceTypes { get; set; }
    }
}