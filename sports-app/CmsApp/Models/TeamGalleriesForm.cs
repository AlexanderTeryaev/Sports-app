
using DataService.DTO;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class TeamGalleriesForm
    {
        public TeamGalleriesForm()
        {
            TeamGallery = new TeamGalleryForm();
        }
        public TeamGalleryForm TeamGallery { get; set; }
        public int TeamId { get; set; }
    }

    public class TeamGalleryModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public IList<TeamGalleriesForm> TeamGalleries { get; set; }
    }
}