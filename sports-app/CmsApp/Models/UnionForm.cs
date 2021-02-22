using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AppModel;
using DataService.DTO;

namespace CmsApp.Models
{
    public class UnionDetailsForm : OfficialsSettingsModel
    {
        public UnionDetailsForm()
        {
            UnionToClubCompetingRegistrationPrices = new List<UnionPriceModel>();
            UnionToClubRegularRegistrationPrices = new List<UnionPriceModel>();
            UnionToClubInsurancePrices = new List<UnionPriceModel>();
            UnionToClubTenicardPrices = new List<UnionPriceModel>();
        }

        public int UnionId { get; set; }
        public int DocId { get; set; }
        public int? SeasonId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public string Logo { get; set; }
        public string PrimaryImage { get; set; }
        public bool IsHadicapEnabled { get; set; }

        [Required]
        [StringLength(250)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        //[StringLength(250)]
        [DataType(DataType.MultilineText)]
        public string StatementOfClub { get; set; }

        public string IndexImage { get; set; }
        public string IndexAbout { get; set; }
        public string ContactPhone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public DateTime? CreateDate { get; set; }
        public string Terms { get; set; }

        public int SportType { get; set; }
        public List<SelectListItem> AvailableSports { get; set; }
        public SectionModel Section { get; set; }
        public string AppLogin { get; set; }
        public string AppPassword { get; set; }

        public IEnumerable<UnionFormModel> UnionForms { get; set; }
        public string UnionFormTitle { get; set; }
        public bool IsReportsEnabled { get; set; }
        public bool IsUnionManager { get; set; }
        public long UnionNumber { get; set; }
        public IEnumerable<KarateUnionPaymentForm> UnionPaymentForm { get; set; }
        public bool IsOfficialSettingsChecked { get; set; }

        public bool EnablePaymentsForPlayerClubRegistrations { get; set; }
        public List<UnionPriceModel> UnionToClubCompetingRegistrationPrices { get; set; }
        public List<UnionPriceModel> UnionToClubRegularRegistrationPrices { get; set; }
        public List<UnionPriceModel> UnionToClubInsurancePrices { get; set; }
        public List<UnionPriceModel> UnionToClubTenicardPrices { get; set; }
        public bool IsClubsBlocked { get; set; }
        public int? TotoReportMaxBirthYear { get; set; }
        public bool IsRegionallevelEnabled { get; set; }
        public bool ApprovePlayerByClubManagerFirst { get; set; }
        public string ContentForFriendshipCard { get; set; }
        public bool EnableIDCorrectionCheck { get; set; }
        public string UnionForeignName { get; set; }

        public string ForeignAddress { get; set; }
        public string UnionWebSite { get; set; }

    }

    public class UnionsForm
    {
        public int SectionId { get; set; }

        [Required]
        public string Name { get; set; }

        public IEnumerable<Union> UnionsList { get; set; }

        public UnionsForm()
        {
            UnionsList = new List<Union>();
        }
    }

    public class EditUnionForm
    {
        public int UnionId { get; set; }
        public string UnionName { get; set; }
        public int SectionId { get; set; }
        public string SectionAlias { get; set; }
        public string SectionName { get; set; }
        public bool IsCatchBall { get; set; }
        public int? SeasonId { get; set; }
        public bool IsRowing { get; set; }
        public List<Season> Seasons { get; set; }
        public bool HasDisciplines { get; set; }
        public bool SectionIsIndividual { get; internal set; }
        public bool IsReportsEnabled { get; set; }
        public bool IsClubsBlocked { get; internal set; }
    }
}