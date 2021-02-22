using CmsApp.Helpers;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class ExportImageModel
    {
        public int? SeasonId { get; set; }
        public int? UnionId { get; set; }
        public int? ClubId { get; set; }
        public int? LeagueId { get; set; }
        public int? TeamId { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasPictures { get; set; }
        public int Count { get; set; }
        public PageType Page { get; set; }
        public IEnumerable<int> LeaguesIds { get; set; }
    }
}