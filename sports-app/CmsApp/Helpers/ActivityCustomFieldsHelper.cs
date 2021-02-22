using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CmsApp.Models;
using Newtonsoft.Json;
using Resources;

namespace CmsApp.Helpers
{
    public static class ActivityCustomFieldsHelper
    {
        public static List<ActivityFormCustomField> DeserializeFields(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<ActivityFormCustomField>();

            var customFields = JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(json);

            if (customFields.Any())
            {
                foreach (var checkbox in customFields.Where(
                    x => x.Type == ActivityFormControlType.CustomCheckBox))
                {
                    checkbox.Value = Convert.ToBoolean(checkbox.Value) ? Messages.Yes : Messages.No;
                }
            }

            return customFields;
        }
    }
}