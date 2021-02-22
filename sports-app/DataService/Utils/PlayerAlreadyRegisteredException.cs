using System;

namespace DataService.Utils
{
    public class PlayerAlreadyRegisteredException : Exception
    {
        public string UserName { get; set; }

        public int UserId { get; set; }

        public bool IsCoxwain { get; set; } = false;
    }
}
