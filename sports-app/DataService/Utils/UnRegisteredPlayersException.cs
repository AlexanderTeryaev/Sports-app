using System;

namespace DataService.Utils
{
    public class UnRegisteredPlayersException : Exception
    {
        public string UserName { get; set; }

        public int UserId { get; set; }
    }
}
