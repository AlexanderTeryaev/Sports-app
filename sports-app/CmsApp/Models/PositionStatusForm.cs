using System.ComponentModel.DataAnnotations;

namespace CmsApp.Models
{
    public class PositionStatusForm
    {
        public int Id { get; set; }
        public int LeagueId { get; set; }
        public int SeasonId { get; set; }
        [Required]
        public int? Position { get; set; }
        [Required]
        public int? Points { get; set; }
    }
}