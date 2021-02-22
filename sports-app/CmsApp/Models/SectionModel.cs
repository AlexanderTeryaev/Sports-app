namespace CmsApp.Models
{
    public class SectionModel
    {
        public int SectionId { get; set; }
        public int LangId { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsIndividual { get; set; }
      //  public bool IsRegionallevelEnabled { get; set; }
    }
}