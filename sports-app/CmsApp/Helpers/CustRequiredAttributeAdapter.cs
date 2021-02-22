using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace CmsApp.Helpers
{
    public class CustRequiredAttributeAdapter : RequiredAttributeAdapter
    {
        public CustRequiredAttributeAdapter(
        ModelMetadata metadata,
        ControllerContext context,
        RequiredAttribute attribute
    )
            : base(metadata, context, attribute)
        {
            attribute.ErrorMessageResourceType = typeof(Messages);
            attribute.ErrorMessageResourceName = "PropertyValueRequired";
        }
    }
}