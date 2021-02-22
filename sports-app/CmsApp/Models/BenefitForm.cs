using AppModel;
using System;
using System.ComponentModel.DataAnnotations;
using CmsApp.Models.Mappers;

namespace CmsApp.Models
{
    public class BenefitForm
    {
        public int BenefitId { get; set; }
        public int SeasonId { get; set; }
        [Required] [StringLength(250)]
        public string Title { get; set; }
        [Required] [StringLength(250)]
        public string Company { get; set; }        
        public bool IsPublished { get; set; }
        public string Description { get; set; }
        public int UnionId { get; set; }
        public string UnionName { get; set; }
        public string Code { get; set; }
    }

    public class BenefitUpdateForm : Benefit
    {
        public bool RemoveBenefitImageFile { get; set; }
    }


}