using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Helpers
{
    public class DateModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bc)
        {
            var vpr = bc.ValueProvider.GetValue(bc.ModelName);

            if (vpr == null)
            {
                return null;
            }

            var date = vpr.AttemptedValue;

            if (String.IsNullOrEmpty(date))
            {
                return null;
            }

            bc.ModelState.SetModelValue(bc.ModelName, bc.ValueProvider.GetValue(bc.ModelName));

            try
            {
                var realDate = DateTime.Parse(date, CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"));
                bc.ModelState.SetModelValue(bc.ModelName, 
                    new ValueProviderResult(date, realDate.ToString("dd/MM/yyyy hh:mm"), 
                    CultureInfo.GetCultureInfoByIetfLanguageTag("he-IL")));

                return realDate;
            }
            catch (Exception)
            {
                bc.ModelState.AddModelError(bc.ModelName, String.Format("\"{0}\" is invalid.", bc.ModelName));
                return null;
            }
        }
    }
}