namespace CmsApp.Models
{
    public class UnionFormModel
    {
        public int FormId { get; set; }
        public int? SeasonId { get; set; }
        public int UnionId { get; set; }
        public string Path { get; set; }
        public string Title { get; set; }
    }
}