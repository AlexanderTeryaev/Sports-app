using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogLigFront.Helpers
{
    public static class LocaleHelper
    {
        public static readonly string LocaleCookieName = "_culture";

        public static CultEnum GetCultureByLanguge(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return CultEnum.He_IL;
            }

            if (string.Equals(Languages.En, language, StringComparison.InvariantCultureIgnoreCase))
            {
                return CultEnum.En_US;
            }
            if (string.Equals(Languages.Uk, language, StringComparison.InvariantCultureIgnoreCase))
            {
                return CultEnum.Uk_UA;
            }
            return CultEnum.He_IL;
        }

        public static CultEnum GetCultureByLocale(string locale)
        {
            if (string.Equals(Locales.En_US, locale, StringComparison.InvariantCultureIgnoreCase))
            {
                return CultEnum.En_US;
            }

            if (string.Equals(Locales.Uk_UA, locale, StringComparison.InvariantCultureIgnoreCase))
            {
                return CultEnum.Uk_UA;
            }

            return CultEnum.He_IL;
        }

        public static void SetCulture(this Controller controller, string culture)
        {
            HttpCookie cookie = controller.Request.Cookies[LocaleHelper.LocaleCookieName];
            if (cookie != null)
            {
                cookie.Value = culture;
            }
            else
            {
                cookie = new HttpCookie(LocaleHelper.LocaleCookieName)
                {
                    Value = culture,
                    Expires = DateTime.Now.AddYears(1)
                };
            }

            controller.Response.Cookies.Add(cookie);
        }

        public static CultEnum GetRequestCulture(this Controller controller)
        {
            return GetCultureByLocale(controller.Request.Cookies[LocaleCookieName]?.Value);
        }

        public static bool IsHebrew(this HttpRequestBase request)
        {
            return IsHebrew(request.Cookies[LocaleCookieName]?.Value);
        }

        public static bool IsHebrew(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                return true; //default
            }
            return string.Equals(Locales.He_IL, locale, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetLocale(CultEnum cultEnum)
        {
            switch (cultEnum)
            {
                case CultEnum.En_US:
                    return Locales.En_US;
                case CultEnum.Uk_UA:
                    return Locales.Uk_UA;
                case CultEnum.He_IL:
                    return Locales.He_IL;
            }

            return Locales.He_IL;
        }
    }
}