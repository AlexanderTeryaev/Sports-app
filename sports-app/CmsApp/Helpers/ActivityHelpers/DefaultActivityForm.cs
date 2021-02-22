using System;
using System.Globalization;
using System.Text;
using AppModel;
using CmsApp.Controllers;
using CmsApp.Models;
using Resources;

namespace CmsApp.Helpers.ActivityHelpers
{
    public static class DefaultActivityForm
    {
        public static StringBuilder GetDefaultForm(this ActivityController controller, Activity activity)
        {
            var stringBuilder = new StringBuilder();

            var isUkraineGymnasticActivity = activity.UnionId == GlobVars.UkraineGymnasticUnionId;

            switch (activity.GetFormType())
            {
                case ActivityFormType.TeamRegistration:
                    #region Team registration default form

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerID",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "playerFullName",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("he-IL")),
                    //        LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("uk-UA")),
                    //        IsRequired = true,
                    //        Culture = controller.getCulture()
                    //    }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerFirstName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLastName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMiddleName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAddress",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBirthDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeam",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            IsReadOnly = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLeague",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.League),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.League),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.League),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            IsReadOnly = true,
                            //DropdownItems = GetLeaguesDropdown(activity),
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "teamsRegistrationPrice",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_TeamRegistrationPrice),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_TeamRegistrationPrice),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_TeamRegistrationPrice),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsReadOnly = true
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "nameForInvoice",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_NameForInvoice), CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_NameForInvoice), CultureInfo.CreateSpecificCulture("he-IL")),
                    //        CanBeRequired = true,
                    //        CanBeDisabled = true,
                    //        Culture = controller.getCulture()
                    //    }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.PaymentByBenefactor,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "paymentByBenefactor",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.ByBenefactor,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "document",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AttachDocuments,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "medicalCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.MedicalCertificate,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicYesNoDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "selfInsurance",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_SelfInsurance),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_SelfInsurance),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_SelfInsurance),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "insuranceCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.InsuranceCertificate,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicYesNoDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "needShirts",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.NeedShirts),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.NeedShirts),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.NeedShirts),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));

                    #endregion
                    break;

                case ActivityFormType.CustomGroup:
                    #region Custom team registration default form

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerID",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "playerFullName",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("he-IL")),
                    //        LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("uk-UA")),
                    //        IsRequired = true,
                    //        Culture = controller.getCulture()
                    //    }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerFirstName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLastName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMiddleName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));


                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerCity",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAddress",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBirthDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLeague",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.League),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.League),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.League),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeam",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsDisabledBySettings = !activity.AllowNewTeamRegistration,
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "teamsRegistrationPrice",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_TeamRegistrationPrice),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_TeamRegistrationPrice),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_TeamRegistrationPrice),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsReadOnly = true
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "nameForInvoice",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_NameForInvoice), CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_NameForInvoice), CultureInfo.CreateSpecificCulture("he-IL")),
                    //        CanBeRequired = true,
                    //        CanBeDisabled = true,
                    //        Culture = controller.getCulture()
                    //    }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.PaymentByBenefactor,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "paymentByBenefactor",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.ByBenefactor,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "document",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AttachDocuments,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "medicalCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.MedicalCertificate,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicYesNoDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "selfInsurance",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_SelfInsurance),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_SelfInsurance),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_SelfInsurance),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "insuranceCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.InsuranceCertificate,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicYesNoDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "needShirts",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.NeedShirts),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.NeedShirts),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.NeedShirts),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));

                    #endregion
                    break;

                case ActivityFormType.CustomPersonal:
                    #region Union custom personal default form

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerID",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerIsTrainer",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_TrainingPlayer),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_TrainingPlayer),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_TrainingPlayer),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CanBeDisabled = true,
                            CanBeRequired = true
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "playerFullName",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("he-IL")),
                    //        LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("uk-UA")),
                    //        IsRequired = true,
                    //        Culture = controller.getCulture()
                    //    }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerFirstName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLastName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMiddleName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.Gender,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerGender",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerCity",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAddress",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBirthDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            IsReadOnly = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.PlayerEscortSelector,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerIsEscort",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_Player_RegisterAs),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_Player_RegisterAs),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_Player_RegisterAs),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.AllowEscortRegistration
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.PaymentByBenefactor,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "paymentByBenefactor",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.ByBenefactor,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "idFile",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.IDFile),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.IDFile),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.IDFile),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "document",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AttachDocuments,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "medicalCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.MedicalCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "insuranceCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.InsuranceCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerProfilePicture",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLeagueDropDown",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Activity_SelectLeagues),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Activity_SelectLeagues),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Activity_SelectLeagues),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsDisabledBySettings = !activity.RestrictLeagues || activity.RegistrationsByCompetitionsCategory,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeamDropDown",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsReadOnly = true,
                            IsRequired = true,
                            IsDisabledBySettings = activity.NoTeamRegistration || activity.RegistrationsByCompetitionsCategory,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdownMultiselect,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerCompetitionCategory",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Category),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Category),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Category),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            IsDisabledBySettings = !activity.RegistrationsByCompetitionsCategory,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerRegistrationPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerRegistrationPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerRegistrationPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerRegistrationPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.RegistrationPrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerInsurancePrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.InsurancePrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMemberFee",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_MemberFees),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_MemberFees),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_MemberFees),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.MembersFee,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerHandlingFee",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_HandlingFee),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_HandlingFee),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_HandlingFee),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.HandlingFee,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "disableRegistrationPayment",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayRegistration),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayRegistration),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayRegistration),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.AllowNoRegistrationPayment
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "disableInsurancePayment",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.AllowNoInsurancePayment
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "disableMembersFeePayment",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayMembersFee),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayMembersFee),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayMembersFee),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.AllowNoFeePayment
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "disableHandlingFeePayment",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayHandlingFee),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayHandlingFee),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayHandlingFee),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.AllowNoHandlingFeePayment
                        }));

                    #endregion
                    break;

                case ActivityFormType.PlayerRegistration:
                    #region Player registration default form

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerID",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "playerFullName",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("he-IL")),
                    //        LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("uk-UA")),
                    //        IsRequired = true,
                    //        Culture = controller.getCulture()
                    //    }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerFirstName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLastName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMiddleName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.Gender,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerGender",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAddress",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerCity",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBirthDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            IsReadOnly = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeam",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            IsReadOnly = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLeague",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.League),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.League),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.League),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            IsReadOnly = true,
                            //DropdownItems = GetLeaguesDropdown(activity),
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.PaymentByBenefactor,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "paymentByBenefactor",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.ByBenefactor,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "document",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AttachDocuments,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "medicalCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.MedicalCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "insuranceCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.InsuranceCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerProfilePicture",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerRegistrationPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerRegistrationPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerRegistrationPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerRegistrationPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.RegistrationPrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerInsurancePrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.LeagueDetail_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.InsurancePrice,
                            IsReadOnly = true
                        }));

                    #endregion
                    break;

                case ActivityFormType.UnionPlayerToClub:
                    #region Player club registration default form

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerID",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerRegion",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.ChooseRegion),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.ChooseRegion),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.ChooseRegion),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !isUkraineGymnasticActivity
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "playerFullName",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("he-IL")),
                    //        LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("uk-UA")),
                    //        IsRequired = true,
                    //        Culture = controller.getCulture()
                    //    }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerFirstName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLastName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMiddleName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParentName", //In ukraine gymnastic union ParentName is used as FatherName
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.FatherName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.FatherName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.FatherName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture(),
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMotherName",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.MotherName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.MotherName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.MotherName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture(),
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParentPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.ParentPhone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.ParentPhone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.ParentPhone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture(),
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParentEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.ParentEmail),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.ParentEmail),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.ParentEmail),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture(),
                        }));

                    #region Ukraine gymnastic union specific fields

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerIdentCard",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.IdentityCard),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.IdentityCard),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.IdentityCard),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !isUkraineGymnasticActivity
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLicenseDate",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.LicenseDate),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.LicenseDate),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.LicenseDate),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !isUkraineGymnasticActivity
                        }));
                    
                    #endregion

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.Gender,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerGender",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerCity",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.City),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAddress",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBirthDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPostalCode",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.PostalCode),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.PostalCode),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.PostalCode),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPassportNumber",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.PassportNum),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.PassportNum),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.PassportNum),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerForeignFirstName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.ForeignFirstName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.ForeignFirstName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.ForeignFirstName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerForeignLastName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.ForeignLastName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.ForeignLastName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.ForeignLastName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerDateOfMedicalExamination",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.MedExamDate),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.MedExamDate),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.MedExamDate),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            IsReadOnly = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerDateOfInsuranceValidity",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.DateOfInsuranceValidity),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.DateOfInsuranceValidity),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.DateOfInsuranceValidity),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            IsReadOnly = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTenicardValidity",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.TenicardValidity),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.TenicardValidity),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.TenicardValidity),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            IsReadOnly = true,
                            IsDisabledBySettings = !string.Equals(activity.Union?.Section?.Alias, SectionAliases.Tennis,
                                StringComparison.CurrentCultureIgnoreCase),
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerClub",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Club),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Club),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Club),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsRequired = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeam",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = activity.MultiTeamRegistrations,
                            IsRequired = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdownMultiselect,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeamMultiple",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            IsDisabledBySettings = !activity.MultiTeamRegistrations,
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            HasOptions = true
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.PaymentByBenefactor,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "paymentByBenefactor",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.ByBenefactor,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "document",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AttachDocuments,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "medicalCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.MedicalCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "insuranceCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.InsuranceCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "idFile",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.IDFile),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.IDFile),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.IDFile),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerProfilePicture",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.RegularOrCompetitiveMemberPrice,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerRegistrationPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.UnionDetail_PlayerToClubRegistrationPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.UnionDetail_PlayerToClubRegistrationPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.UnionDetail_PlayerToClubRegistrationPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CustomFlag = activity.AllowCompetitiveMembers,
                            IsDisabledBySettings = !activity.RegistrationPrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.InsuranceOrSchoolInsurance,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerInsurancePrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.UnionDetail_PlayerToClubInsurancePrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.UnionDetail_PlayerToClubInsurancePrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.UnionDetail_PlayerToClubInsurancePrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.InsurancePrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.TenicardPrice,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTenicardPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.UnionDetail_PlayerToClubTenicardPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.UnionDetail_PlayerToClubTenicardPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.UnionDetail_PlayerToClubTenicardPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsReadOnly = true,
                            IsDisabledBySettings = !string.Equals(activity.Union?.Section?.Alias, SectionAliases.Tennis,
                                StringComparison.CurrentCultureIgnoreCase),
                        }));

                    #endregion
                    break;

                case ActivityFormType.ClubPlayerRegistration:
                    #region Club player registration default form

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerID",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBrotherIdForDiscount",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.BrotherIdForDiscount),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.BrotherIdForDiscount),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.BrotherIdForDiscount),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = false,
                            IsDisabledBySettings = !activity.EnableBrotherDiscount,
                            Culture = controller.getCulture()
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "playerFullName",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("he-IL")),
                    //        LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("uk-UA")),
                    //        IsRequired = true,
                    //        Culture = controller.getCulture()
                    //    }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerFirstName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLastName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMiddleName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.Gender,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerGender",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParentName",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAddress",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBirthDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "clubSchool",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            HasOptions = true,
                            IsDisabledBySettings = activity.ClubLeagueTeamsOnly
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeam",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            IsDisabledBySettings = activity.MultiTeamRegistrations,
                            IsReadOnly = true,
                            Culture = controller.getCulture(),
                            HasOptions = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdownMultiselect,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeamMultiple",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            IsDisabledBySettings = !activity.MultiTeamRegistrations,
                            CanBeRequired = true,
                            Culture = controller.getCulture(),
                            HasOptions = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerProfilePicture",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.PaymentByBenefactor,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "paymentByBenefactor",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.ByBenefactor,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "document",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AttachDocuments,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "medicalCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.MedicalCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "insuranceCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.InsuranceCertificate,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAdjustPricesStartDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_StartDateToAdjustPrices),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_StartDateToAdjustPrices),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_StartDateToAdjustPrices),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AllowToEnterDateToAdjustPrices,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerRegistrationPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.RegistrationPrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParticipationPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.ParticipationPrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "disableParticipationPayment",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayParticipation),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayParticipation),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayParticipation),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.AllowNoParticipationPayment
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "postponeParticipationPayment",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_PayForParticipationLater),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_PayForParticipationLater),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_PayForParticipationLater),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.PostponeParticipationPayment
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerInsurancePrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.InsurancePrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "disableInsurancePayment",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.AllowNoInsurancePayment
                        }));

                    #endregion
                    break;

                case ActivityFormType.ClubCustomPersonal:
                    #region Custom club player registration default form

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerID",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBrotherIdForDiscount",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.BrotherIdForDiscount),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.BrotherIdForDiscount),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.BrotherIdForDiscount),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = false,
                            IsDisabledBySettings = !activity.EnableBrotherDiscount,
                            Culture = controller.getCulture()
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "playerFullName",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("he-IL")),
                    //        LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("uk-UA")),
                    //        IsRequired = true,
                    //        Culture = controller.getCulture()
                    //    }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerFirstName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLastName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMiddleName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.Gender,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerGender",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParentName",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAddress",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBirthDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "clubSchool",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            IsDisabledBySettings = activity.NoTeamRegistration,
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            HasOptions = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeam",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            IsDisabledBySettings = activity.NoTeamRegistration || activity.MultiTeamRegistrations,
                            IsReadOnly = true,
                            Culture = controller.getCulture(),
                            HasOptions = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdownMultiselect,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeamMultiple",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            IsDisabledBySettings = activity.NoTeamRegistration || !activity.MultiTeamRegistrations,
                            CanBeRequired = true,
                            Culture = controller.getCulture(),
                            HasOptions = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerProfilePicture",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.PaymentByBenefactor,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "paymentByBenefactor",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.ByBenefactor,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "document",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AttachDocuments,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "medicalCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.MedicalCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "insuranceCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.InsuranceCertificate,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAdjustPricesStartDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_StartDateToAdjustPrices),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_StartDateToAdjustPrices),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_StartDateToAdjustPrices),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AllowToEnterDateToAdjustPrices,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerRegistrationPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.RegistrationPrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParticipationPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.ParticipationPrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "disableParticipationPayment",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayParticipation),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayParticipation),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayParticipation),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.AllowNoParticipationPayment
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerInsurancePrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.InsurancePrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "disableInsurancePayment",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.AllowNoInsurancePayment
                        }));

                    #endregion
                    break;

                case ActivityFormType.DepartmentClubPlayerRegistration:
                    #region Department club player registration default form

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerID",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "playerFullName",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("he-IL")),
                    //        LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("uk-UA")),
                    //        IsRequired = true,
                    //        Culture = controller.getCulture()
                    //    }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerFirstName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLastName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMiddleName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.Gender,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerGender",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParentName",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAddress",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBirthDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "clubSchool",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            HasOptions = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeam",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            IsReadOnly = true,
                            Culture = controller.getCulture(),
                            HasOptions = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerProfilePicture",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.PaymentByBenefactor,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "paymentByBenefactor",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.ByBenefactor,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "document",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AttachDocuments,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "medicalCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.MedicalCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "insuranceCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.InsuranceCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerRegistrationPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.RegistrationPrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParticipationPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.ParticipationPrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerInsurancePrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.InsurancePrice,
                            IsReadOnly = true
                        }));

                    #endregion
                    break;

                case ActivityFormType.DepartmentClubCustomPersonal:
                    #region Custom department club player registration default form

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerID",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "playerFullName",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("he-IL")),
                    //        LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("uk-UA")),
                    //        IsRequired = true,
                    //        Culture = controller.getCulture()
                    //    }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerFirstName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLastName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMiddleName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.Gender,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerGender",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Gender),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParentName",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAddress",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBirthDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "clubSchool",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_School),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            IsDisabledBySettings = activity.NoTeamRegistration,
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            HasOptions = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerTeam",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Team),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            //CanBeRequired = true,
                            //CanBeDisabled = true,
                            //DropdownItems = GetTeamsDropdown(activity),
                            IsDisabledBySettings = activity.NoTeamRegistration,
                            IsReadOnly = true,
                            Culture = controller.getCulture(),
                            HasOptions = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerProfilePicture",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.PaymentByBenefactor,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "paymentByBenefactor",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.ByBenefactor,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "document",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AttachDocuments,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "medicalCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.MedicalCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "insuranceCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.InsuranceCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerRegistrationPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.RegistrationPrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParticipationPrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_ParticipationPrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.ParticipationPrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "disableParticipationPayment",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.AllowNoParticipationPayment
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerInsurancePrice",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.TeamDetails_PlayerInsurancePrice),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.InsurancePrice,
                            IsReadOnly = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "disableInsurancePayment",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_DoNotPayInsurance),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            IsDisabledBySettings = !activity.AllowNoInsurancePayment
                        }));
                    #endregion
                    break;

                case ActivityFormType.UnionClub:
                    #region New club registration default form

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerID",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.IdentNum),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));

                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "playerFullName",
                    //        LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("he-IL")),
                    //        LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FullName),
                    //            CultureInfo.CreateSpecificCulture("uk-UA")),
                    //        IsRequired = true,
                    //        Culture = controller.getCulture()
                    //    }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerFirstName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.FirstName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerLastName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.LastName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerMiddleName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.MiddleName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerParentName",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerParentName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Email),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Phone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerAddress",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Address),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture(),
                            CanBeDisabled = true
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDateInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerBirthDate",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_BirthDate),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "region",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.ChooseRegion),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.ChooseRegion),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.ChooseRegion),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture(),
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "clubName",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.ClubName),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.ClubName),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.ClubName),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = true,
                            Culture = controller.getCulture(),
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "clubNumberOfCourts",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.Club_ClubNumberOfCourts),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.Club_ClubNumberOfCourts),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.Club_ClubNumberOfCourts),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture(),
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "clubNgoNumber",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.ClubNGONumber),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.ClubNGONumber),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.ClubNGONumber),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture(),
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "clubSportsCenter",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.ClubNameSportsCenter),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.ClubNameSportsCenter),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.ClubNameSportsCenter),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            CanBeDisabled = true,
                            Culture = controller.getCulture(),
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "clubAddress",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.ClubAddress),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.ClubAddress),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.ClubAddress),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            Culture = controller.getCulture(),
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "clubPhone",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.ClubPhone),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.ClubPhone),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.ClubPhone),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            Culture = controller.getCulture(),
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "clubEmail",
                            LabelTextEn = Messages.ResourceManager.GetString(nameof(Messages.ClubEmail),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(nameof(Messages.ClubEmail),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(nameof(Messages.ClubEmail),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            Culture = controller.getCulture(),
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "certificateOfIncorporation",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.CertificateOfIncorporation),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.CertificateOfIncorporation),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.CertificateOfIncorporation),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "approvalOfInsuranceCover",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.ApprovalOfInsuranceCover),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.ApprovalOfInsuranceCover),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.ApprovalOfInsuranceCover),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "authorizedSignatories",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.AuthorizedSignatories),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.AuthorizedSignatories),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.AuthorizedSignatories),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "playerProfilePicture",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_PlayerProfilePicture),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.PaymentByBenefactor,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "paymentByBenefactor",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_ByBenefactor),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.ByBenefactor,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "document",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_Document),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.AttachDocuments,
                            Culture = controller.getCulture()
                        }));

                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "medicalCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_MedicalCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.MedicalCertificate,
                            Culture = controller.getCulture()
                        }));
                    stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicFileInput,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = "insuranceCert",
                            LabelTextEn = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = Messages.ResourceManager.GetString(
                                nameof(Messages.Activity_BuildForm_InsuranceCert),
                                CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeRequired = true,
                            IsDisabledBySettings = !activity.InsuranceCertificate,
                            Culture = controller.getCulture()
                        }));
                    //stringBuilder.Append(controller.GetControlTemplate(ActivityFormControlType.BasicInput,
                    //    new ActivityFormControlTemplateModel
                    //    {
                    //        PropertyName = "playerRegistrationPrice",
                    //        LabelTextEn = Messages.ResourceManager.GetString(
                    //            nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                    //            CultureInfo.CreateSpecificCulture("en-US")),
                    //        LabelTextHeb = Messages.ResourceManager.GetString(
                    //            nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice),
                    //            CultureInfo.CreateSpecificCulture("he-IL")),
                    //        Culture = controller.getCulture(),
                    //        IsDisabledBySettings = !activity.RegistrationPrice,
                    //        IsReadOnly = true
                    //    }));

                    #endregion
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return stringBuilder;
        }
    }
}