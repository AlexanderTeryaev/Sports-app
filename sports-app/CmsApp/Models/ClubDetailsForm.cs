using AppModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DataService.DTO;

namespace CmsApp.Models
{
    public class ClubDetailsForm
    {
        public int ClubId { get; set; }
        public int? UnionId { get; set; }
        public int? SectionId { get; set; }
        public string NGO_Number { get; set; }
        public int? SportCenterId { get; set; }

        public int? ClubNumber { get; set; }
        public DateTime? DateOfClubApproval { get; set; }
        public DateTime? InitialDateOfClubApproval { get; set; }
        public DateTime? DateOfClubApprovalByRegional { get; set; }
        public List<SportCenter> SportCenterList { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string Email { get; set; }
        public string PrimaryImage { get; set; }

        public string MedicalCertificateFile { get; set; }
        public bool RemoveMedicalCertificateFile { get; set; }

        public string CertificateOfIncorporation { get; set; }
        public bool RemoveCertificateOfIncorporation { get; set; }
        public bool IsCertificateApproved { get; set; }

        public string ApprovalOfInsuranceCover { get; set; }
        public bool RemoveApprovalOfInsuranceCover { get; set; }
        public bool IsInsuranceCoverApproved { get; set; }
        public bool IsUnionArchive { get; set; }

        public string AuthorizedSignatories { get; set; }
        public bool RemoveAuthorizedSignatories { get; set; }
        public bool IsAuthorizedSignatoriesApproved { get; set; }
        public string AuthorizedSignPersonName { get; set; }
        public bool SignEachSeparately { get; set; }
        public bool SignTogether { get; set; }


        public string Description { get; set; }
        public string IndexImage { get; set; }
        public string IndexAbout { get; set; }
        public string TermsCondition { get; set; }
        public CultEnum Culture { get; set; }
        public SectionModel Section { get; set; }
        public int SeasonId { get; set; }
        public int SportType { get; set; }
        public List<SelectListItem> AvailableSports { get; set; }
        public bool IsTrainingEnabled { get; set; }
        public bool ShowAppCredentials { get; set; }
        public string AppLogin { get; set; }
        public string AppPassword { get; set; }

        public IEnumerable<UnionFormModel> UnionForms { get; set; }
        public string UnionFormTitle { get; set; }
        public IEnumerable<Discipline> UnionDisciplines { get; set; }
        public IEnumerable<int> DisciplinesIds { get; set; }
        public IEnumerable<int> ClubDisciplinesIds { get; set; }
        public string Statement { get; set; }
        public bool StatementApproved { get; set; }
        public bool IsNationalTeam { get; set; }
        
        public bool IsReportsEnabled { get; set; }
        public bool IsGymnastics { get; set; }
        public bool IsAthletics { get; set; }
        public bool IsRowing { get; set; }
        public bool IsBicycle { get; set; }
        public string DisciplinesString { get; set; }

        public ClubRegistration Registration { get; set; }
        public List<ClubRegistration> CustomRegistrations { get; set; }
        public int NumberOfCourts { get; set; }
        public string ClubInsurance { get; set; }
        public bool RemoveInsuranceFile { get; set; }
        public string ClubDisplayName { get; set; }
        public string ForeignName { get; set; }

        public int? AccountingKeyNumber { get; set; }
    }
}