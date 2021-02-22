using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class MedicalInstituteForm
    {
        public List<MedicalInstituteModel> MedicalInstitutes { get; set; }

        // for creation of new medical institute
        public int MedicalInstitutesId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public int? SeasonId { get; set; }
        public int? UnionId { get; set; }
    }

    public class MedicalInstituteModel
    {
        public int MedicalInstitutesId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}