using Resources;
using System;

namespace CmsApp.Helpers
{
    public class ColumnsHelper
    {
        public static int GetColumnId(string columnName)
        {
            if (string.Equals(columnName, Messages.Active, StringComparison.OrdinalIgnoreCase))
                return 0;

            else if (string.Equals(columnName, Messages.Name, StringComparison.OrdinalIgnoreCase))
                return 2;

            else if (string.Equals(columnName, Messages.ClubDiscipline, StringComparison.OrdinalIgnoreCase)
                || (string.Equals(columnName, Messages.Team, StringComparison.OrdinalIgnoreCase)))
                return 3;

            else if (string.Equals(columnName, Messages.League, StringComparison.OrdinalIgnoreCase))
                return 4;

            else if (string.Equals(columnName, Messages.TeamPlayers_ProfilePicture, StringComparison.OrdinalIgnoreCase))
                return 5;

            else if (string.Equals(columnName, Messages.BirthDay, StringComparison.OrdinalIgnoreCase))
                return 6;

            else if (string.Equals(columnName, Messages.Shirt, StringComparison.OrdinalIgnoreCase))
                return 7;

            else if (string.Equals(columnName, Messages.ShirtSize, StringComparison.OrdinalIgnoreCase))
                return 8;

            else if (string.Equals(columnName, Messages.Position, StringComparison.OrdinalIgnoreCase))
                return 9;

            else if (string.Equals(columnName, Messages.IdentNum, StringComparison.OrdinalIgnoreCase))
                return 10;

            else if (string.Equals(columnName, Messages.Email, StringComparison.OrdinalIgnoreCase))
                return 11;

            else if (string.Equals(columnName, Messages.Phone, StringComparison.OrdinalIgnoreCase))
                return 12;

            else if (string.Equals(columnName, Messages.City, StringComparison.OrdinalIgnoreCase))
                return 13;

            else if (string.Equals(columnName, Messages.Insurance, StringComparison.OrdinalIgnoreCase))
                return 14;

            else if (string.Equals(columnName, Messages.Height, StringComparison.OrdinalIgnoreCase))
                return 15;

            else if (string.Equals(columnName, Messages.Weight, StringComparison.OrdinalIgnoreCase))
                return 16;

            else if (string.Equals(columnName, Messages.Gender, StringComparison.OrdinalIgnoreCase))
                return 17;

            else if (string.Equals(columnName, Messages.ParentName, StringComparison.OrdinalIgnoreCase))
                return 18;

            else if (string.Equals(columnName, Messages.IDFile, StringComparison.OrdinalIgnoreCase))
                return 19;

            else if (string.Equals(columnName, Messages.ClubName, StringComparison.OrdinalIgnoreCase))
                return 20;

            else if (string.Equals(columnName, Messages.Disciplines, StringComparison.OrdinalIgnoreCase))
                return 21;

            else if (string.Equals(columnName, Messages.StartPlaying, StringComparison.OrdinalIgnoreCase))
                return 22;

            else if (string.Equals(columnName, Messages.HandicapLevel, StringComparison.OrdinalIgnoreCase))
                return 23;

            else if (string.Equals(columnName, Messages.CalculatedReduction, StringComparison.OrdinalIgnoreCase))
                return 24;

            else if (string.Equals(columnName, Messages.TotalResult, StringComparison.OrdinalIgnoreCase))
                return 25;

            else if (string.Equals(columnName, Messages.Activity_BuildForm_UnionComment, StringComparison.OrdinalIgnoreCase))
                return 26;

            else if (string.Equals(columnName, Messages.Activity_BuildForm_ClubComment, StringComparison.OrdinalIgnoreCase))
                return 27;

            else if (string.Equals(columnName, Messages.MedicalCertificate, StringComparison.OrdinalIgnoreCase))
                return 28;

            else if (string.Equals(columnName, Messages.Activity_BuildForm_ApproveMedicalCert, StringComparison.OrdinalIgnoreCase))
                return 29;

            else if (string.Equals(columnName, Messages.MedExamDate, StringComparison.OrdinalIgnoreCase))
                return 30;

            else if (string.Equals(columnName, Messages.Approve, StringComparison.OrdinalIgnoreCase))
                return 31;

            else if (string.Equals(columnName, Messages.NotApproved, StringComparison.OrdinalIgnoreCase))
                return 32;

            else if (string.Equals(columnName, Messages.BlockadeEndDate, StringComparison.OrdinalIgnoreCase))
                return 33;

            else if (string.Equals(columnName, Messages.Route, StringComparison.OrdinalIgnoreCase))
                return 34;

            else if (string.Equals(columnName, Messages.Ranks, StringComparison.OrdinalIgnoreCase))
                return 35;

            else if (string.Equals(columnName, Messages.Rank, StringComparison.OrdinalIgnoreCase))
                return 36;

            else return -1;
        }
    }
}