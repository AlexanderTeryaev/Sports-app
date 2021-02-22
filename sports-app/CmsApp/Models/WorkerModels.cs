using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DataService;
using System;
using Resources;
using CmsApp.Helpers;
using AppModel;
using DataService.DTO;

namespace CmsApp.Models
{
    public class Workers
    {

        public int RelevantEntityId { get; set; }

        public LogicaName RelevantEntityLogicalName { get; set; }

        public int JobId { get; set; }
        public int SeasonId { get; set; }
        public int LeagueId { get; set; }

        [Required]
        public string FullName { get; set; }

        public int UserId { get; set; }

        public IEnumerable<SelectListItem> JobsList { get; set; }

        public List<UserJobDto> UsersList { get; set; }

        public IDictionary<int,JobsRole> JobsRoles { get; set; }
        public IEnumerable<string> SelectedValues { get; set; }
        public IDictionary<int, UserJobDto> ReportOfficials { get; set; }

        public bool IsReportsEnabled { get; set; }
        public bool ReportRemoveDistance { get; set; }

        public string StartReportDate { get; set; }
        public string EndReportDate { get; set; }
        public bool IsUnionManager { get; set; }
        public int UnionId { get; internal set; }
        public int RefereeId { get; set; }
        public bool IsIndividualSection { get; set; }
        public bool SaturdaysTariff { get; set; }
        public string SectionAlias { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public int DefaultJobSelected { get; set; }
    }

    public class CreateWorkerForm
    {
        public int UserId { get; set; }

        public int SeasonId { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string FullNameFormatted { get; set; }

        [Required]
        public string IdentNum { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool IsActive { get; set; }

        public string CoachCertificate { get; set; }
        public bool IsUnionCoach { get; set; }

        public int RelevantEntityId { get; set; }

        public LogicaName RelevantEntityLogicalName { get; set; }

        public List<string> FormatPermissions { get; set; } = new List<string>();

        [Required]
        public int JobId { get; set; }

        public IEnumerable<SelectListItem> JobsList { get; set; }
        
        public int UserJobId { get; set; }

        [RegularExpression(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$",
            ErrorMessageResourceType = typeof(Messages),
            ErrorMessageResourceName = "InvalidPhone")]
        public string Phone { get; set; }

        [DateRangeValidate(MaxYearsAgo = 150)]
        public DateTime? BirthDate { get; set; }

        [Required]
        public string Address { get; set; }

        public string City { get; set; }

        public int? WithholdingTax { get; set; }

        public string Education { get; set; }
        public string PlaceOfEducation { get; set; }
        public DateTime? DateOfEdIssue { get; set; }
        public string EducationCert { get; set; }
        public bool RemoveEducationCert { get; set; }
        public bool RemoveCoachCert { get; set; }
        public bool IsKarateReferee { get; set; }
        public bool IsKarate { get; set; }
        public string IsraelLicenseType { get; set; }
        public string EKFLicenseType { get; set; }
        public string WKFLicenseType { get; set; }

        public DateTime? KumiteJudgeBCompleteIsrael { get; set; }
        public DateTime? KumiteJudgeBValidityIsrael { get; set; }
        public DateTime? KumiteJudgeACompleteIsrael { get; set; }
        public DateTime? KumiteJudgeAValidityIsrael { get; set; }
        public DateTime? RefereeBCompleteIsrael { get; set; }
        public DateTime? RefereeBValidityIsrael { get; set; }
        public DateTime? RefereeACompleteIsrael { get; set; }
        public DateTime? RefereeAValidityIsrael { get; set; }

        public DateTime? KataJudgeBCompleteIsrael { get; set; }
        public DateTime? KataJudgeBValidityIsrael { get; set; }
        public DateTime? KataJudgeACompleteIsrael { get; set; }
        public DateTime? KataJudgeAValidityIsrael { get; set; }

        public DateTime? KumiteJudgeBCompleteEKF { get; set; }
        public DateTime? KumiteJudgeBValidityEKF { get; set; }
        public DateTime? KumiteJudgeACompleteEKF { get; set; }
        public DateTime? KumiteJudgeAValidityEKF { get; set; }
        public DateTime? RefereeBCompleteEKF { get; set; }
        public DateTime? RefereeBValidityEKF { get; set; }
        public DateTime? RefereeACompleteEKF { get; set; }
        public DateTime? RefereeAValidityEKF { get; set; }

        public DateTime? KataJudgeBCompleteEKF { get; set; }
        public DateTime? KataJudgeBValidityEKF { get; set; }
        public DateTime? KataJudgeACompleteEKF { get; set; }
        public DateTime? KataJudgeAValidityEKF { get; set; }

        public DateTime? KumiteJudgeBCompleteWKF{ get; set; }
        public DateTime? KumiteJudgeBValidityWKF { get; set; }
        public DateTime? KumiteJudgeACompleteWKF { get; set; }
        public DateTime? KumiteJudgeAValidityWKF { get; set; }
        public DateTime? RefereeBCompleteWKF { get; set; }
        public DateTime? RefereeBValidityWKF { get; set; }
        public DateTime? RefereeACompleteWKF { get; set; }
        public DateTime? RefereeAValidityWKF { get; set; }

        public DateTime? KataJudgeBCompleteWKF { get; set; }
        public DateTime? KataJudgeBValidityWKF { get; set; }
        public DateTime? KataJudgeACompleteWKF { get; set; }
        public DateTime? KataJudgeAValidityWKF { get; set; }

        public string PaymentRateType { get; set; }
        public int? ConnectedClubId { get; set; }
        public IEnumerable<ClubShort> UnionClubs { get; set; }
        public bool IsReferee { get; set; }
        public bool IsRefereeCommittee { get; set; }
        public bool IsRelevantUser { get; set; }
        public bool CanConnectClubs { get; set; }
        public string JobRole { get; set; }
        public bool IsRefereeRole { get; internal set; }
        public bool OnlyReferees { get; set; }
        public int? ConnectedDisciplineId { get; internal set; }
        public IEnumerable<DisciplineDTO> UnionDisciplines { get; set; }
        public IEnumerable<int> ConnectedDisciplineIds { get; set; }
        public string[] SelectedDisciplinesIds { get; internal set; }
        public bool AlternativeId { get; set; }

        public string Function { get; set; }
    }

    public class WorkerForm
    {
        public int UserId { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string IdentNum { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public bool IsActive { get; set; }
    }

    public class CoachForm
    {
        public int UserId { get; set; }
        public int UserJobId { get; set; }
        public int SeasonId { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        [Required]
        public string IdentNum { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [RegularExpression(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$",
            ErrorMessageResourceType = typeof(Messages),
            ErrorMessageResourceName = "InvalidPhone")]
        public string Phone { get; set; }

        [DateRangeValidate(MaxYearsAgo = 150)]
        public DateTime? BirthDate { get; set; }

        [Required]
        public string Address { get; set; }

        public string City { get; set; }

        public int? WithholdingTax { get; set; }
        public string CoachCertificate { get; set; }
        public bool RemoveCoachCert { get; set; }
        public string Education { get; set; }
        public string PlaceOfEducation { get; set; }
        public DateTime? DateOfEdIssue { get; set; }
        public string EducationCert { get; set; }
        public bool RemoveEducationCert { get; set; }

    }
}