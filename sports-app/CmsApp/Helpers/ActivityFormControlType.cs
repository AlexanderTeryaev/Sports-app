using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Helpers
{
    // DO NOT REARRANGE THOSE TYPES, THERE IS DATA THAT DEPENDS ON INDEX OF ENUM ELEMENT
    // IF YOU NEED TO ADD NEW TYPE - PUT IT AT THE END OF LIST
    public enum ActivityFormControlType
    {
        BasicInput,
        BasicCheckBox,
        BasicDateInput,
        BasicDropdown,
        BasicYesNoDropdown,
        PaymentByBenefactor,
        PlayerEscortSelector,
        BasicFileInput,
        BasicTextArea,
        CustomTextBox,
        CustomTextArea,
        CustomDropdown,
        CustomDropdownMultiselect,
        CustomText,
        CustomCheckBox,
        CustomFileReadonly,
        CustomFileUpload,
        CustomPrice,

        InsuranceOrSchoolInsurance,
        TenicardPrice,
        Gender,
        RegularOrCompetitiveMemberPrice,

        BasicDropdownMultiselect,
        CustomTextMultiline,
        CustomLink
    }
}