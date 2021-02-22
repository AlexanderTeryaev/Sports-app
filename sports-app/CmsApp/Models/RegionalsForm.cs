using AppModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataService.DTO;

namespace CmsApp.Models
{
    public class RegionalsForm
    {
        public int? RegionalId { get; set; }
        public int? SeasonId { get; set; }
        public int? UnionId { get; set; }
        public List<Regional> Regionals { get; set; }
        public List<UsersJob> RegionalManagers { get; set; }
        [Required]
        public string Name { get; set; }
    }
}