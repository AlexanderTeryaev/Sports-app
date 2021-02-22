using System;

namespace DataService.Utils
{
    public class CoxwainAlsoPartOfTeamException : Exception
    {
        public int UserId { get; set; }

        public bool IsCoxwain { get; set; } = false;
    }
}
