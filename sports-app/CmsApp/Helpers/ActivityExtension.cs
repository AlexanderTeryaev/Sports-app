using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppModel;

namespace CmsApp.Helpers
{
    public static class ActivityExtension
    {
        public static ActivityFormType GetFormType(this Activity activity)
        {
            if (activity.UnionId != null && activity.ClubId == null)
            {
                if (activity.Type == ActivityType.Club)
                {
                    return ActivityFormType.UnionClub;
                }

                if (activity.Type == ActivityType.Group)
                {
                    return activity.IsAutomatic.HasValue && activity.IsAutomatic.Value
                        ? ActivityFormType.TeamRegistration
                        : ActivityFormType.CustomGroup;
                }

                if (activity.Type == ActivityType.Personal)
                {
                    return activity.IsAutomatic.HasValue && activity.IsAutomatic.Value
                        ? ActivityFormType.PlayerRegistration
                        : ActivityFormType.CustomPersonal;
                }

                if (activity.Type == ActivityType.UnionPlayerToClub)
                {
                    return activity.IsAutomatic == true
                        ? ActivityFormType.UnionPlayerToClub
                        : ActivityFormType.CustomUnionPlayerToClub;
                }
            }

            if (activity.ClubId != null)
            {
                if (activity.Type == ActivityType.Group)
                {
                    return activity.IsAutomatic.HasValue && activity.IsAutomatic.Value
                        ? activity.Club?.ParentClubId != null ? ActivityFormType.DepartmentClubTeamRegistration : ActivityFormType.ClubTeamRegistration
                        : activity.Club?.ParentClubId != null ? ActivityFormType.DepartmentClubCustomGroup : ActivityFormType.ClubCustomGroup;
                }

                if (activity.Type == ActivityType.Personal)
                {
                    return activity.IsAutomatic.HasValue && activity.IsAutomatic.Value
                        ? activity.Club?.ParentClubId != null ? ActivityFormType.DepartmentClubPlayerRegistration : ActivityFormType.ClubPlayerRegistration
                        : activity.Club?.ParentClubId != null ? ActivityFormType.DepartmentClubCustomPersonal : ActivityFormType.ClubCustomPersonal;
                }
            }

            throw new Exception($"Unable to determine form type for activity '{activity.Name}'(id {activity.ActivityId}, unionId: {activity.UnionId}, clubId: {activity.ClubId})");
        }
    }
}