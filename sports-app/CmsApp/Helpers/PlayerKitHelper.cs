using Resources;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CmsApp.Helpers
{
    public static class PlayerKitHelper
    {
        public static List<SelectListItem> KitList = new List<SelectListItem>
        {
            new SelectListItem {Text = "", Value = null},
            new SelectListItem {Text = Messages.Ready, Value = "1"},
            new SelectListItem {Text = Messages.Provided, Value = "2"},
            new SelectListItem {Text = Messages.Printed, Value = "3"}
        };

        public static string GetKitName(int? kitId)
        {
            var id = kitId?.ToString() ?? null;
            return KitList.FirstOrDefault(x => x.Value == id)?.Text;
        }

        public static string GetKitValue(string text)
        {
            return KitList.FirstOrDefault(x => x.Text == text)?.Value;
        }
    }
}