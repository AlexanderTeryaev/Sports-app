using CmsApp.Models;
using Resources;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Helpers.ActivityHelpers
{
    public static class CustomPriceHelper
    {
        public static string GetPriceTitle(CultEnum culture, ActivityCustomPriceModel customPrice)
        {
            if (customPrice == null)
            {
                return string.Empty;
            }

            switch (culture)
            {
                case CultEnum.En_US:
                    return customPrice.TitleEng;      
                case CultEnum.He_IL:
                    return customPrice.TitleHeb;  
                case CultEnum.Uk_UA:
                    return customPrice.TitleUk;

                default:
                    return customPrice.TitleEng;
            }
        }

        public static List<SelectListItem> GetFriendshipPriceTypesSelectList(int? friendshipPriceTypeId, List<int?> ids = null)
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = null,
                    Text = Messages.Select,
                    Selected = !friendshipPriceTypeId.HasValue
                },
                new SelectListItem
                {
                    Value = "0",
                    Text = Messages.Regular,
                    Selected = friendshipPriceTypeId == 0
                },
                new SelectListItem
                {
                    Value = "1",
                    Text = Messages.Special,
                    Selected = friendshipPriceTypeId == 1
                },
                new SelectListItem
                {
                    Value = "2",
                    Text = Messages.Student,
                    Selected = friendshipPriceTypeId == 2
                },
                new SelectListItem
                {
                    Value = "3",
                    Text = Messages.Soldier,
                    Selected = friendshipPriceTypeId == 3
                }
            };
            var result = new List<SelectListItem>();
            if (ids != null)
            {
                foreach (var item in list)
                {
                    if (item.Value == null || ids.Contains(Convert.ToInt32(item.Value)))
                    {
                        result.Add(item);
                    }
                }
            }
            else
            {
                result.AddRange(list);
            }

            return result;
        }

        public static string GetFriendshipPriceTypeName(int? friendshipPriceTypeId)
        {
            switch(friendshipPriceTypeId)
            {
                case 0:
                    return Messages.Regular;
                case 1:
                    return Messages.Special;
                case 2:
                    return Messages.Student;
                case 3:
                    return Messages.Soldier;
                default:
                    return "";
            }
        }
    }
}