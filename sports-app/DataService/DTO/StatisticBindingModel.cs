namespace DataService.DTO
{
    public class StatisticBindingModel
    {
        public string Abbreviation { get; set; }
        public string Category { get; set; }
        public int GameId { get; set; }
        public long GameTime { get; set; }
        public string Id { get; set; }
        public Point Location { get; set; }
        public string Note { get; set; }
        public int PlayerId { get; set; }
        public string ReporterId { get; set; }
        public int SegmentTimeStamp { get; set; }
        public string StatisticTypeId { get; set; }
        public string SyncStatus { get; set; }
        public int TeamId { get; set; }
        public int TimeSegment { get; set; }
        public string TimeSegmentName { get; set; }
        public long Timestamp { get; set; }
    }
}
