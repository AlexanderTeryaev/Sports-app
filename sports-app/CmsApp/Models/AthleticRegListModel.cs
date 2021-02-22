using System.Collections.Generic;
using System.Linq;
using DataService.DTO;

namespace CmsApp.Models
{
    public class AthleticRegListModel
    {
        public string ModalTitle { get; set; }
        public string SectionName { get; set; }
        public List<AthleticRegDto> AthleticRegistrations { get; set; }
    }
}