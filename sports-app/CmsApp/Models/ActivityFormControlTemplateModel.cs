using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CmsApp.Helpers;
using Omu.ValueInjecter;

namespace CmsApp.Models
{
    public class ActivityFormControlTemplateModel
    {
        public ActivityFormControlType ControlType { get; set; }

        public bool IsPublish { get; set; }

        public string PropertyName { get; set; }
        public string LabelTextEn { get; set; }
        public string LabelTextHeb { get; set; }
        public string LabelTextUk { get; set; }

        public bool IsDisabled { get; set; }
        public bool IsDisabledBySettings { get; set; }
        public bool IsRequired { get; set; }
        public bool CanBeRequired { get; set; }
        public bool CanBeDisabled { get; set; }

        public bool CustomFlag { get; set; }

        public bool CanBeRemoved { get; set; }
        public bool HasOptions { get; set; }

        public bool IsReadOnly { get; set; }

        public CultEnum Culture { get; set; }

        public string FieldNote { get; set; }

        public List<DropdownItem> DropdownItems { get; set; }
        public List<string> CustomDropdownValues { get; set; }

        public CustomPriceItem CustomPrice { get; set; }

        public IHtmlString GetDataAttributesForLabels()
        {
            return new HtmlString(
                $"data-labeltextheb=\"{HttpUtility.HtmlAttributeEncode(LabelTextHeb)}\" data-labeltexten=\"{HttpUtility.HtmlAttributeEncode(LabelTextEn)}\" data-labeltextuk=\"{HttpUtility.HtmlAttributeEncode(LabelTextUk)}\"");
        }

        public string GetLabelText()
        {
            switch (Culture)
            {
                case CultEnum.En_US:
                    return LabelTextEn;

                case CultEnum.He_IL:
                    return LabelTextHeb;

                case CultEnum.Uk_UA:
                    return LabelTextUk;

                default:
                    return LabelTextEn;
            }
        }

        public string GetCustomPriceTitle()
        {
            switch (Culture)
            {
                case CultEnum.En_US:
                    return CustomPrice?.TitleEng;

                case CultEnum.He_IL:
                    return CustomPrice?.TitleHeb;

                case CultEnum.Uk_UA:
                    return CustomPrice?.TitleUk;

                default:
                    return CustomPrice?.TitleEng;
            }
        }
    }

    public class DropdownItem
    {
        public string Value { get; set; }
        public string Caption { get; set; }
        public string Group { get; set; }
    }

    public class CustomPriceItem
    {
        public int Id { get; set; }
        public string TitleEng { get; set; }
        public string TitleHeb { get; set; }
        public string TitleUk { get; set; }
        public decimal Price { get; set; }
        public int MaxQuantity { get; set; }
        public int DefaultQuantity { get; set; }
    }
}