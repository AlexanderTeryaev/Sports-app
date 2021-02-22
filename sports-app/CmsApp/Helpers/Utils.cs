using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Helpers
{
    public class Utils
    {
        public static bool IsOrderByFormatAsc(int? format)
        {
            if (!format.HasValue)
            {
                format = 0;
            }
            if ((format >= 6 && format <= 8) || format == 10 || format == 11)
            {
                return false;
            }
            return true;
        }
    }
}