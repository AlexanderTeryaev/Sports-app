using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public static class CultureModel
    {
        public static void ChangeCulture(string ln)
        {
            var code = "he-IL";
            if (ln == "en")
                code = "en-US";

            if (ln == "he")
                code = "he-IL";

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(code);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(code);
        }
    }
}