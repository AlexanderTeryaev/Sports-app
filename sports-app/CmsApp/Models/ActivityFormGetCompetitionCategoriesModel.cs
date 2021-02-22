using System.Collections.Generic;

namespace CmsApp.Models
{
    public class ActivityFormGetCompetitionCategoriesModel
    {
        public List<ActivityFormGetCompetitionCategoriesItemModel> Categories { get; set; }
        public int MinimumSelection { get; set; }
    }

    public class ActivityFormGetCompetitionCategoriesItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal RegistrationPrice { get; set; }

        public bool AlreadyRegistered { get; set; }
        public int RegistrationId { get; set; }

        public bool AlreadyPaid { get; set; }

        public bool SelectionDisabled { get; set; }
    }
}