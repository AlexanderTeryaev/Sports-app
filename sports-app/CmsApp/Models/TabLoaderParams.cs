namespace CmsApp.Models
{
    public class TabLoaderParams
    {
        public string ActionUrl { get; set; }
        public string DataElementId { get; set; }
        public bool LoadImmediately { get; set; }
        public bool UseFadeEffect { get; set; }
        public bool LoadByVisibilityOnly { get; set; }
    }
}