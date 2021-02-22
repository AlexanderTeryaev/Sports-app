using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Helpers
{

    public class NullableInjection : ConventionInjection
    {
        protected override bool Match(ConventionInfo c)
        {
            return c.SourceProp.Name == c.TargetProp.Name &&
                (c.SourceProp.Type == c.TargetProp.Type
              || c.SourceProp.Type == Nullable.GetUnderlyingType(c.TargetProp.Type)
              || (Nullable.GetUnderlyingType(c.SourceProp.Type) == c.TargetProp.Type
                && c.SourceProp.Value != null)
              );
        }

        protected override object SetValue(ConventionInfo c)
        {
            return c.SourceProp.Value;
        }
    }
}