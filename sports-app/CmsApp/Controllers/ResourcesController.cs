using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Resources;

namespace CmsApp.Controllers
{
    public class ResourcesController : Controller
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public ActionResult GetMessages()
        {
            var resourceDictionary = Messages.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true)
                .Cast<DictionaryEntry>()
                .ToDictionary(entry => entry.Key.ToString(), entry => entry.Value.ToString());
            var json = Serializer.Serialize(resourceDictionary);
            var javaScript =
                $"window.Resources = window.Resources || {{}}; window.Resources.{nameof(Messages)} = {json};";

            return JavaScript(javaScript);
        }
    }
}