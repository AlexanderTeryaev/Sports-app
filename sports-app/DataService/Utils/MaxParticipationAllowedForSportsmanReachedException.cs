using System;

namespace DataService.Utils
{
    public class MaxParticipationAllowedForSportsmanReachedException : Exception
    {
        public int MaxParticipationAllowed { get; set; }

        public string UserName { get; set; }

        public int UserId { get; set; }

        public bool IsCoxwain { get; set; } = false;
    }
}
