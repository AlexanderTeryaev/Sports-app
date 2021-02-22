using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Helpers
{
    public class CustRangeAttributeAdapter : System.Web.Mvc.RangeAttributeAdapter
    {

        public CustRangeAttributeAdapter(System.Web.Mvc.ModelMetadata metadata, ControllerContext context, System.ComponentModel.DataAnnotations.RangeAttribute attribute)
            : base(metadata, context, attribute)
        {
            if(Convert.ToInt32(attribute.Minimum) == 0)
            {
                attribute.ErrorMessageResourceType = typeof(Messages);
                attribute.ErrorMessageResourceName = "Error_PositiveNumbersOnly";
            }
        }
    }
}