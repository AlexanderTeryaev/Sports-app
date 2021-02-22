using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Helpers
{
    public static class PlayerHeatTypeHelper
    {
        public static List<SelectListItem> HeatTypeList = new List<SelectListItem>
        {
            new SelectListItem {Text = "", Value = null},
            new SelectListItem {Text = Messages.RoadHeat, Value = "1"},
            new SelectListItem {Text = Messages.MountainHeat, Value = "2"},
        };

        public static string GetHeatTypeName(int? heatTypeId)
        {
            var id = heatTypeId?.ToString() ?? null;
            return HeatTypeList.FirstOrDefault(x => x.Value == id)?.Text;
        }

        public static string GetHeatTypeValue(string text)
        {
            return HeatTypeList.FirstOrDefault(x => x.Text == text)?.Value;
        }
    }
}