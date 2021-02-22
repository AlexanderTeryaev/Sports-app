using System;

namespace CmsApp.Controllers
{
    public class CompetitionResultForm
    {
        public int Id { get; set; }
        public int CompetitionDisciplineId { get; set; }
        public int UserId { get; set; }
        public string Heat { get; set; }
        public Nullable<int> Lane { get; set; }
        public string Result { get; set; }
        public Nullable<double> Wind { get; set; }
        public Nullable<int> Rank { get; set; }
        public Nullable<long> SortValue { get; set; }
        public Nullable<int> Format { get; set; }
        public string Result1 { get; set; }
        public string Result2 { get; set; }
        public string Result3 { get; set; }
        public string Result4 { get; set; }
        public int AlternativeResult { get; set; }
        public int? Points { get; set; }
    }
}