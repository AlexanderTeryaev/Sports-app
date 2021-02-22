using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Helpers
{
    public enum ActivityFormType
    {
        TeamRegistration,
        PlayerRegistration,

        UnionPlayerToClub,
        CustomUnionPlayerToClub,

        CustomGroup,
        CustomPersonal,

        ClubTeamRegistration,
        ClubPlayerRegistration,

        ClubCustomGroup,
        ClubCustomPersonal,

        DepartmentClubTeamRegistration,
        DepartmentClubPlayerRegistration,

        DepartmentClubCustomGroup,
        DepartmentClubCustomPersonal,

        UnionClub
    }
}