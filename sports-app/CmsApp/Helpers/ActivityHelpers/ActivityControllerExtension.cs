using CmsApp.Controllers;
using CmsApp.Models;

namespace CmsApp.Helpers.ActivityHelpers
{
    public static class ActivityControllerExtension
    {
        public static string GetControlTemplate(this ActivityController controller, ActivityFormControlType controlType, ActivityFormControlTemplateModel model)
        {
            model.ControlType = controlType;

            switch (controlType)
            {
                case ActivityFormControlType.BasicInput:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/_BasicInput", model);

                case ActivityFormControlType.BasicCheckBox:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/_BasicCheckbox", model);

                case ActivityFormControlType.BasicDateInput:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/_BasicDateInput", model);

                case ActivityFormControlType.BasicDropdown:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/_BasicDropdownSelect", model);

                case ActivityFormControlType.BasicDropdownMultiselect:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/_BasicDropdownMultiselect", model);

                case ActivityFormControlType.BasicFileInput:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/_BasicFileInput", model);

                case ActivityFormControlType.BasicTextArea:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/_BasicTextArea", model);

                case ActivityFormControlType.BasicYesNoDropdown:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/_BasicYesNoDropdown", model);

                case ActivityFormControlType.PaymentByBenefactor:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/_PaymentByBenefactor", model);

                case ActivityFormControlType.PlayerEscortSelector:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/_PlayerEscortSelector", model);

                case ActivityFormControlType.CustomTextBox:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Custom/_CustomTextBox", model);

                case ActivityFormControlType.CustomTextArea:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Custom/_CustomTextArea", model);

                case ActivityFormControlType.CustomDropdown:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Custom/_CustomDropdown", model);

                case ActivityFormControlType.CustomDropdownMultiselect:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Custom/_CustomDropdownMultiselect", model);

                case ActivityFormControlType.CustomText:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Custom/_CustomText", model);

                case ActivityFormControlType.CustomTextMultiline:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Custom/_CustomTextMultiline", model);

                case ActivityFormControlType.CustomCheckBox:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Custom/_CustomCheckBox", model);

                case ActivityFormControlType.CustomFileReadonly:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Custom/_CustomFileReadonly", model);

                case ActivityFormControlType.CustomFileUpload:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Custom/_CustomFileUpload", model);

                case ActivityFormControlType.CustomPrice:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Custom/_CustomPrice", model);

                case ActivityFormControlType.InsuranceOrSchoolInsurance:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Specific/_InsuranceOrSchoolInsurance", model);

                case ActivityFormControlType.TenicardPrice:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Specific/_TenicardPrice", model);

                case ActivityFormControlType.Gender:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Specific/_Gender", model);

                case ActivityFormControlType.RegularOrCompetitiveMemberPrice:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Specific/_RegularOrCompetitiveMemberPrice", model);

                case ActivityFormControlType.CustomLink:
                    return RazorViewHelper.RenderRazorViewToString(controller.ControllerContext,
                        "ActivityForm/ControlTemplates/Custom/_CustomLink", model);

                default:
                    return string.Empty;
            }
        }
    }
}