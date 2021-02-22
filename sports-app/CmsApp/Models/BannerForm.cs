
using DataService.DTO;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class BannerForm
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string LinkUrl { get; set; }
        public string ImageUrl { get; set; }
        public int UnionId { get; set; }
        public int ClubId { get; set; }
        public int isUnion { get; set; }
        public string BannerFile { get; set; }
    }

    public class BannerModel
    {
        public int parentId { get; set; }
        public int isUnion { get; set; }
        public IList<BannerForm> Banners { get; set; }
    }
}